/******************************************************************************

  %name: o_ndscon.h %
  %version: 6 %
  %date_modified: Tue Aug 29 17:22:20 2000 %
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

#ifndef _OBSOLETE_NWNDSCON_H
#define _OBSOLETE_NWNDSCON_H

#ifdef __cplusplus
   extern "C" {
#endif

   /* replacement - NWCCGetConnInfo 
   */         
N_EXTERN_LIBRARY (NWCCODE)
NWDSGetConnectionInfo
(
   NWCONN_HANDLE  connHandle,
   pnuint8        connStatus,
   pnuint8        connType,
   pnuint8        serverFlags,
   pnuint8        serverName,
   pnuint8        transType,
   pnuint32       transLen,
   pnuint8        transBuf,
   pnuint16       distance,
   pnuint16       maxPacketSize
);

   /* replacement - NWDSOpenMonitoredConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWDSGetMonitoredConnection
(
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWGetPreferredConnName & NWCCOpenConnByName
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetPreferredDSServer
(
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCLicenseConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWDSLockConnection
(
   NWCONN_HANDLE connHandle
);

   /* replacement - NWCCScanConnRefs
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetNextConnectionID
(
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCOpenConnByAddr followed by NWCCLicenseConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWDSGetConnectionSlot
(
   nuint8               connType,
   nuint8               transType,
   nuint32              transLen,
   pnuint8              transBuf,
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCScanConnInfo
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetNearestDirectoryService
(
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCScanConnInfo, NWCCOpenConnByRef, NWCCLicenseConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetConnectionIDFromAddress
(
   nuint8               transType,
   nuint32              transLen,
   pnuint8              transBuf,
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCScanConnInfo, NWCCOpenConnByRef, NWCCLicenseConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetConnectionIDFromName
(
   nuint32              nameLen,
   pnuint8              name,
   NWCONN_HANDLE N_FAR  *connHandle
);

   /* replacement - NWCCScanConnInfo, NWCCOpenConnByRef
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetNearestDSConnRef
(
   pnuint32    connRef
);

   /* replacement - NWDSSetDefNameContext
   */
N_EXTERN_LIBRARY (NWCCODE)
NWSetDefaultNameContext
(
   nuint16  contextLength,
   pnuint8  context
);

   /* replacement - NWDSGetDefNameContext
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetDefaultNameContext
(
   nuint16  bufferSize,
   pnuint8  context
);

   /* replacement - NWCCGetNumConns
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetNumConnections
(
   pnuint16 numConnections
);

   /* replacement - NWDSCanDSAuthenticate
   */
N_EXTERN_LIBRARY (NWCCODE)
NWIsDSAuthenticated
(
   void
);

   /* replacement - NWCCUnlicenseConn
   */
N_EXTERN_LIBRARY (NWCCODE)
NWDSUnlockConnection
(
   NWCONN_HANDLE connHandle
);

   /* replacement - NWCCGetPrefServerName
   */
N_EXTERN_LIBRARY (NWCCODE)
NWGetPreferredConnName
(
   pnuint8  preferredName,
   pnuint8  preferredType
);

   /* replacment - NWCSysCloseConnRef
   */
N_EXTERN_LIBRARY (NWCCODE)
NWFreeConnectionSlot
(
   NWCONN_HANDLE  connHandle,
   nuint8         disconnectType
);

   /* replacement - NONE (monitored connections are managed automatically
    * by the client software)
   */
N_EXTERN_LIBRARY (NWCCODE)
NWDSSetMonitoredConnection
(
   NWCONN_HANDLE  connHandle
);


#ifdef __cplusplus
   }
#endif

#endif  /* _OBSOLETE_NWNDSCON_H */
