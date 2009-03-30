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

#include "applog.h"

void iFolderExceptionLog(NSString *methodLocation, NSException *ex)
{
	NSString *name;
	NSString *reason;
	if(ex != nil)
	{
		name = [ex name];
		if(name == nil)
			name = @"nil";

		reason = [ex reason];
		if(reason == nil)
			reason = @"nil";

		ifconlog4(@"Exception in %@: %@ %@", methodLocation, [ex name], [ex reason]);
	}
	else
		ifconlog2(@"A nil exception was thrown in %@", methodLocation);
}
