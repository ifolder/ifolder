#ifndef _STDLIB_H_
#define _STDLIB_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  stdlib.h
==============================================================================
*/

#ifndef NULL
# define NULL  0
#endif

#define EXIT_FAILURE    -1
#define EXIT_SUCCESS    0
#define RAND_MAX        32767

typedef struct
{
   int   quot;
   int   rem;
} div_t;

typedef struct
{
   long  quot;
   long  rem;
} ldiv_t;

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _WCHAR_T
# define _WCHAR_T
typedef unsigned short  wchar_t;
#endif

#ifdef __cplusplus
extern "C"
{
#endif

extern unsigned char __ctype[];

#if defined(__ECPP__) || defined(__ECC__)
# define atof        __EPC_atof
# define strtod      __EPC_strtod
# define strtod_ld   __EPC_strtod_ld
# define div         __EPC_div
# define ldiv        __EPC_ldiv
#endif

/* ISO/ANSI C library functions... */
void     abort( void );
int      abs( int );
int      atexit( void (*)( void ) );
double   atof( const char * );
int      atoi( const char * );
long     atol( const char * );
void     *bsearch( const void *, const void *, size_t, size_t,
            int (*)( const void *, const void * ) );
void     *calloc( size_t, size_t );
div_t    div( int, int );
void     exit( int );
void     _exit( int );
void     free( void * );
char     *getenv( const char * );
long     labs( long );
ldiv_t   ldiv( long, long );
void     *malloc( size_t );
int      (mblen)( const char *, size_t );
size_t   mbstowcs( wchar_t *, const char *, size_t );
int      mbtowc( wchar_t *, const char *, size_t );
void     qsort( void *, size_t, size_t, int (*)( const void *, const void * ) );
int      rand( void );
void     *realloc( void *, size_t );
void     srand( unsigned int );
double   strtod( const char *, char ** );
long     strtol( const char *, char **, int );

unsigned long  strtoul( const char *, char **, int );

int      system( const char * );
size_t   wcstombs( char *, const wchar_t *, size_t );
int      wctomb( char *, wchar_t );

/* nonstandard functions... */
int   clearenv( void );
char  *ecvt( double, int, int *, int * );
char  *fcvt( double, int, int *, int * );
char  *gcvt( double, int, char * );

unsigned long  htol( const char * );

char  *itoa( int, char *, int );
char  *itoab( unsigned long, char * );
char  *ltoa( long, char *, int );
int   (max)( int, int );
int   (min)( int, int );
int   putenv( const char *name );
int   rand_r( unsigned long *seed, int *result );

unsigned int   (_rotl)( unsigned int, unsigned int );
unsigned int   (_rotr)( unsigned int, unsigned int );

int      scanenv( int *sequence, char *variable, size_t *length, char *value );
int      setenv( const char *name, const char *value, int overwrite );
double   strtod_ld( const char *, char **, long double *);
int      strtoi( const char *, int );
char     *ultoa( unsigned long, char *, int );
int      unsetenv( const char *name );
char     *utoa( unsigned int, char *, int );

/* functions underlying macro support... */
unsigned long  _lrotl( unsigned long, unsigned int );
unsigned long  _lrotr( unsigned long, unsigned int );


#if defined(__ECPP__) || defined(__ECC__)            /* Edinburgh Portable Compilers */
div_t    _EPC_div( int, int );
ldiv_t   _EPC_ldiv( long, long );

# define div   _EPC_div
# define ldiv  _EPC_ldiv
#endif

#ifdef __cplusplus
}
#endif

#define MB_CUR_MAX   2

#ifdef __cplusplus

/* max and min are defined in C++ header <algorithm> */

inline int mblen( const char *s, size_t n ) { return mbtowc(0, s, n); }
inline unsigned _rotl( unsigned v, unsigned s ) { return _lrotl(v, s); }
inline unsigned _rotr( unsigned v, unsigned s ) { return _lrotr(v, s); }

#else

#define mblen(s, n)  mbtowc((wchar_t *) 0, s, n)
#ifndef max
# define max(a, b)    (((a) > (b)) ? (a) : (b))
#endif
#ifndef min
# define min(a, b)    (((a) < (b)) ? (a) : (b))
#endif
#define _rotl(v, s)  ((unsigned int) _lrotl((unsigned long) v, s))
#define _rotr(v, s)  ((unsigned int) _lrotr((unsigned long) v, s))

#endif

#endif
