
/*--------------------------------------------------------------------------

   %name: nwlocale.h %
   %version: 10 %
   %date_modified: Fri Oct 15 14:17:48 1999 %
   $Copyright:

   Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.

   THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
   TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
   COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
   EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
   WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
   OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
   CRIMINAL AND CIVIL LIABILITY.$

--------------------------------------------------------------------------*/


#if !defined( NWLOCALE_H )
#define NWLOCALE_H


/* make sure size_t is defined */
#include <stddef.h>

/* make sure va_list is defined */
#include <stdarg.h>

#ifndef NTYPES_H
#  include "ntypes.h"
#endif

#include "npackon.h"


#if defined N_PLAT_DOS
#  define NWLCODE           nuint
#endif

#define NUMBER_TYPE  nint32

/* (in imitation of stdlib.h) */

#define L_MB_LEN_MAX       2   /* (in imitation of limits.h) */
#define MERIDLEN           5
#define NWSINGLE_BYTE      1
#define NWDOUBLE_BYTE      2

#ifndef NLC_ALL
#  define NLC_ALL            0
#endif
#ifndef LC_ALL
#  define LC_ALL NLC_ALL
#endif

#ifndef NLC_COLLATE
#  define NLC_COLLATE        1
#endif
#ifndef LC_COLLATE
#  define LC_COLLATE NLC_COLLATE
#endif

#ifndef NLC_CTYPE
#  define NLC_CTYPE          2
#endif
#ifndef LC_CTYPE
#  define LC_CTYPE NLC_CTYPE
#endif

#ifndef NLC_MONETARY
#  define NLC_MONETARY       3
#endif
#ifndef LC_MONETARY
#  define LC_MONETARY NLC_MONETARY
#endif

#ifndef NLC_NUMERIC
#  define NLC_NUMERIC        4
#endif
#ifndef LC_NUMERIC
#  define LC_NUMERIC NLC_NUMERIC
#endif

#ifndef NLC_TIME
#  define NLC_TIME           5
#endif
#ifndef LC_TIME
#  define LC_TIME NLC_TIME
#endif

#ifndef NLC_TOTAL
#  define NLC_TOTAL          6
#endif
#ifndef LC_TOTAL
#  define LC_TOTAL NLC_TOTAL
#endif


/* -------------------------------------------------------------------------
 *    country definitions
 * -------------------------------------------------------------------------
 */

#define ARABIC            785
#define WBAHRAIN          973
#define WCYPRUS           357 /* ??? */
#define WEGYPT             20
#define WETHIOPIA         251
#define WIRAN              98
#define WIRAQ             964
#define WJORDAN           962
#define WKUWAIT           965
#define WLIBYA            218
#define WMALTA            356 /* ??? */
#define WMOROCCO          212 /* SHOULD THIS BE FRENCH?? */
#define WPAKISTAN          92
#define WQATAR            974 /* ??? */
#define WSAUDI            966
#define WTANZANIA         255 /* ??? */
#define WTUNISIA          216 /* ??? */
#define WTURKEY            90 /* ??? */
#define WUAE              971
#define WYEMEN            967 /* ??? */
#define AUSTRALIA          61
#define BELGIUM            32
#define CANADA_FR           2
#define CANADA              2
#define DENMARK            45
#define FINLAND           358
#define FRANCE             33
#define GERMANY            49
#define GERMANYE           37
#define HEBREW            972
#define IRELAND           353
#define ITALY              39
#define LATIN_AMERICA       3
#define WARGENTINA         54
#define WBOLIVIA          591
#define WCHILE             56
#define WCOLOMBIA          57
#define WCOSTARICA        506
#define WECUADOR          593
#define WELSALVADOR       503
#define WGUATEMALA        502
#define WHONDURAS         504
#define WMEXICO            52
#define WNICARAGUA        505
#define WPANAMA           507
#define WPARAGUAY         595
#define WPERU              51
#define WURUGUAY          598
#define WVENEZUELA         58
#define NETHERLANDS        31
#define NORWAY             47
#define PORTUGAL          351
#define SPAIN              34
#define SWEDEN             46
#define SWITZERLAND        41
#define UK                 44
#define USA                 1
#define JAPAN              81
#define KOREA              82
#define PRC                86
#define TAIWAN            886
#define TAIWAN2            88
#define WTAIWAN           886
#define ASIAN_ENGLISH      99
#define NEWZEALAND         64


