/******************************************************************************
  Source module name:  ntypes.h
  Release Version:     1.08

  %name: ntypes.h %
  %version: 6 %
  %date_modified: Fri Jun  9 09:00:36 2000 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/


#if ! defined( NTYPES_H )
#define NTYPES_H

/* 
   For brief documentation, please refer to the associated ntypes.txt 
   with the same release version. For in-depth documentation, please
   refer to the Portable API Definition Guide.
*/


/*
   Section 1:  Automatic definitions
   Section 1a: Platforms
*/

#if !defined(N_INC_NO_AUTO)

   #if defined NWOS2
      #if !defined N_PLAT_OS2 
         #define N_PLAT_OS2
      #endif
      #if !defined N_ARCH_16 
         #define N_ARCH_16
      #endif
   #elif defined(NWWIN)
      #if !defined N_PLAT_MSW 
         #define N_PLAT_MSW
      #endif
      #if !defined N_ARCH_16 
         #define N_ARCH_16
      #endif
   #elif defined(NWDOS)
      #if !defined N_PLAT_DOS
         #define N_PLAT_DOS
      #endif
      #if !defined N_ARCH_16
         #define N_ARCH_16
      #endif
   #elif defined(WIN32)
      #if !defined N_PLAT_MSW
         #define N_PLAT_MSW
      #endif
      #if !defined N_ARCH_32
         #define N_ARCH_32
      #endif
   #endif

   #if defined(N_PLAT_WNT3) || defined(N_PLAT_WNT4)
      #if !defined(N_PLAT_WNT)
         #define N_PLAT_WNT
      #endif
      #if !defined(N_PLAT_MSW)
         #define N_PLAT_MSW
      #endif
   #endif

   #if defined(N_PLAT_WNT) 
      #if !defined(N_PLAT_WNT3) && !defined(N_PLAT_WNT4)
         #define N_PLAT_WNT3
      #endif
      #if !defined(N_PLAT_MSW)
         #define N_PLAT_MSW
      #endif
   #endif

   #if defined(N_PLAT_MSW3) || defined(N_PLAT_MSW4)
      #if !defined(N_PLAT_MSW)
         #define N_PLAT_MSW
      #endif
   #endif

   #if defined N_PLAT_MSW && !defined N_PLAT_WNT 
      #if !defined N_PLAT_MSW3 && !defined N_PLAT_MSW4
         #define N_PLAT_MSW3
      #endif
   #endif

   #if defined N_PLAT_NLM
      #if !defined N_IAPX386 && !defined N_PPC
         #define N_IAPX386
      #endif
      #if !defined N_PLAT_NETWARE && !defined N_PLAT_NIOS
         #define N_PLAT_NETWARE
      #endif
   #endif

   #if defined N_PLAT_NIOS || defined N_PLAT_NETWARE
      #if !defined N_PLAT_NLM
         #define N_PLAT_NLM
      #endif
   #endif

   /* Set a default architecture size if none is set.  Test order is important 
   * and must be elif'ed because N_PLAT_MSW can be set for various Windows 
   * platforms that have differing default int sizes. */
   #if !defined(N_ARCH_64) && !defined(N_ARCH_32) && !defined(N_ARCH_16)
      #if defined(N_PLAT_DOS) 
         #define N_ARCH_16
      #elif defined(N_PLAT_WNT) 
         #define N_ARCH_32
      #elif defined(N_PLAT_OS2) && defined(_MSC_VER) 
         #define N_ARCH_16
      #elif defined(N_PLAT_MSW4) 
         #define N_ARCH_32
      #elif defined(N_PLAT_MSW) 
         #define N_ARCH_16
      #else 
         #define N_ARCH_32
      #endif
   #endif

#endif /* #if !defined(N_INC_NO_AUTO) */

/* 
   Section 1b: Compiler specific
*/

#if defined _MSC_VER
   #if(_MSC_VER < 700)
      #define N_MSC_OLD
   #else
      #define N_MSC
   #endif
#endif 

/* 
   Section 1c: Resource compiler specific
*/
#if defined( RC_INVOKED )
   #if !defined( N_RC_INVOKED )
      #define N_RC_INVOKED
   #endif  /* !defined( N_RC_INVOKED ) */
#endif  /* defined( RC_INVOKED ) */

