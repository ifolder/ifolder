/******************************************************************************

  %name: o_del.h %
  %version: 1 %
  %date_modified: Tue Aug 29 18:09:36 2000 %
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
 * Include "nwdel.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */


#ifndef _OBSOLETE_NWDEL_H
#define _OBSOLETE_NWDEL_H


#ifdef __cplusplus
extern "C" {
#endif


N_EXTERN_LIBRARY( NWCCODE )
NWPurgeErasedFiles
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWRestoreErasedFile
(
   NWCONN_HANDLE       conn,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * dirPath,
   pnstr8              oldName,
   pnstr8              newName
);

#ifdef __cplusplus
}
#endif

#endif  /* _OBSOLETE_NWDEL_H */
