/******************************************************************************

  %name: nwadevnt.h %
  %version: 4 %
  %date_modified: Tue Dec  1 10:09:29 1998 %
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

#if ! defined ( NWADEVNT_H )
#define NWADEVNT_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#include "npackon.h"

/* Bit definitions for volume audit events used in bit map */
enum auditBitMapIDs
{
   /* first 32 (0 - 31)bits reserved for dir service */
   A_BIT_BIND_CHG_OBJ_SECURITY    = 32,
   A_BIT_BIND_CHG_PROP_SECURITY,
   A_BIT_BIND_CREATE_OBJ,
   A_BIT_BIND_CREATE_PROPERTY,
   A_BIT_BIND_DELETE_OBJ,
   A_BIT_BIND_DELETE_PROPERTY,
   A_BIT_CHANGE_DATE_TIME,
   A_BIT_CHANGE_EQUIVALENCE,
   A_BIT_CHANGE_SECURITY_GROUP,
   A_BIT_UCLOSE_FILE,
   A_BIT_CLOSE_BINDERY,
   A_BIT_UCREATE_FILE,
   A_BIT_CREATE_USER,
   A_BIT_UDELETE_FILE,
   A_BIT_DELETE_USER,
   A_BIT_DIR_SPACE_RESTRICTIONS,
   A_BIT_DISABLE_ACCOUNT,
   A_BIT_DOWN_SERVER,
   A_BIT_GRANT_TRUSTEE,
   A_BIT_INTRUDER_LOCKOUT_CHANGE,
   A_BIT_LOGIN_USER,
   A_BIT_LOGIN_USER_FAILURE,
   A_BIT_LOGOUT_USER,
   A_BIT_NET_LOGIN,
   A_BIT_UMODIFY_ENTRY,
   A_BIT_OPEN_BINDERY,
   A_BIT_UOPEN_FILE,
   A_BIT_UREAD_FILE,
   A_BIT_REMOVE_TRUSTEE,
   A_BIT_URENAME_MOVE_FILE,
   A_BIT_RENAME_USER,
   A_BIT_USALVAGE_FILE,
   A_BIT_STATION_RESTRICTIONS,
   A_BIT_CHANGE_PASSWORD,
   A_BIT_TERMINATE_CONNECTION,
   A_BIT_UP_SERVER,
   A_BIT_USER_CHANGE_PASSWORD,
   A_BIT_USER_LOCKED,
   A_BIT_USER_SPACE_RESTRICTIONS,
   A_BIT_USER_UNLOCKED,
   A_BIT_VOLUME_MOUNT,
   A_BIT_VOLUME_DISMOUNT,
   A_BIT_UWRITE_FILE,
   A_BIT_GOPEN_FILE,
   A_BIT_GCLOSE_FILE,
   A_BIT_GCREATE_FILE,
   A_BIT_GDELETE_FILE,
   A_BIT_GREAD_FILE,
   A_BIT_GWRITE_FILE,
   A_BIT_GRENAME_MOVE_FILE,
   A_BIT_GMODIFY_ENTRY,
   A_BIT_IOPEN_FILE,
   A_BIT_ICLOSE_FILE,
   A_BIT_ICREATE_FILE,
   A_BIT_IDELETE_FILE,
   A_BIT_IREAD_FILE,
   A_BIT_IWRITE_FILE,
   A_BIT_IRENAME_MOVE_FILE,
   A_BIT_IMODIFY_ENTRY,
   A_BIT_Q_ATTACH_SERVER,
   A_BIT_Q_CREATE,
   A_BIT_Q_CREATE_JOB,
   A_BIT_Q_DESTROY,
   A_BIT_Q_DETACH_SERVER,
   A_BIT_Q_EDIT_JOB,
   A_BIT_Q_JOB_FINISH,
   A_BIT_Q_JOB_SERVICE,
   A_BIT_Q_JOB_SERVICE_ABORT,
   A_BIT_Q_REMOVE_JOB,
   A_BIT_Q_SET_JOB_PRIORITY,
   A_BIT_Q_SET_STATUS,
   A_BIT_Q_START_JOB,
   A_BIT_Q_SWAP_RIGHTS,
   A_BIT_NLM_ADD_RECORD,
   A_BIT_NLM_ADD_ID_RECORD,
   A_BIT_CLOSE_MODIFIED_FILE,
   A_BIT_GCREATE_DIRECTORY,
   A_BIT_ICREATE_DIRECTORY,
   A_BIT_UCREATE_DIRECTORY,
   A_BIT_GDELETE_DIRECTORY,
   A_BIT_IDELETE_DIRECTORY,
   A_BIT_UDELETE_DIRECTORY, /* 113 */
   /* Start of Trusted NetWare Auditing */
   A_BIT_GET_CURRENT_ACCNT_STATS,
   A_BIT_SUBMIT_ACCOUNT_CHARGE,
   A_BIT_SUBMIT_ACCOUNT_HOLD,
   A_BIT_SUBMIT_ACCOUNT_NOTE,
   A_BIT_DISABLE_BROADCASTS,
   A_BIT_GET_BROADCAST_MESSAGE,
   A_BIT_ENABLE_BROADCASTS,
   A_BIT_BROADCAST_TO_CONSOLE,
   A_BIT_SEND_BROADCAST_MESSAGE,
   A_BIT_WRITE_EATTRIB,
   A_BIT_READ_EATTRIB,
   A_BIT_ENUM_EATTRIB,
   A_BIT_SEE_FSO,          /* not used */
   A_BIT_GET_FSO_RIGHTS,
   A_BIT_PURGE_FILE,
   A_BIT_SCAN_DELETED,
   A_BIT_DUPLICATE_EATTRIB,
   A_BIT_ALLOC_DIR_HANDLE,
   A_BIT_SET_HANDLE,
   A_BIT_SEARCH,
   A_BIT_GEN_DIR_BASE_AND_VOL,
   A_BIT_OBTAIN_FSO_INFO,
   A_BIT_GET_REF_COUNT,
   A_BIT_MODIFY_ENTRY_NO_SRCH, /* not used */
   A_BIT_SCAN_TRUSTEES,
   A_BIT_GET_OBJ_EFFECTIVE_RTS,
   A_BIT_PARSE_TREE,
   A_BIT_SET_SPOOL_FILE_FLAGS,
   A_BIT_RESTORE_Q_SERVER_RTS,
   A_BIT_Q_JOB_SIZE,
   A_BIT_Q_JOB_LIST,
   A_BIT_Q_JOB_FROM_FORM_LIST,
   A_BIT_READ_Q_JOB_ENTRY,
   A_BIT_MOVE_Q_JOB,
   A_BIT_READ_Q_STATUS,
   A_BIT_READ_Q_SERVER_STATUS,
   A_BIT_EXTENDED_SEARCH,
   A_BIT_GET_DIR_ENTRY,
   A_BIT_SCAN_VOL_USER_RESTR,
   A_BIT_VERIFY_SERIAL,
   A_BIT_GET_DISK_UTILIZATION,
   A_BIT_LOG_FILE,
   A_BIT_SET_COMP_FILE_SZ,
   A_BIT_DISABLE_LOGIN,
   A_BIT_ENABLE_LOGIN,
   A_BIT_DISABLE_TTS,
   A_BIT_ENABLE_TTS,
   A_BIT_SEND_CONSOLE_BCAST,
   A_BIT_GET_REMAIN_OBJ_DISK_SPC,
   A_BIT_GET_CONN_TASKS,
   A_BIT_GET_CONN_OPEN_FILES,
   A_BIT_GET_CONN_USING_FILE,
   A_BIT_GET_PHYS_REC_LOCKS_CONN,
   A_BIT_GET_PHYS_REC_LOCKS_FILE,
   A_BIT_GET_LOG_REC_BY_CONN,
   A_BIT_GET_LOG_REC_INFO,
   A_BIT_GET_CONN_SEMS,
   A_BIT_GET_SEM_INFO,
   A_BIT_MAP_DIR_TO_PATH,
   A_BIT_CONVERT_PATH_TO_ENTRY,
   A_BIT_DESTROY_SERVICE_CONN,
   A_BIT_SET_Q_SERVER_STATUS,
   A_BIT_CONSOLE_COMMAND,
   A_BIT_REMOTE_ADD_NS,
   A_BIT_REMOTE_DISMOUNT,
   A_BIT_REMOTE_EXE,
   A_BIT_REMOTE_LOAD,
   A_BIT_REMOTE_MOUNT,
   A_BIT_REMOTE_SET,
   A_BIT_REMOTE_UNLOAD,
   A_BIT_GET_CONN_RANGE,
   A_BIT_GET_VOL_LABEL,
   A_BIT_SET_VOL_LABEL,
   A_BIT_FAILED_MASV_ACCESS
};

