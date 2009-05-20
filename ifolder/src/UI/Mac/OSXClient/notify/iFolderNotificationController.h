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
*                 $Author: Timothy Hatcher <timothy@colloquy.info> Karl?Adam?<karl@colloquy.info>
*                 $Modified by: Satyam <ssutapalli@novell.com>  01-01-2008      Added notification for sync fail
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
+ (void) newUserNotification:(iFolder*)ifolder;
+ (void) collisionNotification:(iFolder *)ifolder;
+ (void) readOnlyNotification:(NSString*)ifolderAndFileName;
+ (void) iFolderFullNotification:(NSString*)ifolderAndFileName;
//+ (void) syncFailNotification:(NSString*)ifolderAndFileName;

//+ (void) policyNotification:(iFolder*)ifolder;
+ (void) accessNotification:(NSString*)ifolderAndFileName;
+ (void) lockedNotification:(NSString*)ifolderAndFileName;
+ (void) policySizeNotification:(NSString*)ifolderAndFileName;
+ (void) policyTypeNotification:(NSString*)ifolderAndFileName;
+ (void) diskFullNotification:(NSString*)ifolderAndFileName;


- (void) ifolderNotify:(iFolder *)ifolder;
- (void) userNotify:(iFolder*)ifolder;
- (void) colNotify:(iFolder *)ifolder;
- (void) readOnlyNotify:(NSString*)ifolderAndFileName;
- (void) iFolderFullNotify:(NSString*)ifolderAndFileName;
//- (void) syncFailNotify:(NSString*)ifolderAndFileName;

//- (void) policyNotify:(iFolder*)ifolder;
- (void) accessNotify:(NSString*)ifolderAndFileName;
- (void) lockedNotify:(NSString*)ifolderAndFileName;
- (void) policySizeNotify:(NSString*)ifolderAndFileName;
- (void) policyTypeNotify:(NSString*)ifolderAndFileName;
- (void) diskFullNotify:(NSString*)ifolderAndFileName;

@end
	
