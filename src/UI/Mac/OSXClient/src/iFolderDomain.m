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
*                 $Modified by: Satyam <ssutapalli@novell.com>  23/09/2008    Added new function to set the property "host"
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "iFolderDomain.h"
#import "iFolder.h"

@implementation iFolderDomain

//=================================================================================
// init
// Initialize the default values
//=================================================================================
-(id) init
{
	if(self = [super init])
	{
		NSArray *keys	= [NSArray arrayWithObjects:	@"name", 
														@"password",
														nil];

		NSArray *values = [NSArray arrayWithObjects:	@"New Domain",
														@"",
														nil];

		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys:keys];
	}
	
	return self;
}


//=================================================================================
// dealloc
// Deallocate the resources previously allocated
//=================================================================================
-(void) dealloc
{
	[properties release];
	
	[super dealloc];
}


//=================================================================================
// properties
// Get the properties of domain
//=================================================================================
- (NSMutableDictionary *) properties
{
	return properties;
}



//=================================================================================
// setProperties
// Set the properties of domain
//=================================================================================
-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
	}
}



//=================================================================================
// ID
// Get the domainId of the domain
//=================================================================================
-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}

//=================================================================================
// name
// Get the name of the domain
//=================================================================================
-(NSString *)name
{
	return [self valueForKeyPath:@"properties.name"]; 
}

//=================================================================================
// userName
// Get the user name that connected to domain
//=================================================================================
-(NSString *)userName
{
	return [self valueForKeyPath:@"properties.userName"]; 
}

//=================================================================================
// userID
// Get the user ID of the user connected to domain
//=================================================================================
-(NSString *)userID
{
	return [self valueForKeyPath:@"properties.userID"]; 
}

//=================================================================================
// host
// Get the host details about the domain 
//=================================================================================
-(NSString *)host
{
	return [self valueForKeyPath:@"properties.host"]; 
}

//=================================================================================
// hostURL
// Get the URL of the host
//=================================================================================
-(NSString *)hostURL
{
	return [self valueForKeyPath:@"properties.hostURL"]; 
}

//=================================================================================
// poBoxID
// Get the pobox id of the domain
//=================================================================================
-(NSString *)poBoxID
{
	return [self valueForKeyPath:@"properties.poboxID"]; 
}

//=================================================================================
// password
// Get the password of the user connected to the domain
//=================================================================================
-(NSString *)password
{
	return [self valueForKeyPath:@"properties.password"]; 
}

//=================================================================================
// description
// Get the description about the domain
//=================================================================================
-(NSString *)description
{
	return [self valueForKeyPath:@"properties.description"]; 
}

//=================================================================================
// isDefault
// Chech whether the domain is the default one on the client
//=================================================================================
-(NSNumber *)isDefault
{
	return [self valueForKeyPath:@"properties.isDefault"]; 
}

//=================================================================================
// isSlave
// Check whether the domain is on slave machine
//=================================================================================
-(NSNumber *)isSlave
{
	return [self valueForKeyPath:@"properties.isSlave"]; 
}

//=================================================================================
// isEnabled
// Check whether the domain is enabled or not
//=================================================================================
-(NSNumber *)isEnabled
{
	return [self valueForKeyPath:@"properties.isEnabled"]; 
}

//=================================================================================
// authenticated 
// Find whether domain is aunthenticated or not
//=================================================================================
-(BOOL)authenticated
{
	NSNumber *num = [self valueForKeyPath:@"properties.authenticated"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}

//=================================================================================
// statusCode
// Get the status code of domain connectivity
//=================================================================================
-(NSNumber *)statusCode
{
	return [self valueForKeyPath:@"properties.statusCode"]; 
}

//=================================================================================
// remainingGraceLogins
// Get the remaining grace logins permitted for the domain
//=================================================================================
-(NSNumber *)remainingGraceLogins
{
	return [self valueForKeyPath:@"properties.remainingGraceLogins"]; 
}

//=================================================================================
// setHost
// Set the host of the domain
//=================================================================================
-(void)setHost:(NSString*)newHost
{
	[properties setObject:newHost forKey:@"properties.host"];
}

@end
