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
			
			keyedDomains = [[NSMutableDictionary alloc] init];
			keyediFolders = [[NSMutableDictionary alloc] init];
			keyedSubscriptions = [[NSMutableDictionary alloc] init];
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
// refresh
// reads the current domains and ifolders
//===================================================================
-(void)refresh
{
	[instanceLock lock];

	NSLog(@"Refreshing iFolderData");

	@try
	{
		[keyedDomains removeAllObjects];
		int objCount;

		NSArray *newDomains = [simiasService GetDomains:NO];
		for(objCount = 0; objCount < [newDomains count]; objCount++)
		{
			iFolderDomain *newDomain = [newDomains objectAtIndex:objCount];
			
			if( [[newDomain isDefault] boolValue] )
				defaultDomain = newDomain;

			[keyedDomains setObject:newDomain forKey:[newDomain ID] ];			
		}

		
		[keyediFolders removeAllObjects];
		[keyedSubscriptions removeAllObjects];
		NSArray *newiFolders = [ifolderService GetiFolders];
		for(objCount = 0; objCount < [newiFolders count]; objCount++)
		{
			iFolder *newiFolder = [newiFolders objectAtIndex:objCount];
			
			if([newiFolder IsSubscription])
			{
				[keyediFolders setObject:newiFolder forKey:[newiFolder CollectionID]];
				[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
			}
			else
				[keyediFolders setObject:newiFolder forKey:[newiFolder ID] ];			
		}
	}
	@catch (NSException *e)
	{
		NSLog(@"*********Exception refreshing iFolderData");
		NSLog(@"%@ :: %@", [e name], [e reason]);
	}

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
// returns the iFolder for the specified iFolderID
//===================================================================
-(iFolder *)getiFolder:(NSString *)iFolderID updateData:(BOOL)shouldUpdate
{
	iFolder *ifolder = nil;
	[instanceLock lock];
	NSLog(@"iFolderData getiFolder called for iFolder %@", iFolderID);

	ifolder = [[keyediFolders objectForKey:iFolderID] retain];
	if( (ifolder == nil) || (shouldUpdate) )
	{
		@try
		{
			iFolder *newiFolder = [[ifolderService GetiFolder:iFolderID] retain];
			if(ifolder != nil)
			{
				[ifolder setProperties:[newiFolder properties]];
			}
			else
			{
				ifolder = [newiFolder retain];

				if([newiFolder IsSubscription])
				{
					[keyediFolders setObject:newiFolder forKey:[newiFolder CollectionID] ];
					[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
				}
				else
					[keyediFolders setObject:newiFolder forKey:[newiFolder ID] ];
			}

			[newiFolder release];
		}
		@catch (NSException *e)
		{
			NSLog(@"*********Exception getting iFolder");
			NSLog(@"%@ :: %@", [e name], [e reason]);
		}
	}

	[instanceLock unlock];

	return [ifolder autorelease];
}




//===================================================================
// removeiFolder
// removes an iFolder that was previously here
//===================================================================
-(void)removeiFolder:(NSString *)iFolderID
{
	[instanceLock lock];
	NSString *realID = iFolderID;
	
	iFolder *ifolder = [keyediFolders objectForKey:realID];
	if(ifolder == nil)
	{
		realID = [keyedSubscriptions objectForKey:iFolderID];
		if(realID != nil)
		{
			ifolder = [keyediFolders objectForKey:realID];
			// remove the subscription because we are about to
			// remove the iFolder
			[keyedSubscriptions removeObjectForKey:iFolderID];
		}
	}
	if(ifolder != nil)
	{
		[keyediFolders removeObjectForKey:realID];
	}

	[instanceLock unlock];
}




//===================================================================
// addiFolder
// adds an iFolder
//===================================================================
-(iFolder *)createiFolder:(NSString *)path inDomain:(NSString *)domainID
{
	iFolder *newiFolder = nil;

	[instanceLock lock];

	@try
	{
		iFolder *newiFolder = [[ifolderService CreateiFolder:path InDomain:domainID] retain];
		[keyediFolders setObject:newiFolder forKey:[newiFolder ID] ];
	}
	@catch(NSException *ex)
	{
		[instanceLock unlock];
		[ex raise];
	}
	
	[instanceLock unlock];

	return newiFolder;
}




//===================================================================
// deleteiFolder
// deletes the specified iFolder
//===================================================================
-(void)deleteiFolder:(NSString *)ifolderID
{	
	[instanceLock lock];

	@try
	{
		[ifolderService DeleteiFolder:ifolderID];
		[self removeiFolder:ifolderID];
	}
	@catch(NSException *ex)
	{
		[instanceLock unlock];
		[ex raise];
	}
	
	[instanceLock unlock];
}



//===================================================================
// deleteiFolder
// deletes the specified iFolder
//===================================================================
- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath
{
	[instanceLock lock];
	
	NSString *localID = [keyedSubscriptions objectForKey:iFolderID];
	NSAssert( (localID != nil), @"iFolderID not found for subscription");
	
	@try
	{
		iFolder *newiFolder = [ifolderService AcceptiFolderInvitation:iFolderID InDomain:domainID toPath:localPath];
		if([[newiFolder ID] compare:iFolderID] != 0)
		{
			[keyedSubscriptions removeObjectForKey:iFolderID];
			if([newiFolder IsSubscription])
				[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
		}

		iFolder *curiFolder = [keyediFolders objectForKey:localID];
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
// deleteiFolder
// deletes the specified iFolder
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
// getAvailableiFolder
// returns the iFolder (invitation) for the specified ID
//===================================================================
-(iFolder *)getAvailableiFolder:(NSString *)iFolderID 
									inCollection:(NSString *)collectionID
									updateData:(BOOL)shouldUpdate
{
	iFolder *ifolder = nil;
	[instanceLock lock];

	ifolder = [[keyediFolders objectForKey:iFolderID] retain];
	if( (ifolder == nil) || (shouldUpdate) )
	{
		@try
		{
			iFolder *newiFolder = [[ifolderService GetAvailableiFolder:iFolderID
													inCollection:collectionID] retain];
			if(ifolder != nil)
				[ifolder setProperties:[newiFolder properties]];
			else
			{
				ifolder = [newiFolder retain];

				if([newiFolder IsSubscription])
				{
					[keyediFolders setObject:newiFolder forKey:[newiFolder CollectionID] ];
					[keyedSubscriptions setObject:[newiFolder CollectionID] forKey:[newiFolder ID]];
				}
				else
					[keyediFolders setObject:newiFolder forKey:[newiFolder ID] ];
			}

			[newiFolder release];
		}
		@catch (NSException *e)
		{
			NSLog(@"*********Exception getting iFolder");
			NSLog(@"%@ :: %@", [e name], [e reason]);
		}
	}

	[instanceLock unlock];

	return [ifolder autorelease];
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
// getDomains
// Returns an array of all current Domains
//===================================================================
-(NSArray *)getDomains
{
	NSArray *domains;
	[instanceLock lock];
	domains = [keyedDomains allValues];
	[instanceLock unlock];	
	return domains;
}




//===================================================================
// getiFolders
// Returns an array of all current iFolders
//===================================================================
-(NSArray *)getiFolders
{
	NSArray *ifolders;
	[instanceLock lock];
	ifolders = [keyediFolders allValues];
	[instanceLock unlock];	
	return ifolders;
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


@end
