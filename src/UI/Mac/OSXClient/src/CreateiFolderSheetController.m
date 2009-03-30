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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/04/2008   Added secure sync for creating iFolder
*                 $Modified by: Satyam <ssutapalli@novell.com>	18-06-2008  Added notification while creating new iFolder
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "CreateiFolderSheetController.h"
#import "iFolderWindowController.h"
#import "iFolderData.h"
#import "iFolderApplication.h"

@implementation CreateiFolderSheetController

//=======================================================================
// awakeFromNib
// This method will be called when window/panel is being loaded from NIB.
// Here we will bind the controls to array controllers so that the values
// of the controls are automatically taken from arrays pointed by 
// array controllers. For that we need to bind them as shown in the method.
//=======================================================================
-(void)awakeFromNib
{
	// bind the fields up to our data
	[domainIDField bind:@"value" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"selection.properties.ID" options:nil];

	[domainSelector bind:@"contentValues" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"arrangedObjects.properties.name" options:nil];

	[domainSelector bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] domainArrayController]
				withKeyPath:@"selectionIndex" options:nil];

	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:pathField];
	
	[securityMode setEnabled:NO];
	[okButton setEnabled:NO];
}

//=======================================================================
// showWindow
// To display the window this function has to be called.
//=======================================================================
- (IBAction) showWindow:(id)sender
{
	// Because we use the same dialog, clear out the path
	[pathField setStringValue:@""];
	[self domainSelectionChanges:nil];
	
	[NSApp beginSheet:createSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

//=======================================================================
// cancelCreating
// This method will be called when user clicks cancel button. It will 
// close the UI.
//=======================================================================
- (IBAction) cancelCreating:(id)sender
{
	[createSheet orderOut:nil];
	[NSApp endSheet:createSheet];
}

//=======================================================================
// createiFolder
// This method will be called when user accepts to create iFolder. First 
// it will validate the inputs, then create iFolder accordingly to 
// security features set or not set.
//======================================================================= 
- (IBAction) createiFolder:(id)sender
{
	NSString* newiFolderID  = nil;
	BOOL isDir;
	NSFileManager *fileManager = [NSFileManager defaultManager];
	
	if (([fileManager fileExistsAtPath:[pathField stringValue] isDirectory:&isDir] == NO) || !isDir)
	{
		NSRunAlertPanel(NSLocalizedString(@"Select Directory",@"Select directory title"),NSLocalizedString(@"Either the selected item is not a directory or the selected path does not exist.  Select an available directory to upload",@"Select directory message"),NSLocalizedString(@"OK",@"OK"),nil,nil);
		return;
	}
		
	if(	( [ [domainIDField stringValue] length] > 0 ) &&
		( [ [pathField stringValue] length] > 0 ) )
	{
		if( [securityMode selectedColumn] == 1 )	//Normal upload
		{
			newiFolderID = [[iFolderData sharedInstance] createiFolder:[pathField stringValue] inDomain:[domainIDField stringValue] withSSL:[secureSync state] usingAlgorithm:nil usingPassPhrase:nil];
		}
		else	//Encrypted upload
		{
			NSString *algorithm = [NSString stringWithString:@"BlowFish"];
			NSString* pPhrase = [[iFolderData sharedInstance] getPassPhrase:[domainIDField stringValue]];
			if(pPhrase != nil)
			{
				newiFolderID = [[iFolderData sharedInstance] createiFolder:[pathField stringValue] inDomain:[domainIDField stringValue] withSSL:[secureSync state] usingAlgorithm:algorithm usingPassPhrase:pPhrase];
			}
			else
			{
				NSRunAlertPanel(NSLocalizedString(@"Enter Passphrase",@"Passphrase success title"),
								NSLocalizedString(@"Passphrase should be supplied for encrypting the iFolder",@"PP for Encryption Needed"),
								NSLocalizedString(@"Yes",@"Passphrase successfully set button"),
								nil,nil);
			}
		}
		
		if(newiFolderID != nil && [[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_CREATEIFOLDER])
		{
			NSRunAlertPanel(NSLocalizedString(@"iFolder Created",@"New iFolder Notification title"),
							NSLocalizedString(@"The selected folder is now an iFolder. To learn more about using iFolder and sharing iFolders with other users, see in iFolder Help",@"New iFolder Notification message"),
							NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		}
		
		[createSheet orderOut:nil];
		[NSApp endSheet:createSheet];
	}
}


//=======================================================================
// browseForPath
// This method will be called when Browse button in UI is clicked. This 
// will open a panel asking to select the dir to be uploaded. Then if OK
// is clicked, it will enable/disable security options to user.
//======================================================================= 
- (IBAction) browseForPath:(id)sender
{
	int result;
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	NSString *lastPath = [pathField stringValue];
	if([lastPath length] > 0)
		result = [oPanel runModalForDirectory:lastPath file:nil types:nil];
	else
		result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result == NSOKButton)
	{
		NSString *dirName = [oPanel directory];
		[pathField setStringValue:dirName];
		[self textDidChange:[NSNotification notificationWithName:@"BrowseButtonChange" object:pathField]];
		//[okButton setEnabled:YES];
	}
}


//======================================================================================
// domainSelectionChanges
// By default Encryption is selected and (NSMatrix) securityMode is disabled
// If Encryption is not set, Regular is selected and (NSMatrix) securityMode is enabled
// If Enforce Encryption is set, (NSMatrix) securityMode is not enabled
//======================================================================================
- (IBAction) domainSelectionChanges:(id)sender
{
	[securityMode setEnabled:NO];
	[securityMode setState:1 atRow:0 column:0]; //Set Encryption
	
	secPolicy = [[iFolderData sharedInstance] getSecurityPolicy:[domainIDField stringValue]];
	if(secPolicy != 0)
	{
		if((secPolicy & (int) encryption) == (int)encryption)		//Check for encryption set
		{
			if((secPolicy & (int) enforceEncryption) == (int)enforceEncryption)	//Check for enforce encryption
			{
				[securityMode setState:1 atRow:0 column:0];	//Set Encryption
			}
			else
			{
				[securityMode setEnabled:YES];
			}
		}
		else
		{
			[securityMode setState:1 atRow:0 column:1]; //Set Regular
		}
	}
	else
	{
		[securityMode setState:1 atRow:0 column:1]; //Set Regular
	}
	
	iFolderDomain* dom = [[iFolderData sharedInstance] getDomain:[domainIDField stringValue]];
	if([[dom hostURL] hasPrefix:@"https"])
	{
		[secureSync setEnabled:NO];
		[secureSync setState:NSOnState];
	}
	else
	{
		[secureSync setEnabled:YES];
		[secureSync setState:NSOffState];
	}
}

//=======================================================================
// textDidChange
// Selector method for validating the text fields and activating import
// button accordingly. The validation is that none of the text fields must
// be empty as well as new passphrase and retype passphrase are same.
//=======================================================================
- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == pathField)
	{
		if(![[pathField stringValue] isEqualToString:@""] &&
		   [domainSelector indexOfSelectedItem] != -1 )
		{
			[okButton setEnabled:YES];
		}
		else
		{
			[okButton setEnabled:NO];
		}
	}
}

@end
