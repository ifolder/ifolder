#ifndef _NWERRNO_H_
#define _NWERRNO_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1996 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwerrno.h
==============================================================================
*/
#include <nwtypes.h>

/*
** Multi purpose return values.
*/
#define ESUCCESS 0
#define EFAILURE (-1)

/*
** Function return and NetWareErrno values.
*/
#define ERR_TTS_NOT_AVAILABLE                   0x00  /*   0 */
#define ERR_RECORD_NOT_LOCKED                   0x01  /*   1 */
#define ERR_INSUFFICIENT_SPACE                  0x01  /*   1 */
#define ERR_STRING_EXCEEDS_LENGTH               0x01  /*   1 */
#define ERR_TTS_AVAILABLE                       0x01  /*   1 */
#define ERR_NOT_AVAILABLE_PROTECTED             0x64  /* 100 */
#define ERR_NOT_AVAILABLE_ON_3X                 0x65  /* 101 */
#define ERR_BAD_THREAD_ID                       0x66  /* 102 */
#define ERR_BAD_PRTY_CLASS                      0x67  /* 103 */
#define ERR_BAD_PRTY_SCOPE                      0x68  /* 104 */
#define ERR_NOT_A_POPUP_SCREEN                  0x69  /* 105 */
#define ERR_OPEN_SCREEN                         0x6A  /* 106 */
#define ERR_BAD_SHFLAG                          0x6B  /* 107 */
#define ERR_BAD_ACCESS                          0x6C  /* 108 */
#define ERR_BAD_ORIGIN                          0x6D  /* 109 */
#define ERR_BAD_ACTION_CODE                     0x6E  /* 110 */
#define ERR_OUT_OF_TASKS                        0x6F  /* 111 */
#define ERR_BAD_QUERY_TYPE                      0x70  /* 112 */
#define ERR_BAD_LIBRARY_HANDLE                  0x71  /* 113 */
#define ERR_STREAMS                             0x72  /* 114 */
#define ERR_BAD_FILE_SERVER_ID                  0x73  /* 115 */
#define ERR_BAD_CONNECTION_ID                   0x73  /* 115 */
#define ERR_BAD_FLAGS                           0x74  /* 116 */
#define ERR_STRUCT_NOT_FOUND                    0xC8  /* 200 */
#define ERR_NO_ITEMS_FOUND                      0x79  /* 121 */
#define ERR_NCPEXT_TRANSPORT_PROTOCOL_VIOLATION 0x7E  /* 126 */
#define ERR_FILE_IN_USE                         0x80  /* 128 */
#define ERR_LOCK_FAIL                           0x80  /* 128 */
#define ERR_MAPPED_TO_A_LOCAL_DRIVE             0x80  /* 128 */
#define ERR_NO_MORE_FILE_HANDLES                0x81  /* 129 */
#define ERR_NO_OPEN_PRIVILEGE                   0x82  /* 130 */
#define ERR_NETWORK_DISK_IO                     0x83  /* 131 */
#define ERR_NO_CREATE_PRIVILEGE                 0x84  /* 132 */
#define ERR_NO_CREATE_DELETE_PRIVILEGE          0x85  /* 133 */
#define ERR_R_O_CREATE_FILE                     0x86  /* 134 */
#define ERR_CREATE_FILE_INVALID_NAME            0x87  /* 135 */
#define ERR_INVALID_FILE_HANDLE                 0x88  /* 136 */
#define ERR_NO_SEARCH_PRIVILEGE                 0x89  /* 137 */
#define ERR_NO_DELETE_PRIVILEGE                 0x8A  /* 138 */
#define ERR_NO_RENAME_PRIVILEGE                 0x8B  /* 139 */
#define ERR_NO_MODIFY_PRIVILEGE                 0x8C  /* 140 */
#define ERR_NO_SET_PRIVILEGE                    0x8C  /* 140 */
#define ERR_SOME_FILES_IN_USE                   0x8D  /* 141 */
#define ERR_ALL_FILES_IN_USE                    0x8E  /* 142 */
#define ERR_SOME_READ_ONLY                      0x8F  /* 143 */
#define ERR_ALL_READ_ONLY                       0x90  /* 144 */
#define ERR_SOME_NAMES_EXIST                    0x91  /* 145 */
#define ERR_ALL_NAMES_EXIST                     0x92  /* 146 */
#define ERR_NO_READ_PRIVILEGE                   0x93  /* 147 */
#define ERR_NO_WRITE_PRIVILEGE_OR_READONLY      0x94  /* 148 */
#define ERR_FILE_DETACHED                       0x95  /* 149 */
#define ERR_NO_ALLOC_SPACE                      0x96  /* 150 */
#define ERR_SERVER_OUT_OF_MEMORY                0x96  /* 150 */
#define ERR_TARGET_NOT_A_SUBDIRECTORY           0x96  /* 150 */
#define ERR_NO_SPOOL_SPACE                      0x97  /* 151 */
#define ERR_INVALID_VOLUME                      0x98  /* 152 */
#define ERR_VOLUME_DOES_NOT_EXIST               0x98  /* 152 */
#define ERR_DIRECTORY_FULL                      0x99  /* 153 */
#define ERR_RENAME_ACROSS_VOLUME                0x9A  /* 154 */
#define ERR_BAD_DIR_HANDLE                      0x9B  /* 155 */
#define ERR_HOLE_FOUND                          0x9C  /* 156 */
#define ERR_INVALID_PATH                        0x9C  /* 156 */
#define ERR_NO_SUCH_EXTENSION                   0x9C  /* 156 */
#define ERR_NO_DIR_HANDLES                      0x9D  /* 157 */
#define ERR_BAD_FILE_NAME                       0x9E  /* 158 */
#define ERR_DIRECTORY_ACTIVE                    0x9F  /* 159 */
#define ERR_DIRECTORY_IN_USE                    0x9F  /* 159 */
#define ERR_DIRECTORY_NOT_EMPTY                 0xA0  /* 160 */
#define ERR_DIRECTORY_IO_ERROR                  0xA1  /* 161 */
#define ERR_IO_LOCKED                           0xA2  /* 162 */
#define ERR_TRANSACTION_RESTARTED               0xA3  /* 163 */
#define ERR_RENAME_DIR_INVALID                  0xA4  /* 164 */
#define ERR_INVALID_OPENCREATE_MODE             0xA5  /* 165 */
#define ERR_ALREADY_IN_USE                      0xA6  /* 166 */
#define ERR_SEARCH_DRIVE_VECTOR_FULL            0xB0  /* 176 */
#define ERR_DRIVE_DOES_NOT_EXIST                0xB1  /* 177 */
#define ERR_DRIVE_IS_NOT_MAPPED                 0xB1  /* 177 */
#define ERR_CANT_MAP_LOCAL_DRIVE                0xB2  /* 178 */
#define ERR_INVALID_MAP_TYPE                    0xB3  /* 179 */
#define ERR_INVALID_DRIVE_LETTER                0xB4  /* 180 */
#define ERR_NO_DRIVE_AVAILABLE                  0xB5  /* 181 */
#define ERR_WORKSTATION_OUT_OF_MEMORY           0xB6  /* 182 */
#define ERR_NO_SUCH_SEARCH_DRIVE                0xB7  /* 183 */
#define ERR_INVALID_ENVIRON_VARIABLE            0xB8  /* 184 */
#define ERR_DOES_NOT_RUN_ON_IOENGINE            0xB9  /* 185 */
#define ERR_PACKET_SIGNATURES_REQURIED          0xBC  /* 188 */
#define ERR_PACKET_SIGNATURES_REQUIRED          0xBC  /* 188 */
#define ERR_INVALID_DATA_STREAM                 0xBE  /* 190 */
#define ERR_INVALID_NAME_SPACE                  0xBF  /* 191 */
#define ERR_NO_ACCOUNT_PRIVILEGES               0xC0  /* 192 */
#define ERR_NO_ACCOUNTING_PRIVILEGES            0xC0  /* 192 */
#define ERR_NO_ACCOUNT_BALANCE                  0xC1  /* 193 */
#define ERR_CREDIT_LIMIT_EXCEEDED               0xC2  /* 194 */
#define ERR_LOGIN_DENIED_NO_CREDIT              0xC2  /* 194 */
#define ERR_TOO_MANY_HOLDS                      0xC3  /* 195 */
#define ERR_ACCOUNTING_DISABLED                 0xC4  /* 196 */
#define ERR_LOGIN_LOCKOUT                       0xC5  /* 197 */
#define ERR_NO_CONSOLE_OPERATOR_RIGHTS          0xC6  /* 198 */
#define ERR_MISSING_EA_KEY                      0xC8  /* 200 */
#define ERR_EA_NOT_FOUND                        0xC9  /* 201 */
#define ERR_INVALID_EA_HANDLE_TYPE              0xCA  /* 202 */
#define ERR_EA_NO_KEY_NO_DATA                   0xCB  /* 203 */
#define ERR_EA_NUMBER_MISMATCH                  0xCC  /* 204 */
#define ERR_EXTENT_NUMBER_OUT_OF_RANGE          0xCD  /* 205 */
#define ERR_EA_BAD_DIR_NUM                      0xCE  /* 206 */
#define ERR_INVALID_EA_HANDLE                   0xCF  /* 207 */
#define ERR_EA_POSITION_OUT_OF_RANGE            0xD0  /* 208 */
#define ERR_Q_IO_FAILURE                        0xD0  /* 208 */
#define ERR_EA_ACCESS_DENIED                    0xD1  /* 209 */
#define ERR_NO_QUEUE                            0xD1  /* 209 */
#define ERR_DATA_PAGE_ODD_SIZE                  0xD2  /* 210 */
#define ERR_NO_Q_SERVER                         0xD2  /* 210 */
#define ERR_EA_VOLUME_NOT_MOUNTED               0xD3  /* 211 */
#define ERR_NO_Q_RIGHTS                         0xD3  /* 211 */
#define ERR_BAD_PAGE_BOUNDARY                   0xD4  /* 212 */
#define ERR_Q_FULL                              0xD4  /* 212 */
#define ERR_INSPECT_FAILURE                     0xD5  /* 213 */
#define ERR_NO_Q_JOB                            0xD5  /* 213 */
#define ERR_EA_ALREADY_CLAIMED                  0xD6  /* 214 */
#define ERR_NO_Q_JOB_RIGHTS                     0xD6  /* 214 */
#define ERR_UNENCRYPTED_NOT_ALLOWED             0xD6  /* 214 */
#define ERR_ODD_BUFFER_SIZE                     0xD7  /* 215 */
#define ERR_DUPLICATE_PASSWORD                  0xD7  /* 215 */
#define ERR_Q_IN_SERVICE                        0xD7  /* 215 */
#define ERR_NO_SCORECARDS                       0xD8  /* 216 */
#define ERR_PASSWORD_TOO_SHORT                  0xD8  /* 216 */
#define ERR_Q_NOT_ACTIVE                        0xD8  /* 216 */
#define ERR_BAD_EDS_SIGNATURE                   0xD9  /* 217 */
#define ERR_MAXIMUM_LOGINS_EXCEEDED             0xD9  /* 217 */
#define ERR_LOGIN_DENIED_NO_CONNECTION          0xD9  /* 217 */
#define ERR_Q_STN_NOT_SERVER                    0xD9  /* 217 */
#define ERR_EA_SPACE_LIMIT                      0xDA  /* 218 */
#define ERR_BAD_LOGIN_TIME                      0xDA  /* 218 */
#define ERR_Q_HALTED                            0xDA  /* 218 */
#define ERR_EA_KEY_CORRUPT                      0xDB  /* 219 */
#define ERR_NODE_ADDRESS_VIOLATION              0xDB  /* 219 */
#define ERR_Q_MAX_SERVERS                       0xDB  /* 219 */
#define ERR_EA_KEY_LIMIT                        0xDC  /* 220 */
#define ERR_LOG_ACCOUNT_EXPIRED                 0xDC  /* 220 */
#define ERR_TALLY_CORRUPT                       0xDD  /* 221 */
#define ERR_BAD_PASSWORD                        0xDE  /* 222 */
#define ERR_PASSWORD_EXPIRED_NO_GRACE           0xDE  /* 222 */
#define ERR_PASSWORD_EXPIRED                    0xDF  /* 223 */
#define ERR_NOT_ITEM_PROPERTY                   0xE8  /* 232 */
#define ERR_WRITE_TO_GROUP_PROPERTY             0xE8  /* 232 */
#define ERR_MEMBER_ALREADY_EXISTS               0xE9  /* 233 */
#define ERR_NO_SUCH_MEMBER                      0xEA  /* 234 */
#define ERR_PROPERTY_NOT_GROUP                  0xEB  /* 235 */
#define ERR_NOT_GROUP_PROPERTY                  0xEB  /* 235 */
#define ERR_NO_SUCH_SEGMENT                     0xEC  /* 236 */
#define ERR_NO_SUCH_VALUE_SET                   0xEC  /* 236 */
#define ERR_SPX_CONNECTION_TERMINATED           0xEC  /* 236 */
#define ERR_TERMINATED_BY_REMOTE_PARTNER        0xEC  /* 236 */
#define ERR_PROPERTY_ALREADY_EXISTS             0xED  /* 237 */
#define ERR_SPX_CONNECTION_FAILED               0xED  /* 237 */
#define ERR_SPX_TERMINATED_POORLY               0xED  /* 237 */
#define ERR_SPX_NO_ANSWER_FROM_TARGET           0xED  /* 237 */
#define ERR_OBJECT_ALREADY_EXISTS               0xEE  /* 238 */
#define ERR_SPX_INVALID_CONNECTION              0xEE  /* 238 */
#define ERR_INVALID_NAME                        0xEF  /* 239 */
#define ERR_SPX_CONNECTION_TABLE_FULL           0xEF  /* 239 */
#define ERR_IPX_NOT_INSTALLED                   0xF0  /* 240 */
#define ERR_ILLEGAL_WILDCARD                    0xF0  /* 240 */
#define ERR_WILDCARD_NOT_ALLOWED                0xF0  /* 240 */
#define ERR_SOCKET_NOT_OPEN                     0xF0  /* 240 */
#define ERR_BINDERY_SECURITY                    0xF1  /* 241 */
#define ERR_INVALID_BINDERY_SECURITY            0xF1  /* 241 */
#define ERR_SOCKET_ALREADY_OPEN                 0xF1  /* 241 */
#define ERR_NO_OBJECT_READ_PRIVILEGE            0xF2  /* 242 */
#define ERR_NO_OBJECT_READ_RIGHTS               0xF2  /* 242 */
#define ERR_NO_OBJECT_RENAME_PRIVILEGE          0xF3  /* 243 */
#define ERR_NO_OBJECT_RENAME_RIGHTS             0xF3  /* 243 */
#define ERR_NO_OBJECT_DELETE_PRIVILEGE          0xF4  /* 244 */
#define ERR_NO_OBJECT_DELETE_RIGHTS             0xF4  /* 244 */
#define ERR_NO_OBJECT_CREATE_PRIVILEGE          0xF5  /* 245 */
#define ERR_NO_OBJECT_CREATE_RIGHTS             0xF5  /* 245 */
#define ERR_NO_PROPERTY_DELETE_PRIVILEGE        0xF6  /* 246 */
#define ERR_NO_PROPERTY_DELETE_RIGHTS           0xF6  /* 246 */
#define ERR_NO_PROPERTY_CREATE_PRIVILEGE        0xF7  /* 247 */
#define ERR_NO_PROPERTY_CREATE_RIGHTS           0xF7  /* 247 */
#define ERR_ALREADY_ATTACHED_TO_SERVER          0xF8  /* 248 */
#define ERR_NO_PROPERTY_WRITE_PRIVILEGE         0xF8  /* 248 */
#define ERR_NO_PROPERTY_WRITE_RIGHTS            0xF8  /* 248 */
#define ERR_NOT_ATTACHED_TO_SERVER              0xF8  /* 248 */
#define ERR_ECB_CANNOT_BE_CANCELLED             0xF9  /* 249 */
#define ERR_NO_FREE_CONNECTION_SLOTS            0xF9  /* 249 */
#define ERR_NO_PROPERTY_READ_PRIVILEGE          0xF9  /* 249 */
#define ERR_NO_PROPERTY_READ_RIGHTS             0xF9  /* 249 */
#define ERR_NO_LOCAL_TARGET_IDENTIFIED          0xFA  /* 250 */
#define ERR_NO_MORE_SERVER_SLOTS                0xFA  /* 250 */
#define ERR_TEMP_REMAP                          0xFA  /* 250 */
#define ERR_NO_KNOWN_ROUTE_TO_DESTINATION       0xFA  /* 250 */
#define ERR_INVALID_PARAMETERS                  0xFB  /* 251 */
#define ERR_NO_SUCH_PROPERTY                    0xFB  /* 251 */
#define ERR_UNKNOWN_REQUEST                     0xFB  /* 251 */
#define ERR_EVENT_CANCELLED                     0xFC  /* 252 */
#define ERR_INTERNET_PACKET_REQT_CANCELED       0xFC  /* 252 */
#define ERR_MESSAGE_QUEUE_FULL                  0xFC  /* 252 */
#define ERR_NO_SUCH_BINDERY_OBJECT              0xFC  /* 252 */
#define ERR_NO_SUCH_OBJECT                      0xFC  /* 252 */
#define ERR_REQUEST_CANCELLED                   0xFC  /* 252 */
#define ERR_SPX_COMMAND_CANCELLED               0xFC  /* 252 */
#define ERR_SPX_SOCKET_CLOSED                   0xFC  /* 252 */
#define ERR_UNKNOWN_FILE_SERVER                 0xFC  /* 252 */
#define ERR_TARGET_ALREADY_HAS_MESSAGE          0xFC  /* 252 */
#define ERR_NCPEXT_SERVICE_PROTOCOL_VIOLATION   0xFC  /* 252 */
#define ERR_BAD_SERIAL_NUMBER                   0xFD  /* 253 */
#define ERR_INVALID_PACKET_LENGTH               0xFD  /* 253 */
#define ERR_PACKET_OVERFLOW                     0xFD  /* 253 */
#define ERR_TTS_DISABLED                        0xFD  /* 253 */
#define ERR_FIELD_ALREADY_LOCKED                0xFD  /* 253 */
#define ERR_FSCOPY_DIFFERENT_NETWORKS           0xFD  /* 253 */
#define ERR_BAD_STATION_NUMBER                  0xFD  /* 253 */
#define ERR_BAD_PACKET                          0xFE  /* 254 */
#define ERR_SPX_MALFORMED_PACKET                0xFE  /* 254 */
#define ERR_BINDERY_LOCKED                      0xFE  /* 254 */
#define ERR_DOS_ACCESS_DENIED                   0xFE  /* 254 */
#define ERR_DOS_NO_SEARCH_RIGHTS                0xFE  /* 254 */
#define ERR_IMPLICIT_TRANSACTION_ACTIVE         0xFE  /* 254 */
#define ERR_INCORRECT_ACCESS_PRIVILEGES         0xFE  /* 254 */
#define ERR_INVALID_NAME_LENGTH                 0xFE  /* 254 */
#define ERR_INVALID_SEMAPHORE_NAME_LENGTH       0xFE  /* 254 */
#define ERR_IO_FAILURE                          0xFE  /* 254 */
#define ERR_PACKET_NOT_DELIVERABLE              0xFE  /* 254 */
#define ERR_SPOOL_DIRECTORY_ERROR               0xFE  /* 254 */
#define ERR_SUPERVISOR_HAS_DISABLED_LOGIN       0xFE  /* 254 */
#define ERR_TRANSACTION_ENDS_RECORDS_LOCKED     0xFE  /* 254 */
#define ERR_SERVER_BINDERY_LOCKED               0xFE  /* 254 */
#define ERR_TIMEOUT_FAILURE                     0xFE  /* 254 */
#define ERR_TRUSTEE_NOT_FOUND                   0xFE  /* 254 */
#define ERR_SOCKET_TABLE_FULL                   0xFE  /* 254 */
#define ERR_NCPEXT_NO_HANDLER                   0xFE  /* 254 */
#define ERR_BAD_PARAMETER                       0xFF  /* 255 */
#define ERR_BAD_SPOOL_PRINTER                   0xFF  /* 255 */
#define ERR_RECORD_ALREADY_LOCKED               0xFF  /* 255 */
#define ERR_BAD_RECORD_OFFSET                   0xFF  /* 255 */
#define ERR_BINDERY_FAILURE                     0xFF  /* 255 */
#define ERR_ECB_NOT_IN_USE                      0xFF  /* 255 */
#define ERR_FAILURE                             0xFF  /* 255 */
#define ERR_FILE_EXTENSION_ERROR                0xFF  /* 255 */
#define ERR_HARD_FAILURE                        0xFF  /* 255 */
#define ERR_INVALID_INITIAL_SEMAPHORE_VALUE     0xFF  /* 255 */
#define ERR_INVALID_SEMAPHORE_HANDLE            0xFF  /* 255 */
#define ERR_DOS_FILE_NOT_FOUND                  0xFF  /* 255 */
#define ERR_EXPLICIT_TRANSACTION_ACTIVE         0xFF  /* 255 */
#define ERR_FILE_NOT_OPEN                       0xFF  /* 255 */
#define ERR_NO_EXPLICIT_TRANSACTION_ACTIVE      0xFF  /* 255 */
#define ERR_NO_FILES_FOUND                      0xFF  /* 255 */
#define ERR_NO_RECORD_FOUND                     0xFF  /* 255 */
#define ERR_NO_RESPONSE_FROM_SERVER             0xFF  /* 255 */
#define ERR_NO_SPOOL_FILE                       0xFF  /* 255 */
#define ERR_NO_SUCH_OBJECT_OR_BAD_PASSWORD      0xFF  /* 255 */
#define ERR_OPEN_FILES                          0xFF  /* 255 */
#define ERR_PATH_ALREADY_EXISTS                 0xFF  /* 255 */
#define ERR_PATH_NOT_LOCATABLE                  0xFF  /* 255 */
#define ERR_QUEUE_FULL                          0xFF  /* 255 */
#define ERR_REQUEST_NOT_OUTSTANDING             0xFF  /* 255 */
#define ERR_SOCKET_CLOSED                       0xFF  /* 255 */
#define ERR_SPX_IS_INSTALLED                    0xFF  /* 255 */
#define ERR_SPX_SOCKET_NOT_OPENED               0xFF  /* 255 */
#define ERR_TARGET_NOT_LOGGED_IN                0xFF  /* 255 */
#define ERR_TARGET_NOT_ACCEPTING_MESSAGES       0xFF  /* 255 */
#define ERR_TRANSACTION_NOT_YET_WRITTEN         0xFF  /* 255 */
#define ERR_NO_TRUSTEE_CHANGE_PRIVILEGE         0xFF  /* 255 */
#define ERR_CHECKSUMS_REQUIRED                  0xFF  /* 255 */
#define ERR_SERVICE_NOT_LOADED                  0x101	/* 257 */
#define ERR_NO_LIBRARY_CONTEXT						0x400	/* 1024 */

