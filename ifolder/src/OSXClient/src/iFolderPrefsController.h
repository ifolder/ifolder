/* iFolderPrefsController */

#import <Cocoa/Cocoa.h>

@interface iFolderPrefsController : NSWindowController
{
    IBOutlet NSView *accountsView;
    IBOutlet NSView *generalView;
	IBOutlet NSView *blankView;
	
	NSToolbar				*toolbar;
	NSMutableDictionary		*toolbarItemDict;	
	NSMutableArray			*toolbarItemArray;	
}

- (void) updateSize:(NSSize)newSize;

// Toobar Delegates
- (NSToolbarItem *)toolbar:(NSToolbar *)toolbar
	itemForItemIdentifier:(NSString *)itemIdentifier
	willBeInsertedIntoToolbar:(BOOL)flag;
- (NSArray *)toolbarAllowedItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarDefaultItemIdentifiers:(NSToolbar *)toolbar;
- (NSArray *)toolbarSelectableItemIdentifiers:(NSToolbar *)toolbar;
- (int)count;

// user actions
- (void)generalPreferences:(NSToolbarItem *)item;
- (void)accountPreferences:(NSToolbarItem *)item;

- (void)setupToolbar;



@end
