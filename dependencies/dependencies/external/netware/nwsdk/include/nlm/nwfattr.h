#ifndef _NWFATTR_H_
#define _NWFATTR_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwfattr.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* for multiple thread use, see documentation for sopen()...                */
#define NWSH_PRE_401D_COMPAT  0x80000000

/* Attribute values for use with existing files                             */
#define _A_NORMAL             0x00000000 /* Normal (read/write) file        */
#define _A_RDONLY             0x00000001 /* Read-only file                  */
#define _A_HIDDEN             0x00000002 /* Hidden file                     */
#define _A_SYSTEM             0x00000004 /* System file                     */
#define _A_EXECUTE            0x00000008 /* Execute only file               */
#define _A_VOLID              0x00000008 /* Volume ID entry                 */
#define _A_SUBDIR             0x00000010 /* Subdirectory                    */
#define _A_ARCH               0x00000020 /* Archive file                    */
#define _A_SHARE              0x00000080 /* Sharable file                   */

#define _A_NO_SUBALLOC			0x00000800 /* Don't sub alloc. this file		 */
#define _A_TRANS              0x00001000 /* Transactional file (TTS usable) */
#define _A_READAUD            0x00004000 /* Read audit                      */
#define _A_WRITAUD            0x00008000 /* Write audit                     */

#define _A_IMMPURG            0x00010000 /* Immediate purge                 */
#define _A_NORENAM            0x00020000 /* Rename inhibit                  */
#define _A_NODELET            0x00040000 /* Delete inhibit                  */
#define _A_NOCOPY             0x00080000 /* Copy inhibit                    */

#define _A_FILE_MIGRATED      0x00400000 /* File has been migrated          */
#define _A_DONT_MIGRATE       0x00800000 /* Don't migrate this file         */
#define _A_IMMEDIATE_COMPRESS 0x02000000 /* Compress this file immediately  */
#define _A_FILE_COMPRESSED    0x04000000 /* File is compressed              */
#define _A_DONT_COMPRESS      0x08000000 /* Don't compress this file        */
#define _A_CANT_COMPRESS      0x20000000 /* Can't compress this file        */
#define _A_ATTR_ARCHIVE       0x40000000 /* Entry has had an EA modified,   */
                                         /* an ownerID changed, or trustee  */
                                         /* info changed, etc.              */

/* Attribute values usable during file creation                             */
/* Use: OR value with the file mode value to initialize the mode parameter  */
#define FA_NORMAL   (_A_NORMAL  << 16)
#define FA_RDONLY   (_A_RDONLY  << 16)
#define FA_HIDDEN   (_A_HIDDEN  << 16)
#define FA_SYSTEM   (_A_SYSTEM  << 16)
#define FA_EXECUTE  (_A_EXECUTE << 16)

#define FA_SUBDIR   (_A_SUBDIR  << 16)
#define FA_ARCHIVE  (_A_ARCH    << 16)
#define FA_SHARE    (_A_SHARE   << 16)

/* Extended file attributes values */
#define FA_TRANSAC  (_A_TRANS   << 12)
#define FA_READAUD  (_A_READAUD << 12)
#define FA_WRITEAUD (_A_WRITAUD << 12)

#define FA_IMMPURG  (_A_IMMPURG << 12)
#define FA_NORENAM  (_A_NORENAM << 12)
#define FA_NODELET  (_A_NODELET << 12)
#define FA_NOCOPY   (_A_NOCOPY  << 12)

/* Sharing values for sharable open functions */
#define SH_COMPAT   0x00    /* compatibility mode   */
#define SH_DENYRW   0x10    /* deny read/write mode */
#define SH_DENYWR   0x20    /* deny write mode      */
#define SH_DENYRD   0x30    /* deny read mode       */
#define SH_DENYNO   0x40    /* deny none mode       */

/* FEcreat/FEsopen flagBits parameter values used when creating a file */
#define DELETE_FILE_ON_CREATE_BIT     0x0001
#define NO_RIGHTS_CHECK_ON_CREATE_BIT 0x0002

