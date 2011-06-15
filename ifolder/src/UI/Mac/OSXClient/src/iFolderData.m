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
*                 $Modified by: Satyam <ssutapalli@novell.com>	29/02/2007     Implemented Merge iFolder process
*                 $Modified by: Satyam <ssutapalli@novell.com>	10/04/2008     Added functionality for default ifolder
*                 $Modified by: Satyam <ssutapalli@novell.com>	24/04/2008     Checking ifolder limit policy while creating new iFolder 
*                 $Modified by: Satyam <ssutapalli@novell.com>	13/05/2008     Changed Reset PP Success message
*                 $Modified by: Satyam <ssutapalli@novell.com>	22/05/2008     Modified Create iFolder method signature
*                 $Modified by: Satyam <ssutapalli@novell.com>	20/08/2008     Added functionality to clear PP and getting remember PP option, also commented Re-logi
n dialog after resetting PP
*                 $Modified by: Satyam <ssutapalli@novell.com>  09/09/2008     Added new function for calling IsPassPhraseSet from Simias
*                 $Modified by: Satyam <ssutapalli@novell.com>	18/09/2008     Commented the code which uses poBoxID of a domain
*                 $Modified by: Satyam <ssutapalli@novell.com>	02/12/2008     Handling the UI refresh timer in "refresh" method than in menu event, returning valid value in createiFolder after checking the limit policy status
*                 $Modified by: Satyam <ssutapalli@novell.com>  02/06/2009     Added new functions required for Forgot PP dialog
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "iFolderApplication.h"
#import "iFolderData.h"
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderService.h"
#import "SimiasService.h"
#import "iFolderWindowController.h"
#import "applog.h"
#import "VerifyPassPhraseController.h"
#import "iFolderEncryptController.h"
#import "AuthStatus.h"
#import "clientUpdate.h"

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
			
			loggedDomainsController = [[NSArrayController alloc] init];
			[loggedDomainsController setObjectClass:[iFolderDomain class]];

//			[domainsController bind:@"contentArray" toObject:ifolderDataAlias
//					withKeyPath:@"selection.ifolderdomains" options:nil];
					
//			[ifoldersController bind:@"contentArray" toObject:ifolderDataAlias
//					withKeyPath:@"selection.ifolders" options:nil];

//			forceQuit = NO;
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
// ForceQuit
// When an update is made in client, it must quit application without
// showing any dialog to quit. For this purpose this is used.
//===================================================================
/*- (BOOL)ForceQuit
{
	return forceQuit;
}*/

//===================================================================
// domainArrayController
// returns the domain NSArrayController for the GUI to bind to
//===================================================================
-(NSArrayController *)domainArrayController
{
	return domainsController;
}

