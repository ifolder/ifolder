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

#import "iFolderNotificationController.h"
#import "KABubbleWindowController.h"
#import "KABubbleWindowView.h"
#import "iFolder.h"
#import "iFolderApplication.h"

static iFolderNotificationController *sharedInstance = nil;

@interface iFolderNotificationController (iFolderNotificationControllerPrivate)
- (void) _bounceIconOnce;
- (void) _bounceIconContinuously;
- (void) _showBubble:(NSDictionary *) context;
- (void) _playSound:(NSString *) path;
@end



@implementation iFolderNotificationController

//======================================================================
// defaultManager
// This method will create a shared instance of self and returns it.
//======================================================================
+ (iFolderNotificationController *) defaultManager
{
	extern iFolderNotificationController *sharedInstance;
	return ( sharedInstance ? sharedInstance : ( sharedInstance = [[self alloc] init] ) );
}

//=================
// init
// Constructor
//=================
- (id) init
{
	if( ( self = [super init] ) )
	{
		_bubbles = [[NSMutableDictionary dictionary] retain];
		notifyContext = [[[NSMutableDictionary alloc] init] retain];
		[notifyContext setObject:[NSImage imageNamed:@"newifolder32"] forKey:@"image"];		
	}

	return self;
}

//==============
// dealloc
// Destructor
//==============
- (void) dealloc
{
	extern iFolderNotificationController *sharedInstance;

	[_bubbles release];
	[notifyContext release];

	[[NSNotificationCenter defaultCenter] removeObserver:self];
	if( self == sharedInstance ) sharedInstance = nil;

	_bubbles = nil;
	notifyContext = nil;

	if(notifySound != nil)
	{
		if( [notifySound isPlaying] )
			[notifySound stop];
		
		[notifySound autorelease];
	}
	
	[super dealloc];
}

//============================================================================================================
// newiFolderNotification
// This method will create a thread that shows a notification when u are invited for sharing iFolder
//============================================================================================================
+ (void) newiFolderNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(ifolderNotify:) 
				withObject:ifolder waitUntilDone:YES ];	
}


//============================================================================================
// newiFolderNotification
// This method will create a thread that shows a notification when new users joined iFolder
//============================================================================================
+ (void) newUserNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(userNotify:) 
				withObject:ifolder waitUntilDone:YES ];
}

//==============================================================================================
// collisionNotification
// This method will create a thread that shows a notification when conflict occurs in iFolder
//==============================================================================================
+ (void) collisionNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(colNotify:) 
				withObject:ifolder waitUntilDone:YES ];
}

//==========================================================================================================
// readOnlyNotification
// This method will create a thread that shows a notification when the iFolder going to sync is read-only
//==========================================================================================================
+ (void) readOnlyNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(readOnlyNotify:) 
				withObject:ifolderAndFileName waitUntilDone:YES ];
}

//=========================================================================================================
// iFolderFullNotification
// This method will create a thread that shows a notification when the quota is full for further syncing
//=========================================================================================================
+ (void) iFolderFullNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(iFolderFullNotify:) 
				withObject:ifolderAndFileName waitUntilDone:YES ];
}

//============================================================================================================
// syncFailNotification
// This method will create a thread that shows a notification when sync fails due to exclude file policy.
//============================================================================================================
//+ (void) syncFailNotification:(NSString*)ifolderAndFileName
//{
//	[[self defaultManager] performSelectorOnMainThread:@selector(syncFailNotify:) 
//				withObject:ifolder waitUntilDone:YES ];
//}

/*
+ (void) policyNotification:(iFolder*)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(policyNotify:) 
											withObject:ifolder waitUntilDone:YES ];	
}
*/

+ (void) accessNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(accessNotify:) 
											withObject:ifolderAndFileName waitUntilDone:YES ];		
}

+ (void) lockedNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(lockedNotify:) 
											withObject:ifolderAndFileName waitUntilDone:YES ];	
}

+ (void) policySizeNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(policySizeNotify:) 
											withObject:ifolderAndFileName waitUntilDone:YES ];	

}

+ (void) policyTypeNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(policyTypeNotify:) 
											withObject:ifolderAndFileName waitUntilDone:YES ];	
}

+ (void) diskFullNotification:(NSString*)ifolderAndFileName
{
	[[self defaultManager] performSelectorOnMainThread:@selector(diskFullNotify:) 
											withObject:ifolderAndFileName waitUntilDone:YES ];		
}

+ (void)ioErrorNotification:(NSString*)errorMessage
{
	[[self defaultManager] performSelectorOnMainThread:@selector(ioErrorNotify:) 
						withObject:errorMessage waitUntilDone:YES ];		
}

