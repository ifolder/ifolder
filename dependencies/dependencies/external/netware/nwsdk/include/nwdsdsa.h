/******************************************************************************

  %name:          nwdsdsa.h %
  %version:       10 %
  %date_modified: Tue Oct 12 13:04:23 1999 %
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
#if ! defined ( NWDSDSA_H )
#define NWDSDSA_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSBUFT_H )
#include "nwdsbuft.h"
#endif

#if ! defined ( NWDSATTR_H )
#include "nwdsattr.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NUNICODE_H )
#include "nunicode.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAddObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnint32           iterationHandle,
   nbool8            more,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSBackupObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnint32           iterationHandle,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCompare
(
   NWDSContextHandle context,
   pnstr8            object,
   pBuf_T            buf,
   pnbool8           matched
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetPartitionRoot
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            partitionRoot
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSList
(
   NWDSContextHandle context,
   pnstr8            object,
   pnint32           iterationHandle,
   pBuf_T            subordinates
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSListContainers
(
   NWDSContextHandle context,
   pnstr8            object,
   pnint32           iterationHandle,
   pBuf_T            subordinates
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSListByClassAndName
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            className,
   pnstr8            subordinateName,
   pnint32           iterationHandle,
   pBuf_T            subordinates
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetCountByClassAndName
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            className,
   pnstr8            subordinateName,
   pnint32           count
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSMapIDToName
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   nuint32           objectID,
   pnstr8            object
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSMapNameToID
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnstr8            object,
   pnuint32          objectID
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSModifyObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnint32           iterationHandle,
   nbool8            more,
   pBuf_T            changes
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSModifyDN
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            newDN,
   nbool8            deleteOldRDN
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSModifyRDN
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            newDN,
   nbool8            deleteOldRDN
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSMoveObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            destParentDN,
   pnstr8            destRDN
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRead
(
   NWDSContextHandle context,
   pnstr8            object,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadObjectInfo
(
   NWDSContextHandle    context,
   pnstr8               object,
   pnstr8               distinguishedName,
   pObject_Info_T       objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadObjectDSIInfo
(
   NWDSContextHandle    context,
   pnstr8               object,
   nuint32              infoLength,
   nptr                 objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemoveObject
(
   NWDSContextHandle context,
   pnstr8            object
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRestoreObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnint32           iterationHandle,
   nbool8            more,
   nuint32           size,
   pnuint8           objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSearch
(
   NWDSContextHandle context,
   pnstr8            baseObjectName,
   nint              scope,
   nbool8            searchAliases,
   pBuf_T            filter,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   nint32            countObjectsToSearch,
   pnint32           countObjectsSearched,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSOpenStream
(
   NWDSContextHandle    context,
   pnstr8               objectName,
   pnstr8               attrName,
   nflag32              flags,
   NWFILE_HANDLE N_FAR  *fileHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSWhoAmI
(
   NWDSContextHandle context,
   pnstr8            objectName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetServerDN
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnstr8            serverDN
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetServerAddresses2
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnuint32          countNetAddress,
   pBuf_T            netAddresses
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSInspectEntry
(
   NWDSContextHandle context,
   pnstr8            serverName,
   pnstr8            objectName,
   pBuf_T            errBuffer
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadReferences
(
   NWDSContextHandle context,
   pnstr8            serverName,
   pnstr8            objectName,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   nuint32           timeFilter,
   pnint32           iterationHandle,
   pBuf_T            objectInfo
);


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSExtSyncList
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            className,
   pnstr8            subordinateName,
   pnint32           iterationHandle,
   pTimeStamp_T      timeStamp,
   nbool             onlyContainers,
   pBuf_T            subordinates
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSExtSyncRead
(
   NWDSContextHandle context,
   pnstr8            objectName,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   pTimeStamp_T      timeStamp,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSExtSyncSearch
(
   NWDSContextHandle context,
   pnstr8            baseObjectName,
   nint              scope,
   nbool8            searchAliases,
   pBuf_T            filter,
   pTimeStamp_T      timeStamp,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   nint32            countObjectsToSearch,
   pnint32           countObjectsSearched,
   pBuf_T            objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemSecurityEquiv
(
   NWDSContextHandle context,
   pnstr8            equalFrom,
   pnstr8            equalTo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAddSecurityEquiv
(
   NWDSContextHandle context,
   pnstr8            equalFrom,
   pnstr8            equalTo
);


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSMutateObject
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            newObjectClass,
   nuint32           flags
);

/*----- NDS Register For Event Function Prototypes -----*/
#if defined( N_PLAT_NLM )

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSERegisterForEvent
(
   nint    priority,
   nuint32 type,
   nint    (*handler)(nuint32 type, nuint size, nptr data)
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSERegisterForEventWithResult
(
   nint    priority,
   nuint32 type,
   nint    (*handler)(nuint32 type, nuint size, nptr data, nint result),
   nint    flags
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEUnRegisterForEvent
(
   nint    priority,
   nuint32 type,
   nint    (*handler)(nuint32 type, nuint size, nptr data)
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalEntryName
(
   NWDSContextHandle context,
   nuint32           entryID,
   pnstr             objectName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEConvertEntryName
(
   NWDSContextHandle context,
   const punicode    DSEventName,
   pnstr             objectName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalAttrName
(
   NWDSContextHandle context,
   nuint32           attrID,
   pnstr             name
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalClassName
(
   NWDSContextHandle context,
   nuint32           classID,
   pnstr             name
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalAttrID
(
   NWDSContextHandle context,
   const pnstr       name,
   pnuint32          id
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalClassID
(
   NWDSContextHandle context,
   const pnstr       name,
   pnuint32          id
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSEGetLocalEntryID
(
   NWDSContextHandle context,
   const pnstr       objectName,
   pnuint32          id
);
#endif      /* N_PLAT_NLM */

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_dsdsa.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */


#include "npackoff.h"
#endif   /* NWDSDSA_H */
