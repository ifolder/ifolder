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
			smsutapi.h

PURPOSE/COMMENTS:
	The followings defines are used to control the 
	definitions and structures used in the separate 
	modules.
	SME:     This define selects the definitions and 
	        structures needed for an NLM Engine and
	        should be defined by all NLM Engines source 
	        files before SMSLIB.H is included.
	SME_DOS: This define selects the definitions and 
	        structures needed for a DOS Engine and should 
	        be defined by all DOS Engines source files 
	        before SMSLIB.H is included.
	TSA:     This define selects the definitions and 
	        structures needed for a TSA and should be 
	        defined by all TSA source files before 
	        SMSLIB.H is included.


	DEBUG_CODE, IN_HOUSE:  These defines are for Novell
	internal use only, they are used for debugging and should never be set.


NDK COMPONENT NAME AND VERSION:
					SMS Developer Components

LAST MODIFIED DATE: 
					23 Jul 2004
=================================================================
*/

#ifdef __cplusplus
extern "C" {
#endif

#if !defined(_SMSUTAPI_H_INCLUDED)
   #define _SMSUTAPI_H_INCLUDED


   #if !defined (_CLIB_HDRS_INCLUDED)
       #define _CLIB_HDRS_INCLUDED
       #if defined(__INLINE_FUNCTIONS__)
           #undef __INLINE_FUNCTIONS__
       #endif
       #include <string.h>
       #include <stdlib.h>
   #if (defined(NLM) || defined(__WATCOMC__))
       #include <nwlocale.h>
   #endif
   #endif


   #include <smstypes.h>
   #include <smsdefns.h>
   #include <smsuterr.h>

/* CRC Flags */
   #define CRC_NO         0
   #define CRC_YES   1
   #define CRC_LATER 2

/* Wild card patterns */
   #define ASTERISK  '\x2A'
   #define QUESTION  '\x3F'
   #define SPERIOD   '\xAE' /*period with parity bit set*/
   #define SASTERISK '\xAA' /*asterisk with parity bit set*/
   #define SQUESTION '\xBF' /*question mark with parity set*/

   #define NWSM_MATCH_SUCCESSFUL         0
   #define NWSM_MATCH_TO_END             1
   #define NWSM_MATCH_TO_STRING_END      1
   #define NWSM_MATCH_TO_PATTERN_END     2
   #define NWSM_MATCH_UNSUCCESSFUL      -1




   #define TARGET_SERVICE_AGENT            0x2E
   #define GET_ADDRESS                     (void *)1
   #define GET_ADDRESS_NEED_FILE_OFFSET    (void *)2

   #define SetUINT64Value(a, b)    (*(UINT32 *)(a) = (((b) is 1) ? (1) :\
                                   (((b) is 2) ? (0x100) : (((b) is 4) ?\
                                   (0x10000L) : (0)))), *((UINT32 *)(a) + 1) =\
                                   ((*(UINT32 *)(a)) ? (0) : (1)))
   #define SMSLIB_H

   #define NWSM_BEGIN                      0x10000000L
   #define NWSM_END                        0x20000000L
   #define NWSM_SKIP_FIELD                 0x30000000L

/* nameSpaceType definitions, others are defined in OS, i.e. DOS, MAC etc */
   #define NWSM_ALL_NAME_SPACES            0xFFFFFFFFL
   #define NWSM_TSA_DEFINED_RESOURCE_TYPE  0xFFFFFFFEL
   #define NWSM_CREATOR_NAME_SPACE         0xFFFFFFFDL
   #define NWSM_DIRECTORY_NAME_SPACE       0xFFFFFFFCL
   #define NWSM_NDS_UNICODE_NAME_SPACE     0xFFFFFFFBL

   #define NWSM_TSA_SSI_TYPE               0xFFFFFFFAL
   
   #define NWSM_ALL_NAME_SPACES_UTF8			0xFFFFFFFAL
   #define NWSM_CREATOR_NAME_SPACE_UTF8		0xFFFFFFF9L  
   #define NWSM_ALL_NAME_SPACES_FORMATS		0xFFFFFFF8L
   #define NWSM_NAME_TYPE_MBCS					0x0000001
   #define NWSM_NAME_TYPE_UTF8					0x0000002


	#if defined(SMS_NSS)
   #define NWSM_CREATOR_UNICODE_NAME_SPACE     0xFFFFFFFAL
	#endif

