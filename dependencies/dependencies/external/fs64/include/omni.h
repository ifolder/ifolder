/****************************************************************************
 |
 |	(C) Copyright 1985, 1991, 1993, 1996 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 NetWare Advance File Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   10 Dec 2001 09:56:36  $
 |
 | $Workfile:   omni.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		This is the base INTERNAL include file the contains all of the
 |		base definitions.  This contains information NOT in the SDK.
 |
 | WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING!
 |
 | This header file should ONLY be used for NSS internal development.
 | This includes Semantic Agents (SA) and Loadable Storage Services (LSS).
 | Any other use may cause conflicts which NSS will NOT fix.
 +-------------------------------------------------------------------------*/
#ifndef _OMNI_H_
#define _OMNI_H_

#ifndef _ZOMNI_H_
#include <zOmni.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

/*-------------------------------------------------------------------------
 *	The following conditionals define what OS is being built.
 *		_NWMOAB_
 *		_NWGREENRIVER_
 *	The following turns the language enabling macros on
 *		_LANGUAGEENABLED_
 *-------------------------------------------------------------------------*/

/*---------------------------------------------------------------------------
 *	All of the #DEFINES that control conditional compilation in PSS are now
 *	defined in one place.  All of these conditionals are controlled by the
 *	six main conditionals.  One and only one of these conditionals should
 *	be set to ENABLE.  Both uni and multi processor configurations are defined.
 *---------------------------------------------------------------------------*/
#ifdef UNOPT 
#ifdef MPK
#	define NSS_UNI_OPTIMAL	DISABLE		/* ENABLE for uni-processor production code*/
#	define NSS_UNI_DEBUG	DISABLE //ENABLE		/* ENABLE for development*/
#	define NSS_UNI_PREEMPT	DISABLE		/* ENABLE for uni-proc preemptive env*/
#	define NSS_MPK_OPTIMAL	DISABLE		/* ENABLE for multi-processor production*/
#	define NSS_MPK_DEBUG	ENABLE		/* ENABLE for multi-processor (MP) debugging*/
#	define NSS_MPK_FAKE		DISABLE		/* ENABLE to make a uni-processor fake MP*/
#	define NSS_MCCABE		DISABLE		/* ENABLE to change most macros to functions*/
#else  /*!MPK*/
#	define NSS_UNI_OPTIMAL	DISABLE		/* ENABLE for uni-processor production code*/
#	define NSS_UNI_DEBUG	ENABLE //ENABLE		/* ENABLE for development*/
#	define NSS_UNI_PREEMPT	DISABLE		/* ENABLE for uni-proc preemptive env*/
#	define NSS_MPK_OPTIMAL	DISABLE		/* ENABLE for multi-processor production*/
#	define NSS_MPK_DEBUG	DISABLE		/* ENABLE for multi-processor (MP) debugging*/
#	define NSS_MPK_FAKE		DISABLE		/* ENABLE to make a uni-processor fake MP*/
#	define NSS_MCCABE		DISABLE		/* ENABLE to change most macros to functions*/
#endif /*MPK*/
#else  /*!UNOPT*/
#ifdef MPK
#	define NSS_UNI_OPTIMAL	DISABLE //ENABLE		/* ENABLE for uni-processor production code*/
#	define NSS_UNI_DEBUG	DISABLE		/* ENABLE for development*/
#	define NSS_UNI_PREEMPT	DISABLE		/* ENABLE for uni-proc preemptive env*/
#	define NSS_MPK_OPTIMAL	ENABLE  //DISABLE		/* ENABLE for multi-processor production*/
#	define NSS_MPK_DEBUG	DISABLE		/* ENABLE for multi-processor (MP) debugging*/
#	define NSS_MPK_FAKE		DISABLE		/* ENABLE to make a uni-processor fake MP*/
#	define NSS_MCCABE		DISABLE		/* ENABLE to change most macros to functions*/
#else /*!MPK*/
#	define NSS_UNI_OPTIMAL	ENABLE //ENABLE		/* ENABLE for uni-processor production code*/
#	define NSS_UNI_DEBUG	DISABLE		/* ENABLE for development*/
#	define NSS_UNI_PREEMPT	DISABLE		/* ENABLE for uni-proc preemptive env*/
#	define NSS_MPK_OPTIMAL	DISABLE  //DISABLE		/* ENABLE for multi-processor production*/
#	define NSS_MPK_DEBUG	DISABLE		/* ENABLE for multi-processor (MP) debugging*/
#	define NSS_MPK_FAKE		DISABLE		/* ENABLE to make a uni-processor fake MP*/
#	define NSS_MCCABE		DISABLE		/* ENABLE to change most macros to functions*/
#endif /*MPK*/
#endif /*UNOPT*/


