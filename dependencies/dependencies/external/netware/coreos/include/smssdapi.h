/* **************************************************************************
   * Program Name:	   Storage Management Services (SDAPI)
   *
   * Filename:		   SMSSDAPI.H
   *
   * Date Created:	   Feburary 17, 1992
   *
   * Version:		   4.0
   *
   * Files used:	   smstypes.h, smsdefns.h, and smstserr.h.
   *
   * Date Modified:	   July 22, 1993
   *
   * Modifications:	   
   *
   * Comments:		   * * * P R E L I M I N A R Y * * *
   *
   * (C) Unpublished Copyright of Novell, Inc.	All Rights Reserved.
   *
   * No part of this file may be duplicated, revised, translated, localized or
   * modified in any manner or compiled, linked or uploaded or downloaded to or
   * from any computer system without the prior written consent of Novell, Inc.
   **************************************************************************/


// Constants
   // globally used constants

#if !defined(_SMSSDAPI_H_INCLUDED)
   #define _SMSSDAPI_H_INCLUDED

   #if !defined (_CLIB_HDRS_INCLUDED)
	   #define _CLIB_HDRS_INCLUDED
	   #include <string.h>
	   #include <stdlib.h>
	   #define NETWARE3X
	   #include <nwlocale.h>
   #endif

   #include <smstypes.h>
   #include <smsdefns.h>
   #include <smssderr.h>


   // General Defines

#define NWSMSD_WAIT_PENDING					   0xFFFFFFFF
#define NWSMSD_DONT_CARE					   0xFFFFFFFF

#define NWSMSD_UNKNOWN						   0xFFFFFFFF

#define NWSMSD_MAX_LABEL_LEN				   0x00000040

   // Defines for objectType parameters
#define NWSMSD_OBJECT_TYPE_CHANGER			   0x00000001

#define NWSMSD_OBJECT_TYPE_DEVICE			   0x00000002

#define NWSMSD_OBJECT_TYPE_MAGAZINE			   0x00000003

#define NWSMSD_OBJECT_TYPE_MEDIA			   0x00000004

#define NWSMSD_OBJECT_TYPE_MAIL_SLOT		   0x00000005

#define NWSMSD_OBJECT_TYPE_STORAGE_SLOT		   0x00000006

#define NWSMSD_OBJECT_TYPE_LAST				   0x00000007

   // NWSMSD_OBJECT_STATUS.object

#define NWSMSD_OBJECT_STATUS_IDLE			   0x00000000

#define NWSMSD_OBJECT_STATUS_DEACTIVED		   0x00000001

#define NWSMSD_OBJECT_STATUS_BUSY			   0x00000002

#define NWSMSD_OBJECT_STATUS_ATT_CHNG		   0x00000003

#define NWSMSD_OBJECT_STATUS_LAST			   0x00000004

   // NWSMSD_OBJECT_STATUS.objectOperation

#define NWSMSD_OPERATION_NONE				   0x00000000
#define NWSMSD_OPERATION_NON_IDLE			   0x00000001
#define NWSMSD_OPERATION_WRITING			   0x00000002
#define NWSMSD_OPERATION_READING			   0x00000003
#define NWSMSD_OPERATION_MOVING				   0x00000004
#define NWSMSD_OPERATION_FORMATTING			   0x00000005
#define NWSMSD_OPERATION_DELETING			   0x00000006
#define NWSMSD_OPERATION_LAST				   0x00000007

   // NWSMSD_OBJECT_STATUS.subjugateMode
   // NWSMSDSubjugateObject():subjugateMode
	   // (This field is bit mapped)

#define NWSMSD_SUBJUGATE_NONE				   0x00000000

#define NWSMSD_SUBJUGATE_READ				   0x00000001

#define NWSMSD_SUBJUGATE_WRITE				   0x00000002


   // NWSMSD_OBJECT_STATUS.reservedStatus

#define NWSMSD_RESERVED_NOT					   0x00000000

#define NWSMSD_RESERVED_TO_THIS_SDI			   0x00000001

#define NWSMSD_RESERVED_TO_OTHER_APP		   0x00000002

#define NWSMSD_RESERVE_TYPE_LAST			   0x00000003

   // NWSMSD_OBJECT_CAPACITY.factor

