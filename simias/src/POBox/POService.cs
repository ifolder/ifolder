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
using System.Diagnostics;

using Simias.Service;

namespace Simias.POBox
{
	/// <summary>
	/// PO Service
	/// </summary>
	public class POService : BaseProcessService
	{
		private static POService process;
		
		private Configuration config;
		private POManager manager;

		/// <summary>
		/// Constructor
		/// </summary>
		public POService() : base()
		{
			this.config = GetConfiguration();
			this.manager = new POManager(config);
		}

		static void Main(string[] args)
		{
			process = new POService();
			process.Run();
		}

		#region BaseProcessService Members

		/// <summary>
		/// Start the PO service.
		/// </summary>
		protected override void Start()
		{
		}

		/// <summary>
		/// Stop the PO service.
		/// </summary>
		protected override void Stop()
		{
		}

		/// <summary>
		/// Resume the PO service.
		/// </summary>
		protected override void Resume()
		{
		}

		/// <summary>
		/// Pause the PO service.
		/// </summary>
		protected override void Pause()
		{
		}

		/// <summary>
		/// A custom event for the PO service.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		protected override void Custom(int message, string data)
		{
		}

		#endregion
	}
}
