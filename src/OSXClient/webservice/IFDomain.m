//
//  Domain.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "IFDomain.h"

@implementation IFDomain

-(void) from_gsoap:(struct ns1__DomainWeb *)domainWeb
{
	if(domainWeb->ID != nil)
		self->ID = [NSString stringWithCString:domainWeb->ID];

	if(domainWeb->POBoxID != nil)
		self->POBoxID = [NSString stringWithCString:domainWeb->POBoxID];
	
	if(domainWeb->Name != nil)
		self->Name = [NSString stringWithCString:domainWeb->Name];
		
	if(domainWeb->Description != nil)
		self->Description = [NSString stringWithCString:domainWeb->Description];
	
	if(domainWeb->Host != nil)
		self->Host = [NSString stringWithCString:domainWeb->Host];
	
	if(domainWeb->UserID != nil)
		self->UserID = [NSString stringWithCString:domainWeb->UserID];

	if(domainWeb->UserName != nil)
		self->UserName = [NSString stringWithCString:domainWeb->UserName];
}

@end
