/******************************************************************************

  %name: nwmsg.h %
  %version: 8 %
  %date_modified: Fri Oct 22 15:04:41 1999 %
  $Copyright:

  Copyright (c) 1989-1996 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWMSG_H )
#define NWMSG_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY( NWCCODE )
NWDisableBroadcasts
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWEnableBroadcasts
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWSendBroadcastMessage
(
   NWCONN_HANDLE         conn,
   const nstr8   N_FAR * message,
   nuint16               connCount,
   const nuint16 N_FAR * connList,
   pnuint8               resultList
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetBroadcastMessage
(
   NWCONN_HANDLE  conn,
   pnstr8         message
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetBroadcastMode
(
   NWCONN_HANDLE  conn,
   nuint16        mode
);

N_EXTERN_LIBRARY( NWCCODE )
NWBroadcastToConsole
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * message
);

N_EXTERN_LIBRARY( NWCCODE )
NWSendConsoleBroadcast
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * message,
   nuint16             connCount,
   pnuint16            connList
);


#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_msg.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#endif