/* -------------------------------------------------------------------------
 *    typedef Llconv
 * -------------------------------------------------------------------------
 */

typedef struct Llconv
   {
   char decimal_point[4];     /* non-monetary decimal point */
   char thousands_sep[4];     /* non-monetary separator for digits
                                 left of the decimal-point */
   char grouping[4];          /* String indicating size of groups
                                 of digits*/
   /*
    The international currency symbol applicable to
    the current locale.  The first three characters
    contain the alphabetic international currency
    symbol in accordance with those specified in ISO
    4217 "codes for the representation of currency
    and funds." The fourth character is the character
    used to separate the international currency
    symbol from the monetary quantity.
   */
   char int_curr_symbol[8];
   char currency_symbol[4];   /* Currency symbol for current locale */
   char mon_decimal_point[4]; /* monetary decimal point */
   char mon_thousands_sep[4]; /* monetary separator for digits left
                                 of the decimal-point */
   char mon_grouping[8];      /* String indicating size of
                                 groups of digits */
   char positive_sign[4];     /* String indicating positive
                                 monetary value */
   char negative_sign[4];     /* String indicating negative
                                 monetary value */
   char int_frac_digits;      /* Num of fractional digits in
                                 monetary display */
   char frac_digits;          /* Num of fractional digits in
                                 non-monetary display*/
   char p_cs_precedes;        /* 1=precede or 0=succeeds
                                 (pos currency symbol)*/
   char p_sep_by_space;       /* 1=space separator or
                                 0=no space separator
                                 (currency symbol) */
   char n_cs_precedes;        /* location of currency_symbol
                                 for neg monetary quantity */
   char n_sep_by_space;       /* separation of currency symbol
                                 in neg monetary quantity */
   char p_sign_posn;          /* value indicating position of
                                 positive_sign for positive
                                 monetary quantity */
   char n_sign_posn;          /* value indicating position of
                                 negative_sign for negative
                                 monetary quantity.*/

   /* Novell Additions to the ANSI definition:*/
   nint         code_page;
   nint         country_id;
   char         data_list_separator[2];
   char         date_separator[2];
   char         time_separator[2];
   char         time_format;
   nint         date_format;
   char         am[MERIDLEN];
   char         pm[MERIDLEN];
   char         reserved[40];
} LCONV;


/* -------------------------------------------------------------------------
 *    function prototypes
 * -------------------------------------------------------------------------
 */

