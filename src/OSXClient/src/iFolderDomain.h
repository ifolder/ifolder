/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

#import <Cocoa/Cocoa.h>
#include "iFolderStub.h"

#define WORKGROUP_DOMAIN @"363051d1-8841-4c7b-a1dd-71abbd0f4ada"


/*
	char *ID;
	char *POBoxID;
	char *Name;
	char *Description;
	char *Host;
	char *UserID;
	char *UserName;
	enum xsd__boolean IsDefault;
	enum xsd__boolean IsSlave;
	enum xsd__boolean IsEnabled;
	enum xsd__boolean IsConnected;
*/

@interface iFolderDomain : NSObject
{
	NSMutableDictionary		*properties;
}


- (NSMutableDictionary *) properties;
- (void) setProperties: (NSDictionary *)newProperties;

-(void) setgSOAPProperties:(struct ns1__DomainWeb *)domainWeb;

-(NSString *)ID;
-(NSString *)name;
-(NSString *)userName;
-(NSString *)host;
-(NSString *)password;
-(NSNumber *)isDefault;
-(NSNumber *)isSlave;
-(NSNumber *)isEnabled;


@end
