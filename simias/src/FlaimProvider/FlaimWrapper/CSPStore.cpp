/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
// CSPStore.cpp: implementation of the CSPStore class.
//
//////////////////////////////////////////////////////////////////////

#include "CSPStore.h"
#include <stdio.h>

FLMUINT	CSPTypeToFlaimTypeArray[] =
{	
	FLM_UNDEFINED_TYPE,	//Undefined
	FLM_TEXT_TYPE,			//1	CSPString
	FLM_BINARY_TYPE,		//2	CSPI1
	FLM_BINARY_TYPE,		//3	CSPUI1
	FLM_BINARY_TYPE,		//4	CSPI2
	FLM_BINARY_TYPE,		//5	CSPUI2
	FLM_BINARY_TYPE,		//6	CSPI4
	FLM_BINARY_TYPE,		//7	CSPUI4
	FLM_BINARY_TYPE,		//8	CSPI8
	FLM_BINARY_TYPE,		//9	CSPUI8
	FLM_BINARY_TYPE,		//10 CSPChar
	FLM_BINARY_TYPE,		//11 CSPFloat
	FLM_BINARY_TYPE,		//12 CSPBool
	FLM_BINARY_TYPE,		//13 CSPDTime
	FLM_TEXT_TYPE,			//14 CSPUri
	FLM_TEXT_TYPE,			//15 CSPXml
	FLM_BINARY_TYPE,		//16 CSPTimeSpan
	FLM_TEXT_TYPE,			//17 CSPRelationship
	FLM_NUMBER_TYPE,		//17 not exposed CSPInt
 };


FLMUNICODE *cs_flaim_type_name[] =
{
	(FLMUNICODE*)L"text",
	(FLMUNICODE*)L"numb",
	(FLMUNICODE*)L"bina",
	(FLMUNICODE*)L"cont",
	(FLMUNICODE*)L"real",
	(FLMUNICODE*)L"date",
	(FLMUNICODE*)L"time",
	(FLMUNICODE*)L"tmst",
	(FLMUNICODE*)L"blob" 
};

CS_FIELD_DEF propertyArray[] =
{
	{CS_Id_Node_Type,			CS_Name_Node_Type,		CS_Type_Node_Type},
	{CS_Id_Property_Type,		CS_Name_Property_Type,	CS_Type_Property_Type},
	{CS_Id_Display_Name,		CS_Name_Display_Name,	CS_Type_Display_Name},
	{CS_Id_GUID,				CS_Name_GUID,			CS_Type_GUID},
	{CS_Id_CollectionId,		CS_Name_CollectionId,	CS_Type_CollectionId},
	{CS_Id_Size,				CS_Name_Size,			CS_Type_Size},
	{CS_Id_PFlags,				CS_Name_PFlags,			CS_Type_PFlags},
};

FLMUNICODE *CSPTypeStringString = CSP_Type_String_String;
FLMUNICODE *CSPTypeI1String = CSP_Type_I1_String;
FLMUNICODE *CSPTypeUI1String = CSP_Type_UI1_String;
FLMUNICODE *CSPTypeI2String = CSP_Type_I2_String;
FLMUNICODE *CSPTypeUI2String = CSP_Type_UI2_String;
FLMUNICODE *CSPTypeI4String = CSP_Type_I4_String;
FLMUNICODE *CSPTypeUI4String = CSP_Type_UI4_String;
FLMUNICODE *CSPTypeI8String = CSP_Type_I8_String;
FLMUNICODE *CSPTypeUI8String = CSP_Type_UI8_String;
FLMUNICODE *CSPTypeCharString = CSP_Type_Char_String;
FLMUNICODE *CSPTypeFloatString = CSP_Type_Float_String;
FLMUNICODE *CSPTypeBoolString = CSP_Type_Bool_String;
FLMUNICODE *CSPTypeDTimeString = CSP_Type_DTime_String;
FLMUNICODE *CSPTypeUriString = CSP_Type_Uri_String;
FLMUNICODE *CSPTypeXmlString = CSP_Type_Xml_String;
FLMUNICODE *CSPTypeTimeSpanString = CSP_Type_TimeSpan_String;
FLMUNICODE *CSPTypeRelationshipString = CSP_Type_Relationship_String;

