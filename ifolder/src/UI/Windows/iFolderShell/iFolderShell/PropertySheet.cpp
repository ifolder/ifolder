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
#include "resource.h"


// Reference count of this DLL.
extern UINT g_cRefThisDll;
// Handle to this DLL.
extern HINSTANCE g_hmodThisDll;

extern INT_PTR CALLBACK 
iFolderPermissionsPageDlgProc(HWND hDlg,
							  UINT uMessage,
							  WPARAM wParam,
							  LPARAM lParam);

//
//  FUNCTION: iFolderPageDlgProc(HWND, UINT, WPARAM, LPARAM)
//
//  PURPOSE: Callback dialog procedure for the property page
//
//  PARAMETERS:
//    hDlg      - Dialog box window handle
//    uMessage  - current message
//    wParam    - depends on message
//    lParam    - depends on message
//
//  RETURN VALUE:
//
//    Depends on message.  In general, return TRUE if we process it.
//
//  COMMENTS:
//


//The return type for this function needs to be a INT_PTR for compatibility with Win64.
// See defintion of DLGPROC in WinUser.H

INT_PTR CALLBACK iFolderPageDlgProc(HWND hDlg,
									UINT uMessage,
									WPARAM wParam,
									LPARAM lParam)
{
	LPPROPSHEETPAGE psp=(LPPROPSHEETPAGE)GetWindowLongPtr(hDlg, DWLP_USER);
//	LPCIFOLDERSHELL lpciFolderShell;
	
	switch (uMessage)
	{
		//
		// When the shell creates a dialog box for a property sheet page,
		// it passes the pointer to the PROPSHEETPAGE data structure as
		// lParam. The dialog procedures of extensions typically store it
		// in the DWLP_USER of the dialog box window.
		//
	case WM_INITDIALOG:
/*		SetWindowLongPtr(hDlg, DWLP_USER, lParam);
		psp = (LPPROPSHEETPAGE)lParam;
		lpciFolderShell = (LPCIFOLDERSHELL)psp->lParam;

		HWND hwndLV;
		hwndLV= GetDlgItem(hDlg, IDC_PROPLIST);

		RECT rect;
		GetClientRect(hwndLV, &rect);

		TCHAR szColumnTitle[256];
		LVCOLUMN lvc;

		//Initialized the LV_COLUMN structure
		lvc.mask = LVCF_FMT | LVCF_WIDTH | LVCF_TEXT| LVCF_SUBITEM;
		lvc.fmt = LVCFMT_LEFT;
		lvc.pszText = szColumnTitle;

		//Add the columns
		lvc.iSubItem = 0;
		lvc.cx = rect.right / 3;
		lstrcpy(szColumnTitle, TEXT("Name"));
		ListView_InsertColumn(hwndLV, 0, &lvc);

		lvc.iSubItem= 1;
		lvc.cx = rect.right * 2 / 3;
		lstrcpy(szColumnTitle, TEXT("Value"));
		ListView_InsertColumn(hwndLV, 1, &lvc);

		try
		{
			if (lpciFolderShell->m_spiFolder == NULL)
			{
				// Instantiate the iFolder smart pointer.
				lpciFolderShell->m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
			}

			if (lpciFolderShell->m_spiFolder)
			{
				if (lpciFolderShell->m_spiFolder->GetiFolderNode(lpciFolderShell->m_szFileUserClickedOn))
				{
					if (lpciFolderShell->m_spiFolder->GetiFolderPropInit())
					{
						BSTR name, value;
						LVITEM lvi;
						lvi.mask= LVIF_TEXT;
						lvi.iSubItem= 0;
						int i= 0;

						while (lpciFolderShell->m_spiFolder->GetNextiFolderProp(&name, &value))
						{
							if (lstrcmp(name, TEXT("Description")) == 0)
							{
								// This is the description ... set it in the Description edit box.
								SendMessage(GetDlgItem(hDlg, IDC_DESCRIPTION), (UINT)WM_SETTEXT, 0, (LPARAM)value);
							}
							else
							{
								lvi.iItem= i;
								lvi.pszText= name;

								if (ListView_InsertItem(hwndLV, &lvi) != -1)
								{
									ListView_SetItemText(hwndLV, i, 1, value);
									i++;
								}
							}

							SysFreeString(name);
							SysFreeString(value);
						}
					}
				}
			}
		}
		catch (...)
		{
		}*/
		break;
	case WM_DESTROY:
		break;
	
	case WM_COMMAND:
		int resourceId;
		resourceId= (int)LOWORD(wParam);
		switch (HIWORD(wParam))
		{
		case EN_CHANGE:
			if (resourceId == IDC_DESCRIPTION)
			{
				SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
			}
			break;
		case BN_CLICKED:
			if (resourceId == IDC_ADVANCED)
			{
/*				lpciFolderShell= (LPCIFOLDERSHELL)psp->lParam;
				try
				{
					if (lpciFolderShell->m_spiFolder == NULL)
					{
						// Instantiate the iFolder smart pointer.
						lpciFolderShell->m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
					}

					if (lpciFolderShell->m_spiFolder)
					{
						lpciFolderShell->m_spiFolder->InvokeAdvancedDlg(lpciFolderShell->m_szShellPath, lpciFolderShell->m_szFileUserClickedOn, NULL, true);
					}
				}
				catch (...)
				{
					// TODO - Generate an error message.
				}*/
			}
			break;
		}
		break;

	case WM_NOTIFY:
		switch (((NMHDR FAR *)lParam)->code)
		{
		case PSN_SETACTIVE:
			break;
		case PSN_APPLY:
			//User has clicked the OK or Apply button ...
			try
			{
/*				int length;
				length= SendMessage(GetDlgItem(hDlg, IDC_DESCRIPTION), (UINT)WM_GETTEXTLENGTH, 0, 0); 
				LPTSTR lpszDesc= (LPTSTR)LocalAlloc(LPTR, (length + 1) * sizeof(TCHAR));
				SendMessage(GetDlgItem(hDlg, IDC_DESCRIPTION), (UINT)WM_GETTEXT, (WPARAM)(length + 1), (LPARAM)lpszDesc);

				BSTR strDesc= SysAllocStringLen(lpszDesc, (lstrlen(lpszDesc) + 1) * sizeof(TCHAR));

				lpciFolderShell= (LPCIFOLDERSHELL)psp->lParam;
				lpciFolderShell->m_spiFolder->put_Description(strDesc);

				SysFreeString(strDesc);
				LocalFree(lpszDesc);*/
			}
			catch (...)
			{
			}
			break;
		
		default:
			break;
		}
		
		break;
	default:
		return FALSE;
	}
	
	return TRUE;

}	/*-- iFolderPageDlgProc() --*/