//========================================================================================
// ifolderNotify
// This method will show a notification inviting you to participate in sharing iFolder
//========================================================================================
- (void) ifolderNotify:(iFolder *)ifolder
{
	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_NOTIFYIFOLDERS])
	{
		[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"New iFolder \"%@\"", @"Title in notification window"), [ifolder Name]]
										forKey:@"title"];
		[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"%@ has invited you to participate in this shared iFolder", @"Message in notification window"), [ifolder OwnerName]]
										forKey:@"description"];
		[self performNotification:notifyContext];
	}
}

//========================================================================================
// userNotify
// This method will show a notification when new users have joined the iFolder
//========================================================================================
- (void) userNotify:(iFolder *)ifolder
{
	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_NOTIFYUSER])
	{
		[notifyContext setObject:NSLocalizedString(@"New iFolder Users", @"Title for notification window") forKey:@"title"];
		[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"New users have joined the iFolder \"%@\"", @"Message in notification window"), [ifolder Name]]
										forKey:@"description"];

		[self performNotification:notifyContext];
	}
}

//==============================================================================================
// colNotify
// This method will show a notification when a conflict is observed between client and server
//==============================================================================================
- (void) colNotify:(iFolder *)ifolder
{
	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_NOTIFYCOLL])
	{
		[notifyContext setObject:NSLocalizedString(@"Conflicts detected!", @"Title for notification window") forKey:@"title"];
		[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"The iFolder \"%@\" has conflicts", @"message in notification widnow"), [ifolder Name]]
							forKey:@"description"];

		[self performNotification:notifyContext];
	}
}

//==============================================================================================
// readOnlyNotify
// This method will show a notification when the going to synchronize is read-only one
//==============================================================================================
- (void) readOnlyNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
		
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Read-only iFolder prevented synchronization: \"%@\"", @"iFolder Read-only notification message"),[nameList objectAtIndex:1]] forKey:@"description"];
		
	[self performNotification:notifyContext];
}

//==============================================================================================
// iFolderFullNotify
// This method will show a notification when the space (quota) alloted for iFolder is full.
//==============================================================================================
- (void) iFolderFullNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
		
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Full iFolder prevented synchronization: \"%@\"", @"iFolder Window Status Message"),[nameList objectAtIndex:1]] forKey:@"description"];
		
	[self performNotification:notifyContext];
}

//============================================================================================================
// syncFailNotify
// This method will show a notification on the top right corner when sync fails due to exclude file policy.
//============================================================================================================
//-(void) syncFailNotify:(NSString*)ifolderAndFileName
//{
//	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_SYNCFAIL])
//	{
//		NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
//		
//		[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
//		[notifyContext setObject:NSLocalizedString(@"Synchronization log contains the information regarding the files that are not synchronized", @"iFolder SyncFail Notification Message")
//										forKey:@"description"];
						
//		[self performNotification:notifyContext];
//	}
//}

/*
- (void) policyNotify:(iFolder*)ifolder
{
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: %@",@"iFolder SyncFail Title"), [ifolder Name]] forKey:@"title"];
	[notifyContext setObject:NSLocalizedString(@"A policy prevented complete synchronization", @"iFolder policy Notification Message")
					  forKey:@"description"];
	
	[self performNotification:notifyContext];
	
}
*/

- (void) accessNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Insufficient rights prevented complete synchronization: \"%@\"", @"iFolder Access Notification Message"), [nameList objectAtIndex:1]] forKey:@"description"];
	
	[self performNotification:notifyContext];
	
}

- (void) lockedNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"The iFolder is locked: \"%@\"", @"iFolder Locked Notification Message"),[nameList objectAtIndex:1]] forKey:@"description"];
	
	[self performNotification:notifyContext];	
}

- (void) policySizeNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: %@",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"A size restriction policy prevented complete synchronization: \"%@\"", @"iFolder Policy Size Notification Message"),[nameList objectAtIndex:1]] forKey:@"description"];
	
	[self performNotification:notifyContext];	
}

- (void) policyTypeNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"A file type restriction policy prevented complete synchronization: \"%@\"", @"iFolder Policy Type Notification Message"),[nameList objectAtIndex:1]] forKey:@"description"];
	
	[self performNotification:notifyContext];	
}

- (void) diskFullNotify:(NSString*)ifolderAndFileName
{
	NSArray* nameList = [ifolderAndFileName componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"), [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Insufficient disk space on the server\/client prevented complete synchronization: \"%@\"", @"iFolder disk full Notification Message"),[nameList objectAtIndex:1]] forKey:@"description"];
	
	[self performNotification:notifyContext];	
	
}

