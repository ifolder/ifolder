/* PropGeneralController */
/***********************************************************************
 |  $RCSfile$
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 | Author: 
***********************************************************************/

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
