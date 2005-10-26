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


#import "iFolderData.h"
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderService.h"
#import "SimiasService.h"
#import "iFolderWindowController.h"
#import "applog.h"


static iFolderData *sharedInstance = nil;

@implementation iFolderData

//===================================================================
// init
// Initialize the iFolderData
//===================================================================
- (id)init 
{
	@synchronized(self)
	{
		if (sharedInstance) 
		{
			[self dealloc];
		} 
		else
		{
			instanceLock = [[NSRecursiveLock alloc] init];
			sharedInstance = [super init];
		
			ifolderService = [[iFolderService alloc] init];
			simiasService = [[SimiasService alloc] init];	
			
			ifolderUserChanges = [[NSMutableDictionary alloc] init];	
			
			keyedDomains = [[NSMutableDictionary alloc] init];
			keyediFolders = [[NSMutableDictionary alloc] init];
			keyedSubscriptions = [[NSMutableDictionary alloc] init];
			
			ifolderdomains = [[NSMutableArray alloc] init];
			ifolders = [[NSMutableArray alloc] init];
			
//			ifolderDataAlias = [[NSObjectController alloc] initWithContent:self];

			ifoldersController = [[NSArrayController alloc] init];
			[ifoldersController setObjectClass:[iFolder class]];
			
			domainsController = [[NSArrayController alloc] init];
			[domainsController setObjectClass:[iFolderDomain class]];

//			[domainsController bind:@"contentArray" toObject:ifolderDataAlias
//					withKeyPath:@"selection.ifolderdomains" options:nil];
					
//			[ifoldersController bind:@"contentArray" toObject:ifolderDataAlias
//					withKeyPath:@"selection.ifolders" options:nil];

		}
		return sharedInstance;
	}
}




//===================================================================
// dealloc
// free up the iFolderData
//===================================================================
-(void) dealloc
{
	[ifolderService release];
	[simiasService release];
	[instanceLock release];
	[keyediFolders release];
	[keyedDomains release];
	[keyedSubscriptions release];
	[ifolderdomains release];
	[ifolders release];
//	[ifolderDataAlias release];
	[ifoldersController release];
	[domainsController release];
	[ifolderUserChanges release];

	[super dealloc];
}




//===================================================================
// sharedInstance
// get the single instance for the app
//===================================================================
+ (iFolderData *)sharedInstance
{
    return sharedInstance ? sharedInstance : [[self alloc] init];
}



//===================================================================
// domainArrayController
// returns the domain NSArrayController for the GUI to bind to
//===================================================================
-(NSArrayController *)domainArrayController
{
	return domainsController;
}


//===================================================================
// ifolderArrayController
// returns the iFolder NSArrayController for the GUI to bind to
//===================================================================
-(NSArrayController *)ifolderArrayController
{
	return ifoldersController;
}


//-(NSObjectController *)dataAlias
//{
//	return ifolderDataAlias;
//}





