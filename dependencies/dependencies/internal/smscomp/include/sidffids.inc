/*
===============================================================================
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
	sidffids.inc 

PURPOSE/COMMENTS:
	This file is NOVELL's implementation of the SIDF fids, 
	and IS the current SIDF Association standard. 

NDK COMPONENT NAME AND VERSION:
	SMS Developer Components

LAST MODIFIED DATE: 
	16 Feb 1996

===============================================================================
*/

/*  Available Standard FIDs: */
/*    Small Variable Size: (0x00)     0x23, 0x2F - 0x3F */
/*    Small Fixed Size:    1-Byte     ---- */
/*                         2-Byte     0x71 */
/*                         4-Byte     ---- */
/*                         8-Byte     0x43, 0x53, 0x63, 0x73 */
/*                         16-Byte    ---- */
/*                         32-Byte    0x55, 0x65, 0x75 */
/*                         64-Byte    0x46, 0x56, 0x66, 0x76 */
/*                         128-Byte   0x47, 0x57, 0x67, 0x77 */
/*                         256-Byte   0x48, 0x58, 0x68, 0x78 */
/*                         512-Byte   0x49, 0x59, 0x69, 0x79 */
/*                         1K-Byte    0x4A, 0x5A, 0x6A, 0x7A */
/*                         2K-Byte    0x4B, 0x5B, 0x6B, 0x7B */
/*                         4K-Byte    0x4C, 0x5C, 0x6C, 0x7C */
/*                         8K-Byte    0x4D, 0x5D, 0x6D, 0x7D */
/*                         16K-Byte   0x4E, 0x5E, 0x6E, 0x7E */
/*                         32K-Byte   0x4F, 0x5F, 0x6F, 0x7F */

/*    Short Variable Size: (0x80)     0x0D - 0x3F */
/*    Short Fixed Size:    1-Byte     0x40, 0x50, 0x60, 0x70 */
/*                         2-Byte     0x41, 0x51, 0x61, 0x71 */
/*                         4-Byte     ---- */
/*                         8-Byte     0x43, 0x53, 0x63, 0x73 */
/*                         16-Byte    0x64, 0x74 */
/*                         32-Byte    0x45, 0x55, 0x65, 0x75 */
/*                         64-Byte    0x46, 0x56, 0x66, 0x76 */
/*                         128-Byte   0x47, 0x57, 0x67, 0x77 */
/*                         256-Byte   0x48, 0x58, 0x68, 0x78 */
/*                         512-Byte   0x49, 0x59, 0x69, 0x79 */
/*                         1K-Byte    0x4A, 0x5A, 0x6A, 0x7A */
/*                         2K-Byte    0x4B, 0x5B, 0x6B, 0x7B */
/*                         4K-Byte    0x4C, 0x5C, 0x6C, 0x7C */
/*                         8K-Byte    0x4D, 0x5D, 0x6D, 0x7D */
/*                         16K-Byte   0x4E, 0x5E, 0x6E, 0x7E */
/*                         32K-Byte   0x4F, 0x5F, 0x6F, 0x7F */

/*    Long Variable Size: (0x80)      0x8047 - 0xEFFF */
/*    Long Fixed Size:     1-Byte     0xF004 - 0xF0FF */
/*                         2-Byte     0xF103, 0xF105 - 0xF1FF */
/*                         4-Byte     0xF211 - 0xF2FF */
/*                         8-Byte     0xF301 - 0xF3FF */
/*                         16-Byte    0xF405 - 0xF4FF */
/*                         32-Byte    0xF500 - 0xF5FF */
/*                         64-Byte    0xF600 - 0xF6FF */
/*                         128-Byte   0xF700 - 0xF7FF */
/*                         256-Byte   0xF800 - 0xF8FF */
/*                         512-Byte   0xF900 - 0xF9FF */
/*                         1K-Byte    0xFA00 - 0xFAFF */
/*                         2K-Byte    0xFB00 - 0xFBFF */
/*                         4K-Byte    0xFC00 - 0xFCFF */
/*                         8K-Byte    0xFD00 - 0xFDFF */
/*                         16K-Byte   0xFE00 - 0xFEFF */
/*                         32K-Byte   0xFF00 - 0xFFFF */


