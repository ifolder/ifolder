/*
===============================================================================
Novell Software Developer Kit Sample Code License

Copyright (C) 2003-2004 Novell, Inc.  All Rights Reserved.

THIS WORK IS SUBJECT TO U.S. AND INTERNATIONAL COPYRIGHT LAWS AND TREATIES.  
USE AND REDISTRIBUTION OF THIS WORK IS SUBJECT TO THE LICENSE AGREEMENT 
ACCOMPANYING THE SOFTWARE DEVELOPMENT KIT (SDK) THAT CONTAINS THIS WORK.  
PURSUANT TO THE SDK LICENSE AGREEMENT, NOVELL HEREBY GRANTS TO DEVELOPER 
A ROYALTY-FREE, NON-EXCLUSIVE LICENSE TO INCLUDE NOVELL'S SAMPLE CODE IN ITS 
PRODUCT.  NOVELL GRANTS DEVELOPER WORLDWIDE DISTRIBUTION RIGHTS TO MARKET, 
DISTRIBUTE, OR SELL NOVELL'S SAMPLE CODE AS A COMPONENT OF DEVELOPER'S PRODUCTS.
NOVELL SHALL HAVE NO OBLIGATIONS TO DEVELOPER OR DEVELOPER'S CUSTOMERS WITH 
RESPECT TO THIS CODE.

NAME OF FILE:
	smstsapi.h

PURPOSE/COMMENTS:
	 This file contains function prototypes for parameter checking and function macros 
	 for pointer acquisition. The macros require that the typedefs and SMSPCODE defines 
	 be spelled exactly the same after the initial SMSP_.

FILES USED:
	None

NDK COMPONENT NAME AND VERSION:
	SMS Developer Components

DATE CREATED:
       2 March 1992

LAST MODIFIED DATE: 
	22 July 1993

	

===============================================================================
*/

#ifndef _SMSTSAPI_H_INCLUDED      /* smstsapi.h header latch */
#define _SMSTSAPI_H_INCLUDED