#if NSS_UNI_OPTIMAL IS_ENABLED
		/*** This is the Production/Release options ***/
				/*** Uni-processor only ***/
#	undef  NSS_MCCABE
#	define NSS_MCCABE		DISABLE	/* Make sure NSS_MCCABE options are disabled*/
#	define NSS_DEBUG		DISABLE	/* disable DEBUG code in NSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		DISABLE	/* disable ASSERTS in PSS */
#	define LATCH_MACRO		ENABLE	/* use LATCH macros*/
#	define QUE_CHECK		DISABLE	/* remove QUE debug checks*/
#	define QUE_MACRO		ENABLE	/* use QUE macros*/
#	define MEM_KEEP_LIST	DISABLE	/* don't keep list of allocated memory*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* don't use fake multi-processor code*/
#	define MPK_REAL			DISABLE	/* don't use real multi-processor code*/
#	define HISTOGRAM		DISABLE	/* don't keep histograms*/
#	define LOG_TEST			DISABLE	/* Include undo/redo immediate code.  This
									 * code causes ZLOG to ask for a undo/redo
									 * or redo/undo to be performed while the
									 * system is logging transactions.  The
									 * results are then compared against the
									 * correct answer.  If not correct ASSERTs
									 * are generated.
									 */
#	define ZLOG_DEBUG		DISABLE	/* Include ZLOG debug code.  This code
									 * adds debug information to the ZLOG
									 * Beast and File.  For performance reasons
									 * this should be turned off for release.
									 */
#	define ZLOG_TEST		DISABLE	/* Perform simple ZLOG unit tests.  This
									 * must be turned off for release.  These
									 * Tests are only run if /zlog if specified
									 * when loading the NSS NLM.
									 */
#	define FMAP_TEST		DISABLE	/* Test the filemap code by simulating
									 * large fragmented files.
									 */
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_UNI_DEBUG IS_ENABLED
#	define NSS_DEBUG		ENABLE	/* enable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		ENABLE	/* enable ASSERTS in PSS*/
#	define LATCH_MACRO		DISABLE	/* use LATCH functions*/
#	define QUE_CHECK		ENABLE	/* enable QUE debug checks*/
#	define QUE_MACRO		DISABLE	/* use QUE functions*/
#	define EVENTQ_MACRO		ENABLE	/* use EVENTQ macros*/
#	define MEM_KEEP_LIST	ENABLE	/* keep list of allocated memory blocks*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* don't use fake multi-processor code*/
#	define MPK_REAL			DISABLE	/* don't use real multi-processor code*/
#	define HISTOGRAM		ENABLE	/* keep histograms*/
#	define LOG_TEST			ENABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		ENABLE	/* Include ZLOG debug code*/
#	define ZLOG_TEST		ENABLE	/* Perform simple ZLOG unit tests*/
#	define FMAP_TEST		ENABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_UNI_PREEMPT IS_ENABLED	/* Still need to set flags correctly*/

#	define NSS_DEBUG		ENABLE	/* enable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		ENABLE	/* enable ASSERTS in PSS*/
#	define LATCH_MACRO		DISABLE	/* use LATCH functions*/
#	define QUE_CHECK		ENABLE	/* enable QUE debug checks*/
#	define QUE_MACRO		DISABLE	/* use QUE functions*/
#	define EVENTQ_MACRO		ENABLE	/* use EVENTQ macros*/
#	define MEM_KEEP_LIST	ENABLE	/* keep list of allocated memory blocks*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* don't use fake multi-processor code*/
#	define MPK_REAL			DISABLE	/* don't use real multi-processor code*/
#	define HISTOGRAM		ENABLE	/* keep histograms*/
#	define LOG_TEST			ENABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		ENABLE	/* Include ZLOG debug code*/
#	define ZLOG_TEST		ENABLE	/* Perform simple ZLOG unit tests*/
#	define FMAP_TEST		ENABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_MPK_OPTIMAL IS_ENABLED
		/*** This is the Production/Release options ***/
					/*** MPK only ***/
