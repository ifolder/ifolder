#ifndef __FUNCTION_H__
#define __FUNCTION_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   function.h  $
 *  $Modtime:   Jun 06 2001 16:08:00  $
 *  $Revision$
 *
 ****************************************************************************/

/*********************************************************************************
 * Program Name:  NetWare 386
 *
 * Filename:	  function.h
 *
 * Date Created:  Feb 28, 1991
 *
 * Version: 	  1.0
 *
 * Programmers:   Jim A. Nicolet
 *
 * Files used:
 *
 * Date Modified:
 *
 * Modifications:
 *
 * Comments:	  This file contains inline assembly functions for the Metaware
 *					complier & the Watcom compiler.
 *
 ****************************************************************************/


#include <version.h>
#include <portable.h>

unsigned __int64 SwapInt64(unsigned __int64 value);

unsigned long SwapInt32(unsigned long value);

void *ZeroPad4(void *ptr);

void DoNOP( void );

WORD InvertShort(
		WORD value);

LONG InvertLong(
		LONG value);

WORD NShort(
		void *ptr);

LONG NLong(
		void *ptr);

WORD GetShort(
		void *ptr);

void PutShort(
		WORD sdata,
		void *ptr);

void PutLong(
		LONG sdata,
		void *ptr);

LONG GetLong(
		void *ptr);

void CSetB(
		BYTE value,
		void *address,
		LONG numberOfBytes);

void *RCSetB(
		BYTE value,
		void *address,
		LONG numberOfBytes);


LONG MulReturnUpper32Bits(
		LONG a,
		LONG b );

void FlushTLB(void);

void CSetW(
		WORD value,
		void *address,
		LONG numberOfBytes);

void CSetD(
		LONG value,
		void *address,
		LONG numberOfBytes);

void *RCSetD(
		LONG value,
		void *address,
		LONG numberOfBytes);

LONG CFindB(
		BYTE value,
		void *address,
		LONG numberOfBytes);

LONG CFindW(
		WORD value,
		void *address,
		LONG numberOfBytes);

LONG CFindD(
		LONG value,
		void *address,
		LONG numberOfBytes);

void CMovD(
		void *src,
		void *dst,
		LONG numberOfLongs);

void CMovW(
		void *src,
		void *dst,
		LONG numberOfWords);

void CMovB(
		void *src,
		void *dst,
		LONG numberOfBytes);

void CMovDBackwards(
		void *src,
		void *dst,
		LONG numberOfLongs);

LONG	BitTest(
		void *address,
		LONG bitIndex);

void	BitClear(
		void *address,
		LONG bitIndex);

void	BitSet(
		void *address,
		LONG bitIndex);

LONG	BitTestAndSet(
		void *address,
		LONG bitIndex);

LONG	BitTestAndClear(
		void *address,
		LONG bitIndex);

void	CStrCpy(
		void *dst,
		void *src);

void *RCMovD(
		void *src,
		void *dst,
		LONG numberOfLongs);

void *RCMovW(
		void *src,
		void *dst,
		LONG numberOfWords);

void *RCMovB(
		void *src,
		void *dst,
		LONG numberOfBytes);

LONG NCPBoundaryCheck(
		LONG ipx,
		LONG ncp);

LONG NCPSubFunctionCheck(
		LONG subFunctionLength,
		LONG size);

LONG GetPathLength(
		void *address);

LONG GetPathLengthPCC(
		LONG componentCount,
		void *address);

void NullCheck(BYTE *string);

void *CMovASCIIZ(void *src, void *dst);

void *CMovByteLen(void *src, void *dst);

LONG CStrLen(void *string);

#ifndef _NO_NETWARE_FLAG_INLINES_
void Enable(void);

void Disable(void);

void SetFlags(LONG flag);

LONG RetFlags(void);

LONG DisableAndRetFlags(void);

LONG EnableAndRetFlags(void);
#endif /* _NO_NETWARE_FLAG_INLINES_ */

LONG RotateLeft(LONG value, LONG rotate);
LONG RotateRight(LONG value, LONG rotate);

void Inc32(
		LONG *ptr);

void Dec32(
		LONG *ptr);


LONG BitScanReverse(LONG value);

//-----------------------  WATCOM COMPILER ---------------------
//
//
#ifdef _WATCOMC_

/*
** The MODIFY NOMEMORY auxiliary pragma lets the compiler know that no memory is
** modified by the named function.  If no memory is referenced by the function
** then the PARM NOMEMORY MODIFY NOMEMORY auxilliary pragma may be used.
*/

#pragma aux DoNOP										PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux RDTSC										PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux InvertShort								PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux InvertLong								PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux NShort														MODIFY NOMEMORY;

#pragma aux NLong															MODIFY NOMEMORY;

#pragma aux GetShort														MODIFY NOMEMORY;

#pragma aux GetLong														MODIFY NOMEMORY;

#pragma aux FlushTLB									PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux CFindB														MODIFY NOMEMORY;

#pragma aux CFindW														MODIFY NOMEMORY;

#pragma aux CFindD														MODIFY NOMEMORY;

#pragma aux BitTest														MODIFY NOMEMORY;

#pragma aux NCPBoundaryCheck						PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux NCPSubFunctionCheck					PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux GetPathLength												MODIFY NOMEMORY;

#pragma aux GetPathLengthPCC											MODIFY NOMEMORY;

#pragma aux CStrLen														MODIFY NOMEMORY;

#ifndef _NO_NETWARE_FLAG_INLINES_
#pragma aux Enable									PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux Disable									PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux SetFlags									PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux RetFlags									PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux DisableAndRetFlags					PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux EnableAndRetFlags						PARM NOMEMORY	MODIFY NOMEMORY;
#endif

#pragma aux RotateLeft								PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux RotateRight								PARM NOMEMORY	MODIFY NOMEMORY;

void Int3(void);
#pragma aux Int3 = 									 PARM NOMEMORY	MODIFY NOMEMORY;

#pragma aux BitScanReverse = 						 PARM NOMEMORY	MODIFY NOMEMORY;

#endif

