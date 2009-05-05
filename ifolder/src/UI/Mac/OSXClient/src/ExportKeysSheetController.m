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
*                 $Author: Satyam <ssutapalli@novell.com> 07/02/2007	Export Encrypted keys for PP recovery
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008 Added validation for ifolder path
*                 $Modified by: Satyam <ssutapalli@novell.com> 18/06/2008 Modified validation of file path extention
*                 $Modified by: Satyam <ssutapalli@novell.com> 09/09/2008 UI controls enable/disable according to domain's encryption/regular policy
*-----------------------------------------------------------------------------
* This module is used to:
*			This class is usefull for exporting the encrypted pass phrase keys in form of 
*           xml format. This xml file has to be sent to trusted party to get it decrypted.
*******************************************************************************/

#import "ExportKeysSheetController.h"
#import "iFolderData.h"

@implementation ExportKeysSheetController

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
}

//=======================================================================
// onAccountChange
// This is action method when ever ifolder account is changed. It gets the
// RA name and extracts email from it and displays. Then enable/disable 
// Export button accordingly
//=======================================================================
- (IBAction)onAccountChange:(id)sender
{	
	[recoveryAgent setStringValue:@""];
	[emailID setStringValue:@""];
	[filePath setStringValue:@""];
	
	if([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] == 0 || 
	  ([[iFolderData sharedInstance] getSecurityPolicy:[domainID stringValue]] != 0 && ![[iFolderData sharedInstance] isPassPhraseSet:[domainID stringValue]]))
	{
		[recoveryAgent setEnabled:NO];
		[emailID setEnabled:NO];
		[filePath setEnabled:NO];
		[browseButton setEnabled:NO];
		[exportButton setEnabled:NO];
		return;
	}
	
	[recoveryAgent setEnabled:YES];
	[emailID setEnabled:YES];
	[filePath setEnabled:YES];
	[browseButton setEnabled:YES];
	
	raNameForDomain = nil;
	
	@try
	{
		raNameForDomain = [[iFolderData sharedInstance] getRAName:[domainID stringValue]];	
	}
	@catch(NSException *ex)
	{
		[recoveryAgent setStringValue:@""];
		[emailID setStringValue:@""];
	}
	
	if (raNameForDomain == nil || [raNameForDomain compare:@""] == NSOrderedSame)
	{
		/*
		NSRunCriticalAlertPanel(NSLocalizedString(@"No Recovery Agent Available",@"RA Not found title"),
						NSLocalizedString(@"No Recovery Agent is selected for this domain",@"RA Not found message"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		*/
		return;
	}
	
	[recoveryAgent setStringValue:raNameForDomain];
	NSArray* email = [raNameForDomain componentsSeparatedByString:@"="];
	if([email count] > 0)
	{
		int cnt = [email count] - 1;
		[emailID setStringValue:[email objectAtIndex:cnt]];
	}
}

//=======================================================================
// onBrowse
// This method will be called when "Browse" button is clicked. This will
// be used to get the path where to save the encrypted keys received in 
// xml format file.
//=======================================================================
- (IBAction)onBrowse:(id)sender
{
	int result;
	NSArray* types = [NSArray arrayWithObject:@"xml"];
	
	NSSavePanel *sPanel = [NSSavePanel savePanel];
	[sPanel setAllowedFileTypes:types];
	result = [sPanel runModalForDirectory:NSHomeDirectory() file:nil];
	
	if (result == NSOKButton)
	{
		NSString* fName = [NSString stringWithString:[sPanel filename]];
		[filePath setStringValue:fName];
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
	[exportSheet orderOut:nil];
	[NSApp endSheet:exportSheet];
}

//=======================================================================
// onExport
// This action method will be called when "Export" button is clicked. It
// will call simias service via iFolderData for getting the encrypted keys
//=======================================================================
- (IBAction)onExport:(id)sender
{
	if(![[[[filePath stringValue] pathExtension] lowercaseString] isEqualTo:@"xml"])
	{
		NSRunAlertPanel(NSLocalizedString(@"File Extention Error",@"File extention error title"),
						NSLocalizedString(@"File extention must be of format \"xml\". Pleae check the extention",@"File extention error message"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		return;
	}
	
	BOOL status = [[iFolderData sharedInstance] exportiFoldersCryptoKeys:[domainID stringValue] withfilePath:[filePath stringValue]];
	
	if(status)
	{
		NSRunAlertPanel(NSLocalizedString(@"Export Encrypted Keys",@"ExportKey Title"),
						NSLocalizedString(@"Successfully exported the keys",@"ExportKey Success Message"),
						NSLocalizedString(@"OK",@"Export Keys successfully"),
						nil,nil);		
		
		[exportSheet orderOut:nil];
		[NSApp endSheet:exportSheet];
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
	/*
	if (raNameForDomain == nil || [raNameForDomain compare:@""] == NSOrderedSame)
	{
		return;
	}
	*/
	[filePath setStringValue:@""];
	[exportButton setEnabled:NO];
	
	[NSApp beginSheet:exportSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

//=======================================================================
// textDidChange
// Delegate that will be called whenever text in the observers had changed 
//=======================================================================
- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == filePath)
	{
		if( ([[emailID stringValue] compare:@""] == NSOrderedSame) ||
			([[recoveryAgent stringValue] compare:@""] == NSOrderedSame) ||
			([[filePath stringValue] compare:@""] == NSOrderedSame) )
		{
			[exportButton setEnabled:NO];			
		}
		else
		{
			[exportButton setEnabled:YES];
		}
	}
}
@end