#define NWSMSD_CAPACITY_BYTE				   0x00000001
#define NWSMSD_CAPACITY_KILO				   0x00000002
#define NWSMSD_CAPACITY_MEGA				   0x00000003
#define NWSMSD_CAPACITY_GIGA				   0x00000004
#define NWSMSD_CAPACITY_TERA				   0x00000005

   // NWSMSD_DEVICE_STATUS.magazineMountStatus
   // NWSMSD_DEVICE_STATUS.mediaMountStatus
   // NWSMSD_MAGAZINE_STATUS.magazineMountStatus
   // NWSMSD_MEDIA_STATUS.mediaMountStatus

#define NWSMSD_MOUNT_STATUS_MOUNTED			   0x00000001

#define NWSMSD_MOUNT_STATUS_DISMOUNTED		   0x00000002
#define NWSMSD_MOUNT_STATUS_PENDING			   0x00000003
#define NWSMSD_MOUNT_STATUS_LAST			   0x00000004

   // NWSMSD_MEDIA_SATUS.mediaState
	   // This is a bit mapped field

#define NWSMSD_MEDIA_STATE_RO				   0x00000001

   // NWSMSD_DEVICE_INFO.deviceType

#define NWSMSD_DEVICE_TYPE_DISK				   0x00000001

#define NWSMSD_DEVICE_TYPE_TAPE				   0x00000002

#define NWSMSD_DEVICE_TYPE_WORM				   0x00000003

#define NWSMSD_DEVICE_TYPE_CDROM			   0x00000004

#define NWSMSD_DEVICE_TYPE_MO				   0x00000005

#define NWSMSD_DEVICE_TYPE_LAST				   0x00000007

   // NWSMSD_DEVICE_INFO.relationship
	   // (This is bit mapped)

#define NWSMSD_RELATION_SINGLE_MEDIA		   0x00000000

#define NWSMSD_RELATION_MAGAZINE			   0x00000001

#define NWSMSD_RELATION_CHANGER				   0x00000002

   // NWSMSD_DEVICE_INFO.compressionMethod

#define NWSMSD_COMPRESSION_NONE				   0x00000001

#define NWSMSD_COMPRESSION_DEV_PROP			   0x00000002

#define NWSMSD_COMPRESSION_LAST				   0x00000003

   // NWSMSD_DEVICE_INFO.encryptionMethod

#define NWSMSD_ENCRYPTION_NONE				   0x00000001
#define NWSMSD_ENCRYPTION_DEV_PROP			   0x00000002

#define NWSMSD_ENCRYPTION_LAST				   0x00000003

   // NWSMSD_DEVICE_INFO.eccLevel

#define NWSMSD_ECC_LEVEL_NONE				   0x00000001

#define NWSMSD_ECC_LEVEL_DEV_0				   0x00000002

#define NWSMSD_ECC_LEVEL_DEV_1				   0x00000003

#define NWSMSD_ECC_LEVEL_DEV_2				   0x00000004

#define NWSMSD_ECC_LEVEL_DEV_3				   0x00000005

#define NWSMSD_ECC_LEVEL_LAST				   0x00000006

   //  NWSMSD_MEDIA_INFO.dataFormatType
   //  NWSMSDMediaLabel():dataFormatType

#define NWSMSD_DFT_UNIDENTIFIABLE			   0x00000001
#define NWSMSD_DFT_HIGH_SIERRA_CDROM		   0x00000002
#define NWSMSD_DFT_ISO_CDROM				   0x00000003
#define NWSMSD_DFT_MAC_CDROM				   0x00000004
#define NWSMSD_DFT_NW_FILE_SYSTEM			   0x00000005
#define NWSMSD_DFT_INTERNAL_ID_TYPE			   0x00000007
#define NWSMSD_DFT_SMS_PRE_SIDF				   0x00000008
#define NWSMSD_DFT_SIDF						   0x00000009
#define NWSMSD_DFT_BLANK					   0x0000000A
#define NWSMSD_DFT_ERROR					   0x0000000B
#define NWSMSD_DFT_LAST						   0x0000000C


   // The following are media data format types designated for third parties
#define NWSMSD_UNREGISTERED_MASK			   0xF0000000

   // NWSMSD_MEDIA_INFO.mediaType

#define NWSMSD_MEDIA_TYPE_FIXED				   0x00000001

#define NWSMSD_MEDIA_TYPE_FLOPPY_5_25		   0x00000002

#define NWSMSD_MEDIA_TYPE_FLOPPY_3_50		   0x00000003

#define NWSMSD_MEDIA_TYPE_OPTICAL_5_25		   0x00000004

#define NWSMSD_MEDIA_TYPE_OPTICAL_3_50		   0x00000005

#define NWSMSD_MEDIA_TYPE_TAPE_0_50			   0x00000006

#define NWSMSD_MEDIA_TYPE_TAPE_0_25			   0x00000007

