#ifndef _MATH_H_
#define _MATH_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  math.h
==============================================================================
*/

/* constants for type exception using matherr() */
#define DOMAIN    1 /* argument domain error */
#define SING      2 /* argument singularity */
#define OVERFLOW  3 /* overflow range error */
#define UNDERFLOW 4 /* underflow range error */
#define TLOSS     5 /* total loss of significance */
#define PLOSS     6 /* partial loss of significance */

#ifdef __cplusplus
#define __FP_EXCEPTION __fp_exception
#define __COMPLEX __complex
#else
#define __FP_EXCEPTION exception
#define __COMPLEX complex
#endif

/* for C++, __fp_exception; for C, exception */
struct __FP_EXCEPTION
{
   int    type;
   char   *name;
   double arg1;
   double arg2;
   double retval;
};

/* for C++, __complex; for C, complex */
struct __COMPLEX
{
   double real;
   double imag;
};

#ifdef __cplusplus
extern "C"
{
#endif

extern /*const*/ double gHugeValue;

#if defined(__ECPP__) || defined(__ECC__)
# define acos        __EPC_acos
# define asin        __EPC_asin
# define atan        __EPC_atan
# define atan2       __EPC_atan2
# define ceil        __EPC_ceil
# define cos         __EPC_cos
# define cosh        __EPC_cosh
# define exp         __EPC_exp
# define fabs        __EPC_fabs
# define floor       __EPC_floor
# define fmod        __EPC_fmod
# define frexp       __EPC_frexp
# define ldexp       __EPC_ldexp
# define log         __EPC_log
# define log10       __EPC_log10
# define modf        __EPC_modf
# define pow         __EPC_pow
# define remainder   __EPC_remainder
# define sin         __EPC_sin
# define sinh        __EPC_sinh
# define sqrt        __EPC_sqrt
# define tan         __EPC_tan
# define tanh        __EPC_tanh

# define cabs        __EPC_cabs
# define hypot       __EPC_hypot
# define j0          __EPC_j0
# define j1          __EPC_j1
# define jn          __EPC_jn
# define y0          __EPC_y0
# define y1          __EPC_y1
# define yn          __EPC_yn
#endif

/* ISO/ANSI C library functions... */
double   acos( double );
double   asin( double );
double   atan( double );
double   atan2( double, double );
double   ceil( double );
double   cos( double );
double   cosh( double );
double   exp( double );
double   fabs( double );
double   floor( double );
double   fmod( double, double );
double   frexp( double, int * );
double   ldexp( double, int );
double   log( double );
double   log10( double );
double   modf( double, double * );
double   pow( double, double );
double   sin( double );
double   sinh( double );
double   sqrt( double );
double   tan( double );
double   tanh( double );

/* non-ANSI functions... */
double   cabs( struct __COMPLEX ); 
double   hypot( double, double ); 
double   j0( double ); 
double   j1( double ); 
double   jn( int, double ); 
double   remainder( double, double ); 
double   y0( double ); 
double   y1( double ); 
double   yn( int, double );

int      matherr( struct __FP_EXCEPTION * ); 
int      RegisterMatherrHandler( int (*)(struct __FP_EXCEPTION *) ); 

#ifdef __cplusplus
}
#endif

#define HUGE_VAL  (+gHugeValue)

#undef __FP_EXCEPTION
#undef __COMPLEX

#endif
