#ifndef _SYS_TIME_H_
#define _SYS_TIME_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/time.h
==============================================================================
*/
#ifndef _TIME_T
# define _TIME_T
typedef long time_t;
#endif

/* commonly-used definitions... */
#define SEC       1
#define MILLISEC  1000
#define MICROSEC  1000000
#define NANOSEC   1000000000

#define __CLOCK_REALTIME0  0     /* wall clock, bound to LWP */
#define CLOCK_VIRTUAL      1     /* user CPU usage clock */
#define CLOCK_PROF         2     /* user and system CPU usage clock */
#define __CLOCK_REALTIME3  3     /* wall clock, not bound */
#define  CLOCK_REALTIME    __CLOCK_REALTIME3

#define TIMER_RELTIME      0x0   /* set timer relative */
#define TIMER_ABSTIME      0x1   /* set timer absolute */

/* time expressed in seconds and nanoseconds */
typedef struct timespec
{
   time_t   tv_sec;           /* seconds */
   long     tv_nsec;          /* and nanoseconds */
} timespec_t, timestrc_t;

#endif
