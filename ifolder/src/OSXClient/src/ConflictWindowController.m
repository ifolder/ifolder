#import "ConflictWindowController.h"
#import "iFolder.h"
#import "iFolderService.h"
#import "iFolderWindowController.h"


@implementation ConflictWindowController

static ConflictWindowController *conflictSharedInstance = nil;


+ (ConflictWindowController *)sharedInstance
{
	if(conflictSharedInstance == nil)
	{
		conflictSharedInstance = [[ConflictWindowController alloc] initWithWindowNibName:@"ConflictResolver"];
	}

    return conflictSharedInstance;
}


- (void)windowWillClose:(NSNotification *)aNotification
{
	if(conflictSharedInstance != nil)
	{
		[conflictSharedInstance release];
		conflictSharedInstance = nil;
	}
}


- (void)awakeFromNib
{
	NSLog(@"ConflictWindowController Awoke from Nib");

	ifolderService = [[iFolderService alloc] init];

	ifolder = [[[iFolderWindowController sharedInstance] selectediFolder] retain];

	if(ifolder != nil)
	{
		[ifolderName setStringValue:[ifolder Name]];
		[ifolderPath setStringValue:[ifolder Path]];
		
		if([ifolder HasConflicts])
		{
			@try
			{
				NSArray *ifolderconflicts = [ifolderService GetiFolderConflicts:[ifolder ID]];
				[ifoldersController setContent:ifolderconflicts];
			}
			@catch(NSException *e)
			{
			}
		}
	}
}




- (IBAction)saveLocal:(id)sender
{
}

- (IBAction)saveServer:(id)sender
{
}



@end
