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

namespace Simias.Location
{
	/// <summary>
	/// Location Provider Interface
	/// </summary>
	public interface ILocationProvider
	{
		/// <summary>
		/// Configure the location provider.
		/// </summary>
		/// <param name="configuration">The Simias configuration object.</param>
		void Configure(Configuration configuration);
		
		/// <summary>
		/// Locate the collection master.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		/// <returns>A URI object containing the location of the collection master, or null.</returns>
		Uri Locate(string collection);
		
		/// <summary>
		/// Register a master collection.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		void Register(string collection);
		
		/// <summary>
		/// Unregister a master collection.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		void Unregister(string collection);
	}
}
