/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwqueue.h
==============================================================================
*/

#ifndef _NWQUEUE_H_
#define _NWQUEUE_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#include <npackon.h>

#define NWMAX_QENTRIES 250

/* the following manifest constant applies to server versions BELOW 3.X */
#define NWMAX_JOB_SERVERS 25

/* the following manifest constant applies to server versions ABOVE 2.X */
#define NWQ_MAX_JOB_SERVERS 50

#define QF_AUTO_START      0x08
#define QF_SERVICE_RESTART 0x10
#define QF_ENTRY_OPEN      0x20
#define QF_USER_HOLD       0x40
#define QF_OPERATOR_HOLD   0x80

/* Queue Status Flags */

#define QS_CANT_ADD_JOBS       0x01
#define QS_SERVERS_CANT_ATTACH 0x02
#define QS_CANT_SERVICE_JOBS   0x04

typedef struct JobStruct
{
   BYTE clientStation;
   BYTE clientTaskNumber;
   long clientIDNumber;
   long targetServerIDNumber;
   BYTE targetExecutionTime[6];
   BYTE jobEntryTime[6];
   WORD jobNumber;
   WORD jobType;
   BYTE jobPosition;
   BYTE jobControlFlags;
   BYTE jobFileName[14];
   BYTE jobFileHandle[6];
   BYTE serverStation;
   BYTE serverTaskNumber;
   long serverIDNumber;
   BYTE textJobDescription[50];
   BYTE clientRecordArea[152];
} JobStruct;

typedef struct NWStandardChronRec_t
{
   BYTE year;   /* (19)80 - (20)79   */
   BYTE month;  /* 1 = January,  ... */
   BYTE day;    /* 1 - 31            */
   BYTE hour;   /* 0 - 23            */
   BYTE minute; /* 0 - 59            */
   BYTE second; /* 0 - 59            */
} NWStandardChronRec_t;

typedef NWStandardChronRec_t  NWQChronRec_t;
typedef BYTE                  NWFileHandle_t[6];

typedef struct NWQEntityInfo_t
{
   LONG    clientConnNum;
   LONG    taskNum;
   LONG    id;
} NWQEntityInfo_t;

typedef struct NWQJobServerInfo_t
{
   LONG            id;
   NWQChronRec_t   executionTime;
} NWQJobServerInfo_t;

typedef struct NWQJobInfo_t
{
   NWQChronRec_t   entryTime;
   LONG            num;
   WORD            type;
   WORD            position;
   WORD            controlFlags;
   char            assocFileName[14];
   NWFileHandle_t  fileHandle;
} NWQJobInfo_t;

typedef struct NWQPrintJobInfo_t
{
   NWQChronRec_t   entryTime;
   LONG            num;
   WORD            formType;
   WORD            position;
   WORD            controlFlags;
   char            assocFileName[14];
   NWFileHandle_t  fileHandle;
} NWQPrintJobInfo_t;


typedef struct NWQJobRec_t
{
   BYTE                reserved[10];
   NWQEntityInfo_t     client;
   NWQJobServerInfo_t  target;
   NWQJobInfo_t        job;
   NWQEntityInfo_t     jobServer;
   BYTE                textJobDescription[50];
   BYTE                jobServerRecord[152];
} NWQJobRec_t;

typedef struct NWQPrintServerRec_t
{
   BYTE    versionNumber;
   BYTE    tabSize;
   WORD    numberOfCopies;
   WORD    printControlFlags;
   WORD    maxLinesPerPage;
   WORD    maxCharsPerLine;
   char    formName[13];
   BYTE    reserve[9];
   char    bannerNameField[13];
   char    bannerFileField[13];
   char    bannerFileName[14];
   char    directoryPath[80];
} NWQPrintServerRec_t;

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int AbortServicingQueueJobAndFile
(
   long queueID,
   WORD jobNumber,
   int  fileHandle
);

extern int AttachQueueServerToQueue
(
   long queueID
);

extern int ChangeQueueJobEntry
(
   long             queueID,
   const JobStruct *job
);

extern int ChangeQueueJobPosition
(
   long queueID,
   WORD jobNumber,
   BYTE newPosition
);

extern int ChangeToClientRights
(
   long queueID,
   WORD jobNumber
);

extern int CloseFileAndAbortQueueJob
(
   long queueID,
   WORD jobNumber,
   int  fileHandle
);

extern int CloseFileAndStartQueueJob
(
   long queueID,
   WORD jobNumber,
   int  fileHandle
);

extern int CreateAQueue
(
   const char *queueName,
   int         queueType,
   const char *pathName,
   long       *queueID
);

extern int CreateQueueJobAndFile
(
   long       queueID,
   JobStruct *job,
   int       *fileHandle
);

