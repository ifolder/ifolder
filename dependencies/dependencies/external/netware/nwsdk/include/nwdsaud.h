/******************************************************************************

  %name: nwdsaud.h %
  %version: 4 %
  %date_modified: Wed Dec 18 12:06:58 1996 %
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
#if ! defined ( NWDSAUD_H )
#define NWDSAUD_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWDSTYPE_H )
#include "nwdstype.h"
#endif

#if ! defined ( NWDSDC_H )
#include "nwdsdc.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#include "npackon.h"

   /* The NLM LibC x-plat libraries do not support obsolete apis
   */
#include <stddef.h>
#if !defined(__NOVELL_LIBC__)
   #ifdef INCLUDE_OBSOLETE
      #include "obsolete/o_dsaud.h"
   #endif
#endif /* !defined(__NOVELL_LIBC__) */


#include "npackoff.h"
#endif   /* NWDSAUD_H */