void printflmstring(FLMUNICODE* pString)
{
	int i = 0;
	while (pString[i] != 0) 
	{
		printf("%c", (char)pString[i]);
		i++;
	}
	printf("\n");
} // pringslmstring()


FLMUNICODE* flmstrstr(FLMUNICODE* s1, FLMUNICODE* s2)
{
	register int i;
	register int j;
	register FLMUNICODE first = *s2;

	while (*s1)
	{
		if (*s1++ == first)
		{
			i = 0;
			j = 1;
			while (s2[j])
			{
				if (s1[i++] != s2[j++])
				{
					break;
				}
			}
			if (s2[j] == 0)
			{
				return (s1 - 1);
			}
		}
	}
	return (0);
} // flmstrstr()


wchar_t	CSPDB::nameSuffex[] = L"_index";

CSPDB::CSPDB() :
	m_flaimInitialized(false)
{
	RCODE rc;
	// Make sure the Flaim library has been initialized.
	rc = FlmStartup();
	if (RC_OK(rc))
	{
		m_RefCount = 1;
	}
}

CSPDB::~CSPDB()
{
}

void CSPDB::AddRef()
{
	++m_RefCount;
}

void CSPDB::Release()
{
	if (--m_RefCount == 0)
	{
		m_NameTable.clearTable();
	}
}

void CSPDB::SetupNameTable(HFDB hFlaim)
{
	RCODE		rc = FERR_OK;
	FLMUINT		drn = 1;
	FLMUNICODE	name[MAX_PROPERTY_NAME];

	F_NameTable		nameTable;
	nameTable.setupFromDb(hFlaim);

	while(nameTable.getFromTagNum(drn, name, NULL, MAX_PROPERTY_NAME))
	{
		m_NameTable.addTag(name, 0, drn, 0, 0, TRUE);
		++drn;
	}

	nameTable.clearTable();

	pIndexIDTable = new FLMUINT[indexTSize];
	for (int i = 0; i < indexTSize; ++i)
	{
		pIndexIDTable[i] = 0;
	}
}

RCODE CSPDB::initializeDB(HFDB hFlaim, FLMBOOL created)
{
	RCODE		rc = FERR_OK;
		
	if (!m_flaimInitialized)
	{
		SetupNameTable(hFlaim);
		if (RC_OK(rc))
		{
			if (created)
			{
				// Register the well know properties
				rc = registerFieldArray(hFlaim, propertyArray, sizeof(propertyArray) / sizeof(CS_FIELD_DEF));
				if (RC_OK(rc))
				{
					// Add the indexes
					rc = registerIndexArray(hFlaim, propertyArray, sizeof(propertyArray) / sizeof(CS_FIELD_DEF));
				}
			}
			m_flaimInitialized = true;
		}
	}

	return (rc);
} // CSPDB::initializeDB()

RCODE CSPDB::registerFieldArray(HFDB hFlaim, CS_FIELD_DEF *fieldTable, FLMINT count)
{
	RCODE				rc = FERR_OK;
	int 				i;

	// Begin a flaim transaction.
	rc = FlmDbTransBegin(hFlaim, FLM_UPDATE_TRANS, 255);
	if (RC_OK(rc))
	{
		for (i = 0; i < count; ++i)
		{
			FLMUINT		fieldId = fieldTable[i].id;
			rc = RegisterField(hFlaim, fieldTable[i].name, CSPTypeToFlaimType(fieldTable[i].type), &fieldId);
			if (RC_BAD(rc))
			{
				break;
			}
		}
		if (RC_OK(rc))
		{
	 		FlmDbTransCommit(hFlaim);
		}
		else
		{
			FlmDbTransAbort(hFlaim);
		}
	}
	return (rc);
} // CSPDB::registerFieldArray()


