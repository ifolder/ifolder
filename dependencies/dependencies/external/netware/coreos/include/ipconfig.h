
/**************************************************************************
 * File name   : IpConfig.h
 *
 * Description : This file contains the definition of messages passed
 *               between the Applications & the IpAddressMgmt frame work.
 *
 * Author      : Manjunath, Anoop, Gaurav
 *
 * Date        : Oct/30/2002
 *
 ***************************************************************************
 */

#ifndef _IPCONFIG_H
#define _IPCONFIG_H
#include <sys/types.h>
#include <time.h>
#include <netinet\in.h>

/* --------------------------------------------------------------------------- */
/* 0xMMMMmmmm MMMM - Major version mmmm Minor Version */
/* This needs to be changed each time this file is updated */
/* There will be a Version number synch message */
/* at start up  before initiating any request or response messages */
/* To ensure that applications are using the same message formats */
/* as the Frame work. If there is a mismatch then the applications */
/* operational image  has to be upgraded/downgraded */
/* --------------------------------------------------------------------------- 
*/
#define IPMGMT_MAJOR_VERSION 	1	
#define IPMGMT_MINOR_VERSION 	3
#define IPMGMT_VERSION      	((IPMGMT_MAJOR_VERSION << 16) | IPMGMT_MINOR_VERSION)

#define MAX_NAME_SIZE			20	/* Max size of the service/app/ */
#define MAX_APP_DES_SIZE 	64
#define MAX_DNS_NAME			80
#define ADDR_PORT_DES_SIZE		64
#define CONTEXT_INFO_SIZE		128

#define	 IPDEFAULT		0
#define	 IPPRIVATE		1
#define	 IPPRIMARY		2
#define	 IPSECONDARY	3

/* #defines for the policies ( local & global)  Begin */
#define 		POLICY					1
#define	 	SKIPCR					(POLICY<<0)
#define		CHANGEALL_IP			(POLICY<<1)
#define		CHANGEALL_PORT		(POLICY<<2)
#define		IP_NOEDIT				(POLICY<<3)
#define		PORT_NOEDIT			(POLICY<<4)
/* END */

/*Error codes */
/* These are the new error codes returned by IpMgmtChangeIPAddress*/
/*14/1/2003*/
#define 		ERR_INVALID_DESCRIPTION		 0x0001
#define 		ERR_IPMGMT_LOADING			 0x0002
#define 		ERR_MEMORY_FAILURE	 		 0x0003
#define 		ERR_INVALID_IP_ARGUMENT		 0x0004
#define 		ERR_CONFIG_NOT_FOUND			 0x0005
#define 		ERR_INVALID_IPV4_ADDRESS		 0x0006
#define 		ERR_INVALID_IPV4_CLASS		 0x0007
#define 		ERR_INVALID_DNS_NAME			 0x0008
#define 		ERR_INVALID_ADDR_TYPE		 0x0009
#define 		ERR_INVALID_NOT_CONFIG		 0x000A
/*Error codes */

typedef enum IpAddressType
{
	ipv4,
	ipv6,
	dns
} IpAddressType;

typedef enum TransportType
{	
	tcp,
	udp
} TransportType;

typedef union IpAddr 
{
	struct in_addr   ipv4_addr;
	struct in6_addr  ipv6_addr;
	char 		 dnsName[MAX_DNS_NAME];
} IpAddr;


/*All Ascii strings must be a NULL terminated string.*/

typedef struct IpAddressInfo
{
	u_short 		ipAddrType;		/*Enum IpAddressType*/
	u_short		ipAddrCategory ;	/*Refer # defines for IpAddressCategory*/
	u_short		protocol; 		/*Enum transportType*/
	IpAddr		ipAddress;		/*Union-for ipv4, ipv6 or DNS*/
	u_short		portNum;
	u_short		cfgStatus; 		/*Enum CfgStatus*/
	u_int		localPolicy;
	u_char		addressPortDescription[ADDR_PORT_DES_SIZE];	
	u_char		ContextInformation[CONTEXT_INFO_SIZE];
	void 		*res_ipEntry;
}IpAddressInfo;

#define LONG_MAX     2147483647        /* max value of 'long int' - Anoop 13052003*/

typedef enum CfgStatus
{
/*
	 CfgStatus:  Note that the CfgStatus will reflect the status 
	 of the response messages.
	 Request messages from Application should have CfgStatus=NoError.
	 Response messages from Framework will have 
	 CfgStatus set to any of the defined CfgStatus values.
*/
	noError,
	invalidVersion,
	portConflict,
	cfgResolved,
	invalidAppName,
	noBuffer,
	invalidIpInfo,
	invalidPortInfo,
	otherErrors,
	enumConversion=LONG_MAX-1 /*Forcing compiler to take this as int Anoop 13052003 */
} CfgStatus;


