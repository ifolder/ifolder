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

#import "MemberSearchResults.h"
#include <simiasStub.h>
//#include <simias.nsmap>
#import "Simias.h"
#import "User.h"
#import "SimiasService.h"

#define PAGED_RESULT_COUNT	25


// methods defined in SimiasService.m
void init_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);
void cleanup_simias_gsoap(struct soap *pSoap, GSOAP_CREDS *creds);
void handle_simias_soap_error(struct soap *pSoap, GSOAP_CREDS *creds, NSString *methodName);
NSDictionary *getUserProperties(struct ns1__MemberInfo *member);

@implementation MemberSearchResults

- (id)init 
{
	[super init];
	simiasURL = [[NSString stringWithFormat:@"%@/simias10/Simias.asmx", [[Simias getInstance] simiasURL]] retain];
	results = NULL;
	domainID = nil;
	searchContext = nil;
	totalCount = 0;
    return self;
}
-(void)dealloc
{
//	NSLog(@"Dealloc was called on a MemberSearchResult");
	[self freePreviousSearch];
	[super dealloc];
}


-(void)freePreviousSearch
{
	if( (domainID != nil) && (searchContext != nil) )
	{
		// Call to release the soap structures
		struct soap soap;
		GSOAP_CREDS creds;		

		struct _ns1__FindCloseMembers			findMessage;
		struct _ns1__FindCloseMembersResponse	findResponse;

		findMessage.domainID = (char *)[domainID UTF8String];
		findMessage.searchContext =  (char *)[searchContext UTF8String];

		init_simias_gsoap (&soap, &creds);		
		soap_call___ns1__FindCloseMembers(
				&soap,
				[simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
				NULL,
				&findMessage,
				&findResponse);

		cleanup_simias_gsoap(&soap, &creds);

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



-(void)searchMembers:(NSString *)DomainID onAttribute:(NSString *)attribute usingValue:(NSString *)value
{
    struct soap soap;
	GSOAP_CREDS creds;	
    int err_code;

	struct _ns1__FindFirstSpecificMembers			findMessage;
	struct _ns1__FindFirstSpecificMembersResponse	findResponse;

	[self freePreviousSearch];
	
	domainID = [DomainID retain];
	
	findMessage.domainID = (char *)[domainID UTF8String];
	findMessage.attributeName = (char *)[attribute UTF8String];
	findMessage.searchString = (char *)[value UTF8String];
	findMessage.operation = ns1__SearchType__Begins;

	findMessage.count = PAGED_RESULT_COUNT;

	init_simias_gsoap (&soap, &creds);
    err_code = soap_call___ns1__FindFirstSpecificMembers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

	handle_simias_soap_error(&soap, &creds, @"MemberSearchResults.searchMembers");

	totalCount = findResponse.totalMembers;
	if(totalCount > 0)
	{
		searchContext = [[NSString stringWithUTF8String:findResponse.searchContext] retain];
		results = malloc(totalCount * sizeof(User *));
		memset(results, 0, (totalCount * sizeof(User *)));
		
		if(results != NULL)
		{
			int counter;
			for(counter=0;counter<findResponse.memberList->__sizeMemberInfo;counter++)
			{
				struct ns1__MemberInfo *curMember;
				curMember = findResponse.memberList->MemberInfo[counter];

				User *newUser = [ [User alloc] init];
				[newUser setProperties:getUserProperties(curMember)];
				results[counter] = [newUser retain];
			}
		}
	}

	cleanup_simias_gsoap(&soap, &creds);
}




-(void)getAllMembers:(NSString *)DomainID
{
    struct soap soap;
	GSOAP_CREDS creds;		
    int err_code;

	struct _ns1__FindFirstMembers			findMessage;
	struct _ns1__FindFirstMembersResponse	findResponse;

	[self freePreviousSearch];
	
	domainID = [DomainID retain];
	
	findMessage.domainID = (char *)[domainID UTF8String];
	findMessage.count = PAGED_RESULT_COUNT;

	init_simias_gsoap (&soap, &creds);
    err_code = soap_call___ns1__FindFirstMembers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

	handle_simias_soap_error(&soap, &creds, @"MemberSearchResults.getAllMembers");

	totalCount = findResponse.totalMembers;
	if(totalCount > 0)
	{
		searchContext = [[NSString stringWithUTF8String:findResponse.searchContext] retain];
		results = malloc(totalCount * sizeof(User *));
		memset(results, 0, (totalCount * sizeof(User *)));
		
		if(results != NULL)
		{
			int counter;
			for(counter=0;counter<findResponse.memberList->__sizeMemberInfo;counter++)
			{
				struct ns1__MemberInfo *curMember;
				curMember = findResponse.memberList->MemberInfo[counter];

				User *newUser = [ [User alloc] init];
				[newUser setProperties:getUserProperties(curMember)];
				results[counter] = [newUser retain];
			}
		}
	}

	cleanup_simias_gsoap(&soap, &creds);
}




-(int)count
{
	return totalCount;
}




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




-(BOOL)fillMembers:(int)index
{
    struct soap soap;
	GSOAP_CREDS creds;		
    int err_code;
	int actualIndex;

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

	init_simias_gsoap (&soap, &creds);
    err_code = soap_call___ns1__FindSeekMembers(
			&soap,
            [simiasURL UTF8String], //http://127.0.0.1:8086/simias10/Simias.asmx
            NULL,
            &findMessage,
            &findResponse);

 	if(soap.error)
	{
		NSLog(@"Error calling FindSeekMembers %d", soap.error);
		cleanup_simias_gsoap(&soap, &creds);
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

						User *newUser = [ [User alloc] init];
						[newUser setProperties:getUserProperties(curMember)];
						// If this happens a lot, we need to adjust our search to not
						// fetch results we already have!
						if(results[actualIndex] != nil)
						{
//							NSLog(@"Search results already contained this result!");
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

	cleanup_simias_gsoap(&soap, &creds);
	return YES;
}




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
	
	return newProperties;
}



@end
