/******************************************************************************
  Source module name:  ws2nlm.h
  Release Version:     1.08

  %name:ws2nlm.h %
  %version:7.1.1.1.17 %
  %date_modified:Thu Aug 23 11:54:30 2001 %
  $Copyright:

  Copyright (c) 1989-2000 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if !defined(WS2NLM_H)
#define WS2NLM_H

#if !defined(N_PLAT_NLM)
#define N_PLAT_NLM
#endif

#if !defined unicode
#define unicode unsigned short  /* Unicode data must be 16 bits   */
#endif

#define _INC_WINDOWS


// New Address Types
#define AF_INET_ACP 25
#define AF_IPX_ACP  26
#define AF_ACP		  27

// NetWare Call back IOCTL Flags
#define SIO_RAWCALLBACKS ((DWORD)(0x1ADD0002))
#define SIO_WORKTODOCALLBACKS ((DWORD)(0x1ADD0004))
#define SIO_FASTACCEPTCALLBACK ((DWORD)(0x1ADD0008))
#define SIO_FASTRECVCALLBACK ((DWORD)(0x1ADD000C))

// affects blocking mode as well as overlapped callbacks   
#define SIO_RCVFULLMSG 			((DWORD)(0x1ADD0010)) 



// NetWare SSL Ioctls
#define SECURITY_PROTOCOL_SSL 1
#define SECURITY_PROTOCOL_TLS 2
#define SIO_SSL_CRYPTFILE ((DWORD)(0x1ADD0010))  
	//optval is a zero terminated ASCII string, 
   //optlen is set to length of string and zero termination 
#define SIO_SSL_AUTHTYPE ((DWORD)(0x1ADD0020))
	// There are three interesting authentication types
   // CLIENT -	Client initiates a SSL connection. 
   // SERVER - Listener set up to listen for incoming SSL conns, (Server sends it's cert during auth)
   // MUTUAL is SSL Server requesting client authentication, (Server asks for client cert inaddition to sending it's cert)
   // optval is a DWORD defined as
   #define MUTUAL 	0x00000002
#define SIO_SSL_CONVERT ((DWORD)(0x1ADD0040))  
   // optval is a BOOL if set then convert socket to SSL
   // if cleared then convert socket from SSL


// NetWare Call WSASocket Flags
#define SKTS_RAWCALLBACKS ((DWORD)(0x40000000))
#define SKTS_WORKTODOCALLBACKS ((DWORD)(0x20000000))

// System flags not defined in NetWare
#define INFINITE 0xFFFFFFFF
#define WAIT_OBJECT_0 0
#define WAIT_ABANDONED ((DWORD)0x00000080L)
#define WAIT_TIMEOUT ((DWORD)0x00000102L)


//Various Types that may not be defined
#ifndef FARPROC
#define FARPROC void *
#endif
#ifndef FAR
#define FAR
#endif
#ifndef PASCAL
#define PASCAL
#endif
#ifndef CALLBACK
#define CALLBACK
#endif

#ifndef BYTE
#define BYTE unsigned char 
#endif

#ifndef wsnchar
#define wsnchar unsigned char 
#endif

#ifndef BOOL
#define BOOL unsigned int 
#endif

#ifndef WORD
#define WORD unsigned short 
#endif

#ifndef DWORD
#define DWORD unsigned int 
#endif

#ifndef LPDWORD
#define LPDWORD unsigned int *
#endif

#ifndef ULONG
#define ULONG unsigned long 
#endif

#ifndef UCHAR
#define UCHAR	unsigned char
#endif

#ifndef WPARAM
#define WPARAM DWORD 
#endif

#ifndef LPARAM
#define LPARAM DWORD 
#endif

#if !defined(MAKEWORD)
#define MAKEWORD(low,high) \
        ((WORD)((BYTE)(low)) | (((WORD)(BYTE)(high))<<8))
#endif

//
//
// Predefined Value Types.
//

