#ifndef _NWNSPACE_H_
#define _NWNSPACE_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwnspace.h
==============================================================================
*/
#include <nwtypes.h>

#include <npackon.h>

#ifndef _NW_NS_INFO
# define _NW_NS_INFO
typedef struct NWNSINFO
{
   LONG nsInfoBitMask;
   LONG fixedBitMask;
   LONG reservedBitMask;
   LONG extendedBitMask;
   WORD fixedBitsDefined;
   WORD reservedBitsDefined;
   WORD extendedBitsDefined;
   LONG fieldsLenTable[32];
   BYTE hugeStateInfo[16];
   LONG hugeDataLength;
} NW_NS_INFO;
#endif

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int GetDataStreamName
(
   int   volume,
   BYTE  dataStream,
   char *name,
   int  *numberOfDataStreams
);

extern int GetNameSpaceName
(
   int   volume,
   LONG  nameSpace,
   char *name,
   int  *numberOfNameSpaces
);

extern int NWGetHugeNSInfo
(
   BYTE  volNum,
   BYTE  nameSpace,
   LONG  dirBase,
   LONG  hugeInfoMask,
   BYTE *hugeStateInfo,
   BYTE *hugeData,
   LONG *hugeDataLen,
   BYTE *nextHugeStateInfo
);

extern int NWGetNameSpaceEntryName
(
   const BYTE *path,
   LONG  nameSpace,
   LONG  maxNameBufferLength,
   BYTE *nameSpaceEntryName
);

extern int NWGetNSInfo
(
   BYTE  volNum,
   BYTE  srcNameSpace,
   BYTE  dstNameSpace,
   LONG  dirBase,
   LONG  nsInfoMask,
   BYTE *nsSpecificInfo
);

extern int NWGetNSLoadedList
(
   BYTE  volNum,
   WORD  loadListSize,
   BYTE *NSLoadedList,
   WORD *returnListSize
);

extern int NWQueryNSInfoFormat
(
   BYTE        nameSpace,
   BYTE        volNum,
   NW_NS_INFO *nsInfo
);

extern int NWSetHugeNSInfo
(
   BYTE        volNum,
   BYTE        nameSpace,
   LONG        dirBase,
   LONG        hugeInfoMask,
   const BYTE *hugeStateInfo,
   LONG        hugeDataLen,
   const BYTE *hugeData,
   BYTE       *nextHugeStateInfo,
   LONG       *hugeDataUsed
);

extern int NWSetNameSpaceEntryName
(
   const BYTE *path,
   LONG        nameSpace,
   const BYTE *nameSpaceEntryName
);

extern int NWSetNSInfo
(
   BYTE        volNum,
   BYTE        srcNameSpace,
   BYTE        dstNameSpace,
   LONG        dirBase,
   LONG        nsInfoMask,
   LONG        nsSpecificInfoLen,
   const BYTE *nsSpecificInfo
);

extern BYTE SetCurrentNameSpace
(
   BYTE newNameSpace
);

extern BYTE SetTargetNameSpace
(
   BYTE newNameSpace
);

#ifdef __cplusplus
}
#endif

#endif
