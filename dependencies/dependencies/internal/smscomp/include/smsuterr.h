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
				smsuterr.h

PURPOSE/COMMENTS:
				Error numbers for SMS Utility Library
Prefix:   
				FFFB

NDK COMPONENT NAME AND VERSION:
				SMS Developer Components

LAST MODIFIED DATE:
				22 Jan 2004
=================================================================
*/


#ifndef _SMSUTERR_H_INCLUDED      /* smsuterr.h header latch */
#define _SMSUTERR_H_INCLUDED

	
#define NWSMUT_ERROR_CODE(err)           (0xFFFB0000L | err)
	
#define NWSMUT_BEGIN_ERROR_CODES          NWSMUT_ERROR_CODE(0xFFFF)

#define NWSMUT_INVALID_HANDLE             NWSMUT_ERROR_CODE(0xFFFF) 
/* Handle is tagged INVALID or ptr is NULL     */
	
#define NWSMUT_INVALID_OFFSET_TO_END      NWSMUT_ERROR_CODE(0xFFFE) 
/* The OffsetToEnd field did not offset to the correct end */
/* field */
   
#define NWSMUT_INVALID_PARAMETER          NWSMUT_ERROR_CODE(0xFFFD) 
/* One or more of the paremeters is NULL or invalid */
   
#define NWSMUT_NO_MORE_NAMES              NWSMUT_ERROR_CODE(0xFFFC) 
/* No more entries in list or nameSpace type does not exist */
   
#define NWSMUT_OUT_OF_MEMORY              NWSMUT_ERROR_CODE(0xFFFB) 
/* Server out of memory or memory allocation failed */
   
#define NWSMUT_BUFFER_OVERFLOW            NWSMUT_ERROR_CODE(0xFFFA) 
/* Field identifier buffer overflow */
	
#define NWSMUT_BUFFER_UNDERFLOW           NWSMUT_ERROR_CODE(0xFFF0) 
/* Field identifier buffer underflow */
	
#define NWSMUT_INVALID_FIELD_ID           NWSMUT_ERROR_CODE(0xFFF9) 
/* Invalid field identifier encountered */
	
#define NWSMUT_INVALID_MESSAGE_NUMBER     NWSMUT_ERROR_CODE(0xFFF8) 
/* Invalid message number encountered */

#define NWSMUT_END_ERROR_CODES            NWSMUT_ERROR_CODE(0xFFF8) 

#endif                            /* smsuterr.h header latch */
