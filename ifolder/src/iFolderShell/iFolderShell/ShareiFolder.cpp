/***********************************************************************
 *  ShareiFolder.cpp - Implements the "Share iFolder" property sheet 
 *  page.
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

#include "CommonUI.h"
#include "iFolderShell.h"
#include "resource.h"

#include <string>
#include <map>

using namespace std;

typedef map<wstring, int> SHAREDATA;

// Reference count of this DLL.
extern UINT g_cRefThisDll;
// Handle to this DLL.
extern HINSTANCE g_hmodThisDll;

#define IFOLDER_PERMISSION_UNCHECKED	1
#define IFOLDER_PERMISSION_CHECKED		2

#define IFOLDER_SEND_INVITE				0x00010000
#define IFOLDER_RIGHTS_MASK				0x0000000F

typedef enum _IFOLDER_PERMISSIONS
{
	Deny,
	ReadOnly,
	ReadWrite,
	Admin
} IFOLDER_PERMISSIONS;

BOOL IsSendInviteChecked(HWND hwndULV, int index)
{
	BOOL bRet= FALSE;
	LVITEM lvi;

	lvi.iItem = index;
	lvi.iSubItem = 1;
	lvi.mask = LVIF_IMAGE;
	ListView_GetItem(hwndULV, &lvi);
	if (lvi.iImage == 1)
	{
		bRet= TRUE;
	}

	return bRet;

}	/*-- IsSendInviteChecked() --*/


void UpdateChangedList(HWND hwndULV, int nSel, SHAREDATA *pMyMap, SHAREDATA::iterator MyIterator)
{
	LVITEM lvi;
	TCHAR szGUID[256];
	int rights;
	TCHAR szRights[10];

	lvi.iItem= nSel;
	lvi.iSubItem= 2;
	lvi.pszText= szGUID;
	lvi.cchTextMax= 256;
	lvi.mask= LVIF_TEXT;
	ListView_GetItem(hwndULV, &lvi);

	lvi.pszText= szRights;
	lvi.cchTextMax= 10;
	lvi.iSubItem= 3;
	ListView_GetItem(hwndULV, &lvi);

	rights= _wtoi(szRights);

	lvi.iSubItem= 1;
	lvi.mask= LVIF_IMAGE;
	ListView_GetItem(hwndULV, &lvi);

	if (lvi.iImage == 1)
	{
		// Add a flag to send invite.
		rights |= IFOLDER_SEND_INVITE;
	}

	if ((MyIterator = pMyMap->find(szGUID)) != pMyMap->end())
	{
		// This entry is already in the map ... just update it's value.
		MyIterator->second = rights;
	}
	else
	{
		// Add this entry to the map.
		(*pMyMap)[szGUID] = rights;
	}

}	/*-- UpdateChangedList() --*/


