/******************************************************************************

  %name: nwdel.h %
  %version: 7 %
  %date_modified: Tue Aug 29 17:50:53 2000 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWDEL_H )
#define NWDEL_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{
   nuint32  sequence;
   nuint32  parent;
   nuint32  attributes;
   nuint8   uniqueID;
   nuint8   flags;
   nuint8   nameSpace;
   nuint8   nameLength;
   nuint8   name [256];
   nuint32  creationDateAndTime;
   nuint32  ownerID;
   nuint32  lastArchiveDateAndTime;
   nuint32  lastArchiverID;
   nuint32  updateDateAndTime;
   nuint32  updatorID;
   nuint32  fileSize;
   nuint8   reserved[44];
   nuint16  inheritedRightsMask;
   nuint16  lastAccessDate;
   nuint32  deletedTime;
   nuint32  deletedDateAndTime;
   nuint32  deletorID;
   nuint8   reserved3 [16];
} NWDELETED_INFO;

typedef struct
{
   nuint32  sequence;
   nuint32  parent;
   nuint32  attributes;
   nuint8   uniqueID;
   nuint8   flags;
   nuint8   nameSpace;
   nuint16  nameLength;
   nuint8   name [766];
   nuint32  creationDateAndTime;
   nuint32  ownerID;
   nuint32  lastArchiveDateAndTime;
   nuint32  lastArchiverID;
   nuint32  updateDateAndTime;
   nuint32  updatorID;
   nuint32  fileSize;
   nuint8   reserved[44];
   nuint16  inheritedRightsMask;
   nuint16  lastAccessDate;
   nuint32  deletedTime;
   nuint32  deletedDateAndTime;
   nuint32  deletorID;
   nuint8   reserved3 [16];
} NWDELETED_INFO_EXT;


N_EXTERN_LIBRARY( NWCCODE )
NWPurgeDeletedFile
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   nuint32             iterHandle,
   nuint32             volNum,
   nuint32             dirBase,
   const nstr8 N_FAR * fileName
);

N_EXTERN_LIBRARY( NWCCODE )
NWRecoverDeletedFile
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   nuint32        iterHandle,
   nuint32        volNum,
   nuint32        dirBase,
   pnstr8         delFileName,
   pnstr8         rcvrFileName
);

N_EXTERN_LIBRARY( NWCCODE )
NWRecoverDeletedFileExt
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   nuint32        iterHandle,
   nuint32        volNum,
   nuint32        dirBase,
   pnstr8         delFileName,
   pnstr8         rcvrFileName
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanForDeletedFiles
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   pnuint32       iterHandle,
   pnuint32       volNum,
   pnuint32       dirBase,
   NWDELETED_INFO N_FAR * entryInfo
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanForDeletedFilesExt
(
   NWCONN_HANDLE              conn,
   NWDIR_HANDLE               dirHandle,
   pnuint32                   iterHandle,
   pnuint32                   volNum,
   pnuint32                   dirBase,
   NWDELETED_INFO_EXT N_FAR * entryInfo
);

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_del.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif  /* NWDEL_H */
