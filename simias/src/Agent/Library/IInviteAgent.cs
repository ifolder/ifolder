/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 * 
 ***********************************************************************/

using System;

using Simias.Storage;

namespace Simias.Agent
{
	/// <summary>
	/// Invitation Agent Interface
	/// </summary>
	public interface IInviteAgent
	{
		/// <summary>
		/// Generate a collection share invitation.
		/// </summary>
		/// <param name="identity">The user identity.</param>
		/// <param name="collection">The collection object.</param>
		/// <returns>The generated invitation object.</returns>
		Invitation Generate(string identity, Collection collection);

		/// <summary>
		/// Send a collection share invitation.
		/// </summary>
		/// <param name="identity">The user identity.</param>
		/// <param name="collection">The collection object.</param>
		/// <param name="message">A message describing the collection.</param>
		/// <returns>The generated invitation object.</returns>
		Invitation Generate(string identity, Collection collection, string message);

		/// <summary>
		/// Send a collection share invitation.
		/// </summary>
		/// <param name="identity">The user identity.</param>
		/// <param name="collection">The collection object.</param>
		void Invite(string identity, Collection collection);

		/// <summary>
		/// Send a collection share invitation.
		/// </summary>
		/// <param name="identity">The user identity.</param>
		/// <param name="collection">The collection object.</param>
		/// <param name="message">A message describing the collection.</param>
		void Invite(string identity, Collection collection, string message);

		/// <summary>
		/// Send a collection share invitation.
		/// </summary>
		/// <param name="invitation">The invitation object.</param>
		void Invite(Invitation invitation);

		/// <summary>
		/// Accept a collection share invitation to the local machine.
		/// </summary>
		/// <param name="invitation">The invitation object.</param>
		void Accept(Invitation invitation);

		/// <summary>
		/// The collection store path.
		/// </summary>
		string StorePath
		{
			get;
			set;
		}
	}
}
