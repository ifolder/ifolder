//
//  Domain.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "Domain.h"


@implementation Domain

-(void) readDictionary:(NSDictionary *)dictionary
{
	NSString *value = [dictionary objectForKey:@"ID"];
	if(value != nil)
		self->ID = value;

	value = [dictionary objectForKey:@"POBoxID"];
	if(value != nil)
		self->POBoxID = value;
		
	value = [dictionary objectForKey:@"Name"];
	if(value != nil)
		self->Name = value;

	value = [dictionary objectForKey:@"Description"];
	if(value != nil)
		self->Description = value;
		
	value = [dictionary objectForKey:@"Host"];
	if(value != nil)
		self->Host = value;

	value = [dictionary objectForKey:@"UserID"];
	if(value != nil)
		self->UserID = value;

	value = [dictionary objectForKey:@"UserName"];
	if(value != nil)
		self->UserName = value;
}

@end