#ifndef REG_NONE
#define REG_NONE                    ( 0 )   // No value type
#endif                       

#ifndef REG_SZ
#define REG_SZ                      ( 1 )   // Unicode nul terminated string
#endif

#ifndef REG_EXPAND_SZ
#define REG_EXPAND_SZ               ( 2 )   // Unicode nul terminated string
#endif                                      // (with environment variable references)
#ifndef REG_BINARY
#define REG_BINARY                  ( 3 )   // Free form binary
#endif
#ifndef REG_DWORD
#define REG_DWORD                   ( 4 )   // 32-bit number
#endif
#ifndef REG_DWORD_LITTLE_ENDIAN
#define REG_DWORD_LITTLE_ENDIAN     ( 4 )   // 32-bit number (same as REG_DWORD)
#endif
#ifndef REG_DWORD_BIG_ENDIAN
#define REG_DWORD_BIG_ENDIAN        ( 5 )   // 32-bit number
#endif
#ifndef REG_LINK
#define REG_LINK                    ( 6 )   // Symbolic Link (unicode)
#endif
#ifndef REG_MULTI_SZ
#define REG_MULTI_SZ                ( 7 )   // Multiple Unicode strings
#endif
#ifndef REG_RESOURCE_LIST
#define REG_RESOURCE_LIST           ( 8 )   // Resource list in the resource map
#endif
#ifndef REG_FULL_RESOURCE_DESCRIPTOR
#define REG_FULL_RESOURCE_DESCRIPTOR ( 9 )  // Resource list in the hardware description
#endif
#ifndef REG_RESOURCE_REQUIREMENTS_LIST
#define REG_RESOURCE_REQUIREMENTS_LIST ( 10 )
#endif

#ifdef UNICODE
#define LPTSTR unsigned short *
#else
#define LPTSTR char *
#endif

#ifndef _WHCAR_T_DEFINED
#define WCHAR wchar_t	
#define _WCHAR_T_DEFINED
#endif

#ifndef LPWSTR
#define LPWSTR WCHAR *
#endif

#ifndef LPBYTE
#define LPBYTE char *
#endif

#ifndef INT
#define INT int /*transmit */
#endif

#ifndef LPSTR
#define LPSTR	char * /*[string] */
#endif

#ifndef LPINT
#define LPINT int *
#endif

#ifndef LPVOID
#define LPVOID void *
#endif

#ifndef VOID
#define VOID void
#endif

#ifndef CHAR
#define CHAR wsnchar
#endif


#define HWND void *
#define HANDLE void *
#define LPHANDLE HANDLE  *

#ifndef IN
#define IN
#endif

#ifndef OUT
#define OUT
#endif

#if !defined (WIN32)
#define WIN32
#endif

typedef struct _OVERLAPPED {
    DWORD   Internal;
    DWORD   InternalHigh;
    DWORD   Offset;
    DWORD   OffsetHigh;
    HANDLE  hEvent;
} OVERLAPPED, *LPOVERLAPPED;

// MICROSOFT types used in winsock2.h
#ifndef ERROR_INVALID_HANDLE 
#define ERROR_INVALID_HANDLE             6L
#endif
#ifndef ERROR_NOT_ENOUGH_MEMORY
#define ERROR_NOT_ENOUGH_MEMORY          8L    // dderror
#endif
#ifndef ERROR_INVALID_PARAMETER
#define ERROR_INVALID_PARAMETER          87L    // dderror
#endif
#ifndef ERROR_IO_PENDING
#define ERROR_IO_PENDING                 997L    // dderror
#endif
#ifndef ERROR_OPERATION_ABORTED
#define ERROR_OPERATION_ABORTED          995L
#endif
#ifndef ERROR_IO_INCOMPLETE
#define ERROR_IO_INCOMPLETE				 996L
#endif

