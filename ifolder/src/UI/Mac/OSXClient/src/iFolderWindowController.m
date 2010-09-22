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
*                 $Modified by: Satyam <ssutapalli@novell.com> 29/02/2008    Added menu item "Merge" and its implementation
*                                                                            Also added toolbar images for merge and delete
*                 $Modified by: Satyam <ssutapalli@novell.com> 25/03/2008    Modified visibility of "New" tool bar item
*                 $Modified by: Satyam <ssutapalli@novell.com> 10/04/2008    Security Menu items will be available only after login
*                 $Modified by: Satyam <ssutapalli@novell.com> 22/05/2008    Bypassing Create iFolder directly to iFolderData
*                 $Modified by: Satyam <ssutapalli@novell.com> 16-07-2008    Stopping and starting the refresh timer when manual refresh is made
*                 $Modified by: Satyam <ssutapalli@novell.com> 14-08-2008    Deactivating delete and revert ifolder while syncing
*                 $Modified by: Satyam <ssutapalli@novell.com> 26-08-2008    Removed extra checking for encryption in acceptiFolderInvitation while downloading
*                 $Modified by: Satyam <ssutapalli@novell.com>	17-09-2008   Commented the code which uses poBoxID of a domain
*                 $Modified by: Satyam <ssutapalli@novell.com> 13/10/2008    Vertical alignment of text field cell in table
*                 $Modified by: Satyam <ssutapalli@novell.com> 02/12/2008    Moved the handling of UI refresh timer from refreshWindow to iFolderData:refresh method
*                 $Modified by: Satyam <ssutapalli@novell.com> 02/06/2009    Added menu item for "Forgot Passphrase"
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "iFolderWindowController.h"
#import "iFolderApplication.h"
#import "CreateiFolderSheetController.h"
#import "SetupiFolderSheetController.h"
#import "PropertiesWindowController.h"
#import "ConflictWindowController.h"
#import "ExportKeysSheetController.h"
#import "ImportKeysSheetController.h"
#import "ResetPPKeySheetController.h"
#import "AdvConflictController.h"
#import "iFolder.h"
#import "iFolderDomain.h"
#import "iFolderData.h"
#import "MCTableView.h"
#import "applog.h"
#import "iFolderTextFieldCell.h"
#import "ForgotPassphraseSheetController.h"

@implementation iFolderWindowController


static iFolderWindowController *sharedInstance = nil;


+ (iFolderWindowController *)sharedInstance
{
	if(sharedInstance == nil)
	{
		sharedInstance = [[iFolderWindowController alloc] initWithWindowNibName:@"iFolderWindow"];
	}

    return sharedInstance;
}




+(void)updateStatusTS:(NSString *)message
{
	if(sharedInstance != nil)
	{
		[sharedInstance performSelectorOnMainThread:@selector(updateStatus:) 
					withObject:message waitUntilDone:NO ];		
	}
}




-(void)updateStatus:(NSString *)message
{
	[statusText setStringValue:message];
}


