/***********************************************************************
 *  Registry.h - Implements functions to manipulate the registry.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 * 
 ***********************************************************************/

#ifndef _REGISTRY_H_
#define _REGISTRY_H_

#include "CommonUI.h"

#define GUID_SIZE 128
#define MAX_STRING_LENGTH 256

BOOL AddRegNamedValue(LPTSTR pszKey,
					  LPTSTR pszSubkey,
					  LPTSTR pszValueName,
					  LPTSTR pszValue);

BOOL SetRegKeyValue(LPTSTR pszKey,
					LPTSTR pszSubkey,
					LPTSTR pszValue);

#endif // _REGISTRY_H_