RCODE CSPDB::RegisterField(HFDB hFlaim, FLMUNICODE *pFieldName, FLMUINT flmType, FLMUINT* pFieldId)
{
	RCODE				rc = FERR_OK;
	FlmRecord			*pRec = NULL;
	void				*pvField = 0;

	pRec = new FlmDefaultRec();
	if (pRec != NULL)
	{
		rc = pRec->insertLast(0, FLM_FIELD_TAG, FLM_TEXT_TYPE, &pvField);
		if (RC_OK(rc))
		{
			rc = pRec->setUnicode(pvField, (FLMUNICODE*)pFieldName);
			if (RC_OK(rc))
			{
				rc = pRec->insert(pvField, INSERT_LAST_CHILD, FLM_TYPE_TAG, FLM_TEXT_TYPE, &pvField);
				if (RC_OK(rc))
				{
					if (flmType != FLM_UNDEFINED_TYPE)
					{
						rc = pRec->setUnicode(pvField, (FLMUNICODE*)cs_flaim_type_name[flmType]);
						if (RC_OK(rc))
						{
							rc = FlmRecordAdd(hFlaim, FLM_DICT_CONTAINER, pFieldId, pRec, 0);
							if (RC_OK(rc))
							{
								rc = m_NameTable.addTag((FLMUNICODE*)pFieldName, 0, *pFieldId, 0, 0, TRUE);
							}
						}
					}
					else
					{
						rc = FERR_BAD_FIELD_TYPE;
					}
				}
			}
		}
		pRec->Release();
	}
	else
	{
		rc = FERR_MEM;
	}

	return (rc);
} // CSPDB::registerField()


RCODE CSPDB::registerIndexArray(HFDB hFlaim, CS_FIELD_DEF *indexTable, FLMINT count)
{
	RCODE				rc = FERR_OK;
	int					i;

	rc = FlmDbTransBegin(hFlaim, FLM_UPDATE_TRANS, 255);
	if (RC_OK(rc))
	{
		for (i = 0; i < count; ++i)
		{
			rc = AddIndex(hFlaim, indexTable[i].name, indexTable[i].id);
			if (RC_BAD(rc))
			{
				break;
			}
		}
		if (RC_OK(rc))
		{
	 		rc = FlmDbTransCommit(hFlaim);
		}
		else
		{
			rc = FlmDbTransAbort(hFlaim);
		}
	}

	return (rc);
} // CSPDB::registerIndexArray()


RCODE CSPDB::AddIndex(HFDB hFlaim, FLMUNICODE *pFieldName, FLMUINT fieldId)
{
	RCODE			rc = FERR_OK;
	FlmRecord		*pRec = NULL;
	void			*pvIndex;
	void			*pvField = 0;
	void			*pvKey = 0;
	FLMUNICODE*		pIndexName;
	int				nameLen;
	int				buffLen;
	FLMUINT			indexId = 0;
	
 	pRec = new FlmDefaultRec();
 	if (pRec != NULL)
 	{
 		// Add the index.
 		rc = pRec->insertLast(0, FLM_INDEX_TAG, FLM_TEXT_TYPE, &pvIndex);
 		if (RC_OK(rc))
 		{
			// Create the index name.
			nameLen = f_unilen(pFieldName);
			buffLen = nameLen + sizeof(nameSuffex) + 1;
			pIndexName = new FLMUNICODE[buffLen];
			if (pIndexName)
			{
				f_unicpy(pIndexName, pFieldName);
				f_unicpy(pIndexName + nameLen, (FLMUNICODE*)nameSuffex);
				rc = pRec->setUnicode(pvIndex, (FLMUNICODE*)pIndexName);
				if (RC_OK(rc))
				{
					// Add the key.
					rc = pRec->insert(pvIndex, INSERT_LAST_CHILD, FLM_KEY_TAG, FLM_TEXT_TYPE, &pvKey);
					if (RC_OK(rc))
					{
						// Add the collection
						rc = pRec->insert(pvKey, INSERT_LAST_CHILD, FLM_FIELD_TAG, FLM_NUMBER_TYPE, &pvField);
						if (RC_OK(rc))
						{
							rc = pRec->setINT(pvField, CS_Id_CollectionId);
						}
						// Add the field.
						rc = pRec->insert(pvKey, INSERT_FIRST_CHILD, FLM_FIELD_TAG, FLM_NUMBER_TYPE, &pvField);
						if (RC_OK(rc))
						{
							rc = pRec->setINT(pvField, fieldId);
							if (RC_OK(rc))
							{
								rc = FlmRecordAdd(hFlaim, FLM_DICT_CONTAINER, &indexId, pRec, 0);
								if (RC_OK(rc))
								{
									rc = m_NameTable.addTag((FLMUNICODE*)pIndexName, 0, indexId, 0, 0, TRUE);
								}
							}
						}
						else
						{
							rc = FERR_BAD_FIELD_TYPE;
						}
					}
				}
				delete [] pIndexName;
			}
 		}
 	  	pRec->Release();
 	}
 	else
 	{
 		rc = FERR_MEM;
 	}

	return (rc);
} // CSPDB::AddIndex()


