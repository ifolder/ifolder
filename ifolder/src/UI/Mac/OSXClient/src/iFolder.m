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
*                 $Modified by: Satyam <ssutapalli@novell.com>  15/05/2008  Changed conflict icon
*                 $Modified by: Satyam <ssutapalli@novell.com>  13/10/2008  Changed icon size to 48x48 pixels
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "iFolder.h"
#import "User.h"


@implementation iFolder

//=======================================================================
// init
// Constructor - Initialize the arrays and properties with "nil" values
//=======================================================================
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

//=======================================================================
// dealloc
// Destructor - release the allocated resources
//=======================================================================
-(void) dealloc
{
	[properties release];
	
	[super dealloc];
}

//=======================================================================
// properties
// get method for properties
//=======================================================================
-(NSMutableDictionary *) properties
{
	return properties;
}

//=======================================================================
// setProperties
// set method for properties. then update the display.
//=======================================================================
-(void) setProperties: (NSDictionary *)newProperties
{
	if(properties != newProperties)
	{
		NSNumber *totalSyncCount = [properties objectForKey:@"totalSyncCount"];
		NSNumber *outOfSyncCount = [properties objectForKey:@"outOfSyncCount"];
		NSNumber *syncState = [properties objectForKey:@"syncState"];
	
		[properties autorelease];
		properties = [[NSMutableDictionary alloc] initWithDictionary:newProperties];

		// Preserve the state of the sync even if we refresh for some reason
		if(totalSyncCount != nil)
			[properties setObject:totalSyncCount forKey:@"totalSyncCount"];
		if(outOfSyncCount != nil)
			[properties setObject:outOfSyncCount forKey:@"outOfSyncCount"];
		if(syncState != nil)
			[properties setObject:syncState forKey:@"syncState"];
		
		[self updateDisplayInformation];
	}
}


//=======================================================================
// setSyncProperties
// set method for sync properties. then update the display.
//=======================================================================
-(void) setSyncProperties: (NSDictionary *)syncProperties
{
	if(properties != syncProperties)
	{
		NSNumber *totalSyncCount = [syncProperties objectForKey:@"totalSyncCount"];
		NSNumber *outOfSyncCount = [syncProperties objectForKey:@"outOfSyncCount"];
		NSNumber *syncState = [syncProperties objectForKey:@"syncState"];
	
		if(totalSyncCount != nil)
			[properties setObject:totalSyncCount forKey:@"totalSyncCount"];
		if(outOfSyncCount != nil)
			[properties setObject:outOfSyncCount forKey:@"outOfSyncCount"];
		if(syncState != nil)
			[properties setObject:syncState forKey:@"syncState"];

		[self updateDisplayInformation];
	}
}

//=======================================================================
// syncState
// get method for getting Synchronizing state
//=======================================================================
-(int) syncState
{
	NSNumber *num = [properties objectForKey:@"syncState"];
	if(num != nil)
		return [num intValue];
	else
		return 0;
}

//=======================================================================
// setSyncState
// set method for Sync state
//=======================================================================
-(void) setSyncState:(int)syncState
{
	[properties setObject:[NSNumber numberWithInt:syncState] forKey:@"syncState"];
	[self updateDisplayInformation];
}

//=======================================================================
// outofSyncCount
// get method for getting the count of ifolders that are not synced.
//=======================================================================
-(unsigned long) outOfSyncCount
{
	NSNumber *num = [properties objectForKey:@"outOfSyncCount"];
	if(num != nil)
		return [num unsignedLongValue];
	else
		return 0;
}

//=======================================================================
// setOutofSyncCount
// set method for setting the out of sync ifolders count.
//=======================================================================
-(void) setOutOfSyncCount:(unsigned long)outOfSyncCount
{
	[properties setObject:[NSNumber numberWithUnsignedLong:outOfSyncCount] forKey:@"outOfSyncCount"];
}

//=======================================================================
// Name
// get method for getting the Name of the iFolder
//=======================================================================
-(NSString *)Name
{
	return [properties objectForKey:@"Name"];
}

//=======================================================================
// ID
// get method for ID
//=======================================================================
-(NSString *)ID
{
	return [properties objectForKey:@"ID"];
}

//=======================================================================
// CollectionID
// get method for Collection ID
//=======================================================================
-(NSString *)CollectionID
{
	return [properties objectForKey:@"CollectionID"];
}

