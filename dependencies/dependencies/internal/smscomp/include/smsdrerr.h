/*
===============================================================
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
				smsdrerr.h

PURPOSE/COMMENTS:
				Error numbers for DRAPIs

Prefix:
				FFFE

NDK COMPONENT NAME AND VERSION:
				SMS Developer Components

LAST MODIFIED DATE:
				22 Jan 2004
=================================================================
*/

#ifndef _SMSDRERR_H_INCLUDED      /* smsdrerr.h header latch */
#define _SMSDRERR_H_INCLUDED

#define NWSMDR_ERROR_CODE(err)         (0xFFFE0000L | err)

#define NWSMDR_BEGIN_ERROR_CODES       NWSMDR_ERROR_CODE(0xFFFF)

#define NWSMDR_INVALID_CONNECTION      NWSMDR_ERROR_CODE(0xFFFF) 
/* An invalid connection handle was passed to the SMDR                     */
#define NWSMDR_INVALID_PARAMETER       NWSMDR_ERROR_CODE(0xFFFE) 
/* One or more of the paremeters is NULL or invalid                        */
#define NWSMDR_OUT_OF_MEMORY           NWSMDR_ERROR_CODE(0xFFFD) 
/* SMDR memory allocation failed                                           */
#define NWSMDR_TRANSPORT_FAILURE       NWSMDR_ERROR_CODE(0xFFFC) 
/* The transport mechanism has failed                                      */
#define NWSMDR_UNSUPPORTED_FUNCTION    NWSMDR_ERROR_CODE(0xFFFB) 
/* The requested function is not supported by the SMDR                     */

#define NWSMDR_MODULE_ALREADY_EXPORTED NWSMDR_ERROR_CODE(0xFFFA)
#define NWSMDR_DECRYPTION_FAILURE      NWSMDR_ERROR_CODE(0xFFF9)
#define NWSMDR_ENCRYPTION_FAILURE      NWSMDR_ERROR_CODE(0xFFF8)
#define NWSMDR_TSA_NOT_LOADED          NWSMDR_ERROR_CODE(0xFFF7)
#define NWSMDR_NO_SUCH_SMDR            NWSMDR_ERROR_CODE(0xFFF6)
#define NWSMDR_SMDR_CONNECT_FAILURE    NWSMDR_ERROR_CODE(0xFFF5)
#define NWSMDR_NO_MORE_DATA            NWSMDR_ERROR_CODE(0xFFF4)
#define NWSMDR_NO_SOCKETS              NWSMDR_ERROR_CODE(0xFFF3)
#define NWSMDR_INVALID_PROTOCOL        NWSMDR_ERROR_CODE(0xFFF2)
#define NWSMDR_NO_MORE_CONNECTIONS     NWSMDR_ERROR_CODE(0xFFF1)
#define NWSMDR_NO_SUCH_TSA             NWSMDR_ERROR_CODE(0xFFF0)

#define NWSMDR_INVALID_MESSAGE_NUMBER  NWSMDR_ERROR_CODE(0xFFEF)
#define NWSMDR_END_ERROR_CODES         NWSMDR_ERROR_CODE(0xFFEF)
/* Identifies the end of the SMDR error set  */

#endif                            /* smsdrerr.h header latch */
