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

#include "CommonUI.h"
#include "iFolderShell.h"

// Handle to this DLL.
extern HINSTANCE g_hmodThisDll;
extern HMODULE g_hmodResDll;

//
//  FUNCTION: CiFolderShell::QueryContextMenu(HMENU, UINT, UINT, UINT, UINT)
//
//  PURPOSE: Called by the shell just before the context menu is displayed.
//           This is where you add your specific menu items.
//
//  PARAMETERS:
//    hMenu      - Handle to the context menu
//    indexMenu  - Index of where to begin inserting menu items
//    idCmdFirst - Lowest value for new menu ID's
//    idCmtLast  - Highest value for new menu ID's
//    uFlags     - Specifies the context of the menu event
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::QueryContextMenu(HMENU hMenu,
											 UINT indexMenu,
											 UINT idCmdFirst,
											 UINT idCmdLast,
											 UINT uFlags)
{
	OutputDebugString(TEXT("CiFolderShell::QueryContextMenu()\n"));

	UINT idCmd= idCmdFirst;

	TCHAR sziFolderMenu[MAX_MENU_LENGTH];
	TCHAR szCreateiFolderMenu[MAX_MENU_LENGTH];
	TCHAR szDeleteiFolderMenu[MAX_MENU_LENGTH];
	TCHAR sziFolderConflictMenu[MAX_MENU_LENGTH];
	TCHAR sziFolderPropMenu[MAX_MENU_LENGTH];
	TCHAR sziFolderShareMenu[MAX_MENU_LENGTH];
	TCHAR sziFolderHelpMenu[MAX_MENU_LENGTH];
	BOOL bAppendItems= FALSE;

	LoadString(g_hmodResDll, IDS_IFOLDER, sziFolderMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_CONVERT, szCreateiFolderMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_REVERT, szDeleteiFolderMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_RESOLVE, sziFolderConflictMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_PROPERTIES, sziFolderPropMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_SHARE, sziFolderShareMenu, MAX_MENU_LENGTH/sizeof(TCHAR));
	LoadString(g_hmodResDll, IDS_HELP, sziFolderHelpMenu, MAX_MENU_LENGTH/sizeof(TCHAR));

	STGMEDIUM medium;
	FORMATETC fe= {CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL};
	HRESULT hr= m_pDataObj->GetData(&fe, &medium);
	if (FAILED(hr))
	{
		OutputDebugString(TEXT("CiFolderShell::QueryContextMenu() returning E_INVALIDARG\n"));
		return E_INVALIDARG;
	}

	UINT count= DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), -1, NULL, 0);
	// For now, we only allow one item to be selected.
	if (count == 1)
	{
		DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), 0, m_szFileUserClickedOn, MAX_PATH);

		if (((uFlags & 0x000F) == CMF_NORMAL) || (uFlags & CMF_EXPLORE))
		{
//			OutputDebugString(TEXT("CMF_NORMAL...\n"));

			UINT attrs= GetFileAttributes(m_szFileUserClickedOn);
		
			wchar_t lpszRoot[MAX_ROOT_PATH + 1];
			lstrcpyn(lpszRoot, m_szFileUserClickedOn, MAX_ROOT_PATH + 1);

			// Only allow fixed drives to hold iFolders ... also, the root of a drive cannot be an iFolder.
			// This is also handled in the CanBeiFolder call but sometimes we get delays calling across to the
			// web service.
			UINT type = GetDriveType(lpszRoot);
			if ((attrs == INVALID_FILE_ATTRIBUTES) || 
				(lstrcmp(lpszRoot, m_szFileUserClickedOn) == 0) ||
				((type != DRIVE_FIXED) &&
				(type != DRIVE_REMOVABLE)))
			{
				// Error.
				return 0;
			}

			if (m_hBmpMenu)
			{
				DeleteObject(m_hBmpMenu);
				m_hBmpMenu = NULL;
			}

			BOOL biFolder= FALSE;
			VARIANT_BOOL hasConflicts=FALSE;

			try
			{
				if (m_spiFolder == NULL)
				{
					// Instantiate the iFolder smart pointer.
					hr= m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
				}

				if (m_spiFolder)
				{
					biFolder= m_spiFolder->IsiFolder(m_szFileUserClickedOn, &hasConflicts);
				}
			}
			catch (...)
			{
				OutputDebugString(TEXT("Exception caught in CiFolderShell::QueryContextMenu()\n"));
			}

			if (biFolder)
			{
				// Create a submenu.
				HMENU subMenu= CreateMenu();
				int indexSubMenu= 0;

				// Add the item(s) to the submenu.

				// Initialize MENUITEMINFO struct.
				MENUITEMINFO mii;
				mii.cbSize= sizeof(MENUITEMINFO);

				idCmd++;

				// Add the menu item for "Revert" action.
				mii.wID= idCmd;

				// Always allow a user to revert to normal folder.
				mii.fMask= MIIM_ID | MIIM_TYPE;
				mii.fType= MFT_STRING;
				mii.dwTypeData= szDeleteiFolderMenu;
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				idCmd++;

				if (hasConflicts)
				{
					// Add the menu item for resolving conflicts.
					mii.wID= idCmd;
					mii.dwTypeData= sziFolderConflictMenu;
					mii.cch= lstrlen(sziFolderConflictMenu);
					InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);
				}

				idCmd++;

				// Add the menu item for sharing an iFolder.
				mii.wID= idCmd;
				mii.dwTypeData= sziFolderShareMenu;
				mii.cch= lstrlen(sziFolderShareMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				idCmd++;

				// Add the menu item for iFolder properties.
				mii.wID= idCmd;
				mii.dwTypeData= sziFolderPropMenu;
				mii.cch= lstrlen(sziFolderPropMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				idCmd++;

				// Add the menu item for iFolder help.
				mii.wID = idCmd;
				mii.dwTypeData = sziFolderHelpMenu;
				mii.cch = lstrlen(sziFolderHelpMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				// Add a separator before...
				mii.fType= MFT_SEPARATOR;
				mii.fState= MFS_ENABLED;
				InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

				idCmd++;
				// Add the menu item for "iFolder".
				mii.fMask |= MIIM_SUBMENU;
				mii.wID= idCmd;
				mii.fType= MFT_STRING;
				mii.hSubMenu= subMenu;
				mii.dwTypeData= sziFolderMenu;
				mii.cch= lstrlen(sziFolderMenu);

				// Get bitmap
				HBITMAP hBmp = GetBitmap();
				if (hBmp == NULL)
				{
					OutputDebugString(TEXT("Failed to load icon"));
				}
				else
				{
					mii.fMask |= MIIM_CHECKMARKS | MIIM_STATE;
					mii.fState = MFS_UNCHECKED;
					mii.hbmpChecked = 0;
					mii.hbmpUnchecked = hBmp;
				}

				InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

				idCmd++;

				// Add the separator after ...
				mii.fType= MFT_SEPARATOR;
				InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

				OutputDebugString(TEXT("CiFolderShell::QueryContextMenu() returning\n"));
				return ResultFromShort(idCmd-idCmdFirst);
			}

			// Don't allow System directories to become iFolders.
			if ((attrs & FILE_ATTRIBUTE_DIRECTORY) && !(attrs & FILE_ATTRIBUTE_SYSTEM))
			{
				try
				{
					// A directory has been selected ... check to see if it can be an iFolder.
					bAppendItems= m_spiFolder->CanBeiFolder(m_szFileUserClickedOn);
				}
				catch (...) {}
			}
		}
		// TODO: - investigate case in which these flags are used.
//		else if (uFlags & CMF_VERBSONLY)
//		{
//			OutputDebugString(TEXT("CMF_VERBSONLY...\n"));
//		}
//		else if (uFlags & CMF_EXPLORE)
//		{
//			OutputDebugString(TEXT("CMF_EXPLORE...\n"));
//		}
//		else if (uFlags & CMF_DEFAULTONLY)
//		{
//			OutputDebugString(TEXT("CMF_DEFAULTONLY...\n"));
//		}
		else
		{
			TCHAR szTemp[32];
			wsprintf(szTemp, TEXT("uFlags==>%d\n"), uFlags);
			//OutputDebugString(TEXT("CMF_default...\n"));
			//OutputDebugString(szTemp);
		}
	}

	ReleaseStgMedium(&medium);

    if (bAppendItems)
    {	
		// Add the item(s) to the menu.

		// Initialize MENUITEMINFO struct.
		MENUITEMINFO mii;
		mii.cbSize= sizeof(MENUITEMINFO);
		mii.fMask= MIIM_ID | MIIM_TYPE | MIIM_STATE;

		// Add a separator before...
		mii.fType= MFT_SEPARATOR;
		mii.fState= MFS_ENABLED;
		InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

		// Add the menu item for "Convert" action.
		mii.wID= idCmd;
		mii.fType= MFT_STRING;
		mii.dwTypeData= szCreateiFolderMenu;

		// Get bitmap
		HBITMAP hBmp = GetBitmap();
		if (hBmp == NULL)
		{
			OutputDebugString(TEXT("Failed to load icon"));
		}
		else
		{
			mii.fMask |= MIIM_CHECKMARKS | MIIM_STATE;
			mii.fState = MFS_UNCHECKED;
			mii.hbmpChecked = 0;
			mii.hbmpUnchecked = hBmp;
		}

		InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

		idCmd++;

		// Add the separator after ...
		mii.fType= MFT_SEPARATOR;
		InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

		// Return number of menu items we added.
		OutputDebugString(TEXT("CiFolderShell::QueryContextMenu() returning\n"));
		return ResultFromShort(idCmd-idCmdFirst);
   }

	OutputDebugString(TEXT("CiFolderShell::QueryContextMenu() returning NOERROR\n"));
   return NOERROR;

}	/*-- QueryContextMenu() --*/

