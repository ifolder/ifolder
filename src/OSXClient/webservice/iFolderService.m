//
//  iFolderService.m
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import "iFolderService.h"
#include "ifolder.h"

@implementation iFolderService


-(bool) Ping
{
	return is_ifolder_running();
}

@end
