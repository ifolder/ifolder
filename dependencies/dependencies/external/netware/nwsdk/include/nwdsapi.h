/******************************************************************************
  Source module name:  
  Release Version:     

  %name: nwdsapi.h %
  %version: 8 %
  %date_modified: Mon Jan 26 17:53:18 1998 %
  $Copyright:

  Copyright (c) 1989-1996 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#ifndef   __NWDSAPI_H__
#define   __NWDSAPI_H__
#if defined( N_PLAT_NLM )

/*
 ===============================================================================
 = WARNING: This header is obsolete and is only for backward compatibility 
 = with the legacy DSAPI.NLM.  The equivalent functionality is available
 = in NWNet.h.  The NWNet.h header should be used.
 ===============================================================================
*/

#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
#if ! defined ( _NWFATTR_H_ )
#include <nwtypes.h>
#include <nwfattr.h>
#endif
#undef   FA_NORMAL
#undef   FA_HIDDEN
#undef   FA_SYSTEM
#endif


#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif


/* nwalias.h defines NWCONN_TYPE, NWSTATUS, NWOBJ_TYPE for us */
#if ! defined ( NWALIAS_H )
# include "nwalias.h"
#endif



#ifndef USE_NW_WILD_MATCH
#define USE_NW_WILD_MATCH   0
#endif

#ifndef USE_DOS_WILD_MATCH
#define USE_DOS_WILD_MATCH  1
#endif

/* Scope specifiers */
#define GLOBAL       0
#define PRIVATE      1
#define MY_SESSION   2
#define ALL_SESSIONS 3


/* nwdstype defines NWDSCODE for us */
#if ! defined ( NWDSTYPE_H )
# include "nwdstype.h"
#endif

#ifndef NWCONN_ID
# define NWCONN_ID  unsigned int
#endif
										 
#ifndef NWCONN_NUM_BYTE
# define NWCONN_NUM_BYTE    unsigned char 
#endif

#ifndef   NWDSDEFS_H
# include <NWDSDefs.h>
typedef enum EMAIL_ADDRESS_TYPE 
{
	SMF70 = 1, SMF71, SMTP, X400, SNADS, PROFS
} EMAIL_ADDRESS_TYPE;

#define DS_TYPES_REQUIRED                       0x0010
#endif

#ifndef   __NWDSERR_H
# include <NWDSErr.h>      /* Not present in xplat */
#endif

#ifndef   NWDSNAME_H
# include <NWDSName.h>
#endif

#ifndef   NWDSFILT_H
# include <NWDSFilt.h>
#endif

#ifndef   NWDSMISC_H
# include <NWDSMisc.h>
#endif

#ifndef   NWDSACL_H
# include <NWDSACL.h>
#endif

#ifndef   NWDSAUD_H
# include <NWDSAud.h>
#endif

#ifndef   NWDSDSA_H
# include <NWDSDSA.h>
#endif

#ifndef   NWDSSCH_H
# include <NWDSSch.h>
#endif

#ifndef   NWDSATTR_H
# include <NWDSAttr.h>
#endif

#ifndef   NWDSASA_H
# include <NWDSASA.h>
# define        GENERATE_CERTIFICATION_KP_F                     1
#endif

#ifndef   NWDSPART_H
# include <NWDSPart.h>
#endif

#ifndef   NWDSBUFT_H
# include <NWDSBufT.h>
#endif

#ifndef   NWDSNMTP_H
# include <NWDSNMTP.h>
#endif

#ifndef   NUNICODE_H
# include <nunicode.h>
#define DONT_USE_NOMAP_CHAR     0L   /* for 'noMapFlag' in NWLocalToUnicode() */
#define USE_NOMAP_CHAR          1L   /* and NWUnicodeToLocal() */
#endif


#ifdef __cplusplus
extern "C" {
#endif

N_GLOBAL_LIBRARY( NWCCODE )
NWDSLoginAsServer
(
	NWDSContextHandle       context
);

#ifdef __cplusplus
}
#endif

/*==============================================================================
** NLM-specific error codes which may be returned from Directory Services calls.
** For principal Directory Services error codes, see file NWDSErr.h.
**==============================================================================
*/
#define ERR_BAD_SERVICE_CONNECTION  -400
#define ERR_BAD_NETWORK             -401
#define ERR_BAD_ADDRESS             -402
#define ERR_SLOT_ALLOCATION         -403
#define ERR_BAD_BROADCAST           -404
#define ERR_BAD_SERVER_NAME         -405
#define ERR_BAD_USER_NAME           -406
#define ERR_NO_MEMORY               -408

#define ERR_BAD_SOCKET              -410
#define ERR_TAG_ALLOCATION          -411
#define ERR_CONNECTION_ABORTED      -412
#define ERR_TIMEOUT                 -413
#define ERR_CHECKSUM                -414
#define ERR_NO_FRAGMENT_LIST        -415

#endif
#endif

