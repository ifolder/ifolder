#ifndef _UTIME_H_
#define _UTIME_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  utime.h
==============================================================================
*/

#ifndef _TIME_T
# define _TIME_T
typedef long time_t;
#endif


struct utimbuf
{
   time_t   actime;         /* access time */
   time_t   modtime;        /* modification time */
};


#ifdef __cplusplus
extern "C" {
#endif

int   utime( const char *path, const struct utimbuf *times);

#ifdef __cplusplus
}
#endif


#endif
