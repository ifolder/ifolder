#ifndef _NWMEDIAM_H_
#define _NWMEDIAM_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwmediam.h
==============================================================================
*/
#include <nwtypes.h>

#include <npackon.h>

/* Control Functions */
#define FORMAT_MEDIA        0x0000
#define TAPE_CONTROL        0x0001
      
#define ACTIVATE_FUNCTIONS  0x0003
#define MOUNT_FUNCTIONS     0x0004
#define SELECT_FUNCTIONS    0x0005
#define INSERTION_FUNCTIONS 0x0006
#define LOCK_FUNCTIONS      0x0007
#define MOVE_FUNCTIONS      0x0008
#define STAMP_FUNCTIONS     0x0009
#define SCAN_FUNCTIONS      0x000A

#define MAGAZINE_FUNCTIONS  0x000D


/* IO Functions */
#define RANDOM_READ          0x0020
#define RANDOM_WRITE         0x0021
#define RANDOM_WRITE_ONCE    0x0022
#define SEQUENTIAL_READ      0x0023
#define SEQUENTIAL_WRITE     0x0024
#define RESET_END_OF_TAPE    0x0025
#define SINGLE_FILE_MARK     0x0026
#define MULTIPLE_FILE_MARK   0x0027
#define SINGLE_SET_MARK      0x0028
#define MULTIPLE_SET_MARK    0x0029
#define SPACE_DATA_BLOCKS    0x002A
#define LOCATE_DATA_BLOCKS   0x002B
#define POSITION_PARTITION   0x002C
#define POSITION_MEDIA       0x002D

#define DEVICE_GENERIC_IOCTL 0x003E

/* Object Types */
#define UNKNOWN_OBJECT            0xFFFF
#define ADAPTER_OBJECT            0
#define CHANGER_OBJECT            1
#define DEVICE_OBJECT             2
#define MEDIA_OBJECT              4
#define PARTITION_OBJECT          5
#define SLOT_OBJECT               6
#define HOTFIX_OBJECT             7
#define MIRROR_OBJECT             8
#define PARITY_OBJECT             9
#define VOLUME_SEG_OBJECT         10
#define VOLUME_OBJECT             11
#define CLONE_OBJECT              12
#define MAGAZINE_OBJECT           14

#define UNIDENTIFIABLE_MEDIA      0x00000001
#define HIGH_SIERRA_CDROM_MEDIA   0x00000002
#define ISO_CDROM_MEDIA           0x00000003
#define MAC_CDROM_MEDIA           0x00000004
#define NETWARE_FILE_SYSTEM_MEDIA 0x00000005
#define INTERNAL_IDENTIFY_TYPE    0x00000007
#define SMS_MEDIA_TYPE            0x00000008

/* Notify Event Bits */
#define NOTIFY_OBJECT_CREATION      0x0001
#define NOTIFY_OBJECT_DELETION      0x0002
#define NOTIFY_OBJECT_ACTIVATED     0x0004
#define NOTIFY_OBJECT_DEACTIVATED   0x0008
#define NOTIFY_OBJECT_RESERVATION   0x0010
#define NOTIFY_OBJECT_UNRESERVATION 0x0020

/* Object Status Bits */
#define OBJECT_ACTIVATED         0x00000001
#define OBJECT_PHANTOM           0x00000002
#define OBJECT_ASSIGNABLE        0x00000004
#define OBJECT_ASSIGNED          0x00000008

#define OBJECT_RESERVED          0x00000010
#define OBJECT_BEING_IDENTIFIED  0x00000020
#define OBJECT_MAGAZINE_LOADED   0x00000040
#define OBJECT_FAILURE           0x00000080

#define OBJECT_REMOVABLE         0x00000100
#define OBJECT_READ_ONLY         0x00000200

#define OBJECT_IN_DEVICE         0x00010000
#define OBJECT_ACCEPTS_MAGAZINES 0x00020000
#define OBJECT_IS_IN_A_CHANGER   0x00040000
#define OBJECT_LOADABLE          0x00080000

#define OBJECT_BEING_LOADED      0x00080000
#define OBJECT_DEVICE_LOCK       0x01000000
#define OBJECT_CHANGER_LOCK      0x02000000
#define OBJECT_REMIRRORING       0x04000000
#define OBJECT_SELECTED          0x08000000