/* Name Space UTF8 type definitions  */
#define DOSNameSpaceUtf8Type            0x100
#define MACNameSpaceUtf8Type            0x101
#define NFSNameSpaceUtf8Type            0x102
#define LONGNameSpaceUtf8Type           0x104

/* NetWare Name Space Defines */
#if defined(NLM) || defined(__WATCOMC__)
   #define DOSNameSpace    0
   #define MACNameSpace    1
   #define NFSNameSpace    2
   #define FTAMNameSpace   3
   #define OS2NameSpace    4
   
   #if defined(NCP)
       #define MaxNameSpaces   1
   #else
       #define MaxNameSpaces   4
   #endif
#else
   #define DOSNameSpace    0L
   #define MACNameSpace    1L
   #define NFSNameSpace    2L
   #define FTAMNameSpace   3L
   #define OS2NameSpace    4L

   #if defined(NCP)
       #define MaxNameSpaces   1L
   #else
       #define MaxNameSpaces   4L
   #endif
#endif

   #define PRIMARY_DATA_STREAM 0
   #define MAC_RESOURCE_FORK   1
   #define FTAM_DATA_STREAM    2
   #define MAX_DATA_STREAMS    3

   #define NameSpaceName(n)   _GetMessage(((n) <= MaxNameSpaces) ?\
               ((n) + DOS_NAME_SPACE_NAME) : INVALID_NAME_SPACE_NAME)
   #define DataStreamName(n)  _GetMessage(((n) <= MAX_DATA_STREAMS) ?\
               ((n) + PRIMARY_DATA_STREAM_NAME) : INVALID_DATA_STREAM_NAME)

/*  Miscellaneous defines */
        #define STANDARD_INCREMENT 1024
        #define MIN_BLANK_SPACE_SECTION_SIZE 14

/* Defines for dataSetInfoRetrieved in NWSM_RECORD_HEADER_INFO */
   #define DATA_SET_INFO_NOT_STARTED       0
   #define DATA_SET_INFO_SPANNED           1
   #define DATA_SET_INFO_COMPLETE          2
   #define DATA_SET_INFO_DOES_NOT_EXIST    3
#pragma pack ( push, 1 ) /* Single byte alignment for structures */
/*  List structures */
   typedef struct NWSM_LIST_STRUCT
   {
		UINT8  marked;									/* 0x00 */
		struct NWSM_LIST_STRUCT *prev;					/* 0x01 */
		struct NWSM_LIST_STRUCT *next;					/* 0x05 */
		void  *otherInfo;								/* 0x09 */
		BUFFER text[1];									/* 0x0D */
   } NWSM_LIST;

   typedef struct
   {
		NWSM_LIST *head;								/* 0x00 */
		NWSM_LIST *tail;								/* 0x04 */
		int (*sortProc)();								/* 0x08 */	
		void (*freeProcedure)(void *memoryPointer);		/* 0x0C */
   } NWSM_LIST_PTR;

   typedef struct
   {
       UINT32                   nameSpaceType;
       UINT32                   selectionType;
       UINT16                   count;
       UINT16 SM_HUGE *namePositions;
       UINT16 SM_HUGE *separatorPositions;
       UINT16                   nameLength;
       STRING                   name;
   } NWSM_DATA_SET_NAME;
#pragma pack (pop)

