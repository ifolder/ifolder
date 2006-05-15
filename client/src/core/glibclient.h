/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2006 Novell, Inc.
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
// from a DLL simpler. All files within this DLL are compiled with the GLIBCLIENT_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// GLIBCLIENT_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifndef _GLIBCLIENT_H_
#define _GLIBCLIENT_H_

#ifdef WIN32
#ifdef GLIBCLIENT_EXPORTS
#define GLIBCLIENT_API __declspec(dllexport)
#else
#define GLIBCLIENT_API __declspec(dllimport)
#endif
#else
#define GLIBCLIENT_API
#endif

#ifdef _WINDOWS
#include <io.h>
#define SCANLL "%I64d"
#else
#define SCANLL "%Ld"
#endif


#include <sys/stat.h>
#include <stdio.h>
#include <fcntl.h>

#include <string>

#include <glib.h>
#include <glib/gstdio.h>

typedef std::string utf8string;

// This class is exported from the glibclient.dll
class GLIBCLIENT_API Cglibclient {
public:
	Cglibclient(void);
	// TODO: add your methods here.
};

extern GLIBCLIENT_API int nglibclient;

GLIBCLIENT_API int fnglibclient(void);

class IFFileInfoList;

#endif //_GLIBCLIENT_H_
