#ifndef _SYS_TIMEVAL_H_
#define _SYS_TIMEVAL_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  (C) Copyright 1982, 1985, 1986 Regents of the University of California.
=  All rights reserved. The Berkeley software License Agreement specifies the
=  terms and conditions for redistribution.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/timeval.h
==============================================================================
*/

#ifndef _TIMEVAL
# define _TIMEVAL
/* structure returned by gettimeofday(2) system call, used in select() */
struct timeval
{
   long tv_sec;  /* seconds */
   long tv_usec; /* and microseconds */
};
#endif

#endif
