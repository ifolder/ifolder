#ifndef _SYS_MMAN_H_
#define _SYS_MMAN_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/mman.h
==============================================================================
*/
#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _OFF_T
# define _OFF_T
typedef long   off_t;
#endif

#ifndef _MODE_T
# define _MODE_T
typedef long   mode_t;
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* definitions needed elsewhere... */
#define PAGESIZE  4096

/* test features (to be added to unistd.h when done)... */
#define _POSIX_MEMLOCK
#define _POSIX_MEMLOCK_RANGE
#define _POSIX_MAPPED_FILES
#define _POSIX_SHARED_MEMORY
#define _POSIX_SHARED_MEMORY_OBJECTS
#define _POSIX_MEMORY_PROTECTION
#define _POSIX_SYNCHRONIZED_IO

/* 'flags' values for mlockall()... */
#define MCL_CURRENT     0x00  /* lock all pages currently mapped */
#define MCL_FUTURE      0x01  /* lock all pages currently mapped in future */

/* 'prot' values for mmap()... */
#define PROT_NONE       0     /* data cannot be accessed */
#define PROT_READ       1     /* data can be read */
#define PROT_WRITE      2     /* data can be written */
#define PROT_EXEC       3     /* data can be executed */

/* 'flags' values for mmap()... */
#define MAP_SHARED      0x01  /* changes are shared */
#define MAP_PRIVATE     0x02  /* changes are private */
#define MAP_FIXED       0x04  /* interpret 'addr' exactly */

/* 'flags' values for msync()... */
#define MS_ASYNC        0x01  /* perform asynchronous writes */
#define MS_SYNC         0x02  /* perform synchronous writes */
#define MS_INVALIDATE   0x04  /* invalidate cached data */

/* prototypes... */
int   mlockall(int flags);
int   munlockall(void);
int   mlock(const void *addr, size_t len);
int   munlock(const void *addr, size_t len);
void  *mmap(void *addr, size_t len, int prot, int flags, int fildes, off_t off);
int   munmap(void *addr, size_t len);
int   mprotect(const void *addr, size_t len, int prot);
int   msync(void *addr, size_t len, int flags);
int   shm_open(const char *name, int oflag, mode_t mode);
int   shm_unlink(const char *name);

#ifdef   __cplusplus
}
#endif

#endif