//===================================================================
// refresh
// reads the current domains and ifolders
//===================================================================
- (void)refresh:(BOOL)onlyDomains
{
	[instanceLock lock];

	ifconlog1(@"Refreshing iFolderData");

	@try
	{
		int objCount;
		
		[keyedDomains removeAllObjects];
//		[ifolderdomains removeAllObjects];
		// this is the clean way to remove all objects and make sure everyone
		// knows about it
		if([[domainsController arrangedObjects] count] > 0)
		{
			NSIndexSet *allIndexes = [[NSIndexSet alloc]
									initWithIndexesInRange:
									NSMakeRange(0,[[domainsController arrangedObjects] count])];
			[domainsController removeObjectsAtArrangedObjectIndexes:allIndexes];
		}

		NSArray *newDomains = [simiasService GetDomains:NO];
		for(objCount = 0; objCount < [newDomains count]; objCount++)
		{
			iFolderDomain *newDomain = [newDomains objectAtIndex:objCount];
			
			if( [[newDomain isDefault] boolValue] )
				defaultDomain = newDomain;

			[self _addDomain:newDomain];
		}
		
		if(!onlyDomains)
		{
			// Read all of the new ifolders and update them with the state
			// of the existing iFolders
			NSArray *newiFolders = [ifolderService GetiFolders];
			for(objCount = 0; objCount < [newiFolders count]; objCount++)
			{
				iFolder *newiFolder = [newiFolders objectAtIndex:objCount];
				
				iFolder *existingiFolder = [keyediFolders objectForKey:[newiFolder ID]];
				if(existingiFolder != nil)
				{
					[newiFolder setSyncProperties:[existingiFolder properties]];
				}
			}

			// Now that we have updated all of the new ifolders with the old
			// iFolder's existing state, remove all of them to get ready to
			// re-add the new ifolders
			[keyediFolders removeAllObjects];
			[keyedSubscriptions removeAllObjects];

			if([[ifoldersController arrangedObjects] count] > 0)
			{
				NSIndexSet *allIndexes = [[NSIndexSet alloc]
										initWithIndexesInRange:
										NSMakeRange(0,[[ifoldersController arrangedObjects] count])];
				[ifoldersController removeObjectsAtArrangedObjectIndexes:allIndexes];
			}

			// Now that the state is set in all of the existing iFolders
			// add them back into the iFolder list
			for(objCount = 0; objCount < [newiFolders count]; objCount++)
			{
				iFolder *newiFolder = [newiFolders objectAtIndex:objCount];

				[self _addiFolder:newiFolder];
			}
		}
	}
	@catch (NSException *e)
	{
		ifconlog1(@"Exception refreshing iFolderData");
		ifexconlog(@"iFolderData:refres", e);
	}

	ifconlog1(@"Done Refreshing iFolderData");	

	[instanceLock unlock];

	[self selectDefaultDomain];
}



//===================================================================
// _addDomain
// adds the Domain to the iFolderData structures
//===================================================================
-(void)_addDomain:(iFolderDomain *)domain
{
	[instanceLock lock];
	ifconlog2(@"Addding domain: %@", [domain name]);
	[domainsController addObject:domain];
	[keyedDomains setObject:domain forKey:[domain ID]];
	
	[instanceLock unlock];	
}




//===================================================================
// _addiFolder
// adds the iFolder to the iFolderData structures
//===================================================================
-(void)_addiFolder:(iFolder *)ifolder
{
	[instanceLock lock];
	if([ifolder IsSubscription])
	{
		[ifoldersController addObject:ifolder];
		NSString *colID = [ifolder CollectionID];
		[keyediFolders setObject:ifolder forKey:colID];
		[keyedSubscriptions setObject:colID forKey:[ifolder ID]];
	}
	else
	{
		[ifoldersController addObject:ifolder];
		[keyediFolders setObject:ifolder forKey:[ifolder ID]];
	}
	[instanceLock unlock];	
}




//===================================================================
// _deliFolder
// deletes the iFolder to the iFolderData structures
//===================================================================
-(void)_deliFolder:(NSString *)ifolderID
{
	[instanceLock lock];
	NSString *realID = ifolderID;
	
	if(![self isiFolder:realID])
	{
		realID = [self getiFolderID:ifolderID];
		if((realID == nil) || (![self isiFolder:realID]) )
		{
			[instanceLock unlock];	
			return;
		}
		else
		{
			// remove this key since we found it
			[keyedSubscriptions removeObjectForKey:ifolderID];
		}
	}

	iFolder *ifolder = [keyediFolders objectForKey:realID];
	if(ifolder == nil)
	{
		[instanceLock unlock];	
		return;
	}
	
	[keyediFolders removeObjectForKey:realID];
	[ifoldersController removeObject:ifolder];

	[instanceLock unlock];	
}




//===================================================================
// getiFolderID
// Gets the iFolder ID for a subscription we've added
//===================================================================
-(NSString *)getiFolderID:(NSString *)subscriptionID
{
	NSString *ifolderID = nil;
	[instanceLock lock];

	ifolderID = [[keyedSubscriptions objectForKey:subscriptionID] retain];

	[instanceLock unlock];
	return ifolderID;
}




//===================================================================
// isDomain
// Checks if the passed domainID is one of the current domains
//===================================================================
-(BOOL)isDomain:(NSString *)domainID
{
	BOOL retCode = NO;
	[instanceLock lock];

	if([keyedDomains objectForKey:domainID] != nil)
		retCode = YES;

	[instanceLock unlock];

	return retCode;
}