/* Resource Tag Allocation Signatures */
#define MMApplicationSignature 0x50424D4D /* 'PAMM' */
#define MMNotifySignature      0x4F4E4D4D /* 'ONMM' */
#define MMIdentifySignature    0x44494D4D /* 'DIMM' */

/* AlertTypes */
#define ALERT_MESSAGE    0x00000001
#define ALERT_ACTIVATE   0x00000002
#define ALERT_DEACTIVATE 0x00000003
#define ALERT_DELETE     0x00000004

/* AlertReasons */
#define ALERT_HOTFIX_ERROR                   0x00000000
#define ALERT_DRIVER_UNLOAD                  0x00000001
#define ALERT_DEVICE_FAILURE                 0x00000002
#define ALERT_PROGRAM_CONTROL                0x00000003
#define ALERT_MEDIA_DISMOUNT                 0x00000004
#define ALERT_MEDIA_EJECT                    0x00000005
#define ALERT_SERVER_DOWN                    0x00000006
#define ALERT_SERVER_FAILURE                 0x00000007
#define ALERT_MEDIA_LOAD                     0x00000008
#define ALERT_MEDIA_MOUNT                    0x00000009
#define ALERT_DRIVER_LOAD                    0x0000000A
#define ALERT_LOST_SOFTWARE_FAULT_TOLERANCE  0x0000000B
#define ALERT_INTERNAL_OBJECT_DELETE         0x0000000C
#define ALERT_MAGAZINE_LOAD                  0x0000000D
#define ALERT_MAGAZINE_UNLOAD                0x0000000E
#define ALERT_DEVICE_GOING_TO_BE_REMOVED     0x0000000F
#define ALERT_CHECK_DEVICE                   0x00000010
#define ALERT_CONFIGURATION_CHANGE           0x00000011
#define ALERT_APPLICATION_UNREGISTER         0x00000012
#define ALERT_DAI_EMMULATION                 0x00000013
#define ALERT_LOST_HARDWARE_FAULT_TOLERANCE  0x00000014
#define ALERT_INTERNAL_OBJECT_CREATE         0x00000015
#define ALERT_INTERNAL_MANAGER_REMOVE        0x00000016
#define ALERT_DEVICE_GOING_TO_BE_DEACTIVATED 0x00000017
#define ALERT_DEVICE_END_OF_MEDIA            0x00000018
#define ALERT_MEDIA_INSERTED                 0x00000019
#define ALERT_UNKNOWN_DEVICE_ALERT           0x0000001A
#define ALERT_UNKNOWN_ADAPTER_ALERT          0x0000001B

/* Function Control (Priority) Bits */
#define PRIORITY_1            0x0001
#define PRIORITY_2            0x0002
#define ACCELERATED_BIT       0x0004
#define ELEVATOR_OFF_BIT      0x0008
#define RETURN_RAW_COMPLETION 0x0010
#define SCRAMBLE_BIT          0x0020

/* Application Alert Codes */
#define GOING_TO_BE_DEACTIVATED     0x0001
#define OBJECT_BEING_DEACTIVATED    0x0002
#define OBJECT_SIZE_CHANGED         0x0003
#define OBJECT_BEING_ACTIVATED      0x0004
#define OBJECT_BEING_DELETED        0x0005
#define OBJECT_LOST_FAULT_TOLERANCE 0x0006

/* Initial Completion Codes */
#define MESSAGE_PROCESSED              0x00
#define MESSAGE_DATA_MISSING           0x01
#define MESSAGE_POSTPONE               0x02
#define MESSAGE_ABORTED                0x03
#define MESSAGE_INVALID_PARAMETERS     0x04
#define MESSAGE_OBJECT_NOT_ACTIVE      0x05
#define MESSAGE_INVALID_OJECT          0x06
#define MESSAGE_FUNCTION_NOT_SUPPORTED 0x07
#define MESSAGE_INVALID_MODE           0x08
#define MESSAGE_INTERNAL_ERROR         0x09

/* FinalCompletion Codes */
#define FUNCTION_OK                          0x00

