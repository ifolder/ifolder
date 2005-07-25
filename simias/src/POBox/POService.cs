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
	public class POService : IThreadService
	{
		private POManager manager;

		/// <summary>
		/// Constructor
		/// </summary>
		public POService()
		{
		}

		#region BaseProcessService Members

		/// <summary>
		/// Start the PO service.
		/// </summary>
		public void Start(Configuration config)
		{
			this.manager = new POManager(config);
			manager.Start();
		}

		/// <summary>
		/// Stop the PO service.
		/// </summary>
		public void Stop()
		{
			Debug.Assert(manager != null);

			manager.Stop();
		}

		/// <summary>
		/// Resume the PO service.
		/// </summary>
		public void Resume()
		{
			Debug.Assert(manager != null);

			manager.Start();
		}

		/// <summary>
		/// Pause the PO service.
		/// </summary>
		public void Pause()
		{
			Debug.Assert(manager != null);

			manager.Stop();
		}

		/// <summary>
		/// A custom event for the PO service.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		#endregion
	}
}
