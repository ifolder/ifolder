#ifndef _SYS_TYPES_H_
#define _SYS_TYPES_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/types.h
==============================================================================
*/
#include <string.h>

#ifndef _TIME_T
# define _TIME_T
typedef long time_t;
#endif

#ifndef _OFF_T
# define _OFF_T
typedef long   off_t;            /* file offset value */
#endif

#ifndef _PID_T
# define _PID_T
typedef long   pid_t;            /* process IDs */
#endif

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;     /* byte counts/error indication */
#endif

#ifndef _SSIZE_T
# define _SSIZE_T
typedef long   ssize_t;          /* byte counts/error indication */
#endif

#ifndef _GID_T
# define _GID_T
typedef unsigned long   gid_t;   /* group IDs */
#endif
                                 
#ifndef _UID_T
# define _UID_T
typedef unsigned long   uid_t;   /* user IDs */
#endif

#ifndef _MODE_T
# define _MODE_T
typedef unsigned long   mode_t;  /* some files attributes, permissions */
#endif

#ifndef _NLINK_T
# define _NLINK_T
typedef unsigned long   nlink_t; /* link counts */
#endif

#ifndef _INO_T
# define _INO_T
typedef long            ino_t;   /* i-node # type */
#endif

#ifndef _DEV_T
# define _DEV_T
typedef long            dev_t;   /* device code (drive number) */
#endif

/* non-POSIX types */
typedef unsigned long   ulong;
typedef unsigned int    uint;
typedef unsigned short  ushort;
typedef unsigned long   u_long;
typedef unsigned int    u_int;
typedef unsigned short  u_short;
typedef unsigned char   u_char;
typedef void            *caddr_t;


/* Berkeley Sockets definitions and types */
#define FD_SETSIZE      16

typedef long   fd_array[FD_SETSIZE];
typedef struct fd_set 
{
   fd_array fds;
} fd_set;

/* use of these requires inclusion of sys/bsdskt.h */
#define FD_SET(n, p)    bsd_fd_set(n, p)
#define FD_CLR(n, p)    bsd_fd_clr(n, p)
#define FD_ISSET(n, p)  bsd_fd_isset(n, p)

/*
** Caution: don't use FD_ZERO on dynamically-allocated data of type 'fd_set'
** or 'dyn_fd_set.'
*/
#define FD_ZERO(p)      memset((void *) (p), 0, sizeof(*(p)))

/*
** For select(), FD_SETSIZE determines the number of socket descriptors in
** 'fd_array.' This number can be increased for version v4.11 and beyond of
** NLMLib.NLM by calling Set_FD_SETSIZE() and using a dynamic version of
** 'fd_set.'
*/
typedef long   dyn_fd_array[1];
typedef struct dyn_fd_set 
{
   dyn_fd_array fds;
} dyn_fd_set;

size_t  Get_FD_SETSIZE( void );
size_t  Set_FD_SETSIZE( size_t fd_setsize );

#endif