#define NWSMSD_MEDIA_TYPE_TAPE_8MM			   0x00000008

#define NWSMSD_MEDIA_TYPE_TAPE_4MM			   0x00000009

#define NWSMSD_MEDIA_TYPE_BERN_DISK			   0x0000000A

#define NWSMSD_MEDIA_TYPE_LAST				   0x0000000B

   // Defines for  NWSMSD_MEDIA_INFO.formatType

	  // These are to be determined

#define NWSMSD_FORMAT_TYPE_LAST				   0x00000001

   // NWSMSD_CONTROL_BLOCK.transferBufferState

#define NWSMSD_TBS_UNASSIGNED				   0x00000000

#define NWSMSD_TBS_AVAILABLE				   0x00000001

#define NWSMSD_TBS_READY_TO_TRANSFER		   0x00000002

#define NWSMSD_TBS_TRANSFER_IN_PROGRESS		   0x00000003

#define NWSMSD_TBS_TRANSFER_COMPLETE		   0x00000004

#define NWSMSD_TBS_TRANSFER_STATUS_LAST		   0x00000005

   // NWSMSD_CONTROL_BLOCK.sessionDataType

#define NWSMSD_SDT_TSA_DATA					   0x00000001

#define NWSMSD_SDT_END_OF_TSA_DATA			   0x00000002

#define NWSMSD_SDT_SESSION_TRAILER			   0x00000003

#define NWSMSD_SDT_SESSION_INDEX			   0x00000004

#define NWSMSD_SDT_MEDIA_INDEX				   0x00000005

#define NWSMSD_SDT_END_OF_SESSION			   0x00000006
#define NWSMSD_SDT_LAST						   0x00000007

   // NSWMSDMediaDismount():dismountMode

#define NWSMSD_DISMOUNT_NORMAL				   0x00000000

#define NWSMSD_DISMOUNT_WRITE_TRAILER		   0x00000001

#define NWSMSD_DISMOUNT_LAST				   0x00000002

   // NWSMSDMediaFormat():formatCommand
	   // (Bit map)

#define NWSMSD_FORMAT_MEDIA					   0x00000001

#define NWSMSD_FORMAT_PARTITION				   0x00000002

#define NWSMSD_FORMAT_LAST					   0x00000004

   // NWSMSDMediaDelete():deleteCommand

#define NWSMSD_DELETE_HEADER				   0x00000001

#define NWSMSD_DELETE_ERASE					   0x00000002

#define NWSMSD_DELETE_LAST					   0x00000003

   // NWSMSDMediaPosition():positionCommand

#define NWSMSD_POS_INQUIRE					   0x00000001

#define NWSMSD_POS_PARTITION				   0x00000002

#define NWSMSD_POS_SESSION_BEG_ABS			   0x00000003

#define NWSMSD_POS_SESSION_BEG_REL			   0x00000004

#define NWSMSD_POS_SESSION_END_ABS			   0x00000005

#define NWSMSD_POS_SESSION_END_REL			   0x00000006

#define NWSMSD_POS_SECTOR_SESSION_ABS		   0x00000007

#define NWSMSD_POS_SECTOR_SESSION_REL		   0x00000008

#define NWSMSD_POS_SECTOR_PART_ABS			   0x00000009

#define NWSMSD_POS_SECTOR_PART_REL			   0x0000000A

#define NWSMSD_POS_FILE_MARK_ABS			   0x0000000B

#define NWSMSD_POS_FILE_MARK_REL			   0x0000000C

#define NWSMSD_POS_SET_MARK_ABS				   0x0000000D

#define NWSMSD_POS_SET_MARK_REL				   0x0000000E

#define NWSMSD_POS_MEDIA_INDEX				   0x0000000F

#define NWSMSD_POS_REWIND_MEDIA				   0x00000010

#define NWSMSD_POS_RETENSION_MEDIA			   0x00000011

#define NWSMSD_POS_END_OF_MEDIA				   0x00000012

#define NWSMSD_POS_LAST_COMMAND				   0x00000013

   // NWSMSDMoveObject():moveCommand

#define NWSMSD_MOVE_INQUIRE					   0x00000001

#define NWSMSD_MOVE_OBJECT					   0x00000002

#define NWSMSD_MOVE_EJECT					   0x00000003

#define NWSMSD_MOVE_LAST_COMMAND			   0x00000004

   // NWSMSDModifyObjectInfo():modifyMap
   // NWSMSDQueryModifyObjectInfo():modifyMap
	   // (Bit map)