/*  Available Novell FIDs: */
/*    Short Variable Size: (0x81)     0x00 - 0x9, */
/*    Short Fixed Size:    1-Byte     0x40, 0x50, 0x60, 0x70 */
/*                         2-Byte     0x41, 0x51, 0x61, 0x71 */
/*                         4-Byte     0x62, 0x72 */
/*                         8-Byte     0x43, 0x53, 0x63, 0x73 */
/*                         16-Byte    0x44, 0x54, 0x64, 0x74 */
/*                         32-Byte    0x45, 0x55, 0x65, 0x75 */
/*                         64-Byte    0x46, 0x56, 0x66, 0x76 */
/*                         128-Byte   0x47, 0x57, 0x67, 0x77 */
/*                         256-Byte   0x48, 0x58, 0x68, 0x78 */
/*                         512-Byte   0x49, 0x59, 0x69, 0x79 */
/*                         1K-Byte    0x4A, 0x5A, 0x6A, 0x7A */
/*                         2K-Byte    0x4B, 0x5B, 0x6B, 0x7B */
/*                         4K-Byte    0x4C, 0x5C, 0x6C, 0x7C */
/*                         8K-Byte    0x4D, 0x5D, 0x6D, 0x7D */
/*                         16K-Byte   0x4E, 0x5E, 0x6E, 0x7E */
/*                         32K-Byte   0x4F, 0x5F, 0x6F, 0x7F */

/*    Long Variable Size: (0x81)      0x8000 - 0xEFCD */
/*    Long Fixed Size:     1-Byte     0xF000 - 0xF0FB */
/*                         2-Byte     0xF100 - 0xF1FF */
/*                         4-Byte     0xF200 - 0xF2F5 */
/*                         8-Byte     0xF300 - 0xF3FF */
/*                         16-Byte    0xF400 - 0xF4FF */
/*                         32-Byte    0xF500 - 0xF5FF */
/*                         64-Byte    0xF600 - 0xF6FF */
/*                         128-Byte   0xF700 - 0xF7FF */
/*                         256-Byte   0xF800 - 0xF8FF */
/*                         512-Byte   0xF900 - 0xF9FF */
/*                         1K-Byte    0xFA00 - 0xFAFF */
/*                         2K-Byte    0xFB00 - 0xFBFF */
/*                         4K-Byte    0xFC00 - 0xFCFF */
/*                         8K-Byte    0xFD00 - 0xFDFF */
/*                         16K-Byte   0xFE00 - 0xFEFF */
/*                         32K-Byte   0xFF00 - 0xFFFF */
/*  */
/* Extended Developer FIDS  0xC002 - 0xFFFF */
/*  */
/*        Palindrome      0xC000 */
/*        Mountain        0xC001 */
/*  */
/*  */


/*  General Fields */
    #define SIDF_OFFSET_TO_END                0x00000001L
    #define SIDF_HEADER_DEBUG_STRING          0x0081EFFFL
    #define SIDF_BLANK_SPACE                  0x00808019L
    #define SIDF_CODE_PAGE                    0x0000002AL