INT_PTR CALLBACK iFolderPermissionsPageDlgProc(HWND hDlg,
									UINT uMessage,
									WPARAM wParam,
									LPARAM lParam)
{
	LPPROPSHEETPAGE psp= (LPPROPSHEETPAGE)GetWindowLongPtr(hDlg, DWLP_USER);
	LPCIFOLDERSHELL lpciFolderShell;
	TCHAR *szItems[]= {TEXT("Full Control"), TEXT("Change"), TEXT("Read")};
	int maxPermissions= 3;
	static SHAREDATA MyMap;
	static SHAREDATA::iterator MyIterator;
	HWND hwndPLV, hwndULV;
	
	TCHAR szMsg[256];
//	wsprintf(szMsg, TEXT("uMessage = %d\n"), uMessage);
//	OutputDebugString(szMsg);
	switch (uMessage)
	{
		//
		// When the shell creates a dialog box for a property sheet page,
		// it passes the pointer to the PROPSHEETPAGE data structure as
		// lParam. The dialog procedures of extensions typically store it
		// in the DWLP_USER of the dialog box window.
		//
	case WM_INITDIALOG:
		SetWindowLongPtr(hDlg, DWLP_USER, lParam);
		psp = (LPPROPSHEETPAGE)lParam;
		lpciFolderShell = (LPCIFOLDERSHELL)psp->lParam;

		hwndPLV= GetDlgItem(hDlg, IDC_PERMISSIONS);
		hwndULV= GetDlgItem(hDlg, IDC_USERS);

		HIMAGELIST hImageList, hOldList;
		HBITMAP hBmp;
		int test;
		hImageList= ImageList_Create(GetSystemMetrics(SM_CXICON)/2,
		                             GetSystemMetrics(SM_CYICON)/2,
									 ILC_COLORDDB, 2, 8);
		if(hImageList)
		{
			hBmp= LoadBitmap(g_hmodThisDll, MAKEINTRESOURCE(IDB_IMAGES));
			test = ImageList_Add(hImageList, hBmp, (HBITMAP)NULL);
			DeleteObject(hBmp);

			hOldList= ListView_SetImageList(hwndULV, hImageList, LVSIL_SMALL);
		}

		ListView_SetExtendedListViewStyle(hwndPLV, LVS_EX_CHECKBOXES);
		ListView_SetExtendedListViewStyle(hwndULV, LVS_EX_SUBITEMIMAGES);

		RECT rect;
		GetClientRect(hwndPLV, &rect);

		LVCOLUMN lvc;

		//Initialize the LV_COLUMN structure
		lvc.mask = LVCF_FMT | LVCF_WIDTH | LVCF_SUBITEM;
		lvc.fmt = LVCFMT_LEFT;

		//Add the columns
		lvc.iSubItem = 0;
		lvc.cx = rect.right / 3;
		ListView_InsertColumn(hwndPLV, 0, &lvc);

		lvc.iSubItem= 1;
		lvc.cx = rect.right * 2 / 3;
		int n;
		n = ListView_InsertColumn(hwndPLV, 0, &lvc);

		LVITEM lvi;
		lvi.mask= LVIF_TEXT;
		lvi.iSubItem= 0;
		int i;
		
		TCHAR szText[256];

		for (i = 0; i < 3; i++)
		{
			lvi.iItem= i;
			lvi.mask= LVIF_TEXT;
			lvi.iSubItem= 0;
			lvi.pszText= szText;
			lstrcpy(szText, szItems[i]);

			if (ListView_InsertItem(hwndPLV, &lvi) != -1)
			{
//				lvi.iSubItem = 1;
//				lvi.mask= LVIF_IMAGE;
//				lvi.iImage = 0;
//				ListView_SetItem(hwndPLV, &lvi);
				//lstrcpy(szText, TEXT("test"));
				//ListView_SetItemText(hwndPLV, i, 1, szText);
			}
		}

		try
		{
			if (lpciFolderShell->m_spiFolder == NULL)
			{
				// Instantiate the iFolder smart pointer.
				lpciFolderShell->m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
			}

			if (lpciFolderShell->m_spiFolder)
			{
				if (lpciFolderShell->m_spiFolder->GetiFolderAclInit())
				{
					BSTR guid, name;

					GetClientRect(hwndULV, &rect);

					LVCOLUMN lvc;

					//Initialize the LV_COLUMN structure
					lvc.mask = LVCF_FMT | LVCF_WIDTH | LVCF_SUBITEM;
					lvc.fmt = LVCFMT_LEFT;

					//Add the columns
					lvc.iSubItem = 0;
					lvc.cx = rect.right * 3/4;
					ListView_InsertColumn(hwndULV, 0, &lvc);

					lvc.iSubItem = 1;
					lvc.cx = rect.right / 4;
					ListView_InsertColumn(hwndULV, 1, &lvc);

					lvc.iSubItem = 2;
					lvc.cx = rect.right;
					ListView_InsertColumn(hwndULV, 2, &lvc);

					lvc.iSubItem = 3;
					lvc.cx = rect.right;
					ListView_InsertColumn(hwndULV, 3, &lvc);

					LVITEM lvi;
					int i= 0;

					long rights;
					TCHAR szRights[10];

					while (lpciFolderShell->m_spiFolder->GetNextiFolderAce(&guid, &name, &rights))
					{
						if (name != NULL)
						{
							lvi.iItem= i;
							lvi.iSubItem= 0;
							lvi.mask= LVIF_TEXT;
							lvi.pszText= name;

							if (ListView_InsertItem(hwndULV, &lvi) != -1)
							{
								lvi.iSubItem = 1;
								lvi.mask= LVIF_IMAGE;
								lvi.iImage = 2;
								ListView_SetItem(hwndULV, &lvi);


								ListView_SetItemText(hwndULV, i, 2, guid);
								ListView_SetItemText(hwndULV, i, 3, _itow(rights, szRights, 16));
								i++;
							}

							SysFreeString(name);
						}

						SysFreeString(guid);
					}

					// Select the first item in the list.
					ListView_SetItemState(hwndULV, 0, LVIS_SELECTED, LVIS_SELECTED);
				}
			}
		}
		catch (...)
		{
			// TODO - Generate an error message.
		}

		break;

	case WM_DESTROY:
		break;
	
	case WM_COMMAND:
		int resourceId;
		lpciFolderShell = (LPCIFOLDERSHELL)psp->lParam;
		resourceId= (int)LOWORD(wParam);
		switch (HIWORD(wParam))
		{
		case BN_CLICKED:
			hwndULV= GetDlgItem(hDlg, IDC_USERS);

			if (resourceId == IDC_ADD)
			{
				try
				{
					if (lpciFolderShell->m_spiFolder->InvokeContactPickerDlg())
					{
						BSTR guid, name;
						LVITEM lvi;
						int index = ListView_GetItemCount(hwndULV);

						// Default to Read/Write access;
						long rights = 2;
						TCHAR szRights[10];
						_itow(rights, szRights, 16);

						while (lpciFolderShell->m_spiFolder->GetNextAddedItem(&guid, &name))
						{
							lvi.iItem= index;
							lvi.iSubItem= 0;
							lvi.mask= LVIF_TEXT;
							lvi.pszText= name;

							if (ListView_InsertItem(hwndULV, &lvi) != -1)
							{
								lvi.iSubItem = 1;
								lvi.mask= LVIF_IMAGE;
								lvi.iImage = 1;
								ListView_SetItem(hwndULV, &lvi);

								ListView_SetItemText(hwndULV, index, 2, guid);
								ListView_SetItemText(hwndULV, index, 3, szRights);
							}

							SysFreeString(guid);
							SysFreeString(name);

							UpdateChangedList(hwndULV, index, &MyMap, MyIterator);
							index++;

							// Enable the Apply button.
							SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
						}
					}
				}
				catch (...)
				{
					// TODO - Generate an error message.
				}
			}
			else if (resourceId == IDC_REMOVE)
			{
				TCHAR szGUID[256];
				LVITEM lvi;

				// Get the GUID for the selected item.
				lvi.iItem= ListView_GetSelectionMark(hwndULV);
				lvi.iSubItem= 2;
				lvi.pszText= szGUID;
				lvi.cchTextMax= 256;
				lvi.mask= LVIF_TEXT;
				ListView_GetItem(hwndULV, &lvi);

				if ((MyIterator = MyMap.find(szGUID)) != MyMap.end())
				{
					// This entry is already in the map ... just update it's value.
					MyIterator->second = 0;
				}
				else
				{
					// Add this entry to the map.
					MyMap[szGUID] = 0;
				}

				// Remove the item from the listview.
				ListView_DeleteItem(hwndULV, lvi.iItem);

				// Enable the Apply button.
				SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
			}
			break;
		}
		break;

	case WM_NOTIFY:
		NMLISTVIEW *pnmv;
//		wsprintf(szMsg, TEXT("lParam->code = %d\n"), ((NMHDR FAR *)lParam)->code);
//		OutputDebugString(szMsg);
		switch (((NMHDR FAR *)lParam)->code)
		{
		case NM_CLICK:
			hwndULV= GetDlgItem(hDlg, IDC_USERS);
			hwndPLV= GetDlgItem(hDlg, IDC_PERMISSIONS);

			pnmv = (LPNMLISTVIEW)lParam;
			if (pnmv->hdr.hwndFrom == hwndULV)
			{
				int index;
				LVHITTESTINFO info;
				DWORD dwPos;
				RECT rect;

				// Get the position of the mouse in the user client area.
				dwPos= GetMessagePos();
				GetWindowRect(hwndULV, &rect);
				info.pt.x= LOWORD(dwPos) - rect.left;
				info.pt.y= HIWORD(dwPos) - rect.top;

				// Do a hit test to see which sub-item was clicked.
				index = ListView_SubItemHitTest(hwndULV, &info);
				if ((index != -1) && (info.iSubItem == 1))
				{
					LVITEM lvi;

					lvi.iItem= index;
					lvi.iSubItem= info.iSubItem;
					lvi.mask= LVIF_IMAGE;
					lvi.iImage= IsSendInviteChecked(hwndULV, index) ? 2 : 1;

					ListView_SetItem(hwndULV, &lvi);

					UpdateChangedList(hwndULV, index, &MyMap, MyIterator);

					// Enable the Apply button.
					SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
				}
			}
			break;
		case LVN_ITEMCHANGED:
			hwndPLV= GetDlgItem(hDlg, IDC_PERMISSIONS);
			hwndULV= GetDlgItem(hDlg, IDC_USERS);

			pnmv = (LPNMLISTVIEW)lParam;
			if (pnmv->hdr.hwndFrom == hwndPLV)
			{
				//
				// The permissions for a given user are changing.
				//

				// Which user?
				int nSel= ListView_GetNextItem(hwndULV, -1, LVNI_SELECTED);

				UINT uOldState, uNewState;
				uOldState= (pnmv->uOldState & LVIS_STATEIMAGEMASK) >> 12;
				uNewState= (pnmv->uNewState & LVIS_STATEIMAGEMASK) >> 12;

				int index;
				LVHITTESTINFO info;
				DWORD dwPos;
				RECT rect;

				// Get the position of the mouse in the permissions client area.
				dwPos= GetMessagePos();
				GetWindowRect(hwndPLV, &rect);
				info.pt.x= LOWORD(dwPos) - rect.left;
				info.pt.y= HIWORD(dwPos) - rect.top;

				// Do a hit test to see which item was clicked.
				index = ListView_HitTest(hwndPLV, &info);

				TCHAR szRights[10];

				if ((uOldState == IFOLDER_PERMISSION_UNCHECKED) && 
					(uNewState == IFOLDER_PERMISSION_CHECKED))
				{
					// If the index is valid, use it to update the permissions on the user.
					if (index != -1)
					{
						int rights= maxPermissions-index;

						ListView_SetItemText(hwndULV, nSel, 3, _itow(rights, szRights, 16));

						UpdateChangedList(hwndULV, nSel, &MyMap, MyIterator);

						// Enable the Apply button.
						SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
					}

					if (pnmv->iItem == 0)
					{
						// Full Control was checked ... Check Read/Write rights, 
						// which will cause Read-only to be checked also.
						if (!ListView_GetCheckState(hwndPLV, 1))
						{
							ListView_SetCheckState(hwndPLV, 1, TRUE);
						}
					}
					else if (pnmv->iItem == 1)
					{
						// Read/Write was checked ... check Read-only also.
						if (!ListView_GetCheckState(hwndPLV, 2))
						{
							ListView_SetCheckState(hwndPLV, 2, TRUE);
						}
					}
				}
				else if ((uOldState == IFOLDER_PERMISSION_CHECKED) &&
						(uNewState == IFOLDER_PERMISSION_UNCHECKED))
				{
					// If the index is valid, use it to update the permissions on the user.
					if (index != -1)
					{
						ListView_SetItemText(hwndULV, nSel, 3, _itow(maxPermissions-(index+1), szRights, 16));

						UpdateChangedList(hwndULV, nSel, &MyMap, MyIterator);

						// Enable the Apply button.
						SendMessage(GetParent(hDlg), (UINT)PSM_CHANGED, (WPARAM)hDlg, 0);
					}

					if (pnmv->iItem == 1)
					{
						// Read/Write was unchecked ... uncheck Full Control also.
						if (ListView_GetCheckState(hwndPLV, 0))
						{
							ListView_SetCheckState(hwndPLV, 0, FALSE);
						}
					}
					else if (pnmv->iItem == 2)
					{
						// Read-only was unchecked ... uncheck Read/Write (which will cause
						// Full Control to be unchecked also).
						if (ListView_GetCheckState(hwndPLV, 1))
						{
							ListView_SetCheckState(hwndPLV, 1, FALSE);
						}
					}
				}
			}
			else if (pnmv->hdr.hwndFrom == hwndULV)
			{
				if (pnmv->uNewState & LVIS_SELECTED)
				{
					TCHAR szRights[10];

					// A user has been selected ... update the permissions to reflect
					// the correct rights for the user.
					ListView_GetItemText(hwndULV, pnmv->iItem, 3, szRights, 10);
					IFOLDER_PERMISSIONS ifPerm= (IFOLDER_PERMISSIONS)_wtoi(szRights);
					switch (ifPerm)
					{
					case Deny:
						ListView_SetCheckState(hwndPLV, 2, FALSE);
						break;
					case ReadOnly:
						if (ListView_GetCheckState(hwndPLV, 1))
						{
							ListView_SetCheckState(hwndPLV, 1, FALSE);
						}

						ListView_SetCheckState(hwndPLV, 2, TRUE);
						break;
					case ReadWrite:
						if (ListView_GetCheckState(hwndPLV, 0))
						{
							ListView_SetCheckState(hwndPLV, 0, FALSE);
						}

						ListView_SetCheckState(hwndPLV, 1, TRUE);
						break;
					case Admin:
						ListView_SetCheckState(hwndPLV, 0, TRUE);
						break;
					}
				}
			}

			break;
		case PSN_SETACTIVE:
			break;
		case PSN_APPLY:
			//User has clicked the OK or Apply button ...
			
			//handle to Hour Glass cursor
			HCURSOR	hHourGlass;
			//handle to regular cursor
			HCURSOR	hSaveCursor;

			// Change the pointer to an hourglass.
			hHourGlass = LoadCursor(NULL, IDC_WAIT);
			SetCapture(hDlg);
			hSaveCursor = SetCursor(hHourGlass);

			// Process the changes.
			if (!MyMap.empty())
			{
				psp = (LPPROPSHEETPAGE)lParam;
				lpciFolderShell = (LPCIFOLDERSHELL)psp->lParam;

				try
				{
					if (lpciFolderShell->m_spiFolder == NULL)
					{
						// Instantiate the iFolder smart pointer.
						lpciFolderShell->m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
					}

					if (lpciFolderShell->m_spiFolder)
					{
						// Process each entry in the map list...
						MyIterator= MyMap.begin();
						while (MyIterator != MyMap.end())
						{
							int rights = MyIterator->second & IFOLDER_RIGHTS_MASK;
							bool invite = (MyIterator->second & IFOLDER_SEND_INVITE) ? true : false;
							lpciFolderShell->m_spiFolder->ShareiFolder(MyIterator->first.c_str(), rights, invite);
							MyIterator++;
						}

						// Clear out the map list.
						MyMap.clear();
					}
				}
				catch(...)
				{
					// TODO - Generate an error message.

					// Clear out the map list.
					MyMap.clear();
				}
			}

			hwndULV= GetDlgItem(hDlg, IDC_USERS);

			int count, i;
			count = ListView_GetItemCount(hwndULV);
			for (i = 0; i < count; i++)
			{
				if (IsSendInviteChecked(hwndULV, i))
				{
					// Clear the invite checkbox for each listview item.
					LVITEM lvi;
					lvi.iItem= i;
					lvi.iSubItem= 1;
					lvi.mask= LVIF_IMAGE;
					lvi.iImage= 2;
					ListView_SetItem(hwndULV, &lvi);
				}
			}

			SetCursor(hSaveCursor);
			ReleaseCapture();

			break;
		
		default:
			break;
		}
		
		break;
	default:
		return FALSE;
	}
	
	return TRUE;

}	/*-- iFolderPermissionsPageDlgProc() --*/


