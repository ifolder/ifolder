/* MainWindowController */

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>
#import "IFFoldersView.h"

@class LoginWindowController;  // Forward declaration

@interface MainWindowController : NSWindowController
{
    IBOutlet NSTabView		*ifolderTabView;
	IFFoldersView			*iFolderView;


	LoginWindowController	*_loginController;
	iFolderService			*webService;
	NSMutableDictionary		*domains;

}

-(void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;

- (void)showLoginWindow;

- (IBAction)login:(id)sender;

// This is called whent he nib is done loading.
- (void)awakeFromNib;

// This delegate method is called when a tab is selected.  This allows us to insert our own views
// for each tab.
- (void)tabView:(NSTabView*)tabView didSelectTabViewItem:(NSTabViewItem*)tabViewItem;

// This method returns the view from the nib specified.
-(NSView*)loadViewFromNib:(NSString*)nibName;



@end
