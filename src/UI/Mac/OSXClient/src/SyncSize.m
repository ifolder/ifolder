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

#import "SyncSize.h"


@implementation SyncSize



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
// Deallocate the resouces previously allocated
//==================================================================
-(void) dealloc
{
	[properties release];
	
	[super dealloc];
}


//==================================================================
// properties
// Get the properties about sync size
//==================================================================
- (NSMutableDictionary *) properties
{
	return properties;
}



//==================================================================
// setProperties 
// Set the properties of sync size
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
// SyncNodeCount
// Get the count of sync nodes
//==================================================================
-(unsigned long)SyncNodeCount
{
	NSNumber *num = [properties objectForKey:@"SyncNodeCount"];
	if(num != nil)
		return [num unsignedLongValue];
	else
		return 0;
}

//==================================================================
// SyncByteCount
// Get the count of sync bytes
//==================================================================
-(unsigned long long) SyncByteCount
{
	NSNumber *num = [properties objectForKey:@"SyncByteCount"];
	if(num != nil)
		return [num unsignedLongLongValue];
	else
		return 0;
}



@end
