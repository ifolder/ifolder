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
#include "Registry.h"

//
// Initialize GUIDs (should be done only and at-least once per DLL/EXE)
//
#pragma data_seg(".text")
#define INITGUID
#include <initguid.h>
#include <shlguid.h>
#include "iFolderShell.h"
#pragma data_seg()

//
// Global variables
//
UINT      g_cRefThisDll = 0;    // Reference count of this DLL.
HINSTANCE g_hmodThisDll = NULL; // Handle to this DLL itself.
HMODULE g_hmodResDll = NULL; // Handle to resource DLL.


// static members
//IiFolderComponentPtr CiFolderShell::m_spiFolder = NULL;

extern "C" int APIENTRY
DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
    if (dwReason == DLL_PROCESS_ATTACH)
    {
        OutputDebugString(TEXT("In DLLMain, DLL_PROCESS_ATTACH\n"));

		InitCommonControls();

        // Extension DLL one-time initialization
        g_hmodThisDll = hInstance;
    }
    else if (dwReason == DLL_PROCESS_DETACH)
    {
        OutputDebugString(TEXT("In DLLMain, DLL_PROCESS_DETACH\n"));
    }

    return 1;
}

//---------------------------------------------------------------------------
// DllCanUnloadNow
//---------------------------------------------------------------------------

STDAPI DllCanUnloadNow(void)
{
    OutputDebugString(TEXT("In DLLCanUnloadNow\n"));

    return (g_cRefThisDll == 0 ? S_OK : S_FALSE);
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppvOut)
{
    OutputDebugString(TEXT("In DllGetClassObject\n"));

    *ppvOut = NULL;

	iFolderClass iFClass = IFOLDER_INVALID;
	if (IsEqualIID(rclsid, CLSID_iFolderShell0))
	{
		iFClass = IFOLDER_ISIFOLDER;
	}
	else if (IsEqualIID(rclsid, CLSID_iFolderShell1))
	{
		iFClass = IFOLDER_CONFLICT;
	}

	if (iFClass != IFOLDER_INVALID)
    {
        CiFolderShellClassFactory *pcf = new CiFolderShellClassFactory(iFClass);

        return pcf->QueryInterface(riid, ppvOut);
    }

    return CLASS_E_CLASSNOTAVAILABLE;
}

//
// DllRegisterServer(void)
//
// Instructs an in-process server to create its registry entries for all classes 
// supported in this server module. If this function fails, the state of the registry
// for all its classes is indeterminate.
//
// This function supports the standard return values E_OUTOFMEMORY and E_UNEXPECTED,
// as well as the following: 
//
// S_OK - The registry entries were created successfully. 
// SELFREG_E_TYPELIB - The server was unable to complete the registration of all the 
//		type libraries used by its classes. 
// SELFREG_E_CLASS - The server was unable to complete the registration of all the 
//		object classes. 
//
STDAPI DllRegisterServer(void)
{
    //OutputDebugString(TEXT("In DllRegisterServer\n"));

	HRESULT  hr= SELFREG_E_CLASS;
	TCHAR    szID[GUID_SIZE+1];
	TCHAR    szCLSID[GUID_SIZE+32];
	TCHAR    szModulePath[MAX_PATH];
	
	// Obtain the path to this module's executable file for later use.
	if (GetModuleFileName(
		g_hmodThisDll,
		szModulePath,
		sizeof(szModulePath)/sizeof(TCHAR)))
	{	
		//OutputDebugString(szModulePath);
		// Create some base key strings.
		StringFromGUID2(CLSID_iFolderShell0, szID, GUID_SIZE);
		lstrcpyn(szCLSID, TEXT("CLSID\\"), sizeof(szCLSID)/sizeof(TCHAR));
		lstrcat(szCLSID, szID);
		
		// Create entries under CLSID.
		if (SetRegKeyValue(
			szCLSID,
			NULL,
			TEXT("iFolderShellExtension")))
		{
			if (SetRegKeyValue(
				szCLSID,
				TEXT("InprocServer32"),
				szModulePath))
			{
				if (AddRegNamedValue(
					szCLSID,
					TEXT("InprocServer32"),
					TEXT("ThreadingModel"),
					TEXT("Apartment")))
				{
					// Create the entry for the other shell extension GUID.
					StringFromGUID2(CLSID_iFolderShell1, szID, GUID_SIZE);
					lstrcpyn(szCLSID, TEXT("CLSID\\"), sizeof(szCLSID)/sizeof(TCHAR));
					lstrcat(szCLSID, szID);
					
					// Create entries under CLSID.
					if (SetRegKeyValue(
						szCLSID,
						NULL,
						TEXT("iFolderShellExtension")))
					{
						if (SetRegKeyValue(
							szCLSID,
							TEXT("InprocServer32"),
							szModulePath))
						{
							if (AddRegNamedValue(
								szCLSID,
								TEXT("InprocServer32"),
								TEXT("ThreadingModel"),
								TEXT("Apartment")))
							{
								hr= S_OK;
							}
						}
					}
				}
			}
		}
	}
	else
	{
		TCHAR szMsg[256];
		wsprintf(szMsg, TEXT("Error = 0x%08X\n"), GetLastError());
		//OutputDebugString(szMsg);
	}
	
	return hr;
}