/****************************************************************************/
/****************************************************************************/

#if InLineAssemblyEnabled

#ifdef _WATCOMC_

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

#pragma aux SwapInt64			PARM NOMEMORY	MODIFY NOMEMORY;

unsigned __int64 SwapInt64(unsigned __int64 value);

#pragma aux SwapInt64 parm [EDX EAX] = \
	0x92			/* xchg eax,edx */			\
	0x0F 0xC8	/* bswap eax */				\
	0x0F 0xCA	/* bswap edx */				\
	modify exact [EAX EDX];


#pragma aux SwapInt32			PARM NOMEMORY	MODIFY NOMEMORY;

unsigned long SwapInt32(unsigned long value);

#pragma aux SwapInt32 parm [EAX] = \
	0x0F 0xC8	/* bswap eax */	  	\
	modify exact [EAX];

#pragma aux ZeroPad4			PARM NOMEMORY;
void *ZeroPad4(void *ptr);
#pragma aux ZeroPad4 parm [EDI] = \
	"mov eax, 0"	\
	"mov ecx, edi"	\
	"add ecx, 3"	\
	"and ecx, 0xfffffffc"	\
	"sub ecx, edi"	\
	"rep stosb"		\
	"mov eax, edi"	\
	modify exact [EAX ECX EDI];


/****************************************************************************/

#pragma aux BitScanReverse parm [EAX] =											\
	0x0F 0xBD 0xC0				/* bsr eax, eax */	\
	modify exact [EAX];

/****************************************************************************/

/*	added for PS/2 model 80 bug reference ckernel.c MakeThread */

#pragma aux DoNOP parm [] =				\
	0x90						/* nop */	\
	modify exact [];

/****************************************************************************/

#pragma aux RDTSC parm [] = 			\
	0x0F 0x31				/* rdtsc */	\
	modify exact [eax edx];

/****************************************************************************/

#pragma aux InvertShort parm [EAX] =											\
	0x86 0xc4				/* xchg	ah, al	; make LO-HI from HI-LO */	\
	modify exact [EAX];

/****************************************************************************/

/* START BLOCK COMMENT
** #pragma aux InvertLong parm [EAX];
**	#pragma aux InvertLong =															\
**		0x86 0xc4			 #* xchg	ah, al	; make LO-HI from HI-LO #/		\
**		0xc1 0xc0 0x10		 #* rol	eax, 16	; swap upper word to lower #/	\
**		0x86 0xc4			 #* xchg	ah, al	; make LO-HI from HI-LO #/		\
**		parm [EAX]																			\
**		modify exact [EAX];
END BLOCK COMMENT */

/* START BLOCK COMMENT
**	#pragma aux InvertLong parm [eax] =														\
**		0x66 0xc1 0xc0 0x08 	#* rol	ax, 8		; swap upper byte to lower  #/	\
**		0xc1 0xc0 0x10			#* rol	eax, 16	; swap upper word to lower #/		\
**		0x66 0xc1 0xc0 0x08 	#* rol	ax, 8		; swap upper byte to lower  #/	\
**		modify exact [EAX];
END BLOCK COMMENT */

#pragma aux InvertLong parm [eax] =														\
	0x0f 0xc8				/* bswap eax  */  											\
modify exact [EAX];

/****************************************************************************/


#pragma aux NShort parm [ecx] =									\
	0x0f 0xb7 0x01			/* movzx	eax, word ptr [ecx] */	\
	modify exact [EAX];

/****************************************************************************/

#pragma aux NLong parm [ecx ]=						\
	0x8b 0x01				/* mov	eax, [ecx] */	\
	modify exact [EAX];

/****************************************************************************/

/* START BLOCK COMMENT
** #pragma aux GetShort parm [ECX];
**	#pragma aux GetShort =															\
**		0x0f 0xb7 0x01		#* movzx	eax, word ptr [ecx] #/ 					\
**		0x86 0xc4			#* xchg	ah, al ; make LO-HI from HI-LO #/	\
**		parm [ECX]																		\
**		modify exact [EAX];
END BLOCK COMMENT */

#pragma aux GetShort parm [ecx] =						\
	0x33 0xc0				/* xor	eax, eax */ 		\
	0x8a 0x41 0x01			/* mov	al, [ecx + 1] */	\
	0x8a 0x21				/* mov	ah, [ecx] */		\
	modify exact [EAX];

/****************************************************************************/

/* START BLOCK COMMENT
** #pragma aux PutShort parm [EAX] [ECX];
**	#pragma aux PutShort =															\
**		0x86 0xc4			#* xchg	ah, al	; make LO-HI from HI-LO #/ \
**		0x66 0x89 0x01		#* mov	[ecx], ax #/ 								\
**		parm [EAX] [ECX]																\
**		modify exact [EAX];
END BLOCK COMMENT */

#pragma aux PutShort parm [eax] [ecx] =				\
	0x88 0x41 0x01			/* mov	[ecx + 1], al */	\
	0x88 0x21				/* mov	[ecx], ah */		\
	modify exact [];

/****************************************************************************/

/* START BLOCK COMMENT
** #pragma aux PutLong parm [EAX] [ECX];
**	#pragma aux PutLong =																\
**		0x86 0xc4			#* xchg	ah, al	; make LO-HI from HI-LO #/ 	\
**		0xc1 0xc0 0x10		#* rol	eax, 16	; swap upper word to lower #/ \
**		0x86 0xc4			#* xchg	ah, al	; make LO-HI from HI-LO #/ 	\
**		0x89 0x01			#* mov	[ecx], eax #/ 									\
**		parm [EAX] [ECX];
END BLOCK COMMENT */

#pragma aux PutLong parm [eax] [ecx] =					\
	0x88 0x41 0x03			/* mov	[ecx + 3], al */	\
	0x88 0x61 0x02			/* mov	[ecx + 2], ah */	\
	0xc1 0xe8 0x10			/* shr	eax, 16 */			\
	0x88 0x41 0x01			/* mov	[ecx + 1], al */	\
	0x88 0x21				/* mov	[ecx], ah */		\
	modify exact [EAX];

/****************************************************************************/