RCODE CSPDB::GetIndexId(FLMUNICODE *pFieldName, FLMUINT fieldID, FLMUINT *pId)
{
	RCODE rc = FERR_OK;

	*pId = 0;

	if (fieldID < indexTSize)
		*pId = pIndexIDTable[fieldID];
	
	if (*pId == 0)
	{
		// Create the index name.
		int nameLen = f_unilen(pFieldName);
		int buffLen = nameLen + sizeof(nameSuffex) + 1;
		FLMUNICODE *pIndexName = new FLMUNICODE[buffLen];
		if (pIndexName)
		{
			f_unicpy(pIndexName, pFieldName);
			f_unicpy(pIndexName + nameLen, (FLMUNICODE*)nameSuffex);
			if (!m_NameTable.getFromTagName(pIndexName, 0, pId, 0, 0))
			{
				rc = FERR_BAD_FIELD_NUM;
			}
			if (fieldID < indexTSize)
				pIndexIDTable[fieldID] = *pId;
			delete [] pIndexName;
		}
	}
	return rc;
} // CSPDB::GetIndexId()



//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CSPStore::CSPStore(CSPDB *pDB) :
	m_hFlaim(0),
	m_connected(0)
	//m_InTrans(false)
{
	if (pDB == 0) 
	{
		m_pDB = new CSPDB();
	}
	else
	{
		m_pDB = pDB;
		m_pDB->AddRef();
	}
	m_flaimDbPath[0]=L'\0';
}

CSPStore::~CSPStore()
{
	m_pDB->Release();
}


RCODE CSPStore::_CREATE(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB)
{
	RCODE			rc = FERR_OK;
	PCSStore		pStore;

	pStore = new CSPStore(*ppDB);
	if (pStore != NULL)
	{
		rc = pStore->CreateStore(pStorePath);
		if (RC_BAD(rc))
		{
			delete pStore;
			pStore = NULL;
		}
		else
		{
			*ppStore = pStore;
			*ppDB = pStore->m_pDB;
		}
	}
	
	return (rc);
}

RCODE CSPStore::_OPEN(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB)
{
	RCODE			rc = FERR_OK;
	PCSStore		pStore;

	pStore = new CSPStore(*ppDB);
	if (pStore != NULL)
	{
		rc = pStore->OpenStore(pStorePath);
		if (RC_BAD(rc))
		{
			delete pStore;
			pStore = NULL;
		}
		else
		{
			*ppStore = pStore;
			*ppDB = pStore->m_pDB;
		}

	}

	return (rc);
}

