/***********************************************************************
 *  File: IAuthService.cs
 *  Author: Todd Throne (tthrone@novell.com)
 *  Date of Creation: October 2004.
 * 
 *  Namespace: Novell.Security.Web.AuthenticationService
 * 
 *  Interfaces defined: IAuthenticationService
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
 ***********************************************************************/

using System;
using System.IO;

//using Simias.Client;
//using Simias.Client.Authentication;

namespace Simias.Security.Web.AuthenticationService
{
    /// <summary>
	/// Attribute used to identify a class that implements the
	/// IAuthenticationService.
	/// 
	/// It is necessary to associate this attribute with the
	/// class that implements the interface to allow for your
	/// authentication service to be configured via Web Configuration.
	/// </summary>
	public class IAuthenticationServiceAttribute: System.Attribute {}


	/// <summary>
	/// Well known path for logging into a Simias domain
	/// An HttpRequest (get/post) can be issued against this path
	/// </summary>
	public class Login
	{
		public static string Path = "/simias10/login";

		// Response headers set by the Http Authentication Module
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		public readonly static string SimiasErrorHeader = "Simias-Error";
		public readonly static string DomainIDHeader = "Domain-ID";
	}

	/// <summary>
	/// Defines the AuthenticationService interface.
    /// </summary>
	public interface IAuthenticationService
	{
       /// <summary>
       /// Authenticates the user with password
       /// </summary>
       /// <returns>
       /// Returns a string representing the user name for the identity
       /// of the principle to be set on the current context and session.
       /// </returns>

	   string Authenticate(string user, string password);

		/// <summary>
		/// Authenticates the user by name and password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		Simias.Authentication.Status AuthenticateByName(string user, string password);

		/// <summary>
		/// Authenticates the user using their unique ID and a password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		Simias.Authentication.Status AuthenticateByID(string id, string password);
    }
}
