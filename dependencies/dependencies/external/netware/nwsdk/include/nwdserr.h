/*****************************************************************************
 *
 *	Copyright (C) Unpublished Work of Novell, Inc.
 *	All Rights Reserved.
 *
 *	This work is an unpublished work and contains confidential,
 *	proprietary and trade secret information of Novell, Inc. Access
 *	to this work is restricted to (i) Novell, Inc. employees who have
 *	a need to know how to perform tasks within the scope of their
 *	assignments and (ii) entities other than Novell, Inc. who have
 *	entered into appropriate license agreements. No part of this work
 *	may be used, practiced, performed, copied, distributed, revised,
 *	modified, translated, abridged, condensed, expanded, collected,
 *	compiled, linked, recast, transformed or adapted without the
 *	prior written consent of Novell, Inc. Any use or exploitation of
 *	this work without authorization could subject the perpetrator to
 *	criminal and civil liability.
 *
 *	Name:					nwdserr.h
 *	%version:       	29 %
 *	%created_by:    	layne %
 *	%date_modified:	Mon Dec 14 15:35:14 1998 %
 *
 ****************************************************************************/

#ifndef NWDSERR_H
#define NWDSERR_H

/* errors from the file system, IPX, NCP, and other NW OS services are one byte
 * and are mapped to -1 to -256 when returned as a directory services error
 */
