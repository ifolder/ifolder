#ifndef _IP_ROUTE_H_
#define _IP_ROUTE_H_
/*
----------------------------------------------------------------------------

	(C) Copyright 1991 Novell, Inc.	All Rights Reserved.

	This program is an unpublished copyrighted work which is proprietary
	to Novell, Inc. and contains confidential information that is not
	to be reproduced or disclosed to any other person or entity without
	prior written consent from Novell, Inc. in each and every instance.

	WARNING:  Unauthorized reproduction of this program as well as
	unauthorized preparation of derivative works based upon the
	program or distribution of copies by sale, rental, lease or
	lending are violations of federal copyright laws and state trade
	secret laws, punishable by civil and criminal penalties.

----------------------------------------------------------------------------
*/



/* total size of an IP address in bytes */
#define	IP_ADDR_SZ	4

typedef	union ip_addr {
		unsigned char	ip_array[ IP_ADDR_SZ ];
		unsigned short	ip_short[ IP_ADDR_SZ / 2 ];
		unsigned long	ip_long;
} ip_addr;

/* maximum address mapping size is that largest we currently use */
#define	SNPA_MX		10


/* 
** Simple IP interface information block --
*/

struct ip_if_info {
	ip_addr		ifi_local_addr;		/* interface's IP address     */
	ip_addr		ifi_net_mask;		/* network mask               */
	ip_addr		ifi_broadcast;		/* broadcast address          */
};

/* 
** Extended IP interface information block --
*/

typedef struct ip_extended_if_info {
        unsigned long   iex_signature;          /* API signature              */
        unsigned long   iex_version;            /* API version                */
        unsigned long   iex_length;             /* buffer size                */
   	unsigned long	iex_flags;		/* flags                      */
        unsigned long   iex_if_id;              /* interface id               */
	unsigned long	iex_timestamp;		/* creation time              */
	ip_addr		iex_local_addr;		/* interface's IP address     */
	ip_addr		iex_net_mask;		/* network mask               */
	ip_addr		iex_broadcast;		/* broadcast address          */
        unsigned long   iex_packet_mx;          /* maximum outgoing packet    */
        unsigned long   iex_packet_opt;         /* optimum outgoing packet    */
        unsigned long   iex_reasm_mx;           /* maximum reassembled packet */
	int		iex_net_type;		/* network type               */
	unsigned long	iex_board_num;		/* ODLI board number          */
	unsigned char	iex_our_snpa[ SNPA_MX ];/* SNPA for interface         */
} ip_extended_if_info;



int IPExtendedIFInfo ( struct ip_extended_if_info * info_pt );

int IPGetIFInfo ( struct ip_if_info * if_info_pt );

unsigned long IPGetLocalAddr ( unsigned long last_addr );

unsigned long IPGetLocalAddrIncludingAux ( unsigned long last_addr );

#endif