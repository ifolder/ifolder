#ifndef _NWIPX_H_
#define _NWIPX_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwipx.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* ---------------------------------------------------------------------------
** Note: This file now holds the contents of two previous files, nwipxspx.h
** and nwsap.h.
** ---------------------------------------------------------------------------
*/

#include <npackon.h>

/* ECB status field completion codes */
#define STS_SPX_CONNECTION_TERMINATED         0xFFEC
#define STS_SPX_TERMINATED_POORLY             0xFFED
#define STS_SPX_INVALID_CONNECTION            0xFFEE
#define STS_SPX_CONNECTION_TABLE_FULL         0xFFEF
#define STS_SPX_SOCKET_NOT_OPEN               0xFFF0
#define STS_SPX_SOCKET_ALREADY_OPEN           0xFFF1
#define STS_SPX_ECB_CANNOT_BE_CANCELLED       0xFFF9
#define STS_SPX_NO_KNOWN_ROUTE_TO_DESTINATION 0xFFFA
#define STS_SPX_EVENT_CANCELLED               0xFFFC
#define STS_SPX_PACKET_OVERFLOW               0xFFFD
#define STS_SPX_MALFORMED_PACKET              0xFFFE
#define STS_SPX_TRANSMIT_FAILURE              0xFFFF

/*
** This define is for the Queued IPX/SPX Calls. The return code passed in will
** be set to this value until the packet is actually sent to IPX/SPX.
*/

#define PACKET_IN_QUEUE								 0x0001

/*---------------------------------------------------------------------------*
 *                                                                           *
 *   IPX_ECB status field busy (in-process) codes:                           *
 *                                                                           *
 *   0x11 - AES (asynchronous event service) waiting                         *
 *   0x12 - Holding                                                          *
 *   0x13 - Session listen                                                   *
 *   0x14 - Processing                                                       *
 *   0x15 - Receiving                                                        *
 *   0x16 - Sending                                                          *
 *   0x17 - Waiting                                                          *
 *                                                                           *
 *--------------------------------------------------------------------------*/

/*---------------------------------------------------------------------------*
 *   The comment characters in the IPX_ECB structure have the                *
 *   following meanings                                                      *
 *   s - this field must be filled in prior to a send                        *
 *   r - this field must be filled in prior to a receive                     *
 *   R - this field is reserved                                              *
 *   A - this field may be used when the ECB is not in use by IPX/SPX        *
 *   q - the application may read this field                                 *
 *--------------------------------------------------------------------------*/

/* Packet type codes */
#define UNKNOWN_PACKET_TYPE              0
#define ROUTING_INFORMATION_PACKET       1
#define ECHO_PACKET                      2
#define ERROR_PACKET                     3
#define PACKET_EXCHANGE_PACKET           4
#define SEQUENCED_PACKET_PROTOCOL_PACKET 5

#define SPX_END_OF_MESSAGE ((BYTE)0x10)

#define SPX_ECB   struct IPX_ECBStruct
#define ENABLE_WATCHDOG ((BYTE)0xFF)


/* various SAP definitions */
#define SAP_SOCKET               			0x0452

#define GENERAL_SERVICE_QUERY    			1
#define GENERAL_SERVICE_RESPONSE 			2
#define NEAREST_SERVICE_QUERY    			3
#define NEAREST_SERVICE_RESPONSE 			4
#define PERIODIC_ID_PACKET       			2

#define NOT_SUPPORTED            			1
#define INVALID_QUERY_TYPE       			2

#define SAP_RESPONSES_PER_PACKET    		8
#define QUERY_LIST_SIGNATURE        		0x454C5253	/* 'ELRS' */


/* type definitions */
typedef struct tagECBFrag
{
   void *fragAddress;
   LONG  fragSize;
} ECBFrag;

