#ifndef __unilib_h__
#define __unilib_h__
/*============================================================================
=  Novell Software Development Kit
=
=  Copyright (C) 1999-2002 Novell, Inc. All Rights Reserved.
=
=  This work is subject to U.S. and international copyright laws and treaties.
=  Use and redistribution of this work is subject  to  the  license  agreement
=  accompanying  the  software  development kit (SDK) that contains this work.
=  However, no part of this work may be revised and/or  modified  without  the
=  prior  written consent of Novell, Inc. Any use or exploitation of this work
=  without authorization could subject the perpetrator to criminal  and  civil
=  liability. 
=
=  Source(s): Novell Header
=
=  unilib.h
==============================================================================
*/
#include <stddef.h>

/* error codes... */
#define UNI_ERR_MEM_ALLOC        (-494) /* no memory for allocation          */
#define UNI_ERR_BAD_HANDLE       (-496) /* nonexistant rule table handle     */
#define UNI_ERR_TABLE_CORRUPT    (-498) /* table corruption detected         */
#define UNI_ERR_TOO_FEW_BYTES    (-500) /* insufficient room in string       */
#define UNI_ERR_FILE_OPEN        (-501) /* unable to open data file          */
#define UNI_ERR_FILE_EXIST       (-502) /*                                   */
#define UNI_ERR_FILE_READ        (-504) /* unable to read data file          */
#define UNI_ERR_UNIMPLEMENTED    (-505) /* functional stub only              */
#define UNI_ERR_PREMATURE_END    (-506) /* premature end-of-string           */
#define UNI_ERR_UNMAPPABLE_CHAR  (-532) /* discovered during translation     */
#define UNI_ERR_INVALID_UTF8_SEQ 0xFFFF /* invalid UTF-8 character sequence  */

/* the local, default rule table for argument 'table' below... */
#define UNI_LOCAL_DEFAULT        (-1)   /* respective to local codepage      */

/* 'noMapFlag' values; when no mapping found... */
#define UNI_MAP_NO_CHAR     0           /* return UNI_ERR_UNMAPPABLE_CHAR    */
#define UNI_MAP_CHAR        1           /* use value in 'noMapChar' unless 0 */
#define UNI_MAP_BY_FUNC     1           /* use 'noMapFunc' if non-nil        */
#define UNI_MAP_SELF        2           /* use character itself              */

#define UNI_NOMAP_DEFAULT  '?'          /* no-map character if 'noMapChar' 0 */

/* character classification (UniClass_t)... */
#define UNI_UNDEF       0x00000000  /* no classification                     */
#define UNI_CNTRL       0x00000001  /* control character                     */
#define UNI_SPACE       0x00000002  /* non-printing space                    */
#define UNI_PRINT       0x00000004  /* printing (visible) character          */
#define UNI_SPECIAL     0x00000008  /* dingbats, special symbols, et al.     */
#define UNI_PUNCT       0x00000010  /* general punctuation                   */
#define UNI_DIGIT       0x00000020  /* decimal digit                         */
#define UNI_XDIGIT      0x00000040  /* hexadecimal digit                     */
#define UNI_RESERVED1   0x00000080  /* reserved for future use               */
#define UNI_LOWER       0x00000100  /* lower-case if applicable              */
#define UNI_UPPER       0x00000200  /* upper-case if applicable              */
#define UNI_RESERVED2   0x00000400  /* reserved for future use               */
#define UNI_ALPHA       0x00000800  /* non-number, non-punctuation including:*/
#define UNI_LATIN       0x00001000  /* Latin-based                           */
#define UNI_GREEK       0x00002000  /* Greek                                 */
#define UNI_CYRILLIC    0x00004000  /* Cyrillic                              */
#define UNI_HEBREW      0x00008000  /* Hebrew                                */
#define UNI_ARABIC      0x00010000  /* Arabic                                */
#define UNI_CJK         0x00020000  /* Chinese/Japanese/Korean characters    */
#define UNI_INDIAN      0x00040000  /* Devanagari, Bengali, Tamil, et al.    */
#define UNI_SEASIA      0x00080000  /* southeast Asia: Thai, Lao             */
#define UNI_CENASIA     0x00100000  /* cent. Asia: Armenian Tibetain, Georg. */
#define UNI_OTHER       0x80000000  /* none of the above                     */


#ifndef _UNICODE_T
# define _UNICODE_T
typedef unsigned short  unicode_t;
#endif

typedef int             UniRuleTable_t;/* more a cookie than anything else   */

typedef unsigned long   UniClass_t; /* Unicode character classification      */

