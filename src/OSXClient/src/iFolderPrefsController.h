/* iFolderPrefsController */

#import <Cocoa/Cocoa.h>

@class iFolderDomain;
@class iFolderService;

@interface iFolderPrefsController : NSWindowController
{
    IBOutlet NSView		*generalView;
    IBOutlet NSView		*accountsView;
    IBOutlet NSView		*notifyView;
    IBOutlet NSView		*syncView;
	IBOutlet NSView		*blankView;

    IBOutlet NSArrayController		*domainsController;
	iFolderService					*webService;	
	
	NSMutableArray		*domains;
	iFolderDomain		*selectedDomain;
	
	NSToolbar				*toolbar;
	NSMutableDictionary		*toolbarItemDict;	
	NSMutableArray			*toolbarItemArray;
	int			modalReturnCode;
}

- (IBAction)addDomain:(id)sender;
- (IBAction)removeDomain:(id)sender;
- (IBAction)loginToDomain:(id)sender;

- (void) updateSize:(NSSize)newSize;

// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarSelectableItemIdentifiers:(NSToolbar *)toolbar;


// user actions
- (void)generalPreferences:(NSToolbarItem *)item;
- (void)accountPreferences:(NSToolbarItem *)item;
- (void)syncPreferences:(NSToolbarItem *)item;
- (void)notifyPreferences:(NSToolbarItem *)item;

- (void)setupToolbar;

// NSTableViewDelegates
- (BOOL)selectionShouldChangeInTableView:(NSTableView *)aTableView;

// delegate for error sheet
- (void)changeSelectionResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;

@end