#define DSERR_INSUFFICIENT_SPACE					-001		/* FFFFFFFF */
#define DSERR_BUFFER_TOO_SMALL					-119		/* FFFFFF89 */
#define DSERR_VOLUME_FLAG_NOT_SET				-120		/* FFFFFF88 */
#define DSERR_NO_ITEMS_FOUND						-121		/* FFFFFF87 */
#define DSERR_CONN_ALREADY_TEMPORARY			-122		/* FFFFFF86 */
#define DSERR_CONN_ALREADY_LOGGED_IN			-123		/* FFFFFF85 */
#define DSERR_CONN_NOT_AUTHENTICATED			-124		/* FFFFFF84 */
#define DSERR_CONN_NOT_LOGGED_IN					-125		/* FFFFFF83 */
#define DSERR_NCP_BOUNDARY_CHECK_FAILED		-126		/* FFFFFF82 */
#define DSERR_LOCK_WAITING							-127		/* FFFFFF81 */
#define DSERR_LOCK_FAIL 							-128		/* FFFFFF80 */
#define DSERR_OUT_OF_HANDLES						-129		/* FFFFFF7F */
#define DSERR_NO_OPEN_PRIVILEGE 					-130		/* FFFFFF7E */
#define DSERR_HARD_IO_ERROR 						-131		/* FFFFFF7D */
#define DSERR_NO_CREATE_PRIVILEGE				-132		/* FFFFFF7C */
#define DSERR_NO_CREATE_DELETE_PRIV 			-133		/* FFFFFF7B */
#define DSERR_R_O_CREATE_FILE						-134		/* FFFFFF7A */
#define DSERR_CREATE_FILE_INVALID_NAME			-135		/* FFFFFF79 */
#define DSERR_INVALID_FILE_HANDLE				-136		/* FFFFFF78 */
#define DSERR_NO_SEARCH_PRIVILEGE				-137		/* FFFFFF77 */
#define DSERR_NO_DELETE_PRIVILEGE				-138		/* FFFFFF76 */
#define DSERR_NO_RENAME_PRIVILEGE				-139		/* FFFFFF75 */
#define DSERR_NO_SET_PRIVILEGE					-140		/* FFFFFF74 */
#define DSERR_SOME_FILES_IN_USE 					-141		/* FFFFFF73 */
#define DSERR_ALL_FILES_IN_USE					-142		/* FFFFFF72 */
#define DSERR_SOME_READ_ONLY						-143		/* FFFFFF71 */
#define DSERR_ALL_READ_ONLY 						-144		/* FFFFFF70 */
#define DSERR_SOME_NAMES_EXIST					-145		/* FFFFFF6F */
#define DSERR_ALL_NAMES_EXIST						-146		/* FFFFFF6E */
#define DSERR_NO_READ_PRIVILEGE 					-147		/* FFFFFF6D */
#define DSERR_NO_WRITE_PRIVILEGE					-148		/* FFFFFF6C */
#define DSERR_FILE_DETACHED 						-149		/* FFFFFF6B */
#define ERR_INSUFFICIENT_MEMORY 					-150		/* FFFFFF6A */
#define DSERR_NO_ALLOC_SPACE						-150		/* FFFFFF6A */
#define DSERR_TARGET_NOT_A_SUBDIR				-150		/* FFFFFF6A */
#define DSERR_NO_SPOOL_SPACE						-151		/* FFFFFF69 */
#define DSERR_INVALID_VOLUME						-152		/* FFFFFF68 */
#define DSERR_DIRECTORY_FULL						-153		/* FFFFFF67 */
#define DSERR_RENAME_ACROSS_VOLUME				-154		/* FFFFFF66 */
#define DSERR_BAD_DIR_HANDLE						-155		/* FFFFFF65 */
#define DSERR_INVALID_PATH							-156		/* FFFFFF64 */
#define DSERR_NO_SUCH_EXTENSION 					-156		/* FFFFFF64 */
#define DSERR_NO_DIR_HANDLES						-157		/* FFFFFF63 */
#define DSERR_BAD_FILE_NAME 						-158		/* FFFFFF62 */
#define DSERR_DIRECTORY_ACTIVE					-159		/* FFFFFF61 */
#define DSERR_DIRECTORY_NOT_EMPTY				-160		/* FFFFFF60 */
#define DSERR_DIRECTORY_IO_ERROR					-161		/* FFFFFF5F */
#define DSERR_IO_LOCKED 							-162		/* FFFFFF5E */
#define DSERR_TRANSACTION_RESTARTED 			-163		/* FFFFFF5D */
#define DSERR_RENAME_DIR_INVALID					-164		/* FFFFFF5C */
#define DSERR_INVALID_OPENCREATE_MODE			-165		/* FFFFFF5B */
#define DSERR_ALREADY_IN_USE						-166		/* FFFFFF5A */
#define DSERR_INVALID_RESOURCE_TAG				-167		/* FFFFFF59 */
#define DSERR_ACCESS_DENIED 						-168		/* FFFFFF58 */
#define DSERR_LOGIN_SIGNING_REQUIRED			-188		/* FFFFFF44 */
#define DSERR_LOGIN_ENCRYPT_REQUIRED			-189		/* FFFFFF43 */
#define DSERR_INVALID_DATA_STREAM				-190		/* FFFFFF42 */
#define DSERR_INVALID_NAME_SPACE					-191		/* FFFFFF41 */
#define DSERR_NO_ACCOUNTING_PRIVILEGES			-192		/* FFFFFF40 */
#define DSERR_NO_ACCOUNT_BALANCE					-193		/* FFFFFF3F */
#define DSERR_CREDIT_LIMIT_EXCEEDED 			-194		/* FFFFFF3E */
#define DSERR_TOO_MANY_HOLDS						-195		/* FFFFFF3D */
#define DSERR_ACCOUNTING_DISABLED				-196		/* FFFFFF3C */
#define DSERR_LOGIN_LOCKOUT 						-197		/* FFFFFF3B */
#define DSERR_NO_CONSOLE_RIGHTS 					-198		/* FFFFFF3A */
#define DSERR_Q_IO_FAILURE							-208		/* FFFFFF30 */
#define DSERR_NO_QUEUE								-209		/* FFFFFF2F */
#define DSERR_NO_Q_SERVER							-210		/* FFFFFF2E */
#define DSERR_NO_Q_RIGHTS							-211		/* FFFFFF2D */
#define DSERR_Q_FULL									-212		/* FFFFFF2C */
#define DSERR_NO_Q_JOB								-213		/* FFFFFF2B */
#define DSERR_NO_Q_JOB_RIGHTS						-214		/* FFFFFF2A */
#define DSERR_UNENCRYPTED_NOT_ALLOWED			-214		/* FFFFFF2A */
#define DSERR_Q_IN_SERVICE							-215		/* FFFFFF29 */
#define DSERR_DUPLICATE_PASSWORD					-215		/* FFFFFF29 */
#define DSERR_Q_NOT_ACTIVE							-216		/* FFFFFF28 */
#define DSERR_PASSWORD_TOO_SHORT					-216		/* FFFFFF28 */
#define DSERR_Q_STN_NOT_SERVER					-217		/* FFFFFF27 */
#define DSERR_MAXIMUM_LOGINS_EXCEEDED			-217		/* FFFFFF27 */
#define DSERR_Q_HALTED								-218		/* FFFFFF26 */
#define DSERR_BAD_LOGIN_TIME						-218		/* FFFFFF26 */
#define DSERR_Q_MAX_SERVERS 						-219		/* FFFFFF25 */
#define DSERR_NODE_ADDRESS_VIOLATION			-219		/* FFFFFF25 */
#define DSERR_LOG_ACCOUNT_EXPIRED				-220		/* FFFFFF24 */
#define DSERR_BAD_PASSWORD							-222		/* FFFFFF22 */
#define DSERR_PASSWORD_EXPIRED					-223		/* FFFFFF21 */
#define DSERR_NO_LOGIN_CONN_AVAILABLE			-224		/* FFFFFF20 */
#define DSERR_WRITE_TO_GROUP_PROPERTY			-232		/* FFFFFF18 */
#define DSERR_MEMBER_ALREADY_EXISTS 			-233		/* FFFFFF17 */
#define DSERR_NO_SUCH_MEMBER						-234		/* FFFFFF16 */
#define DSERR_PROPERTY_NOT_GROUP					-235		/* FFFFFF15 */
#define DSERR_NO_SUCH_VALUE_SET 					-236		/* FFFFFF14 */
#define DSERR_PROPERTY_ALREADY_EXISTS			-237		/* FFFFFF13 */
#define DSERR_OBJECT_ALREADY_EXISTS 			-238		/* FFFFFF12 */
#define DSERR_ILLEGAL_NAME							-239		/* FFFFFF11 */
#define DSERR_ILLEGAL_WILDCARD					-240		/* FFFFFF10 */
#define DSERR_BINDERY_SECURITY					-241		/* FFFFFF0F */
#define DSERR_NO_OBJECT_READ_RIGHTS 			-242		/* FFFFFF0E */
#define DSERR_NO_OBJECT_RENAME_RIGHTS			-243		/* FFFFFF0D */
#define DSERR_NO_OBJECT_DELETE_RIGHTS			-244		/* FFFFFF0C */
#define DSERR_NO_OBJECT_CREATE_RIGHTS			-245		/* FFFFFF0B */
#define DSERR_NO_PROPERTY_DELETE_RIGHTS 		-246		/* FFFFFF0A */
#define DSERR_NO_PROPERTY_CREATE_RIGHTS 		-247		/* FFFFFF09 */
#define DSERR_NO_PROPERTY_WRITE_RIGHTS			-248		/* FFFFFF08 */
#define DSERR_NO_PROPERTY_READ_RIGHTS			-249		/* FFFFFF07 */
#define DSERR_TEMP_REMAP							-250		/* FFFFFF06 */
#define ERR_REQUEST_UNKNOWN 						-251		/* FFFFFF05 */
#define DSERR_UNKNOWN_REQUEST						-251		/* FFFFFF05 */
#define DSERR_NO_SUCH_PROPERTY					-251		/* FFFFFF05 */
#define DSERR_MESSAGE_QUEUE_FULL					-252		/* FFFFFF04 */
#define DSERR_TARGET_ALREADY_HAS_MSG			-252		/* FFFFFF04 */
#define DSERR_NO_SUCH_OBJECT						-252		/* FFFFFF04 */
#define DSERR_BAD_STATION_NUMBER					-253		/* FFFFFF03 */
#define DSERR_BINDERY_LOCKED						-254		/* FFFFFF02 */
#define DSERR_DIR_LOCKED							-254		/* FFFFFF02 */
#define DSERR_SPOOL_DELETE							-254		/* FFFFFF02 */
#define DSERR_TRUSTEE_NOT_FOUND 					-254		/* FFFFFF02 */
#define DSERR_TIMEOUT			 					-254		/* FFFFFF02 */
#define DSERR_HARD_FAILURE							-255		/* FFFFFF01 */
#define DSERR_FILE_NAME 							-255		/* FFFFFF01 */
#define DSERR_FILE_EXISTS							-255		/* FFFFFF01 */
#define DSERR_CLOSE_FCB 							-255		/* FFFFFF01 */
#define DSERR_IO_BOUND								-255		/* FFFFFF01 */
#define DSERR_NO_SPOOL_FILE 						-255		/* FFFFFF01 */
#define DSERR_BAD_SPOOL_PRINTER 					-255		/* FFFFFF01 */
#define DSERR_BAD_PARAMETER 						-255		/* FFFFFF01 */
#define DSERR_NO_FILES_FOUND						-255		/* FFFFFF01 */
#define DSERR_NO_TRUSTEE_CHANGE_PRIV			-255		/* FFFFFF01 */
#define DSERR_TARGET_NOT_LOGGED_IN				-255		/* FFFFFF01 */
#define DSERR_TARGET_NOT_ACCEPTING_MSGS		-255		/* FFFFFF01 */
#define DSERR_MUST_FORCE_DOWN						-255		/* FFFFFF01 */
#define ERR_OF_SOME_SORT							-255		/* FFFFFF01 */