/* Parser defines, etc.*/
/* Bit Field defines */
   #define SMDF_BIT_NONE   0xC0
   #define SMDF_BIT_ONE    0xC1
   #define SMDF_BIT_TWO    0xC2
   #define SMDF_BIT_THREE  0xC4
   #define SMDF_BIT_FOUR   0xC8
   #define SMDF_BIT_FIVE   0xD0
   #define SMDF_BIT_SIX    0xE0

/* Misc. defines */
   #define SMDF_MIN_PARSER_BUFFER  13 
/* 
	The minimum parsr buffer size is 13, this provides space for the maximum size header,
	(13 bytes, 4 for the Fid and 9 for the Size) is present.
*/

/*
	SMDF Compare Macros, these macros compare a UINT64 (a) with a UINT32 (b)
	and return TRUE if (a op b) where op is EQ (Equal), GE (Greater Than
	or Equal), GT (Greater Than), LE (Less Than or Equal), or LT (Less Than)
*/

   #define SMDF_EQ(a, b)   (!*((UINT32 *)a + 1) and *(UINT32 *)a is b)
   #define SMDF_GE(a, b)   (*((UINT32 *)a + 1) or *(UINT32 *)a >= b)
   #define SMDF_GT(a, b)   (*((UINT32 *)a + 1) or *(UINT32 *)a > b)
   #define SMDF_LE(a, b)   (!*((UINT32 *)a + 1) and *(UINT32 *)a <= b)
   #define SMDF_LT(a, b)   (!*((UINT32 *)a + 1) and *(UINT32 *)a < b)

/* SMDF Misc. Macros */
   #define SMDFFixedFid(fid)       ((longFid) ? (((fid) AND 0xF000) is 0xF000)\
                                       : ((fid) AND 0x40))

   #define SMDFFixedSize(fid)      (1L << ((longFid) ? (*((UINT8 *)&(fid) + 1)\
                                       AND 0x07) : ((fid) AND 0x07)))

   #define SMDFPutUINT64(a, v)     (*((UINT32 *)(a) + 1) = 0, *(UINT32 *)(a) =\
                                       (v))

   #define SMDFBit1IsSet(v)        (((v) & SMDF_BIT_ONE) is SMDF_BIT_ONE)
   #define SMDFBit2IsSet(v)        (((v) & SMDF_BIT_TWO) is SMDF_BIT_TWO)
   #define SMDFBit3IsSet(v)        (((v) & SMDF_BIT_THREE) is SMDF_BIT_THREE)
   #define SMDFBit4IsSet(v)        (((v) & SMDF_BIT_FOUR) is SMDF_BIT_FOUR)
   #define SMDFBit5IsSet(v)        (((v) & SMDF_BIT_FIVE) is SMDF_BIT_FIVE)
   #define SMDFBit6IsSet(v)        (((v) & SMDF_BIT_SIX) is SMDF_BIT_SIX)

   #define SMDFSetBit1(v)          ((v) |= SMDF_BIT_ONE)
   #define SMDFSetBit2(v)          ((v) |= SMDF_BIT_TWO)
   #define SMDFSetBit3(v)          ((v) |= SMDF_BIT_THREE)
   #define SMDFSetBit4(v)          ((v) |= SMDF_BIT_FOUR)
   #define SMDFSetBit5(v)          ((v) |= SMDF_BIT_FIVE)
   #define SMDFSetBit6(v)          ((v) |= SMDF_BIT_SIX)

   #define SMDFSizeOfFID(d)        ((*((UINT8 *)&d + 3)) ? 4 :\
                                   ((*((UINT8 *)&d + 2)) ? 3 :\
                                   ((*((UINT8 *)&d + 1)) ? 2 : 1)))

   #define SMDFSizeOfUINT32Data(d) ((*((UINT16 *)&(d) + 1)) ? 4 :\
                                   (((*((UINT8 *)&(d) + 1))) ? 2 : 1))

   #define SMDFSizeOfUINT32Data0(d)    ((*((UINT8 *)&(d) + 3)) ? 4 :\
                                   ((*((UINT8 *)&(d) + 2)) ? 3 :\
                                   ((*((UINT8 *)&(d) + 1)) ? 2 : 1)))

   #define SMDFSizeOfUINT64Data(d) ((*((UINT32 *)&(d) + 1)) ? 8 :\
                                   ((*((UINT16 *)&(d) + 1)) ? 4 :\
                                   ((*((UINT8 *)&(d) + 1)) ? 2 :\
                                   ((*(UINT8 *)&(d)) ? 1 : 0))))

   #define SMDFSizeOfFieldData(d, m)   (m = 0, ((*((UINT32 *)&(d) + 1)) ?\
                                           (m = 0x83, 8) :\
                                   ((*((UINT16 *)&(d) + 1)) ? (m = 0x82, 4) :\
                                   ((*((UINT8 *)&(d) + 1)) ? (m = 0x81, 2) :\
                                   ((*(UINT8 *)&(d) AND 0x80) ? (m = 0x80, 1) :\
                                           0)))))

   #define SMDFZeroUINT64(a)       (*(UINT32 *)(a) = *((UINT32 *)(a) + 1) = 0)