//===================================================================
// isiFolder
// Checks if the passed ifolderID is one of the current iFolders
//===================================================================
-(BOOL)isiFolder:(NSString *)ifolderID
{
	BOOL retCode = NO;
	[instanceLock lock];

	if([keyediFolders objectForKey:ifolderID] != nil)
		retCode = YES;

	[instanceLock unlock];

	return retCode;
}




//===================================================================
// getiFolder
// gets the iFolder from the iFolderData structures
//===================================================================
-(iFolder *)getiFolder:(NSString *)ifolderID
{
	iFolder *ifolder = nil;
	[instanceLock lock];
	ifolder = [keyediFolders objectForKey:ifolderID];
	[instanceLock unlock];	
	return ifolder;
}




//===================================================================
// readiFolder
// reads and returns the ifolder for the specified ifolderID
//===================================================================
-(iFolder *)readiFolder:(NSString *)ifolderID
{
	iFolder *ifolder = nil;
	[instanceLock lock];

	ifconlog2(@"iFolderData readiFolder called for iFolder %@", ifolderID);

	ifolder = [self getiFolder:ifolderID];

	@try
	{
		iFolder *newiFolder = [[ifolderService GetiFolder:ifolderID] retain];

		if(ifolder != nil)
		{
			[ifolder setProperties:[newiFolder properties]];
		}
		else
		{
			ifolder = newiFolder;
			[self _addiFolder:ifolder];
		}
		[newiFolder release];
	}
	@catch (NSException *e)
	{
		ifconlog1(@"Exception getting iFolder");
		ifexconlog(@"GetiFolder", e);
	}

	[instanceLock unlock];

	return ifolder;
}




//===================================================================
// createiFolder
// adds an iFolder
//===================================================================
-(void)createiFolder:(NSString *)path inDomain:(NSString *)domainID
{
	[instanceLock lock];

	@try
	{
		iFolder *newiFolder = [[ifolderService CreateiFolder:path InDomain:domainID] retain];
		[self _addiFolder:newiFolder];		
	}
	@catch(NSException *ex)
	{
		ifexconlog(@"iFolderData:createiFolder", ex);
		[instanceLock unlock];
		[ex raise];
	}

	[instanceLock unlock];
}





