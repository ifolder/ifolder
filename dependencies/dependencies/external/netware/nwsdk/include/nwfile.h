/******************************************************************************

  %name: nwfile.h %
  %version: 13 %
  %date_modified: Thu Oct 14 14:54:51 1999 %
  $Copyright:

  Copyright (c) 1989-1997 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWFILE_H )
#define NWFILE_H

#if ! defined ( NTYPES_H )
# include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef FILE_LOCKS_ONLY

typedef struct NW_FILE_INFO
{
   nstr8   fileName[14];
   nuint8  fileAttributes;
   nuint8  extendedFileAttributes;
   nuint32 fileSize;
   nuint16 creationDate;
   nuint16 lastAccessDate;
   nuint32 lastUpdateDateAndTime;
   nuint32 fileOwnerID;
   nuint32 lastArchiveDateAndTime;
} NW_FILE_INFO;

typedef struct NW_FILE_INFO2
{
   nuint8  fileAttributes;
   nuint8  extendedFileAttributes;
   nuint32 fileSize;
   nuint16 creationDate;
   nuint16 lastAccessDate;
   nuint32 lastUpdateDateAndTime;
   nuint32 fileOwnerID;
   nuint32 lastArchiveDateAndTime;
   nstr8   fileName[260];
} NW_FILE_INFO2;

typedef struct NW_FILE_INFO2_EXT
{
   nuint8  fileAttributes;
   nuint8  extendedFileAttributes;
   nuint32 fileSize;
   nuint16 creationDate;
   nuint16 lastAccessDate;
   nuint32 lastUpdateDateAndTime;
   nuint32 fileOwnerID;
   nuint32 lastArchiveDateAndTime;
   nstr8   fileName[766]; /* 255*3 + 1 */
} NW_FILE_INFO2_EXT;

typedef struct SEARCH_FILE_INFO
{
   nuint16 sequenceNumber;
   nuint16 reserved;
   nstr8   fileName[15];
   nuint8  fileAttributes;
   nuint8  fileMode;
   nuint32 fileLength;
   nuint16 createDate;
   nuint16 accessDate;
   nuint16 updateDate;
   nuint16 updateTime;
} SEARCH_FILE_INFO;

typedef struct SEARCH_DIR_INFO
{
   nuint16 sequenceNumber;
   nuint16 reserved1;
   nstr8   directoryName[15];
   nuint8  directoryAttributes;
   nuint8  directoryAccessRights;
   nuint16 createDate;
   nuint16 createTime;
   nuint32 owningObjectID;
   nuint16 reserved2;
   nuint16 directoryStamp;
} SEARCH_DIR_INFO;

typedef struct
{
   nuint8  taskNumber;
   nuint8  lockType;
   nuint8  accessControl;
   nuint8  lockFlag;
   nuint8  volNumber;
   nuint16 dirEntry;
   nstr8   fileName[14];
} CONN_OPEN_FILE;

typedef struct
{
   nuint16 nextRequest;
   nuint8  connCount;
   CONN_OPEN_FILE connInfo[22];
} CONN_OPEN_FILES;

typedef struct
{
   nuint16 taskNumber;
   nuint8  lockType;
   nuint8  accessControl;
   nuint8  lockFlag;
   nuint8  volNumber;
   nuint32 parent;
   nuint32 dirEntry;
   nuint8  forkCount;
   nuint8  nameSpace;
   nuint8  nameLen;
   nstr8   fileName[255];
} OPEN_FILE_CONN;

typedef struct
{
   nuint16 nextRequest;
   nuint16 openCount;
   nuint8  buffer[512];
   nuint16 curRecord;
} OPEN_FILE_CONN_CTRL;

typedef struct
{
   nuint16 connNumber;
   nuint16 taskNumber;
   nuint8  lockType;
   nuint8  accessControl;
   nuint8  lockFlag;
} CONN_USING_FILE;

typedef struct
{
   nuint16 nextRequest;
   nuint16 useCount;
   nuint16 openCount;
   nuint16 openForReadCount;
   nuint16 openForWriteCount;
   nuint16 denyReadCount;
   nuint16 denyWriteCount;
   nuint8  locked;
   nuint8  forkCount;
   nuint16 connCount;
   CONN_USING_FILE connInfo[70];
} CONNS_USING_FILE;

#define  SEEK_FROM_BEGINNING        1
#define  SEEK_FROM_CURRENT_OFFSET   2
#define  SEEK_FROM_END              3

