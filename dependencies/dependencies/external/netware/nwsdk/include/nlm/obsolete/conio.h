/*============================================================================
=
=  NetWare NLM Library source code
=
=  Unpublished Copyright (C) 1995 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  conio.h
==============================================================================
*/
#ifdef _FIND_OLD_HEADERS_
# error This is an obsolete, Novell SDK header!
#else
# include <nwconio.h>
# ifdef __INLINE_FUNCTIONS__
#  if (__WATCOMC__ < 950)
#   define inp(__x)        _inline_inp(__x)
#   define inpw(__x)       _inline_inpw(__x)
#   define outp(__x,__y)   _inline_outp(__x,__y)
#   define outpw(__x,__y)  _inline_outpw(__x,__y)
#  else
#   pragma intrinsic(inp,inpw,outp,outpw)
#  endif
# endif

/* hardware-specific prototypes to be removed in future... */
extern unsigned int inp( int __port ); 
extern unsigned int inpw( int __port ); 
extern unsigned int outp( int __port, int __value );
extern unsigned int outpw( int __port, int __value );

/*
** Prototypes already removed per NLM Library Reference of 1993:
** These will inhibit an NLM's compliance with NetWare SFT III.
extern void _disable( void );
extern void _enable( void );
*/

#endif