/*  Media Header/ Trailer fields */
    #define SIDF_PARTITION_HEADER             0x00808000L
    #define SIDF_REVISION_LEVEL               0x0080F300L
    #define SIDF_NAME_ID_STRING               0x00008052L
    #define SIDF_VERSION_NUMBER               0x00008062L
    #define SIDF_PARTITION_SET_DATE_TIME      0x0080F400L
    #define SIDF_PARTITION_DATE_TIME          0x0080F401L
    #define SIDF_PARTITION_SET_LABEL          0x00808030L
    #define SIDF_PARTITION_ALTERNATE_LABEL    0x00808027L
    #define SIDF_PARTITION_SET_SEQUENCE       0x0080F100L
    #define SIDF_MEDIA_TYPE                   0x0080F101L
    #define SIDF_PRIOR_MEDIA_TYPE             0x00808026L
    #define SIDF_PARTITION_SIZE               0x0080F201L
    #define SIDF_EXPIRATION_DATE              0x0080F404L
    #define SIDF_MEDIA_USAGE_COUNT            0x0080F104L
    #define SIDF_CRC_TYPE                     0x00808002L
    #define SIDF_PARTITION_TRAILER            0x00808003L
    #define SIDF_CLOSE_DATE_TIME              0x0080F402L
    #define SIDF_SECTOR_SIZE                  0x0080800EL
    #define SIDF_MEDIA_MARK_FILESTONE         0x00808028L
    #define SIDF_MEDIA_MARK_USAGE             0x00808020L
    #define SIDF_DEVICE_INFO                  0x00808032L
    #define SIDF_PARTITION_INDEX_REQUIRED     0x0080802FL
    #define SIDF_SESSION_INDEX_PRESENT        0x0080802DL
    #define SIDF_CHAR_SPEC                    0x00808040L
    #define SIDF_VOLUME_SET_ALIAS             0x00808041L
    #define SIDF_VOLUME_SET_LOCATION          0x00808042L

/*  Session Header Fields */
    #define SIDF_SESSION_HEADER               0x00808004L
    #define SIDF_SESSION_ID                   0x00008072L
    #define SIDF_SESSION_SUB_HEADER           0x00808035L
    #define SIDF_SESSION_DATE_TIME            0x0080F403L
    #define SIDF_SESSION_LABEL                0x00808005L
    #define SIDF_SESSION_COMMENT              0x0080802BL
    #define SIDF_SOFTWARE_NAME                0x00808006L
    #define SIDF_SOFTWARE_TYPE                0x00808007L
    #define SIDF_SOFTWARE_VERSION             0x00808008L
    #define SIDF_SOURCE_NAME                  0x00000002L
    #define SIDF_SOURCE_NAME_ALIAS            0x00808036L
    #define SIDF_SOURCE_NAME_TYPE             0x00008009L
    #define SIDF_SOURCE_OS                    0x00000003L
    #define SIDF_SOURCE_OS_VERSION            0x00000004L
    #define SIDF_NEXT_OBJECT_LOCATION         0x00808029L
    #define SIDF_PREV_OBJECT_LOCATION         0x0080802AL
    #define SIDF_VENDOR_SPECIFIC_SESSION      0x0080802EL

/*  Session Trailer */
    #define SIDF_SESSION_TRAILER              0x00808009L
    #define SIDF_SESSION_ABORTED              0x00808039L

/*  Transfer Buffer Header */
    #define SIDF_TRANSFER_BUFFER_HEADER       0x00000005L
    #define SIDF_TRANSFER_BUFFER_SIZE         0x00000006L
    #define SIDF_UNUSED_IN_THIS_BUFFER        0x00008000L
    #define SIDF_TRANSFER_BUFFER_SEQUENCE     0x00000007L
    #define SIDF_TRANSFER_BUFFER_CRC          0x00008008L
    #define SIDF_TRANSFER_BUFFER_TYPE         0x00000060L
    #define SIDF_TRANSFER_BUFFER_ADDRESS      0x00000008L

