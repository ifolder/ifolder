/******************************************************************************

  %name: nwqms.h %
  %version: 5 %
  %date_modified: Fri Oct 15 11:31:46 1999 %
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

#if ! defined ( NWQMS_H )
#define NWQMS_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define QF_AUTO_START          0x08
#define QF_ENTRY_RESTART       0x10
#define QF_ENTRY_OPEN          0x20
#define QF_USER_HOLD           0x40
#define QF_OPERATOR_HOLD       0x80

#define QS_CANT_ADD_JOBS       0x01
#define QS_SERVERS_CANT_ATTACH 0x02
#define QS_CANT_SERVICE_JOBS   0x04

/*
This struct is taken from NLM platform in the nwqueue.h file.  This
structure is the format for a print queue only.  Other queue types
might have different structures.  Used with the clientRecordArea field
in some of the structures listed below.
*/
typedef struct 
{
nuint8	versionNumber;
nuint8  tabSize;
nuint16 numberOfCopies;
nuint16 printControlFlags;
nuint16 maxLinesPerPage;
nuint16 maxCharsPerLine;
nuint8  formName[13];
nuint8  reserve[9];
nuint8  bannerNameField[13];
nuint8  bannerFileField[13];
nuint8  bannerFileName[14];
nuint8  directoryPath[80];
} QueuePrintJobStruct;


typedef struct
{
  nuint8  clientStation;
  nuint8  clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint16 jobNumber;
  nuint16 jobType;
  nuint8  jobPosition;
  nuint8  jobControlFlags;
  nuint8  jobFileName[14];
  nuint8  jobFileHandle[6];
  nuint8  servicingServerStation;
  nuint8  servicingServerTask;
  nuint32 servicingServerID;
  nuint8  jobDescription[50];
  nuint8  clientRecordArea[152];
} QueueJobStruct;

typedef struct
{
  nuint8  clientStation;
  nuint8  clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint16 jobNumber;
  nuint16 jobType;
  nuint8  jobPosition;
  nuint8  jobControlFlags;
  nuint8  jobFileName[14];
  nuint8  jobFileHandle[6];
  nuint8  servicingServerStation;
  nuint8  servicingServerTask;
  nuint32 servicingServerID;
} ReplyJobStruct;

typedef struct
{
  nuint32 clientStation;
  nuint32 clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint32 jobNumber;
  nuint16 jobType;
  nuint16 jobPosition;
  nuint16 jobControlFlags;
  nuint8  jobFileName[14];
  nuint32 jobFileHandle;
  nuint32 servicingServerStation;
  nuint32 servicingServerTask;
  nuint32 servicingServerID;
  nuint8  jobDescription[50];
  nuint8  clientRecordArea[152];
} NWQueueJobStruct;

typedef struct
{
  nuint32 clientStation;
  nuint32 clientTask;
  nuint32 clientID;
  nuint32 targetServerID;
  nuint8  targetExecutionTime[6];
  nuint8  jobEntryTime[6];
  nuint32 jobNumber;
  nuint16 jobType;
  nuint16 jobPosition;
  nuint16 jobControlFlags;
  nuint8  jobFileName[14];
  nuint32 jobFileHandle;
  nuint32 servicingServerStation;
  nuint32 servicingServerTask;
  nuint32 servicingServerID;
} NWReplyJobStruct;

typedef struct
{
  nuint32 totalQueueJobs;
  nuint32 replyQueueJobNumbers;
  nuint32 jobNumberList[250];   /* 250 to hold job #'s for old NCP*/
} QueueJobListReply;

N_EXTERN_LIBRARY( NWCCODE )
NWCreateQueueFile
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   QueueJobStruct N_FAR * job,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCreateQueueFile2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   NWQueueJobStruct N_FAR * job,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseFileAndStartQueueJob
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseFileAndStartQueueJob2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseFileAndAbortQueueJob
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseFileAndAbortQueueJob2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWRemoveJobFromQueue
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWRemoveJobFromQueue2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetQueueJobList
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   pnuint16       jobCount,
   pnuint16       jobList
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetQueueJobList2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        queueStartPos,
   QueueJobListReply N_FAR * job
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueJobEntry
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   QueueJobStruct N_FAR * job
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueJobEntry2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   NWQueueJobStruct N_FAR * job
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetQueueJobFileSize
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   pnuint32       fileSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetQueueJobFileSize2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   pnuint32       fileSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeQueueJobEntry
(
   NWCONN_HANDLE                conn,
   nuint32                      queueID,
   const QueueJobStruct N_FAR * job
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeQueueJobEntry2
(
   NWCONN_HANDLE                  conn,
   nuint32                        queueID,
   const NWQueueJobStruct N_FAR * job
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeQueueJobPosition
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   nuint8         newJobPos
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeQueueJobPosition2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   nuint32        newJobPos
);

N_EXTERN_LIBRARY( NWCCODE )
NWServiceQueueJob
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        targetJobType,
   QueueJobStruct N_FAR * job,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWServiceQueueJob2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        targetJobType,
   NWQueueJobStruct N_FAR * job,
   NWFILE_HANDLE N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWAbortServicingQueueJob
(
   NWCONN_HANDLE  conn,
   nuint32        QueueID,
   nuint16        JobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWAbortServicingQueueJob2
(
   NWCONN_HANDLE  conn,
   nuint32        QueueID,
   nuint32        JobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeToClientRights
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeToClientRights2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWFinishServicingQueueJob
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint16        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWFinishServicingQueueJob2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        jobNumber,
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetPrinterQueueID
(
   NWCONN_HANDLE  conn,
   nuint16        printerNum,
   pnuint32       queueID
);

N_EXTERN_LIBRARY( NWCCODE )
NWCreateQueue
(
   NWCONN_HANDLE  conn,
   pnstr8         queueName,
   nuint16        queueType,
   nuint8         dirPath,
   pnstr8         path,
   pnuint32       queueID
);

N_EXTERN_LIBRARY( NWCCODE )
NWDestroyQueue
(
   NWCONN_HANDLE  conn,
   nuint32        queueID
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueCurrentStatus
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   pnuint8        queueStatus,
   pnuint16       numberOfJobs,
   pnuint16       numberOfServers,
   pnuint32       serverIDlist,
   pnuint16       serverConnList
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueCurrentStatus2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   pnuint32       queueStatus,
   pnuint32       numberOfJobs,
   pnuint32       numberOfServers,
   pnuint32       serverIDlist,
   pnuint32       serverConnList
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetQueueCurrentStatus
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint8         queueStatus
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetQueueCurrentStatus2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        queueStatus
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueServerCurrentStatus
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        serverID,
   nuint16        serverConn,
   nptr           statusRec
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadQueueServerCurrentStatus2
(
   NWCONN_HANDLE  conn,
   nuint32        queueID,
   nuint32        serverID,
   nuint32        serverConn,
   nptr           statusRec
);

N_EXTERN_LIBRARY( NWCCODE )
NWAttachQueueServerToQueue
(
   NWCONN_HANDLE  conn,
   nuint32        queueID
);

N_EXTERN_LIBRARY( NWCCODE )
NWDetachQueueServerFromQueue
(
   NWCONN_HANDLE  conn,
   nuint32        queueID
);

N_EXTERN_LIBRARY( NWCCODE )
NWRestoreQueueServerRights
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetQueueServerCurrentStatus
(
   NWCONN_HANDLE      conn,
   nuint32            queueID,
   const void N_FAR * statusRec
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
