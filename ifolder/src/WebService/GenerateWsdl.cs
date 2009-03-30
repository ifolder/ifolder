/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 | Author: Rob Lyon <rlyon@novell.com> 
 |**************************************************************************/

using System;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Web.Services.Description;

/// <summary>
/// Generate WSDL File
/// </summary>
class GenerateWsdl
{
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static int Main(string[] args)
	{
		int result = 0;

		if (args.Length == 0)
		{
			Console.WriteLine("USAGE: GenerateWsdl.exe [Assembly] [Type] [URL] [File]");
			result = -1;;
		}
		else
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(args[0]);
				Type type = assembly.GetType(args[1]);

				ServiceDescriptionReflector reflector = new ServiceDescriptionReflector();
				reflector.Reflect(type, args[2]);

				XmlTextWriter writer = new XmlTextWriter(args[3], Encoding.ASCII);
				writer.Formatting = Formatting.Indented;
				reflector.ServiceDescriptions[0].Write(writer);
				writer.Close();
			}
			catch(Exception ex)
			{
				Console.Error.WriteLine(ex);
				result = -1;
			}
		}

		return result;
	}
}
