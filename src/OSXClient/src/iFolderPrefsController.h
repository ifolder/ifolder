/* iFolderPrefsController */

#import <Cocoa/Cocoa.h>

@class AccountsController;

@interface iFolderPrefsController : NSWindowController
{
    IBOutlet NSView				*generalView;
    IBOutlet NSView				*accountsView;
    IBOutlet NSView				*notifyView;
    IBOutlet NSView				*syncView;
	IBOutlet NSView				*blankView;
	IBOutlet AccountsController	*accountsController;

	NSToolbar				*toolbar;
	NSMutableDictionary		*toolbarItemDict;	
	NSMutableArray			*toolbarItemArray;
	int						modalReturnCode;
}

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


						
					
@end
