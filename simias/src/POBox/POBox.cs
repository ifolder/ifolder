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
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// A POBox object is a specialized collection used to hold messages.
	/// </summary>
	public class POBox : Collection
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a POBox object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this POBox belongs to.</param>
		/// <param name="node">Node object to construct POBox object from.</param>
		public POBox(Store storeObject, Node node) :
			base(storeObject, node)
		{
		}
		#endregion

		#region Private Methods
		#endregion

		#region Public Methods
		#endregion
	}
}
