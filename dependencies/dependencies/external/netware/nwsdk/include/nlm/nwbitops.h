#ifndef _NWBITOPS_H_
#define _NWBITOPS_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwbitops.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


#ifdef __cplusplus
extern "C"
{
#endif

extern void BitClear
(
   void *bitArray,
   LONG bitNumber
);

extern void BitSet
(
   void *bitArray,
   LONG  bitNumber
);

extern LONG BitTest
(
   const void *bitArray,
   LONG  bitNumber
);

extern LONG BitTestAndClear
(
   void *bitArray,
   LONG  bitNumber
);

extern LONG BitTestAndSet
(
   void *bitArray,
   LONG bitNumber
);

extern LONG ScanBits
(
   const void *bitArray,
   LONG  startingBitNumber,
   LONG  totalBitCount);

extern LONG ScanClearedBits
(
   const void *bitArray,
   LONG  startingBitNumber,
   LONG  totalBitCount
);

#ifdef __cplusplus
}
#endif


#endif
