#ifndef __nwfileio_h__
#define __nwfileio_h__
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwfileio.h
==============================================================================
*/
#include <fcntl.h>
#include <dirent.h>
#include <unistd.h>
#include <sys/stat.h>
#include <nwtypes.h>
#include <nwfattr.h>

#ifndef _STRUCT_CACHEBUFFERSTRUCTURE
# define _STRUCT_CACHEBUFFERSTRUCTURE
typedef struct cacheBufferStructure
{
   char *cacheBufferPointer;
   LONG  cacheBufferLength;
   int   completionCode;
} T_cacheBufferStructure;
#endif

#ifndef _STRUCT_MWRITEBUFFERSTRUCTURE
# define _STRUCT_MWRITEBUFFERSTRUCTURE
typedef struct mwriteBufferStructure
{
   char *mwriteBufferPointer;
   LONG  mwriteBufferLength;
   int   reserved;
} T_mwriteBufferStructure;
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* NetWare additions to POSIX... */
LONG  filelength( int fildes );
int   gwrite( int fildes, const T_mwriteBufferStructure *bufferP, LONG numberBufs,
         LONG *numberBufsWritten );
int   lock( int fildes, LONG offset, LONG nbytes ); 
int   qread( int fildes, void *buffer, LONG len, LONG position ); 
int   qwrite( int fildes, const void *buffer, LONG len, LONG position ); 
int   setmode( int fildes, int mode );
int   sopen( const char *path, int oflag, int shflag, ... );
LONG  tell( int fildes );
int   unlock( int fildes, LONG offset, LONG nbytes );

/* other NetWare file I/O utilities... */
int   AsyncRead( int handle, LONG startingOffset, LONG numberBytesToRead,
         LONG *numberBytesRead, LONG localSemaHandle,
         T_cacheBufferStructure **cacheBufferInfo, LONG *numOfCacheBufs );
void  AsyncRelease( T_cacheBufferStructure *cacheBufferInfo ); 
int   CountComponents( const BYTE *pathString, int len ); 
int   GetExtendedFileAttributes( const char *pathName, BYTE *extFileAttrs );
void _makepath( char *path, const char *drive, const char *dir,
         const char *fname, const char *ext );
LONG  NWGetVolumeFlags(LONG volume, LONG *flags);
LONG  NWSetVolumeFlags(LONG volume, LONG flags);
int   ParsePath( const char *path, char *server, char *volume, char *directories );
int   SetReaddirAttribute(DIR *dirP, unsigned long newAttribute );
void  _splitpath( const char *path, char *drive, char *dir, char *fname,
         char *ext );
void  UseAccurateCaseForPaths( int yesno );	/* (default/original is FALSE) */
void  UnAugmentAsterisk( int yesno );			/* (default/original is FALSE) */

#ifdef __cplusplus
}
#endif

#endif
