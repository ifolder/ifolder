#ifndef _SETJMP_H_
#define _SETJMP_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  setjmp.h
==============================================================================
*/

#define _JBLEN 32

typedef double jmp_buf[_JBLEN];

#ifdef __cplusplus
extern "C"
{
#endif

void  _LongJmp( jmp_buf, int );
int   setjmp( jmp_buf );

#ifdef __cplusplus
}

inline void longjmp(jmp_buf env, int val) { _LongJmp(env, val); }

#else

#define longjmp(env, val)  _LongJmp(env, val)

#endif

#define setjmp(env) setjmp(env)

#endif