#	undef  NSS_MCCABE
#	define NSS_MCCABE		DISABLE	/* Make sure NSS_MCCABE options are disabled*/
#	define NSS_DEBUG		DISABLE	/* disable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT	    DISABLE	/* disable ASSERTS in PSS */
#	define LATCH_MACRO		ENABLE	/* use LATCH macros*/
#	define QUE_CHECK		DISABLE	/* remove QUE debug checks*/
#	define QUE_MACRO		ENABLE	/* use QUE macros*/
#	define EVENTQ_MACRO		ENABLE	/* use EVENTQ macros*/
#	define MEM_KEEP_LIST	DISABLE	/* don't keep list of allocated memory*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* don't use fake multi-processor code*/
#	define MPK_REAL			ENABLE	/* use real multi-processor code*/
#	define HISTOGRAM		DISABLE	/* don't keep histograms*/
#	define LOG_TEST			DISABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		DISABLE	/* Include ZLOG debug code*/
#	define ZLOG_TEST		DISABLE	/* Perform simple ZLOG unit tests*/
#	define FMAP_TEST		DISABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_MPK_DEBUG IS_ENABLED
#	define NSS_DEBUG		ENABLE	/* enable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		ENABLE	/* enable ASSERTS in PSS*/
#	define LATCH_MACRO		DISABLE	/* use LATCH functions*/
#	define QUE_CHECK		ENABLE	/* enable QUE debug checks*/
#	define QUE_MACRO		DISABLE	/* use QUE functions*/
#	define EVENTQ_MACRO		ENABLE	/* use EVENTQ macros*/
#	define MEM_KEEP_LIST	ENABLE	/* keep list of allocated memory blocks*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* don't use fake multi-processor code*/
#	define MPK_REAL			ENABLE	/* use real multi-processor code*/
#	define HISTOGRAM		ENABLE	/* keep histograms*/
#	define LOG_TEST			ENABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		ENABLE	/* Include ZLOG debug code*/
#	define ZLOG_TEST		ENABLE	/* Perform simple ZLOG unit tests*/
#	define FMAP_TEST		ENABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_MPK_FAKE IS_ENABLED
#	define NSS_DEBUG		ENABLE	/* enable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		ENABLE	/* enable ASSERTS in PSS*/
#	define LATCH_MACRO		DISABLE	/* use LATCH functions*/
#	define QUE_CHECK		ENABLE	/* enable QUE debug checks*/
#	define QUE_MACRO		DISABLE	/* use QUE functions*/
#	define EVENTQ_MACRO		ENABLE	/* use EVENTQ macros*/
#	define MEM_KEEP_LIST	ENABLE	/* keep list of allocated memory blocks*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			ENABLE	/* use fake multi-processor code*/
#	define MPK_REAL			DISABLE	/* don't use real multi-processor code*/
#	define HISTOGRAM		ENABLE	/* keep histograms*/
#	define LOG_TEST			ENABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		ENABLE	/* Include ZLOG debug code*/
#	define ZLOG_TEST		ENABLE	/* Perform simple ZLOG unit tests*/
#	define FMAP_TEST		ENABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#elif NSS_MCCABE IS_ENABLED
#	define NSS_DEBUG		DISABLE	/* disable DEBUG code in PSS*/
#	define NSS_DEBUG_OPT	ENABLE	/* ENABLE for debugging optimised code*/
#	define NSS_ASSERT		DISABLE	/* disable ASSERTS in PSS*/
#	define LATCH_MACRO		DISABLE	/* use LATCH functions*/
#	define QUE_CHECK		DISABLE	/* remove QUE debug checks*/
#	define QUE_MACRO		DISABLE	/* use QUE functions*/
#	define EVENTQ_MACRO		DISABLE	/* use EVENTQ functions*/
#	define MEM_KEEP_LIST	DISABLE	/* don't keep list of allocated memory*/
#	define ZTREE_DEBUG		DISABLE	/* use only to debug ztree code*/
#	define MPK_FAKE			DISABLE	/* ignore multi-processor code*/
#	define MPK_REAL			DISABLE	/* ignore multi-processor code*/
#	define HISTOGRAM		DISABLE	/* don't keep histograms*/
#	define LOG_TEST			DISABLE	/* Include undo/redo immediate code*/
#	define ZLOG_DEBUG		DISABLE	/* don't include ZLOG debug code*/
#	define ZLOG_TEST		DISABLE	/* don't perform simple ZLOG unit tests*/
#	define FMAP_TEST		DISABLE	/* Perform Filemap Btree tests*/
#	define BLKNUM_64		DISABLE	/* Use 64 bit block numbers*/

