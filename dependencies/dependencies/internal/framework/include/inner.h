/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:  $
 |
 | $Workfile: inner.h $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Declare the functions for premitive string operations based on the NameSpaceType
 +-------------------------------------------------------------------------*/

#if !defined(_INNER_H_INCLUDED)
   #define _INNER_H_INCLUDED
#ifdef N_PLAT_NLM
#include <nwlocale.h>
#include <unicode.h>
#endif
#include "smuni.h"
#include "smstypes.h"
#include "smsutapi.h"


#define _colon                  GenericChar(nameSpaceType, ':')
#define _slash                  GenericChar(nameSpaceType, '/')
#define _backslash              GenericChar(nameSpaceType, '\\')
#define _zero                   GenericChar(nameSpaceType, '\0')
#define _asterisk               GenericChar(nameSpaceType, '*')
#define _question               GenericChar(nameSpaceType, '?')
#define _period                 GenericChar(nameSpaceType, '.')
#define _extensionseparator     GenericChar(nameSpaceType, '.')

#define _colonslash             GenericString(nameSpaceType, ":/")
#define _colonslashbackslash    GenericString(nameSpaceType, ":/\\")
#define _slashbackslash         GenericString(nameSpaceType, "/\\")
#define _slashslash         	GenericString(nameSpaceType, "//")
#define _colonstr               GenericString(nameSpaceType, ":")
#define _slashstr               GenericString(nameSpaceType, "/")
#define _doublecolon            GenericString(nameSpaceType, "::")
#define _lessthan               GenericString(nameSpaceType, "<")
#define _greaterthan            GenericString(nameSpaceType, ">")
#define _or 					GenericString(nameSpaceType, "|")

#define _squestion              GenericChar(nameSpaceType, '?' | 0x80)
#define _speriod                GenericChar(nameSpaceType, '.' | 0x80)
#define _sasterisk              GenericChar(nameSpaceType, '*' | 0x80)

#define _size                   SMsizeOfChar(nameSpaceType)

/* Name Space Unicode type definitions */
#define DOSNameSpaceUnicodeType     0x200
#define MACNameSpaceUnicodeType     0x201
#define NFSNameSpaceUnicodeType     0x202   
#define LONGNameSpaceUnicodeType    0x204

/* nameSpace size definitions */
#define UNICODE_CHAR_SIZE			(sizeof(unicode_t))
#define ASCII_CHAR_SIZE				1

NWBOOLEAN NWSMIsNameSpaceAscii(UINT32 nameSpaceType);
UINT32 MapToNSSNameSpace(UINT32 nameSpaceType);

UINT32 ConvertAsciiToUnicodeNameSpace(UINT32 nameSpace);
UINT32 ConvertUnicodeToAsciiNameSpace(UINT32 nameSpace);
UINT32 ConvertUTF8ToUnicodeNameSpace(UINT32 nameSpace);
UINT32 ConvertUTF8ToAsciiNameSpace(UINT32 nameSpace);
UINT32 ConvertAsciiToUTF8NameSpace(UINT32 nameSpace);
UINT32 ConvertUnicodeToUtf8NameSpace(UINT32 nameSpace);
CCODE isNameSpaceSupported(void *dSetName, UINT32 nameSpace);

/*
 *  These following premitive functions are not part of standard LIBC on Linux. 
 *  So, these fucntions are implemented
 */
#define UNI_MAP_NO_CHAR     0

#if defined (N_PLAT_GNU)
    char *NWPrevChar(char *mbs, char*cp1);
	char* NWNextChar(char *mbs);
	int NWCharType(char *mbs);
   	char *strrev(char *str1, char* str);
    char* strupr( char* str );
	char* strlwr( char* str );
	#ifndef strnicmp
	#define strnicmp(a,b,c) 			strncasecmp( (a), (b), (c))
	#endif
	#ifndef stricmp
	#define stricmp 			strcasecmp
	#endif
#endif
UINT32 SMstrlen(
                                UINT32  nameSpaceType,
                                void    *string);

void *SMstrstr(
                           UINT32       nameSpaceType,
                           void         *string,
                           void         *substring);

void *SMstrchr(
                           UINT32       nameSpaceType,
                           void         *string,
                           void         *character);

void *SMstrrchr(
                                UINT32  nameSpaceType,
                                void    *string,
                                void    *character);

void *SMstrupr(
                           UINT32       nameSpaceType,
                           void         *string);

CCODE SMstrcpy(
                           UINT32       nameSpaceType,
                           void         *destString,
                           void         *srcString);

CCODE SMstrncpy(
                                UINT32  nameSpaceType,
                                void    *destString,
                                void    *srcString,
                                UINT32  n);

CCODE SMstrcat(
                           UINT32       nameSpaceType,
                           void         *destString,
                           void         *srcString);

CCODE SMstrcmp(
                           UINT32       nameSpaceType,
                           void         *string1,
                           void         *string2);

CCODE SMstrncmp(
                                UINT32  nameSpaceType,
                                void    *string1,
                                void    *string2,
                                UINT32  n);

CCODE SMstricmp(
                                UINT32  nameSpaceType,
                                void    *string1,
                                void    *string2);

CCODE SMstrnicmp(
                                 UINT32 nameSpaceType,
                                 void   *string1,
                                 void   *string2,
                                 UINT32 n);

void *SMstrrev(
                           UINT32       nameSpaceType,
                           void         *string);

void *SMstrdup(
                           UINT32       nameSpaceType,
                           void         *string);

void *SMprevChar(
                                 UINT32 nameSpaceType,
                                 void   *currPos);

void *SMnextChar(
                                 UINT32 nameSpaceType,
                                 void   *currPos);

void *SMlastChar(
                                 UINT32 nameSpaceType,
                                 void   *string);

void *SMendChar(
                                UINT32  nameSpaceType,
                                void    *string,
                                void    *substring);

UINT8 SMsizeOfChar(
                                   UINT32       nameSpaceType);

void SMsetChar(
                           UINT32       nameSpaceType,
                           void         *stringPtr,
                           void         *character);

NWBOOLEAN SMcheckChar(
                                          UINT32        nameSpaceType,
                                          void          *stringPtr,
                                          void          *character);

NWBOOLEAN SMisWild(
                                   UINT32       nameSpaceType,
                                   void         *character);

NWBOOLEAN SMisQuestion(
                                           UINT32       nameSpaceType,
                                           void         *character);


void *GenericChar(
                                  UINT32        nameSpaceType,
                                  int           character);

void *GenericString(
                                        UINT32  nameSpaceType,
                                        char    *string);
void *GenericUniString( 
                                                                                void *                  string);
void *GenericUtf8String(
                                                                                void *          string);
void *GenericUniChar(
                                                                                int             character);
void *GenericUtf8Char(
                                                                                int             character);
void *GenericMBCSChar(
                                                                                int             character);


#endif