//=======================================================================
// Path
// get method for Path of the iFolder located on the client if setup.
//=======================================================================
-(NSString *)Path
{
	return [properties objectForKey:@"Path"];
}

//=======================================================================
// IsSubscription
// get method to find out whether the iFolder is Subscription or not.
// (whether it is on server or setup on client)
//=======================================================================
-(BOOL)IsSubscription
{
	NSNumber *num = [properties objectForKey:@"IsSubscription"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}

//=======================================================================
// HasConflicts
// get method to know if iFolder has conflicts
//=======================================================================
-(BOOL)HasConflicts
{
	NSNumber *num = [properties objectForKey:@"HasConflicts"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}

//=======================================================================
// DomainID
// get method for DomainID
//=======================================================================
-(NSString *)DomainID
{
	return [properties objectForKey:@"DomainID"];
}

//=======================================================================
// OwnerUserID
// get method for OwnerUserID
//=======================================================================
-(NSString *)OwnerUserID
{
	return [properties objectForKey:@"OwnerID"];
}

//=======================================================================
// OwnerName
// get method for OwnerName
//=======================================================================
-(NSString *)OwnerName
{
	return [properties objectForKey:@"Owner"];
}

//=======================================================================
// CurrentUserID
// get method for CurrentUserID
//=======================================================================
-(NSString *)CurrentUserID
{
	return [properties objectForKey:@"CurrentUserID"];
}

//=======================================================================
// CurrentUserRights
// get method for getting the current's users access rights to iFolder
//=======================================================================
-(NSString *)CurrentUserRights
{
	return [properties objectForKey:@"CurrentUserRights"];
}

//=======================================================================
// State
// get method for knowing the state of iFolder ie synced or waiting etc.
//=======================================================================
-(NSString *)State
{
	return [properties objectForKey:@"State"];
}

//=======================================================================
// LastSync
// get method for getting last sync interval time
//=======================================================================
-(NSString *)LastSync
{
	return [properties objectForKey:@"LastSyncTime"];
}

//=======================================================================
// Role
// get method for Role
//=======================================================================
-(NSString *)Role
{
	return [properties objectForKey:@"Role"];
}

//=======================================================================
// SyncInterval
// get method for getting the Sync duration set
//=======================================================================
-(long)SyncInterval
{
	return [ [properties objectForKey:@"SyncInterval"] longValue];
}

//=======================================================================
// EffectiveSyncInterval
// get method for Effective Sync interval
//=======================================================================
-(long)EffectiveSyncInterval
{
	return [ [properties objectForKey:@"EffectiveSyncInterval"] longValue];
}

//=======================================================================
// SSL
// get the status of SSL for iFolder
//=======================================================================
-(BOOL)SSL
{
	NSNumber *num = [properties objectForKey:@"ssl"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}

//=======================================================================
// EncryptionAlgorithm
// get method for encryption algorithm used on iFolder
//=======================================================================
-(NSString *)EncryptionAlgorithm
{
	return [properties objectForKey:@"encryptionAlgorithm"];
}

//=======================================================================
// Shared
// get the status iFolder whether it is shared or not
//=======================================================================
-(BOOL) Shared
{
	NSNumber *num = [properties objectForKey:@"Shared"];
	if(num != nil)
		return [num boolValue];
	else
		return NO;
}
//=======================================================================
// DomainName
// get the domain name of this iFolder
//=======================================================================
-(NSString *) DomainName
{
       iFolderDomain *domain = [[iFolderData sharedInstance] getDomain: [self DomainID]];
       return [domain name];
}
//===================================================================
// updateDisplayInformation
// Updates the Window according to latest info available. First it
// checks whether it is on server or setup on client. If on server, 
// it will show as "not setup". If setup on client, depending on the
// status (state) ie, synced or conflict or waiting to sync etc, the icon 
// will be updated accordingly.
//===================================================================
-(void) updateDisplayInformation
{
	NSImage *statusImage = nil;
	if([self IsSubscription])
	{		
		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
			[properties setObject:NSLocalizedString(@"Not set up", @"iFolder Status Message") forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitConnect"])
			[properties setObject:NSLocalizedString(@"Waiting to connect", @"iFolder Status Message") forKey:@"Status"];
		else if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"])
			[properties setObject:NSLocalizedString(@"Not Setup", @"iFolder Status Message") forKey:@"Status"];
		else
			[properties setObject:NSLocalizedString(@"Unknown", @"iFolder Status Message") forKey:@"Status"];

		if([ [properties objectForKey:@"State"] isEqualToString:@"Available"])
		{
			// set the location to the owner's name
			[properties setObject:[properties objectForKey:@"Owner"]
								forKey:@"Location"];
		}
		
		NSImage *subscriptionImage = [NSImage imageNamed:@"serverifolder24"];
		[subscriptionImage setScalesWhenResized:YES];
		[properties setObject:subscriptionImage forKey:@"Image"];
	}
	else
	{
		if([self syncState] == SYNC_STATE_SYNCING)
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-sync48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Synchronizing", @"iFolder Status Message") forKey:@"Status"];
		}
		else if([self syncState] == SYNC_STATE_PREPARING)
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-waiting48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Preparing to synchronize", @"iFolder Status Message") forKey:@"Status"];
		}
		else if([self syncState] == SYNC_STATE_NOPASSPHRASE)
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-warning48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Passphrase not provided", @"No passphrase iFolder Status Message") forKey:@"Status"];
		}
		
		else if([self syncState] == SYNC_STATE_DISABLEDSYNC)
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-waiting48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Synchronization disabled", @"iFolder Status Message") forKey:@"Status"];
		}
				
		else if([ [properties objectForKey:@"HasConflicts"] boolValue])
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-conflict48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Has conflicts", @"iFolder Status Message") forKey:@"Status"];
		}
		
		else if([self syncState] == SYNC_STATE_DISCONNECTED)
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-warning48"];
			//Set the status
			[properties setObject:NSLocalizedString(@"Server unavailable", @"iFolder Status Message") forKey:@"Status"];
		}
		else if( ([self syncState] == SYNC_STATE_OUT_OF_SYNC) &&
				 ([self outOfSyncCount] > 0) )
		{
			//Set the image
			statusImage = [NSImage imageNamed:@"ifolder-error48"];
			//Set the status
			[properties setObject:[NSString stringWithFormat:NSLocalizedString(@"%u items not synchronized", @"iFolder Status Message"), [self outOfSyncCount]] forKey:@"Status"];
		}
		else if( ([self syncState] == 0) ||
				 ([self syncState] == SYNC_STATE_OK) )
		{
			if([ [properties objectForKey:@"State"] isEqualToString:@"WaitSync"] || [[properties objectForKey:@"State"] isEqualToString:@"InitialSync"] )
			{
				//Set the image
				statusImage = [NSImage imageNamed:@"ifolder-waiting48"];
				//Set the status
				[properties setObject:NSLocalizedString(@"Waiting to synchronize", @"iFolder Status Message") forKey:@"Status"];
			}
/*
			else if([ [properties objectForKey:@"State"] isEqualToString:@"InitialSync"])
			{
				//Set the image
				statusImage = [NSImage imageNamed:@"ifolder-waiting22"];
				//Set the status
				[properties setObject:NSLocalizedString(@"Waiting to synchronize", @"iFolder Status Message") forKey:@"Status"];
			}
*/
            else
			{
				if( ([[properties objectForKey:@"encryptionAlgorithm"] compare:@""] == NSOrderedSame) ||
						([properties objectForKey:@"encryptionAlgorithm"] == nil) )
                {
					if([self Shared])
					{
						statusImage = [NSImage imageNamed:@"ifolder_user_48"];
						
					}
					else
					{
						statusImage = [NSImage imageNamed:@"ifolder48"];	
					}
				}
				else
				{
					//Set the image
					statusImage = [NSImage imageNamed:@"encrypt-ilock-48"];
				}
				//Set the status
				NSString *statusMessage = [NSString stringWithFormat:NSLocalizedString(@"Synchronized: %@", @"iFolder Status Message"),[self LastSync]];
				[properties setObject:statusMessage forKey:@"Status"];
			}
		}
		
		[statusImage setScalesWhenResized:YES];
		[properties setObject:statusImage forKey:@"Image"];
		
		// update the location
		NSString *location = [properties objectForKey:@"Path"];
		if(location != nil)
			[properties setObject:location forKey:@"Location"];
		else
			[properties setObject:[properties objectForKey:@"Owner"]
							forKey:@"Location"];
	}
}

//=======================================================================
// SetOwner
// Sets the owner of the iFolder. We can change the owner of the iFolder.
//=======================================================================
-(void)SetOwner:(User *)user
{
	[properties setObject:[user Name] forKey:@"Owner"];
	[properties setObject:[user UserID] forKey:@"OwnerID"];
	[self updateDisplayInformation];	
}

@end
