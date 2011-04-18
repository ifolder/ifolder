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
*                 $Author: Satyam <ssutapalli@novell.com>    2/04/2008    Default iFolder functionality
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/05/2008 Added Secure sync option during upload
*-----------------------------------------------------------------------------
* This module is used to:
*        To create default iFolder for a domain
*
*
*******************************************************************************/

#import "DefaultiFolderSheetController.h"
#import "iFolderWindowController.h"
#import "iFolderData.h"
#import "iFolderDomain.h"
#import "iFolder.h"

@implementation DefaultiFolderSheetController

NSString* domID;
NSString* defiFolderID;

//======================================================================================
// awakeFromNib
// Default method called when linking the panel to application. Here linking of textfield
// to a textDidChange will be implemented. By default using delegates will not solve the
// problem
//======================================================================================
-(void) awakeFromNib
{
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:ifolderPath];
}

//======================================================================================
// onBrowse
// Method to select the default directory by clicking browse button
//======================================================================================
- (IBAction)onBrowse:(id)sender
{
	int result;
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	
	NSString *lastPath = [ifolderPath stringValue];
	if([lastPath length] > 0)
		result = [oPanel runModalForDirectory:lastPath file:nil types:nil];
	else
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result == NSOKButton)
	{
		[ifolderPath setStringValue:[oPanel directory]];
	}
}

//======================================================================================
// onCancel
// Method called when cancel button is pressed
//======================================================================================
- (IBAction)onCancel:(id)sender
{
	[defaultiFolderWindow orderOut:nil];
	[NSApp endSheet:defaultiFolderWindow];
}

//======================================================================================
// onCreate
// When create button is pressed, it will create the default directory on local machine
// if not already present. Then depending on encryption or regular mode of upload, the
// folder will be updated.
//======================================================================================
- (IBAction)onCreate:(id)sender
{
	NSString* createdDefaultiFolderID = nil;
	if(![[iFolderData sharedInstance] createDirectoriesRecurssively:[ifolderPath stringValue]])
	{
		return;
	}		
	
	if(isUpload)
	{
		if([security selectedColumn] ==1)
		{
			createdDefaultiFolderID = [[iFolderData sharedInstance] createiFolder:[ifolderPath stringValue] inDomain:domID withSSL:[defaultSecureSync state] usingAlgorithm:nil usingPassPhrase:nil];
		}
		else
		{
			NSString* pPhrase = [[iFolderData sharedInstance] getPassPhrase:domID];
			
			if(pPhrase != nil)
			{
				createdDefaultiFolderID = [[iFolderData sharedInstance] createiFolder:[ifolderPath stringValue] inDomain:domID withSSL:[defaultSecureSync state] usingAlgorithm:@"BlowFish" usingPassPhrase:pPhrase];
			}
			else
			{
				NSRunAlertPanel(NSLocalizedString(@"Enter Passphrase",@"Passphrase success title"),
								NSLocalizedString(@"Passphrase should be supplied for encrypting the iFolder",@"PP for Encryption Needed"),
								NSLocalizedString(@"OK",@"OK Button"),
								nil,nil);
				return;
			}
		}
		if(createdDefaultiFolderID != nil)
		{
			[[iFolderData sharedInstance] defaultAccountInDomainID:domID foriFolderID:createdDefaultiFolderID];
		}
	}
	else
	{
		BOOL isDir;
		int option;
		//int merge = NO;
		
		if([[NSFileManager defaultManager] fileExistsAtPath:[ifolderPath stringValue] isDirectory:&isDir] && isDir)
		{
			 option = NSRunAlertPanel(NSLocalizedString(@"A folder with the name you specified already exists",@"Folder already exists"),NSLocalizedString(@"Click Yes to merge or No to select a different location",@"Download default ifolder merge option"),NSLocalizedString(@"Yes",@"   Confirmation dialog to download default ifolder and merge"),NSLocalizedString(@"No",@"Negative confiramtion to download default ifolder and merge"),nil);
			
			if(option != NSAlertDefaultReturn)
			{
				return;
			}
			else
			{
			NSLog(@"onCreate2");
				//iFolder* tempiFolder = [[iFolderData sharedInstance] mergeiFolder:defiFolderID InDomain:domID toPath:[ifolderPath stringValue]];
				[[iFolderWindowController sharedInstance] acceptiFolderInvitation:defiFolderID InDomain:domID toPath:[ifolderPath stringValue] canMerge:YES];
				//merge = YES;
			}
		}
		else
		{
		NSLog(@"onCreate3");
			//download the default iFolder
			//iFolder* tempiFolder = nil;
			//tempiFolder = [[iFolderData sharedInstance] getiFolder:defiFolderID];
			//if(tempiFolder != nil)
			//{
				[[iFolderWindowController sharedInstance] acceptiFolderInvitation:defiFolderID InDomain:domID toPath:[ifolderPath stringValue] canMerge:NO];
			//}			
		}
	}
	
	[defaultiFolderWindow orderOut:nil];
	[NSApp endSheet:defaultiFolderWindow];
}

