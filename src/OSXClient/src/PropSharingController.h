/* PropSharingController */

#import <Cocoa/Cocoa.h>

@class iFolderService;
@class User;

@interface PropSharingController : NSObject
{
	iFolderService				*ifolderService;
	NSMutableArray				*users;
	NSMutableArray				*foundUsers;
	NSMutableDictionary			*keyedUsers;
	NSString					*ifolderID;

    IBOutlet NSArrayController	*usersController;
    IBOutlet NSArrayController	*foundUsersController;
    IBOutlet NSTableView		*currentUsers;
    IBOutlet NSTableView		*searchedUsers;
    IBOutlet NSSearchField		*userSearch;
}

- (IBAction)addSelectedUsers:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)removeSelectedUsers:(id)sender;
- (IBAction)searchUsers:(id)sender;

-(void)awakeFromNib;
-(void) addUser:(User *)newUser;

@end
