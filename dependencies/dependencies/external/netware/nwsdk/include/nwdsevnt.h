/******************************************************************************
  Source module name:  nwdsevnt.h
  Release Version:

  %name: nwdsevnt.h %
  %version: 10 %
  %date_modified: Wed Aug 25 09:48:37 1999 %
  $Copyright:

  Copyright (c) 1989-1998 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#ifndef  _NWDSEVNT_HEADER_
#define  _NWDSEVNT_HEADER_

/*---------------------------------------------------------------------------
 * definitions and functions required to receive event reports
 */
typedef enum DSEventPriority {EP_INLINE, EP_JOURNAL, EP_WORK} DSEventPriority;

/*---------------------------------------------------------------------------
 * event types
 */

#define DSE_INLINE_ONLY             0x80000000
#define DSE_EVENT_MASK              0x7FFFFFFF

/* First 19 events in 4.10 release; events 20-25 added to IW2 */
#define DSE_INVALID                    0
#define DSE_CREATE_ENTRY               1  /* data is DSEEntryInfo */
#define DSE_DELETE_ENTRY               2  /* data is DSEEntryInfo */
#define DSE_RENAME_ENTRY               3  /* data is DSEEntryInfo */
#define DSE_MOVE_SOURCE_ENTRY          4  /* data is DSEEntryInfo */
#define DSE_ADD_VALUE                  5  /* data is DSEValueInfo */
#define DSE_DELETE_VALUE               6  /* data is DSEValueInfo */
#define DSE_CLOSE_STREAM               7  /* data is DSEValueInfo */
#define DSE_DELETE_ATTRIBUTE           8  /* data is DSEValueInfo */
#define DSE_SET_BINDERY_CONTEXT        9  /* no data */
#define DSE_CREATE_BINDERY_OBJECT      10 /* data is DSEBinderyObjectInfo */
#define DSE_DELETE_BINDERY_OBJECT      11 /* data is DSEBinderyObjectInfo */
#define DSE_CHECK_SEV                  12 /* data is DSESEVInfo */
#define DSE_UPDATE_SEV                 13 /* no data */
#define DSE_MOVE_DEST_ENTRY            14 /* data is DSEEntryInfo */
#define DSE_DELETE_UNUSED_EXTREF       15 /* data is DSEEntryInfo */
#define DSE_REMOTE_SERVER_DOWN         17 /* data is DSENetAddress */
#define DSE_NCP_RETRY_EXPENDED         18 /* data is DSENetAddress */
#define DSE_REMOTE_CONN_CLEARED        19 /* data is DSENetAddress */
#define DSE_PARTITION_OPERATION_EVENT  20 /* data is DSEEventData; used by SCALE */
#define DSE_CHANGE_MODULE_STATE        21 /* data is DSEModuleState */
#define DSE_RESERVED_2                 22 /* reserved */
#define DSE_RESERVED_3                 23 /* reserved */
#define DSE_RESERVED_4                 24 /* reserved */
#define DSE_RESERVED_5                 25 /* reserved */

/* Events added post 4.10 */
   /* All the DSE_DB_... events are "debug" trace events, each event
    * corresponds to one old DSTrace type TV_..., most have many messages
    * using the same event ID. No attempt has been made to make these calls
    * outside the NDS locks, so you must presume that all these calls come
    * inside the locks.  These all use the DSEDebugInfo data structure,
    * and the result (error) field is always 0 for these events
   */
#define DSE_DB_AUTHEN                  26 /* authentication */
#define DSE_DB_BACKLINK                27 /* backlink */
#define DSE_DB_BUFFERS                 28 /* request buffer display */
#define DSE_DB_COLL                    29 /* collisions */
#define DSE_DB_DSAGENT                 30 /* low level DSA tracing */
#define DSE_DB_EMU                     31 /* bindery emulator */
#define DSE_DB_FRAGGER                 32 /* fragger */
#define DSE_DB_INIT                    33 /* initialization */
#define DSE_DB_INSPECTOR               34 /* inspector */
#define DSE_DB_JANITOR                 35 /* janitor */
#define DSE_DB_LIMBER                  36 /* limber */
#define DSE_DB_LOCKING                 37 /* locking */
#define DSE_DB_MOVE                    38 /* move */
#define DSE_DB_MIN                     39 /* default dstrace messages (equivalent to ON) */
#define DSE_DB_MISC                    40 /* miscellaneous */
#define DSE_DB_PART                    41 /* partition operations */
#define DSE_DB_RECMAN                  42 /* record manager */
#define DSE_DB_RESNAME                 44 /* resolve name */
#define DSE_DB_SAP                     45 /* SAP */
#define DSE_DB_SCHEMA                  46 /* schema */
#define DSE_DB_SKULKER                 47 /* skulker */
#define DSE_DB_STREAMS                 48 /* streams */
#define DSE_DB_SYNC_IN                 49 /* incoming sync traffic */
#define DSE_DB_THREADS                 50 /* DS thread scheduling */
#define DSE_DB_TIMEVECTOR              51 /* time vectors */
#define DSE_DB_VCLIENT                 52 /* virtual client */

   /* Nearly all the following events use the same DSEEventData structure.
    * Not all fields are filled in for each event.  The data in the
    * structure is shown for each event.  Any unused fields are set to -1
    * for the ids or values, or set to 0 for the pointers.
    */
