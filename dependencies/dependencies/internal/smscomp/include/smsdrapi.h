/*
=================================================================	
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
			smsdrapi.h

PURPOSE/COMMENTS:
			SMDR Apis

NDK COMPONENT NAME AND VERSION:
			SMS Developer Components

LAST MODIFIED DATE:
			22 Jan 2004
=================================================================
*/

#ifndef _SMSDRAPI_H_INCLUDED      /* smsdrapi.h header latch */
#define _SMSDRAPI_H_INCLUDED

#ifdef __cplusplus
extern "C" {
#endif

#include <smsdrerr.h>

#if defined(NWWIN)
#	if defined(API)
#		undef API
#	endif
#	define	API	pascal far
#else
#	define	API
#endif

	CCODE API NWSMListTSAs(
		char *pattern, 
		NWSM_NAME_LIST **tsaNameList);

	CCODE API NWSMListSMDRs(
		char *pattern, 
		NWSM_NAME_LIST **tsaNameList);

	CCODE API NWSMTSListTargetServices(
		UINT32 connectionID, 
		char *pattern, 
		NWSM_NAME_LIST **tsNameList);

	CCODE API NWSMConvertError(
		UINT32 connectionID, 
		CCODE error, 
		STRING message);

#ifdef __cplusplus
}
#endif

#endif                            /* smsdrapi.h header latch */
