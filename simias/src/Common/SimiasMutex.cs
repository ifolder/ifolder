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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
 

// This class is a temporary hack to get around the problems that
// exist with a Mutex and multiple processes

// It's slow, it's dirty, but it's damn solid


using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Simias
{
	public class SimiasMutex
	{
		private string muxPath;
		private FileStream fs;
		private Mutex mux;

		public SimiasMutex(string name)
		{
			if(MyEnvironment.Mono)
			{
				muxPath = fixupPath(name);
				mux = null;
			}
			else
			{
				fs = null;
				mux = new Mutex(false, name);
			}
		}

		public void WaitOne()
		{
			if(mux == null)
			{
				while(fs == null)
				{
					try
					{
						fs = new FileStream(muxPath, FileMode.OpenOrCreate,
										FileAccess.ReadWrite, FileShare.None);
					}
					catch(Exception e)
					{
						Thread.Sleep(100);
					}
				}
			}
			else
			{
				mux.WaitOne();
			}
		}

		public void ReleaseMutex()
		{
			if(mux == null)
			{
				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
			}
			else
			{
				mux.ReleaseMutex();
			}
		}

		private string fixupPath(string name)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (path == null || path.Length == 0)
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			path = Path.Combine(path, ".simias");
			path = Path.Combine(path, "mutex");

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			path = Path.Combine(path, name);

			return path;
		}
	}
}
