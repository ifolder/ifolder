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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/
using System;

namespace Novell.iFolder
{
	/// <summary>
	/// A class used to define an excluded directory.
	/// </summary>
	public class ExcludeDirectory
	{
		private string name;
		private bool environment;
		private bool deep;

		/// <summary>
		/// Instantiates an ExcludeDirectory object.
		/// </summary>
		/// <param name="name">The path to exclude.</param>
		/// <param name="environment">Specifies if <b>name</b> holds an environment variable.</param>
		/// <param name="deep">Specifies if subdirectories of <b>name</b> should also be excluded.</param>
		public ExcludeDirectory(string name, bool environment, bool deep)
		{
			this.name = name;
			this.environment = environment;
			this.deep = deep;
		}

		/// <summary>
		/// Returns the name of the path to exclude.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Returns <b>true</b> if <b>Name</b> holds an environment variable.
		/// </summary>
		public bool Environment
		{
			get { return environment; }
		}

		/// <summary>
		/// Returns <b>true</b> if subdirectories should also be excluded.
		/// </summary>
		public bool Deep
		{
			get { return deep; }
		}
	}
}
