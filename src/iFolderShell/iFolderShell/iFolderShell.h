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

#ifndef _IFOLDERSHELL_H
#define _IFOLDERSHELL_H

// Import the .NET component's typelibrary
#import "..\iFolderComponent.tlb" no_namespace

// {AA81D830-3B41-497c-B508-E9D02F8DF421}
DEFINE_GUID(CLSID_iFolderShell0, 0xaa81d830L, 0x3b41, 0x497c, 0xb5, 0x08, 0xe9, 0xd0, 0x2f, 0x8d, 0xf4, 0x21);
// {AA81D831-3B41-497c-B508-E9D02F8DF421}
DEFINE_GUID(CLSID_iFolderShell1, 0xaa81d831L, 0x3b41, 0x497c, 0xb5, 0x08, 0xe9, 0xd0, 0x2f, 0x8d, 0xf4, 0x21);

#define MAX_ROOT_PATH 3 // the length of a root path i.e. c:\

enum iFolderClass
{
	IFOLDER_INVALID,
	IFOLDER_CONFLICT,
	IFOLDER_ISIFOLDER,
};

// this class factory object creates context menu handlers for Windows 95 shell
class CiFolderShellClassFactory : public IClassFactory
{
protected:
    ULONG   m_cRef;
	iFolderClass m_class;

public:
    CiFolderShellClassFactory(iFolderClass iFClass);
    ~CiFolderShellClassFactory();

    //IUnknown members
    STDMETHODIMP            QueryInterface(REFIID, LPVOID FAR *);
    STDMETHODIMP_(ULONG)    AddRef();
    STDMETHODIMP_(ULONG)    Release();

    //IClassFactory members
    STDMETHODIMP        CreateInstance(LPUNKNOWN, REFIID, LPVOID FAR *);
    STDMETHODIMP        LockServer(BOOL);

};
typedef CiFolderShellClassFactory *LPCIFOLDERSHELLCLASSFACTORY;

// this is the actual OLE Shell context menu handler
class CiFolderShell : public IContextMenu,
                         IShellExtInit,
						 IShellIconOverlayIdentifier,
                         IShellPropSheetExt
                         //IExtractIcon,
                         //IPersistFile,
                         //ICopyHook
{
protected:
    ULONG        m_cRef;
    LPDATAOBJECT m_pDataObj;

public:
	static IiFolderComponentPtr m_spiFolder;
    TCHAR m_szFileUserClickedOn[MAX_PATH];
	static TCHAR m_szShellPath[MAX_PATH];
	iFolderClass m_iFolderClass;

public:
	CiFolderShell(iFolderClass iFClass);
	~CiFolderShell();
	
	//IUnknown members
	STDMETHODIMP			QueryInterface(REFIID, LPVOID FAR *);
	STDMETHODIMP_(ULONG)	AddRef();
	STDMETHODIMP_(ULONG)	Release();
	
	//IContextMenu members
	STDMETHODIMP			QueryContextMenu(HMENU hMenu,
											UINT indexMenu,
											UINT idCmdFirst,
											UINT idCmdLast,
											UINT uFlags);

	STDMETHODIMP			InvokeCommand(LPCMINVOKECOMMANDINFO lpcmi);
	
	STDMETHODIMP			GetCommandString(UINT_PTR idCmd,
											UINT uFlags,
											UINT FAR *reserved,
											LPSTR pszName,
											UINT cchMax);
	
	//IShellExtInit methods
	STDMETHODIMP			Initialize(LPCITEMIDLIST pidlFolder,
										LPDATAOBJECT pDataObj,
										HKEY hKeyProgID);

	//IShellIconOverlayIdentifier methods
	STDMETHODIMP			IsMemberOf(LPCWSTR pwszPath,
										DWORD dwAttrib);

	STDMETHODIMP			GetOverlayInfo(LPWSTR pwszIconFile,
											int cchMax,
											int *pIndex,
											DWORD *pdwFlags);

	STDMETHODIMP			GetPriority(int *pIPriority);
	
	//IShellPropSheetExt methods
	STDMETHODIMP			AddPages(LPFNADDPROPSHEETPAGE lpfnAddPage,
									LPARAM lParam);
	
	STDMETHODIMP			ReplacePage(UINT uPageID,
										LPFNADDPROPSHEETPAGE lpfnReplaceWith,
										LPARAM lParam);

    //IExtractIcon methods
//    STDMETHODIMP GetIconLocation(UINT   uFlags,
//                                 LPSTR  szIconFile,
//                                 UINT   cchMax,
//                                 int   *piIndex,
//                                 UINT  *pwFlags);

//    STDMETHODIMP Extract(LPCSTR pszFile,
//                         UINT   nIconIndex,
//                         HICON  *phiconLarge,
//                         HICON  *phiconSmall,
//                         UINT   nIconSize);

    //IPersistFile methods
//    STDMETHODIMP GetClassID(LPCLSID lpClassID);

//    STDMETHODIMP IsDirty();

//    STDMETHODIMP Load(LPCOLESTR lpszFileName, DWORD grfMode);

//    STDMETHODIMP Save(LPCOLESTR lpszFileName, BOOL fRemember);

//    STDMETHODIMP SaveCompleted(LPCOLESTR lpszFileName);

//    STDMETHODIMP GetCurFile(LPOLESTR FAR* lplpszFileName);

    //ICopyHook method
//    STDMETHODIMP_(UINT) CopyCallback(HWND hwnd,
//                                     UINT wFunc,
//                                     UINT wFlags,
//                                     LPCSTR pszSrcFile,
//                                     DWORD dwSrcAttribs,
//                                     LPCSTR pszDestFile,
//                                     DWORD dwDestAttribs);

};
typedef CiFolderShell *LPCIFOLDERSHELL;

#endif // _IFOLDERSHELL_H
