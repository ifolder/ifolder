#ifndef _NWAUDNLM_H_
#define _NWAUDNLM_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwaudnlm.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* ---------------------------------------------------------------------------
** Note: This file now holds the contents of nwaudit.h.
** ---------------------------------------------------------------------------
*/

/* defined network address types: */
#define ASCIIZ_STRING_NET_ADDRESS_TYPE 0x00
#define IPX_NET_ADDRESS_TYPE           0x01

/* special value network address type: */
#define NO_IDENTITY_HAS_BEEN_SET       0xFF


#ifdef __cplusplus
extern "C" {
#endif

extern int NWAddRecordToAuditingFile
(
   LONG  volumeNumber,
   LONG  recordType,
   LONG  stationNumber,
   LONG  statusCode,
   const BYTE *data,
   LONG  dataSize
);

extern int NWGetAuditingIdentity
(
   LONG  *addressType,
   BYTE  *networkAddress,
   char  *identityName
);

extern int NWSetAuditingIdentity
(
   LONG   addressType,
   const BYTE *networkAddress,
   const char *identityName
);

#ifdef __cplusplus
}
#endif


#endif
