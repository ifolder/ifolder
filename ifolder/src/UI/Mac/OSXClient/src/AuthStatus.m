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
*                 $Modified by: Satyam <ssutapalli@novell.com>  23/09/2008    Added new function to get the property UserName
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "AuthStatus.h"


@implementation AuthStatus


//==================================================================
// init
// Initialize with default values
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
// Get the properties about authentication status 
//==================================================================
- (NSMutableDictionary *) properties
{
	return properties;
}



//==================================================================
// setProperties
// Set the properties of authentication
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
// statusCode
// Get the status code of authentication
//==================================================================
-(NSNumber *)statusCode
{
	return [self valueForKeyPath:@"properties.statusCode"]; 
}



//==================================================================
// totalGraceLogins
// Get the total grace logins allowed
//==================================================================
-(int)totalGraceLogins
{
	NSNumber *num = [self valueForKeyPath:@"properties.totalGraceLogins"];
	if(num != nil)
		return [num intValue];
	else
		return NO;
}



//==================================================================
// remainingGraceLogins
// Get the remaining grace logins out of total allowed
//==================================================================
-(int)remainingGraceLogins
{
	NSNumber *num = [self valueForKeyPath:@"properties.remainingGraceLogins"];
	if(num != nil)
		return [num intValue];
	else
		return NO;
}

//==================================================================
// userName
// Get the user who authenticated
//==================================================================
-(NSString*)userName
{
	return [self valueForKeyPath:@"properties.userName"];
}

@end
