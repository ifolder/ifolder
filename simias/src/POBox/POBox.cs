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
		internal POBox(Store storeObject, Node node) :
			base (storeObject, node)
		{
		}

		internal POBox(Store storeObject, string collectionName, string domainName) :
			base (storeObject, collectionName, domainName)
		{
			SetType(this, typeof(POBox).Name);
		}
		#endregion

		#region Private Methods
		#endregion

		#region Public Methods
		public static POBox GetPOBox(Store storeObject, string domainName)
		{
			POBox poBox = null;

			// Search for an existing POBox
			ICSList list = storeObject.GetCollectionsByType(typeof(POBox).Name);
			foreach (ShallowNode shallowNode in list)
			{
				Collection collection = new Collection(storeObject, shallowNode);
				if (collection.Domain.Equals(domainName))
				{
					poBox = new POBox(storeObject, collection);
					break;
				}
			}

			// If one cannot be found then create it.
			if (poBox == null)
			{
				poBox = new POBox(storeObject, "POBox", domainName);
				poBox.Commit();
			}

			return poBox;
		}

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
		public ICSList GetMessagesByMessageType(string type)
		{
			return this.Search(Message.MessageTypeProperty, type, SearchOp.Equal);
		}
		#endregion
	}
}