#define FUNCTION_CORRECTED_MEDIA_ERROR       0x10
#define FUNCTION_MEDIA_ERROR                 0x11
#define FUNCTION_DEVICE_ERROR                0x12
#define FUNCTION_ADAPTER_ERROR               0x13
#define FUNCTION_NOT_SUPPORTED_BY_DEVICE     0x14
#define FUNCTION_NOT_SUPPORTED_BY_DRIVER     0x15
#define FUNCTION_PARAMETER_ERROR             0x16
#define FUNCTION_MEDIA_NOT_PRESENT           0x17
#define FUNCTION_MEDIA_CHANGED               0x18
#define FUNCTION_PREVIOUSLY_WRITTEN          0x19
#define FUNCTION_MEDIA_NOT_FORMATED          0x1A
#define FUNCTION_BLANK_MEDIA                 0x1B
#define FUNCTION_END_OF_MEDIA                0x1C /*end of partition*/
#define FUNCTION_FILE_MARK_DETECTED          0x1D
#define FUNCTION_SET_MARK_DETECTED           0x1E
#define FUNCTION_WRITE_PROTECTED             0x1F
#define FUNCTION_OK_EARLY_WARNING            0x20
#define FUNCTION_BEGINNING_OF_MEDIA          0x21
#define FUNCTION_MEDIA_NOT_FOUND             0x22
#define FUNCTION_MEDIA_NOT_REMOVED           0x23
#define FUNCTION_UNKNOWN_COMPLETION          0x24
#define FUNCTION_DATA_MISSING                0x25
#define FUNCTION_HOTFIX_ERROR                0x26
#define FUNCTION_HOTFIX_UPDATE_ERROR         0x27
#define FUNCTION_IO_ERROR                    0x28
#define FUNCTION_CHANGER_SOURCE_EMPTY        0x29
#define FUNCTION_CHANGER_DEST_FULL           0x2A
#define FUNCTION_CHANGER_JAMMED              0x2B
#define FUNCTION_MAGAZINE_NOT_PRESENT        0x2D
#define FUNCTION_MAGAZINE_SOURCE_EMPTY       0x2E
#define FUNCTION_MAGAZINE_DEST_FULL          0x2F
#define FUNCTION_MAGAZINE_JAMMED             0x30
#define FUNCTION_ABORT_CAUSED_BY_PRIOR_ERROR 0x31
#define FUNCTION_CHANGER_ERROR               0x32
#define FUNCTION_MAGAZINE_ERROR              0x33

/* ErrorCodes */
#define MM_OK                      0x00
#define MM_INVALID_OBJECT          0x01
#define MM_INVALID_APPLICATION     0x02
#define MM_INVALID_RESOURCETAG     0x03
#define MM_MEMORY_ALLOCATION_ERROR 0x04
#define MM_INVALID_MODE            0x05
#define MM_RESERVATION_CONFLICT    0x06
#define MM_PARAMETER_ERROR         0x07
#define MM_OBJECT_NOT_FOUND        0x08
#define MM_ATTRIBUTE_NOT_SETABLE   0x09
#define MM_FAILURE                 0x0A

/* Console Human Jukebox Definitions */
#define HJ_INSERT_MESSAGE 0
#define HJ_EJECT_MESSAGE  1
#define HJ_ACK_MESSAGE    2
#define HJ_NACK_MESSAGE   3
#define HJ_ERROR          4


/* Media Manager Structures */
struct MM_F1_Structure
{
   WORD code;
   WORD control;
};

struct PrivateIOConfigurationStucture
{
   LONG f1;
   WORD f2;
   WORD f3;
   WORD f4[4];
   LONG f5;
   WORD f6;
   LONG f7;
   WORD f8;
   BYTE f9[2];
   BYTE f10[2];
   LONG f11;
   LONG f12;
   LONG f13;
   BYTE f14[18];
   LONG f15[2];
   WORD f16;
   BYTE f17[6];
};

struct AdapterInfoDef
{
   BYTE                                  systemtype;    
   BYTE                                  processornumber;
   WORD                                  uniquetag;
   LONG                                  systemnumber;
   LONG                                  devices[32];
   struct PrivateIOConfigurationStucture configinfo;
   BYTE                                  drivername[36];
   BYTE                                  systemname[64];
   LONG                                  numberofdevices;
   LONG                                  reserved[7];
};