/* 
   Section 2: Platform specific definitions
*/
#if !defined( N_RC_INVOKED )

   #if defined(N_ARCH_16) && defined(N_PLAT_MSW)

      #if !defined NWWIN
         #define NWWIN
      #endif

      #if defined(N_MSC_OLD)
         #define N_NEAR   near
         #define N_FAR    far
         #define N_HUGE   _huge
         #define N_PASCAL pascal
         #define N_CDECL  cdecl
         #define N_EXPORT _export
      #else
         #define N_NEAR   __near
         #define N_FAR    __far
         #define N_HUGE   _huge
         #define N_PASCAL __pascal
         #define N_CDECL  __cdecl
         #define N_EXPORT _export
      #endif

      #define N_API              N_FAR N_PASCAL
      #define N_API_VARARGS      N_FAR N_CDECL
      #define N_CALLBACK         N_FAR N_PASCAL
      #define N_CALLBACK_VARARGS N_FAR N_CDECL

      #undef  N_INT_ENDIAN_HI_LO
      #undef  N_INT_STRICT_ALIGNMENT

   #elif defined(N_ARCH_32) && defined(N_PLAT_MSW)

      #if !defined WIN32
         #define WIN32
      #endif

      #define N_NEAR
      #define N_FAR
      #define N_HUGE    
      #define N_PASCAL  
      #define N_CDECL   __cdecl
      #define N_EXPORT

      #define N_API              __stdcall
      #define N_API_VARARGS      N_CDECL
      #define N_VARARGS          N_CDECL
      #define N_CALLBACK         __stdcall
      #define N_CALLBACK_VARARGS N_CDECL

      #undef  N_INT_ENDIAN_HI_LO
      #undef  N_INT_STRICT_ALIGNMENT

   #elif defined(N_ARCH_16) && defined(N_PLAT_OS2)

      #if !defined NWOS2
         #define NWOS2
      #endif

      #if defined(N_MSC_OLD)
         #define N_NEAR   near
         #define N_FAR    far
         #define N_HUGE   _huge 
         #define N_PASCAL pascal
         #define N_CDECL  cdecl
      #else
         #define N_NEAR   __near
         #define N_FAR    __far
         #define N_HUGE   _huge
         #define N_PASCAL __pascal
         #define N_CDECL  __cdecl
      #endif

      #define N_API              N_FAR N_PASCAL
      #define N_API_VARARGS      N_FAR N_CDECL
      #define N_CALLBACK         N_FAR N_PASCAL
      #define N_CALLBACK_VARARGS N_FAR N_CDECL

      #undef  N_INT_ENDIAN_HI_LO
      #undef  N_INT_STRICT_ALIGNMENT

   #elif defined(N_ARCH_32) && defined(N_PLAT_OS2)

      #define N_NEAR
      #define N_FAR
      #define N_HUGE    
      #if defined __BORLANDC__
         #define N_PASCAL __pascal
      #else
         #define N_PASCAL _Pascal
      #endif
      #define N_CDECL

      #define N_API              N_PASCAL
      #define N_API_VARARGS      N_CDECL
      #define N_CALLBACK         N_PASCAL
      #define N_CALLBACK_VARARGS N_CDECL

      #undef  N_INT_ENDIAN_HI_LO
      #undef  N_INT_STRICT_ALIGNMENT

   #elif defined(N_PLAT_DOS)

      #if !defined NWDOS
         #define NWDOS
      #endif

      #if defined(N_MSC_OLD)
         #define N_NEAR   near
         #define N_FAR    far
         #define N_HUGE   _huge
         #define N_PASCAL pascal
         #define N_CDECL  cdecl
      #else
         #define N_NEAR   __near
         #define N_FAR    __far
         #define N_HUGE   _huge
         #define N_PASCAL __pascal
         #define N_CDECL  __cdecl
      #endif

      #define N_API              N_FAR N_PASCAL
      #define N_API_VARARGS      N_FAR N_CDECL
      #define N_CALLBACK         N_FAR N_PASCAL
      #define N_CALLBACK_VARARGS N_FAR N_CDECL

      #undef  N_INT_ENDIAN_HI_LO
      #undef  N_INT_STRICT_ALIGNMENT

   #elif defined(N_PLAT_NLM)

      #if defined(N_IAPX386)

         #undef  N_INT_ENDIAN_HI_LO
         #undef  N_INT_STRICT_ALIGNMENT

      #elif defined(N_PPC)

         #define N_INT_ENDIAN_HI_LO
         #define N_INT_STRICT_ALIGNMENT

      #else    /* no machine type defined */

         #error A machine type must be defined

      #endif
      #define N_NEAR
      #define N_FAR
      #define N_HUGE
      #define N_PASCAL  pascal
		#ifdef __WATCOMC__
			#define N_CDECL   cdecl
		#else
			#define N_CDECL   __cdecl
		#endif
      #define N_API              
      #define N_API_VARARGS      
      #define N_CALLBACK         
      #define N_CALLBACK_VARARGS 

   #elif defined(N_PLAT_MAC)

      #define N_NEAR
      #define N_FAR
      #define N_HUGE
      #define N_PASCAL  pascal
      #define N_CDECL

      #define N_API              N_PASCAL
      #define N_API_VARARGS      N_CDECL
      #define N_CALLBACK         N_PASCAL
      #define N_CALLBACK_VARARGS N_CDECL

      #define N_INT_ENDIAN_HI_LO
      #if defined( mc68020 )
         #undef  N_INT_STRICT_ALIGNMENT
      #else
         #define N_INT_STRICT_ALIGNMENT
      #endif

   #elif defined(N_PLAT_UNIX)

      #define N_NEAR
      #define N_FAR
      #define N_HUGE
      #define N_PASCAL
      #define N_CDECL

      #define N_API              N_PASCAL
      #define N_API_VARARGS      N_CDECL
      #define N_CALLBACK         N_PASCAL
      #define N_CALLBACK_VARARGS N_CDECL

      #if defined(BYTE_ORDER) && defined(BIG_ENDIAN)
         #if (BYTE_ORDER == BIG_ENDIAN)
            #define N_INT_ENDIAN_HI_LO
         #endif
      #elif defined(vax) || defined(ns32000) || defined(sun386) || \
            defined(MIPSEL) || defined(BIT_ZERO_ON_RIGHT)
         /* do nothing (little endian) */
      #elif defined(sel) || defined(pyr) || defined(mc68000) || \
            defined(sparc) || defined(is68k) || defined(tahoe) || \
            defined(ibm032) || defined(ibm370) || defined(MIPSEB) || \
            defined(__hpux) || defined (BIT_ZERO_ON_LEFT)
         #define N_INT_ENDIAN_HI_LO
      #endif

      /* For now, we assume strict alignment for all Unix platforms since it
         is the worst case (safe); we could optimize for certain platforms
         in the future */

      #define N_INT_STRICT_ALIGNMENT

   #else    /* no platform constant */

      #error A platform must be defined

   #endif   /* platforms */

#endif  /* !defined( N_RC_INVOKED ) */

/*
   Section 3:  Other constants
   Section 3a: NULL
*/

#if !defined(NULL) && defined(__cplusplus)
   #define NULL   0
#endif

#if defined( N_RC_INVOKED )
   #define NULL   0
#endif

#if !defined NULL
   #if defined N_MSC
      #define NULL ((void *) 0)
   #else
      #if defined(M_I86S) || defined(M_I86SM) || defined(M_I86C) || \
          defined(M_I86CM) || \
          defined(__TINY__) || defined(__SMALL__) || defined(__MEDIUM__) || \
          defined(N_PLAT_NLM)

         #define NULL   0
      #else
         #define NULL   0L
      #endif
   #endif
#endif /* NULL */

/*
   Section 3b: Standard constants
*/

#define N_ALWAYS     1
#define N_SUCCESS    0
#define N_FAILURE    (-1)
#define N_YES        1
#define N_NO         0
#define N_FALSE      0
#define N_TRUE       1
#define N_UNKNOWN    (N_TRUE + 1)

/*
   Section 3c: System constants
*/

#define N_SYS_USER          0
#define N_SYS_NETWARE       1
#define N_SYS_NAWF          2
#define N_SYS_MAX_COUNT     20
#define N_SYS_NAME_MAX_LEN  31

/*
   Section 3d: Historical constant synonyms
*/

#if ! defined( N_INC_NO_OLD_CONSTANTS )

   #if ! defined( TRUE )
      #define TRUE        1
   #endif

   #if ! defined( FALSE )
      #define FALSE       0
   #endif

#endif

/*
   Section 3e: Miscellaneous constants
*/

/* N_BITSPERBYTE is currently only used internally */
#if defined BITSPERBYTE
   #define N_BITSPERBYTE  BITSPERBYTE
#else
   #define N_BITSPERBYTE  8
#endif

/*
   Section 4:  Types
   Section 4a: Machine dependent types
*/

