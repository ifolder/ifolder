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
*                 $Author: <Creator>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "MCTableView.h"

@implementation MCTableView

//=========================================================================
// menuForEvent
// Get the menu if the row in table view is selected
//=========================================================================
- (NSMenu *)menuForEvent:(NSEvent *)theEvent;
{
    // what row are we at?
    int row = [self rowAtPoint: [self convertPoint: [theEvent
							locationInWindow] fromView: nil]];
    if (row != -1)
	{
		if([self isRowSelected:row] == NO)
			[self selectRow: row byExtendingSelection: NO];
    }

    return [super menu]; // use what we've got
}


@end