#ifndef WAIT_FAILED
#define WAIT_FAILED 			((DWORD)-1)
#endif
#ifndef WAIT_OBJECT_0
#define WAIT_OBJECT_0			((DWORD)0)
#endif
#ifndef WAIT_TIMEOUT
#define	WAIT_TIMEOUT			((DWORD)0x102L)
#endif
#ifndef INFINITE	
#define INFINITE				((DWORD)-1)
#endif
#ifndef MAXIMUM_WAIT_OBJECTS
#define MAXIMUM_WAIT_OBJECTS	((DWORD)64)
#endif
#ifndef WAIT_IO_COMPLETION
#define WAIT_IO_COMPLETION		((DWORD)0x000000C0L)
#endif


// BSD sockets Clashes that must first be maped to
// WS2_32.NLM calls before they can be converted to Winsock 2.
  
#define htonl WS2_32_htonl 
#define ntohl WS2_32_ntohl 
#define htons WS2_32_htons 
#define ioctlsocket WS2_32_ioctlsocket
#define ntohs WS2_32_ntohs 
#define send WS2_32_send
#define recv WS2_32_recv
#define bind WS2_32_bind
#define listen WS2_32_listen
#define closesocket WS2_32_closesocket
#define	getpeername WS2_32_getpeername
#define getsockname WS2_32_getsockname
#define getsockopt WS2_32_getsockopt
#define recvfrom WS2_32_recvfrom
#define select WS2_32_select
#define sendto WS2_32_sendto
#define setsockopt WS2_32_setsockopt
#define socket WS2_32_socket
#define shutdown WS2_32_shutdown
#define inet_addr WS2_32_inet_addr
#define inet_ntoa WS2_32_inet_ntoa
#define gethostbyaddr WS2_32_gethostbyaddr
#define gethostbyname WS2_32_gethostbyname
#define gethostname WS2_32_gethostname
#define getprotobyname WS2_32_getprotobyname
#define getprotobynumber WS2_32_getprotobynumber
#define getservbyname WS2_32_getservbyname
#define getservbyport WS2_32_getservbyport

#include <winsock2.h>            // Winsock 2 extensions.

// BSD sockets Clashes that can be maped directly to WSA calls.
#define connect(s,name,namelen) WSAConnect(s,name,namelen, 0,0,0,0)
#define accept(s,addr,addrlen) WSAAccept(s,addr,addrlen,0,0)

#define SO_CONNTIMEO       0x1009          /* connect timeout */


// NetWare Fast Accept and Recv option structures
// Fast Recv also has a cleanup routine returned.

typedef	
int (CALLBACK *LPFASTACCEPT_COMPLETION_ROUTINE)(
	SOCKET acceptSkt, 
	LPSOCKADDR peerAddr, 
	int peerAddrLen,
	void *arg);


typedef	
int 
(CALLBACK *LPFASTRECV_COMPLETION_ROUTINE)(SOCKET s, 
	void *recvBuf,
	LPWSABUF wsBuf,
	DWORD wsBufCnt,
	DWORD recvLen,
	void *arg);
 
typedef struct FASTACCEPT_OP
{
	LPFASTACCEPT_COMPLETION_ROUTINE acceptHandler;
	void *arg;

}*LPFAST_ACCEPT_OPT, FAST_ACCEPT_OPT;

typedef struct FASTRECV_OP
{
	LPFASTRECV_COMPLETION_ROUTINE recvHandler;
	void *Arg;

}*LPFAST_RECV_OPT, FAST_RECV_OPT;


// Winsock 2 applications that want to use SSL need to define WS_SSL
#ifdef WS_SSL

#ifndef _TIME_T
#define	_TIME_T
typedef unsigned long time_t;
#endif

// Secure Sockets Layer - needed until Winsock SDK supplies ssl header file.
// Taken from Winsock 2 protocol Annex for SSL Security Protocol. Unsupported
// options are labeled "not supported".
/*
** This value is the SSL protocol tag and WSAIoctl dwIoControlCode
** "T" value.
*/
#define _SO_SSL	((2L << 27) | (0x73L << 16))



