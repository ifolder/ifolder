/******************************************************************************

  %name: nwdsconn.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:07:08 1996 %
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
#if ! defined ( NWDSCONN_H )
#define NWDSCONN_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWDSDC_H )
#include "nwdsdc.h"
#endif

#ifdef __cplusplus
   extern "C" {
#endif


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSOpenConnToNDSServer
(
   NWDSContextHandle context,
   pnstr8            serverName,
   pNWCONN_HANDLE    connHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetDefNameContext
(
   NWDSContextHandle context,
   nuint             nameContextLen,
   pnstr8            nameContext
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSetDefNameContext
(
   NWDSContextHandle context,
   nuint             nameContextLen,
   pnstr8            nameContext
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetMonitoredConnRef
(
   NWDSContextHandle context,
   pnuint32          connRef
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSOpenMonitoredConn
(
   NWDSContextHandle context,
   pNWCONN_HANDLE    connHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSScanConnsForTrees
(
   NWDSContextHandle context,
   nuint             numOfPtrs,
   pnuint            numOfTrees,
   ppnstr8           treeBufPtrs
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSScanForAvailableTrees
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnstr             scanFilter,
   pnint32           scanIndex,
   pnstr             treeName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReturnBlockOfAvailableTrees
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnstr             scanFilter,
   pnstr             lastBlocksString,
   pnstr             endBoundString,
   nuint32           maxTreeNames,           
   ppnstr            arrayOfNames,
   pnuint32          numberOfTrees,
   pnuint32          totalUniqueTrees
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCanDSAuthenticate
(
   NWDSContextHandle context
);


#ifdef __cplusplus
   }
#endif
#endif /* NWDSCONN_H */