/* The following is used for Volume Auditing Events */
typedef struct tagNWVolAuditRecord
{
   nuint16  eventTypeID;
   nuint16  chkWord;
   nuint32  connectionID;
   nuint32  processUniqueID;
   nuint32  successFailureStatusCode;
   nuint16  dosDate;
   nuint16  dosTime;
/* nuint8   extra[0];   start of 'union EventUnion'  */
}NWVolumeAuditRcd, N_FAR *pNWVolumeAuditRcd;

/* auditing events that are returned in the AuditRecord eventTypeID field */
enum auditedEventIDs
{
   A_EVENT_BIND_CHG_OBJ_SECURITY    = 1,
   A_EVENT_BIND_CHG_PROP_SECURITY   = 2,
   A_EVENT_BIND_CREATE_OBJ          = 3,
   A_EVENT_BIND_CREATE_PROPERTY     = 4,
   A_EVENT_BIND_DELETE_OBJ          = 5,
   A_EVENT_BIND_DELETE_PROPERTY     = 6,
   A_EVENT_CHANGE_DATE_TIME         = 7,
   A_EVENT_CHANGE_EQUIVALENCE       = 8,
   A_EVENT_CHANGE_SECURITY_GROUP    = 9,
   A_EVENT_CLOSE_FILE               = 10,
   A_EVENT_CLOSE_BINDERY            = 11,
   A_EVENT_CREATE_FILE              = 12,
   A_EVENT_CREATE_USER              = 13,
   A_EVENT_DELETE_FILE              = 14,
   A_EVENT_DELETE_USER              = 15,
   A_EVENT_DIR_SPACE_RESTRICTIONS   = 16,
   A_EVENT_DISABLE_ACCOUNT          = 17,
   A_EVENT_DOWN_SERVER              = 18,
   A_EVENT_GRANT_TRUSTEE            = 19,
   A_EVENT_INTRUDER_LOCKOUT_CHNG    = 20,
   A_EVENT_LOGIN_USER               = 21,
   A_EVENT_LOGIN_USER_FAILURE       = 22,
   A_EVENT_LOGOUT_USER              = 23,
   A_EVENT_NET_LOGIN                = 24,
   A_EVENT_MODIFY_ENTRY             = 25,
   A_EVENT_OPEN_BINDERY             = 26,
   A_EVENT_OPEN_FILE                = 27,
   A_EVENT_Q_ATTACH_SERVER          = 28,
   A_EVENT_Q_CREATE                 = 29,
   A_EVENT_Q_CREATE_JOB             = 30,
   A_EVENT_Q_DESTROY                = 31,
   A_EVENT_Q_DETACH_SERVER          = 32,
   A_EVENT_Q_EDIT_JOB               = 33,
   A_EVENT_Q_JOB_FINISH             = 34,
   A_EVENT_Q_JOB_SERVICE            = 35,
   A_EVENT_Q_JOB_SERVICE_ABORT      = 36,
   A_EVENT_Q_REMOVE_JOB             = 37,
   A_EVENT_Q_SET_JOB_PRIORITY       = 38,
   A_EVENT_Q_SET_STATUS             = 39,
   A_EVENT_Q_START_JOB              = 40,
   A_EVENT_Q_SWAP_RIGHTS            = 41,
   A_EVENT_READ_FILE                = 42,
   A_EVENT_REMOVE_TRUSTEE           = 43,
   A_EVENT_RENAME_MOVE_FILE         = 44,
   A_EVENT_RENAME_USER              = 45,
   A_EVENT_SALVAGE_FILE             = 46,
   A_EVENT_STATION_RESTRICTIONS     = 47,
   A_EVENT_CHANGE_PASSWORD          = 48,
   A_EVENT_TERMINATE_CONNECTION     = 49,
   A_EVENT_UP_SERVER                = 50,
   A_EVENT_USER_CHANGE_PASSWORD     = 51,
   A_EVENT_USER_LOCKED              = 52,
   A_EVENT_USER_SPACE_RESTRICTION   = 53,
   A_EVENT_USER_UNLOCKED            = 54,
   A_EVENT_VOLUME_MOUNT             = 55,
   A_EVENT_VOLUME_DISMOUNT          = 56,
   A_EVENT_WRITE_FILE               = 57,
   A_ACTIVE_CONNECTION_RCD          = 58,
   A_ADD_AUDITOR_ACCESS             = 59,
   A_ADD_AUDIT_PROPERTY             = 60,
   A_CHANGE_AUDIT_PASSWORD          = 61,
   A_DELETE_AUDIT_PROPERTY          = 62,
   A_DISABLE_VOLUME_AUDIT           = 63,
   A_OPEN_FILE_HANDLE_RCD           = 64,
   A_ENABLE_VOLUME_AUDITING         = 65,
   A_REMOVE_AUDITOR_ACCESS          = 66,
   A_RESET_AUDIT_FILE               = 67,
   A_RESET_AUDIT_FILE2              = 68,
   A_RESET_CONFIG_FILE              = 69,
   A_WRITE_AUDIT_BIT_MAP            = 70,
   A_WRITE_AUDIT_CONFIG_HDR         = 71,
   A_NLM_ADD_RECORD                 = 72,
   A_ADD_NLM_ID_RECORD              = 73,
   A_CHANGE_AUDIT_PASSWORD2         = 74,
   A_EVENT_CREATE_DIRECTORY         = 75,
   A_EVENT_DELETE_DIRECTORY         = 76,
   A_INTRUDER_DETECT                = 77,
   A_VOLUME_NAME_RCD                = 78,
   A_BEGIN_AUDIT_FILE_READ          = 79,
   A_VOLUME_NAME_RCD_2              = 80,
   A_DELETE_OLD_AUDIT_FILE          = 81,
   A_QUERY_AUDIT_STATUS             = 82,