/*-----------------------------------------------------------------------------
** Important Note:
** Additional NetWareErrno values that don't employ a ERR_ prefix have been
** moved from this position into obsolete header niterror.h. Many of these had
** been included for compatibility with the now-obsolete NIT API for DOS
** clients and many conflict with current cross-platform headers.
**-----------------------------------------------------------------------------
*/

/*
** NetWare Core Protocol (NCP) error codes.
*/
#define DISKFULL                 1
#define BADNET                   2
#define LISTENERROR              2
#define BADLADDRESS              3
#define INVALIDSESSION           3
#define NOSLOTS                  4
#define SLOTALLOCERR             4
#define BROADCASTERROR           5
#define BADSERVERNAME            6
#define BADUSERNAME              7
#define BADPASSWORD              8
#define MEMERROR                 9
#define INVALIDCONNECTION        10
#define INVALIDHANDLE            11
#define INVALIDREQUEST           12
#define SOCKETERROR              13
#define ALLOCTAGERR              14
#define CONNECTIONABORTED        15
#define TIMEOUTERR               16
#define CHECKSUMS_NOT_SUPPORTED  17 /* frame type: Ethernet 802.3 */
#define CHECKSUM_FAILURE         18
#define NO_FRAGMENT_LIST         19

/*
** Values for 'NetWareErrno' as set by spawnlp() and spawnvp().
*/
#define LOAD_COULD_NOT_FIND_FILE          1
#define LOAD_ERROR_READING_FILE           2
#define LOAD_NOT_NLM_FILE_FORMAT          3
#define LOAD_WRONG_NLM_FILE_VERSION       4
#define LOAD_REENTRANT_INITIALIZE_FAILURE 5
#define LOAD_CAN_NOT_LOAD_MULTIPLE_COPIES 6
#define LOAD_ALREADY_IN_PROGRESS          7
#define LOAD_NOT_ENOUGH_MEMORY            8
#define LOAD_INITIALIZE_FAILURE           9
#define LOAD_INCONSISTENT_FILE_FORMAT     10
#define LOAD_CAN_NOT_LOAD_AT_STARTUP      11
#define LOAD_AUTO_LOAD_MODULES_NOT_LOADED 12
#define LOAD_UNRESOLVED_EXTERNAL          13
#define LOAD_PUBLIC_ALREADY_DEFINED       14

