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
 |  $Workfile:   neb.h  $
 |  $Modtime:   06 Nov 2001 08:45:56  $
 |  $Revision$
 |
 +***************************************************************************/

#ifndef _NEB_H_
#define _NEB_H_

#ifdef __cplusplus
extern "C" {
#endif

#define NEB_EVENT_VERSION1				1
#define NEB_PRODUCER_VERSION1			1
#define NEB_CONSUMER_VERSION1			1
#define NEB_FILTER_VERSION1				1

#define EVENT_BLOCK_STAMP			0x4B4C4245	/* 'KLBE' */

#define EVENT_BLOCK_SIGNATURE		0x42545645	/* 'BTVE' */

#define MAX_EVENT_USER_NAME_LENGTH	80	/* Includes NULL terminator */

#define MAX_FILTER_NAME_LENGTH		80	/* Includes NULL terminator */

#define MAX_EVENT_NAME_LENGTH		256	/* Included NULL terminator */

	/* Event flags */
#define MAY_NOT_SLEEP_BIT			0x00000001
#define SORT_LOW_TO_HIGH_BIT		0x00000002
#define CONSUMABLE_BIT				0x00000004
#define NO_SA_BIT					0x00000008
#define NO_AUDITOR_BIT				0x00000010
#define NO_CHECK_BIT				0x00000020
	/* Data is NESL encapsulated data */
#define NESL_DATA_BIT				0x00000040
	/* NESL shim's way of telling neb how to resolve bit definitions */
	/* with regular NEB components. */
#define NESL_SHIM_BIT				0x00000080
#define EF_UNDEFINED_MASK	!(MAY_NOT_SLEEP_BIT | SORT_LOW_TO_HIGH_BIT | \
		CONSUMABLE_BIT | NO_SA_BIT | NO_AUDITOR_BIT | NO_CHECK_BIT | \
		NESL_DATA_BIT | NESL_SHIM_BIT)
#define DATA_FILTERED_BIT			0x80000000

	/* ProducerFlags */
#define UNIQUE_PRODUCER_BIT			0x00000001

	/* ConsumerFlags */
#define CONSUMABLE_CONSUMER_BIT		0x00000001
#define ACCESS_CONTROL_CHECK_BIT	0x00000002

	/* Shared (Producer/Consumer)Flags */
#define EVENT_CLASS_BIT				0x40000000
#define SMP_ENABLED_BIT				0x80000000

	/* User types */
#define PRODUCER						0
#define SECURITY_AUTHORITY_CONSUMER		1
#define AUDITOR_CONSUMER				2
#define CHECK_CONSUMER					3
#define SYNCHRONOUS_CONSUMER			4
#define ASYNCHRONOUS_CONSUMER			5
#define DEBUG_CONSUMER		 			6
#define RESERVED						7

	/* Consumer Notify type codes */
#define NOTIFY_AUDITOR							0

#define VALIDATE_PRODUCER_REGISTRATION			1
#define VALIDATE_CHECK_CONSUMER_REGISTRATION	2
#define VALIDATE_SYNC_CONSUMER_REGISTRATION		3
#define VALIDATE_ASYNC_CONSUMER_REGISTRATION	4

#define NOTIFY_SECURITY_AUTHORITY				5
#define VERIFY_CHECK_CONSUMER					6
#define VERIFY_SYNC_CONSUMER					7
#define VERIFY_ASYNC_CONSUMER					8

#define NOTIFY_CHECK_CONSUMER					9
#define NOTIFY_SYNCHRONOUS_CONSUMER				10
#define NOTIFY_ASYNCHRONOUS_CONSUMER			11
#define NOTIFY_DEBUGGER							12

#define NOTIFY_PRODUCER_UNREGISTRATION			13
#define NOTIFY_CHECK_CONSUMER_UNREGISTRATION	14
#define NOTIFY_SYNC_CONSUMER_UNREGISTRATION		15
#define NOTIFY_ASYNC_CONSUMER_UNREGISTRATION	16

#define NOTIFY_REGISTRATION_FAILED				17

#define NOTIFY_PROD_OF_CONS_REG					18
#define NOTIFY_PROD_OF_CONS_UNREG				19

#define NOTIFY_CONS_OF_PROD_REG					20
#define NOTIFY_CONS_OF_PROD_UNREG				21

#define ADD_CONS_TO_FILTER						22
#define REMOVE_CONS_FROM_FILTER					23
#define VALIDATE_FILTER							24
#define NOTIFY_FILTER							25

