/* PropSharingController */

#import <Cocoa/Cocoa.h>

@class iFolderService;
@class User;
@class iFolder;

@interface PropSharingController : NSObject
{
	iFolderService				*ifolderService;
	NSMutableArray				*users;
	NSMutableArray				*foundUsers;
	NSMutableDictionary			*keyedUsers;
	iFolder						*curiFolder;

    IBOutlet NSArrayController	*usersController;
    IBOutlet NSArrayController	*foundUsersController;
    IBOutlet NSTableView		*currentUsers;
    IBOutlet NSTableView		*searchedUsers;
    IBOutlet NSSearchField		*userSearch;
	IBOutlet NSPopUpButton		*defaultAccess;
	IBOutlet NSWindow			*propertiesWindow;
}

- (IBAction)addSelectedUsers:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)removeSelectedUsers:(id)sender;
- (IBAction)searchUsers:(id)sender;

- (void)removeSelectedUsersResponse:(NSWindow *)sheet returnCode:(int)returnCode contextInfo:(void *)contextInfo;
- (void)awakeFromNib;
- (void)addUser:(User *)newUser;

@end