struct AttributeInfoDef
{
   BYTE name[64];
   LONG attributetype;
   LONG nextattributeid;
   LONG attributesize;
};

struct ChangerInfoDef
{
   LONG numberofdevices;
   LONG numberofslots;
   LONG numberofmailslots;
   LONG reserved[8];
   LONG slotmappingtable[1];  
};

struct DeviceInfoDef
{
   LONG status;
   BYTE controllernumber;
   BYTE drivenumber;
   BYTE cardnumber;
   BYTE systemtype;
   BYTE accessflags;
   BYTE type;
   BYTE blocksize;
   BYTE sectorsize;
   BYTE heads;
   BYTE sectors;
   WORD cylinders;
   LONG capacity;
   LONG mmadapternumber;
   LONG mmmedianumber;
   BYTE rawname[40];
   LONG reserved[8];
};

struct PrivateMediaInfoDef
{
   BYTE f1[64];
   LONG f2;
   LONG f3;
};

struct GenericInfoDef
{
   struct PrivateMediaInfoDef mediainfo;
   LONG mediatype;
   LONG cartridgetype;

   LONG unitsize;
   LONG blocksize;
   LONG capacity;
   LONG preferredunitsize;

   BYTE name[64];
        
   LONG type;
   LONG status;
   LONG functionmask;
   LONG controlmask;

   LONG parentcount;
   LONG siblingcount;
   LONG childcount;
   LONG specificinfosize;

   LONG objectuniqueid;
   LONG mediaslot;
};

struct HotfixInfoDef
{
   LONG hotfixoffset;
   LONG hotfixidentifier;
   LONG numberoftotalblocks;
   LONG numberofusedblocks;
   LONG numberofavailableblocks;
   LONG numberofsystemblocks;
   LONG reserved[8];
};

struct IdentifierInfoDef
{
   LONG applicationtype;
   LONG mediatype;
   LONG cartridgetype;
   BYTE name[64];
   LONG stampflag;
};

struct InsertRequestDef
{
   LONG devicenumber;
   LONG mailslot;
   LONG medianumber;
   LONG mediacount;
};

struct MagazineInfoDef
{
   LONG numberofslots;
   LONG reserved[8];
   LONG slotmappingtable[1];  
};

struct MappintInfoHeaderDef
{
   LONG parentcount;
   LONG siblingcount;
   LONG childcount;
};

struct MediaInfoDef
{
   BYTE label[64];
   LONG identificationtype;
   LONG identificationtimestamp;
};

struct MediaRequestDef
{
   LONG devicenumber;
   LONG mailslot;
   LONG medianumber;
   LONG mediacount;
};

struct MirrorInfoDef
{
   LONG mirrorcount;      
   LONG mirroridentifier;
   LONG mirrormembers[8];
   BYTE mirrorsynchflags[8];
   LONG reserved[8];
};

struct PartitionInfoDef
{
   LONG partitionertype;
   LONG partitiontype;
   LONG partitionoffset;
   LONG partitionsize;
   LONG reserved[8];
};

struct ResourceTagDef
{
   LONG                   reserved[2];
   LONG                   resourcetagtype;
   LONG                   resourcetagcount;
   struct ResourceTagDef *resourcenext;  /* these also correspond to offsets in struct  ObjectDef */
   struct ResourceTagDef *resourcelast;
};

#include <npackoff.h>


