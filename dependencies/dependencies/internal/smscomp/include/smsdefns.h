/*
============================================================
Novell Software Developer Kit Sample Code License

Copyright (C) 2003-2004 Novell, Inc.  All Rights Reserved.

THIS WORK IS SUBJECT TO U.S. AND INTERNATIONAL COPYRIGHT LAWS AND TREATIES.  
USE AND REDISTRIBUTION OF THIS WORK IS SUBJECT TO THE LICENSE AGREEMENT 
ACCOMPANYING THE SOFTWARE DEVELOPMENT KIT (SDK) THAT CONTAINS THIS WORK.  
PURSUANT TO THE SDK LICENSE AGREEMENT, NOVELL HEREBY GRANTS TO DEVELOPER 
A ROYALTY-FREE, NON-EXCLUSIVE LICENSE TO INCLUDE NOVELL'S SAMPLE CODE IN ITS 
PRODUCT.  NOVELL GRANTS DEVELOPER WORLDWIDE DISTRIBUTION RIGHTS TO MARKET, 
DISTRIBUTE, OR SELL NOVELL'S SAMPLE CODE AS A COMPONENT OF DEVELOPER'S PRODUCTS.
NOVELL SHALL HAVE NO OBLIGATIONS TO DEVELOPER OR DEVELOPER'S CUSTOMERS WITH 
RESPECT TO THIS CODE.

NAME OF FILE:
		smsdefns.h

PURPOSE/COMMENTS:
		Definitions for SMS, this file is included by SMS.H and should not be 
		included directly.
        
NDK COMPONENT NAME AND VERSION:
		SMS Developer Components

LAST MODIFIED DATE:
		23 Jul 2004	
===============================================================
*/


#if !defined(_SMSDEFNS_H_INCLUDED)
#define _SMSDEFNS_H_INCLUDED

#include <limits.h>

#if defined(STATIC)
#undef STATIC
#endif
#if defined(DEBUG_CODE)
        #define STATIC
#else
        #define STATIC static
#endif

#if !defined(TRUE)
        #define TRUE            1
        #define FALSE           0
#endif

#if !defined(loop)
        #define loop    for (;;)
        #define is              ==
        #define isnt    !=
        #define and             &&
        #define or              ||
        #define AND             &
        #define OR              |
#endif

#if !defined(NULL)
        #define NULL 0L
#endif

#ifdef N_PLAT_NLM
        #define UINT64_ZERO     { 0, 0, 0, 0 }
#elif defined(N_PLAT_UNIX)
		#define UINT64_ZERO		0
#endif
        #define UINT64_MAX      { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF }

/* Defines for NWSM_DATA_STREAM_TYPE fid */
        #define NWSM_CLEAR_TEXT_DATA_STREAM                     0x0
        #define NWSM_SPARSE_DATA_STREAM                         0x1
        #define NWSM_COMPRESSED_DATA_STREAM                     0x2
        #define NWSM_STUB_DATA_STREAM                           0x3

/* Defines for NWSM_DATA_STREAM_NUMBER fid */
        #define NWSM_PRIMARY_DATA_STREAM_NUM					0x0
        #define NWSM_MAC_RESOURCE_FORK_NUM                      0x1
        #define NWSM_FTAM_DATA_STREAM_NUM                       0x2
/* leave 3-9 for future expansion on NetWare */
        #define NWSM_EXTENDED_ATTR_STREAM_NUM         			0xA
        #define NWSM_NT_ALTERNATE_DATA                          0xB
        #define NWSM_NT_SECURITY_DATA                           0xC
        #define NWSM_NT_LINK_DATA 								0xD
        #define NWSM_NT_PROPERTY_DATA                           0xE

/* Defines for NWSM_MEDIA_MARK_TYPE fid  */
        #define NWSM_MEDIA_MARK_HARD                            0x0     // Both file and set marks
                                                                                                        // are done by the hardware
        #define NWSM_MEDIA_MARK_SOFT                            0x1     // Both file and set marks
                                                                                                        // are simulated with a
                                                                                                        // linked list of soft
                                                                                                        // media mark fids.
        #define NWSM_MEDIA_MARK_SIM_SET                         0x2     // File marks are done by
                                                                                                        // the hardware, set marks
                                                                                                        // are multiple consecutive
                                                                                                        // file marks.

/* Defines for address types of GetTargetServiceAddress */
        #define SPX             1
        #define TCPIP   2
        #define ADSP    4

        #define SPX_INTERNET_ADDRESS_LENGTH             10


