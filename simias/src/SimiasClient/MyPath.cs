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
using System.IO;

namespace Simias.Client
{
	/// <summary>
	/// Summary description for MyPath.
	/// </summary>
	public class MyPath
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		private MyPath()
		{
		}

		/// <summary>
		/// Get the local path of the path string.
		/// </summary>
		/// <param name="path">The file or directory path.</param>
		/// <returns>The local path.</returns>
		public static string GetLocalPath(string path)
		{
			string result = null;

			if (path != null)
			{
				result = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return result;
		}

		/// <summary>
		/// Get the full local path of the path string.
		/// </summary>
		/// <param name="path">The file or directory path.</param>
		/// <returns>The full local path.</returns>
		public static string GetFullLocalPath(string path)
		{
			string result = null;

			if (path != null)
			{
				result = Path.GetFullPath(GetLocalPath(path));
			}

			return result;
		}
	}
}
