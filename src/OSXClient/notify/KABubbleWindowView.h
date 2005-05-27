/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright � 2003-2004, Timothy Hatcher
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
 *			 Karl�Adam�<karl@colloquy.info>
 * 
 ***********************************************************************/

@interface KABubbleWindowView : NSView
{
	NSImage *_icon;
	NSString *_title;
	NSAttributedString *_text;
	SEL _action;
	id _target;
}
- (void) setIcon:(NSImage *) icon;
- (void) setTitle:(NSString *) title;
- (void) setAttributedText:(NSAttributedString *) text;
- (void) setText:(NSString *) text;

- (id) target;
- (void) setTarget:(id) object;

- (SEL) action;
- (void) setAction:(SEL) selector;
@end