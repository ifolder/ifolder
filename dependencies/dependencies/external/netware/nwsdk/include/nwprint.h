/******************************************************************************

  %name: nwprint.h %
  %version: 6 %
  %date_modified: Wed Oct 20 10:33:30 1999 %
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

#if ! defined ( NWPRINT_H )
#define NWPRINT_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define LPT1 1
#define LPT2 2
#define LPT3 3
#define LPT4 4
#define LPT5 5
#define LPT6 6
#define LPT7 7
#define LPT8 8
#define LPT9 9

#define START_CAPTURE             1
#define END_CAPTURE               2
#define CANCEL_CAPTURE            3
#define GET_PRINT_JOB_FLAGS       4
#define SET_PRINT_JOB_FLAGS       5
#define GET_BANNER_USER_NAME      6
#define SET_BANNER_USER_NAME      7
#define GET_PRINTER_SETUP_STRING  8
#define SET_PRINTER_SETUP_STRING  9
#define GET_PRINTER_RESET_STRING  10
#define SET_PRINTER_RESET_STRING  11

typedef struct
{
  nuint8  clientStation;
  nuint8  clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint16 jobNumber;
  nuint16 formType;
  nuint8  jobPosition;
  nuint8  jobControlFlags;
  nuint8  jobFileName[14];
  nuint8  jobFileHandle[6];
  nuint8  servicingServerStation;
  nuint8  servicingServerTask;
  nuint32 servicingServerID;
  nuint8  jobDescription[50];
  nuint8  clientJobInfoVer;
  nuint8  tabSize;
  nuint16 numberCopies;
  nuint16 printFlags;
  nuint16 maxLines;
  nuint16 maxChars;
  nuint8  formName[16];
  nuint8  reserved[6];    /* must be set to zeros */
  nuint8  bannerUserName[13];
  nuint8  bannerFileName[13];
  nuint8  bannerHeaderFileName[14];
  nuint8  filePathName[80];
} PrintJobStruct;

typedef struct
{
  nuint32 clientStation;
  nuint32 clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint32 jobNumber;
  nuint16 formType;
  nuint16 jobPosition;
  nuint16 jobControlFlags;
  nuint8  jobFileName[14];
  nuint32 jobFileHandle;
  nuint32 servicingServerStation;
  nuint32 servicingServerTask;
  nuint32 servicingServerID;
  nuint8  jobDescription[50];
  nuint8  clientJobInfoVer;
  nuint8  tabSize;
  nuint16 numberCopies;
  nuint16 printFlags;
  nuint16 maxLines;
  nuint16 maxChars;
  nuint8  formName[16];
  nuint8  reserved[6];      /* must be set to zeros */
  nuint8  bannerUserName[13];
  nuint8  bannerFileName[13];
  nuint8  bannerHeaderFileName[14];
  nuint8  filePathName[80];
} NWPrintJobStruct;

typedef struct PRINTER_STATUS
{
  nuint8  printerHalted;
  nuint8  printerOffline;
  nuint8  currentFormType;
  nuint8  redirectedPrinter;
} PRINTER_STATUS;

typedef struct
{
  nuint8    jobDescription[ 50 ];   /* OS/2, VLM only                         */
                                    /* VLM returns or sets only 12 characters */
                                    /* plus the NULL -- a total of 13 nuint8's   */
  nuint8    jobControlFlags;        /* OS/2, VLM only */
  nuint8    tabSize;
  nuint16   numCopies;
  nuint16   printFlags;
  nuint16   maxLines;
  nuint16   maxChars;
  nuint8    formName[ 13 ];
  nuint8    reserved[ 9 ];
  nuint16   formType;
  nuint8    bannerText[ 13 ];
  nuint8    reserved2;
  nuint16   flushCaptureTimeout;    /* DOS/WIN only */
  nuint8    flushCaptureOnClose;    /* DOS/WIN only */
} NWCAPTURE_FLAGSRW;

#define NWCAPTURE_FLAGS1 NWCAPTURE_FLAGSRW

