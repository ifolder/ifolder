/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwacctng.h
==============================================================================
*/

#ifndef _NWACCTNG_H_
#define _NWACCTNG_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int AccountingInstalled
(
   WORD fileServerID
);

extern int GetAccountStatus
(
   WORD  binderyObjectType,
   const char *binderyObjectName,
   long *balance,
   long *limits,
   long *holds
);

extern int SubmitAccountCharge
(
   WORD  binderyObjectType,
   const char *binderyObjectName,
   WORD  serviceType,
   long  chargeAmount,
   long  cancelHoldAmount,
   WORD  commentType,
   const char *comment
);

extern int SubmitAccountChargeWithLength
(
   WORD  binderyObjectType,
   const char *binderyObjectName,
   WORD  serviceType,
   long  chargeAmount,
   long  cancelHoldAmount,
   WORD  commentType,
   const void *commentData,
   WORD  commentLength
);

extern int SubmitAccountHold
(
   WORD  binderyObjectType,
   const char *binderyObjectName,
   long  reserveAmount
);


extern int SubmitAccountNote
(
   WORD  binderyObjectType,
   const char *binderyObjectName,
   WORD  serviceType,
   WORD  commentType,
   const char *comment
);

#ifdef __cplusplus
}
#endif


#endif
