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

#ifndef __SimiasService__
#define __SimiasService__

#import <Cocoa/Cocoa.h>
#include <Carbon/Carbon.h>
#import "iFolderDomain.h"


@interface SimiasService : NSObject
{
	NSString	*simiasURL;
}

-(NSArray *) GetDomains:(BOOL)onlySlaves;
-(iFolderDomain *) ConnectToDomain:(NSString *)UserName usingPassword:(NSString *)Password andHost:(NSString *)Host;
-(void) LeaveDomain:(NSString *)domainID withOption:(BOOL)localOnly;
-(BOOL) ValidCredentials:(NSString *)domainID forUser:(NSString *)userID;
-(void) SetDomainPassword:(NSString *)domainID password:(NSString *)password;
-(NSString *) GetDomainPassword:(NSString *)domainID;
-(void) SetDomainActive:(NSString *)domainID;
-(void) SetDomainInactive:(NSString *)domainID;
-(void) SetDefaultDomain:(NSString *)domainID;

-(void) LoginToRemoteDomain:(NSString *)domainID usingPassword:(NSString *)password;


@end

#endif // __SimiasService__