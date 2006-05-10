/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2006 Novell, Inc.
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
//using log4net;

namespace iFolder.Client
{
	/// <summary>
	/// Summary description for Exception.
	/// </summary>
	public class NifException : System.Exception
	{
		//private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SimiasException));

		/// <summary>
		/// Constructs an iFolder Client exception
		/// </summary>
		public NifException()
		{
			//logger.Debug(this, this.GetType().ToString());
		}
		
		/// <summary>
		/// Constructs an iFolder Client exception with a message.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		public NifException( string message ) :
			base( message )
		{
			//logger.Debug(this, Message);
		}

		/// <summary>
		/// Constructs an iFolder Client exception.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public NifException( string message, Exception innerException ) :
			base( message, innerException )
		{
			//logger.Debug(this, Message);
		}

		/*
		/// <summary>
		/// Logs the exception as an error. Only logs the message
		/// </summary>
		public void LogError()
		{
			logger.Error(this.Message);
		}

		/// <summary>
		/// Logs the exception as fatal. Logs the message and the stack trace.
		/// </summary>
		public void LogFatal()
		{
			logger.Fatal(this.Message, this);
		}
		*/
	}

	/// <summary>
	/// Create iFolder Exception.
	/// Failed creating an iFolder.
	/// </summary>
	public class CreateiFolderException : NifException
	{
		private string name;
		private string domainid;

		/// <summary>
		/// iFolder Name
		/// </summary>
		public string Name
		{
			get 
			{ 
				return name;
			}
		}

		/// <summary>
		/// Domain ID
		/// </summary>
		public string DomainID
		{
			get 
			{ 
				return domainid;
			}
		}

		/// <summary>
		/// Generate a create ifolder exception.
		/// </summary>
		/// <param name="name">The name caller attempted to create with.</param>
		/// <param name="domainID">Domain the caller attempted to create the iFolder in</param>
		public CreateiFolderException( string domainID, string name ) :
			base( string.Format( "Failed creating iFolder {0} in domain {1}", name, domainID ) )
		{
			this.domainid = domainID;
			this.name = name;
		}
	}

	/// <summary>
	/// UnknownDomain Exception.
	/// The specified domain does not exist.
	/// </summary>
	public class UnknownDomainException : NifException
	{
		private string id;

		/// <summary>
		/// Specified user
		/// </summary>
		public string DomainID
		{
			get 
			{ 
				return id;
			}
		}
		/// <summary>
		/// Generate an unknown domain exception.
		/// </summary>
		/// <param name="domainID">The specified domain id.</param>
		public UnknownDomainException( string domainID ) :
			base( string.Format( "The specified domain {0} does not exist", domainID ) )
		{
			this.id = domainID;
		}
	}

	/// <summary>
	/// UnknowniFolder Exception.
	/// The specified iFolder does not exist.
	/// </summary>
	public class UnknowniFolderException : NifException
	{
		private string id;

		/// <summary>
		/// Specified user
		/// </summary>
		public string iFolderID
		{
			get 
			{ 
				return id;
			}
		}
		/// <summary>
		/// Generate an unknown ifolder exception.
		/// </summary>
		/// <param name="iFolderID">The specified iFolder.</param>
		public UnknowniFolderException( string iFolderID ) :
			base( string.Format( "The specified iFolder {0} does not exist", iFolderID ) )
		{
			this.id = iFolderID;
		}
	}

	/// <summary>
	/// UnknownUser Exception.
	/// The specified user does not exist in the domain.
	/// </summary>
	public class UnknownUserException : NifException
	{
		private string user;

		/// <summary>
		/// Specified user
		/// </summary>
		public string User
		{
			get 
			{ 
				return user;
			}
		}
		/// <summary>
		/// Generate an unknown user exception.
		/// </summary>
		/// <param name="user">The specified user.</param>
		public UnknownUserException( string user ) :
			base( string.Format( "The specified user {0} does not exist in the target domain", user ) )
		{
			this.user = user;
		}
	}

	/// <summary>
	/// Version Exception.
	/// </summary>
	public class VersionException : NifException
	{
		/// <summary>
		/// Generate a version exception.
		/// </summary>
		/// <param name="obj">The object whose version was invalid.</param>
		/// <param name="v1">The version.</param>
		/// <param name="v2">The expected version.</param>
		public VersionException( string obj, string v1, string v2 ) :
			base( string.Format( "The {0} Version is {1} expected {2}.", obj, v1, v2 ) )
		{
		}
	}

	/// <summary>
	/// Grace Login Exception.
	/// </summary>
	public class LoginGraceException : NifException
	{
		private int total;
		private int remaining;

		/// <summary>
		/// Total grace logins for the user
		/// </summary>
		public int TotalGraceLogins
		{
			get 
			{ 
				return total;
			}
		}

		/// <summary>
		/// Remaining grace logins for the user
		/// </summary>
		public int RemainingGraceLogins
		{
			get 
			{ 
				return remaining;
			}
		}

		/// <summary>
		/// Generate a grace login acception.
		/// </summary>
		/// <param name="message">grace login exception message.</param>
		/// <param name="total">The number of configured total grace logins.</param>
		/// <param name="remaining">The number of remaining grace logins.</param>
		public LoginGraceException( string message, int total, int remaining ) :
			base( string.Format( "{0}  Total grace logins {1}  Remaining grace logins {2}", message, total, remaining ) )
		{
			this.total = total;
			this.remaining = remaining;
		}
	}

	/// <summary>
	/// Login Exception.
	/// </summary>
	public class LoginException : NifException
	{
		private LoginStatus loginstatus;

		/// <summary>
		/// Login status returned from the remote server
		/// </summary>
		public LoginStatus LoginStatus
		{
			get 
			{ 
				return loginstatus;
			}
		}

		/// <summary>
		/// Generate a login failure exception.
		/// </summary>
		/// <param name="status">login failure status code.</param>
		public LoginException( LoginStatus status ) :
			base( string.Format( "Login failed  Status Code: {0}", status ) )
		{
			loginstatus = status;
		}
	}
}
