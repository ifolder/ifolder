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
#ifndef _CSTYPE_H_
#define _CSTYPE_H_

#define MAX_INT_STRING 21

class CSUtil
{
public:
	static LONGLONG funitoll(FLMUNICODE *pll)
	{
		char pmbcsString[MAX_INT_STRING + 1];
		LONGLONG ll;
		int i = 0;
	
		while ((pll[i] != 0) && (i < MAX_INT_STRING)) 
		{		
			pmbcsString[i] = (char)pll[i];
			i++;
		}
		pmbcsString[i] = 0;
		sscanf(pmbcsString, _FORMAT_LL, &ll);

		return ll;
	} // funitoll()

static LONGLONG funitoull(FLMUNICODE *pll)
	{
		char pmbcsString[MAX_INT_STRING + 1];
		LONGLONG ll;
		int i = 0;
	
		while ((pll[i] != 0) && (i < MAX_INT_STRING)) 
		{		
			pmbcsString[i] = (char)pll[i];
			i++;
		}
		pmbcsString[i] = 0;
		sscanf(pmbcsString, _FORMAT_LLU, &ll);

		return ll;
	} // funitoull()

	static FLMINT lltofuni(LONGLONG ll, FLMUNICODE *pll, FLMINT size)
	{
		char pmbcsString[MAX_INT_STRING + 1];
		int i=0;
		int len;

		len = sprintf(pmbcsString, _FORMAT_LL, ll);

		if (size > len)
		{
			while ((pmbcsString[i] != 0) && ( i < MAX_INT_STRING))
			{
				pll[i] = pmbcsString[i];
				i++;
			}
			pll[i] = 0;
		}

		return (len);
	} // lltofuni()

	static FLMINT ulltofuni(LONGLONG ll, FLMUNICODE *pll, FLMINT size)
	{
		char pmbcsString[MAX_INT_STRING + 1];
		int i=0;
		int len;

		len = sprintf(pmbcsString, _FORMAT_LLU, ll);

		if (size > len)
		{
			while ((pmbcsString[i] != 0) && ( i < MAX_INT_STRING))
			{
				pll[i] = pmbcsString[i];
				i++;
			}
			pll[i] = 0;
		}

		return (len);
	} // ulltofuni()
};

// Base class for all builtin types.
// Derived classes CSPBool, CSPInt, CSPFloat, CSPString, CSPBinary
class CSPValue
{
public:
	CSPValue(FLMUNICODE *pName, FLMUNICODE *pType)
	{
		m_pType = pType;
		m_pName = new FLMUNICODE[f_unilen(pName) + 1];
		if (m_pName)
		{
			f_unicpy(m_pName, pName);
		}
		else
		{
			m_pName = NULL;
		}

		m_flags = 0;
	}

	virtual ~CSPValue()
	{
		if (m_pName)
		{
			delete [] m_pName;
		}
	};
	virtual int ToString(FLMUNICODE* pString, FLMUINT size) = 0;
	virtual RCODE ToFlaim(FlmRecord *pRec, void * pvField) = 0;
	virtual RCODE FromFlaim(FlmRecord *pRec, void *pvField) = 0;
	virtual FLMUINT GetFlaimType() = 0;
	virtual QTYPES GetSearchType() = 0;
	virtual void * SearchVal() = 0;
	virtual int SearchSize() = 0;
	virtual int StringSize() = 0;
	int ToXml(FLMUNICODE* pOriginalBuffer, int nChars)
	{
		int charsWritten = nChars;
		int len;
		FLMUNICODE* pBuffer = pOriginalBuffer;

		if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlPropertyNameString, nChars)) != -1)
		{
			nChars -= len;
			pBuffer += len;
			if ((len = flmstrcpyesc(pBuffer, m_pName, nChars)) != -1)
			{
				nChars -= len;
				pBuffer += len;
				if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlTypeString, nChars)) != -1)
				{
					nChars -= len;
					pBuffer += len;
					if ((len = flmstrcpy(pBuffer, m_pType, nChars)) != -1)
					{
						nChars -= len;
						pBuffer += len;
						if (m_flags)
						{
							// Now set the Flags.
							if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlFlagsString, nChars)) != -1)
							{
								nChars -= len;
								pBuffer += len;
								if ((len = CSUtil::lltofuni(m_flags, pBuffer, nChars)) != -1)
								{
									nChars -= len;
									pBuffer += len;
								}
							}
						}
						if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlQEndTag, nChars)) != -1)
						{
							nChars -= len;
							pBuffer += len;
							if ((len = ToString(pBuffer, nChars)) != -1)
							{
								nChars -= len;
								pBuffer += len;
								if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlPropertyEndString, nChars)) != -1)
								{
									nChars -= len;
									pBuffer += len;
								}
							}
						}
					}
				}
			}
		}
		return (len != -1 ? charsWritten - nChars : -1);
	}

	void SetFlags(FLMUINT flags)
	{
		m_flags = flags;
	}

	FLMUNICODE *m_pType;
	FLMUNICODE *m_pName;
	FLMUINT		m_flags;
};


