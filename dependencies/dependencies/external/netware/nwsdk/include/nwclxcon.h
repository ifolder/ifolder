/****************************************************************************

  %name: nwclxcon.h %
  %version: 21 %
  %date_modified: Thu Oct 14 13:13:16 1999 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 ***************************************************************************/


#if ! defined ( NWCLXCON_H )
#define NWCLXCON_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWAPIDEF_H )
#include "nwapidef.h"
#endif

#if ! defined ( NWCALDEF_H )  /* include nwcaldef.h for connection handle */
#include "nwcaldef.h"
#endif

#include "npackon.h"

/* Authentication States */
#define NWCC_AUTHENT_STATE_NONE     0x0000
#define NWCC_AUTHENT_STATE_BIND     0x0001
#define NWCC_AUTHENT_STATE_NDS      0x0002

/* Broadcast States */
#define NWCC_BCAST_PERMIT_ALL       0x0000
#define NWCC_BCAST_PERMIT_SYSTEM    0x0001
#define NWCC_BCAST_PERMIT_NONE      0x0002
#define NWCC_BCAST_PERMIT_POLL      0x0003  /* DOS Windows and OS/2 only */

/* NDS States */
#define NWCC_NDS_NOT_CAPABLE        0x0000
#define NWCC_NDS_CAPABLE            0x0001

/* License States */
#define NWCC_NOT_LICENSED           0x0000
#define NWCC_CONNECTION_LICENSED    0x0001
#define NWCC_HANDLE_LICENSED        0x0002

/* Name Format types */
#define NWCC_NAME_FORMAT_NDS        0x0001  /* Unicode full dot format name */
#define NWCC_NAME_FORMAT_BIND       0x0002
#define NWCC_NAME_FORMAT_NDS_TREE   0x0008
#define NWCC_NAME_FORMAT_WILD       0x8000

/* Transport types */
#define NWCC_TRAN_TYPE_IPX          0x00000001
#define NWCC_TRAN_TYPE_DDP          0x00000003
#define NWCC_TRAN_TYPE_ASP          0x00000004
#define NWCC_TRAN_TYPE_UDP          0x00000008
#define NWCC_TRAN_TYPE_TCP          0x00000009
#define NWCC_TRAN_TYPE_UDP6         0x0000000A
#define NWCC_TRAN_TYPE_TCP6         0x0000000B
#define NWCC_TRAN_TYPE_WILD         0x00008000

/* Open States */
#define NWCC_OPEN_LICENSED          0x0001
#define NWCC_OPEN_UNLICENSED        0x0002
#define NWCC_OPEN_PRIVATE           0x0004
#define NWCC_OPEN_PUBLIC            0x0008
#define NWCC_OPEN_EXISTING_HANDLE   0x0010
#define NWCC_OPEN_NEAREST           0x0100
#define NWCC_OPEN_IGNORE_CACHE      0x0200

/* Scan connection information flags (nuint value) */
#define NWCC_MATCH_NOT_EQUALS              0x0000
#define NWCC_MATCH_EQUALS                  0x0001
#define NWCC_RETURN_PUBLIC                 0x0002
#define NWCC_RETURN_PRIVATE                0x0004
#define NWCC_RETURN_LICENSED               0x0008
#define NWCC_RETURN_UNLICENSED             0x0010

/* Reserved Value */
#define NWCC_RESERVED               0x0000

/* Values used with Security Flags */
/* NOTE: 
Two previously defined security flags have been redefined in order to be
compliant with the ANSI standard maximum length of 31.  Here is a list showing
what the #define was previously, and what it has been redefined to be.
PREVIOUSLY                          CURRENTLY
NWCC_SECURITY_SIGNING_NOT_IN_USE    NWCC_SECUR_SIGNING_NOT_IN_USE
NWCC_SECURITY_LEVEL_SIGN_HEADERS    NWCC_SECUR_LEVEL_SIGN_HEADERS
*/
#define NWCC_SECURITY_SIGNING_NOT_IN_USE     0x00000000 
#define NWCC_SECURITY_SIGNING_IN_USE      0x00000001
#define NWCC_SECURITY_LEVEL_CHECKSUM      0x00000100
#define NWCC_SECURITY_LEVEL_SIGN_HEADERS     0x00000200
#define NWCC_SECURITY_LEVEL_SIGN_ALL      0x00000400
#define NWCC_SECURITY_LEVEL_ENCRYPT       0x00000800

/* Feature Codes */
#define NWCC_FEAT_PRIV_CONN         0x0001
#define NWCC_FEAT_REQ_AUTH          0x0002
#define NWCC_FEAT_SECURITY          0x0003
#define NWCC_FEAT_NDS               0x0004
#define NWCC_FEAT_NDS_MTREE         0x0005
#define NWCC_FEAT_PRN_CAPTURE       0x0006

