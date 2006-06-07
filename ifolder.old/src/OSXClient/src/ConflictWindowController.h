/* ConflictWindowController */

#import <Cocoa/Cocoa.h>

@class iFolder;
@class iFolderService;

@interface ConflictWindowController : NSWindowController
{
	NSMutableArray					*conflicts;
	IBOutlet NSArrayController		*ifoldersController;	
	IBOutlet NSTextField			*ifolderName;	
	IBOutlet NSTextField			*ifolderPath;	

	iFolder				*ifolder;
	iFolderService		*ifolderService;	
}
+ (ConflictWindowController *)sharedInstance;

- (IBAction)saveLocal:(id)sender;
- (IBAction)saveServer:(id)sender;
- (IBAction)renameFile:(id)sender;
-(void)resolveFileConflicts:(BOOL)saveLocal;


@end
