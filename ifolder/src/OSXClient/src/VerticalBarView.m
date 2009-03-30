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

#import "VerticalBarView.h"

@implementation VerticalBarView

- (id)initWithFrame:(NSRect)frameRect
{
	if ((self = [super initWithFrame:frameRect]) != nil)
	{
		divNum = 1;
	}
	return self;
}




-(void)setBarSize:(long long)topValue withFill:(long long)fillValue;
{
	if( (topValue == 0) || (fillValue == 0) )
		divNum = 0;
	else
	{
		divNum = (float)fillValue / (float)topValue;
		if(divNum >= 1)
			divNum = 1;
		if(divNum < 0)
			divNum = 0;
	}
	[self setNeedsDisplay:YES];
}




- (void)drawRect:(NSRect)rect
{

//	if(divNum == 0)
//	{
//		drawRect = rect;
//		[[NSColor windowBackgroundColor] set];
//	}
//	else
	if(divNum != 0)
	{
		NSRect drawRect;

		drawRect.size.height = rect.size.height * divNum;
		drawRect.size.width = rect.size.width;
		drawRect.origin.x = rect.origin.x;
		drawRect.origin.y = rect.origin.y;

		[[NSColor darkGrayColor] set];

		NSRectClip(drawRect);
		NSRectFill(drawRect);
	}

    return;
}

@end
