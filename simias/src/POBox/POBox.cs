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
			base (storeObject, node)
		{
		}

		public POBox(Store storeObject, string name) :
			base (storeObject, name)
		{
			SetType(this, typeof(POBox).Name);
		}
		#endregion

		#region Private Methods
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a message to the POBox object.
		/// </summary>
		/// <param name="message">The message to add to the collection.</param>
		public void AddMessage(Message message)
		{
			Commit(message);
		}

		/// <summary>
		/// Adds an array of Message objects to the POBox object.
		/// </summary>
		/// <param name="messageList">An array of Message objects to add to the POBox object.</param>
		public void AddMessage(Message[] messageList)
		{
			Commit(messageList);
		}

		/// <summary>
		/// Get all the Message objects that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified name.</returns>
		public ICSList GetMessagesByName(string name)
		{
			return this.GetNodesByName(name);		
		}

		/// <summary>
		/// Get all the Message objects that have the specified type.
		/// </summary>
		/// <param name="type">A string containing the type to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified type.</returns>
		public ICSList GetMessagesByType(string type)
		{
			return this.GetNodesByType(type);
		}
		#endregion
	}
}
