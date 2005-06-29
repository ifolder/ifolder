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

#define	TheConsoleLogSwitch	( 1 )

#if TheConsoleLogSwitch
#define ifconlog1(x)		NSLog(x)
#define ifconlog2(x,y)		NSLog(x,y)
#define ifconlog3(x,y,z)	NSLog(x,y,z)
#define ifconlog4(w,x,y,z)	NSLog(w,x,y,z)
#define ifexconlog(x,y)		iFolderExceptionLog(x,y)
#else
#define ifconlog1(x)
#define ifconlog2(x,y)
#define ifconlog3(x,y,z)
#define ifconlog4(w,x,y,z)
#define ifexconlog(x,y)
#endif

void iFolderExceptionLog(NSString *methodLocation, NSException *ex);
