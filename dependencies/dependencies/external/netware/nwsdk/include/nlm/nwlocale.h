#ifndef _NWLOCALE_H_
#define _NWLOCALE_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwlocale.h
==============================================================================
*/
#include <stdarg.h>

#define NUMBER_TYPE     double
#define _UCHAR          (unsigned char)
#define MERIDLEN        5

/* multibyte character length maximum is 2                                   */
#define L_MB_LEN_MAX    2

/* returned from NWCharType()...                                             */
#define NWSINGLE_BYTE   1
#define NWDOUBLE_BYTE   2

/* country/language ID definitions (field 'country_id' in struct Llconv)...  */
#define ARABIC          785   /* Pan-Arabic convention                       */
#define AUSTRALIA       61    /* Australian English convention               */
#define BELGIUM         32    /* Belgian French and Flemmish convention      */
#define CANADA_ENG      1     /* Canadian English convention                 */
#define CANADA_FR       2     /* Canadian French convention                  */
#define DENMARK         45
#define FINLAND         358
#define FRANCE          33    /* France French convention                    */
#define GERMANY         49
#define HEBREW          972
#define ITALY           39
#define LATIN_AMERICA   003   /* Latin American Spanish convention           */
#define NETHERLANDS     31
#define NORWAY          47
#define PORTUGAL        351   /* Portugal Portuguese convention              */
#define SPAIN           34    /* Spain Spanish convention                    */
#define SWEDEN          46
#define SWITZERLAND     41    /* Swiss French, German, Italian convention    */
#define UK              44    /* British English convention                  */
#define USA             1     /* American English convention                 */
#define JAPAN           81
#define KOREA           82
#define PRC             86    /* Mainland Chinese convention                 */
#define TAIWAN          88    /* Taiwanese Chinese convention                */
#define ASIAN_ENGLISH   99    /* Asian English convention                    */

/* type definitions... */
#ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _STRUCT_TM
#define _STRUCT_TM
struct tm
{
   int   tm_sec;              /* seconds after the minute--[0, 59]           */
   int   tm_min;              /* minutes after the hour--[0, 59]             */
   int   tm_hour;             /* hours since midnight--[0, 23]               */
   int   tm_mday;             /* days of the month--[1, 31]                  */
   int   tm_mon;              /* months since January--[0, 11]               */
   int   tm_year;             /* years since 1900--[0, 99]                   */
   int   tm_wday;             /* days since Sunday--[0, 6]                   */
   int   tm_yday;             /* days since first of January--[0, 365]       */
   int   tm_isdst;            /* Daylight Savings Time flag--[-, 0, +]       */
};                            /* (+ in effect, 0 if not, - if unknown)       */
#endif

typedef struct Llconv
{  /* ---------------- Numeric (Nonmonetary) Conventions ------------------- */
   char decimal_point[4];     /* decimal point                               */
   char thousands_sep[4];     /* separator for digits left of decimal point  */
   char grouping[4];          /* string indicating size of groups of digits  */

/*    ----------------------- Monetary Conventions -------------------------
The international currency symbol applicable to the current locale. The first
three characters contain the alphabetic international currency symbol in
accordance with those specified by ISO 4217 "codes for the representation of
currency and funds." The fourth is the character used to separate the inter-
national currency symbol from the monetary quantity.
*/
   char int_curr_symbol[8];   /* (per preceeding comment)                    */
   char currency_symbol[4];   /* currency symbol for the current locale      */
   char mon_decimal_point[4]; /* decimal point                               */
   char mon_thousands_sep[4]; /* separator for digits left of the decimal    */
   char mon_grouping[8];      /* string indicating size of groups of digits  */
   char positive_sign[4];     /* sign indicating positive monetary value     */
   char negative_sign[4];     /* sign indicating negative monetary value     */
   char int_frac_digits;      /* fractional digits in (international) quant. */
   char frac_digits;          /* fractional digits in quantity               */
                              /* for positive monetary quantities:           */
   char p_cs_precedes;        /* 1=precedes or 0=succeeds currency symbol    */
   char p_sep_by_space;       /* 1=space or 0=no space separator from symbol */
                              /* for negative monetary quantities:           */
   char n_cs_precedes;        /* location of symbol to quantity              */
   char n_sep_by_space;       /* separation of symbol in quantity            */
   char p_sign_posn;          /* positive sign position for positive quant.  */
   char n_sign_posn;          /* negative sign position for negative quant.  */

   /* -------------- Novell Additions to the ANSI definition: -------------- */
   unsigned short code_page;
   unsigned short country_id; /* (see definitions above)                     */
   char           data_list_separator[2];
   char           date_separator[2];
   char           time_separator[2];
   char           time_format;
   unsigned short date_format;
   char           reserved[50];
} LCONV;