+(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal
{
	if(sharedInstance != nil)
	{
		[sharedInstance updateProgress:curVal withMin:minVal withMax:maxVal];
	}
}


-(void)updateProgress:(double)curVal withMin:(double)minVal withMax:(double)maxVal
{
	if(curVal == -1)
		[statusProgress setHidden:YES];
	else
	{
		[statusProgress setHidden:NO];
		[statusProgress setMinValue:minVal];
		[statusProgress setMaxValue:maxVal];
		[statusProgress setDoubleValue:curVal];
	}
}




- (void)windowWillClose:(NSNotification *)aNotification
{
	if(sharedInstance != nil)
	{
		[sharedInstance release];
		sharedInstance = nil;
		[[NSUserDefaults standardUserDefaults] setBool:NO forKey:STATE_SHOWMAINWINDOW];		
	}
}




- (void)dealloc
{
	[toolbar release];
	[toolbarItems release];	
	[toolbarItemKeys release];	
    [super dealloc];
}




-(void)awakeFromNib
{
	[self setupToolbar];

	if([[NSUserDefaults standardUserDefaults] boolForKey:PREFKEY_WINPOS])
	{
		[super setShouldCascadeWindows:NO];
		[super setWindowFrameAutosaveName:@"iFolderWindow"];
	}

	ifoldersController = [[iFolderData sharedInstance] ifolderArrayController];

	NSMutableDictionary *bindingOptions = [NSMutableDictionary dictionary];
    	
	// binding options for "name"
	[bindingOptions setObject:@"" forKey:@"NSNullPlaceholder"];

	//Create user defined text field cell and associate them to table coloumns
	ifolderCell = [[iFolderTextFieldCell alloc] init];
	
	[nameColumn setDataCell:ifolderCell];
	[locationColumn setDataCell:ifolderCell];
	[statusColumn setDataCell:ifolderCell];
	[serverColumn setDataCell:ifolderCell];

	// bind the table column to the log to display it's contents
	[iconColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Image" options:bindingOptions];
	[nameColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Name" options:bindingOptions];	
	[locationColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Location" options:bindingOptions];	
	[statusColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.properties.Status" options:bindingOptions];	
	[serverColumn bind:@"value" toObject:ifoldersController
					withKeyPath:@"arrangedObjects.DomainName" options:bindingOptions];	

	// Setup the double click black magic
	[iFolderTable setDoubleAction:@selector(doubleClickedTable:)];
	
	[[NSUserDefaults standardUserDefaults] setBool:YES forKey:STATE_SHOWMAINWINDOW];		
}




- (IBAction)refreshWindow:(id)sender
{
	//[[NSApp delegate] stopRefreshTimer];
	
	int domainCount;
	[[NSApp delegate] addLog:NSLocalizedString(@"Refreshing iFolder view", @"Log Message when refreshing main window")];

	if([[NSApp delegate] simiasIsRunning])
		[[iFolderData sharedInstance] refresh:NO];

	// Get all of the domains and refresh their POBoxes
	/*
	NSArray *domains = [[iFolderData sharedInstance] getDomains];
	
	for(domainCount = 0; domainCount < [domains count]; domainCount++)
	{
		iFolderDomain *dom = [domains objectAtIndex:domainCount];
		
		if(dom != nil)
		{
			//[[iFolderData sharedInstance] synciFolderNow:[dom poBoxID]];
		}
	}
	*/
	
	//[[NSApp delegate] startRefreshTimer];
	// calling refresh on iFolderData calls refreshDomains
//	[self refreshDomains];

//	NSArray *newiFolders = [[iFolderData sharedInstance] getiFolders];
//	if(newiFolders != nil)
//	{
//		[ifoldersController setContent:newiFolders];
//	}
}



+(void)refreshDomainsTS
{
	if(sharedInstance != nil)
	{
		[sharedInstance performSelectorOnMainThread:@selector(refreshDomains:) 
					withObject:self waitUntilDone:YES ];
	}
}
-(void)refreshDomains:(id)args
{
/*
	NSArray *newDomains = [[iFolderData sharedInstance] getDomains];
	if(newDomains != nil)
	{
		[domainsController setContent:newDomains];
	}
*/
}





- (IBAction)showHideToolbar:(id)sender
{
	[toolbar setVisible:![toolbar isVisible]];
}




- (IBAction)customizeToolbar:(id)sender
{
	[toolbar runCustomizationPalette:sender];
}




- (IBAction)newiFolder:(id)sender
{
	if([[iFolderData sharedInstance] getDomainCount] < 1)
	{
		NSBeginAlertSheet(NSLocalizedString(@"Set up an iFolder account", @"Set Up an iFolder Account message"), NSLocalizedString(@"OK", @"Error dialog button new iFolder"), nil, nil,
			[self window], self, nil, nil, NULL, 
			NSLocalizedString(@"To begin using iFolder, you must first set up an iFolder account.", @"Error dialog message new iFolder"));
	}
	else
	{
		[[iFolderData sharedInstance] selectDefaultDomain];
		[createSheetController showWindow:self];
	}
}





- (IBAction)setupiFolder:(id)sender
{
	// We don't have to tell the sheet anything about the iFolder because
	// it's all bound in the nib
	[setupSheetController showWindow:sender];
}




- (IBAction)revertiFolder:(id)sender
{
	[revertiFolderController showWindow:sender];
}

- (void)revertiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			@try
			{
				[[iFolderData sharedInstance] revertiFolder:[ifolder ID]];		
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Revert Error", @"Revert error Dialog title"),  
								NSLocalizedString(@"An error was encountered while reverting the iFolder.", @"Revert error Dialog message"), 
								NSLocalizedString(@"OK", @"Revert error Dialog button"),
								nil, nil);
			}
			break;
		}
	}
}



- (IBAction)synciFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	[[iFolderData sharedInstance] synciFolderNow:[ifolder ID]];
}