   /* begin TNW changes */

   A_EVENT_GET_CURRENT_ACNT_STATS      = 200,
   A_EVENT_SUBMIT_ACCOUNT_CHARGE       = 201,
   A_EVENT_SUBMIT_ACCOUNT_HOLD         = 202,
   A_EVENT_SUBMIT_ACCOUNT_NOTE         = 203,
   A_EVENT_DISABLE_BROADCASTS          = 204,
   A_EVENT_GET_BROADCAST_MESSAGE       = 205,
   A_EVENT_ENABLE_BROADCASTS           = 206,
   A_EVENT_BROADCAST_TO_CONSOLE        = 207,
   A_EVENT_SEND_BROADCAST_MESSAGE      = 208,
   A_EVENT_WRITE_EATTRIB               = 209,
   A_EVENT_READ_EATTRIB                = 210,
   A_EVENT_ENUM_EATTRIB                = 211,
   A_EVENT_SEE_FSO                     = 212, /* not used */
   A_EVENT_GET_FSO_RIGHTS              = 213,
   A_EVENT_PURGE_FILE                  = 214,
   A_EVENT_SCAN_DELETED                = 215,
   A_EVENT_DUPLICATE_EATTRIB           = 216,
   A_EVENT_ALLOC_DIR_HANDLE            = 217,
   A_EVENT_SET_HANDLE                  = 218,
   A_EVENT_SEARCH                      = 219,
   A_EVENT_GEN_DIR_BASE_AND_VOL        = 220,
   A_EVENT_OBTAIN_FSO_INFO             = 221,
   A_EVENT_GET_REF_COUNT               = 222,
   A_EVENT_MODIFY_ENTRY_NO_SEARCH      = 223,
   A_EVENT_SCAN_TRUSTEES               = 224,
   A_EVENT_GET_OBJ_EFFECTIVE_RGHT      = 225,
   A_EVENT_PARSE_TREE                  = 226,
   A_EVENT_SET_SPOOL_FILE_FLAGS        = 227,
   A_EVENT_RESTORE_Q_SERVER_RGHT       = 228,
   A_EVENT_Q_JOB_SIZE                  = 229,
   A_EVENT_Q_JOB_LIST                  = 230,
   A_EVENT_Q_JOB_FROM_FORM_LIST        = 231,
   A_EVENT_READ_Q_JOB_ENTRY            = 232,
   A_EVENT_MOVE_Q_JOB                  = 233,
   A_EVENT_READ_Q_STATUS               = 234,
   A_EVENT_READ_Q_SERVER_STATUS        = 235,
   A_EVENT_EXTENDED_SEARCH             = 236,
   A_EVENT_GET_DIR_ENTRY               = 237,
   A_EVENT_SCAN_VOL_USER_RESTR         = 238,
   A_EVENT_VERIFY_SERIAL               = 239,
   A_EVENT_GET_DISK_UTILIZATION        = 240,
   A_EVENT_LOG_FILE                    = 241,
   A_EVENT_SET_COMP_FILE_SZ            = 242,
   A_EVENT_DISABLE_LOGIN               = 243,
   A_EVENT_ENABLE_LOGIN                = 244,
   A_EVENT_DISABLE_TTS                 = 245,
   A_EVENT_ENABLE_TTS                  = 246,
   A_EVENT_SEND_CONSOLE_BROADCAST      = 247,
   A_EVENT_GET_REMAIN_OBJ_DISK_SPC     = 248,
   A_EVENT_GET_CONN_TASKS              = 249,
   A_EVENT_GET_CONN_OPEN_FILES         = 250,
   A_EVENT_GET_CONN_USING_FILE         = 251,
   A_EVENT_GET_PHYS_REC_LOCKS_CONN     = 252,
   A_EVENT_GET_PHYS_REC_LOCKS_FILE     = 253,
   A_EVENT_GET_LOG_REC_BY_CONN         = 254,
   A_EVENT_GET_LOG_REC_INFO            = 255,
   A_EVENT_GET_CONN_SEMS               = 256,
   A_EVENT_GET_SEM_INFO                = 257,
   A_EVENT_MAP_DIR_TO_PATH             = 258,
   A_EVENT_CONVERT_PATH_TO_ENTRY       = 259,
   A_EVENT_DESTROY_SERVICE_CONN        = 260,
   A_EVENT_SET_Q_SERVER_STATUS         = 261,
   A_EVENT_CONSOLE_COMMAND             = 262,
   A_EVENT_REMOTE_ADD_NS               = 263,
   A_EVENT_REMOTE_DISMOUNT             = 264,
   A_EVENT_REMOTE_EXE                  = 265,
   A_EVENT_REMOTE_LOAD                 = 266,
   A_EVENT_REMOTE_MOUNT                = 267,
   A_EVENT_REMOTE_SET                  = 268,
   A_EVENT_REMOTE_UNLOAD               = 269,
   A_EVENT_GET_CONN_RANGE              = 270,
   A_EVENT_GET_VOL_LABEL               = 271,
   A_EVENT_SET_VOL_LABEL               = 272,
   A_EVENT_FAILED_MASV_ACCESS          = 273,
   A_EVENT_LAST_PLUS_ONE               = 274
};

