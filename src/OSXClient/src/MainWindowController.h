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
	
	iFolder *selectediFolder;	
}

- (IBAction)showLoginWindow:(id)sender;
- (IBAction)refreshWindow:(id)sender;


- (void)login:(NSString *)username withPassword:(NSString *)password toServer:(NSString *)server;
- (void)createiFolder:(NSString *)path inDomain:(NSString *)domainID;
- (void)AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)domainID toPath:(NSString *)localPath;


- (void)awakeFromNib;

- (void)addDomain:(iFolderDomain *)newDomain;
- (void)addiFolder:(iFolder *)newiFolder;


@end