//
//  FUNCTION: CiFolderShell::AddPages(LPFNADDPROPSHEETPAGE, LPARAM)
//
//  PURPOSE: Adds one or more pages to a property sheet that the Shell displays 
//			for a file object. When it is about to display the property sheet, 
//			the Shell calls this method for each property sheet handler registered
//			to the file type. 
//
//  PARAMETERS:
//    lpfnAddPage - Address of a function that the property sheet handler calls to 
//			add a page to the property sheet. The function takes a property sheet 
//			handle returned by the CreatePropertySheetPage function and the lParam 
//			parameter passed to the AddPages method. 
//    lParam - Parameter to pass to the function specified by the lpfnAddPage method. 
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::AddPages(LPFNADDPROPSHEETPAGE lpfnAddPage, LPARAM lParam)
{
    //OutputDebugString(TEXT("CiFolderShell::AddPages()\n"));

	return E_NOTIMPL;

/*	STGMEDIUM medium;
	FORMATETC fe= {CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL};
	HRESULT hr= m_pDataObj->GetData(&fe, &medium);
	if (SUCCEEDED(hr))
	{
		UINT count= DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), -1, NULL, 0);
		// For now, we only allow one item to be selected.
		if (count == 1)
		{
			DragQueryFile(reinterpret_cast<HDROP>(medium.hGlobal), 0, m_szFileUserClickedOn, MAX_PATH);

			BOOL bDispPropPage= FALSE;
			BOOL bDispSharePage= FALSE;
	
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
					bDispPropPage= m_spiFolder->IsiFolderNode(m_szFileUserClickedOn);
					bDispSharePage= m_spiFolder->IsiFolder(m_szFileUserClickedOn, &hasConflicts);
				}
			}
			catch (...)
			{
				//OutputDebugString(TEXT("Exception caught in CiFolderShell::AddPages()\n"));
			}

			if (bDispPropPage || bDispSharePage)
			{
				//
				// Create a property sheet page object from a dialog box.
				//
				// We store a pointer to our class in the psp.lParam, so we
				// can access our class members from within the iFolderPageDlgProc
				//
			    PROPSHEETPAGE psp;
				ZeroMemory(&psp, sizeof(PROPSHEETPAGE));

				psp.dwSize      = sizeof(psp);
				psp.dwFlags     = PSP_USEREFPARENT | PSP_USETITLE | PSP_USECALLBACK;
				psp.hInstance   = g_hmodThisDll;
				psp.pszTemplate = MAKEINTRESOURCE(IDD_IFOLDERPROP);
				psp.hIcon       = 0;
				// TODO - Get this from the resource file.
				psp.pszTitle    = TEXT("iFolder");
				psp.pfnDlgProc  = iFolderPageDlgProc;
				psp.pcRefParent = &g_cRefThisDll;
				psp.lParam      = (LPARAM)this;

				AddRef();
				HPROPSHEETPAGE hPage= CreatePropertySheetPage(&psp);

				if (hPage)
				{
					if (!lpfnAddPage(hPage, lParam))
					{
						DestroyPropertySheetPage(hPage);
						Release();
					}
				}

//				if (bDispSharePage)
//				{
//					psp.pszTemplate = MAKEINTRESOURCE(IDD_IFOLDERSHARINGPROP);
					// TODO - Get this from the resource file.
//					psp.pszTitle = TEXT("iFolder Sharing");
//					psp.pfnDlgProc = iFolderPermissionsPageDlgProc;

//					HPROPSHEETPAGE hPage2 = CreatePropertySheetPage(&psp);

//					if (hPage2)
//					{
//						if (!lpfnAddPage(hPage2, lParam))
//						{
//							DestroyPropertySheetPage(hPage2);
//						}
//					}
//				}
			}
		}
	}

	return NOERROR;
*/
}	/*-- AddPages() --*/
	
//
//  FUNCTION: CiFolderShell::ReplacePage(UINT, LPFNADDPROPSHEETPAGE)
//
//  PURPOSE: Replaces a page in a property sheet for a Control Panel object. 
//
//  PARAMETERS:
//    uPageID - Identifier of the page to replace. The values for this parameter
//			for Control Panels can be found in the Cplext.h header file. 
//    lpfnReplaceWith - Address of a function that the property sheet handler 
//			calls to replace a page to the property sheet. The function takes a 
//			property sheet handle returned by the CreatePropertySheetPage 
//			function and the lParam parameter passed to the ReplacePage method. 
//    lParam - Parameter to pass to the function specified by the lpfnReplaceWith method. 
//
//  RETURN VALUE:
//
//  COMMENTS:
//

STDMETHODIMP CiFolderShell::ReplacePage(UINT uPageID,
										LPFNADDPROPSHEETPAGE lpfnReplaceWith,
										LPARAM lParam)
{
    //OutputDebugString(TEXT("CiFolderShell::ReplacePage()\n"));
	return E_NOTIMPL;
}