#if !defined(N_RC_INVOKED)

   typedef signed char     nint8;
	typedef unsigned char   nuint8;
   typedef signed short    nint16;
   typedef unsigned short  nuint16;
   #if defined(__alpha)
      typedef signed int nint32;
      typedef unsigned int nuint32;
      typedef signed long nint64;
      typedef unsigned long nuint64;
      #define nint64_type
   #else
      typedef signed long     nint32;
      typedef unsigned long   nuint32;
		#if (defined(_INTEGRAL_MAX_BITS) && (_INTEGRAL_MAX_BITS >= 64)) && defined __MWERKS__
		   /* Metrowerks */
			typedef signed long long  nint64;
			typedef unsigned long long nuint64;
			#define nint64_type
		#elif (defined(_INTEGRAL_MAX_BITS) && (_INTEGRAL_MAX_BITS >= 64)) || defined (__WATCOM_INT64__)
			/* MS Visual C++ or WATCOM  */	
			typedef signed __int64 nint64;
			typedef unsigned __int64 nuint64;
         #define nint64_type
		#endif
   #endif
   typedef float           nreal32;
   typedef double          nreal64;

   #if !defined N_PLAT_MAC

      #if defined N_FORCE_INT_32
         typedef signed long     nint;
         typedef unsigned long   nuint;
      #elif defined N_FORCE_INT_16
         typedef signed short    nint;
         typedef unsigned short  nuint;
      #else
         typedef signed int      nint;
         typedef unsigned int    nuint;
      #endif

      typedef unsigned int    nbool;
      #if !(defined N_PLAT_MSW && defined N_ARCH_32)
         #if ! defined(__alpha) && ! defined(_AIX)
            typedef long double     nreal80;
            #define nreal80_type
         /* VC++ for NT does not support nreal80's by default. Check readme for
            instructions on how to enable them */
         #endif
      #endif

      typedef double          nreal;
   #else
      typedef signed long     nint;
      typedef unsigned long   nuint;
      typedef unsigned char   nbool;

      #if defined( powerc ) || defined( __powerc ) || defined( THINK_C )
         typedef long double nreal80;
         typedef double          nreal;
      #else
         typedef extended        nreal80;
         typedef extended        nreal;
      #endif
      #define nreal80_type
   #endif

/*
   Section 4b: Derived types
*/

   #if defined(__alpha)
      typedef nuint64 nparam;
   #else
      typedef nuint32 nparam;
   #endif
   typedef nparam nhdl;
   typedef nparam nid;
   typedef nuint8  nflag8;
   typedef nuint16 nflag16;
   typedef nuint32 nflag32;

   typedef nuint8    nbool8;
   typedef nuint16   nbool16;
   typedef nuint32   nbool32;

   typedef nint32  nfixed;
   #if defined(N_USE_UNSIGNED_CHAR)
      typedef nuint8  nstr8;
   #else
      typedef char  nstr8;
   #endif
   typedef nuint16 nstr16;
   #if defined(N_USE_STR_16)
      typedef  nstr16 nstr;
   #else
      typedef  nstr8  nstr;
   #endif
   typedef nstr    nchar;
   typedef nstr8   nchar8;
   typedef nstr16  nchar16;
   typedef nchar16 nwchar;

   /* 
      Return code for functions that return status/errors.
   */

   #define NWRCODE  nint32


   /*
      Pointers to scalars.
   */

   typedef void      N_FAR *  nptr;
   typedef void      N_FAR *  npproc;

   typedef nint8     N_FAR *  pnint8;
   typedef nuint8    N_FAR *  pnuint8;
   typedef nint16    N_FAR *  pnint16;
   typedef nuint16   N_FAR *  pnuint16;
   typedef nint32    N_FAR *  pnint32;
   typedef nuint32   N_FAR *  pnuint32;
   #if defined( nint64_type )
      typedef nint64    N_FAR *  pnint64;
      typedef nuint64   N_FAR *  pnuint64;
   #endif
   typedef nint      N_FAR *  pnint;
   typedef nuint     N_FAR *  pnuint;
   typedef nflag8    N_FAR *  pnflag8;
   typedef nflag16   N_FAR *  pnflag16;
   typedef nflag32   N_FAR *  pnflag32;
   typedef nbool     N_FAR *  pnbool;
   typedef nbool8    N_FAR *  pnbool8;
   typedef nbool16   N_FAR *  pnbool16;
   typedef nbool32   N_FAR *  pnbool32;
   typedef nfixed    N_FAR *  pnfixed;
   typedef nstr      N_FAR *  pnstr;
   typedef pnstr     N_FAR *  ppnstr;
   typedef nstr16    N_FAR *  pnstr16;
   typedef pnstr16   N_FAR *  ppnstr16;
   typedef nstr8     N_FAR *  pnstr8;
   typedef pnstr8    N_FAR *  ppnstr8;
   typedef nchar     N_FAR *  pnchar;
   typedef pnchar    N_FAR *  ppnchar;
   typedef nchar16   N_FAR *  pnchar16;
   typedef pnchar16  N_FAR *  ppnchar16;
   typedef nchar8    N_FAR *  pnchar8;
   typedef pnchar8   N_FAR *  ppnchar8;
   typedef nwchar    N_FAR *  pnwchar;
   typedef pnwchar   N_FAR *  ppnwchar;
   typedef nreal     N_FAR *  pnreal;
   typedef nreal32   N_FAR *  pnreal32;
   typedef nreal64   N_FAR *  pnreal64;
   #if defined( nreal80_type )
      typedef nreal80   N_FAR *  pnreal80;
   #endif
   typedef nid       N_FAR *  pnid;
   typedef nhdl      N_FAR *  pnhdl;
   typedef nparam    N_FAR *  pnparam;

   /*
      Pointers to pointers.
   */
   typedef nptr      N_FAR *  pnptr;
   typedef npproc    N_FAR *  pnpproc;

   typedef pnint8    N_FAR *  ppnint8;
   typedef pnuint8   N_FAR *  ppnuint8;
   typedef pnint16   N_FAR *  ppnint16;
   typedef pnuint16  N_FAR *  ppnuint16;
   typedef pnint32   N_FAR *  ppnint32;
   typedef pnuint32  N_FAR *  ppnuint32;
   #if defined( nint64_type )
      typedef pnint64   N_FAR *  ppnint64;
      typedef pnuint64  N_FAR *  ppnuint64;
   #endif
   typedef pnint     N_FAR *  ppnint;
   typedef pnuint    N_FAR *  ppnuint;
   typedef pnflag8   N_FAR *  ppnflag8;
   typedef pnflag16  N_FAR *  ppnflag16;
   typedef pnflag32  N_FAR *  ppnflag32;
   typedef pnbool    N_FAR *  ppnbool;
   typedef pnbool8   N_FAR *  ppnbool8;
   typedef pnbool16  N_FAR *  ppnbool16;
   typedef pnbool32  N_FAR *  ppnbool32;
   typedef pnfixed   N_FAR *  ppnfixed;
   typedef ppnstr    N_FAR *  pppnstr;
   typedef ppnstr16  N_FAR *  pppnstr16;
   typedef ppnstr8   N_FAR *  pppnstr8;
   typedef ppnchar   N_FAR *  pppnchar;
   typedef ppnchar16 N_FAR *  pppnchar16;
   typedef ppnchar8  N_FAR *  pppnchar8;
   typedef ppnwchar  N_FAR *  pppnwchar;
   typedef pnreal    N_FAR *  ppnreal;
   typedef pnreal32  N_FAR *  ppnreal32;
   typedef pnreal64  N_FAR *  ppnreal64;
   #if defined( nreal80_type )
      typedef pnreal80  N_FAR *  ppnreal80;
   #endif
   typedef pnid      N_FAR *  ppnid;
   typedef pnhdl     N_FAR *  ppnhdl;
   typedef pnparam   N_FAR *  ppnparam;
   typedef pnptr     N_FAR *  ppnptr;
   typedef pnpproc   N_FAR *  ppnpproc;

   /*
      Section 4c: GUI structures - Removed
   */

   /*
      Section 4d: Platform dependent types
   */

   #if defined( N_PLAT_MSW )

      typedef unsigned char N_HUGE *   neptr;

   #elif defined( N_PLAT_MAC )

      typedef unsigned char N_HUGE *   neptr;

   #elif defined( N_PLAT_UNIX )

      typedef unsigned char N_HUGE *   neptr;

   #elif defined( N_PLAT_OS2 )

      typedef unsigned char N_HUGE *   neptr;

   #elif defined( N_PLAT_DOS )

      typedef unsigned char N_HUGE *   neptr;

   #elif defined( N_PLAT_NLM ) 
   
      /* We just need to include NLM in our thinking */
      typedef unsigned char N_HUGE *   neptr;

   #else    /* This should not be reached because it was already */
            /* tested earlier. */
      #error A platform must be defined
   #endif   /* MSW or MAC or UNIX or OS2 undefined */

   /*
      Pointers to scalars.
   */

   typedef neptr       N_FAR * pneptr;

   /*
      Section 5: Macros
   */

   #if defined( __cplusplus )

      /* Special 'extern' for C++ to avoid name mangling... */

      #define _N_EXTERN          extern "C"

   #else
      #define _N_EXTERN          extern
   #endif /* __cplusplus */

   #define N_UNUSED_VAR( x ) x = x

   #define N_REG1              register
   #define N_REG2              register
   #define N_REG3              register
   #define N_REG4              register

   #define N_STATIC_VAR        static
   #define N_INTERN_VAR        static
   #define N_GLOBAL_VAR
   #define N_EXTERN_VAR        extern

   #if defined( N_PLAT_MSW )
      #define N_INTERN_FUNC( retType )       static    retType N_NEAR       
      #define N_INTERN_FUNC_C( retType )     static    retType N_NEAR     
      #define N_INTERN_FUNC_PAS( retType )   static    retType N_NEAR N_PASCAL     
      #define N_GLOBAL_FUNC( retType )                 retType     
      #define N_EXTERN_FUNC( retType )       _N_EXTERN retType     
      #define N_GLOBAL_FUNC_C( retType )               retType     
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN retType     
      #define N_GLOBAL_FUNC_PAS( retType )             retType        N_PASCAL     
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN retType        N_PASCAL     
   #elif defined( N_PLAT_MAC )
      #define N_INTERN_FUNC( retType )       static             retType
      #define N_INTERN_FUNC_C( retType )     static             retType
      #define N_INTERN_FUNC_PAS( retType )   static    N_PASCAL retType
      #define N_GLOBAL_FUNC( retType )                          retType
      #define N_EXTERN_FUNC( retType )       _N_EXTERN          retType
      #define N_GLOBAL_FUNC_C( retType )                        retType
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN          retType
      #define N_GLOBAL_FUNC_PAS( retType )             N_PASCAL retType
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN N_PASCAL retType
   #elif defined( N_PLAT_UNIX )
      #define N_INTERN_FUNC( retType )       static    retType
      #define N_INTERN_FUNC_C( retType )     static    retType
      #define N_INTERN_FUNC_PAS( retType )   static    retType
      #define N_GLOBAL_FUNC( retType )                 retType
      #define N_EXTERN_FUNC( retType )       _N_EXTERN retType
      #define N_GLOBAL_FUNC_C( retType )               retType
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN retType
      #define N_GLOBAL_FUNC_PAS( retType )             retType
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN retType
   #elif defined( N_PLAT_OS2 )
      #define N_INTERN_FUNC( retType )       static    retType
      #define N_INTERN_FUNC_C( retType )     static    retType
      #define N_INTERN_FUNC_PAS( retType )   static    retType N_PASCAL
      #define N_GLOBAL_FUNC( retType )                 retType
      #define N_EXTERN_FUNC( retType )       _N_EXTERN retType
      #define N_GLOBAL_FUNC_C( retType )               retType
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN retType
      #define N_GLOBAL_FUNC_PAS( retType )             retType N_PASCAL
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN retType N_PASCAL
   #elif defined( N_PLAT_DOS )
      #define N_INTERN_FUNC( retType )       static    retType N_NEAR
      #define N_INTERN_FUNC_C( retType )     static    retType N_NEAR
      #define N_INTERN_FUNC_PAS( retType )   static    retType N_NEAR N_PASCAL
      #define N_GLOBAL_FUNC( retType )                 retType
      #define N_EXTERN_FUNC( retType )       _N_EXTERN retType
      #define N_GLOBAL_FUNC_C( retType )               retType
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN retType
      #define N_GLOBAL_FUNC_PAS( retType )             retType        N_PASCAL
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN retType        N_PASCAL
   #elif defined( N_PLAT_NLM )
      #define N_INTERN_FUNC( retType )       static    retType
      #define N_INTERN_FUNC_C( retType )     static    retType
      #define N_INTERN_FUNC_PAS( retType )   static    retType
      #define N_GLOBAL_FUNC( retType )                 retType
      #define N_EXTERN_FUNC( retType )       _N_EXTERN retType
      #define N_GLOBAL_FUNC_C( retType )               retType
      #define N_EXTERN_FUNC_C( retType )     _N_EXTERN retType
      #define N_GLOBAL_FUNC_PAS( retType )             retType
      #define N_EXTERN_FUNC_PAS( retType )   _N_EXTERN retType
   #endif

   #if ! defined( N_PLAT_MAC )
      #define N_GLOBAL_LIBRARY( retType )              retType N_API
      #define N_EXTERN_LIBRARY( retType )    _N_EXTERN retType N_API
      #define N_GLOBAL_LIBRARY_C( retType )            retType N_API_VARARGS
      #define N_EXTERN_LIBRARY_C( retType )  _N_EXTERN retType N_API_VARARGS
   #else
      #define N_GLOBAL_LIBRARY( retType )              N_PASCAL retType
      #define N_EXTERN_LIBRARY( retType )    _N_EXTERN N_PASCAL retType
      #define N_GLOBAL_LIBRARY_C( retType )                     retType
      #define N_EXTERN_LIBRARY_C( retType )  _N_EXTERN          retType
   #endif

   #if defined( N_PLAT_MSW )

      #define N_GLOBAL_LIBRARY_PAS( retType ) \
                                     retType N_FAR N_PASCAL
      #define N_EXTERN_LIBRARY_PAS( retType ) \
                           _N_EXTERN retType N_FAR N_PASCAL

      #define N_GLOBAL_CALLBACK( retType ) \
                                     retType N_CALLBACK     N_EXPORT
      #define N_EXTERN_CALLBACK( retType ) \
                           _N_EXTERN retType N_CALLBACK     N_EXPORT
      #define N_GLOBAL_CALLBACK_C( retType ) \
                                     retType N_FAR N_CDECL  N_EXPORT
      #define N_EXTERN_CALLBACK_C( retType ) \
                           _N_EXTERN retType N_FAR N_CDECL  N_EXPORT
      #define N_GLOBAL_CALLBACK_PAS( retType ) \
                                     retType N_FAR N_PASCAL N_EXPORT
      #define N_EXTERN_CALLBACK_PAS( retType ) \
                           _N_EXTERN retType N_FAR N_PASCAL N_EXPORT

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                             typedef retType (                        *typeName)
      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                             typedef retType (N_CDECL                 *typeName)
      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                             typedef retType (N_PASCAL                *typeName)
      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                             typedef retType (N_FAR N_PASCAL          *typeName)
      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                             typedef retType (N_FAR N_CDECL           *typeName)
      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                             typedef retType (N_FAR N_PASCAL          *typeName)
      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                             typedef retType (N_CALLBACK     N_EXPORT *typeName)

      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                             typedef retType (N_FAR N_CDECL  N_EXPORT *typeName)

      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                             typedef retType (N_FAR N_PASCAL N_EXPORT *typeName)

      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                             typedef retType (N_NEAR                  *typeName)

      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                             typedef retType (N_NEAR N_CDECL          *typeName)

      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                             typedef retType (N_NEAR N_PASCAL         *typeName)

   #elif defined( N_PLAT_MAC )

      #define N_GLOBAL_LIBRARY_PAS( retType )            N_PASCAL   retType
      #define N_EXTERN_LIBRARY_PAS( retType )  _N_EXTERN N_PASCAL   retType

      #define N_GLOBAL_CALLBACK( retType )               N_CALLBACK retType
      #define N_EXTERN_CALLBACK( retType )     _N_EXTERN N_CALLBACK retType
      #define N_GLOBAL_CALLBACK_C( retType )             N_CDECL    retType
      #define N_EXTERN_CALLBACK_C( retType )   _N_EXTERN N_CDECL    retType
      #define N_GLOBAL_CALLBACK_PAS( retType )           N_PASCAL   retType
      #define N_EXTERN_CALLBACK_PAS( retType ) _N_EXTERN N_PASCAL   retType

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                                            typedef            retType (*typeName)
      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                                            typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                                            typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                                            typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                                            typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                                            typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                                            typedef N_CALLBACK retType (*typeName)
      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                                            typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                                            typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                                            typedef            retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                                            typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                                            typedef N_PASCAL   retType (*typeName)

   #elif defined( N_PLAT_UNIX )

      #define N_GLOBAL_LIBRARY_PAS( retType )                       retType
      #define N_EXTERN_LIBRARY_PAS( retType )  _N_EXTERN N_PASCAL   retType

      #define N_GLOBAL_CALLBACK( retType )               N_CALLBACK retType
      #define N_EXTERN_CALLBACK( retType )     _N_EXTERN N_CALLBACK retType
      #define N_GLOBAL_CALLBACK_C( retType )             N_CDECL    retType
      #define N_EXTERN_CALLBACK_C( retType )   _N_EXTERN N_CDECL    retType
      #define N_GLOBAL_CALLBACK_PAS( retType )           N_PASCAL   retType
      #define N_EXTERN_CALLBACK_PAS( retType ) _N_EXTERN N_PASCAL   retType

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                                          typedef            retType (*typeName)
      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                                          typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                                          typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                                          typedef            retType (*typeName)
      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                                          typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                                          typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                                          typedef N_CALLBACK retType (*typeName)
      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                                          typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                                          typedef N_PASCAL   retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                                          typedef            retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                                          typedef N_CDECL    retType (*typeName)
      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                                          typedef N_PASCAL   retType (*typeName)

   #elif defined( N_PLAT_OS2)

      #define N_PRAGMA_ONCE

      #define N_GLOBAL_LIBRARY_PAS( retType )               retType  N_PASCAL
      #define N_EXTERN_LIBRARY_PAS( retType )     _N_EXTERN retType  N_PASCAL

      #define N_GLOBAL_CALLBACK( retType )                  retType  N_CALLBACK _export
      #define N_EXTERN_CALLBACK( retType )        _N_EXTERN retType  N_CALLBACK
      #define N_GLOBAL_CALLBACK_NATIVE( retType )           retType  EXPENTRY   _export
      #define N_EXTERN_CALLBACK_NATIVE( retType ) _N_EXTERN retType  EXPENTRY
      #define N_GLOBAL_CALLBACK_C( retType )                retType  N_CDECL    _export
      #define N_EXTERN_CALLBACK_C( retType )      _N_EXTERN retType  N_CDECL
      #define N_GLOBAL_CALLBACK_PAS( retType )              retType  N_PASCAL   _export
      #define N_EXTERN_CALLBACK_PAS( retType )    _N_EXTERN retType  N_PASCAL

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                              typedef retType (                    *typeName)

      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                              typedef retType (         N_CDECL    *typeName)

      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                              typedef retType (         N_PASCAL   *typeName)

      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                              typedef retType (         N_PASCAL   *typeName)

      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                              typedef retType (         N_CDECL    *typeName)

      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                              typedef retType (         N_PASCAL   *typeName)

      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                              typedef retType (_export  N_CALLBACK *typeName)

      #define N_TYPEDEF_CALLBACK_NATIVE( retType, typeName ) \
                              typedef retType (_export  EXPENTRY   *typeName)

      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                              typedef retType (_export  N_CDECL    *typeName)

      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                              typedef retType (_export  N_PASCAL   *typeName)

      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                              typedef retType (                    *typeName)

      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                              typedef retType (         N_CDECL    *typeName)

      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                              typedef retType (         N_PASCAL   *typeName)

   #elif defined( N_PLAT_DOS )

      #define N_GLOBAL_LIBRARY_PAS( retType )    retType N_FAR N_PASCAL
      #define N_EXTERN_LIBRARY_PAS( retType ) \
                                       _N_EXTERN retType N_FAR N_PASCAL
      #define N_GLOBAL_CALLBACK( retType )       retType N_FAR N_CALLBACK _export
      #define N_EXTERN_CALLBACK( retType ) \
                                       _N_EXTERN retType N_FAR N_CALLBACK _export
      #define N_GLOBAL_CALLBACK_C( retType )     retType N_FAR N_CDECL    _export
      #define N_EXTERN_CALLBACK_C( retType ) \
                                       _N_EXTERN retType N_FAR N_CDECL    _export
      #define N_GLOBAL_CALLBACK_PAS( retType )   retType N_FAR N_PASCAL   _export
      #define N_EXTERN_CALLBACK_PAS( retType ) \
                                       _N_EXTERN retType N_FAR N_PASCAL   _export

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                            typedef retType (                          *typeName)

      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                            typedef retType (       N_CDECL            *typeName)

      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                            typedef retType (       N_PASCAL           *typeName)

      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                            typedef retType (N_FAR  N_PASCAL           *typeName)

      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                            typedef retType (N_FAR                     *typeName)

      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                            typedef retType (N_FAR  N_PASCAL           *typeName)

      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                            typedef retType (N_FAR  N_CALLBACK _export *typeName)

      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                            typedef retType (N_FAR  N_CDECL    _export *typeName)

      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                            typedef retType (N_FAR  N_PASCAL   _export *typeName)

      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                            typedef retType (N_NEAR                    *typeName)

      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                            typedef retType (N_NEAR N_CDECL            *typeName)

      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                            typedef retType (N_NEAR  N_PASCAL          *typeName)

   #elif defined( N_PLAT_NLM )

      #define N_GLOBAL_LIBRARY_PAS( retType ) \
                                                 retType N_FAR N_PASCAL
      #define N_EXTERN_LIBRARY_PAS( retType ) \
                                       _N_EXTERN retType N_FAR N_PASCAL
      #define N_GLOBAL_CALLBACK( retType )       retType N_FAR 
      #define N_EXTERN_CALLBACK( retType ) \
                                       _N_EXTERN retType N_FAR 
      #define N_GLOBAL_CALLBACK_C( retType )     retType N_FAR N_CDECL
      #define N_EXTERN_CALLBACK_C( retType ) \
                                       _N_EXTERN retType N_FAR N_CDECL
      #define N_GLOBAL_CALLBACK_PASCAL( retType ) \
                                                 retType N_FAR N_PASCAL
      #define N_EXTERN_CALLBACK_PAS( retType ) \
                                       _N_EXTERN retType N_FAR N_PASCAL

      #define N_TYPEDEF_FUNC( retType, typeName ) \
                            typedef retType (            *typeName)
      #define N_TYPEDEF_FUNC_C( retType, typeName ) \
                            typedef retType (N_CDECL     *typeName)
      #define N_TYPEDEF_FUNC_PAS( retType, typeName ) \
                            typedef retType (N_PASCAL    *typeName)
      #define N_TYPEDEF_LIBRARY( retType, typeName ) \
                            typedef retType (            *typeName)
      #define N_TYPEDEF_LIBRARY_C( retType, typeName ) \
                            typedef retType (N_CDECL     *typeName)
      #define N_TYPEDEF_LIBRARY_PAS( retType, typeName ) \
                            typedef retType (N_PASCAL    *typeName)
      #define N_TYPEDEF_CALLBACK( retType, typeName ) \
                            typedef retType (N_CALLBACK  *typeName)
      #define N_TYPEDEF_CALLBACK_C( retType, typeName ) \
                            typedef retType (N_CDECL     *typeName)
      #define N_TYPEDEF_CALLBACK_PAS( retType, typeName ) \
                            typedef retType (N_PASCAL    *typeName)
      #define N_TYPEDEF_INTERN_FUNC( retType, typeName ) \
                            typedef retType (            *typeName)
      #define N_TYPEDEF_INTERN_FUNC_C( retType, typeName ) \
                            typedef retType (N_CDECL     *typeName)
      #define N_TYPEDEF_INTERN_FUNC_PAS( retType, typeName ) \
                            typedef retType (N_PASCAL    *typeName)

   #endif /* N_PLAT_NLM */

   /*
      Untyped logical macros.
   */

   #define NMin(a,b)               ((a) < (b) ? (a) : (b))
   #define NMax(a,b)               ((a) > (b) ? (a) : (b))

   /*
      Integer construction macros.
   */

   /* NMakePtrParam is defined here since it can't be used in a resource file */

   #define NMakePtrParam(p)    ((nparam) ((nptr) (p)))

#endif /* ! defined(N_RC_INVOKED) */

#define NMake32(lo,hi)     ((nuint32) (((nuint16)(lo)) | \
                             (((nuint32)(nuint16)(hi)) << 16)))
#define NGetLo16(a32)      ((nuint16)((nuint32)(a32) & 0xFFFF))
#define NGetHi16(a32)      ((nuint16)((nuint32)(a32) >> 16))
#define NMake16(lo,hi)     ((nuint16) (((nuint8)(lo)) | \
                            (((nuint16)((nuint8)(hi))) << 8)))
#define NGetLo8(a16)       ((nuint8)((nuint16)(a16) & 0xFF))
#define NGetHi8(a16)       ((nuint8)((nuint16)(a16) >> 8))


#define NMakePair32         NMake32
#define NGetFirst16(a)      ((nint16) NGetLo16(a))
#define NGetSecond16(a)     ((nint16) NGetHi16(a))

#define NMakePair16         NMake16
#define NGetFirst8(a)       ((nint8) NGetLo8(a))
#define NGetSecond8(a)      ((nint8) NGetHi8(a))

#define NMakeFixed32(i,f)   NMake32(f,i)
#define NGetFixedInt16(a)   ((nint16) NGetHi16(a))
#define NGetFixedFrac16(a)  ((nint16) NGetLo16(a))

/* Swap, Copy, Get and Set macros */

#define NSwap32( x ) ((nuint32)            \
   ((((nuint32)(x) & 0x000000FFL) << 24) | \
    (((nuint32)(x) & 0x0000FF00L) <<  8) | \
    (((nuint32)(x) & 0x00FF0000L) >>  8) | \
    (((nuint32)(x) & 0xFF000000L) >> 24)))

#define NSwap16( x ) ((nuint16)            \
   ((((nuint16)(x) & 0x00FF) << 8)       | \
    (((nuint16)(x) & 0xFF00) >> 8)))

#define NCopySwap32( pDest, pSrc )                  \
   (((pnuint8) (pDest))[0] = ((pnuint8) (pSrc))[3], \
    ((pnuint8) (pDest))[1] = ((pnuint8) (pSrc))[2], \
    ((pnuint8) (pDest))[2] = ((pnuint8) (pSrc))[1], \
    ((pnuint8) (pDest))[3] = ((pnuint8) (pSrc))[0])

#define NCopySwap16( pDest, pSrc )                  \
   (((pnuint8) (pDest))[0] = ((pnuint8) (pSrc))[1], \
    ((pnuint8) (pDest))[1] = ((pnuint8) (pSrc))[0])

#if defined( N_INT_ENDIAN_HI_LO )

   #define NSwapHiLo32( x )      ((nuint32) (x))
   #define NSwapHiLo16( x )      ((nuint16) (x))
   #define NSwapLoHi32           NSwap32
   #define NSwapLoHi16           NSwap16

#else

   #define NSwapHiLo32           NSwap32
   #define NSwapHiLo16           NSwap16
   #define NSwapLoHi32( x )      ((nuint32) (x))
   #define NSwapLoHi16( x )      ((nuint16) (x))

#endif

#define NSwapToHiLo32            NSwapHiLo32
#define NSwapToHiLo16            NSwapHiLo16
#define NSwapToLoHi32            NSwapLoHi32
#define NSwapToLoHi16            NSwapLoHi16

#define NSwapFromHiLo32          NSwapHiLo32
#define NSwapFromHiLo16          NSwapHiLo16
#define NSwapFromLoHi32          NSwapLoHi32
#define NSwapFromLoHi16          NSwapLoHi16

#if defined( N_INT_STRICT_ALIGNMENT )

   #define NCopy32( pDest, pSrc )                      \
      (((pnuint8) (pDest))[0] = ((pnuint8) (pSrc))[0], \
       ((pnuint8) (pDest))[1] = ((pnuint8) (pSrc))[1], \
       ((pnuint8) (pDest))[2] = ((pnuint8) (pSrc))[2], \
       ((pnuint8) (pDest))[3] = ((pnuint8) (pSrc))[3])

   #define NCopy16( pDest, pSrc )                      \
      (((pnuint8) (pDest))[0] = ((pnuint8) (pSrc))[0], \
       ((pnuint8) (pDest))[1] = ((pnuint8) (pSrc))[1])

   #define NSet32( pDest, src ) NCopy32(pDest, &(src))

   #define NSet16( pDest, src ) NCopy16(pDest, &(src))

   #if defined( N_INT_ENDIAN_HI_LO )

      #define NGet32( pSrc )                        \
         ((nuint32)(((pnuint8) (pSrc))[3])        | \
         ((nuint32)(((pnuint8) (pSrc))[2]) << 8)  | \
         ((nuint32)(((pnuint8) (pSrc))[1]) << 16) | \
         ((nuint32)(((pnuint8) (pSrc))[0]) << 24))

      #define NGet16( pSrc )                        \
         ((nuint16)(((pnuint8) (pSrc))[1])        | \
         ((nuint16)(((pnuint8) (pSrc))[0]) << 8))

   #else

      #define NGet32( pSrc )                        \
         ((nuint32)(((pnuint8) (pSrc))[0])        | \
         ((nuint32)(((pnuint8) (pSrc))[1]) << 8)  | \
         ((nuint32)(((pnuint8) (pSrc))[2]) << 16) | \
         ((nuint32)(((pnuint8) (pSrc))[3]) << 24))

      #define NGet16( pSrc )                        \
         ((nuint16)(((pnuint8) (pSrc))[0])        | \
         ((nuint16)(((pnuint8) (pSrc))[1]) << 8))

   #endif

   #if defined( N_INT_ENDIAN_HI_LO )

      #define NWrite32( dest, src )                        \
         (((nuint8 *)(&(dest)))[0] = (nuint32)(src) >> 24, \
          ((nuint8 *)(&(dest)))[1] = (nuint32)(src) >> 16, \
          ((nuint8 *)(&(dest)))[2] = (nuint32)(src) >>  8, \
          ((nuint8 *)(&(dest)))[3] = (nuint32)(src))

      #define NWrite16( dest, src )                        \
         (((nuint8 *)(&(dest)))[0] = (nuint16)(src) >> 8,  \
          ((nuint8 *)(&(dest)))[1] = (nuint16)(src))

   #else

      #define NWrite32( dest, src )                        \
         (((nuint8 *)(&(dest)))[3] = (nuint32)(src) >> 24, \
          ((nuint8 *)(&(dest)))[2] = (nuint32)(src) >> 16, \
          ((nuint8 *)(&(dest)))[1] = (nuint32)(src) >>  8, \
          ((nuint8 *)(&(dest)))[0] = (nuint32)(src))

      #define NWrite16( dest, src )                        \
         (((nuint8 *)(&(dest)))[1] = (nuint16)(src) >> 8,  \
          ((nuint8 *)(&(dest)))[0] = ((nuint16)(src)))

   #endif

#else /* if defined( N_INT_STRICT_ALIGNMENT ) */

   #define NCopy32( pDest, pSrc )   \
      (*((pnuint32) (pDest)) = *((pnuint32) (pSrc)))

   #define NCopy16( pDest, pSrc )   \
      (*((pnuint16) (pDest)) = *((pnuint16) (pSrc)))

   #define NSet32( pDest, src ) \
      (*((pnuint32) (pDest)) = (nuint32)(src))

   #define NSet16( pDest, src ) \
      (*((pnuint16) (pDest)) = (nuint16)(src))

   #define NGet32( pSrc ) \
      (*((pnuint32) (pSrc)))

   #define NGet16( pSrc ) \
      (*((pnuint16) (pSrc)))

   #define NWrite32( dest, src ) NSet32(&(dest), src)

   #define NWrite16( dest, src ) NSet16(&(dest), src)

