#import "PropGeneralController.h"
#import "iFolderService.h"
#import "iFolderApplication.h"
#import "iFolderWindowController.h"
#import "iFolder.h"
#import "DiskSpace.h"
#import "TimeSpan.h"
#import "VerticalBarView.h"

@implementation PropGeneralController

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

		
		// Change this so the check box is gone
		if([[curiFolder CurrentUserID] compare:[curiFolder OwnerUserID]] == 0)
		{
			[enableLimit setEnabled:YES];
		}
		else
		{
			[enableLimit setEnabled:NO];
		}

		@try
		{
			diskSpace = [ifolderService GetiFolderDiskSpace:[curiFolder ID]];

			valueString = [NSString stringWithFormat:@"%qi", 
									[diskSpace UsedSpace]/(1024 * 1024)];
			if(valueString != nil)
				[currentSpace setStringValue:valueString];

			prevLimit = [diskSpace Limit];

			if([diskSpace Limit] != 0)
			{
				[enableLimit setState:YES];
				[limitSpace setEnabled:YES];
				[limitSpaceUnits setEnabled:YES];
				[limitSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([diskSpace Limit]/(1024 * 1024))]];
				[availableSpace setStringValue:[NSString stringWithFormat:@"%qi", 
										([diskSpace AvailableSpace]/(1024 * 1024))]];

				[barView setBarSize:[diskSpace Limit] withFill:[diskSpace UsedSpace]];
			}
			else
			{
				[enableLimit setState:NO];
				[limitSpace setStringValue:@""];
				[limitSpace setEnabled:NO];
				[limitSpaceUnits setEnabled:NO];
				[availableSpace setStringValue:@"0"];
				[barView setBarSize:0 withFill:0];
			}
		}
		@catch(NSException *ex)
		{
			prevLimit = 0;
		
			[enableLimit setState:NO];
			[limitSpace setStringValue:@""];
			[limitSpace setEnabled:NO];
			[limitSpaceUnits setEnabled:NO];
			[availableSpace setStringValue:@"0"];
			[currentSpace setStringValue:@"0"];
			[barView setBarSize:0 withFill:0];
		}

		// Display last sync time
		if([curiFolder LastSync] != nil)
			[lastSync setStringValue:[curiFolder LastSync]];
		else
			[lastSync setStringValue:NSLocalizedString(@"Not available", nil)];

		[syncInterval setStringValue:[NSString stringWithFormat:@"%d", 
								[curiFolder SyncInterval] ]];

		if([[curiFolder Role] compare:@"Master"] == 0)
		{
			[syncNow setEnabled:NO];
			[syncInterval setHidden:YES];
			[syncIntervalLabel setStringValue:@"Add controls here to set the allowed sync interval"];
		}
		else
		{
			[syncNow setEnabled:YES];
			if([curiFolder EffectiveSyncInterval] == 0)
			{
				[syncInterval setHidden:YES];
				[syncIntervalLabel setStringValue:NSLocalizedString(@"This iFolder does not automatically sync", nil)];
			}
			else
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

	}

}




- (IBAction)enableLimitToggled:(id)sender
{
	if([enableLimit state] == YES)
	{
		[limitSpace setStringValue:@"0"];
		[limitSpace setEnabled:YES];
		[limitSpaceUnits setEnabled:YES];
		[barView setBarSize:0 withFill:0];
	}
	else
	{
		[availableSpace setStringValue:@"0"];
		[limitSpace setStringValue:@""];
		[limitSpace setEnabled:NO];
		[limitSpaceUnits setEnabled:NO];
		[barView setBarSize:0 withFill:0];

		@try
		{
			[ifolderService SetiFolderDiskSpace:0 oniFolder:[curiFolder ID]];
		}
		@catch (NSException *e)
		{
		}

	}
}


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



- (IBAction)updateLimitValue:(id)sender
{
	long long usedValue = 0;

	if(diskSpace != nil)
		usedValue = [diskSpace UsedSpace];

	long long limitValue = ([limitSpace doubleValue] * 1024 * 1024);

	if(limitValue <= usedValue)
	{
		[availableSpace setStringValue:@"0"];
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
			[ifolderService SetiFolderDiskSpace:limitValue oniFolder:[curiFolder ID]];
		}
		@catch (NSException *e)
		{
		}
	}
}




@end