#else
#error "One and only one configuration should be enabled"
#endif


/*---------------------------------------------------------------------------
 *	Global settings that are independent of different builds
 *---------------------------------------------------------------------------*/
#define _FSHOOKS	ENABLE

#ifdef _LANGUAGEENABLED_
#	define NSS_MSG_TAGS ENABLE
#else
#	define NSS_MSG_TAGS DISABLE
#endif

/*-------------------------------------------------------------------------
 *	Global type definitions
 *-------------------------------------------------------------------------*/

#undef TRUE
#undef FALSE
#undef NULL
#undef EOF
#undef OK
#undef FAILURE
#undef SUCCESS
#undef zMIN
#undef zMAX
#undef SET_BIT
#undef CLR_BIT
#undef TST_BIT
#undef ALIGN
#undef UNUSED_PARAM
#undef PAGE_SHIFT
#undef PAGE_SIZE
#undef STATIC
#undef CONST

#define TRUE	1	/* Though TRUE is really non-zero, we use this
					 * definition for pseudo boolean expressions
					 */
#define FALSE	0

#define NULL	0
#define EOF		-1

#if NSS_DEBUG IS_ENABLED
#define STATIC		/* don't really do "static" for now (messes up debugger)*/
#else
#define STATIC		/* Static makes it hard to profile code */
#endif

#define CONST		/* don't really do "const" for now (it messes up the src debugger)*/
#define UNUSED_PARAM(_x) (_x = _x)


#define QUAD_GT_LONG(quadValue) (quadValue > 0xffffffff)

/*-------------------------------------------------------------------------
 * Typedefs for basic types
 *-------------------------------------------------------------------------*/

#ifndef unisizeof
#	define unisizeof(buf) (sizeof((buf))/sizeof(unicode_t))	/* get size of buffer in UNICODE chars*/
#endif

#ifndef _CFS_T
#define _CFS_T
typedef NINT			CFS_t;	/* holds the Compressed File Size (CFS)*/
#endif

#ifndef _SIZE_T
#	include <size_t.h>
#endif

#if BLKNUM_64 IS_ENABLED
	typedef SQUAD	Blknum_t;	/* Block numbers */
#else
	typedef SLONG	Blknum_t;	/* Block numbers */
#endif
typedef Blknum_t	Blkcnt_t;	/* Used to hold a count of block numbers */

typedef char	ObjName_t[16];		/* Used for name field in objects for debugging*/

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
#define SEQ_GT(_x, _y)	(((Seq_t)((_x) - (_y))) > 0)
#define SEQ_LT(_x, _y)	(((Seq_t)((_x) - (_y))) < 0)
#define SEQ_GE(_x, _y)	(((Seq_t)((_x) - (_y))) >= 0)
#define SEQ_LE(_x, _y)	(((Seq_t)((_x) - (_y))) <= 0)

#define SEQ_INVALID		0
#define SEQ_INC(_x)		((++(_x) == SEQ_INVALID) ? ((_x) = SEQ_INVALID + 1) : (_x))

/*---------------------------------------------------------------------------
 * Log sequence numbers are defined to never repeat
 *---------------------------------------------------------------------------*/

typedef QUAD	Lsn_t;

#define MAX_LSN		0xffffffffffffffff	/* All LSNs will be less than
										 * this (~(QUAD)0)
										 */

