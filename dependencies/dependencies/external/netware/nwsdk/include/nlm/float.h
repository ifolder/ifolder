#ifndef _FLOAT_H_
#define _FLOAT_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  float.h
==============================================================================
*/

#define __FLT                    0
#define __DBL                    1
#define __LDBL                   (__DBL + __LDBL_IS_EXTENDED)
#define __LDBL_IS_EXTENDED       (sizeof(long double) != sizeof(double))

#if defined(__cplusplus)
# if defined(__MWERKS__) || defined(__ECPP__) || defined(_MSC_VER)
#  define __LDBL_(x)				(DBL_##x)
# elif defined(__BORLANDC__)
#  define __LDBL_(x)				(EXT_##x)
# endif
#endif
#ifndef __LDBL_
# define __LDBL_(x)				(__fp_characteristics[__LDBL].__##x)
#endif

#ifndef __FP_MIN_IS_DENORMAL
# define __FP_MIN_IS_DENORMAL     0
#endif

#ifndef __FP_EPSILON_IS_DENORMAL
# define __FP_EPSILON_IS_DENORMAL 0
#endif

#ifndef __FP_MAX_IS_INFINITY
# define __FP_MAX_IS_INFINITY     0
#endif


#define FLT_RADIX       2
#define FLT_ROUNDS      1  /* round to nearest */

/* number of base-FLT_RADIX digits in the floating point mantissa */
#define FLT_MANT_DIG    24
#define DBL_MANT_DIG    53
#define EXT_MANT_DIG    64
#define LDBL_MANT_DIG   __LDBL_(MANT_DIG)

/* number of decimal digits of precision */
#define FLT_DIG         6
#define DBL_DIG         15
#define EXT_DIG         18
#define LDBL_DIG        __LDBL_(DIG)

/* minimum negative integer such that FLT_RADIX raised to that power minus 1
   is a normalized floating point number */
#define FLT_MIN_EXP     (-125)
#define DBL_MIN_EXP     (-1021)
#define EXT_MIN_EXP     (-16381)
#define LDBL_MIN_EXP    __LDBL_(MIN_EXP)

/* minimum negative integer such that 10 raised to that power is in the
   range of normalized floating point numbers */
#define FLT_MIN_10_EXP  (-37)
#define DBL_MIN_10_EXP  (-307)
#define EXT_MIN_10_EXP  (-4931)
#define LDBL_MIN_10_EXP __LDBL_(MIN_10_EXP)

/* maximum integer such that FLT_RADIX raised to that power minus 1 is a
   representable floating point number */
#define FLT_MAX_EXP     (+128)
#define DBL_MAX_EXP     (+1024)
#define EXT_MAX_EXP     (+16384)
#define LDBL_MAX_EXP    __LDBL_(MAX_EXP)

/* maximum integer such that 10 raised to that power is in the range of
   representable floating point numbers */
#define FLT_MAX_10_EXP  (+38)
#define DBL_MAX_10_EXP  (+308)
#define EXT_MAX_10_EXP  (+4932)
#define LDBL_MAX_10_EXP __LDBL_(MAX_10_EXP)

/* maximum representable floating point number */
#define FLT_MAX         \
   (__fp_characteristics[__FLT].__MAX[__FP_MAX_IS_INFINITY].__f)
#define DBL_MAX         \
   (__fp_characteristics[__DBL].__MAX[__FP_MAX_IS_INFINITY].__d)
#define LDBL_MAX        \
   (__fp_characteristics[__LDBL].__MAX[__FP_MAX_IS_INFINITY].__ld)

/* minimum positive floating point number x such that 1.0 + x != 1.0 */
#define FLT_EPSILON     \
   (__fp_characteristics[__FLT].__EPSILON[__FP_EPSILON_IS_DENORMAL].__f)
#define DBL_EPSILON     \
   (__fp_characteristics[__DBL].__EPSILON[__FP_EPSILON_IS_DENORMAL].__d)
#define LDBL_EPSILON    \
   (__fp_characteristics[__LDBL].__EPSILON[__FP_EPSILON_IS_DENORMAL].__ld)

/* minimum representable positive floating point number */
#define FLT_MIN         \
   (__fp_characteristics[__FLT].__MIN[__FP_MIN_IS_DENORMAL].__f)
#define DBL_MIN         \
   (__fp_characteristics[__DBL].__MIN[__FP_MIN_IS_DENORMAL].__d)
#define LDBL_MIN        \
   (__fp_characteristics[__LDBL].__MIN[__FP_MIN_IS_DENORMAL].__ld)

typedef union __fp_u
{
   unsigned char __uc[16]; /* establish maximum size */
   float         __f;      /* establish alignment    */
   double        __d;      /* establish alignment    */
   long double   __ld;     /* establish alignment    */
} __fp_u;

typedef struct __fp_s
{
   int    __MANT_DIG;
   int    __DIG;
   int    __MIN_EXP;
   int    __MIN_10_EXP;
   int    __MAX_EXP;
   int    __MAX_10_EXP;
   __fp_u __EPSILON[2];
   __fp_u __MIN[2];
   __fp_u __MAX[2];
} __fp_s;

#ifdef __cplusplus
extern "C"
{
#endif

extern const __fp_s __fp_characteristics[3];

#ifdef __cplusplus
}
#endif

#endif
