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
	enum ns1__StatusCodes {	ns1__StatusCodes__Success = 0, 
							ns1__StatusCodes__SuccessInGrace = 1, 
							ns1__StatusCodes__UnknownUser = 2, 
							ns1__StatusCodes__AmbiguousUser = 3, 
							ns1__StatusCodes__InvalidCredentials = 4, 
							ns1__StatusCodes__InvalidPassword = 5, 
							ns1__StatusCodes__AccountDisabled = 6, 
							ns1__StatusCodes__AccountLockout = 7, 
							ns1__StatusCodes__UnknownDomain = 8, 
							ns1__StatusCodes__InternalException = 9, 
							ns1__StatusCodes__MethodNotSupported = 10, 
							ns1__StatusCodes__Timeout = 11, 
							ns1__StatusCodes__Unknown = 12};
*/

@interface AuthStatus : NSObject
{
	NSMutableDictionary * properties;
}

-(NSMutableDictionary *) properties;
-(void) setProperties: (NSDictionary *)newProperties;

-(NSNumber *)statusCode;
-(int)totalGraceLogins;
-(int)remainingGraceLogins;

@end