typedef struct
{
  NWCONN_HANDLE connID;
  nuint32 queueID;
  nuint16 setupStringMaxLen;
  nuint16 resetStringMaxLen;
  nuint8  LPTCaptureFlag;         /* DOS/WIN only */
  nuint8  fileCaptureFlag;        /* DOS/WIN only */
  nuint8  timingOutFlag;          /* DOS/WIN only */
  nuint8  inProgress;             /* DOS/WIN only */
  nuint8  printQueueFlag;         /* DOS/WIN only */
  nuint8  printJobValid;          /* DOS/WIN only */
  nstr8   queueName[ 65 ];        /* VLM only     */
} NWCAPTURE_FLAGSRO;

#define NWCAPTURE_FLAGS2 NWCAPTURE_FLAGSRO

typedef struct
{
  nuint32 connRef;
  nuint32 queueID;
  nuint16 setupStringMaxLen;
  nuint16 resetStringMaxLen;
  nuint8  LPTCaptureFlag;         /* DOS/WIN only */
  nuint8  fileCaptureFlag;        /* DOS/WIN only */
  nuint8  timingOutFlag;          /* DOS/WIN only */
  nuint8  inProgress;             /* DOS/WIN only */
  nuint8  printQueueFlag;         /* DOS/WIN only */
  nuint8  printJobValid;          /* DOS/WIN only */
  nstr8   queueName[ 65 ];        /* VLM only     */
} NWCAPTURE_FLAGSRO3;

#define NWCAPTURE_FLAGS3 NWCAPTURE_FLAGSRO3

#ifdef N_PLAT_OS2

#define N_APIPIPE                 "\\PIPE\\NWSPOOL\\API"  /*IPC to API*/
#define NET_SPOOL_SEG             "\\sharemem\\nwspool\\seg1"
#define NET_SPOOL_SEM1            "\\sem\\nwspool\\sem1"
#define NET_SPOOL_SEM2            "\\sem\\nwspool\\sem2"
#define NET_SPOOL_SEM3            "\\sem\\nwspool\\sem3"

typedef struct
{
  nuint32   targetServerID;
  nuint8    targetExecutionTime[6];
  nuint8    jobDescription[50];
  nuint8    jobControlFlags;
  nuint8    tabSize;
  nuint16   numberCopies;
  nuint16   printFlags;
  nuint16   maxLines;
  nuint16   maxChars;
  nuint8    formName[16];
  nuint8    reserved1[6];  /* must be set to zeros */
  nuint16   formType;
  nuint8    bannerFileName[13];
  nuint8    reserved2;    /* must be set to zero */

  /* The following fields can be gotten, but not set */
  NWCONN_HANDLE connID;
  nuint32   queueID;
  nuint16   setupStringMaxLength;
  nuint16   resetStringMaxLength;
} SpoolFlagsStruct;

