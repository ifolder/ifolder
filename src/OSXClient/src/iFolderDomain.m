//
//  Domain.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "iFolderDomain.h"
#import "iFolder.h"

@implementation iFolderDomain

-(id) init
{
	if(self = [super init])
	{
		NSArray *keys	= [NSArray arrayWithObjects: @"Name", nil];
		NSArray *values = [NSArray arrayWithObjects: @"New Domain", nil];
		
		properties = [[NSMutableDictionary alloc]
			initWithObjects:values forKeys:keys];
	}
	return self;
}




-(void) dealloc
{
	[properties release];
	
	[super dealloc];
}




- (NSMutableDictionary *) properties
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




-(void) setgSOAPProperties:(struct ns1__DomainWeb *)domainWeb
{
	NSMutableDictionary *newProperties = [[NSMutableDictionary alloc] init];
	
	if(domainWeb->ID != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->ID] forKey:@"ID"];
						
	if(domainWeb->POBoxID != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->POBoxID] forKey:@"POBoxID"];
	
	if(domainWeb->Name != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->Name] forKey:@"Name"];
		
	if(domainWeb->Description != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->Description] forKey:@"Description"];
	
	if(domainWeb->Host != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->Host] forKey:@"Host"];
	
	if(domainWeb->UserID != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->UserID] forKey:@"UserID"];

	if(domainWeb->UserName != nil)
		[newProperties setObject:[NSString stringWithCString:domainWeb->UserName] forKey:@"UserName"];

	[self setProperties:newProperties];
}



@end
