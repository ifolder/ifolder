/* PreferencesController */

#import <Cocoa/Cocoa.h>

@interface PreferencesController : NSWindowController
{
    IBOutlet NSButton *accAddButton;
    IBOutlet NSTabViewItem *accountsTab;
    IBOutlet NSTableView *accountsView;
    IBOutlet NSButton *accPropButton;
    IBOutlet NSButton *accRemoveButton;
    IBOutlet NSButton *autoSyncButton;
    IBOutlet NSTabViewItem *generalTab;
    IBOutlet NSButton *loginLaunchButton;
    IBOutlet NSButton *notifyButton;
    IBOutlet NSTabView *prefsTabView;
    IBOutlet NSButton *showConfButton;
    IBOutlet NSPopUpButton *syncUnits;
    IBOutlet NSTextField *syncValue;
}
- (IBAction)addAccount:(id)sender;
- (IBAction)removeAccount:(id)sender;
- (IBAction)showProperties:(id)sender;

+ (PreferencesController *)sharedInstance;

- (void)windowWillClose:(NSNotification *)aNotification;


@end
