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

#import "SMQueue.h"

@implementation SMQueue

//===================================================================
// init
// Initialize the iFolderData
//===================================================================
- (id)init 
{
	queueArray = [[NSMutableArray alloc] init];
	return self;
}

- (void) push: (id) object
{
    [queueArray addObject: object];
}

- (id) pop
{
  id object;

  object = [[queueArray objectAtIndex: 0] retain];
  [queueArray removeObjectAtIndex: 0];

  return [object autorelease];
}

- (BOOL) isEmpty
{
  return [queueArray count] == 0;
}

- (int) count
{
	return [queueArray count];
}

@end
