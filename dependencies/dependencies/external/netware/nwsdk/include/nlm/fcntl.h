#ifndef _FCNTL_H_
#define _FCNTL_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  fcntl.h
==============================================================================
*/

/* 'cmd' values for fcntl()... */
#define F_GETFL      1        /* get file status flags */
#define F_SETFL      2        /* set file status flags */
#define F_DUPFD      3        /* duplicate file descriptor */

/* unimplemented 'cmd' values for fcntl()... */
#define F_GETFD      4        /* get file descriptor flags */
#define F_SETFD      5        /* set file descriptor flags */
#define F_SETLK      6        /* set record locking info */
#define F_GETLK      7        /* get record locking info */
#define F_SETLKW     8        /* get record locking info; wait if blocked*/
#define F_RDLCK      9        /* shared or read lock */
#define F_UNLCK      10       /* unlock */
#define F_WRLCK      11       /* exclusive or write lock */
#define F_CLOEXEC    12       /* close on execute */

/* values for 'o_flag' in open()... */
#define O_RDONLY     0x0000   /* open for read only */
#define O_WRONLY     0x0001   /* open for write only */
#define O_RDWR       0x0002   /* open for read and write */
#define O_ACCMODE    0x0003   /* AND with value to extract access flags */
#define O_APPEND     0x0010   /* writes done at end of file */
#define O_CREAT      0x0020   /* create new file */
#define O_TRUNC      0x0040   /* truncate existing file */
#define O_EXCL       0x0080   /* exclusive open */
#define O_TEXT       0x0100   /* text file--unsupported */
#define O_BINARY     0x0200   /* binary file */
#define O_NDELAY     0x0400   /* nonblocking flag */
#define O_NOCTTY     0x0800   /* currently unsupported */
#define O_NONBLOCK   O_NDELAY

/* value for third argument when 'cmd' is F_SETFL in fcntl()... */
#define FNDELAY      0x0004   /* fcntl() non-blocking I/O */

#ifndef _OFF_T
# define _OFF_T
typedef long   off_t;         /* file offset value */
#endif

#ifndef _PID_T
# define _PID_T
typedef long   pid_t;         /* process IDs */
#endif

#ifndef _SSIZE_T
# define _SSIZE_T
typedef long   ssize_t;       /* byte counts/error indication */
#endif

#ifndef _MODE_T
# define _MODE_T
typedef unsigned long   mode_t;/* some files attributes, permissions */
#endif

struct flock                  /* unimplemented */
{
   short l_type;              /* F_RDLCK, F_WRLCK or F_UNLCK */
   short l_whence;            /* flag for starting offset */
   off_t l_start;             /* relative offset in bytes */
   off_t l_len;               /* size; if 0, then until EOF */
   pid_t l_pid;               /* process ID of owner, get with F_GETLK */
};


#ifdef __cplusplus
extern "C"
{
#endif

int   creat( const char *path, mode_t mode );
int   fcntl( int fildes, int cmd, ... );
int   open( const char *path, int oflag, ... );
int   sopen( const char *path, int oflag, int shflag, ... );

#ifdef __cplusplus
}
#endif

#endif
