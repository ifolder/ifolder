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

namespace Simias.Domain
{
	/// <summary>
	/// Simias Domain Service Interface
	/// </summary>
	public interface IDomainService
	{
		/// <summary>
		/// Get the domain information.
		/// </summary>
		/// <returns></returns>
		DomainInfo GetDomainInfo();

		/// <summary>
		/// Provision the user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		ProvisionInfo ProvisionUser(string user, string password);

		/// <summary>
		/// Create the master on the server.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="rootID"></param>
		/// <param name="rootName"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		string CreateMaster(string id, string name, string rootID, string rootName, string user);
	}
}
