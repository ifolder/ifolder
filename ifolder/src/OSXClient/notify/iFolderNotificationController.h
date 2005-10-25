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
 *  based on the JVNotificationController
 *		Authors: Timothy Hatcher <timothy@colloquy.info>
 *				 Karl Adam <karl@colloquy.info>
 * 
 *	Modifications for iFolder: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

@class iFolder;

@interface iFolderNotificationController : NSObject
{
	NSMutableDictionary *_bubbles;
	BOOL _growlInstalled;

	NSMutableDictionary		*notifyContext;
	NSSound					*notifySound;	
}
+ (iFolderNotificationController *) defaultManager;
- (void) performNotification:(NSDictionary *) context;

+ (void) newiFolderNotification:(iFolder *)ifolder;
+ (void) newUserNotification:(iFolder *)ifolder;
+ (void) collisionNotification:(iFolder *)ifolder;

- (void) ifolderNotify:(iFolder *)ifolder;
- (void) userNotify:(iFolder *)ifolder;
- (void) colNotify:(iFolder *)ifolder;

@end
	