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

#ifndef __iFolderService__
#define __iFolderService__

#import <Cocoa/Cocoa.h>
#include <Carbon/Carbon.h>
#import "iFolder.h"


@interface iFolderService : NSObject
{
}

-(bool) Ping;
-(NSArray *) GetiFolders;
-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID;
-(iFolder *) AcceptiFolderInvitation:(NSString *)iFolderID InDomain:(NSString *)DomainID toPath:(NSString *)localPath;
-(void) DeleteiFolder:(NSString *)iFolderID;


@end

#endif // __iFolderService__