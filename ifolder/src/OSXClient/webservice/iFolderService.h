//
//  iFolderService.h
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#ifndef __iFolderService__
#define __iFolderService__

#import <Cocoa/Cocoa.h>
#include <Carbon/Carbon.h>
#import "iFolderDomain.h"
#import "iFolder.h"


@interface iFolderService : NSObject
{
}

-(bool) Ping;
-(NSArray *) GetDomains;
-(iFolderDomain *) ConnectToDomain:(NSString *)UserName usingPassword:(NSString *)Password andHost:(NSString *)Host;
-(NSArray *) GetiFolders;
-(iFolder *) CreateiFolder:(NSString *)Path InDomain:(NSString *)DomainID;

@end

#endif // __iFolderService__