/*  Session/Media Index */
    #define SIDF_SESSION_INDEX                0x00808010L
    #define SIDF_SESSION_SUB_INDEX            0x00808033L
    #define SIDF_PARTITION_INDEX              0x00808011L
    #define SIDF_PARTITION_SUB_INDEX          0x00808031L
    #define SIDF_PARTITION_NUMBER             0x00808012L
    #define SIDF_TRANSFER_BUFFER_OFFSET       0x00808014L
    #define SIDF_NUMBER_OF_SESSIONS           0x00808015L
    #define SIDF_NUMBER_OF_DATA_SETS          0x00808021L
    #define SIDF_TOTAL_SESSION_SIZE           0x00808022L
    #define SIDF_RESOURCE_TYPE                0x00808037L
    #define SIDF_RESOURCE_NAME_SPACE          0x00808038L
    #define SIDF_RESOURCE_NAME                0x00808023L
    #define SIDF_SESSION_INDEX_FIELDS         0x00808034L

    #define SIDF_SH_PARTITION_SET_SEQUENCE    0x0080803AL
    #define SIDF_SH_PARTITION_NUMBER          0x0080803BL
    #define SIDF_SESSION_HEADER_LOCATION      0x0080803CL


    #define SIDF_ST_PARTITION_SET_SEQUENCE    0x0080803DL
    #define SIDF_ST_PARTITION_NUMBER          0x0080803EL
    #define SIDF_SESSION_TRAILER_LOCATION     0x0080803FL


/*  Data Set Header */
    #define SIDF_DATA_SET_HEADER              0x00000009L
    #define SIDF_DATA_SET_CHUNK_SIZE          0x0000000BL

/*  Data Set Sub Header */
    #define SIDF_DATA_SET_SUB_HEADER          0x00008001L

/*  Directory Information */
    #define SIDF_DIRECTORY_ATTRIBUTE          0x00008142L
    #define SIDF_DATA_SET_INFORMATION         0x0000813FL
    #define SIDF_SCAN_INFO_ATTRIBUTES         0x0081F2FEL
    #define SIDF_SCAN_INFO_CREATOR_ID         0x0081F2FDL
    #define SIDF_SCAN_INFO_CREATOR_NS         0x0081F2FCL
    #define SIDF_SCAN_INFO_PRIM_DS_SIZE       0x0081F2FBL
    #define SIDF_SCAN_INFO_TOTAL_DS_SIZE      0x0081F2FAL
    #define SIDF_SCAN_INFO_MODIFY_FLAG        0x0081F0FFL
    #define SIDF_SCAN_INFO_DELETE_FLAG        0x0081F0FEL
    #define SIDF_SCAN_INFO_PARENT_FLAG        0x0081F0FDL
    #define SIDF_SCAN_INFO_OTHER_INFO         0x0081EFFDL

/*  Record Headers/Trailers */
    #define SIDF_VOLUME_HEADER                0x0081EFFCL
    #define SIDF_VOLUME_TRAILER               0x0081EFFBL
    #define SIDF_DELTA_EXTENT                 0x0000800AL
    #define SIDF_DELTA_EXTENT_OLD_SIZE        0x0000800BL
    #define SIDF_DELTA_START_TIME             0x00008044L
    #define SIDF_DELTA_END_TIME               0x00008054L
    #define SIDF_DIRECTORY_HEADER             0x0000000CL
    #define SIDF_DIRECTORY_TRAILER            0x0000000DL
    #define SIDF_FILE_HEADER                  0x0000000EL
    #define SIDF_FILE_TRAILER                 0x0000000FL
    #define SIDF_BINDERY_HEADER               0x0081EFFAL
    #define SIDF_BINDERY_TRAILER              0x0081EFF9L
    #define SIDF_286_BINDERY_HEADER           0x0081EFEBL
    #define SIDF_286_BINDERY_TRAILER          0x0081EFEAL
    #define SIDF_REGISTRATION_ID              0x00808043L
    #define SIDF_VOLUME_INTERCHANGE_LEVEL     0x00808044L
    #define SIDF_SERVER_SPECIFIC_HEADER       0x0081EFCFL
    #define SIDF_SERVER_SPECIFIC_TRAILER      0x0081EFCEL

    #define SIDF_BACKUP_OPTIONS               0x00000029L