/*---------------------------------------------------------------------------
 * Event counters are synchronization primitives.  They can only be incremented
 * (never decremented).  They are typically used in pairs, the reader incrementing
 * one and the other incremented by the writer.  They rely on atomic writes
 * and if used with multiple readers/writers will need atomic increment.
 * Even counters must be aligned on SNINT boundaries.  Event counters use the
 * same primitives for comparisons has sequence numbers but increment is
 * the same as other integers unless we are in a multi-reader/writer environment
 * where we will need an atomic increment.  The first environment we are looking
 * at is for a single reader/writer.
 *---------------------------------------------------------------------------*/
typedef struct EventCounter_s
{
	Seq_t	in;		/* The event counter incremented when entering a
					 * critical section.
					 */
	Seq_t	out;	/* Event counter incremented to synchronize */
} EventCounter_s;

#define EV_INIT(_x)	((_x).in = (_x).out = 1)
#define EV_IN(_x)	(++((_x).in))
#define EV_OUT(_x)	(++((_x).out))
#define EV_EQ(_x)	((_x).in == (_x).out)
#define EV_NE(_x)	((_x).in != (_x).out)
#define EV_CHG(_x)	(((_x).in - (_x).out) == 1)	/* If after doing an EV_IN
												 * this relation is still
												 * true, then we went from
												 * the empty to the not
												 * empty state and may
												 * want to do something
												 * about it.
												 * Could use == 0 if done
												 * before operation.
												 */

#define EV_GT(_x, _y)	SEQ_GT(_x, _y)
#define EV_LT(_x, _y)	SEQ_LT(_x, _y)
#define EV_GE(_x, _y)	SEQ_GE(_x, _y)
#define EV_LE(_x, _y)	SEQ_LE(_x, _y)


/*-------------------------------------------------------------------------
 * Name_t is a standard way of assigning a name to an entity or object.
 * The name is guaranteed to be null terminated.  Need to include <string.h>
 * to use COPY_NAME.
 *-------------------------------------------------------------------------*/
#define MAX_NAME	16

typedef char	Name_t[MAX_NAME];

#define COPY_NAME(_target, _name)					\
{													\
	strncpy((_target), (_name), MAX_NAME);			\
	(_target)[MAX_NAME-1] = '\0';					\
}


/*-------------------------------------------------------------------------
 * Macros for finding min and max.  Be careful that the values plugged in
 * for x and y do not have any side affects.
 *-------------------------------------------------------------------------*/
#define zMIN(_x_, _y_)			(((_x_) < (_y_)) ? (_x_) : (_y_))
#define zMAX(_x_, _y_)			(((_x_) > (_y_)) ? (_x_) : (_y_))

#define ALIGN(_x_, _p_)			(((_x_) + (_p_) - 1) & ~((_p_) - 1))
#define FLOOR(_x_, _p_)			((_x_) - ((_x_) % (_p_)))
#define FLOOR2(_x_, _shift_)	(((_x_) >> (_shift_)) << (_shift_))
#define CEILING(_x_, _p_)		(FLOOR((_x_) - 1 + (_p_), (_p_)))
#define MASK(startbit, numbits)	(((1 << (numbits)) - 1) << (startbit))

/*-------------------------------------------------------------------------
 * Macro for determining number of elements in an array.  Can only
 * be used for 'local' arrays.  Arrays that are only known by the
 * EXTERN have an unknown sizeof.
 *-------------------------------------------------------------------------*/
#define	NELEMS(_array)			( sizeof(_array) / sizeof(_array[0]) )

/*-------------------------------------------------------------------------
 * Bitmap manipulation instructions.  The macros take a bit vector that
 * is composed of an array of NINTs for the first argument and a bit
 * number in that array for the second argument.
 *	TST_BIT	tests if the bit has been set
 *	SET_BIT	sets the specified bit
 *	CLR_BIT	clears the specified bit
 *	XOT_BIT	complements the specified bit
 *-------------------------------------------------------------------------*/
#define NINTBITS		5	/* log 2 bits per NINT */
#if (BITS_PER_NINT != (1 << NINTBITS))
#error "BITS_PER_NINT doesn't match NINTBITS"
#endif
#define NINTMASK		(BITS_PER_NINT - 1)

#define TST_BIT(_v_, _n_)	((((NINT *)(_v_))[(_n_)>>NINTBITS])	\
								& (1<<((_n_) & NINTMASK)))

#define SET_BIT(_v_, _n_)	((((NINT *)(_v_))[(_n_)>>NINTBITS])	\
								|= (1<<((_n_) & NINTMASK)))

