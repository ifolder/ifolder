/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | Author:    Sudhakar GNVS  
 | Modtime:   9 Jul 2002 11:32:01 AM  
 |
 | Workfile:   SmsMac.h  
 | Revision:   1.0  
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Header file with the mac name space and characteristics related structures
 +-------------------------------------------------------------------------*/
#ifndef _SMS_MAC_H_IS_INCLUDED
#define _SMS_MAC_H_IS_INCLUDED

#pragma pack(push,1)

typedef struct 
{
	BUFFER						  finderInfo[32];
	BUFFER						  proDOSInfo[6];
	UINT32						  dirRightsMask;

}	MAC_NAME_SPACE_INFO;

#pragma pack(pop)

#endif  /* _SMS_MAC_H_IS_INCLUDED */
