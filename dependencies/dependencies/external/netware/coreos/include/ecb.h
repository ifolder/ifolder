#ifndef __ECB_H__
#define __ECB_H__
/*****************************************************************************
 *
 *	(C) Copyright 1989-1994 Novell, Inc.
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
 *  $Workfile:   ecb.h  $
 *  $Modtime:   Sep 02 1999 16:31:16  $
 *  $Revision$
 *  
 ****************************************************************************/

typedef struct IPXAddress
{
    BYTE        network[4];     /* high-low */
    BYTE        node[6];        /* high-low */
    BYTE        socket[2];      /* high-low */
} IPXAddress;

typedef struct IPXHeader
{
    WORD        checkSum;       /* high-low */
    WORD        length;         /* high-low */
    BYTE        transportControl;
    BYTE        packetType;
    IPXAddress  destination;
    IPXAddress  source;
} IPXHeader;

typedef struct SPXHeader
{
    WORD        checksum;
    WORD        length;                 /* high-low*/
    BYTE        transportControl;
    BYTE        packetType;
    IPXAddress  destination;
    IPXAddress  source;
    BYTE        connectionControl;      /* bit flags */
    BYTE        dataStreamType;
    WORD        sourceConnectionID;     /* high-low unsigned */
    WORD        destConnectionID;       /* high-low unsigned */
    WORD        sequenceNumber;         /* high-low unsigned */
    WORD        acknowledgeNumber;      /* high-low unsigned */
    WORD        allocationNumber;       /* high-low unsigned */
} SPXHeader;

typedef struct ECBFragment {
    void     *address;
    LONG     size;                      /* low-high */
} ECBFragment;

typedef void (*ECB_ESRFunc)();

typedef struct ECB {
    void        *fLinkAddress;
    void        *bLinkAddress;
    WORD        status;
    ECB_ESRFunc ESRAddress;
    WORD        stackID;
    BYTE        protocolID[6];
    LONG        boardNumber;
    BYTE        immediateAddress[6];    /* high-low */
    BYTE        driverWorkspace[4];     /* N/A */
    LONG        ESREBXValue;
    WORD        socketNumber;           /* high-low */
    WORD        protocolWorkspace;      /* Session ID */
    LONG        packetLength;
    LONG        fragmentCount;          /* low-high */
    ECBFragment fragmentDescriptor[2];
} ECB;

typedef struct ECB_2 {
    void        *fLinkAddress;
    void        *bLinkAddress;
    WORD        status;
    ECB_ESRFunc ESRAddress;
    WORD        stackID;
    BYTE        protocolID[6];
    LONG        boardNumber;
    BYTE        immediateAddress[6];    /* high-low */
    BYTE        driverWorkspace[4];     /* N/A */
    LONG        ESREBXValue;
    WORD        socketNumber;           /* high-low */
    WORD        protocolWorkspace;      /* Session ID */
    LONG        packetLength;
    LONG        fragmentCount;          /* low-high */
    ECBFragment fragmentDescriptor[16];
} ECB_2;

/****************************************************************************/
/****************************************************************************/


#endif /* __ECB_H__ */
