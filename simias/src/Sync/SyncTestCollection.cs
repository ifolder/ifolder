/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// A Sync Test Collection
	/// </summary>
	public class SyncTestCollection : IDisposable
	{
		SyncTestStore store;
		string name;
		string path;
		Uri uri;

		Collection collection;

		static byte[] junk;

		static SyncTestCollection()
		{
			byte b = 7;
			
			junk = new byte[1024];
			
			// TODO: Is there a better way in C#?
			for(int i=0; i < junk.Length; i++)
			{
				junk[i] = b;
			}
		}

		public SyncTestCollection(SyncTestStore store, string name, bool create)
		{
			this.store = store;
			this.name = name;
			this.path = Path.Combine(store.StorePath, name);
			this.uri = new Uri(Path.GetFullPath(path));
			
			if (create == true)
			{
				collection = store.BaseStore.CreateCollection(name, uri);
				collection.Commit(true);
			}
			else
			{
				ICSList list = store.BaseStore.GetCollectionsByName(name);
				IEnumerator e = list.GetEnumerator();

				if (e.MoveNext())
				{
					collection = (Collection)e.Current;
				}
				else
				{
					throw new ArgumentException("Collection Not Found", name);
				}
			}
		}

		public void CreateFile(string file, long size)
		{
			FileStream fs = File.Create(GetFilePath(file));

			for(int i=0; i < size; i++)
			{
				fs.Write(junk, 0, junk.Length);
			}
			
			fs.Close();
		}

		public void DeleteFile(string file)
		{
			File.Delete(GetFilePath(file));
		}

		public void RenameFile(string srcFile, string dstFile)
		{
			File.Move(GetFilePath(srcFile), GetFilePath(dstFile));
		}

		public long GetFileSize(string file)
		{
			long length;

			FileStream fs = File.OpenRead(GetFilePath(file));
			length = fs.Length;
			fs.Close();

			return length;
		}

		public byte[] ComputeHash(string file)
		{
			byte[] hash;

			FileStream fs = File.OpenRead(GetFilePath(file));
			hash = MD5.Create().ComputeHash(fs);
			fs.Close();

			return hash;
		}

		internal string GetFilePath(string file)
		{
			return Path.Combine(path, file);
		}

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion

		#region Properties

		internal string Name
		{
			get { return name; }
		}

		internal SyncTestStore Store
		{
			get { return store; }
		}

		internal Collection BaseCollection
		{
			get { return collection; }
		}

		#endregion
	}
}
