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
 *  Author: Russ Young
 *
 ***********************************************************************/
using System;

namespace Simias.Client.Event
{
	/// <summary>
	/// Event args for a NeedCredentials Event.
	/// </summary>
	public class NeedCredentialsEventArgs : SimiasEventArgs
	{
		#region Fields
		
		string domainID;
		string collectionID;
		
		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a NeedCredentialsEventArgs for the specified domain and collection.
		/// </summary>
		/// <param name="domainID">The domain</param>
		/// <param name="collectionID">The collection.</param>
		public NeedCredentialsEventArgs(string domainID, string collectionID)
		{
			this.domainID = domainID;
			this.collectionID = collectionID;
		}

		/// <summary>
		/// Constructs a NeedCredetialsEventArgs for the specified domian.
		/// </summary>
		/// <param name="domainID"></param>
		public NeedCredentialsEventArgs(string domainID) :
			this(domainID, null)
		{
		}

		#endregion

		#region Properties
		/// <summary>
		/// Get the domain ID for which the credentials are needed.
		/// </summary>
		public string DomainID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Get the collection ID for which the credentials are need.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}

		#endregion
	}
}