/* Defines for versions of compression */
        #define NWSM_NOVELL_COMPRESSION_V1				0x1

/* Defines for NWSMTSGetUnsupportedOptions */
        #define NWSM_BACK_ACCESS_DATE_TIME				0x01
        #define NWSM_BACK_CREATE_DATE_TIME				0x02
        #define NWSM_BACK_MODIFIED_DATE_TIME            0x04
        #define NWSM_BACK_ARCHIVE_DATE_TIME 			0x08
        #define NWSM_BACK_SKIPPED_DATA_SETS				0x10

        #define NWSM_RESTORE_NEW_DATA_SET_NAME          0x01
        #define NWSM_RESTORE_CHILD_UPDATE_MODE          0x02
        #define NWSM_RESTORE_PARENT_UPDATE_MODE         0x04
  		#define NWSM_RESTORE_PARENT_HANDLE          	0x08


/* Generic selectionType defines */
        #define NWSM_TSA_DEFINED_RESOURCE_EXC           0x02
        #define NWSM_TSA_DEFINED_RESOURCE_INC           0x03
        #define NWSM_PARENT_TO_BE_EXCLUDED				0x04
        #define NWSM_PARENT_TO_BE_INCLUDED				0x05
        #define NWSM_CHILD_TO_BE_EXCLUDED				0x08
        #define NWSM_CHILD_TO_BE_INCLUDED				0x09
        #define NWSM_EXCLUDE_CHILD_BY_FULL_NAME         0x10
        #define NWSM_INCLUDE_CHILD_BY_FULL_NAME         0x11

/* Generic scanType defines */
        #define NWSM_DO_NOT_TRAVERSE					0x0001
        #define NWSM_EXCLUDE_ARCHIVED_CHILDREN          0x0002
        #define NWSM_EXCLUDE_HIDDEN_CHILDREN            0x0004
        #define NWSM_EXCLUDE_HIDDEN_PARENTS				0x0008
        #define NWSM_EXCLUDE_SYSTEM_CHILDREN            0x0010
        #define NWSM_EXCLUDE_SYSTEM_PARENTS				0x0020

        #define NWSM_EXCLUDE_CHILD_TRUSTEES				0x0040
        #define NWSM_EXCLUDE_PARENT_TRUSTEES            0x0080
        #define NWSM_EXCLUDE_ACCESS_DATABASE            0x0100
        #define NWSM_EXCLUDE_VOLUME_RESTS				0x0200
        #define NWSM_EXCLUDE_DISK_SPACE_RESTS           0x0400
        #define NWSM_EXCLUDE_EXTENDED_ATTRIBUTS         0x0800
        #define NWSM_EXCLUDE_DATA_STREAMS				0x1000
        #define NWSM_EXCLUDE_MIGRATED_CHILD				0x2000
/* NWSM_EXCLUDE_MIGRATED_CHILD means to exclude the data stream, but include
all other information about the file */
        #define NWSM_EXPAND_COMPRESSED_DATA				0x4000
        #define NWSM_EXCLUDE_ARCH_CHILD_DATA            0x8000
        #define NWSM_EXCLUDE_ARCH_CHILD_CHAR 		    0x10000
        #define NWSM_FLAG_PURGE_IMMED_ON_DELETE		    0x20000
        #define NWSM_EXCLUDE_MIGRATED_FILES				0x40000

/* NDS tsa scan type "Exclude objects for which user has no rights."*/
        #define NWSM_NDS_STOP_SCAN_IF_NO_RIGHTS			0x80000000