#pragma pack ( push, 1 ) /* Single byte alignment for structures */

typedef struct
{
	UINT32  fid;					/* 0x00 */
	UINT64  dataSize;				/* 0x04 */
	void   *data;					/* 0x0C */
	UINT32  bytesTransfered;		/* 0x10 */
	UINT64  dataOverflow;			/* 0x14 */
} SMDF_FIELD_DATA;

typedef struct
{
	SMDF_FIELD_DATA field;			/* 0x00 */
	UINT32  sizeOfData;				/* 0x1C */
	void   *addressOfData;			/* 0x20 */
	UINT8   dataSizeMap;			/* 0x24 */
	UINT8   reserved[3];			/* 0x25 */
} NWSM_FIELD_TABLE_DATA;

typedef struct
{
	UINT32  fid;					/* 0x00 */
	void   *data;					/* 0x04 */
	UINT32  dataSize; 				/* 0x08 */	/* The size of the data variable passed in */	
	NWBOOLEAN found;				/* 0x0C */
} NWSM_GET_FIELDS_TABLE;

typedef struct
{
	ECMATime		timeStamp;       /* time of individual media */
	ECMATime		setTimeStamp;    /* time of entire media set */
	BUFFER			label[NWSM_MAX_MEDIA_LABEL_LEN];
	UINT32			number;
	NWBOOLEAN32		indexRequired;	 /* partition index is on media */
} NWSM_MEDIA_INFO;

/* Defines that are used only in session_info struct. */

#define   NWSM_MAX_SIDF_SRC_NM_TYP_LEN    32
#define   NWSM_MAX_SIDF_SRC_NAME_LEN     256

typedef struct
{
	ECMATime                  timeStamp;
	UINT32                    sessionID;
	NWBOOLEAN32                               indexPresent;         /* session index is on media */
	BUFFER                    description[NWSM_MAX_DESCRIPTION_LEN];
	BUFFER                    softwareName[NWSM_MAX_SOFTWARE_NAME_LEN];
	BUFFER                    softwareType[NWSM_MAX_SOFTWARE_TYPE_LEN];
	BUFFER                    softwareVersion[NWSM_MAX_SOFTWARE_VER_LEN];
	BUFFER                    sourceName[NWSM_MAX_TARGET_SRVC_NAME_LEN];
	BUFFER                    sourceType[NWSM_MAX_TARGET_SRVC_TYPE_LEN];
	BUFFER                    sourceVersion[NWSM_MAX_TARGET_SRVC_VER_LEN];
	BUFFER                    sidfSourceNameType[NWSM_MAX_SIDF_SRC_NM_TYP_LEN];
	BUFFER                    sidfSourceName[NWSM_MAX_SIDF_SRC_NAME_LEN];
} NWSM_SESSION_INFO;

