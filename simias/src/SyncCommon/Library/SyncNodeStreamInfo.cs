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

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Node Stream Info
	/// </summary>
	[Serializable]
	public class SyncNodeStreamInfo
	{
		private string id;
		private string name;
		private string type;
		private string path;

		public SyncNodeStreamInfo(SyncNodeStream stream)
		{
			this.id = stream.ID;
			this.name = stream.Name;
			this.type = stream.Type;
			this.path = stream.Path;
		}

		public string ID
		{
			get { return id; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Type
		{
			get { return type; }
		}

		public string Path
		{
			get { return path; }
		}
	}
}