#define DSE_AGENT_OPEN_LOCAL           53 /* d1-state(2-Audit, 1-start, 0-end), result-valid for end state only, not in locks */
#define DSE_AGENT_CLOSE_LOCAL          54 /* d1-state(1-start, 0-end), not in locks */
#define DSE_DS_ERR_VIA_BINDERY         55 /* d1-error code being returned via the bindery, not in locks */
#define DSE_DSA_BAD_VERB               56 /* d1-bad verb number given to DSA Request (NCP 104, 2), not in locks */
#define DSE_DSA_REQUEST_START          57 /* d1-verb number (NCP 104, 2), not in locks */
#define DSE_DSA_REQUEST_END            58 /* d1-verb number, d2-primaryID, d3-request size, d4-reply size, not in locks */
#define DSE_MOVE_SUBTREE               59 /* d1-source ID, d1-Dest ID, not in locks */
#define DSE_NO_REPLICA_PTR             60 /* d1-partitionID of partition, inside locks */
#define DSE_SYNC_IN_END                61 /* d1-ID of server sending changes, d2 - partition Root ID, p3-Number entries sent(if no error), not in locks */
#define DSE_BKLINK_SEV                 62 /* d1-ID of object being updated, not in locks */
#define DSE_BKLINK_OPERATOR            63 /* d1-ID of object whose console operator privilegs were changes, d2-ID of server privileges changed on, not in locks */
#define DSE_DELETE_SUBTREE             64 /* d1-ID of subtree root, d2-count of objects deleted, inside locks & transaction */
#define DSE_SET_NEW_MASTER             65 /* d1-ID of partition being changed, not in locks */
#define DSE_PART_STATE_CHG_REQ         66 /* d1-ID of partition, d2-partnerPartID, d3-(function<<16)|type, d4-state */
#define DSE_REFERRAL                   67 /* d1-ID of local entry, d2-ID of partition, d3-referral type */
#define DSE_UPDATE_CLASS_DEF           68 /* uname-name of schema class updated (added), inside locks & transaction */
#define DSE_UPDATE_ATTR_DEF            69 /* uname-name of schema attribute updated (added), inside locks & transaction */
#define DSE_LOST_ENTRY                 70 /* d1-parent ID of entry, d2-timestamp.seconds of entry, d3-timestamp.replicaNumber, d4-timestamp.event, uname-unicode name of entry, inside locks & transaction */
#define DSE_PURGE_ENTRY_FAIL           71 /* d1-ID of entry that failed, inside lock & transaction */
#define DSE_PURGE_START                72 /* d1-ID of partition being purged, d2-replica type, inside lock */
#define DSE_PURGE_END                  73 /* d1-ID of partition purged, d2-number entries purged, d3-number values purged, not in locks */
#define DSE_FLAT_CLEANER_END           74 /* d1-number entries purged, d2-number values purged, not in locks */
#define DSE_ONE_REPLICA                75 /* d1-ID of partition with only one replica, inside lock */
#define DSE_LIMBER_DONE                76 /* d1-all initialized (boolean value), d2-found new RDN (boolean), not in lock */
#define DSE_SPLIT_DONE                 77 /* d1-ID of parent partition root, d2-ID of child partition root, not in lock */
#define DSE_SYNC_SVR_OUT_START         78 /* d1-ID of server, d2-rootID, d3-replica number, d4-replica state&type&flags, not in locks */
#define DSE_SYNC_SVR_OUT_END           79 /* d1-ID of server, d2-partition rootID, d3-objects sent, d4-values sent, not in locks */
#define DSE_SYNC_PART_START            80 /* d1-partition ID, d2-partition state, d3-replica type, inside lock */
#define DSE_SYNC_PART_END              81 /* d1-partition ID, d2-(boolean value=)AllProcessed, not in lock */
#define DSE_MOVE_TREE_START            82 /* d1-ID of subtree root being moved, d2-destination (parent) ID, d3-server ID starting from, not in lock */
#define DSE_MOVE_TREE_END              83 /* d1-ID of subtree root being moved, d2-server ID starting from, not in lock */
#define DSE_RECERT_PUB_KEY             84 /* d1-ID of entry whose keys are being certified, inside locks & transaction */
#define DSE_GEN_CA_KEYS                85 /* d1-ID of entry having CA Keys generated, inside locks & transaction */
#define DSE_JOIN_DONE                  86 /* d1-ID of parent partition root, d2-ID of child partition root, inside lock */
#define DSE_PARTITION_LOCKED           87 /* d1-ID of partition being locked, not in locks */
#define DSE_PARTITION_UNLOCKED         88 /* d1-ID of partition being unlocked, not in locks */
#define DSE_SCHEMA_SYNC                89 /* d1-(boolean value=)allProcessed, not in locks */
#define DSE_NAME_COLLISION             90 /* d1-ID of original entry, d2-ID of duplicate entry, inside locks & transaction */
#define DSE_NLM_LOADED                 91 /* d1-module handle of NLM that was loaded, not in locks */
#define DSE_PARTITION_EVENT            92 /* ** Uses DSEPartitionData structure */
#define DSE_SKULKER_EVENT              93 /* ** Uses DSESkulkData structure */
#define DSE_LUMBER_DONE                94 /* no parameters, not in lock */
#define DSE_BACKLINK_PROC_DONE         95 /* no parameters, not in lock */
#define DSE_SERVER_RENAME              96 /* name-ascii new server name, inside locks */
#define DSE_SYNTHETIC_TIME             97 /* d1-root entry ID of partition issuing timestamp, d2-partition id, d3-count of timestamps requested, inside locks & transaction */
#define DSE_SERVER_ADDRESS_CHANGE      98 /* no parameters, in locks */
#define DSE_DSA_READ                   99 /* d1-ID of entry being read, not in locks */

   /* The following section of events are primarily for auditing, and thus
    * whenever possible, the event is inside a transaction, so they can
    * return an error and abort the transaction if necessary
    */