- (IBAction)deleteiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	if([ifolder IsSubscription])
	{
		if([[ifolder OwnerUserID] compare:[ifolder CurrentUserID]] == 0)
		{
			NSBeginAlertSheet(NSLocalizedString(@"Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog Title"), 
							  NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"), 
							  NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
							  [self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex,
							  NSLocalizedString(@"Because you are the iFolder owner, this unshares the iFolder, reverts all copies of the iFolder to normal folders, and deletes the iFolder from the server. Local copies of the iFolder contents are not deleted from computers where the iFolder is currently set up.", @"Delete iFolder Dialog message"));
		}
		else
		{
			NSBeginAlertSheet(NSLocalizedString(@"Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog Title"), 
							  NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"), 
							  NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
							  [self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex,
							  NSLocalizedString(@"This removes you as a member of the iFolder. Local copies of the iFolder contents are not deleted from computers where the iFolder is currently set up. You cannot access the original iFolder from any computer unless the owner re-invites you.", @"Delete iFolder Dialog message"));
		}
	}
	else
	{
		if([[ifolder OwnerUserID] compare:[ifolder CurrentUserID]] == 0)
		{
			NSBeginAlertSheet(NSLocalizedString(@"Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog Title"), 
							  NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"),
							  NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
							  [self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex,
							  NSLocalizedString(@"This removes the iFolder from your local computer.  Because you are the owner, the iFolder is deleted from the server and all member computers.  The iFolder cannot be recovered or re-shared on another computer.  The files are not deleted from your local hard drive.", @"Delete iFolder Dialog message"));
		}
		else
		{
			NSBeginAlertSheet(NSLocalizedString(@"Are you sure you want to delete the iFolder?", @"Delete iFolder Dialog Title"), 
							  NSLocalizedString(@"Yes", @"Delete iFolder Dialog button"), 
							  NSLocalizedString(@"Cancel", @"Delete iFolder Dialog button"), nil,
							  [self window], self, @selector(deleteiFolderResponse:returnCode:contextInfo:), nil, (void *)selIndex,
							  NSLocalizedString(@"This removes you as a member of the iFolder.  You cannot access the iFolder unless the owner re-invites you.  The files are not deleted from your local hard drive.", @"Delete iFolder Dialog message"));
		}
	}
}

- (IBAction)mergeiFolder:(id)sender
{
	//Get the path of the folder
	int result;
	NSString* dirName;
	
	int selIndex = [ifoldersController selectionIndex];
	iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	
	NSOpenPanel *oPanel = [NSOpenPanel openPanel];
	
	[oPanel setAllowsMultipleSelection:NO];
	[oPanel setCanChooseDirectories:YES];
	[oPanel setCanChooseFiles:NO];
	result = [oPanel runModalForDirectory:NSHomeDirectory() file:nil types:nil];
	
	if (result != NSOKButton)
	{
		return;
	}
	
	dirName = [oPanel directory];
	
	if([[dirName lastPathComponent] isEqualToString:[ifolder Name]] == NO)
	{
		NSRunInformationalAlertPanel(NSLocalizedString(@"Folder does not exist!", @"iFolder Folder DoesNot Exist Error Title"),
									 NSLocalizedString(@"The name of folder and iFolder must be the same.", @"iFolder Folder DoesNot Exist Error Message"),
									 NSLocalizedString(@"OK", @"Folder Doesnot exist dialog button"),nil, nil);
		return;
	}
	else if([[iFolderData sharedInstance] isiFolderByPath:dirName])
	{
		NSRunInformationalAlertPanel(NSLocalizedString(@"The selected location is inside another iFolder.", @"iFolder insideCollection create error dialog message"),
									 NSLocalizedString(@"iFolders cannot exist inside other iFolders.  Please select a different location and try again.", @"iFolder insideCollection create error dialog details"),
									 NSLocalizedString(@"OK", @"iFolder insideCollection create error dialog button"),nil, nil);
		return;
	}
	
	[self acceptiFolderInvitation:[ifolder ID] InDomain:[ifolder DomainID] toPath:dirName canMerge:YES];
}


- (IBAction)resolveConflicts:(id)sender
{
	[advConflictSheetController showWindow:self];
	//[[ConflictWindowController sharedInstance] showWindow:self];
}



- (void)deleteiFolderResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo
{
	switch(returnCode)
	{
		case NSAlertDefaultReturn:		// Revert iFolder
		{
			iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:(int)contextInfo];

			@try
			{
				[[iFolderData sharedInstance] deleteiFolder:[ifolder ID] fromDomain:[ifolder DomainID]];
			}
			@catch (NSException *e)
			{
				NSRunAlertPanel(NSLocalizedString(@"iFolder Deletion Error", @"iFolder delete error dialog title"), 
								NSLocalizedString(@"An error was encountered while deleting the iFolder.", @"iFolder delete error dialog message"),
								NSLocalizedString(@"OK", @"iFolder delete error dialog button"),nil, nil);
			}
			break;
		}
	}
}




- (IBAction)openiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];

	if(selIndex != NSNotFound)
	{
		iFolder *ifolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
		NSString *path = [ifolder Path];

		if(	([path length] > 0) &&
			([ifolder IsSubscription] == NO) )
		{
			if([[NSUserDefaults standardUserDefaults] integerForKey:PREFKEY_CLICKIFOLDER] == 0)
				[[NSWorkspace sharedWorkspace] openFile:path];
			else
				[self showProperties:self];
		}
		else
			[self setupiFolder:sender];
	}
}




