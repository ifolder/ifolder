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

// Handle to this DLL.
extern HINSTANCE g_hmodThisDll;


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
//    OutputDebugString(TEXT("CiFolderShell::QueryContextMenu()\n"));

    UINT idCmd= idCmdFirst;
	// TODO - get these from a resource file.
    TCHAR szCreateiFolderMenu[]= TEXT("Convert to an iFolder");
    TCHAR szDeleteiFolderMenu[]= TEXT("Revert to a Normal Folder");
	TCHAR sziFolderMenu[]= TEXT("iFolder");
	TCHAR sziFolderPropMenu[]= TEXT("Properties...");
	TCHAR sziFolderShareMenu[]= TEXT("Share with...");
	TCHAR sziFolderHelpMenu[] = TEXT("Help...");
    BOOL bAppendItems= FALSE;

	STGMEDIUM medium;
	// TODO - should we use pidl's instead?
	FORMATETC fe= {CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL};
	HRESULT hr= m_pDataObj->GetData(&fe, &medium);
	if (FAILED(hr))
		return E_INVALIDARG;

	UINT count= DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), -1, NULL, 0);
	// TODO - for now, we only allow one item to be selected.
	if (count == 1)
	{
		DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), 0, m_szFileUserClickedOn, MAX_PATH);

		if (((uFlags & 0x000F) == CMF_NORMAL) || (uFlags & CMF_EXPLORE))
		{
//			OutputDebugString(TEXT("CMF_NORMAL...\n"));

			UINT attrs= GetFileAttributes(m_szFileUserClickedOn);
		
			wchar_t lpszRoot[MAX_ROOT_PATH + 1];
			lstrcpyn(lpszRoot, m_szFileUserClickedOn, MAX_ROOT_PATH + 1);

			if ((attrs == INVALID_FILE_ATTRIBUTES) || (GetDriveType(lpszRoot) & DRIVE_REMOTE))
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

			try
			{
				if (m_spiFolder == NULL)
				{
					// Instantiate the iFolder smart pointer.
					hr= m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
				}

				if (m_spiFolder)
				{
					VARIANT_BOOL hasConflicts;
					biFolder= m_spiFolder->IsiFolder(m_szFileUserClickedOn, &hasConflicts);
				}
			}
			catch (...)
			{
//				OutputDebugString(TEXT("Exception caught in CiFolderShell::QueryContextMenu()\n"));
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
				mii.fMask= MIIM_ID | MIIM_TYPE;

				// Add the menu item for sharing an iFolder.
				mii.wID= idCmd;
				mii.fType= MFT_STRING;
				mii.dwTypeData= sziFolderShareMenu;
				mii.cch= lstrlen(sziFolderShareMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				idCmd++;

				// Add the menu item for iFolder properties.
				mii.wID= idCmd;
				mii.fType= MFT_STRING;
				mii.dwTypeData= sziFolderPropMenu;
				mii.cch= lstrlen(sziFolderPropMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				idCmd++;

				// Add the menu item for iFolder help.
				mii.wID = idCmd;
				mii.fType = MFT_STRING;
				mii.dwTypeData = sziFolderHelpMenu;
				mii.cch = lstrlen(sziFolderHelpMenu);
				InsertMenuItem(subMenu, indexSubMenu++, TRUE, &mii);

				// Add a separator before...
				mii.fType= MFT_SEPARATOR;
				mii.fState= MFS_ENABLED;
				InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

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

				return ResultFromShort(idCmd-idCmdFirst);
			}

			if (attrs & FILE_ATTRIBUTE_DIRECTORY)
			{
				// A directory has been selected ... display the menus.
				bAppendItems= m_spiFolder->CanBeiFolder(m_szFileUserClickedOn);
			}
		}
		// TODO - investigate case in which these flags are used.
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

		// TODO - need to work this out ... for now we leave space for two
		// menu items so that we can change between create and remove (even
		// though we only added one menu item).
		idCmd += 2;

		// Add the separator after ...
		mii.fType= MFT_SEPARATOR;
		InsertMenuItem(hMenu, indexMenu++, TRUE, &mii);

		// Return number of menu items we added.
		return ResultFromShort(idCmd-idCmdFirst);
   }

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
//    OutputDebugString(TEXT("CiFolderShell::InvokeCommand()\n"));

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
            case 0:
				try
				{
					// Make this folder an iFolder.
					if(m_spiFolder->CreateiFolder(m_szFileUserClickedOn))
					{
						// Tell the shell to refresh the icon...
						SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, m_szFileUserClickedOn, NULL);
						m_spiFolder->NewiFolderWizard(m_szShellPath, m_szFileUserClickedOn);
					}
				}
				catch (...)
				{
					//OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

            case 1:
				try
				{
					// Change this iFolder back to a regular folder.
					m_spiFolder->DeleteiFolder(m_szFileUserClickedOn);

					// Tell the shell to refresh the icon...
					SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, m_szFileUserClickedOn, NULL);
				}
				catch (...)
				{
					//OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

            case 2:
				hr= NOERROR;
				try
				{
					// Invoke the iFolder Advanced properties dialog.
					m_spiFolder->InvokeAdvancedDlg(m_szShellPath, m_szFileUserClickedOn, 1, false);
				}
				catch (...)
				{
					//OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
                break;

            case 3:
				// Invoke the properties dialog for this folder with the iFolder tab active.
//				SHObjectProperties(lpcmi->hwnd, SHOP_FILEPATH, m_szFileUserClickedOn, TEXT("iFolder"));
				try
				{
					// Invoke the iFolder Advanced properties dialog.
					m_spiFolder->InvokeAdvancedDlg(m_szShellPath, m_szFileUserClickedOn, 0, false);
				}
				catch (...)
				{
					//OutputDebugString(TEXT("Exception caught in CiFolderShell::InvokeCommand()\n"));
				}
				hr= NOERROR;
                break;
			case 4:
				// Display the help.
				m_spiFolder->ShowHelp(m_szShellPath);
				hr = NOERROR;
				break;
        }
    }

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
//    OutputDebugString(TEXT("CiFolderShell::GetCommandString()\n"));

	// TODO - Do xA and xW work ... and display the correct strings.  Also, retrieve strings from resource file.
    switch (idCmd)
    {
        case 0:
            lstrcpyn((LPTSTR)pszName, TEXT("Make this folder an iFolder"), cchMax);
            break;

        case 1:
            lstrcpyn((LPTSTR)pszName, TEXT("Change this iFolder back to a normal folder"), cchMax);
            break;

        case 2:
            lstrcpyn((LPTSTR)pszName, TEXT("iFolder menu item 3"), cchMax);
            break;

        case 3:
            lstrcpyn((LPTSTR)pszName, TEXT("iFolder menu item 4"), cchMax);
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
	HBITMAP hBmp = CreateCompatibleBitmap(hScreenDC, x, y);
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
	lstrcat(szIconName, TEXT("res\\ifolder_loaded.ico"));

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