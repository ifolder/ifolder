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
