#ifndef _TIME_H_
#define _TIME_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  time.h
==============================================================================
*/

#define CLOCKS_PER_SEC  100

#ifndef NULL
# define NULL 0
#endif

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _CLOCK_T
# define _CLOCK_T
typedef unsigned long clock_t;
#endif

#ifndef _TIME_T
# define _TIME_T
typedef long time_t;
#endif

#ifndef _STRUCT_TM
#define _STRUCT_TM
struct tm
{
   int   tm_sec;     /* seconds after the minute--range [0, 59]     */
   int   tm_min;     /* minutes after the hour--range [0, 59]       */
   int   tm_hour;    /* hours since midnight--range [0, 23]         */
   int   tm_mday;    /* days of the month--range [1, 31]            */
   int   tm_mon;     /* months since January--range [0, 11]         */
   int   tm_year;    /* years since 1900--range [0, 99]             */
   int   tm_wday;    /* days since Sunday--range [0, 6]             */
   int   tm_yday;    /* days since first of January--range [0, 365] */
   int   tm_isdst;   /* Daylight Savings Time flag--set [-, 0, +]:  */
};                   /*    (+ in effect, 0 if not, - if unknown)    */
#endif


#ifdef __cplusplus
extern "C"
{
#endif

/* ISO/ANSI C functions... */
char        *asctime( const struct tm * );
clock_t     clock( void );
char        *ctime( const time_t * );
double      difftime( time_t, time_t );
struct tm   *gmtime( const time_t * );
struct tm   *localtime( const time_t * );
time_t      mktime( struct tm * );
size_t      strftime( char *, size_t, const char *, const struct tm * );
time_t      time( time_t * );

/* POSIX data and functions... */
/* For extern char tzname[2], see macro below */
void     tzset( void );
clock_t  __get_CLK_TCK( void );
char     **__get_tzname( void );

/* POSIX-defined additions ... */
char        *asctime_r ( const struct tm *, char * );
char        *ctime_r ( const time_t *, char * );
struct tm   *gmtime_r ( const time_t *, struct tm * );
struct tm   *localtime_r ( const time_t *, struct tm * );

#ifdef __cplusplus
}
#endif

#define CLK_TCK          (__get_CLK_TCK())
#define tzname           (__get_tzname())

#ifdef __cplusplus
inline double difftime( time_t t1, time_t t0 ) { return (double)t1 - (double)t0; }
#else
#define difftime(t1, t0) ( (double) (t1) - (double) (t0) )
#endif

#endif
