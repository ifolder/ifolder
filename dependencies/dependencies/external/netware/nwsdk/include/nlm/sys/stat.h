#ifndef _SYS_STAT_H_
#define _SYS_STAT_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1999 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/stat.h
==============================================================================
*/

/* POSIX file types */
#define S_IFMT       0170000     /* type of file (mask for following) */
#define S_IFIFO      0010000     /* first-in/first-out (pipe) */
#define S_IFCHR      0020000     /* character-special file */
#define S_IFDIR      0040000     /* directory */
#define S_IFBLK      0060000     /* blocking device (not used on NetWare) */
#define S_IFREG      0100000     /* regular */
#define S_IFLNK      0120000     /* symbolic link (not used on NetWare) */
#define S_IFSOCK     0140000     /* Berkeley socket */

#define S_ISFIFO(m)  (((m) & S_IFMT) == S_IFIFO)   /* (e.g.: pipe) */
#define S_ISCHR(m)   (((m) & S_IFMT) == S_IFCHR)   /* (e.g.: console) */
#define S_ISDIR(m)   (((m) & S_IFMT) == S_IFDIR)
#define S_ISBLK(m)   (((m) & S_IFMT) == S_IFBLK)   /* (e.g.: pipe) */
#define S_ISREG(m)   (((m) & S_IFMT) == S_IFREG)
#define S_ISLNK(m)   (((m) & S_IFMT) == S_IFLNK)   /* should be FALSE */
#define S_ISSOCK(m)  (((m) & S_IFMT) == S_IFSOCK)  /* (e.g.: socket) */

/* POSIX file modes: owner (user) permission */
#define S_IRWXU      0000700
#define S_IRUSR      0000400
#define S_IWUSR      0000200
#define S_IXUSR      0000100

#define S_IREAD      S_IRUSR
#define S_IWRITE     S_IWUSR
#define S_IEXEC      S_IXUSR

/* POSIX file modes: group permission */
#define S_IRWXG      0000070
#define S_IRGRP      0000040
#define S_IWGRP      0000020
#define S_IXGRP      0000010

/* POSIX file modes: other permission */
#define S_IRWXO      0000007
#define S_IROTH      0000004
#define S_IWOTH      0000002
#define S_IXOTH      0000001

/* Novell-defined additional directory modes for mkdir() */
#define S_DSYSTEM    0x00100000  /* system directory */
#define S_DHIDE      0x00200000  /* hidden directory */
#define S_DDEL_INH   0x00400000  /* delete-inhibit */
#define S_DREN_INH   0x00800000  /* rename-inhibit */
#define S_DPURGE_IMM 0x01000000  /* purge-immediate */
#define S_DCOMP_IMM  0x02000000  /* compress-immediate */
#define S_DCOMP_NO   0x04000000  /* no compression */
#define S_DALL       0x01FB      /* equivalent to mode = 0 */

/* POSIX setuid, setgid, and sticky */
#define S_ISUID      0004000
#define S_ISGID      0002000
#define S_ISVTX      0001000

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _OFF_T
#  define _OFF_T
typedef long      off_t;
#endif

#ifndef _INO_T
# define _INO_T
typedef long      ino_t;
#endif

#ifndef _DEV_T
# define _DEV_T
typedef long      dev_t;
#endif

#ifndef _TIME_T
#  define _TIME_T
typedef long   time_t;
#endif

#ifndef _MODE_T
#  define _MODE_T
typedef unsigned long   mode_t;
#endif

