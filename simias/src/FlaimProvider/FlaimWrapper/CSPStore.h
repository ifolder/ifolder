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
// CSPStore.h: interface for the CSPStore class.
//
//////////////////////////////////////////////////////////////////////

#ifndef _CSPSTORE_H_
#define _CSPSTORE_H_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifdef _MSC_VER
#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers

#include <windows.h>
#endif

#include "string"
using namespace std;
#include "flaim.h"
//#include "flaimsys.hpp"
//typedef wstring flmstring;

class CSPStore;
typedef CSPStore *PCSStore;
class CSPStoreObject;
class CSPropertyIterator;
class CSPObjectIterator;

typedef enum 
{
	// These cannot be reordered.
	CSP_Type_Undefined,
	CSP_Type_String,
	CSP_Type_I1,
	CSP_Type_UI1,    
	CSP_Type_I2,     
	CSP_Type_UI2,    
	CSP_Type_I4,     
	CSP_Type_UI4,    
	CSP_Type_I8,     
	CSP_Type_UI8,    
	CSP_Type_Char,
	CSP_Type_Float,  
	CSP_Type_Bool,
	CSP_Type_DateTime,
	CSP_Type_Uri,
	CSP_Type_Xml,
	CSP_Type_TimeSpan,
	CSP_Type_Relationship,
	// Not exposed to upper layers.
	CSP_Type_Int,

	// Add new type above here.
	CSP_Type_Max
} CSP_TYPE;

#define CS_Name_Node_Type			(FLMUNICODE*)L"Object Type"
#define CS_Type_Node_Type			CSP_Type_String
#define CS_Id_Node_Type				1
#define CS_Name_Property_Type		(FLMUNICODE*)L"Property Type"
#define CS_Type_Property_Type		CSP_Type_Int
#define CS_Id_Property_Type			2
#define CS_Name_Display_Name		(FLMUNICODE*)L"Display Name"
#define CS_Type_Display_Name		CSP_Type_String
#define CS_Id_Display_Name			3
#define CS_Name_GUID				(FLMUNICODE*)L"GUID"
#define CS_Type_GUID				CSP_Type_String
#define CS_Id_GUID					4
#define CS_Name_CollectionId		(FLMUNICODE*)L"CollectionId"
#define CS_Type_CollectionId		CSP_Type_String
#define CS_Id_CollectionId			5
#define CS_Name_Size				(FLMUNICODE*)L"CsObjectSize"
#define CS_Type_Size				CSP_Type_Int
#define	CS_Id_Size					6
#define CS_Name_PFlags				(FLMUNICODE*)L"Property Flags"
#define CS_Type_PFlags				CSP_Type_Int
#define CS_Id_PFlags				7


#define TYPE_LEVEL 0
#define USER_PROP_LEVEL 1
#define SYS_PROP_LEVEL 1

#define FERR -1
#define MAX_PROPERTY_NAME	260

#ifndef _MSC_VER
#define MAX_PATH			260
#define PATH_SEPERATOR		L'/'
#define swnprintf			swprintf
typedef unsigned long long ULONGLONG;
typedef long long 			LONGLONG;
#define _LL(n)					n ## LL
#define _ULL(n)				n ## ULL
#define _FORMAT_LL			"%lld"
#define _FORMAT_LLU			"%llu"
#else
#define PATH_SEPERATOR		L'\\'
#define swnprintf			_snwprintf
typedef __int64				LONGLONG;
#define _LL(n)					n ## I64
#define _ULL(n)				n ## UI64
#define _FORMAT_LL			"%I64d"
#define _FORMAT_LLU			"%I64u"
#endif


#define CSP_Type_String_String (FLMUNICODE*)L"String"
#define CSP_Type_I1_String (FLMUNICODE*)L"SByte"
#define CSP_Type_UI1_String (FLMUNICODE*)L"Byte"
#define CSP_Type_I2_String (FLMUNICODE*)L"Int16"
#define CSP_Type_UI2_String (FLMUNICODE*)L"UInt16"
#define CSP_Type_I4_String (FLMUNICODE*)L"Int32"
#define CSP_Type_UI4_String (FLMUNICODE*)L"UInt32"
#define CSP_Type_I8_String (FLMUNICODE*)L"Int64"
#define CSP_Type_UI8_String (FLMUNICODE*)L"UInt64"
#define CSP_Type_Char_String (FLMUNICODE*)L"Char"
#define CSP_Type_Float_String (FLMUNICODE*)L"Single"
#define CSP_Type_Bool_String (FLMUNICODE*)L"Boolean"
#define CSP_Type_DTime_String (FLMUNICODE*)L"DateTime"
#define CSP_Type_Uri_String (FLMUNICODE*)L"Uri"
#define CSP_Type_Xml_String (FLMUNICODE*)L"XmlDocument"
#define CSP_Type_TimeSpan_String (FLMUNICODE*)L"TimeSpan"
#define CSP_Type_Relationship_String (FLMUNICODE*)L"Relationship"


#define XmlObectListString		L"<ObjectList>"
#define	XmlObectNameString 		L"<Object name=\""
#define	XmlIdString 			L"\" id=\""
#define	XmlTypeString 			L"\" type=\""
#define	XmlColIdString 			L"\" cid=\""
#define XmlEndTag				L"\">"
#define XmlEndTagNoChildren		L"\"/>"
#define	XmlObjectEndString 		L"</Object>"
#define	XmlObjectListEndString 	L"</ObjectList>"
#define	XmlPropertyNameString 	L"<Property name=\""
#define	XmlTypeString 			L"\" type=\""
#define XmlFlagsString			L"\" flags=\""
#define XmlQEndTag				L"\">"
#define	XmlPropertyEndString	L"</Property>"