#ifdef __cplusplus
extern "C"
{
#endif

extern LONG HJ_Media_Request
(
   struct InsertRequestDef *minfo,
   LONG                     requestcode,
   LONG                    *uniqueid
);

extern LONG HJ_Media_Request_Ack
(
   struct InsertRequestDef *minfo,
   LONG                     ackcode,
   LONG                     uniqueid
);

extern LONG MM_Abort_Function
(
   LONG messagehandle
);

extern LONG MM_Check_For_Pending_Aborts
(
   LONG OSRequestHandle
);

extern LONG MM_Create_Media_Object
(
   LONG                 objectnumber,
   struct MediaInfoDef *mediainfo
);

extern void MM_ExecuteMessages
(
   void
);

extern LONG MM_Find_Identifier
(
   LONG *lastidentifiernumber
);

extern LONG MM_Find_Object_Type
(
   LONG  type,
   LONG *nextindicator
);

extern LONG MM_Object_Blocking_IO
(
   LONG                  *returnparameter,
   LONG                   objecthandle,
   struct MM_F1_Structure function,
   LONG                   parameter0,
   LONG                   parameter1,
   LONG                   parameter2,
   LONG                   bufferlength,
   void                  *buffer
);

#if 0
/* This call is not handled by the server libraries */
extern LONG MM_Object_IO
(
   LONG                  *messagehandle,
   LONG                   applicationrequesthandle,
   LONG                   objecthandle,
   struct MM_F1_Structure function,
   LONG                   parameter0,
   LONG                   parameter1,
   LONG                   parameter2,
   LONG                   bufferlength,
   void                  *buffer,
   void                 (*callbackroutine)()
);
#endif

extern LONG MM_Register_Application
(
   LONG                  *applicationhandle,
   LONG                   applicationid,
   BYTE                  *name,
   LONG                   reserved,
   LONG                 (*mediaconsoleroutine)(),
   struct ResourceTagDef *resourcetag
);
                                  
extern LONG MM_Register_Identification_Routines
(
   LONG                  *oshandle,
   LONG                   applicationhandle,
   LONG                 (*identifyroutine)(),
   LONG                 (*unstamproutine)(),
   LONG                 (*stamproutine)(),
   LONG                   identifiertype,
   BYTE                  *identifiername,
   struct ResourceTagDef *resourcetag
);

extern LONG MM_Register_Notify_Routine
(
   LONG                  *oshandle,
   LONG                   applicationhandle,
   void                 (*notifyroutine)(),
   LONG                   objectclass,
   LONG                   eventmask,
   struct ResourceTagDef *resourcetag
);

#if 0
/* This call is not handled by the server libraries */
extern LONG MM_Release_Object
(
   LONG  objecthandle,
   LONG  applicationhandle
);
#endif

extern LONG MM_Release_Unload_Semaphore
(
   LONG currentinstance
);

extern LONG MM_Rename_Object
(
   LONG  objectID,
   const BYTE  *
);

#if 0
/* This call is not handled by the server libraries */
extern LONG MM_Reserve_Object
(
   LONG  *objecthandle,
   LONG   applicationidentifier,
   LONG   objectid,
   LONG   iomode,
   LONG   applicationhandle,
   LONG (*notifyroutine)()
);
#endif

extern LONG MM_Return_Identifier_Info
(
   LONG                      identifiernumber,
   struct IdentifierInfoDef *info
);

extern LONG MM_Return_Object_Attribute
(
   LONG  objectid,
   LONG  attributeid,
   LONG  length,
   void *info
);

extern LONG MM_Return_Object_Generic_Info
(
   LONG                   objectid,
   struct GenericInfoDef *info
);

extern LONG MM_Return_Object_Mapping_Info
(
   LONG  objectid,
   LONG  mappinginfolength,
   LONG *mappinginfo
);

extern LONG MM_Return_Object_Specific_Info
(
   LONG  objectid,
   LONG  infolength,
   void *info
);

extern LONG MM_Return_Object_Table_Size
(
   void
);

extern LONG MM_Return_Objects_Attributes
(
   LONG                     objectid,
   LONG                     attributeid,
   struct AttributeInfoDef *info
);

extern LONG MM_Set_Object_Attribute
(
   LONG  objecthandle,
   LONG  attributeid,
   LONG  length,
   void *info
);

extern LONG MM_Set_Unload_Semaphore
(
   LONG *currentinstance
);

extern LONG MM_Special_Object_Blocking_IO
(
   LONG                  *returnparameter,
   LONG                   objectnumber,
   struct MM_F1_Structure function,
   LONG                   parameter0,
   LONG                   parameter1,
   LONG                   parameter2,
   LONG                   bufferlength,
   void                  *buffer
);

extern LONG MM_Unregister_Application
(
   LONG applicationhandle,
   LONG applicationid
);

extern LONG MM_Unregister_Identification_Routines
(
   LONG handle,
   LONG applicationtype
);

extern LONG MM_Unregister_Notify_Routine
(
   LONG oshandle,
   LONG applicationhandle
);

#ifdef __cplusplus
}
#endif

#endif
