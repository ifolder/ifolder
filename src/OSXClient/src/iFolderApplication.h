/* iFolderApplication */

#import <Cocoa/Cocoa.h>


@class LoginWindowController;
@class iFolder;


@interface iFolderApplication : NSObject
{
}

//==========================================
// IBAction Methods
//==========================================
- (IBAction)showSyncLog:(id)sender;
- (IBAction)showAboutBox:(id)sender;
- (IBAction)showPrefs:(id)sender;
- (IBAction)showiFolderWindow:(id)sender;



//==========================================
// All other methods
//==========================================
- (void)showLoginWindow:(NSString *)domainID;
- (void)addLog:(NSString *)entry;
- (void)initializeSimiasEvents;



//==========================================
// Thread Safe calls
//==========================================
- (void)addLogTS:(NSString *)entry;
- (void)showLoginWindowTS:(NSString *)domainID;


//==========================================
// NSApplication Delegates
//==========================================
- (void)applicationDidFinishLaunching:(NSNotification*)notification;
- (void)applicationWillTerminate:(NSNotification *)notification;


//==========================================
// Simias startup and shutdown methods
//==========================================
- (void)startSimiasThread:(id)arg;
- (void)simiasDidFinishStarting:(id)arg;





@end
