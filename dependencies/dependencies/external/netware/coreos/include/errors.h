#ifndef __ERRORS_H__
#define __ERRORS_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   errors.h  $
 *  $Revision$
 *  $Modtime:   Sep 02 1999 16:31:18  $
 *  $Author$
 *
 ****************************************************************************/


/* ERROR CODES */

#define OK												000
#define ERR_INSUFFICIENT_SPACE					001
#define	ERR_TTS_OUT_OF_RESOURCES				002

#define ERR_REVOKE_HANDLE_RIGHTS_NOT_FOUND	115	/* 0x73 - also defined in locks.h */
#define ERR_REMOTE_NOT_ALLOWED					116	/* 0x74 - also defined in locks.h */
#define ERR_UNKNOWN_SUCCESS_OR_FAILURE			117	/* 0x75 */
#define ERR_BUFFER_NOT_LONG_ALIGNED		      118	/* 0x76 */
#define ERR_BUFFER_TOO_SMALL						119	/* 0x77 */
#define ERR_VOLUME_FLAG_NOT_SET 					120	/* 0x78 */
#define ERR_NO_ITEMS_FOUND							121	/* 0x79 */
#define ERR_CONNECTION_ALREADY_TEMPORARY		122	/* 0x7a */
#define ERR_CONNECTION_ALREADY_LOGGED_IN		123	/* 0x7b */
#define ERR_CONNECTION_NOT_AUTHENTICATED		124	/* 0x7c */
#define ERR_CONNECTION_NOT_LOGGED_IN			125	/* 0x7d */
#define ERR_NCP_BOUNDARY_CHECK_FAILED			126	/* 0x7e */
#define ERR_LOCK_WAITING							127	/* 0x7f */
#define ERR_LOCK_FAIL								128	/* 0x80 */
#define ERR_OUT_OF_HANDLES							129	/* 0x81 */
#define ERR_NO_OPEN_PRIVILEGE						130	/* 0x82 */
#define ERR_HARD_IO_ERROR							131	/* 0x83 */
#define ERR_NO_CREATE_PRIVILEGE 					132	/* 0x84 */
#define ERR_NO_CREATE_DELETE_PRIVILEGE			133	/* 0x85 */
#define ERR_R_O_CREATE_FILE						134	/* 0x86 */
#define ERR_CREATE_FILE_INVALID_NAME			135	/* 0x87 */
#define ERR_INVALID_FILE_HANDLE 					136	/* 0x88 */
#define ERR_NO_SEARCH_PRIVILEGE 					137	/* 0x89 */
#define ERR_NO_DELETE_PRIVILEGE 					138	/* 0x8a */
#define ERR_NO_RENAME_PRIVILEGE 					139	/* 0x8b */
#define ERR_NO_SET_PRIVILEGE						140	/* 0x8c */
#define ERR_SOME_FILES_IN_USE						141	/* 0x8d */
#define ERR_ALL_FILES_IN_USE						142	/* 0x8e */
#define ERR_SOME_READ_ONLY							143	/* 0x8f */
#define ERR_ALL_READ_ONLY							144	/* 0x90 */
#define ERR_SOME_NAMES_EXIST						145	/* 0x91 */
#define ERR_ALL_NAMES_EXIST						146	/* 0x92 */
#define ERR_NO_READ_PRIVILEGE						147	/* 0x93 */
#define ERR_NO_WRITE_PRIVILEGE					148	/* 0x94 */
#define ERR_FILE_DETACHED							149	/* 0x95 */
#define ERR_NO_ALLOC_SPACE							150	/* 0x96 */
#define ERR_TARGET_NOT_A_SUBDIRECTORY			150	/* 0x97 */
#define ERR_NO_SPOOL_SPACE							151	/* 0x97 */
#define ERR_INVALID_VOLUME							152	/* 0x98 */
#define ERR_DIRECTORY_FULL							153	/* 0x99 */
#define ERR_RENAME_ACROSS_VOLUME					154	/* 0x9a */
#define ERR_BAD_DIR_HANDLE							155	/* 0x9b */
#define ERR_INVALID_PATH							156	/* 0x9c */
#define ERR_NO_SUCH_EXTENSION						156	/* 0x9d */
#define ERR_NO_DIR_HANDLES							157	/* 0x9d */
#define ERR_BAD_FILE_NAME							158	/* 0x9e */
#define ERR_DIRECTORY_ACTIVE						159	/* 0x9f */
#define ERR_DIRECTORY_NOT_EMPTY 					160	/* 0xa0 */
#define ERR_DIRECTORY_IO_ERROR					161	/* 0xa1 */
#define ERR_IO_LOCKED								162	/* 0xa2 */
#define ERR_TRANSACTION_RESTARTED				163	/* 0xa3 */
#define ERR_RENAME_DIR_INVALID					164	/* 0xa4 */
#define ERR_INVALID_OPENCREATE_MODE				165	/* 0xa5 */
#define ERR_ALREADY_IN_USE							166	/* 0xa6 */
#define ERR_INVALID_RESOURCE_TAG					167	/* 0xa7 */
#define ERR_ACCESS_DENIED							168	/* 0xa8 */