RCODE CSPStore::CreateStore(char *pStorePath)
{
	RCODE			rc = FERR_OK;
	char*			pDbPath;
	
	// Create the path to the database.
	pDbPath = setupDbPath(pStorePath);

	if (pDbPath)
	{
		// Create the New Store.
		rc = FlmDbCreate(
			(FLMBYTE*)pDbPath,
			NULL,
			NULL,			//default pRflDir
			FO_SHARE,
			NULL,
			NULL,
			NULL,
			&m_hFlaim);

		if (RC_OK(rc))
		{
			// Now setup the store.
			rc = m_pDB->initializeDB(m_hFlaim, true);

			if (RC_OK(rc))
			{
				m_connected = true;
			}
			else 
			{
				FlmDbRemove(
					(FLMBYTE*)pDbPath,
					NULL,
					NULL,
					true);
			}
		}
	}

	return (rc);
} // CSPStore::CreateStore()


RCODE CSPStore::DeleteStore(FLMBYTE* dbPath)
{
	RCODE			rc = FERR_OK;

	// Make sure we are disconnected.
	//Close();

	rc = FlmDbRemove(
		dbPath,
		NULL,
		NULL,
		true);

	return (rc);
} // CSPStore::DeleteStore()




/**
 * Method to connect to the store.
 * 
 * @param storePath The full path to the store to connect to.
 * @return HRESULT
 */
RCODE CSPStore::OpenStore( char *pStorePath)
{
	RCODE				rc = FERR_OK;
	char*				pDbPath;

	// Convert the path to current code page and then create the data base.
	pDbPath = setupDbPath(pStorePath);

	if (pDbPath)
	{

// OS X uses the new flaim which requires the NULL be passed
#ifdef OSX	
		rc = FlmDbOpen(
			(FLMBYTE*)pDbPath,
			NULL,
			NULL,			//default pRflDir
			FO_SHARE,
			NULL,
			&m_hFlaim);
#else
		rc = FlmDbOpen(
			(FLMBYTE*)pDbPath,
			NULL,
			NULL,			//default pRflDir
			FO_SHARE,
			//NULL,
			&m_hFlaim);
#endif

		if (RC_OK(rc))
		{
			// Now setup the store.
			rc = m_pDB->initializeDB(m_hFlaim, false);
			if (RC_OK(rc))
			{
				m_connected = true;
			}
		}
	}
	return (rc);
} // CSPStore::OpenStore()


void CSPStore::Close()
{
	if (m_connected)
	{
		if (m_hFlaim != 0)
		{
			FlmDbClose(&m_hFlaim);
			m_hFlaim = 0;
		}
		m_connected = false;
	}
} // CSPStore::disconnect()


char* CSPStore::setupDbPath(char *pDbPath)
{
	strcpy(m_flaimDbPath, pDbPath);
	return (m_flaimDbPath);
} // CSPStore::setupDbPath()

RCODE CSPStore::RegisterField(FLMUNICODE *pFieldName, FLMUINT type, FLMUINT* pFieldId)
{
	return (m_pDB->RegisterField(m_hFlaim, pFieldName, type, pFieldId));
} // CSPStore::RegisterField()

RCODE CSPStore::AddIndex(FLMUNICODE *pFieldName, FLMUINT fieldId)
{
	return (m_pDB->AddIndex(m_hFlaim, pFieldName, fieldId));
} // CSPStore::AddIndex()



RCODE CSPStore::BeginTrans()
{
	RCODE rc = FERR_OK;

	//if (!m_InTrans)
	//{
		rc = FlmDbTransBegin(m_hFlaim, FLM_UPDATE_TRANS, 255);
	//	if (RC_OK(rc))
	//	{
	//		m_InTrans = true;
	//	}
	//}
	//else
	//{
	//	rc = FERR_TRANS_ACTIVE;
	//}
	return (rc);
} // CSPStore::BeginTrans()


void CSPStore::AbortTrans()
{
	//if (m_InTrans)
	//{
		FlmDbTransAbort(m_hFlaim);
	//	m_InTrans = false;
	//}
} // CSPStore::AbortTrans()


