#ifndef _NWMALLOC_H_
#define _NWMALLOC_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1998 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwmalloc.h
==============================================================================
*/
#include <nwtypes.h>

#ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int size_t;
#endif

#ifdef __cplusplus
extern "C"
{
#endif

void     NWGarbageCollect( LONG NLMHandle );
size_t   NWGetAllocPageOverhead( size_t pageCount );
LONG     NWGetAvailableMemory( void );
size_t   NWGetPageSize( void );
int      NWMemorySizeAddressable( const void *addr, size_t size );

void     *alloca( size_t size );       /* (formal prototype) */
size_t   _msize( void *buffer );
void     *__qcalloc( size_t n, size_t el_size );
void     *__qmalloc( size_t size );
void     *__qrealloc( void  *old, size_t size );
size_t   stackavail( void );

/* compiler-specific implementations of alloca()... */
#if defined(__BORLANDC__)
void  *__alloca__( size_t );           /* from Borland static runtime */
#define alloca(s)    __alloca__(s)
#define _alloca(s)   alloca(s)

#elif defined(__ECC__) || defined(__ECPP__)
void  *__builtin_alloca( size_t );     /* Edinburgh Portable Compilers */
#define alloca       __builtin_alloca

#elif defined(__MWERKS__)              /* Metrowerks */
void  *_alloca( size_t );
#define alloca(s)    _alloca(s)

#elif defined(_MSC_VER)
void  *_alloca( size_t );              /* from Microsoft static runtime */
#define alloca(s)    _alloca(s)

#elif defined(__WATCOMC__)
void     *__alloca( size_t size );     /* Watcom in-lined assembly */
void     __push_eax( int __value );
#define ALLOCA_ALIGN(s) (((s) + (sizeof(int) - 1)) & ~(sizeof(int) - 1))
#define alloca(size) (__push_eax(stackavail()), __alloca(ALLOCA_ALIGN(size)))
#pragma aux __push_eax = 0x50                   \
   parm caller [eax];
#pragma aux __alloca =                          \
   0x59           /*       pop  ecx          */ \
   0x85 0xC0      /*       test eax, eax     */ \
   0x74 0x0F      /*       jz   short err    */ \
   0x8D 0x40 0x03 /*       lea  eax, [eax+3] */ \
   0x24 0xFC      /*       and  al, 0FCh     */ \
   0x3B 0xC1      /*       cmp  eax, ecx     */ \
   0x7F 0x06      /*       jg   short err    */ \
   0x2B 0xE0      /*       sub  esp, eax     */ \
   0x8B 0xC4      /*       mov  eax, esp     */ \
   0xEB 0x02      /*       jmp  done         */ \
   0x33 0xC0      /* err:  xor  eax, eax     */ \
                  /* done:                   */ \
   parm caller [eax] value[eax] modify [esp ecx];
#endif

#ifdef __cplusplus
}
#endif

#endif