// The CSPBool type is a boolean type. It is stored in flaim as a
// binary byte. 0 = false; 1 = true;
class CSPBool : public CSPValue
{
private:
	unsigned char 		m_Value;

public:
	CSPBool(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeBoolString)
	{
		m_Value = pString[0] - L'0';
	}

	CSPBool(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeBoolString)
	{
		FromFlaim(pRec, pvField);
	}

	virtual ~CSPBool()
	{
	}

	int ToString(FLMUNICODE* pString, FLMUINT size)
	{
		if (size > 2) 
		{
			if (m_Value)
			{
				f_unicpy(pString, (FLMUNICODE*)L"1");
			}
			else
			{
				f_unicpy(pString, (FLMUNICODE*)L"0");
			}
			return (1);
		}
		return (-1);
	}


	RCODE ToFlaim(FlmRecord *pRec, void *pvField)
	{
		return (pRec->setBinary(pvField, &m_Value, sizeof(m_Value)));
	}

	void* SearchVal()
	{
		return (&m_Value);
	}

	int SearchSize()
	{
		return (sizeof(m_Value));
	}

	QTYPES GetSearchType()
	{
		return (FLM_BINARY_VAL);
	}

	RCODE FromFlaim(FlmRecord *pRec, void *pvField)
	{
		FLMUINT buffSize = sizeof(m_Value);
		return (pRec->getBinary(pvField, &m_Value, &buffSize));
	}

	FLMUINT GetFlaimType()
	{
		return (FLM_BINARY_TYPE);
	}

	int StringSize()
	{
		return (1);
	}
}; // CSPBool()


// All integer sizes will be stored in flaim with the following structure.
// Integers will be stored in this format so that compares will work in flaim
// The string is stored for runtime optimization. It only needs to be created
// at store time.

typedef struct _CSPIntegerStruct_
{
	char			sign;		// 0 = negative, 1 = positive.
	char			value[8];	// Used to hold the MSB representation of the value.
} CSPIntegerStruct, *PCSPIntegerStruct;


class CSPInt : public CSPValue
{
private:
	CSPIntegerStruct	m_Value;
	FLMUNICODE			m_String[MAX_INT_STRING + 1];	// Holds the string representation of the value.
	FLMUINT				m_Length;		// Holds the length of the string.


public:

	CSPInt(FLMUNICODE *pString, FLMUNICODE *pName, FLMUNICODE *pType) :
		CSPValue(pName, pType)
	{
		LONGLONG	value;
		char		pmbcsString[MAX_INT_STRING + 1];
		
		m_Length = f_unilen(pString);
		if (m_Length < MAX_INT_STRING) 
		{
			memcpy(m_String, pString, (m_Length + 1) * sizeof(FLMUNICODE));
			value = CSUtil::funitoll(pString);
			init(pString[0], value);
		}
		else
		{
			pmbcsString[0] = 0;
			m_Length = 0;
			m_String[0] = 0;
			init(L'+', 0);
		}
	}

	CSPInt(FlmRecord *pRec, void *pvField, FLMUNICODE *pName, FLMUNICODE *pType) :
		CSPValue(pName, pType)
	{
		FromFlaim(pRec, pvField);
	}

	virtual ~CSPInt()
	{
	}

	int ToString(FLMUNICODE* pString, FLMUINT size)
	{
		if (size > m_Length)
		{
			f_unicpy(pString, m_String);
			return (m_Length);
		}
		return (-1);
	}

