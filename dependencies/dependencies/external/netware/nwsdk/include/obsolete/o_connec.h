/******************************************************************************

  %name: o_connec.h %
  %version: 3 %
  %date_modified: Tue Aug 29 17:33:06 2000 %
  $Copyright:

  Copyright (c) 1999 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/


/*
 * This file contains prototypes for library function calls that are being
 * deprecated. They are preserved here in the interest of all legacy software
 * that depends on them. Please update such software to use the preferred API
 * calls.
 *
 * DO NOT INCLUDE THIS HEADER EXPLICITLY!!
 *
 * Include "nwconnec.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */


#ifndef _OBSOLETE_NWCONNEC_H
#define _OBSOLETE_NWCONNEC_H


#define CONNECTION_PRIVATE              0x0002
#define CONNECTION_PNW                  0x4000

/* the following are for NWGetConnInfo */
/* ALL is VLM, OS2 and NT - NOT NETX */
#define NW_CONN_TYPE           1   /* returns nuint16  (VLM) */
#define NW_CONN_BIND      0x0031
#define NW_CONN_NDS       0x0032
#define NW_CONN_PNW       0x0033
#define NW_AUTHENTICATED       3  /* returns nuint16  = 1 if authenticated (ALL)*/
#define NW_PBURST              4  /* returns nuint16  = 1 if pburst (VLM) */
#define NW_VERSION             8  /* returns nuint16  (VLM)  */
#define NW_HARD_COUNT          9  /* returns WORD (VLM)  */
#define NW_CONN_NUM           13  /* returns nuint16  (ALL)  */
#define NW_TRAN_TYPE          15  /* returns nuint16  (VLM)  */
#define NW_TRAN_IPX       0x0021
#define NW_TRAN_TCP       0x0022
#define NW_SESSION_ID     0x8000  /* returns nuint16) (VLM) */
#define NW_SERVER_ADDRESS 0x8001  /* returns 12 byte address (ALL) */
#define NW_SERVER_NAME    0x8002  /* returns 48 byte string  (ALL) */

#define C_SNAMESIZE 48

#ifdef __cplusplus
extern "C" {
#endif

typedef struct
{
   nuint32  systemElapsedTime;
   nuint8   bytesRead[6];
   nuint8   bytesWritten[6];
   nuint32  totalRequestPackets;
} CONN_USE;

typedef struct
{
  NWCONN_HANDLE   connID;
  nuint16         connectFlags;
  nuint16         sessionID;
  nuint16         connNumber;
  nuint8          serverAddr[12];
  nuint16         serverType;
  nstr8           serverName[C_SNAMESIZE];
  nuint16         clientType;
  nstr8           clientName[C_SNAMESIZE];
} CONNECT_INFO;

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnInfo
(
   NWCONN_HANDLE  connHandle,
   nuint16        type,
   nptr           pData
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionList
(
   nuint16        Mode,
   NWCONN_HANDLE N_FAR * connListBuffer,
   nuint16        connListSize,
   pnuint16       pNumConns
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionNumber
(
   NWCONN_HANDLE        connHandle,
   pnuint16             connNumber
);

#define NWGetConnectionID(a, b, c, d) NWGetConnectionHandle(a, b, c, d)

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionHandle
(
   pnuint8        pServerName,
   nuint16        reserved1,
   NWCONN_HANDLE N_FAR * pConnHandle,
   pnuint16       reserved2
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetPrimaryConnectionID
(
   NWCONN_HANDLE connHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetPrimaryConnectionID
(
   NWCONN_HANDLE N_FAR * pConnHandle
);

#ifndef NWOS2
N_EXTERN_LIBRARY( NWCCODE )
NWIsIDInUse
(
   NWCONN_HANDLE connHandle
);
#endif

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionUsageStats
(
   NWCONN_HANDLE  connHandle,
   nuint16        connNumber,
   CONN_USE N_FAR * pStatusBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetConnectionStatus
(
   NWCONN_HANDLE        connHandle,
   CONNECT_INFO N_FAR * pConnInfo,
   nuint16              connInfoSize
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetDefaultConnectionID
(
   NWCONN_HANDLE N_FAR * pConnHandle
);

N_EXTERN_LIBRARY( void )
NWGetMaximumConnections
(
   pnuint16    pMaxConns
);

#ifdef __cplusplus
}
#endif

#endif
