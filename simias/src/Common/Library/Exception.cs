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
			logger.Debug(this, this.GetType().ToString(), null);
		}
		
		/// <summary>
		/// Constructs a SimiasException.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		public SimiasException(string message) :
			base(message)
		{
			logger.Debug(this, Message, null);
		}

		/// <summary>
		/// Constructs a SimiasException.
		/// </summary>
		/// <param name="message">The message describing the exception.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public SimiasException(string message, SystemException innerException) :
			base(message, innerException)
		{
			logger.Debug(this, Message, null);
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
}