/*      º Open modes for backup and restore, passed to NWSMOpenDataSetForBackup	   º
        º      and NWSMOpenDataSetForRestore                                      º
        º                                                                         º
        º     Backup                             Restore                          º
        º SMS Numeric modes                                                       º
        º                                                                         º
        º 1:   NWSM_USE_LOCK_MODE_IF_DW_FAILS    NWSM_OVERWRITE_DATA_SET          º
        º 2:   NWSM_NO_LOCK_NO_PROTECTION        NWSM_DO_NOT_OVERWRITE_DATA_SET   º
        º 3:   Reserved                          NWSM_CREATE_PARENT_HANDLE        º
        º 4:   Reserved                          NWSM_UPDATE_DATA_SET             º
        º 5 to 15:                      Reserved                                  º
        º SMS Non-numeric mode options, ie., can be added (ORed) to numeric mode  º
        º                                                                         º
        º 0010                          	 	 Reserved                     	  º
        º 0020                          		 Reserved                         º
        º 0040 Reserved                          NWSM_CLEAR_MODIFY_FLAG           º
        º 0080 Reserved                          NWSM_RESTORE_MODIFY_FLAG         º

        º TSA Specific Non-numeric mode options, i.e., can be added (logical OR)	º
        º     to numeric mode and SMS Non-numeric options.                        º
        º                                                                         º
        º 0100 NWSM_NO_DATA_STREAMS              NWSM_NO_DATA_STREAMS             º
        º 0200 NWSM_NO_EXTENDED_ATTRIBUTES       NWSM_NO_EXTENDED_ATTRIBUTES      º
        º 0400 NWSM_NO_PARENT_TRUSTEES           NWSM_NO_PARENT_TRUSTEES          º
        º 0800 NWSM_NO_CHILD_TRUSTEES            NWSM_NO_CHILD_TRUSTEES           º
        º 1000 NWSM_NO_VOLUME_RESTRICTIONS       NWSM_NO_VOLUME_RESTRICTIONS      º
        º 2000 NWSM_NO_DISK_SPACE_RESTRICTIONS   NWSM_NO_DISK_SPACE_RESTRICTIONS  º
        º 4000 N/A                               NWSM_INCLUDE_MIGRATED_DATA               º
        º 8000 N/A                               NWSM_DELETE_EXISTING_TRUSTEES    º
        º10000 NWSM_EXPAND_COMPRESSED_DATA_SET      N/A                                                      º
        º20000 NWSM_EXCLUDE_MIGRATED_DATA           N/A                                                          º
        º 00040000 to 80000000:         Reserved                                  º
*/

/* Mask for numeric modes, backup and restore */
        #define NWSM_OPEN_MODE_MASK                             0x000F

/* NWSMOpenDataSetForBackup modes */
        #define NWSM_OPEN_READ_DENY_WRITE                       0x0000
        #define NWSM_USE_LOCK_MODE_IF_DW_FAILS     			    0x0001
        #define NWSM_NO_LOCK_NO_PROTECTION                      0x0002
        #define NWSM_OPEN_READ_ONLY           					0x0003

/* TSA Specific Non-numeric mode options */
        #define NWSM_NO_DATA_STREAMS                            0x0100 /* Also used for Restore */
        #define NWSM_NO_EXTENDED_ATTRIBUTES                     0x0200 /* Also used for Restore */
        #define NWSM_NO_PARENT_TRUSTEES                         0x0400 /* Also used for Restore */
        #define NWSM_NO_CHILD_TRUSTEES                          0x0800 /* Also used for Restore */
        #define NWSM_NO_VOLUME_RESTRICTIONS                     0x1000 /* Also used for Restore */
        #define NWSM_NO_DISK_SPACE_RESTRICTIONS         		0x2000 /* Also used for Restore */
        #define NWSM_INCLUDE_MIGRATED_DATA                      0x4000 /* Also used for Restore */
/*      #define NWSM_DELETE_EXISTING_TRUSTEES           		0x8000 */
        #define NWSM_EXPAND_COMPRESSED_DATA_SET   				0x10000
        #define NWSM_EXCLUDE_MIGRATED_DATA         				0x20000

/*      NWSMOpenDataSetForRestore modes */
        #define NWSM_OVERWRITE_DATA_SET                         0x0001
        #define NWSM_DO_NOT_OVERWRITE_DATA_SET          		0x0002
        #define NWSM_CREATE_PARENT_HANDLE                       0x0003
        #define NWSM_UPDATE_DATA_SET                            0x0004

        #define NWSM_SET_MODIFY_FLAG_RESTORE            		0x0000
        #define NWSM_CLEAR_MODIFY_FLAG_RESTORE          		0x0040
        #define NWSM_RESTORE_MODIFY_FLAG                        0x0080

/* TSA Specific Non-numeric mode options */
/*      #define NWSM_NO_DATA_STREAMS                            0x0100  */
/*      #define NWSM_NO_EXTENDED_ATTRIBUTES                     0x0200  */
/*      #define NWSM_NO_PARENT_TRUSTEES                         0x0400  */
/*      #define NWSM_NO_CHILD_TRUSTEES                          0x0800  */
/*      #define NWSM_NO_VOLUME_RESTRICTIONS                     0x1000  */
/*      #define NWSM_NO_DISK_SPACE_RESTRICTIONS         		0x2000  */ 
/*      #define NWSM_INCLUDE_MIGRATED_DATA                      0x4000  */
        #define NWSM_DELETE_EXISTING_TRUSTEES           		0x8000  