RCODE CSPStore::EndTrans()
{
	RCODE rc = FERR_OK;
	//if (m_InTrans)
	//{
		rc = FlmDbTransCommit(m_hFlaim);
		if (RC_BAD(rc))
		{
			FlmDbTransAbort(m_hFlaim);
		}

	//	m_InTrans = false;
	//}
	//else
	//{
	//	rc = FERR_NO_TRANS_ACTIVE;
	//}
	return (rc);
} // CSPStore::EndTrans()


RCODE CSPStore::NameToId(FLMUNICODE *pName, FLMUINT *pId)
{
	RCODE rc = FERR_OK;
//	if (!m_pDB->m_NameTable.getFromTagTypeAndName((FLMUNICODE*)pName, 0, SIMIAS_TYPE, pId, 0))
	if (!m_pDB->m_NameTable.getFromTagName((FLMUNICODE*)pName, 0, pId, 0, 0))
	{
		rc = FERR_BAD_FIELD_NUM;
	}

	return (rc);
} // CSPStore::NameToId()

FLMBOOL CSPStore::IdToName(FLMUINT id, FLMUNICODE *pName, int size)
{
	return (m_pDB->m_NameTable.getFromTagNum(id, pName, NULL, size, NULL, NULL));
} // CSPStore::IdToName()


CSPStoreObject *CSPStore::CreateObject(FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FLMBOOL *pNewObject, FLMINT flmId)
{
	FlmRecord			*pRec = NULL;
	CSPStoreObject		*pObject = NULL;


	// Make sure the object does not exist.
	pRec = FindObject(pId);

	if (pRec)
	{
		//if (openExisting)
		{
			FLMUINT drn = pRec->getID();
			pRec->clear();
			pRec->setID(drn);
			pObject = new CSPStoreObject(this, pName, pId, pType, pRec);
			*pNewObject = 0;
		}
		//else
		//{
		//	pRec->Release();
		//	pRec = NULL;
		//	return (pObject);
		//}
	}
	else
	{
		pObject = new CSPStoreObject(this, pName, pId, pType, flmId);
		*pNewObject = 1;
	}

	return (pObject);
} // CSPStore::CreateObject()


RCODE CSPStore::DeleteObject(FLMUNICODE *pId, int *pFlmId)
{
	CSPStoreObject	*pObject;
	FlmRecord		*pRec;
	RCODE			rc = FERR_NOT_FOUND;

	pRec = FindObject(pId);
	if (pRec)
	{
		*pFlmId = pRec->getID();
		pObject = new CSPStoreObject(this, pRec);
		pObject->Delete();
		rc = pObject->Flush();
		delete pObject;
	}
	return (rc);
} // CSPStore::DeleteObject()


FlmRecord * CSPStore::FindObject(FLMUNICODE *pId)
{
	RCODE			rc = FERR_OK;
	HFCURSOR		cursor = 0;
	FlmRecord	*pRec = NULL;
	FlmRecord	*pRWRec = NULL;
	FLMUINT		fieldId;
	FLMUINT		count;

	rc = NameToId(CS_Name_GUID, &fieldId);
	if (RC_OK(rc))
	{
		rc = FlmCursorInit(m_hFlaim, FLM_DATA_CONTAINER, &cursor);
		if (RC_OK(rc))
		{
			rc = FlmCursorAddField(cursor, fieldId, 0);
			if (RC_OK(rc))
			{
				rc = FlmCursorAddOp(cursor, FLM_EQ_OP, 0);
				if (RC_OK(rc))
				{
					rc = FlmCursorAddValue(cursor, FLM_UNICODE_VAL, pId, 0);
					if (RC_OK(rc))
					{
						rc = FlmCursorRecCount(cursor, &count);
						if (RC_OK(rc) && count != 0)
						{
							rc = FlmCursorNext(cursor, &pRec);
							if (RC_OK(rc))
							{
								pRWRec = pRec->copy();
								pRec->Release();
								pRec = NULL;
							}
						}
					}
				}
			}
			FlmCursorFree(&cursor);
		}
	}
	return (pRWRec);
} // CSPStore::FindObject()


