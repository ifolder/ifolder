/******************************************************************************

  %name :          %
  %version :       %
  %date_modified : %
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
#if ! defined ( NWDSBUFT_H )
#define NWDSBUFT_H

#include <time.h>

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWDSDC_H )
#include "nwdsdc.h"
#endif

#if ! defined ( NWDSDEFS_H )
#include "nwdsdefs.h"
#endif

#if ! defined ( NWDSATTR_H )
#include "nwdsattr.h"
#endif

#include "npackon.h"

#define  INPUT_BUFFER   0x00000001

typedef struct
{
   nuint32  operation;
   nuint32  flags;
   nuint32  maxLen;
   nuint32  curLen;
   pnuint8  lastCount;
   pnuint8  curPos;
   pnuint8  data;
} Buf_T, N_FAR *pBuf_T, N_FAR * N_FAR *ppBuf_T;

typedef struct
{
   nuint32  objectFlags;
   nuint32  subordinateCount;
   time_t   modificationTime;
   char     baseClass[MAX_SCHEMA_NAME_BYTES + 2];
} Object_Info_T, N_FAR *pObject_Info_T;

typedef struct
{
   nuint32  length;
   nuint8   data[MAX_ASN1_NAME];
} Asn1ID_T, N_FAR *pAsn1ID_T;

typedef struct
{
   nuint32  attrFlags;
   nint32  attrSyntaxID;
   nint32  attrLower;
   nint32  attrUpper;
   Asn1ID_T asn1ID;
} Attr_Info_T, N_FAR *pAttr_Info_T;

typedef struct
{
   nuint32  classFlags;
   Asn1ID_T asn1ID;
} Class_Info_T, N_FAR *pClass_Info_T;

typedef struct
{
   nuint32  ID;
   char     defStr[MAX_SCHEMA_NAME_BYTES + 2];
   nflag16  flags;
} Syntax_Info_T, N_FAR *pSyntax_Info_T;

#define NWDSPutClassName(c, b, n) NWDSPutClassItem(c, b, n)
#define NWDSPutSyntaxName(c, b, n) NWDSPutClassItem(c, b, n)

#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAllocBuf
(
   size_t   size,
   ppBuf_T  buf
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSComputeAttrValSize
(
   NWDSContextHandle context,
   pBuf_T            buf,
   nuint32           syntaxID,
   pnuint32          attrValSize
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSFreeBuf
(
   pBuf_T   buf
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrCount
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          attrCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrDef
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            attrName,
   pAttr_Info_T      attrInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrName
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            attrName,
   pnuint32          attrValCount,
   pnuint32          syntaxID
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrVal
(
   NWDSContextHandle context,
   pBuf_T            buf,
   nuint32           syntaxID,
   nptr              attrVal
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrValModTime
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pTimeStamp_T      timeStamp
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetAttrValFlags
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          valueFlags
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetClassDef
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            className,
   pClass_Info_T     classInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetClassDefCount
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          classDefCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetClassItem
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            itemName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetClassItemCount
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          itemCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetObjectCount
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          objectCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetObjectName
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            objectName,
   pnuint32          attrCount,
   pObject_Info_T    objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetObjectNameAndInfo
(
   NWDSContextHandle    context,
   pBuf_T               buf,
   pnstr8               objectName,
   pnuint32             attrCount,
   ppnstr8              objectInfo
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetDSIInfo
(
   NWDSContextHandle context,
   nptr              buf,
   nuint32           bufLen,
   nuint32           infoFlag,
   nptr              data
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetPartitionInfo
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            partitionName,
   pnuint32          replicaType
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetReplicaReferenceRootID
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          replicaRootID
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetServerName
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            serverName,
   pnuint32          partitionCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetPartitionExtInfoPtr
(
   NWDSContextHandle context,
   pBuf_T            buf,
   ppnstr8            infoPtr,
   ppnstr8            infoPtrEnd
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetPartitionExtInfo
(
   NWDSContextHandle context,
   pnstr8            infoPtr,
   pnstr8            limit,
   nflag32           infoFlag,
   pnuint32          length,
   nptr              data
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetSyntaxCount
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnuint32          syntaxCount
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetSyntaxDef
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            syntaxName,
   pSyntax_Info_T    syntaxDef
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSInitBuf
(
   NWDSContextHandle context,
   nuint32           operation,
   pBuf_T            buf
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutAttrName
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            attrName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutAttrVal
(
   NWDSContextHandle context,
   pBuf_T            buf,
   nuint32           syntaxID,
   nptr              attrVal
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutAttrNameAndVal
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            attrName,
   nuint32           syntaxID,
   nptr              attrVal
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutChange
(
   NWDSContextHandle context,
   pBuf_T            buf,
   nuint32           changeType,
   pnstr8            attrName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutChangeAndVal
(
   NWDSContextHandle context,
   pBuf_T            buf,
   nuint32           changeType,
   pnstr8            attrName,
   nuint32           syntaxID,
   nptr              attrVal
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPutClassItem
(
   NWDSContextHandle context,
   pBuf_T            buf,
   pnstr8            itemName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSBeginClassItem
(
   NWDSContextHandle context,
   pBuf_T            buf
);

#ifdef __cplusplus
   }
#endif

#include "npackoff.h"
#endif   /* NWDSBUFT_H */
