#ifndef _NWADV_H_
#define _NWADV_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwadv.h
==============================================================================
*/
#include <nwtypes.h>
#include <nwfattr.h>

#include <npackon.h>

/* Resource tag signatures for AllocateResourceTag */
#define AllocSignature                 0x54524C41
#define AESProcessSignature            0x50534541
#define CacheNonMovableMemorySignature 0x544D4E43
#define ConsoleCommandSignature        0x4D4F4343
#define HardwareInterruptSignature     0x50544E49
#define InterruptTimeCallBackSignature 0x524D4954
#define SemiPermMemorySignature        0x454D5053
#define DebuggerSignature              0x47554244
#define BreakpointSignature            0x54504B42

/* Data structure for RegisterConsoleCommand */
struct commandParserStructure
{
   struct commandParserStructure *Link; /* set by RegisterConsoleCommand */
   LONG (*parseRoutine)(                /* parsing routing (user defined) */
         LONG screenID,
         BYTE *commandLine);
   LONG RTag;                          /* set to resource tag */
};

/*
** Structures and constants for RegisterForEvent function. Unless otherwise
** noted an event does NOT call a Warn routine.
*/

#define EVENT_VOL_SYS_MOUNT          0
   /* parameter is undefined. Report Routine will be called immediately 
    * after vol SYS has been mounted.
    */

#define EVENT_VOL_SYS_DISMOUNT       1
   /* parameter is undefined. Warn Routine and Report Routine will be
    * called before vol SYS is dismounted.
    */

#define EVENT_ANY_VOL_MOUNT          2
   /* parameter is volume number. Report Routine will be called immediately
    * after any volume is mounted.
    */

#define EVENT_ANY_VOL_DISMOUNT       3
   /* parameter is volume number. Warn Routine and Report Routine will be
    * called before any volume is dismounted.
    */

#define EVENT_DOWN_SERVER            4
   /* parameter is undefined. Warn Routine and Report Routine will be
    * called before the server is shut down.
    */

#define EVENT_EXIT_TO_DOS            7
   /* parameter is undefined. The Report Routine will be called before the
    * server exits to DOS.
    */

#define EVENT_MODULE_UNLOAD          8
   /* parameter is module handle. Warn Routine and Report Routine will be
    * called when a module is unloaded from the console command line. Only
    * the Report Routine will be called when a module unloads itself.
    */

#define EVENT_CLEAR_CONNECTION       9
   /* parameter is connection number. Report Routine is called before the
    * connection is cleared.
    */

#define EVENT_LOGIN_USER            10
   /* parameter is connection number. Report Routine is called after the
    * connection has been allocated.
    */

#define EVENT_CREATE_BINDERY_OBJ    11
   /* parameter is object ID. Report Routine is called after the object is
    * created and entered in the bindery.
    */

#define EVENT_DELETE_BINDERY_OBJ    12
   /* parameter is object ID. Report Routine is called before the object is
    * removed from the bindery.
    */

#define EVENT_CHANGE_SECURITY       13
   /* parameter is a pointer a structure of type EventSecurityChangeStruct.
    * Report Routine is called after a security
    * equivalence change has occurred.
    */

#define EVENT_ACTIVATE_SCREEN       14
   /* Parameter is screen ID. Report routine is called after the
    * screen becomes the active screen.
    */

#define EVENT_UPDATE_SCREEN         15
   /* Parameter is screen ID. Report routine is called after a change is
    * made to the screen image.
    */

#define EVENT_UPDATE_CURSOR         16
   /* Parameter is screen ID. Report routine is called after a change to
    * the cursor position or state occurs.
    */

#define EVENT_KEY_WAS_PRESSED       17
   /* Parameter is undefined. Report routine is called whenever a
    * key on the keyboard is pressed (including shift/alt/control).
    * This routine is called at interrupt time.
    */

#define EVENT_DEACTIVATE_SCREEN     18
   /* Parameter is screen ID. Report routine is called when the
    * screen becomes inactive.
    */

