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
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#include "Registry.h"
#include "CommonUI.h"

BOOL AddRegNamedValue(LPTSTR pszKey,
					  LPTSTR pszSubkey,
					  LPTSTR pszValueName,
					  LPTSTR pszValue)
{
	BOOL bRetVal= FALSE;
	TCHAR szKey[MAX_STRING_LENGTH];
	
	lstrcpyn(szKey, pszKey, MAX_STRING_LENGTH);
	if (NULL != pszSubkey)
	{
		lstrcat(szKey, TEXT("\\"));
		lstrcat(szKey, pszSubkey);
	}

	LONG status;
	HKEY hKey;
	status= RegOpenKeyEx(
		HKEY_CLASSES_ROOT,
		szKey,
		0,
		KEY_ALL_ACCESS,
		&hKey);
	if (NULL != pszValue && ERROR_SUCCESS == status)
	{
		status= RegSetValueEx(
			hKey,
			pszValueName,
			0,
			REG_SZ,
			(BYTE *)pszValue,
			(lstrlen(pszValue)+1)*sizeof(TCHAR));
		if (ERROR_SUCCESS == status)
			bRetVal= TRUE;
		
		RegCloseKey(hKey);
	}

	return bRetVal;
}


BOOL SetRegKeyValue(LPTSTR pszKey,
					LPTSTR pszSubkey,
					LPTSTR pszValue)
{
	BOOL bRetVal= FALSE;
	TCHAR szKey[MAX_STRING_LENGTH];

	lstrcpyn(szKey, pszKey, MAX_STRING_LENGTH);
	if (NULL != pszSubkey)
	{
		lstrcat(szKey, TEXT("\\"));
		lstrcat(szKey, pszSubkey);
	}

	LONG status;
	HKEY hKey;

	status= RegCreateKeyEx(
		HKEY_CLASSES_ROOT,
		szKey,
		0,
		NULL,
		REG_OPTION_NON_VOLATILE,         
		KEY_READ | KEY_WRITE,
		NULL,
		&hKey,
		NULL);
	if (NULL != pszValue && ERROR_SUCCESS == status)
	{
		status= RegSetValueEx(
			hKey,
			NULL,
			0,
			REG_SZ,
			(BYTE *)pszValue,
			(lstrlen(pszValue)+1)*sizeof(TCHAR));
		if (ERROR_SUCCESS == status)
			bRetVal= TRUE;

		RegCloseKey(hKey);
	}

	return bRetVal;
}


