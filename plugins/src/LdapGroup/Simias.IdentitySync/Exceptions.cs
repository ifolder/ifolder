/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections;

using Simias;

namespace Simias.Identity
{
	/// <summary>
	/// Server Exception
	/// </summary>
	public class ServerException : SimiasException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ServerException() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public ServerException( string message ) : base( message )
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="e"></param>
		public ServerException( string message, Exception e ) : base( message, e )
		{
		}
	}

	/// <summary>
	/// Authentication Failed
	/// </summary>
	public class AuthenticationException : ServerException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public AuthenticationException() : base()
		{
		}
	}

	/// <summary>
	/// Authorization Failed
	/// </summary>
	public class AuthorizationException : ServerException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public AuthorizationException( string message ) : base( message )
		{
		}
	}

	/// <summary>
	/// Specified member already exists
	/// </summary>
	public class MemberNameAlreadyExists : ServerException
	{
		private string membername;
		static private string basemessage = "The specified member already exists in the server domain";

		/// <summary>
		/// Returns the specified member name that did not exist
		/// </summary>
		public string MemberName
		{
			get
			{
				return membername;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="MemberName"></param>
		public MemberNameAlreadyExists( string MemberName ) : base( MemberNameAlreadyExists.basemessage )
		{
			membername = MemberName;
		}
	}

	/// <summary>
	/// Specified member does not exist
	/// </summary>
	public class MemberNameDoesNotExist : ServerException
	{
		private string membername;
		static private string basemessage = "The specified member does not exist in the server domain";

		/// <summary>
		/// Returns the specified member name that did not exist
		/// </summary>
		public string MemberName
		{
			get
			{
				return membername;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="MemberName"></param>
		public MemberNameDoesNotExist( string MemberName ) : base( MemberNameDoesNotExist.basemessage )
		{
			membername = MemberName;
		}
	}

}