/*
** These values are used to form the WSAIoctl dwIoControlCode
** "Code" value.
*/
#define _SO_SSL_FLAGS				0x01
#define _SO_SSL_CIPHERS 			0x02
#define _SO_SSL_SERVER  			0x04
#define _SO_SSL_AUTH_CERT_HOOK 		0x08	
#define _SO_SSL_RSA_ENCRYPT_HOOK 	0x10	// not supported
#define _SO_SSL_RSA_DECRYPT_HOOK 	0x20	// not supported

// _SO_SSL_CLIENT has been changed from 0x03 to 0x80 to avoid bitwise 
// conflicts with _SO_SSL_CIPHERS _SO_SSL_FLAGS. 
#define _SO_SSL_CLIENT  				0x80  


/*
** Actual SSL Ioctl commands
*/
#define SO_SSL_GET_FLAGS 	(IOC_IN |_SO_SSL|_SO_SSL_FLAGS)
#define SO_SSL_SET_FLAGS 	(IOC_OUT|_SO_SSL|_SO_SSL_FLAGS)
#define SO_SSL_GET_CIPHERS	(IOC_IN |_SO_SSL|_SO_SSL_CIPHERS)
#define SO_SSL_SET_CIPHERS	(IOC_OUT|_SO_SSL|_SO_SSL_CIPHERS) //not supported
#define SO_SSL_GET_CLIENT	(IOC_IN |_SO_SSL|_SO_SSL_CLIENT)
#define SO_SSL_SET_CLIENT	(IOC_OUT|_SO_SSL|_SO_SSL_CLIENT)
#define SO_SSL_GET_SERVER	(IOC_IN |_SO_SSL|_SO_SSL_SERVER)
#define SO_SSL_SET_SERVER	(IOC_OUT|_SO_SSL|_SO_SSL_SERVER)
#define SO_SSL_GET_AUTH_CERT_HOOK 	(IOC_IN |_SO_SSL|_SO_SSL_AUTH_CERT_HOOK)

//not supported
#define SO_SSL_SET_AUTH_CERT_HOOK 	(IOC_OUT|_SO_SSL|_SO_SSL_AUTH_CERT_HOOK)
//not supported
#define SO_SSL_GET_RSA_ENCRYPT_HOOK	(IOC_IN |_SO_SSL|_SO_SSL_RSA_ENCRYPT_HOOK)
//not supported
#define SO_SSL_SET_RSA_ENCRYPT_HOOK	(IOC_OUT|_SO_SSL|_SO_SSL_RSA_ENCRYPT_HOOK)
//not supported
#define SO_SSL_GET_RSA_DECRYPT_HOOK	(IOC_IN |_SO_SSL|_SO_SSL_RSA_DECRYPT_HOOK)
//not supported
#define SO_SSL_SET_RSA_DECRYPT_HOOK	(IOC_OUT|_SO_SSL|_SO_SSL_RSA_DECRYPT_HOOK)

#define SO_SSL_ENABLE 			0x001
#define SO_SSL_SERVER 			0x002
#define SO_SSL_AUTH_CLIENT 		0x004
#define SO_SSL_ACCEPT_WEAK		0x008	 //not supported

#ifndef int32
#define int32	int
#endif

struct sslauthhook {
	BYTE	*certificateChain;
	char	*subjectDN;
	unsigned char	*cipher;
	unsigned char	*sessionID;
	unsigned int	sessionIDLen;
};

struct sslcipheropts {
	int n;
	char specs[3];
};

struct sslclientopts {
	char *cert;
	int certlen;
	time_t sidtimeout;
	int32 sidentries;
	char *siddir;
};

struct sslserveropts {
	char *cert;
	int certlen;
	time_t sidtimeout;
	int32 sidentries;
	char *siddir;
};

//not suppported
struct sslauthcertopts {
	int type;
	int (*func)(void *arg, char *cert, int len);
	void *arg;
};

