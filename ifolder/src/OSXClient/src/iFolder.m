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

#import "iFolder.h"


@implementation iFolder

- (id) init
{
	if(self = [super init])
	{
		NSArray *keys = [NSArray arrayWithObjects:
			@"Name", nil];
			
		NSArray *values		= [NSArray arrayWithObjects:
			@"New iFolder", nil];
		
		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys: keys];
	}

	synchronizing = NO;
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



-(BOOL) isSynchronizing
{
	return synchronizing;
}

-(void) setIsSynchronizing:(BOOL)isSynchronizing
{
	synchronizing = isSynchronizing;
	[self updateDisplayInformation];
}


-(id)Image
{
	return icon;
}

-(NSString *)Name
{
	return [properties objectForKey:@"Name"];
}

-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}

-(NSString *)Path
{
	return [properties objectForKey:@"Path"];
}

-(NSNumber *)IsSubscription
{
	return [properties objectForKey:@"IsSubscription"];
}

-(NSString *)DomainID
{
	return [properties objectForKey:@"DomainID"];
}

-(NSString *)OwnerUserID
{
	return [properties objectForKey:@"OwnerID"];
}
-(NSString *)CurrentUserID
{
	return [properties objectForKey:@"CurrentUserID"];
}

-(NSString *)CurrentUserRights
{
	return [properties objectForKey:@"CurrentUserRights"];
}

-(NSString *)State
{
	return [properties objectForKey:@"State"];
}





-(void) updateDisplayInformation
{
	if([ [self IsSubscription] boolValue])
	{
		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
			[properties setObject:@"Available" forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitConnect"])
			[properties setObject:@"Waiting to Connect" forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			[properties setObject:@"Waiting to Sync" forKey:@"Status"];
		else
			[properties setObject:@"Unknown" forKey:@"Status"];

		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
		{
			[properties setObject:[properties objectForKey:@"Owner"]
								forKey:@"Location"];
		}

		if(icon != nil)
		{
			[icon release];
		}
		icon = [NSImage imageNamed:@"serverifolder24"];
		[icon setScalesWhenResized:YES];
	}
	else
	{
		if(synchronizing)
			[properties setObject:@"Syncrhonizing" forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			[properties setObject:@"Waiting to Sync" forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"Local"])
		{
			if([ [properties objectForKey:@"HasConflicts"] boolValue])
				[properties setObject:@"Has File Conflicts" forKey:@"Status"];
			else
				[properties setObject:@"OK" forKey:@"Status"];
		}

		// update the location
		NSString *location = [properties objectForKey:@"Path"];
		if(location != nil)
			[properties setObject:location forKey:@"Location"];
			
		if(icon != nil)
			[icon release];

		icon = [NSImage imageNamed:@"ifolder24"];
		[icon setScalesWhenResized:YES];
	}
}


@end