/*  Full Paths Section */
    #define SIDF_FULL_PATHS                   0x00000010L
    #define SIDF_PATH_IS_FULLY_QUALIFIED      0x00000050L
    #define SIDF_NAME_SPACE_TYPE              0x00000011L
    #define SIDF_NAME_POSITIONS               0x00000027L
    #define SIDF_SEPARATOR_POSITIONS          0x00000028L
    #define SIDF_PATH_NAME                    0x00000012L

/*  Characteristics Section */
    #define SIDF_CHARACTERISTICS              0x00000013L
    #define SIDF_ACCESS_DATE_TIME             0x00000044L
    #define SIDF_ARCHIVER_ID                  0x0000813DL
    #define SIDF_ARCHIVER_NAME                0x00008117L
    #define SIDF_ARCHIVE_DATE_TIME            0x00000054L
    #define SIDF_AUDIT_READ_AND_WRITE         0x0000813CL
    #define SIDF_CREATION_DATE_TIME           0x00000064L
    #define SIDF_DIRECTORY                    0x00000014L
    #define SIDF_EXECUTE_ONLY                 0x0000813BL
    #define SIDF_FILE_IS_INVALID              0x0080F003L
    #define SIDF_FILE_TYPE                    0x00000070L
    #define SIDF_HIDDEN                       0x00000015L
    #define SIDF_INDEXED                      0x0081EFF8L
    #define SIDF_INHERITED_RIGHTS_MASK        0x00008152L
    #define SIDF_INHIBIT_REN_DEL_COPY         0x0000813AL
    #define SIDF_LOW_MED_HIGH_SEARCH          0x00008139L
    #define SIDF_MODIFIER_ID                  0x00008138L
    #define SIDF_MODIFIER_NAME                0x00008120L
    #define SIDF_MODIFY_DATE_TIME             0x00000074L
    #define SIDF_NEEDS_ARCHIVE                0x00000016L
    #define SIDF_NEEDS_ARCHIVE_CHAR           0x0000002DL
    #define SIDF_OWNER_ID                     0x00008137L
    #define SIDF_OWNER_NAME                   0x00008119L
    #define SIDF_PURGE                        0x00008136L
    #define SIDF_READ_ONLY                    0x00000017L
    #define SIDF_SHAREABLE                    0x00000018L
    #define SIDF_SYSTEM                       0x00000019L
    #define SIDF_STREAM_TYPE_SEQUENCE         0x00000061L
    #define SIDF_TRANSACTIONAL                0x00008135L
    #define SIDF_REMOTE_DATA_INHIBIT          0x0081EFE8L
    #define SIDF_REMOTE_DATA_ACCESS           0x0081EFE7L
    #define SIDF_COMPRESS_FILE_IMMEDIATE      0x00008116L
    #define SIDF_DO_NOT_COMPRESS_FILE         0x00008115L
    #define SIDF_CANT_COMPRESS_DATA           0x0081EFE6L
    #define SIDF_OTHER_CHARACTERISTICS        0x00008134L
    #define SIDF_DIR_SPACE_RESTRICTIONS       0x00008133L
    #define SIDF_SUPPORT_MODULE_ROOT_BIT      0x00008121L
    #define SIDF_DONT_SUB_ALLOCATE_BIT        0x0081EFE5L

/*  MAC Name Space Specific Characteristics */
    #define SIDF_MAC_CHARACTERISTICS          0x00000026L
    #define SIDF_MAC_FINDER_INFO              0x00000045L
    #define SIDF_MAC_PRO_DOS_INFO             0x0000812DL
    #define SIDF_MAC_DIR_RIGHTS_MASK          0x0000812CL
    #define SIDF_MAC_SYSTEM_SCRIPT            0x00008003L
    #define SIDF_MAC_BLESSED_FOLDER           0x00008004L
    #define SIDF_MAC_NAME_LOCKED              0x0000800CL