//===================================================================
// acceptiFolderInvitation
// accepts an iFolder Invitation and updates the iFolder Information
//===================================================================
- (void)acceptiFolderInvitation:(NSString *)iFolderID 
									InDomain:(NSString *)domainID 
									toPath:(NSString *)localPath
{
	[instanceLock lock];
	
	NSString *collectionID = [keyedSubscriptions objectForKey:iFolderID];
	NSAssert( (collectionID != nil), @"collectionID not found for subscription");
	
	@try
	{
		iFolder *newiFolder = [ifolderService AcceptiFolderInvitation:iFolderID 
													InDomain:domainID
													toPath:localPath];
		if([[newiFolder ID] compare:iFolderID] != 0)
		{
			[keyedSubscriptions removeObjectForKey:iFolderID];
			if([newiFolder IsSubscription])
				[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
		}

		iFolder *curiFolder = [keyediFolders objectForKey:collectionID];
		[curiFolder setProperties:[newiFolder properties]];
	}
	@catch (NSException *e)
	{
		[instanceLock unlock];
		[e raise];
	}
	[instanceLock unlock];
}




//===================================================================
// declineiFolderInvitation
// decliens an iFolder invitation
//===================================================================
-(void)declineiFolderInvitation:(NSString *)iFolderID 
									fromDomain:(NSString *)domainID
{
	[instanceLock lock];
	
	// Get the collectionID for this subscription
	NSString *collectionID = [keyedSubscriptions objectForKey:iFolderID];
	NSAssert( (collectionID != nil), @"collectionID not found for subscription");
	
	@try
	{
		[ifolderService DeclineiFolderInvitation:iFolderID fromDomain:domainID];
		[self _deliFolder:iFolderID];
	}
	@catch (NSException *e)
	{
		[instanceLock unlock];
		[e raise];
	}
	[instanceLock unlock];
}





//===================================================================
// revertiFolder
// reverts an iFolder to an invitation
//===================================================================
- (void)revertiFolder:(NSString *)iFolderID
{
	[instanceLock lock];

	iFolder *curiFolder = [keyediFolders objectForKey:iFolderID];
	NSAssert( (curiFolder != nil), @"iFolderID did not match an existing iFolder");

	iFolder *revertediFolder;
	@try
	{
		revertediFolder = [ifolderService RevertiFolder:iFolderID];

		[curiFolder setProperties:[revertediFolder properties]];

		if([revertediFolder IsSubscription])
		{
			[keyedSubscriptions setObject:[revertediFolder CollectionID] forKey:[revertediFolder ID]];
		}
	}
	@catch (NSException *e)
	{
		[instanceLock unlock];
		[e raise];
	}
	[instanceLock unlock];
}




//===================================================================
// deleteiFolder
// reverts an iFolder and then decline's it's invitation
//===================================================================
-(void)deleteiFolder:(NSString *)ifolderID
{
	[instanceLock lock];

	iFolder *revertediFolder = nil;

	@try
	{
		if([self isiFolder:ifolderID])
		{
			// Take care to delete this ifolder if it's role is master on this
			// collection (workgroup)
			iFolder *ifolder = [self getiFolder:ifolderID];
			if( (ifolder != nil) && ([[ifolder Role] compare:@"Master"] == 0) )
			{
				[ifolderService DeleteiFolder:ifolderID];
				[self _deliFolder:ifolderID];
				return;
			}
		
			// This is a real iFolder so revert it and get the invitaion ifolder
			revertediFolder = [ifolderService RevertiFolder:ifolderID];
			[self _deliFolder:ifolderID];
		}
		else
		{
			// This is not a real iFolder so get the invitation ifolder
			NSString *realID = [self getiFolderID:ifolderID];
			if(realID != nil)
			{
				revertediFolder = [self getiFolder:realID];
			}
		}

		if( (revertediFolder != nil) && ([revertediFolder IsSubscription]) )
		{
			[ifolderService DeclineiFolderInvitation:[revertediFolder ID]
												fromDomain:[revertediFolder DomainID]];
			[self _deliFolder:[revertediFolder ID]];
		}
	}
	@catch (NSException *e)
	{
		[instanceLock unlock];
		[e raise];
	}
	[instanceLock unlock];	
}




//===================================================================
// getAvailableiFolder
// gets the available iFolder from the iFolderData structures
//===================================================================
-(iFolder *)getAvailableiFolder:(NSString *)ifolderID
{
	iFolder *ifolder = nil;
	[instanceLock lock];

	NSString *realID = [self getiFolderID:ifolderID];
	if(realID != nil)
		ifolder = [keyediFolders objectForKey:realID];

	[instanceLock unlock];	
	return ifolder;
}




//===================================================================
// readAvailableiFolder
// returns the iFolder (invitation) for the specified ID
//===================================================================
-(iFolder *)readAvailableiFolder:(NSString *)ifolderID 
									inCollection:(NSString *)collectionID
{
	iFolder *ifolder = nil;
	[instanceLock lock];

	ifolder = [self getAvailableiFolder:ifolderID];

	// If we read an iFolder and it is not a subscription, remove it from the
	// available iFolders and return nil
	if(	(ifolder != nil) &&
		(![ifolder IsSubscription]) )
	{
		[keyedSubscriptions removeObjectForKey:ifolderID];
		[instanceLock unlock];	
		return nil;
	}

	@try
	{
		iFolder *newiFolder = [[ifolderService GetAvailableiFolder:ifolderID
													inCollection:collectionID] retain];
		if(ifolder != nil)
			[ifolder setProperties:[newiFolder properties]];
		else
		{
			// If there isn't an iFolder already there with this collectionID
			// the addit, otherwise don't
			if([self getiFolder:[newiFolder CollectionID]] == nil)
			{
				ifolder = newiFolder;
				[self _addiFolder:ifolder];
			}
		}
		[newiFolder release];
	}
	@catch (NSException *e)
	{
		ifexconlog(@"readAvailableiFolder", e);
	}

	[instanceLock unlock];

	return ifolder;
}




//===================================================================
// isPOBox
// Checks if the passed nodeIDs is one of the current POBoxes
//===================================================================
-(BOOL)isPOBox:(NSString *)nodeID
{
	BOOL retCode = NO;
	[instanceLock lock];

	NSEnumerator *enumerator = [keyedDomains objectEnumerator];
	iFolderDomain *dom;

	while ((dom = [enumerator nextObject]))
	{
		if([[dom poBoxID] compare:nodeID] == 0)
		{
			retCode = YES;
			break;
		}
	}

	[instanceLock unlock];

	return retCode;
}




//===================================================================
// getDomain
// Returns the domain for the current domainID or nil if it ain't there
//===================================================================
-(iFolderDomain *)getDomain:(NSString *)domainID
{
	iFolderDomain *dom;
	[instanceLock lock];

	dom = [keyedDomains objectForKey:domainID];

	[instanceLock unlock];

	return dom;
}


//===================================================================
// getDomain
// Returns the domain for the current domainID or nil if it ain't there
//===================================================================
-(iFolderDomain *)getPOBoxDomain:(NSString *)poBoxID
{
	iFolderDomain *dom;
	[instanceLock lock];

	NSEnumerator *enumerator = [keyedDomains objectEnumerator];

	while ((dom = [enumerator nextObject]))
	{
		if([[dom poBoxID] compare:poBoxID] == 0)
		{
			break;
		}
	}

	[instanceLock unlock];

	return dom;
}




//===================================================================
// getDomains
// Returns an array of all current Domains
//===================================================================
-(NSArray *)getDomains
{
	NSArray *alldomains;
	[instanceLock lock];
	alldomains = [keyedDomains allValues];
	[instanceLock unlock];	
	return alldomains;
}




//===================================================================
// getiFolders
// Returns an array of all current iFolders
//===================================================================
-(NSArray *)getiFolders
{
	NSArray *allifolders;
	[instanceLock lock];
	allifolders = [keyediFolders allValues];
	[instanceLock unlock];	
	return allifolders;
}




//===================================================================
// getDefaultDomain
// Returns the default domain
//===================================================================
-(iFolderDomain *)getDefaultDomain
{
	iFolderDomain *dom;
	[instanceLock lock];
	dom = defaultDomain;
	[instanceLock unlock];	
	return dom;
}




//===================================================================
// getDomainCount
// Returns the number of current domains
//===================================================================
-(int)getDomainCount
{
	int count = 0;
	[instanceLock lock];
	count = [keyedDomains count];
	[instanceLock unlock];	
	return count;	
}




//===================================================================
// getSyncSize
// Returns the syncSize for an iFolder
//===================================================================
-(SyncSize *)getSyncSize:(NSString *)ifolderID
{
	SyncSize *ss = nil;
	[instanceLock lock];
	@try
	{
		ss = [[ifolderService CalculateSyncSize:ifolderID] retain];
	}
	@catch(NSException *e)
	{
		ss = [[SyncSize alloc] init];
	}
	[instanceLock unlock];	
	return [ss autorelease];
}




//===================================================================
// selectDefaultDomain
// This will select the default domain if there is one and select
// the first one if not
//===================================================================
-(void)selectDefaultDomain
{
	[instanceLock lock];

	if(defaultDomain != nil)
	{
		int index = [[domainsController arrangedObjects] indexOfObject:defaultDomain];
		if(index != NSNotFound)
			[domainsController setSelectionIndex:index];
		else
			[domainsController setSelectionIndex:0];
	}
	else
		[domainsController setSelectionIndex:0];

	[instanceLock unlock];	
}



-(void)setUsersAdded:(NSString *)ifolderID
{
	ifconlog2(@"Setting user added for %@", ifolderID);
	[instanceLock lock];
	[ifolderUserChanges setObject:[NSNumber numberWithBool:YES] forKey:ifolderID];
	[instanceLock unlock];	
}



-(BOOL)usersAdded:(NSString *)ifolderID
{
	ifconlog2(@"Checking user added for %@", ifolderID);
	BOOL useradded = NO;
	[instanceLock lock];
	useradded = ([ifolderUserChanges objectForKey:ifolderID] != nil);
	[instanceLock unlock];	
	return useradded;
}



-(void)clearUsersAdded:(NSString *)ifolderID
{
	ifconlog2(@"Clearing user added for %@", ifolderID);
	[instanceLock lock];
	[ifolderUserChanges removeObjectForKey:ifolderID];
	[instanceLock unlock];	
}

-(MemberSearchResults *) InitMemberSearchResults
{
	return [simiasService InitMemberSearchResults];
}



@end