/* START BLOCK COMMENT
** #pragma aux GetLong parm [ECX];
**	#pragma aux GetLong = \
**		0x8b 0x01			#* mov	eax, [ecx] #/ 									\
**		0x86 0xc4			#* xchg	ah, al	; make LO-HI from HI-LO #/ 	\
**		0xc1 0xc0 0x10		#* rol	eax, 16	; swap upper word to lower #/ \
**		0x86 0xc4			#* xchg	ah, al	; make LO-HI from HI-LO #/ 	\
**		parm [ECX];
END BLOCK COMMENT */

/* START BLOCK COMMENT
**		#pragma aux GetLong parm [ecx] =							\
**		0x8a 0x21				#* mov	ah, [ecx] #/		\
**		0x8a 0x41 0x01			#* mov	al, [ecx + 1] #/	\
**		0xc1 0xe0 0x10			#* shl	eax, 16 #/			\
**		0x8a 0x61 0x02			#* mov	ah, [ecx + 2] #/	\
**		0x8a 0x41 0x03			#* mov	al, [ecx + 3] #/	\
**		modify exact [EAX];
END BLOCK COMMENT */

#pragma aux GetLong parm [ecx] =							\
	0x8b 0x01				/* mov	eax, [ecx] */ 									\
	0x0f 0xc8				/* bswap eax  */  											\
	modify exact [EAX];
/****************************************************************************/

#pragma aux CSetB parm [EAX] [EDI] [ECX] =	\
	0xf3 0xaa				/* rep	stosb */		\
	modify exact [edi ecx];

#pragma aux RCSetB parm [EAX] [EDI] [ECX] =	\
	0xf3 0xaa				/* rep	stosb */		\
	0x8b 0xc7				/* mov	eax, edi */ \
	modify exact [edi ecx];

/****************************************************************************/

#pragma aux MulReturnUpper32Bits parm [EAX] [ECX] =	\
	0xf7 0xe1			/* MUL  ECX  */                \
	0x8b 0xc2			/* MOV  EAX, EDX */               \
	modify exact [EAX ECX EDX];


/****************************************************************************/

#pragma aux FlushTLB parm [] =														\
	0x0f 0x20 0xd8			/* mov	eax, cr3	; get current PET pointer */	\
	0x0f 0x22 0xd8 		/* mov	cr3, eax	; write cr3 and flush TLB */	\
	modify exact [eax];

/****************************************************************************/

#pragma aux CSetW parm [EAX] [EDI] [ECX] =	\
	0xf3 0x66 0xab			/* rep	stosw */		\
	modify exact [edi ecx];

/****************************************************************************/

#pragma aux CSetD parm [EAX] [EDI] [ECX] =	\
	0xf3 0xab				/* rep	stosd */		\
	modify exact [EDI ECX];

/****************************************************************************/

#pragma aux RCSetD parm [EAX] [EDI] [ECX] =	\
	0xf3 0xab				/* rep	stosd */		\
	0x8b 0xc7				/* mov	eax, edi */ \
	modify exact [EAX EDI ECX];

/****************************************************************************/

#pragma aux CFindB parm [EAX] [EDI] [ECX] =			\
	0x85 0xc9				/* test	ecx, ecx */ 		\
	0x74 0x0d				/* jz		short NotFound */	\
	0x8b 0xd1				/* mov	edx, ecx */			\
	0xf2 0xae				/* repne	scasb */				\
	0x75 0x07				/* jnz	short NotFound */	\
	0x8d 0x42 0xff			/* lea	eax, [edx - 1] */	\
	0x2b 0xc1				/* sub	eax, ecx */			\
	0xeb 0x05				/* jmp	short Done */		\
								/* NotFound: */				\
	0xb8 0xff 0xff 0xff 0xff	/* mov	eax, -1 */	\
								/* Done: */						\
	modify exact [EAX ECX EDX EDI];

/****************************************************************************/

#pragma aux CFindW parm [EAX] [EDI] [ECX] =			\
	0x85 0xc9				/* test	ecx, ecx */			\
	0x74 0x0e				/* jz		short NotFound */	\
	0x8b 0xd1				/* mov	edx, ecx */			\
	0xf2 0x66 0xaf			/* repne	scasw */				\
	0x75 0x07				/* jnz	short NotFound */	\
	0x8d 0x42 0xff			/* lea	eax, [edx - 1] */	\
	0x2b 0xc1				/* sub	eax, ecx */			\
	0xeb 0x05				/* jmp	short Done */		\
								/* NotFound: */				\
	0xb8 0xff 0xff 0xff 0xff	/* mov	eax, -1 */	\
								/* Done: */						\
	modify exact [EAX ECX EDX EDI];

/****************************************************************************/

#pragma aux CFindD parm [EAX] [EDI] [ECX] =			\
	0x85 0xc9				/* test	ecx, ecx */			\
	0x74 0x0d				/* jz		short NotFound */	\
	0x8b 0xd1				/* mov	edx, ecx */			\
	0xf2 0xaf				/* repne	scasd */				\
	0x75 0x07				/* jnz	short NotFound */	\
	0x8d 0x42 0xff			/* lea	eax, [edx - 1] */	\
	0x2b 0xc1				/* sub	eax, ecx */			\
	0xeb 0x05				/* jmp	short Done */		\
								/* NotFound: */				\
	0xb8 0xff 0xff 0xff 0xff	/* mov	eax, -1 */	\
								/* Done: */						\
	modify exact [EAX ECX EDX EDI];

/****************************************************************************/

#pragma aux CMovD parm [ESI] [EDI] [ECX] =	\
	0xf3	0xa5				/* rep	movsd */		\
	modify exact [ESI EDI ECX];

/****************************************************************************/

#pragma aux CMovW parm [ESI] [EDI] [ECX] =				\
	0xd1	0xe9				/* shr	ecx, 1 */				\
	0xf3	0xa5				/* rep	movsd */					\
	0x73	0x02				/* jnc	short NoExtraByte */	\
	0x66	0xa5				/* movsw */							\
								/* NoExtraByte: */				\
	modify exact [ESI EDI ECX];

/****************************************************************************/

