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
using log4net;

namespace Simias
{
	/// <summary>
	/// Summary description for Exception.
	/// </summary>
	public class SimiasException : Exception
	{
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SimiasException));

		/// <summary>
		/// Constructs a SimiasException.
		/// </summary>
		public SimiasException()
		{
			logger.Debug(this, this.GetType().ToString());
		}
		
		/// <summary>
		/// Constructs a SimiasException.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		public SimiasException(string message) :
			base(message)
		{
			logger.Debug(this, Message);
		}

		/// <summary>
		/// Constructs a SimiasException.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public SimiasException(string message, Exception innerException) :
			base(message, innerException)
		{
			logger.Debug(this, Message);
		}

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
	}

	/// <summary>
	/// Version Exception.
	/// </summary>
	public class VersionException : SimiasException
	{
		/// <summary>
		/// Generate a version exception.
		/// </summary>
		/// <param name="obj">The object whose version was invalid.</param>
		/// <param name="v1">The version.</param>
		/// <param name="v2">The expected version.</param>
		public VersionException(string obj, string v1, string v2) :
			base(string.Format("The {0} Version is {1} expected {2}.", obj, v1, v2))
		{
		}
	}

	/// <summary>
	/// Create Exception
	/// </summary>
	public class CreateException : SimiasException
	{
		/// <summary>
		/// Create a CreateException
		/// </summary>
		/// <param name="obj">The object that failed to create.</param>
		/// <param name="ex">The exception that caused this exception.</param>
		public CreateException(string obj, Exception ex) :
			base(string.Format("Failed to create {0}.", obj), ex)
		{
		}
	}

	/// <summary>
	/// Delete Exception
	/// </summary>
	public class DeleteException : SimiasException
	{
		/// <summary>
		/// Create a DeleteException
		/// </summary>
		/// <param name="obj">The object that failed to delete.</param>
		/// <param name="ex">The exception that caused this exception.</param>
		public DeleteException(string obj, Exception ex) :
			base(string.Format("Failed to delete {0}.", obj), ex)
		{
		}
	}

	/// <summary>
	/// Exists Exception
	/// </summary>
	public class ExistsException : SimiasException
	{
		/// <summary>
		/// Create a ExistsException.
		/// </summary>
		/// <param name="obj">The object that already existed.</param>
		public ExistsException(string obj) :
			base(string.Format("{0} already exists.", obj))
		{
		}
	}

	/// <summary>
	/// Malformed Exception
	/// </summary>
	public class MalformedException : SimiasException
	{
		/// <summary>
		/// Create a MalformedException
		/// </summary>
		/// <param name="obj">The malformed object.</param>
		public MalformedException(string obj) :
			base(string.Format("Malformed {0} object.", obj))
		{
		}
	}

	/// <summary>
	/// Not exist Exception
	/// </summary>
	public class NotExistException : SimiasException
	{
		/// <summary>
		/// Create a NotExistsException.
		/// </summary>
		/// <param name="obj">The object that does not exist.</param>
		public NotExistException(string obj) :
			base(string.Format("{0} does not exist.", obj))
		{
		}
	}

	/// <summary>
	/// Open Exception
	/// </summary>
	public class OpenException : SimiasException
	{
		/// <summary>
		/// Create an OpenException
		/// </summary>
		/// <param name="obj">The object that could not be opened.</param>
		public OpenException(string obj) :
			base(string.Format("Failed to open {0}.", obj))
		{
		}

		/// <summary>
		/// Could not open exception.
		/// </summary>
		/// <param name="obj">The object that could not be opened.</param>
		/// <param name="ex">The internal exception that caused this one.</param>
		public OpenException(string obj, Exception ex) :
			base(string.Format("Failed to open {0}.", obj), ex)
		{
		}
	}

	/// <summary>
	/// Search Exception.
	/// </summary>
	public class SearchException : SimiasException
	{
		/// <summary>
		/// Create a SearchException.
		/// </summary>
		/// <param name="expression">The search expression.</param>
		public SearchException(string expression) :
			base(string.Format("Invalid Search expression = {0}.", expression))
		{
		}
	}

	/// <summary>
	/// Need Credentials Exception.
	/// </summary>
	public class NeedCredentialsException : SimiasException
	{
		/// <summary>
		/// Create a NeedCredentialsException
		/// </summary>
		public NeedCredentialsException() :
			base ("Need Credentials")
		{
		}
	}
	
	/// <summary>
	/// Insufficient space Exception.
	/// </summary>
	public class InsufficientStorageException : SimiasException
	{
		/// <summary>
		/// Create a NeedCredentialsException
		/// </summary>
		public InsufficientStorageException() :
			base ("Insufficient Storage")
		{
		}
	}
}
