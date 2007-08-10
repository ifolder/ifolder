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
 |  based on the JVNotificationController
 |		Authors: Timothy Hatcher <timothy@colloquy.info>
 |				 Karl Adam <karl@colloquy.info>
 | 
 |	Modifications for iFolder: Calvin Gaisford <cgaisford@novell.com>
 |
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
+ (void) readOnlyNotification:(iFolder *)ifolder;
+ (void) iFolderFullNotification:(iFolder *)ifolder;

- (void) ifolderNotify:(iFolder *)ifolder;
- (void) userNotify:(iFolder *)ifolder;
- (void) colNotify:(iFolder *)ifolder;
- (void) readOnlyNotify:(iFolder *)ifolder;
- (void) iFolderFullNotify:(iFolder *)ifolder;

@end
	