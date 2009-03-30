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

#import "iFolderConflict.h"


@implementation iFolderConflict

//==================================================================
// init
// Initialize with default values
//==================================================================
- (id) init
{
	if(self = [super init])
	{
		NSArray *keys = [NSArray arrayWithObjects:
			@"Name", nil];
			
		NSArray *values		= [NSArray arrayWithObjects:
			@"nil conflict", nil];
		
		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys: keys];
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
// Get the properties about the conflict of ifolder
//==================================================================
-(NSMutableDictionary *) properties
{
	return properties;
}

//==================================================================
// setProperties
// Set the properties of conflict
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
// mergeNameConflicts
// Merge the name conflicts
//==================================================================
-(void) mergeNameConflicts:(iFolderConflict *)conflict
{
	NSMutableDictionary *serverProps = nil;
	NSMutableDictionary *localProps = nil;

	if( ( [properties objectForKey:@"LocalName"] != nil ) &&
		( [[conflict properties] objectForKey:@"ServerName"] != nil) )
	{
		serverProps = [[conflict properties] retain];
		localProps = [properties retain];
	}
	else
	{
		localProps = [[conflict properties] retain];
		serverProps = [properties retain];
	}

	NSString *valHolder = [localProps objectForKey:@"LocalName"];
	if(valHolder != nil)
		[serverProps setObject:valHolder forKey:@"LocalName"];

	valHolder = [localProps objectForKey:@"LocalDate"];
	if(valHolder != nil)
		[serverProps setObject:valHolder forKey:@"LocalDate"];

	valHolder = [localProps objectForKey:@"LocalSize"];
	if(valHolder != nil)
		[serverProps setObject:valHolder forKey:@"LocalSize"];

	valHolder = [localProps objectForKey:@"ConflictID"];
	if(valHolder != nil)
		[serverProps setObject:valHolder forKey:@"LocalConflictID"];

	[self setProperties:serverProps];

	[serverProps release];
	[localProps release];
}




@end
