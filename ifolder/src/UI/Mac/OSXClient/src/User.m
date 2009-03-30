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

#import "User.h"


@implementation User

//========================================================================
// init
// Initialize the object with default values needed
//========================================================================
- (id) init
{
	if(self = [super init])
	{
		NSArray *keys = [NSArray arrayWithObjects:
			@"Name", nil];
			
		NSArray *values		= [NSArray arrayWithObjects:
			@"New User", nil];
		
		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys: keys];
	}
	return self;
}

//========================================================================
// dealloc
// Deallocate the resources previously allocated
//========================================================================
-(void) dealloc
{
	[properties release];
	[icon release];
	
	[super dealloc];
}

//========================================================================
// properties
// Get the properties of user
//========================================================================
-(NSMutableDictionary *) properties
{
	return properties;
}



//========================================================================
// setProperties
// Set the properties of user
//========================================================================
-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
		[self updateDisplayInformation];
	}
}

//========================================================================
// UserID
// Get the User ID from list of properties
//========================================================================
-(NSString *) UserID
{
	return [self valueForKeyPath:@"properties.UserID"]; 
}

//========================================================================
// FN
// Get the FN from list of properties
//========================================================================
-(NSString *) FN
{
	return [self valueForKeyPath:@"properties.FN"]; 
}

//========================================================================
// Name
// Get the name of user from list of properties
//========================================================================
-(NSString *) Name
{
	return [self valueForKeyPath:@"properties.Name"]; 
}

//========================================================================
// FirstName
// Get first name of user from list of properties
//========================================================================
-(NSString *) FirstName
{
	return [self valueForKeyPath:@"properties.FirstName"];
}

//========================================================================
// Surname
// Get surname of user from list of properties
//========================================================================
-(NSString *) Surname
{
	return [self valueForKeyPath:@"properties.Surname"];
}

//========================================================================
// isOwner
// Check whether the user is owner of the iFolder or not
//========================================================================
-(BOOL)isOwner
{
	NSNumber *num = [properties objectForKey:@"IsOwner"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;	
}


//========================================================================
// setRights
// Set the rights of iFolder to the user
//========================================================================
-(void)setRights:(NSString *)rights
{
	[properties setObject:rights forKey:@"Rights"];
	[self updateDisplayInformation];
}

//========================================================================
// setIsOwner
// Set the flag whether the user is owner of ifolder or not
//========================================================================
-(void)setIsOwner:(BOOL)isOwner
{
	[properties setObject:[NSNumber numberWithBool:isOwner] forKey:@"IsOwner"];
	[self updateDisplayInformation];	
}


//========================================================================
// updateDispalyInformation
// Update the display information about the user
//========================================================================
-(void) updateDisplayInformation
{
	if([self isOwner])
		[properties setObject:NSLocalizedString(@"Owner", @"iFolder Member Status") forKey:@"Status"];
	else
	{
		NSString *state = [properties objectForKey:@"State"];
		if(state != nil)
		{
			if([state compare:@"Member"] != 0)
				[properties setObject:NSLocalizedString(@"Invited User", @"iFolder Member Status") forKey:@"Status"];
			else
				[properties setObject:NSLocalizedString(@"Member", @"iFolder Member Status") forKey:@"Status"];
		}
	}

	NSString *rights = [properties objectForKey:@"Rights"];
	if(rights != nil)
	{
		if([rights compare:@"Admin"] == 0)
			[properties setObject:NSLocalizedString(@"Full Control", @"iFolder Member Access") forKey:@"Access"];
		else if([rights compare:@"ReadWrite"] == 0)
			[properties setObject:NSLocalizedString(@"Read/Write", @"iFolder Member Access") forKey:@"Access"];
		else if([rights compare:@"ReadOnly"] == 0)
			[properties setObject:NSLocalizedString(@"Read Only", @"iFolder Member Access") forKey:@"Access"];
		else
			[properties setObject:NSLocalizedString(@"Unknown", @"iFolder Member Access") forKey:@"Access"];
	}

}


@end