typedef struct tagNWCCTranAddr
{
   nuint32  type;
   nuint32  len;
   pnuint8  buffer;
} NWCCTranAddr, N_FAR *pNWCCTranAddr;

typedef struct tagNWCCVersion
{
   nuint major;
   nuint minor;
   nuint revision;
}NWCCVersion, N_FAR *pNWCCVersion;

/* Info Types */
#define NWCC_INFO_NONE               0
#define NWCC_INFO_AUTHENT_STATE      1
#define NWCC_INFO_BCAST_STATE        2
#define NWCC_INFO_CONN_REF           3
#define NWCC_INFO_TREE_NAME          4
#define NWCC_INFO_CONN_NUMBER        5
#define NWCC_INFO_USER_ID            6
#define NWCC_INFO_SERVER_NAME        7
#define NWCC_INFO_NDS_STATE          8
#define NWCC_INFO_MAX_PACKET_SIZE    9
#define NWCC_INFO_LICENSE_STATE     10
#define NWCC_INFO_DISTANCE          11
#define NWCC_INFO_SERVER_VERSION    12
#define NWCC_INFO_TRAN_ADDR         13    /* Version 2 */
#define NWCC_INFO_IDENTITY_HANDLE   14    /* Version 3 */

#define NWCC_INFO_RETURN_ALL        0xFFFF

/* Current Info Version */
#define NWCC_INFO_VERSION_1         0x0001
#define NWCC_INFO_VERSION_2         0x0002
#define NWCC_INFO_VERSION_3         0x0003

/* Should use above definitions instead of this one */
#define NWCC_INFO_VERSION           NWCC_INFO_VERSION_1

typedef struct tagNWCCConnInfo
{
   nuint          authenticationState;
   nuint          broadcastState;
   nuint32        connRef;
   nstr           treeName[NW_MAX_TREE_NAME_LEN];
   nuint          connNum;
   nuint32        userID;
   nstr           serverName[NW_MAX_SERVER_NAME_LEN];
   nuint          NDSState;
   nuint          maxPacketSize;
   nuint          licenseState;
   nuint          distance;
   NWCCVersion    serverVersion;
#ifdef NWCC_INFO_VERSION_2
   pNWCCTranAddr  tranAddr;
#endif
#ifdef NWCC_INFO_VERSION_3
   nuint32        identityHandle;
#endif
}NWCCConnInfo, N_FAR *pNWCCConnInfo;

typedef struct tagNWCCFrag
{
   nptr        address;
   nuint       length;
}NWCCFrag, N_FAR *pNWCCFrag;

#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY( NWRCODE )
NWCLXInit
(
   nptr reserved1, 
   nptr reserved2
);

N_EXTERN_LIBRARY( NWRCODE )
NWCLXTerm
(
   nptr reserved
);

