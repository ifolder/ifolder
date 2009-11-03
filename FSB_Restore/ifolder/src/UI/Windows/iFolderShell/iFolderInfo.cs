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
using System;
using System.Runtime.InteropServices;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for iFolderInfo.
	/// </summary>
	[ComVisible(false)]
	public class iFolderInfo
	{
		#region Class Members
		private string localPath;
		private string id;
		#endregion

		/// <summary>
		/// Constructs a ShallowiFolder object.
		/// </summary>
		public iFolderInfo()
		{
		}

		#region Properties
		/// <summary>
		/// Gets/sets the local path for this iFolder.
		/// </summary>
		public string LocalPath
		{
			get { return localPath; }
			set { localPath = value; }
		}

		/// <summary>
		/// Gets/sets the ID for this iFolder.
		/// </summary>
		public string ID
		{
			get { return id; }
			set { id = value; }
		}
		#endregion

		/// <summary>
		/// This method overrides Object.ToString().
		/// </summary>
		public override string ToString()
		{
			return localPath;
		}

	}
}
