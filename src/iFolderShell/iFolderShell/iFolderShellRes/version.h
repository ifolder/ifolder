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
 
#ifndef _VERSIONH_
#define _VERSIONH_

//===[ Header files specific to NT          ]==============================

#include "..\nifver.h"

//===[ Header files specific to this module ]==============================

//===[ External data                        ]==============================

//===[ External prototypes                  ]==============================

//===[ Manifest constants                   ]==============================

#define VER_FILETYPE					VFT_DLL
#define VER_FILESUBTYPE				VFT2_UNKNOWN
#define VER_FILEDESCRIPTION_STR		"Novell iFolder Shell Extension Resources"
#define VER_INTERNALNAME_STR		"iFolderShellRes.dll"
// VER_FILEVERSION and VER_FILEVERSION_STR may be defined in nifver.h
#ifndef VER_FILEVERSION
#define VER_FILEVERSION				VER_PRODUCTVERSION
#endif
#ifndef VER_FILEVERSION_STR
#define VER_FILEVERSION_STR		VER_PRODUCTVERSION_STR
#endif
#define VER_ORIGINALFILENAME_STR	"iFolderShellRes.dll"

//===[ Type definitions                     ]==============================

//===[ Function Prototypes                  ]==============================

//===[ Global Variables                     ]==============================

//
// Stuff in the version information.
//

VER_VERSION_TEXT(VER_INTERNALNAME_STR);
VER_COPYRIGHT_TEXT;

#endif

//=========================================================================
//=========================================================================
