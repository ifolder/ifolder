#import "MCTableView.h"

@implementation MCTableView


- (NSMenu *)menuForEvent:(NSEvent *)theEvent;
{
    // what row are we at?
    int row = [self rowAtPoint: [self convertPoint: [theEvent
							locationInWindow] fromView: nil]];
    if (row != -1)
	{
		if([self isRowSelected:row] == NO)
			[self selectRow: row byExtendingSelection: NO];
    }

    return [super menu]; // use what we've got
}


@end