#define SSL_ACK_OK	  			1	//not supported 
#define SSL_ACH_WEAK_OK			2	//not supported 
#define SSL_ACH_LONG_DATA  		3	//not supported 
#define SSL_ACH_BAD_DATA   		4	//not supported 
#define SSL_ACH_BAD_SIG    		5	//not supported 
#define SSL_ACH_CERT_EXPIRED 	6	//not supported 

//not suppported
struct sslrsaencrypthook {
	int (*func)(void *arg, int blockType, char *dest, int *destlen, char *src,
		int srclen);
	void *arg;
};

#define SSL_REH_OK    			0	//not supported 
#define SSL_REH_BAD_TYPE 		1	//not supported 
#define SSL_REH_BAD_LEN 		2	//not supported 

//not suppported
struct sslrsadecrypthook {
	int (*func)(void *arg, int blockType, char *dest, int *destlen, char *src,
		int srclen);
	void *arg;
};

#define SSL_RDH_OK    			0	//not supported 
#define SSL_RDH_BAD_TYPE 		1	//not supported 
#define SSL_RDH_BAD_LEN 		2	//not supported 


// TLS options



// Secure Sockets Layer - needed until Winsock SDK supplies ssl header file.
// Taken from Winsock 2 protocol Annex for SSL Security Protocol. Unsupported
// options are labeled "not supported".
/*
** This value is the SSL protocol tag and WSAIoctl dwIoControlCode
** "T" value. This value is unique to distinguish a TLS Ioctl from an SSL
** Ioctl due to different structure definitions.
*/
#define _SO_TLS	((2L << 27) | (0x74L << 16))



/*
** These values are used to form the WSAIoctl dwIoControlCode
** "Code" value.
*/
#define _SO_TLS_FLAGS					0x01
#define _SO_TLS_CIPHERS 				0x02
#define _SO_TLS_SERVER  				0x04
#define _SO_TLS_AUTH_CERT_HOOK		0x08	// not supported
#define _SO_TLS_RSA_ENCRYPT_HOOK 	0x10	// not supported
#define _SO_TLS_RSA_DECRYPT_HOOK 	0x20	// not supported
#define _SO_TLS_CERT  			    	0x40

// _SO_TLS_CLIENT has been changed from 0x03 to 0x80 to avoid bitwise 
// conflicts with _SO_TLS_CIPHERS _SO_TLS_FLAGS. 
#define _SO_TLS_CLIENT  				0x80  

/*
** Actual TLS Ioctl commands
*/
#define SO_TLS_GET_FLAGS 	(IOC_IN |_SO_TLS|_SO_TLS_FLAGS)
#define SO_TLS_SET_FLAGS 	(IOC_OUT|_SO_TLS|_SO_TLS_FLAGS)
#define SO_TLS_GET_CIPHERS	(IOC_IN |_SO_TLS|_SO_TLS_CIPHERS)
#define SO_TLS_SET_CIPHERS	(IOC_OUT|_SO_TLS|_SO_TLS_CIPHERS) //not supported
#define SO_TLS_GET_CLIENT	(IOC_IN |_SO_TLS|_SO_TLS_CLIENT)
#define SO_TLS_SET_CLIENT	(IOC_OUT|_SO_TLS|_SO_TLS_CLIENT)
#define SO_TLS_GET_SERVER	(IOC_IN |_SO_TLS|_SO_TLS_SERVER)
#define SO_TLS_SET_SERVER	(IOC_OUT|_SO_TLS|_SO_TLS_SERVER)
#define SO_TLS_GET_CERT		(IOC_IN |_SO_TLS|_SO_TLS_CERT)

