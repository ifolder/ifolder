#import "MainWindowController.h"

@implementation MainWindowController

-(void)awakeFromNib
{
	webService = [[iFolderService alloc] init];
//	domains = [[NSMutableDictionary alloc] initWithCapacity:2];

	@try
	{
		domains = [webService GetDomains];

		NSArray *keys = [domains allKeys];

		// if we have less than two domains, we don't have enterprise
		// so we better ask the user to login
		if([keys count] < 2)
			[self showLoginWindow];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		domains = nil;
	}
}

- (void)showLoginWindow
{
	if(_loginController == nil)
	{
		_loginController = [[LoginWindowController alloc] initWithWindowNibName:@"LoginWindow"];
	}
	
	[[_loginController window] center];
	[_loginController showWindow:self];
}

-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server
{
	Domain *domain;
	
	@try
	{
		domain = [webService ConnectToDomain:username in_Password:password in_Host:server];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		
		if([error hasPrefix:@"Error: NameResolutionFailure"])
			NSRunAlertPanel(@"Error contacting Server", @"Unable to resolve the iFolder Server host", @"OK",nil, nil);
		else if([error hasPrefix:@"/CFStreamFault"])
			NSRunAlertPanel(@"Simias Communication Error", @"Unable to communicate with the Simias Process", @"OK", nil, nil);
		else if([error hasSuffix:@"Unauthorized"])
			NSRunAlertPanel(@"Invalid iFolder Credentials", error, @"OK", nil, nil);
		else if([error hasPrefix:@"The request timed out"])
			NSRunAlertPanel(@"Error contacting Server", @"The Login request timed out", @"OK", nil, nil);
		else
			NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);

		domain = nil;
	}

	if(domain != nil)
	{
		// If the domain is not know, add it to our Domains
		if([domains objectForKey:domain->ID] == nil)
			[domains setObject:domain forKey:domain->ID];
		[[_loginController window]  close];
	}	
}

- (IBAction)login:(id)sender
{
	[self showLoginWindow];
}


@end
