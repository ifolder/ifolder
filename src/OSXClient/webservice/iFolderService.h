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
#import "Domain.h"


@interface iFolderService : NSObject
{
}

-(NSMutableDictionary *) GetDomains;
-(Domain*) ConnectToDomain:(NSString*) in_UserName in_Password:(NSString*) in_Password in_Host:(NSString*) in_Host;

@end

#endif // __iFolderService__