#define NWSMSD_MODIFY_UNIT_SIZE				   0x00000001

#define NWSMSD_MODIFY_READ_AFTER_WRITE		   0x00000002

#define NWSMSD_MODIFY_COMPRES_METHOD		   0x00000004

#define NWSMSD_MODIFY_ENCRYPT_METHOD		   0x00000008

#define NWSMSD_MODIFY_ECC_LEVEL				   0x00000010

#define NWSMSD_MODIFY_FORMAT_TYPE			   0x00000020

   // NWSMSDRegisterAlertRoutine():alertType

#define NWSMSD_ALRT_NEW_MEDIA				   0x00000001

#define NWSMSD_ALRT_NEW_MEDIA_NEEDED		   0x00000002

#define NWSMSD_ALRT_NEW_MEDIA_NOT_BLANK		   0x00000004

#define NWSMSD_ALRT_NEW_MEDIA_INCORRECT		   0x00000008

#define NWSMSD_ALRT_OBJECT_ADDED                           0x00000010



#define NWSMSD_ALRT_OBJECT_DELETED                         0x00000020

#define NWSMSD_ALRT_OBJECT_RES_CHANGE		   0x00000040

#define NWSMSD_ALRT_OBJECT_ATTR_CHANGE		   0x00000080

#define NWSMSD_ALRT_MEDIA_READ_ONLY			   0x00000100

//changed

//#define NWSMSD_ALRT_LAST_NUMBER                            0x00000200

#define NWSMSD_ALRT_NEW_MAGAZINE_NEEDED                            0x00000200

#define NWSMSD_ALRT_LAST_NUMBER                            0x00000400

// #define NWSMSD_ALRT_MAX_NUMBER                             0x00000020
#define NWSMSD_ALRT_MAX_NUMBER                             0x00000040

//end change



   // NWSMSDAlertResponse():alertResponseValue

#define NWSMSD_RESP_NEW_MEDIA_CONTINUE		   0x00000001

#define NWSMSD_RESP_NEW_MEDIA_ABORT			   0x00000002

#define NWSMSD_RESP_NEW_MEDIA_SKIP                 0x00000004

#define NWSMSD_RESP_MAX_NUMBER				   0x00000020

   // Defines That Have SDI Strings Associated With them

   // NWSMSDConvertValueToMessage():valueType

#define NWSMSD_VALUE_TYPE_MEDIA				   0x00000001

#define NWSMSD_VALUE_TYPE_DEVICE			   0x00000002

#define NWSMSD_VALUE_TYPE_OBJECT			   0x00000003

#define NWSMSD_VALUE_TYPE_RELATION			   0x00000004

#define NWSMSD_VALUE_TYPE_RESERVED			   0x00000005

#define NWSMSD_VALUE_TYPE_MODE				   0x00000006

#define NWSMSD_VALUE_TYPE_MOUNTED			   0x00000007

#define NWSMSD_VALUE_TYPE_FORMAT_TYPE		   0x00000008

#define NWSMSD_VALUE_TYPE_CAPACITY			   0x00000009

#define NWSMSD_VALUE_TYPE_OPERATION			   0x0000000A

#define NWSMSD_VALUE_TYPE_LAST				   0x0000000B

// End of constant definitions

   // Data Structures

   // NWSMSD_OBJECT_LOCATION
typedef struct
{
   UINT32					 parentUniqueID;
   UINT32					 parentObjectType;
   UINT32					 elementType;
   UINT32					 elementNumber;
} NWSMSD_OBJECT_LOCATION;


   // NWSMSD_OBJECT_CAPACITY
typedef struct
{
   UINT32					 factor;
   UINT32					 value;
} NWSMSD_OBJECT_CAPACITY;


   // NWSMSD_OBJECT_STATUS
typedef struct
{
   UINT32					 object;
   UINT32					 operation;
   UINT32					 subjugateMode;
   UINT32					 reservedStatus;
   UINT32					 numberOfSharedApps;
} NWSMSD_OBJECT_STATUS;


   // NWSMSD_CHANGER_STATUS
typedef struct
{
   NWSMSD_OBJECT_STATUS		 status;
   NWBOOLEAN32				 objectBeingMoved;
} NWSMSD_CHANGER_STATUS;


   // NWSMSD_DEVICE_STATUS
typedef struct
{
   NWSMSD_OBJECT_STATUS		 status;
   UINT32					 magazineMountStatus;
   UINT32					 mediaMountStatus;
} NWSMSD_DEVICE_STATUS;


   // NWSMSD_MAGAZINE_STATUS
