/******************************************************************************

  %name: nwdsacl.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:06:37 1996 %
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
#if ! defined ( NWDSACL_H )
#define NWDSACL_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWDSBUFT_H ) /* Needed to defined pBuf_T */
#include "nwdsbuft.h"
#endif

#if ! defined ( NWDSDC_H )   /* Needed to defined NWDSContextHandle */
#include "nwdsdc.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetEffectiveRights
(
   NWDSContextHandle context,
   pnstr8            subjectName,
   pnstr8            objectName,
   pnstr8            attrName,
   pnuint32          privileges
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSListAttrsEffectiveRights
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            subjectName,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   pBuf_T            privilegeInfo
);

#ifdef __cplusplus
   }
#endif

#include "npackoff.h"
#endif   /* NWDSACL_H */