//======================================================================================
// onCreateDefaultiFolder
// Method that will be called when the check box to create default ifolder will pressed
//======================================================================================
- (IBAction)onCreateDefaultiFolder:(id)sender
{
	if([createDefaultiFolder state])
	{
		[ifolderPath setEnabled:YES];
		[self setSecurityButtonState];
		[browseButton setEnabled:YES];
		if(![[ifolderPath stringValue] isEqualToString:@""]  )
		{
			[createButton setEnabled:YES];
		}
		else
		{
			[createButton setEnabled:NO];
		}
	}
	else
	{
		[ifolderPath setEnabled:NO];
		[security setEnabled:NO];
		[browseButton setEnabled:NO];
		[createButton setEnabled:NO];
	}
}

//======================================================================================
// setDomainAndShow
// Main function to show the panel. Before showing the dialog, setting up of controls to
// initial values done here
//======================================================================================
- (void)setDomainAndShow:(NSString*)domainID
{
	if(domainID == nil || [domainID isEqualToString:@""])
	{
		return;
	}
		
	domID = domainID;
	//Set the default iFolder path
	NSString* homeDir = NSHomeDirectory();
	iFolderDomain* dom = [[iFolderData sharedInstance] getDomain:domainID];
	homeDir = [homeDir stringByAppendingFormat:@"/ifolder/%@/%@",[dom name],[dom userName]];
	homeDir = [homeDir stringByAppendingFormat:@"/%@",NSLocalizedString(@"Default",@"DefaultDirName")];
	homeDir = [homeDir stringByAppendingFormat:@"_%@",[dom userName]];
	[ifolderPath setStringValue:homeDir];

	[security setEnabled:NO];
	
	defiFolderID = [[[iFolderData sharedInstance] getDefaultiFolder:[dom ID]] retain];
	
	if( (defiFolderID == nil)  || [defiFolderID isEqualToString:@""])
	{
		isUpload = YES;
		//Get the passphrase status and set the security radio button accordingly
		[security setHidden:NO];
		[securityText setHidden:NO];
		[defaultSecureSync setHidden:NO];
		
		[security setState:1 atRow:0 column:0]; //Set Encryption
		secPolicy = [[iFolderData sharedInstance] getSecurityPolicy:domainID];
		[self setSecurityButtonState];
		[createDefaultiFolder setTitle:NSLocalizedString(@"Create Default iFolder",@"To Upload default iFolder Text")];
		[createButton setTitle:NSLocalizedString(@"Create",@"Create Default iFolder Text")];

		if([[dom hostURL] hasPrefix:@"https"])
		{
			[defaultSecureSync setEnabled:NO];
			[defaultSecureSync setState:NSOnState];
		}
		else
		{
			[defaultSecureSync setState:NSOffState];
			[defaultSecureSync setEnabled:YES];
		}
	}
	else
	{
		isUpload = NO;
		[security setHidden:YES];
		[securityText setHidden:YES];
		[defaultSecureSync setHidden:YES];
		
		[createDefaultiFolder setTitle:NSLocalizedString(@"Download Default iFolder",@"To download default iFolder Text")];
		[createButton setTitle:NSLocalizedString(@"Download",@"Download Default iFolder Text")];		
	}
	
	[createDefaultiFolder setState:NSOnState];
	[createButton setEnabled:YES];
	
	
	[NSApp beginSheet:defaultiFolderWindow modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];	
}

//======================================================================================
// setSecurityButtonState
// Sets the security radio buttons state and visibility according to that domains
// encryption is available or not.
//======================================================================================
- (void)setSecurityButtonState
{
	if(secPolicy != 0)
	{
		if((secPolicy & (int) encryption) == (int)encryption)		//Check for encryption set
		{
			if((secPolicy & (int) enforceEncryption) == (int)enforceEncryption)	//Check for enforce encryption
			{
				[security setState:1 atRow:0 column:0];	//Set Encryption
			}
			else
			{
				[security setEnabled:YES];
			}
		}
		else
		{
			[security setState:1 atRow:0 column:1]; //Set Regular
		}
	}
	else
	{
		[security setState:1 atRow:0 column:1]; //Set Regular
	}
}

//======================================================================================
// textDidChange
// Call back method whenever text in ifolder path changes
//======================================================================================
- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == ifolderPath)
	{
		if(![[ifolderPath stringValue] isEqualToString:@""]  )
		{
			[createButton setEnabled:YES];
		}
		else
		{
			[createButton setEnabled:NO];
		}
	}
}
@end
