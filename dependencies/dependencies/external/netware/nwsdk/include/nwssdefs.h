/******************************************************************************

  %name: nwssdefs.h %
  %version: 3 %
  %date_modified: Wed Dec 18 12:09:20 1996 %
  $Copyright:

  Copyright (c) 1996 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/
#ifndef  NWSSDEFS_H
#define  NWSSDEFS_H

#ifndef NTYPES_H
#include "ntypes.h"
#endif

#ifndef NWDSDEFS_H
#include "nwdsdefs.h"
#endif

#ifndef NWDSDC_H
#include "nwdsdc.h"
#endif

#ifndef NWDSDC_H
#include "nwdsdc.h"
#endif

#ifndef __NWDSERR_H
#include "nwdserr.h"
#endif



/* ##################### DECLARATIONS START HERE ######################### */

#define  NWSS_GLOBAL_RCODE    N_EXTERN_LIBRARY(NWRCODE)

/* ############################## ERROR CODES ############################ */

#define     NWSS_SUCCESS                     0x00000000     /*    0 */
#define     NWSS_ERR_BAD_HANDLE              0xFFFFFCE0     /* -800 */
#define     NWSS_ERR_ENCODING_ASN1ID         0xFFFFFCDF     /* -801 */
#define     NWSS_ERR_INVALID_APP             0xFFFFFCDE     /* -802 */
#define     NWSS_ERR_APP_EXISTS              0xFFFFFCDD     /* -803 */
#define     NWSS_ERR_ACCESS_DENIED           0xFFFFFCDC     /* -804 */
#define     NWSS_ERR_OBJ_READ_FAILED         0xFFFFFCDB     /* -805 */
#define     NWSS_ERR_KEYCHAIN_EXISTS         0xFFFFFCDA     /* -806 */
#define     NWSS_ERR_CMC_FAILURE             0xFFFFFCD9     /* -807 */
#define     NWSS_ERR_INVALID_KEYCHAIN        0xFFFFFCD8     /* -808 */
#define     NWSS_ERR_SHORT_ASN1ID            0xFFFFFCD7     /* -809 */
#define     NWSS_ERR_ID_EXISTS               0xFFFFFCD6     /* -810 */
#define     NWSS_ERR_INVALID_USER_ID         0xFFFFFCD5     /* -811 */
#define     NWSS_ERR_HANDLE_ALLOC_FAILED     0xFFFFFCD4     /* -812 */
#define     NWSS_ERR_NOT_LOGGED_IN           0xFFFFFCD3     /* -813 */
#define     NWSS_ERR_ALREADY_REVOKED         0xFFFFFCD2     /* -814 */
#define     NWSS_ERR_SECRET_ALREADY_EXIST    0xFFFFFCD1     /* -815 */
#define     NWSS_ERR_ILLEGAL_PASSWORD        0xFFFFFCD0     /* -816 */
#define     NWSS_ERR_WRONG_VERSION           0xFFFFFCCF     /* -817 */
#define     NWSS_ERR_POLICY_RES_FAILED       0xFFFFFCCE     /* -818 */
#define     NWSS_ERR_TTS_DISABLED            0xFFFFFCCD     /* -819 */
#define     NWSS_ERR_NO_NDS_CONTEXT          0xFFFFFCCC     /* -820 */
#define     NWSS_ERR_KC_SCHEMA_EXT_FAILED    0xFFFFFCCB     /* -821 */
#define     NWSS_ERR_APP_SCHEMA_EXT_FAILED   0xFFFFFCCA     /* -822 */
#define     NWSS_ERR_MEM_ALLOC_FAILED        0xFFFFFCC9     /* -823 */
#define     NWSS_ERR_OBJ_CREATION_FAILED     0xFFFFFCC8     /* -824 */
#define     NWSS_ERR_BAD_POINTER             0xFFFFFCC7     /* -825 */
#define     NWSS_ERR_OBJ_MODIFY_FAILED       0xFFFFFCC6     /* -826 */
#define     NWSS_ERR_CANT_REMOVE_KC          0xFFFFFCC5     /* -827 */
#define     NWSS_ERR_ASN1ID_NOT_EQUAL        0xFFFFFCC4     /* -828 */

/* ########################### FLAG DEFINITIONS ######################### */

/* application flags */
#define  SS_APP_ENABLED_C     0x0001

/* Secret Store Application flags */
#define  SS_MODIFYENABLED_C   0x0001

/* ########################### STRUCTURED DEFINITIONS ######################### */


/* String and Data type */
typedef  struct   _ss_string_type
{
   nuint16              length;
   nuint8               *dataByte;
} SS_Data_T;



/* Distinguished name type */
typedef  nuint8            SS_DN_T;

/* Case Ignore String */
typedef  SS_Data_T         SS_String_T;

/* Application User ID */
typedef  SS_Data_T         SS_AppUID_T;

/* Application Secrets Contents */
typedef  SS_Data_T         SS_AppSC_T;

/* ASN1 ID */
typedef  SS_Data_T         SS_ASN1ID_T;



/* User Name List */
typedef  struct   _ss_app_uid_list
{
   nuint16                 userCount;
   SS_AppUID_T       N_FAR *userID[MAX_RDN_BYTES];
} SS_AppUIDList_T;


/* Keychain List */
typedef  struct   _ss_keychain_list
{
   SS_DN_T                 appName[MAX_DN_BYTES];
   nuint16                 kcDNCount;
   SS_DN_T           N_FAR *kcDN[MAX_DN_BYTES];
} SS_KeychainList_T;


/* ########################## FUNCTION PROTOTYPES ######################### */


/******************************************************************
 ************************** ENABLING APIS *************************
 ******************************************************************/

NWSS_GLOBAL_RCODE NWSSOpenSecretStore
(
   nuint32           N_FAR *ssHandle,
   SS_DN_T           N_FAR *owner,
   SS_ASN1ID_T       N_FAR *appID,
   SS_KeychainList_T N_FAR *kcDN,
   SS_AppUIDList_T   N_FAR *appUID
);


NWSS_GLOBAL_RCODE NWSSCloseSecretStore
(
   nuint32                 ssHandle
);


NWSS_GLOBAL_RCODE NWSSAppUserLogin
(
   nuint32                 ssHandle,
   nuint16           N_FAR *flags,
   SS_DN_T           N_FAR *appDN,
   SS_DN_T           N_FAR *kcDN,
   SS_AppUID_T       N_FAR *userID,
   SS_AppSC_T        N_FAR *appSecrets
);



NWSS_GLOBAL_RCODE NWSSSaveAppSecretsOnKeychain
(
   nuint32                 ssHandle,
   SS_DN_T           N_FAR *appDN,
   SS_DN_T           N_FAR *kcDN,
   SS_AppUID_T       N_FAR *userID,
   SS_AppSC_T        N_FAR *appSecrets
);


NWSS_GLOBAL_RCODE NWSSRevokeKeychain
(
   nuint32                 ssHandle,
   SS_DN_T           N_FAR *kcDN
);


NWSS_GLOBAL_RCODE NWSSEncodeAppASN1ID
(
   nuint16           N_FAR *oid,
   SS_ASN1ID_T       N_FAR *appID
);

NWSS_GLOBAL_RCODE NWSSSetDefaultAppUserID
(
   nuint32                 ssHandle,
   SS_DN_T           N_FAR *owner,
   SS_KeychainList_T N_FAR *kcDN,
   SS_AppUIDList_T   N_FAR *userID
);
/* ########################## CODE ENDS HERE ##################### */

#endif /* NWSSDEFS_H */