	RCODE ToFlaim(FlmRecord *pRec, void *pvField)
	{
		return (pRec->setBinary(pvField, &m_Value, sizeof(m_Value)));
	}

	FLMUINT GetFlaimType()
	{
		return (FLM_BINARY_TYPE);
	}

	void* SearchVal()
	{
		return (&m_Value);
	}

	int SearchSize()
	{
		return (sizeof(m_Value));
	}

	QTYPES GetSearchType()
	{
		return (FLM_BINARY_VAL);
	}

private:
	void init(FLMUNICODE sign, LONGLONG value)
	{
		// Setup the CSPIntegerStruct.
		m_Value.sign = (sign == L'-') ? 0 : 1;
		
//#ifdef BIG_ENDIAN
//		(LONGLONG)m_Value.value = value;
//#else
		char *pValue = (char*)&value;
		m_Value.value[0] = pValue[7];
		m_Value.value[1] = pValue[6];
		m_Value.value[2] = pValue[5];
		m_Value.value[3] = pValue[4];
		m_Value.value[4] = pValue[3];
		m_Value.value[5] = pValue[2];
		m_Value.value[6] = pValue[1];
		m_Value.value[7] = pValue[0];
//#endif
	}


	RCODE FromFlaim(FlmRecord *pRec, void *pvField)
	{
		RCODE rc;
		FLMUINT buffSize = sizeof(m_Value);
		LONGLONG value;
		FLMUNICODE *pString = m_String;
		FLMUINT		stringLen = sizeof(m_String) / sizeof(FLMUNICODE);
		rc = pRec->getBinary(pvField, &m_Value, &buffSize);
//#ifdef BIG_ENDIAN
//		value = m_Value.value;
//#else
		char *pValue = (char*)&value;
		pValue[0] = m_Value.value[7];
		pValue[1] = m_Value.value[6];
		pValue[2] = m_Value.value[5];
		pValue[3] = m_Value.value[4];
		pValue[4] = m_Value.value[3];
		pValue[5] = m_Value.value[2];
		pValue[6] = m_Value.value[1];
		pValue[7] = m_Value.value[0];
//#endif
		if (m_Value.sign == 0)
		{
			m_Length = CSUtil::lltofuni(value, pString, stringLen);
		}
		else
		{
			m_Length = CSUtil::ulltofuni(value, pString, stringLen);
		}
		return (rc);
	}

	int StringSize()
	{
		return (m_Length);
	}
}; // CSPInt()

class CSPI1 : public CSPInt
{
public:
	CSPI1(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeI1String)
	{
	}

	CSPI1(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeI1String)
	{
	}

	virtual ~CSPI1()
	{
	}
};

class CSPUI1 : public CSPInt
{
public:
	CSPUI1(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeUI1String)
	{
	}

	CSPUI1(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeUI1String)
	{
	}

	virtual ~CSPUI1()
	{
	}
};


class CSPI2 : public CSPInt
{
	public:
	CSPI2(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeI2String)
	{
	}

	CSPI2(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeI2String)
	{
	}

	virtual ~CSPI2()
	{
	}
};


class CSPUI2 : public CSPInt
{
public:
	CSPUI2(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeUI2String)
	{
	}

	CSPUI2(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeUI2String)
	{
	}

	virtual ~CSPUI2()
	{
	}
};


class CSPI4 : public CSPInt
{
public:
	CSPI4(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeI4String)
	{
	}

	CSPI4(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeI4String)
	{
	}

	virtual ~CSPI4()
	{
	}
};


class CSPUI4 : public CSPInt
{
public:
	CSPUI4(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeUI4String)
	{
	}

	CSPUI4(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeUI4String)
	{
	}

	virtual ~CSPUI4()
	{
	}
};


class CSPI8 : public CSPInt
{
public:
	CSPI8(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeI8String)
	{
	}

	CSPI8(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeI8String)
	{
	}

	virtual ~CSPI8()
	{
	}
};


class CSPUI8 : public CSPInt
{
public:
	CSPUI8(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeUI8String)
	{
	}

	CSPUI8(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeUI8String)
	{
	}

	virtual ~CSPUI8()
	{
	}
};

class CSPTimeSpan : public CSPInt
{
public:
	CSPTimeSpan(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeTimeSpanString)
	{
	}

