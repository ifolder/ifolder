/******************************************************************************

  %name: nwdspart.h %
  %version: 8 %
  %date_modified: Tue Aug 29 18:11:41 2000 %
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
#if ! defined ( NWDSPART_H )
#define NWDSPART_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWDSBUFT_H )
#include "nwdsbuft.h"
#endif

#if ! defined ( NWDSDC_H )
#include "nwdsdc.h"
#endif

#include "npackon.h"

/*---------------------------------------------------------------------------
 * flags which specify partition info output of
 * DSV_LIST_PARTITIONS
 */
#define DSP_OUTPUT_FIELDS               0x00000001L
#define DSP_PARTITION_ID                0x00000002L
#define DSP_REPLICA_STATE               0x00000004L
#define DSP_MODIFICATION_TIMESTAMP      0x00000008L
#define DSP_PURGE_TIME                  0x00000010L
#define DSP_LOCAL_PARTITION_ID          0x00000020L
#define DSP_PARTITION_DN                0x00000040L
#define DSP_REPLICA_TYPE                0x00000080L
#define DSP_PARTITION_BUSY              0x00000100L

#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAddReplica
(
   NWDSContextHandle context,
   pnstr8            server,
   pnstr8            partitionRoot,
   nuint32           replicaType
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSChangeReplicaType
(
   NWDSContextHandle context,
   pnstr8            replicaName,
   pnstr8            server,
   nuint32           newReplicaType
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSJoinPartitions
(
   NWDSContextHandle context,
   pnstr8            subordinatePartition,
   nflag32           flags
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSListPartitions
(
   NWDSContextHandle context,
   pnint32           iterationHandle,
   pnstr8            server,
   pBuf_T            partitions
);

N_GLOBAL_LIBRARY (NWDSCCODE)
NWDSListPartitionsExtInfo
(
   NWDSContextHandle context,
   pnint32           iterationHandle,
   pnstr8            server,
   nflag32           DSPFlags,
   pBuf_T            partitions
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemovePartition
(
   NWDSContextHandle context,
   pnstr8            partitionRoot
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSRemoveReplica
(
   NWDSContextHandle context,
   pnstr8            server,
   pnstr8            partitionRoot
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSplitPartition
(
   NWDSContextHandle context,
   pnstr8            subordinatePartition,
   nflag32           flags
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPartitionReceiveAllUpdates
(
   NWDSContextHandle context,
   pnstr8            partitionRoot,
   pnstr8            serverName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSPartitionSendAllUpdates
(
   NWDSContextHandle context,
   pnstr8            partitionRoot,
   pnstr8            serverName
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSSyncPartition
(
   NWDSContextHandle context,
   pnstr8            server,
   pnstr8            partition,
   nuint32           seconds
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAbortPartitionOperation
(
   NWDSContextHandle context,
   pnstr8            partitionRoot
);


#ifdef __cplusplus
}
#endif


   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_dspart.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif  /* NWDSPART_H */
