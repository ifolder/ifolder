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
#import "IFDomain.h"


@interface iFolderService : NSObject
{
}

-(bool) Ping;
-(NSMutableDictionary *) GetDomains;
-(IFDomain *) ConnectToDomain:(NSString *)username usingPassword:(NSString *)password andHost:(NSString *)host;

@end

#endif // __iFolderService__