#define ERR_INVALID_DATA_STREAM 					190	/* 0xbe */
#define ERR_INVALID_NAME_SPACE					191	/* 0xbf */
#define ERR_NO_ACCOUNTING_PRIVILEGES			192	/* 0xc0 */
#define ERR_NO_ACCOUNT_BALANCE					193	/* 0xc1 */
#define ERR_CREDIT_LIMIT_EXCEEDED				194	/* 0xc2 */
#define ERR_TOO_MANY_HOLDS							195	/* 0xc3 */
#define ERR_ACCOUNTING_DISABLED 					196	/* 0xc4 */
#define ERR_LOGIN_LOCKOUT							197	/* 0xc5 */
#define ERR_NO_CONSOLE_RIGHTS						198	/* 0xc6 */

#define ERR_Q_IO_FAILURE							208	/* 0xd0 */
#define ERR_NO_QUEUE									209	/* 0xd1 */
#define ERR_NO_Q_SERVER 							210	/* 0xd2 */
#define ERR_NO_Q_RIGHTS 							211	/* 0xd3 */
#define ERR_Q_FULL									212	/* 0xd4 */
#define ERR_NO_Q_JOB									213	/* 0xd5 */
#define ERR_NO_Q_JOB_RIGHTS						214	/* 0xd6 */
#define ERR_UNENCRYPTED_NOT_ALLOWED				214	/* 0xd6 */
#define ERR_Q_IN_SERVICE							215	/* 0xd7 */
#define ERR_DUPLICATE_PASSWORD					215	/* 0xd7 */
#define ERR_Q_NOT_ACTIVE							216	/* 0xd8 */
#define ERR_PASSWORD_TOO_SHORT					216	/* 0xd8 */
#define ERR_Q_STN_NOT_SERVER						217	/* 0xd9 */
#define ERR_MAXIMUM_LOGINS_EXCEEDED				217	/* 0xd9 */
#define ERR_Q_HALTED									218	/* 0xda */
#define ERR_BAD_LOGIN_TIME							218	/* 0xda */
#define ERR_Q_MAX_SERVERS							219	/* 0xdb */
#define ERR_NODE_ADDRESS_VIOLATION				219	/* 0xdb */
#define ERR_LOG_ACCOUNT_EXPIRED 					220	/* 0xdc */
#define ERR_BAD_PASSWORD							222	/* 0xde */
#define ERR_PASSWORD_EXPIRED						223	/* 0xdf */
#define ERR_NO_LOGIN_CONNECTIONS_AVAILABLE	224	/* 0xe0 */

