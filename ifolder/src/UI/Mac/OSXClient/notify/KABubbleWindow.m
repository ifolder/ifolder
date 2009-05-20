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
*                 $Author: Timothy Hatcher <timothy@colloquy.info> Karl Adam <karl@colloquy.info>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