typedef struct tagNWModifyStructure
{
   nuint8   *MModifyName;
   nuint32  MFileAttributes;
   nuint32  MFileAttributesMask;
   nuint16  MCreateDate;
   nuint16  MCreateTime;
   nuint32  MOwnerID;
   nuint16  MLastArchivedDate;
   nuint16  MLastArchivedTime;
   nuint32  MLastArchivedID;
   nuint16  MLastUpdatedDate;    /* also last modified date and time. */
   nuint16  MLastUpdatedTime;
   nuint32  MLastUpdatedID;
   nuint16  MLastAccessedDate;
   nuint16  MInheritanceGrantMask;
   nuint16  MInheritanceRevokeMask;
   nuint32  MMaximumSpace;
} NWModifyStructure, N_FAR *pNWModifyStructure;

#ifndef MModifyNameBit
#define MModifyNameBit           0x0001L
#define MFileAttributesBit       0x0002L
#define MCreateDateBit           0x0004L
#define MCreateTimeBit           0x0008L
#define MOwnerIDBit              0x0010L
#define MLastArchivedDateBit     0x0020L
#define MLastArchivedTimeBit     0x0040L
#define MLastArchivedIDBit       0x0080L
#define MLastUpdatedDateBit      0x0100L
#define MLastUpdatedTimeBit      0x0200L
#define MLastUpdatedIDBit        0x0400L
#define MLastAccessedDateBit     0x0800L
#define MInheritedRightsMaskBit  0x1000L
#define MMaximumSpaceBit         0x2000L
#endif

