/******************************************************************************

  %name: nwdsname.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:07:42 1996 %
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
#if ! defined ( NWDSNAME_H )
#define NWDSNAME_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if! defined ( NWDSDC_H )
#include "nwdsdc.h"
#endif


#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAbbreviateName
(
   NWDSContextHandle context,
   pnstr8            inName,
   pnstr8            abbreviatedName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCanonicalizeName
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            canonName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemoveAllTypes
(
   NWDSContextHandle context,
   pnstr8            name,
   pnstr8            typelessName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSResolveName
(
   NWDSContextHandle    context,
   pnstr8               objectName,
   NWCONN_HANDLE  N_FAR *conn,
   pnuint32             objectID
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCIStringsMatch
(
   NWDSContextHandle context,
   pnstr8            string1,
   pnstr8            string2,
   pnint             matches
);

#ifdef __cplusplus
   }
#endif
#endif /* NWDSNAME_H */
