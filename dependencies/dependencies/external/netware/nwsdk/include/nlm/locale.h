#ifndef _LOCALE_H_
#define _LOCALE_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  locale.h
==============================================================================
*/

#ifndef NULL
# define NULL 0
#endif

/* locale categories... */
#define LC_CTYPE     0
#define LC_NUMERIC   1
#define LC_TIME      2
#define LC_COLLATE   3
#define LC_MONETARY  4
#define LC_MESSAGES  5        /* new starting in NetWare v4.11 */
#define LC_ALL       6

#ifndef _STRUCT_LCONV
# define _STRUCT_LCONV
struct lconv                  /* for the current locale...                */
{  /* ---------------- Numeric (Nonmonetary) Conventions ---------------- */
   char decimal_point[4];     /* decimal point                            */
   char thousands_sep[4];     /* separator for digits left of decimal     */
   char grouping[4];          /* digit grouping size                      */
   /* ----------------------- Monetary Conventions ---------------------- */
   char int_curr_symbol[8];   /* international currency symbol, separator */
   char currency_symbol[4];   /* currency symbol                          */
   char mon_decimal_point[4]; /* decimal point                            */
   char mon_thousands_sep[4]; /* separator for digits left of decimal     */
   char mon_grouping[8];      /* digit grouping size                      */
   char positive_sign[4];     /* string indicating positive quantities    */
   char negative_sign[4];     /* string indicating negative quantities    */
   char int_frac_digits;      /* (international) digits right of decimal  */
   char frac_digits;          /* count of digits right of decimal         */
                              /* for positive monetary quantities:        */
   char p_cs_precedes;        /* currency symbol precedes quantity        */
   char p_sep_by_space;       /* currency symbol separated by blank       */
                              /* for negative monetary quantities:        */
   char n_cs_precedes;        /* currency symbol precedes quantity        */
   char n_sep_by_space;       /* currency symbol separated by blank       */
                              /* for positive monetary quantities:        */
   char p_sign_posn;          /* position of positive symbol              */
                              /* for negative monetary quantities:        */
   char n_sign_posn;          /* position of negative symbol              */
};
#endif

#ifdef __cplusplus
extern "C"
{
#endif

struct lconv  *localeconv( void );
char          *setlocale( int, const char * );
char          *setlocale_411( int, const char * );

#ifdef __cplusplus
}

inline char*setlocale(int c, const char*l) {return setlocale_411(c, l);}

#else

/* macros... */
#define setlocale(c, l) setlocale_411(c, l)

#endif

#endif
