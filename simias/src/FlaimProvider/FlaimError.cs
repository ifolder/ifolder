/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
using System;
using Simias;

namespace Simias.Storage.Provider.Flaim
{
	/// <summary>
	/// FLAIM error codes.
	/// </summary>
	public class FlaimError
	{
		/// <summary>
		/// Flaim defined errors.
		/// </summary>
		public enum Error
		{
			/// <summary>
			/// Successful return code.
			/// </summary>
			FERR_OK						=0,
			/// <summary>
			/// First (lowest) error currently defined
			/// </summary>
			FIRST_FLAIM_ERROR 			=0xC001,					
			/// <summary>
			/// Beginning of file hit
			/// </summary>
			FERR_BOF_HIT					=0xC001,
			/// <summary>
			/// End of file hit
			/// </summary>
			FERR_EOF_HIT					=0xC002,
			/// <summary>
			/// End of file for gedcom routine.  This is an internal error
			/// </summary>
			FERR_END						=0xC003,
			/// <summary>
			/// Record already exists
			/// </summary>
			FERR_EXISTS					=0xC004,					
			/// <summary>
			/// Internal failure
			/// </summary>
			FERR_FAILURE					=0xC005,				
			/// <summary>
			/// A record, key, or key reference was not found
			/// </summary>
			FERR_NOT_FOUND				=0xC006,			
			/// <summary>
			/// Invalid dictionary record number -- outside unreserved range
			/// </summary>
			FERR_BAD_DICT_ID				=0xC007,
			/// <summary>
			/// Invalid Container Number
			/// </summary>
			FERR_BAD_CONTAINER			=0xC008,			
			/// <summary>
			/// LFILE does not have a root block.
			/// </summary>
			FERR_NO_ROOT_BLOCK			=0xC009,				
			// Always handled internally - never returned to application
			/// <summary>
			/// Cannot pass a zero DRN into modify or delete or 0xFFFFFFFF into add
			/// </summary>
			FERR_BAD_DRN					=0xC00A,
			/// <summary>
			/// Bad field number in record being added
			/// </summary>
			FERR_BAD_FIELD_NUM			=0xC00B,			
			/// <summary>
			/// Bad field type in record being added
			/// </summary>
			FERR_BAD_FIELD_TYPE			=0xC00C,				
			/// <summary>
			/// Request contained bad db handle
			/// </summary>
			FERR_BAD_HDL					=0xC00D,
			/// <summary>
			/// Invalid Index Number Given
			/// </summary>
			FERR_BAD_IX					=0xC00E,			
			/// <summary>
			/// Operation could not be completed - a backup is being performed
			/// </summary>
			FERR_BACKUP_ACTIVE			=0xC00F,				
			/// <summary>
			/// Comparison of serial numbers failed
			/// </summary>
			FERR_SERIAL_NUM_MISMATCH		=0xC010,
			/// <summary>
			/// Bad database serial number in RFL file header
			/// </summary>
			FERR_BAD_RFL_DB_SERIAL_NUM	=0xC011,
			/// <summary>
			/// The B-Tree in the file system is bad
			/// </summary>
			FERR_BTREE_ERROR				=0xC012,
			/// <summary>
			/// The B-tree in the file system is full
			/// </summary>
			FERR_BTREE_FULL				=0xC013,			
			/// <summary>
			/// Bad RFL file number in RFL file header
			/// </summary>
			FERR_BAD_RFL_FILE_NUMBER		=0xC014,
			/// <summary>
			/// Cannot delete field definitions
			/// </summary>
			FERR_CANNOT_DEL_ITEM			=0xC015,				
			/// <summary>
			/// Cannot modify a field's type
			/// </summary>
			FERR_CANNOT_MOD_FIELD_TYPE	=0xC016,			
			/// <summary>
			/// Cannot recover database -- read only
			/// </summary>
			FERR_CANNOT_RECOV_RDONLY		=0xC017,
			/// <summary>
			/// Bad destination type specified for conversion
			/// </summary>
			FERR_CONV_BAD_DEST_TYPE		=0xC018,			
			/// <summary>
			/// Non-numeric digit found in text to numeric conversion
			/// </summary>
			FERR_CONV_BAD_DIGIT			=0xC019,				
			/// <summary>
			/// Bad source type specified for conversion
			/// </summary>
			FERR_CONV_BAD_SRC_TYPE		=0xC01A,				
			/// <summary>
			/// Could not open an RFL file.
			/// </summary>
			FERR_RFL_FILE_NOT_FOUND		=0xC01B,				
			/// <summary>
			/// Destination buffer not large enough to hold converted data
			/// </summary>
			FERR_CONV_DEST_OVERFLOW		=0xC01C,				
			/// <summary>
			/// Illegal conversion -- not supported
			/// </summary>
			FERR_CONV_ILLEGAL				=0xC01D,
			/// <summary>
			/// Source cannot be a NULL pointer in conversion
			/// </summary>
			FERR_CONV_NULL_SRC			=0xC01E,			
			/// <summary>
			/// Destination cannot be a NULL pointer in conversion
			/// </summary>
			FERR_CONV_NULL_DEST			=0xC01F,				
			/// <summary>
			/// Numeric overflow (GT upper bound) converting to numeric type
			/// </summary>
			FERR_CONV_NUM_OVERFLOW		=0xC020,				
			/// <summary>
			/// Numeric underflow (LT lower bound) converting to numeric type
			/// </summary>
			FERR_CONV_NUM_UNDERFLOW		=0xC021,				
			/// <summary>
			/// Data in the database is invalid
			/// </summary>
			FERR_DATA_ERROR				=0xC022,				
			/// <summary>
			/// Out of FLAIM Session Database handles
			/// </summary>
			FERR_DB_HANDLE				=0xC023,				
			/// <summary>
			/// Internal logical DD compromised
			/// </summary>
			FERR_DD_ERROR					=0xC024,
			/// <summary>
			/// Inc. backup file provided during a restore is invalid
			/// </summary>
			FERR_INVALID_FILE_SEQUENCE	=0xC025,			
			/// <summary>
			/// Illegal operation for database
			/// </summary>
			FERR_ILLEGAL_OP				=0xC026,				
			/// <summary>
			/// Duplicate dictionary record found
			/// </summary>
			FERR_DUPLICATE_DICT_REC		=0xC027,				
			/// <summary>
			/// Condition occurred which prevents database conversion
			/// </summary>
			FERR_CANNOT_CONVERT			=0xC028,				
			/// <summary>
			/// Db version is an unsupported ver of FLAIM (ver 1.2)
			/// </summary>
			FERR_UNSUPPORTED_VERSION		=0xC029,
			/// <summary>
			/// File error in a gedcom routine
			/// </summary>
			FERR_FILE_ER					=0xC02A,				
			/// <summary>
			/// GEDCOM level missing or bad syntax
			/// </summary>
			FERR_GED_BAD_LEVEL			=0xC02B,			
			/// <summary>
			/// Bad record ID syntax
			/// </summary>
			FERR_GED_BAD_RECID			=0xC02C,				
			/// <summary>
			/// Bad or ambiguous/extra value in GEDCOM
			/// </summary>
			FERR_GED_BAD_VALUE			=0xC02D,				
			/// <summary>
			/// Exceeded GED_MAXLVLNUM in gedcom routines
			/// </summary>
			FERR_GED_MAXLVLNUM			=0xC02E,				
			/// <summary>
			/// Bad GEDCOM tree structure -- level skipped
			/// </summary>
			FERR_GED_SKIP_LEVEL			=0xC02F,				
			/// <summary>
			/// Attempt to start an illegal type of transaction
			/// </summary>
			FERR_ILLEGAL_TRANS			=0xC030,				
			/// <summary>
			/// Illegal operation for transaction type
			/// </summary>
			FERR_ILLEGAL_TRANS_OP			=0xC031,
			/// <summary>
			/// Incomplete log record encountered during recovery
			/// </summary>
			FERR_INCOMPLETE_LOG			=0xC032,			
			/// <summary>
			/// Invalid Block Length
			/// </summary>
			FERR_INVALID_BLOCK_LENGTH		=0xC033,
			/// <summary>
			/// Invalid tag name
			/// </summary>
			FERR_INVALID_TAG				=0xC034,				
			/// <summary>
			/// A key|reference is not found -- modify/delete error
			/// </summary>
			FERR_KEY_NOT_FOUND			=0xC035,			
			/// <summary>
			/// Value too large
			/// </summary>
			FERR_VALUE_TOO_LARGE			=0xC036,
			/// <summary>
			/// General memory allocation error
			/// </summary>
			FERR_MEM						=0xC037,				
			/// <summary>
			/// Bad serial number in RFL file header
			/// </summary>
			FERR_BAD_RFL_SERIAL_NUM		=0xC038,			
			/// <summary>
			/// Multiple field types defined in index definition
			/// </summary>
			FERR_MULTIPLE_FIELD_TYPES		=0xC039,
			/// <summary>
			/// Running old code on a newer database code must be upgraded
			/// </summary>
			FERR_NEWER_FLAIM				=0xC03A,				
			/// <summary>
			/// Attempted to change a field state illegally
			/// </summary>
			FERR_CANNOT_MOD_FIELD_STATE	=0xC03B,			
			/// <summary>
			/// The highest DRN number has already been used in an add
			/// </summary>
			FERR_NO_MORE_DRNS				=0xC03C,
			/// <summary>
			/// Attempted to updated DB outside transaction
			/// </summary>
			FERR_NO_TRANS_ACTIVE			=0xC03D,				
			/// <summary>
			/// Found Duplicate key for unique index
			/// </summary>
			FERR_NOT_UNIQUE				=0xC03E,			
			/// <summary>
			/// Opened a file that was not a FLAIM file
			/// </summary>
			FERR_NOT_FLAIM				=0xC03F,				
			/// <summary>
			/// NULL Record cannot be passed to add or modify
			/// </summary>
			FERR_NULL_RECORD				=0xC040,
			/// <summary>
			/// No http stack was loaded
			/// </summary>
			FERR_NO_HTTP_STACK			=0XC041,			
			/// <summary>
			/// While reading was unable to get previous version of block
			/// </summary>
			FERR_OLD_VIEW					=0xC042,
			/// <summary>
			/// The integrity of the dictionary PCODE in the database has been compromised
			/// </summary>
			FERR_PCODE_ERROR				=0xC043,				
			/// <summary>
			/// Invalid permission for file operation
			/// </summary>
			FERR_PERMISSION				=0xC044,			
			/// <summary>
			/// Dictionary record has improper syntax
			/// </summary>
			FERR_SYNTAX					=0xC045,				
			/// <summary>
			/// Callback failure
			/// </summary>
			FERR_CALLBACK_FAILURE			=0xC046,
			/// <summary>
			/// Attempted to close DB while transaction was active
			/// </summary>
			FERR_TRANS_ACTIVE				=0xC047,				
			/// <summary>
			/// A gap was found in the transaction sequence in the RFL
			/// </summary>
			FERR_RFL_TRANS_GAP			=0xC048,			
			/// <summary>
			/// Something in collated key is bad.
			/// </summary>
			FERR_BAD_COLLATED_KEY			=0xC049,
			/// <summary>
			/// Attempting a feature that is not supported for the database version.
			/// </summary>
			FERR_UNSUPPORTED_FEATURE		=0xC04A,				
			/// <summary>
			/// Attempting to delete a container that has indexes defined for it.  Indexes must be deleted first.
			/// </summary>
			FERR_MUST_DELETE_INDEXES		=0xC04B,				
			/// <summary>
			/// RFL file is incomplete.
			/// </summary>
			FERR_RFL_INCOMPLETE			=0xC04C,		
			/// <summary>
			/// Cannot restore RFL files - not using multiple RFL files.
			/// </summary>
			FERR_CANNOT_RESTORE_RFL_FILES	=0xC04D,
			/// <summary>
			/// A problem (corruption, etc.) was detected in a backup set
			/// </summary>
			FERR_INCONSISTENT_BACKUP		=0xC04E,				
			/// <summary>
			/// Block checksum error
			/// </summary>
			FERR_BLOCK_CHECKSUM			=0xC04F,			
			/// <summary>
			/// Attempted operation after a critical error - should abort transaction
			/// </summary>
			FERR_ABORT_TRANS				=0xC050,
			/// <summary>
			/// Attempted to open RFL file which was not an RFL file
			/// </summary>
			FERR_NOT_RFL					=0xC051,				
			/// <summary>
			/// RFL packet was bad
			/// </summary>
			FERR_BAD_RFL_PACKET			=0xC052,			
			/// <summary>
			/// Bad data path specified to open database
			/// </summary>
			FERR_DATA_PATH_MISMATCH		=0xc053,				
			/// <summary>
			/// FlmConfig( FLM_HTTP_REGISTER_URL) failed
			/// </summary>
			FERR_HTTP_REGISTER_FAILURE	=0xC054,				
			/// <summary>
			/// FlmConfig( FLM_HTTP_DEREGISTER_URL) failed
			/// </summary>
			FERR_HTTP_DEREG_FAILURE		=0xC055,				
			/// <summary>
			/// Indexing process failed, non-unique data was found when a unique index was being created
			/// </summary>
			FERR_IX_FAILURE				=0xC056,				
			/// <summary>
			/// Tried to import new http related symbols before unimporting the old ones
			/// </summary>
			FERR_HTTP_SYMS_EXIST			=0xC057,
			/// <summary>
			/// Database has already been rebuilt
			/// </summary>
			FERR_DB_ALREADY_REBUILT		=0xC058,			
			/// <summary>
			/// Attempt to create a database or store file, but the file already exists
			/// </summary>
			FERR_FILE_EXISTS				=0xC059,
			/// <summary>
			/// Call to SAL_ModResolveSym failed.
			/// </summary>
			FERR_SYM_RESOLVE_FAIL			=0xC05A,				
			/// <summary>
			/// Connection to FLAIM server is bad
			/// </summary>
			FERR_BAD_SERVER_CONNECTION	=0xC05B,			
			/// <summary>
			/// Database is being closed due to a critical erro
			/// </summary>
			FERR_CLOSING_DATABASE			=0xC05C,
			/// <summary>
			/// CRC could not be verified.
			/// </summary>
			FERR_INVALID_CRC				=0xC05D,				
			//#define FERR_NU_C05E					0xC05E
			/// <summary>
			/// function not implemented (possibly client/server)
			/// </summary>
			FERR_NOT_IMPLEMENTED			=0xC05F,				
			/// <summary>
			/// Mutex operation failed
			/// </summary>
			FERR_MUTEX_OPERATION_FAILED	=0xC060,			
			/// <summary>
			/// Unable to get the mutex lock
			/// </summary>
			FERR_MUTEX_UNABLE_TO_LOCK		=0xC061,
			/// <summary>
			/// Semaphore operation failed
			/// </summary>
			FERR_SEM_OPERATION_FAILED		=0xC062,				
			/// <summary>
			/// Unable to get the semaphore lock
			/// </summary>
			FERR_SEM_UNABLE_TO_LOCK		=0xC063,			
			//#define FERR_NU_C064					0xC064
			//#define FERR_NU_C065					0xC065
			//#define FERR_NU_C066					0xC066
			//#define FERR_NU_C067					0xC067
			//#define FERR_NU_C068					0xC068
			/// <summary>
			/// Bad reference in the dictionary
			/// </summary>
			FERR_BAD_REFERENCE			=0xC069,				
			/// <summary>
			/// Invalid area ID
			/// </summary>
			FERR_INVALID_AREA				=0xC06A,
			/// <summary>
			/// Machine class not defined in area definition
			/// </summary>
			FERR_MACHINE_NOT_DEF			=0xC06B,				
			/// <summary>
			/// Could not find a matching driver in machine's area definition
			/// </summary>
			FERR_DRIVER_NOT_FOUND			=0xC06C,				
			/// <summary>
			/// Tried one or more driver directory paths and failed
			/// </summary>
			FERR_BAD_DRIVER_PATH			=0xC06D,				
			//#define FERR_NU_C06E					0xC06E
			//#define FERR_NU_C06F					0xC06F
			/// <summary>
			/// FlmDbUpgrade cannot upgrade the database
			/// </summary>
			FERR_UNALLOWED_UPGRADE		=0xC070,			
			//#define FERR_NU_C071					0xC071
			//#define FERR_NU_C072					0xC072
			/// <summary>
			/// No query source, or bad source
			/// </summary>
			FERR_BAD_QUERY_SOURCE			=0xC073,
			/// <summary>
			/// Attempted to use a dictionary ID that has been reserved at a lower level
			/// </summary>
			FERR_ID_RESERVED				=0xC074,				
			/// <summary>
			/// Attempted to reserve a dictionary ID that has been used at a higher level
			/// </summary>
			FERR_CANNOT_RESERVE_ID		=0xC075,			
			/// <summary>
			/// Dictionary record with duplicate name found
			/// </summary>
			FERR_DUPLICATE_DICT_NAME		=0xC076,
			/// <summary>
			/// Attempted to reserve a dictionary name that is in use
			/// </summary>
			FERR_CANNOT_RESERVE_NAME		=0xC077,				
			/// <summary>
			/// Attempted to add, modify, or delete a dictionary DRN >= FLM_RESERVED_TAG_NUMS
			/// </summary>
			FERR_BAD_DICT_DRN				=0xC078,			
			/// <summary>
			/// Cannot modify a dictionary item into another type of item, must delete then add
			/// </summary>
			FERR_CANNOT_MOD_DICT_REC_TYPE	=0xC079,			
			/// <summary>
			/// Record contained 'purged' field
			/// </summary>
			FERR_PURGED_FLD_FOUND			=0xC07A,				
			//#define FERR_NU_C07B					0xC07B
			/// <summary>
			/// No session file handles could be closed - in to increase via FlmSetMaxPhysOpens()
			/// </summary>
			FERR_TOO_MANY_OPEN_FILES		=0xC07C,				
			/// <summary>
			/// Access Denied from setting in log header
			/// </summary>
			FERR_ACCESS_DENIED			=0xC07D,			
			/// <summary>
			/// Setting an access mode where paired mode is also set
			/// </summary>
			FERR_ACCESS_CONFLICT			=0xC07E,
			/// <summary>
			/// Cache Block is somehow corrupt
			/// </summary>
			FERR_CACHE_ERROR				=0xC07F,				
			//#define FERR_NU_C080					0xC080
			/// <summary>
			/// Missing BLOB file on add/modify
			/// </summary>
			FERR_BLOB_MISSING_FILE		=0xC081,			
			/// <summary>
			/// Record pointed to by an index key is missing
			/// </summary>
			FERR_NO_REC_FOR_KEY			=0xC082,				
			/// <summary>
			/// Database is full, cannot create more blocks
			/// </summary>
			FERR_DB_FULL					=0xC083,
			/// <summary>
			/// Query operation timed out
			/// </summary>
			FERR_TIMEOUT					=0xC084,				
			/// <summary>
			/// Cursor operation had improper syntax
			/// </summary>
			FERR_CURSOR_SYNTAX			=0xC085,			
			/// <summary>
			/// Thread Error
			/// </summary>
			FERR_THREAD_ERR				=0xC086,				
			/// <summary>
			/// File system is not supported for the requested operation
			/// </summary>
			FERR_SYS_CHECK_FAILED			=0xC087,
			/// <summary>
			/// Warning: Query has no results
			/// </summary>
			FERR_EMPTY_QUERY				=0xC088,				
			/// <summary>
			/// Warning: Index is offline and being rebuild
			/// </summary>
			FERR_INDEX_OFFLINE			=0xC089,			
			/// <summary>
			/// Warning: Can't evaluate truncated key against selection criteria
			/// </summary>
			FERR_TRUNCATED_KEY			=0xC08A,				
			/// <summary>
			/// Invalid parm
			/// </summary>
			FERR_INVALID_PARM				=0xC08B,
			/// <summary>
			/// User or application aborted the operation
			/// </summary>
			FERR_USER_ABORT				=0xC08C,			
			/// <summary>
			/// No space on RFL device for logging
			/// </summary>
			FERR_RFL_DEVICE_FULL			=0xC08D,
			/// <summary>
			/// Must wait for a checkpoint before starting transaction - due to disk problems - usually in RFL volume.
			/// </summary>
			FERR_MUST_WAIT_CHECKPOINT		=0xC08E,				
			/// <summary>
			/// Something bad happened with the named semaphore class (F_NamedSemaphore)
			/// </summary>
			FERR_NAMED_SEMAPHORE_ERR		=0xC08F,				
			/// <summary>
			/// Failed to load a shared library module
			/// </summary>
			FERR_LOAD_LIBRARY				=0xC090,				
			/// <summary>
			/// Failed to unload a shared library module
			/// </summary>
			FERR_UNLOAD_LIBRARY			=0xC091,			
			/// <summary>
			/// Failed to import a symbol from a shared library module
			/// </summary>
			FERR_IMPORT_SYMBOL			=0xC092,				

