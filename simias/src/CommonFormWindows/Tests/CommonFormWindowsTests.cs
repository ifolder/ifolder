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
using System.Threading;

using NUnit.Framework;

using Simias;

namespace Simias.Tests
{
	/// <summary>
	/// Common Form Windows Tests
	/// </summary>
	[TestFixture]
	public class CommonFormWindowsTests : Assertion
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CommonFormWindowsTests));

		/// <summary>
		/// Default Constructor
		/// </summary>
		public CommonFormWindowsTests()
		{
		}

		/// <summary>
		/// Test the MyTraceForm
		/// </summary>
		[Test]
		public void TestMyTraceForm()
		{
			MyTraceForm form = new MyTraceForm();
			form.Show();
			form.Refresh();

			log.Debug("test1");
			form.Refresh();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			log.Debug("test2");
			form.Refresh();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			log.Debug("test3");
			form.Refresh();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			MyTrace.WriteLine("test4");
			form.Refresh();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			MyTrace.WriteLine("test5");
			form.Refresh();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			form.Close();
			form.Dispose();
			form = null;
		}
	}
}
