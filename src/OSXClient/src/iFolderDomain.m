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
