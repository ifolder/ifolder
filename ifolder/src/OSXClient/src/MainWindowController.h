/* MainWindowController */

#import <Cocoa/Cocoa.h>
#import <iFolderService.h>


// Forward Declarations
@class LoginWindowController;

@interface MainWindowController : NSWindowController
{
	LoginWindowController			*loginController;
	iFolderService					*webService;
	NSMutableArray					*domains;
	NSMutableArray					*ifolders;	
    IBOutlet NSArrayController		*ifoldersController;
    IBOutlet NSArrayController		*domainsController;
}

- (IBAction)showLoginWindow:(id)sender;

- (void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;
- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;

- (void)awakeFromNib;

- (void)addDomain:(iFolderDomain *)newDomain;
- (void)addiFolder:(iFolder *)newiFolder;


@end
