using System;
using System.Web;

namespace SimiasTests
{
	/// <summary>
	/// Summary description for Command
	/// </summary>
	class Command
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool preAuth = false;
			bool verbose = false;
			bool walkUp = false;
			int iterations = 1;
			int size = 64;
			string wsUrl = "";
			string command = "";
			string host = "";
			string username = "";
			string password = "";

			if (args.Length == 0)
			{
				Command.PrintUsage();
				return;
			}

			char[] separator = {'='};
			string[] cmdValue = new string[2];

			for(int i = 0; i < args.Length; i++)
			{
				cmdValue = args[i].Split(separator, 2);
				string cmd = cmdValue[0].ToLower();
				if (cmd == "--url")
				{
					wsUrl = cmdValue[1];
				}
				else
				if (cmd == "--i")
				{
					iterations = Convert.ToInt32(cmdValue[1]);
				}
				else
				if (cmd == "--cmd")
				{
					command = cmdValue[1];
				}
				else
				if (cmd == "--size")
				{
					size = Convert.ToInt32(cmdValue[1]);
				}
				else
				if (cmd == "--walkup")
				{
					walkUp = Convert.ToBoolean(cmdValue[1]);
				}
				else
				if (cmd == "--verbose")
				{
					verbose = Convert.ToBoolean(cmdValue[1]);
				}
				else
				if (cmd == "--host")
				{
					host = cmdValue[1];
				}
				else
				if (cmd == "--username")
				{
					username = cmdValue[1];
				}
				else
				if (cmd == "--password")
				{
					password = cmdValue[1];
				}
				else
				if (cmd == "--preauth")
				{
					preAuth = Convert.ToBoolean(cmdValue[1]);
				}
			}

			if (command == "")
			{
				Command.PrintUsage();
				return;
			}

			if (wsUrl != "")
			{
				wsUrl += "/SimiasTests.asmx";
			}
			else
			{
				Command.PrintUsage();
				return;
			}

			Actions proxy = new Actions();
			proxy.Url = wsUrl;

			if (command.ToLower() == "getarray")
			{
				if (verbose == true)
				{
					Console.WriteLine("Exercising GetArray method");
					Console.WriteLine("  iterations: " + iterations.ToString());
					Console.WriteLine("  size: " + size.ToString());
					Console.WriteLine("  walkup: " + walkUp.ToString());
				}

				for(int i = 0; i < iterations; i++)
				{
					if (walkUp == false)
					{
						char[] returnChars = proxy.GetArray(size, 'A');
						returnChars = null;
					}
					else
					{
						for(int x = 1; x <= size; x++)
						{
							char[] returnChars = proxy.GetArray(x, 'A');
							returnChars = null;
						}
					}
				}
			}
			else
			if (command.ToLower() == "remoteauthentication")
			{
				if (verbose == true)
				{
					Console.WriteLine("Exercising RemoteAuthentication method");
					Console.WriteLine("  preauth: " + preAuth.ToString());
					Console.WriteLine("  host: " + host);
					Console.WriteLine("  username: " + username);
				}

				for(int i = 0; i < iterations; i++)
				{
					int status = proxy.RemoteAuthentication(preAuth, host, username, password);
					if (status != 0 && verbose == true)
					{
						Console.WriteLine("the RemoteAuthentication method failed");
					}
				}
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("usage: SimiasTestCmd --cmd=<Command> --url=<web service url> --i=<iterations> --verbose=<true|false>");
			Console.WriteLine("         cmd:<GetArray | Ping | RemoteAuthentication>");
			Console.WriteLine("         if cmd = GetArray then --size=numbercharstoreturn --walkup=true|false");
			Console.WriteLine("         if cmd = RemoteAuthentication them --host=name --username=user --password=pwd --preauth=true|false");
		}
	}
}
