/******************************************************************************

  %name:          nwdsdc.h %
  %version:       17 %
  %date_modified: Tue Aug 29 18:16:45 2000 %
  $Copyright:

  Copyright (c) 1989-1997 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/
#if ! defined ( NWDSDC_H )
#define NWDSDC_H

#if ! defined( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#include "npackon.h"

/* Directory Context Key names */

#define  DCK_FLAGS               1
#define  DCK_CONFIDENCE          2
#define  DCK_NAME_CONTEXT        3
#define  DCK_TRANSPORT_TYPE      4
#define  DCK_REFERRAL_SCOPE      5
#define  DCK_LAST_CONNECTION     8
#define  DCK_LAST_SERVER_ADDRESS 9  /* CLIB NLM only */
#define  DCK_LAST_ADDRESS_USED   10 /* CLIB NLM only */
#define  DCK_TREE_NAME           11
#define  DCK_DSI_FLAGS           12
#define  DCK_NAME_FORM           13
#define  DCK_NAME_CACHE_DEPTH    15
#define  DCK_AUTHENTICATION_MODE 20

/* DCK_FLAGS bit values key */

#define  DCV_DEREF_ALIASES              0x00000001L
#define  DCV_XLATE_STRINGS              0x00000002L
#define  DCV_TYPELESS_NAMES             0x00000004L
#define  DCV_ASYNC_MODE                 0x00000008L
#define  DCV_CANONICALIZE_NAMES         0x00000010L
#define  DCV_DEREF_BASE_CLASS           0x00000040L
#define  DCV_DISALLOW_REFERRALS         0x00000080L
#define  DCV_ALWAYS_EVALUATE_REFERRALS  0x00000100L
#define  DCV_EXTERNAL_REFERENCES        0x00000200L


/* values for DCK_CONFIDENCE key */
#define  DCV_LOW_CONF         0
#define  DCV_MED_CONF         1
#define  DCV_HIGH_CONF        2

#define  MAX_MESSAGE_LEN            (0xFC00)  /* (63*1024) */
#define  DEFAULT_MESSAGE_LEN        (4*1024)

/* values for DCK_REFERRAL_SCOPE key */
#define  DCV_ANY_SCOPE              0
#define  DCV_COUNTRY_SCOPE          1
#define  DCV_ORGANIZATION_SCOPE     2
#define  DCV_LOCAL_SCOPE            3

/* values for DCK_DSI_FLAGS key */
#define DSI_OUTPUT_FIELDS               0x00000001L
#define DSI_ENTRY_ID                    0x00000002L
#define DSI_ENTRY_FLAGS                 0x00000004L
#define DSI_SUBORDINATE_COUNT           0x00000008L
#define DSI_MODIFICATION_TIME           0x00000010L
#define DSI_MODIFICATION_TIMESTAMP      0x00000020L
#define DSI_CREATION_TIMESTAMP          0x00000040L
#define DSI_PARTITION_ROOT_ID           0x00000080L
#define DSI_PARENT_ID                   0x00000100L
#define DSI_REVISION_COUNT              0x00000200L
#define DSI_REPLICA_TYPE                0x00000400L
#define DSI_BASE_CLASS                  0x00000800L
#define DSI_ENTRY_RDN                   0x00001000L
#define DSI_ENTRY_DN                    0x00002000L
#define DSI_PARTITION_ROOT_DN           0x00004000L
#define DSI_PARENT_DN                   0x00008000L
#define DSI_PURGE_TIME                  0x00010000L
#define DSI_DEREFERENCE_BASE_CLASS      0x00020000L
#define DSI_REPLICA_NUMBER              0x00040000L
#define DSI_REPLICA_STATE               0x00080000L
#define DSI_FEDERATION_BOUNDARY         0x00100000L
#define DSI_SCHEMA_BOUNDARY             0x00200000L


/* values for DCK_NAME_FORM key */
#define DCV_NF_PARTIAL_DOT             1
#define DCV_NF_FULL_DOT                2
#define DCV_NF_SLASH                   3

/* values for DCK_AUTHENTICATION_MODE key */
#define DCV_PUBLIC_AUTHEN              1
#define DCV_PRIVATE_AUTHEN             2


typedef  nuint32   NWDSContextHandle;


#if defined(N_PLAT_NLM)
typedef struct
{
   nuint32   addressType;
   nuint32   addressLength;
   nuint8    address[12];
} NWDSIPXNetworkAddr;
#endif

#ifdef __cplusplus
   extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSFreeContext
(
   NWDSContextHandle context
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGetContext
(
   NWDSContextHandle context,
   nint              key,
   nptr              value
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSetContext
(
   NWDSContextHandle context,
   nint              key,
   nptr              value
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSCreateContextHandle
(
   NWDSContextHandle N_FAR *newHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSDuplicateContextHandle
(
   NWDSContextHandle       srcContextHandle,
   NWDSContextHandle N_FAR *destContextHandle
);

#ifdef __cplusplus
   }
#endif

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_dsdc.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"

#endif   /* NWDSDC_H */
