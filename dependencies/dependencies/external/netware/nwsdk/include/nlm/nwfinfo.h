#ifndef _NWFILE_H_
#define _NWFILE_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwfile.h
==============================================================================
*/
#include <nwtypes.h>
#include <nwfattr.h>
#include <dirent.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int FileServerFileCopy
(
   int   sourceFileHandle,
   int   destinationFileHandle,
   LONG  sourceFileOffset,
   LONG  destinationFileOffset,
   LONG  numberOfBytesToCopy,
   LONG *numberOfBytesCopied
);

extern int NWGetCompressedFileLengths
(
   int   handle,
   LONG *uncompressedLength,
   LONG *compressedLength
);

extern int NWGetDiskIOsPending(void);

extern int NWSetCompressedFileLengths
(
   int   handle,
   LONG  uncompressedLength,
   LONG  compressedLengt
);

extern int PurgeErasedFile
(
   const char *pathName,
   long  sequenceNumber
);

extern int SalvageErasedFile
(
   const char *pathName,
   long  sequenceNumber,
   const char *newFileName
);

#define ScanErasedFiles(pathName, nextEntryNumber, deletedFileInfo)     \
    ScanErasedFiles_411(pathName, nextEntryNumber, deletedFileInfo)

extern int ScanErasedFiles_411
(
   const char *pathName,
   long *nextEntryNumber,
   DIR  *deletedFileInfo
);

extern int SetExtendedFileAttributes
(
   const char *pathName,
   BYTE  extendedFileAttributes
);

extern int SetFileInfo
(
   const char *pathName,
   BYTE        searchAttributes,
   LONG        fileAttributes,
   const char *creationDate,
   const char *lastAccessDate,
   const char *lastUpdateDateAndTime,
   const char *lastArchiveDateAndTime,
   LONG        fileOwnerID
);

#ifdef __cplusplus
}
#endif


#endif