typedef struct
{
   NWBOOLEAN32               isSubRecord;
   UINT32                    headerSize;
   UINT32                    recordSize;
   NWSM_DATA_SET_NAME_LIST  *dataSetName;
   NWSM_SCAN_INFORMATION    *scanInformation;
   ECMATime                  archiveDateAndTime;
   UINT32                   *addressOfRecordSize;
   UINT32                   *addressForCRC;
   UINT32                   *addressOfInvalid;
   BUFFERPTR                 crcBegin;
   UINT32                    crcLength;
   UINT32                    dataSetInfoRetrieved;
   UINT32                    saveBufferSize;
   BUFFER                    *saveBuffer;
   NWBOOLEAN32               pathIsFullyQualified;
   NWBOOLEAN32               dataSetIsInvalid;
} NWSM_RECORD_HEADER_INFO;

#pragma pack ( pop )
/* F U N C T I O N S */

/* generic */
void *		NWSMFixGenericDirectoryPath(
                    void *path,
                    UINT32 nameSpaceType,
                    STRING_BUFFER **newPath,
                    NWBOOLEAN wildAllowedOnTerminal);

void *		NWSMCatGenericString(
		            UINT32  			nameSpaceType,
		            STRING_BUFFER 		**dest,
		            void				*source);

void *		NWSMCatGenericStrings(
	                UINT32                  nameSpaceType,
	                UINT8                   numStrings,
	                STRING_BUFFER  			**dest,
	                void                    *src1,
	                void                    *src2,
	                ...);

void *		NWSMGenericStr(
	                UINT32  nameSpaceType,
	                UINT8   n,
	                void    *dest,
	                void    *src1,
	                void    *src2,
	                ...);

void *		NWSMCopyGenericString(
	                UINT32                  nameSpaceType,
	                STRING_BUFFER   		**dest,
	                void                    *src);

STRING_BUFFER*	NWSMAllocGenericString(
	                UINT32                  nameSpaceType,
	                STRING_BUFFER			**string,
	                INT32                   size);

void		NWSMFreeGenericString(
	                STRING_BUFFER   **string);

NWBOOLEAN	NWSMGenericIsWild(
                    UINT32 nameSpaceType,
                    void *string);

CCODE 		NWSMGenericWildMatch(
	                UINT32  nameSpaceType,
	                void    *pattern,
	                void    *string);


/* internal */

void 		GenericCountPositions(
	                UINT32          nameSpaceType,
	                void            *sep1,
	                void            *sep2,
	                void            *name,
	                UINT16          *positionCount);

void *		NWSMFixGenericDOSPath(
	                UINT32  nameSpaceType,
	                void    *path,
	                void    *newPath);

void *		NWSMGenericStripEndSeparator(
	                UINT32  nameSpaceType,
	                void    *path,
	                void    **separatorPos);


/* vstring */
STRING_BUFFER*	NWSMAllocString(
                   STRING_BUFFER   **path,
                   INT16             size);

/* list */
NWSM_LIST*	NWSMAppendToList(
                   NWSM_LIST_PTR    *list,
                   BUFFERPTR         text,
                   void             *otherInfo);

/* vstring */
STRING		NWSMCatString(
	               STRING_BUFFER   **dest,
	               void             *source,
	               INT16             srcLen);

/* datetime */
CCODE		NWSMCheckDateAndTimeRange(
                   UINT32            firstDateAndTime,
                   UINT32            lastDateAndTime,
                   UINT32            compareDateAndTime);

/* name */
CCODE		NWSMCloseName(
                   UINT32 SM_HUGE   *handle);

/* vstring */
STRING		NWSMCopyString(
                   STRING_BUFFER	**dest,
                   void             *src,
                   INT16             srcLen);

/* list */
void		NWSMDestroyList(
                   NWSM_LIST_PTR    *list);
/* strip */
STRING		NWSMFixDirectoryPath(
                   STRING            path,
                   UINT32            nameSpaceType,
                   STRING_BUFFER	 **newPath,
                   NWBOOLEAN         wildAllowedOnTerminal);

