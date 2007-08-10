/***********************************************************************
 |  $RCSfile$
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/


#import "SyncSize.h"


@implementation SyncSize




-(id) init
{
	if(self = [super init])
	{
		NSArray *keys	= [NSArray arrayWithObjects:	@"name", 
														nil];

		NSArray *values = [NSArray arrayWithObjects:	@"",
														nil];

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


-(unsigned long)SyncNodeCount
{
	NSNumber *num = [properties objectForKey:@"SyncNodeCount"];
	if(num != nil)
		return [num unsignedLongValue];
	else
		return 0;
}

-(unsigned long long) SyncByteCount
{
	NSNumber *num = [properties objectForKey:@"SyncByteCount"];
	if(num != nil)
		return [num unsignedLongLongValue];
	else
		return 0;
}



@end