#define FLM_UNDEFINED_TYPE 0xffff

extern FLMUNICODE *CSPTypeStringString;
extern FLMUNICODE *CSPTypeI1String;
extern FLMUNICODE *CSPTypeUI1String;
extern FLMUNICODE *CSPTypeI2String;
extern FLMUNICODE *CSPTypeUI2String;
extern FLMUNICODE *CSPTypeI4String;
extern FLMUNICODE *CSPTypeUI4String;
extern FLMUNICODE *CSPTypeI8String;
extern FLMUNICODE *CSPTypeUI8String;
extern FLMUNICODE *CSPTypeCharString;
extern FLMUNICODE *CSPTypeFloatString;
extern FLMUNICODE *CSPTypeBoolString;
extern FLMUNICODE *CSPTypeDTimeString;
extern FLMUNICODE *CSPTypeUriString;
extern FLMUNICODE *CSPTypeXmlString;
extern FLMUNICODE *CSPTypeTimeSpanString;
extern FLMUNICODE *CSPTypeRelationshipString;

extern int flmstrcpy(FLMUNICODE *pDest, FLMUNICODE *pSrc, int size);
extern int flmstrcpyesc(FLMUNICODE *pDest, FLMUNICODE *pSrc, int size);
extern void printflmstring(FLMUNICODE* pString);
extern FLMUNICODE* flmstrstr(FLMUNICODE* s1, FLMUNICODE* s2);
extern FLMUINT	CSPTypeToFlaimTypeArray[];
#define CSPTypeToFlaimType(t)	(t >= CSP_Type_Max) ? FLM_UNDEFINED_TYPE : CSPTypeToFlaimTypeArray[t]

#include "CSPType.h"
#include "CSPStoreObject.h"
#include "CSPropertyIterator.h"
#include "CSPObjectIterator.h"

typedef struct _CS_FIELD_DEF_
{
	FLMUINT		id;
	FLMUNICODE		*name;
	FLMUINT		type;
} CS_FIELD_DEF, *PCS_FIEDL_DEF;

class CSPDB
{
public:
	CSPDB();
	virtual ~CSPDB();
	void AddRef();
	void Release();
	void CSPDB::SetupNameTable(HFDB hFlaim);
	RCODE initializeDB(HFDB hFlaim, FLMBOOL created);
	RCODE RegisterField(HFDB hFlaim, FLMUNICODE *pFieldName, FLMUINT type, FLMUINT* pFieldId);
	RCODE AddIndex(HFDB hFlaim, FLMUNICODE *pFieldName, FLMUINT fieldId);
	RCODE CSPDB::GetIndexId(FLMUNICODE *pFieldName, FLMUINT fieldID, FLMUINT *pId);

private:
	RCODE registerFieldArray(HFDB hFlaim, CS_FIELD_DEF *fieldTable, FLMINT count);
	RCODE registerIndexArray(HFDB hFlaim, CS_FIELD_DEF *indexTable, FLMINT count);
	RCODE addIndex(FLMUNICODE *pFieldName, FLMUINT fieldId);

public:
	F_NameTable		m_NameTable;
	FLMINT			m_RefCount;
	FLMBOOL			m_flaimInitialized;
	static wchar_t	nameSuffex[];
	const static int indexTSize = 1024;
	FLMUINT			*pIndexIDTable;
};

class CSPStore  
{
private:
	CSPStore(CSPDB *pDB);

public:
	virtual ~CSPStore();

	static RCODE _CREATE(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB);
	static RCODE _OPEN(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB);
	static RCODE DeleteStore(FLMBYTE* dbPath);
	void Close();
	HFDB GetDB() {return (m_hFlaim);};
	RCODE NameToId(FLMUNICODE *pName, FLMUINT *pId);
	FLMBOOL IdToName(FLMUINT id, FLMUNICODE *pName, int size);
	CSPStoreObject *CreateObject(FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FLMBOOL *pNewObject, FLMINT flmId);
	FlmRecord * FindObject(FLMUNICODE *pId);
	static CSP_TYPE StringToType(FLMUNICODE *pTypeName);
	RCODE BeginTrans();
	void AbortTrans();
	RCODE EndTrans();
	RCODE GetObject(FLMUNICODE *pProperty, FLMUNICODE *pValue, int* pnChars, FLMUNICODE *pBuffer);
	RCODE Search(FLMUNICODE *pCollectionId, FLMUNICODE *pProperty, FLMINT op, FLMUNICODE *pValue, FLMUNICODE *pType, FLMBOOL caseSensitive, FLMUINT *pCount, CSPObjectIterator **ppIterator);
	RCODE DeleteObject(FLMUNICODE *pId, int *pFlmId);
	RCODE RegisterField(FLMUNICODE *pFieldName, FLMUINT type, FLMUINT* pFieldId);
	RCODE AddIndex(FLMUNICODE *pFieldName, FLMUINT fieldId);


private:
	RCODE CreateStore(char *pStorePath);
	RCODE OpenStore( char *pStorePath);
	char* CSPStore::setupDbPath(char *pDbPath);

	HFDB			m_hFlaim;
	FLMBOOL			m_connected;
	//FLMBOOL			m_InTrans;
	char			m_flaimDbPath[MAX_PATH];
	CSPDB			*m_pDB;
};

#endif // _CSPSTORE_H_