extern int DestroyQueue
(
   long queueID
);

extern int DetachQueueServerFromQueue
(
   long queueID
);

extern int FinishServicingQueueJobAndFile
(
   long queueID,
   WORD jobNumber,
   long charge,
   int  fileHandle
);

extern int GetQueueJobList
(
   long  queueID,
   WORD *jobCount,
   WORD *jobNumberList,
   WORD  maxJobNumbers
);

extern int GetQueueJobsFileSize
(
   long  queueID,
   int   jobNumber,
   long *fileSize
);

extern int NWQAbortJob
(
   LONG queueID,
   LONG jobNum,
   int  fileHandle
);

extern int NWQAbortJobService
(
   LONG queueID,
   LONG jobNum,
   int  fileHandle
);

extern int NWQAttachServer
(
   LONG queueID
);

extern int NWQBeginJobService
(
   LONG         queueID,
   WORD         targetJobType,
   NWQJobRec_t *jobInfo,
   int         *fileHandle
);

extern int NWQChangeJobEntry
(
   LONG               queueID,
   const NWQJobRec_t *jobInfo
);

extern int NWQChangeJobPosition
(
   LONG queueID,
   LONG jobNum,
   LONG newPosition
);

extern int NWQChangeJobQueue
(
   LONG  srcQueueID,
   LONG  srcJobNum,
   LONG  dstQueueID,
   LONG *dstJobNum
);

extern int NWQChangeToClientRights
(
   LONG queueID,
   LONG jobNum
);

extern int NWQCreate
(
   const char *queueName,
   WORD        queueType,
   const char *pathName,
   LONG       *queueID
);

extern int NWQCreateJob
(
   LONG         queueID,
   NWQJobRec_t *jobInfo,
   int         *fileHandle
);

extern int NWQDestroy
(
   LONG queueID
);

extern int NWQDetachServer
(
   LONG queueID
);

extern int NWQEndJobService
(
   LONG queueID,
   LONG jobNum,
   LONG chargeInfo,
   int  fileHandle
);

extern int NWQGetJobEntry
(
   LONG         queueID,
   LONG         jobNum,
   NWQJobRec_t *jobInfo
);

extern int NWQGetJobFileSize
(
   LONG  queueID,
   LONG  jobNum,
   LONG *fileSize
);

extern int NWQGetServers
(
   LONG  queueID,
   LONG *currentServers,
   LONG *qServerIDs,
   LONG *qServerConnNums
);

extern int NWQGetServerStatus
(
   LONG  queueID,
   LONG  jobServerID,
   LONG  jobServerConnNum,
   void *jobServerRecord
);

extern int NWQGetStatus
(
   LONG  queueID,
   LONG *queueStatus,
   LONG *currentEntries,
   LONG *currentServers
);

extern int NWQMarkJobForService
(
   LONG queueID,
   LONG jobNum,
   int  fileHandle
);

extern int NWQRemoveJob
(
   LONG queueID,
   LONG jobNum
);

extern int NWQRestoreServerRights
(
   void
);

extern int NWQScanJobNums
(
   LONG  queueID,
   LONG *queueSequence,
   LONG *totalJobs,
   LONG *jobCount,
   LONG *jobNumList
);

extern int NWQServiceJob
(
   LONG         queueID,
   LONG         targetJobTypesCount,
   const WORD  *targetJobTypes,
   NWQJobRec_t *jobInfo,
   int         *fileHandle
);

extern int NWQSetServerStatus
(
   LONG        queueID,
   const void *serverStatusRecord
);

extern int NWQSetStatus
(
   LONG queueID,
   LONG queueStatus
);

extern int ReadQueueCurrentStatus
(
   long  queueID,
   BYTE *queueStatus,
   BYTE *numberOfJobs,
   BYTE *numberOfServers,
   long *serverIDList,
   WORD *serverStationList,
   WORD  maxNumberOfServers
);

extern int ReadQueueJobEntry
(
   long       queueID,
   WORD       jobNumber,
   JobStruct *job
);

extern int ReadQueueServerCurrentStatus
(
   long  queueID,
   long  serverID,
   char  serverStation,
   char *serverStatusRecord
);

extern int RemoveJobFromQueue
(
   long queueID,
   WORD jobNumber
);

extern int RestoreQueueServerRights
(
   void
);

extern int ServiceQueueJobAndOpenFile
(
   long       queueID,
   WORD       targetJobType,
   JobStruct *job,
   int       *fileHandle
);

extern int SetQueueCurrentStatus
(
   long queueID,
   BYTE queueStatus
);

extern int SetQueueServerCurrentStatus
(
   long        queueID,
   const BYTE *serverStatusRecord
);

#ifdef __cplusplus
}
#endif


#endif
