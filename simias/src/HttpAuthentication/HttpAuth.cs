/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 *
 ***********************************************************************/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.ComponentModel;
//using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using Simias;
using Simias.Storage;


namespace Simias.Authentication
{
	public class Session
	{
		public Session()
		{
			Requests = 0;
		}

		public GenericPrincipal User;
		public string			MemberID;
		public UInt64			Requests;
	}

	/// <summary>
	/// Summary description for Http
	/// </summary>
	public class Http
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Response headers set by the Http Authentication Module
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		public readonly static string SimiasErrorHeader = "Simias-Error";
		public readonly static string DomainIDHeader = "Domain-ID";

		private static readonly string sessionTag = "simias";
		private static readonly string[] rolesArray = { "users" };

		public Http()
		{
		}

		private 
			static 
			void 
			SetResponseHeaders( HttpContext ctx, Simias.Authentication.Status status )
		{
			switch ( status.statusCode )
			{
				case StatusCodes.SuccessInGrace:
				{
					ctx.Response.AppendHeader(
						GraceTotalHeader,
						status.TotalGraceLogins.ToString() );

					ctx.Response.AppendHeader(
						GraceRemainingHeader,
						status.RemainingGraceLogins.ToString() );
					break;
				}

				case StatusCodes.AccountDisabled:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountDisabled.ToString() );
					break;
				}

				case StatusCodes.AccountLockout:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountLockout.ToString() );
					break;
				}

				case StatusCodes.AmbiguousUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AmbiguousUser.ToString() );
					break;
				}

				case StatusCodes.UnknownUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.UnknownUser.ToString() );
					break;
				}

				case StatusCodes.Unknown:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.Unknown.ToString() );
					break;
				}

				case StatusCodes.InvalidCredentials:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InvalidCredentials.ToString() );
					break;
				}

				case StatusCodes.InvalidPassword:
				{
					ctx.Response.StatusCode = 401;
					/*
					context.Response.AppendHeader(
						Login.SimiasErrorHeader,
						StatusCodes.InvalidPassword.ToString() );
					*/
					break;
				}

				case StatusCodes.InternalException:
				{
					ctx.Response.StatusCode = 500;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InternalException.ToString() );
					break;
				}

				default:
					ctx.Response.StatusCode = 401;
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		static public Simias.Storage.Member GetMember( HttpContext ctx )
		{
			return GetMember( null, ctx );
		}
		

		/// <summary>
		/// Summary description for Http
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="ctx"></param>
		static public Simias.Storage.Member GetMember( string domainID, HttpContext ctx )
		{
			Simias.Authentication.Session simiasSession;
			Simias.Authentication.Status status;
			Simias.Storage.Domain domain;
			Simias.Storage.Member member = null;
			Store store = Store.GetStore();

			ctx.Response.Cache.SetCacheability( HttpCacheability.NoCache );

			//
			// Look for the special domain ID header in the request.  If the
			// header doesn't exist use the default domain
			//

			if ( domainID != null && domainID != "" )
			{
				domain = store.GetDomain( domainID );
			}
			else
			{
				domain = store.GetDomain( store.DefaultDomain );
			}

			if ( domain == null )
			{
				ctx.Response.StatusCode = 500;
				ctx.Response.StatusDescription = "Invalid Domain";
				ctx.ApplicationInstance.CompleteRequest();
				return null;
			}

			if ( ctx.Session != null )
			{
				simiasSession = ctx.Session[ sessionTag ] as Simias.Authentication.Session;
				if (simiasSession != null)
					ctx.User = simiasSession.User;

				if ( ctx.User.Identity.IsAuthenticated == false )
				{
					status = DomainProvider.Authenticate( domain, ctx );
					if ( status.statusCode != StatusCodes.Success &&
						status.statusCode != StatusCodes.SuccessInGrace )
					{
						Simias.Authentication.Http.SetResponseHeaders( ctx, status );
						if ( ctx.Response.StatusCode == 401 )
						{
							ctx.Response.AddHeader( 
								"WWW-Authenticate", 
								String.Concat("Basic realm=\"", domain.Name, "\""));
						}

						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					// Authentication modules are required to set the member's
					// userID on successful authentication - let's make sure
					if ( status.UserID == null || status.UserID == "")
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					member = domain.GetMemberByID( status.UserID );
					if ( member == null )
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					simiasSession = new Simias.Authentication.Session();
					simiasSession.MemberID = member.UserID;
					simiasSession.Requests++;
					ctx.Session[ sessionTag ] = simiasSession;

					// Setup a principal
					simiasSession.User = 
						new GenericPrincipal(
						new GenericIdentity(
						member.UserID,
						"Simias Authentication"), 
						//"Basic authentication"), 
						rolesArray);

					ctx.User = simiasSession.User;
					Thread.CurrentPrincipal = ctx.User;
				}
				else
				{
					simiasSession.Requests++;
					Thread.CurrentPrincipal = ctx.User;
					member = domain.GetMemberByID( simiasSession.MemberID );
				}
			}
			else
			{
				// No session exists so "authenticate" every request
			}

			return member;
		}
	}
}