typedef enum                        /* for uni2mono(), unicase(), et al.     */
{
   UNI_CASE_DEFAULT   = 0xFFFFFFFD, /* default monocasing as implemented     */
   UNI_CASE_NONE      = 0xFFFFFFFE, /* character is not 'alphabetic'         */
   UNI_CASE_AMBIGUOUS = 0xFFFFFFFF, /* character has no case                 */
   UNI_CASE_UPPER     = 0x00000000, /* emphatically upper case               */
   UNI_CASE_LOWER     = 0x00000001, /* emphatically lower case               */
   UNI_CASE_TITLE     = 0x00000002
} UniCase_t;


/* unmappable character handling function types... */
typedef int (*Loc2UniNoMapFunc_t)( unicode_t **dest, size_t remaining,
                                    const char **src, void *userParm );
typedef int (*Loc2Utf8NoMapFunc_t)( char **dest, size_t remaining,
                                    const char **src, void *userParm );
typedef int (*Utf82LocNoMapFunc_t)( char **dest, size_t remaining,
                                    const char **src, void *userParm );
typedef int (*Utf82UniNoMapFunc_t)( char **dest, size_t remaining,
                                    const unicode_t **src, void *userParm );
typedef int (*Uni2LocNoMapFunc_t)( char **dest, size_t remaining,
                                    const unicode_t **src, void *userParm );
typedef int (*Uni2Utf8NoMapFunc_t)( char **dest, size_t remaining,
                                    const unicode_t **src, void *userParm );

#ifdef __cplusplus
extern "C" {
#endif

/* rule table management... */
int        UniGetTable       ( int codePage, UniRuleTable_t *table );
int        UniSetDefault     ( UniRuleTable_t table );
int        UniDisposeTable   ( UniRuleTable_t table );
int        UniGetHostCodePage( void );

/* translation between local and other codepages, Unicode and UTF-8... */
int loc2uni      ( UniRuleTable_t table, unicode_t *dest, const char *src,
                   unicode_t noMapCh, int noMapFlag );
int loc2unipath  ( UniRuleTable_t table, unicode_t *dest, const char *src,
                   size_t *dryRunSize );
int locn2uni     ( UniRuleTable_t table, unicode_t *dest, size_t *destLen,
                   const char *src, size_t srcLen, unicode_t noMapCh,
                   int noMapFlag );
int locnx2uni    ( UniRuleTable_t table, unicode_t *dest, size_t *destLen,
                   const char *src, size_t srcLen,
                   Loc2UniNoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag );
int locnx2unipath( UniRuleTable_t table, unicode_t *dest, size_t *destLen,
                   const char *src, size_t srcLen,
                   Loc2UniNoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag, size_t *dryRunSize );
int loc2utf8     ( UniRuleTable_t handle, char *dest, const char *src,
                    char noMapCh, int noMapFlag );
int loc2utf8path ( UniRuleTable_t table, char *dest, const char *src,
                   size_t *dryRunSize );
int locn2utf8    ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const char *src, size_t srcLen, char noMapCh,
                   int noMapFlag );
int locnx2utf8   ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const char *src, size_t srcLen,
                   Loc2Utf8NoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag );
int uni2loc      ( UniRuleTable_t table, char *dest, const unicode_t *src,
                   char noMapCh, int noMapFlag );
int uni2locpath  ( UniRuleTable_t table, char *dest, const unicode_t *src,
                   size_t *dryRunSize );
int unin2loc     ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const unicode_t *src, size_t srcLen, char noMapCh,
                   int noMapFlag );
int uninx2loc    ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const unicode_t *src, size_t srcLen,
                   Uni2LocNoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag );
int uninx2locpath( UniRuleTable_t table, char *dest, size_t *destLen,
                   const unicode_t *src, size_t srcLen,
                   Uni2LocNoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag, size_t *dryRunSize );
int uni2utf8     ( char *dest, const unicode_t *src );
int uni2utf8path ( char *dest, const unicode_t *src, size_t *dryRunSize );
int unin2utf8    ( char *dest, size_t *destLen, const unicode_t *src,
                   size_t srcLen );
int utf82loc     ( UniRuleTable_t handle, char *dest, const char *src,
                   char noMapCh, int noMapFlag );
int utf8n2loc    ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const char *src, size_t srcLen, char noMapCh,
                   int noMapFlag );
int utf8nx2loc   ( UniRuleTable_t table, char *dest, size_t *destLen,
                   const char *src, size_t srcLen,
                   Utf82LocNoMapFunc_t noMapFunc, void *noMapFuncParm,
                   int noMapFlag );
int utf82uni     ( unicode_t *dest, const char *src );
int utf8n2uni    ( unicode_t *dest, size_t *destLen, const char *src,
                   size_t srcLen );

