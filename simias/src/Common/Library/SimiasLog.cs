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
	/// Simias Log
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

		public void Debug(string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args));
			}
		}

		public void Info(string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args));
			}
		}

		public void Warn(string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args));
			}
		}

		public void Error(string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args));
			}
		}

		public void Fatal(string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args));
			}
		}

		public void Debug(Exception e, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args), e);
			}
		}

		public void Info(Exception e, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args), e);
			}
		}

		public void Warn(Exception e, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args), e);
			}
		}

		public void Error(Exception e, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args), e);
			}
		}

		public void Fatal(Exception e, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args), e);
			}
		}
	}
}