#define DSE_LOGIN                      100 /* d1-parent id, d2-entry id, d3-usedNullPassword(boolean), d4-bindery login(0) or NDS login (-1) */
#define DSE_CHGPASS                    101 /* d1-parent id, d2-entry id */
#define DSE_LOGOUT                     102 /* d1-parent id, d2-entry id */
#define DSE_ADD_REPLICA                103 /* d1-partition root id, d2-server ID, d3-replicaType, uname-servername */
#define DSE_REMOVE_REPLICA             104 /* d1-partition root id, d2-server ID, uname-servername */
#define DSE_SPLIT_PARTITION            105 /* d1-parent partition root id, d2-new partition root id, uname-new partition entry name */
#define DSE_JOIN_PARTITIONS            106 /* d1-parent partition root id, d2-child partition root id */
#define DSE_CHANGE_REPLICA_TYPE        107 /* d1-partition root id, d2-target server ID, d3-old type, d4-new type*/
#define DSE_REMOVE_ENTRY               108 /* d1-parent id, d2-entry id, uname-entry name */
#define DSE_ABORT_PARTITION_OP         109 /* d1-parent id, d2-entry id */
#define DSE_RECV_REPLICA_UPDATES       110 /* d1-replica root id */
#define DSE_REPAIR_TIME_STAMPS         111 /* d1-replica root id */
#define DSE_SEND_REPLICA_UPDATES       112 /* d1-replica root id */
#define DSE_VERIFY_PASS                113 /* d1-parent id, d2-entry id */
#define DSE_BACKUP_ENTRY               114 /* d1-entry id */
#define DSE_RESTORE_ENTRY              115 /* d1-parent id, name-entry rdn */
#define DSE_DEFINE_ATTR_DEF            116 /* uname-attribute name*/
#define DSE_REMOVE_ATTR_DEF            117 /* d1-attr id, d2-schema root ID, uname-attribute name */
#define DSE_REMOVE_CLASS_DEF           118 /* d1-class id, d2-schema root ID, uname-class name */
#define DSE_DEFINE_CLASS_DEF           119 /* uname-class name*/
#define DSE_MODIFY_CLASS_DEF           120 /* d1-class id, d2-schema root ID, uname-class name */
#define DSE_RESET_DS_COUNTERS          121 /* d2-server ID */
#define DSE_REMOVE_ENTRY_DIR           122 /* d1-parent id, d2-entry id, uname-entry name*/
#define DSE_COMPARE_ATTR_VALUE         123 /* d1-parent id, d2-entry id, uname-attribute name*/
#define DSE_STREAM                     124 /* d1-DSE_ST_OPEN, d2-entry id, d3-attr id, d4-requested rights */
                                           /* d1-DSE_ST_CLOSE, d2-entry id, d3-attr id */
