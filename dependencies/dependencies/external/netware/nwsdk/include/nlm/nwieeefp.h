#ifndef _NWIEEEFP_H_
#define _NWIEEEFP_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwieeefp.h
==============================================================================
*/

#define fp_except int
#define FP_X_INV  0x01 /* invalid operation exception   */
#define FP_X_DNML 0x02 /* denormal operation exception  */
#define FP_X_DZ   0x04 /* divide by zero exception      */
#define FP_X_OFL  0x08 /* overflow exception            */
#define FP_X_UFL  0x10 /* underflow exception           */
#define FP_X_IMP  0x20 /* inexact (precision) exception */

typedef enum fp_rnd
{
   FP_RN = 0,  /* round to nearest representable number, tie -> even */
   FP_RM = 1,  /* round toward minus infinity                        */
   FP_RP = 2,  /* round toward plus infinity                         */
   FP_RZ = 3   /* round toward zero (truncate)                       */
} fp_rnd;


#ifdef __cplusplus
extern "C"
{
#endif

extern fp_except fpgetmask ( void ); 
extern fp_rnd fpgetround ( void );
extern fp_except fpgetsticky ( void ); 
extern fp_except fpsetmask ( fp_except newmask ); 
extern fp_rnd fpsetround ( fp_rnd newround );
extern fp_except fpsetsticky ( fp_except newsticky );

#ifdef __cplusplus
}
#endif

#endif
