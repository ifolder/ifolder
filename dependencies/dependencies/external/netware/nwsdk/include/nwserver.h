/******************************************************************************

  %name: nwserver.h %
  %version: 9 %
  %date_modified: Tue Aug 29 18:44:54 2000 %
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

#if ! defined ( NWSERVER_H )
#define NWSERVER_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define LNS_CHECK 0

#ifndef NW_SHORT_NAME_SERVER
# define NW_SHORT_NAME_SERVER 0
#endif

#ifndef NW_LONG_NAME_SERVER
#define NW_LONG_NAME_SERVER 1
#endif

#ifndef NW_ENCP_SERVER
#define NW_ENCP_SERVER 1
#endif

#ifndef NW_EXTENDED_NCP_SERVER
#define NW_EXTENDED_NCP_SERVER 1
#endif

#define VERSION_CHECK 1
# define NW_2X  0
# define NW_30  1
# define NW_311 2
# define NW_32  3
# define NW_40  4

typedef struct
{
   nuint8  serverName[48];
   nuint8  fileServiceVersion;
   nuint8  fileServiceSubVersion;
   nuint16 maximumServiceConnections;
   nuint16 connectionsInUse;
   nuint16 maxNumberVolumes;
   nuint8  revision;
   nuint8  SFTLevel;
   nuint8  TTSLevel;
   nuint16 maxConnectionsEverUsed;
   nuint8  accountVersion;
   nuint8  VAPVersion;
   nuint8  queueVersion;
   nuint8  printVersion;
   nuint8  virtualConsoleVersion;
   nuint8  restrictionLevel;
   nuint8  internetBridge;
   nuint8  reserved[60];
}  VERSION_INFO;

typedef struct
{
   nuint16 majorVersion;
   nuint16 minorVersion;
   nuint16 revision;
}  NETWARE_PRODUCT_VERSION;

/* Defines that are used for the NWCheckNetWareVersion call for values
   that can be returned in the compatibilityFlag nuint8.  */
#define COMPATIBLE               0x00
#define VERSION_NUMBER_TOO_LOW   0x01
#define SFT_LEVEL_TOO_LOW        0x02
#define TTS_LEVEL_TOO_LOW        0x04

N_EXTERN_LIBRARY( NWCCODE )
NWCheckConsolePrivileges
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWDownFileServer
(
   NWCONN_HANDLE  conn,
   nuint8         forceFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerDateAndTime
(
   NWCONN_HANDLE  conn,
   pnuint8        dateTimeBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWSetFileServerDateAndTime
(
   NWCONN_HANDLE  conn,
   nuint8         year,
   nuint8         month,
   nuint8         day,
   nuint8         hour,
   nuint8         minute,
   nuint8         second
);

N_EXTERN_LIBRARY( NWCCODE )
NWCheckNetWareVersion
(
   NWCONN_HANDLE  conn,
   nuint16        minVer,
   nuint16        minSubVer,
   nuint16        minRev,
   nuint16        minSFT,
   nuint16        minTTS,
   pnuint8        compatibilityFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerVersionInfo
(
   NWCONN_HANDLE  conn,
   VERSION_INFO N_FAR * versBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNetWareProductVersion
(
   NWCONN_HANDLE  conn,
   NETWARE_PRODUCT_VERSION N_FAR * version
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerInformation
(
   NWCONN_HANDLE  conn,
   pnstr8         serverName,
   pnuint8        majorVer,
   pnuint8        minVer,
   pnuint8        rev,
   pnuint16       maxConns,
   pnuint16       maxConnsUsed,
   pnuint16       connsInUse,
   pnuint16       numVolumes,
   pnuint8        SFTLevel,
   pnuint8        TTSLevel
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerExtendedInfo
(
   NWCONN_HANDLE  conn,
   pnuint8        accountingVer,
   pnuint8        VAPVer,
   pnuint8        queueingVer,
   pnuint8        printServerVer,
   pnuint8        virtualConsoleVer,
   pnuint8        securityVer,
   pnuint8        internetBridgeVer
);

N_EXTERN_LIBRARY( NWCCODE )
_NWGetFileServerType
(
   NWCONN_HANDLE  conn,
   nuint16        typeFlag,
   pnuint16       serverType
);

N_EXTERN_LIBRARY( NWCCODE )
NWAttachToFileServer
(
   const nstr8   N_FAR * serverName,
   nuint16               scopeFlag,
   NWCONN_HANDLE N_FAR * newConnID
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerLoginStatus
(
   NWCONN_HANDLE  conn,
   pnuint8        loginEnabledFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWLogoutFromFileServer
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWLoginToFileServer
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * password
);

N_EXTERN_LIBRARY( NWCCODE )
NWEnableFileServerLogin
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWDisableFileServerLogin
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetFileServerDescription
(
   NWCONN_HANDLE  conn,
   pnstr8         companyName,
   pnstr8         revision,
   pnstr8         revisionDate,
   pnstr8         copyrightNotice
);

N_EXTERN_LIBRARY( NWCCODE )
NWAttachToFileServerByConn
(
   NWCONN_HANDLE         conn,
   const nstr8   N_FAR * serverName,
   nuint16               scopeFlag,
   NWCONN_HANDLE N_FAR * newConnID
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNetworkSerialNumber
(
   NWCONN_HANDLE  conn,
   pnuint32       serialNum,
   pnuint16       appNum
);

N_EXTERN_LIBRARY( NWCCODE )
NWIsManager
(
   NWCONN_HANDLE  conn
);

#ifdef NWOS2
N_EXTERN_LIBRARY( NWCCODE )
NWLogoutWithLoginID
(
   nuint32        citrixLoginID
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
      #include "obsolete/o_server.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif
