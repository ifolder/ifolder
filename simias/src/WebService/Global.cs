/***********************************************************************
 *  $RCSfile$
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *          Rob Lyon <rlyon@novell.com>
 *
 ***********************************************************************/


using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.IO;

using Simias;

namespace Simias.Web
{
	/// <summary>
	/// Global
	/// </summary>
	public class Global : HttpApplication
	{
		private Simias.Service.Manager serviceManager;

		public Global()
		{
			Configuration config;

			config = Configuration.GetConfiguration();
			serviceManager = new Simias.Service.Manager(config);
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
			// CRG: This is a total hack for now
			string newdir = Directory.GetCurrentDirectory() +
								System.IO.Path.DirectorySeparatorChar + "bin";

			Console.WriteLine(newdir);

			Environment.CurrentDirectory = newdir;

			Console.WriteLine("Starting Simias Process");
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();
			Console.WriteLine("Simias Process Running");
		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{
		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(Object sender, 
							EventArgs e)
		{
		}

		protected void Application_Error(Object sender, EventArgs e)
		{
		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{
			Console.WriteLine("Shutdown Simias Process now");
			serviceManager.StopServices();
			serviceManager.WaitForServicesStopped();
		}
	}
}