/* -301 to -399 are returned by the directory services client library */
#define ERR_NOT_ENOUGH_MEMORY						-301		/* 0xFFFFFED3 */
#define ERR_BAD_KEY 									-302		/* 0xFFFFFED2 */
#define ERR_BAD_CONTEXT 							-303		/* 0xFFFFFED1 */
#define ERR_BUFFER_FULL 							-304		/* 0xFFFFFED0 */
#define ERR_LIST_EMPTY								-305		/* 0xFFFFFECF */
#define ERR_BAD_SYNTAX								-306		/* 0xFFFFFECE */
#define ERR_BUFFER_EMPTY							-307		/* 0xFFFFFECD */
#define ERR_BAD_VERB									-308		/* 0xFFFFFECC */
#define ERR_EXPECTED_IDENTIFIER 					-309		/* 0xFFFFFECB */
#define ERR_EXPECTED_EQUALS 						-310		/* 0xFFFFFECA */
#define ERR_ATTR_TYPE_EXPECTED					-311		/* 0xFFFFFEC9 */
#define ERR_ATTR_TYPE_NOT_EXPECTED				-312		/* 0xFFFFFEC8 */
#define ERR_FILTER_TREE_EMPTY						-313		/* 0xFFFFFEC7 */
#define ERR_INVALID_OBJECT_NAME 					-314		/* 0xFFFFFEC6 */
#define ERR_EXPECTED_RDN_DELIMITER				-315		/* 0xFFFFFEC5 */
#define ERR_TOO_MANY_TOKENS 						-316		/* 0xFFFFFEC4 */
#define ERR_INCONSISTENT_MULTIAVA				-317		/* 0xFFFFFEC3 */
#define ERR_COUNTRY_NAME_TOO_LONG				-318		/* 0xFFFFFEC2 */
#define ERR_SYSTEM_ERROR							-319		/* 0xFFFFFEC1 */
#define ERR_CANT_ADD_ROOT							-320		/* 0xFFFFFEC0 */
#define ERR_UNABLE_TO_ATTACH						-321		/* 0xFFFFFEBF */
#define ERR_INVALID_HANDLE							-322		/* 0xFFFFFEBE */
#define ERR_BUFFER_ZERO_LENGTH					-323		/* 0xFFFFFEBD */
#define ERR_INVALID_REPLICA_TYPE					-324		/* 0xFFFFFEBC */
#define ERR_INVALID_ATTR_SYNTAX 					-325		/* 0xFFFFFEBB */
#define ERR_INVALID_FILTER_SYNTAX				-326		/* 0xFFFFFEBA */
#define ERR_CONTEXT_CREATION						-328		/* 0xFFFFFEB8 */
#define ERR_INVALID_UNION_TAG						-329		/* 0xFFFFFEB7 */
#define ERR_INVALID_SERVER_RESPONSE 			-330		/* 0xFFFFFEB6 */
#define ERR_NULL_POINTER							-331		/* 0xFFFFFEB5 */
#define ERR_NO_SERVER_FOUND 						-332		/* 0xFFFFFEB4 */
#define ERR_NO_CONNECTION							-333		/* 0xFFFFFEB3 */
#define ERR_RDN_TOO_LONG							-334		/* 0xFFFFFEB2 */
#define ERR_DUPLICATE_TYPE							-335		/* 0xFFFFFEB1 */
#define ERR_DATA_STORE_FAILURE					-336		/* 0xFFFFFEB0 */
#define ERR_NOT_LOGGED_IN							-337		/* 0xFFFFFEAF */
#define ERR_INVALID_PASSWORD_CHARS				-338		/* 0xFFFFFEAE */
#define ERR_FAILED_SERVER_AUTHENT				-339		/* 0xFFFFFEAD */
#define ERR_TRANSPORT								-340		/* 0xFFFFFEAC */
#define ERR_NO_SUCH_SYNTAX							-341		/* 0xFFFFFEAB */
#define ERR_INVALID_DS_NAME 						-342		/* 0xFFFFFEAA */
#define ERR_ATTR_NAME_TOO_LONG					-343		/* 0xFFFFFEA9 */
#define ERR_INVALID_TDS 							-344		/* 0xFFFFFEA8 */
#define ERR_INVALID_DS_VERSION					-345		/* 0xFFFFFEA7 */
#define ERR_UNICODE_TRANSLATION 					-346		/* 0xFFFFFEA6 */
#define ERR_SCHEMA_NAME_TOO_LONG					-347		/* 0xFFFFFEA5 */
#define ERR_UNICODE_FILE_NOT_FOUND				-348		/* 0xFFFFFEA4 */
#define ERR_UNICODE_ALREADY_LOADED				-349		/* 0xFFFFFEA3 */
#define ERR_NOT_CONTEXT_OWNER						-350		/* 0xFFFFFEA2 */
#define ERR_ATTEMPT_TO_AUTHENTICATE_0			-351		/* 0xFFFFFEA1 */
#define ERR_NO_WRITABLE_REPLICAS					-352		/* 0xFFFFFEA0 */
#define ERR_DN_TOO_LONG 							-353		/* 0xFFFFFE9F */
#define ERR_RENAME_NOT_ALLOWED					-354		/* 0xFFFFFE9E */
#define ERR_NOT_NDS_FOR_NT							-355		/* 0xFFFFFE9D */
#define ERR_NDS_FOR_NT_NO_DOMAIN          	-356		/* 0xFFFFFE9C */
#define ERR_NDS_FOR_NT_SYNC_DISABLED      	-357		/* 0xFFFFFE9B */
#define ERR_ITR_INVALID_HANDLE		      	-358		/* 0xFFFFFE9A */
#define ERR_ITR_INVALID_POSITION		      	-359		/* 0xFFFFFE99 */
#define ERR_ITR_INVALID_SEARCH_DATA	      	-360		/* 0xFFFFFE98 */
#define ERR_ITR_INVALID_SCOPE			      	-361		/* 0xFFFFFE97 */

