/* PropSharingController */

#import <Cocoa/Cocoa.h>

@interface PropSharingController : NSObject
{
    IBOutlet NSTableView *currentUsers;
    IBOutlet NSTableView *searchedUsers;
    IBOutlet NSSearchField *userSearch;
}
- (IBAction)addSelectedUsers:(id)sender;
- (IBAction)refreshWindow:(id)sender;
- (IBAction)removeSelectedUsers:(id)sender;
- (IBAction)searchUsers:(id)sender;
@end
