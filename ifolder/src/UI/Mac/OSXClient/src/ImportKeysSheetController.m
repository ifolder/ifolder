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
*                 $Author: Satyam <ssutapalli@novell.com> 06/02/2007	Import decrypted passphrase
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008 Added validation for textfields
*                 $Modified by: Satyam <ssutapalli@novell.com> 13/05/2008 Allowing user to select only xml format files
*                 $Modified by: Satyam <ssutapalli@novell.com> 20/08/2008 Storing the PP after importing the keys according to remember PP option
*                 $Modified by: Satyam <ssutapalli@novell.com> 09/09/2008 UI controls enable/disable according to domain's encryption/regular policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class will be used to import the decrypted xml file received from trusted party and
*           then set the new pass phrase accordingly.
*******************************************************************************/

#import "ImportKeysSheetController.h"
#import "iFolderData.h"

@implementation ImportKeysSheetController

//=======================================================================
// awakeFromNib
// Method to set default's related to UI
//=======================================================================
-(void)awakeFromNib
{
	[domainID bind:@"value" toObject:[[iFolderData sharedInstance] domainArrayController]
	   withKeyPath:@"selection.properties.ID" options:nil];
	
	[ifolderAccount bind:@"contentValues" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"arrangedObjects.properties.name" options:nil];
	
	[ifolderAccount bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] domainArrayController]
			 withKeyPath:@"selectionIndex" options:nil];	
	
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:filePath];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:newPassphrase];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:retypePassphrase];
}

//=======================================================================
// onAccountChange
// This is action method when ever ifolder account is changed. It enables/
// disables other controls accordingly.
//=======================================================================
- (IBAction)onAccountChange:(id)sender
{
	[onetimePassphrase setStringValue:@""];
	[newPassphrase setStringValue:@""];
	[retypePassphrase setStringValue:@""];
	[filePath setStringValue:@""];
	[importButton setEnabled:NO];
	
	
	if([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] == 0 ||
	  ([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] != 0 && ![[iFolderData sharedInstance] isPassPhraseSet:[domainID stringValue]]))
	{
		[filePath setEnabled:NO];
		[browseButton setEnabled:NO];
		[newPassphrase setEnabled:NO];
		[onetimePassphrase setEnabled:NO];
		[retypePassphrase setEnabled:NO];
	}
	else
	{
		[filePath setEnabled:YES];
		[browseButton setEnabled:YES];
		[newPassphrase setEnabled:YES];
		[onetimePassphrase setEnabled:YES];
		[retypePassphrase setEnabled:YES];
	}
}

//=======================================================================
// onBrowse
// This method will be called when "Browse" button is clicked. This will
// be used to get the path of the decrypted keys xml file.
//=======================================================================
- (IBAction)onBrowse:(id)sender
{
	int result; 
	NSArray* types = [NSArray arrayWithObject:@"xml"];
	
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:NO];
	[oPanel setCanChooseFiles:YES];
	
	NSString *lastPath = [filePath stringValue];
	if([lastPath length] > 0)
		result = [oPanel runModalForDirectory:lastPath file:nil types:types];
	else
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:types];
	
	if (result == NSOKButton)
	{
		NSString *fileName = [oPanel filename];
		[filePath setStringValue:fileName];
		[self textDidChange:[NSNotification notificationWithName:@"BrowseButtonChange" object:filePath]];
	}		
}

//=======================================================================
// onCancel
// To take action when "cancel" button is clicked. This will just close
// the panel
//=======================================================================
- (IBAction)onCancel:(id)sender
{
	[importSheet orderOut:nil];
	[NSApp endSheet:importSheet];
}

//=======================================================================
// onImport
// This action method will be called when "Import" button is clicked. At
// first this will trim the empty characters from the passphrase's and 
// validate them. Then it will call appropriate simias function via 
// ifolderData else throw error/show alert.
//=======================================================================
- (IBAction)onImport:(id)sender
{
	if(![[[[filePath stringValue] pathExtension] lowercaseString] isEqualTo:@"xml"])
	{
		return;
	}
	   
	NSCharacterSet* charSet = [NSCharacterSet whitespaceCharacterSet];
	[newPassphrase setStringValue:[ [newPassphrase stringValue] stringByTrimmingCharactersInSet:charSet]];
	[onetimePassphrase setStringValue:[ [onetimePassphrase stringValue] stringByTrimmingCharactersInSet:charSet]];
	[retypePassphrase setStringValue:[ [retypePassphrase stringValue] stringByTrimmingCharactersInSet:charSet]];
	
	BOOL status = [[iFolderData sharedInstance] importiFoldersCryptoKeys:[domainID stringValue] withNewPP:[newPassphrase stringValue] onetimePassPhrase:[onetimePassphrase stringValue] andFilePath:[filePath stringValue]];
			
	if(status)
	{
		[[iFolderData sharedInstance] clearPassPhrase: [domainID stringValue]];
		[[iFolderData sharedInstance] storePassPhrase: [domainID stringValue] PassPhrase:[newPassphrase stringValue]  andRememberPP:[[iFolderData sharedInstance] getRememberPassphraseOption:[domainID stringValue]]];
		
		NSRunAlertPanel(NSLocalizedString(@"Import Decrypted Keys",@"ImportKey Title"),
						NSLocalizedString(@"Successfully imported the keys. You can use the new passphrase",@"ImportKey Success Message"),
						NSLocalizedString(@"OK",@"Import Keys Check"),
						nil,nil);
		
		[importSheet orderOut:nil];
		[NSApp endSheet:importSheet];
	}
}

//=======================================================================
// showWindow
// This method will be used to show the import keys window and sets 
// default values in UI. 
//=======================================================================
- (IBAction)showWindow:(id)sender
{
	[self onAccountChange:nil];
	
	[NSApp beginSheet:importSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

//=======================================================================
// textDidChange
// Selector method for validating the text fields and activating import
// button accordingly. The validation is that none of the text fields must
// be empty as well as new passphrase and retype passphrase are same.
//=======================================================================
- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == filePath || [aNotification object] == newPassphrase ||[aNotification object] == retypePassphrase )
	{
		if(![[filePath stringValue] isEqualToString:@""] &&
		   ([[newPassphrase stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePassphrase stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePassphrase stringValue] isEqualToString:[newPassphrase stringValue]] == YES ) &&
		   [ifolderAccount indexOfSelectedItem] != -1 )
		{
			[importButton setEnabled:YES];
		}
		else
		{
			[importButton setEnabled:NO];
		}
	}
}

@end
