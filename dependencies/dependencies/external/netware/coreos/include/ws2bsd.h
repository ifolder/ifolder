/* This header is used for the WinSock and BSD coexistence
SMDR will use either BSD or WinSock based on the load time switch
It would default to BSD due to performance reason, but might be
switched to WinSock for time tested solution
*/
#ifndef _WS2BSD_H_
#define _WS2BSD_H_
#include <sys/bsdskt.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <ws2nlm.h>

#define BSD 0x0001
#define WS2 0x0002

typedef struct _SMsockVTBL SMsockVTBL;

/* Socket function prototypes */

typedef int	(*_SMBSDconnect)(SOCKET s,	
								struct sockaddr * name, 
								int namelen);

typedef SOCKET (*_SMBSDaccept)(SOCKET s,
								struct sockaddr * addr,
								int * addrlen);

typedef int	(*_SMWSAconnect)(SOCKET s,	
								struct sockaddr * name, 
								int namelen,
								LPWSABUF lpCallerData,
								LPWSABUF lpCalleeData,
								LPQOS lpSQOS,
								LPQOS lpGQOS);

typedef SOCKET (*_SMWSAaccept)(SOCKET s,
								struct sockaddr * addr,
								int * addrlen,
								LPCONDITIONPROC lpfnCondition,
								DWORD dwCallbackData);

typedef int (*_SMbind)(SOCKET s,
						struct sockaddr FAR * name,
						int namelen);

typedef int (*_SMclosesocket)(SOCKET s);


typedef int (*_SMgetpeername)(SOCKET s,
								struct sockaddr FAR * name,
								int FAR * namelen);

typedef int (*_SMgetsockname)(SOCKET s,
								struct sockaddr FAR * name,
								int FAR * namelen);

typedef int (*_SMgetsockopt)(SOCKET s,
								int level,
								int optname,
								char FAR * optval,
								int FAR * optlen);

typedef u_long (*_SMhtonl)(u_long hostlong	);

typedef u_short (*_SMhtons)(u_short hostshort);

typedef unsigned long (*_SMinet_addr)(const char FAR * cp);

typedef char * (*_SMinet_ntoa)(struct in_addr in);

typedef int (*_SMlisten)(	SOCKET s,
						int backlog);

typedef u_long (*_SMntohl)(u_long netlong);
	
typedef u_short (*_SMntohs)(u_short netshort);
	
typedef int (*_SMrecv)(SOCKET s,
						char FAR * buf,
						int len,
						int flags);
	
typedef int (*_SMrecvfrom)(SOCKET s,
							char FAR * buf,
							int len,
							int flags,
							struct sockaddr FAR * from,
							int FAR * fromlen);
	
typedef int (*_SMselect)(int nfds,
						fd_set FAR * readfds,
						fd_set FAR * writefds,
						fd_set FAR *exceptfds,
						const struct timeval FAR * timeout);
	
typedef int (*_SMsend)(SOCKET s,
						const char FAR * buf,
						int len,
						int flags);
	
typedef int (*_SMsendto)(SOCKET s,
						const char FAR * buf,
						int len,
						int flags,
						const struct sockaddr FAR * to,
						int tolen);
	
typedef int (*_SMsetsockopt)(SOCKET s,
							int level,
							int optname,
							const char FAR * optval,
							int optlen	);
	
typedef int (*_SMshutdown)(SOCKET s,
							int how);
	
typedef SOCKET (*_SMsocket)(int af,
								int type,
								int protocol);
	
	/* Database function prototypes */
	
typedef HOSTENT FAR * (*_SMgethostbyaddr)(const char FAR * addr,
											int len,
											int type);
	
typedef HOSTENT FAR * (*_SMgethostbyname)(const char FAR * name	);
	
typedef int (*_SMgethostname)(	char FAR * name,
								int namelen);

typedef struct servent SERVENT;

typedef SERVENT FAR * (*_SMgetservbyport)(int port,
											const char FAR * proto);
	
typedef SERVENT FAR * (*_SMgetservbyname)(const char FAR * name,
												const char FAR * proto);
	
typedef SERVENT FAR * (*_SMgetprotobynumber)(int number);
	
typedef SERVENT FAR * (*_SMgetprotobyname)(const char FAR * name);

struct _SMsockVTBL
{
	_SMBSDaccept 		SMBSDaccept;
	_SMWSAaccept 		SMWSAaccept;
	_SMbind 		SMbind;
	_SMclosesocket 		SMclosesocket;
	_SMBSDconnect 		SMBSDconnect;
	_SMWSAconnect 		SMWSAconnect;
	_SMgetpeername 		SMgetpeername;
	_SMgetsockname 		SMgetsockname;
	_SMgetsockopt 		SMgetsockopt;
	_SMhtonl 		SMhtonl;
	_SMhtons 		SMhtons;
	_SMinet_addr 		SMinet_addr;
	_SMinet_ntoa 		SMinet_ntoa;
	_SMlisten 		SMlisten;
	_SMntohl 		SMntohl;
	_SMntohs 		SMntohs;
	_SMrecv 		SMrecv;
	_SMrecvfrom 		SMrecvfrom;
	_SMselect 		SMselect;
	_SMsend 		SMsend;
	_SMsendto 		SMsendto;
	_SMsetsockopt 		SMsetsockopt;
	_SMshutdown 		SMshutdown;
	_SMsocket 		SMsocket;
	_SMgethostbyaddr 		SMgethostbyaddr;
	_SMgethostbyname 		SMgethostbyname;
	_SMgethostname 		SMgethostname;
	_SMgetservbyport 		SMgetservbyport;
	_SMgetservbyname 		SMgetservbyname;
	_SMgetprotobynumber 		SMgetprotobynumber;
	_SMgetprotobyname 		SMgetprotobyname;
	
};

#endif //_WS2BSD_H_