/*  NFS Name Space Specific Characteristics */
    #define SIDF_NFS_CHARACTERISTICS          0x0080800AL
    #define SIDF_NFS_FILE_ACCESS_MODE         0x0080F203L
    #define SIDF_NFS_GROUP_OWNER_ID           0x0080F204L
    #define SIDF_NFS_LINKED_FLAG              0x0080F000L
    #define SIDF_NFS_FIRST_CREATOR_FLAG       0x0081F0FCL
    #define SIDF_NFS_HARD_LINK_PATHS          0x0080F001L
    #define SIDF_NFS_HL_PATHNAME              0x0080800BL
    #define SIDF_NFS_HL_USER_ID               0x0080800CL
    #define SIDF_NFS_HL_GROUP_OWNER_ID        0x0080F207L
    #define SIDF_NFS_HL_FILE_ACCESS_MODE      0x0080F208L
    #define SIDF_NFS_HL_TERMINATOR            0x0080800DL
    #define SIDF_NFS_USER_ID                  0x0080F209L
    #define SIDF_NFS_MY_FLAGS                 0x0080F20AL
    #define SIDF_NFS_ACS_FLAGS                0x0080F002L
    #define SIDF_NFS_NUMBER_OF_LINKS          0x0080F20DL
    #define SIDF_NFS_RDEVICE                  0x0080F20EL
    #define SIDF_NFS_FSID                     0x0080F20FL
    #define SIDF_NFS_FILEID                   0x0080F210L
    #define SIDF_POSIX_FILEID                 0x00808046L


/*  Trustees */
    #define SIDF_TRUSTEE_HEADER               0x00008131L
    #define SIDF_TRUSTEE_ID                   0x00008130L
    #define SIDF_TRUSTEE_NAME                 0x00008118L
    #define SIDF_TRUSTEE                      0x0000812FL

/*  Volume Restrictions */
    #define SIDF_VOLUME_RESTRICTIONS          0x0081EFF7L
    #define SIDF_VOLUME_NAME                  0x0081EFF6L
    #define SIDF_VOLUME_RSTRCTNS_ID           0x0081EFF5L
    #define SIDF_VOLUME_RSTRCTNS_NAME         0x0081EFE9L
    #define SIDF_VOLUME_RSTRCTNS_LIMIT        0x0081EFF4L

/*  Data Streams Sections */
    #define SIDF_DATA_STREAM_HEADER           0x0000001DL
    #define SIDF_DATA_STREAM_TRAILER          0x0000001EL
    #define SIDF_DATA_STREAM_NAME             0x0000001FL
    #define SIDF_DATA_STREAM_NUMBER           0x0000002BL
    #define SIDF_DATA_STREAM_TYPE             0x0000002CL
    #define SIDF_DATA_STREAM_SIZE             0x00000020L
    #define SIDF_DATA_STREAM_IS_INVALID       0x00000021L
    #define SIDF_DATA_STREAM_CRC              0x00000022L
    #define SIDF_DATA_STREAM_COMPRESS_TYPE    0x00008005L
    #define SIDF_DATA_STREAM_EXPANDED_SIZE    0x00008006L
    #define SIDF_DATA_STREAM_MIGRATION_KEY    0x00008007L
    #define SIDF_DATA_STREAM_ATTRIBUTES       0x00808045L


/*  Sparse Data Streams Sections */
    #define SIDF_SPARSE_DATA_BLOCK_SIZE       0x00000024L
    #define SIDF_SPARSE_DATA_STREAM_MAP       0x00000025L
    #define SIDF_EXTEND_SPARSE_DATA_STREAM_ENTRY_SIZE       0x00000026L
    #define SIDF_EXTEND_SPARSE_DATA_STREAM_MAP       0x00000027L