#pragma aux CMovB parm [ESI] [EDI] [ECX] =	\
	0x8a	0xc1				/* mov	al, cl */ 	\
	0xc1	0xe9	0x02		/* shr	ecx, 2 */ 	\
	0xf3	0xa5				/* rep	movsd */ 	\
	0x24	0x03				/* and	al, 3 */ 	\
	0x8a	0xc8				/* mov	cl, al */ 	\
	0xf3	0xa4				/* rep	movsb */		\
	modify exact [eax ESI EDI ECX];

/****************************************************************************/

#pragma aux CMovDBackwards parm [ESI] [EDI] [ECX] =	\
	0xfd						/* std */							\
	0xf3	0xa5				/* rep	movsd */					\
	0xfc						/* cld */							\
	modify exact [ESI EDI ECX];

/****************************************************************************/

#pragma aux BitTest parm [EDX] [ECX] =				\
	0x33	0xc0				/* xor	eax, eax */		\
	0x0f	0xa3	0x0a		/* bt		[edx], ecx */	\
	0x0f	0x92	0xc0		/* setc	al */				\
	modify exact [EAX];

/****************************************************************************/

#pragma aux BitClear parm [EAX] [ECX] =			\
	0x0f	0xb3	0x08		/* btr	[eax], ecx */	\
	modify exact [];

/****************************************************************************/

#pragma aux BitSet parm [EAX] [ECX] =				\
	0x0f	0xab	0x08		/* bts	[eax], ecx */	\
	modify exact [];

/****************************************************************************/

#pragma aux BitTestAndSet parm [EDX] [ECX] =		\
	0x33	0xc0				/* xor	eax, eax */		\
	0x0f	0xab	0x0a		/* bts	[edx], ecx */	\
	0x0f	0x92	0xc0		/* setc	al */				\
	modify exact [EAX];

/****************************************************************************/

#pragma aux BitTestAndClear parm [EDX] [ECX] =	\
	0x33	0xc0				/* xor	eax, eax */		\
	0x0f	0xb3	0x0a		/* btr	[edx], ecx */	\
	0x0f	0x92	0xc0		/* setc	al */				\
	modify exact [EAX];

/****************************************************************************/

#ifdef TURNED_OFF
#pragma aux CStrCpy parm [EDI] [ESI];
#pragma aux CStrCpy =											\
								/* StrCpyLoop: */					\
	0xac						/* lodsb */							\
	0xaa						/* stosb */							\
	0x3c	0x00				/* cmp	al, 0 */					\
	0x75	0xfa				/* jnz	short StrCpyLoop */	\
	parm [EDI] [ESI];
#endif

#pragma aux CStrCpy parm [EDI] [ESI] =					\
	0x33 0xc9				/*	xor	ecx, ecx */ 		\
								/* copyLoop: */				\
	0x8a 0x04 0x0e			/*	mov	al, [esi+ecx] */	\
	0x88 0x04 0x0f			/*	mov	[edi+ecx], al */	\
	0x41						/*	inc	ecx */				\
	0x3c 0x00				/*	cmp	al, 0 */				\
	0x75 0xf5				/*	jnz	short copyLoop */	\
	modify exact [EAX ECX];

/****************************************************************************/

#pragma aux RCMovD parm [ESI] [EDI] [ECX] =	\
	0xf3	0xa5				/* rep	movsd */		\
	0x8b	0xc7				/* mov	eax, edi */	\
	modify exact [eax ESI EDI ECX];

/****************************************************************************/

#pragma aux RCMovW parm [ESI] [EDI] [ECX] =				\
	0xd1	0xe9				/* shr	ecx, 1 */				\
	0xf3	0xa5				/* rep	movsd */					\
	0x73	0x02				/* jnc	short NoExtraByte */	\
	0x66	0xa5				/* movsw */ 						\
								/* NoExtraByte: */				\
	0x8b	0xc7				/* mov	eax, edi */				\
	modify exact [eax ESI EDI ECX];

/****************************************************************************/

#pragma aux RCMovB parm [ESI] [EDI] [ECX] =	\
	0x8a	0xc1				/* mov	al, cl */	\
	0xc1	0xe9	0x02		/* shr	ecx, 2 */	\
	0xf3	0xa5				/* rep	movsd */		\
	0x24	0x03				/* and	al, 3 */		\
	0x8a	0xc8				/* mov	cl, al */	\
	0xf3	0xa4				/* rep	movsb */		\
	0x8b	0xc7				/* mov	eax, edi */	\
	modify exact [eax ESI EDI ECX];

/****************************************************************************/


/* Guarentee that IPX length is put in EAX, NCP len in ECX */
/* 0x86 0xc4		#* xchg al, ah *# ; (length already swapped in 4.1) */

#pragma aux NCPBoundaryCheck parm [EAX] [ECX] =	\
	0x2b 0xc1				/* sub	eax, ecx */		\
	0x72 0x02				/* jc		short error */	\
	0x33 0xc0				/* xor	eax, eax */		\
								/* error: */				\
	modify exact [EAX];

/****************************************************************************/

/* Guarentee that sub-function length is put in EAX, the calculate len in ECX */

#pragma aux NCPSubFunctionCheck parm [EAX] [ECX] =	\
	0x2b 0xc1				/* sub	eax, ecx */			\
	0x72 0x02				/* jc		short error */		\
	0x33 0xc0				/* xor	eax, eax */			\
								/* error: */					\
	modify exact [EAX];

/****************************************************************************/

#pragma aux GetPathLength parm [EDI] =															\
	0x0f 0xb6 0x0f			/* movzx	ecx, byte ptr [edi]	; get component count */	\
	0x33 0xc0				/* xor	eax, eax					; count variable */			\
	0x47						/* inc	edi						; point to first string */	\
	0x0b 0xc9				/* or		ecx, ecx					; test for components */	\
	0x74 0x0b				/* jz		short NoComponents */									\
								/* Again: */															\
	0x0f 0xb6 0x17			/* movzx	edx, byte ptr [edi]	; get string len */			\
	0x42						/* inc	edx						; count len byte */			\
	0x03 0xc2				/* add	eax, edx */													\
	0x03 0xfa				/* add	edi, edx */													\
	0x49						/* dec	ecx */														\
	0x75 0xf5				/* jnz	short Again */												\
								/* NoComponents: */													\
	modify exact [eax ecx edx edi];

