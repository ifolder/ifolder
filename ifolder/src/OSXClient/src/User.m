/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

#import "User.h"


@implementation User

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


-(void) dealloc
{
	[properties release];
	[icon release];
	
	[super dealloc];
}


-(NSMutableDictionary *) properties
{
	return properties;
}




-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];
		[self updateDisplayInformation];
	}
}


-(NSString *) UserID
{
	return [self valueForKeyPath:@"properties.UserID"]; 
}

-(NSString *) FN
{
	return [self valueForKeyPath:@"properties.FN"]; 
}

-(NSString *) Name
{
	return [self valueForKeyPath:@"properties.Name"]; 
}

-(NSString *) FirstName
{
	return [self valueForKeyPath:@"properties.FirstName"];
}

-(NSString *) Surname
{
	return [self valueForKeyPath:@"properties.Surname"];
}



-(BOOL)isOwner
{
	NSNumber *num = [properties objectForKey:@"IsOwner"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;	
}


-(void)setRights:(NSString *)rights
{
	[properties setObject:rights forKey:@"Rights"];
	[self updateDisplayInformation];
}

-(void)setIsOwner:(BOOL)isOwner
{
	[properties setObject:[NSNumber numberWithBool:isOwner] forKey:@"IsOwner"];
	[self updateDisplayInformation];	
}



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