	CSPTimeSpan(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeTimeSpanString)
	{
	}

	virtual ~CSPTimeSpan()
	{
	}
};


#define MAX_FLOAT_STRING	39
typedef struct _CSPFloatStruct_
{
	char			sign;					// 0 = negative, 1 = positive.
	char			integerPart[8];	// Used to hold the MSB integer part of the value.
	char			fractionalPart[4];// Used to hold the MSB fractional part of the value.
	FLMUNICODE		string[MAX_FLOAT_STRING + 1];			// Holds the string representation of the value.
	FLMUINT			length;				// Holds the length of the string.
} CSPFloatStruct, *PCSPFloatStruct;


class CSPFloat : public CSPValue
{
private:
	CSPFloatStruct	m_Value;

public:

	CSPFloat(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeFloatString)
	{
		float		value;
		char 		pmbcsString[MAX_FLOAT_STRING + 1];
		int			i = 0;

		m_Value.length = f_unilen(pString);
		if (m_Value.length < MAX_FLOAT_STRING) 
		{
			memcpy(m_Value.string, pString, (m_Value.length +1)*sizeof(wchar_t));

			// Convert string to single byte.
			// This works because only digits are allowed.
			while ((pString[i] != 0) && (i < MAX_FLOAT_STRING)) 
			{		
				pmbcsString[i] = (char)pString[i];
				i++;
			}
			pmbcsString[i] = 0;
			sscanf(pmbcsString, "%f", &value);
			init(pmbcsString, value);
		}
		else
		{
			pmbcsString[0] = 0;
			m_Value.length = 0;
			m_Value.string[0] = 0;
			init(pmbcsString, 0);
		}
	}

//	CSPFloat(float value)
//	{
//		swnprintf(m_Value.string, sizeof(m_Value.string), L"%f", value);
//		init(m_Value.string, value);
//	}

	CSPFloat(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeFloatString)
	{
		FromFlaim(pRec, pvField);
	}

	virtual ~CSPFloat()
	{
	}

	int ToString(FLMUNICODE* pString, FLMUINT size)
	{
		if (size > m_Value.length)
		{
			f_unicpy(pString, m_Value.string);
			return (m_Value.length);
		}
		return (-1);
	}

	RCODE ToFlaim(FlmRecord *pRec, void *pvField)
	{
		return (pRec->setBinary(pvField, &m_Value, sizeof(m_Value)));
	}

	FLMUINT GetFlaimType()
	{
		return (FLM_BINARY_TYPE);
	}

	void* SearchVal()
	{
		return (&m_Value);
	}

	int SearchSize()
	{
		return (sizeof(m_Value));
	}

	QTYPES GetSearchType()
	{
		return (FLM_BINARY_VAL);
	}

	RCODE FromFlaim(FlmRecord *pRec, void *pvField)
	{
		RCODE rc;
		FLMUINT buffSize = sizeof(m_Value);
		rc = pRec->getBinary(pvField, &m_Value, &buffSize);
		return (rc);
	}

private:
	void init(char *pString, float value)
	{
		LONGLONG 	integerPart;
		FLMINT		fractionalPart;

		// Setup the CSPIntegerStruct.
		m_Value.sign = (pString[0] == '-') ? 0:1;
		integerPart = (LONGLONG)value;
		value = value - integerPart;
		fractionalPart = (FLMINT)(value/0xFFFFFFFF);
//#ifdef BIG_ENDIAN
//		// Store the integer part.
//		(LONGLONG)m_Value.integerPart = integerPart;
//		// Store the fractional part.
//		(LONG)m_Value.fractionalPart = fractionalPart;
//#else
		// Store the integer part.
		char *pValue = (char*)&integerPart;
		m_Value.integerPart[0] = pValue[7];
		m_Value.integerPart[1] = pValue[6];
		m_Value.integerPart[2] = pValue[5];
		m_Value.integerPart[3] = pValue[4];
		m_Value.integerPart[4] = pValue[3];
		m_Value.integerPart[5] = pValue[2];
		m_Value.integerPart[6] = pValue[1];
		m_Value.integerPart[7] = pValue[0];
		// Store the fractional part.
		pValue = (char*)&fractionalPart;
		m_Value.fractionalPart[0] = pValue[3];
		m_Value.fractionalPart[1] = pValue[2];
		m_Value.fractionalPart[2] = pValue[1];
		m_Value.fractionalPart[3] = pValue[0];
//#endif
	}