/* quick, 7-bit ASCII-capable translations--not preferred set... */
unicode_t *asc2uni     ( unicode_t *dest, const char *src );
unicode_t *ascn2uni    ( unicode_t *dest, const char *src, size_t nbytes );
char      *uni2asc     ( char *dest, const unicode_t *src );
char      *unin2asc    ( char *dest, const unicode_t *src, size_t nchars );

/* default 'noMapFunc' for X-translation to ensure round-trip conversion... */
int        LocToUniTagFunc( unicode_t **dest, size_t remaining,
                               const char **src, void *userParm );
int        UniToLocTagFunc( char **dest, size_t remaining,
                               const unicode_t **src, void *userParm );

/* string size calculation... */
int        LocToUniSize ( UniRuleTable_t table, const char *str,
                            size_t unmappedCharSize, int noMapFlag,
                            size_t *uniBufSize );
int        LocToUtf8Size( UniRuleTable_t table, const char *str,
                            size_t unmappedCharSize, int noMapFlag,
                            size_t *utf8BufSize );
int        UniToLocSize ( UniRuleTable_t table, const unicode_t *str,
                            size_t unmappedCharSize, int noMapFlag,
                            size_t *locBufSize );
int        UniToUtf8Size( const unicode_t *str, size_t *utf8BufSize );
int        Utf8ToLocSize( UniRuleTable_t table, const char *str,
                            size_t unmappedCharSize, int noMapFlag,
                            size_t *locBufSize );
int        Utf8ToUniSize( const char *str, size_t *uniBufSize );

/*-----------------------------------------------------------------------------
** Little utility functions. These are not to be preferred over the interfaces
** from wchar.h.
*/

/* classification... */
UniClass_t unitype  ( unicode_t ch );

/* collation... */
int        unicoll  ( const unicode_t *s1, const unicode_t *s2 );
int        unincoll ( const unicode_t *s1, const unicode_t *s2, size_t n );

/* casing... */
UniCase_t  unicase  ( unicode_t ch );
unicode_t *uni2mono ( unicode_t *dest, const unicode_t *src, UniCase_t casing );
unicode_t  chr2upr  ( unicode_t ch );
unicode_t  chr2lwr  ( unicode_t ch );
unicode_t  chr2title( unicode_t ch );
unicode_t *unilwr   ( unicode_t *string );
unicode_t *uniupr   ( unicode_t *string );
unicode_t *uni2lwr  ( unicode_t *dest, const unicode_t *src );
unicode_t *uni2upr  ( unicode_t *dest, const unicode_t *src );
unicode_t *uni2title( unicode_t *dest, const unicode_t *src );

/* length... */
size_t     unilen   ( const unicode_t *string );
size_t     uninlen  ( const unicode_t *string, size_t max );
size_t     unisize  ( const unicode_t *string );

/* copying... */
unicode_t *unicpy   ( unicode_t *tgt, const unicode_t *src );
unicode_t *unincpy  ( unicode_t *tgt, const unicode_t *src, size_t n );
unicode_t *uniset   ( unicode_t *base, unicode_t ch );
unicode_t *uninset  ( unicode_t *base, unicode_t ch, size_t n );

/* concatenation... */
unicode_t *unicat   ( unicode_t *tgt, const unicode_t *src );
unicode_t *unincat  ( unicode_t *tgt, const unicode_t *src, size_t n );
unicode_t *unilist  ( unicode_t *tgt, const unicode_t *s1, ... );

/* comparison... */
int        unicmp   ( const unicode_t *s1, const unicode_t *s2 );
int        uniicmp  ( const unicode_t *s1, const unicode_t *s2 );
int        unincmp  ( const unicode_t *s1, const unicode_t *s2, size_t n );
int        uninicmp ( const unicode_t *s1, const unicode_t *s2, size_t n );

/* character matching, indexing and miscellaneous... */
unicode_t *unichr   ( const unicode_t *string, unicode_t ch );
unicode_t *unirchr  ( const unicode_t *string, unicode_t ch );
unicode_t *uniindex ( const unicode_t *string, const unicode_t *search );

unicode_t *unistr   ( const unicode_t *as1, const unicode_t *as2 );
unicode_t *unirev   ( unicode_t *base );

size_t     unispn   ( const unicode_t *string, const unicode_t *charset );
size_t     unicspn  ( const unicode_t *string, const unicode_t *charset );

unicode_t *unipbrk  ( const unicode_t *s1, const unicode_t *s2 );
unicode_t *unitok   ( unicode_t *string, const unicode_t *sepset );
unicode_t *unitok_r ( unicode_t *string, const unicode_t *sepset,
                           unicode_t **lasts );

unicode_t *unidup   ( const unicode_t *s1 );

#ifdef __cplusplus
}
#endif


#endif
