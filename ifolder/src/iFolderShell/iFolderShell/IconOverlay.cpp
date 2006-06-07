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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

#include "CommonUI.h"
#include "iFolderShell.h"

// Defined in iFolderShell.cpp
extern HINSTANCE g_hmodThisDll;

// static members
TCHAR CiFolderShell::m_szShellPath[MAX_PATH];

//
//  FUNCTION: CiFolderShell::IsMemberOf(LPCWSTR, DWORD)
//
//  PURPOSE: Specifies whether an icon overlay should be added to a Shell object's icon.
//
//  PARAMETERS:
//    pwszPath   - Unicode string that contains the fully qualified path of the Shell object. 
//    dwAttrib   - Object's attributes.
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::IsMemberOf(LPCWSTR pwszPath,
									   DWORD dwAttrib)
{
	OutputDebugString(TEXT("CiFolderShell::IsMemberOf()\n"));
	OutputDebugString(TEXT("\tpwszPath = "));
	OutputDebugString(pwszPath);

	wchar_t lpszRoot[MAX_ROOT_PATH + 1];

	lstrcpyn(lpszRoot, pwszPath, MAX_ROOT_PATH + 1);

	UINT type = GetDriveType(lpszRoot);
	if ((dwAttrib & FILE_ATTRIBUTE_DIRECTORY) && 
		((type == DRIVE_FIXED) || (type == DRIVE_REMOVABLE)))
	{
		VARIANT_BOOL hasConflicts;
		try
		{
			if (m_spiFolder == NULL)
			{
				// Instantiate the iFolder smart pointer.
				m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
			}

			if (m_spiFolder)
			{
				// Call into iFolder lib to see if this item is an iFolder.
				BOOL isiFolder = m_spiFolder->IsiFolder((LPWSTR)pwszPath, &hasConflicts);
				switch (m_iFolderClass)
				{
				case IFOLDER_ISIFOLDER:
					if (isiFolder && !hasConflicts)
					{
						OutputDebugString(TEXT("\nCiFolderShell::IsMemberOf() - returning S_OK\n"));
						return S_OK;
					}
					break;
				case IFOLDER_CONFLICT:
					if (isiFolder && hasConflicts)
					{
						OutputDebugString(TEXT("\nCiFolderShell::IsMemberOf() - returning S_OK\n"));
						return S_OK;
					}
				}
			}
		}
		catch (...)
		{
			OutputDebugString(TEXT("\nException caught in CiFolderShell::IsMemberOf()\n"));
		}
	}

	OutputDebugString(TEXT("\nCiFolderShell::IsMemberOf() - returning S_FALSE\n"));
	return S_FALSE;
}	/*-- IsMemberOf() --*/

//
//  FUNCTION: CiFolderShell::GetOverlayInfo(LPWSTR, int, int, DWORD)
//
//  PURPOSE: Provides the location of the icon overlay's bitmap.
//
//  PARAMETERS:
//    pwszIconFile - [out] Null-terminated Unicode string that 
//			contains the fully qualified path of the file containing
//			the icon. The .dll, .exe, and .ico file types are all 
//			acceptable. You must set the ISIOI_ICONFILE flag in pdwFlags
//			if you return a file name.
//    cchMax - [in] Size of the pwszIconFile buffer, in Unicode characters.
//    pIndex - [out] Address of the index of the icon in a file containing 
//			multiple icons. You must set the ISIOI_ICONINDEX flag in 
//			pdwFlags if you return an index.
//    pdwFlags - Address of a flag that specifies what information is being
//			returned. This parameter can be one or both of the following values.
//				ISIOI_ICONFILE - The path of the icon file is returned through 
//					pwszIconFile.
//				ISIOI_ICONINDEX - There is more than one icon in pwszIconFile. 
//					The icon's index is returned through pIndex.
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::GetOverlayInfo(LPWSTR pwszIconFile,
										   int cchMax,
										   int *pIndex,
										   DWORD *pdwFlags)
{
	OutputDebugString(TEXT("CiFolderShell::GetOverlayInfo()\n"));

	if(IsBadWritePtr(pIndex, sizeof(int)))
	{
		OutputDebugString(TEXT("CiFolderShell::GetOverlayInfo() returning E_INVALIDARG\n"));
		return E_INVALIDARG;
	}
	
	if(IsBadWritePtr(pdwFlags, sizeof(DWORD)))
	{
		OutputDebugString(TEXT("CiFolderShell::GetOverlayInfo() returning E_INVALIDARG\n"));
		return E_INVALIDARG;
	}

	// Get the path where the shell extension is running.
	TCHAR szModule[MAX_PATH];
	GetModuleFileName(g_hmodThisDll, szModule, MAX_PATH);

	int index= lstrlen(szModule) - 1;
	while (szModule[index] != '\\') index--;

	szModule[++index]= '\0';

	// Save this path.
	lstrcpy(m_szShellPath, szModule);

	switch (m_iFolderClass)
	{
	case IFOLDER_ISIFOLDER:
        lstrcat(szModule, TEXT("ifolder_emblem.ico"));
		break;
	case IFOLDER_CONFLICT:
		lstrcat(szModule, TEXT("ifolder_conflict_emb.ico"));
		break;
	}

	lstrcpyn(pwszIconFile, szModule, cchMax);

	*pIndex = 0;
	*pdwFlags = ISIOI_ICONFILE;

	OutputDebugString(TEXT("CiFolderShell::GetOverlayInfo() returning S_OK\n"));
	return S_OK;
}	/*-- GetOverlayInfo() --*/

//
//  FUNCTION: CiFolderShell::GetPriority(int)
//
//  PURPOSE: Specifies the priority of an icon overlay.
//
//  PARAMETERS:
//    pIPriority - [out] Address of a value that indicates the 
//			priority of the overlay identifier. Possible values
//			range from zero to 100, with zero the highest priority. 
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::GetPriority(int *pIPriority)
{
    //OutputDebugString(TEXT("CiFolderShell::GetPriority()\n"));

	// For now, all the icon overlays are set to highest priority.

	switch (m_iFolderClass)
	{
	case IFOLDER_ISIFOLDER:
		*pIPriority = 10;
		break;
	case IFOLDER_CONFLICT:
		*pIPriority = 0;
		break;
	}

	return S_OK;
}	/*-- GetPriority() --*/