#define EVENT_TRUSTEE_CHANGE        19
   /* Parameter is a pointer to type struct EventTrusteeChangeStruct. The
    * report routine is called everytime there is a change to a trustee in
    * the file system. Shouldn't sleep.
    */

#define EVENT_OPEN_SCREEN           20
   /* Parameter is the screen ID for the newly created screen. The report
    * routine will be called after the screen is created.
    */

#define EVENT_CLOSE_SCREEN          21
   /* Parameter is the screen ID for the screen that will be closed. The
    * report routine will be called before the screen is closed.
    */

#define EVENT_MODIFY_DIR_ENTRY      22
   /* Parameter is a pointer to a structure of type EventModifyDirEntryStruct
    * which contains the modify information. The report routine will be 
    * called right after the entry is changed but before the directory
    * entry is unlocked. The report routine must not go to sleep. 
    */

#define EVENT_NO_RELINQUISH_CONTROL 23
   /* Parameter is the running process. This will be called when the
    * timer detects that a process is hogging the processor. The report
    * routine must not sleep.
    */

#define EVENT_THREAD_SWITCH         25
   /* Parameter is the threadID of the thread that was executing when the
    * thread switch occurred. The report routine will be called when the
    * new thread begins executing. The report routine must not go to sleep.
    */

#define EVENT_MODULE_LOAD           27
   /* parameter is module handle. The report routine will be called
    * after a module has loaded.
    */

#define EVENT_CREATE_PROCESS        28
   /* parameter is the PID of the process being created. It is called
    * after the process is created. The report routine may not sleep.
    */

#define EVENT_DESTROY_PROCESS       29
   /* parameter is the PID of the process being destroyed. It is called
    * before the process is actually destroyed. The report routine may not
    * sleep.
    */

#define EVENT_NEW_PUBLIC            32
   /* Parameter is a pointer to a length preceded string which is the name
    * of the new public entry point. This event may not sleep.
    */

#define EVENT_PROTOCOL_BIND         33
   /* Parameter is a pointer to a structure of type EventProtocolBindStruct.
    * This event is generated every time a board is bound to a protocol.
    * This event may sleep.
    */

#define EVENT_PROTOCOL_UNBIND       34
   /* Parameter is a pointer to a structure of type EventProtocolBindStruct.
    * This event is generated every time a board is unbound from a protocol.
    * This event may sleep.
    */

#define EVENT_ALLOCATE_CONNECTION   37
   /* parameter is connection number. Report Routine is called after the
    * connection is allocated.
    */

#define EVENT_LOGOUT_CONNECTION     38
   /* parameter is connection number. Report Routine is called before the
    * connection is logged out. The event handler may sleep.
    */

#define EVENT_MLID_REGISTER         39
   /* parameter is board number. Report Routine is called after the MLID
    * is registered.
    */

#define EVENT_MLID_DEREGISTER       40
   /* parameter is board number. Report Routine is called before the MLID
    * is deregistered.
    */

#define   EVENT_DATA_MIGRATION      41
   /* Parameter is a pointer to a structure of type EventDateMigrationInfo.
    * This event is generated when a file's data has been migrated.
   */

#define   EVENT_DATA_DEMIGRATION    42
   /* Parameter is a pointer to a structure of type EventDateMigrationInfo.
    * This event is generated when a file's data has been de-migrated.
   */

#define   EVENT_QUEUE_ACTION        43
   /*   Parameter is a pointer to a structure of type EventQueueNote.
    * This event is generated when a queue is activated, deactivated,
    * created, or deleted.
   */

#define   EVENT_NETWARE_ALERT       44
   /* Parameter is a pointer to a structure of type EventNetwareAlertStruct.
    * This event is generated anytime the following alert calls are
    * made:
    *         NetWareAlert        NW 4.X
    *
    *   The report routine may sleep.
    */

#define EVENT_CREATE_OBJECT         46
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_DELETE_OBJECT         47
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_RENAME_OBJECT         48
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_VALUE_CHANGE          49
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_CLOSE_FILE            50
   /* Parameter is a pointer to a structure of type EventCloseFileInfo. */