typedef struct IPX_ECBStruct
{
   LONG                   semHandleSave;      /* R */
   struct IPX_ECBStruct **queueHead;          /* sr */
   struct IPX_ECBStruct  *next;               /* A */
   struct IPX_ECBStruct  *prev;               /* A */
   WORD                   status;             /* q */
   LONG                   semHandle;          /* sr ignored for IpxSend */
   WORD                   lProtID;            /* R */
   BYTE                   protID[6];          /* R */
   LONG                   boardNumber;        /* R */
   BYTE                   immediateAddress[6];/* s (IpxSend only) */
   BYTE                   driverWS[4];        /* R */
   LONG                   ESREBXValue;        /* R */
   WORD                   socket;             /* sr ignored if socket parm !=0*/
   WORD                   protocolWorkspace;  /* R */
   LONG                   dataLen;            /* q */
   LONG                   fragCount;          /* sr */
   ECBFrag                fragList[2];        /* sr */
} IPX_ECB;

typedef struct tagInternetAddress
{
   MisalignedLONG network; /* H/L */ /* "Misaligned" depending on usage */
   BYTE           node[6]; /* H/L */
   MisalignedWORD socket;  /* H/L */ /* "Misaligned" depending on usage */
} InternetAddress;

typedef struct tagIPX_HEADER
{
   WORD           checksum;     /* H/L */
   WORD           packetLen;    /* H/L */
   BYTE           transportCtl;
   BYTE           packetType;
   MisalignedLONG destNet;      /* H/L */
   BYTE           destNode[6];
   WORD           destSocket;   /* H/L */
   MisalignedLONG sourceNet;    /* H/L */
   BYTE           sourceNode[6];
   WORD           sourceSocket; /* H/L */
} IPX_HEADER;

typedef struct tagIPX_STATS /* included only for compatibility */
{
   char dummy;
} IPX_STATS;

typedef struct tagSPX_HEADER
{
   WORD           checksum;        /* H/L */
   WORD           packetLen;       /* H/L */
   BYTE           transportCtl;
   BYTE           packetType;
   MisalignedLONG destNet;         /* H/L */
   BYTE           destNode[6];
   WORD           destSocket;      /* H/L */
   MisalignedLONG sourceNet;       /* H/L */
   BYTE           sourceNode[6];
   WORD           sourceSocket;    /* H/L */

   BYTE           connectionCtl;
   BYTE           dataStreamType;
   WORD           sourceConnectID; /* H/L */
   WORD           destConnectID;   /* H/L */
   WORD           sequenceNumber;  /* H/L */
   WORD           ackNumber;       /* H/L */
   WORD           allocNumber;     /* H/L */
} SPX_HEADER;

typedef struct SPX_ConnStruct
{
   BYTE sStatus;
   BYTE sFlags;
   WORD sSourceConnectID;      /* H/L */
   WORD sDestConnectID;        /* H/L */
   WORD sSequenceNumber;       /* H/L */
   WORD sAckNumber;            /* H/L */
   WORD sAllocNumber;          /* H/L */

   WORD sRemoteAckNumber;      /* H/L */
   WORD sRemoteAllocNumber;    /* H/L */

   WORD sLocalSocket;          /* L/H */
   BYTE sImmediateAddrees[6];
/* permits use of correct spelling for the previous field: */
#define sImmediateAddress  sImmediateAddrees

   LONG sRemoteNet;            /* H/L */
   BYTE sRemoteNode[6];        /* H/L */
   WORD sRemoteSocket;         /* H/L */

   BYTE sRetransmitCount;
   BYTE sRetransmitMax;
   WORD sRoundTripTimer;       /* L/H */
   WORD sRetransmittedPackets; /* L/H */
   WORD sSuppressedPackets;    /* L/H */

   WORD sLastReceiveTime;
   WORD sLastSendTime;
   WORD sRoundTripMax;
   WORD sWatchdogTimeout;
   BYTE sSessionXmitQHead[4];
   BYTE sSessionXmitECBp[4];
} SPX_SESSION;

/*  SPX_SESSION sStatus field codes: */
#define SPX_SSTATUS_ABORTED     0x00
#define SPX_SSTATUS_WAITING     0x01
#define SPX_SSTATUS_STARTING    0x02
#define SPX_SSTATUS_ESTABLISHED 0x03
#define SPX_SSTATUS_TERMINATING 0x04

/* SAP type definitions */
typedef struct tagT_SAP_ID_PACKET
{

   WORD            SAPPacketType;       /* 2 or 4 */
   WORD            serverType;          /* assigned by Novell */
   BYTE            serverName[48];      /* service name */
   InternetAddress serverAddress;       /* server internetwork address */
   WORD            interveningNetworks; /* # of networks packet must traverse */
} T_SAP_ID_PACKET;

