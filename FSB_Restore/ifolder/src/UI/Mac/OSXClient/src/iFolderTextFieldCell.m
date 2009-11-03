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
*                 $Author: Satyam <ssutapalli@novell.com> 
*-----------------------------------------------------------------------------
* This module is used to:
*              From IB, we can align the text horizontally only, but inorder to
* align it vertically when icon size in first colomn grows bigger this class is
* needed.
*              To use this class: Assign the instance of this class to data text
* cell of the table coloumn
*
*******************************************************************************/

#import "iFolderTextFieldCell.h"


@implementation iFolderTextFieldCell

//=====================================================================
// drawingRectForBounds
// Setting the appearance of new row in the table view
//=====================================================================
- (NSRect)drawingRectForBounds:(NSRect)theRect
{
	NSRect newRect = [super drawingRectForBounds:theRect]; //Get the parent's rect
	
	if (isEditOrSelect == NO)  //Forget about alignment when editing or selecting
	{
		NSSize textSize = [self cellSizeForBounds:theRect];  //Get the text size from the cell
		
		// Center the text vertically
		float remainingHeight = newRect.size.height - textSize.height;	
		if (remainingHeight > 0)
		{
			newRect.size.height -= remainingHeight;
			newRect.origin.y += (remainingHeight / 2);
		}
	}
	
	return newRect;
}

//=====================================================================
// selectWithFrame
// This method will be called whenever cell is selected
//=====================================================================
- (void)selectWithFrame:(NSRect)aRect inView:(NSView *)controlView editor:(NSText *)textObj delegate:(id)anObject start:(int)selStart length:(int)selLength
{
	aRect = [self drawingRectForBounds:aRect];
	isEditOrSelect = YES;	
	[super selectWithFrame:aRect inView:controlView editor:textObj delegate:anObject start:selStart length:selLength];
	isEditOrSelect = NO;
}

//=====================================================================
// editWithFrame
// Method that will be called when a cell is being edited
//=====================================================================
- (void)editWithFrame:(NSRect)aRect inView:(NSView *)controlView editor:(NSText *)textObj delegate:(id)anObject event:(NSEvent *)theEvent
{	
	aRect = [self drawingRectForBounds:aRect];
	isEditOrSelect = YES;
	[super editWithFrame:aRect inView:controlView editor:textObj delegate:anObject event:theEvent];
	isEditOrSelect = NO;
}


@end
