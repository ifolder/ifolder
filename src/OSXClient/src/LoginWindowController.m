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
 
#import "LoginWindowController.h"

@implementation LoginWindowController

- (IBAction)cancel:(id)sender
{
	[[self window] orderOut:nil];
}

- (IBAction)login:(id)sender
{
	if( ( [ [userNameField stringValue] length] > 0 ) &&
		( [ [passwordField stringValue] length] > 0 ) &&
		( [ [serverField stringValue] length] > 0 ) )
	{
		[[NSApp delegate] login:[userNameField stringValue] withPassword:[passwordField stringValue] 
					toServer:[serverField stringValue] ];
		[[self window] orderOut:nil];
	}
}

@end
