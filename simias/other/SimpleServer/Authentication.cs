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
using System.Xml;

using Simias;
using Simias.Authentication;
using Simias.Security.Web.AuthenticationService;
using Simias.Service;
using Simias.Storage;

using SCodes = Simias.Authentication.StatusCodes;


namespace Simias.SimpleServer
{
    [IAuthenticationServiceAttribute]
    public class Authentication : IAuthenticationService
    {
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Simias.Storage.Domain domain = null;
		private Simias.SimpleServer.Domain ssDomain = null;
		private Simias.Storage.Store store = null;
		private Simias.Storage.Roster roster = null;

		//private XmlDocument simpleServerDoc = null;
		//private XmlElement domainElement = null;

		#region Constructors
		/// <summary>
		/// One and only one constructor
		/// </summary>
		public  Authentication()
		{
			this.store = Store.GetStore();
			this.ssDomain = new Simias.SimpleServer.Domain( true );
			this.domain = store.GetDomain( ssDomain.ID );
			this.roster = this.domain.Roster;

			/*
			this.simpleServerDoc = new XmlDocument();
			this.simpleServerDoc.Load("SimpleServer.xml");
			domainElement = this.simpleServerDoc.DocumentElement;
			*/
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

		public Simias.Authentication.Status AuthenticateByName(string user, string password)
		{
			Simias.Authentication.Status status =
				new Simias.Authentication.Status(SCodes.Unknown);

			try
			{
				//
				// First verify the user exists in the SimpleServer roster
				//

				if (this.roster != null)
				{
					Simias.Storage.Member member = this.roster.GetMemberByName( user );
					if ( member != null )
					{
						Property pwd = member.Properties.GetSingleProperty( "SS:Password" );
						if (pwd != null)
						{
							if (password == (string) pwd.Value)
							{
								status.statusCode = SCodes.Success;
								status.UserID = member.UserID;
								status.UserName = member.Name;
							}
							else
							{
								status.statusCode = SCodes.InvalidPassword;
							}
						}
					}
					else
					{
						log.Debug( "Unknown user: " + user + " attempted to authenticate" );
						status.statusCode = SCodes.UnknownUser;
					}
				}
				else
				{
					log.Debug( "Failed to instantiate the Roster" );
				}
			}
			catch(Exception authEx)
			{
				log.Debug(authEx.Message);
				log.Debug(authEx.StackTrace);

				status.statusCode = SCodes.InternalException;
				status.ExceptionMessage = authEx.Message;
			}

			return status;
		}

		/// <summary>
		/// Authenticates the user using their unique ID and a password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		public Simias.Authentication.Status AuthenticateByID(string id, string password)
		{
			return new Simias.Authentication.Status(SCodes.MethodNotSupported);
		}
    }
}