typedef struct
{
   NWSMSD_OBJECT_STATUS		 status;
   UINT32					 magazineMountStatus;
} NWSMSD_MAGAZINE_STATUS;


   // NWSMSD_MEDIA_STATUS
typedef struct
{
   NWSMSD_OBJECT_STATUS		 status;
   UINT32					 mediaMountStatus;
   UINT32					 mediaState;
} NWSMSD_MEDIA_STATUS;


   // NWSMSD_OBJECT_INFO
typedef struct
{
   UINT32					 uniqueID;
   UINT32					 objectType;
   NWSMSD_OBJECT_LOCATION	 location;
   BUFFER					 name[NWSMSD_MAX_LABEL_LEN];
} NWSMSD_OBJECT_INFO;


   // NWSMSD_CHANGER_INFO
typedef struct
{
   NWSMSD_OBJECT_INFO		 objectInfo;
   NWSMSD_CHANGER_STATUS	 changerStatus;
   UINT32					 numberOfDevices;
   UINT32					 numberOfSlots;
   UINT32					 numberOfMailSlots;
} NWSMSD_CHANGER_INFO;


   // NWSMSD_DEVICE_INFO
typedef struct
{
   NWSMSD_OBJECT_INFO		 objectInfo;
   NWSMSD_DEVICE_STATUS		 deviceStatus;
   UINT32					 deviceType;
   UINT32					 relationship;
   NWBOOLEAN32				 sequential;
   NWBOOLEAN32				 removable;
   NWSMSD_OBJECT_CAPACITY	 capacity;
   UINT32					 unitSize;
   NWBOOLEAN32				 readAfterWrite;
   UINT32					 compressionMethod;
   UINT32					 encryptionMethod;
   UINT32					 eccLevel;
   UINT32					 reserved0;
   UINT32					 reserved1;
   UINT32					 reserved2;
   UINT32					 reserved3;
   UINT32					 reserved4;
} NWSMSD_DEVICE_INFO;


   // NWSMSD_MAGAZINE_INFO
typedef struct
{
   NWSMSD_OBJECT_INFO		 objectInfo;
   NWSMSD_MAGAZINE_STATUS	 magazineStatus;
   UINT32					 numberOfSlots;
} NWSMSD_MAGAZINE_INFO;


   // NWSMSD_MEDIA_INFO
typedef struct
{
   NWSMSD_OBJECT_INFO		 objectInfo;
   NWSMSD_MEDIA_STATUS		 mediaStatus;
   ECMATime					 dateAndTime;
   ECMATime					 setDateAndTime;
   UINT32					 number;
   UINT32					 dataFormatType;
   UINT32					 mediaType;
   UINT32					 formatType;
   UINT32					 unitSize;
   NWSMSD_OBJECT_CAPACITY	 totalCapacity;
   NWSMSD_OBJECT_CAPACITY	 capacityRemaining;
} NWSMSD_MEDIA_INFO;


   // NWSMSD_SESSION_INFO
typedef struct
{
   ECMATime					 sessionDateAndTime;
   char						 sessionDescription[NWSM_MAX_DESCRIPTION_LEN];
   char						 sourceName[NWSM_MAX_TARGET_SRVC_NAME_LEN];
   char						 sourceType[NWSM_MAX_TARGET_SRVC_TYPE_LEN];
   char						 sourceVersion[NWSM_MAX_TARGET_SRVC_VER_LEN];
   UINT32					 sessionID;
} NWSMSD_SESSION_INFO;


// Handles

typedef	   UINT32		   NWSMSD_CHANGER_HANDLE;
typedef	   UINT32		   NWSMSD_DEVICE_HANDLE;
typedef	   UINT32		   NWSMSD_MAGAZINE_HANDLE;
typedef	   UINT32		   NWSMSD_MEDIA_HANDLE;
typedef	   UINT32		   NWSMSD_SESSION_HANDLE;

   // NWSMSD_HEADER_BUFFER
typedef struct
{
   UINT32					 bufferSize;
   UINT32					 headerSize;
   UINT32					 overflowSize;
   BUFFER					 headerBuffer[1];
} NWSMSD_HEADER_BUFFER;


   // NWSMSD_TRANS_BUF_POSITION

typedef struct
{
   UINT32					 mediaNumber;
   UINT32					 partitionNumber;
   UINT32					 sessionSectorAddress;
   UINT32					 absoluteSectorAddress;
} NWSMSD_TRANS_BUF_POSITION;


   // NWSMSD_MEDIA_POSITION