CSP_TYPE CSPStore::StringToType(FLMUNICODE *pTypeName)
{
	CSP_TYPE	cspType = CSP_Type_Undefined;

	// Get the CSPType.
	switch (pTypeName[0])
	{
	case L'B':
		{
			// Check for Boolean.
			if (pTypeName[1] == L'o')
			{
				// Boolean
				cspType = CSP_Type_Bool;
			}
			else if (pTypeName[1] == L'y')
			{
				// Byte
				cspType = CSP_Type_UI1;
			}
			break;
		}

	case L'C':
		// Char
		cspType = CSP_Type_Char;
		break;

	case L'D':
		cspType = CSP_Type_DateTime;
		break;
	case L'I':
		{
			switch (pTypeName[3])
			{
			case L'1':
				// Int16
				cspType = CSP_Type_I2;
				break;
			case L'3':
				// Int32
				cspType = CSP_Type_I4;
				break;
			case L'6':
				// Int64
				cspType = CSP_Type_I8;
				break;
			}
			break;
		}
	case L'R':
		// RelationShip
		cspType = CSP_Type_Relationship;
		break;
	case L'S':
		{
			switch (pTypeName[1])
			{
			case L't':
				// String
				cspType = CSP_Type_String;
				break;
			case L'B':
				// SByte
				cspType = CSP_Type_I1;
				break;
			case L'i':
				// Single
				cspType = CSP_Type_Float;
				break;
			}
		}
		break;
	case L'T':
		// TimeSpan
		cspType = CSP_Type_TimeSpan;
		break;
	case L'U':
		{
			if (pTypeName[1] == L'r')
			{
				// Uri
				cspType = CSP_Type_Uri;
			}
			else
			{
				switch (pTypeName[4])
				{
				case L'1':
					// UInt16
					cspType = CSP_Type_UI2;
					break;
				case L'3':
					// UInt32
					cspType = CSP_Type_UI4;
					break;
				case L'6':
					// UInt64
					cspType = CSP_Type_UI8;
					break;
				}
			}
			break;
		}
		break;
	case L'X':
		// Xml
		cspType = CSP_Type_Xml;
		break;
	}

	return (cspType);
} // CSPStore::StringToFlaimType()


RCODE CSPStore::GetObject(FLMUNICODE *pProperty, FLMUNICODE *pValue, int* pnChars, FLMUNICODE *pBuffer)
{
	RCODE			rc = FERR_OK;
	HFCURSOR		cursor = 0;
	FlmRecord		*pRec = NULL;
	FLMUINT			fieldId;
	FLMUINT			count;
	CSPStoreObject *pObject = NULL;
	int				buffSize = *pnChars;
	int				nChars = *pnChars;
	int				len = 0;

	*pnChars = 0;
	rc = NameToId(pProperty, &fieldId);
	if (RC_OK(rc))
	{
		rc = FlmCursorInit(m_hFlaim, FLM_DATA_CONTAINER, &cursor);
		if (RC_OK(rc))
		{
			rc = FlmCursorAddField(cursor, fieldId, 0);
			if (RC_OK(rc))
			{
				rc = FlmCursorAddOp(cursor, FLM_EQ_OP,	0);
				if (RC_OK(rc))
				{
					rc = FlmCursorAddValue(cursor, FLM_UNICODE_VAL,	pValue, 0);
					if (RC_OK(rc))
					{
						rc = FlmCursorRecCount(cursor, &count);
						if (RC_OK(rc) && count != 0)
						{
							rc = FlmCursorNext(cursor,	&pRec);
						}
						else
						{
							// Not found
							rc = FERR_NOT_FOUND;
						}
					}
				}
			}
			FlmCursorFree(&cursor);
		}
	}

	if (RC_OK(rc))
	{
		pObject = new CSPStoreObject(this, pRec);
		if (pObject != NULL)
		{
			if (pObject->GetXmlSize() < buffSize)
			{
				int			endTagLen = f_unilen((FLMUNICODE*)XmlObjectListEndString);
				FLMUNICODE *pBuff = pBuffer;
			
				if ((len = flmstrcpy(pBuff, (FLMUNICODE*)XmlObectListString, nChars)) != -1)
				{
					nChars -= len + endTagLen;
					pBuff += len;
					if ((len = pObject->ToXML(pBuff, nChars, true, false)) != -1)
					{
						nChars -= len;
						pBuff += len;					
						if ((len = flmstrcpy(pBuff, (FLMUNICODE*)XmlObjectListEndString, nChars + endTagLen)) != -1)
						{
							nChars++;
							*pnChars = buffSize - nChars + 1;
						}
					}
				}
			}
			else
			{
				*pnChars = pObject->GetXmlSize() + 1;
				rc = FERR_MEM;
			}
			delete pObject;
		}
	}
	return (rc);
}


