/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Johnny Jacob <johnnyjacob@gmail.com>
*
*******************************************************************************/

#import "NotificationPrefsController.h"
#import "iFolderApplication.h"

#include <Foundation/Foundation.h>
#include <Foundation/NSLocale.h>

#define KEYCOLUMNINDEX 0
#define DESCRIPTIONCOLUMNINDEX 1

@implementation NotificationPrefsController

NSMutableDictionary *notifyTableViewSource;

-(void)awakeFromNib
{
    id descriptionAndKey;
    NSArray *notifyDescriptions;
    unsigned count, index;

    // Generate the data source for Notification preferences TableView. 
    // notifyDescriptions merges NSUserDefaults keys with description strings.
    notifyDescriptions = [NSArray arrayWithObjects:
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYIFOLDERS,
					   NSLocalizedString(@"When iFolders are shared",
							     @"Display notification when iFolders are shared")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYCOLL,
					   NSLocalizedString(@"When conflicts arise",
							     @"Display notification when conflicts arise")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYUSER,
					   NSLocalizedString(@"When user joins the iFolder domain",
							     @"Display notification when user joins the iFolder domain")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYBYINDEX,
					   NSLocalizedString(@"When new iFolder is created",
							     @"Display notification when new iFolder is created")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYQUOTAVIOLATION,
					   NSLocalizedString(@"When quota policy is violated", 
							     @"Display notification when quota policy is violated")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYSIZEVIOLATION, 
					   NSLocalizedString(@"When file size policy is violated", 
							     @"Display notification when file size policy is violated")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYEXCLUDEFILE, 
					   NSLocalizedString(@"When file exclusion policy is violated",
							     @"Display notification when file exclusion policy is violated")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYDISKFULL,
					   NSLocalizedString(@"When disk is full", 
							     @"Display notification when disk is full")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYPERMISSIONDENIED, 
					   NSLocalizedString(@"When required permissions are unavailable", 
							     @"Display notification When required permissions are unavailable")],
				  [NSArray arrayWithObjects:PREFKEY_NOTIFYPATHLENGTHEXCEEDS, 
					   NSLocalizedString(@"When file path exceeds optimal limit",
							     @"Display notification when file path exceeds optimal limit")],
				  nil];

    notifyTableViewSource = [[NSMutableDictionary alloc] init];
    count = [notifyDescriptions count];

    for (index = 0; index < count; index++) {
      descriptionAndKey = [notifyDescriptions objectAtIndex:index];
      [notifyTableViewSource setObject:descriptionAndKey forKey:[NSNumber numberWithInt:index]];      
    }

    //Workaround for a possible bug in cocoa. The table doesn't get rendered.
    [notificationPrefsTable reloadData];
}

-(void) dealloc
{
  [super dealloc];
}

// Delegates for TableView
//========================================================================
// numberOfRowsInTableView
// Get the number of rows in tableView
//========================================================================
-(int)numberOfRowsInTableView:(NSTableView *)aTableView
{
  return [notifyTableViewSource count];
}

//========================================================================
// tableView
// Get the object at row mentioned
//========================================================================
-(id)tableView:(NSTableView *)aTableView objectValueForTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
  NSArray *descriptionAndKey;
  NSString *key;

  descriptionAndKey = [notifyTableViewSource objectForKey:[NSNumber numberWithInt:rowIndex]]; 

  if (aTableColumn == notificationDescriptionColumn)
    return [descriptionAndKey objectAtIndex:DESCRIPTIONCOLUMNINDEX];
  else if (aTableColumn == notificationEnabledColumn) {
    key = [descriptionAndKey objectAtIndex:KEYCOLUMNINDEX];
    return [NSNumber numberWithBool:[[NSUserDefaults standardUserDefaults] boolForKey:key]];
  }
  return nil;
}

//========================================================================
// tableView
// Set the object in table view at column and row specified
//========================================================================
-(void)tableView:(NSTableView *)aTableView setObjectValue:(id)anObject forTableColumn:(NSTableColumn *)aTableColumn row:(int)rowIndex
{
   NSArray *descriptionAndKey;
   NSString *descriptionKey;
   BOOL state;

   descriptionAndKey = [notifyTableViewSource objectForKey:[NSNumber numberWithInt:rowIndex]]; 
   descriptionKey = [descriptionAndKey objectAtIndex:KEYCOLUMNINDEX];
   state = [[NSUserDefaults standardUserDefaults] boolForKey:descriptionKey];

   //Respond to Enabled column only as it is editable. Preserve the value in the model. 
   //SwitchButton responds to this through objectValueForTableColumn.
   if (aTableColumn == notificationEnabledColumn)
       [[NSUserDefaults standardUserDefaults] setBool:!state forKey:descriptionKey];     
}

@end
