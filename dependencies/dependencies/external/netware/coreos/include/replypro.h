#ifndef __REPLYPRO_H__
#define __REPLYPRO_H__
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
 *  $Workfile:   replypro.h  $
 *  $Modtime:   27 Aug 1998 16:31:08  $
 *  $Revision$
 *  
 ****************************************************************************/

struct ReplyProceduresStructure
{
	void (*ReplyKeep)(
		struct RequestPacketStructure *requestPacket,
		LONG completionCode,
		LONG fragmentCount,
		void *fragmentAddress0,
		LONG fragmentLength0,
		...);
	void (*ReplyDisgard)(
		struct RequestPacketStructure *requestPacket,
		LONG completionCode,
		LONG fragmentCount,
		void *fragmentAddress0,
		LONG fragmentLength0,
		...);
	void (*ReplyKeepNoFragments)(
		struct RequestPacketStructure *requestPacket,
		LONG completionCode);
	BYTE *(*GetReplyKeepBuffer)(
		struct RequestPacketStructure *requestPacket);
	void (*ReplyKeepBufferFilledOut)(
		struct RequestPacketStructure *requestPacket,
		LONG extraDataCount);
	void (*ReplyKeepNoFragmentsWithStation)(
		LONG stationNumber,
		LONG completionCode,
		LONG task);
	void (*ReplyUsingAllocBuffer)(
		struct RequestPacketStructure *requestPacket,
		LONG completionCode,
		void *fragmentAddress,
		LONG fragmentLength);
	void (*ReplyKeepWithBufferAndFreePtr)(
		struct RequestPacketStructure *requestPacket,
		LONG completionCode,
		void *freePtr /* May be NULL */,
		LONG fragmentCount,
		void *fragmentAddress0,
		LONG fragmentLength0,
		...);
	void (*ReplyReleaseWithFragments)(		/* Redefined 4-23-97 by Jim A. Nicolet */
		LONG stationNumber,
		struct RequestPacketStructure *requestPacket,
		void (*resourceReleaseFunction)(void *ecbPtr)
		);
};

/****************************************************************************/
/****************************************************************************/


#endif /* __REPLYPRO_H__ */
