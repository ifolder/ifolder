#ifndef _NWSTRING_H_
#define _NWSTRING_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=	nwstring.h
==============================================================================
*/
#include <nwtypes.h>

#ifdef __cplusplus
extern "C"
{
#endif

extern int ASCIIZToLenStr
(
   char       *lenString,
   const char *ASCIIZstring
);

extern int ASCIIZToMaxLenStr
(
   char       *lenString,
   const char *ASCIIZstring,
   int         maximumLength
);

extern WORD IntSwap
(
   WORD __wordToSwap
);

extern char *LenStrCat
(
   char *destStr,
   const char *srcStr
);

extern int LenStrCmp
(
   const char *string1,
   const char *string2
);

extern char *LenStrCpy
(
   char *destStr,
   const char *srcStr
);

extern int LenToASCIIZStr
(
   char *ASCIIZstring,
   const char *lenString
);

extern long LongSwap
(
   long __longToBeSwapped
);

#ifdef __cplusplus
}
#endif


#endif
