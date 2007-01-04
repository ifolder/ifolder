/***********************************************************************
 *  $RCSfile: iFolder.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

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
