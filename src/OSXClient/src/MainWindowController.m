#import "MainWindowController.h"
#import "IFSubViewController.h"

@implementation MainWindowController

-(id)init
{
    [super init];
    iFolderView = nil;
    return self;
}

- (void)dealloc
{
    [super dealloc];
    [iFolderView release];
}

-(void)awakeFromNib
{
	// Setup the views to look the way they should when
	//  load
	iFolderView = [[self loadViewFromNib:@"iFolderView"] retain];
    if (iFolderView) 
	{
		[[ifolderTabView tabViewItemAtIndex:0] setView:iFolderView];
	}

	webService = [[iFolderService alloc] init];

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
		domains = [ [NSMutableDictionary alloc]  initWithCapacity:2];
		[self showWindow:self];
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
	IFDomain *domain;
	
	@try
	{
		domain = [webService ConnectToDomain:username usingPassword:password andHost:server];
	}
	@catch (NSException *e)
	{
		NSString *error = [e name];
		
		NSRunAlertPanel(@"Error connecting to Server", [e name], @"OK",nil, nil);

		domain = nil;
	}

	if(domain != nil)
	{
		// If the domain is not know, add it to our Domains
		if([domains objectForKey:domain->ID] == nil)
			[domains setObject:domain forKey:domain->ID];
		[[_loginController window]  close];
		
		[self showWindow:self];
	}	
}




- (IBAction)login:(id)sender
{
	[self showLoginWindow];
}




-(NSView*)loadViewFromNib:(NSString*)nibName
{
    NSView * 		newView;
    IFSubViewController *	subViewController;
    
    subViewController = [IFSubViewController alloc];
    // Creates an instance of SubViewController which loads the specified nib.
    [subViewController initWithNibName:nibName andOwner:self];
    newView = [subViewController view];
    return newView;
}




- (void)tabView:(NSTabView*)tabView didSelectTabViewItem:(NSTabViewItem*)tabViewItem
{
    NSString * 		nibName;
    nibName = nil;
    // The NSTabView will manage the views being displayed but without the NSTabView, you need to use removeSubview: which releases the view and you need to retain it if you want to use it again later.

    // Based on the tab selected, we load the appropriate nib and set the tabViewItem's view to the 
    // view fromt he nib.
    if([[tabViewItem identifier] isEqualToString:@"1"])
	{
        if(iFolderView) 
            [tabViewItem setView:iFolderView];
        else 
		{
            iFolderView = [[self loadViewFromNib:@"iFolderView"] retain];
            if (iFolderView) 
			{
                [tabViewItem setView:iFolderView];
            }
        }
    }
	
/*
    if([[tabViewItem identifier] isEqualToString:@"2"]){
        if(_setColorNibView) 
            [tabViewItem setView:_setColorNibView];
        else {
            _setColorNibView = [[self loadViewFromNib:@"SetColor"] retain];
            if (_setColorNibView) {
                [tabViewItem setView:_setColorNibView];
            }
        }
    }
*/
}



@end