typedef struct tagSERVICE_QUERY_PACKET
{
  WORD queryType;  /* 1 or 3 */
  WORD serverType; /* assigned by Novell */
} SERVICE_QUERY_PACKET;

typedef struct SAPResponse
{
   WORD                SAPPacketType;      /* 2 or 4 */
   struct
   {
      WORD            serverType;          /* assigned by Novell */
      BYTE            serverName[48];      /* service name */
      InternetAddress serverAddress;       /* server internetwork address */
      WORD            interveningNetworks; /* # of networks "hops" */
   }                   responses[SAP_RESPONSES_PER_PACKET];
   struct SAPResponse *next;
   LONG                signature;
   int                 count;
} SAP_RESPONSE_LIST_ENTRY;

#include <npackoff.h>


#ifdef __cplusplus
extern "C"
{
#endif

/* IPX function prototypes... */
int	IpxCheckSocket( WORD socket );
int	IpxCancelEvent( IPX_ECB *ECBp );
int	IpxCloseSocket( WORD socket );
int	IpxConnect( IPX_ECB *ECBp );
int	IpxDisconnect( IPX_ECB *ECBp );
IPX_ECB	*IpxGetAndClearQ( IPX_ECB **replyQptr );
int	IpxGetInternetworkAddress( BYTE *address );
int	IpxGetLocalTarget( BYTE *address, IPX_ECB *ECBp, LONG *timeToNet );
int	IpxGetStatistics( IPX_STATS *ipxStats );
int	IpxGetVersion( BYTE *majorVersion, BYTE *minorVersion, WORD *revision );
int	IpxOpenSocket( WORD *socketP );
int	IpxQueuedSend( WORD socket, IPX_ECB *ECBp, int *rcode );
int	IpxQueuedReceive( WORD socket, IPX_ECB *ECBp, int *rcode );
int	IpxReceive( WORD socket, IPX_ECB *ECBp );
int	IpxResetStatistics( void );
int	IpxSend( WORD socket, IPX_ECB *ECBp );

/* SPX function prototypes... */
int	SpxAbortConnection( WORD connection );
int	SpxCancelEvent( SPX_ECB *ecb );
int	SpxCheckSocket( WORD socket );
int	SpxCloseSocket( WORD socket );
int	SpxEstablishConnection( WORD socket, SPX_ECB *ecb, BYTE retryCount,
			BYTE watchDogFlag, WORD *connection );
int	SpxGetConfiguration( LONG *maxConn, LONG *availConn );
int	SpxGetConnectionStatus( WORD connection, SPX_SESSION *buffer );
																	/* (56 bytes) */
int	SpxGetTime( LONG *marker );
int	SpxGetVersion( BYTE *major, BYTE *minor, WORD *revision, LONG *revDate );
int	SpxListenForConnection( WORD socket, SPX_ECB *ecb, BYTE retryCount,
			BYTE watchDogFlag, WORD *connection );
int	SpxListenForConnectedPacket( WORD socket, SPX_ECB *ecb, WORD connection );
int	SpxListenForSequencedPacket( WORD socket, SPX_ECB *ecb );
int	SpxOpenSocket( WORD *socket );
int	SpxQueuedListenForSequencedPacket( WORD socket, SPX_ECB *ecb, int *rcode);
int	SpxQueuedSendSequencedPacket( WORD connection, SPX_ECB *ecb, int *rcode );
int	SpxSendSequencedPacket( WORD connection, SPX_ECB *ecb );
int	SpxTerminateConnection( WORD connection, SPX_ECB *ecb);

/* SAP function prototypes... */
LONG	AdvertiseService( WORD serviceType, const char *serviceName,WORD serviceSocket);
int	FreeQueryServicesList( SAP_RESPONSE_LIST_ENTRY *listP );
SAP_RESPONSE_LIST_ENTRY	*QueryServices( WORD queryType, WORD serviceType );
int	ShutdownAdvertising( LONG advertisingHandle );

#ifdef __cplusplus
}
#endif

#endif
