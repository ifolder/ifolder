/****************************************************************************
 |
 |	(C) Copyright 1985, 1991, 1993, 1996-1999 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Novell Storage Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   03 Aug 2001 17:12:40  $
 |
 | $Workfile:   zError.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		This is in the NSS SDK.
 |		This contains ALL error codes for NSS.
 +-------------------------------------------------------------------------*/
#ifndef _ZERROR_H_
#define _ZERROR_H_

#ifdef __cplusplus
extern "C" {
#endif

/*-------------------------------------------------------------------------
 *	Defined error range for NSS
 *-------------------------------------------------------------------------*/
#define ERR_NSS_FIRST_ERROR	   			20000
#define	ERR_NSS_LAST_ERROR	   			24999


/*=========================================================================
 *=========================================================================
 *	Common Layer Errors (Range 20000-24999)
 *=========================================================================
 *=========================================================================*/
/* general errors */
#define zERR_NO_MEMORY						20000 /* insufficent memory to complete the request */
#define zERR_BAD_CONNECTION_ID				20001 /* bad connection id */
#define zERR_NOT_CONNECTED					20002 /* station does not have a connection */
#define zERR_XID_NOT_SUPPORTED				20003 /* reserved xid != NULL */
#define zERR_BUFFER_TOO_SMALL				20004 /* the given buffer is too small */
#define zERR_RETURN_PARA_NULL				20005 /* if return parameter is null */
#define zERR_QUAD_TOO_BIG_FOR_LONG			20006 /* the upper 32 bits of a 64 bit number are not zero */
#define zERR_CONNECTION_NOT_LOGGED_IN		20007 /* the connection has not been logged in */
#define zERR_BAD_PARAMETER_VALUE			20008 /* a passed in parameter was invalid */
#define zERR_INVALID_SEMANTIC_AGENT_ID		20009 /* a bad semantic agent ID was given */
#define zERR_INVALID_STATE					20010 /* an invalid state was requested */
#define zERR_NOT_SUPPORTED					20011 /* the operation is not supported */
#define	zERR_MEDIA_CORRUPTED				20012 /* The media is corrupted */
#define	zERR_TIMEOUT						20015 /* an event didn't occur befor a timer popped */
#define zERR_EXCEEDED_MAX_ALERTS			20016 /* exceeded the maximum number of outstanding ALERTS */
#define	zERR_USER_ABORTED					20017 /* User requested action to stop */
#define zERR_BAD_ADDRESS					20018 /* A bad user space (ring 3) address */

/* Message, user transaction, and task errors */

#define zERR_BAD_KEY						20051 /* key couldn't be found */
#define zERR_BAD_METHOD						20052 /* method number out of range */
#define zERR_BROKEN_DOOR					20053 /* key is still valid but object doesn't exist anymore */
#define zERR_NO_DUP							20054 /* can't duplicate key */
#define zERR_NO_METHOD						20055 /* no method for this number */
#define zERR_BROKEN_OBJECT					20056 /* object already broken */

#define zERR_NO_TASK						20060 /* Couldn't find task specified by user */
#define zERR_TASK_EXISTS					20061 /* This task already exits */
#define zERR_NO_XACTION						20062 /* User Xaction ID doesn't exist */

#define zERR_FINISHED_WITH_EXTENTS			20070 /* Finished processing number of extents passed in by user */

/* NDS errors */
#define zERR_OBJECT_NOT_FOUND				20090 /* object not found in NDS */
#define zERR_UNABLE_TO_IMPORT_NDS_PUBLICS	20091 /* problem importing the NDS publics */
#define zERR_UNABLE_TO_GET_LONG_NAME		20092 /* error getting the distinguished name from NDS */
#define zERR_GUID_NOT_FOUND					20093 /* looking up by GUID did not succeed */

/* General File System errors*/
#define zERR_END_OF_FILE					20100 /* read past the end of file*/
#define zERR_HARD_READ_ERROR				20101 /* read error from media*/
#define zERR_HARD_WRITE_ERROR				20102 /* write error from media*/
#define zERR_OUT_OF_SPACE					20103 /* no available disk space is left*/
#define zERR_PURGED_SPACE_UNAVAILABLE		20104 /* there is purgeable space, it is just not free yet*/
#define zERR_FILE_TOO_LARGE					20105 /* the file is too large for the given POOL*/
#define zERR_INVALID_BLOCK					20106 /* requested a read on an invalid block*/

/* Virtual File System errors */
#define zERR_BAD_TRANSFORMATION				20150 /* bad transformation in a virtual file */
#define zERR_SYMBOL_NAME_TOO_LONG			20151 /* the symbol name in a virtual file is too long */
#define zERR_SYMBOL_NOT_DEFINED				20152 /* the symbol for a virtual request is not defined */
#define zERR_XML_TOO_LONG					20153 /* a generated XML string is too large */
#define zERR_DATASTREAM_NOT_FOUND			20154 /* a searched for datastream was found in a virtual file */
#define zERR_BAD_FUNCTION_PTR				20155 /* a virtual file has a bad function pointer */
#define zERR_BAD_FORMAT						20156 /* a virtual file has a bad format type */
#define zERR_BAD_OFFSET						20157 /* the passed in offset for a read or write is not valid */
#define zERR_NO_FUNCTION_DEFINED			20158 /* no function is present where one is needed */
#define zERR_SYMBOL_NAME_MISSING			20159 /* the symbol name in a virtual file is missing */

/* General Storage System Errors */
#define zERR_VOLUME_ALREADY_INITIALIZED		20200 /* attem to initialize a volume that is already setup*/
#define zERR_QUEUE_READ_FAILURE				20201 /* was unable to que a read request */
#define zERR_QUEUE_WRITE_FAILURE			20202 /* was unable to que a WRITE request */
#define zERR_READ_FAILURE					20203 /* the low level async block read failed*/
#define zERR_WRITE_FAILURE					20204 /* the low level async block WRITE failed*/
#define zERR_VOLUME_DISABLING				20205 /* Volume is being disabled(I/O not allowed) */
#define zERR_POOL_DISABLING					20206 /* Pool is being disabled(I/O not allowed) */
#define zERR_POOL_NOT_ACCESSIBLE			20207 /* Pool is not accessible(I/O not allowed) */
#define	zERR_READ_FAILURE_UNKNOWN			20208 /* Unknown read error */
#define	zERR_READ_FAILURE_MEDIA				20209 /* Media read error */
#define	zERR_READ_FAILURE_POSTPONE			20210 /* Postpone read error */
#define	zERR_WRITE_FAILURE_UNKNOWN			20211 /* Unknown write error */
#define	zERR_WRITE_FAILURE_MEDIA	  		20212 /* Media write error */
#define	zERR_WRITE_FAILURE_POSTPONE			20213 /* Postpone write error */

/* Admin Volume Errors */
#define zERR_NO_ADMIN_VOLUME				20250 /* No admin volume exists */
#define zERR_NO_PERSIST_ADMIN_VOLUME		20251 /* No persistent admin volume exists */
#define zERR_UNABLE_TO_INIT_ADMIN_VOL		20252 /* could not start NSS_ADMIN volume */

/* Beast Specific errors */
#define zERR_INVALID_BEAST_ID				20300 /* and invalid beast ID was given*/
#define zERR_BEAST_CLASS_ALREADY_DEFINED	20301 /* the given beast class ID is already in use*/
#define zERR_BEAST_CLASS_NOT_DEFINED		20302 /* the given beast class ID is not defined*/
#define zERR_BEAST_CLASS_ROUTINE_NOT_DEF	20303 /* a required beast class routine is missing*/
#define zERR_BEAST_CLASS_ROUTINE_MULT_DEF	20304 /* an beast class routine is multiply defined*/
#define zERR_COMN_OP_NOT_SUPPORTED			20305 /* the given COMN operation (beast or volume) is not supported*/
#define zERR_INHERITANCE_DEPTH_EXCEEDED		20306 /* the maximum inheritance depth has been exceeded*/
#define zERR_BEAST_SIZE_CHANGED				20307 /* a beast's size changed when it shouldn't have*/
#define zERR_BAD_LENGTH_UNPACKING_BEAST		20308 /* the system detected an inconsistent length while unpacking a beast*/
#define zERR_UNSUPPORTED_OBJECT_LAYOUT		20309 /* the object's layout on disk is in an unsupported format */
#define	zERR_BEAST_CORRUPTED				20310 /* Beast is corrupted.  This is the only error ZLSS's rebuild will delete
												   * the beast.  Note rebuilds should repair the beast if possible, but if
												   * rebuild can not then it can delete the beast.
												   */
#define	zERR_FILE_IN_USE					20311 /* Beast has a use count > 0 */

/* Naming errors */
#define zERR_INVALID_HANDLE_PATH			20400 /* path handle type is not valid */
#define zERR_BAD_FILE_HANDLE				20401 /* the file handle is out of range, bad instance, or doesn't exist */
#define zERR_BAD_CONTEXT_HANDLE	 			20402 /* invalid context for context handle */
#define zERR_INVALID_NAME					20403 /* path name is invalid -- bad syntax */
#define zERR_INVALID_CHAR_IN_NAME			20404 /* path name had an invalid character */
#define zERR_INVALID_PATH					20405 /* the path is syntactically incorrect */
#define zERR_RESERVED_NAME					20406 /* name is reserved, can not be used in currently request */
#define zERR_NAME_NOT_FOUND_IN_DIRECTORY	20407 /* name does not exist in the direcory being searched */
#define zERR_NOT_DIRECTORY_FILE				20408 /* found name but it referred to something that was not a directory */
#define zERR_NO_NAMES_IN_PATH				20409 /* a NULL file name was given*/
#define zERR_NO_MORE_NAMES_IN_PATH 			20410 /* doing a wild search but ran out of names to search */
#define zERR_PATH_MUST_BE_FULLY_QUALIFIED	20411 /* path name must be fully qualified in this context */
#define zERR_FILE_ALREADY_EXISTS			20412 /* the given file already exists*/
#define zERR_NAME_NO_LONGER_VALID			20413 /* the dir/file name is no longer valid*/
#define zERR_BAD_SEARCHMAP_ID				20414 /* searchMapID is invalid*/
#define zERR_INVALID_TYPE_FILE				20415 /* the file type is wrong for the requested operation*/
#define zERR_INVALID_FILE_TYPE				20416 /* an invalid file type was specified*/
#define zERR_DIRECTORY_NOT_EMPTY			20417 /* the directory still has files in it*/
#define zERR_BAD_SEARCH_OPTIONS				20418 /* an invalid search option was specified*/
#define zERR_INVALID_SEARCH_SEQ_NUM			20419 /* an invalid search sequence number was given*/
#define zERR_INTERNAL_DIRECTORY_ERROR		20420 /* an internal error has occured accessing a directory*/
#define zERR_INVALID_MODIFY_PARAMETER		20421 /* there is an invalid parameter to modify info*/
#define zERR_INVALID_USER_ID				20422 /* the user ID is not valid*/
#define zERR_NOTHING_CHANGED				20423 /* nothing changed on a modify call*/
#define zERR_NO_FILES_FOUND					20424 /* no files matched the given wildcard pattern*/
#define zERR_UNABLE_TO_RETURN_INFO			20425 /* the build info routines could not complete*/
#define zERR_FILE_DID_NOT_MATCH_ATTR		20426 /* the file did not match the matchFileAttrSet/Clear criteria*/
#define zERR_FILE_DID_NOT_MATCH_TYPE		20427 /* the file did not match the matchFileType criteria*/
#define zERR_FILE_DID_NOT_MATCH_TYPEATTR	20428 /* the file did not match the matchTypeAttrSet/Clear criteria*/
#define zERR_LINK_IN_PATH					20429 /* A link object was found as a component in a path */
#define zERR_LINK_IN_DEST_PATH				20430 /* A link object was found as a component in a destination path */
#define zERR_UNABLE_TO_OPEN_BEAST			20431 /* unable to open a beast*/
#define zERR_NSPACE_NAME_ALREADY_DEFINED	20432 /* a name for the given namespace is already defined*/
#define zERR_NAME_NOT_FOUND_IN_BEAST		20433 /* the requested name was not found in the beast*/
#define zERR_PARENT_NOT_FOUND_IN_BEAST		20434 /* the requested parent was not found in the beast*/
#define zERR_DIR_CANNOT_BE_OPENED			20435 /* the requested parent was not found in the beast*/
#define zERR_INVALID_CONTEXT_HANDLE_TYPE	20436 /* the context handle type is invalid */
#define zERR_CONTAINER_NOT_FILE_BEAST		20437 /* The container for a beast must be a File_s beast */
#define zERR_NO_OPEN_PRIVILEGE				20438 /* No the right privileges to open the file */
#define zERR_NO_MORE_CONTEXT_HANDLE_IDS		20439 /* There are no more available context handle IDs */
#define zERR_PREV_DIR_AFTER_DATASTREAM		20440 /* PrevDir not allowed after data stream is processed */
#define zERR_INVALID_PATH_FORMAT			20441 /* The pathFormat is either invalid or unsupported */
#define zERR_CANT_WILDOPEN_A_DATASTREAM		20442 /* It is illegal to do a zWildOpen call on a datastream */
#define zERR_NAMING_INCONSISTENCY			20443 /* An internal naming inconsistency has occurred.  The volume needs recovery */
#define zERR_ZID_NOT_FOUND					20444 /* Zid not found in the directory */
#define zERR_LAST_STATE_UNKNOWN				20445 /* The last consistent state of this file was that it did not exist */
#define zERR_BAD_PATH_FORMAT				20446 /* Path format specification is incorrect */

/* name type errors */
#define zERR_INVALID_NAME_TYPE				20499 /* an invalid name type was specified*/

/* rename errors*/
#define zERR_ALL_FILES_IN_USE				20500 /* all files were in use*/
#define zERR_SOME_FILES_IN_USE				20501 /* some of the files were in use*/
#define zERR_ALL_FILES_READ_ONLY			20502 /* all files were READONLY*/
#define zERR_SOME_FILES_READ_ONLY			20503 /* some of the files were READONLY*/
#define zERR_ALL_NAMES_EXIST				20504 /* all of the names already existed*/
#define zERR_SOME_NAMES_EXIST				20505 /* some of the names already existed*/
#define zERR_NO_RENAME_PRIVILEGE			20506 /* you do not have privilege to rename the file*/
#define zERR_RENAME_DIR_INVALID				20507 /* the selected directory may not be renamed */
#define zERR_RENAME_TO_OTHER_VOLUME			20508 /* a rename/move may not move the beast to a different volume */
#define zERR_CANT_RENAME_DATA_STREAMS		20509 /* not allowed to rename a data stream */
#define zERR_FILE_RENAME_IN_PROGRESS		20510 /* the file is already being renamed by a different process */
#define zERR_CANT_RENAME_TO_DELETED			20511 /* only deleted files may be renamed to a deleted state */
#define zERR_RENAME_TO_OTHER_NAMESPACE		20512 /* a file may not be renamed from one name space to another */

/* Data Stream errors */
#define zERR_INVALID_DATA_STREAM			20550 /* the data stream is invalid */
#define zERR_CANT_MOD_DATA_STREAM_METADATA	20551 /* data stream's metadata may not be modified */

/* Semantic Agent handle errors */			
#define zERR_INVALID_SA_HANDLE			    20601 /* invalid semantic agent handle */
#define zERR_SA_HANDLE_TOO_SMALL			20602 /* An attempt was made to allocate an SA Handle that was too small */

/* DFS/DIO (Direct FS I/O) errors       */
#define zERR_FILE_NOT_IN_DIO_MODE           20650 /* file was not switched to DIO mode */
#define zERR_HOLE_IN_DIO_FILE  	            20651 /* DIO files cannot have holes*/
#define zERR_BEYOND_EOF  	            	20652 /* DIO files cannot be read beyond EOF*/
#define zERR_FILE_IN_DIO_MODE              	20653 /* DIO file is in DIO mode*/
#define zERR_FILE_DETACHED              	20654 /* DIO file is in DIO mode*/
#define zERR_DIO_BAD_PARAMETER             	20655 /* DIO bad parameter(unit count is zero) */


/* name space errors */
#define zERR_INVALID_NAMESPACE_ID			20700 /* an invalid NAMESPACEID was specified*/
#define zERR_UNABLE_TO_FIND_NAMESPACE		20701 /* the code for the given namespace could not be located*/
#define zERR_INVALID_NAMESPACE_VERSION		20702 /* the name space version number is bad*/
#define zERR_NAMESPACE_ID_IN_USE			20703 /* the given name space ID is already in use*/
#define zERR_INVALID_PATH_SEPARATOR			20704 /* The name space does not support the requested path separator type */
#define zERR_VOLUME_SEPARATOR_NOT_SUPPORTED	20705 /* The name space does not support volume separators */

/* AsyncIO errors */
#define zERR_BAD_ASYNCIO_HANDLE				20750 /* The AsyncIOHandle ID was invalid */
#define zERR_ASYNCIO_CANCELED				20751 /* The Async IO was canceled */

/* volume and pool errors */
#define zERR_BAD_VOLUME_NAME   				20800 /* the given volume name is syntactically incorrect */
#define zERR_VOLUME_NOT_FOUND  				20801 /* the given volume name could not be found */
#define zERR_DEACTIVATING_ADMINVOL			20802 /* can not deactivate the NSS_ADMIN volume */
#define zERR_VOLUME_STATE_CHANGE_ABORTED 	20803 /* had to abort the volume state change */
#define zERR_DATA_MIGRATION_NOT_ENABLED     20804 /* NSS does not support data migration */
#define	zERR_VOLUME_STATE_CHANGE_A_TO_M		20805 /* Set by LSS if an attempt to go to ACTIVE state was
												   * not completed because the volume should be placed into
												   * MAINTENANCE state. */
#define zERR_VOLUME_NOT_IN_MAINT_MODE		20806 /* the given volume is not in MAINTANENCE mode */
#define	zERR_VOLUME_STATE_NOT_SUPPORTED		20807 /* The volume does not support the state
												   * change requested.
												   */
#define zERR_DUPLICATE_VOLUME_NAME			20808 /* The volume name already exists */
#define zERR_VOLUME_SCHEDULED_FOR_MAINT		20809 /* The volume is already scheduled for MAINTANENCE */
#define zERR_VOLUME_SHOULD_NOT_ACTIVATE		20810 /* Volume should not be activated (LSS can return if corrupt or rebuilding) */
#define zERR_VOLUME_NOT_IN_ACTIVE_STATE		20811 /* the given volume is not in ACTIVE state */
#define zERR_POOL_NOT_FOUND  				20812 /* the given pool name could not be found */
#define zERR_POOL_STATE_INCOMPATIBLE	   	20813 /* A Volume change STATE has failed becuase the volume's pool is
												   * not in a compatible state.  For example, if a pool is DEACTIVE
												   * and the volume wants to be ACTIVE this error would be returned.
												   * To fix the problem the POOL must first be placed in an acceptable
												   * STATE.
												   */
#define zERR_RESERVED_VOLUME_NAME			20814 /* The given volume name is a reserved name */
#define zERR_BAD_VOLUME_NAME_CHARACTER		20815 /* Volume name contains invalid character */
#define zERR_BAD_VOLUME_NAME_SIZE_LONG		20816 /* Volume name is too long */
#define zERR_BAD_VOLUME_NAME_SIZE_SHORT		20817 /* Volume name is too short */
#define zERR_BAD_VOLUME_NAME_UNDERSCORE		20818 /* Volume name can not start or end with an underscore */
#define zERR_BAD_VOLUME_NAME_TWO_UNDERSCORES 20819/* Volume name can not have two consective underscores */
#define zERR_DUPLICATE_VOLUME_ID			20820 /* The volume ID already exists */
#define zERR_INVALID_VOLUME_ID				20821 /* The volume ID is invalid */
#define zERR_VOLUME_NOT_IN_DEACTIVE_STATE 	20822 /* The given volume is not in DEACTIVE state */
#define zERR_VOLUME_DELETION_MODE	 		20823 /* The given volume is currently being deleted */
#define zERR_VOLUME_CREATION_MODE		 	20824 /* The given volume is currently being created */
#define zERR_VOLUME_INVALID_MODE		 	20825 /* The given volume is in a unknown mode */
#define zERR_VOLUME_STATE_CHANGE_REQUESTED	20826 /* The volume that an operation is running
												   * on is switching state.  For example, when a
												   * LV is being deleted the thread that does the
												   * deletion will return this error if the thread
												   * detects that the volume is changing state.
												   */
#define zERR_VOLUME_STOP_REQUESTED			20827 /* The volume that an operation is running
												   * on has detected that it must stop.
												   */
#define	zERR_VOLUME_ALREADY_UNLOADING		20828 /* The volume is already being unloaded
												   */
#define	zERR_VOLUME_RENAME_NOT_ALLOWED		20829 /* The volume can not be renamed.  For example,
												   * _ADMIN can not be renamed.
												   */
#define zERR_VOLUME_ACTIVE_ELSEWHERE		20830 /* The volume is active on another server in the cluster.
												   * This is a 5.1 error 6Pack should use zERR_NWCS_VOLUME_IS_ACTIVE. */
#define zERR_VOLUME_READ_ONLY				20831 /* Update operation failed on a read-only volume */
#define zERR_VOLUME_BUSY_WITH_REQUEST		20832 /* Operation cannot be completed because of a competing request */
#define zERR_POOL_SHARED_NO_BROKER			20833 /* The pool is marked SHARED but no cluster/broker software
												   * is loaded.  It is unsafe to do operation because
												   * pool may be in ACTIVE or MAITENANCE state on
												   * another server.
												   */
#define zERR_POOL_SHARED_STATE_UNKNOWN		20834 /* Unable to detect if the pool is marked SHARED and no cluster/broker
												   * software is loaded.  It is unsafe to do operation because pool
												   * may be in ACTIVE or MAITENANCE state on another server.
												   */
#define zERR_DUPLICATE_POOL_NAME			20835 /* The pool name already exists on the server */
#define zERR_POOL_NOT_IN_ACTIVE_STATE		20836 /* The given pool is not in ACTIVE state */
#define zERR_POOL_RESERVATION_FAILED		20837 /* When going activate, the pool failed to get a reservation
														* within the MAL. */
 

/** DSI and adding volume to NDS errors **/
#define	zERR_IMPORT_DSI_SYMBOL_FAILED		20840
#define	zERR_DSI_LOAD_FAILED				20841
#define	zERR_DSIREG_RET2					20842
#define zERR_DS_NOT_SETUP					20843
#define	zERR_DSIREG_FAILED					20844
#define	zERR_DSI_LOGIN_FAILED				20845
#define	zERR_ADD_TO_NDS_FAILED				20846
#define	zERR_DEL_TO_NDS_FAILED				20847
#define	zERR_REN_TO_NDS_FAILED				20848
#define	zERR_VOL_UNAVAILABLE				20849

/* Authorization errors */
#define zERR_NO_SET_PRIVILEGE  				20850 /* does not have rights to modify metadata */
#define zERR_NO_CREATE_PRIVILEGE			20851 /* does not have rights to create an object */
#define zERR_INVALID_AUTHORIZE_SPACE		20852 /* bad authorization space */
#define zERR_INVALID_AUTHORIZE_MODEL		20853 /* bad authorization model */
#define zERR_INVALID_AUTHORIZE_OPERATION	20854 /* bad operation passed to an op function */
#define zERR_AUTHORIZE_LOAD_FAILED			20855 /* failed to load part of the authorization system */
#define zERR_TRUSTEE_NOT_FOUND				20856 /* unable to find the specified trustee id */
#define zERR_NO_TRUSTEES_FOUND				20857 /* There were no trustees */
#define zERR_NO_TRUSTEE_CHANGE_PRIVILEGE	20858 /* no rights to change trustees */
#define zERR_ACCESS_DENIED					20859 /* authorization/attributes denied access */
#define zERR_NO_WRITE_PRIVILEGE				20860 /* no granted write privileges */
#define zERR_NO_READ_PRIVILEGE				20861 /* no granted read privileges */
#define zERR_NO_DELETE_PRIVILEGE			20862 /* no delete privileges */
#define zERR_SOME_NO_DELETE_PRIVILEGE		20863 /* on wildcard some do not have delete privileges */
#define zERR_INVALID_AUTH_MODEL_VERSION		20864 /* version being registered is not correct */
#define zERR_EXCEEDED_MAX_AUTH_SPACES		20865 /* exceeded the maximum number of authorization spaces */
#define zERR_EXCEEDED_MAX_AUTH_MODELS		20866 /* exceeded the maximum number of authorization models */
#define zERR_NO_SUCH_OBJECT					20867 /* no such object in the naming services */
#define zERR_CANT_DELETE_OPEN_FILE			20868 /* cant delete an open file without rights */
#define zERR_NO_CREATE_DELETE_PRIVILEGE		20869 /* no delete on create privileges */
#define zERR_NO_SALVAGE_PRIVILEGE			20870 /* no privileges to salvage this file */
#define zERR_NO_SCAN_PRIVILEGE				20871 /* no privilege to scan the directory/file */


/* NWCS Errors */
#define zERR_NWCS_DUPLICATE_POOL_NAME		20890 /* The pool name already exists within the cluster. Indicates that
												   * a local pool on one of the servers already is using the
												   * requested SHARED pool name.
												   */
#define zERR_NWCS_DUPLICATE_VOLUME_NAME		20891 /* The volume name already exists within the cluster. Indicates that
												   * a local volume on one of the servers already is using the
												   * requested SHARED volume name.
												   */
#define zERR_NWCS_POOL_IS_ACTIVE			20892 /* The pool is ACTIVE elsewhere in the cluster. */
#define	zERR_NWCS_VOLUME_IS_ACTIVE			20893 /* The volume is ACTIVE elsewhere in the cluster. */
#define	zERR_NWCS_NOT_THE_OWNER				20894 /* For attribute changing */
#define	zERR_NWCS_OPERATION_IN_PROGRESS		20895 /* Operation in progress on another cluster? */
#define	zERR_NWCS_SHARE_VIOLATION			20896 /* General(non-specific) NWCS error */
#define	zERR_NWCS_NOT_A_MEMBER				20897 /* Server is not a member of the cluster but it is trying to access one of the shared resources */

/* Locking-related Errors */
#define zERR_IOLOCK_ERROR           		20900 /* tried to do read/write on a locked range of a file */
#define zERR_LOCK_ERROR  	      			20901 /* general lock error */
#define zERR_LOCK_COLLISION           		20902 /* tried to lock a range that was already locked */
#define zERR_LOCK_WAITING           		20903 /* timed out waiting for a lock */
#define zERR_NONEXISTENT_LOCK           	20904 /* tried to release a lock that doesn't exist */
#define zERR_FILE_READ_LOCKED				20905 /* cant grant read access to the file */
#define zERR_FILE_WRITE_LOCKED				20906 /* cant grant write access to the file */
#define zERR_CANT_DENY_READ_LOCK			20907 /* cant grant deny read access to the file */
#define zERR_CANT_DENY_WRITE_LOCK			20908 /* cant grant deny write access to the file */

/* Unicode errors */
#define zERR_UNICODE_INVALID_CONVERSION_TYPE 20950 /* invalid unicode_t conversion type */
#define zERR_UNICODE_CONVERSION_ERROR		20951 /* unicode_t conversion error */
#define zERR_UNICODE_INIT					20952 /* error initializing unicode_t sub-system*/
#define zERR_UNICODE_NON_MAPPABLE_CHAR		20953 /* non mappable char encountered converting unicode */
#define zERR_EXCEEDED_MAX_CONVERSION_TYPES	20954 /* exceeded the maximum number of loadable conversion types */
#define zERR_INVALID_UTF8_CHAR				20955 /* error converting utf8 to unicode, invalid utf8 sequence */

/* Link errors */
#define zERR_INVALID_LINK_TYPE				21000 /* The specified link type is not supported */
#define zERR_CANT_HARD_LINK_DATA_STREAMS   	21001 /* not allowed to create hard links to or from a dataStream */
#define zERR_CANT_HARD_LINK_TO_DIRECTORY 	21002 /* not allowed to create hard links to a directory/container */
#define zERR_MUST_HARD_LINK_FROM_DIRECTORY 	21003 /* not allowed to create hard links from a non-directory/container */
#define zERR_CANT_HARD_LINK_TO_NON_FILE		21004 /* not allowed to create hard links to beasts not derived from file */
#define zERR_LINK_DEST_FILE_ALREADY_EXISTS	21005 /* the new name for the link already exists */

/* NLM Registration errors */
#define zERR_MODULE_NAME_ALREADY_USED		21050 /* the given NSS MODULE name is already in use */
#define zERR_MODULE_NAME_NOT_FOUND			21051 /* the given NSS module name could not be found */
#define zERR_INVALID_MODULE_VERSION			21052 /* the given MODULE has an invalid version number*/
#define zERR_INCOMPATIBLE_API_VERSION		21053 /* the given MODULE has an incompatible API version number */
#define zERR_INCOMPATIBLE_DEBUG_STATE		21054 /* the given MODULE has an incompatible DEBUG version state */
#define zERR_UNKNOWN_MODULE_TYPE			21055 /* the given MODULE has an unknown module type */
#define zERR_INVALID_REGISTRATION_TYPE		21056 /* the given type is invalid for the item being registered */
#define zERR_TYPE_ALREADY_REGISTERED		21057 /* the given type is already registered */
#define zERR_INCOMPATIBLE_MP_FLAG			21058 /* the given MODULE has an incompatible MP state */

/* MASV errors */
#define	zERR_MASV_LABEL_ALREADY_SET			21100 /* The label is already set */

/* Modify Volume Info errors */
#define zERR_SOME_ATTRS_NOT_CHANGED			21150 /* On a modify of pool/volume enabledAttributes, some weren't changed */
#define zERR_ALL_ATTRS_NOT_CHANGED			21151 /* On a modify of pool/volume enabledAttributes, all weren't changed */

/* Feature Not Enabled errors */
#define zERR_EXTENDED_ATTR_NOT_ENABLED			21200 /* Attempt to create extended attributes on a volume where the feature is not enabled */
#define zERR_DATA_STREAMS_NOT_ENABLED			21201 /* Attempt to create a named data stream on a volume where the feature is not enabled */
#define zERR_DOS_METADATA_NOT_ENABLED			21202 /* Attempt to write DOS metadata on a volume where the feature is not enabled */
#define zERR_NETWARE_METADATA_NOT_ENABLED		21203 /* Attempt to write NetWare metadata on a volume where the feature is not enabled */
#define zERR_MAC_METADATA_NOT_ENABLED			21204 /* Attempt to write MACintosh metadata on a volume where the feature is not enabled */
#define zERR_UNIX_METADATA_NOT_ENABLED			21205 /* Attempt to write UNIX metadata on a volume where the feature is not enabled */
#define zERR_HARD_LINKS_NOT_ENABLED				21206 /* Attempt to create hard links on a volume where the feature is not enabled */
#define zERR_TRANSACTIONS_NOT_ENABLED			21207 /* Attempt to create user-level transactions on a volume where the feature is not enabled */
#define zERR_USER_SPACE_RESTRICT_NOT_ENABLED	21208 /* Attempt to create user space restrictions on a volume where the feature is not enabled */
#define zERR_COMPRESSION_NOT_ENABLED			21209 /* Attempt to compress on a volume where the feature is not enabled */
#define zERR_SPARSE_FILES_NOT_ENABLED			21210 /* Attempt to modify EOF on a file without modifying the physical size of the file */
#define zERR_PHYSICAL_EOF_NOT_ENABLED			21211 /* Attempt to extend physical size of a file independently of it's logical size */
#define zERR_DIRECT_IO_NOT_ENABLED				21212 /* Attempt to use Direct IO on a volume where it is not enabled */
#define zERR_MFL_NOT_ENABLED				    21213 /* Attempt to use MFL on a volume where it is not enabled */


/* User space restriction errors */
#define zERR_ADDED_USER_TWICE				21300 /* Tried to add the same user twice */
#define zERR_NO_SUCH_USER					21301 /* The requested user was not found in the tree */
#define zERR_USER_SPACE_NOT_ENABLED			21302 /* User Space restrictions are not enabled */
#define zERR_NOT_ENOUGH_USER_SPACE			21303 /* Tried to allocate more than the restruction would allow */

/* User store errors */
#define zERR_FULL_NAME_NOT_FOUND			21350 /* Name not found for a GUID */
#define zERR_NEGATIVE_CACHE_ENTRY_FOUND		21351 /* A negative entry was found in the cache */

/* Compression Manager-generated errors */
#define zERR_CM_ABORTED                         21400 /* Compression/decompression aborted */
#define zERR_CM_INVALID_COMP_FILE_HEADER        21401 /* Invalid compressed file header */
#define zERR_CM_UNKNOWN_COMP_ALGO_VERSION       21402 /* Unknown minor version specified for Compression Algorithm */
#define zERR_CM_COMP_ALGO_ALREADY_REGISTERED    21403 /* Compression Algorithm already registered */
#define zERR_CM_CANT_DECOMPRESS                 21404 /* Cannot decompress file */
#define zERR_CM_CANT_COMPRESS                   21405 /* Cannot compress file */
#define zERR_CM_CORRUPT_COMPRESSED_FILE         21406 /* Compressed file is corrupt */
#define zERR_CM_COMP_ALGO_ERROR                 21407 /* Compression Algorithm-specific error */
#define zERR_CM_COMP_ALGO_NOT_REGISTERED        21408 /* Compression Algorithn not registered */
#define zERR_CM_INVALID_STREAM_HANDLE           21409 /* Invalid stream handle */
#define zERR_CM_INVALID_BUFFER_HANDLE           21410 /* Invalid buffer handle */

/* Directory quota errors */
#define zERR_ADDED_DIR_TWICE				21500 /* Tried to add the same directory twice */
#define zERR_NO_SUCH_DIR					21501 /* The requested directory was not found in the tree */
#define zERR_DIR_QUOTAS_NOT_ENABLED			21502 /* Directory quotas are not enabled */
#define zERR_NOT_ENOUGH_DIR_SPACE			21503 /* Tried to allocate more than the quota would allow */
#define zERR_DIR_QUOTA_LATCH_ERROR			21504 /* Could not get a latch on a cache entry */
#define zERR_DIR_QUOTA_CACHE_ERROR			21505 /* Error during cache add/lookup */

/* User transaction errors */
#define zERR_TRANSACTION_DATA_TOO_LARGE			21600 /* Trying to xaction too much data at once */
#define zERR_TRANSACTION_LOG_FILE_NOT_WRITTEN	21601 /* Could not write log file */
#define zERR_NESTED_XACTIONS_NOT_IMPLIMENTED	21602 /* Tried to use a nested transaction */
#define zERR_TRANSACTION_LOG_FILE_OVERFLOW		21603 /* Log file is full */
#define zERR_VOLUME_NOT_TRANSACTIONED			21604 /* Xactions not supported on this volume */
#define zERR_TRANSACTION_INVALID_STATE			21605 /* Xaction not in correct state for operation */
#define	zERR_TRANSACTION_CROSSES_VOLUMES		21606 /* Single xactions that operate on multiple volumes not allowed */

/* Management file errors */
#define zERR_MODULE_NOT_FOUND					21700 /* a module looked for is not found */
#define zERR_UNABLE_TO_GET_SET_PARAM_VALUE		21701 /* got an error getting the value of a "Set" param */
#define zERR_XML_IS_BAD							21702 /* bad XML found during parsing */
#define zERR_XML_IS_INCOMPLETE					21703 /* XML found without terminating tag */

/*=========================================================================
 *=========================================================================
 *	NW Symantic Agent Specific Errors (Range 22000-22099)
 *=========================================================================
 *=========================================================================*/
/* legacy errors (above the common layer) */
#define zERR_INVALID_OPENCREATE_MODE		22000 /* invalid mode passed in NCP 87 (create/open) */

/*=========================================================================
 *=========================================================================
 *	CD  Specific Errors (Range 22500-22599)
 *=========================================================================
 *=========================================================================*/

#define zERR_ISO_NO_VOLUME_TERMINATOR			22500
#define zERR_ISO_NOT_AN_ISO_CD					22501
#define zERR_ISO_NOT_A_RECOGNIZED_ISO_VERSION	22502
#define zERR_ISO_NO_VALID_VOLUME_FOUND			22503
#define zERR_ISO_CANT_READ_ROOT_DIRECTORY		22504
#define zERR_ISO_CANT_ADD_VOLUME_TO_SYSTEM		22505
#define zERR_ISO_CANT_ALLOCATE_MEMORY			22506
#define zERR_ISO_NO_ROOT_DIRECTORY				22507
#define zERR_ISO_ASSOCIATED_FILE				22508
#define zERR_ISO_INVALID_DIRECTORY_PTR			22509
#define zERR_ISO_FILE_NOT_FOUND					22510
#define zERR_ISO_ROOTDIR_IS_NOT_LOADED			22511
#define zERR_ISO_CANT_CREATE_BEAST				22512
#define zERR_ISO_ZID_NOT_FOUND_IN_HASH			22513
#define zERR_ISO_FEATURE_NOT_SUPPORTED			22514


/*=========================================================================
 *=========================================================================
 *	DOSFAT/FAT32 Specific Errors (Range 22600-22699)
 *=========================================================================
 *=========================================================================*/
#define zERR_FAT_FILETYPE_NOT_SUPPORTED			22600 /* Unsupported File Type */
#define zERR_FAT_CANT_INSTANTIATE_FILE			22601 /* Unable to export the file to NSS */
#define zERR_FAT_ZID_NOT_FOUND_IN_HASH			22602 /* Failed to find file in FAT hash */
#define zERR_FAT_NOT_YET_IMPLEMENTED  			22603 /* FAT Function not implemented */
#define zERR_FAT_INVALID_DIR_ENTRY	  			22604 /* Invalid FAT Dir Entry */
#define zERR_FAT_ROOT_DIR_FULL		  			22605 /* Rootdir is full */
#define zERR_FAT_INVALID_FAT_ENTRY	  			22606 /* Invalid Fat Entry */
#define zERR_FAT_INVALID_CLUSTER_SIZE			22607 /* Cluster Size not power of 2 */

/*=========================================================================
 *	NSS Java interface reserved error codes (Range 22900-22999)
 *=========================================================================*/
#define	zERR_JAVA_JNI_ERROR						22901


/*=========================================================================
 *=========================================================================
 *	LSS Assignable Error Area (Range 23000-23999)
 *
 *  This range is used to assign external NSS groups error ranges
 *  for their LSSes.  The define names of error codes that they
 *  use should be zERR_xxxx_yyyy where xxxx is the last part of the LSSes
 *  ID define name.  yyyy is whatever the LSS wishes it to be.
 *
 *  For example, the LSS whose ID is zLSS_ID_SMSTAPE would have errors
 *  code names of zERR_SMSTAPE_yyyy.
 *
 *  To obtain a range of error codes send an E-Mail to NSS@novell.com.
 *  Please specify the number of error codes you require.  Generally,
 *  error codes are given out 25 at a time.
 *=========================================================================
 *=========================================================================*/

/*=========================================================================
 *	SMS Tape LSS (zLSS_ID_SMSTAPE) reserved error codes (Range 23000-23024)
 *  Reserved by Greg Pachner on Nov 18, 1998 for Sudhir Subbarao
 *  <sksubbarao@novell.com> of the SMS Team of Novell.
 *=========================================================================*/
#define	zERR_SMSTAPE_FIRST					23000
#define	zERR_SMSTAPE_LAST					23024

/*=========================================================================
 *	NFS Gateway LSS (zLSS_ID_NFSGATEWAY) reserved error codes
 *	(Range 23100-23199)
 *  Reserved by Paul Taysom on October 14, 1999 for Giridhar V.
 *   <vgiridhar@novell.com> of the NFS Gateway Team of Novell.
 *=========================================================================*/
#define	zERR_NFSGATEWAY_FIRST				23100
#define	zERR_NFSGATEWAY_LAST				23199

/*=========================================================================
 *=========================================================================
 *	ZFS Storage System Specific Errors (Range 24800-24899)
 *=========================================================================
 *=========================================================================*/
/* B-tree Errors */
#define zERR_NO_NODE						24800 /* No node at the requested block location. */
#define zERR_BAD_LOG_RECORD					24801 /* bad log record found */
#define zERR_BEAST_TOO_BIG					24802 /* the beast is too big to fit in the b-tree */
#define	zERR_TREE_LEAF_CORRUPT 				24803 /* Returned when the free size stored in a B-Tree
												   * does not agree with the size of all the
												   * free chunks in the leaf.
												   */
#define zERR_MISSING_BEAST					24804 /* Missing beast.  Required beast is mssing (ADmin will need to verify volume) */

/* ZLOG Errors */
#define	zERR_ZLOG_BAD_CHECKSUM				24820 /* ZLOG log record checksum error */
#define zERR_ZLOG_FILE_TOO_SMALL			24821 /* ZLOG file is too small */
#define zERR_ZLOG_BAD_BLOCK_SIGNATURE		24822 /* ZLOG file's log block signature is invalid */
#define zERR_ZLOG_BAD_RECORD_COUNT			24823 /* ZLOG file's log block log record count is invalid */
#define zERR_ZLOG_BAD_RECORD_SIZE			24824 /* ZLOG file's record size is invalid */
#define zERR_ZLOG_BAD_LSN					24825 /* ZLOG file's log record LSN is invalid */
#define zERR_ZLOG_FILE_INIT_FAILED			24826 /* ZLOG could not create ZLOG file during POOL initialize */ 
#define zERR_ZLOG_BAD_BEAST_SIGNATURE		24827 /* ZLOG beast's signature is invalid */
#define zERR_ZLOG_UNSUPPORTED_BEAST_VERSION	24828 /* ZLOG code does not support ZLOG Beast version in persistent storage */ 
#define zERR_ZLOG_UNSUPPORTED_FILE_VERSION	24829 /* ZLOG code does not support ZLOG File version
												  * in persistent storage.  If user does a clean
												  * shutdown with previos nss.nlm then this error
												  * will go away.  Otherwise, a reset is required.
												  */ 
#define zERR_ZLOG_FILE_FULL					24830 /* ZLOG file is full - file too small for transaction rate */
#define zERR_ZLOG_NO_MORE_RECORDS			24831 /* No more ZLOG recovery information (Not a USER error) */

/* ZVL Errors (ZLSS Volume Locator) */
#define zERR_ZVL_UNSUPPORTED_BEAST_VERSION	24838 /* ZLSS Volume Locator code does not support ZVL Beast version in persistent storage */ 
#define zERR_ZVL_BAD_BEAST_SIGNATURE		24839 /* ZLSS Volume Locator(ZVL) beast's signature is invalid */

/* ZFSVOL/ZLSSPOOL volume data Errors */
#define	zERR_ZFSVOL_BAD_CHECKSUM	   		24840 /* ZFS volume data checksum error */
#define	zERR_ZFSVOL_AIPU_TOO_MANY_LVS  		24841 /* Too many Logical Volumes during upgrade. Auto inplace upgrade returns this when
												   * upgrading to LVs if more the one LV exists that expected.
												   */
#define	zERR_ZFSVOL_AIPU_LVDB_CORRUPTED		24842 /* Logged Volume Data Block number was incorrect during AIPU */
#define	zERR_ZFSVOL_AIPU_PHYSICAL_POOL		24843 /* Physical pool went away during AIPU */
#define	zERR_ZFSVOL_NOT_A_ZLSS_VOLUME		24844 /*  */
#define	zERR_ZLSSPOOL_NOT_A_ZLSS_POOL		24845 /*  */
#define	zERR_ZFSVOL_AIPU_NOT_ACTIVE			24846 /* Volume did not activate during LV AIPU */
#define	zERR_ZLSSPOOL_UPGRADE_POOL_FIRST	24847 /* The ZLSS pool must be upgraded before the operation can be done. */
#define zERR_ZLSSPOOL_NO_PHYSICAL_POOL		24848 /* Could not find physical pool */

/* Checkpoint Errors */
#define	zERR_CHECKPOINT_BAD_CHECKSUM	   	24850 /* Checkpoint checksum error */
#define zERR_CHECKPOINT_BAD_BLOCK_SIGNATURE	24851 /* Checkpoint block signature is invalid */
#define zERR_CHECKPOINT_BAD_BLOCK_SIZE	    24852 /* Checkpoint block size is invalid */
#define zERR_CHECKPOINT_UNSUPPORTED_VERSION	24853 /* Checkpoint code does not support version in checkpoint */ 

/* Superblock Errors */
#define	zERR_SUPERBLOCK_BAD_CHECKSUM	   	24860 /* Superblock checksum error */
#define zERR_SUPERBLOCK_BAD_BLOCK_SIGNATURE	24861 /* Superblock block signature is invalid */
#define zERR_SUPERBLOCK_BAD_BLOCK_SIZE	    24862 /* Superblock block size is invalid */
#define zERR_SUPERBLOCK_UNSUPPORTED_VERSION	24863 /* Superblock code does not support version in superblock header */ 
#define zERR_SUPERBLOCK_UNSUPPORTED_MEDIA 	24864 /* Media version not supportted */	
#define zERR_SUPERBLOCK_MISMATCH		 	24865 /* Two or more valid superblock headers do not match each other */	
#define zERR_SUPERBLOCK_UNDESIRED_LOCATION	24866 /* A super block is not in desired mathamatical location */
#define zERR_SUPERBLOCK_NOT_ENOUGH			24867 /* Not enough valid super block headers */
#define zERR_SUPERBLOCK_CORRUPTED			24868 /* Valid super block header has bad information in it */

/* Recovery Errors */
#define	zERR_RECOVERY_TOO_MANY_UNCOMMITS	24880 /* Recovery could not uncommit all transactions (that it needed
												   * to) before it hit the last checkpoint. */

/***************************************************************************
 *	Generic Error for FSHooks
 ***************************************************************************/
#define zERR_GENERIC_NSS_ERROR				24999

#ifdef __cplusplus
}
#endif

#endif /* _ZERROR_H_ */