#define CLR_BIT(_v_, _n_)	((((NINT *)(_v_))[(_n_)>>NINTBITS])	\
								&= (~(1<<((_n_) & NINTMASK))))

#define XOR_BIT(_v_, _n_)	((((NINT *)(_v_))[(_n_)>>NINTBITS])	\
								^= (1<<((_n_) & NINTMASK)))

#define SIZE_BIT_MAP(_n_)	(CEILING(_n_, BITS_PER_NINT)/BITS_PER_NINT)

/*-------------------------------------------------------------------------
 * Macro for placing assertions in your code.
 *	ASSERT - if the expression is not true, AssertError is called which
 *				stops the system.
 *	WARN - same as assert but just prints a warning and only prints that
 *			warning once.  Useful for problems that need to be fixed but
 *			are not fatal at this time.  Unlike ASSERT, WARN can not be
 *			used in an expression.
 *------------------------------------------------------------------------*/
extern int DBG_AssertError(char *);
extern int DBG_AssertError_MP(char *);
extern int DBG_AssertErrorStub(void);
extern int DBG_AssertWarning(char *, int *);
extern void EnterDebugger(void);
extern int DBG_DebugBreak;

#if NSS_ASSERT IS_ENABLED

#define ASSERT(_e_)														\
	((void)((_e_) || (DBG_AssertError(WHERE " (" # _e_ ")"),			\
	(DBG_DebugBreak && (EnterDebugger(), DBG_AssertErrorStub())))))

#define ASSERTMP(_e_)													\
	((void)((_e_) || (DBG_AssertError_MP(WHERE " (" # _e_ ")"),			\
	(DBG_DebugBreak && (EnterDebugger(), DBG_AssertErrorStub())))))

#define WARN(_e_)														\
{																		\
	static int	numWarnings = 0;										\
	((void)((_e_) || DBG_AssertWarning(WHERE " (" # _e_ ")", &numWarnings))); \
}

#else

#define ASSERT(_e_)		((void) 0)
#define ASSERTMP(_e_)	((void) 0)
#define WARN(_e_)		((void) 0)

#endif

/*-------------------------------------------------------------------------
 *	This macro will ASSERT if interrupts are not enabled.
 *-------------------------------------------------------------------------*/
#if ((MPK_REAL IS_DISABLED) && (NSS_DEBUG IS_ENABLED)) 	
	#define CHECK_INTERRUPTS()	ASSERT(IntsEnabled());
#else
	#define CHECK_INTERRUPTS()
#endif
/*---------------------------------------------------------------------------
 *	This is a special macro we use to make variable length arrays visible
 *	in the source level debugger.
 *---------------------------------------------------------------------------*/
#if NSS_DEBUG IS_ENABLED
	#define DEBUG_VISIBLE_IDX	128
	#define DEBUG_VISIBLE_VALUE	128
#else
	#define DEBUG_VISIBLE_IDX
	#define DEBUG_VISIBLE_VALUE	0
#endif

/*-------------------------------------------------------------------------
 *	Page size definitions
 *-------------------------------------------------------------------------*/
#define PAGE_SHIFT		12		/* this is for 4K pages*/
#define PAGE_SIZE		(1<<PAGE_SHIFT)
#define PAGE_MASK		(PAGE_SIZE-1)
#define PAGES_PER_PAGE	1		/* defines how many real pages are in one of our pages*/


/*-------------------------------------------------------------------------
 *	This contains all of the macros necessary to support Novell's Language
 *	enabling model.  This model has utilities that scan the source files
 *	and update msg macros of the syntax:
 *			xxxxxxMSG("string",idx)
 *	where "xxxxxx" may be anything or not there.
 *
 *	These utilities put the "string" information into an external database
 *	and then update the source code with the proper IDX value to access that
 *	string.  This is all well defined and wellknown constructs.
 *
 *	You should use the following macros for the following purposses:
 *		MSG(s,id)		for strings you want to be translated
 *		MSGNot(s)		for strings you do NOT want to be translated
 *		MSGNew(s,id)		for new strings to be translated
 *		MSGChg(s,id)		for changed strings to be translated
 *
 *	We also support msg enabled strings in structures inited an compile time.
 *	You must used the "LangEnabledStruct_s" where you want one of the strings
 *	and then use the following macros to set and access the values:
 *		StructMSG(s,id)		for strings you want to be translated
 *		StructMSGNot(s)		for strings you do NOT want to be translated
 *		StructMSGNull()		when you want to assign a NULL string to this field
 *		StructGetMSGStr(les) use this to extract the correct string out of the structure
 *-------------------------------------------------------------------------*/
/* Strings Tagged? */
#undef MSG
#undef InxMSG
#undef TxtMSG

extern char **NSSMessageTable;	/* the message table for NSS */

/* Structure used to supported compile time inited structure that contain
 * languaged enabled strings */
typedef struct LangEnabledStruct_s
{
	NINT le_idx;			/* contains language enabled index */
	char *le_str;			/* contains pointer to string */
} LangEnabledStruct_s;



#if NSS_MSG_TAGS IS_ENABLED /* Language Enabling turned ON */
	/*
	 * have a language enabled string
	 */
#if !defined(USER_JOWENS) && !defined(USER_GPACHNER) && !defined(USER_GPACHNER2)	/* This allows us to skip message updates in debug builds */
#	define MSG(s,id)		(NSSMessageTable[(id)])
#else
#	define MSG(s,id)		s
#endif
	/*
	 * have a string to NOT language enable
	 */
#	define MSGNot(s)		s
#	define MSGNew(s,id)		id ? " msg # must be zero " : s
#	define MSGChg(s,id)		id ? s : " msg # must be non-zero "

#	define InxMSG(s,id) 	id
//#	define TxtMSG(s,id) 	s
	/*
	 * have a language enabled string in a structure
	 */
#if !defined(USER_JOWENS) && !defined(USER_GPACHNER) && !defined(USER_GPACHNER2)	/* This allows us to skip message updates in debug builds */
#	define StructMSG(s,id)	{id,NULL}
#else
#	define StructMSG(s,id)	{0,s}
#endif
	/*
	 * have a string to NOT language enable in a structure
	 */
#	define StructMSGNot(s)		{0,s}
#	define StructMSGNew(s,id)	{id,s}
#	define StructMSGChg(s,id)	{id,s}
	/*
	 * have a NULL string in a language enabled structure
	 */
#	define StructMSGNull()	{0,NULL}
	/*
	 * get the string from a language enabled structure
	 */
#	define StructGetMSGStr(les)										\
		(((les).le_idx != 0) ? NSSMessageTable[(les).le_idx] : (les).le_str)
	/*
	 * get the string from a language enabled structure
	 */
#	define StructGetMSGStrWithTable(les,msgtbl)						\
		(((les).le_idx != 0) ? (msgtbl)[(les).le_idx] : (les).le_str)
	/*
	 * MACRO used if you got an error loading the MESSAGe tables, only does
	 * something if language enabling is turned on
	 */
#	define ERROR_LOADING_MSG_TABLES(errorScreen) 		\
	{													\
		OutputToScreen(errorScreen,						\
			MSGNot("\nError loading message table\n"));	\
 	}
#else /* Language Enabling turned OFF */

#	define MSG(s,id)		s
#	define MSGNot(s)		s
#	define MSGNew(s,id)		s
#	define MSGChg(s,id)		s
#	define InxMSG(s,id) 	id
/*#	define TxtMSG(s,id) 	s*/

#	define StructMSG(s,id)	{0,s}
#	define StructMSGNot(s)	{0,s}
#	define StructMSGNew(s,id)	{0,s}
#	define StructMSGChg(s,id)	{0,s}
#	define StructMSGNull()	{0,NULL}
#	define StructGetMSGStr(les) ((les).le_str)
#	define StructGetMSGStrWithTable(les,msgtbl) ((les).le_str)

#	define ERROR_LOADING_MSG_TABLES(errorScreen)

#endif

/*---------------------------------------------------------------------------
 *	There are some unnecessary warnings coming out of the WATCOM compiler,
 *	disable those warnings.
 *---------------------------------------------------------------------------*/
#ifdef _WATCOMC_
#pragma disable_message(124);		/* disable this warning number */
#endif

#ifdef __cplusplus
}
#endif

#endif	/* _OMNI_H_ */