/****************************************************************************/

#pragma aux GetPathLengthPCC parm [ECX] [EDI] =												\
	0x33 0xc0				/* xor	eax, eax					; count variable */			\
	0x0b 0xc9				/* or		ecx, ecx					; test for components */	\
	0x74 0x0b				/* jz		short NoComponents */									\
								/* Again: */															\
	0x0f 0xb6 0x17			/* movzx	edx, byte ptr [edi]	; get string len */			\
	0x42						/* inc	edx						; count len byte */ 			\
	0x03 0xc2				/* add	eax, edx */													\
	0x03 0xfa				/* add	edi, edx */													\
	0x49						/* dec	ecx */														\
	0x75 0xf5				/* jnz	short Again */												\
								/* NoComponents: */													\
	modify exact [eax ecx edx edi];

/****************************************************************************/

#pragma aux NullCheck parm [EDI] =								\
	0x33 0xc9				/* xor	ecx, ecx */					\
	0x0a 0x0f				/* or		cl, byte ptr [edi] */	\
	0x74 0x0c				/* jz		short NullDone */			\
	0x8b 0xd7				/* mov	edx, edi */					\
	0x47						/* inc	edi */						\
	0x33 0xc0				/* xor	eax, eax */					\
	0xf2 0xae				/* repnz	scasb */ 					\
	0x75 0x03				/* jnz	short NullDone */ 		\
	0x41						/* inc	ecx */ 						\
	0x28 0x0a				/* sub	byte ptr [edx], cl */	\
								/* NullDone: */						\
	modify exact [eax ecx edx edi];

/****************************************************************************/

#pragma aux CMovASCIIZ parm [ESI] [EDI] = 				\
								/* StrCpyLoop: */					\
	0xac						/* lodsb */ 						\
	0xaa						/* stosb */ 						\
	0x3c 0x00				/* cmp	al, 0 */ 				\
	0x75 0xfa				/* jnz	short StrCpyLoop */	\
	0x8b 0xc7				/* mov	eax, edi */				\
	modify exact [eax ESI EDI];

/****************************************************************************/

#pragma aux CMovByteLen parm [ESI] [EDI] =					\
	0x33 0xc9				/* xor	ecx, ecx */					\
	0x8a 0x0e				/* mov	cl, byte ptr [esi] */	\
	0x46						/* inc	esi */						\
	0x88 0x0f				/* mov	byte ptr [edi], cl */	\
	0x47						/* inc	edi */						\
	0x8a 0xc1				/* mov	al, cl */					\
	0xc1 0xe9 0x02			/* shr	ecx, 2 */					\
	0xf3 0xa5				/* rep	movsd */						\
	0x24 0x03				/* and	al, 3 */						\
	0x8a 0xc8				/* mov	cl, al */					\
	0xf3 0xa4				/* rep	movsb */						\
	0x8b 0xc7				/* mov	eax, edi */					\
	modify exact [eax ECX ESI EDI];

/****************************************************************************/

#pragma aux CStrLen parm [EDI] =							\
	0xB9 0xFF 0xFF 0xFF 0xFF	/* mov	ecx, -1 */	\
	0x33 0xC0				/* xor	eax, eax */			\
	0xF2 0xAE				/* repnz	scasb */				\
	0xB8 0xFE 0xFF 0xFF 0xFF	/* mov	eax, -2 */	\
	0x2B 0xC1				/* sub	eax, ecx */			\
	modify exact [eax ECX EDI];

#ifndef _NO_NETWARE_FLAG_INLINES_
/****************************************************************************/

#if (FSEngine)
#pragma aux Enable parm [] =										\
	modify exact [];
#else
#pragma aux Enable parm [] =										\
	0xfb						/* sti	; enable Int. Reg. */	\
	modify exact [];
#endif /* (!FSEngine) */

/****************************************************************************/

#if (FSEngine)
#pragma aux Disable parm [] =										\
	modify exact [];
#else
#pragma aux Disable parm [] =										\
	0xfa						/* cli	; disable Int. Reg. */	\
	modify exact [];
#endif /* (!FSEngine) */

/****************************************************************************/

#if (FSEngine)
#pragma aux SetFlags parm [] =				\
	modify exact [];
#else
#pragma aux SetFlags parm [EAX] =			\
	0x50						/* push	eax */	\
	0x9d						/* popfd */			\
	modify exact [eax];
#endif /* (!FSEngine) */

/****************************************************************************/

#if (FSEngine)
#pragma aux RetFlags parm [] =				\
	modify exact [];
#else
#pragma aux RetFlags parm [EAX] =			\
	0x9c						/* pushfd */		\
	0x58						/* pop	eax */	\
	modify exact [eax];
#endif /* (!FSEngine) */

/****************************************************************************/

#if (FSEngine)
#pragma aux DisableAndRetFlags parm [] =						\
	modify exact [];
#else
#pragma aux DisableAndRetFlags parm [EAX] =					\
	0x9c						/* pushfd */							\
	0x58						/* pop	eax */						\
	0xfa						/* cli	; disable Int. Reg. */	\
	modify exact [eax];
#endif /* (!FSEngine) */

/****************************************************************************/

#if (FSEngine)
#pragma aux EnableAndRetFlags parm [] =						\
	modify exact [];
#else
#pragma aux EnableAndRetFlags parm [EAX] =					\
	0x9c						/* pushfd */							\
	0x58						/* pop	eax */						\
	0xfb						/* sti	; enable Int. Reg. */	\
	modify exact [eax];
#endif /* (!FSEngine) */

#endif /* _NO_NETWARE_FLAG_INLINES_ */
/****************************************************************************/

#pragma aux RotateLeft parm [EAX] [ECX] = \
	0xD3 0xC0 \
	modify exact [EAX];

/****************************************************************************/

#pragma aux RotateRight parm [EAX] [ECX] = \
	0xD3 0xC8 \
	modify exact [EAX];