/* FEsopen flagBits parameter values used when opening a file */
#define FILE_WRITE_THROUGH_BIT           0x00000040
#define ENABLE_IO_ON_COMPRESSED_DATA_BIT 0x00000100
#define LEAVE_FILE_COMPRESSED_DATA_BIT   0x00000200
#define DELETE_FILE_ON_CLOSE_BIT         0x00000400
#define NO_RIGHTS_CHECK_ON_OPEN_BIT      0x00010000
#define OK_TO_OPEN_DOS_FILE              0x80000000


/* Volume Flags used with NWGetVolumeFlags and NWSetVolumeFlags */
#define SUB_ALLOCATION_FLAG				0x02	/* if set sub allocation units valid on this volume */
#define FILE_COMPRESSION_FLAG				0x04	/* if set file compression enabled on this volume */
#define DATA_MIGRATION_FLAG				0x08	/* if set data migration is allowed on this volume */
#define VOLUME_IMMEDIATE_PURGE_FLAG		0x40	/* if set volume is marked as immediate purge */

/* Name space values */
#define DOSNameSpace   0
#define MACNameSpace   1
#define MacNameSpace   MACNameSpace
#define NFSNameSpace   2
#define FTAMNameSpace  3
#define OS2NameSpace   4
#define LONGNameSpace  4
#define NTNameSpace    5
#define MAX_NAMESPACES 6

#define NWDOS_NAME_SPACE  DOSNameSpace
#define NWMAC_NAME_SPACE  MACNameSpace
#define NWNFS_NAME_SPACE  NFSNameSpace
#define NWFTAM_NAME_SPACE FTAMNameSpace
#define NWOS2_NAME_SPACE  OS2NameSpace
#define NWLONG_NAME_SPACE LONGNameSpace
#define NWNT_NAME_SPACE   NTNameSpace

/* Data stream values */
#define PrimaryDataStream         0
#define MACResourceForkDataStream 1
#define FTAMStructuringDataStream 2

/* File path length values */
#define _MAX_PATH    255 /* maximum length of full pathname */
#define _MAX_SERVER  48  /* maximum length of server name */
#define _MAX_VOLUME  16  /* maximum length of volume component */
#define _MAX_DRIVE   3   /* maximum length of drive component */
#define _MAX_DIR     255 /* maximum length of path component */
#define _MAX_FNAME   9   /* maximum length of file name component */
#define _MAX_EXT     5   /* maximum length of extension component */
#define _MAX_NAME    13  /* maximum length of file name */
#define NAME_MAX     12  /* maximum length of file name (alternate view) */

/* Modify structure mask values */
#define MModifyNameBit                 0x0001L
#define MFileAttributesBit             0x0002L
#define MCreateDateBit                 0x0004L
#define MCreateTimeBit                 0x0008L
#define MOwnerIDBit                    0x0010L
#define MLastArchivedDateBit           0x0020L
#define MLastArchivedTimeBit           0x0040L
#define MLastArchivedIDBit             0x0080L
#define MLastUpdatedDateBit            0x0100L
#define MLastUpdatedTimeBit            0x0200L
#define MLastUpdatedIDBit              0x0400L
#define MLastAccessedDateBit           0x0800L
#define MInheritanceRestrictionMaskBit 0x1000L
#define MMaximumSpaceBit               0x2000L
#define MLastUpdatedInSecondsBit       0x4000L

#include <npackon.h>

struct ModifyStructure
{
   BYTE *MModifyName;
   LONG  MFileAttributes;
   LONG  MFileAttributesMask;
   WORD  MCreateDate;
   WORD  MCreateTime;
   LONG  MOwnerID;
   WORD  MLastArchivedDate;
   WORD  MLastArchivedTime;
   LONG  MLastArchivedID;
   WORD  MLastUpdatedDate;
   WORD  MLastUpdatedTime;
   LONG  MLastUpdatedID;
   WORD  MLastAccessedDate;
   WORD  MInheritanceGrantMask;
   WORD  MInheritanceRevokeMask;
   int   MMaximumSpace;
   LONG  MLastUpdatedInSeconds;
};

#include <npackoff.h>

#endif
