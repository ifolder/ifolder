/******************************************************************************

  %name: nwdssch.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:08:00 1996 %
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
#if ! defined ( NWDSSCH_H )
#define NWDSSCH_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWDSBUFT_H )
#include "nwdsbuft.h"
#endif

#if ! defined ( NWDSATTR_H ) 
#include "nwdsattr.h"
#endif


#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSDefineAttr
(
   NWDSContextHandle context,
   pnstr8            attrName,
   pAttr_Info_T      attrDef
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSDefineClass
(
   NWDSContextHandle context,
   pnstr8            className,
   pClass_Info_T     classInfo,
   pBuf_T            classItems
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSListContainableClasses
(
   NWDSContextHandle context,
   pnstr8            parentObject,
   pnint32           iterationHandle,
   pBuf_T            containableClasses
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSModifyClassDef
(
   NWDSContextHandle context,
   pnstr8            className,
   pBuf_T            optionalAttrs
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadAttrDef
(
   NWDSContextHandle context,
   nuint32           infoType,
   nbool8            allAttrs,
   pBuf_T            attrNames,
   pnint32           iterationHandle,
   pBuf_T            attrDefs
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadClassDef
(
   NWDSContextHandle context,
   nuint32           infoType,
   nbool8            allClasses,
   pBuf_T            classNames,
   pnint32           iterationHandle,
   pBuf_T            classDefs
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemoveAttrDef
(
   NWDSContextHandle context,
   pnstr8            attrName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemoveClassDef
(
   NWDSContextHandle context,
   pnstr8            className
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSyncSchema
(
   NWDSContextHandle context,
   pnstr8            server,
   nuint32           seconds
);

#ifdef __cplusplus
}
#endif
#endif /* NWDSSCH_H */
