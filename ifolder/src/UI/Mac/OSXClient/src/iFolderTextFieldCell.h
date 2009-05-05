/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Satyam <ssutapalli@novell.com> 
*-----------------------------------------------------------------------------
* This module is used to:
*              From IB, we can align the text horizontally only, but inorder to
* align it vertically when icon size in first colomn grows bigger this class is
* needed.
*              To use this class: Assign the instance of this class to data text
* cell of the table coloumn
*
*******************************************************************************/

#import <Cocoa/Cocoa.h>


@interface iFolderTextFieldCell : NSTextFieldCell {
	BOOL isEditOrSelect;
}

@end
