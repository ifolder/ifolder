#ifndef _NWFILENG_H_
#define _NWFILENG_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwfileng.h
==============================================================================
*/
#include <dirent.h>
#include <nwtypes.h>
#include <nwfattr.h>

/* values for flags parameter in FEGetOpenFileInfo()... */
#define _NotReadableBit                     0x00000001
#define _NotWriteableBit                    0x00000002
#define _WrittenBit                         0x00000004
#define _DetachedBit                        0x00000008
#define _SwitchingToDirectFileSystemModeBit 0x00000010
#define _DirectFileSystemModeBit            0x00000020
#define _FileWriteThroughBit                0x00000040

/* extra flags */
#define _DiskBlockReturnedBit               0x00010000
#define _IAmOnTheOpenFileListBit            0x00020000
#define _FileReadAuditBit                   0x00040000
#define _FileWriteAuditBit                  0x00080000
#define _FileCloseAuditBit                  0x00100000
#define _DontFileWriteSystemAlertBit        0x00200000
#define _ReadAheadHintBit                   0x00400000
#define _NotifyCompressionOnCloseBit        0x00800000

/* extra extra flags */
#define _IsWritingCompressedBit             0x01000000
#define _HasTimeDateBit                     0x02000000
#define _DoingDeCompressionBit              0x04000000
#define _NoSubAllocBit                      0x08000000
#define _IsATransactionFileBit              0x10000000
#define _HasFileWritePrivilegeBit           0x20000000
#define _TTSReadAuditBit                    0x40000000
#define _TTSWriteAuditBit                   0x80000000

typedef int (*T_PathParseFunc)
(
   const char *inputPath,
   WORD       *connectionIDp,
   int        *volumeNumberP,
   LONG       *directoryNumberP,
   BYTE       *outPathStringP,
   LONG       *outPathCountP
);


#ifdef __cplusplus
extern "C" {
#endif

extern int FEConvertDirectoryNumber
(
   int   sourceNameSpace,
   LONG  volumeNumber,
   LONG  sourceDirectoryNumber,
   int   destinationNameSpace,
   LONG *destinationDirectoryNumberP
);

extern int FEcreat
(
   const char *name,
   int   permission,
   int   flagBits
);

extern int FEFlushWrite
(
   int handle
);

extern LONG FEGetCWDnum
(
   void
);

extern LONG FEGetCWVnum
(
   void
);

extern int FEGetDirectoryEntry
(
   int    volumeNumber,
   LONG   directoryNumber,
   const BYTE *pathString,
   LONG   pathCount,
   int    desiredNameSpace,
   void **namespaceDirectoryStructPp,
   void **DOSdirectoryStructPp
);

extern int FEGetEntryVersion
(
   int   volumeNumber,
   LONG  directoryNumber,
   const BYTE *pathString,
   LONG  pathCount,
   WORD *version
);

extern int FEGetOpenFileInfo
(
	LONG	connection,
	LONG	handle,
	LONG	*volume,
	LONG	*directoryNumber,
	LONG	*dataStream,
	LONG	*flags
);

extern int FEGetOpenFileInfoForNS
(
	LONG	connection,
	LONG	handle,
	LONG	*volume,
	LONG	*DOSdirectoryNumber,
	LONG	*directoryNumber,
	LONG	*nameSpace,
	LONG	*dataStream,
	LONG	*flags
);

extern LONG FEGetOriginatingNameSpace
(
   LONG volumeNumber,
   LONG directoryNumber
);

extern int FEMapConnsHandleToVolAndDir
(
   LONG  connection,
   int   handle,
   int  *volumeNumberP,
   LONG *directoryNumberP
);

extern int FEMapHandleToVolumeAndDirectory
(
   int   handle,
   int  *volumeNumberP,
   LONG *directoryNumberP
);

extern int FEMapPathVolumeDirToVolumeDir
(
   const char *pathName,
   int   volumeNumber,
   LONG  directoryNumber,
   int  *newVolumeNumberP,
   LONG *newDirectoryNumberP
);

extern int FEMapVolumeAndDirectoryToPath
(
   int   volumeNumber,
   LONG  directoryNumber,
   BYTE *pathString,
   LONG *pathCount
);

extern int FEMapVolumeAndDirectoryToPathForNS
(
   int   volumeNumber,
   LONG  directoryNumber,
   LONG  nameSpace,
   BYTE *pathString,
   LONG *pathCount
);

extern int FEMapVolumeNumberToName
(
   int   volumeNumber,
   BYTE *volumeName
);

extern int FEQuickClose
(
	LONG connection,
	LONG task,
	LONG fileHandle
);

extern int FEQuickFileLength
(
	LONG	connection,
	LONG	handle,
	LONG	*fileSize
);

extern int FEQuickOpen
(
	LONG connection,
	LONG task,
	LONG volumeNumber,
	LONG directoryNumber,
	const BYTE *pathString,
	LONG pathCount,
	LONG nameSpace,
	LONG attributeMatchBits,
	LONG requestedAccessRights,
	LONG dataStreamNumber,
	LONG *fileHandle
);

extern int FEQuickRead
(
	LONG	connection,
	LONG	handle,
	LONG	postition,
	LONG	bytesToRead,
	LONG	*bytesRead,
	void	*buffer
);

extern int FEQuickWrite
(
	LONG	connection,
	LONG	handle,
	LONG	position,
	LONG	bytesToWrite,
	void	*buffer
);

extern int FERegisterNSPathParser
(
   T_PathParseFunc normalFunc
);

extern LONG FESetCWDnum
(
   LONG CWDnum
);

extern LONG FESetCWVandCWDnums
(
   LONG CWVnum, 
   LONG CWDnum
);

extern LONG FESetCWVnum
(
   LONG CWVnum
);

extern LONG FESetOriginatingNameSpace
(
   LONG volumeNumber,
   LONG directoryNumber,
   LONG currentNameSpace,
   LONG newNameSpace
);

extern int FEsopen
(
   const char *name,
   int   access,
   int   share,
   int   permission,
   int   flagBits,
   BYTE  dataStream
);

extern int NWGetDirBaseFromPath
(
   const char *path,
   BYTE  nameSpace,
   LONG *volNum,
   LONG *NSDirBase,
   LONG *DOSDirBase
);

#ifdef __cplusplus
}
#endif


#endif