#define DSE_LIST_SUBORDINATES          125 /* d1-parent id, d2-entry id, uname-entry name */
#define DSE_LIST_CONT_CLASSES          126 /* d1-parent id, d2-entry id, uname-entry name */
#define DSE_INSPECT_ENTRY              127 /* d1-parent id, d2-entry id */
#define DSE_RESEND_ENTRY               128 /* d1-parent id, d2-entry id */
#define DSE_MUTATE_ENTRY               129 /* d1-entry id, d2-new class id, uname-new class name */
#define DSE_MERGE_ENTRIES              130 /* d1-winner parent id, d2-winner entry id, uname-loser entry name*/
#define DSE_MERGE_TREE                 131 /* d1-root entry id */
#define DSE_CREATE_SUBREF              132 /* d1-sub ref id */
#define DSE_LIST_PARTITIONS            133 /* d1-partition root entry id */
#define DSE_READ_ATTR                  134 /* d1-entry id, d2-attribute id */
#define DSE_READ_REFERENCES            135 /* d1-entry id */
#define DSE_UPDATE_REPLICA             136 /* d1-partition root id, d2-entry id, uname-entry name */
#define DSE_START_UPDATE_REPLICA       137 /* d1-partition root id */
#define DSE_END_UPDATE_REPLICA         138 /* d1-parititon root id */
#define DSE_SYNC_PARTITION             139 /* d1-partition root id */
#define DSE_SYNC_SCHEMA                140 /* d1-tree root id */
#define DSE_CREATE_BACKLINK            141 /* d1-tree root id, d2-server ID request came from, d3-local entry ID, d4-remote entry ID */
#define DSE_CHECK_CONSOLE_OPERATOR     142 /* d1-tree root id, d2-server ID, d3-isOperator boolean, d4-object ID being checked */
#define DSE_CHANGE_TREE_NAME           143 /* d1-tree root id, uname-new tree name */
#define DSE_START_JOIN                 144 /* d1-parent partition root id, d2-child partition root id */
#define DSE_ABORT_JOIN                 145 /* d1-parent partition root id, d2-child partition root id */
#define DSE_UPDATE_SCHEMA              146 /* d1-tree root id, d2-server id, inside locks & transaction */
#define DSE_START_UPDATE_SCHEMA        147 /* d1-tree root id, d2-server id */
#define DSE_END_UPDATE_SCHEMA          148 /* d1-tree root id, d2-server id */
#define DSE_MOVE_TREE                  149 /* d1-source parent id, d2-dest. parent id, d3-type (0|1), uname-(0=source DN)|(1=newName) */
#define DSE_RELOAD_DS                  150 /* d1-tree root id */
#define DSE_ADD_PROPERTY               151 /* d1-object id, d3-security, d4-flags, name-object name */
#define DSE_DELETE_PROPERTY            152 /* d1-object id, name-object name */
#define DSE_ADD_MEMBER                 153 /* d1-object id, d3-member id, name-property name */
#define DSE_DELETE_MEMBER              154 /* d1-object id, d3-member id, name-property name */
#define DSE_CHANGE_PROP_SECURITY       155 /* d1-object id, d3-new security, name-property name */
#define DSE_CHANGE_OBJ_SECURITY        156 /* d1-object parent id, d2-object id, d3-new security */
#define DSE_READ_OBJ_INFO              157 /* d1-parent id, d2-entry id */
#define DSE_CONNECT_TO_ADDRESS         158 /* d1-task id, d3-address type, d4-address size, name-address data */
#define DSE_SEARCH                     159 /* d1-base object id, d2-scope, d3-nodes to search (not used currently) d4-infoType */
#define DSE_PARTITION_STATE_CHG        160 /* d1-partitionRootID, d2-partnerPartID, d3-(function<<16)|type, d4-state */
#define DSE_REMOVE_BACKLINK            161 /* d1-object ID affected, d2-serverID of removed backlink, d3-remoteID of removed backlink */
#define DSE_LOW_LEVEL_JOIN             162 /* d1-parent partition Root ID, d2-child partition Root ID, not in lock */
#define DSE_CREATE_NAMEBASE            163 /* no data, not in lock */
#define DSE_CHANGE_SECURITY_EQUALS     164 /* d1-object ID, d2-equiv ID, d3-0=delete,1=add equivalence, inside locks & transaction */
#define DSE_DB_NCPENG                  166
#define DSE_CRC_FAILURE                167 /* d1-CRC failure type (0=server | 1=client) d2-server | client CRC error count */
#define DSE_ADD_ENTRY                  168 /* d1-parent id, d2-object id, (success DSE_DATATYPE_STRUCT1) */
#define DSE_MODIFY_ENTRY               169 /* d1-parent id, d2-object id, (success DSE_DATATYPE_STRUCT1) */
#define DSE_OPEN_BINDERY               171 /* d1-tree root id */
#define DSE_CLOSE_BINDERY              172 /* d1-tree root id */
#define DSE_CHANGE_CONN_STATE          173 /* data is DSEChangeConnState */
#define DSE_NEW_SCHEMA_EPOCH           174 /* d1-tree root id */
#define DSE_DB_AUDIT                   175 /* auditing debug messages */
#define DSE_DB_AUDIT_NCP               176 /* audit ncp debug messages */
#define DSE_DB_AUDIT_SKULK             177 /* audit skulking debug messages */
#define DSE_MODIFY_RDN                 178 /* d1-parentID, d2-entry ID, uname-oldRDN */
#define DSE_DB_LDAP                    179 /* ldap trace messages */
#define DSE_ORPHAN_PARTITION           180 /* d1-DSE_OP_CREATE, d2-newPartitionID, d3-targetPartitionID */
                                           /* d1-DSE_OP_REMOVE, d2-partitionID */
                                           /* d1-DSE_OP_LINK, d2-partitionID, d3-targetPartitionID, d4-targetServerID */
                                           /* d1-DSE_OP_UNLINK, d2-partitionID, d3-targetPartitionID */
