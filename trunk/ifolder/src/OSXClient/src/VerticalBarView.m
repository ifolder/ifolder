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