//
// DllUnregisterServer(void)
//
// Instructs an in-process server to remove only those entries created through 
// DllRegisterServer.
//
// This function supports the standard return values E_OUTOFMEMORY and E_UNEXPECTED,
// as well as the following: 
//
// S_OK - The registry entries were created successfully. 
// S_FALSE - Unregistration of this server's known entries was successful, but other 
//		entries still exist for this server's classes. 
// SELFREG_E_TYPELIB - The server was unable to remove the entries of all the type 
//		libraries used by its classes. 
// SELFREG_E_CLASS - The server was unable to remove the entries of all the object classes. 
//
STDAPI DllUnregisterServer(void)
{
    //OutputDebugString(TEXT("In DllUnregisterServer\n"));

	HRESULT  hr= SELFREG_E_CLASS;
	TCHAR    szID[GUID_SIZE+1];
	TCHAR    szCLSID[GUID_SIZE+32];
	TCHAR    szTemp[MAX_PATH+GUID_SIZE];
	
	//Create some base key strings.
	StringFromGUID2(CLSID_iFolderShell0, szID, GUID_SIZE);
	lstrcpyn(szCLSID, TEXT("CLSID\\"), sizeof(szCLSID)/sizeof(TCHAR));
	lstrcat(szCLSID, szID);

	wsprintf(szTemp, TEXT("%s\\%s"), szCLSID, TEXT("InprocServer32"));
	//OutputDebugString(szTemp);
	if (RegDeleteKey(HKEY_CLASSES_ROOT, szTemp) == ERROR_SUCCESS)
	{
		if (RegDeleteKey(HKEY_CLASSES_ROOT, szCLSID) == ERROR_SUCCESS)
		{
			//Create some base key strings.
			StringFromGUID2(CLSID_iFolderShell1, szID, GUID_SIZE);
			lstrcpyn(szCLSID, TEXT("CLSID\\"), sizeof(szCLSID)/sizeof(TCHAR));
			lstrcat(szCLSID, szID);

			wsprintf(szTemp, TEXT("%s\\%s"), szCLSID, TEXT("InprocServer32"));
			//OutputDebugString(szTemp);
			if (RegDeleteKey(HKEY_CLASSES_ROOT, szTemp) == ERROR_SUCCESS)
			{
				if (RegDeleteKey(HKEY_CLASSES_ROOT, szCLSID) == ERROR_SUCCESS)
				{
					hr= S_OK;
				}
			}
		}
	}
	
	return hr;
}

CiFolderShellClassFactory::CiFolderShellClassFactory(iFolderClass iFClass)
{
    OutputDebugString(TEXT("CiFolderShellClassFactory::CiFolderShellClassFactory()\n"));

    m_cRef = 0L;
	m_class = iFClass;

    InterlockedIncrement((LONG *)&g_cRefThisDll);
}

CiFolderShellClassFactory::~CiFolderShellClassFactory()
{
    InterlockedDecrement((LONG *)&g_cRefThisDll);
}

STDMETHODIMP CiFolderShellClassFactory::QueryInterface(REFIID riid,
                                                   LPVOID FAR *ppv)
{
    OutputDebugString(TEXT("CiFolderShellClassFactory::QueryInterface()\n"));

    *ppv = NULL;

    // Any interface on this object is the object pointer

    if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_IClassFactory))
    {
        *ppv = (LPCLASSFACTORY)this;

        AddRef();

        return NOERROR;
    }

    return E_NOINTERFACE;
}

STDMETHODIMP_(ULONG) CiFolderShellClassFactory::AddRef()
{
    return ++m_cRef;
}

