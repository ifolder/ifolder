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
*                 $Author: Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        	iFolder Exception logging
*
*
*******************************************************************************/
 
#import <Cocoa/Cocoa.h>

#define	TheConsoleLogSwitch	( 1 )

#if TheConsoleLogSwitch
#define ifconlog1(x)		NSLog(x)
#define ifconlog2(x,y)		NSLog(x,y)
#define ifconlog3(x,y,z)	NSLog(x,y,z)
#define ifconlog4(w,x,y,z)	NSLog(w,x,y,z)
#define ifexconlog(x,y)		iFolderExceptionLog(x,y)
#else
#define ifconlog1(x)
#define ifconlog2(x,y)
#define ifconlog3(x,y,z)
#define ifconlog4(w,x,y,z)
#define ifexconlog(x,y)
#endif

void iFolderExceptionLog(NSString *methodLocation, NSException *ex);