#define EVENT_CHANGE_TIME           51
   /* This event is given when the time is changed or when Time
    * Synchronization schedules a nonuniform adjustment. The parameter is
    * the UTC time (in seconds) before the time change. The current time
    * is available from the OS. Since you have no way of knowing the
    * magnitudue of the time change, nor whether it has taken place or is
    * scheduled for the next clock interrupt, you must detect the time
    * change on your own. In general, if current time is less than old
    * time, or at least two seconds ahead of old time, then the time change
    * has been applied. You must wait for one of those conditions to be
    * sure that the time change has "settled down" before you can assume
    * that the event has "happened."
    */

#define EVENT_MOVE_OBJECT           52
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_VALUE_ADD             53
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_VALUE_DEL             54
   /* Parameter is a pointer to a structure of type EventBinderyObject
    * or EventDSObject
    */

#define EVENT_DM_KEY_MODIFIED       55
   /* Parameter is a pointer to a structure of type EventDMKeyModified
    */

#define EVENT_MODULE_UNLOADED       56
   /* Parameter is module handle. Report Routine will be called after the
    * NLM's exit routine has been called, after his resources have been
    * returned to the OS, and after he has been unlinked from the OS's lists.
    * The only thing left of this NLM is the memory for his load definition
    * structure, data image, and code image.
    */

#define EVENT_REMOVE_PUBLIC            57
   /* Parameter is the address of the public entry point. This only happens
    * on module unload.
    */

#define EVENT_DS_EVENT                 58
   /* Parameter is the address of a DS defined event structure  */

#define EVENT_UNICODE                  59
   /* Parameter is the address of a UNICODE defined event structure */

#define EVENT_SFT3_SERVER_STATE           60
   /* Parameter is the ServerState Number
    * (Refer to messtype.h, server state codes)
    * IOEngineState                          0
    * PrimaryNoSecondaryState                1
    * PrimarySyncingWithSecondaryState       2
    * PrimaryTransferingMemoryImageState     3
    * PrimaryWithSecondaryState              4
    * SecondaryTransferingMemoryImageState   5
    * SecondaryMirroredState                 6
   */

#define EVENT_SFT3_IMAGE_STATE         61
    /* Parameter is memory mirror state */
    /* 0 = Not mirrored */
    /* 1 = Mirrored */

#define EVENT_SFT3_PRESYNC_STATE           62
    /* called when the primary is about ready to synchronize */
    /* with the secondary  */
    /* Parameter is unsed for now. */
    /* This event report is allowed to sleep */

#define  EVENT_ALTERNATE_MOUNT_VOLUME     63
    /* called when NetWare is not aware of the volume name to be mounted, */
    /* Parameter is used to pass a event structure EventAlternateMountVolume.*/
    /* This event report is allowed to sleep, also the return code is in the */
    /* structre, after it has been processed. */

#define  EVENT_CONSOLE_CONFIG_COMMAND     64
   /* called when the console command CONFIG is typed on the server command */
   /* line. The event report is allowed to sleep.  The console screen handle */
   /* pointer is passed as the only parameter */

#define  EVENT_CONSOLE_VERSION_COMMAND    65
   /* called when the console command VERSION is typed on the server command */
   /* line. The event report is allowed to sleep.  A pointer to the structure */
   /* struct EventConfigVersionCmdInfo to help in the displaying to the screen */

#define EVENT_PRE_LOAD_NLM             66
   /* called while an NLM is being loaded but before most of the work is
    * done. The data and code segments have not been allocated yet. The
    * event report is allowed to sleep. The parameter is a pointer to an
    * NLM Load File Header structure.
    */

#define EVENT_LOW_MEMORY               67
   /* called when the cache memory allocator tries to allocate a cache block
    * and fails; only one event per minute will be generated. It happens
    * in conjunction with the netware alert. The event report can block.
    * The parameter is a zero. This event is mainly for OS2 based NetWare
    * so it can try to borrow memory back from OS2.
    */

   /*-----------------------------------------------------------*
    *Flags for the trustee change event (EVENT_TRUSTEE_CHANGE)  *
    *-----------------------------------------------------------*/
