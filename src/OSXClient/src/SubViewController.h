/* IFSubViewController */

#import <Cocoa/Cocoa.h>

@interface SubViewController : NSObject
{
@private
    IBOutlet NSView *view;
	id		_owner;
}

// This method takes the stores the nib name and the owner of the object.
- (id)initWithNibName:(NSString*)nibName andOwner:(id)owner;

// The nib file's have an outlet on the file's owner connected to the view.  This method 
// returns that view.
- (NSView*)view;

@end