union EventUnion
{
   struct eventChgDate
   {
      nuint32  newDosDateTime;
   } EChgDate;

   struct eventCreateUser
   {
      nuint32  userID;
      nuint8   name[1];
   } ECreateUser;

   struct eventBindChgSecurity
   {
      nuint32  newSecurity;
      nuint32  oldSecurity;
      nuint8   name[1];
   } EBindChgSecurity;

   struct eventBindChgSecGrp
   {
      nuint32  addFlag;
      nuint8   objName[1];                             /* obj name */
      nuint8   name[1];                                /* member name */
   } EBindChgSecGrp;

   struct eventBindCreateObj
   {
      nuint32  objectID;
      nuint32  security;
      nuint8   name[1];
   } EBindCreateObj;

   struct eventBindCreateProp
   {
      nuint32  security;
      nuint8   name[1];
   } EBindCreateProp;

   struct eventBindDeleteProp
   {
      nuint8   name[1];
   } EBindDeleteProp;

   struct eventIntruderLockoutChg
   {
      nuint8   hbaa;         /* nuint8 exchanged allowed attempts */
      nuint8   lbaa;
      nuint8   hbrm;         /* reset minutes */
      nuint8   lbrm;
      nuint8   hblm;         /* lock minutes */
      nuint8   lblm;
   } EILockChg;

