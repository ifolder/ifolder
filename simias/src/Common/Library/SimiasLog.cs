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

using log4net;
using log4net.spi;

namespace Simias
{
	/// <summary>
	/// A light wrapper around the log4net ILog class.
	/// </summary>
	public class SimiasLog : ISimiasLog
	{
		private ILog log;

		/// <summary>
		/// Internal Constructor
		/// </summary>
		/// <param name="log">The ILog object.</param>
		internal SimiasLog(ILog log)
		{
			this.log = log;
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(Exception e, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(Exception e, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(Exception e, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(Exception e, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(Exception e, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args), e);
			}
		}
	}
}
