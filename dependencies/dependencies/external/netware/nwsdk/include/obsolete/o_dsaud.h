/******************************************************************************

  %name: o_dsaud.h %
  %version: 1 %
  %date_modified: %
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
 * Include "nwdsaud.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */

#ifndef _OBSOLETE_NWDSAUD_H
#define _OBSOLETE_NWDSAUD_H

#ifdef __cplusplus
   extern "C" {
#endif

   /* Replacement API - NWDSResolveName
   */         
N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAuditGetObjectID
(
   NWDSContextHandle    context,
   pnstr8               objectName,
   NWCONN_HANDLE  N_FAR *conn,
   pnuint32             objectID
);


#ifdef __cplusplus
   }
#endif

#endif   /* _OBSOLETE_NWDSAUD_H */
