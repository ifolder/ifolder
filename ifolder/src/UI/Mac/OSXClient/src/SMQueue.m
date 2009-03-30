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

//===================================================================
// push
// Push to queue
//===================================================================
- (void) push: (id) object
{
    [queueArray addObject: object];
}

//===================================================================
// pop
// Pops from the queue
//===================================================================
- (id) pop
{
  id object;

  object = [[queueArray objectAtIndex: 0] retain];
  [queueArray removeObjectAtIndex: 0];

  return [object autorelease];
}

//===================================================================
// isEmpty
// Check whether queue is empty
//===================================================================
- (BOOL) isEmpty
{
  return [queueArray count] == 0;
}

//===================================================================
// count
// Get the count of objects in queue
//===================================================================
- (int) count
{
	return [queueArray count];
}

@end
