/*****************************************************************************
 *
 *	Copyright (C) Unpublished Work of Novell, Inc.
 *	All Rights Reserved.
 *
 *	This work is an unpublished work and contains confidential,
 *	proprietary and trade secret information of Novell, Inc. Access
 *	to this work is restricted to (i) Novell, Inc. employees who have
 *	a need to know how to perform tasks within the scope of their
 *	assignments and (ii) entities other than Novell, Inc. who have
 *	entered into appropriate license agreements. No part of this work
 *	may be used, practiced, performed, copied, distributed, revised,
 *	modified, translated, abridged, condensed, expanded, collected,
 *	compiled, linked, recast, transformed or adapted without the
 *	prior written consent of Novell, Inc. Any use or exploitation of
 *	this work without authorization could subject the perpetrator to
 *	criminal and civil liability.
 *	
 *	$release$
 *	$modname: dtypes.h$
 *	$version: 1.3$
 *	$date: Fri, Mar 6, 1998$
 *	$nokeywords$
 *
 ****************************************************************************/

#ifndef DTYPES_H
#define DTYPES_H

#ifdef __cplusplus
#define externC extern "C"
#else
#define externC extern
#endif

#define FileScope

typedef unsigned short unicode;
typedef unsigned __int64 uint64;
typedef signed __int64 int64;
typedef unsigned long uint32;
typedef signed long int32;
typedef unsigned short uint16;
typedef signed short int16;
typedef unsigned char uint8;
typedef signed char int8;
typedef unsigned long streamSize;

#define MAX_UINT64	0xffffffffffffffffL
#define MAX_UINT32	0xffffffffL
#define MAX_UINT16	0xffffL

/*===========================================================================*/
#endif
