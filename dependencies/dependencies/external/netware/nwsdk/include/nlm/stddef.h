#ifndef _STDDEF_H_
#define _STDDEF_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  stddef.h
==============================================================================
*/

typedef int    ptrdiff_t;

#ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int size_t;
#endif

#ifndef _WCHAR_T
#  define _WCHAR_T
typedef unsigned short   wchar_t;
#endif

#ifndef NULL
#  define NULL  0
#endif

#define offsetof(s, m)  (size_t) (&(((s *) 0)->m))

#endif