typedef struct
{
   NWSMSD_SESSION_HANDLE	 sessionHandle;
   NWSMSD_SESSION_INFO		 sessionDesc;
   UINT32					 mediaNumber;
   UINT32					 partitionNumber;
   union
   {
	   INT32	 relative;
	   UINT32	 absolute;
   }						 session;
   union
   {
	   INT32	 relative;
	   UINT32	 absolute;
   }						 sectorOrMark;
} NWSMSD_MEDIA_POSITION;


   // NWSMSD_CONTROL_BLOCK

typedef struct
{
   UINT32					 transferBufferState;
   union
   {
	   NWSMSD_SESSION_HANDLE	 session;
	   NWSMSD_MEDIA_HANDLE		 media;
   } handle;
   UINT32					 transferBufferSequence;
   NWBOOLEAN32				 finalTransferBuffer;
   BUFFERPTR				 transferBuffer;
   UINT32					 transferBufferSizeAllocated;
   UINT32					 transferBufferSizeData;
   UINT32					 sessionDataType;
   UINT32					 transferBufferDataOffset;
   UINT32					 bytesNotTransfered;
   UINT32					 bytesSpanned;
   NWSMSD_TRANS_BUF_POSITION beginningPosition;
   NWSMSD_TRANS_BUF_POSITION endingPosition;
   UINT32					 completionStatus;
} NWSMSD_CONTROL_BLOCK;


   // NWSMSD_OBJECT_LIST
typedef struct
{
   UINT32					 totalCount;
   UINT32					 maxCount;
   UINT32					 responseCount;
   UINT32					 uniqueID;
   void						*objectInfoStructArray;
} NWSMSD_OBJECT_LIST;


   // NWSMSD_TRANSFER_BUF_INFO
typedef struct
{
   UINT32					 sectorSize;
   UINT32					 maxTransferBufferSize;
   UINT32					 applicationAreaSize;
   UINT32					 applicationAreaOffset;
   UINT32					 transferBufferDataOffset;
} NWSMSD_TRANSFER_BUF_INFO;


   // NWSMSD_TIMEOUTS
typedef struct
{
   UINT32					 NWSMSDListObjectsDevices;
   UINT32					 NWSMSDListObjectsMedia;
   UINT32					 NWSMSDSubjugateDevice;
   UINT32					 NWSMSDSubjugateMedia;
   UINT32					 NWSMSDMediaMount;
   UINT32					 NWSMSDMediaDismount;
   UINT32					 NWSMSDSessionOpenForWriting;
   UINT32					 NWSMSDSessionOpenForReading;
   UINT32					 NWSMSDSessionClose;
   UINT32					 NWSMSDSessionWriteData;
   UINT32					 NWSMSDSessionReadData;
   UINT32					 NWSMSDDataTransferCancel;
   UINT32					 NWSMSDMediaLabel;
   UINT32					 NWSMSDMediaFormat;
   UINT32					 NWSMSDMediaDelete;
   UINT32					 NWSMSDMediaHeaderReturn;
   UINT32					 NWSMSDMediaPosition;
   UINT32					 NWSMSDMoveObject;
   UINT32					 NWSMSDWriteRawData;
   UINT32					 NWSMSDReadRawData;
} NWSMSD_TIMEOUTS;


// End of structure definitions

// Function prototypes

   // These first two are used in sdi.h, not down below

typedef
CCODE _NWSMSDConnectToSDI(
		   STRING					 sdiName,
		   STRING					 sdiUserName,
		   void						*reserved,
		   UINT32					*connectionID);

typedef
CCODE _NWSMSDReleaseSDI(
		   UINT32					*connectionID);