#define EVENT_NEW_TRUSTEE    1
#define EVENT_REMOVE_TRUSTEE 2

   /*-------------------------------------------------------------*
    *Flags for the change security event (EVENT_CHANGE_SECURITY)  *
    *-------------------------------------------------------------*/
#define EVENT_ADD_EQUIVALENCE    1
#define EVENT_REMOVE_EQUIVALENCE 2

   /*----------------------------------------------*
    *Structure returned for EVENT_TRUSTEE_CHANGE   *
    *----------------------------------------------*/
struct EventTrusteeChangeStruct
{
   LONG objectID;
   LONG entryID;
   LONG volumeNumber;
   LONG changeFlags;  /* flags are EVENT_NEW_TRUSTEE and EVENT_REMOVE_TRUSTEE */
   LONG newRights;
};

   /*-----------------------------------------------*
    *Structure returned for EVENT_CHANGE_SECURITY   *
    *-----------------------------------------------*/
struct EventSecurityChangeStruct
{
   LONG objectID;
   LONG equivalentID;
   LONG changeFlags;    /* EVENT_ADD_EQUIVALENCE and EVENT_REMOVE_EQUIVALENCE */
};

   /*------------------------------------------------*
    *Structure returned for EVENT_MODIFY_DIR_ENTRY   *
    *------------------------------------------------*/
struct EventModifyDirEntryStruct
{
   LONG primaryDirectoryEntry;
   LONG nameSpace;
   LONG modifyBits;
   struct ModifyStructure *modifyVector;
   LONG volumeNumber;
   LONG directoryEntry;
};

   /*----------------------------------------------------*
    *Structure returned for EVENT_PROTOCOL_BIND & UNBIND *
    *----------------------------------------------------*/
struct EventProtocolBindStruct
{
   LONG boardNumber;
   LONG protocolNumber;
};

   /*----------------------------------------------------------*
    *Structure returned for EVENT_DATA_MIGRATION & DEMIGRATION *
    *----------------------------------------------------------*/
struct EventDateMigrationInfo
{
   LONG FileSystemTypeID;
   LONG Volume;
   LONG DOSDirEntry;
   LONG OwnerDirEntry;
   LONG OwnerNameSpace;
   BYTE OwnerFileName[256];   /* 255 + 1 len byte */
};


   /*------------------------------------------------*
    *Structure returned for EVENT_QUEUE_ACTION       *
    *------------------------------------------------*/
struct EventQueueNote
{
   LONG   QAction;   /* 0=created, 1=deleted, 2 = activated, 3 = deactivated */
   LONG   QID;
   BYTE   QName[50];
};


   /*------------------------------------------------*
    *Structure returned for EVENT_NETWARE_ALERT      *
    *------------------------------------------------*/
struct EventNetwareAlertStruct
{
   LONG alertFlags;
   LONG alertId;
   LONG alertLocus;
   LONG alertClass;
   LONG alertSeverity;
   LONG targetStationCount;
   LONG targetStationList[32];
   LONG targetNotificationBits;
   LONG alertParmCount;
   void *alertDataPtr;
   void *NetWorkManagementAttributePointer;
   LONG alertUnused[2];
   LONG alertControlStringMessageNumber;
   BYTE alertControlString[256];
   BYTE alertParameters[256+256];
   BYTE alertModuleName[36];
   LONG alertModuleMajorVersion;
   LONG alertModuleMinorVersion;
   LONG alertModuleRevision;
};


struct EventBinderyObject
{
   LONG EventObjectType;      /* set to 'BIND' for bindery */
   LONG ObjectID;
   LONG ObjectType;
};

#define EventBinderySignature    0x444e4942   /* 'DNIB' */
#define EventDSSignature         0x43565344   /* 'CVSD' */

struct EventDSObject
{
   LONG EventObjectType;      /* set to 'DSVC' for directory services */
   LONG EventType;            /* add, delete, etc. */
   void *entry;               /* DS defined entry structure */
};

struct EventCloseFileInfo
{
   LONG fileHandle;
   LONG station;
   LONG task;
   LONG fileHandleFlags;
   LONG completionCode;
};