STDMETHODIMP_(ULONG) CiFolderShellClassFactory::Release()
{
    if (--m_cRef)
        return m_cRef;

    delete this;

    return 0L;
}

STDMETHODIMP CiFolderShellClassFactory::CreateInstance(LPUNKNOWN pUnkOuter,
													   REFIID riid,
													   LPVOID *ppvObj)
{
    OutputDebugString(TEXT("CiFolderShellClassFactory::CreateInstance()\n"));

    *ppvObj = NULL;

    // Shell extensions typically don't support aggregation (inheritance)

    if (pUnkOuter)
        return CLASS_E_NOAGGREGATION;

    // Create the main shell extension object.  The shell will then call
    // QueryInterface with IID_IShellExtInit--this is how shell extensions are
    // initialized.

	//Create the CiFolderShell object
	LPCIFOLDERSHELL piFolderShell = new CiFolderShell(m_class);

    if (NULL == piFolderShell)
        return E_OUTOFMEMORY;

    return piFolderShell->QueryInterface(riid, ppvObj);
}


STDMETHODIMP CiFolderShellClassFactory::LockServer(BOOL fLock)
{
    return NOERROR;
}

// *********************** CiFolderShell *************************
CiFolderShell::CiFolderShell(iFolderClass iFClass)
{
    OutputDebugString(TEXT("CiFolderShell::CiFolderShell()\n"));

    m_cRef = 0L;
    m_pDataObj = NULL;
	m_hBmpMenu = NULL;
	m_spiFolder= NULL;
	m_iFolderClass = iFClass;

    InterlockedIncrement((LONG *)&g_cRefThisDll);
}

CiFolderShell::~CiFolderShell()
{
    //OutputDebugString(TEXT("CiFolderShell::~CiFolderShell()\n"));

	if (m_hBmpMenu)
	{
		DeleteObject(m_hBmpMenu);
		m_hBmpMenu = NULL;
	}

    if (m_pDataObj)
        m_pDataObj->Release();

//	if (m_spiFolder)
//		m_spiFolder.Release();

    InterlockedDecrement((LONG *)&g_cRefThisDll);

	if ((g_cRefThisDll == 0) && (g_hmodResDll != g_hmodThisDll))
	{
		FreeLibrary(g_hmodResDll);
		g_hmodResDll = NULL;
	}
}

STDMETHODIMP CiFolderShell::QueryInterface(REFIID riid, LPVOID FAR *ppv)
{
    *ppv = NULL;

    if (IsEqualIID(riid, IID_IShellExtInit) || IsEqualIID(riid, IID_IUnknown))
    {
        OutputDebugString(TEXT("CiFolderShell::QueryInterface()==>IID_IShellExtInit\n"));

        *ppv = (LPSHELLEXTINIT)this;
    }
    else if (IsEqualIID(riid, IID_IContextMenu))
    {
        OutputDebugString(TEXT("CiFolderShell::QueryInterface()==>IID_IContextMenu\n"));

        *ppv = (LPCONTEXTMENU)this;
    }
	else if (IsEqualIID(riid, IID_IShellIconOverlayIdentifier))
	{
		OutputDebugString(TEXT("CiFolderShell::QueryInterface()==>IID_IShellIconOverlayIdentifier\n"));

		*ppv= (IShellIconOverlayIdentifier *)this;
	}
    else if (IsEqualIID(riid, IID_IShellPropSheetExt))
    {
        OutputDebugString(TEXT("CiFolderShell::QueryInterface()==>IShellPropSheetExt\n"));

        *ppv = (LPSHELLPROPSHEETEXT)this;
    }
//    else if (IsEqualIID(riid, IID_IExtractIcon))
//    {
//        OutputDebugString("CiFolderShell::QueryInterface()==>IID_IExtractIcon\n");

//        *ppv = (LPEXTRACTICON)this;
//    }
//    else if (IsEqualIID(riid, IID_IPersistFile))
//    {
//        OutputDebugString("CiFolderShell::QueryInterface()==>IPersistFile\n");

//        *ppv = (LPPERSISTFILE)this;
//    }
//    else if (IsEqualIID(riid, IID_IShellCopyHook))
//    {
//        OutputDebugString("CiFolderShell::QueryInterface()==>ICopyHook\n");

//        *ppv = (LPCOPYHOOK)this;
//    }

    if (*ppv)
    {
        AddRef();

        return NOERROR;
    }

	OutputDebugString(TEXT("CiFolderShell::QueryInterface()==>Unknown Interface!\n"));

    return E_NOINTERFACE;
}