/*  Extended Attributes Data Streams Sections */
    #define SIDF_EA_ACCESS                    0x00008132L
    #define SIDF_EA_KEY                       0x0000001BL

/*  BTrieve TSA Section */
    #define SIDF_TS_HEADER                    0x0081EFF3L
    #define SIDF_TS_TRAILER                   0x0081EFF2L
    #define SIDF_TS_NAME                      0x0081EFEFL
    #define SIDF_TS_TYPE                      0x0081EFEEL

/*  Old Directory Services TSA data */
    #define SIDF_DIRECTORY_OBJECT_NAME        0x00008114L
    #define SIDF_DIRECTORY_OBJECT_DATA        0x00008113L
    #define SIDF_DIRECTORY_OBJECT_TRAILER     0x0000811AL

/* New Section format for Directory Services TSA */

    #define SIDF_DS_OBJECT_HEADER             0x0081EFE4L
    #define SIDF_DS_VERSION                   0x0081EFE3L

/*  DS names will be put out with SIDF_FULL_PATHS, SIDF_NAME_SPACE_TYPE and */
/*  SIDF_PATH_NAME  */

    #define SIDF_DS_DATA                      0x00008112L
    #define SIDF_DS_DATA_SEQUENCE             0x00008111L
    #define SIDF_DS_DATA_CHUNK                0x00008110L

    #define SIDF_DS_OBJECT_TRAILER            0x0081EFE2L

    #define SIDF_SCHEMA_HEADER                0x0081EFE1L

    #define SIDF_SCHEMA_ATTRIBUTE             0x0081EFE0L
    #define SIDF_SCHEMA_ATTR_NAME             0x0081EFDFL
    #define SIDF_SCHEMA_ATTR_FLAG             0x0081EFDEL
    #define SIDF_SCHEMA_ATTR_SYNTAX_ID        0x0081EFDDL
    #define SIDF_SCHEMA_ATTR_LOWER_LIMIT      0x0081EFDCL
    #define SIDF_SCHEMA_ATTR_UPPER_LIMIT      0x0081EFDBL
    #define SIDF_SCHEMA_ATTR_ASN1ID           0x0081EFDAL

    #define SIDF_SCHEMA_CLASS                 0x0081EFD9L
    #define SIDF_SCHEMA_CLASS_NAME            0x0081EFD8L
    #define SIDF_SCHEMA_CLASS_FLAGS           0x0081EFD7L
    #define SIDF_SCHEMA_CLASS_ASN1ID          0x0081EFD6L
    #define SIDF_SCHEMA_SUPER_CLASS           0x0081EFD5L
    #define SIDF_SCHEMA_CONTAINMENT_CLASS     0x0081EFD4L
    #define SIDF_SCHEMA_NAMING_ATTR_NAMES     0x0081EFD0L
    #define SIDF_SCHEMA_MANDITORY_ATTRS       0x0081EFD3L
    #define SIDF_SCHEMA_OPTIONAL_ATTRS        0x0081EFD2L

    #define SIDF_SCHEMA_TRAILER               0x0081EFD1L



/*  Future FIDs already defined */
    #define SIDF_AUTHENTICATION               0x00008002L
    #define SIDF_COMPRESSION_TYPE             0x00808024L
    #define SIDF_ENCRYPTION_TYPE              0x00808025L