typedef
CCODE _NWSMSDListObjects(
   UINT32					 connection,
   UINT32					 objectType,
   UINT32					 parentUniqueID,
   NWBOOLEAN32				 reScan,
   void						*authentication,
   NWSMSD_OBJECT_LIST		*objectList,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSubjugateObject(
   UINT32					 connection,
   UINT32					 objectType,
   void						*objectInfo,
   UINT32					 subjugateMode,
   void						*authentication,
   void						*objectHandle,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDEmancipateObject(
   UINT32					 connection,
   void						*objectHandle);


typedef
CCODE _NWSMSDMediaMount(
   UINT32					 connection,
   NWSMSD_DEVICE_HANDLE		 deviceHandle,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   void						*mediaAuthentication,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMediaDismount(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   UINT32					 dismountMode,
   NWSMSD_HEADER_BUFFER		*mediaTrailerInfo,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSessionOpenForWriting(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   NWSMSD_HEADER_BUFFER		*sessionHeaderInfo,
   NWSMSD_TRANSFER_BUF_INFO *transferBufferInfo,
   NWSMSD_SESSION_HANDLE	*sessionHandle,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSessionOpenForReading(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   NWSMSD_SESSION_INFO		*sessionDesc,
   void						*sessionAuthentication,
   NWSMSD_HEADER_BUFFER		*sessionHeader,
   UINT32					*sectorSize,
   UINT32					*transferBufferSize,
   NWSMSD_SESSION_HANDLE	*sessionHandle,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSessionClose(
   UINT32					 connection,
   NWSMSD_SESSION_HANDLE	*sessionHandle,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSessionWriteData(
   UINT32					 connection,
   UINT32					 engineHandle,
   NWSMSD_CONTROL_BLOCK		*controlBlock);


typedef
CCODE _NWSMSDSessionReadData(
   UINT32					 connection,
   UINT32					 engineHandle,
   NWSMSD_CONTROL_BLOCK		*controlBlock);


typedef
CCODE _NWSMSDDataTransferCancel(
   UINT32					 connection,
   UINT32					 engineHandle,
   NWSMSD_CONTROL_BLOCK		*controlBlock);


typedef
CCODE _NWSMSDMediaLabel(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   UINT32					 dataFormatType,
   NWSMSD_HEADER_BUFFER		*mediaHeaderInfo,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMediaFormat(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   UINT32					 formatCommand,
   UINT32					 numberOfPartitions,
   NWSMSD_OBJECT_CAPACITY	*partitionSizeArray,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMediaDelete(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   UINT32					 deleteMode,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMediaHeaderReturn(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   NWBOOLEAN32				 verifyHeader,
   NWSMSD_HEADER_BUFFER		*mediaHeader,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMediaPosition(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   NWSMSD_MEDIA_POSITION	*mediaPosition,
   UINT32					 positionCommand,
   void						*authentication,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDMoveObject(
   UINT32					 connection,
   UINT32					 objectHandle,
   UINT32					 moveCommand,
   NWSMSD_OBJECT_LOCATION	*location,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDGetObjectStatus(
   UINT32					 connection,
   UINT32					 uniqueID,
   UINT32					 allocatedStatusSize,
   void						*objectStatus);


typedef
CCODE _NWSMSDGetObjectInfo(
   UINT32					 connection,
   UINT32					 objectHandleOrID,
   void						*authentication,
   UINT32					 allocatedInfoSize,
   void						*objectInfo);


typedef
CCODE _NWSMSDModifyObjectInfo(
   UINT32					 connection,
   UINT32					 objectHandle,
   UINT32					 modifyMap,
   void						*objectInfo);


typedef
CCODE _NWSMSDQueryModifyObjectInfo(
   UINT32					 connection,
   UINT32					 objectHandle,
   UINT32					*modifyMap);


typedef
CCODE _NWSMSDRenameObject(
   UINT32					 connection,
   UINT32					 objectHandle,
   char						*newName);


typedef
CCODE _NWSMSDWriteRawData(
   UINT32					 connection,
   UINT32					 engineHandle,
   NWSMSD_CONTROL_BLOCK		*controlBlock);


typedef
CCODE _NWSMSDReadRawData(
   UINT32					 connection,
   UINT32					 engineHandle,
   NWSMSD_CONTROL_BLOCK		*controlBlock);


typedef
CCODE _NWSMSDAbortFunction(
   UINT32					 connection,
   UINT32					 engineHandle,
   CCODE					*completionStatus);


typedef
CCODE _NWSMSDSetSpanningSequence(
   UINT32					 connection,
   NWSMSD_MEDIA_HANDLE		 mediaHandle,
   UINT32					 number,
   NWSMSD_OBJECT_LOCATION	*sequenceArray);


typedef
CCODE _NWSMSDSetReadTimeouts(
   UINT32					 connection,
   NWSMSD_TIMEOUTS			*sdiTimeouts,
   NWBOOLEAN32				 setReadMode);


// Note: The NWSMSDAlertRoutine() routine is actually provided by the Engine

typedef 
void _NWSMSDEngineAlertRoutine(
   UINT32					 alertHandle,
   UINT32					 alertType,
   UINT32					 objectType,
   UINT32					 uniqueID,
   UINT32					 alertNumber,
   STRING					 alertString);

/* ExtRegisterAlertRoutine AK */
typedef 
void _NWSMSDExtEngineAlertRoutine(
   UINT32					 alertHandle,
   UINT32					 alertType,
   UINT32					 objectType,
   UINT32					 uniqueID,
   UINT32					 alertNumber,
   STRING					 alertString,
   void *			 objectPtr);
/* ExtRegisterAlertRoutine END AK */

typedef
CCODE _NWSMSDRegisterAlertRoutine(
   UINT32					 connection,
   UINT32					 alertType,
   _NWSMSDEngineAlertRoutine *alertRoutine);

/* ExtRegisterAlertRoutine AK */
typedef
CCODE _NWSMSDExtRegisterAlertRoutine(
   UINT32					 connection,
   UINT32					 alertType,
   _NWSMSDExtEngineAlertRoutine *alertRoutine,
   void			*objectPtr);
/* ExtRegisterAlertRoutine AK */

typedef
CCODE _NWSMSDAlertResponse(
   UINT32					 connection,
   UINT32					 alertHandle,
   UINT32					 alertType,
   UINT32					 alertResponseValue);


typedef
CCODE _NWSMSDConvertValueToMessage(
   UINT32					 connection,
   UINT32					 valueType,
   UINT32					 value,
   UINT32					 stringSize,
   STRING					 string);


typedef
CCODE _NWSMSDConvertError(
   UINT32					 connection,
   CCODE					 errorNumber,
   STRING					 string);


// end of Function Prototypes


CCODE NWSMListSDIs(
	   char							*pattern,
	   NWSM_NAME_LIST			   **nameList);

CCODE GetSDIFunction(
	   UINT32						 connectionID, 
	   UINT16						 smspcode);

CCODE NWSMSDConnectToSDI(
	   STRING						 sdiName,
	   STRING						 sdiUserName,
	   void							*reserved,
	   UINT32						*connectionID);

CCODE NWSMSDReleaseSDI(
	   UINT32						*connectionID);

CCODE NWSMUnsupported(
	   UINT32						 cp,
	   ...);

_NWSMSDListObjects				   NWSMSDListObjects;
_NWSMSDSubjugateObject			   NWSMSDSubjugateObject;
_NWSMSDEmancipateObject			   NWSMSDEmancipateObject;
_NWSMSDMediaMount				   NWSMSDMediaMount;
_NWSMSDMediaDismount			   NWSMSDMediaDismount;
_NWSMSDSessionOpenForWriting	   NWSMSDSessionOpenForWriting;
_NWSMSDSessionOpenForReading	   NWSMSDSessionOpenForReading;
_NWSMSDSessionClose				   NWSMSDSessionClose;
_NWSMSDSessionWriteData			   NWSMSDSessionWriteData;
_NWSMSDSessionReadData			   NWSMSDSessionReadData;
_NWSMSDDataTransferCancel		   NWSMSDDataTransferCancel;
_NWSMSDMediaLabel				   NWSMSDMediaLabel;
_NWSMSDMediaFormat				   NWSMSDMediaFormat;
_NWSMSDMediaDelete				   NWSMSDMediaDelete;
_NWSMSDMediaHeaderReturn		   NWSMSDMediaHeaderReturn;
_NWSMSDMediaPosition			   NWSMSDMediaPosition;
_NWSMSDMoveObject				   NWSMSDMoveObject;
_NWSMSDGetObjectStatus			   NWSMSDGetObjectStatus;
_NWSMSDGetObjectInfo			   NWSMSDGetObjectInfo;
_NWSMSDModifyObjectInfo			   NWSMSDModifyObjectInfo;
_NWSMSDQueryModifyObjectInfo	   NWSMSDQueryModifyObjectInfo;
_NWSMSDRenameObject				   NWSMSDRenameObject;
_NWSMSDWriteRawData				   NWSMSDWriteRawData;
_NWSMSDReadRawData				   NWSMSDReadRawData;
_NWSMSDAbortFunction			   NWSMSDAbortFunction;
_NWSMSDSetSpanningSequence		   NWSMSDSetSpanningSequence;
_NWSMSDSetReadTimeouts			   NWSMSDSetReadTimeouts;
_NWSMSDRegisterAlertRoutine		   NWSMSDRegisterAlertRoutine;
/* ExtRegisterAlertRoutine AK */
_NWSMSDExtRegisterAlertRoutine	   NWSMSDExtRegisterAlertRoutine;
/* ExtRegisterAlertRoutine END AK */
_NWSMSDAlertResponse			   NWSMSDAlertResponse;
_NWSMSDConvertValueToMessage	   NWSMSDConvertValueToMessage;
_NWSMSDConvertError				   NWSMSDConvertError;

#endif					  
/***************************************************************************/