STDMETHODIMP_(ULONG) CiFolderShell::AddRef()
{
    //OutputDebugString(TEXT("CiFolderShell::AddRef()\n"));

    return ++m_cRef;
}

STDMETHODIMP_(ULONG) CiFolderShell::Release()
{
    //OutputDebugString(TEXT("CiFolderShell::Release()\n"));

    if (--m_cRef)
        return m_cRef;

    delete this;

    return 0L;
}

//
//  FUNCTION: CiFolderShell::Initialize(LPCITEMIDLIST, LPDATAOBJECT, HKEY)
//
//  PURPOSE: Called by the shell when initializing a context menu or property
//           sheet extension.
//
//  PARAMETERS:
//    pidlFolder - Address of an ITEMIDLIST structure that uniquely identifies a folder. 
//				For property sheet extensions, this parameter is NULL. For shortcut menu 
//				extensions, it is the item identifier list for the folder that contains 
//				the item whose shortcut menu is being displayed. For nondefault drag-and-drop
//				menu extensions, this parameter specifies the target folder.
//    pDataObj  - Address of an IDataObject interface object that can be used to retrieve the objects being acted upon.
//    hKeyProgID   - Registry key for the file object or folder type.
//
//  RETURN VALUE:
//
//    NOERROR in all cases.
//
//  COMMENTS:   Note that at the time this function is called, we don't know
//              (or care) what type of shell extension is being initialized.
//              It could be a context menu or a property sheet.
//

STDMETHODIMP CiFolderShell::Initialize(LPCITEMIDLIST pidlFolder,
									   LPDATAOBJECT pDataObj,
									   HKEY hKeyProgID)
{
    OutputDebugString(TEXT("CiFolderShell::Initialize()\n"));
	HRESULT hr = 0;
	if (pDataObj == NULL)
	{
	    OutputDebugString(TEXT("CiFolderShell::Initialize() returning E_INVALIDARG\n"));
		return E_INVALIDARG;
	}

	// Initialize can be called more than once
	if (m_pDataObj)
		m_pDataObj->Release();

	// duplicate the object pointer and registry handle
	if (pDataObj)
	{
		m_pDataObj= pDataObj;
		pDataObj->AddRef();
	}

	if (g_hmodResDll == NULL)
	{
		// Default to English.
		g_hmodResDll = g_hmodThisDll;

		if (m_spiFolder == NULL)
		{
			OleInitialize(NULL);
			// Instantiate the iFolder smart pointer.
			hr = m_spiFolder.CreateInstance(__uuidof(iFolderComponent));
			if (FAILED(hr))
			{
				OutputDebugString(TEXT("Unable to CreateInstance of iFodlerComponent::Initialize()\n"));
			}
		}

		try
		{
			// Get the language directory
			BSTR strLang = m_spiFolder->GetLanguageDirectory();

			// Build the path to the resource dll.
			TCHAR szPath[MAX_PATH];
			lstrcpy(szPath, m_szShellPath);
			lstrcat(szPath, strLang);
			lstrcat(szPath, TEXT("\\iFolderShellRes.dll"));

			g_hmodResDll = LoadLibrary(szPath);
			if (g_hmodResDll == NULL)
			{
				// Can't find the resource file ... default to English.
				g_hmodResDll = g_hmodThisDll;
			}

			SysFreeString(strLang);
		}
		catch (...)
		{
			OutputDebugString(TEXT("***Caught exception in CiFolderShell::Initialize()***\n"));
		}
	}


//	STGMEDIUM medium;
//	FORMATETC fe= {CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL};
//	HRESULT hr= pDataObj->GetData(&fe, &medium);
//	if (FAILED(hr))
//		return E_INVALIDARG;

	// TODO - build the list of selected files here ... then we don't have
	// to drag around the HDROP.
//	m_hDrop= reinterpret_cast<HDROP>(medium.hGlobal);

//	UINT count= DragQueryFile(m_hDrop, -1, NULL, 0);
	// TODO - for now, we only allow one item to be selected.
//	if (count == 1)
//	{
//		DragQueryFile(m_hDrop, 0, m_szFileUserClickedOn, MAX_PATH);
//	}

//	ReleaseStgMedium(&medium);

    OutputDebugString(TEXT("CiFolderShell::Initialize() returning NOERROR\n"));
    return NOERROR;
}