/*  Old, obsolete FIDs */
    #define SIDF_DATABASE_LOCATION_METHOD     0x0080800FL
    #define SIDF_NUMBER_OF_DATABASES          0x00808016L
    #define SIDF_PREVIOUS_MEDIA_INDEX         0x00808017L
    #define SIDF_DATABASE_NAME                0x00808018L
    #define SIDF_ACCESS_DATE                  0x00000041L
    #define SIDF_ACCESS_TIME                  0x00000051L
    #define SIDF_ARCHIVE_DATE_AND_TIME        0x00000052L
    #define SIDF_ARCHIVE_DATE_TIME_OLD        0x00000042L
    #define SIDF_CLOSE_DATE_TIME_OLD          0x0080F202L
    #define SIDF_CREATION_DATE_AND_TIME       0x00000062L
    #define SIDF_DATA_RECORD_SEQUENCE         0x0000000AL
    #define SIDF_DATA_SET_NAME                0x0000813EL
    #define SIDF_EA_VALUE                     0x0000001CL
    #define SIDF_EXTENDED_ATTRIBUTES          0x0000001AL
    #define SIDF_LAST_BUFFER_IN_SESSION       0x00000040L
    #define SIDF_MEDIA_OPEN_DATE_TIME         0x0080F200L
    #define SIDF_MEDIA_OPEN_DATE_TIME_CAL     0x0080F20BL
    #define SIDF_MEDIA_OWNER                  0x00808001L
    #define SIDF_MEDIA_SECTOR_ADDRESS         0x00808013L
    #define SIDF_MODIFY_DATE_AND_TIME         0x00000072L
    #define SIDF_MS_OPEN_DATE_TIME            0x0081F2FFL
    #define SIDF_MS_OPEN_DATE_TIME_CAL        0x0080F20CL
    #define SIDF_NFS_NUMBER_OF_LINKS_OLD      0x0080F206L
    #define SIDF_NFS_RDEVICE_OLD              0x0080F205L
    #define SIDF_SCAN_INFO_ACC_DATE_TIME      0x0081F2F9L
    #define SIDF_SCAN_INFO_ARC_DATE_TIME      0x0081F2F6L
    #define SIDF_SCAN_INFO_CREAT_DATE_TIME    0x0081F2F8L
    #define SIDF_SCAN_INFO_MOD_DATE_TIME      0x0081F2F7L
    #define SIDF_SECTOR_ADDRESS               0x0000002EL
    #define SIDF_SESSION_DATE_TIME_OLD        0x00008042L
    #define SIDF_SESSION_INDEX_ADDRESS        0x0080802CL
    #define SIDF_SMM_HEADER                   0x0080801AL
    #define SIDF_SMM_TYPE                     0x0080801BL
    #define SIDF_SMM_PREV_FILE_MARK           0x0080801CL
    #define SIDF_SMM_NEXT_FILE_MARK           0x0080801DL
    #define SIDF_SMM_PREV_SET_MARK            0x0080801EL
    #define SIDF_SMM_NEXT_SET_MARK            0x0080801FL
    #define SIDF_SPANNED_SECTOR_COUNT         0x0080F102L
    #define SIDF_SPARSE_DATA_STREAM_HEADER    0x00000023L
    #define SIDF_TS_CHARACTERISTICS           0x00008127L
    #define SIDF_TS_DATA_STREAM_HEADER        0x00008123L
    #define SIDF_TS_DATA_STREAM_TRAILER       0x00008122L
    #define SIDF_TS_EXTENDED_ATTRIBUTES       0x00008125L
    #define SIDF_TS_FILE_HEADER               0x0081EFF1L
    #define SIDF_TS_FILE_TRAILER              0x0081EFF0L
    #define SIDF_TS_MAC_CHARACTERISTICS       0x00008126L
    #define SIDF_TS_NAME_POSITIONS            0x0000812BL
    #define SIDF_TS_NAME_SPACE_TYPE           0x00008129L
    #define SIDF_TS_NFS_CHARACTERISTICS       0x0081EFEDL
    #define SIDF_TS_PATH_NAME                 0x00008128L
    #define SIDF_TS_REQUESTED_NS_TYPE         0x0000812EL
    #define SIDF_TS_SEPARATOR_POSITIONS       0x0000812AL
    #define SIDF_TS_TRUSTEE_HEADER            0x00008124L
    #define SIDF_TS_VOLUME_RESTRICTIONS       0x0081EFECL
    #define SIDF_MEDIA_SET_LABEL              0x0081EFFEL

