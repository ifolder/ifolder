/******************************************************************************

  %name: nwmisc.h %
  %version: 7 %
  %date_modified: Fri Oct 15 15:27:49 1999 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWMISC_H )
#define NWMISC_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef NW_SHORT_NAME_SERVER
# define NW_SHORT_NAME_SERVER 0
#endif

#ifndef NW_LONG_NAME_SERVER
# define NW_LONG_NAME_SERVER 1
#endif

#ifndef NW_ENCP_SERVER
#define NW_ENCP_SERVER 1
#endif

#ifndef NW_EXTENDED_NCP_SERVER
#define NW_EXTENDED_NCP_SERVER 1
#endif

#ifndef _NETX_COM
#define _NETX_COM     0x0001
#define _NETX_VLM     0x0002
#define _REDIR_LOADED 0x4000
#define _VLM_LOADED   0x8000
#endif

#ifdef   N_PLAT_UNIX
#define  NWWordSwap(x) ((nuint16) ( \
                       (((nuint16)((x) & 0x00FF)) << 8) | \
                       (((nuint16)((x) & 0xFF00)) >> 8) ))
#define  NWLongSwap(x) ((nuint32) ( \
                       (((nuint32)((x) & 0x000000FFL)) << 24) | \
                       (((nuint32)((x) & 0x0000FF00L)) <<  8) | \
                       (((nuint32)((x) & 0x00FF0000L)) >>  8) | \
                       (((nuint32)((x) & 0xFF000000L)) >> 24) ))
#endif

typedef struct
{
  nuint8    day;
  nuint8    month;
  nuint16   year;
} NW_DATE;

/* hours is a nuint16  so that this structure will be the same length as a dword */
typedef struct
{
  nuint8    seconds;
  nuint8    minutes;
  nuint16   hours;
} NW_TIME;

typedef enum
{
  NW_LONG_NAME_REQUESTER,
  NW_SHORT_NAME_REQUESTER,
  NW_ERROR_ON_REQUESTER_TYPE
} NW_REQUESTER_TYPE;

#ifndef NW_FRAGMENT_DEFINED
#define NW_FRAGMENT_DEFINED
typedef struct
{
  nptr fragAddress;
#if defined(N_PLAT_NLM) || defined(WIN32)
  nuint32 fragSize;
#else
  nuint16  fragSize;
#endif
} NW_FRAGMENT;
#endif

typedef struct
{
  nuint16   taskNumber;
  nuint8    taskState;
} CONN_TASK;

typedef struct
{
  nuint16   serverVersion;    /* use NW_ constants from nwserver.h */
  nuint8    lockState;
  nuint16   waitingTaskNumber;
  nuint32   recordStart;
  nuint32   recordEnd;
  nuint8    volNumber;
  nuint32   dirEntry;         /* this field is only valid in 3.11 */
  nuint8    nameSpace;        /* this field is only valid in 3.11 */
  nuint16   dirID;            /* this field is only valid in 2.x  */
  nstr8     lockedName[256];
  nuint8    taskCount;
  CONN_TASK tasks[256];
} CONN_TASK_INFO;

typedef struct
{
  nuint8  volNumber;
  nuint32 dirEntry;
} DIR_ENTRY;

N_EXTERN_LIBRARY( void )
NWUnpackDateTime
(
  nuint32         dateTime,
  NW_DATE N_FAR *   sDate,
  NW_TIME N_FAR *   sTime
);

N_EXTERN_LIBRARY( void )
NWUnpackDate
(
  nuint16         date,
  NW_DATE N_FAR *   sDate
);

N_EXTERN_LIBRARY( void )
NWUnpackTime
(
  nuint16         time,
  NW_TIME N_FAR *   sTime
);

N_EXTERN_LIBRARY( nuint32 )
NWPackDateTime
(
   const NW_DATE N_FAR *  sDate,
   const NW_TIME N_FAR *  sTime
);

N_EXTERN_LIBRARY( nuint16 )
NWPackDate
(
   const NW_DATE N_FAR *  sDate
);

N_EXTERN_LIBRARY( nuint16 )
NWPackTime
(
   const NW_TIME N_FAR *  sTime
);

/* Avoid using the following three NWConvert{Date/Time} functions,
   they just call the NWUnpack{Date/Time} functions. They are here for
   compatibility reasons only. */
N_EXTERN_LIBRARY( void )
NWConvertDateTime
(
   nuint32        dateTime,
   NW_DATE N_FAR *  sDate,
   NW_TIME N_FAR *  sTime
);

N_EXTERN_LIBRARY( void )
NWConvertDate
(
   nuint16        date,
   NW_DATE N_FAR *  sDate
);

N_EXTERN_LIBRARY( void )
NWConvertTime
(
   nuint16        time,
   NW_TIME N_FAR *  sTime
);

N_EXTERN_LIBRARY( NWCCODE )
NWRequest
(
  NWCONN_HANDLE             conn,
  nuint16                   function,
  nuint16                   numReqFrags,
  const NW_FRAGMENT N_FAR * reqFrags,
  nuint16                   numReplyFrags,
  NW_FRAGMENT       N_FAR * replyFrags
);

