/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwmsg.h
==============================================================================
*/

#ifndef _NWMSG_H_
#define _NWMSG_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#define MAX_CONSOLE_MESSAGE_LENGTH  80
#define MAX_MESSAGE_LENGTH          58		/* NetWare v3.0, v2.2 and earlier */
#define NEW_MAX_MESSAGE_LENGTH      250	/* NetWare v3.11b and later */


#ifdef __cplusplus
extern "C" {
#endif

extern int BroadcastToConsole
(
   const char *message
);

extern int DisableStationBroadcasts
(
   void
);

extern int EnableStationBroadcasts
(
   void
);

extern int GetBroadcastMessage
(
   char *messageBuffer
);

extern int SendBroadcastMessage
(
   const char *message,
   const WORD *connectionList,
   BYTE       *resultList,
   WORD        connectionCount
);

#ifdef __cplusplus
}
#endif


#endif
