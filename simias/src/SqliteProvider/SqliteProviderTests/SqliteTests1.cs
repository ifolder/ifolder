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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using System.Xml;
using Simias.Storage.Provider;
using Simias.Storage.Provider.Sqlite;

namespace Simias.Storage.Provider.Sqlite.Tests
{
	/// <summary>
	/// Unit Test for the Flaim Storage Provider.
	/// </summary>
	/// 
	[TestFixture]
	public class SqliteTests1 : Simias.Storage.Provider.Tests
	{
		/// <summary>
		/// Used to setup all the needed resources before the tests are run.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			base.Init("SqliteProvider.dll", "Simias.Storage.Provider.Sqlite.SqliteProvider");
		}

	}
}
