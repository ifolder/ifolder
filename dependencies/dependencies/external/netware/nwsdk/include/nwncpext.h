/******************************************************************************

  %name: nwncpext.h %
  %version: 7 %
  %date_modified: Mon Oct 18 15:48:25 1999 %
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

#if ! defined ( NWNCPEXT_H )
#define NWNCPEXT_H

#if ! defined ( NWCALDEF_H )
# include "nwcaldef.h"
#endif

#if ! defined ( NWMISC_H )    /* Needed to defined NWFRAGMENT */
# include "nwmisc.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define BEGIN_SCAN_NCP_EXTENSIONS    0xFFFFFFFF
#define NW_NCPX_BEGIN_SCAN 0xFFFFFFFF
#define MAX_NCP_EXTENSION_NAME_BYTES 33

#if defined( N_PLAT_NLM )
   #define NWGetNCPExtensionInfo       NWGetNCPExtensionInfo2
   #define NWScanNCPExtensions         NWScanNCPExtensions2
   
N_EXTERN_LIBRARY_C( nint )
NWDeRegisterNCPExtension
(
   nptr     queryData
);

N_EXTERN_LIBRARY( nint )
NWRegisterNCPExtension
(
   nstr8        NCPExtensionName,
   nuint8       (*NCPExtensionHandler)(
   struct NCPExtensionClient *NCPExtensionClient,
   nptr     requestData,
   nuint32  requestDataLen,
   nptr     replyData,
   pnuint32 replyDataLen),
   void         (*ConnectionEventHandler)(
   nuint32  connection,
   nuint32  eventType),
   void         (*ReplyBufferManager)(
   struct NCPExtensionClient *NCPExtensionClient,
   nptr     replyBuffer),
   nuint8       majorVersion,
   nuint8       minorVersion,
   nuint8       revision,
   pnptr        queryData
);

typedef struct NCPExtensionClient NCPExtensionClient;

struct NCPExtensionClient {
   nuint32  connection;
   nuint32  task;
};

N_EXTERN_LIBRARY( nint )
NWRegisterNCPExtensionByID
(
   nuint32      NCPExtensionID,
   nstr8        NCPExtensionName,
   nuint8       (*NCPExtensionHandler)(
   struct NCPExtensionClient *NCPExtensionClient,
   nptr     requestData,
   nuint32  requestDataLen,
   nptr     replyData,
   pnuint32 replyDataLen),
   void         (*ConnectionEventHandler)(
   nuint32  connection,
   nuint32  eventType),
   void         (*ReplyBufferManager)(
   struct NCPExtensionClient *NCPExtensionClient,
   nptr     replyBuffer),
   nuint8       majorVersion,
   nuint8       minorVersion,
   nuint8       revision,
   pnptr        queryData
);

#endif

N_EXTERN_LIBRARY( NWCCODE )
NWGetNCPExtensionInfo
(
   NWCONN_HANDLE  conn,
   nuint32        NCPExtensionID,
   pnstr8         NCPExtensionName,
   pnuint8        majorVersion,
   pnuint8        minorVersion,
   pnuint8        revision,
   pnuint8        queryData
);

N_EXTERN_LIBRARY( NWCCODE )
NWNCPExtensionRequest
(
   NWCONN_HANDLE      conn,
   nuint32            NCPExtensionID,
   const void N_FAR * requestData,
   nuint16            requestDataLen,
   nptr               replyData,
   pnuint16           replyDataLen
);

N_EXTERN_LIBRARY( NWCCODE )
NWFragNCPExtensionRequest
(
   NWCONN_HANDLE  conn,
   nuint32        NCPExtensionID,
   nuint16        reqFragCount,
   NW_FRAGMENT N_FAR * reqFragList,
   nuint16        replyFragCount,
   NW_FRAGMENT N_FAR * replyFragList
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanNCPExtensions
(
   NWCONN_HANDLE  conn,
   pnuint32       NCPExtensionID,
   pnstr8         NCPExtensionName,
   pnuint8        majorVersion,
   pnuint8        minorVersion,
   pnuint8        revision,
   pnuint8        queryData
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNCPExtensionInfoByName
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * NCPExtensionName,
   pnuint32            NCPExtensionID,
   pnuint8             majorVersion,
   pnuint8             minorVersion,
   pnuint8             revision,
   pnuint8             queryData
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNCPExtensionsList
(
   NWCONN_HANDLE  conn,
   pnuint32       startNCPExtensionID,
   pnuint16       itemsInList,
   pnuint32       NCPExtensionIDList
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetNumberNCPExtensions
(
   NWCONN_HANDLE  conn,
   pnuint32       numNCPExtensions
);

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif
