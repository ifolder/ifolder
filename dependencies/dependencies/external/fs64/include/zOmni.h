/****************************************************************************
 | Copyright (c) 2001 Novell, Inc. All Rights Reserved.
 |
 | This work is subject to U.S. and international copyright laws and
 | treaties. Use and redistribution of this work is subject to the
 | license agreement accompanying the software development kit (SDK)
 | that contains this work.  Pursuant to the SDK license agreement,
 | Novell hereby grants to developer a royalty-free, non-exclusive
 | license to include Novell's sample code in its product. Novell
 | grants developer worldwide distribution rights to market, distribute,
 | or sell Novell's sample code as a component of developer's products.
 | Novell shall have no obligations to developer or developer's customers
 | with respect to this code.
 |
 |***************************************************************************
 |
 |	 NetWare Advance File Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   16 Jan 2003 17:06:40  $
 |
 | $Workfile:   zOmni.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		This is the root include file for NSS that exists inside the SDK
 +-------------------------------------------------------------------------*/
#ifndef _ZOMNI_H_
#define _ZOMNI_H_


#ifdef __cplusplus
extern "C" {
#endif

#include <dirent.h>

#ifdef N_PLAT_GNU
#include <wchar.h>
#endif

#pragma pack(push,1)
/*-------------------------------------------------------------------------
 * Used to enable/disable various pieces of code.  These macros have
 * the advantage over normal ifdefs in that you have to either
 * specifically enable or disable the code, otherwise, you get a
 * syntax error from the preprocessor.
 *
 * Example: (see que.h for an actual case)
 *
 *	#define FEATURE ENABLE
 *	#if FEATURE IS_ENABLED
 *		... code ...
 *	#endif
 *-------------------------------------------------------------------------*/
#define ENABLE		(1
#define DISABLE		(0
#define IS_ENABLED	==1)
#define IS_DISABLED	==0)

/*---------------------------------------------------------------------------
 *	Global settings that are independent of different builds
 *---------------------------------------------------------------------------*/
#define NSS_64BIT_SUPPORT ENABLE

/*-------------------------------------------------------------------------
 *	Global type definitions
 *-------------------------------------------------------------------------*/
#undef BYTE
#undef WORD
#undef LONG
#undef SLONG
#undef QUAD
#undef ADDR

#define zNILXID	((Xid_t)0)

/*-------------------------------------------------------------------------
 * Typedefs for quads.  Had to add quad_t because UNIX uses _QUAD_T.
 *-------------------------------------------------------------------------*/
#ifndef _QUAD_T
#define _QUAD_T
#	if NSS_64BIT_SUPPORT IS_ENABLED
#		if defined(GNU)
			typedef unsigned long long	QUAD;
			//typedef long long			quad_t;
#		elif defined(__MWERKS__)
			typedef unsigned long long	QUAD;
			typedef long long			quad_t;
#		else
			typedef unsigned __int64	QUAD;	
			typedef __int64				quad_t;
#		endif
#	else
		typedef unsigned int	QUAD;
#	endif
#endif

#ifndef _SQUAD_T
#define _SQUAD_T
#	if NSS_64BIT_SUPPORT IS_ENABLED
#		if defined(GNU)
			typedef long long	SQUAD;
#		elif defined(__MWERKS__)
			typedef long long	SQUAD;
#		else
			typedef __int64 SQUAD;
#		endif
#	else
		typedef signed int	SQUAD;
#	endif
#endif

/*-------------------------------------------------------------------------
 * Typedefs for basic types
 *-------------------------------------------------------------------------*/
#ifndef _BYTE_T
#define _BYTE_T
typedef unsigned char	BYTE;	/*  8 bit unsigned data item	*/
#endif

#ifndef _WORD_T
#define _WORD_T
typedef unsigned short	WORD;	/* 16 bit unsigned data item	*/
#endif

#ifndef _LONG_T
#define _LONG_T
typedef unsigned long	LONG;	/* 32 bit unsigned data item	*/
#endif

#ifndef _NINT_T
#define _NINT_T
typedef unsigned int	NINT;	/* Unsigned native data item	*/
#endif

#ifndef _SBYTE_T
#define _SBYTE_T
typedef signed char		SBYTE;	/*  8 bit signed data item	*/
#endif

#ifndef _SWORD_T
#define _SWORD_T
typedef signed short	SWORD;	/* 16 bit signed data item	*/
#endif

#ifndef _SLONG_T
#define _SLONG_T
typedef signed long		SLONG;	/* 32 bit signed data item	*/
#endif

#ifndef _SNINT_T
#define _SNINT_T
typedef signed int		SNINT;	/* Signed native data item	*/
#endif

#ifndef _ADDR_T
#define _ADDR_T
typedef unsigned long	ADDR;	/* Arithmetic type that can hold a pointer	*/
#endif

#ifndef _SADDR_T
#define _SADDR_T
typedef long			SADDR;	/* Signed Arithmetic type for a pointer	*/
#endif


#ifndef _BOOL_T
#define _BOOL_T
typedef NINT 			BOOL;	/* 0 means FALSE, NON-0 means TRUE*/
#endif

#ifndef _UNICODE_T
#define _UNICODE_T
#if defined(GNU)
			typedef wchar_t unicode_t;
#else
			typedef unsigned short unicode_t; /* A UNICODE (UTF-16) character */
#endif
#endif

#ifndef _UTF8_T
#define _UTF8_T
typedef char utf8_t;	/* a UTF-8 character */
#endif

/*-------------------------------------------------------------------------
 * MAGIC_STRING lets the preprocessor convert an integer to a string
 * MAKE_STRING provides the level indirection need to fake out the
 *		the preprocessor
 * WHERE a string with both the file name and line number concatenated
 *		into a single string
 *-------------------------------------------------------------------------*/
#define MAGIC_STRING(_x_)	# _x_
#define MAKE_STRING(_x_)	MAGIC_STRING(_x_)
#ifdef UNIX
#define WHERE	__FILE__ "[" MAKE_STRING(__LINE__) "]"
#else
#define WHERE	FNAM_ "[" MAKE_STRING(__LINE__) "]"
#endif

/*-------------------------------------------------------------------------
 *	In NSS we have the model that functions do NOT return actual error codes.
 *	They only return zOK and zFAILURE.  The real error status is returned in
 *	the "errno" field of the "GeneralMsg_s" structure.  When looking at the
 *	function return value you must ONLY compare to zOK.  Comparing to anything
 *	else is invalid.
 *-------------------------------------------------------------------------*/
typedef signed int		STATUS;		/* the type for return status values*/
#define zOK				0			/* the operation succeeded.*/
#define zFAILURE		-1			/* the operation failed*/

/*---------------------------------------------------------------------------
 * Number of bits in each of the various base data sizes
 *---------------------------------------------------------------------------*/
#define BITS_PER_BYTE	8
#define BITS_PER_WORD	16
#define BITS_PER_LONG	32
#define BITS_PER_QUAD	64
#define BITS_PER_NINT	32
#define BITS_PER_ADDR	32

typedef QUAD			Zid_t;		/* Znode identifiers*/
typedef struct	GUID_t				/* holds GUIDs*/
{
	LONG	timeLow;
	WORD	timeMid;
	WORD	timeHighAndVersion;
	BYTE	clockSeqHighAndReserved;
	BYTE	clockSeqLow;
	BYTE	node[6];
} GUID_t;
typedef unsigned long	DomainID_t;	/* domain for object ids from directory services */
typedef GUID_t			VolumeID_t;	/* holds a volume ID*/
typedef GUID_t			NDSid_t;	/* NDS 128 bit ID*/
typedef NDSid_t			UserID_t;
typedef SQUAD			Key_t;		/* holds a key */
	/*
	 * External representation of a token.  A token or capability
	 * is a persistent ID for a file, directory, extended attribute, etc.
	 * Even though it is an array, we encapsulate it in a structure
	 * so when we pass it, we can take its address so that it looks
	 * like we are passing by reference.
	 */
#define SIZE_TOKEN		8			/* Size is in QUADs so that it ends up
									 * quad aligned.
									 */
typedef struct Token_t
{
	QUAD	token[SIZE_TOKEN];
} Token_t;

typedef LONG			Time_t;		/* Universal Time Coordinated (UTC)
									 * Time in seconds from January 1, 1970
									 * Time_t is placed on media so it must
									 * be a fixed size (ie LONG (4 bytes)).
									 * When time_t becomes 64 bits we will
									 * have to make changes
									 */
typedef unsigned long	DOSTime_t;	/* Time in DOS format, kept in local time
									 * set to INVALID_TIME if not valid (must
									 * be converted from UTC time).
									 */
typedef SQUAD			MSTime_t;	/* Time in CIFS SMB format, signed 64 bit
									 * value which stores the number of 100ns
									 * intervals from January 1, 1601
									 */

typedef QUAD	Xid_t;		/* transaction ID*/

#ifdef __cplusplus
typedef void (*voidfunc_t)(...);	/* generic void function definition*/
typedef STATUS (*statusfunc_t)(...);/* generic STATUS return value function definitions*/
typedef LONG (*longfunc_t)(...);	/* generic LONG return value function definition*/
typedef BOOL (*boolfunc_t)(...);	/* generic BOOL return value function definition*/
#else
typedef void (*voidfunc_t)();		/* generic void function definition*/
typedef STATUS (*statusfunc_t)();	/* generic STATUS return value function definitions*/
typedef LONG (*longfunc_t)();		/* generic LONG return value function definition*/
typedef BOOL (*boolfunc_t)();		/* generic BOOL return value function definition*/
#endif

/*---------------------------------------------------------------------------
 * Sequence Numbers have the property that they monotonically increase
 * and can be compared even after they wrap.  Zero, 0, has been reserved
 * as invalid.  They are used for transactions and log sequence numbers.
 *
 * Use the following macros to compare Sequence Numbers.
 * They work correctly for numbers that wrap.
 *
 *	SEQ_GT(x, y) is true if x comes after y by less than 0x80000000.
 *		0x80000000 > 0x7fffffff
 *		0x00000001 > 0xffffffff
 *---------------------------------------------------------------------------*/
typedef SLONG	Seq_t;		/* Sequence number */

/*-------------------------------------------------------------------------
 *	This defines the sizes that name strings can be in the system.  Note
 *	that these are the sizes of the buffers that hold these string which
 *	include room for a NULL.  You should compare and individual string length
 *	with one less then these numbers.
 *-------------------------------------------------------------------------*/
#define zMAX_COMPONENT_NAME	256		/* maximum length of an individual component of
									 * a file name string (like a directory/file
									 * name) */

#define zMAX_FULL_NAME		1024	/* maximum length of a fully qualified file 
									 * name string.  This must be
									 * >= zMAX_COMPONENT_NAME */

#ifdef __cplusplus
}
#endif

#pragma pack(pop)

#endif /* _ZOMNI_H_ */
