/*****************************************************************************
 *
 *	(C) Copyright 1998 Novell, Inc.
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
 *  $Workfile:   clstrlib.h  $
 *  $Modtime:   $
 *  $Revision$
 *
 ****************************************************************************/

#ifndef _CLSTRLIB_H
#define _CLSTRLIB_H

#include <dconst.h>
#include <dtypes.h>


#define MAX_NODES 32
#define LOAD_UNLOAD_SCRIPT_SIZE 600
typedef enum _CLSTR_ERRORS {
	CLSTR_OK, 
	CLSTR_UNKNOWN,
	CLSTR_BAD_PARAMETER,
	CLSTR_ALLOC_ERROR,
	CLSTR_NODE_NOT_FOUND,
	CLSTR_IP_DUPLICATE
} CLSTR_ERRORS;

#ifndef IN
#define IN
#define OUT
#endif

/* Resource structure passed by the CRM to be filled by the clstlib */
typedef struct{

	struct ResourceTagStructure *allocTag; /* Filled in by the caller     */
	/* The name field is filled by the caller when a new resource is      */
	/* online by the GUI. The GUI supplies the name of the new resource.  */
	UINT8  name[MAX_RDN_CHARS+1];/* Name filled by the caller             */
	UINT32 loadWait;	/* Load wait time filled by the ReadApp call  */
	UINT32 unloadWait;	/* Unload wait time filled by the ReadApp call*/
	UINT32 failOver;	/* FailOver policy filled by the ReadApp call */
	UINT32 failBack;	/* FailBack policy filled by the ReadApp call */
	UINT32 ignore_quorum;	/* ignore_quorum filled by the ReadApp call   */
	UINT32 revision;	/* revision filled by the ReadApp call        */
	UINT8 fail_order[MAX_NODES];/*     fail_order filled by the ReadApp   */
	/* Caller fills in the Unicode name of the object. The clstrlib       */
	/* resolve this name and fills the information desired by the caller. */
	UINT8 *loadScript;	/* LoadScript filled by CRMSaveScript call    */
	UINT8 *unloadScript;	/* UnloadScript filled by CRMSaveScript call  */
	UINT8  uniObjName[MAX_DN_CHARS * 2 + 4];
}RESOURCE;

/* The size of unicode name must be either MAX_DN_BYTES+2, MAX_RDN_BYTES+2.   */
/* All the strings must be of MAX_RDN_CHARS+1 size long.                      */
/* RetryErrorFlag = TRUE (The library retries the API for retry kind of error.*/
/* RetryErrorFlag = FALSE(The library returns the errors to the caller)       */
/* RetryErrorFlag = FALSE(The library sets this field to TRUE or FALSE if the */
/*                        NDS error can be retried.                           */

/* Used for WherePools call to fill in with the resource information */

typedef struct{
	UINT16 VolFDN [ MAX_RDN_CHARS+2];
	UINT8  VolName[ 128];
}VOLUME_INFO;

typedef struct{
	UINT16  ResFDN[ MAX_RDN_CHARS+2];    // Cluster Resource unicode NDS FDN 
	UINT16  PoolFDN[ MAX_RDN_CHARS+2];    // Pool unicode NDS FDN 
	UINT16  VirtServFDN[ MAX_RDN_CHARS+2];		// Hold the Virtual Server FDN
	UINT8	  ResName[ 128];					// ASCII resource name used by Crm
	UINT8   PoolName[ 128];					// physical poolname for NSS
	UINT32  numVols;
	UINT32  WhichNode;      	// 0 based, -1 if not running on any node.
	UINT32  ResourceIP;			// IP address from virtual server.
	void 	  *otherHooks;			// for third party hooks to use as desired
	VOLUME_INFO	*volArray;		// to be allocated by the caller when doing
										// a WhatVolumes call
}POOL_INFO;

CLSTR_ERRORS NWCLSTR_PortNo( UINT32 *PortNo );