- (void) ioErrorNotify:(NSString*)notificationMessageDetails
{
	NSArray* nameList = [notificationMessageDetails componentsSeparatedByString:@"###"];
	
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"Incomplete Synchronization: \"%@\"",@"iFolder SyncFail Title"),
				    [nameList objectAtIndex:0]] forKey:@"title"];
	[notifyContext setObject:[NSString stringWithFormat:NSLocalizedString(@"%@", @"iFolder IO error Notification Message"),
				    [nameList objectAtIndex:2]] forKey:@"description"];
	
	[self performNotification:notifyContext];	
	
}

//=================================================================================================
// performNotification
// This method will change the preference of showing the notification bubble or bounce the icon
//=================================================================================================
- (void) performNotification:(NSDictionary *) context
{
//	if( [[eventPrefs objectForKey:@"playSound"] boolValue] && ! [[NSUserDefaults standardUserDefaults] boolForKey:@"JVChatNotificationsMuted"] )
//		[self _playSound:[eventPrefs objectForKey:@"soundPath"]];
		[self _playSound:@"bogus"];


//	if( [[eventPrefs objectForKey:@"bounceIcon"] boolValue] )
//	{
//		if( [[eventPrefs objectForKey:@"bounceIconUntilFront"] boolValue] )
//			[self _bounceIconContinuously];
//		else [self _bounceIconOnce];
//	}
	if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_NOTIFYBYINDEX] == 0)
	{
		[self _bounceIconOnce];
	}
	else
		[self _bounceIconContinuously];
		


	if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_NOTIFYBYINDEX] == 0)
		[self _showBubble:context];
}
@end

#pragma mark -

@implementation iFolderNotificationController (iFolderNotificationControllerPrivate)
- (void) _bounceIconOnce
{
	[NSApp requestUserAttention:NSInformationalRequest];
}

- (void) _bounceIconContinuously
{
	[NSApp requestUserAttention:NSCriticalRequest];
}

- (void) _showBubble:(NSDictionary *) context
{
	KABubbleWindowController *bubble = nil;
	NSImage *icon = [context objectForKey:@"image"];
	id title = [context objectForKey:@"title"];
	id description = [context objectForKey:@"description"];
	
	if( ! icon ) icon = [[NSApplication sharedApplication] applicationIconImage];

		if( ( bubble = [_bubbles objectForKey:[context objectForKey:@"coalesceKey"]] ) ) {
			[(id)bubble setTitle:title];
			[(id)bubble setText:description];
			[(id)bubble setIcon:icon];
		} else {
			bubble = [KABubbleWindowController bubbleWithTitle:title text:description icon:icon];
		}

		[bubble setAutomaticallyFadesOut:(! [[context objectForKey:@"keepBubbleOnScreen"] boolValue] )];
		[bubble setTarget:[context objectForKey:@"target"]];
		[bubble setAction:NSSelectorFromString( [context objectForKey:@"action"] )];
		[bubble setRepresentedObject:[context objectForKey:@"representedObject"]];
		[bubble startFadeIn];

		if( [(NSString *)[context objectForKey:@"coalesceKey"] length] ) {
			[bubble setDelegate:self];
			[_bubbles setObject:bubble forKey:[context objectForKey:@"coalesceKey"]];
		}
}

- (void) bubbleDidFadeOut:(KABubbleWindowController *) bubble
{
	NSEnumerator *e = [[[_bubbles copy] autorelease] objectEnumerator];
	NSEnumerator *ke = [[[_bubbles copy] autorelease] keyEnumerator];
	KABubbleWindowController *cBubble = nil;
	NSString *key = nil;

	while( ( key = [ke nextObject] ) && ( cBubble = [e nextObject] ) )
		if( cBubble == bubble ) [_bubbles removeObjectForKey:key];
}


- (void) _playSound:(NSString *) path
{
	// This sucks, fix this up as soon as possible to let the users
	// pick a sound!
	NSString *sName = [[NSUserDefaults standardUserDefaults] objectForKey:PREFKEY_NOTIFYSOUND];
	// uh... this sucks! Can you say English only?
	if([sName compare:@"No sound"] == 0)
		return;

	notifySound = [NSSound soundNamed:sName];
	
//	if( ! path ) return;

//	if( ! [path isAbsolutePath] ) path = [[NSString stringWithFormat:@"%@/Sounds", [[NSBundle mainBundle] resourcePath]] stringByAppendingPathComponent:path];

//	NSSound *sound = [[NSSound alloc] initWithContentsOfFile:path byReference:YES];
	//[sound setDelegate:self];

	// When run on a laptop using battery power, the play method may block while the audio
	// hardware warms up.  If it blocks, the sound WILL NOT PLAY after the block ends.
	// To get around this, we check to make sure the sound is playing, and if it isn't
	// we call the play method again.

	[notifySound play];
	if( ! [notifySound isPlaying] ) [notifySound play];
}

//- (void) sound:(NSSound *) sound didFinishPlaying:(BOOL) finish
//{
//	[sound autorelease];
//}
@end