#define DSE_ENTRYID_SWAP               181 /* d1-srcID, d2-destID */

#define DSE_DB_NCP_REQUEST             182 /* no data - used by lock check */
#define DSE_DB_LOST_ENTRY              183 /* uses DSEDebugInfo */
#define DSE_DB_CHANGE_CACHE            184 /* uses DSEDebugInfo */
#define DSE_LOW_LEVEL_SPLIT            185 /* d1-parent partition Root ID, d2-child partition Root ID, not in lock */
#define DSE_DB_PURGE                   186 /* uses DSEDebugInfo */
#define DSE_END_NAMEBASE_TRANSACTION   187 /* no data */
#define DSE_ALLOW_LOGIN                188 /* d1-entryID, d2-flags */
#define DSE_DB_CLIENT_BUFFERS          189 /* uses DSEDebugInfo, request buffer display */

   /* The following section of events are primarily for WAN Traffic Manager,
    * and thus are expected to be used as inline events so that the policy
    * results can be returned to DS.
    */

#define DSE_DB_WANMAN                  190 /* uses DSEDebugInfo */
#define DSE_WTM_NDS_BACKLINKS          191 /* DSEWtmInfo */
#define DSE_WTM_NDS_SCHEMA_SYNC        192 /* DSEWtmInfo */
#define DSE_WTM_NDS_LIMBER             193 /* DSEWtmInfo */
#define DSE_WTM_NDS_LOGIN_RESTRICTIONS 194 /* DSEWtmInfo */
#define DSE_WTM_NDS_JANITOR            195 /* DSEWtmInfo */
#define DSE_WTM_NDS_OPEN_CONNECTION    196 /* DSEWtmOpenInfo */

#define DSE_LOCAL_REPLICA_CHANGE       197 /* d1-DSE_LRC_* opcode, d2-replicaRootID */
#define DSE_DB_DRL                     198 /* reference link debug messages */
#define DSE_MOVE_ENTRY_SOURCE          199 /* d1-parentID, d2-dest parentID, d3-sourceID, uname-name */
#define DSE_MOVE_ENTRY_DEST            200 /* d1-parentID, d2-dest parentID, d3-sourceID, uname-newName */
#define DSE_NOTIFY_REF_CHANGE          201 /* d1-entryID used by obituary added to */
#define DSE_DB_ALLOC                   202 /* uses DSEDebugInfo */
#define DSE_CONSOLE_OPERATION          203 /* d1-opCode - DSC_* flag */
#define DSE_DB_SERVER_PACKET           204 /* uses DSEDebugInfo */
#define DSE_START_DIB_CHECK            205 /* RecMan DIB validation */
#define DSE_END_DIB_CHECK              206 /* RecMan DIB validation, d1-error status */
#define DSE_DB_OBIT                    207 /* uses DSEDebugInfo */
#define DSE_REPLICA_IN_TRANSITION      208 /* d1-partition root ID, d2-last ID */
#define DSE_DB_SYNC_DETAIL             209 /* uses DSEDebugInfo */
#define DSE_DB_CONN_TRACE              210 /* uses DSEDebugInfo */
#define DSE_BEGIN_NAMEBASE_TRANSACTION 211 /* no data */
#define DSE_DB_VIRTUAL_REPLICA         212 /* uses DSEDebugInfo */
#define DSE_VR_DRIVER_STATE_CHANGE     213 /* used DSEEntryInfo */
#define DSE_CHANGE_CONFIG_PARM         214 /* uses DSEChangeConfigParm */

#define DSE_MAX_EVENTS                 215

/*---------------------------------------------------------------------------
 * data structures used for the data associated with events
 */

/* DSE_STREAM operation definitions */
#define DSE_ST_OPEN              0x0001
#define DSE_ST_CLOSE             0x0002

/* DSE_ORPHAN_PARTITION operation definitions */
#define DSE_OP_CREATE            0x0001
#define DSE_OP_REMOVE            0x0002
#define DSE_OP_LINK              0x0003
#define DSE_OP_UNLINK            0x0004

