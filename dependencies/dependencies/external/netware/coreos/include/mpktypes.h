/*****************************************************************************
 *
 *	(C) Copyright 1988-1996 Novell, Inc.
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
 * $Author$
 * $Modtime:   09 Jun 1998 01:07:04  $
 * $Workfile:   mpktypes.h  $
 * $Revision$                                                                              
 ****************************************************************************/




#ifndef _MPKTYPES_H_
#define _MPKTYPES_H_

#include "portable.h"


/*
 * Various data typedefs
 */
typedef void * THREAD;
typedef void * APPL;
typedef void * SPINLOCK;
typedef void * MUTEX;
typedef void * SEMAPHORE;
typedef void * RWLOCK;
typedef void * RWMUTEX;
typedef void * BARRIER;
typedef	void * QUE;
typedef	void * QUE_LIGHT;
typedef void * INTTAG;
//typedef void * HANDLE;

typedef	void * SCREEN;

typedef unsigned long FLAGS;
typedef unsigned long ERROR;
typedef unsigned long PROCESSOR;

typedef	void * CONDITION;

//
// WARNING!!!:
// The following are for Novell Internal use only.
// Until an alternate place is found, they will
// be retained here.
//

typedef	void * SPINRWLOCK;
typedef	void * KCONDITION;
typedef	void * RESOURCETAG;
typedef	void * MODULEHANDLE;

/**********
	Macros
**********/

#define	CpuBitSet( x )		while( ( x & CpuBit ) == 0 ) { x |= CpuBit; }
#define	CpuBitClear( x )	while( ( x & CpuBit ) != 0 ) { x &= ~CpuBit; }

//
// Clear the Nth bit (0 based) in a bit map.
//

#define	ClearBit(BitMap, BitNum)	(BitMap &= ~(1 << (BitNum)))

//
// Set the Nth bit (0 based) in a bit map.
//

#define	SetBit(BitMap, BitNum)		(BitMap |= (1 << (BitNum)))

//
// Module internal data/routines should be "visible" only during debugging.
//


#endif