/*
** Values for _msize() error return and NWMemorySizeAddressable().
*/
#define ERR_HEAP_BAD_PTR            0xFFFFFFFF
#define ERR_HEAP_BLOCK_ALREADY_FREE 0xFFFFFFFE
#define ERR_INVALID_ADDRESS         0xFFFFFFFD

/*
** Values for NetWare Virtual Memory (NVM) APIs as returned by GetVMErrno().
** These values should be examined after calling a Win32 VM API without a
** satisfactorily-lucid error in 'errno' or from (Win32) GetLastError().
*/
#define ERROR_INSUFFICIENT_CONTIGUOUS_MEMORY				0x1000	/* 4096 */
#define ERROR_INSUFFICIENT_DISK_SWAP_SPACE				0x1001	/* 4097 */
#define ERROR_INSUFFICIENT_MEMORY							0x1002	/* 4098 */
#define ERROR_INSUFFICIENT_RESOURCES_TO_COMMIT_MEMORY	0x1003	/* 4099 */
#define ERROR_INVALID_ATTRIBUTE_FLAGS						0x1004	/* 4100 */
#define ERROR_INVALID_ADDRESS									0x1005	/* 4101 */
#define ERROR_INVALID_LOCK_FLAGS								0x1006	/* 4102 */
#define ERROR_INVALID_PAGE_COUNT								0x1007	/* 4103 */
#define ERROR_INVALID_PROTECTION_FLAGS						0x1008	/* 4104 */
#define ERROR_NON_SHARED_MEMORY_ADDRESS					0x1009	/* 4105 */
#define ERROR_SHARED_MEMORY_ADDRESS							0x100A	/* 4106 */

#ifdef __cplusplus
extern "C"
{
#endif

extern LONG GetVMErrno( void );
extern void SetVMErrno( LONG );
extern int  __get_NWErrno( void ); 
extern int  *__get_NWErrno_ptr( void );

#ifdef __cplusplus
}
#endif


#define NetWareErrno *__get_NWErrno_ptr()

#endif