N_EXTERN_LIBRARY( void )
NWCCGetCLXVersion
(
  pnuint8 majorVersion,
  pnuint8 minorVersion,
  pnuint8 revisionLevel,
  pnuint8 betaReleaseLevel
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCOpenConnByName
(
   NWCONN_HANDLE       startConnHandle,     /* in     */
   const nstr8 N_FAR * name,                /* in     */
   nuint               nameFormat,          /* in     */
   nuint               openState,           /* in     */
   nuint               tranType,            /* in     * use NWCC_RESERVED */
   pNWCONN_HANDLE      pConnHandle          /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCOpenConnByAddr
(
   const NWCCTranAddr N_FAR * tranAddr,   /* in     */
   nuint                      openState,  /* in     */
   nuint                      reserved,   /* in     * use NWCC_RESERVED */
   pNWCONN_HANDLE             pConnHandle /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCOpenConnByRef
(
   nuint32        connRef,             /* in     */
   nuint          openState,           /* in     */
   nuint          reserved,            /* in     * use NWCC_RESERVED */
   pNWCONN_HANDLE pConnHandle          /*    out */          
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCCloseConn
(
   NWCONN_HANDLE  connHandle                    /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCSysCloseConnRef
(
   nuint32        connRef                       /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCMakeConnPermanent
(
   NWCONN_HANDLE  connHandle                    /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCLicenseConn
(
   NWCONN_HANDLE  connHandle                    /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCUnlicenseConn
(
   NWCONN_HANDLE  connHandle                    /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnRef
(
   NWCONN_HANDLE  connHandle,                   /* in     */
   pnuint32       connRef                       /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetPrefServerName
(
   nuint       len,                             /* in     */
   pnstr       prefServer                       /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCSetPrefServerName
(
   const nstr N_FAR * prefServer                /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetPrimConnRef
(
   pnuint32    connRef                          /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCSetPrimConn
(
   NWCONN_HANDLE  connHandle                    /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCScanConnRefs
(  
   pnuint32    scanIterator,                    /* in/out : initialize to 0 */
   pnuint32    connRef                          /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnInfo
(
   NWCONN_HANDLE  connHandle,                   /* in     */
   nuint          infoType,                     /* in     */
   nuint          len,                          /* in     */
   nptr           buffer                        /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnRefInfo
(
   nuint32        connRef,                      /* in     */
   nuint          infoType,                     /* in     */
   nuint          len,                          /* in     */
   nptr           buffer                        /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetAllConnInfo
(
   NWCONN_HANDLE  connHandle,                /* in     */
   /* connInfoVersion should always be set to NWCC_INFO_VERSION 
         or NWCC_INFO_VERSION_n */
   nuint          connInfoVersion,           /* in     */
   pNWCCConnInfo  connInfoBuffer             /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetAllConnRefInfo
(
   nuint32        connRef,                   /* in     */
   /* connInfoVersion should always be set to NWCC_INFO_VERSION 
         or NWCC_INFO_VERSION_n */
   nuint          connInfoVersion,           /* in     */
   pNWCCConnInfo  connInfoBuffer             /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCScanConnInfo
(
   pnuint32           scanIterator,                   /* in    */
   nuint              scanInfoLevel,                  /* in    */
   const void N_FAR * scanConnInfo,                   /* in    */
   nuint              scanFlags,                      /* in    */
   /* connInfoVersion should always be set to NWCC_INFO_VERSION 
         or NWCC_INFO_VERSION_n */
   nuint              connInfoVersion,                /* in  */
   nuint              returnInfoLevel,                /* in  */
   nptr               returnConnInfo,                 /*   out */
   pnuint32           connReference                   /*   out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnAddressLength
(
   NWCONN_HANDLE  connHandle,                /* in     */
   pnuint32       addrLen                    /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnRefAddressLength
(
   nuint32           connRef,                /* in     */
   pnuint32          addrLen                 /*    out */
);


N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnAddress
(
   NWCONN_HANDLE  connHandle,                /* in     */
   nuint32        bufferLen,                 /* in     */
   pNWCCTranAddr  tranAddr                   /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetConnRefAddress
(
   nuint32        connRef,                   /* in     */
   nuint32        bufferLen,                 /* in     */
   pNWCCTranAddr  tranAddr                   /*    out */
); 

N_EXTERN_LIBRARY( NWRCODE )
NWCCOpenConnByPref
(
   nuint          tranType,            /* in     * NWCC_TRAN_TYPE_IPX */
   nuint          openState,           /* in     */
   nuint          reserved,            /* in     * use NWCC_RESERVED  */
   pNWCONN_HANDLE pConnHandle          /*    out */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCQueryFeature
(
   nuint          featureCode          /* in     */
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCGetNumConns
(
   pnuint         maxConns,            /*    out */
   pnuint         publicConns,         /*    out */
   pnuint         myPrivateConns       /*    out */
);


N_EXTERN_LIBRARY( NWRCODE )
NWCCGetSecurityFlags
(
   pnuint32     enabSecurityFlags,
   pnuint32     prefSecurityFlags,
   pnuint32     reqSecurityFlags
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCSetSecurityFlags
(
   nuint32     prefSecurityFlags,
   nuint32     reqSecurityFlags
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCRenegotiateSecurityLevel
(
   NWCONN_HANDLE           connHandle,
   nuint32                 securityFlags
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCResetRequester
(
   pnuint32       keepConnRef,
   nuint          connFlags,
   pnstr          firstLocalAlias
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCRequest
(
   NWCONN_HANDLE           connHandle,
   nuint                   function,
   nuint                   numReqFrags,
   const NWCCFrag N_FAR *  reqFrags,
   nuint                   numReplyFrags,
   pNWCCFrag               replyFrags,
   pnuint                  actualReplyLen
);

N_EXTERN_LIBRARY( NWRCODE )
NWCCFragmentRequest
(
   NWCONN_HANDLE           connHandle,
   nuint                   function,
   nuint                   verb,
   nuint                   reserved,
   nuint                   numReqFrags,
   const NWCCFrag N_FAR  * reqFrags,
   nuint                   numReplyFrags,
   pNWCCFrag               replyFrags,
   pnuint                  actualReplyLen
);

   /* The following functions are NLM CLIB specific and not supported in the 
    * NLM LibC x-plat libraries.  The LibC stddef.h file defines
    * __NOVELL_LIBC__
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)

N_EXTERN_LIBRARY( NWRCODE )
NWCCSetCurrentConnection
(
   NWCONN_HANDLE           connHandle
);

#endif

#ifdef __cplusplus
}
#endif

#include "npackoff.h"
#endif /* NWCLXCON_INC */
