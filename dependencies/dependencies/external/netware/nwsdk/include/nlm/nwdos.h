#ifndef _NWDOS_H_
#define _NWDOS_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwdos.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#include <npackon.h>

struct find_t
{
   char           reserved[21];
   char           attrib;
   unsigned short wr_time;
   unsigned short wr_date;
   long           size;
   char           name[13];
};

#include <npackoff.h>

#ifdef __cplusplus
extern "C"
{
#endif

extern int DOSChangeFileMode
(
	const char *name, 
	LONG *attributes, 
	LONG function, /* 0 - read attributes, 1 - set attributes */
	LONG newAttributes
);
	
extern int DOSClose
(
   int handle
);

extern int DOSCopy
(
   const char *NetWareFileName, 
   const char *DOSFileName
);

extern int DOSCreate
(
   const char *fileName, 
   int        *handle
);

extern int DOSsopen
(
   const char *filename, 
   int         access, 
   int         share, 
   int         permission
);

extern int DOSFindFirstFile
(
   const char    *fileName, 
   WORD           searchAttributes,
   struct find_t *diskTransferAddress
);

extern int DOSFindNextFile
(
   struct find_t *diskTransferAddress
);

extern int DOSMkdir
(
	const char *__dirName
);

extern int DOSOpen
(
   const char *fileName, 
   int  *handle
);

extern int DOSPresent
(
   void
);

extern int DOSRead
(
   int   handle, 
   LONG  fileOffset, 
   void *buffer,
   LONG  numberOfBytesToRead, 
   LONG *numberOfBytesRead
);

extern int DOSRemove
(
	const char *__name
);

extern int DOSRename
(
	const char *srcName,
	const char *dstName
);

extern int DOSRmdir
(
	const char *__dirName
);

extern int DOSSetDateAndTime
(
	int  handle,
	LONG date,
	LONG time
);

extern void DOSShutOffFloppyDrive
(
	void
);

extern int DOSUnlink
(
	const char *__name
);

extern int DOSWrite
(
   int   handle, 
   LONG  fileOffset, 
   const void *buffer,
   LONG  numberOfBytesToWrite, 
   LONG *numberOfBytesWritten
);

#ifdef __cplusplus
}
#endif


#endif
