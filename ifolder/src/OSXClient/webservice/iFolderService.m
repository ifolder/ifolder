//
//  iFolderService.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "iFolderService.h"
#import "iFolderWSStubs.h"


@implementation iFolderService

-(NSMutableDictionary *) GetDomains
{
	NSMutableDictionary *domains = nil;
    GetDomains* _invocation = [[GetDomains alloc] init];    
    NSDictionary *result = [[_invocation resultValue] retain];
	
	if([_invocation isFault] == TRUE)
	{
		NSString *faultString = [result objectForKey:@"/FaultString"];
		[NSException raise:faultString format:faultString];
	}
	else
	{
		NSArray *resultArray = (NSArray *)[result objectForKey:@"GetDomainsResult"];

		unsigned count = [resultArray count];
		
		domains = [[NSMutableDictionary alloc] initWithCapacity:count];

		unsigned i;
		
		for(i=0; i < count; i++)
		{
			Domain *domain = [[Domain alloc] init];
		
			NSDictionary *domainDict = [resultArray objectAtIndex:i];
			[domain readDictionary:domainDict];
			
			[domains setObject:domain forKey:domain->ID];
		}
	}
	
    [_invocation release];
    return domains;
}


-(Domain *)ConnectToDomain:(NSString*) in_UserName in_Password:(NSString*) in_Password in_Host:(NSString*) in_Host
{
    Domain *domain = NULL;
    ConnectToDomain* _invocation = [[ConnectToDomain alloc] init];
    [_invocation setParameters: in_UserName in_Password:in_Password in_Host:in_Host];
    NSDictionary *result = [[_invocation resultValue] retain];

	if([_invocation isFault] == TRUE)
	{
		NSString *faultString = [result objectForKey:@"/FaultString"];
		[NSException raise:faultString format:faultString];
		domain = NULL;
	}
	else
	{
		domain = [[Domain alloc] init];
		NSDictionary *resultDict = (NSDictionary *)[result objectForKey:@"ConnectToDomainResult"];
		[domain readDictionary:resultDict];
	}

	[_invocation release];
    return domain;
}


@end