CLSTR_ERRORS NWCLSTR_Revision( UINT32 *Revision );

CLSTR_ERRORS NWCLSTR_NodeNumber( UINT32 *NodeNum );

CLSTR_ERRORS NWCLSTR_MyNodeIPAddress( UINT32 *IpAddress );

CLSTR_ERRORS NWCLSTR_HeartBeat( UINT32 *HeartBeat );

CLSTR_ERRORS NWCLSTR_Tolerance( UINT32 *Tolerance );

CLSTR_ERRORS NWCLSTR_CICPRead( UINT8 *cBuff, UINT32 *buf_len );

CLSTR_ERRORS NWCLSTR_ClusterName( UINT8 *cBuff, UINT32 *buf_len );

CLSTR_ERRORS NWCLSTR_NodeNumToName( UINT32 NodeNum, UINT8 *Name );

CLSTR_ERRORS NWCLSTR_GetNodeConfig(UINT32 *NodeBitMask, UINT32 *NumberOfNodes);

CLSTR_ERRORS NWCLSTR_CRMReadPriList(BYTE **list, struct ResourceTagStructure *rtag,UINT32 *RetryErrorFlag);

CLSTR_ERRORS NWCLSTR_CRMReadApp( RESOURCE *Res, UINT32 *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_CRMConfig( UINT32 *PortNum,
                                UINT32 *Quorum,
                                UINT32 *MinWait );

/* Type = TRUE  NDS_UNLOAD_SCRIPT */
/* Type = FALSE NDS_LOAD_SCRIPT   */
/* BuffPtr should be big enough to hold the script LOAD_UNLOAD_SCRIPT_SIZE */

CLSTR_ERRORS NWCLSTR_CRMScriptMem( RESOURCE *Res,
                                   UINT32   Type,
                                   UINT8    *BuffPtr,
                                   UINT32   *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_CRMBuildList(
                      void (*ResCB) (UINT8 *rdn, unicode *Dn, UINT32 DnLen),
                      void (*NodeCB)(UINT32 nodeNo, UINT32 nodeNetAddr),
                      UINT32        *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_CRMNewRes(UINT8 *resName,
                          void (*ResCB) (UINT8 *rdn, unicode *Dn, UINT32 DnLen),
                          UINT32 *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_IDToName( UINT32 TrusteeID,
                               UINT8  *UnicodeName,
                               UINT32 *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_NameToID( UINT32 *TrusteeID,
                               UINT8  *UnicodeName,
                               UINT32 *RetryErrorFlag );

CLSTR_ERRORS NWCLSTR_CRMInitRes ( RESOURCE *Res,
                                  UINT32   *RetryErrorFlag);

void         NWCLSTR_Abend(STR *message);

CLSTR_ERRORS NWCLSTR_SetOnOffLine( RESOURCE *Res, 
											  UINT32 OnOffFlag,
											  UINT32 Read_Only);

CLSTR_ERRORS NWCLSTR_ClusterIPAddress( UINT32 *IPNo, 
													UINT32 SaveFlag,
													UINT32 SavePortNo);

CLSTR_ERRORS NWCLSTR_CVSBind( UINT8 *ServerName, 
										UINT8 *IpAddress, 
										UINT32 RegisterFlag);

CLSTR_ERRORS NWCLSTR_RetrieveDN( RESOURCE *Res, 
											UINT32 *RetryErrorFlag, UINT32 AlreadyLocked);

CLSTR_ERRORS NWCLSTR_DHCPStart(BYTE *context);

CLSTR_ERRORS NWCLSTR_IsThis6Pack(void);

CLSTR_ERRORS NWCLSTR_AdminEmails( UINT32 WriteFlag, 
											 UINT8 *RecipList[], 
											 UINT32 *numRecips, 
											 UINT32 *BitFilter);

#define EMAIL_OFF   		1
#define EMAIL_CRITICAL	2
#define EMAIL_VERBOSE	4
// default will be human readable, or bit 8 off
#define EMAIL_XML			8

// Following the PCLUSTER.H format of 0 normal, 1 volume, 2 template
// BACKUP means go closer to the root.
typedef enum{ RES_NORMAL, RES_VOLUME, RES_TEMPLATE, RES_NCP, NCP_SERV_TYPE,
	ORG_TYPE, ORG_UNIT_TYPE, VOLUME_TYPE, BACKUP
	}BACKTYPE;

// Write Flag is 3 way: 0 is read, 1 is update, 2 is new
								
CLSTR_ERRORS NWCLSTR_ResVol( RESOURCE *Res, UINT16 *VolServName,
                                UINT16 **VolName, UINT32 WriteFlag,
                                UINT32 *NumNames, UINT16 *PoolName, UINT32 IsPool);

CLSTR_ERRORS NWCLSTR_UpdateVol( unicode *VolServName, unicode *NewName, 
	unicode *OldName);

// Write Flag is 3 way: 0 is read, 1 is update, 2 is create new Virtual Server
// 3 is add attributes on create.

CLSTR_ERRORS NWCLSTR_PortalVolServ( UINT8 *VolServName, UINT32 *NetAddress, 
                                  UINT32 WriteFlag, unicode *ClustVolName,
                                  unicode *RealVolName, unicode *VolServUDN,
                                  unicode *virtServContainer,
                                  UINT32 AlreadyClustered, UINT32 IsPool );

/* NewFlag = 0 is udpate attributes, 
		1 is create regular resource, 
		2 is create volume resource
		3 is create template resource
*/

CLSTR_ERRORS NWCLSTR_UpdateRes( RESOURCE *Res, UINT32 NewFlag, UINT32 *RetryErrorFlag);

CLSTR_ERRORS NWCLSTR_DelObject( RESOURCE *Res, unicode *anObj, 
										UINT32 *RetryErrorFlag);

CLSTR_ERRORS NWCLSTR_GetClusterO( UINT8 *Name);

CLSTR_ERRORS NWCLSTR_ListObjs(void(*ObjCB)(), BACKTYPE ObjType, 
	UINT8 *ContextDN, UINT32 *RetryErrorFlag);

/*
OprFlag 1 read for NCS:Netware Cluster in NCP Server Object,
 2 Add NCS:NetwareCluster to NCP Server Object and add object to Cluster Container,
 3 Del Node from Cluster Container and remove NCS:Netware Cluster attribute
 4 Update IP Addr or Node Number in Node.
*/


CLSTR_ERRORS NWCLSTR_UpdateClusterNode( unicode *NodeUDN, 
													 UINT8 *NodeName, 
													 UINT32 OprFlag, 
													 UINT32 nodeNum, 
													 UINT32 *netAddr, 
													 UINT32 *UDNFound);

CLSTR_ERRORS NWCLSTR_ConfigWatch(UINT32 *MasterWatch, 
									UINT32 *SlaveWatch, 
									UINT32 *MaxRetrans, 
									UINT8 *clusterGUID);

CLSTR_ERRORS NWCLSTR_UpdateConfig( UINT32 *Tolerance,
											  UINT32 *Heartbeat,
                              	  UINT32 *Quorum,
                              	  UINT32 *MinWait,
                              	  UINT32 *MasterWatch,
                              	  UINT32 *SlaveWatch,
                              	  UINT32 *MaxRetrans,
                              	  UINT8  *configStream );

CLSTR_ERRORS NWCLSTR_GenVolNames( unicode *OldVolUDN, 
                                  UINT8 *NewVolResName, 
                                  UINT8 *NewVirtServName,
                                  UINT8 *NewVirtVolName,
                                  UINT8 *RawName,
                                  UINT8 *NewPoolName,
                                  UINT8 *RawPoolName, 
                                  UINT32 *AlreadyClustered,
                                  unicode *OldPoolUDN,
                                  unicode *OldVirtServUDN,
                                  unicode *OldClustResUDN,
                                  UINT32 IsPool );
                                  

CLSTR_ERRORS NWCLSTR_GenFullVol( unicode *oldVolUDN,
											UINT8 *newClustResName, 
											UINT8 *newVirtServName, 
											UINT8 *newVirtVolName, 
											UINT8 *newVirtPoolName,
											unicode *newClustVolUDN, 
											unicode *newVirtServUDN, 
											unicode *newVirtVolUDN, 
											unicode *virtServContainer, 
											unicode *newVirtVolUni,
											unicode *newVirtPoolUDN,
											unicode *newVirtPoolUni,
											UINT32 AlreadyClustered,
											UINT32 IsPool );

CLSTR_ERRORS NWCLSTR_NewConfig(  UINT32 WriteFlag, 
											UINT8 *IsolateStr,
											UINT8 *PriorityStr,
											UINT32 strSizes);

CLSTR_ERRORS NWCLSTR_NodeNumToUDN( UINT32 NodeNum, UINT8 *Name );
											
CLSTR_ERRORS NWCLSTR_NumPools( UINT32 *NumPools);

CLSTR_ERRORS NWCLSTR_WherePools( UINT32 *numPools, POOL_INFO *PoolArray); 

CLSTR_ERRORS NWCLSTR_WhatVolumes( POOL_INFO *pPool); 

/* Look for ProposedIP in clusterip, node ip's and resource IP's. If found,
 return CLSTR_IP_DUPLICATE, else return 0.
 If only using this routine to find the next VolID, then set the IP address
 to -1 or something which won't be found in the cluster.
 
 workType	1 = Find IP, and next VolID
 				2 = Find Pool name in load script, return Resource FDN
 				3 = Change Pool name in all load and unload scripts
 				4 = Change Volume name in all load scripts
 
*/


CLSTR_ERRORS NWCLSTR_UsedIP( UINT32 *CheckIP, UINT32 *newVolID, UINT32 AlreadyLocked,
	OUT UINT16 *ResFDN, IN UINT32 workType, UINT8 *PoolVolName, UINT8 *NewPoolVolName,
	void *pThd );

#define NCP_ADVERTISE	1
#define CIFS_ADVERTISE	2
#define AFP_ADVERTISE	4

CLSTR_ERRORS NWCLSTR_ClusterEnable( IN UINT8 *Fdn, IN UINT8 *strIP, 
	IN UINT8 *specialVirt, IN UINT8 *CIFSVirt, IN UINT32 AutoOnline, OUT UINT16 *UDNClustRes, 
	IN UINT32 Advertise, IN UINT32 IsPool, IN UINT32 OtherNDSContext);


typedef enum{
	POOL_RENAME, POOL_DELETE, 
	POOL_ADD, VOLUME_RENAME, 
	VOLUME_DELETE, VOLUME_ADD
}POOL_WORK;

CLSTR_ERRORS NWCLSTR_PoolNames( IN UINT8 *PoolName, OUT UINT8 *ClusterPool,
	OUT UINT8 *ClusterVirtServer, OUT UINT8 *ClusterCIFSVirtServer);


CLSTR_ERRORS NWCLSTR_PoolVolActivity( IN POOL_WORK workType, UINT16 *PoolName,
	UINT16 *VolName, UINT16 *NewName);


/* 
 * Following are the prototypes to support the various cluster stats.
 */

void NWCLSTR_Clear_Stats();
void NWCLSTR_Display_Stats();
void NWCLSTR_Sbd_Tick();
void NWCLSTR_Node_Tick(UINT32 nodeNumber);
void NWCLSTR_Enable_Stats(UINT32 joinBitmask, UINT32 failBitmask, UINT32 leaveBitmask);

#endif /* _CLSTRLIB_H */
