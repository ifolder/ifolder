//
//  iFolder.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/17/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

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
	}
}





-(void) setgSOAPProperties:(struct ns1__iFolderWeb *)ifolder
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(ifolder->DomainID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->DomainID] forKey:@"DomainID"];

	if(ifolder->ID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->ID] forKey:@"ID"];

	if(ifolder->ManagedPath != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->ManagedPath] forKey:@"ManagedPath"];

	if(ifolder->UnManagedPath != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->UnManagedPath] forKey:@"Path"];

	if(ifolder->Name != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Name] forKey:@"Name"];

	if(ifolder->Owner != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Owner] forKey:@"Owner"];

	if(ifolder->OwnerID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->OwnerID] forKey:@"OwnerID"];

	if(ifolder->Type != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Type] forKey:@"Type"];

	if(ifolder->Description != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->Description] forKey:@"Description"];

	if(ifolder->State != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->State] forKey:@"State"];

	if(ifolder->CurrentUserID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CurrentUserID] forKey:@"CurrentUserID"];

	if(ifolder->CurrentUserRights != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CurrentUserRights] forKey:@"CurrentUserRights"];

	if(ifolder->CollectionID != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->CollectionID] forKey:@"CollectionID"];

	if(ifolder->LastSyncTime != nil)
		[newProperties setObject:[NSString stringWithCString:ifolder->LastSyncTime] forKey:@"LastSyncTime"];

	[newProperties setObject:[NSNumber numberWithInt:ifolder->EffectiveSyncInterval] forKey:@"EffectiveSyncInterval"];

	[newProperties setObject:[NSNumber numberWithInt:ifolder->SyncInterval] forKey:@"SyncInterval"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->IsSubscription] forKey:@"IsSubscription"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->IsWorkgroup] forKey:@"IsWorkgroup"];

	[newProperties setObject:[NSNumber numberWithBool:ifolder->HasConflicts] forKey:@"HasConflicts"];
	
	[self setProperties:newProperties];
	
	[self updateDisplayInformation];
}

-(id)Image
{
	return icon;
}


-(NSString *)Location
{
	return location;
}

-(NSString *)Status
{
	return state;
}

-(NSNumber *)IsSubscription
{
	return [properties objectForKey:@"IsSubscription"];
}


-(void) updateDisplayInformation
{
	if([ [self IsSubscription] boolValue])
	{
		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
			state = @"Available";
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitConnect"])
			state = @"Waiting to Connect";
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			state = @"Waiting to Sync";
		else
			state = @"Unknown";

		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
		{
			location = [properties objectForKey:@"Owner"];
		}

		if(icon != nil)
		{
			[icon release];
		}
		icon = [NSImage imageNamed:@"ifolderonserver24"];
		[icon setScalesWhenResized:YES];
	}
	else
	{
		if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			state = @"Waiting to Sync";
		else if([ [properties objectForKey:@"State"] isEqualToString:@"Local"])
		{
			if([ [properties objectForKey:@"HasConflicts"] boolValue])
				state = @"Has File Conflicts";
			else
				state = @"OK";
		}

		location = [properties objectForKey:@"Path"];

		if(icon != nil)
			[icon release];

		icon = [NSImage imageNamed:@"ifolder24"];
		[icon setScalesWhenResized:YES];

	}
}


@end
