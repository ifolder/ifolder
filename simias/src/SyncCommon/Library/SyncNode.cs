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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Node
	/// </summary>
	public class SyncNode : Node
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SyncNode(Node node) : base(node)
		{
			this.baseNode = node;
		}

		/// <summary>
		/// Get a property value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <returns>The value of the property.</returns>
		public object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		/// <summary>
		/// Get a poperty value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="defaultValue">A default value to return if the property has no value.</param>
		/// <returns>The property value, if it exists, or the default value.</returns>
		public object GetProperty(string name, object defaultValue)
		{
			object result = defaultValue;

			Property p = baseNode.Properties.GetSingleProperty(name);

			if (p != null)
			{
				result = p.Value;
			}

			return result;
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		public void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="local">Is this a local only property? (non-synced)</param>
		public void SetProperty(string name, object value, bool local)
		{
			if (value != null)
			{
				Property p = new Property(name, value);
				p.LocalProperty = local;

				baseNode.Properties.ModifyProperty(p);
			}
			else
			{
				baseNode.Properties.DeleteSingleProperty(name);
			}
		}
	}
}
