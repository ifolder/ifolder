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
using System.IO;
using System.Reflection;

using log4net;
using log4net.Config;
using log4net.Appender;
using log4net.Repository;
using log4net.spi;
using log4net.Layout;

namespace Simias
{
	/// <summary>
	/// A light wrapper around the log4net LogManager class.
	/// </summary>
	public class SimiasLogManager
	{
		private static readonly string SimiasLogFile = "simias.log";
		private static readonly string SimiasPatternLayout = "%d [%t] %-5p %c - %m%n";
		
        private static bool configured = false;

		static SimiasLogManager()
		{
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		private SimiasLogManager()
		{
		}

		/// <summary>
		/// Create or retrieve the logger for the type in the Simias domain.
		/// </summary>
		/// <param name="type">The fully qualified name of the type is the name of the logger.</param>
		/// <returns>A Simias log interface object.</returns>
		public static ISimiasLog GetLogger(Type type)
		{
			return new SimiasLog(LogManager.GetLogger(type));
		}

		/// <summary>
		/// Configure the log manager to a specific Simias store.
		/// </summary>
		/// <param name="configuration">A Simias configuration object.</param>
		public static void Configure(Configuration configuration)
		{
			Configure(configuration.StorePath);
		}

		/// <summary>
		/// Configure the log manater to a specific Simias store.
		/// </summary>
		/// <param name="storePath">The full path to the store directory.</param>
		public static void Configure(String storePath)
		{
			lock(typeof(SimiasLogManager))
            {
                // only configure once
                if (!configured)
                {
                    // TODO: temp code for log file name
        			string[] args = Environment.GetCommandLineArgs();
        
        			string file = SimiasLogFile;
        
        			if ((args.Length > 0) && (args[0] != null) && (args[0].Length > 0) && !args[0].StartsWith("mono"))
        			{
        				file = Path.GetFileName(args[0]) + ".log";
        			}
        			else if ((args.Length > 1) && (args[1] != null) && (args[1].Length > 0))
        			{
        				file = Path.GetFileName(args[1]) + ".log";
        			}
        
        			// log file path
                    string filePath = Path.Combine(storePath, file);
        
                    // default appender
        			BasicConfigurator.Configure(new FileAppender(new PatternLayout(SimiasPatternLayout),
        				filePath));

                    configured = true;
                }
            }
		}
	}
}
