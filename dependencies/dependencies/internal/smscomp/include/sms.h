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
				sms.h

PURPOSE/COMMENTS:
				Definitions for SMS, this file includes all SMS header files necessary to 
				develop an SME
	
NDK COMPONENT NAME AND VERSION:
				SMS Developer Components

LAST MODIFIED DATE:
				22 Jan 2004
=================================================================
*/

#if !defined(_SMS_H_INCLUDED)
	#define _SMS_H_INCLUDED

	#include <string.h>
	#include <stdlib.h>
	#include <stdio.h>
	#if defined(__WATCOMC__) && !defined(NETWARE_V311) && !defined(NETWARE_V312)
		#include <nwlocale.h>
	#endif
	#define _CLIB_HDRS_INCLUDED

	#include <smstypes.h>
	#include <smsdefns.h>
	#include <smsdrapi.h>

/*	#include <smspcode.h> */
	#if !defined(NETWARE_V311) && !defined(NETWARE_V312)
		#include <smssdapi.h>
	#endif

	#include <smstsapi.h>
	#include <smsutapi.h>
#endif
