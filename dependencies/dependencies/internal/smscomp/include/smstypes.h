/*
=================================================================
Novell Software Developer Kit Sample Code License

Copyright (C) 2003-2004 Novell, Inc.  All Rights Reserved.

THIS WORK IS SUBJECT TO U.S. AND INTERNATIONAL COPYRIGHT LAWS AND TREATIES.  
USE AND REDISTRIBUTION OF THIS WORK IS SUBJECT TO THE LICENSE AGREEMENT 
ACCOMPANYING THE SOFTWARE DEVELOPMENT KIT (SDK) THAT CONTAINS THIS WORK.  
PURSUANT TO THE SDK LICENSE AGREEMENT, NOVELL HEREBY GRANTS TO DEVELOPER 
A ROYALTY-FREE, NON-EXCLUSIVE LICENSE TO INCLUDE NOVELL'S SAMPLE CODE IN ITS 
PRODUCT.  NOVELL GRANTS DEVELOPER WORLDWIDE DISTRIBUTION RIGHTS TO MARKET, 
DISTRIBUTE, OR SELL NOVELL'S SAMPLE CODE AS A COMPONENT OF DEVELOPER'S PRODUCTS.
NOVELL SHALL HAVE NO OBLIGATIONS TO DEVELOPER OR DEVELOPER'S CUSTOMERS WITH 
RESPECT TO THIS CODE.

NAME OF FILE: 
				smstypes.h

PURPOSE/COMMENTS:
				Type definitions for SMS, this file is included by SMS.H and should not be 
				included directly.

NDK COMPONENT NAME AND VERSION:
				SMS Developer Components

LAST MODIFIED DATE: 
				23 Jul 2004
=================================================================
*/

#if !defined(_SMSTYPES_H_INCLUDED)
#define _SMSTYPES_H_INCLUDED

#if !defined(TRUE)
#define TRUE  1
#define FALSE 0
#endif

#define  KILO  1024
#define  MEGA  (KILO * KILO)
#define  GIGA  (MEGA * KILO)
#define  TERA  (GIGA * KILO)

typedef unsigned long 	CCODE;

#if !defined(_INT8_T) && !defined(INT8)
#define _INT8_T
typedef char 			INT8;			/*  8 bit signed data item		*/
#endif

#if !defined(_UINT8_T) && !defined(UINT8)
#define _UINT8_T
typedef unsigned char 	UINT8;			/*  8 bit unsigned data item	*/
#endif

#if !defined(_BYTE_T) && !defined(BYTE)
#define _BYTE_T
typedef unsigned char 	BYTE;			/*  8 bit unsigned data item	*/
#endif

#if !defined(_INT16_T) && !defined(INT16)
#define _INT16_T
typedef short 			INT16;			/* 16 bit signed data item	  	*/
#endif

#if !defined(_WORD_T) && !defined(WORD)
#define _WORD_T
typedef unsigned short 	WORD;			/* 16 bit unsigned data item	*/
#endif

#if !defined(_UINT16_T) && !defined(UINT16)
#define _UINT16_T
typedef unsigned short 	UINT16;			/* 16 bit unsigned data item	*/
#endif

#if !defined(_UINT_T) && !defined(UINT)
#define _UINT_T
typedef unsigned int 	UINT;			/* Unsigned native data item	*/
#endif

#if !defined(_INT32_T) && !defined(INT32)
#define _INT32_T
typedef long 			INT32;			/* 32 bit signed data item		*/
#endif

#if !defined(_UINT32_T) && !defined(UINT32)
#define _UINT32_T
typedef unsigned long 	UINT32;			/* 32 bit unsigned data item	*/
#endif

#if !defined(_LONG_T) && !defined(LONG)
#define _LONG_T
typedef unsigned long 	LONG;			/* 32 bit unsigned data item	*/
#endif

#ifndef _NWBOOLEAN_T
#define _NWBOOLEAN_T
typedef short 			NWBOOLEAN;
#endif

#ifndef _BOOL_T
#define _BOOL_T
typedef unsigned int 	BOOL;
#endif

#if defined(__TURBOC__) || defined(_MSC_VER)	/* was (MSC) */
#if defined(WIN32)
#define SM_HUGE
#elif !defined(SM_HUGE)
#define SM_HUGE		huge
#endif /* WIN32 */
	typedef char            BUFFER;
	typedef char SM_HUGE   *BUFFERPTR;
#if !defined(OS2_INCLUDED)
		typedef char        CHAR;
#endif
	typedef char SM_HUGE   *LSTRING;
	typedef char           *PSTRING;
	typedef char SM_HUGE   *STRING;
#endif /* __TURBOC__ || _MSC_VER */

#if defined(NLM) || defined(__WATCOMC__) || defined(__ICC__)
	#define SM_HUGE
	typedef unsigned char    BUFFER;
	typedef unsigned char   *BUFFERPTR;
	#if !defined(OS2_INCLUDED)
		typedef char         CHAR;
	#endif
	typedef unsigned char   *LSTRING;
	typedef unsigned char   *PSTRING;
	typedef unsigned char   *STRING;
#else
	#define SM_HUGE
	typedef unsigned char    BUFFER;
	typedef unsigned char   *BUFFERPTR;
	#if !defined(OS2_INCLUDED)
		typedef char         CHAR;
	#endif
	typedef unsigned char   *LSTRING;
	typedef unsigned char   *PSTRING;
	typedef unsigned char   *STRING;
	#ifndef _QUAD_T
	typedef unsigned long long	QUAD;
	#define _QUAD_T
	#endif
#endif

typedef UINT32           NWBOOLEAN32;

#pragma pack(push,1)