/*      #define NWSM_EXPAND_COMPRESSED_DATA_SET  				0x10000 */
/*      #define NWSM_EXCLUDE_MIGRATED_DATA         				0x20000 */

/* NWSMTSSetArchiveStatus setFlag defines */
        #define NWSM_CLEAR_MODIFY_FLAG                          0x01
        #define NWSM_SET_ARCHIVE_DATE_AND_TIME          		0x02
        #define NWSM_SET_ARCHIVER_ID                            0x04
        #define NWSM_CLEAR_MOD_DATA_FLAG_ONLY       			0x08
        #define NWSM_CLEAR_MOD_CHAR_FLAG_ONLY       			0x10

/* Buffer Lengths */
        #define NWSM_MAX_DESCRIPTION_LEN                        80
        #define NWSM_OLD_MAX_RESOURCE_LEN                       30 /* to allow TSAs to work */
                                                                                                                // with old smdr and engines
        #define NWSM_MAX_RESOURCE_LEN                           256 /* MAX_DN_CHARS in nwdsdefs.h */
        #define NWSM_MAX_STRING_LEN                        		60
        #define NWSM_MAX_TARGET_SRVC_NAME_LEN           		48
        #define NWSM_MAX_TARGET_SRVC_TYPE_LEN           		40
        #define NWSM_MAX_TARGET_SRVC_VER_LEN            		10
        #define NWSM_MAX_SOFTWARE_NAME_LEN                      80
        #define NWSM_MAX_SOFTWARE_TYPE_LEN                      40
        #define NWSM_MAX_SOFTWARE_VER_LEN                       10
        #define NWSM_MAX_TARGET_USER_NAME_LEN           		256 /* MAX_DN_CHARS in nwdsdefs.h */
        #define NWSM_MAX_ERROR_STRING_LEN                       255
        #define NWSM_MAX_MM_MODULE_LABEL_LEN            		64
        #define NWSM_MAX_DEVICE_LABEL_LEN                       64
        #define NWSM_MAX_MEDIA_LABEL_LEN                        64

        #define EndChar(p, b)           strchr((PSTRING)(b), *LastChar(p))
      	#if defined(N_PLAT_NLM)
			#define LastChar(p)                     (NWPrevChar(p, (&(p)[strlen((PSTRING)p)])))
		#elif defined(N_PLAT_UNIX)
			#define LastChar(p)                     (&((unsigned char *)p)[strlen((PSTRING)p) - 1])
      	#endif
        #define StrEnd(p)                       (&(p)[strlen(p)])
        #define StrEqu                          !strcmp
        #define StrNEqu                         !strncmp
        #define _min(a, b)                      ((a) < (b)) ? (a) : (b)

/* NWSMTSConnectToTargetServiceEx options */

		#define NWSM_AUTH_LOCAL_DATA				1
		#define NWSM_AUTH_RAW_DATA					NWSM_AUTH_LOCAL_DATA
		#define NWSM_AUTH_UNICODE_DATA				2
		#define NWSM_AUTH_UTF8_DATA					4

		typedef enum 
		{
			NWSM_TYPE_UINT32				=   0x00000001, 
			NWSM_TYPE_ASCIIZ_STRING			=   0x00000002,
			NWSM_TYPE_UNICODEZ_STRING		=   0x00000003,
			NWSM_TYPE_END					= 	INT_MAX
		}NWSMTypes;


/* NWSMTSConfigureTargetService actionFlag defines */

		#define SMS_ACTION_FLAG_TYPE(type, flag)    	((type << 16) | flag)

		#define CONFIGURE_SKIPPED_LOG               	SMS_ACTION_FLAG_TYPE(NWSM_TYPE_ASCIIZ_STRING, 0x0001)
		#define CONFIGURE_ERROR_LOG                 	SMS_ACTION_FLAG_TYPE(NWSM_TYPE_ASCIIZ_STRING, 0x0002)
		#define FLUSH_LOG_FILES							SMS_ACTION_FLAG_TYPE(NWSM_TYPE_UINT32, 0x0001)
		#define DONOT_IGNORE_BACKUP_BIT             	SMS_ACTION_FLAG_TYPE(NWSM_TYPE_UINT32, 0x0002)
		#define USE_CACHING_MODE						SMS_ACTION_FLAG_TYPE(NWSM_TYPE_UINT32, 0x0003)
		#define MAP_NEW_TREE_NAMES_ONLY 				SMS_ACTION_FLAG_TYPE(NWSM_TYPE_UINT32, 0x0004)
		
#endif