typedef struct
{
   nuint32 seconds;
   nuint16 replicaNumber;
   nuint16 event;
} DSETimeStamp;

typedef struct
{
   nuint8 data[16];
} DSEGUID;

/* newDN used for DSE_MOVE_SOURCE_ENTRY and DSE_RENAME_ENTRY,
 * otherwise it is 0
 */

/* NetWare 4.x returns DSEntryInfo structure, NetWare 5.x returns
 * DSEntryInfo2 structure
*/
typedef struct
{
   nuint32 perpetratorID;
   nuint32 verb;
   nuint32 entryID;
   nuint32 parentID;
   nuint32 classID;
   nuint32 flags;
   DSETimeStamp creationTime;
   const unicode *dn;
   const unicode *newDN;
   char data[1];     /* used to store data for dn and newDN fields */
} DSEEntryInfo;

typedef struct
{
   nuint32 perpetratorID;
   nuint32 verb;
   nuint32 entryID;
   nuint32 parentID;
   nuint32 classID;
   nuint32 flags;
   DSETimeStamp creationTime;
   const unicode *dn;
   const unicode *newDN;
   nuint32 connID;
   char data[1];     /* used to store data for dn and newDN fields */
} DSEEntryInfo2;


/* DSEEntryInfo flag definitions */
#define DSEF_PARTITION_ROOT   0x0001
#define DSEF_EXTREF           0x0002
#define DSEF_ALIAS            0x0004

typedef struct
{
   nuint32   perpetratorID;
   nuint32   verb;
   nuint32   entryID;
   nuint32   attrID;
   nuint32   syntaxID;
   nuint32   classID;
   DSETimeStamp timeStamp;
   nuint     size;
   char      data[1];    /* see DSEVal... structures for meanings of this field */
} DSEValueInfo;

typedef struct
{
   nuint32 entryID;
   nuint32 parentID;
   nuint32 type;
   nuint32 emuObjFlags;
   nuint32 security;
   char    name[48];
} DSEBinderyObjectInfo;

typedef struct
{
   nuint32 entryID;
   nuint32 retryCount;
   nuint32 valueID;
   unicode valueDN[MAX_DN_CHARS + 1];
   char referral[1];
} DSESEVInfo;

typedef struct
{
   nuint32   type;
   nuint32   length;
   nuint8    data[1];
} DSENetAddress;

typedef struct
{
   nuint32   entryID;
   nuint32   attrID[2];
} DSESkulkData;

/* DSE_LOCAL_REPLICA_CHANGE flag definitions */
#define DSE_LRC_ADD              0x0001
#define DSE_LRC_REMOVE           0x0002
#define DSE_LRC_MODIFY           0x0003

/* DSE_PARTITION_OPERATION_EVENT flag definitions */
#define DSE_PF_STATE_CHANGE      0x0001
#define DSE_PF_ADD               0x0002
#define DSE_PF_PURGE             0x0004
#define DSE_PF_JOIN              0x0008
#define DSE_PF_SPLIT             0x0010

/* DSE_CHANGE_CONN_STATE flag definitions */
#define DSE_CONN_VALID              0x0001
#define DSE_CONN_AUTHENTIC          0x0002
#define DSE_CONN_SUPERVISOR         0x0004
#define DSE_CONN_OPERATOR           0x0008
#define DSE_CONN_LICENSED           0x0010
#define DSE_CONN_SEV_IS_STALE       0x0020
#define DSE_CONN_IS_NCP             0x0040
#define DSE_CONN_CHECKSUMMING       0x0080
#define DSE_CONN_OPERATIONAL_FLAGS  0x00FF
#define DSE_CONN_SIGNATURES         0x0100
#define DSE_CONN_CSIGNATURES        0x0200
#define DSE_CONN_ENCRYPTION         0x0400
#define DSE_CONN_SECURITY_FLAGS     0x0700

typedef struct
{
   nuint32 connID;
   nuint32 entryID;
   nuint32 oldFlags;
   nuint32 newFlags;
} DSEChangeConnState;

/* DSEChangeConfigState parameter values */
#define DSE_CFG_UNSPECIFIED            0
#define DSE_CFG_RESPOND_TO_GET_NEAREST 1
#define DSE_CFG_SERVER_NAME            2

typedef struct
{
   nuint32 configParm;
} DSEChangeConfigParm;

/* DSEModuleState flags, the CHANGING flag is combined with LOADED
 * to show starting to load or starting to unload.
 */
