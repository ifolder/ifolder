/******************************************************************************

  %name: nwdsmisc.h %
  %version: 8 %
  %date_modified: Fri Apr  9 09:51:36 1999 %
  $Copyright:

  Copyright (c) 1989-1998 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/
#if ! defined ( NWDSMISC_H )
#define NWDSMISC_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWDSBUFT_H )
#include "nwdsbuft.h"
#endif

#if ! defined ( NUNICODE_H )
#include "nunicode.h"
#endif

#include "npackon.h"

#define DS_SYNTAX_NAMES    0
#define DS_SYNTAX_DEFS     1

#define DS_STRING             0x0001   /* string, can be used in names */
#define DS_SINGLE_VALUED      0x0002
#define DS_SUPPORTS_ORDER     0x0004
#define DS_SUPPORTS_EQUAL     0x0008
#define DS_IGNORE_CASE        0x0010   /* Ignore case          */
#define DS_IGNORE_SPACE       0x0020   /* Ignore white space   */
#define DS_IGNORE_DASH        0x0040   /* Ignore dashes        */
#define DS_ONLY_DIGITS        0x0080
#define DS_ONLY_PRINTABLE     0x0100
#define DS_SIZEABLE           0x0200
#define DS_BITWISE_EQUAL      0x0400

typedef struct
{
   nuint32  statsVersion;
   nuint32  noSuchEntry;
   nuint32  localEntry;
   nuint32  typeReferral;
   nuint32  aliasReferral;
   nuint32  requestCount;
   nuint32  requestDataSize;
   nuint32  replyDataSize;
   nuint32  resetTime;
   nuint32  transportReferral;
   nuint32  upReferral;
   nuint32  downReferral;
} NDSStatsInfo_T, N_FAR *pNDSStatsInfo_T;


/* the following structure is used by NWDSGetNDSInfo() */
typedef struct
{
   nuint32  major;
   nuint32  minor;
   nuint32  revision;
} NDSOSVersion_T, N_FAR *pNDSOSVersion_T;


#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCloseIteration
(
   NWDSContextHandle context,
   nint32            iterationHandle,
   nuint32           operation
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetSyntaxID
(
   NWDSContextHandle context,
   pnstr8            attrName,
   pnuint32          syntaxID
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadSyntaxes
(
   NWDSContextHandle context,
   nuint32           infoType,
   nbool8            allSyntaxes,
   pBuf_T            syntaxNames,
   pnint32           iterationHandle,
   pBuf_T            syntaxDefs
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReadSyntaxDef
(
   NWDSContextHandle context,
   nuint32           syntaxID,
   pSyntax_Info_T    syntaxDef
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReplaceAttrNameAbbrev
(
   NWDSContextHandle context,
   pnstr8            inStr,
   pnstr8            outStr
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetObjectHostServerAddress
(
   NWDSContextHandle context,
   pnstr8            objectName,
   pnstr8            serverName,
   pBuf_T            netAddresses
);

N_EXTERN_LIBRARY (void)
NWGetNWNetVersion
(
   nuint8 N_FAR *majorVersion,
   nuint8 N_FAR *minorVersion,
   nuint8 N_FAR *revisionLevel,
   nuint8 N_FAR *betaReleaseLevel
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWIsDSServer
(
   NWCONN_HANDLE  conn,
   pnstr8         treeName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetBinderyContext
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle,
   pnuint8           BinderyEmulationContext
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRepairTimeStamps
(
   NWDSContextHandle context,
   pnstr8            partitionRoot
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWGetFileServerUTCTime
(
   NWCONN_HANDLE  conn,
   pnuint32       time
);


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetDSVerInfo
(
   NWCONN_HANDLE  conn,
   pnuint32       dsVersion,
   pnuint32       rootMostEntryDepth,
   pnstr8         sapName,
   pnuint32       flags,
   punicode       treeName
);

N_EXTERN_LIBRARY( NWDSCCODE )
NWDSGetNDSInfo
(
   NWDSContextHandle context,
   pBuf_T            resultBuffer,
   nflag32           requestedField,
   nptr              data
);

N_EXTERN_LIBRARY( NWDSCCODE )
NWDSReadNDSInfo
(
   NWCONN_HANDLE  connHandle,
   nflag32        requestedFields,
   pBuf_T         resultBuffer
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSyncReplicaToServer
(
   NWDSContextHandle context,
   pnstr8            serverName,
   pnstr8            partitionRootName,
   pnstr8            destServerName,
   nuint32           actionFlags,
   nuint32           delaySeconds
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSReloadDS
(
   NWDSContextHandle context,
   pnstr8            serverName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWNetInit
(
   nptr  reserved1,
   nptr  reserved2
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWNetTerm
(
   nptr  reserved
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetNDSStatistics
(
   NWDSContextHandle context,
   pnstr8            serverName,
   nuint             statsInfoLen,
   pNDSStatsInfo_T   statsInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSResetNDSStatistics
(
   NWDSContextHandle context,
   pnstr8            serverName
);


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetNDSIntervals
(
   NWCONN_HANDLE  connHandle,       /* IN     */
   nflag32        reqIntervalFlags, /* IN     */
   pnflag32       repIntervalFlags, /*    OUT */
   pnuint32       intervals         /*    OUT */
);


N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSetNDSIntervals
(
   NWCONN_HANDLE  connHandle,       /* IN     */
   nflag32        intervalFlags,    /* IN     */
   nuint          numIntervals,     /* IN     */
   pnuint32       intervals         /* IN     */
);


#ifdef __cplusplus
   }
#endif

#include "npackoff.h"
#endif