/****************************************************************************/

#pragma aux Inc32 parm [ecx] =					\
	0xFF 0x01			/* inc dword ptr [ecx] */	\
	modify exact [];

/****************************************************************************/

#pragma aux Dec32 parm [ecx] =					\
	0xFF 0x09			/* dec dword ptr [ecx] */	\
	modify exact [];

#pragma aux Int3 = 	\
		"int	3";

#endif /* (_WATCOMC_) */




/****************************************************************************/
/******** NEW INTEL COMPILER from VTUNE *************************************/
/****************************************************************************/

#ifdef _INTELC_

#define Int3()		__asm  int 3


#endif  // (_INTELC_)






/****************************************************************************/
/******** OLD INTEL PROTON COMPILER *****************************************/
/****************************************************************************/

#ifdef _PROTONC_

/**************************************************************************
 *	Definitions for the Intel PROTON compiler
 ***************************************************************************/

asm void DoNOP(void) /*added for PS/2 model 80 bug reference ckernel.c MakeThread*/
{
		nop
}

/**************************************************************************/
asm void CPush(void)
{
		push	ebp
		push	ebx
		push	esi
		push	edi
}

/**************************************************************************/
asm void CPop(void)
{
		pop 	edi
		pop 	esi
		pop 	ebx
		pop 	ebp
}

/**************************************************************************/
asm LONG GetCR0(void)
{
		mov 	eax,cr0
}

/**************************************************************************/
asm void SetCR0(LONG newcr0)
{
	%mem newcr0;
		mov 	eax,newcr0
		mov 	cr0,eax
}

/**************************************************************************/
asm void FlushTLB(void)
{
		mov 	eax,cr3
		mov 	cr3,eax
}

/**************************************************************************/
asm LONG NCPBoundaryCheck(LONG ipx,LONG ncp)
{
	%mem ipx, ncp; lab error;
		mov 	eax,ipx
		mov 	ecx,ncp
;;; 	xchg	al,ah
		sub 	eax,ecx
		jb		error
		xor 	eax,eax
error:
}

/****************************************************************************/

asm WORD InvertShort( WORD value)
{
	%mem value;

		mov 	ax, value
		xchg	al,ah
}

/****************************************************************************/

asm void Enable(void)
{
#if (!FSEngine)

		sti

#endif /* (!FSEngine) */
}

/****************************************************************************/

asm void Disable(void)
{
#if (!FSEngine)

	cli

#endif /* (!FSEngine) */
}

/****************************************************************************/
/*										 i486	Pentium(tm) 				*/
/* Optimization summary: size change:	  -9/16 	-9/16	 56% (bytes)	*/
/*						clock change:	  -3/6		-3/6	 50%			*/
/****************************************************************************/

asm LONG InvertLong( LONG value)
{
	%mem value;

#ifndef PENTIUM
		mov 	eax,value
		rol 	ax,8
		rol 	eax,16
		rol 	ax,8
#else
		mov 	eax,value
		bswap	eax
#endif
}

/****************************************************************************/
/*										 i486	Pentium(tm) 				*/
/* Optimization summary: size change:	  1/7		1/7 	 14% (bytes)	*/
/*						clock change:	 -1/4	   -1/4 	 25%			*/
/****************************************************************************/

asm WORD NShort(void *ptrparm)
{
	%mem ptrparm;

#ifndef PENTIUM
		mov 	ecx, ptrparm
		movzx	eax, word ptr [ecx]
#else
		xor 	eax, eax
		mov 	ecx, ptrparm
		mov 	ax, word ptr [ecx]
#endif
}

/****************************************************************************/

asm LONG NLong(void *ptrparm)
{
	%mem ptrparm;
		mov 	ecx, ptrparm
		mov 	eax, dword ptr [ecx]
}

/****************************************************************************/

asm WORD GetShort(void *ptrparm)
{
	%mem ptrparm;
		mov 	ecx, ptrparm
		xor 	eax,eax
		mov 	al, byte ptr [ecx + 1]
		mov 	ah, byte ptr [ecx]
}

/****************************************************************************/

asm void PutShort(WORD sdata, void *ptrparm)
{
	%mem sdata,ptrparm;
		mov 	ecx,ptrparm
		mov 	ax,sdata
		mov 	byte ptr [ecx+1], al
		mov 	byte ptr [ecx], ah

}

/****************************************************************************/
/*										 i486	Pentium(tm) 				*/
/* Optimization summary: size change:	 -10/25    -10/25	  40% (bytes)	*/
/*						clock change:	 -2/7	   -3/7 	  42%			*/
/****************************************************************************/

asm void PutLong( LONG sdata, void *ptrparm)
{
	%mem sdata,ptrparm;

#ifndef PENTIUM
		mov 	ecx,ptrparm
		mov 	eax,sdata
		mov 	byte ptr [ecx+3],al
		mov 	byte ptr [ecx+2],ah
		shr 	eax, 16
		mov 	byte ptr [ecx+1],al
		mov 	byte ptr [ecx], ah
#else
		mov 	ecx,ptrparm
		mov 	eax,sdata
		bswap	eax
		mov 	dword ptr [ecx],eax
#endif
}

/****************************************************************************/
/*										 i486	Pentium(tm) 				*/
/* Optimization summary: size change:	 -10/25    -10/25	  40% (bytes)	*/
/*						clock change:	 -2/7	   -2/7 	  42%			*/
/****************************************************************************/

asm LONG GetLong( void *ptrparm)
{
	%mem ptrparm;

#ifndef PENTIUM
		mov 	ecx,ptrparm
		mov 	ah, byte ptr [ecx]
		mov 	al, byte ptr [ecx + 1]
		shl 	eax, 16
		mov 	ah, byte ptr [ecx + 2]
		mov 	al, byte ptr [ecx + 3]
#else
		mov 	ecx,ptrparm
		mov 	eax,dword ptr [ecx]
		bswap	eax
#endif
}

/****************************************************************************/

