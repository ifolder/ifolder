/* AccountsController */

#import <Cocoa/Cocoa.h>

@class iFolderDomain;
@class iFolderService;

@interface AccountsController : NSObject
{
    IBOutlet NSTableView *accounts;
    IBOutlet NSButton *activate;
    IBOutlet NSButton *addAccount;
    IBOutlet NSButton *defaultAccount;
    IBOutlet NSButton *enableAccount;
    IBOutlet NSTextField *host;
    IBOutlet NSTextField *name;
    IBOutlet NSSecureTextField *password;
    IBOutlet NSButton *rememberPassword;
    IBOutlet NSButton *removeAccount;
    IBOutlet NSTextField *userName;
    IBOutlet NSView *view;
	IBOutlet NSWindow	*parentWindow;

	iFolderService		*webService;	
	NSMutableArray		*domains;
	iFolderDomain		*selectedDomain;

	BOOL				createMode;
}

- (IBAction)activateAccount:(id)sender;
- (IBAction)addAccount:(id)sender;
- (IBAction)removeAccount:(id)sender;
- (IBAction)toggleActive:(id)sender;
- (IBAction)toggleDefault:(id)sender;
- (IBAction)toggleSavePassword:(id)sender;

- (void)awakeFromNib;

-(void)refreshData;

// NSTableViewDelegates
- (BOOL)selectionShouldChangeInTableView:(NSTableView *)aTableView;

// delegate for error sheet
- (void)changeAccountResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;

// Delegates for TableView
-(int)numberOfRowsInTableView:(NSTableView *)aTableView;
-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex;
-(void)tableView:(NSTableView *)aTableView setObjectValue:(id)anObject forTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex;
- (void)tableViewSelectionDidChange:(NSNotification *)aNotification;

// Delegates for text view
- (void)controlTextDidChange:(NSNotification *)aNotification;

@end