   struct eventLogin
   {
      nuint32  userID;
      nuint8   networkAddressType;
      nuint8   networkAddressLength;
      nuint8   networkAddress[1];   /* variable length */
      nuint8   name[1];
   } ELogin;


   struct eventChgPasswd
   {
      nuint8   name[1];      /* object or user name */
   } EChgPasswd;

   struct eventChgSecurity
   {
      nuint32  newSecurity;
      nuint32  oldSecurity;
      nuint8   name[1];
   } EChgSecurity;

   struct eventFDelete
   {
      nuint32  nameSpace;
      nuint8   fileName[1];
   } EFDelete;

   struct eventFOpen
   {
      nuint32  handle;
      nuint32  rights;
      nuint32  nameSpace;
      nuint8   fileName[1];
   } EFOpen;

   struct eventFClose
   {
      nuint32  handle;
      nuint32  modified;
   } EFClose;

   struct eventFRead
   {
      nuint32  handle;
      nuint32  byteCount;
      nuint32  offset;
   } EFRead;

   struct eventAuditProperty
   {
      nuint8   name[1];
   } EAuditProperty;

   struct eventModify                            /* modify dir entry */
   {
      nuint32  modifyBits;
      nuint32  nameSpace;
      nuint8   modifyStruct[ sizeof(NWModifyStructure) ];
      nuint8   fileName[1];
      /* the following length preceeded strings are optional
         as defined by the modify bits */
      nuint8   oldDosName[1];
      nuint8   newOwner[1];
      nuint8   lastArchivedBy[1];
      nuint8   lastModifiedBy[1];
   } EModify;

   struct eventQAttach
   {
      nuint8   qname[1];
   } EQAttach;

   struct eventQCreate
   {
      nuint32  qType;
      nuint8   fileName[1];
   } EQCreate;

   struct eventQJobService
   {
      nuint32  tType;
      nuint8   qname[1];
   } EQJobService;

   struct eventQSetStatus
   {
      nuint32  status;
      nuint8   qname[1];
   } EQSetStatus;

   struct eventStationRestrictions
   {
      nuint8   name[1];
      nuint8   netAddress[1];
   } EStnRestrictions;

   struct eventTrustee
   {
      nuint32  trusteeID;
      nuint32  rights;
      nuint32  nameSpace;
      nuint8   trusteeName[1];
      nuint8   fileName[1];
   } ETrustee;

   struct eventTrusteeSpace
   {
      nuint32  spaceValue;
      nuint8   trusteeName[1];
   } ETSpace;

   struct auditingNLMAddRecord
   {
      nuint32  recordTypeID;
      nuint32  dataLen;
      nuint8   userName[1];
      nuint8   data[1];
   } ENLMRecord;
};

/* The following is used for Directory Services Auditing Events */
typedef struct tagNWContAuditRecord
{
   nuint16  replicaNumber;
   nuint16  eventTypeID;
   nuint32  recordNumber;
   nuint32  dosDateTime;
   nuint32  userID;
   nuint32  processUniqueID;
   nuint32  successFailureStatusCode;
/* nuint8   extra[0];   start of 'union EventUnion'  */
}NWContAuditRecord, N_FAR *pNWContAuditRecord;

/* Audit event bit definitions for Container Auditing */
#define ADS_BIT_ADD_ENTRY                    1  /* first bit no. is 1 */
#define ADS_BIT_REMOVE_ENTRY                 2
#define ADS_BIT_RENAME_OBJECT                3
#define ADS_BIT_MOVE_ENTRY                   4
#define ADS_BIT_CHANGE_SECURITY_EQUIV        5
#define ADS_BIT_CHG_SECURITY_ALSO_EQUAL      6
#define ADS_BIT_CHANGE_ACL                   7
#define ADS_BIT_CHG_STATION_RESTRICTION      8
#define ADS_BIT_LOGIN                        9
#define ADS_BIT_LOGOUT                       10
#define ADS_BIT_CHANGE_PASSWORD              11
#define ADS_BIT_USER_LOCKED                  12
#define ADS_BIT_USER_UNLOCKED                13
#define ADS_BIT_USER_DISABLE                 14
#define ADS_BIT_USER_ENABLE                  15
#define ADS_BIT_CHANGE_INTRUDER_DETECT       16

