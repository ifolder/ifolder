/******************************************************************************

  %name: nwndscon.h %
  %version: 9 %
  %date_modified: Tue Aug 29 17:19:36 2000 %
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
#if ! defined ( NWNDSCON_H )
#define NWNDSCON_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#define NWNDS_CONNECTION         0x0001
#define NWNDS_LICENSED           0x0002
#define NWNDS_AUTHENTICATED      0x0004
#define NWNDS_PACKET_BURST_AVAIL 0x0001
#define NWNDS_NEEDED_MAX_IO      0x0040
#define SYSTEM_LOCK              0x0
#define TASK_LOCK                0x4
#define SYSTEM_DISCONNECT        0x0
#define TASK_DISCONNECT          0x1

#define ALLREADY_ATTACHED        0x1
#define ATTACHED_NOT_AUTH        0X2
#define ATTACHED_AND_AUTH        0X4

#ifdef __cplusplus
   extern "C" {
#endif


N_EXTERN_LIBRARY (NWCCODE)
NWSetPreferredDSTree
(
   nuint16  length,
   pnuint8  treeName
);


#ifdef __cplusplus
   }
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_ndscon.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#endif /* NWNDSCON_H */
