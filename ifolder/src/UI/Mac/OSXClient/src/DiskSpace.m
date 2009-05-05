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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "DiskSpace.h"


@implementation DiskSpace


//==================================================================
// init
// Initialize the default values here
//==================================================================
-(id) init
{
	if(self = [super init])
	{
		NSArray *keys	= [NSArray arrayWithObjects:	@"name", 
														nil];

		NSArray *values = [NSArray arrayWithObjects:	@"",
														nil];

		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys:keys];
	}
	
	return self;
}


//==================================================================
// dealloc
// Deallocate the resources previously allocated
//==================================================================
-(void) dealloc
{
	[properties release];
	
	[super dealloc];
}


//==================================================================
// properties
// Get the properties about the disk space
//==================================================================
- (NSMutableDictionary *) properties
{
	return properties;
}



//==================================================================
// setProperties
// Set the properties of disk space
//==================================================================
-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
	}
}


//==================================================================
// AvailableSpace
// Get the available space left out of quota
//==================================================================
-(long long) AvailableSpace
{
	return [ [properties objectForKey:@"AvailableSpace"] longLongValue];
}

//==================================================================
// Limit
// Get the quota limit
//==================================================================
-(long long) Limit
{
	return [ [properties objectForKey:@"Limit"] longLongValue];
}

//==================================================================
// UsedSpace
// Get the used space out of quota allocated
//==================================================================
-(long long) UsedSpace
{
	return [ [properties objectForKey:@"UsedSpace"] longLongValue];
}



@end