- (IBAction)shareiFolder:(id)sender
{
	int selIndex = [ifoldersController selectionIndex];
	iFolder *curiFolder = [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
	
	if ([curiFolder EncryptionAlgorithm] == nil || [[curiFolder EncryptionAlgorithm] compare:@""] == NSOrderedSame)
	{
		[[PropertiesWindowController sharedInstance] setSharingTab];
		[[PropertiesWindowController sharedInstance] showWindow:self];		
	}
	else
	{
		NSRunAlertPanel(NSLocalizedString(@"Cannot share iFolder",@"Cannot share iFolder title"),NSLocalizedString(@"It is not possible to share an Encrypted iFolder. Only regular iFolders can be shared",@"Cannot share iFolder message"),NSLocalizedString(@"OK",@"OK"),nil,nil);
	}

}


- (IBAction)showProperties:(id)sender
{
	[[PropertiesWindowController sharedInstance] setGeneralTab];
	[[PropertiesWindowController sharedInstance] showWindow:self];
}


- (IBAction)exportKeys:(id)sender
{
	[[iFolderData sharedInstance] selectDefaultLoggedDomain];
	[exportKeysSheetController showWindow:self];
}

- (IBAction)importKeys:(id)sender
{
	[[iFolderData sharedInstance] selectDefaultLoggedDomain];
	[importKeysSheetController showWindow:self];
}

- (IBAction)resetPassPhrase:(id)sender
{
	[[iFolderData sharedInstance] selectDefaultLoggedDomain];
	[resetPPKeySheetController showWindow:self];
}

- (void)doubleClickedTable:(id)sender
{
	[self openiFolder:sender];
}

- (IBAction)changePassword:(id)sender
{
	[[iFolderData sharedInstance] selectDefaultLoggedDomain];
	[changePasswordSheetController showWindow:self];
}

- (IBAction)forgotPassphrase:(id)sender
{
	[[iFolderData sharedInstance] selectDefaultLoggedDomain];
	[forgotPPSheetController showWindow:self];
}

//=======================================================================
// createiFolder
// This will further call iFolderData's create iFolder. This is just a
// bridge between UI and simias/ifolder webservice.
//=======================================================================
//-(NSString*)createiFolder:(NSString *) path inDomain:(NSString *)domainID usingAlgorithm:(NSString *)encrAlgthm usingPassPhrase:(NSString *)passPhrase
/*
- (NSString*)createiFolder:(NSString *)path inDomain:(NSString *)domainID withSSL:(BOOL)ssl usingAlgorithm:(NSString *)encrAlgthm usingPassPhrase:(NSString *)passPhrase
{
	NSString* ifolderID = nil;
	@try
	{
		ifolderID = [[iFolderData sharedInstance] createiFolder:path inDomain:domainID usingAlgorithm:encrAlgthm usingPassPhrase:passPhrase];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];

		if(![self handleiFolderError:error])
		{
			NSRunAlertPanel(	NSLocalizedString(@"Error creating iFolder", @"iFolder create error dialog title"),
								NSLocalizedString(@"iFolder was unable to convert the selected folder to an iFolder for an unknown reason.  Please check the path selected and try again.", @"iFolder create error dialog details"), 
								NSLocalizedString(@"OK", @"iFolder create error dialog button"),nil, nil);
		}
	}
	return ifolderID;
}
*/
//=======================================================================
// acceptiFolderInvitation
// This will be used to create ifolder or merge with current one.
//=======================================================================
- (void)acceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath canMerge:(BOOL)merge;
{
	@try
	{
		//[[iFolderData sharedInstance] checkForEncryption:domainID atLogin:NO];
		[[iFolderData sharedInstance] acceptiFolderInvitation:iFolderID InDomain:domainID toPath:localPath canMerge:merge];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];

		if(![self handleiFolderError:error])
		{
			NSRunAlertPanel(NSLocalizedString(@"Error setting up iFolder", @"iFolder setup error dialog title"),
							NSLocalizedString(@"iFolder was unable to set up the selected iFolder for an unknown reason.  Please check the path selected and try again.", @"iFolder setup error dialog details"),
							NSLocalizedString(@"OK", @"iFolder setup error dialog button"),nil, nil);
		}
	}
}