			/*============================================================================
									IO Errors
			============================================================================*/

			/// <summary>
			/// Access denied. Caller is not allowed access to a file.
			/// </summary>
			FERR_IO_ACCESS_DENIED			=0xC201,			
			/// <summary>
			/// Bad file handle
			/// </summary>
			FERR_IO_BAD_FILE_HANDLE		=0xC202,		
			/// <summary>
			/// Copy error
			/// </summary>
			FERR_IO_COPY_ERR				=0xC203,
			/// <summary>
			/// Disk full
			/// </summary>
			FERR_IO_DISK_FULL				=0xC204,			
			/// <summary>
			/// End of file
			/// </summary>
			FERR_IO_END_OF_FILE			=0xC205,		
			/// <summary>
			/// Error opening file
			/// </summary>
			FERR_IO_OPEN_ERR				=0xC206,
			/// <summary>
			/// File seek error
			/// </summary>
			FERR_IO_SEEK_ERR				=0xC207,			
			/// <summary>
			/// File modify error
			/// </summary>
			FERR_IO_MODIFY_ERR			=0xC208,		
			/// <summary>
			/// Path not found
			/// </summary>
			FERR_IO_PATH_NOT_FOUND		=0xC209,			
			/// <summary>
			/// Too many files open
			/// </summary>
			FERR_IO_TOO_MANY_OPEN_FILES	=0xC20A,			
			/// <summary>
			/// Path too long
			/// </summary>
			FERR_IO_PATH_TOO_LONG			=0xC20B,
			/// <summary>
			/// No more files in directory
			/// </summary>
			FERR_IO_NO_MORE_FILES			=0xC20C,			
			/// <summary>
			/// Had error deleting a file
			/// </summary>
			FERR_DELETING_FILE			=0xC20D,		
			/// <summary>
			/// File lock error
			/// </summary>
			FERR_IO_FILE_LOCK_ERR			=0xC20E,
			/// <summary>
			/// File unlock error
			/// </summary>
			FERR_IO_FILE_UNLOCK_ERR		=0xC20F,		
			/// <summary>
			/// Path create failed
			/// </summary>
			FERR_IO_PATH_CREATE_FAILURE	=0xC210,			
			/// <summary>
			/// File rename failed
			/// </summary>
			FERR_IO_RENAME_FAILURE		=0xC211,			
			/// <summary>
			/// Invalid file password
			/// </summary>
			FERR_IO_INVALID_PASSWORD		=0xC212,
			/// <summary>
			/// Had error setting up to do a read
			/// </summary>
			FERR_SETTING_UP_FOR_READ		=0xC213,			
			/// <summary>
			/// Had error setting up to do a write
			/// </summary>
			FERR_SETTING_UP_FOR_WRITE		=0xC214,			
			/// <summary>
			/// Currently positioned at the path root level
			/// </summary>
			FERR_IO_AT_PATH_ROOT			=0xC215,			
			/// <summary>
			/// Had error initializing the file system
			/// </summary>
			FERR_INITIALIZING_IO_SYSTEM	=0xC216,		
			/// <summary>
			/// Had error flushing a file
			/// </summary>
			FERR_FLUSHING_FILE			=0xC217,			
			/// <summary>
			/// Invalid path
			/// </summary>
			FERR_IO_INVALID_PATH			=0xC218,
			/// <summary>
			/// Failed to connect to a remote network resource
			/// </summary>
			FERR_IO_CONNECT_ERROR			=0xC219,			
			/// <summary>
			/// Had error opening a file
			/// </summary>
			FERR_OPENING_FILE				=0xC21A,			
			/// <summary>
			/// Had error opening a file for direct I/O
			/// </summary>
			FERR_DIRECT_OPENING_FILE		=0xC21B,			
			/// <summary>
			/// Had error creating a file
			/// </summary>
			FERR_CREATING_FILE			=0xC21C,		
			/// <summary>
			/// Had error creating a file for direct I/O
			/// </summary>
			FERR_DIRECT_CREATING_FILE		=0xC21D,
			/// <summary>
			/// Had error reading a file
			/// </summary>
			FERR_READING_FILE				=0xC21E,			
			/// <summary>
			/// Had error reading a file using direct I/O
			/// </summary>
			FERR_DIRECT_READING_FILE		=0xC21F,			
			/// <summary>
			/// Had error writing to a file
			/// </summary>
			FERR_WRITING_FILE				=0xC220,			
			/// <summary>
			/// Had error writing to a file using direct I/O
			/// </summary>
			FERR_DIRECT_WRITING_FILE		=0xC221,			
			/// <summary>
			/// Had error positioning within a file
			/// </summary>
			FERR_POSITIONING_IN_FILE		=0xC222,			
			/// <summary>
			/// Had error getting file size
			/// </summary>
			FERR_GETTING_FILE_SIZE		=0xC223,		
			/// <summary>
			/// Had error truncating a file
			/// </summary>
			FERR_TRUNCATING_FILE			=0xC224,
			/// <summary>
			/// Had error parsing a file name
			/// </summary>
			FERR_PARSING_FILE_NAME		=0xC225,		
			/// <summary>
			/// Had error closing a file
			/// </summary>
			FERR_CLOSING_FILE				=0xC226,
			/// <summary>
			/// Had error getting file information
			/// </summary>
			FERR_GETTING_FILE_INFO		=0xC227,		
			/// <summary>
			/// Had error expanding a file (using direct I/O)
			/// </summary>
			FERR_EXPANDING_FILE			=0xC228,			
			/// <summary>
			/// Had error getting free blocks from file system
			/// </summary>
			FERR_GETTING_FREE_BLOCKS		=0xC229,
			/// <summary>
			/// Had error checking if a file exists
			/// </summary>
			FERR_CHECKING_FILE_EXISTENCE	=0xC22A,			
			/// <summary>
			/// Had error renaming a file
			/// </summary>
			FERR_RENAMING_FILE			=0xC22B,		
			/// <summary>
			/// Had error setting file information
			/// </summary>
			FERR_SETTING_FILE_INFO		=0xC22C,			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		internal static bool IsError(Error error)
		{
			return (error == Error.FERR_OK ? false : true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		internal static bool IsSuccess(Error error)
		{
			return (error == Error.FERR_OK ? true : false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		internal static SimiasException GetException(Error error)
		{
			SimiasException e = null;
			switch (error)
			{
				case Error.FERR_OK:
					break;
				default:
					e = new SimiasException(string.Format("Flaim error {0} {1}", error, error.ToString()));
					break;
			}
			return e;
		}
	}
}
