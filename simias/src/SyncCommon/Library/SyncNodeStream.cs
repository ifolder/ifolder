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
	/// Sync Node Stream
	/// </summary>
	public class SyncNodeStream : IDisposable
	{
		public static readonly string LocalLastWritePropertyName = "Local Last Write";

		private NodeStream baseStream;

		public SyncNodeStream(NodeStream nodeStream)
		{
			this.baseStream = nodeStream;
		}
		
		public object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		public object GetProperty(string name, object defaultValue)
		{
			object result = defaultValue;

			Property p = baseStream.Properties.GetSingleProperty(name);

			if (p != null)
			{
				result = p.Value;
			}

			return result;
		}

		public void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		public void SetProperty(string name, object value, bool local)
		{
			if (value != null)
			{
				Property p = new Property(name, value);
				p.LocalProperty = local;

				baseStream.Properties.ModifyProperty(p);
			}
			else
			{
				baseStream.Properties.DeleteSingleProperty(name);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.baseStream = null;
		}

		#endregion

		#region Properties

		public NodeStream BaseStream
		{
			get { return baseStream; }
		}

		public string ID
		{
			get { return baseStream.Id; }
		}

		public string Name
		{
			get { return baseStream.Name; }
		}

		public string Type
		{
			get { return baseStream.Type; }
		}

		public string Path
		{
			get { return baseStream.RelativePath; }
		}
		
		public DateTime LocalLastWrite
		{
			get { return (DateTime)GetProperty(LocalLastWritePropertyName, DateTime.MinValue); }
			set { SetProperty(LocalLastWritePropertyName, value, true); }
		}

		public DateTime LastWrite
		{
			get { return baseStream.LastWriteTime; }
			set { baseStream.LastWriteTime = value; }
		}

		#endregion
	}
}