- (BOOL)handleiFolderError:(NSString *)error
{
	if([error compare:@"PathExists"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"Cannot download iFolder", @"iFolder folderexists create error dialog message"),
						NSLocalizedString(@"A folder with the same name already exists in the location you specified. Specify a different location and try again", @"iFolder folderexists create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder folderexists create error dialog button"),nil, nil);
	}
	else if([error compare:@"RootOfDrivePath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"iFolders cannot exist at the drive level.", @"iFolder rootdrive create error dialog message"),
						NSLocalizedString(@"The location you selected is at the root of the drive.  Please select a location that is not at the root of a drive and try again.", @"iFolder rootdrive create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder rootdrive create error dialog button"),nil, nil);
	}
	else if([error compare:@"InvalidCharactersPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location contains invalid characters.", @"iFolder badchar create error dialog message"),
						NSLocalizedString(@"The characters \\:*?\"<>| cannot be used in an iFolder. Please select a different location and try again.", @"iFolder badchar create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder badchar create error dialog button"),nil, nil);
	}
	else if([error compare:@"AtOrInsideStorePath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location is inside the iFolder data folder.", @"iFolder instorepath create error dialog message"),
						NSLocalizedString(@"The iFolder data folder is normally located in your home folder in the folder \".local/share\".  Please select a different location and try again.", @"iFolder instorepath create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder instorepath create error dialog button"),nil, nil);
	}
	else if([error compare:@"ContainsStorePath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location contains the iFolder data files.", @"iFolder storepath create error dialog message"),
						NSLocalizedString(@"The location you have selected contains the iFolder data files.  These are normally located in your home folder in the folder \".local/share\".  Please select a different location and try again.", @"iFolder storepath create error dialog details"),
						NSLocalizedString(@"OK", @"iFolder storepath create error dialog button"),nil, nil);
	}
	else if([error compare:@"NotFixedDrivePath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location is on a network or non-physical drive.", @"iFolder notfixed create error dialog message"),
						NSLocalizedString(@"iFolders must reside on a physical drive.  Please select a different location and try again.", @"iFolder notfixed create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder notfixed create error dialog button"),nil, nil);
	}
	else if([error compare:@"SystemDirectoryPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location contains a system folder.", @"iFolder sysdir create error dialog message"), 
						NSLocalizedString(@"System folders cannot be contained in an iFolder.  Please select a different location and try again.", @"iFolder sysdir create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder sysdir create error dialog button"),nil, nil);
	}
	else if([error compare:@"SystemDrivePath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location is a system drive.", @"iFolder sysdrive create error dialog message"),
						NSLocalizedString(@"System drives cannot be contained in an iFolder.  Please select a different location and try again.", @"iFolder sysdrive create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder sysdrive create error dialog button"),nil, nil);
	}
	else if([error compare:@"IncludesWinDirPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location includes the Windows folder.", @"iFolder windir create error dialog message"), 
						NSLocalizedString(@"The Windows folder cannot be contained in an iFolder.  Please select a different location and try again.", @"iFolder windir create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder windir create error dialog button"),nil, nil);
	}
	else if([error compare:@"IncludesProgFilesPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location includes the Program Files folder.", @"iFolder progdir create error dialog message"),
						NSLocalizedString(@"The Program Files folder cannot be contained in an iFolder.  Please select a different location and try again.", @"iFolder progdir create error dialog details"),
						NSLocalizedString(@"OK", @"iFolder progdir create error dialog button"),nil, nil);
	}
	else if([error compare:@"DoesNotExistPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location does not exist.", @"iFolder notexist create error dialog message"),
						NSLocalizedString(@"iFolders can only be created from folders that exist.  Please select a different location and try again.", @"iFolder notexist create error dialog details"), 
						NSLocalizedString(@"OK", @"iFolder notexist create error dialog button"),nil, nil);
	}
	else if([error compare:@"NoReadRightsPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"You do not have access to read files in the selected location.", @"iFolder readrights create error dialog message"),
						NSLocalizedString(@"iFolders can only be created from folders where you have access to read and write files.  Please select a different location and try again.", @"iFolder readrights create error dialog details"),
						NSLocalizedString(@"OK", @"iFolder readrights create error dialog button"),nil, nil);
	}
	else if([error compare:@"NoWriteRightsPath"] == 0)
	{
		NSRunAlertPanel(	NSLocalizedString(@"You do not have access to write files in the selected location.", @"iFolder writerights create error dialog message"), 
			NSLocalizedString(@"iFolders can only be created from folders where you have access to read and write files.  Please select a different location and try again.", @"iFolder writerights create error dialog details"), 
			NSLocalizedString(@"OK", @"iFolder writerights create error dialog button"),nil, nil);
	}
	else if([error compare:@"ContainsCollectionPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location already contains an iFolder.", @"iFolder containsCollection create error dialog message"),
						NSLocalizedString(@"iFolders cannot exist inside other iFolders.  Please select a different location and try again.", @"iFolder containsCollection create error dialog details"),
						NSLocalizedString(@"OK", @"iFolder containsCollection create error dialog button"),nil, nil);
	}
	else if([error compare:@"AtOrInsideCollectionPath"] == 0)
	{
		NSRunAlertPanel(NSLocalizedString(@"The selected location is inside another iFolder.", @"iFolder insideCollection create error dialog message"),
						NSLocalizedString(@"iFolders cannot exist inside other iFolders.  Please select a different location and try again.", @"iFolder insideCollection create error dialog details"),
						NSLocalizedString(@"OK", @"iFolder insideCollection create error dialog button"),nil, nil);
	}
	else
		return NO;
	
	return YES;
}




