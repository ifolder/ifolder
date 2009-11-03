/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author:  Satyam <ssutapalli@novell.com>	23/10/2008	Support class to check Mac update
*-----------------------------------------------------------------------------
* This module is used to:
*        This will hold the response received from server to check Mac latest 
* client availability
*
*******************************************************************************/

#import "clientUpdate.h"


@implementation clientUpdate

//==========================================================
// Status
// Get the status of client update
//==========================================================
-(int)Status
{
	return status;
}

//==========================================================
// ServerVersion
// Get the available client version on Server
//==========================================================
-(NSString*)ServerVersion
{
	return serverVersion;
}

//==========================================================
// setStatus
// Set the status of update
//==========================================================
-(void)setStatus:(int)stat
{
	status = stat;
}

//==========================================================
// setServerVersion
// Set the serverVersion variable available on server
//==========================================================
-(void)setServerVersion:(char*)servVer
{
	serverVersion = [NSString stringWithUTF8String:servVer];
}

@end