RCODE CSPStore::Search(FLMUNICODE *pCollectionId, FLMUNICODE *pProperty, FLMINT op, FLMUNICODE *pValue, FLMUNICODE* pType, FLMBOOL caseSensitive, FLMUINT *pCount, CSPObjectIterator **ppIterator)
{
	RCODE				rc = FERR_OK;
	HFCURSOR			cursor = 0;
	FLMUINT				fieldId;
	CSPObjectIterator	*pIterator = 0;
	CSPValue			*pCspValue;
	FLMBOOL				includeColId = true;


	pCspValue = CSPStoreObject::CreateProperty(pValue, pProperty, CSPStore::StringToType(pType));

	if (pCspValue)
	{
		rc = NameToId(pProperty, &fieldId);
		if (RC_OK(rc))
		{
			rc = FlmCursorInit(m_hFlaim, FLM_DATA_CONTAINER, &cursor);
			if (RC_OK(rc))
			{
				FLMUINT indexId;
				// Setup the index to use
				if (RC_OK(m_pDB->GetIndexId(pProperty, fieldId, &indexId)))
				{
					FlmCursorConfig(cursor, FCURSOR_SET_FLM_IX, (void *)indexId, NULL);
				}

				if (caseSensitive)
				{
					rc = FlmCursorSetMode(cursor, FLM_CASE | FLM_WILD);
				}
				rc = FlmCursorAddField(cursor, fieldId, 0);
				if (RC_OK(rc))
				{
					rc = FlmCursorAddOp(cursor, (QTYPES)op,	0);
					if (RC_OK(rc))
					{
						rc = FlmCursorAddValue(cursor, pCspValue->GetSearchType(),	pCspValue->SearchVal(), pCspValue->SearchSize());
						if (pCollectionId && RC_OK(rc))
						{
							includeColId = false;
							rc = FlmCursorAddOp(cursor, FLM_AND_OP, 0);
							if (RC_OK(rc))
							{
								rc = FlmCursorAddField(cursor, CS_Id_CollectionId, 0);
								if (RC_OK(rc))
								{
									rc = FlmCursorAddOp(cursor, FLM_EQ_OP,	0);
									if (RC_OK(rc))
									{
										rc = FlmCursorAddValue(cursor, FLM_UNICODE_VAL,	pCollectionId, 0);
									}
								}
							}
						}
						if (RC_OK(rc))
						{
							rc = FlmCursorRecCount(cursor, pCount);
							pIterator = new CSPObjectIterator(this, cursor, *pCount, includeColId);
						}
					}
				}
				FlmCursorFree(&cursor);
			}
		}
		else
		{
			// No properties exist with the specified name.
			rc = FERR_OK;
			*pCount = 0;
			pIterator = new CSPObjectIterator(this, NULL, *pCount, includeColId);
		}
		delete pCspValue;
	}

	*ppIterator = pIterator;
	return (rc);
} // CSPStore::Search()