typedef struct ServiceConfigEntry 	
{
u_int		versionInfo; 					/*  IPMGMT_CFG_VERSION */
u_char		serviceName[MAX_NAME_SIZE]; 			/* Service Name */
u_char		serviceDescription[MAX_APP_DES_SIZE]; 	/* Service Description */
u_short		numIpAddrInfo;		  				/* Number of IpAddressInfo entries*/
IpAddressInfo	*ipCfgInfo;							/* pointer to an array of IPAddressInfo*/
CfgStatus	cfgStatus;							/* Enum CfgStatus*/
u_int		globalPolicy;
void 		*res_configEntry;
}ServiceConfigEntry;



	/* --------Interface structure for  intermediate XML file Format --------*/

#define MAX_HANDLER_SIZE		64
#define MAX_IPADDRESS_SUPPORT 10

typedef struct IntermediateNode 
{
	struct IntermediateNode *next;
	struct IntermediateNode *prev;
	struct IntermediateServiceConfigEntry *cfgData; // Refer IPAddressMgmt.h
}IntermediateNode;

typedef struct IntermediateServiceConfigEntry 	
{
u_int		versionInfo; 					/*  IPMGMT_CFG_VERSION */
u_char		serviceName[MAX_NAME_SIZE]; 			/* Service Name */
u_char		serviceDescription[MAX_APP_DES_SIZE]; 	/* Service Description */
u_short		numIpAddrInfo;		  		/* Number of IpAddressInfo entries*/
IpAddressInfo ipCfgInfo[MAX_IPADDRESS_SUPPORT];			/* pointer to an array of IPAddressInfo*/
u_char		Handler[MAX_HANDLER_SIZE];
u_short		cfgStatus;					/* Refer to Enum CfgStatus*/
u_int		globalPolicy;
void 		*res_configEntry;

}IntermediateServiceConfigEntry;

/* -----------Structure for NRM  Utility ONLY ----------*/
#define CONFIGLIST    1
#define CONFLICTLIST  2
#define INVALIDLIST   3
#define NRM_IP_LENGTH 64

typedef struct NRMConfigEntry {
u_char		serviceName[MAX_NAME_SIZE]; 
u_char 		addressPortDescription[ADDR_PORT_DES_SIZE];
char 		IPInfo[NRM_IP_LENGTH];
u_short		portNum;
u_short		protocol;
int			Category;
u_int		policy;
u_short		status; //Anoop added for NRM
}NRMConfigEntry;

	/* ----------------API  Support---------------- */
/*This API can be used by the appliations to register with the framework.*/
	int IpMgmtRegisterApp(struct ServiceConfigEntry * );
	/* The Return value of this API */
	#define REGISTER_SUCCESS	0
	#define REGISTER_FAILURE	(-1)
	#define REGISTERED_ALREADY	 1
	
/*This API can be used by the appliations to get the configuration from the framework.*/
	void IpMgmtGetCfgInfo(ServiceConfigEntry *);

/* Based on the config status applications can avail or make use the following APIs 
	for any further assistance from the framework.	
	
This API can be used to get the list of configured IpAddresses from the framework */
	int IpMgmtGetValidIPAddress(char *IPArray[], int IPCat[]);
	/* where IPArray is char IPArray[256][20]; */
	/* where IPCat is the category and is int IPCat[256] */

/*This API can be used by the appliations to change thier configuration with the framework.*/
	int IpMgmtChangeIPAddress(char *AppName, u_short Protocol, char * OldIp, u_short  OldPort, char *NewIp, u_short NewPort);

/*This API can be used by the appliations to change thier configuration with the framework. This will work for applications which want to change their code for Beta-3 Enhancements*/
	int IpMgmtChangeIPAddress_V1dot3(char *AppName, u_short Protocol, char * OldIp, u_short  OldPort, char *NewIp, u_short NewPort, u_int policy, char *description);


/*This ApI can be used by the application to deregister from the framework*/
	int IpMgmtDeregister(char * AppName);
	/* where AppName is char AppName[20]; */

/*This ApI can be used by the application to get the default IpAddress  from the framework*/
	void	IpMgmtGetDefaultIp(char *IpAddr);
	/* where IpAddr is char IpAddr[20]; */



#endif