N_EXTERN_LIBRARY( NWCCODE )
_NWGetRequesterType
(
  NW_REQUESTER_TYPE N_FAR * type
);

#ifndef  N_PLAT_UNIX

N_EXTERN_LIBRARY( nuint16 )
NWWordSwap
(
   nuint16  swapWord
);

N_EXTERN_LIBRARY( nuint32 )
NWLongSwap
(
   nuint32  swapLong
);

#endif

N_EXTERN_LIBRARY( nint16 )
NWInitDBCS
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWConvertPathToDirEntry
(
  NWCONN_HANDLE       conn,
  NWDIR_HANDLE        dirHandle,
  const nstr8 N_FAR * path,
  DIR_ENTRY   N_FAR * dirEntry
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetTaskInformationByConn
(
  NWCONN_HANDLE          conn,
  nuint16                connNum,
  CONN_TASK_INFO N_FAR * taskInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetRequesterVersion
(
  pnuint8         majorVer,
  pnuint8         minorVer,
  pnuint8         revision
);

N_EXTERN_LIBRARY( NWCCODE )
NWIsLNSSupportedOnVolume
(
  NWCONN_HANDLE       conn,
  NWDIR_HANDLE        dirHandle,
  const nstr8 N_FAR * path
);

N_EXTERN_LIBRARY( NWCCODE )
NWConvertFileHandle
(
  NWFILE_HANDLE         fileHandle,
  nuint16               handleType,
  pnuint8               NWHandle,
  NWCONN_HANDLE N_FAR * conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWConvertFileHandleConnRef
(
  NWFILE_HANDLE    fileHandle,
  nuint16          handleType,
  pnuint8          NWHandle,
  pnuint32         connRef
);

N_EXTERN_LIBRARY( void )
_NWConvert4ByteTo6ByteHandle
(
  pnuint8         NW4ByteHandle,
  pnuint8         NW6ByteHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWEndOfJob
(
  void
);

N_EXTERN_LIBRARY( NWCCODE )
NWCallsInit
(
  nptr reserved1,
  nptr reserved2
);

N_EXTERN_LIBRARY( NWCCODE )
NWCallsTerm
(
  nptr reserved
);

N_EXTERN_LIBRARY( nuint16 )
NWGetClientType
(
   void
);

#ifndef WIN32
N_EXTERN_LIBRARY( nuint16 )
__NWGetNWCallsState
(
  void
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetNetWareErrorMode
(
  nuint8    errorMode,
  pnuint8   prevMode
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetEndOfJobStatus
(
  nuint8    endOfJobStatus,
  pnuint8   prevStatus
);
#else
N_EXTERN_LIBRARY( NWCCODE )
NWFSRequest
(
  nuint32   request,
  nptr      inBuf,
  nuint16   inLen,
  nptr      outBuf,
  nuint16   outLen
);
#endif

N_EXTERN_LIBRARY( void )
NWGetNWCallsVersion
(
  pnuint8   majorVer,
  pnuint8   minorVer,
  pnuint8   revLevel,
  pnuint8   betaLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWConvertHandle
(
  NWCONN_HANDLE         conn,
  nuint8                accessMode,
  const void    N_FAR * NWHandle,
  nuint16               handleSize,
  nuint32               fileSize,
  NWFILE_HANDLE N_FAR * fileHandle
);

/* The stuff below this line may NOT be documented. Use with care. */
#if !defined(NWOS2) && !defined(WIN32)
#ifndef _REGISTERS_DEF
#define _REGISTERS_DEF

typedef struct
{
  nuint16  si;
  nuint16  ds;
  nuint16  di;
  nuint16  es;
  nuint8 al, ah;
  nuint8 bl, bh;
  nuint8 cl, ch;
  nuint8 dl, dh;
} BYTE_REGISTERS;

typedef struct
{
  nuint16  si;
  nuint16  ds;
  nuint16  di;
  nuint16  es;
  nuint16  ax;
  nuint16  bx;
  nuint16  cx;
  nuint16  dx;
  nuint16  bp;
  nuint16  flags;
} WORD_REGISTERS;

typedef struct
{
  nptr requestBuffer;
  nptr replyBuffer;
} PTR_REGISTERS;

typedef struct
{
  nptr ds_si;
  nptr es_di;
} SEG_OFF_REGISTERS;

typedef union
{
  WORD_REGISTERS w;
  BYTE_REGISTERS b;
  PTR_REGISTERS  p;
  SEG_OFF_REGISTERS s;
} REGISTERS;
#endif

#ifndef USE_DS
#define USE_DS  1
#define USE_ES  2
#define USE_DOS 0x80
#endif

N_EXTERN_LIBRARY( nint )
NWShellRequest
(
  REGISTERS N_FAR * , 
  nuint16
);


N_EXTERN_LIBRARY( nuint16 )
NWVLMRequest
(
  nuint16   callerID,
  nuint16   destID,
  nuint16   destFunc,
  REGISTERS N_FAR * regs,
  nuint16   mask
);
#endif

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_misc.h"
   #endif 
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif
