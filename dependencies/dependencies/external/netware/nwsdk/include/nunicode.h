/******************************************************************************

  %name:          nunicode.h %
  %version:       30 %
  %date_modified: Mon May  1 10:11:11 2000 %
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
#if !defined(NUNICODE_H)
#define NUNICODE_H


#include <stddef.h>

   /* For LIBC builds the XPlat libraries use the LIBC unicode and 
    * localization support.  LIBC is the next generation of the c-runtime
    * on NetWare.  All other platforms will continue to use unicode.h
    * NOTE:  stddef.h in the LIBC sdk defines __NOVELL_LIBC__ 
    *  
   */
#if defined(__NOVELL_LIBC__)
   
   #include "unilib.h"

      /* unilib.h doesn't define the following with are used significantly
       * in the XPlat SDK.  Define them for the XPlat SDK.
      */

      /* NOTE:  LibC WinSock2 #defines "unicode" inside ws2defs.h.  
       * If LibC WinSock2 headers have been included, undefine unicode
       * and typdef it the way XPlat SDK expects it.
      */
   #if defined(unicode) 
      #undef unicode
   #endif
   
   #ifndef UNICODE_TYPE_DEFINED
   #define UNICODE_TYPE_DEFINED
   typedef unicode_t    unicode;  /* use LibC's unicode_t type */  
   #endif 

   typedef unicode *  punicode;
   typedef unicode ** ppunicode;

#else /* All non-LibC builds */
   
   #include "unicode.h"

#endif /* #if defined(__NOVELL_LIBC__) */ 
					 

#endif /* #if !defined(NUNICODE_H) */
