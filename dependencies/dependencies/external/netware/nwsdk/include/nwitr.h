/******************************************************************************

  %name:          nwitr.h %
  %version:       11 %
  %date_modified: Wed May  3 10:00:12 2000 %
  $Copyright:

  Copyright (c) 1998 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

/* Public include file for Large Virtual List Iterator */

#if !defined( NWITR_H )
#define NWITR_H

#include "ntypes.h"
#include "nwdsbuft.h"

#define DS_ITR_FIRST     0          /* First entry position in list */
#define DS_ITR_LAST   1000          /* Last  entry position in list */
#define DS_ITR_EOF    1001          /* End-of-file position. */

#define DS_ITR_UNICODE_STRING    0  /* Indicates a unicode string */
#define DS_ITR_BYTE_STRING       2  /* Indicates a byte string */

#define DS_ITR_PREFER_SCALABLE   0  /* If can't get scalable, emulate [not supported in FCS] */
#define DS_ITR_REQUIRE_SCALABLE  1  /* If can't get scalable, return error */
#define DS_ITR_FORCE_EMULATION   2  /* Always force emulation mode */
#define DS_ITR_ANY_SERVER        3  /* Get any server */

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrCreateList
(
   NWDSContextHandle context,
   pnstr8            baseObjectName,      /* Starting object to search */
   pnstr8            className,           /* Class name if List operation */
   pnstr8            subordinateName,     /* RDN if List operation */
   nuint32           scalability,         /* Require or prefer SKADS server */
   nuint32           timeout,             /* Timeout in milliseconds */
   pnuint32          pIterator            /* Returned Iterator Ptr */
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrCreateSearch
(
   NWDSContextHandle context,
   pnstr8            baseObjectName,      /* Starting object to search */
   nint              scope,               /* Object, immed subord or subtree */
   nbool8            searchAliases,       /* True to follow aliases */
   pBuf_T            filter,              /* Search filter */
   pTimeStamp_T      pTimeFilter,         /* Filter on modification time */
   nuint32           infoType,            /* Names only, or names and attrib */
   nbool8            allAttrs,            /* True = return all attributes */
   pBuf_T            attrNames,           /* List of attributes to return */
   pnstr8            indexSelect,         /* Index selection string */
   pnstr8            sortKey,             /* Attributes to sort on */
   nuint32           scalability,         /* Require or prefer SKADS server */
   nuint32           timeout,             /* Timeout in milliseconds */
   pnuint32          pIterator            /* Returned Iterator Ptr */
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrDestroy(nuint32 Iterator);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrClone(nuint32 Iterator, pnuint32 pNewIterator);

N_EXTERN_LIBRARY (nbool8)
NWDSItrAtFirst(nuint32 Iterator);

N_EXTERN_LIBRARY (nbool8)
NWDSItrAtEOF(nuint32 Iterator);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrGetInfo(nuint32 Iterator, pnbool8 pIsScalable, pnbool8 pIisPositionable);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrGetPosition(nuint32 Iterator, pnuint32 pPosition, nuint32 timeout);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrSetPosition(nuint32 Iterator, nuint32 position, nuint32 timeout);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrSetPositionFromIterator(nuint32 Iterator, nuint32 srcIterator,
                               nuint32 timeout);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrTypeDown(nuint32 Iterator, pnstr8 attrString, pnstr8 value,
                nuint32 byteUniFlag, nuint32 timeout);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrSkip(nuint32 Iterator, nint32 numToSkip, nuint32 timeout,
            pnint32 pNumSkipped);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrGetNext(nuint32 Iterator, nuint32 numEntries, nuint32 timeout,
               pnint32 pIterationHandle, pBuf_T pData);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrGetPrev(nuint32 Iterator, nuint32 numEntries, nuint32 timeout,
               pnint32 pIterationHandle, pBuf_T pData);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrGetCurrent(nuint32 Iterator, pnint32 pIterationHandle, pBuf_T pData);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSItrCount(nuint32 Iterator, nuint32 timeout, nuint32 maxCount,
             nbool8 updatePosition, pnuint32 pCount);

#endif
