/******************************************************************************

  %name: o_dspart.h %
  %version: 4 %
  %date_modified: Tue Aug 29 18:12:29 2000 %
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
 * Include "nwdspart.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */


#ifndef _OBSOLETE_NWDSPART_H
#define _OBSOLETE_NWDSPART_H

#ifdef __cplusplus
extern "C" {
#endif

      /* replacements - NWDSAddReplica and NWDSSplitPartition
      */
N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAddPartition
(
   NWDSContextHandle context,
   pnstr8            server,
   pnstr8            partitionRoot,
   pnint32           iterationHandle,
   nbool8            more,
   pBuf_T            objectInfo
);

#ifdef __cplusplus
}
#endif

#endif   /* _OBSOLETE_NWDSPART_H */
