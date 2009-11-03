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

#import "MemberSearchResults.h"
#include <simiasStub.h>
//#include <simias.nsmap>
#import "Simias.h"
#import "User.h"
#import "SimiasService.h"

#define PAGED_RESULT_COUNT	25

// methods defined in SimiasService.m
void handle_simias_soap_error(void *soapData, NSString *methodName);
struct soap *lockSimiasSoap(void *soapData);
void unlockSimiasSoap(void *soapData);

NSDictionary *getUserProperties(struct ns1__MemberInfo *member);

@implementation MemberSearchResults

//=========================================================================
// initWithSoapData
// Initialize the soap details
//=========================================================================
- (id)initWithSoapData:(void *)soapdata
{
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/Simias.asmx", [[Simias getInstance] simiasURL]] retain];
	results = NULL;
	domainID = nil;
	searchContext = nil;
	totalCount = 0;
	soapData = soapdata;
    return self;
}

//=========================================================================
// dealloc
// Deallocate the resources that are already allocated
//=========================================================================
-(void)dealloc
{
//	NSLog(@"Dealloc was called on a MemberSearchResult");
	[self freePreviousSearch];
	[super dealloc];
}

//=========================================================================
// freePreviousSearch
// Release the previous search details
//=========================================================================
-(void)freePreviousSearch
{
	if( (domainID != nil) && (searchContext != nil) )
	{
		// Call to release the soap structures
		struct soap *pSoap = lockSimiasSoap(soapData);

		struct _ns1__FindCloseMembers			findMessage;
		struct _ns1__FindCloseMembersResponse	findResponse;

		findMessage.domainID = (char *)[domainID UTF8String];
		findMessage.searchContext =  (char *)[searchContext UTF8String];

		soap_call___ns1__FindCloseMembers(
				pSoap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&findMessage,
				&findResponse);

		unlockSimiasSoap(soapData);

		[domainID release];
		[simiasURL release];
		[searchContext release];
	}
	if(results != NULL)
	{
		int counter;
		for(counter=0; counter < totalCount; counter++)
		{
			if(results[counter] != nil)
			{
				[results[counter] release];
				results[counter] = nil;
			}
		}
		free(results);
	}
}


//=========================================================================
// searchMembers
// Search for domain ID from the list of members
//=========================================================================
-(void)searchMembers:(NSString *)DomainID onAttribute:(NSString *)attribute usingValue:(NSString *)value
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);	

	struct _ns1__FindFirstSpecificMembers			findMessage;
	struct _ns1__FindFirstSpecificMembersResponse	findResponse;

	[self freePreviousSearch];
	
	domainID = [DomainID retain];
	
	findMessage.domainID = (char *)[domainID UTF8String];
	findMessage.attributeName = (char *)[attribute UTF8String];
	findMessage.searchString = (char *)[value UTF8String];
	findMessage.operation = ns1__SearchType__Begins;

	findMessage.count = PAGED_RESULT_COUNT;

    err_code = soap_call___ns1__FindFirstSpecificMembers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

	handle_simias_soap_error(soapData, @"MemberSearchResults.searchMembers");

	totalCount = findResponse.totalMembers;
	if(totalCount > 0)
	{
		searchContext = [[NSString stringWithUTF8String:findResponse.searchContext] retain];
		results = malloc(totalCount * sizeof(User *));
		memset(results, 0, (totalCount * sizeof(User *)));
		
		if(results != NULL)
		{
			int counter;
			int objCounter = 0;
			for(counter=0;counter<findResponse.memberList->__sizeMemberInfo;counter++)
			{
				struct ns1__MemberInfo *curMember;
				curMember = findResponse.memberList->MemberInfo[counter];

				if(curMember->IsHost == FALSE )
				{
					User *newUser = [ [User alloc] init];
					[newUser setProperties:getUserProperties(curMember)];
					results[objCounter++] = [newUser retain];
				}
			}
		}
	}

	unlockSimiasSoap(soapData);
}



//=========================================================================
// getAllMembers
// Get the list of all the members
//=========================================================================
-(void)getAllMembers:(NSString *)DomainID
{
    int err_code;
	struct soap *pSoap = lockSimiasSoap(soapData);
	
	struct _ns1__FindFirstMembers			findMessage;
	struct _ns1__FindFirstMembersResponse	findResponse;

	[self freePreviousSearch];
	
	domainID = [DomainID retain];
	
	findMessage.domainID = (char *)[domainID UTF8String];
	findMessage.count = PAGED_RESULT_COUNT;

    err_code = soap_call___ns1__FindFirstMembers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

	handle_simias_soap_error(soapData, @"MemberSearchResults.getAllMembers");

	totalCount = findResponse.totalMembers;
	
	if(totalCount > 0)
	{
		searchContext = [[NSString stringWithUTF8String:findResponse.searchContext] retain];
		results = malloc(totalCount * sizeof(User *));
		memset(results, 0, (totalCount * sizeof(User *)));
		
		if(results != NULL)
		{
			int counter;
			int objCounter = 0;
			for(counter=0;counter<findResponse.memberList->__sizeMemberInfo;counter++)
			{
				struct ns1__MemberInfo *curMember;
				curMember = findResponse.memberList->MemberInfo[counter];
				if(curMember->IsHost == FALSE )
				{
					User *newUser = [ [User alloc] init];
					[newUser setProperties:getUserProperties(curMember)];
					results[objCounter++] = [newUser retain];
				}
			}
		}
	}

	unlockSimiasSoap(soapData);
}



