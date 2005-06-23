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
 
#ifndef _NIFVER_
#define _NIFVER_

//===[ Header files specific to NT          ]==============================

#include <windef.h>
#include <winver.h>

//===[ Header files specific to this module ]==============================

//===[ External data                        ]==============================

//===[ External prototypes                  ]==============================

//===[ Manifest constants                   ]==============================

//
// VER_ID, VER_PROD_VER, VER_PROD_STR, VER_STR, and VER_DATE are normally defined on the command line.
// These are the default values.
//

#ifndef VER_ID
#define VER_ID   1,0,0,0
#endif

#ifndef VER_STR
#define VER_STR  "v1.0"
#endif

#ifndef VER_PROD_ID
#define VER_PROD_ID   1,0,0,0
#endif

#ifndef VER_PROD_STR
#define VER_PROD_STR  "v1.0.0"
#endif

#ifndef VER_DATE
#define VER_DATE "(20041020_v1.0.0)"
#endif

#define VER_VERSION_TEXT(module)   static char versionText[] = \
	"VeRsIoN=" VER_STR " Novell iFolder " module " module " VER_DATE

#ifndef VER_COPYRIGHT_YEARS
#define VER_COPYRIGHT_YEARS "2004"
#endif
#define VER_COPYRIGHT_TEXT static char copyrightText[] = \
   "CoPyRiGhT=Copyright " VER_COPYRIGHT_YEARS ", by Novell, Inc. All rights reserved."

// Definitions for resource files
#define VER_PRODUCTVERSION      	VER_PROD_ID
#define VER_FILEVERSION      		VER_ID
#define VER_COMPANYNAME_STR		TEXT("Novell, Inc.")
#define VER_PRODUCTNAME_STR		TEXT("Novell iFolder\0")
#define VER_PRODUCTVERSION_STR	TEXT(VER_PROD_STR) TEXT(" ") TEXT(VER_DATE)
#define VER_FILEVERSION_STR		TEXT(VER_STR) TEXT(" ") TEXT(VER_DATE)
#define VER_LEGALCOPYRIGHT_STR	TEXT("Copyright \251 2004 ") VER_COMPANYNAME_STR, TEXT("\0")

//===[ Type definitions                     ]==============================

//===[ Function Prototypes                  ]==============================

//===[ Global Variables                     ]==============================

#endif

//=========================================================================
//=========================================================================
