#ifndef _UNISTD_H_
#define _UNISTD_H_
/*============================================================================
=  Novell Software Development Kit--NetWare Loadable Module (NLM) Source Code
=
=  Copyright (C) 1999 Novell, Inc. All Rights Reserved.
=
=  This work is subject to U.S. and international copyright laws and treaties.
=  Use and redistribution of this work is subject  to  the  license  agreement
=  accompanying  the  software  development kit (SDK) that contains this work.
=  However, no part of this work may be revised and/or  modified  without  the
=  prior  written consent of Novell, Inc. Any use or exploitation of this work
=  without authorization could subject the perpetrator to criminal  and  civil
=  liability. 
=
=  unistd.h
==============================================================================
*/
#ifndef NULL
# define NULL  0
#endif

/* 'mode' values for access() and sem_open, etc. ... */
#define F_OK          0 /* test for existence of file */
#define R_OK          4 /* test for read permission */
#define W_OK          2 /* test for write permission */
#define X_OK          1 /* test for execute permission */

/* values for 'whence' in lseek()... */
#define SEEK_SET      0 /* set file pointer to 'offset' */
#define SEEK_CUR      1 /* set file pointer to current plus 'offset' */
#define SEEK_END      2 /* set file pointer to EOF plus 'offset' */

#define EFF_ONLY_OK   8 /* test using effective ids */

#define STDIN_FILENO  0
#define STDOUT_FILENO 1
#define STDERR_FILENO 2

#ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _SSIZE_T
#  define _SSIZE_T
typedef long ssize_t;
#endif

#ifndef _OFF_T
#  define _OFF_T
typedef long off_t;
#endif


#ifdef __cplusplus
extern "C"
{
#endif

/* POSIX-defined functions... */
int      access( const char *path, int mode );
int      chdir( const char *path );
int      chsize( int fildes, unsigned long size );
int      close( int fildes );
int      dup( int fildes );
int      dup2( int fildes1, int fildes2 );
int      eof( int fildes );
void     _exit( int status );
char     *getcwd( char *path, size_t len );
int      isatty( int fildes );
off_t    lseek( int fildes, off_t offset, int whence );
int      pipe( int fildes[2] );
ssize_t  read( int fildes, void *buf, size_t nbytes );
int      rmdir( const char *path );
int      unlink( const char *path );
ssize_t  write( int fildes, const void *buf, size_t nbytes );

/* thread-safe POSIX additions... */
ssize_t  pread( int fildes, void *buf, size_t nbytes, off_t offset );
ssize_t  pwrite( int fildes, const void *buf, size_t nbytes, off_t offset );

#ifdef __cplusplus
}
#endif


#endif