#define DSE_MOD_CHANGING   0x01
#define DSE_MOD_LOADED     0x02
#define DSE_MOD_AUTOLOAD   0x04
#define DSE_MOD_HIDDEN     0x08
#define DSE_MOD_ENGINE     0x10
#define DSE_MOD_AUTOMATIC  0x20
#define DSE_MOD_DISABLED   0x40
#define DSE_MOD_MANUAL     0x80
#define DSE_MOD_SYSTEM     0x100 /* This is entries with PRELOAD_MODULE */
#define DSE_MOD_WAITING    0x200 /* state change is queued */

#define DSE_MAX_MODULE_NAME_CHARS   32

typedef struct DSEModuleState
{
   nuint32  connID;
   nuint32  flags;
   long     handle;
   unicode  name[DSE_MAX_MODULE_NAME_CHARS];
} DSEModuleState;

/*---------------------------------------------------------------------------
 * data structures used for contents of the data member of the
 * DSEValueInfo structure.
 */

typedef struct
{
   nuint32   nameSpaceType;
   nuint32   volumeEntryID;
   nuint32   length;
   unicode   data[1];
} DSEPath;

typedef struct
{
   nuint32 serverID;
   nuint32 type;
   nuint32 number;
   nuint32 replicaRootID;   /* partition root ID on server where replica resides */
   char    referral[1];     /* transport address data in wire format */
} DSEReplicaPointer;

typedef struct
{
   nuint32 serverID;
   nuint32 remoteID;
} DSEBackLink;

typedef struct
{
   nuint32 entryID;
   nuint32 attrID;
   nuint32 privileges;
} DSEACL;

typedef struct
{
   nuint32 entryID;
   nuint32 level;
   nuint32 interval;
} DSETypedName;

typedef struct
{
   nuint32 entryID;
   nuint32 amount;
} DSEHold;

typedef struct
{
   nuint32  type;        /* SMF70, SMF71, SMTP, X400, SNADS, PROFS */
   nuint32  length;                     /* length of address unicode string */
   unicode  address[1/*or more*/];   /* null terminated unicode string */
} DSEEmailAddress;

typedef struct
{
   nuint32 numOfBits;
   nuint32 numOfBytes;
   char    data;
} DSEBitString;

typedef struct
{
   nuint32  length;         /* length of telephone number unicode string */
   unicode  telephoneNumber[1/*or more*/]; /* null terminated unicode string */
   /*   The following field is dword aligned after unicode NULL:
   DSEBitString   parameters;
   */
} DSEFaxNumber;

typedef struct
{
   nuint32  numOfStrings;   /* number of uint32 length proceeded, null terminated,
                              dword aligned unicode strings to follow */
   nuint32  length1;        /* length of first unicode string */
   unicode  string1[1];     /* 1st unicode string */
} DSECIList;

typedef struct
{
   nuint32 numOfStrings;    /* number of null terminated, dword aligned byte
                              strings to follow */
   nuint8  string1[1];      /* 1st octet string */
} DSEOctetList;

typedef union
{
   /* used for SYN_CE_STRING, SYN_CI_STRING, SYN_PR_STRING,
    * SYN_NU_STRING, and SYN_TEL_NUMBER
    */
   unicode string[1/*or more*/]; /* null terminated unicode string */
   nuint32 num;                   /* used for SYN_INTEGER, SYN_COUNTER, SYN_TIME, SYN_INTERVAL */
   nuint32 entryID;               /* used for SYN_DIST_NAME */
   nuint32 classID;               /* used for SYN_CLASS_NAME */
   nuint8 boolean;                /* used for SYN_BOOLEAN */
   DSENetAddress netAddress;     /* used for SYN_NET_ADDRESS */
   DSEPath path;                 /* used for SYN_PATH */
   DSEReplicaPointer replica;    /* used for SYN_REPLICA_POINTER */
   DSEACL acl;                   /* used for SYN_OBJECT_ACL */
   DSETimeStamp timeStamp;       /* used for SYN_TIMESTAMP */
   DSEBackLink backLink;         /* used for SYN_BACK_LINK */
   DSETypedName typedName;       /* used for SYN_TYPED_NAME */
   DSEHold hold;                 /* used for SYN_HOLD */
   DSEEmailAddress emailAddress; /* used for SYN_EMAIL_ADDRESS */
   DSEFaxNumber faxNumber;       /* used for SYN_FAX_NUMBER */
   DSECIList ciList;             /* used for SYN_CI_LIST, SYN_PO_ADDRESS */
   nuint8 octetString[1];         /* used for SYN_OCTET_STRING, SYN_STREAM */
   DSEOctetList octetList;       /* used for SYN_OCTET_LIST */
} DSEValData;


#define MAX_EVENT_PARMS 32
typedef struct
{
   nuint32   dstime;                 /* time event occurred */
   nuint32   milliseconds;
   nuint32   curThread;              /* thread running when event occurs */
   nuint32   connID;                 /* connection number that caused this event */
   nuint32   perpetratorID;          /* "user" id that owns the connection */
   char      *fmtStr;                /* sprintf type format string describing parms */
   char      *parms[MAX_EVENT_PARMS];/* pseudo stack of parameters for va_list */
   char      data[1];                /* variable size, holds fmtStr & non-integer parm data */
} DSEDebugInfo;