asm void CSetB( BYTE value, void *address, LONG numberOfBytes)
{
	%mem value, address, numberOfBytes;
		push	edi
		mov 	al,value
		mov 	edi,address
		mov 	ecx,numberOfBytes
		repe	stosb
		pop 	edi
}

asm void *RCSetB( BYTE value, void *address, LONG numberOfBytes)
{
	%mem value, address, numberOfBytes;
		push	edi
		mov 	al,value
		mov 	edi,address
		mov 	ecx,numberOfBytes
		repe	stosb
		mov 	eax, edi
		pop 	edi
}

/****************************************************************************/

asm void CSetW( WORD value, void *address, LONG numberOfWords)
{
	%mem value, address, numberOfWords;
		push	edi
		mov 	edi,address
		mov 	ax,value
		mov 	ecx,numberOfWords
		repe	stosw
		pop 	edi
}

/****************************************************************************/

asm void CSetD( LONG value, void *address, LONG numberOfLongs)
{
	%mem value, address, numberOfLongs;
		push	edi
		mov 	edi, address
		mov 	eax, value
		mov 	ecx, numberOfLongs
		repe	stosd
		pop 	edi
}

asm void *RCSetD( LONG value, void *address, LONG numberOfLongs)
{
	%mem value, address, numberOfLongs;
		push	edi
		mov 	edi, address
		mov 	eax, value
		mov 	ecx, numberOfLongs
		repe	stosd
		mov 	eax, edi
		pop 	edi
}
/****************************************************************************/

/* NOTE: Use eax here for value, since Proton only passes DWORDS */

asm LONG CFindB( BYTE value, void *address, LONG numberOfBytes)
{
	%mem value, address, numberOfBytes; lab NotFound, Done;
		push	edi
		mov 	edx, numberOfBytes
		test	edx, edx
		jz		short NotFound
		mov 	edi, address
		mov 	eax, value
		mov 	ecx, edx
		repnz	scasb
		jnz 	short NotFound
		lea 	eax, [edx - 1]
		sub 	eax, ecx
		jmp 	short Done
NotFound:
		mov 	eax, -1
Done:
		pop 	edi
}

/****************************************************************************/
/* NOTE: Use eax here for value, since Proton only passes DWORDS */

asm LONG CFindW( WORD value, void *address, LONG numberOfBytes)
{
	%mem value, address, numberOfBytes; lab NotFound, Done;
		push	edi
		mov 	edx, numberOfBytes
		test	edx, edx
		jz		short NotFound
		mov 	edi, address
		mov 	eax, value
		mov 	ecx, edx
		repnz	scasw
		jnz 	short NotFound
		lea 	eax, [edx - 1]
		sub 	eax, ecx
		jmp 	short Done
NotFound:
		mov 	eax, -1
Done:
		pop 	edi
}

/****************************************************************************/
/* NOTE: Use eax here for value, since Proton only passes DWORDS */

asm LONG CFindD( LONG value, void *address, LONG numberOfBytes)
{
	%mem value, address, numberOfBytes; lab NotFound, Done;
		push	edi
		mov 	edx, numberOfBytes
		test	edx, edx
		jz		short NotFound
		mov 	edi, address
		mov 	eax, value
		mov 	ecx, edx
		repnz	scasd
		jnz 	short NotFound
		lea 	eax, [edx - 1]
		sub 	eax, ecx
		jmp 	short Done
NotFound:
		mov 	eax, -1
Done:
		pop 	edi
}

/****************************************************************************/
asm void CMovD( void *src, void *dst, LONG numberOfLongs)
{
	%mem src, dst, numberOfLongs;
		push	edi
		push	esi
		mov 	esi, src
		mov 	edi, dst
		mov 	ecx, numberOfLongs
		rep 	movsd
		pop 	esi
		pop 	edi
}

/****************************************************************************/
asm void CMovW( void *src, void *dst, LONG numberOfWords)
{
	%mem src, dst, numberOfWords; lab NoExtraByte;
		push	esi
		push	edi
		mov 	ecx, numberOfWords
		mov 	esi, src
		shr 	ecx,1
		mov 	edi, dst
		rep 	movsd
		jnc 	short NoExtraByte
		movsw
	NoExtraByte:
		pop 	edi
		pop 	esi
}

/****************************************************************************/
asm void CMovB( void *src, void *dst, LONG numberOfBytes)
{
	%mem src, dst, numberOfBytes;
		push	edi
		push	esi
		mov 	ecx, numberOfBytes
		mov 	esi, src
		mov 	edi, dst
		mov 	al,cl
		shr 	ecx,2
		rep 	movsd
		and 	al,3
		mov 	cl,al
		rep 	movsb
		pop 	esi
		pop 	edi
}

/****************************************************************************/
asm void CMovDBackwards( void *src, void *dst, LONG numberOfLongs)
{
	%mem src, dst, numberOfLongs;
		push	edi
		push	esi
		mov 	esi, src
		mov 	edi, dst
		mov 	ecx, numberOfLongs
		std
		rep movsd
		cld
		pop 	esi
		pop 	edi
}

/****************************************************************************/
asm LONG	BitTest( void *address, LONG bitIndex)
{
	%mem address,bitIndex;
		mov 	edx, address
		mov 	ecx, bitIndex
		xor 	eax, eax
		bt		dword ptr [edx], ecx
		setc	al
}

/****************************************************************************/
asm void	BitClear( void *address, LONG bitIndex)
{
	%mem address,bitIndex;
		mov 	edx, address
		mov 	ecx, bitIndex
		btr 	dword ptr [edx], ecx
}

/****************************************************************************/
asm void	BitSet( void *address, LONG bitIndex)
{
	%mem address,bitIndex;
		mov 	edx, address
		mov 	ecx, bitIndex
		bts 	dword ptr [edx], ecx
}

/****************************************************************************/
asm LONG	BitTestAndSet( void *address, LONG bitIndex)
{
	%mem address,bitIndex;
		mov 	edx,address
		xor 	eax,eax
		mov 	ecx,bitIndex
		bts 	dword ptr [edx], ecx
		setc	al
}

