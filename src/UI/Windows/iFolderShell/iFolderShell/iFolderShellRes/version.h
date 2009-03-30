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