typedef struct _NWPipeStruct
{
  nuint16 fwCommand;
  nuint16 idSession;
  nuint32 idQueue;
  nuint16 idConnection;
  nuint16 idDevice;
  nuint16 fwMode;
  nuint16 fwScope;
  nuint16 cbBufferLength;
  nuint8  fbValidBuffer;
  SpoolFlagsStruct  nwsSpoolFlags;
  nuint8  szBannerUserName[13];
  nuint16 rc;
} NWPipeStruct;

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolGetPrintJobFlags
(
   nuint16     deviceID,
   SpoolFlagsStruct N_FAR * flagsBuffer,
   nuint16     mode,
   pnuint16    scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolSetPrintJobFlags
(
  nuint16      deviceID,
  SpoolFlagsStruct N_FAR * flagsBuffer,
  nuint16      unused
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolGetPrinterSetupString
(
  nuint16      deviceID,
  pnuint16     bufferLen,
  pnstr8       buffer,
  nuint16      mode,
  pnuint16     scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolSetPrinterSetupString
(
  nuint16      deviceID,
  nuint16      bufferLen,
  pnstr8       buffer,
  nuint16      scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolGetPrinterResetString
(
  nuint16      deviceID,
  pnuint16     bufferLen,
  pnstr8       buffer,
  nuint16      mode,
  pnuint16     scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolSetPrinterResetString
(
  nuint16      deviceID,
  nuint16      bufferLen,
  pnstr8       buffer,
  nuint16      scope
);

#else

typedef struct
{
  nuint8  status;
  nuint8  flags;
  nuint8  tabSize;
  nuint8  serverPrinter;
  nuint8  numberCopies;
  nuint8  formType;
  nuint8  reserved;
  nuint8  bannerText[13];
  nuint8  reserved2;
  nuint8  localLPTDevice;
  nuint16 captureTimeOutCount;
  nuint8  captureOnDeviceClose;
} CaptureFlagsStruct;

N_EXTERN_LIBRARY( NWCCODE )
NWGetPrinterDefaults
(
   pnuint8     status,
   pnuint8     flags,
   pnuint8     tabSize,
   pnuint8     serverPrinter,
   pnuint8     numberCopies,
   pnuint8     formType,
   pnstr8      bannerText,
   pnuint8     localLPTDevice,
   pnuint16    captureTimeOutCount,
   pnuint8     captureOnDeviceClose
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetPrinterDefaults
(
   nuint8      flags,
   nuint8      tabSize,
   nuint8      serverPrinter,
   nuint8      numberCopies,
   nuint8      formType,
   pnstr8      bannerText,
   nuint8      localLPTDevice,
   nuint16     captureTimeOutCount,
   nuint8      captureOnDeviceClose
);

N_EXTERN_LIBRARY( NWCCODE )
NWStartLPTCapture
(
   nuint16     deviceID
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetLPTCaptureStatus
(
   NWCONN_HANDLE N_FAR * conn
);

#endif

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolStartCapture
(
   nuint16        deviceID,
   nuint32        queueID,
   NWCONN_HANDLE  conn,
   nuint16        scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolEndCapture
(
   nuint16        deviceID,
   nuint16        scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolCancelCapture
(
   nuint16        deviceID,
   nuint16        scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolGetBannerUserName
(
   pnstr8         username,
   nuint16        mode,
   pnuint16       scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWSpoolSetBannerUserName
(
   const nstr8 N_FAR * username,
   nuint16             scope
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetPrinterStatus
(
   NWCONN_HANDLE  conn,
   nuint16        printerNumber,
   PRINTER_STATUS N_FAR * status
);

N_EXTERN_LIBRARY( NWCCODE )
NWStartQueueCapture
(
   NWCONN_HANDLE       conn,
   nuint8              LPTDevice,
   nuint32             queueID,
   const nstr8 N_FAR * queueName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetCaptureStatus
(
   nuint8         LPTDevice
);

N_EXTERN_LIBRARY( NWCCODE )
NWFlushCapture
(
   nuint8         LPTDevice
);

N_EXTERN_LIBRARY( NWCCODE )
NWEndCapture
(
   nuint8         LPTDevice
);

N_EXTERN_LIBRARY( NWCCODE )
NWCancelCapture
(
   nuint8         LPTDevice
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetBannerUserName
(
   pnstr8         userName
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetBannerUserName
(
   const nstr8 N_FAR * userName
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetCaptureFlags
(
   nuint8         LPTDevice,
   NWCAPTURE_FLAGS1 N_FAR * captureFlags1,
   NWCAPTURE_FLAGS2 N_FAR * captureFlags2
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetCaptureFlagsConnRef
(
   nuint8         LPTDevice,
   NWCAPTURE_FLAGS1 N_FAR * captureFlags1,
   NWCAPTURE_FLAGS3 N_FAR * captureFlags3
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetCaptureFlags
(
   NWCONN_HANDLE                  conn,
   nuint8                         LPTDevice,
   const NWCAPTURE_FLAGS1 N_FAR * captureFlags1
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetPrinterStrings
(
   nuint8         LPTDevice,
   pnuint16       setupStringLen,
   pnstr8         setupString,
   pnuint16       resetStringLen,
   pnstr8         resetString
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetPrinterStrings
(
   nuint8              LPTDevice,
   nuint16             setupStringLen,
   const nstr8 N_FAR * setupString,
   nuint16             resetStringLen,
   const nstr8 N_FAR * resetString
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetMaxPrinters
(
   pnuint16       numPrinters
);

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_print.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */


#include "npackoff.h"
#endif
