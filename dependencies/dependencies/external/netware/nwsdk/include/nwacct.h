/******************************************************************************

  %name: nwacct.h %
  %version: 4 %
  %date_modified: Mon Oct 25 11:17:01 1999 %
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

#if ! defined ( NWACCT_H )
#define NWACCT_H

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
   nuint32  objectID;
   nint32   amount;
} HOLDS_INFO;

typedef struct
{
   nuint16  holdsCount;
   HOLDS_INFO holds[16];
} HOLDS_STATUS;

N_EXTERN_LIBRARY( NWCCODE )
NWGetAccountStatus
(
   NWCONN_HANDLE        conn,
   nuint16              objType,
   const nstr8 N_FAR *  objName,
   pnint32              balance,
   pnint32              limit,
   HOLDS_STATUS N_FAR * holds
);

N_EXTERN_LIBRARY( NWCCODE )
NWQueryAccountingInstalled
(
   NWCONN_HANDLE  conn,
   pnuint8        installed
);

N_EXTERN_LIBRARY( NWCCODE )
NWSubmitAccountCharge
(
   NWCONN_HANDLE       conn,
   nuint16             objType,
   const nstr8 N_FAR * objName,
   nuint16             serviceType,
   nint32              chargeAmt,
   nint32              holdCancelAmt,
   nuint16             noteType,
   const nstr8 N_FAR * note
);

N_EXTERN_LIBRARY( NWCCODE )
NWSubmitAccountHold
(
   NWCONN_HANDLE       conn,
   nuint16             objType,
   const nstr8 N_FAR * objName,
   nint32              holdAmt
);

N_EXTERN_LIBRARY( NWCCODE )
NWSubmitAccountNote
(
   NWCONN_HANDLE       conn,
   nuint16             objType,
   const nstr8 N_FAR * objName,
   nuint16             serviceType,
   nuint16             noteType,
   const nstr8 N_FAR * note
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