/* The following flags are to be used in the createFlag parameter of
   the NWCreateFile call. */

#define NWCREATE_NEW_FILE	1
#define NWOVERWRITE_FILE	2

N_EXTERN_LIBRARY( NWCCODE )
NWSetCompressedFileSize
(
   NWCONN_HANDLE  conn,
   NWFILE_HANDLE  fileHandle,
   nuint32        reqFileSize,
   pnuint32       resFileSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWFileServerFileCopy
(
   NWFILE_HANDLE  srcFileHandle,
   NWFILE_HANDLE  dstFileHandle,
   nuint32        srcOffset,
   nuint32        dstOffset,
   nuint32        bytesToCopy,
   pnuint32       bytesCopied
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileConnectionID
(
   NWFILE_HANDLE  fileHandle,
   NWCONN_HANDLE N_FAR *  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileConnRef
(
   NWFILE_HANDLE  fileHandle,
   pnuint32       connRef
);

N_GLOBAL_LIBRARY( NWCCODE )
NWFileSearchInitialize
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   pnuint8             volNum,
   pnuint16            dirID,
   pnuint16            iterhandle,
   pnuint8             accessRights
);

#define NWIntFileSearchInitialize(a, b, c, d, e, f, g, h) \
        NWFileSearchInitialize(a, b, c, d, e, f, g)

N_EXTERN_LIBRARY( NWCCODE )
NWIntFileSearchContinue
(
   NWCONN_HANDLE       conn,
   nuint8              volNum,
   nuint16             dirID,
   nuint16             searchContext,
   nuint8              searchAttr,
   const nstr8 N_FAR * searchPath,
   pnuint8             retBuf,
   nuint16             augmentFlag
);

#define NWScanFileInformation(a, b, c, d, e, f) \
        NWIntScanFileInformation(a, b, c, d, e, f, 0)

N_EXTERN_LIBRARY( NWCCODE )
NWIntScanFileInformation
(
   NWCONN_HANDLE        conn,
   NWDIR_HANDLE         dirHandle,
   const nstr8  N_FAR * filePattern,
   nuint8               searchAttr,
   pnint16              iterhandle,
   NW_FILE_INFO N_FAR * info,
   nuint16              augmentFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetFileInformation
(
   NWCONN_HANDLE        conn,
   NWDIR_HANDLE         dirHandle,
   const nstr8  N_FAR * fileName,
   nuint8               searchAttrs,
   NW_FILE_INFO N_FAR * info
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetFileInformation2
(
   NWCONN_HANDLE         conn,
   NWDIR_HANDLE          dirHandle,
   const nstr8   N_FAR * fileName,
   nuint8                searchAttrs,
   NW_FILE_INFO2 N_FAR * info
);

N_EXTERN_LIBRARY( NWCCODE )
NWIntScanFileInformation2
(
   NWCONN_HANDLE         conn,
   NWDIR_HANDLE          dirHandle,
   const nstr8   N_FAR * filePattern,
   nuint8                searchAttrs,
   pnuint8               iterHandle,
   NW_FILE_INFO2 N_FAR * info,
   nuint16               augmentFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWIntScanFileInformation2Ext
(
   NWCONN_HANDLE         conn,
   NWDIR_HANDLE          dirHandle,
   const nstr8   N_FAR * filePattern,
   nuint8                searchAttrs,
   pnuint8               iterHandle,
   NW_FILE_INFO2_EXT N_FAR * info,
   nuint16               augmentFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetFileAttributes
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * fileName,
   nuint8              searchAttrs,
   nuint8              newAttrs
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetExtendedFileAttributes2
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   pnuint8             extAttrs
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanConnectionsUsingFile
(
   NWCONN_HANDLE            conn,
   NWDIR_HANDLE             dirHandle,
   const nstr8      N_FAR * filePath,
   pnint16                  iterhandle,
   CONN_USING_FILE  N_FAR * fileUse,
   CONNS_USING_FILE N_FAR * fileUsed
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanOpenFilesByConn2
(
   NWCONN_HANDLE               conn,
   nuint16                     connNum,
   pnint16                     iterHandle,
   OPEN_FILE_CONN_CTRL N_FAR * openCtrl,
   OPEN_FILE_CONN      N_FAR * openFile
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanOpenFilesByConn
(
   NWCONN_HANDLE           conn,
   nuint16                 connNum,
   pnint16                 iterHandle,
   CONN_OPEN_FILE  N_FAR * openFile,
   CONN_OPEN_FILES N_FAR * openFiles
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetExtendedFileAttributes2
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              extAttrs
);

N_EXTERN_LIBRARY( NWCCODE )
NWRenameFile
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        oldDirHandle,
   const nstr8 N_FAR * oldFileName,
   nuint8              searchAttrs,
   NWDIR_HANDLE        newDirHandle,
   const nstr8 N_FAR * newFileName
);

N_EXTERN_LIBRARY( NWCCODE )
NWIntEraseFiles
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              searchAttrs,
   nuint16             augmentFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetSparseFileBitMap
(
   NWCONN_HANDLE  conn,
   nuint32        fileHandle,
   nint16         flag,
   nuint32        offset,
   pnuint32       blockSize,
   pnuint8        bitMap
);

#endif

#define NWLOCKS_INCLUDED

N_EXTERN_LIBRARY( NWCCODE )
NWLogPhysicalRecord
(
   NWFILE_HANDLE  fileHandle,
   nuint32        recStartOffset,
   nuint32        recLength,
   nuint8         lockFlags,
   nuint16        timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWLockPhysicalRecordSet
(
   nuint8      lockFlags,
   nuint16     timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleasePhysicalRecordSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearPhysicalRecordSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleasePhysicalRecord
(
   NWFILE_HANDLE  fileHandle,
   nuint32        recStartOffset,
   nuint32        recSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearPhysicalRecord
(
   NWFILE_HANDLE  fileHandle,
   nuint32        recStartOffset,
   nuint32        recSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWLockFileLockSet
(
   nuint16        timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleaseFileLockSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearFileLockSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearFileLock2
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleaseFileLock2
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path
);

N_EXTERN_LIBRARY( NWCCODE )
NWLogFileLock2
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              lockFlags,
   nuint16             timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWLogLogicalRecord
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * logRecName,
   nuint8              lockFlags,
   nuint16             timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWLockLogicalRecordSet
(
   nuint8         lockFlags,
   nuint16        timeOut
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleaseLogicalRecordSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearLogicalRecordSet
(
   void
);

N_EXTERN_LIBRARY( NWCCODE )
NWReleaseLogicalRecord
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * logRecName
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearLogicalRecord
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * logRecName
);


N_EXTERN_LIBRARY( NWCCODE )
NWCloseFile
(
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWCreateFile
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   pnstr8         fileName,
   nuint8         fileAttrs,
   NWFILE_HANDLE  N_FAR * fileHandle,
   nflag32        createFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenFile
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   pnstr8         fileName,
   nuint16        searchAttr,
   nuint8         accessRights,
   NWFILE_HANDLE  N_FAR * fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadFile
(
   NWFILE_HANDLE  fileHandle,
   nuint32        bytesToRead,
   pnuint32       bytesActuallyRead,
   pnuint8        data
);

N_EXTERN_LIBRARY( NWCCODE )
NWWriteFile
(
   NWFILE_HANDLE  fileHandle,
   nuint32        bytesToWrite,
   pnuint8        data
);

N_EXTERN_LIBRARY( NWCCODE )
NWCommitFile
(
   NWFILE_HANDLE  fileHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetEOF
(
   NWFILE_HANDLE  fileHandle,
   pnuint32       getEOF
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetEOF
(
   NWFILE_HANDLE  fileHandle,
   nuint32        setEOF
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFilePos
(
   NWFILE_HANDLE  fileHandle,
   pnuint32       filePos
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetFilePos
(
   NWFILE_HANDLE  fileHandle,
   nuint          mode,
   nuint32        filePos
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileDirEntryNumber
(
   NWFILE_HANDLE  fileHandle,
	pnuint32			volumeNum,
	pnuint32			directoryEntry,
	pnuint32			DOSDirectoryEntry,
	pnuint32			nameSpace,
	pnuint32			dataStream,
	pnuint32			parentDirEntry,
	pnuint32			parentDOSDirEntry
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDirectoryEntryNumber
(
   NWCONN_HANDLE	conn,
	nuint8			dirHandle,
	pnuint32			volumeNum,
	pnuint32			directoryEntry,
	pnuint32			DOSDirectoryEntry,
	pnuint32			nameSpace,
	pnuint32			parentDirEntry,
	pnuint32			parentDOSDirEntry
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNSFileDirEntryNumber
(
	NWFILE_HANDLE	fileHandle,
	nuint8			nameSpace,
	pnuint32			volumeNum,
	pnuint32			directoryEntry,
	pnuint32			dataStream
);

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_file.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif
