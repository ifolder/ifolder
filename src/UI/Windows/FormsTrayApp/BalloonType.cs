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

namespace Novell.CustomUIControls
{
	/// <summary>
	/// The type of balloon to display.
	/// </summary>
	public enum BalloonType
	{
		/// <summary>
		/// No balloon type.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// An informational balloon type.
		/// </summary>
		Info = 0x1,

		/// <summary>
		/// A warning balloon type.
		/// </summary>
		Warning = 0x2,

		/// <summary>
		/// An error balloon type.
		/// </summary>
		Error = 0x3
	};
}
