#ifndef _NWTYPES_H_
#define _NWTYPES_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=	nwtypes.h
==============================================================================
*/

#ifndef LONG
# define LONG	unsigned long
#endif

#ifndef WORD
# define WORD	unsigned short
#endif

#ifndef BYTE
# define BYTE	unsigned char
#endif

#ifndef _SIZE_T_DEFINED_
#define _SIZE_T_DEFINED_
# ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int	size_t;
# endif
#endif

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif

#define N_INC_NO_OLD_MACROS 
#define N_INC_NO_OLD_CONSTANTS 
#include <ntypes.h>

#if ! defined (MisalignedLONG)
# if defined(N_INT_STRICT_ALIGNMENT)
#  define MisalignedLONG struct{ BYTE l[4];}
# else
#  define MisalignedLONG LONG
# endif
#endif

#if ! defined (MisalignedWORD)
# if defined(N_INT_STRICT_ALIGNMENT)
#  define MisalignedWORD struct{ BYTE w[2];}
# else
#  define MisalignedWORD unsigned short
# endif
#endif

#if ! defined (PackedLONG)
# if defined(N_INT_STRICT_ALIGNMENT)
#  define PackedLONG struct{ BYTE l[4];}
# else
#  define PackedLONG LONG
# endif
#endif

#if ! defined (PackedWORD)
# if defined(N_INT_STRICT_ALIGNMENT)
#  define PackedWORD struct{ BYTE w[2];}
# else
#  define PackedWORD unsigned short
# endif
#endif

#endif