//===================================================================
// loggedDomainArrayController
// returns the logged in domains NSArrayController for the GUI to bind to
//===================================================================
-(NSArrayController *)loggedDomainArrayController
{
	return loggedDomainsController;
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
	
	[[NSApp delegate] stopRefreshTimer];
	
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
		
		if([[loggedDomainsController arrangedObjects] count] > 0)
		{
			NSIndexSet *allLoggedIndexes = [[NSIndexSet alloc]
									  initWithIndexesInRange:
									  NSMakeRange(0,[[loggedDomainsController arrangedObjects] count])];
			[loggedDomainsController removeObjectsAtArrangedObjectIndexes:allLoggedIndexes];
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

[ifoldersController rearrangeObjects];

	[[NSApp delegate] startRefreshTimer];
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
	[domainsController addObject:domain];
	[keyedDomains setObject:domain forKey:[domain ID]];
	//Only if the domain is authenticated, add to logged doamins list
	if([domain authenticated])
		[loggedDomainsController addObject:domain];   
	
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
		[NSThread sleepUntilDate:[[NSDate date] addTimeInterval:.03] ];
	}
	else
	{
		[ifoldersController addObject:ifolder];
		[keyediFolders setObject:ifolder forKey:[ifolder ID]];
	}
	
	
	[instanceLock unlock];
	[ifoldersController rearrangeObjects];	
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

-(BOOL)isiFolderByPath:(NSString*)localPath
{
	return [ifolderService IsiFolder:localPath];
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

	//ifconlog2(@"iFolderData readiFolder called for iFolder %@", ifolderID);

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
	[ifoldersController rearrangeObjects];

	return ifolder;
}




//===================================================================
// createiFolder
// adds an iFolder
//===================================================================
-(NSString*)createiFolder:(NSString *) path inDomain:(NSString *)domainID withSSL:(BOOL)ssl usingAlgorithm:(NSString *)encrAlgthm usingPassPhrase:(NSString *)passPhrase
{
	iFolder* newiFolder = nil;
	NSString* returnValue = nil;
	[instanceLock lock];
	
	
	if([ifolderService GetLimitPolicyStatus:domainID] != 1)
	{
		NSRunAlertPanel(NSLocalizedString(@"iFolder Error",@"iFolderLimitPolicyStatusTitle"),NSLocalizedString(@"iFolder could not be created as you are exceeding the limit of ifolders set by your Administrator.",@"iFolderLimitPolicyStatusMessage"), NSLocalizedString(@"OK",@"OK Button"),nil,nil);
	}
	else
	{
		@try
	    {
			newiFolder = [[ifolderService CreateEncryptediFolder:path InDomain:domainID 
													 withSSL:ssl usingAlgorithm:encrAlgthm usingPassPhrase:passPhrase] retain];

			if(newiFolder != nil)
			{
				[self _addiFolder:newiFolder];
				returnValue = [newiFolder ID];
			}
		}
		@catch(NSException *ex)
	    {
			ifexconlog(@"iFolderDate:createiFolder(encrypted)",ex);
			NSRunAlertPanel(NSLocalizedString(@"Exception in Create iFolder",@"Create iFolder Exception Title"), 
							NSLocalizedString(@"Invalid Path. Please check the path that you entered",@"Create iFolder Exception Message"), 
							NSLocalizedString(@"OK",@"OK Button"),nil,nil);
			
			[instanceLock unlock];
			[ex raise];
		}
	}
	
	
	[instanceLock unlock];
	return returnValue;
}


//===================================================================
// acceptiFolderInvitation
// accepts an iFolder Invitation and updates the iFolder Information 
// checks for Merge and does merging accordingly
//===================================================================
-(void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath canMerge:(BOOL)merge
{
	BOOL download = NO;
	iFolder *folder = [ifolderService GetiFolder:iFolderID];
	if ([folder EncryptionAlgorithm] == nil || [[folder EncryptionAlgorithm] compare:@""] == NSOrderedSame)
	{
		download = YES;
	}
	else
	{
		NSString* passPhraseCheck = nil;
		passPhraseCheck = [simiasService GetPassPhrase:domainID];
		if(passPhraseCheck == nil || [passPhraseCheck compare:@""] == NSOrderedSame)
		{
			[[VerifyPassPhraseController verifyPPInstance] setAndShow:simiasService andDomain:domainID];
			[NSApp runModalForWindow:[[VerifyPassPhraseController verifyPPInstance] window]];
			NSString* pp = [simiasService GetPassPhrase:domainID];
			if(pp != nil)
			{
				download = YES;
			}
			else
			{
				download = NO;
			}
		}
		else
		{
			download = YES;
		}
	}
	
	[instanceLock lock];
	
	NSString *collectionID = [keyedSubscriptions objectForKey:iFolderID];
	NSAssert( (collectionID != nil), @"collectionID not found for subscription");
	iFolder *newiFolder = nil;
	if(download)
	{
		if(!merge)
		{
			@try
		{
			newiFolder = [ifolderService AcceptiFolderInvitation:iFolderID 
														InDomain:domainID
														  toPath:localPath];
		}
			@catch (NSException *e)
		{
				[instanceLock unlock];
				[e raise];
		}
		}
		else
		{
			@try
		{
			newiFolder = [ifolderService MergeiFolder:iFolderID 
											 InDomain:domainID
											   toPath:localPath];
		}
			@catch(NSException *e)
		{
				[instanceLock unlock];
				[e raise];
		}
		}
		
		if([[newiFolder ID] compare:iFolderID] != 0)
		{
			[keyedSubscriptions removeObjectForKey:iFolderID];
			if([newiFolder IsSubscription])
				[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
		}
		
		iFolder *curiFolder = [keyediFolders objectForKey:collectionID];
		[curiFolder setProperties:[newiFolder properties]];
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
-(void)deleteiFolder:(NSString *)ifolderID fromDomain:(NSString *) DomainID
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
			if([[ifolder CurrentUserID] compare:[ifolder OwnerUserID]] == 0)
			{
				[ifolderService DeleteiFolder: ifolderID fromDomain:[ifolder DomainID]];
				[self _deliFolder:ifolderID];
				return;
			}
		
			// This is a real iFolder so revert it and get the invitaion ifolder
			//revertediFolder = [ifolderService RevertiFolder:ifolderID];
			if(![ifolder IsSubscription])		//if on the local box, only then revert
					revertediFolder	=   [ifolderService RevertiFolder:ifolderID];
			
			[ifolderService DeclineiFolderInvitation: ifolderID fromDomain:[ifolder DomainID]];
			
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
// synciFolderNow
// causes an iFolder to be synced
//===================================================================
-(void)synciFolderNow:(NSString *)ifolderID
{
	[instanceLock lock];
	@try
	{
		ifconlog2(@"Calling to refresh: %@", ifolderID);
		[ifolderService SynciFolderNow:ifolderID];
	}
	@catch (NSException *e)
	{
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
		if([dom poBoxID] != nil && [[dom poBoxID] compare:nodeID] == 0)
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
	/* Since POBox creation is completely removed from 3.7 and above clients, commenting this code
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
	*/
	return nil;
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


-(int)getLoggedDomainCount
{
	iFolderDomain* dom = nil;
	int loggedDomainCount = 0;
	
	[instanceLock lock];
	
	NSArray* loggedDomains = [self getDomains];

	NSEnumerator *enumerator = [loggedDomains objectEnumerator];
	
	while ((dom = [enumerator nextObject]))
	{
		if([dom authenticated])
		{
			loggedDomainCount++;
		}
	}
	
	[instanceLock unlock];
	
	return loggedDomainCount;
	
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

//===================================================================
// selectDefaultDomain
// This will select the default domain if there is one and select
// the first one if not
//===================================================================
-(void)selectDefaultLoggedDomain
{
	[instanceLock lock];
	
	if(defaultDomain != nil)
	{
		int index = [[loggedDomainsController arrangedObjects] indexOfObject:defaultDomain];
		if(index != NSNotFound)
			[loggedDomainsController setSelectionIndex:index];
		else
			[loggedDomainsController setSelectionIndex:0];
	}
	else
		[loggedDomainsController setSelectionIndex:0];
	
	[instanceLock unlock];	
}



-(void)setUsersAdded:(NSString *)ifolderID
{
	[instanceLock lock];
	[ifolderUserChanges setObject:[NSNumber numberWithBool:YES] forKey:ifolderID];
	[instanceLock unlock];	
}



-(BOOL)usersAdded:(NSString *)ifolderID
{
	BOOL useradded = NO;
	[instanceLock lock];
	//useradded = ([ifolderUserChanges objectForKey:ifolderID] != nil);
	useradded = [[ifolderUserChanges valueForKey:ifolderID] boolValue];
	[instanceLock unlock];	
	return useradded;
}



-(void)clearUsersAdded:(NSString *)ifolderID
{
	[instanceLock lock];
	[ifolderUserChanges removeObjectForKey:ifolderID];
	[instanceLock unlock];	
}

-(MemberSearchResults *) InitMemberSearchResults
{
	return [simiasService InitMemberSearchResults];
}

-(void)readCredentials
{
	[instanceLock lock];
	[simiasService readCredentials];
	[ifolderService readCredentials];
	[instanceLock unlock];		
}

//=====================================================================
// getSecurityPolicy
// Returns the security policy for a particular domain by getting it 
// from Simias.
//=====================================================================
-(int) getSecurityPolicy:(NSString*)domainID
{
	return [ifolderService GetSecurityPolicy:domainID];
}

//=====================================================================
// getPassPhrase
// It gets the pass phrase from simias, if not available, it will ask
// for inputting pass phrase or to validate the pass phrase if already
// set. Then it will return the pass phrase.
//=====================================================================
-(NSString*) getPassPhrase:(NSString*)domID
{
	BOOL passPhraseStatus = NO;

	[self checkForEncryption:domID atLogin:NO];
	NSString* pp = [simiasService GetPassPhrase:domID];

	if(pp != nil)
		return pp;
	
	return nil;
}

//=====================================================================
// getRAName
// It gets the recovery agent name for the domainID that it receives
//=====================================================================
-(NSString*)getRAName:(NSString*)domainID
{
	return [ifolderService GetRAName:domainID];
}

//=====================================================================
// exportiFoldersCryptoKeys
// This method will export the ifolder's keys encrypted.
//=====================================================================
-(BOOL)exportiFoldersCryptoKeys:(NSString*)domainID withfilePath:(NSString*)filePath
{
	@try
	{
		[simiasService ExportiFolderCryptoKeys:domainID withFile:filePath];		
	}
	@catch(NSException* ex)
	{
		NSRunAlertPanel(NSLocalizedString(@"Export Encrypted Keys",@"ExportKey Title"),
						[NSString stringWithFormat:NSLocalizedString(@"Unable to export keys: %@",@"ExportKey Failure Message"),ex],
						NSLocalizedString(@"OK",@"Export Keys successfully"),
						nil,nil);	
		
		return NO;
	}
	
	return YES;
}

//=====================================================================
// importiFoldersCryptoKeys
// This method will import ifolders decrypted keys.
//=====================================================================
-(BOOL)importiFoldersCryptoKeys:(NSString*)domainID withNewPP:(NSString*)newPassPhrase onetimePassPhrase:(NSString*)otPassPhrase andFilePath:(NSString*)filePath
{
	@try
	{
		[simiasService ImportiFoldersCryptoKeys:domainID withNewPP:newPassPhrase onetimePassPhrase:otPassPhrase andFilePath:filePath];
	}
	@catch(NSException *ex)
	{
		NSRunAlertPanel(NSLocalizedString(@"Import Decrypted Keys",@"ImportKey Title"),
						[NSString stringWithFormat:NSLocalizedString(@"Error importing the keys: %@ for Reason: %@",@"ImportKey Failure Message"),[ex name],[ex reason]],
						NSLocalizedString(@"OK",@"Import Keys Check"),
						nil,nil);
		
		return NO;
	}
	
	return YES;
}

//=====================================================================
// getRAListOnClient
// This method will get the recovery agents of a domain using simias
//=====================================================================
-(NSArray*)getRAListOnClient:(NSString*)domainID
{
	NSArray* recoveryAgents = nil;
	
	recoveryAgents = [simiasService GetRAListOnClient:domainID];
		
	return recoveryAgents;
}

//=====================================================================
// validatePassPhrase
// This method will validate the domains pass phrase, if failed shows
// an alert
//=====================================================================
-(BOOL)validatePassPhrase:(NSString*)domainID andPassPhrase:(NSString*)pPhrase
{
	int passPhraseStatus;
	@try
	{
		AuthStatus* authStatus = [simiasService ValidatePassPhrase:domainID withPassPhrase:pPhrase];
		passPhraseStatus =  [[authStatus statusCode] intValue];
	}
	@catch(NSException* e)
	{
		ifexconlog(@"iFolderData:resetPassPhrase", e);
	}
	
	if(passPhraseStatus == ns1__StatusCodes__PassPhraseInvalid)
	{
		NSRunAlertPanel(NSLocalizedString(@"Invalid Passphrase",@"Invalid passphrase title"),
						NSLocalizedString(@"The passphrase entered is invalid",@"Invalid passphrase"),
						NSLocalizedString(@"OK",@"Verify passphrase default button"),
						nil,nil);
		return NO;
	}	
	return YES;
}

//=====================================================================
// getCertificate
// This method will get the RA certificate for the domain
//=====================================================================
-(SecCertificateRef)getCertificate:(NSString*)domainID withRAName:(NSString*)raName
{	
	SecCertificateRef certRef = NULL;
	@try
	{
		certRef = [simiasService GetRACertificateOnClient:domainID withRecoveryAgent:raName];
	}
	@catch(NSException *ex)
	{
		ifexconlog(@"GetCertificate", ex);
	}	

	return certRef;
}

//=====================================================================
// reSetPassPhrase
// This method will reset the passphrase for the domainID using the 
// new pass phrase and RA name
//=====================================================================
-(BOOL)reSetPassPhrase:(NSString*)domainID oldPassPhrase:(NSString*)oldPP passPhrase:(NSString*)newPP withRAName:(NSString*)raName
{
	NSString* recAgent = @"";
	NSString* publicKey = @"";
	if(![raName isEqualToString:NSLocalizedString(@"Server_Default",@"Server_Default encrypt RA")])
	{
		//recAgent = raName;
		@try
		{
			publicKey = [simiasService GetPublicKey:domainID forRecoveryAgent:raName];
		}
			@catch(NSException *ex)
		{
				ifexconlog(@"GetPublicKey", ex);
		}		
	}
	else
	{
		iFolderDomain *domainInfo = [simiasService GetDomainInformation:domainID];
		publicKey = [self getDefaultServerPublicKey:domainID forUser:[domainInfo userID]];
		raName = @"DEFAULT";
	}
	
	int resetPassPhraseStatus;
	@try
	{
		AuthStatus* authStatus = [simiasService ReSetPassPhrase:domainID oldPassPhrase:oldPP passPhrase:newPP withRAName:raName andPublicKey:publicKey];
		
		resetPassPhraseStatus = [[authStatus statusCode] intValue];
	}
	@catch(NSException* ex)
	{
		ifexconlog(@"reSetPassPhrase",ex);
	}
	
	if(resetPassPhraseStatus == ns1__StatusCodes__Success)
	{
		/*
		NSRunAlertPanel(NSLocalizedString(@"Successfully Reset the Passphrase",@"ResetPP Success Title"),
				NSLocalizedString(@"Please Re-login to iFolder and web client to continue the encrypted iFolder Processing",@"ResetPP Success Message"),
				NSLocalizedString(@"OK",@"OK Button"),
						nil,nil);
		 */
		
		return YES;
	}
	else
	{
		NSRunAlertPanel(NSLocalizedString(@"Reset PassPhrase",@"ResetPP Title"),
	                    NSLocalizedString(@"Error changing the Passphrase",@"ResetPP Failure Message"),
						NSLocalizedString(@"OK",@"OKButton Text"),
						nil,nil);
		return NO;
	}
	
	return NO;
}

-(AuthStatus*)loginToRemoteDomain:(NSString*)domainID usingPassword:(NSString*)password
{
	return [simiasService LoginToRemoteDomain:domainID usingPassword:password];
}

-(AuthStatus *)logoutFromRemoteDomain:(NSString *)domainID
{
	return [simiasService LogoutFromRemoteDomain:domainID];
}

-(iFolderDomain*)connectToDomain:(NSString *)UserName usingPassword:(NSString *)Password andHost:(NSString *)Host
{
	return [simiasService ConnectToDomain:UserName usingPassword:Password andHost:Host];
}

-(void)clearPassPhrase: (NSString*)domainID
{
	[simiasService StorePassPhrase:domainID PassPhrase:@"" Type:None andRememberPP:NO];	
}

-(void)storePassPhrase: (NSString*)domainID	PassPhrase:(NSString*)passPhrase andRememberPP:(BOOL)rememberPassPhrase
{
	[simiasService StorePassPhrase:domainID PassPhrase:passPhrase Type:Basic andRememberPP:rememberPassPhrase];
}

-(NSArray *) getiFolderConflicts:(NSString *)ifolderID
{
	return [ifolderService GetiFolderConflicts:ifolderID];
}

-(void)resolveNameConflict:(NSString*)iFolderID withID:(NSString*)serverID usingName:(NSString*)serverName
{
	[ifolderService ResolveNameConflict:iFolderID withID:serverID usingName:serverName];
}

-(void)renameAndResolveConflict:(NSString*)iFolderID withID:(NSString*)serverID usingFileName:(NSString*)newName
{
	[ifolderService RenameAndResolveConflict:iFolderID withID:serverID usingFileName:newName];
}

-(void)resolveEnhancedFileConflict:(NSString*)ifolderID havingConflictID:(NSString*)conflictID hasLocalChange:(BOOL)localOnly withConflictBinPath:(NSString*)conflictBinPath
{
	[ifolderService ResolveEnhancedFileConflict:ifolderID havingConflictID:conflictID hasLocalChange:localOnly withConflictBinPath:conflictBinPath];		
}

-(NSString*)getDefaultServerPublicKey:(NSString*)domainID forUser:(NSString*)userID
{
	return [ifolderService GetDefaultServerPublicKey:domainID forUser:userID];
}

-(void) exportRecoverImport:(NSString*)domainID forUser:(NSString*)userID withPassphrase:(NSString*)newPP
{
	[simiasService ExportRecoverImport:domainID forUser:userID withPassphrase:newPP];
}

-(BOOL)createDirectoriesRecurssively:(NSString*)path
{
	NSFileManager *fm = [NSFileManager defaultManager];
	BOOL isDirectory;
	BOOL isSubDir;
	
	if([path hasSuffix:@"/"])
	{
		path = [path stringByDeletingPathExtension];
	}
	
	if(![fm fileExistsAtPath:path isDirectory:&isDirectory])
	{
		NSCharacterSet* slashSet = [NSCharacterSet characterSetWithCharactersInString:@"/"];
		NSScanner* scanner = [NSScanner scannerWithString:path];
		NSString* createPath = @"";
		NSString* folderName = nil;
		while([scanner isAtEnd] == NO)
		{
			if([scanner scanString:@"/" intoString:nil ] && [scanner scanUpToCharactersFromSet:slashSet intoString:&folderName])
			{
				createPath = [createPath stringByAppendingFormat:@"/%@",folderName];
				if(![fm fileExistsAtPath:createPath isDirectory:&isSubDir])
				{
					if(![fm createDirectoryAtPath:createPath attributes:nil])
					{
						NSRunAlertPanel(NSLocalizedString(@"Directory creation failed",@"Creating Directory failure title"),
										NSLocalizedString(@"Cannot create directory at the specified path.",@"Creating Directory failure"),
										NSLocalizedString(@"OK",@"OK Button"),nil,nil);
						
						return NO;
					}
				}
			}
			else if(!isSubDir)
			{
				NSRunAlertPanel(NSLocalizedString(@"Invalid Directory",@"Invalid Directory Selection title"),
								NSLocalizedString(@"Selected path is a file. Please select a Directory",@"Invalid Directory Selection Message"),
								NSLocalizedString(@"OK",@"OK Button"),nil,nil);
				return NO;
			}
		}
	}
	else if(!isDirectory)
	{
		NSRunAlertPanel(NSLocalizedString(@"Not a valid Directory name",@"Invalid Default Directory Selection Title"),
						NSLocalizedString(@"The path entered is not a valid directory. Please select/enter a valid directory name",@"Invalid Default Directory Selection Message"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		return NO;
	}
	
	return YES;
}

//=======================================================================
// checkForEncryption
// This method will check whether the domainID received is set for 
// encryption or not. If set, tries to get the pass phrase. Else, it will
// display asking for new pass phrase or validating old pass phrase thru
// UI.
//=======================================================================
//- (void)checkForEncryption:(NSString*)domID
-(void)checkForEncryption:(NSString*)domID atLogin:(BOOL)loginFlag
{
	BOOL passPhraseStatus = NO;
	int securityPolicy = 0;
	
	if( ifolderService == nil )
	{
		return;
	}
	
	securityPolicy = [ifolderService GetSecurityPolicy:domID];
	
	if( securityPolicy % 2 == 0 )
	{
		return;
	}
	
	@try 
	{
		passPhraseStatus = [simiasService IsPassPhraseSet:domID];
	}
	@catch (NSException * e) 
	{
		ifexconlog(@"PassPhraseStatus",e);
		return;
	}
	
	
	if(passPhraseStatus == YES)
	{
		NSString* passPhraseValue = nil;
		if(loginFlag)
		{
			if([simiasService GetRememberPassPhraseOption:domID] == YES)
			{
				passPhraseValue = [simiasService GetPassPhrase:domID];
			}			
		}
		else
		{
			passPhraseValue = [simiasService GetPassPhrase:domID];
		}
		
		if(passPhraseValue == nil || [passPhraseValue isEqualToString:@""])
		{
			[[VerifyPassPhraseController verifyPPInstance] setAndShow:simiasService andDomain:domID];
			[NSApp runModalForWindow:[[VerifyPassPhraseController verifyPPInstance] window]];
		}
	}
	else
	{
		[[iFolderEncryptController encryptionInstance] setAndShow:simiasService andDomain:domID];
		[NSApp runModalForWindow:[[iFolderEncryptController encryptionInstance] window]];
	}
}

//=======================================================================
// getDefaultiFolder
// This method will return the default ifolder for a domain thru simias
//=======================================================================
-(NSString*)getDefaultiFolder:(NSString*)domID
{
	return [simiasService GetDefaultiFolder:domID];
}

-(BOOL)defaultAccountInDomainID:(NSString*)domainID foriFolderID:(NSString*)ifolderID
{
	return [simiasService DefaultAccountInDomainID:domainID foriFolderID:ifolderID];
}

-(BOOL)getRememberPassphraseOption: (NSString*) domainID
{
	return [simiasService GetRememberPassPhraseOption:domainID];
}

-(BOOL)canOwnerBeChanged:(NSString*)newUserID forDomain:(NSString*)domainID
{
	return [ifolderService CanOwnerBeChanged:newUserID forDomain:domainID];
}

-(BOOL) isPassPhraseSet:(NSString*)domainID
{
	return [simiasService IsPassPhraseSet:domainID];
}

//=====================================================================
// clientUpdates
// This method will check for latest client updates.
// Note: need to handle the download and run latest updates.
//=====================================================================
-(void)clientUpdates:(NSString*)domID
{	
	cliUpdate = nil;
	//forceQuit = NO;
	
	//Variables needed to handle upgrade client
	NSString* dirName;
	int loopCount;
	int result;
	BOOL updateStatus;
	
	int answer;
	ifconlog2(@"clientupdate..%@",domID );
	NSDictionary *infoDictionary;
	infoDictionary = [[NSBundle mainBundle] infoDictionary];
	NSString* currentVersion = [infoDictionary objectForKey:@"CFBundleVersion"];

	@try
	{
		cliUpdate = [ifolderService CheckForMacUpdate:domID forCurrentVersion:currentVersion];
		if([cliUpdate Status] == UpgradeAvailable)
		{
			NSLog(@"Client Upgrade is Available");
			NSArray* localVersionArray = [currentVersion componentsSeparatedByString:@"."];
			NSArray* serverVersionArray = [[cliUpdate ServerVersion] componentsSeparatedByString:@"."];
			BOOL upgradeNeeded = NO;

			 //Check for Major version
			if ((  ([[localVersionArray objectAtIndex:0] intValue] == [[serverVersionArray objectAtIndex:0] intValue]) && ([[localVersionArray objectAtIndex:1] intValue] < [[serverVersionArray objectAtIndex:1] intValue])) 
						 || ([[localVersionArray objectAtIndex:0] intValue]  < [[serverVersionArray objectAtIndex:0] intValue]))
			{
				[cliUpdate setStatus:UpgradeAvailable];
			}      
			else
			{
				[cliUpdate setStatus:Latest];
			}

			if(([[localVersionArray objectAtIndex:0] intValue] == [[serverVersionArray objectAtIndex:0] intValue] ) && ([[localVersionArray objectAtIndex:1] intValue] == [[serverVersionArray objectAtIndex:1] intValue]))
			{
					if ((([[localVersionArray objectAtIndex:2] intValue] == [[serverVersionArray objectAtIndex:2] intValue]) && ([[localVersionArray objectAtIndex:3] intValue]  < [[serverVersionArray objectAtIndex:3] intValue]))
						|| ([[localVersionArray objectAtIndex:2] intValue] < [[serverVersionArray objectAtIndex:2] intValue]))
					{
							[cliUpdate setStatus:UpgradeAvailable];
					}
					else
					{
							[cliUpdate setStatus:Latest];
					}

			}
		}
	}
	@catch(NSException* ex)
	{
		NSLog(@"Exception : show  server old ");
		NSLog(@"Exception is name:%@ reason:%@",[ex name],[ex reason] );
		NSRunAlertPanel(NSLocalizedString(@"Old Server",@"ServerOldTitle"),
						NSLocalizedString(@"Server is old. Cannot connect to the server",@"ServerOldMessage"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		return;
	}
	switch([cliUpdate Status])
	{
		case  Latest:
			NSLog(@"Client status is latest");
			
			//Client is latest and not necessary to handle
			break;
			
		case UpgradeNeeded:
		
			 //Client upgrade is mandatory. So just logout the domain
			//Alert can be handled this way also
			/*NSBeginAlertSheet(NSLocalizedString(@"Client upgrade available.",@"UpgradeAvailableTitle"),
							  NSLocalizedString(@"OK",@"OK Button"),
							  NSLocalizedString(@"Cancel",@"Cancel"),
							  nil,
							  nil,
							  self,
							  @selector(runUpdates:returnCode:contextInfo:),
							  nil,
							  domID,
							  [NSString stringWithFormat:NSLocalizedString(@"Would you like to upgrade your client to %@. The client will be closed automatically if you click Yes",@"UpgradeNeededMessage"),[cliUpdate ServerVersion]]);
            */

			/*answer = NSRunAlertPanel(NSLocalizedString(@"Client upgrade available.",@"UpgradeAvailableTitle"),
									 [NSString stringWithFormat:NSLocalizedString(@"A new version of iFolder Mac client (%@) is available on server, Please download the latest version",@"UpgradeNeededMessage"),[cliUpdate ServerVersion]],
									 NSLocalizedString(@"OK",@"OK Button"),
									 nil,nil);
			*/
			//Actual message to display for upgrade available and handling it.

			answer = NSRunAlertPanel(NSLocalizedString(@"Client Upgrade Available",@"UpgradeAvailableTitle"),
									 [NSString stringWithFormat:NSLocalizedString(@"Would you like to upgrade your client to %@? If you click Yes, the latest DMG is downloaded to your system and client is closed automatically.\nYou must then click the downloaded DMG to proceed with the installation.",@"UpgradeAvailableTitle"),[cliUpdate ServerVersion]],
									 NSLocalizedString(@"Yes",@"Yes"),
									 NSLocalizedString(@"No",@"No"),nil);
			
			
			if(answer == NSAlertDefaultReturn)
			{
				NSOpenPanel *oPanel = [NSOpenPanel openPanel];
				
				[oPanel setAllowsMultipleSelection:NO];
				[oPanel setCanChooseDirectories:YES];
				[oPanel setCanChooseFiles:NO];
				[oPanel setTitle:NSLocalizedString(@"Browse the Location",@"Download latest client title")];
				
				result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
				
				if (result == NSOKButton)
				{
					dirName = [oPanel directory];
				}
				else
				{
					NSRunAlertPanel(NSLocalizedString(@"Upgrade Interrupted",@"Stop upgrade title"),
									NSLocalizedString(@"Cannot upgrade to the latest client",@"Stop upgrade message"),
									NSLocalizedString(@"OK",@"OK Button"),
									nil,nil);
					return;
				}
				
				@try
				{
					updateStatus = [ifolderService RunClientUpdate:domID withDownloadPath:dirName];
				}
				@catch(NSException *ex)
				{
					ifexconlog(@"iFolderData:clientUpdates", ex);		
					NSRunAlertPanel(NSLocalizedString(@"Upgrade Failed",@"Upgrade exception title"),
									NSLocalizedString(@"Error in upgrade. Upgrading to the latest client failed",@"Upgrade exception message"),
									NSLocalizedString(@"OK",@"OK Button"),
									nil,nil);
					return;
				}
				if(updateStatus)
				{
					//exit the application
					NSRunAlertPanel(NSLocalizedString(@"New Client is Available for Installing",@"Upgrade status title"),
									[NSString stringWithFormat:NSLocalizedString(@"Install the latest client available at the location %@",@"Upgrade status message"),dirName],
									NSLocalizedString(@"OK",@"OK Button"),nil,nil);
									
				//	forceQuit = YES;
					[[NSApplication sharedApplication] terminate:nil ];
				}
				
			}
			else
			{
				//FIXME: Leave Domain
				[simiasService LeaveDomain:domID withOption:NO];
			}
				
			break;
			
		case ServerOld:
			NSLog(@"case server old ");
			//Server is old and cannot continue, so terminate the application
			NSRunAlertPanel(NSLocalizedString(@"Old Server",@"ServerOldTitle"),
							NSLocalizedString(@"Server is old. Cannot connect to the server",@"ServerOldMessage"),
							NSLocalizedString(@"OK",@"OK Button"),nil,nil);

		//	forceQuit = YES;
			[[NSApplication sharedApplication] terminate:nil ];
			break;
			
		case UpgradeAvailable:
			//Latest upgrade available and can or cannot download. Still the current one runs
			/*Alert can be handled this way also
			NSBeginAlertSheet(NSLocalizedString(@"Client upgrade available.",@"UpgradeAvailableTitle"),
			NSLocalizedString(@"OK",@"OK Button"),
			NSLocalizedString(@"Cancel",@"Cancel"),
			nil,
			nil,
			self,
			@selector(runUpdates:returnCode:contextInfo:),
			nil,
			domID,
			[NSString stringWithFormat:NSLocalizedString(@"Would you like to upgrade your client to %@. The client will be closed automatically if you click Yes",@"UpgradeNeededMessage"),[cliUpdate ServerVersion]]);
           */			
			
			/*
			answer = NSRunAlertPanel(NSLocalizedString(@"Client upgrade available.",@"UpgradeAvailableTitle"),
									 [NSString stringWithFormat:NSLocalizedString(@"A new version of iFolder Mac client (%@) is available on server, Please download the latest version",@"UpgradeNeededMessage"),[cliUpdate ServerVersion]],
									 NSLocalizedString(@"OK",@"OK Button"),
									 nil,nil);
			*/
			answer = NSRunAlertPanel(NSLocalizedString(@"Client Upgrade Available",@"UpgradeAvailableTitle"),
									 [NSString stringWithFormat:NSLocalizedString(@"Would you like to upgrade your client to %@? If you click Yes, the latest DMG is downloaded to your system and client is closed automatically.\nYou must then click the downloaded DMG to proceed with the installation.",@"UpgradeAvailableTitle"),[cliUpdate ServerVersion]],
									 NSLocalizedString(@"Yes",@"Yes"),
									 NSLocalizedString(@"No",@"No"),nil);
			
			if(answer == NSAlertDefaultReturn)
			{
				NSOpenPanel *oPanel = [NSOpenPanel openPanel];
				
				[oPanel setAllowsMultipleSelection:NO];
				[oPanel setCanChooseDirectories:YES];
				[oPanel setCanChooseFiles:NO];
				[oPanel setTitle:NSLocalizedString(@"Browse the Location",@"Download latest client title")];
				
				result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
				
				if (result == NSOKButton)
				{
					dirName = [oPanel directory];
				}
				else
				{
					NSRunAlertPanel(NSLocalizedString(@"Upgrade Interrupted",@"Stop upgrade title"),
									NSLocalizedString(@"Cannot upgrade to the latest client",@"Stop upgrade message"),
									NSLocalizedString(@"OK",@"OK Button"),
									nil,nil);
					return;
				}
				
				@try
				{
					updateStatus = [ifolderService RunClientUpdate:domID withDownloadPath:dirName];
				}
				@catch(NSException *ex)
				{
					ifexconlog(@"iFolderData:runUpdates", ex);		
					NSRunAlertPanel(NSLocalizedString(@"Upgrade Failed",@"Upgrade exception title"),
									NSLocalizedString(@"Error in upgrade. Upgrading to the latest client failed",@"Upgrade exception message"),
									NSLocalizedString(@"OK",@"OK Button"),
									nil,nil);
					return;
				}
				if(updateStatus)
				{
					//exit the application
					NSString* iFolderUpdateDirectory = @"ead51d60-cd98-4d35-8c7c-b43a2ca949c8";
					NSRunAlertPanel(NSLocalizedString(@"New Client is Available for Installing",@"Upgrade status title"),
									[NSString stringWithFormat:NSLocalizedString(@"Install the latest client available at the location %@/%@",@"Upgrade status message"),dirName,iFolderUpdateDirectory],
									NSLocalizedString(@"OK",@"OK Button"),nil,nil);
					
				//	forceQuit = YES;
					[[NSApplication sharedApplication] terminate:nil ];
				}
				
			}

			break;
			
		case Unknown:
			//Force the application to close
		//	forceQuit = YES;
			[[NSApplication sharedApplication] terminate:nil ];
			break;
	}
}




/* This method is to handle NSBeginAlertSheet to handle the upgrade client
- (void)runUpdates:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	int result;
	NSString* dirName;
	BOOL updateStatus;
	
	//if(returnCode == NSAlertDefaultReturn)
	if(returnCode == NSAlertSecondButtonReturn)
	{
		NSString* domainID = (NSString*)contextInfo;
		
		NSOpenPanel *oPanel = [NSOpenPanel openPanel];
		
		[oPanel setAllowsMultipleSelection:NO];
		[oPanel setCanChooseDirectories:YES];
		[oPanel setCanChooseFiles:NO];
		
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
		
		if (result == NSOKButton)
		{
			dirName = [oPanel directory];
		}
		else
		{
			NSLog(@"Returning ............................");
			return;
		}
		
		@try
		{
			updateStatus = [ifolderService RunClientUpdate:domainID withDownloadPath:dirName];
		}
		@catch(NSException *ex)
		{
			ifexconlog(@"iFolderData:runUpdates", ex);		
		}
		if(updateStatus)
		{
			//exit the application
			forceQuit = YES;
			[[NSApplication sharedApplication] terminate:nil ];
		}
	}
	else
	{
		//check if upgrade needed, then logout of the client ....Simias->LeaveDomain
		switch([cliUpdate Status])
		{
				case UpgradeNeeded:	
					forceQuit = YES;
					[[NSApplication sharedApplication] terminate:nil ];
					break;
		}
	}
}
*/

-(NSNumber*)changeUserPassword:(NSString*)domainID changePassword:(NSString*)oldPasswd withNewPassword:(NSString*)newPasswd
{
	return [ifolderService ChangePassword:domainID changePassword:oldPasswd withNewPassword:newPasswd];
}

-(BOOL)checkFileName:(NSString*)name
{
	return [ifolderService CheckFileName:name];
}

-(void)setDomainPassword:(NSString*)domainID withPassword:(NSString*)newPasswd
{
	[simiasService SetDomainPassword:domainID password:newPasswd];
}
@end
