/***********************************************************************
 *  $RCSfile: Common.cs,v $
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;

namespace iFolder.Client
{
	/// <summary>
	/// static directory paths to configuration xml documents
	/// </summary>
	internal class Configuration
	{
		static internal readonly string StoreComponent = "ifolder-store";
		static internal readonly string DomainComponent = "domains";
		static internal readonly string iFolderComponent = "ifolders";
		static internal readonly string DefaultiFolderDirectory = "My iFolders";
	}
}