//
//  FUNCTION: CiFolderShell::InvokeCommand(LPCMINVOKECOMMANDINFO)
//
//  PURPOSE: Called by the shell after the user has selected on of the
//           menu items that was added in QueryContextMenu().
//
//  PARAMETERS:
//    lpcmi - Pointer to an CMINVOKECOMMANDINFO structure
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::InvokeCommand(LPCMINVOKECOMMANDINFO lpcmi)
{
	OutputDebugString(TEXT("CiFolderShell::InvokeCommand()\n"));

    HRESULT hr = E_INVALIDARG;

	// Call into iFolder lib to create/delete iFolder.

    //If HIWORD(lpcmi->lpVerb) then we have been called programmatically
    //and lpVerb is a command that should be invoked.  Otherwise, the shell
    //has called us, and LOWORD(lpcmi->lpVerb) is the menu ID the user has
    //selected.  Actually, it's (menu ID - idCmdFirst) from QueryContextMenu().
    if (!HIWORD(lpcmi->lpVerb))
    {
        UINT idCmd = LOWORD(lpcmi->lpVerb);

        switch (idCmd)
        {
            case 0: // Convert to an iFolder
				try
				{
					// Make this folder an iFolder.
					if(m_spiFolder->CreateiFolder(m_szShellPath, m_szFileUserClickedOn))
					{
						// Tell the shell to refresh the icon...
						SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, m_szFileUserClickedOn, NULL);
						m_spiFolder->NewiFolderWizard(m_szShellPath, m_szFileUserClickedOn);
					}
				}
				catch (...)
				{
					OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

            case 1: // Revert to a normal folder
				try
				{
					TCHAR szTitle[MAX_MENU_LENGTH];
					TCHAR szMessage[MAX_MESSAGE_LENGTH];
					LoadString(g_hmodResDll, IDS_REVERT_PROMPT, szMessage, MAX_MESSAGE_LENGTH/sizeof(TCHAR));
					LoadString(g_hmodResDll, IDS_REVERT_TITLE, szTitle, MAX_MENU_LENGTH/sizeof(TCHAR));

					// Change this iFolder back to a regular folder.
					m_spiFolder->RevertToNormal(m_szFileUserClickedOn);

					// Tell the shell to refresh the icon...
					SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, m_szFileUserClickedOn, NULL);
				}
				catch (...)
				{
					OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

			case 2: // Resolve conflicts
				try
				{
					// Invoke the conflict resolver.
					m_spiFolder->InvokeConflictResolverDlg(m_szShellPath, m_szFileUserClickedOn);
				}
				catch (...)
				{
					OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
				break;

            case 3: // Share with...
				hr= NOERROR;
				try
				{
					// Invoke the iFolder Advanced properties dialog.
					m_spiFolder->InvokeAdvancedDlg(m_szShellPath, m_szFileUserClickedOn, 1, false);
				}
				catch (...)
				{
					OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

            case 4: // Properties
				// Invoke the properties dialog for this folder with the iFolder tab active.
//				SHObjectProperties(lpcmi->hwnd, SHOP_FILEPATH, m_szFileUserClickedOn, TEXT("iFolder"));
				try
				{
					// Invoke the iFolder Advanced properties dialog.
					m_spiFolder->InvokeAdvancedDlg(m_szShellPath, m_szFileUserClickedOn, 0, false);
				}
				catch (...)
				{
					OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
				hr= NOERROR;
                break;
			case 5: // Help...
				// Display the help.
				m_spiFolder->ShowHelp(m_szShellPath, TEXT(""));
				hr = NOERROR;
				break;
        }
    }

	OutputDebugString(TEXT("CiFolderShell::InvokeCommand() returning\n"));
    return hr;

}	/*-- InvokeCommand() --*/


//
//  FUNCTION:	CiFolderShell::InvokeCommand(UINT_PTR, UINT, UINT, LPSTR, UINT)
//
//  PURPOSE:	Called by the shell to retrieve information about a 
//				shortcut menu command, including the help string and 
//				the language-independent, or canonical, name for the command.
//
//  PARAMETERS:
//		idCmd - Menu command identifier offset.
//		uFlags - Flags specifying the information to return. This parameter can have one of the following values.
//			GCS_HELPTEXTA - Sets pszName to an ANSI string containing the help text for the command.
//			GCS_HELPTEXTW - Sets pszName to a Unicode string containing the help text for the command.
//			GCS_VALIDATEA - Returns S_OK if the menu item exists, or S_FALSE otherwise.
//			GCS_VALIDATEW - Returns S_OK if the menu item exists, or S_FALSE otherwise.
//			GCS_VERBA - Sets pszName to an ANSI string containing the language-independent command name for the menu item.
//			GCS_VERBW - Sets pszName to a Unicode string containing the language-independent command name for the menu item.
//		pwReserved - Reserved. Applications must specify NULL when calling this method, and handlers must ignore this parameter when called.
//		pszName - Address of the buffer to receive the null-terminated string being retrieved.
//		cchMax - Size of the buffer, in characters, to receive the null-terminated string.
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::GetCommandString(UINT_PTR idCmd,
											 UINT uFlags,
											 UINT FAR *reserved,
											 LPSTR pszName,
											 UINT cchMax)
{
    //OutputDebugString(TEXT("CiFolderShell::GetCommandString()\n"));

    switch (idCmd)
    {
        case 0:
			LoadString(g_hmodResDll, IDS_CONVERT_DESC, (LPTSTR)pszName, cchMax);
			break;

        case 1:
			LoadString(g_hmodResDll, IDS_REVERT_DESC, (LPTSTR)pszName, cchMax);
			break;

        case 2:
			LoadString(g_hmodResDll, IDS_RESOLVE_DESC, (LPTSTR)pszName, cchMax);
			break;

        case 3:
			LoadString(g_hmodResDll, IDS_SHARE_DESC, (LPTSTR)pszName, cchMax);
			break;

		case 4:
			LoadString(g_hmodResDll, IDS_PROPERTIES_DESC, (LPTSTR)pszName, cchMax);
			break;

		case 5:
			LoadString(g_hmodResDll, IDS_HELP_DESC, (LPTSTR)pszName, cchMax);
			break;
    }

    return NOERROR;

}	/*-- GetCommandString() --*/

HBITMAP CiFolderShell::GetBitmap()
{
	HDC hScreenDC = NULL;
	HDC hDC = NULL;
	HBRUSH hBrush = NULL;
	HICON hIcon = NULL;
    HBITMAP hBmp=NULL;
	if (m_hBmpMenu)
		return m_hBmpMenu;
	
	// Get the device context.
	hScreenDC = CreateDC(TEXT("DISPLAY"), NULL, NULL, NULL);
	hDC = CreateCompatibleDC(hScreenDC);
	if (!hDC)
		goto Cleanup;
	
	// Get the dimensions of the bitmap.
	int x = GetSystemMetrics(SM_CXMENUCHECK);
	int y = GetSystemMetrics(SM_CYMENUCHECK);

	// Create the bitmap.
	hBmp = CreateCompatibleBitmap(hScreenDC, x, y);
	if (!hBmp)
		goto Cleanup;
	
	// Select the bitmap into the device context ... save the old one to replace later.
	HBITMAP hBmpOld = (HBITMAP)SelectObject(hDC, hBmp);

	// Draw the bitmap background
	RECT rect;
	rect.left = 0;
	rect.right = x;
	rect.top = 0;
	rect.bottom = y;
	hBrush = CreateSolidBrush(RGB(255, 255, 255));
	FillRect(hDC, &rect, hBrush);

	// Build the path name to the icon.
	TCHAR szIconName[MAX_PATH];
	lstrcpy(szIconName, m_szShellPath);
	lstrcat(szIconName, TEXT("res\\ifolder_16.ico"));

	// Load the icon
	hIcon = (HICON)LoadImage(g_hmodThisDll, szIconName, IMAGE_ICON, x, y, LR_LOADFROMFILE);
	if (!hIcon)
		goto Cleanup;

	// Draw the icon.
	DrawIconEx(hDC, (x - y) / 2, 0, hIcon, y, y, 0, NULL, DI_NORMAL);

	// Restore the object to the device context.
	SelectObject(hDC, hBmpOld);

	// Save the bitmap so that we can delete it later.
	m_hBmpMenu = hBmp;
   
Cleanup:
	if (hScreenDC)
		DeleteDC(hScreenDC);
	if (hBrush)
		DeleteObject(hBrush);
	if (hDC)
		DeleteDC(hDC);
	if (hIcon)
		DestroyIcon(hIcon);
	
	return hBmp;
}