/* strip */
STRING		NWSMFixDOSPath(
	               STRING            path,
	               STRING            newPath);

/* strip */
STRING		NWSMGenericFixDOSPath(
	                UINT32  nameSpace,
	                STRING  path,
	                STRING  newPath);
/* free */
CCODE		NWSMFreeNameList(
                   NWSM_NAME_LIST  **list);

/* vstring */
void		NWSMFreeString(
                   STRING_BUFFER   **path);

/* gencrc */
UINT32		NWSMGenerateCRC(
	               UINT32            size,
	               UINT32            crc,
	               BUFFERPTR         ptr);

/* datetime */
UINT32		NWSMGetCurrentDateAndTime(void);

/* name */
CCODE		NWSMGetDataSetName(
                   void SM_HUGE                         *buffer,
                   UINT32                                nameSpaceType,
                   NWSM_DATA_SET_NAME SM_HUGE  *name);

/* name */
CCODE		NWSMGetFirstName(
                   void SM_HUGE                 *buffer,
                   NWSM_DATA_SET_NAME SM_HUGE   *name,
                   UINT32 SM_HUGE               *handle);

/* list */
NWSM_LIST *	NWSMGetListHead(
                   NWSM_LIST_PTR    *listPtr);

/* headers */
CCODE		NWSMGetMediaHeaderInfo(
			       BUFFERPTR                     headerBuffer,
			       UINT32                        headerBufferSize, 
			       NWSM_MEDIA_INFO              *mediaInfo);

/* name */
CCODE		NWSMGetNextName(
                   UINT32 SM_HUGE               *handle,
                   NWSM_DATA_SET_NAME SM_HUGE   *name);

/* name */
CCODE		NWSMGetOneName(
                   void SM_HUGE                 *buffer,
                   NWSM_DATA_SET_NAME SM_HUGE   *name);

/* headers */
void		NWSMPadBlankSpace(
                   BUFFERPTR              bufferPtr, 
                   UINT32                         unusedSize);

/* strip */
void*		NWSMGetPathChild(
                   UINT32               nameSpaceType,
                   void                 *path,
                   void                 **child);

