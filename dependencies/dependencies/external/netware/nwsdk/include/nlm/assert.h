#ifndef _ASSERT_H_
#define _ASSERT_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  assert.h
==============================================================================
*/

#undef assert

#ifdef NDEBUG
# define assert(e) ((void) 0)
#else

#ifdef __cplusplus
extern "C"
{
#endif

int  ___assert( const char *, const char *, int );

#ifdef __cplusplus
}
#endif

#define assert(e) ((void) ((e) || (___assert(#e, __FILE__, __LINE__), 0)))

#endif
#endif
