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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/


using System;
using System.Reflection;

using log4net;

using Simias;
using Simias.Service;
using Simias.Storage;

using Novell.Security.Web.AuthenticationService;


namespace Simias.SimpleServer
{
    [IAuthenticationServiceAttribute]
    public class Authentication : IAuthenticationService
    {
		private static readonly log4net.ILog log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		#region Constructors
		/// <summary>
		/// One and only one constructor
		/// </summary>
		public  Authentication()
		{
		}
		#endregion


		/// <summary>
		/// Authenticates the user with password
		/// </summary>
        /// <returns>
		/// Returns a string representing the user name for the identity
		/// of the principle to be set on the current context and session.
		/// </returns>

		public string Authenticate(string user, string password)
		{
			return("");
		}

		/// <summary>
		/// Authenticates the user by name and password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		public AuthenticationStatus AuthenticateByName(string user, string password)
		{
			AuthenticationStatus status = new AuthenticationStatus(StatusCode.Success);
			return status;
		}

		/// <summary>
		/// Authenticates the user using their unique ID and a password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		public AuthenticationStatus AuthenticateByID(string id, string password)
		{
			AuthenticationStatus status = new AuthenticationStatus(StatusCode.Success);
			return status;
		}
    }
}


