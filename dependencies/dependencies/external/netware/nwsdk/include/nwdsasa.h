/******************************************************************************

  %name: nwdsasa.h %
  %version: 10 %
  %date_modified: Mon Nov  1 13:40:27 1999 %
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
#if ! defined ( NWDSASA_H )
#define NWDSASA_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWDSDC_H )
#include "nwdsdc.h"     /* for NWDSContextHandle typedef */
#endif

#include "npackon.h"

#define SESSION_KEY_SIZE   16
typedef nuint8 NWDS_Session_Key_T[SESSION_KEY_SIZE];  /* Optional session key */
typedef NWDS_Session_Key_T N_FAR *  pNWDS_Session_Key_T;

#ifdef __cplusplus
extern "C" {
#endif

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAuthenticateConn
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSAuthenticateConnEx
(
   NWDSContextHandle context,
   NWCONN_HANDLE     connHandle
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSChangeObjectPassword
(
   NWDSContextHandle context,
   nflag32           pwdOption,
   pnstr8            objectName,
   pnstr8            oldPassword,
   pnstr8            newPassword
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGenerateObjectKeyPair
(
   NWDSContextHandle contextHandle,
   pnstr8            objectName,
   pnstr8            objectPassword,
   nflag32           pwdOption
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGenerateObjectKeyPair2
(
   NWDSContextHandle  context,
   pnstr8             objectName,
   nuint32            pseudoID,
   nuint32            pwdLen,
   pnstr8             pwdHash,
   nuint32            optionsFlag
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSLogin
(
   NWDSContextHandle context,
   nflag32           optionsFlag,
   pnstr8            objectName,
   pnstr8            password,
   nuint32           validityPeriod
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSLogout
(
   NWDSContextHandle context
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSVerifyObjectPassword
(
   NWDSContextHandle context,
   nflag32           optionsFlag,
   pnstr8            objectName,
   pnstr8            password
);


   /* The following APIs support extended and international characters in
    * passwords - see nwdsdefs.h for a list of supported password 
    * formats
   */
N_EXTERN_LIBRARY (NWDSCCODE)
NWDSGenerateKeyPairEx
(
   NWDSContextHandle context,
   pnstr8            objectName,
   nuint32           pwdFormat,
   nptr              pwd,
   nuint32           pwdOption
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSChangePwdEx
(
   NWDSContextHandle context,
   pnstr8            objectName,
   nuint32           pwdFormat,
   nptr              oldPwd,
   nptr              newPwd,
   nuint32           pwdOption
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSLoginEx
(
   NWDSContextHandle context,
   pnstr8            objectName,
   nuint32           pwdFormat,
   nptr              pwd
);

N_EXTERN_LIBRARY (NWDSCCODE)
NWDSVerifyPwdEx
(
   NWDSContextHandle context,
   pnstr8            objectName,
   nuint32           pwdFormat,
   nptr              pwd
);


#if defined( N_PLAT_NLM )
N_GLOBAL_LIBRARY( NWCCODE )
NWDSLoginAsServer
(
	NWDSContextHandle       context
);
#endif

#ifdef __cplusplus
}
#endif


   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_dsasa.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */

#include "npackoff.h"
#endif