#define SIDE_BAND_MESSAGE						26

#define NOTIFY_CHECK_OPERATION_DENIED			27

	/* Consumer ESR ccodes */
		/* Security Authority, Auditor, and Check Consumers ESR ccodes bits */
#define ACCESS_APPROVED			0x00000000
#define ACCESS_DENIED			0x00000001
#define AUDITOR_APPROVED		0x00000000
#define AUDITOR_DENIED			0x00000002
#define CCODE_RESERVED			0x7F0000FC
#define USER_DEFINABLE			0x00FFFF00
#define FUNCTION_NOT_SUPPORTED	0x80000000

		/* Synchronous consumer ESR ccodes */
#define EVENT_NOT_CONSUMED		0x00000000
#define EVENT_CONSUMED			0x00000001
#define FUNCTION_NOT_SUPPORTED	0x80000000

	/* Produce Event ccodes */
#define SUCCESS					0
#define INVALID_PARAMETER		-1
#define EVENT_DENIED			-2
#define OPERATION_DENIED		-3

	/* Send Event ccodes */
#define SUCCESS					0
#define INVALID_PARAMETER		-1
#define SIDEBAND_NOT_SUPPORTED	-2

	/* RegisterProducer ccodes */
#define SUCCESS							0
#define INVALID_PARAMETER				-1
#define OUT_OF_RESOURCES				-2
#define EVENT_CONFLICT					-3
#define REGISTRATION_DENIED				-4
#define PRODUCER_NOT_UNIQUE				-5

	/* RegisterConsumer ccodes */
#define SUCCESS									0
#define INVALID_PARAMETER						-1
#define OUT_OF_RESOURCES						-2
#define EVENT_CONFLICT							-3
#define REGISTRATION_DENIED						-4
#define CONSUMER_ALREADY_REGISTERED				-5
#define INVALID_CONSUMER_TYPE					-6
#define INVALID_FILTER							-7

	/* RegisterFilter ccodes */
#define SUCCESS							0
#define INVALID_PARAMETER				-1
#define OUT_OF_RESOURCES				-2
#define	DUPLICATE_FILTER				-3



struct EventBlock
{
	LONG EBVersion;
	LONG EBStamp;
	struct EventBlock *EBILink;
	struct ResourceTagStructure *EBRTag;

	void *EBLink;
	void *EBProducerID;
	void *EBProducerWorkSpace;
	void *EBConsumingConsumer;

	LONG EBEventDataLength;
	void *EBEventData;
	union DataBlock
	{
		BYTE B8[16];
		WORD B16[8];
		LONG B32[4];
	} EBShortData;

	LONG EBEventNotifyType;
	void *EBUserParameter;

	void *EBParm0;
	void *EBParm1;
	LONG EBEventFlags;
	LONG EBReserved[1];
};

struct ProducerRegistrationInfo
{
	LONG PRIVersion;
	BYTE *PRIProducerName;
	BYTE *PRIEventName;
	void *PRIUserParameter;
	LONG PRIEventFlags;
	struct LoadDefinitionStructure *PRIOwnerID;
	LONG (*PRIProducerESR)(
			struct EventBlock *eventBlock);
	void *PRISecurityToken;
	LONG PRIProducerFlags;
	LONG PRIPriority;
	void *PRIEventID;
	void *PRIProducerID;
};

struct ConsumerRegistrationInfo
{
	LONG CRIVersion;
	BYTE *CRIConsumerName;
	BYTE *CRIEventName;
	void *CRIUserParameter;
	LONG CRIEventFlags;
	struct LoadDefinitionStructure *CRIOwnerID;
	LONG (*CRIConsumerESR)(
			struct EventBlock *eventBlock);
	void *CRISecurityToken;
	LONG CRIConsumerFlags;
	BYTE *CRIFilterName;
	LONG CRIFilterDataLength;
	void *CRIFilterData;
	LONG (*CRIConsumerCallback)(
			struct EventBlock *eventBlock);
	LONG CRIConsumerType;
	LONG CRIOrder;
	void *CRIConsumerID;
};

struct FilterRegistrationInfo
{
	LONG FRIVersion;
	BYTE *FRIFilterName;
	struct LoadDefinitionStructure *FRIOwnerID;
	LONG (*FRIFilterESR)(
			struct EventBlock *eventBlock);
	void *FRIFilterID;
};

#ifdef __cplusplus
}
#endif

#endif
