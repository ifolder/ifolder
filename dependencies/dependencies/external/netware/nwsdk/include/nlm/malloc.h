#ifndef _MALLOC_H_
#define _MALLOC_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  malloc.h
==============================================================================
*/
#include <nwtypes.h>

#ifdef __cplusplus
extern "C"
{
#endif

void  *calloc( size_t n, size_t size );
void  free( void *ptr );
void  *malloc( size_t size );
void  *realloc( void *oldMem, size_t size );

#ifdef __cplusplus
}
#endif

#endif
