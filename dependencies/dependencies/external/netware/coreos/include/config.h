#ifndef __CONFIG_H__
#define __CONFIG_H__
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
 *  $Workfile:   config.h  $
 *  $Modtime:   20 Jul 2001 10:37:26  $
 *  $Revision$
 *  
 ****************************************************************************/

/* 10 name spaces are the top end of allowable name spaces */
#define	MaximumNumberOfNameSpaces		6
#define	MaximumNumberOfVolumes			64

/* Leave room in ServerProcessStackSize for the memory management headers to
 * be allocated in the same 4K block so we don't end up allocating another
 * 4K block of memory.*/
#define ServerProcessStackSize			((4096 * 8) - 128)
#define	ServerProcessPriority			50

#if ((FSEngine) || (IOEngine))
//#define MaximumNumberOfIOLans			2048 - 16
#define MaximumNumberOfIOLans			256 - 16

#endif	/* (FSEngine or IOEngine) */

#if (IOEngine)
#define MaximumNumberOfMSLs			8
#endif	/* (IOEngine) */

//#define MaximumNumberOfLans				2048
#define MaximumNumberOfLans				256
#define MaximumNumberOfMediaTypes		32
#define MaximumNumberOfProtocols		16

#define MAXMaximumSubdirectoryTreeDepth	100
#define MaximumNumberOfDataStreams		3
#define MaximumNumberOfSpoolPrinters 	5

#define RestrictedServers				1

#define PseudoPreEmptionEnabled		-1
#define RealModeInterruptSupport		-1

/* really defined in ipxparm.inc */
#define	MaximumNumberOfSockets		100

/* used in CDMAIN.C & NETMANC.C */
#define COMPRESS_ERROR_COUNT				18
#define UNCOMPRESS_ERROR_COUNT			    18

/* used in FSHOOKS.C & NETMANC.C */
#define	NumberOfHookRoutines	23


/*******
 * No longer used in the server, only here for extern modules that may
 * include this file
 */
#define HistogramEnabled  				0

/****************************************************************************/
/****************************************************************************/


#endif /* __CONFIG_H__ */