#define ADS_BIT_ADD_PARTITION                17
#define ADS_BIT_REMOVE_PARTITION             18
#define ADS_BIT_ADD_REPLICA                  19
#define ADS_BIT_REMOVE_REPLICA               20

#define ADS_BIT_SPLIT_PARTITION              21
#define ADS_BIT_JOIN_PARTITIONS              22
#define ADS_BIT_CHANGE_REPLICA_TYPE          23
#define ADS_BIT_REPAIR_TIME_STAMPS           24
#define ADS_BIT_MOVE_SUB_TREE                25
#define ADS_BIT_ABORT_PARTITION_OP           26
#define ADS_BIT_SEND_REPLICA_UPDATES         27
#define ADS_BIT_RECEIVE_REPLICA_UPDATES      28

/* Added for Trusted NetWare auditing  */
#define ADS_BIT_ADD_MEMBER                   29
#define ADS_BIT_BACKUP_ENTRY                 30
#define ADS_BIT_CHANGE_BIND_OBJ_SECUR        31
#define ADS_BIT_CHANGE_PROP_SECURITY         32
#define ADS_BIT_CHANGE_TREE_NAME             33
#define ADS_BIT_CHECK_CONSOLE_OPERATOR       34
#define ADS_BIT_COMPARE_ATTR_VALUE           35
#define ADS_BIT_CREATE_PROPERTY              36
#define ADS_BIT_CREATE_SUBORDINATE_REF       37
#define ADS_BIT_DEFINE_ATTR_DEF              38
#define ADS_BIT_DEFINE_CLASS_DEF             39
#define ADS_BIT_DELETE_MEMBER                40
#define ADS_BIT_DELETE_PROPERTY              41
#define ADS_BIT_DS_NCP_RELOAD                42
#define ADS_BIT_RESET_DS_COUNTERS            43
#define ADS_BIT_FRAG_REQUEST                 44
#define ADS_BIT_INSPECT_ENTRY                45
#define ADS_BIT_LIST_CONTAINABLE_CLASS       46
#define ADS_BIT_LIST_PARTITIONS              47
#define ADS_BIT_LIST_SUBORDINATES            48
#define ADS_BIT_MERGE_TREE                   49
#define ADS_BIT_MODIFY_CLASS_DEF             50
#define ADS_BIT_MOVE_TREE                    51
#define ADS_BIT_OPEN_STREAM                  52
#define ADS_BIT_READ                         53
#define ADS_BIT_READ_REFERENCES              54
#define ADS_BIT_REMOVE_ATTR_DEF              55
#define ADS_BIT_REMOVE_CLASS_DEF             56
#define ADS_BIT_REMOVE_ENTRY_DIR             57
#define ADS_BIT_RESTORE_ENTRY                58
#define ADS_BIT_START_JOIN                   59
#define ADS_BIT_START_UPDATE_REPLICA         60
#define ADS_BIT_START_UPDATE_SCHEMA          61
#define ADS_BIT_SYNC_PARTITION               62
#define ADS_BIT_SYNC_SCHEMA                  63
#define ADS_BIT_UPDATE_REPLICA               64
#define ADS_BIT_UPDATE_SCHEMA                65
#define ADS_BIT_VERIFY_PASSWORD              66
#define ADS_BIT_ABORT_JOIN                   67
#define ADS_BIT_RESEND_ENTRY                 68
#define ADS_BIT_MUTATE_ENTRY                 69
#define ADS_BIT_MERGE_ENTRIES                70
#define ADS_BIT_END_UPDATE_REPLICA           71
#define ADS_BIT_END_UPDATE_SCHEMA            72
#define ADS_BIT_CREATE_BACKLINK              73
#define ADS_BIT_MODIFY_ENTRY                 74
#define ADS_BIT_REMOVE_BACKLINK              75
#define ADS_BIT_NEW_SCHEMA_EPOCH             76
#define ADS_BIT_CLOSE_BINDERY                77
#define ADS_BIT_OPEN_BINDERY                 78
#define ADS_BIT_NLM_FIRST                    89
#define ADS_BIT_NLS_NLM                      89
#define ADS_BIT_NLM_LAST                     99


/* Audit Event ID for Container Audit Events */