/* headers */
CCODE		NWSMGetRecordHeaderOnly(
			       BUFFERPTR                    *buffer, 
			       UINT32                       *bufferSize,
			       NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* headers */
CCODE		NWSMGetDataSetInfo(
			       BUFFERPTR                    *buffer, 
			       UINT32                       *bufferSize,
			       NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* headers */
CCODE		NWSMGetRecordHeader(
			       BUFFERPTR                    *buffer, 
			       UINT32                       *bufferSize,
			       NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* headers */
CCODE		NWSMGetSessionHeaderInfo(
			       BUFFERPTR                     headerBuffer,
			       UINT32                        headerBufferSize, 
			       NWSM_SESSION_INFO            *sessionInfo);

/* strip */
STRING		NWSMGetVolume(
                   STRING           *ptr,
                   UINT32            nameSpaceType,
                   STRING            volume);


/* str */
void		NWSMGetTargetName(
                   STRING            source,
                   STRING            name,
                   STRING            type,
                   STRING            version);

/* list */
void		NWSMInitList(
                   NWSM_LIST_PTR    *listPtr,
                   void (*freeRoutine)(void *memoryPointer));

/* match (should use NWSMMatchName()) */
NWBOOLEAN	NWSMIsWild(
                   STRING            string);

/* strip */
NWBOOLEAN	NWSMLegalDOSName(
                   STRING            name);

/* pattern match */
int			NWSMMatchName(
                   UINT32            nameSpaceType, 
                   char             *pattern, 
                   char             *string,
                                        NWBOOLEAN                 returnMatchToPatternEndIfWild);

/* datetime */
UINT16		NWSMPackDate(
                   UINT16            year,
                   UINT16            month,
                   UINT16            day);

/* datetime */
UINT32		NWSMPackDateTime(
                   UINT16            year,
                   UINT16            month,
                   UINT16            day,
                   UINT16            hours,
                   UINT16            minutes,
                   UINT16            seconds);

/* datetime */
UINT16		NWSMPackTime(
                   UINT16            hours,
                   UINT16            minutes,
                   UINT16            seconds);

/* strip */
NWBOOLEAN		NWSMPathIsNotFullyQualified(
                   UINT32            nameSpaceType,
                   void              *path);
          

CCODE		NWSMPutFirstName(
                   void SM_HUGE    **buffer,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void             *sep1,
                   void             *sep2,
                   void             *name,
                   UINT32 SM_HUGE   *handle);

/* name */
CCODE		NWSMPutFirstLName(
                   void SM_HUGE      **buffer,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void              *sep1,
                   void              *sep2,
                   UINT32			 nameLength,
                   void              *name,
                   UINT32 SM_HUGE    *handle);

/* name */
CCODE		NWSMPutNextName(
                   void SM_HUGE    **buffer,
                   UINT32 SM_HUGE   *handle,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void              *sep1,
                   void              *sep2,
                   void              *name);
                   
                  

/* name */
CCODE		NWSMPutNextLName(
                   void SM_HUGE    **buffer,
                   UINT32 SM_HUGE   *handle,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void              *sep1,
                   void              *sep2,
                   UINT32            nameLength,
                   void              *name);



/* name */
CCODE 		NWSMPutOneName(
                   void SM_HUGE    **buffer,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void              *sep1,
                   void              *sep2,
                   void              *name);
  


/* name */
CCODE		NWSMPutOneLName(
                   void SM_HUGE    **buffer,
                   UINT32            nameSpaceType,
                   UINT32            selectionType,
                   NWBOOLEAN         reverseOrder,
                   void              *sep1,
                   void              *sep2,
                   UINT32            nameLength,
                   void              *name);


/* headers */
CCODE 		NWSMSetMediaHeaderInfo(
			       NWSM_MEDIA_INFO              *mediaInfo, 
			       BUFFERPTR                     buffer,
			       UINT32                        bufferSize, 
			       UINT32                       *headerSize);

/* headers */
CCODE		NWSMSetRecordHeader(
					BUFFERPTR                    *buffer, 
					UINT32                       *bufferSize,
					UINT32                       *bufferData,
					NWBOOLEAN                     setCRC, 
					NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* headers */
CCODE		NWSMSetNewRecordHeader(
			       BUFFERPTR                    *buffer, 
			       UINT32                       *bufferSize,
			       UINT32                       *bufferData,
			       NWBOOLEAN                     setCRC, 
			       NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* headers */
CCODE		NWSMSetSessionHeaderInfo(
			       NWSM_SESSION_INFO            *sessionInfo, 
			       BUFFERPTR                     buffer,
			       UINT32                        bufferSize, 
			       UINT32                       *headerSize);

/* str */
STRING		NWSMStr(
                   UINT8             n,
                   void             *dest,
                   void             *src1,
                   void             *src2,
                   ...);

/* strip */
CHAR 		NWSMStripEndSeparator(
                   UINT32            nameSpaceType,
                   STRING            path,
                   CHAR            **separatorPos);

/* strip */
void* 		NWSMStripPathChild(
                   UINT32            nameSpaceType,
                   void             *path,
                   void             *child,
                   size_t            maxChildLength);

/* vstring */
STRING 		NWSMCatStrings(
                   UINT8             numStrings,
                   STRING_BUFFER   **dest,
                   void             *src1,
                   void             *src2,
                   ...);

/* datetime */
void		NWSMUnPackDate(
                   UINT16            date,
                   UINT16           *year,
                   UINT16           *month,
                   UINT16           *day);

/* datetime */
void		NWSMUnPackDateTime(
                   UINT32            dateTime,
                   UINT16           *year,
                   UINT16           *month,
                   UINT16           *day,
                   UINT16           *hours,
                   UINT16           *minutes,
                   UINT16           *seconds);

/* datetime */
void		NWSMUnPackTime(
                   UINT16            time,
                   UINT16           *hours,
                   UINT16           *minutes,
                   UINT16           *seconds);

/* headers */
CCODE		NWSMUpdateRecordHeader(
					NWSM_RECORD_HEADER_INFO      *recordHeaderInfo);

/* match (should use NWSMMatchName()) */
CCODE		NWSMWildMatch(
                   STRING            pattern,
                   STRING            string);

/* parser */
CCODE		SMDFAddUINT64(                                  /* sum = a + b */
                   UINT64           *a, 
                   UINT64           *b, 
                   UINT64           *sum);

/* parser */
CCODE		SMDFDecrementUINT64(                            /* a -= b */
                   UINT64           *a, 
                   UINT32            b);

/* parser */
CCODE		SMDFGetNextField(
                   BUFFERPTR         buffer, 
                   UINT32            bufferSize, 
                   SMDF_FIELD_DATA  *field);

/* parser */
CCODE		SMDFGetFields(
                   UINT32            headFID, 
                   NWSM_GET_FIELDS_TABLE table[], 
                   BUFFERPTR        *buffer, 
                   UINT32           *bufferSize);

/* parser */
CCODE		SMDFGetUINT64(
                   UINT64           *a, 
                   UINT32           *v);

/* parser */
CCODE		SMDFIncrementUINT64(                            /* a += b */
                   UINT64           *a, 
                   UINT32            b);

/* parser */
CCODE		SMDFPutFields(
                   NWSM_FIELD_TABLE_DATA table[], 
                   BUFFERPTR        *buffer, 
                   UINT32           *bufferSize, 
                   UINT32            crcFlag);

#if defined(DEBUG_CODE)
/* parser */
void		SMDFPrintUINT64(
                   BUFFERPTR         buffer, 
                   UINT64           *data, 
                   UINT16            pad);
#endif

/* parser */
CCODE		SMDFPutNextField(
                   BUFFERPTR         buffer, 
                   UINT32            bufferSize, 
                   SMDF_FIELD_DATA  *field, 
                   UINT8             dataSizeMap, 
                   UINT32            sizeOfData);

/* parser */
CCODE		SMDFSetUINT32Data(
                   UINT64           *dataSize, 
                   BUFFERPTR         buffer, 
                   UINT32           *data);

/* parser */
CCODE		SMDFSetUINT64(
                   UINT64           *a, 
                   void             *buffer, 
                   UINT16            n);

/* parser */
CCODE		SMDFSubUINT64(                                  /* dif = a - b */
                   UINT64           *a, 
                   UINT64           *b, 
                   UINT64           *dif);

/* str */
int			StrNIEqu(
					UINT32 nameSpace,
                    const char *__s1,
                    const char *__s2,
                    size_t      __n );
/* str */
int			StrIEqu(
					UINT32 nameSpace,
                    const char *__s1, 
                    const char *__s2 );

void 		GetVolumeAndLastSeparator(
					UINT32 nameSpaceType,
					void **path,
					void **volSep,
					void **lastSep);

#define ECMA_TIME_ZONE_UNKNOWN     -2047                   /* from the ECMA 167 spec */

CCODE		NWSMUnixTimeToECMA(
	               UINT32        unixTime,
	               ECMATime     *ECMATime,
	               NWBOOLEAN32   local);

CCODE		NWSMECMAToUnixTime(
	               ECMATime     *ECMATime,
	               UINT32       *unixTime,
	               INT32        *tzOffset );

CCODE		NWSMDOSTimeToECMA(
	               UINT32        dosTime,
	               ECMATime     *ECMATime );

CCODE		NWSMECMAToDOSTime(
	               ECMATime     *ECMATime,
	               UINT32       *dosTime );

int			NWSMECMATimeCompare(
	               ECMATime     *ECMATime1,
	               ECMATime     *ECMATime2);

#endif

#ifdef __cplusplus
}
#endif

