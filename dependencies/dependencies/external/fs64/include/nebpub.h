/*****************************************************************************
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
 |  $Workfile:   nebpub.h  $
 |  $Modtime:   06 Nov 2001 08:45:56  $
 |  $Revision$
 |
 +***************************************************************************/

#ifndef _NEBPUB_H_
#define _NEBPUB_H_

#ifdef __cplusplus
extern "C" {
#endif

extern LONG GetConsumerName(
		void *consumerID,
		BYTE *consumerName);

extern struct EventBlock *GetEventBlocks(
		void *ownerID,
		LONG numberOfEventBlocksToGet);

extern LONG GetProducersEventName(
		void *producerID,
		BYTE *eventName);

extern LONG GetProducerName(
		void *producerID,
		BYTE *producerName);

extern LONG ReturnEventBlocks(
		struct EventBlock *eventBlock);

extern LONG ProduceEvent(
		struct EventBlock *eventBlock);

extern LONG RegisterConsumer(
		struct ConsumerRegistrationInfo *regConsumer);

extern LONG RegisterFilter(
		struct FilterRegistrationInfo *regFilter);

extern LONG RegisterProducer(
		struct ProducerRegistrationInfo *regProducer);

extern LONG UnRegisterConsumer(
		void *consumerID,
		void *userParameter);

extern LONG UnRegisterFilter(
		void *filterID);


extern LONG UnRegisterProducer(
		void *producerID,
		void *userParameter);

#ifdef __cplusplus
}
#endif

#endif
