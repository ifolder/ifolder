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
	public enum MessageState
	{
		New,
		Opened,
		Deleted
	};

	public enum MessageType
	{
		Inbound,
		Outbound
	};

	/// <summary>
	/// A Message object is a specialized node used to hold ...
	/// </summary>
	public class Message : Node
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Message object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the object.</param>
		public Message(string messageName) :
			base (messageName)
		{
		}
		#endregion
	}
}
