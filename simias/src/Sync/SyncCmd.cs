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
 *  Author: Dale Olds <olds@novell.com>
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Diagnostics;

namespace Simias.Sync.Cmd
{

//---------------------------------------------------------------------------
/// <summary>
/// command line execution of sync operations
/// </summary>
public class SyncCmd
{
	int port = 8088;
	string host = null;

	int Invite(string user, Uri docRoot, string invitationFile)
	{
		if (host == null)
			host = MyDns.GetHostName();
		FileInviter fi = new FileInviter();
		if (!fi.Invite(user, docRoot, host, port, invitationFile))
		{
			Console.WriteLine("could not make invitation");
			return 20;
		}
		return 0;
	}

	int Accept(string invitationFile, string docRootParent)
	{
		FileInviter fi = new FileInviter();
		return fi.Accept(docRootParent, invitationFile)? 0: 30;
	}

	int RunServer()
	{
		if (host == null)
			host = MyDns.GetHostName();
		CmdServer server = new CmdServer(host, port);
		Console.WriteLine("server {0} started, press enter to exit", port);
		Console.ReadLine();
		server.Stop();
		return 0;
	}

	int Usage(string errMsg)
	{
		string name = Process.GetCurrentProcess().ProcessName;
		string[] usage =
		{
			"",
			"Usage: " + name + " [options] operation operationParams",
			"",
			"    operations:",
			"        invite folder invitationFile [userToInvite]",
			"            prints the invitation info to the invitationFile",
			"            initializes the collection store if necessary",
			"",
			"        accept invitationFile folderRoot",
			"            accepts the invitation into the collection store",
			"",
			"        sync folder",
			"            does one sync of specified folder",
			"",
			"        localsync folder localServerStore",
			"            does one sync of specified folder",
			"",
			"        server",
			"            runs as simple server",
			"",
			"    options:",
			"        -s storeLocation",
			"        -l traceLevel (off, error, warning, info, verbose)",
			"        -c traceClass (all or class name)",
			"        -p port (used by server and invite operations)",
			"        -n (name to use for local host, can be ip address)",
			"",
		};
		if (errMsg != null)
		{
			Console.WriteLine();
			Console.WriteLine(errMsg);
		}
		foreach (string s in usage)
			Console.WriteLine(s);
		return errMsg == null? 0: 10;
	}

	int Run(string[] args)
	{
		if (args.Length == 0)
			return Usage(null);

		for (int i = 0; i < args.Length;)
		{
			string s = args[i++];
			if (s.StartsWith("-"))
			{
				if (i == args.Length)
					return Usage("incomplete option");
				if (s == "-s")
					Configuration.CreateDefaultConfig(Path.GetFullPath(args[i++]));
				else if (s == "-p")
				{
					port = Int32.Parse(args[i++]);
					if (port == 0)
						return Usage("invalid port: 0");
				}
				else if (s == "-l")
				{
					//if (!Log.SetLevel(args[i++]))
					//	return Usage("Unknown trace level");
					Console.WriteLine("trace level option '-l' not supported"); 
				}
				else if (s == "-c")
				{
					//Log.SetCategory(args[i++]);
					Console.WriteLine("category option '-c' not supported"); 
				}
				else if (s == "-n")
					host = args[i++];
				else
					return Usage("unknown option");
			}
			else switch (s)
			{
				case "invite":
					if (args.Length - i != 2 && args.Length - i != 3)
						return Usage("operation 'invite' takes 2 or 3 params");
					return Invite(args.Length - i == 2? null: args[i + 2], new Uri(Path.GetFullPath(args[i])), args[i + 1]);
				case "accept":
					if (args.Length - i != 2)
						return Usage("operation 'accept' takes 2 params");
					return Accept(args[i], args[i + 1]);
				case "sync":
					if (args.Length - i != 1)
						return Usage("operation 'sync' takes 1 param");
					return CmdClient.RunOnce(new Uri(Path.GetFullPath(args[i])), null)? 0: 40;
				case "localsync":
					if (args.Length - i != 2)
						return Usage("operation 'localsync' takes 2 params");
					return CmdClient.RunOnce(new Uri(Path.GetFullPath(args[i])), args[i + 1])? 0: 41;
				case "server":
					if (args.Length - i != 0)
						return Usage("operation 'server' takes 0 params");
					return RunServer();
				default:
					return Usage("unknown operation '" + s + "'");
			}
		}
		return Usage("no operation specified");
	}

	/// <summary>
	/// command line execution of sync operations, run with no args for usage
	/// </summary>
	public static int Main(string[] args)
	{
		try
		{
			Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
			SyncCmd cmd = new SyncCmd();
			return cmd.Run(args);
		}
		catch (Exception e)
		{
			Console.WriteLine("Uncaught exception in main: {0}", e.Message);
			Console.WriteLine(e.StackTrace);
		}
		catch
		{
			Console.WriteLine("Uncaught foreign exception in main");
		}
		finally
		{
			// just trying to get repeatable results
			//GC.Collect();
			//Thread.Sleep(1000);
		}
		return 42;
	}

}

//===========================================================================
}
