/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwserial.h
==============================================================================
*/

#ifndef _NWSERIAL_H_
#define _NWSERIAL_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#ifdef __cplusplus
extern "C" {
#endif

extern int GetNetworkSerialNumber
(
   LONG *networkSerialNumber,
   WORD *applicationNumber
);

extern int VerifyNetworkSerialNumber
(
   LONG networkSerialNumber,
   WORD *applicationNumber
);

#ifdef __cplusplus
}
#endif


#endif