#endif /* if defined( N_INT_STRICT_ALIGNMENT ) */

#if defined( N_INT_ENDIAN_HI_LO )

   #define NGetHiLo32  NGet32
                            
   #define NGetHiLo16  NGet16

   #define NGetLoHi32( pSrc )          \
      (((pnuint8) (pSrc))[0]         | \
      (((pnuint8) (pSrc))[1] << 8)   | \
      (((pnuint8) (pSrc))[2] << 16)  | \
      (((pnuint8) (pSrc))[3] << 24))

   #define NGetLoHi16( pSrc )          \
      (((pnuint8) (pSrc))[0]         | \
      (((pnuint8) (pSrc))[1] << 8))

#else

   #define NGetHiLo32( pSrc )          \
      (((pnuint8) (pSrc))[3]         | \
      (((pnuint8) (pSrc))[2] << 8)   | \
      (((pnuint8) (pSrc))[1] << 16)  | \
      (((pnuint8) (pSrc))[0] << 24)) 

   #define NGetHiLo16( pSrc )          \
      (((pnuint8) (pSrc))[1]         | \
      (((pnuint8) (pSrc))[0] << 8))

   #define NGetLoHi32  NGet32

   #define NGetLoHi16  NGet16

#endif

#if defined( N_INT_ENDIAN_HI_LO )

   #define NSetHiLo32  NSet32

   #define NSetHiLo16  NSet16

   #define NSetLoHi32( pDest, src )                   \
      (((nuint8 *)(pDest))[3] = (nuint32)(src) >> 24, \
       ((nuint8 *)(pDest))[2] = (nuint32)(src) >> 16, \
       ((nuint8 *)(pDest))[1] = (nuint32)(src) >>  8, \
       ((nuint8 *)(pDest))[0] = (nuint32)(src))

   #define NSetLoHi16( pDest, src )                   \
      (((nuint8 *)(pDest))[1] = (nuint16)(src) >> 8,  \
       ((nuint8 *)(pDest))[0] = ((nuint16)(src)))

#else

   #define NSetHiLo32( pDest, src )                   \
      (((nuint8 *)(pDest))[0] = (nuint32)(src) >> 24, \
       ((nuint8 *)(pDest))[1] = (nuint32)(src) >> 16, \
       ((nuint8 *)(pDest))[2] = (nuint32)(src) >>  8, \
       ((nuint8 *)(pDest))[3] = (nuint32)(src))

   #define NSetHiLo16( pDest, src )                   \
      (((nuint8 *)(pDest))[0] = (nuint16)(src) >> 8,  \
       ((nuint8 *)(pDest))[1] = ((nuint16)(src)))

   #define NSetLoHi32  NSet32

   #define NSetLoHi16  NSet16

#endif

#define NRead32( src )            NGet32(&(src))
                                  
#define NRead16( src )            NGet16(&(src))

#define NReadHiLo32( src )        NGetHiLo32(&(src))

#define NReadHiLo16( src )        NGetHiLo16(&(src))

#define NReadLoHi32( src )        NGetLoHi32(&(src))

#define NReadLoHi16( src )        NGetLoHi16(&(src))

#define NWriteHiLo32( dest, src ) NSetHiLo32(&(dest), src)

#define NWriteHiLo16( dest, src ) NSetHiLo16(&(dest), src)

#define NWriteLoHi32( dest, src ) NSetLoHi32(&(dest), src)

#define NWriteLoHi16( dest, src ) NSetLoHi16(&(dest), src)

#if defined( N_INT_ENDIAN_HI_LO )

   /* Copy from native format to named format */
   #define NCopyHiLo32        NCopy32
   #define NCopyHiLo16        NCopy16
   #define NCopyLoHi32        NCopySwap32
   #define NCopyLoHi16        NCopySwap16

#else

   /* Copy from native format to named format */
   #define NCopyHiLo32        NCopySwap32
   #define NCopyHiLo16        NCopySwap16
   #define NCopyLoHi32        NCopy32
   #define NCopyLoHi16        NCopy16

#endif

#define NCopyToHiLo32      NCopyHiLo32        
#define NCopyToHiLo16      NCopyHiLo16        
#define NCopyToLoHi32      NCopyLoHi32        
#define NCopyToLoHi16      NCopyLoHi16        

/* Copy from named format to native format */
#define NCopyFromHiLo32    NCopyHiLo32        
#define NCopyFromHiLo16    NCopyHiLo16        
#define NCopyFromLoHi32    NCopyLoHi32        
#define NCopyFromLoHi16    NCopyLoHi16        

#define NPad32( src )      (((src) + 3) & ~3)
#define NPad16( src )      (((src) + 1) & ~1)

#define NAlign32(src)  *(src) = (void N_FAR *) (((nuint32)*(src) + 3) & ~3);

/*
   Character and String macros.
*/

#if defined(N_USE_STR_16)
   #define NText(a)        L a
#else
   #define NText(a)        a
#endif

#if defined( N_PLAT_MSW ) || defined( N_PLAT_OS2 ) || defined( N_PLAT_DOS )
   #define N_NEWLINE       NText( "\r\n" )
#elif defined( N_PLAT_MAC )
   #if defined( THINK_C ) || defined( __MWERKS__ )
      #define N_NEWLINE               NText( "\r" )
   #else
      #define N_NEWLINE               NText( "\n" )
   #endif
#else
   #define N_NEWLINE       NText( "\n" )
#endif

/*
   Historical macro synonyms.
*/

#if ! defined( N_INC_NO_OLD_MACROS ) 
   #if !defined( FAR )
      #define FAR         N_FAR
   #endif
   #if !defined( NEAR )
      #define NEAR        N_NEAR
   #endif
   #if !defined( MIN )
      #define MIN         NMin
   #endif
   #if !defined( MAX )
      #define MAX         NMax
   #endif
   #if !defined( MAKELONG )
      #define MAKELONG    NMake32
   #endif
   #if !defined( HIWORD )
      #define HIWORD      NGetHi16
   #endif
   #if !defined( LOWORD )
      #define LOWORD      NGetLo16
   #endif
   #if !defined( MAKEWORD )
      #define MAKEWORD    NMake16
   #endif
   #if !defined( HIBYTE )
      #define HIBYTE      NGetHi8
   #endif
   #if !defined( LOBYTE )
      #define LOBYTE      NGetLo8
   #endif
#endif

#endif /* ! defined( NTYPES_H ) */
