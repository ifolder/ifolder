/******************************************************************************

  %name: o_direct.h %
  %version: 2 %
  %date_modified: Tue Aug 29 18:38:24 2000 %
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
 * Include "nwdirect.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */

#ifndef _OBSOLETE_NWDIRECT_H
#define _OBSOLETE_NWDIRECT_H

#ifdef __cplusplus
extern "C" {
#endif

#define NWScanDirectoryInformation2(a, b, c, d, e, f, g, h) \
        NWIntScanDirectoryInformation2(a, b, c, d, e, f, g, h, 0)


N_EXTERN_LIBRARY( NWCCODE )
NWSaveDirectoryHandle
(
   NWCONN_HANDLE  conn,
   NWDIR_HANDLE   dirHandle,
   pnstr8         saveBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWRestoreDirectoryHandle
(
   NWCONN_HANDLE        conn,
   const nstr8  N_FAR * saveBuffer,
   NWDIR_HANDLE N_FAR * newDirHandle,
   pnuint8              rightsMask
);

#ifdef __cplusplus
}
#endif

#endif   /* _OBSOLETE_NWDIRECT_H */
