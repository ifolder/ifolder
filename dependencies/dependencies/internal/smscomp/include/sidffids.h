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
			sidffids.h

PURPOSE/COMMENTS:
			FID definitions for SMS
	
NDK COMPONENT NAME AND VERSION:
			SMS Developer Components

LAST MODIFIED DATE:
			22 Jan 2004
=================================================================
*/


#ifndef _SIDFFIDS_H_INCLUDED       /* smsfids.h header latch */
#define _SIDFFIDS_H_INCLUDED
 
/* Miscellaneous defines */

#if defined(DEBUG_CODE)
#define DEBUG_HEADER_STRING_FIELD      SIDF_HEADER_DEBUG_STRING,      \
                                       UINT64_ZERO, HEADER_STRING
#define DEBUG_HEADER_FIELD             HEADER_STRING_LEN, NULL,       \
                                       HEADER_STRING_LEN
#define DEBUG_HEADER_STRING_FIELD_ALT  SIDF_HEADER_DEBUG_STRING,      \
                                       UINT64_ZERO, HEADER_STRING_ALT
#define DEBUG_HEADER_FIELD_ALT         HEADER_STRING_LEN_ALT, NULL,   \
                                       HEADER_STRING_LEN_ALT
#endif

#define NWSM_REV_DATA                  "SIDF 1.0"
#define NWSM_SYNC_DATA                 0x5AA5
#define NWSM_VARIABLE_SIZE             0x80

/* Media Mark Defines */

#define NWSM_MARK_TYPE_HARDWARE        1
#define NWSM_MARK_TYPE_SOFTWARE        2
#define NWSM_SMM_FILE_MARK             1
#define NWSM_SMM_SET_MARK              2

#include <sidffids.inc>

#endif                            /* sidffids.h header latch */