- (void)addDomain:(iFolderDomain *)newDomain
{
//	[domainsController addObject:newDomain];
//	[keyedDomains setObject:newDomain forKey:[newDomain ID] ];
}





- (BOOL)validateUserInterfaceItem:(id)anItem
{
	SEL action = [anItem action];

	if(action == @selector(newiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;
		/*
		if([ifoldersController selectionIndex]  != NSNotFound)
		{
			return NO;
		}
		*/
		
		return YES;
	}
	else if(action == @selector(setupiFolder:) || action == @selector(mergeiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex]
						IsSubscription] == YES)
				return YES;
		}
		return NO;
	}
	else if(action == @selector(deleteiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if ([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] syncState] != SYNC_STATE_SYNCING)
			{
				return YES;				
			}
		}
		return NO;
	}
	else if(	(action == @selector(openiFolder:)) ||
				(action == @selector(showProperties:)) ||
				(action == @selector(shareiFolder:)) ||
				(action == @selector(synciFolder:)) )
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];
			
		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex]
						IsSubscription] == NO)
				return YES;
		}
		return NO;
	}
	//if([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] EncryptionAlgorithm] == nil || [[[ifoldersController arrangedObjects] objectAtIndex:selIndex] EncryptionAlgorithm] compare:@""] == NSOrderedSame) Check for "Shar with" menu item
	else if(action == @selector(revertiFolder:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];
			
		if (selIndex != NSNotFound)
		{
			if( ([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] IsSubscription] == NO) &&
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] valueForKeyPath:@"properties.IsWorkgroup"] boolValue] == NO) &&
				([[[[ifoldersController arrangedObjects] objectAtIndex:selIndex] Role] compare:@"Master"] != 0) &&
				([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] syncState] != SYNC_STATE_SYNCING) )
			    {
				     return YES;
				}
		}
		return NO;
	}
	else if(action == @selector(resolveConflicts:))
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;

		int selIndex = [ifoldersController selectionIndex];

		if (selIndex != NSNotFound)
		{
			if([[[ifoldersController arrangedObjects] objectAtIndex:selIndex] HasConflicts] == YES)
				return YES;
		}
		return NO;
	}
	else if( (action == @selector(exportKeys:)) ||
			 (action == @selector(importKeys:)) ||
			 (action == @selector(forgotPassphrase:)) || 
			 (action == @selector(resetPassPhrase:)) ||
			 (action == @selector(changePassword:)) )
	{
		if([[NSApp delegate] simiasIsRunning] == NO)
			return NO;
		
		if([[iFolderData sharedInstance] getLoggedDomainCount] > 0)
		{
			return YES;
		}
		return NO;
	}

	return YES;
}


