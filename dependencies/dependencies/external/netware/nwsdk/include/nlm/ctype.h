#ifndef _CTYPE_H_
#define _CTYPE_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  ctype.h
==============================================================================
*/

#ifdef __cplusplus
extern "C"
{
#endif

extern unsigned char __ctype[];

/* ISO/ANSI C library functions... */
int   (isalnum)( int );
int   (isalpha)( int );
int   (iscntrl)( int );
int   (isdigit)( int );
int   (isgraph)( int );
int   (islower)( int );
int   (isprint)( int );
int   (ispunct)( int );
int   (isspace)( int );
int   (isupper)( int );
int   (isxdigit)( int );
int   (tolower)( int );
int   (toupper)( int );

/* non-ANSI functions... */
int   (isascii)( int );
int   (toascii)( int );

#ifdef __cplusplus
}

static const int _UPPER_  = 0x01;	/* Upper case        */
static const int _LOWER_  = 0x02;	/* Lower case        */
static const int _DIGIT_  = 0x04;	/* Numeral (digit)   */
static const int _SPACE_  = 0x08;	/* Spacing character */
static const int _PUNCT_  = 0x10;	/* Punctuation       */
static const int _CNTRL_  = 0x20;	/* Control character */
static const int _BLANK_  = 0x40;	/* Blank             */
static const int _XDIGIT_ = 0x80;	/* heXadecimal Digit */

inline int isalnum ( int c ) { return ((__ctype+1)[c] & (_UPPER_|_LOWER_|_DIGIT_)); }
inline int isalpha( int c )  { return ((__ctype+1)[c] & (_UPPER_|_LOWER_)); }
inline int iscntrl( int c )  { return ((__ctype+1)[c] & _CNTRL_); }
inline int isdigit( int c )  { return ((__ctype+1)[c] & _DIGIT_); }
inline int isxdigit( int c ) { return ((__ctype+1)[c] & _XDIGIT_); }
inline int isgraph( int c )  { return ((__ctype+1)[c] & (_PUNCT_|_UPPER_|_LOWER_|_DIGIT_)); }
inline int islower( int c )  { return ((__ctype+1)[c] & _LOWER_); }
inline int isupper( int c )  { return ((__ctype+1)[c] & _UPPER_); }
inline int isprint( int c )  { return ((__ctype+1)[c] & (_PUNCT_|_UPPER_|_LOWER_|_DIGIT_|_BLANK_)); }
inline int ispunct( int c )  { return ((__ctype+1)[c] & _PUNCT_); }
inline int isspace( int c )  { return ((__ctype+1)[c] & _SPACE_); }

inline int isascii( int c )  { return (!(c & ~0x7f)); }
inline int toascii( int c )  { return (c & 0x7f); }

#else	/* standard C */

#ifndef _UPPER_
# define _UPPER_  0x01	/* Upper case        */
# define _LOWER_  0x02	/* Lower case        */
# define _DIGIT_  0x04	/* Numeral (digit)   */
# define _SPACE_  0x08	/* Spacing character */
# define _PUNCT_  0x10	/* Punctuation       */
# define _CNTRL_  0x20	/* Control character */
# define _BLANK_  0x40	/* Blank             */
# define _XDIGIT_ 0x80	/* heXadecimal Digit */
#endif

#define isalnum(c)  ((__ctype+1)[c] & (_UPPER_|_LOWER_|_DIGIT_))
#define isalpha(c)  ((__ctype+1)[c] & (_UPPER_|_LOWER_))
#define iscntrl(c)  ((__ctype+1)[c] & _CNTRL_)
#define isdigit(c)  ((__ctype+1)[c] & _DIGIT_)
#define isxdigit(c) ((__ctype+1)[c] & _XDIGIT_)
#define isgraph(c)  ((__ctype+1)[c] & (_PUNCT_|_UPPER_|_LOWER_|_DIGIT_))
#define islower(c)  ((__ctype+1)[c] & _LOWER_)
#define isupper(c)  ((__ctype+1)[c] & _UPPER_)
#define isprint(c)  ((__ctype+1)[c] & (_PUNCT_|_UPPER_|_LOWER_|_DIGIT_|_BLANK_))
#define ispunct(c)  ((__ctype+1)[c] & _PUNCT_)
#define isspace(c)  ((__ctype+1)[c] & _SPACE_)

#define isascii(c)  (!(c & ~0x7f))
#define toascii(c)  (c & 0x7f)

#endif

#endif
