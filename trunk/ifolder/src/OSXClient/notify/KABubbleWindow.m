/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright © 2003-2004, Timothy Hatcher
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
 *  Authors: Timothy Hatcher <timothy@colloquy.info>
 *			 Karl Adam <karl@colloquy.info>
 * 
 ***********************************************************************/
 
#import "KABubbleWindow.h"

@implementation KABubbleWindow

- (id)initWithContentRect:(NSRect)contentRect
				styleMask:(unsigned int)aStyle
				  backing:(NSBackingStoreType)bufferingType
					defer:(BOOL)flag
{

	//use NSWindow to draw for us
	NSWindow* result = [super initWithContentRect:contentRect 
										styleMask:NSBorderlessWindowMask 
										  backing:NSBackingStoreBuffered 
											defer:NO];
	
	//set up our window
	[result setBackgroundColor: [NSColor clearColor]];
	[result setLevel: NSStatusWindowLevel];
	[result setAlphaValue:0.15];
	[result setOpaque:NO];
	[result setHasShadow: YES];
	[result setCanHide:NO ];
	
	return result;
}

@end