struct stat 
{
   dev_t          st_dev;        /* volume number                         */
   ino_t          st_ino;        /* directory entry number of the st_name */
   unsigned short st_mode;       /* emulated file mode                    */
   unsigned short st_pad1;       /* reserved for alignment                */
   unsigned long  st_nlink;      /* count of hard links (always 1)        */
   unsigned long  st_uid;        /* object id of owner                    */
   unsigned long  st_gid;        /* group-id (always 0)                   */
   dev_t          st_rdev;       /* device type (always 0)                */
   off_t          st_size;       /* total file size--files only           */
   time_t         st_atime;      /* last access date--files only          */
   time_t         st_mtime;      /* last modify date and time             */
   time_t         st_ctime;      /* POSIX: last status change time...     */
                                 /* ...NetWare: creation date/time        */
   time_t         st_btime;      /* last archived date and time           */
   unsigned long  st_attr;       /* file attributes                       */
   unsigned long  st_archivedID; /* user/object ID of last archive        */
   unsigned long  st_updatedID;  /* user/object ID of last update         */
   unsigned short st_inheritedRightsMask;  /* inherited rights mask       */
   unsigned short st_pad2;       /* reserved for alignment                */
   unsigned int   st_originatingNameSpace; /* namespace of creation       */
   size_t         st_blksize;    /* block size for allocation--files only */
   size_t         st_blocks;     /* count of blocks allocated to file     */
   unsigned int   st_flags;      /* user-defined flags                    */
   unsigned long  st_spare[4];   /* for future use                        */
   unsigned char  st_name[255+1];/* TARGET_NAMESPACE name                 */
};

/* definitions of older structure technology are mostly for reference */
struct stat411                   /* v4.11 */
{
   dev_t          st_dev;
   ino_t          st_ino;
   unsigned short st_mode;
   short          st_nlink;
   unsigned long  st_uid;
   short          st_gid;
   dev_t          st_rdev;
   off_t          st_size;
   time_t         st_atime;
   time_t         st_mtime;
   time_t         st_ctime;
   time_t         st_btime;
   unsigned long  st_attr;
   unsigned long  st_archivedID;
   unsigned long  st_updatedID;
   unsigned short st_inheritedRightsMask;
   unsigned char  st_originatingNameSpace;
   /*----------------- new fields starting in v4.11 ------------------------- */
   unsigned char  st_name[255+1];
   size_t         st_blksize;
   size_t         st_blocks;
   unsigned int   st_flags;
   unsigned long  st_spare[4];
};

struct stat410                   /* v3.12, v4.0, v4.01, v4.02 and v4.10 */
{
   dev_t          st_dev;
   ino_t          st_ino;
   unsigned short st_mode;
   short          st_nlink;
   unsigned long  st_uid;
   short          st_gid;
   dev_t          st_rdev;
   off_t          st_size;
   time_t         st_atime;
   time_t         st_mtime;
   time_t         st_ctime;
   time_t         st_btime;
   unsigned long  st_attr;
   unsigned long  st_archivedID;
   unsigned long  st_updatedID;
   unsigned short st_inheritedRightsMask; 
   unsigned char  st_originatingNameSpace;
   unsigned char  st_name[13]; 
};

#ifdef __cplusplus
extern "C" {
#endif

int      chmod( const char *path, mode_t mode );
int      fstat_500( int fildes, struct stat *buf );
int      mkdir( const char *path );
int      mkdir_510( const char *pathname, mode_t mode );
int      stat_500( const char *path, struct stat *buf );
mode_t   umask( mode_t cmask );

/* explicit, direct calls to older technology (use not encouraged) */
int      fstat_410( int fildes, struct stat410 *buf );      /* or (fstat)() */
int      fstat_411( int fildes, struct stat411 *buf );
int      stat_410( const char *path, struct stat410 *buf ); /* or (stat)() */
int      stat_411( const char *path, struct stat411 *buf );

#ifdef __cplusplus
}
#endif


/* calls to current technology */
#define fstat(fd, buf)        fstat_500(fd, buf)
#define stat(path, buf)       stat_500(path, buf)

/* This define was in the code for a few months and actually released in the
** April 1999 NDK.  It is being removed as it was to intrusive to developers
** to make mkdir go from 1 to 2 parms.  SPD 232180
**
** #define mkdir(pathname, mode) mkdir_510(pathname, mode)
*/

#endif