/* -601 to -799 are returned by the directory services agent in the server */
#define ERR_NO_SUCH_ENTRY							-601		/* 0xFFFFFDA7 */
#define ERR_NO_SUCH_VALUE							-602		/* 0xFFFFFDA6 */
#define ERR_NO_SUCH_ATTRIBUTE						-603		/* 0xFFFFFDA5 */
#define ERR_NO_SUCH_CLASS							-604		/* 0xFFFFFDA4 */
#define ERR_NO_SUCH_PARTITION						-605		/* 0xFFFFFDA3 */
#define ERR_ENTRY_ALREADY_EXISTS					-606		/* 0xFFFFFDA2 */
#define ERR_NOT_EFFECTIVE_CLASS 					-607		/* 0xFFFFFDA1 */
#define ERR_ILLEGAL_ATTRIBUTE						-608		/* 0xFFFFFDA0 */
#define ERR_MISSING_MANDATORY						-609		/* 0xFFFFFD9F */
#define ERR_ILLEGAL_DS_NAME 						-610		/* 0xFFFFFD9E */
#define ERR_ILLEGAL_CONTAINMENT 					-611		/* 0xFFFFFD9D */
#define ERR_CANT_HAVE_MULTIPLE_VALUES			-612		/* 0xFFFFFD9C */
#define ERR_SYNTAX_VIOLATION						-613		/* 0xFFFFFD9B */
#define ERR_DUPLICATE_VALUE 						-614		/* 0xFFFFFD9A */
#define ERR_ATTRIBUTE_ALREADY_EXISTS			-615		/* 0xFFFFFD99 */
#define ERR_MAXIMUM_ENTRIES_EXIST				-616		/* 0xFFFFFD98 */
#define ERR_DATABASE_FORMAT 						-617		/* 0xFFFFFD97 */
#define ERR_INCONSISTENT_DATABASE				-618		/* 0xFFFFFD96 */
#define ERR_INVALID_COMPARISON					-619		/* 0xFFFFFD95 */
#define ERR_COMPARISON_FAILED						-620		/* 0xFFFFFD94 */
#define ERR_TRANSACTIONS_DISABLED				-621		/* 0xFFFFFD93 */
#define ERR_INVALID_TRANSPORT						-622		/* 0xFFFFFD92 */
#define ERR_SYNTAX_INVALID_IN_NAME				-623		/* 0xFFFFFD91 */
#define ERR_REPLICA_ALREADY_EXISTS				-624		/* 0xFFFFFD90 */
#define ERR_TRANSPORT_FAILURE						-625		/* 0xFFFFFD8F */
#define ERR_ALL_REFERRALS_FAILED					-626		/* 0xFFFFFD8E */
#define ERR_CANT_REMOVE_NAMING_VALUE			-627		/* 0xFFFFFD8D */
#define ERR_OBJECT_CLASS_VIOLATION				-628		/* 0xFFFFFD8C */
#define ERR_ENTRY_IS_NOT_LEAF						-629		/* 0xFFFFFD8B */
#define ERR_DIFFERENT_TREE							-630		/* 0xFFFFFD8A */
#define ERR_ILLEGAL_REPLICA_TYPE					-631		/* 0xFFFFFD89 */
#define ERR_SYSTEM_FAILURE							-632		/* 0xFFFFFD88 */
#define ERR_INVALID_ENTRY_FOR_ROOT				-633		/* 0xFFFFFD87 */
#define ERR_NO_REFERRALS							-634		/* 0xFFFFFD86 */
#define ERR_REMOTE_FAILURE							-635		/* 0xFFFFFD85 */
#define ERR_UNREACHABLE_SERVER					-636		/* 0XFFFFFD84 */
#define ERR_PREVIOUS_MOVE_IN_PROGRESS			-637		/* 0XFFFFFD83 */
#define ERR_NO_CHARACTER_MAPPING					-638		/* 0XFFFFFD82 */
#define ERR_INCOMPLETE_AUTHENTICATION			-639		/* 0XFFFFFD81 */
#define ERR_INVALID_CERTIFICATE 					-640		/* 0xFFFFFD80 */
#define ERR_INVALID_REQUEST 						-641		/* 0xFFFFFD7F */
#define ERR_INVALID_ITERATION						-642		/* 0xFFFFFD7E */
#define ERR_SCHEMA_IS_NONREMOVABLE				-643		/* 0xFFFFFD7D */
#define ERR_SCHEMA_IS_IN_USE						-644		/* 0xFFFFFD7C */
#define ERR_CLASS_ALREADY_EXISTS					-645		/* 0xFFFFFD7B */
#define ERR_BAD_NAMING_ATTRIBUTES				-646		/* 0xFFFFFD7A */
#define ERR_NOT_ROOT_PARTITION					-647		/* 0xFFFFFD79 */
#define ERR_INSUFFICIENT_STACK					-648		/* 0xFFFFFD78 */
#define ERR_INSUFFICIENT_BUFFER 					-649		/* 0xFFFFFD77 */
#define ERR_AMBIGUOUS_CONTAINMENT				-650		/* 0xFFFFFD76 */
#define ERR_AMBIGUOUS_NAMING						-651		/* 0xFFFFFD75 */
#define ERR_DUPLICATE_MANDATORY 					-652		/* 0xFFFFFD74 */
#define ERR_DUPLICATE_OPTIONAL					-653		/* 0xFFFFFD73 */
#define ERR_PARTITION_BUSY							-654		/* 0XFFFFFD72 */
#define ERR_MULTIPLE_REPLICAS						-655		/* 0xFFFFFD71 */
#define ERR_CRUCIAL_REPLICA 						-656		/* 0xFFFFFD70 */
#define ERR_SCHEMA_SYNC_IN_PROGRESS 			-657		/* 0xFFFFFD6F */
#define ERR_SKULK_IN_PROGRESS						-658		/* 0xFFFFFD6E */
#define ERR_TIME_NOT_SYNCHRONIZED				-659		/* 0xFFFFFD6D */
#define ERR_RECORD_IN_USE							-660		/* 0xFFFFFD6C */
#define ERR_DS_VOLUME_NOT_MOUNTED				-661		/* 0xFFFFFD6B */
#define ERR_DS_VOLUME_IO_FAILURE					-662		/* 0xFFFFFD6A */
#define ERR_DS_LOCKED								-663		/* 0xFFFFFD69 */
#define ERR_OLD_EPOCH								-664		/* 0xFFFFFD68 */
#define ERR_NEW_EPOCH								-665		/* 0xFFFFFD67 */
#define ERR_INCOMPATIBLE_DS_VERSION 			-666		/* 0xFFFFFD66 */
#define ERR_PARTITION_ROOT							-667		/* 0xFFFFFD65 */
#define ERR_ENTRY_NOT_CONTAINER 					-668		/* 0xFFFFFD64 */
#define ERR_FAILED_AUTHENTICATION				-669		/* 0xFFFFFD63 */
#define ERR_INVALID_CONTEXT 						-670		/* 0xFFFFFD62 */
#define ERR_NO_SUCH_PARENT							-671		/* 0xFFFFFD61 */
#define ERR_NO_ACCESS								-672		/* 0xFFFFFD60 */
#define ERR_REPLICA_NOT_ON							-673		/* 0xFFFFFD5F */
#define ERR_INVALID_NAME_SERVICE					-674		/* 0xFFFFFD5E */
#define ERR_INVALID_TASK							-675		/* 0xFFFFFD5D */
#define ERR_INVALID_CONN_HANDLE 					-676		/* 0xFFFFFD5C */
#define ERR_INVALID_IDENTITY						-677		/* 0xFFFFFD5B */
#define ERR_DUPLICATE_ACL							-678		/* 0xFFFFFD5A */
#define ERR_PARTITION_ALREADY_EXISTS			-679		/* 0xFFFFFD59 */
#define ERR_TRANSPORT_MODIFIED					-680		/* 0xFFFFFD58 */
#define ERR_ALIAS_OF_AN_ALIAS						-681		/* 0xFFFFFD57 */
#define ERR_AUDITING_FAILED 						-682		/* 0xFFFFFD56 */
#define ERR_INVALID_API_VERSION 					-683		/* 0xFFFFFD55 */
#define ERR_SECURE_NCP_VIOLATION					-684		/* 0xFFFFFD54 */
#define ERR_MOVE_IN_PROGRESS						-685		/* 0xFFFFFD53 */
#define ERR_NOT_LEAF_PARTITION					-686		/* 0xFFFFFD52 */
#define ERR_CANNOT_ABORT							-687		/* 0xFFFFFD51 */
#define ERR_CACHE_OVERFLOW							-688		/* 0xFFFFFD50 */
#define ERR_INVALID_SUBORDINATE_COUNT			-689		/* 0xFFFFFD4F */
#define ERR_INVALID_RDN 							-690		/* 0xFFFFFD4E */
#define ERR_MOD_TIME_NOT_CURRENT					-691		/* 0xFFFFFD4D */
#define ERR_INCORRECT_BASE_CLASS					-692		/* 0xFFFFFD4C */
#define ERR_MISSING_REFERENCE						-693		/* 0xFFFFFD4B */
#define ERR_LOST_ENTRY								-694		/* 0xFFFFFD4A */
#define ERR_AGENT_ALREADY_REGISTERED			-695		/* 0xFFFFFD49 */
#define ERR_DS_LOADER_BUSY							-696		/* 0xFFFFFD48 */
#define ERR_DS_CANNOT_RELOAD						-697		/* 0xFFFFFD47 */
#define ERR_REPLICA_IN_SKULK						-698		/* 0xFFFFFD46 */
#define ERR_FATAL										-699		/* 0xFFFFFD45 */
#define ERR_OBSOLETE_API							-700		/* 0xFFFFFD44 */
#define ERR_SYNCHRONIZATION_DISABLED			-701		/* 0xFFFFFD43 */
#define ERR_INVALID_PARAMETER						-702		/* 0xFFFFFD42 */
#define ERR_DUPLICATE_TEMPLATE					-703		/* 0xFFFFFD41 */
#define ERR_NO_MASTER_REPLICA						-704		/* 0xFFFFFD40 */
#define ERR_DUPLICATE_CONTAINMENT				-705		/* 0xFFFFFD3F */
#define ERR_NOT_SIBLING 							-706		/* 0xFFFFFD3E */
#define ERR_INVALID_SIGNATURE						-707		/* 0xFFFFFD3D */
#define ERR_INVALID_RESPONSE						-708		/* 0xFFFFFD3C */
#define ERR_INSUFFICIENT_SOCKETS					-709		/* 0xFFFFFD3B */
#define ERR_DATABASE_READ_FAIL					-710		/* 0xFFFFFD3A */
#define ERR_INVALID_CODE_PAGE						-711		/* 0xFFFFFD39 */
#define ERR_INVALID_ESCAPE_CHAR 					-712		/* 0xFFFFFD38 */
#define ERR_INVALID_DELIMITERS					-713		/* 0xFFFFFD37 */
#define ERR_NOT_IMPLEMENTED 						-714		/* 0xFFFFFD36 */
#define ERR_CHECKSUM_FAILURE						-715		/* 0xFFFFFD35 */
#define ERR_CHECKSUMMING_NOT_SUPPORTED			-716		/* 0xFFFFFD34 */
#define ERR_CRC_FAILURE 							-717		/* 0xFFFFFD33 */
#define ERR_INVALID_ENTRY_HANDLE					-718		/* 0xFFFFFD32 */
#define ERR_INVALID_VALUE_HANDLE					-719		/* 0xFFFFFD31 */
#define ERR_CONNECTION_DENIED						-720		/* 0xFFFFFD30 */
#define ERR_NO_SUCH_FEDERATION_LINK 			-721		/* 0xFFFFFD2F */
#define ERR_OP_SCHEMA_MISMATCH 					-722		/* 0xFFFFFD2E */
#define ERR_STREAM_NOT_FOUND						-723		/* 0xFFFFFD2D */
#define ERR_DCLIENT_UNAVAILABLE 					-724		/* 0xFFFFFD2C */
#define ERR_MASV_NO_ACCESS							-725		/* 0xFFFFFD2B */
#define ERR_MASV_INVALID_REQUEST					-726		/* 0xFFFFFD2A */
#define ERR_MASV_FAILURE							-727		/* 0xFFFFFD29 */
#define ERR_MASV_ALREADY_EXISTS					-728		/* 0xFFFFFD28 */
#define ERR_MASV_NOT_FOUND							-729		/* 0xFFFFFD27 */
#define ERR_MASV_BAD_RANGE							-730		/* 0xFFFFFD26 */
#define ERR_VALUE_DATA								-731		/* 0xFFFFFD25 */
#define ERR_DATABASE_LOCKED						-732		/* 0xFFFFFD24 */
#define ERR_DATABASE_ALREADY_EXISTS				-733		/* 0xFFFFFD23 */
#define ERR_DATABASE_NOT_FOUND					-734		/* 0xFFFFFD22 */
#define ERR_NOTHING_TO_ABORT						-735		/* 0xFFFFFD21 */
#define ERR_END_OF_STREAM							-736		/* 0xFFFFFD20 */
#define ERR_NO_SUCH_TEMPLATE						-737		/* 0xFFFFFD1F */
#define ERR_SAS_LOCKED								-738		/* 0xFFFFFD1E */
#define ERR_INVALID_SAS_VERSION					-739		/* 0xFFFFFD1D */
#define ERR_SAS_ALREADY_REGISTERED				-740		/* 0xFFFFFD1C */
#define ERR_NAME_TYPE_NOT_SUPPORTED				-741		/* 0xFFFFFD1B */
#define ERR_WRONG_DS_VERSION						-742		/* 0xFFFFFD1A */
#define ERR_INVALID_CONTROL_FUNCTION			-743		/* 0xFFFFFD19 */
#define ERR_INVALID_CONTROL_STATE				-744		/* 0xFFFFFD18 */
#define ERR_CACHE_IN_USE							-745		/* 0xFFFFFD17 */
#define ERR_ZERO_CREATION_TIME					-746		/* 0xFFFFFD16 */
#define ERR_WOULD_BLOCK								-747		/* 0xFFFFFD15 */
#define ERR_CONN_TIMEOUT							-748		/* 0xFFFFFD14 */
#define ERR_TOO_MANY_REFERRALS					-749		/* 0xFFFFFD13 */
#define ERR_OPERATION_CANCELLED					-750		/* 0xFFFFFD12 */
#define ERR_UNKNOWN_TARGET							-751		/* 0xFFFFFD11 */
#define ERR_GUID_FAILURE							-752		/* 0xFFFFFD10 */
#define ERR_INCOMPATIBLE_OS						-753		/* 0xFFFFFD0F */
#define ERR_CALLBACK_CANCEL						-754		/* 0xFFFFFD0E */
#define ERR_INVALID_SYNC_DATA						-755		/* 0xFFFFFD0D */
#define ERR_STREAM_EXISTS							-756		/* 0xFFFFFD0C */
#define ERR_AUXILIARY_HAS_CONTAINMENT			-757		/* 0xFFFFFD0B */
#define ERR_AUXILIARY_NOT_CONTAINER				-758		/* 0xFFFFFD0A */
#define ERR_AUXILIARY_NOT_EFFECTIVE				-759		/* 0XFFFFFD09 */
#define ERR_AUXILIARY_ON_ALIAS					-760		/* 0xFFFFFD08 */
#define ERR_HAVE_SEEN_STATE						-761		/* 0xFFFFFD07 */
#define ERR_VERB_LOCKED								-762		/* 0xFFFFFD06 */
#define ERR_VERB_EXCEEDS_TABLE_LENGTH			-763		/* 0xFFFFFD05 */
#define ERR_BOF_HIT									-764		/* 0xFFFFFD04 */
#define ERR_EOF_HIT									-765		/* 0xFFFFFD03 */
#define ERR_INCOMPATIBLE_REPLICA_VER			-766		/* 0xFFFFFD02 */
#define ERR_QUERY_TIMEOUT							-767		/* 0xFFFFFD01 */
#define ERR_QUERY_MAX_COUNT						-768		/* 0xFFFFFD00 */
#define ERR_DUPLICATE_NAMING						-769		/* 0xFFFFFCFF */
#define ERR_NO_TRANS_ACTIVE						-770		/* 0xFFFFFCFE */
#define ERR_TRANS_ACTIVE							-771		/* 0xFFFFFCFD */
#define ERR_ILLEGAL_TRANS_OP						-772		/* 0xFFFFFCFC */
#define ERR_ITERATOR_SYNTAX						-773		/* 0xFFFFFCFB */
#define ERR_REPAIRING_DIB							-774     /* 0xFFFFFCFA */

/* NOTE: In order to accommodate older compilers, do not introduce a new
 * error code name longer than 31 characters.
 */

/*===========================================================================*/
#endif

