/******************************************************************************

  %name: nwconnec.h %
  %version: 11 %
  %date_modified: Tue Aug 29 17:32:28 2000 %
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

#if ! defined ( NWCONNECT_H )
#define NWCONNECT_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct tNWINET_ADDR
{
  nuint8   networkAddr[4];
  nuint8   netNodeAddr[6];
  nuint16  socket;
  nuint16  connType;  /* 3.11 and above only: 0=not in use, 2=NCP over IPX, 4=AFP */
} NWINET_ADDR;

#define CONNECTION_AVAILABLE            0x0001
#define CONNECTION_PRIVATE              0x0002  /* obsolete */
#define CONNECTION_LOGGED_IN            0x0004
#define CONNECTION_LICENSED             0x0004
#define CONNECTION_BROADCAST_AVAILABLE  0x0008
#define CONNECTION_ABORTED              0x0010
#define CONNECTION_REFUSE_GEN_BROADCAST 0x0020
#define CONNECTION_BROADCASTS_DISABLED  0x0040
#define CONNECTION_PRIMARY              0x0080
#define CONNECTION_NDS                  0x0100
#define CONNECTION_PNW                  0x4000  /* obsolete */
#define CONNECTION_AUTHENTICATED        0x8000


/* End of new connection model calls. */

N_EXTERN_LIBRARY( NWCCODE )
NWLockConnection
(
   NWCONN_HANDLE connHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionInformation
(
   NWCONN_HANDLE  connHandle,
   nuint16        connNumber,
   pnstr8         pObjName,
   pnuint16       pObjType,
   pnuint32       pObjID,
   pnuint8        pLoginTime
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetInternetAddress
(
   NWCONN_HANDLE connHandle,
   nuint16        connNumber,
   pnuint8       pInetAddr
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetInetAddr
(
   NWCONN_HANDLE  connHandle,
   nuint16        connNum,
   NWINET_ADDR N_FAR * pInetAddr
);

N_EXTERN_LIBRARY( NWCCODE )
NWClearConnectionNumber
(
   NWCONN_HANDLE  connHandle,
   nuint16        connNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDefaultConnRef
(
   pnuint32 pConnReference
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectConnectionNumbers
(
   NWCONN_HANDLE        connHandle,
   const nstr8 N_FAR *  pObjName,
   nuint16              objType,
   pnuint16             pNumConns,
   pnuint16             pConnHandleList,
   nuint16              maxConns
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnListFromObject
(
   NWCONN_HANDLE  connHandle,
   nuint32        objID,
   nuint32        searchConnNum,
   pnuint16       pConnListLen,
   pnuint32       pConnList
);

#ifndef NWOS2
N_EXTERN_LIBRARY( NWCCODE )
NWGetPreferredServer
(
   NWCONN_HANDLE N_FAR * pConnHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetPreferredServer
(
   NWCONN_HANDLE connHandle
);

#else
N_EXTERN_LIBRARY( NWCCODE )
NWResetConnectionConfig
(
   nuint32  flags
);
#endif

#ifdef __cplusplus
}
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_connec.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif   /* NWCONNECT_H */
