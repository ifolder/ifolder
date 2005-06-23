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
+ (iFolderNotificationController *) defaultManager
{
	extern iFolderNotificationController *sharedInstance;
	return ( sharedInstance ? sharedInstance : ( sharedInstance = [[self alloc] init] ) );
}



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

- (void) dealloc
{
	extern iFolderNotificationController *sharedInstance;

	[_bubbles release];
	[notifyContext release];

	[[NSNotificationCenter defaultCenter] removeObserver:self];
	if( self == sharedInstance ) sharedInstance = nil;

	_bubbles = nil;
	notifyContext = nil;
	
	[super dealloc];
}


+ (void) newiFolderNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(ifolderNotify:) 
				withObject:ifolder waitUntilDone:YES ];	
}

+ (void) newUserNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(userNotify:) 
				withObject:ifolder waitUntilDone:YES ];
}

+ (void) collisionNotification:(iFolder *)ifolder
{
	[[self defaultManager] performSelectorOnMainThread:@selector(colNotify:) 
				withObject:ifolder waitUntilDone:YES ];
}


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
		[self _bounceIconOnce];
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

	NSSound *sound = [NSSound soundNamed:sName];
	
//	if( ! path ) return;

//	if( ! [path isAbsolutePath] ) path = [[NSString stringWithFormat:@"%@/Sounds", [[NSBundle mainBundle] resourcePath]] stringByAppendingPathComponent:path];

//	NSSound *sound = [[NSSound alloc] initWithContentsOfFile:path byReference:YES];
	[sound setDelegate:self];

	// When run on a laptop using battery power, the play method may block while the audio
	// hardware warms up.  If it blocks, the sound WILL NOT PLAY after the block ends.
	// To get around this, we check to make sure the sound is playing, and if it isn't
	// we call the play method again.

	[sound play];
	if( ! [sound isPlaying] ) [sound play];
}

- (void) sound:(NSSound *) sound didFinishPlaying:(BOOL) finish
{
	[sound autorelease];
}
@end