#if defined(__cplusplus)
extern "C" {
#endif

N_EXTERN_LIBRARY(LCONV N_FAR *) NWLlocaleconv(LCONV N_FAR *lconvPtr);

N_EXTERN_LIBRARY(nint) NWLmblen(const nstr N_FAR * string, size_t maxBytes);

N_EXTERN_LIBRARY(pnstr) NWLsetlocale(nint category, const nstr N_FAR *locale);

N_EXTERN_LIBRARY(pnstr) NWLstrchr(const nstr N_FAR * string, nint find);

N_EXTERN_LIBRARY(nint) NWLstricmp
(
   const nstr N_FAR *str1,
   const nstr N_FAR *str2
);

/* NWLstrcoll  (see below) */

N_EXTERN_LIBRARY(size_t) NWLstrcspn
(
   const nstr N_FAR *string1,
   const nstr N_FAR *string2
);

#if !defined NWL_EXCLUDE_TIME
N_EXTERN_LIBRARY(size_t) NWLstrftime
(
   pnstr dst,
   size_t max,
   const nstr N_FAR *fmt,
   const struct tm N_FAR *ptm
);
#endif

N_EXTERN_LIBRARY(pnstr) NWLstrpbrk(pnstr string1, const nstr N_FAR *string2);

N_EXTERN_LIBRARY(pnstr) NWLstrrchr(const nstr N_FAR * string, nint find);

N_EXTERN_LIBRARY(pnstr) NWLstrrev(pnstr string1, pnstr string2);

N_EXTERN_LIBRARY(size_t) NWLstrspn
(
   const nstr N_FAR *string1,
   const nstr N_FAR *string2
);

N_EXTERN_LIBRARY(pnstr) NWLstrstr(const nstr N_FAR * string, 
                                  const nstr N_FAR * searchString);

N_EXTERN_LIBRARY(pnstr) NWLstrtok(pnstr parse, const nstr N_FAR * delim);

N_EXTERN_LIBRARY(pnstr) NWLstrtok_r(pnstr              parse, 
                                    const nstr N_FAR * delim, 
                                    ppnstr             last);

/* NWLstrupr ( see below )*/

N_EXTERN_LIBRARY(pnstr) NWIncrement(const nstr N_FAR * string, 
                                    size_t numChars);

N_EXTERN_LIBRARY(pnstr) NWstrImoney(pnstr buffer, NUMBER_TYPE Value);

N_EXTERN_LIBRARY(pnstr) NWstrmoney(pnstr buffer, NUMBER_TYPE Value);

N_EXTERN_LIBRARY(nint) NWstrncoll(const nstr N_FAR * string1, 
                                  const nstr N_FAR * string2, 
                                  size_t             maxChars);

N_EXTERN_LIBRARY(pnstr) NWstrncpy(pnstr              target_string, 
                                  const nstr N_FAR * source_string, 
                                  nint               numChars);

N_EXTERN_LIBRARY(pnstr) NWLstrbcpy
(
   pnstr dest,
   const nstr N_FAR *src,
   size_t maxlen
);

N_EXTERN_LIBRARY(pnstr) NWstrnum(pnstr buffer, NUMBER_TYPE Value);

N_EXTERN_LIBRARY(nint) NWstrlen
(
   const nstr N_FAR *string
);

N_EXTERN_LIBRARY(size_t) NWLmbslen
(
   const nuint8 N_FAR *string
);

N_EXTERN_LIBRARY(nint) NWLTruncateString(pnchar8 pStr, nint  iMaxLen);

N_EXTERN_LIBRARY(nint) NWLInsertChar(pnstr              src, 
                                     const nstr N_FAR * insertableChar);

N_EXTERN_LIBRARY_C(nint)
NWprintf(const nstr N_FAR *format, ...);

#ifndef NWL_EXCLUDE_FILE
#  ifdef N_PLAT_DOS
N_EXTERN_LIBRARY_C(nint) NWfprintf
(
   FILE N_FAR *stream,
   const nstr N_FAR *format,
   ...
);
#  endif
#endif

#if defined N_PLAT_MSW && defined N_ARCH_32
#  if !defined(__BORLANDC__)
#     define NWsprintf _NWsprintf
#  endif
#elif defined N_PLAT_MSW  && defined N_ARCH_16
#  define NWsprintf NWSPRINTF
#endif
N_EXTERN_LIBRARY_C(nint) NWsprintf
(
   pnstr buffer,
   const nstr N_FAR *format,
   ...
);

/*
 * NWwsprintf has been set as obsolete. These prototypes and macros are
 * scheduled for removal by September 1999.
 */
#if defined N_PLAT_MSW && defined N_ARCH_32
#  if !defined(__BORLANDC__)
#     define NWwsprintf _NWwsprintf
#  endif
#elif defined N_PLAT_MSW && defined N_ARCH_16
#  define NWwsprintf NWWSPRINTF
#endif

N_EXTERN_LIBRARY_C(nint) NWwsprintf(pnstr buffer, pnstr format, ...);


/* Functions using variable parameter lists have the pointer to the */
/* variable list declared as void instead of va_list to enable the user to */
/* compile without including stdarg.h in every module. */

N_EXTERN_LIBRARY(nint)
NWvprintf(const nstr N_FAR *format, va_list arglist);

#ifndef NWL_EXCLUDE_FILE
#  ifdef N_PLAT_DOS
N_EXTERN_LIBRARY(nint) NWvfprintf
(
   FILE N_FAR *stream,
   const nstr N_FAR *format,
   va_list arglist
);
#  endif
#endif

N_EXTERN_LIBRARY(nint)
NWvsprintf(pnstr buffer, const nstr N_FAR *format, va_list arglist);

N_EXTERN_LIBRARY(nint) NWatoi(const nstr N_FAR * string);

N_EXTERN_LIBRARY(pnstr) NWitoa(nint value, pnstr string, nuint radix);
N_EXTERN_LIBRARY(pnstr) NWutoa(nuint value, pnstr string, nuint radix);
N_EXTERN_LIBRARY(pnstr) NWltoa(nint32 value, pnstr buf, nuint radix);
N_EXTERN_LIBRARY(pnstr) NWultoa(nuint32 value, pnstr buf, nuint radix);

N_EXTERN_LIBRARY(nint) NWisalpha(nuint ch);
N_EXTERN_LIBRARY(nint) NWisalnum(nuint ch);
N_EXTERN_LIBRARY(nint) NWisdigit(nuint ch);
N_EXTERN_LIBRARY(nint) NWisxdigit(nuint ch);

N_EXTERN_LIBRARY(void) NWGetNWLOCALEVersion(pnuint8 majorVersion,
                                            pnuint8 minorVersion,
                                            pnuint8 revisionLevel,
                                            pnuint8 betaReleaseLevel);

#if defined N_PLAT_DOS && !defined N_LOC_NO_OLD_FUNCS
N_EXTERN_LIBRARY(NWLCODE) NWGetShortMachineName(pnstr shortMachineName);
#endif

/* This call is not needed for Windows */
N_EXTERN_LIBRARY(nint) NWGetCollateTable(pnstr retCollateTable, size_t maxLen);

#if (defined N_PLAT_MSW && defined N_ARCH_16) && !defined N_LOC_NO_OLD_MACROS
#  define NWNextChar(s)         AnsiNext(s)
#  define NWPrevChar(t, s)      AnsiPrev(t, s)
#  define NWLstrupr(s)          AnsiUpper(s)
#  define NWLstrcoll(s1, s2)    lstrcmp(s1, s2)
#  define NWLstrxfrm(s1, s2, t) strxfrm(s1, s2, t)
#  define NWCharUpr(c)   (nint)(LOWORD((DWORD)AnsiUpper((LPSTR)(DWORD)c)))
#else
N_EXTERN_LIBRARY(pnstr) NWNextChar(const nstr N_FAR *string);
N_EXTERN_LIBRARY(pnstr) NWPrevChar(const nstr N_FAR *string, pnstr position);
N_EXTERN_LIBRARY(pnstr) NWLstrupr(pnstr string);
N_EXTERN_LIBRARY(nint) NWLstrcoll(const nstr N_FAR * string1, 
                                  const nstr N_FAR * string2);
N_EXTERN_LIBRARY(size_t) NWLstrxfrm(pnstr              string1, 
                                    const nstr N_FAR * string2, 
                                    size_t             numBytes);
N_EXTERN_LIBRARY(nint) NWCharUpr(nint chr);
#endif /* (N_PLAT_MSW && N_ARCH_16) && !N_LOC_NO_OLD_MACROS */

N_EXTERN_LIBRARY(pnstr) NWLstrlwr(pnstr string);
N_EXTERN_LIBRARY(nint) NWCharLwr(nint chr);
                                                             
N_EXTERN_LIBRARY(nint) NWCharType(nint ch);
N_EXTERN_LIBRARY(nint) NWCharVal(const nstr N_FAR *string);

N_EXTERN_LIBRARY(nint) NWLIsAnsi();
N_EXTERN_LIBRARY(void) NWLOemToAnsi(const nstr8 N_FAR * oemStr, pnstr8 ansiStr);
N_EXTERN_LIBRARY(void) NWLAnsiToOem(const nstr8 N_FAR * ansiStr, pnstr8 oemStr);

#if defined(__cplusplus)
}
#endif

#ifdef INCLUDE_OBSOLETE
# include "obsolete/o_locale.h"
#endif

#include "npackoff.h"

#endif /* NWLOCALE_H */


