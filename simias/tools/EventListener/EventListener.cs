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
using System.IO;
using Simias.Event;
using Simias.Storage;

namespace Simias.Event.Util
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class EventListener
	{
		//private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(EventListener));

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Configuration conf = null;

			if (args.Length != 0)
			{
				try
				{
					if (Directory.Exists(args[0]))
					{
						conf = Configuration.CreateDefaultConfig(args[0]);
					}
				}
				catch
				{
				}
				if (conf == null)
				{
					if (MyEnvironment.Windows)
						Console.WriteLine("Usage: EventListener [store path]");
					else
						Console.WriteLine("Usage: mono EventListener [store path]");
				}
			}
			EventSubscriber subscriber = new EventSubscriber();

			subscriber.CollectionRootChanged += new CollectionRootChangedHandler(subscriber_CollectionRootChanged);
			subscriber.FileChanged += new FileEventHandler(subscriber_FileChanged);
			subscriber.FileCreated += new FileEventHandler(subscriber_FileCreated);
			subscriber.FileDeleted += new FileEventHandler(subscriber_FileDeleted);
			subscriber.FileRenamed += new FileRenameEventHandler(subscriber_FileRenamed);
			subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
			subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
			subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);

			Console.WriteLine("Press Enter to exit:");
			Console.ReadLine();
		}

		private static void subscriber_CollectionRootChanged(CollectionRootChangedEventArgs args)
		{
			Console.WriteLine("Changed Root Dir: {0} Type: {1}", args.NewRoot, args.Type);
			//logger.Info("Changed Root Dir: {0} Type: {1}", args.NewRoot, args.Type);
		}

		private static void subscriber_FileChanged(FileEventArgs args)
		{
			Console.WriteLine("Changed File: {0}", args.FullPath);
			//logger.Info("Changed File: {0}", args.FullPath);
		}

		private static void subscriber_FileCreated(FileEventArgs args)
		{
			Console.WriteLine("Created File: {0}", args.FullPath);
			//logger.Info("Created File: {0}", args.FullPath);
		}

		private static void subscriber_FileDeleted(FileEventArgs args)
		{
			Console.WriteLine("Deleted File: {0}",  args.FullPath);
			//logger.Info("Deleted File: {0}",  args.FullPath);
		}

		private static void subscriber_FileRenamed(FileRenameEventArgs args)
		{
			Console.WriteLine("Renamed File: {0} to {1}", args.OldPath, args.FullPath);
			//logger.Info("Renamed File: {0} to {1}", args.OldPath, args.FullPath);
		}

		private static void subscriber_NodeChanged(NodeEventArgs args)
		{
			Console.WriteLine("Changed Node: {0} Type: {1}", args.Node, args.Type);
			//logger.Info("Changed Node: {0} Type: {1}", args.Node, args.Type);
		}

		private static void subscriber_NodeCreated(NodeEventArgs args)
		{
			Console.WriteLine("Created Node: {0} Type: {1}", args.Node, args.Type);
			//logger.Info("Created Node: {0} Type: {1}", args.Node, args.Type);
		}

		private static void subscriber_NodeDeleted(NodeEventArgs args)
		{
			Console.WriteLine("Deleted Node: {0} Type: {1}", args.Node, args.Type);
			//logger.Info("Deleted Node: {0} Type: {1}", args.Node, args.Type);
		}
	}
}