/* struct EventCloseFileInfo's fileHandleFlags */
#define ECNotReadableBit      0x00000001
#define ECNotWriteableBit     0x00000002
#define ECWrittenBit          0x00000004
#define ECDetachedBit         0x00000008
#define ECDirectFileSystemBit 0x00000020
#define ECFileWriteThroughBit 0x00000040

#include <npackoff.h>

#ifdef __cplusplus
extern "C"
{
#endif

extern int AddLanguage
(
   int   languageID,
   const BYTE *newLanguageName,
   int   showErrorsOnConsole
);

#ifndef _AllocateResourceTag_
# define _AllocateResourceTag_
extern LONG AllocateResourceTag
(
   LONG NLMHandle,
   const BYTE *descriptionString,
   LONG resourceType
);
#endif


extern void *DSAllocateEventTag
(
   LONG DSEventSignature
);


extern int GetCurrentOSLanguageID
(
   void
);

extern int GetFileHoleMap
(
   int   handle,
   LONG  startingPosition,
   LONG  numberOfBlocks,
   BYTE *replyBitMapP,
   LONG *allocationUnitSizeP
);

/* use this define if the misspelling is too annoying */
#define GetSettableParameterValue   GetSetableParameterValue
extern LONG GetSetableParameterValue
(
   LONG  connectionNumber,
   BYTE *setableParameterString, /* NULL terminated */
   void *returnValue
);

extern void *GetThreadDataAreaPtr
(
   void
);

extern void *ImportSymbol
(
   int  NLMHandle,
   const char *symbolName
);

extern LONG LoadLanguageMessageTable
(
   char ***messageTable,
   LONG *messageCount,
   LONG *languageID
);

extern int NWAddSearchPathAtEnd
(
   const BYTE *searchPath,
   LONG *number
);

extern int NWDeleteSearchPath(LONG searchPathNumber);

extern int NWGetSearchPathElement
(
   LONG searchPathNumber,
   LONG *isDOSSearchPath,
   BYTE *searchPath
);

extern int NWInsertSearchPath
(
   LONG searchPathNumber,
   const BYTE *path
);

extern LONG RegisterConsoleCommand
(
   struct commandParserStructure *newCommandParser
);

extern LONG RegisterForEvent
(
   LONG eventType,
   void (*reportProcedure)( LONG parameter, LONG userParameter ),
   LONG (*warnProcedure)
   (
      void (*OutputRoutine)( const void *controlString, ... ),
      LONG parameter,
      LONG userParameter
   )
);

extern int RenameLanguage
(
   int   languageID,
   const BYTE *newLanguageName,
   int   showErrorsToConsole
);

extern int ReturnLanguageName
(
   int   languageID,
   BYTE *languageName
);

extern void SaveThreadDataAreaPtr
(
   void  *threadDataAreaPtr
);

extern LONG ScanSetableParameters
(
   LONG  scanCategory,  /* -1 for all, COMMUNICATIONS, MEMORY, etc */
   LONG *scanSequence,  /* 0 for first time */
   BYTE *rParameterName,
   LONG *rType,         /* 0 = number, 1 = boolean, 2 = time ticks., etc */
   LONG *rFlags,        /* STARTUP, HIDE, etc */
   LONG *rCategory,     /* COMMUNICATIONS, MEMORY, etc */
   void *rParameterDescription, /* description string */
   void *rCurrentValue,
   LONG *rLowerLimit,
   LONG *rUpperLimit
);

extern int SetCurrentOSLanguageID
(
   LONG newLanguageID
);

extern LONG SetSetableParameterValue
(
   LONG connectionNumber,
   BYTE *setableParameterString,   /* NULL terminated */
   void *newValue
);

extern void SynchronizeStart
(
   void
);

extern int UnimportSymbol
(
   int   NLMHandle,
   const char *symbolName
);

extern LONG UnRegisterConsoleCommand
(
   struct commandParserStructure *commandParserToDelete
);

extern int UnregisterForEvent
(
   LONG eventHandle
);

#ifdef __cplusplus
}
#endif


#endif
