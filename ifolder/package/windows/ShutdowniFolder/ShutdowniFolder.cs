using System;
using System.Diagnostics;

namespace Novell.iFolder
{
	/// <summary>
	/// Summary description for ShutdowniFolder.
	/// </summary>
	class ShutdowniFolder
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Process[] ifolderProcesses = Process.GetProcessesByName("iFolderApp");

			foreach (Process process in ifolderProcesses)
			{
				process.Kill();
				process.Close();
			}
		}
	}
}