#define DSE_DATATYPE_CHAR        1
#define DSE_DATATYPE_UNICODE     2
#define DSE_DATATYPE_STRUCT_1    3

typedef struct
{
   nuint32 attrID;
   nuint32 flags;
   nint len;
   void *data;
} DSES1data;

typedef struct
{
   nuint32    count;
   DSES1data  value[1];
} DSEStruct1;

typedef struct
{
   nuint32      dstime;        /* time event occurred */
   nuint32      milliseconds;
   nuint32      curProcess;    /* process running when event occurred */
   nuint32      connID;        /* connection number that caused this event */
   nuint32      verb;          /* verb */
   nuint32      perpetratorID; /* "user" id that owns the connection */
   nuint32      d1;            /* remaining fields vary depending on event number */
   nuint32      d2;
   nuint32      d3;
   nuint32      d4;
   nuint32      dataType;
   const void  *dataPtr;
   char         data[1];       /* variable size, holds either name or uname data */
} DSEEventData;

typedef struct /* use for WAN Traffic Manager Events */
{
   nuint32   lastTime;
   nint32    version;
   nint      checkEachNewOpen;
   nint      checkEachAlreadyOpen;
   nuint32   expirationInterval;
   nuint32   nextTime;
   nuint32   result;
   nint      send;
   nuint32   trafficTypeSubClass;
} DSEWtmInfo;

typedef struct /* used for WAN Traffic Manager Open Events */
{
   nint32    version;
   nuint32   expirationInterval;
   nint      connIsAlreadyOpen;
   nuint32   lastAccessTime;
   nuint32   trafficType;
   char      *trafficTypeName;
   nint      send;
   nint      dstAdrType;
   nuint     dstAdrSize;
   void      *dstAdrData;
   char      *localReferral;
   nuint32   lastSyncTime;
   nuint32   trafficTypeSubClass;
} DSEWtmOpenInfo;

 /*
 * End data structures used for contents of the data member of the
 * DSEValueInfo structure.
 *-------------------------------------------------------------------------*/

/* define trace vector category numbers */
#define TV_ON           0x00000001L    /* if set, tracing is enabled */
#define TV_AUDIT        0x00000002L    /* auditing */
#define TV_INIT         0x00000004L    /* initialization */
#define TV_FRAGGER      0x00000008L    /* fragger */
#define TV_MISC         0x00000010L    /* miscellaneous */
#define TV_RESNAME      0x00000020L    /* resolve name */
#define TV_STREAMS      0x00000040L    /* streams */
#define TV_LIMBER       0x00000080L    /* limber */
#define TV_JANITOR      0x00000100L    /* janitor */
#define TV_BACKLINK     0x00000200L    /* backlink */
#define TV_MERGE        0x00000400L    /* merge */
#define TV_SKULKER      0x00000800L    /* skulker */
#define TV_LOCKING      0x00001000L    /* locking */
#define TV_SAP          0x00002000L    /* SAP */
#define TV_SCHEMA       0x00004000L    /* schema */
#define TV_COLL         0x00008000L    /* collisions */
#define TV_INSPECTOR    0x00010000L    /* inspector */
#define TV_ERRORS       0x00020000L    /* errors */
#define TV_PART         0x00040000L    /* partition operations */
#define TV_EMU          0x00080000L    /* bindery emulator */
#define TV_VCLIENT      0x00100000L    /* virtual client */
#define TV_AUTHEN       0x00200000L    /* authentication */
#define TV_RECMAN       0x00400000L    /* record manager */
#define TV_TIMEVECTOR   0x00800000L    /* time vectors */
#define TV_REPAIR       0x01000000L    /* ds_repair */
#define TV_DSAGENT      0x02000000L    /* low level DSA tracing */
#define TV_ERRET        0x04000000L    /* ERRET and ERRTRACE */
#define TV_SYNC_IN      0x08000000L    /* incoming sync traffic */
#define TV_THREADS      0x10000000L    /* DS thread scheduling */
#define TV_MIN          0x20000000L    /* default dstrace messages */
#define TV_CHECK_BIT    0x80000000L    /* all TV_ values must have this bit */
#define TV_ALL          0xBFFFFFFEL    /* All vectors--default for DSTrace */

/*===========================================================================*/

/* Event handler flags - for NWDSERegisterForEventWithResult.
 */
#define HF_ALL          0x0000         /* Invoke handler regardless of event status */
#define HF_SUCCESS_ONLY 0x0004         /* Invoke handler if event is successful */
#define HF_FAIL_ONLY    0x0008         /* Invoke handler if event fails */


#endif