#define ERR_WRITE_TO_GROUP_PROPERTY				232	/* 0xe8 */
#define ERR_MEMBER_ALREADY_EXISTS				233	/* 0xe9 */
#define ERR_NO_SUCH_MEMBER							234	/* 0xea */
#define ERR_PROPERTY_NOT_GROUP					235	/* 0xeb */
#define ERR_NO_SUCH_VALUE_SET						236	/* 0xec */
#define ERR_PROPERTY_ALREADY_EXISTS				237	/* 0xed */
#define ERR_OBJECT_ALREADY_EXISTS				238	/* 0xee */
#define ERR_ILLEGAL_NAME							239	/* 0xef */
#define ERR_ILLEGAL_WILDCARD						240	/* 0xf0 */
#define ERR_BINDERY_SECURITY						241	/* 0xf1 */
#define ERR_NO_OBJECT_READ_RIGHTS				242	/* 0xf2 */
#define ERR_NO_OBJECT_RENAME_RIGHTS				243	/* 0xf3 */
#define ERR_NO_OBJECT_DELETE_RIGHTS				244	/* 0xf4 */
#define ERR_NO_OBJECT_CREATE_RIGHTS				245	/* 0xf5 */
#define ERR_NO_PROPERTY_DELETE_RIGHTS			246	/* 0xf6 */
#define ERR_NO_PROPERTY_CREATE_RIGHTS			247	/* 0xf7 */
#define ERR_NO_PROPERTY_WRITE_RIGHTS			248	/* 0xf8 */
#define ERR_NO_PROPERTY_READ_RIGHTS				249	/* 0xf9 */
#define ERR_TEMP_REMAP								250	/* 0xfa */
#define ERR_UNKNOWN_REQUEST						251	/* 0xfb */
#define ERR_NO_SUCH_PROPERTY						251	/* 0xfb */
#define ERR_MESSAGE_QUEUE_FULL					252	/* 0xfc */
#define ERR_TARGET_ALREADY_HAS_MESSAGE			252	/* 0xfc */
#define ERR_NO_SUCH_OBJECT							252	/* 0xfc */
#define ERR_BAD_STATION_NUMBER					253	/* 0xfd */
#define ERR_BINDERY_LOCKED							254	/* 0xfe */
#define ERR_DIR_LOCKED								254	/* 0xfe */
#define ERR_SPOOL_DELETE							254	/* 0xfe */
#define ERR_TRUSTEE_NOT_FOUND						254	/* 0xfe */
#define ERR_HARD_FAILURE							255	/* 0xff */
#define ERR_FILE_NAME								255	/* 0xff */
#define ERR_FILE_EXISTS 							255	/* 0xff */
#define ERR_CLOSE_FCB								255	/* 0xff */
#define ERR_IO_BOUND									255	/* 0xff */
#define ERR_NO_SPOOL_FILE							255	/* 0xff */
#define ERR_BAD_SPOOL_PRINTER						255	/* 0xff */
#define ERR_BAD_PARAMETER							255	/* 0xff */
#define ERR_NO_FILES_FOUND							255	/* 0xff */
#define ERR_NO_TRUSTEE_CHANGE_PRIVILEGE 		255	/* 0xff */
#define ERR_TARGET_NOT_LOGGED_IN					255	/* 0xff */
#define ERR_TARGET_NOT_ACCEPTING_MESSAGES		255	/* 0xff */
#define ERR_MUST_FORCE_DOWN						255	/* 0xff */
#define ERR_CHECKSUM_REQUIRED						255	/* 0xff */
#define ERR_SERVICE_ALREADY_LOADED				256	/* 0x100 */
#define ERR_SERVICE_NOT_LOADED					257	/* 0x101 */
#define ERR_INCORRECT_VERSION						258	/* 0x102 */
#define ERR_NO_MEMORY_READ_ACCESS				259	/* 0x103 */
#define ERR_NO_MEMORY_WRITE_ACCESS				260	/* 0x104 */

/* The following errors are returned in a LONG */
#define	ERR_MODULE_NOT_UNLOADED				513
#define	ERR_MODULE_NOT_LOADED				514
#define	ERR_UNABLE_TO_MOUNT_VOLUME			515
#define	ERR_UNABLE_TO_DISMOUNT_VOLUME		516
#define ERR_UNABLE_TO_ADD_NAME_SPACE		517
#define	ERR_UNABLE_TO_SET_PARAMETER_VALUE	518
#define ERR_UNABLE_TO_EXECUTE_NCF_FILE		519

