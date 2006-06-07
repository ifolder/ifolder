/* PropGeneralController */

#import <Cocoa/Cocoa.h>

@class iFolderService;
@class iFolder;
@class DiskSpace;
@class VerticalBarView;

@interface PropGeneralController : NSObject
{
	iFolder					*curiFolder;
	iFolderService			*ifolderService;
	DiskSpace				*diskSpace;
	
	long long				prevLimit;	

    IBOutlet NSTextField	*availableSpace;
    IBOutlet NSTextField	*currentSpace;
    IBOutlet NSButton		*enableLimit;
    IBOutlet NSTextField	*filesToSync;
    IBOutlet NSTextField	*lastSync;
    IBOutlet NSTextField	*limitSpace;
    IBOutlet NSTextField	*limitSpaceUnits;
    IBOutlet NSTextField	*syncIntervalLabel;
    IBOutlet NSTextField	*syncInterval;
    IBOutlet NSButton		*syncNow;
	IBOutlet NSWindow		*propertiesWindow;
	IBOutlet NSTextField	*hasConflicts;
	IBOutlet NSImageView	*hasConflictsImage;
	IBOutlet NSTextField	*ownerName;
	IBOutlet NSTextField	*ifolderName;
	IBOutlet VerticalBarView	*barView;
}

- (IBAction)enableLimitToggled:(id)sender;
- (IBAction)syncNow:(id)sender;
- (IBAction)updateLimitValue:(id)sender;

@end
