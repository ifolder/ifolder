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
*                 $Modified by: Satyam <ssutapalli@novell.com>  22/05/2008  Added Secure sync check box in properties
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#import "PropGeneralController.h"
#import "iFolderService.h"
#import "iFolderApplication.h"
#import "iFolderWindowController.h"
#import "iFolder.h"
#import "DiskSpace.h"
#import "TimeSpan.h"
#import "VerticalBarView.h"
#include "applog.h"

@implementation PropGeneralController

//===========================================================================
// awakeFromNib
// Method that will be called before loading Nib
//===========================================================================
-(void)awakeFromNib
{
	diskSpace = nil;
	ifolderService = [[iFolderService alloc] init];
	NSString *valueString;

	curiFolder = [[[iFolderWindowController sharedInstance] selectediFolder] retain];
	if(curiFolder != nil)
	{
		valueString = [curiFolder Name];
		if(valueString != nil)
			[ifolderName setStringValue:valueString];

		valueString = [curiFolder OwnerName];
		if(valueString != nil)
			[ownerName setStringValue:valueString];
		
		if([curiFolder HasConflicts])
		{
			[hasConflicts setHidden:NO];
			[hasConflictsImage setHidden:NO];
		}

		
		@try
		{
			diskSpace = [ifolderService GetiFolderDiskSpace:[curiFolder ID]];
			
			valueString = [NSString stringWithFormat:@"%qi", 
									[diskSpace UsedSpace]/(1024 * 1024)];
			if(valueString != nil)
				[currentSpace setStringValue:valueString];

			prevLimit = [diskSpace Limit];
			if([diskSpace Limit] >= 0)
			{
				[limitSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([diskSpace Limit]/(1024 * 1024))]];
				[availableSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([diskSpace AvailableSpace]/(1024 * 1024))]];

				[barView setBarSize:[diskSpace Limit] withFill:[diskSpace UsedSpace]];
			}
			else
			{
				[limitSpace setStringValue:@"N/A"];
				[availableSpace setStringValue:@"N/A"];
				[barView setBarSize:0 withFill:0];
			}
		}
		@catch(NSException *ex)
		{
			prevLimit = 0;
		
			[limitSpace setStringValue:@"N/A"];
			[availableSpace setStringValue:@"N/A"];
			[currentSpace setStringValue:@"N/A"];
			[barView setBarSize:0 withFill:0];
		}

		// Display last sync time
		if([curiFolder LastSync] != nil)
			[lastSync setStringValue:[curiFolder LastSync]];
		else
			[lastSync setStringValue:NSLocalizedString(@"Not available", nil)];

		//[syncInterval setStringValue:[NSString stringWithFormat:@"%d", [curiFolder SyncInterval] ]];

		if([[curiFolder Role] compare:@"Master"] == 0)
		{
			[syncNow setEnabled:NO];
			[syncInterval setHidden:YES];
			[syncIntervalLabel setStringValue:@""];
		}
		else
		{
			[syncNow setEnabled:YES];
			if([curiFolder EffectiveSyncInterval] == 0)
			{
				[syncInterval setHidden:YES];
				[syncIntervalLabel setStringValue:NSLocalizedString(@"This iFolder does not automatically sync", nil)];
			}
			else if([curiFolder EffectiveSyncInterval] > 0)
			{
				[syncInterval setStringValue:[NSString stringWithFormat:@"%d %@", 
								[TimeSpan getTimeSpanValue:[curiFolder EffectiveSyncInterval]],
								[TimeSpan getTimeSpanUnits:[curiFolder EffectiveSyncInterval]]]];
			}
		}


		@try
		{
			SyncSize *ss = [ifolderService CalculateSyncSize:[curiFolder ID]];
			
			if(ss != nil)
			{
				[filesToSync setStringValue:[NSString stringWithFormat:@"%d", [ss SyncNodeCount]]];
			}
		}
		@catch(NSException *e)
		{
			[filesToSync setStringValue:NSLocalizedString(@"unknown", nil)];
		}
		
		[secureSync setState:[curiFolder SSL]];
		if([[curiFolder CurrentUserID] compare:[curiFolder OwnerUserID]] != 0)
		{
			[secureSync setEnabled:NO];
		}
	}

}

//===========================================================================
// dealloc
// Deallocate the resouces that are allocated previously
//===========================================================================
-(void)dealloc
{
	//[self updateLimitValue:self];
	[ifolderService release];	
	[super dealloc];
}

//===========================================================================
// syncNow
// Initiate sync from client to server
//===========================================================================
- (IBAction)syncNow:(id)sender
{
	@try
	{
		[ifolderService SynciFolderNow:[curiFolder ID]];
	}
	@catch (NSException *e)
	{
	}
}

//===========================================================================
// updateLimitValue
// Update the limit value of space for the ifolder
//===========================================================================
- (IBAction)updateLimitValue:(id)sender
{
	long long usedValue = 0;

	if(diskSpace != nil)
		usedValue = [diskSpace UsedSpace];

	long long limitValue = ([limitSpace doubleValue] * 1024 * 1024);

	if(limitValue <= usedValue)
	{
		[availableSpace setStringValue:@"N/A"];
		[barView setBarSize:1 withFill:1];
	}
	else
	{
		long long availSpace = 0;
		if(usedValue > 0)
			availSpace = limitValue - usedValue;

		[availableSpace setStringValue:
			[NSString stringWithFormat:@"%qi", availSpace / (1024 * 1024)]];

		[barView setBarSize:limitValue withFill:usedValue];
	}

	if(prevLimit != limitValue)
	{
		prevLimit = limitValue;
		@try
		{
			NSLog(@"Saving the disk space settings");
			// thick client user does not have right to set disk quota policy so commenting the code
			//[ifolderService SetiFolderDiskSpace:limitValue oniFolder:[curiFolder ID]];
		}
		@catch (NSException *e)
		{
		}
	}
}

//===========================================================================
// setiFolderSecureSync
// Set the iFolder secure sync checkbox depending on the securesync state
//===========================================================================
- (IBAction)setiFolderSecureSync:(id)sender
{
	@try
	{
		[ifolderService SetiFolderSecureSync:[curiFolder ID] withSSL:[secureSync state]];			
	}
	@catch(NSException* ex)
	{
		ifexconlog(@"Cannot set ifolder's secure sync setting",ex);
	}
}


@end