/****************************************************************************/
/*

The following sections catagorize error codes by the routines that return
them
------------------------------------------------------------------------------
Return codes from login procedures

	ERR_LOG_ACCOUNT_EXPIRED
	ERR_NODE_ADDRESS_VIOLATION
	ERR_BAD_LOGIN_TIME
	ERR_MAXIMUM_LOGINS_EXCEEDED
	ERR_PASSWORD_TOO_SHORT
	ERR_DUPLICATE_PASSWORD
	ERR_UNENCRYPTED_NOT_ALLOWED
	ERR_LOGIN_LOCKOUT
	ERR_PASSWORD_EXPIRED
	ERR_BAD_PASSWORD

	Note that ERR_BAD_PASSWORD is used for two reasons
	1) on a login call, it means the password was correct, but it has
		expired and all grace logins have been used up.
	2) on a change password call, it means that the old password given was
		correct, but the account is not allowed to change the password
		(typical of the GUEST account).
	ERR_BAD_PASSWORD is not used to indicate that an incorrect password was
	given, instead 255 indicates that the given password was invalid,
	no such account exists, etc.

------------------------------------------------------------------------------
Return codes from accounting procedures

	ERR_NO_ACCOUNTING_PRIVILEGES
	ERR_NO_ACCOUNT_BALANCE
	ERR_CREDIT_LIMIT_EXCEEDED
	ERR_TOO_MANY_HOLDS
	ERR_ACCOUNTING_DISABLED

------------------------------------------------------------------------------
Return codes from queue procedures

	ERR_Q_IO_FAILURE
	ERR_NO_QUEUE
	ERR_NO_Q_SERVER
	ERR_NO_Q_RIGHTS
	ERR_Q_FULL
	ERR_NO_Q_JOB
	ERR_NO_Q_JOB_RIGHTS
	ERR_Q_IN_SERVICE
	ERR_Q_NOT_ACTIVE
	ERR_Q_STN_NOT_SERVER
	ERR_Q_HALTED
	ERR_Q_MAX_SERVERS

------------------------------------------------------------------------------
Return codes from broadcast procedures

	ERR_TARGET_NOT_LOGGED_IN
	ERR_TARGET_NOT_ACCEPTING_MESSAGES
	ERR_TARGET_ALREADY_HAS_MESSAGE

------------------------------------------------------------------------------
Return codes from bindery procedures

	ERR_HARD_FAILURE				Unrecoverable error (disk error, etc.)
	ERR_BINDERY_LOCKED
	ERR_NO_SUCH_OBJECT
	ERR_NO_SUCH_PROPERTY
	ERR_NO_PROPERTY_READ_RIGHTS
	ERR_NO_PROPERTY_WRITE_RIGHTS
	ERR_NO_PROPERTY_CREATE_RIGHTS	No Property Creation or Change privileges
	ERR_NO_PROPERTY_DELETE_RIGHTS
	ERR_NO_OBJECT_CREATE_RIGHTS		No Object Creation or Change privileges
	ERR_NO_OBJECT_DELETE_RIGHTS
	ERR_NO_OBJECT_RENAME_RIGHTS
	ERR_NO_OBJECT_READ_RIGHTS		No Object Read (Property Scan) privileges
	ERR_BINDERY_SECURITY			Attempt to change security to bindery-only status
	ERR_ILLEGAL_WILDCARD
	ERR_ILLEGAL_NAME
	ERR_OBJECT_ALREADY_EXISTS
	ERR_PROPERTY_ALREADY_EXISTS
	ERR_NO_SUCH_VALUE_SET			No property value to retrieve (in read/write)
	ERR_PROPERTY_NOT_GROUP			Set request made for non-set property
	ERR_NO_SUCH_MEMBER				Set request for non-existent member
	ERR_MEMBER_ALREADY_EXISTS		Setadd request for member already in set
	ERR_WRITE_TO_GROUP_PROPERTY		Attempt to Write Property to a set

	OK								Operation successfully completed
------------------------------------------------------------------------------
*/

/****************************************************************************/
/****************************************************************************/


#endif /* __ERRORS_H__ */
