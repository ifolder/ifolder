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
