#import "ChangePasswordSheetController.h"
#import "iFolderData.h"

@implementation ChangePasswordSheetController

//=======================================================================
// awakeFromNib
// Method to set default's related to UI
//=======================================================================
-(void)awakeFromNib
{
	[domainID bind:@"value" toObject:[[iFolderData sharedInstance] loggedDomainArrayController]
	   withKeyPath:@"selection.properties.ID" options:nil];
	
	[ifolderAccount bind:@"contentValues" toObject:[[iFolderData sharedInstance] loggedDomainArrayController]
			 withKeyPath:@"arrangedObjects.properties.name" options:nil];
	
	[ifolderAccount bind:@"selectedIndex" toObject:[[iFolderData sharedInstance] loggedDomainArrayController]
			 withKeyPath:@"selectionIndex" options:nil];	
	
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:enterOldPassword];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:enterNewPassword];
	[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(textDidChange:)  name:NSControlTextDidChangeNotification object:retypePassword];
}


- (IBAction)onCancel:(id)sender
{
	[changePasswordSheet orderOut:nil];
	[NSApp endSheet:changePasswordSheet];
}

- (IBAction)onChange:(id)sender
{
	if([[enterOldPassword stringValue] isEqualToString:[enterNewPassword stringValue]] == YES)
	{
		NSRunAlertPanel(NSLocalizedString(@"Invalid new password",@"Invalid new password"),
						NSLocalizedString(@"Old password and new password should not be same",@"Old password and new password should not be same"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		return;
	}
	
	if([[enterNewPassword stringValue] isEqualToString:[retypePassword stringValue]] == NO)
    {
		NSRunAlertPanel(NSLocalizedString(@"Invalid new password",@"Invalid new password"),
						NSLocalizedString(@"New password and confirm password do not match",@"New password and confirm password do not match"),
						NSLocalizedString(@"OK",@"OK Button"),nil,nil);
		return;
    }
	
	NSNumber* returnStatus = [[iFolderData sharedInstance] changeUserPassword:[domainID stringValue] changePassword:[enterOldPassword stringValue] withNewPassword:[enterNewPassword stringValue]];
	NSString* message = nil;
	NSString* title = NSLocalizedString(@"Error changing password",@"Error changing password");
	
	switch([returnStatus intValue])
	{
		case 0:
			if([rememberPassword state] == YES)
			{
				[[iFolderData sharedInstance] setDomainPassword:[domainID stringValue] withPassword:[enterNewPassword stringValue]];
			}
			else
			{
				[[iFolderData sharedInstance] setDomainPassword:[domainID stringValue] withPassword:nil];				
			}
			title = NSLocalizedString(@"Successful",@"Successful");
			message = NSLocalizedString(@"Successfuly changed password.Log on to the domain with new password",@"Successfuly changed password");
			
			break;
		case 1:
			message = NSLocalizedString(@"Incorrect old password",@"Incorrect old password");
			break;
		case 2:
			message = NSLocalizedString(@"Failed to reset password",@"Failed to reset password");
			break;
		case 3:
			message = NSLocalizedString(@"Login disabled",@"Login disabled");
			break;
		case 4:
			message = NSLocalizedString(@"User account expired",@"User account expired");
			break;
		case 5:
			message = NSLocalizedString(@"User cannot change password",@"User cannot change password");
			break;
		case 6:
			message = NSLocalizedString(@"User password expired",@"User password expired");
			break;
		case 7:
			message = NSLocalizedString(@"Minimum password length restriction not met",@"Minimum password length restriction not met");
			break;
		case 8:
			message = NSLocalizedString(@"User not found in simias",@"User not found in simias");
			break;
		default:
			message = NSLocalizedString(@"Error changing password",@"Error changing password");
			break;
	}
	
	NSRunAlertPanel(title,message,NSLocalizedString(@"OK",@"OK Button"),nil,nil);
	
	if([returnStatus intValue] == 0)
	{
		[[iFolderData sharedInstance] logoutFromRemoteDomain:[domainID stringValue]];
		[changePasswordSheet orderOut:nil];
		[NSApp endSheet:changePasswordSheet];
		[[iFolderData sharedInstance] refresh:NO];
	}
}

- (IBAction)showWindow:(id)sender
{
	[enterOldPassword setStringValue:@""];
	[enterNewPassword setStringValue:@""];
	[retypePassword setStringValue:@""];
	
	[NSApp beginSheet:changePasswordSheet modalForWindow:mainWindow
		modalDelegate:self didEndSelector:NULL contextInfo:nil];
}

- (void)textDidChange:(NSNotification *)aNotification
{
	if([aNotification object] == enterOldPassword || [aNotification object] == enterNewPassword || [aNotification object] == retypePassword )
	{
		if(([[enterOldPassword stringValue] compare:@""] != NSOrderedSame) &&
		   ([[enterNewPassword stringValue] compare:@""] != NSOrderedSame) &&
		   ([[retypePassword stringValue] compare:@""] != NSOrderedSame) &&
		   ([[enterNewPassword stringValue] isEqualToString:[retypePassword stringValue]]) &&
		   [ifolderAccount indexOfSelectedItem] != -1 )
		{
			[changeButton setEnabled:YES];
		}
		else
		{
			[changeButton setEnabled:NO];
		}
	}	
}

@end