/****************************************************************************/
asm LONG	BitTestAndClear( void *address, LONG bitIndex)
{
	%mem address,bitIndex;
		mov 	edx, address
		xor 	eax, eax
		mov 	ecx, bitIndex
		btr 	dword ptr [edx], ecx
		setc	al
}

/****************************************************************************/
asm void	CStrCpy( void *dst, void *src)
{
	%mem dst, src; lab copyLoop;
		push	edi
		push	esi
		mov 	esi,src
		mov 	edi,dst
		xor 	ecx,ecx
	copyLoop:
		mov 	al, byte ptr [esi+ecx]
		mov 	byte ptr [edi+ecx], al
		inc 	ecx
		cmp 	al, 0
		jnz 	copyLoop
		pop 	esi
		pop 	edi
}

/****************************************************************************/
asm void *RCMovD( void *src, void *dst, LONG numberOfLongs)
{
	%mem src, dst, numberOfLongs;
		push	edi
		push	esi
		mov 	esi, src
		mov 	edi, dst
		mov 	ecx, numberOfLongs
		rep 	movsd
		mov eax,edi
		pop 	esi
		pop 	edi
}

/****************************************************************************/
asm void *RCMovW( void *src, void *dst, LONG numberOfWords)
{
	%mem src, dst, numberOfWords; lab NoExtraByte;
		push	esi
		push	edi
		mov 	ecx, numberOfWords
		mov 	esi, src
		shr 	ecx,1
		mov 	edi, dst
		rep 	movsd
		jnc 	short NoExtraByte
		movsw
	NoExtraByte:
		mov eax,edi
		pop 	edi
		pop 	esi
}

/****************************************************************************/
asm void *RCMovB( void *src, void *dst, LONG numberOfBytes)
{
	%mem src, dst, numberOfBytes;
		push	edi
		push	esi
		mov 	ecx, numberOfBytes
		mov 	esi, src
		mov 	edi, dst
		mov 	al,cl
		shr 	ecx,2
		rep 	movsd
		and 	al,3
		mov 	cl,al
		rep 	movsb
		mov eax,edi
		pop 	esi
		pop 	edi
}

/****************************************************************************/

asm LONG NCPSubFunctionCheck( LONG subFunctionLength, LONG size)
{
	%mem subFunctionLength, size; lab error;
		mov 	eax, subFunctionLength
		mov 	ecx, size
		sub 	eax, ecx
		jc		short error
		xor 	eax, eax
	error:
}

/****************************************************************************/

asm LONG GetPathLength( void *address)
{
	%mem address; lab AllDone,Loop;
		push	edi
		mov 	edi, address
		movzx	ecx, byte ptr [edi]
		xor 	eax, eax
		inc 	edi
		or		ecx,ecx
		jz		short AllDone
	Loop:
		movzx	edx, byte ptr [edi]
		inc 	edx
		add 	eax, edx
		add 	edi, edx
		dec 	ecx
		jnz 	short Loop
	AllDone:
		pop 	edi
}

/****************************************************************************/

asm LONG GetPathLengthPCC( LONG componentCount, void *address)
{
	%mem componentCount,address; lab Loop,AllDone;
		xor 	eax, eax
		mov 	ecx, componentCount
		or		ecx, ecx
		jz		AllDone
		push	edi
		mov 	edi, address
	Loop:
		movzx	edx, byte ptr [edi]
		inc 	edx
		add 	eax, edx
		add 	edi, edx
		dec 	ecx
		jnz 	Loop
		pop 	edi
	AllDone:
}

/****************************************************************************/

asm void NullCheck( BYTE *string)
{
	%mem string; lab NullDone;
		push	edi
		mov 	edi,string
		xor 	ecx,ecx
		or		cl, byte ptr [edi]
		jz		short NullDone
		mov 	edx, edi
		inc 	edi
		xor 	eax,eax
		repnz	scasb
		jnz 	short NullDone
		inc 	ecx
		sub 	byte ptr [edx], cl
	NullDone:
		pop 	edi
}

/****************************************************************************/

asm void *CMovASCIIZ( void *src, void *dst)
{
	%mem src, dst; lab StrCpyLoop;
		push	edi
		push	esi
		mov 	esi, src
		mov 	edi, dst
	StrCpyLoop:
		lodsb
		stosb
		cmp 	al, 0
		jnz 	short StrCpyLoop
		mov 	eax, edi
		pop 	esi
		pop 	edi
}

/****************************************************************************/

asm void *CMovByteLen( void *src, void *dst)
{
	%mem src,dst;
		push	edi
		push	esi
		mov 	esi, src
		mov 	edi, dst
		xor 	ecx,ecx
		mov 	cl, byte ptr [esi]
		inc 	esi
		mov 	byte ptr [edi], cl
		inc 	edi
		mov 	al,cl
		shr 	ecx, 2
		rep 	movsd
		and 	al,3
		mov 	cl,al
		rep 	movsb
		mov 	eax,edi
		pop 	esi
		pop 	edi
}

/****************************************************************************/

asm LONG CStrLen(void *string)
{
	%mem string;
		push	edi
		mov 	edi,string
		mov 	ecx,-1
		xor 	eax,eax
		repnz scasb
		mov 	eax,-2
		sub 	eax,ecx
		pop 	edi
}

/****************************************************************************/

asm void SetFlags( LONG flag)
{
	%mem flag;

#if (!FSEngine)

		mov 	eax,flag
		push	eax
		popfd

#endif /* (!FSEngine) */
}

/****************************************************************************/

asm LONG RetFlags(void)
{

#if (!FSEngine)

		pushfd
		pop 	eax

#endif /* (!FSEngine) */
}

/****************************************************************************/

asm LONG DisableAndRetFlags(void)
{

#if (!FSEngine)

		pushfd
		pop 	eax
		cli

#endif /* (!FSEngine) */
}

/****************************************************************************/

asm LONG EnableAndRetFlags(void)
{

#if (!FSEngine)

		pushfd
		pop 	eax
		sti

#endif /* (!FSEngine) */
}

/****************************************************************************/
/****************************************************************************/
/****************************************************************************/

#endif /* (_PROTONC_) */

#endif /* InLineAssemblyEnabled */

#endif /* __FUNCTION_H__ */
