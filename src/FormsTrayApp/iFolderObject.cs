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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// iFolder states.
	/// </summary>
	public enum iFolderState
	{
		/// <summary>
		/// The Normal state.
		/// </summary>
		Normal,

		/// <summary>
		/// The Synchronizing state.
		/// </summary>
		Synchronizing,

		/// <summary>
		/// The FailedSync state.
		/// </summary>
		FailedSync
	}

	/// <summary>
	/// Summary description for iFolderObject.
	/// </summary>
	public class iFolderObject
	{
		private iFolderWeb ifolderWeb;
		private iFolderState ifolderState;

		/// <summary>
		/// Constructs an iFolderObject object.
		/// </summary>
		/// <param name="ifolderWeb">The iFolderWeb object to base this object on.</param>
		public iFolderObject(iFolderWeb ifolderWeb)
		{
			this.ifolderWeb = ifolderWeb;
		}

		/// <summary>
		/// Gets/sets the iFolderWeb object.
		/// </summary>
		public iFolderWeb iFolderWeb
		{
			get { return ifolderWeb; }
			set { ifolderWeb = value; }
		}

		/// <summary>
		/// Gets/sets the iFolderState for this object.
		/// </summary>
		public iFolderState iFolderState
		{
			get { return ifolderState; }
			set { ifolderState = value; }
		}
	}
}