#define ADS_ADD_ENTRY                  101 /* unsigned long newEntryID */
#define ADS_REMOVE_ENTRY               102 /* unsigned long oldEntryID */
#define ADS_RENAME_OBJECT              103 /* unsigned long renamedEntryID, char *oldRDN */
#define ADS_MOVE_ENTRY                 104 /* unsigned long movedEntryID, char *oldDN */
#define ADS_CHANGE_SECURITY_EQUIV      105
#define ADS_CHG_SECURITY_ALSO_EQUAL    106
#define ADS_CHANGE_ACL                 107
#define ADS_CHG_STATION_RESTRICTION    108
#define ADS_LOGIN                      109 /* unsigned long entryID */
#define ADS_LOGOUT                     110
#define ADS_CHANGE_PASSWORD            111 /* unsigned long entryID */
#define ADS_USER_LOCKED                112 /* unsigned long entryID */
#define ADS_USER_UNLOCKED              113 /* unsigned long entryID */
#define ADS_USER_DISABLE               114 /* unsigned long entryID */
#define ADS_USER_ENABLE                115 /* unsigned long entryID */
#define ADS_CHANGE_INTRUDER_DETECT     116
#define ADS_ADD_PARTITION              117
#define ADS_REMOVE_PARTITION           118
#define ADS_ADD_REPLICA                119
#define ADS_REMOVE_REPLICA             120
#define ADS_SPLIT_PARTITION            121
#define ADS_JOIN_PARTITIONS            122
#define ADS_CHANGE_REPLICA_TYPE        123
#define ADS_REPAIR_TIME_STAMPS         124
#define ADS_MOVE_SUB_TREE              125
#define ADS_ABORT_PARTITION_OP         126
#define ADS_SEND_REPLICA_UPDATES       127
#define ADS_RECEIVE_REPLICA_UPDATES    128

/* start Trusted NetWare auditing events */
#define ADS_ADD_MEMBER                 129
#define ADS_BACKUP_ENTRY               130
#define ADS_CHANGE_BIND_OBJ_SECURITY   131
#define ADS_CHANGE_PROP_SECURITY       132
#define ADS_CHANGE_TREE_NAME           133
#define ADS_CHECK_CONSOLE_OPERATOR     134
#define ADS_COMPARE_ATTR_VALUE         135
#define ADS_CREATE_PROPERTY            136
#define ADS_CREATE_SUBORDINATE_REF     137
#define ADS_DEFINE_ATTR_DEF            138
#define ADS_DEFINE_CLASS_DEF           139
#define ADS_DELETE_MEMBER              140
#define ADS_DELETE_PROPERTY            141
#define ADS_DS_NCP_RELOAD              142
#define ADS_RESET_DS_COUNTERS          143
#define ADS_FRAG_REQUEST               144
#define ADS_INSPECT_ENTRY              145
#define ADS_LIST_CONTAINABLE_CLASSES   146
#define ADS_LIST_PARTITIONS            147
#define ADS_LIST_SUBORDINATES          148
#define ADS_MERGE_TREE                 149
#define ADS_MODIFY_CLASS_DEF           150
#define ADS_MOVE_TREE                  151
#define ADS_OPEN_STREAM                152
#define ADS_READ                       153
#define ADS_READ_REFERENCES            154
#define ADS_REMOVE_ATTR_DEF            155
#define ADS_REMOVE_CLASS_DEF           156
#define ADS_REMOVE_ENTRY_DIR           157
#define ADS_RESTORE_ENTRY              158
#define ADS_START_JOIN                 159
#define ADS_START_UPDATE_REPLICA       160
#define ADS_START_UPDATE_SCHEMA        161
#define ADS_SYNC_PARTITION             162
#define ADS_SYNC_SCHEMA                163
#define ADS_UPDATE_REPLICA             164
#define ADS_UPDATE_SCHEMA              165
#define ADS_VERIFY_PASSWORD            166
#define ADS_ABORT_JOIN                 167
#define ADS_MUTATE_ENTRY               169
#define ADS_MERGE_ENTRIES              170

#define ADS_END_UPDATE_REPLICA         171
#define ADS_END_UPDATE_SCHEMA          172
#define ADS_CREATE_BACKLINK            173
#define ADS_MODIFY_ENTRY               174
#define ADS_REMOVE_BACKLINK            175
#define ADS_NEW_SCHEMA_EPOCH           176
#define ADS_CLOSE_BINDERY              177
#define ADS_OPEN_BINDERY               178
#define ADS_CLOSE_STREAM               179
/*
   180 - 188 reserved
*/
#define ADS_NLM_FIRST                  189
#define ADS_NLS_NLM                    189
#define ADS_NLM_LAST                   199
#define ADS_LAST_PLUS_ONE              200 /* Must be last one */
/* end TNW additions */


#include "npackoff.h"
#endif   /* NWADEVNT_H */
