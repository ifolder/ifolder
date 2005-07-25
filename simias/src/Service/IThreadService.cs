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

namespace Simias.Service
{
	/// <summary>
	/// Thread Service Interface.
	/// A Thread service must implement this interface.
	/// </summary>
	public interface IThreadService
	{
		/// <summary>
		/// Called to start the service.
		/// </summary>
		/// <param name="conf"></param>
		void Start(Configuration conf);
		/// <summary>
		/// Called to stop the service.
		/// </summary>
		void Stop();
		/// <summary>
		/// Called to pause the service.
		/// </summary>
		void Pause();
		/// <summary>
		/// Called to resume the service after a pause.
		/// </summary>
		void Resume();
		/// <summary>
		/// Called to process the service defined message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		int Custom(int message, string data);
	}
}
