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
 |
 |  Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>

/*
	char *Name;
	char *UserID;
	char *Rights;
	char *ID;
	char *State;
	char *iFolderID;
	enum xsd__boolean IsOwner;
	char *FirstName;
	char *Surname;
	char *FN;
*/

@interface User : NSObject
{
	NSMutableDictionary * properties;
	NSImage				* icon;
}

-(NSMutableDictionary *) properties;
-(void) setProperties: (NSDictionary *)newProperties;

-(NSString *) UserID;
-(NSString *) FN;
-(NSString *) Name;
-(NSString *) FirstName;
-(NSString *) Surname;
-(BOOL)isOwner;
-(void)setRights:(NSString *)rights;
-(void)setIsOwner:(BOOL)isOwner;

-(void) updateDisplayInformation;

@end