#ifndef UINT64
#ifdef N_PLAT_NLM
	typedef struct
	{
	   	UINT16 v[4];
	} UINT64;
#else
	typedef unsigned long long UINT64;
#endif
#endif

typedef struct
{
	/*   int     type:4; */
	/*   int     timeZone:12; */
	UINT16  typeAndTimeZone;
	INT16   year;
	UINT8   month;
	UINT8   day;
	UINT8   hour;
	UINT8   minute;
	UINT8   second;
	UINT8   centiSecond;
	UINT8   hundredsOfMicroseconds;
	UINT8   microSeconds;
	UINT32  reserved;
} ECMATime;

typedef struct
{
	UINT16
	size,
	buffer[1];
} UINT16_BUFFER;

typedef struct
{
	UINT16	size;
	char	string[1];
} STRING_BUFFER;

typedef struct
{
	UINT16	bufferSize;												/* 0x00 */
	UINT16	dataSetNameSize;										/* 0x02 */
	UINT8		nameSpaceCount;										/* 0x04 */
	UINT8		keyInformationSize;									/* 0x05 */
	UINT8		keyInformation[1];									/* 0x06 */
} NWSM_DATA_SET_NAME_LIST;
/*	UINT32	nameSpaceType;
	UINT32	reserved;
	UINT8		count;
	UINT16	namePositions[count];
	UINT16	separatorPositions[count];
	UINT16	nameLength;
	UINT8		name[nameLength + 1];
*/

typedef struct
{
	UINT16	bufferSize;												/* 0x00 */
	UINT16	scanControlSize;										/* 0x02 */
	UINT32	scanType;												/* 0x04 */
	UINT32	firstAccessDateAndTime;									/* 0x08 */
	UINT32	lastAccessDateAndTime;									/* 0x0C */
	UINT32	firstCreateDateAndTime;									/* 0x10 */
	UINT32	lastCreateDateAndTime;									/* 0x14 */
	UINT32	firstModifiedDateAndTime;								/* 0x18 */
	UINT32	lastModifiedDateAndTime;								/* 0x1C */
	UINT32	firstArchivedDateAndTime;								/* 0x20 */
	UINT32	lastArchivedDateAndTime;								/* 0x24 */
	UINT8	returnChildTerminalNodeNameOnly;						/* 0x28 */
	UINT8	parentsOnly;											/* 0x29 */
	UINT8	childrenOnly;											/* 0x2A */
	UINT8	createSkippedDataSetsFile;								/* 0x2B */
	UINT8	generateCRC;											/* 0x2C */
	UINT8	returnNFSHardLinksInDataSetName;						/* 0x2D */
	UINT8	reserved[6];											/* 0x2E */
	UINT32	scanChildNameSpaceType;									/* 0x34 */
	UINT32	returnNameSpaceType;									/* 0x38 */
	UINT8	callScanFilter;											/* 0x3C */
	UINT16	otherInformationSize;									/* 0x3D */
	UINT8	otherInformation[1];									/* 0x3F */
} NWSM_SCAN_CONTROL;

typedef struct
{
	UINT16	bufferSize;												/* 0x00 */
	UINT16	scanInformationSize;									/* 0x02 */
	UINT32	attributes;												/* 0x04 */
	UINT32	creatorID;												/* 0x08 */
	UINT32	creatorNameSpaceNumber;									/* 0x0C */
	UINT32	primaryDataStreamSize;									/* 0x10 */
	UINT32	totalStreamsDataSize;									/* 0x14 */
	UINT8	modifiedFlag;											/* 0x18 */
	UINT8	deletedFlag;											/* 0x19 */
	UINT8	parentFlag;												/* 0x1A */
	UINT8	reserved[5];											/* 0x1B */
	UINT32	accessDateAndTime;										/* 0x20 */
	UINT32	createDateAndTime;										/* 0x24 */
	UINT32	modifiedDateAndTime;									/* 0x28 */
	UINT32	archivedDateAndTime;									/* 0x2C */
	UINT16	otherInformationSize;									/* 0x30 */
	UINT8	otherInformation[1];									/* 0x32 */
} NWSM_SCAN_INFORMATION;

typedef struct
{
	UINT16	bufferSize;												/* 0x00 */
	UINT16	selectionListSize;										/* 0x02 */
	UINT8	selectionCount; 										/* 0x04 */
	UINT8	keyInformationSize;										/* 0x05 */
	UINT8	keyInformation[1];										/* 0x06 */
} NWSM_SELECTION_LIST;
/*	UINT32	selectionNameSpaceType;
	UINT32	selectionType;
	UINT8	count;
	UINT16	namePositions[count];
	UINT16	separatorPositions[count];
	UINT16	nameLength;
	UINT8	name[nameLength + 1];
*/

typedef struct
{
	char	
		moduleFileName[256];

	UINT8
		moduleMajorVersion,
		moduleMinorVersion;

	UINT16
		moduleRevisionLevel;

	char
		baseOS[64];

	UINT8
		baseOSMajorVersion,
		baseOSMinorVersion;

	UINT16
		baseOSRevisionLevel;
} 
NWSM_MODULE_VERSION_INFO;

typedef struct _NWSM_NAME_LIST
{
	struct _NWSM_NAME_LIST *next;									/* 0x00 */
	STRING	name;													/* 0x04 */
	void   *other_info;
} NWSM_NAME_LIST;

#define	NWSM_NETWORK_CONNECTION      	0
#define	NWSM_NAME_PASSWORD_PAIR     	1

typedef struct                         /* this is the header of a generic, length-preceeded array of bytes*/
{                                       
	UINT32     length;                  
	INT8       stream[4];                

} NWSM_LongByteStream;

#pragma pack(pop)

#endif /* _SMSTYPES_H_INCLUDED */