//not supported
#define SO_TLS_GET_AUTH_CERT_HOOK 	(IOC_IN |_SO_TLS|_SO_TLS_AUTH_CERT_HOOK)
//not supported
#define SO_TLS_SET_AUTH_CERT_HOOK 	(IOC_OUT|_SO_TLS|_SO_TLS_AUTH_CERT_HOOK)
//not supported
#define SO_TLS_GET_RSA_ENCRYPT_HOOK	(IOC_IN |_SO_TLS|_SO_TLS_RSA_ENCRYPT_HOOK)
//not supported
#define SO_TLS_SET_RSA_ENCRYPT_HOOK	(IOC_OUT|_SO_TLS|_SO_TLS_RSA_ENCRYPT_HOOK)
//not supported
#define SO_TLS_GET_RSA_DECRYPT_HOOK	(IOC_IN |_SO_TLS|_SO_TLS_RSA_DECRYPT_HOOK)
//not supported
#define SO_TLS_SET_RSA_DECRYPT_HOOK	(IOC_OUT|_SO_TLS|_SO_TLS_RSA_DECRYPT_HOOK)

#define SO_TLS_ENABLE 				0x0001
#define SO_TLS_SERVER 				0x0002
#define SO_TLS_AUTH_CLIENT 			0x0004
#define SO_TLS_ACCEPT_WEAK    		0x0008	 //not supported
#define SO_TLS_MAP_DISABLE			0x0010
#define SO_TLS_MAP_IDENTITY			0x0020
#define SO_TLS_BLIND_ACCEPT			0x0040
#define SO_TLS_INTERACTIVE_ACCEPT	0x0080

#ifndef int32
#define int32	int
#endif

struct tlscipheropts {
	int n;
	char specs[3];
};

struct tlsclientopts {
	unicode *wallet;
	int walletlen;
	time_t sidtimeout;
	int32 sidentries;
	char *siddir;
	void *options;
};

struct tlsserveropts {
	unicode *wallet;
	int 	walletlen;
	time_t 	sidtimeout;
	int32 	sidentries;
	char 	*siddir;
	void 	*options;
};

struct nwtlsopts{
        unicode *walletProvider;     //wallet content provider e.g. PFX, KMO, DER.
        unicode **keysList;	     //alias for private key in wallet to be used
                                     //  not used for anything but pfx wallet provider
        int numElementsInKeyList;    //number of elements in the array
        unicode **TrustedRootList;   //array of trusted root names
        int numElementsInTRList;     //number of elements in the array
        void *reservedforfutureuse;  //reserved to set ciphers
        void *reservedforfutureCRL;  //reserved for CRL
        int reservedforfutureCRLLen; //reserved for CRL len.
        void *reserved1;	
        void *reserved2;	
        void *reserved3;	
};

//not suppported
struct tlsauthcertopts {
	int type;
	int (*func)(void *arg, char *cert, int len);
	void *arg;
};

#define TLS_ACK_OK	  			1	//not supported 
#define TLS_ACH_WEAK_OK			2	//not supported 
#define TLS_ACH_LONG_DATA  		3	//not supported 
#define TLS_ACH_BAD_DATA   		4	//not supported 
#define TLS_ACH_BAD_SIG    		5	//not supported 
#define TLS_ACH_CERT_EXPIRED 	6	//not supported 

//not suppported
struct tlsrsaencrypthook {
	int (*func)(void *arg, int blockType, char *dest, int *destlen, char *src,
		int srclen);
	void *arg;
};

#define TLS_REH_OK    		0	//not supported 
#define TLS_REH_BAD_TYPE 	1	//not supported 
#define TLS_REH_BAD_LEN 	2	//not supported 

//not suppported
struct tlsrsadecrypthook {
	int (*func)(void *arg, int blockType, char *dest, int *destlen, char *src,
		int srclen);
	void *arg;
};

#define TLS_RDH_OK    		0	//not supported 
#define TLS_RDH_BAD_TYPE 	1	//not supported 
#define TLS_RDH_BAD_LEN 	2	//not supported 

struct tlscert {
	char *cert;
	int certlen;
};


#if defined unicode
#undef unicode
#endif

#endif


#undef WIN32
#endif
