//
//  Domain.h
//  iFolder
//
//  Created by Calvin Gaisford on 12/14/04.
//  Copyright 2004 __MyCompanyName__. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#include "iFolderStub.h"

@interface IFDomain : NSObject
{
	@public
		NSString	*ID;
		NSString	*POBoxID;
		NSString	*Name;
		NSString	*Description;
		NSString	*Host;
		NSString	*UserID;
		NSString	*UserName;
}

-(void) from_gsoap:(struct ns1__DomainWeb *)domainWeb;


@end
