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
 *  Author: Russ Young
 *
 ***********************************************************************/
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the FLAIMWRAPPER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// FLAIMWRAPPER_API functions as being imported from a DLL, wheras this DLL sees symbols
// defined with this macro as being exported.
#ifdef WIN32
#ifdef FLAIMWRAPPER_EXPORTS
#define FLAIMWRAPPER_API __declspec(dllexport)
#else
#define FLAIMWRAPPER_API __declspec(dllimport)
#endif
#else
#ifdef UNIX
#define FLAIMWRAPPER_API
#endif
#endif

// This class is exported from the FlaimWrapper.dll
class FLAIMWRAPPER_API CFlaimWrapper {
public:
	CFlaimWrapper(void);
	// TODO: add your methods here.
};

extern FLAIMWRAPPER_API int nFlaimWrapper;

FLAIMWRAPPER_API int fnFlaimWrapper(void);