	int StringSize()
	{
		return (m_Value.length);
	}
}; // CSPFloat()


class CSPString : public CSPValue
{
private:
	FLMUNICODE	*m_pString;
	FLMUINT	m_Length;

public:

	CSPString(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeStringString)
	{
		m_Length = f_unilen(pString);
		m_pString = new FLMUNICODE[m_Length + 1];
		f_unicpy(m_pString, pString);
	}

	CSPString(FLMUNICODE *pString, FLMUNICODE *pName, FLMUNICODE *pType) :
		CSPValue(pName, pType)
	{
		m_Length = f_unilen(pString);
		m_pString = new FLMUNICODE[m_Length + 1];
		f_unicpy(m_pString, pString);
	}

	CSPString(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPValue(pName, CSPTypeStringString)
	{
		FromFlaim(pRec, pvField);
	}

	CSPString(FlmRecord *pRec, void *pvField, FLMUNICODE *pName, FLMUNICODE *pType) :
		CSPValue(pName, pType)
	{
		FromFlaim(pRec, pvField);
	}

	int ToString(FLMUNICODE* pString, FLMUINT size)
	{
		if (size > m_Length)
		{
			f_unicpy(pString, m_pString);
			return (m_Length);
		}
		return (-1);
	}

	virtual ~CSPString()
	{
		delete [] m_pString;
	}

	RCODE ToFlaim(FlmRecord *pRec, void *pvField)
	{
		return (pRec->setUnicode(pvField, (FLMUNICODE*)m_pString));
	}

	FLMUINT GetFlaimType()
	{
		return (FLM_TEXT_TYPE);
	}

	void* SearchVal()
	{
		return (m_pString);
	}

	int SearchSize()
	{
		return (0);
	}

	QTYPES GetSearchType()
	{
		return (FLM_UNICODE_VAL);
	}

	RCODE FromFlaim(FlmRecord *pRec, void *pvField)
	{
		RCODE rc;
		rc = pRec->getUnicodeLength(pvField, &m_Length);
		if (RC_OK(rc))
		{
			m_Length += sizeof(FLMUNICODE);
			m_pString = new FLMUNICODE[m_Length];
			rc = pRec->getUnicode(pvField, (FLMUNICODE*)m_pString, &m_Length);
			m_Length /= sizeof(FLMUNICODE);
		}
		return (rc);
	}

	int StringSize()
	{
		return (m_Length);
	}
}; // CSPString()


class CSPChar : public CSPInt
{
public:
	CSPChar(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeCharString)
	{
	}

	CSPChar(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeCharString)
	{
	}

	virtual ~CSPChar()
	{
	}
}; // Class CSPChar


class CSPDTime : public CSPInt
{
public:
	CSPDTime(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPInt(pString, pName, CSPTypeDTimeString)
	{
	}

	CSPDTime(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPInt(pRec, pvField, pName, CSPTypeDTimeString)
	{
	}

	virtual ~CSPDTime()
	{
	}
}; // Class CSPDTime


class CSPUri : public CSPString
{
public:
	CSPUri(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPString(pString, CSPTypeUriString)
	{
	}

	CSPUri(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPString(pRec, pvField, pName, CSPTypeUriString)
	{
	}

	virtual ~CSPUri()
	{
	}
}; // class CSPUri


class CSPXml : public CSPString
{
public:
	CSPXml(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPString(pString, CSPTypeXmlString)
	{
	}

	CSPXml(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPString(pRec, pvField, pName, CSPTypeXmlString)
	{
	}

	virtual ~CSPXml()
	{
	}
}; // class CSPXml

class CSPRelationship : public CSPString
{
public:
	CSPRelationship(FLMUNICODE *pString, FLMUNICODE *pName) :
		CSPString(pString, CSPTypeRelationshipString)
	{
	}

	CSPRelationship(FlmRecord *pRec, void *pvField, FLMUNICODE *pName) :
		CSPString(pRec, pvField, pName, CSPTypeRelationshipString)
	{
	}

	virtual ~CSPRelationship()
	{
	}
}; // class CSPRelationship


#endif // _CSTYPE_H_