#ifdef __cplusplus
extern "C" {
#endif

#include <smstypes.h>
#include <smstserr.h>

#if defined(NWWIN)
#	if defined(API)
#		undef API
#	endif
#	define	API	pascal far
#else
#	if defined(API)
#		undef API
#	endif
#	define	API
#endif


/*
 *	Function Prototypes
 */

typedef 
CCODE API _NWSMTSConnectToTargetService(
			UINT32					 *connectionID, 
			STRING					  targetServiceName,
			STRING					  targetUserName, 
			void					 *authentication);
typedef
CCODE API _NWSMTSAuthenticateTS(
			UINT32                  *connectionHandle,
           STRING                   targetServiceName,
           UINT32					 authType,
           NWSM_LongByteStream     *authData);

typedef 
CCODE API _NWSMTSReleaseTargetService(
			UINT32					 *connectionID);

typedef
CCODE API _NWSMTSReturnToParent(
			UINT32					  connectionID, 
			UINT32					 *sequence);

typedef
CCODE API _NWSMTSScanTargetServiceName(
			UINT32					  connectionID, 
			UINT32					 *sequence,
			STRING					  pattern, 
			STRING					  name);

typedef
CCODE API _NWSMTSBeginRestoreSession(
			UINT32					  connectionID);

typedef
CCODE API _NWSMTSGetTargetServiceType(
			UINT32					  connectionID, 
			STRING					  name,
			STRING					  type, 
			STRING					  version);

typedef
CCODE API _NWSMTSGetTargetServiceAddress(
			UINT32					  connectionID,
			STRING					  targetServiceName,
			UINT32					  *addressType,
			STRING					  address);

typedef
CCODE API _NWSMTSGetTargetResourceInfo(
			UINT32					  connectionID, 
			STRING					  resource,
			UINT16					 *blocksize, 
			UINT32					 *totalblocks, 
			UINT32					 *freeblocks,
			NWBOOLEAN				 *isRemoveable, 
			UINT32					 *purgableblocks, 
			UINT32					 *unpurgedblocks,
			UINT32					 *migratedSectors,
			UINT32					 *preCompressedSectors,
			UINT32					 *compressedSectors);

typedef
CCODE API _NWSMTSGetUnsupportedOptions(
			UINT32					  connectionID, 
			UINT32					 *unsupportedBackupOptions,
			UINT32					 *unsupportedRestoreOptions);

typedef
CCODE API _NWSMTSScanTSResource(
			UINT32					  connectionID, 
			UINT32					 *sequence,
			STRING					  resource);

typedef
CCODE API _NWSMTSListTSResources(
			UINT32						connectionID, 
			NWSM_NAME_LIST				**serviceResourceList);
			
typedef
CCODE API _NWSMTSScanDataSetBegin(
			UINT32					  connectionID,
			NWSM_DATA_SET_NAME_LIST	 *resourceName,
			NWSM_SCAN_CONTROL		 *scanControl, 
			NWSM_SELECTION_LIST		 *selectionList,
			UINT32					 *sequence, 
			NWSM_SCAN_INFORMATION	**scanInformation, 
			NWSM_DATA_SET_NAME_LIST	**dataSetNames);

typedef
CCODE API _NWSMTSScanDataSetContinue(
		UINT32					  connectionHandle,
		NWSM_DATA_SET_NAME_LIST	 *resourceName,
		NWSM_SCAN_CONTROL		 *scanControl,
		NWSM_SELECTION_LIST		 *selectionList,
		NWSM_DATA_SET_NAME_LIST	 *cursorDataSetName,
		UINT32					 *dsSequence,
		NWSM_SCAN_INFORMATION	**scanInformation,
		NWSM_DATA_SET_NAME_LIST	**dataSetNames);


typedef
CCODE API _NWSMTSScanNextDataSet(
			UINT32					  connectionID, 
			UINT32					 *sequence, 
			NWSM_SCAN_INFORMATION	**scanInformation,
			NWSM_DATA_SET_NAME_LIST	**dataSetNames);

typedef
CCODE API _NWSMTSScanDataSetEnd(
			UINT32					  connectionID, 
			UINT32					 *sequence, 
			NWSM_SCAN_INFORMATION	**scanInformation,
			NWSM_DATA_SET_NAME_LIST	**dataSetNames);

typedef
CCODE API _NWSMTSRenameDataSet(
			UINT32					  connectionID, 
			UINT32					  sequence, 
			UINT32					  nameSpaceType, 
			LSTRING					  newDataSetName);

typedef
CCODE API _NWSMTSDeleteDataSet(
			UINT32					  connectionID, 
			UINT32					  sequence);

typedef
CCODE API _NWSMTSSetArchiveStatus(
			UINT32  				  connectionID, 
			UINT32  				  handle, 
			UINT32  				  setFlag, 
			UINT32  				  archivedDateAndTime);

typedef
CCODE API _NWSMTSOpenDataSetForRestore(
			UINT32					  connectionID, 
			UINT32					  parentHandle, 
			NWSM_DATA_SET_NAME_LIST	 *dataSetName, 
			UINT32					  mode, 
			UINT32					 *handle);

typedef
CCODE API _NWSMTSOpenDataSetForBackup(
			UINT32					  connectionID, 
			UINT32  				  sequence, 
			UINT32  				  mode, 
			UINT32 					 *handle);

typedef
CCODE API _NWSMTSCloseDataSet(
			UINT32					  connectionID, 
			UINT32					 *handle);

typedef
CCODE API _NWSMTSReadDataSet(
			UINT32					  connectionID, 
			UINT32					  handle, 
			UINT32					  bytesToRead, 
			UINT32					 *bytesRead, 
			BUFFERPTR				  buffer);

typedef
CCODE API _NWSMTSWriteDataSet(
			UINT32					  connectionID, 
			UINT32					  handle, 
			UINT32					  bytesToWrite, 
			BUFFERPTR				  buffer);
	
typedef
CCODE API _NWSMTSIsDataSetExcluded(
			UINT32					  connectionID, 
			NWBOOLEAN				  isParent,
			NWSM_DATA_SET_NAME_LIST	 *namelist);

typedef
CCODE API _NWSMTSBuildResourceList(
			UINT32					  connectionID);

typedef
CCODE API _NWSMTSSetRestoreOptions(
			UINT32					  connectionID, 
			NWBOOLEAN				  checkCRC, 
			NWBOOLEAN				  dontCheckSelectionList, 
			NWSM_SELECTION_LIST		 *selectionList);

typedef
CCODE API _NWSMTSParseDataSetName(
			UINT32  				  connectionID, 
			UINT32  				  nameSpaceType,
			STRING  				  dataSetName, 
			UINT16					 *count, 
			UINT16_BUFFER 			**namePositions,
			UINT16_BUFFER 			**separatorPositions);

typedef
CCODE API _NWSMTSCatDataSetName(
			UINT32 					  connectionID, 
			UINT32 					  nameSpaceType,
			STRING 					  dataSetName, 
			STRING 					  terminalName, 
			NWBOOLEAN				  terminalIsParent,
			STRING_BUFFER			**newDataSetName);

typedef
CCODE API _NWSMTSSeparateDataSetName(
			UINT32					  connectionID, 
			UINT32					  nameSpaceType,
			STRING					  dataSetName, 
			STRING_BUFFER 			**parentDataSetName,
			STRING_BUFFER 			**childDataSetName);

typedef
CCODE API _NWSMTSFixDataSetName(
			UINT32					  connectionID, 
			STRING					  dataSetName,
           	UINT32 					  nameSpaceType,
			NWBOOLEAN				  isParent,
			NWBOOLEAN				  wildAllowedOnTerminal,
			STRING_BUFFER			**newDataSetName);

typedef
CCODE API _NWSMTSGetNameSpaceTypeInfo(
			UINT32 					  connectionID,
			UINT32 					  nameSpaceType, 
			NWBOOLEAN				 *reverseOrder, 
			STRING_BUFFER 			**sep1, 
			STRING_BUFFER 			**sep2);

typedef
CCODE API _NWSMTSScanSupportedNameSpaces(
			UINT32 					  connectionID, 
			UINT32					 *sequence,
			STRING 					  resourceName, 
			UINT32					 *nameSpaceType, 
			STRING 					  nameSpaceName);

typedef
CCODE  API _NWSMTSListSupportedNameSpaces(
			UINT32						connectionID, 
			STRING						resourceName,
			NWSM_NAME_LIST 				**nameSpaceList);
		
typedef
CCODE API _NWSMTSGetTargetSelTypeStr(
			UINT32					  connectionID, 
			UINT8					  typeNumber, 
			STRING					  selectionTypeString1, 
			STRING					  selectionTypeString2);

typedef
CCODE API _NWSMTSGetTargetScanTypeString(
			UINT32					  connectionID,
			UINT8					  typeNumber, 
			STRING					  scanTypeString, 
			UINT32					 *required,
			UINT32					 *disallowed);

typedef
CCODE API _NWSMTSGetOpenModeOptionString(
			UINT32					  connectionID, 
			UINT8					  optionNumber, 
			STRING					  optionString);

typedef
CCODE API _NWSMTSConvertError(
			UINT32					  connectionID, 
			CCODE					  error, 
			STRING					  message);

typedef 
CCODE API _NWSMGetVersionInfo(
			UINT32					  connectionID,
			NWSM_MODULE_VERSION_INFO *info);


typedef 
CCODE API _NWSMTSConnectToTargetServiceEx(
			UINT32					 *connectionID, 
			STRING					  targetServiceName,
			STRING					  targetUserName, 
			void					 	  *authentication,
			UINT32					  optionFlag);

typedef 
CCODE API _NWSMTSGetTargetServiceAPIVersion(
			UINT32					  connectionID,
		  	UINT32 					  *MajorVersion,
			UINT32 					  *MinorVersion);
			
typedef 
CCODE API _NWSMTSConfigureTargetService(
			UINT32					  connectionID,
			UINT32					  actionFlag,
			void		 				  *actionData);

 
CCODE API NWSMConnectToTSA(
		char						 *TSA, 
		UINT32						 *connectionID);

CCODE API NWSMReleaseTSA(
		UINT32						 *connectionID);

/*
 * Function Macros
 */

extern _NWSMTSConnectToTargetService    	NWSMTSConnectToTargetService;
extern _NWSMTSAuthenticateTS            		NWSMTSAuthenticateTS;
extern _NWSMTSReleaseTargetService      		NWSMTSReleaseTargetService;
extern _NWSMTSReturnToParent            		NWSMTSReturnToParent;
extern _NWSMTSScanTargetServiceName     	NWSMTSScanTargetServiceName;
extern _NWSMTSBeginRestoreSession       	NWSMTSBeginRestoreSession;
extern _NWSMTSGetTargetServiceType      	NWSMTSGetTargetServiceType;
extern _NWSMTSGetTargetServiceAddress   	NWSMTSGetTargetServiceAddress;
extern _NWSMTSGetTargetResourceInfo     	NWSMTSGetTargetResourceInfo;
extern _NWSMTSScanTSResource            		NWSMTSScanTargetServiceResource;
extern _NWSMTSScanDataSetBegin          		NWSMTSScanDataSetBegin;
extern _NWSMTSScanNextDataSet           		NWSMTSScanNextDataSet;
extern _NWSMTSScanDataSetEnd            		NWSMTSScanDataSetEnd;
extern _NWSMTSRenameDataSet             	NWSMTSRenameDataSet;
extern _NWSMTSDeleteDataSet             		NWSMTSDeleteDataSet;
extern _NWSMTSSetArchiveStatus          	NWSMTSSetArchiveStatus;
extern _NWSMTSOpenDataSetForRestore     	NWSMTSOpenDataSetForRestore;
extern _NWSMTSOpenDataSetForBackup      	NWSMTSOpenDataSetForBackup;
extern _NWSMTSCloseDataSet              		NWSMTSCloseDataSet;
extern _NWSMTSReadDataSet               		NWSMTSReadDataSet;
extern _NWSMTSWriteDataSet              		NWSMTSWriteDataSet;
extern _NWSMTSIsDataSetExcluded         		NWSMTSIsDataSetExcluded;
extern _NWSMTSBuildResourceList         	NWSMTSBuildResourceList;
extern _NWSMTSSetRestoreOptions         		NWSMTSSetRestoreOptions;
extern _NWSMTSParseDataSetName          		NWSMTSParseDataSetName;
extern _NWSMTSCatDataSetName            		NWSMTSCatDataSetName;
extern _NWSMTSSeparateDataSetName       	NWSMTSSeparateDataSetName;
extern _NWSMTSFixDataSetName            		NWSMTSFixDataSetName;
extern _NWSMTSGetNameSpaceTypeInfo      	NWSMTSGetNameSpaceTypeInfo;
extern _NWSMTSScanSupportedNameSpaces   	NWSMTSScanSupportedNameSpaces;
extern _NWSMTSGetTargetSelTypeStr       	NWSMTSGetTargetSelectionTypeStr;
extern _NWSMTSGetTargetScanTypeString   	NWSMTSGetTargetScanTypeString;
extern _NWSMTSGetOpenModeOptionString  NWSMTSGetOpenModeOptionString;
extern _NWSMTSConvertError              		NWSMTSConvertError;
extern _NWSMTSListSupportedNameSpaces   	NWSMTSListSupportedNameSpaces;
extern _NWSMTSListTSResources           		NWSMTSListTSResources;
extern _NWSMTSGetUnsupportedOptions     	NWSMTSGetUnsupportedOptions;
extern _NWSMGetVersionInfo              		NWSMGetRequestorVersionInfo,
		                                 				NWSMGetResponderVersionInfo,
		                                 				NWSMGetSMSModuleVersionInfo;
extern _NWSMTSScanDataSetContinue       	NWSMTSScanDataSetContinue;
extern _NWSMTSConnectToTargetServiceEx	NWSMTSConnectToTargetServiceEx;
extern _NWSMTSGetTargetServiceAPIVersion	NWSMTSGetTargetServiceAPIVersion;
extern _NWSMTSConfigureTargetService	 	NWSMTSConfigureTargetService;



void DestroyConnectionList(void);

#ifdef __cplusplus
}
#endif

#endif /* smstsapi.h header latch */