typedef struct tagVECTOR
{
   char lowValue;
   char highValue;
} VECTOR;


#ifdef __cplusplus
extern "C"
{
#endif

/* extern double-byte table data... */
extern VECTOR _DBCSVector[5];

/* prototypes... */
extern int NWCharType(unsigned int ch); 
extern int NWCharVal(const char *string); 
extern int NWCharUpr(int chr); 
extern int NWcprintf(const char *format, ...); 
extern char *NWIncrement(const char *string, size_t numChars); 
/* 
   NWatoi, NWisalnum, NWisalpha, and NWisdigit are preferred over NWLatoi,
   NWisalnum, NWLisalpha, and NWLisdigit respectively.
*/
extern int NWatoi(const char *string); 
extern int NWisalnum(unsigned int ch); 
extern int NWisalpha(unsigned int ch); 
extern int NWisdigit(unsigned int ch); 
extern int NWisxdigit(unsigned int ch);
extern int NWitoa(int value, char *string, int radix);
extern int NWutoa(unsigned int value, char *string, int radix);
extern int NWltoa(long value, char *string, int radix);
extern int NWultoa(unsigned long value, char *string, int radix);
extern int NWLatoi(const char *string); 
extern int NWLisalnum(unsigned int ch); 
extern int NWLisalpha(unsigned int ch); 
extern int NWLisdigit(unsigned int ch); 

extern LCONV *NWLlocaleconv(LCONV *lconvPtr); 
extern int NWLmblen(const char *string, size_t maxBytes);
extern int NWLmbslen(const char *string); 
extern char *NWLsetlocale(int category, const char *locale); 
extern char *NWLsetlocale_411(int category, const char *locale); 
extern char *NWLstrbcpy(char *dest, const char *src, size_t maxlen); 
extern char *NWLstrchr(const char *string, int find); 
extern int NWLstrcoll(const char *string1, const char *string2); 
extern size_t NWLstrcspn(const char *string1, const char *string2); 
extern size_t NWLstrftime(char *string, size_t maxSize, const char *format, const struct tm *timePtr);
extern int NWLstricmp(const char *str1, const char *str2); 
extern char *NWLstrlwr(char *string); 
extern char *NWLstrpbrk(const char *string1, const char *string2); 
extern char *NWLstrrchr(const char *string, int find); 
extern char *NWLstrrev(char *string1, const char *string2); 
extern size_t NWLstrspn(const char *string1, const char *string2); 
extern char *NWLstrstr(const char *string, const char *searchString); 
extern char *NWLstrupr(char *string); 
extern size_t NWLstrxfrm(char *string1, const char *string2, size_t numChars); 
extern char *NWPrevChar(const char *string, const char *position); 
extern int NWprintf(const char *format, ...); 
extern int NWsprintf(char *s, const char *format, ...); 
extern char *NWstrImoney(char *buffer, NUMBER_TYPE Value); 
extern char *NWstrmoney(char *buffer, NUMBER_TYPE Value); 
extern int NWstrncoll(const char *string1, const char *string2, size_t maxChars);
extern char *NWstrncpy(char *target_string, const char *source_string, int numChars); 
extern char *NWstrnum(char *buffer, NUMBER_TYPE Value); 
extern int NWvcprintf(const char *format, va_list arg); 
extern int NWvprintf(const char *format, va_list arg); 
extern int NWvsprintf(char *s, const char *format, va_list arg);

#ifdef __cplusplus
}
#endif


/* macros... */
#define IF_DOUBLE_BYTE     (_DBCSVector[0].lowValue)

#ifdef __cplusplus

inline char *NWNextChar(const char *s) { return const_cast<char *>(s + (IF_DOUBLE_BYTE ? NWCharType(*s) : 1)); }
inline char *_NWIncrement(const char *s, size_t x) { return IF_DOUBLE_BYTE ? NWIncrement(s, x) : const_cast<char *>(s + x); }
inline char *NWLsetlocale(int c, const char *l) { return NWLsetlocale_411(c, l); }

#else

#define NWNextChar(s)      (IF_DOUBLE_BYTE ? ((s) + NWCharType(*(s))) : ((s) + 1))
#define _NWIncrement(s,x)  (IF_DOUBLE_BYTE ? NWIncrement(s,x) : ((s) + (x)))
#define NWLsetlocale(c, l)	 NWLsetlocale_411(c, l)

#endif

#endif