-(iFolder *)selectediFolder
{
	int selIndex = [ifoldersController selectionIndex];
	return [[ifoldersController arrangedObjects] objectAtIndex:selIndex];
}

// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag
{
	return[toolbarItems objectForKey:itemIdentifier];
}


- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar
{
	return toolbarItemKeys;
}


- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar
{
	return [toolbarItemKeys subarrayWithRange:NSMakeRange(0,8)];
}


- (void)setupToolbar
{
	toolbarItems =		[[NSMutableDictionary alloc] init];
	toolbarItemKeys =	[[NSMutableArray alloc] init];

	// New iFolder ToolbarItem
	NSToolbarItem *item=[[NSToolbarItem alloc] initWithItemIdentifier:@"NewiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Create a new iFolder", @"iFolderWin Toolbar New Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Upload", @"iFolderWin Toolbar New Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Create a new iFolder", @"iFolderWin Toolbar New ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(newiFolder:)];
	[item setImage:[NSImage imageNamed:@"newifolder32"]];
    [toolbarItems setObject:item forKey:@"NewiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"NewiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SetupiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Download iFolder", @"iFolderWin Toolbar Set Up Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Download", @"iFolderWin Toolbar Set Up Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Download the selected iFolder", @"iFolderWin Toolbar Set Up ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(setupiFolder:)];
	[item setImage:[NSImage imageNamed:@"ifolder-download32"]];
    [toolbarItems setObject:item forKey:@"SetupiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SetupiFolder"];
	[item release];
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarSpaceItemIdentifier];
	[item release];

	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ShareiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Share an iFolder", @"iFolderWin Toolbar Share Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Share", @"iFolderWin Toolbar Share Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Share the selected iFolder", @"iFolderWin Toolbar Share ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(shareiFolder:)];
	[item setImage:[NSImage imageNamed:@"share32"]];
    [toolbarItems setObject:item forKey:@"ShareiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ShareiFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"ResolveConflicts"];
	[item setPaletteLabel:NSLocalizedString(@"Resolve Conflicts", @"iFolderWin Toolbar Resolve Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Resolve", @"iFolderWin Toolbar Resolve Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Resolve conflicts in the selected iFolder", @"iFolderWin Toolbar Resolve ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(resolveConflicts:)];
	[item setImage:[NSImage imageNamed:@"conflict32"]];
    [toolbarItems setObject:item forKey:@"ResolveConflicts"]; // add to toolbar list
	[toolbarItemKeys addObject:@"ResolveConflicts"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"SynciFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Synchronize iFolder", @"iFolderWin Toolbar Synchronize Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Synchronize", @"iFolderWin Toolbar Synchronize Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Synchronize the selected iFolder", @"iFolderWin Toolbar Synchronize ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(synciFolder:)];
	[item setImage:[NSImage imageNamed:@"sync32"]];
    [toolbarItems setObject:item forKey:@"SynciFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"SynciFolder"];
	[item release];

	item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItems setObject:item forKey:NSToolbarFlexibleSpaceItemIdentifier];
	[toolbarItemKeys addObject:NSToolbarFlexibleSpaceItemIdentifier];
	[item release];
	
    item=[[NSToolbarItem alloc] initWithItemIdentifier:NSToolbarCustomizeToolbarItemIdentifier];
    [toolbarItems setObject:item forKey:NSToolbarCustomizeToolbarItemIdentifier];
    [toolbarItemKeys addObject:NSToolbarCustomizeToolbarItemIdentifier];
    [item release];

	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"MergeiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Merge iFolder", @"iFolderWin Toolbar Merge iFolder Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Merge", @"iFolderWin Toolbar Merge Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Merge the selected iFolder with local folder", @"iFolderWin Toolbar Merge ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(mergeiFolder:)];
	[item setImage:[NSImage imageNamed:@"merge32"]];
    [toolbarItems setObject:item forKey:@"MergeiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"MergeiFolder"];
	[item release];
	
	item=[[NSToolbarItem alloc] initWithItemIdentifier:@"DeleteiFolder"];
	[item setPaletteLabel:NSLocalizedString(@"Delete iFolder", @"iFolderWin Toolbar Delete Pallette Button")]; // name for the "Customize Toolbar" sheet
	[item setLabel:NSLocalizedString(@"Delete", @"iFolderWin Toolbar Delete Selector Button")]; // name for the item in the toolbar
	[item setToolTip:NSLocalizedString(@"Delete the selected iFolder", @"iFolderWin Toolbar Delete ToolTip")]; // tooltip
    [item setTarget:self]; // what should happen when it's clicked
    [item setAction:@selector(deleteiFolder:)];
	[item setImage:[NSImage imageNamed:@"delete32"]];
    [toolbarItems setObject:item forKey:@"DeleteiFolder"]; // add to toolbar list
	[toolbarItemKeys addObject:@"DeleteiFolder"];
	[item release];
	
	
	toolbar = [[NSToolbar alloc] initWithIdentifier:@"iFolderToolbar"];
	[toolbar setDelegate:self];
	[toolbar setAllowsUserCustomization:YES];
	[toolbar setAutosavesConfiguration:YES];
	[[self window] setToolbar:toolbar];
	
}

@end