//=========================================================================
// count
// Get the total count of members
//=========================================================================
-(int)count
{
	return totalCount;
}



//=========================================================================
// objectAtIndex
// Get the object at selected row
//=========================================================================
-(User *)objectAtIndex:(int)index
{
	User *user = nil;
	if( (index < totalCount) && (results != NULL) )
	{
		if(results[index] != nil)
			user = results[index];
		else if([self fillMembers:index])
		{
			if(results[index] != nil)
				user = results[index];
		}
	}

	if(user == nil)
		NSLog(@"Returning a nil user, prepare to die!");
		
	return user;
}



//=========================================================================
// fillMembers
// Fill the members list
//=========================================================================
-(BOOL)fillMembers:(int)index
{
    int err_code;
	int actualIndex;
	struct soap *pSoap = lockSimiasSoap(soapData);

	struct _ns1__FindSeekMembers			findMessage;
	struct _ns1__FindSeekMembersResponse	findResponse;

//	NSLog(@"fillMembers called to fill in more members");

	findMessage.domainID = (char *)[domainID UTF8String];
	findMessage.searchContext = (char *)[searchContext UTF8String];

	// Fix up index so we get the full page of results that includes index
	if(index > (totalCount - PAGED_RESULT_COUNT) )
	{
		if(( totalCount - PAGED_RESULT_COUNT) < 0)
			actualIndex = 0;
		else
			actualIndex = (totalCount - PAGED_RESULT_COUNT);
	}
	else
		actualIndex = index;

	findMessage.offset = actualIndex;
	findMessage.count = PAGED_RESULT_COUNT;

    err_code = soap_call___ns1__FindSeekMembers(
			pSoap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

 	if(pSoap->error)
	{
		NSLog(@"Error calling FindSeekMembers %d", pSoap->error);
		unlockSimiasSoap(soapData);
		return NO;
	}
	else
	{
		if(totalCount > 0)
		{
			[searchContext release];
			searchContext = [[NSString stringWithUTF8String:findResponse.searchContext] retain];
			if(results != NULL)
			{
				int counter;
				int resultIndex = actualIndex;
				
				for(counter=0;counter<findResponse.memberList->__sizeMemberInfo;counter++)
				{
					// This is our safety belt just in case the search returns more
					// that what it said it was going to return
					if(actualIndex < totalCount)
					{
						struct ns1__MemberInfo *curMember;
						curMember = findResponse.memberList->MemberInfo[counter];

						if(curMember->IsHost == FALSE )
						{
							User *newUser = [ [User alloc] init];
							[newUser setProperties:getUserProperties(curMember)];
						// If this happens a lot, we need to adjust our search to not
						// fetch results we already have!
							if(results[actualIndex] != nil)
							{
//								NSLog(@"Search results already contained this result!");
								[results[actualIndex] release];
								results[actualIndex] = nil;
							}

							results[actualIndex] = [newUser retain];
							actualIndex++;
						}
					}
				}
			}
		}
    }

	unlockSimiasSoap(soapData);
	return YES;
}



//=========================================================================
// getUserProperties
// Get the properties of User
//=========================================================================
NSDictionary *getUserProperties(struct ns1__MemberInfo *member)
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];

	if(member->ObjectID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:member->ObjectID] forKey:@"ID"];
	if(member->UserID != nil)
		[newProperties setObject:[NSString stringWithUTF8String:member->UserID] forKey:@"UserID"];
	if(member->Name != nil)
		[newProperties setObject:[NSString stringWithUTF8String:member->Name] forKey:@"Name"];
	if(member->GivenName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:member->GivenName] forKey:@"FirstName"];
	if(member->FamilyName != nil)
		[newProperties setObject:[NSString stringWithUTF8String:member->FamilyName] forKey:@"Surname"];
	if( (member->FullName != nil) && (strlen(member->FullName) > 0) )
		[newProperties setObject:[NSString stringWithUTF8String:member->FullName] forKey:@"FN"];
	else
		[newProperties setObject:[NSString stringWithUTF8String:member->Name] forKey:@"FN"];

	[newProperties setObject:[NSNumber numberWithBool:member->IsOwner] forKey:@"IsOwner"];
	[newProperties setObject:[NSNumber numberWithBool:member->IsHost] forKey:@"IsHost"];
	
	return newProperties;
}



@end
