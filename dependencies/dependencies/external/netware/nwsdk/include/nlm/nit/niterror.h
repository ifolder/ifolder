#ifndef _NITERROR_H_
#define _NITERROR_H_
/*============================================================================
=
=  NetWare NLM Library source code
=
=  Unpublished Copyright (C) 1995-8 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  niterror.h
==============================================================================
*/
#ifdef _FIND_OLD_HEADERS_
# error This is an obsolete, Novell SDK header! You should use nwerrno.h.
#endif

/*-----------------------------------------------------------------------------
** Important Note:
** 'NetWareErrno' values (without a ERR_ prefix--included for compatibility
** with the NIT API for DOS clients). These errors have been removed to here
** from nwerrno.h as part of progressing obsolescence especially since some of
** these conflict with definitions in the cross-platform headers.
**-----------------------------------------------------------------------------
*/
#define NITERROR

#define OUT_OF_MEMORY                             -1
#define COMPATIBLE                              0x00    /*   0 */
#define PERMANENT_DIRECTORY_HANDLE              0x00    /*   0 */
#define SERVER_NOT_IN_USE                       0x00    /*   0 */
#define SHELL_VERSION_TOO_OLD                   0x00    /*   0 */
#define SPX_CONNECTION_OK                       0x00    /*   0 */
#define SPX_CONNECTION_STARTED                  0x00    /*   0 */
#define SPX_NOT_INSTALLED                       0x00    /*   0 */
#define SPX_PACKET_SUCCESSFUL                   0x00    /*   0 */
#define SUCCESSFUL                              0x00    /*   0 */
#define TTS_NOT_AVAILABLE                       0x00    /*   0 */
#define TEMPORARY_DIRECTORY_HANDLE              0x01    /*   1 */
#define TTS_AVAILABLE                           0x01    /*   1 */
#define SEMAPHORE_OVERFLOW                      0x01    /*   1 */
#define SERVER_IN_USE                           0x01    /*   1 */
#define STRING_EXCEEDS_LENGTH                   0x01    /*   1 */
#define VERSION_NUMBER_TOO_LOW                  0x01    /*   1 */
#define SFT_LEVEL_TOO_LOW                       0x02    /*   2 */
#define TTS_LEVEL_TOO_LOW                       0x03    /*   3 */
#define FILE_IN_USE_ERROR                       0x80    /* 128 */
#define MAPPED_TO_A_LOCAL_DRIVE                 0x80    /* 128 */
#define NO_MORE_FILE_HANDLES                    0x81    /* 129 */
#define NO_OPEN_PRIVILEGES                      0x82    /* 130 */
#define IO_ERROR_NETWORK_DISK                   0x83    /* 131 */
#define NO_CREATE_PRIVILEGES                    0x84    /* 132 */
#define NO_CREATE_DELETE_PRIVILEGES             0x85    /* 133 */
#define CREATE_FILE_EXISTS_READ_ONLY            0x86    /* 134 */
#define WILD_CARDS_IN_CREATE_FILE_NAME          0x87    /* 135 */
#define INVALID_FILE_HANDLE                     0x88    /* 136 */
#define NO_SEARCH_PRIVILEGES                    0x89    /* 137 */
#define NO_DELETE_PRIVILEGES                    0x8A    /* 138 */
#define NO_RENAME_PRIVILEGES                    0x8B    /* 139 */
#define NO_MODIFY_PRIVILEGES                    0x8C    /* 140 */
#define SOME_FILES_AFFECTED_IN_USE              0x8D    /* 141 */
#define NO_FILES_AFFECTED_IN_USE                0x8E    /* 142 */
#define SOME_FILES_AFFECTED_READ_ONLY           0x8F    /* 143 */
#define NO_FILES_AFFECTED_READ_ONLY             0x90    /* 144 */
#define SOME_FILES_RENAMED_NAME_EXISTS          0x91    /* 145 */
#define NO_FILES_RENAMED_NAME_EXISTS            0x92    /* 146 */
#define NO_READ_PRIVILEGES                      0x93    /* 147 */
#define NO_WRITE_PRIVILEGES_OR_READONLY         0x94    /* 148 */
#define FILE_DETACHED                           0x95    /* 149 */
#define SERVER_OUT_OF_MEMORY                    0x96    /* 150 */
#define NO_DISK_SPACE_FOR_SPOOL_FILE            0x97    /* 151 */
#define BadVolumeNameCode                       0x98    /* 152 */
#define VOLUME_DOES_NOT_EXIST                   0x98    /* 152 */
#ifndef DIRECTORY_FULL
# define DIRECTORY_FULL                         0x99    /* 153 */
#endif
#define RENAMING_ACROSS_VOLUMES                 0x9A    /* 154 */
#define BAD_DIRECTORY_HANDLE                    0x9B    /* 155 */
#define INVALID_PATH                            0x9C    /* 156 */
#define InvalidPathCode                         0x9C    /* 156 */
#define NO_MORE_TRUSTEES                        0x9C    /* 156 */
#define NO_MORE_DIRECTORY_HANDLES               0x9D    /* 157 */
#define INVALID_FILENAME                        0x9E    /* 158 */
#define INVALID_FILE_NAME                       0x9E    /* 158 */
#define DIRECTORY_ACTIVE                        0x9F    /* 159 */
#define DIRECTORY_IN_USE                        0x9F    /* 159 */
#define DIRECTORY_NOT_EMPTY                     0xA0    /* 160 */
#define DIRECTORY_IO_ERROR                      0xA1    /* 161 */
#define READ_FILE_WITH_RECORD_LOCKED            0xA2    /* 162 */
#define SEARCH_DRIVE_VECTOR_FULL                0xB0    /* 176 */
#define DRIVE_DOES_NOT_EXIST                    0xB1    /* 177 */
#define DRIVE_IS_NOT_MAPPED                     0xB1    /* 177 */
#define CANT_MAP_LOCAL_DRIVE                    0xB2    /* 178 */
#define INVALID_MAP_TYPE                        0xB3    /* 179 */
#define INVALID_DRIVE_LETTER                    0xB4    /* 180 */
#define NO_DRIVE_AVAILABLE                      0xB5    /* 181 */
#define WORKSTATION_OUT_OF_MEMORY               0xB6    /* 182 */
#define NO_SUCH_SEARCH_DRIVE                    0xB7    /* 183 */
#define PATH_ENVIRON_VARIABLE_INVALID           0xB8    /* 184 */
#define DOES_NOT_RUN_ON_IOENGINE						0xB9	  /* 185 */
#define NO_ACCOUNT_PRIVILEGES                   0xC0    /* 192 */
#define NO_ACCT_PRIVLEGES                       0xC0    /* 192 */
#define LOGIN_DENIED_NO_ACCOUNT_BALANCE         0xC1    /* 193 */
#define NO_ACCOUNT_BALANCE                      0xC1    /* 193 */
#define NO_ACCT_BALANCE                         0xC1    /* 193 */
#define ACCT_CREDIT_LIMIT_EXCEEDED              0xC2    /* 194 */
#define CREDIT_LIMIT_EXCEEDED                   0xC2    /* 194 */
#define LOGIN_DENIED_NO_CREDIT                  0xC2    /* 194 */
#define ACCT_TOO_MANY_HOLDS                     0xC3    /* 195 */
#define TOO_MANY_HOLDS                          0xC3    /* 195 */
#define INTRUDER_DETECTION_LOCK                 0xC5    /* 197 */
#define NO_CONSOLE_OPERATOR                     0xC6    /* 198 */
#define NO_CONSOLE_RIGHTS                       0xC6    /* 198 */
#define Q_ERROR                                 0xD0    /* 208 */
#define NO_QUEUE                                0xD1    /* 209 */
#define NO_Q_SERVER                             0xD2    /* 210 */
#define NO_Q_RIGHTS                             0xD3    /* 211 */
#define Q_FULL                                  0xD4    /* 212 */
#define NO_Q_JOB                                0xD5    /* 213 */
#define NO_JOB_RIGHTS                           0xD6    /* 214 */
#define PASSWORD_NOT_UNIQUE                     0xD7    /* 215 */
#define Q_SERVICING                             0xD7    /* 215 */
#define PASSWORD_TOO_SHORT                      0xD8    /* 216 */
#define LOGIN_DENIED_NO_CONNECTION              0xD9    /* 217 */
#define STN_NOT_SERVER                          0xD9    /* 217 */
#define Q_HALTED                                0xDA    /* 218 */
#define UNAUTHORIZED_LOGIN_TIME                 0xDA    /* 218 */
#define MAX_Q_SERVERS                           0xDB    /* 219 */
#define UNAUTHORIZED_LOGIN_STATION              0xDB    /* 219 */
#define ACCOUNT_DISABLED                        0xDC    /* 220 */
#define PASSWORD_HAS_EXPIRED_NO_GRACE           0xDE    /* 222 */
#define PASSWORD_HAS_EXPIRED                    0xDF    /* 223 */
#define NO_ITEM_PROPERTY                        0xE8    /* 232 */
#define NOT_ITEM_PROPERTY                       0xE8    /* 232 */
#define WRITE_PROPERTY_TO_GROUP                 0xE8    /* 232 */
#define MEMBER_ALREADY_EXISTS                   0xE9    /* 233 */
#define NO_SUCH_MEMBER                          0xEA    /* 234 */
#define NOT_GROUP_PROPERTY                      0xEB    /* 235 */
#define NO_SUCH_SEGMENT                         0xEC    /* 236 */
#define SPX_CONNECTION_TERMINATED               0xEC    /* 236 */
#define TERMINATED_BY_REMOTE_PARTNER            0xEC    /* 236 */
#define PROPERTY_ALREADY_EXISTS                 0xED    /* 237 */
#define SPX_CONNECTION_FAILED                   0xED    /* 237 */
#define SPX_TERMINATED_POORLY                   0xED    /* 237 */
#define SPX_NO_ANSWER_FROM_TARGET               0xED    /* 237 */
#define OBJECT_ALREADY_EXISTS                   0xEE    /* 238 */
#define SPX_INVALID_CONNECTION                  0xEE    /* 238 */
#define INVALID_NAME                            0xEF    /* 239 */
#define SPX_CONNECTION_TABLE_FULL               0xEF    /* 239 */
#define IPX_NOT_INSTALLED                       0xF0    /* 240 */
#define WILD_CARD_NOT_ALLOWED                   0xF0    /* 240 */
#define WILDCARD_NOT_ALLOWED                    0xF0    /* 240 */
#define SOCKET_NOT_OPEN                         0xF0    /* 240 */
#define SOCKET_ALREADY_OPEN                     0xF1    /* 241 */
#define INVALID_BINDERY_SECURITY                0xF1    /* 241 */
#define NO_OBJECT_READ_PRIVILEGE                0xF2    /* 242 */
#define NO_OBJECT_RENAME_PRIVILEGE              0xF3    /* 243 */
#define NO_OBJECT_DELETE_PRIVILEGE              0xF4    /* 244 */
#define NO_OBJECT_CREATE_PRIVILEGE              0xF5    /* 245 */
#define NO_PROPERTY_DELETE_PRIVILEGE            0xF6    /* 246 */
#define NOT_SAME_LOCAL_DRIVE                    0xF6    /* 246 */
#define TARGET_AND_BASE_DRIVES_DONT_MATCH       0xF6    /* 246 */
#define NO_PROPERTY_CREATE_PRIVILEGE            0xF7    /* 247 */
#define TARGET_DRIVE_NOT_LOCAL                  0xF7    /* 247 */
#define ALREADY_ATTACHED_TO_SERVER              0xF8    /* 248 */
#define NO_PROPERTY_WRITE_PRIVILEGE             0xF8    /* 248 */
#define NOT_ATTACHED_TO_SERVER                  0xF8    /* 248 */
#define ECB_CANNOT_BE_CANCELLED                 0xF9    /* 249 */
#define NO_FREE_CONNECTION_SLOTS                0xF9    /* 249 */
#define NO_PROPERTY_READ_PRIVILEGE              0xF9    /* 249 */
#define NO_LOCAL_TARGET_IDENTIFIED              0xFA    /* 250 */
#define NO_MORE_SERVER_SLOTS                    0xFA    /* 250 */
#define TEMP_REMAP_ERROR                        0xFA    /* 250 */
#define INVALID_PARAMETERS                      0xFB    /* 251 */
#define NO_SUCH_PROPERTY                        0xFB    /* 251 */
#define UNKNOWN_REQUEST                         0xFB    /* 251 */
#define EVENT_CANCELLED                         0xFC    /* 252 */
#define INTERNET_PACKET_REQT_CANCELED           0xFC    /* 252 */
#define MESSAGE_QUEUE_FULL                      0xFC    /* 252 */
#define NO_SUCH_OBJECT                          0xFC    /* 252 */
#define REQUEST_CANCELLED                       0xFC    /* 252 */
#define SPX_COMMAND_CANCELLED                   0xFC    /* 252 */
#define SPX_SOCKET_CLOSED                       0xFC    /* 252 */
#define UNKNOWN_FILE_SERVER                     0xFC    /* 252 */
#define BAD_STATION_NUMBER                      0xFD    /* 253 */
#define INVALID_PACKET_LENGTH                   0xFD    /* 253 */
#define SPX_PACKET_OVERFLOW                     0xFD    /* 253 */
#define TTS_DISABLED                            0xFD    /* 253 */
#define FIELD_ALREADY_LOCKED                    0xFD    /* 253 */
#define FSCOPY_DIFFERENT_NETWORKS               0xFD    /* 253 */
#define BAD_PACKET                              0xFE    /* 254 */
#define SPX_MALFORMED_PACKET                    0xFE    /* 254 */
#define BINDERY_LOCKED                          0xFE    /* 254 */
#define DIRECTORY_LOCKED                        0xFE    /* 254 */
#define DOS_ACCESS_DENIED                       0xFE    /* 254 */
#define DOS_NO_SEARCH_RIGHTS                    0xFE    /* 254 */
#define IMPLICIT_TRANSACTION_ACTIVE             0xFE    /* 254 */
#define INCORRECT_ACCESS_PRIVILEGES             0xFE    /* 254 */
#define INVALID_SEMAPHORE_NAME_LENGTH           0xFE    /* 254 */
#define IO_FAILURE                              0xFE    /* 254 */
#define PACKET_NOT_DELIVERABLE                  0xFE    /* 254 */
#define SERVER_BINDERY_LOCKED                   0xFE    /* 254 */
#define SOCKET_TABLE_FULL                       0xFE    /* 254 */
#define SPOOL_DIRECTORY_ERROR                   0xFE    /* 254 */
#define SUPERVISOR_HAS_DISABLED_LOGIN           0xFE    /* 254 */
#define TIMEOUT_FAILURE                         0xFE    /* 254 */
#define TRANSACTION_ENDS_RECORDS_LOCKED         0xFE    /* 254 */
#define BAD_RECORD_OFFSET                       0xFF    /* 255 */
#define BINDERY_FAILURE                         0xFF    /* 255 */
#define CLOSE_FCB_ERROR                         0xFF    /* 255 */
#define EXPLICIT_TRANSACTION_ACTIVE             0xFF    /* 255 */
#define EXPLICIT_TRANSACTION_NOT_ACTIVE         0xFF    /* 255 */
#ifndef FAILURE
# define FAILURE                                0xFF    /* 255 */
#endif
#define FILE_EXTENSION_ERROR                    0xFF    /* 255 */
#define FILE_NAME_ERROR                         0xFF    /* 255 */
#define HARDWARE_FAILURE                        0xFF    /* 255 */
#define INVALID_DRIVE_NUMBER                    0xFF    /* 255 */
#define INVALID_INITIAL_SEMAPHORE_VALUE         0xFF    /* 255 */
#define INVALID_SEMAPHORE_HANDLE                0xFF    /* 255 */
#define BAD_PRINTER_ERROR                       0xFF    /* 255 */
#define DOS_FILE_NOT_FOUND                      0xFF    /* 255 */
#define ECB_NOT_IN_USE                          0xFF    /* 255 */
#define FILE_NOT_OPEN                           0xFF    /* 255 */
#define IO_BOUND_ERROR                          0xFF    /* 255 */
#define NO_EXPLICIT_TRANSACTION_ACTIVE          0xFF    /* 255 */
#define NO_FILES_FOUND_ERROR                    0xFF    /* 255 */
#define NO_RECORD_FOUND                         0xFF    /* 255 */
#define NO_RESPONSE_FROM_SERVER                 0xFF    /* 255 */
#define NO_SUCH_OBJECT_OR_BAD_PASSWORD          0xFF    /* 255 */
#define OPEN_FILES                              0xFF    /* 255 */
#define PATH_ALREADY_EXISTS                     0xFF    /* 255 */
#define PATH_NOT_LOCATABLE                      0xFF    /* 255 */
#define QUEUE_FULL_ERROR                        0xFF    /* 255 */
#define REQUEST_NOT_OUTSTANDING                 0xFF    /* 255 */
#define SOCKET_CLOSED                           0xFF    /* 255 */
#define SPX_IS_INSTALLED                        0xFF    /* 255 */
#define SPX_SOCKET_NOT_OPENED                   0xFF    /* 255 */
#define TARGET_AND_PATH_SEPARATE_LOCAL_DRIVES   0xFF    /* 255 */
#define TRANSACTION_NOT_YET_WRITTEN             0xFF    /* 255 */

#endif
