/***********************************************************************
 *  iFolderComponentTests.cs - Unit tests for iFolder COM.
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
 *  Author: Bruce Getter <bgetter@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Novell.iFolder.iFolderCom;

namespace Novell.iFolder.iFolderCom.Tests
{
	[TestFixture]
	public class Iteration1Tests
	{
		private iFolderComponent ifoldercom;
		[SetUp]
		public void Init()
		{
			//ifoldercom = new iFolderComponent(new Uri(Directory.GetCurrentDirectory()));
//			ifoldercom = new iFolderComponent();
		}

		[Test]
		public void Iteration1_Test()
		{
/*			string path = Directory.GetCurrentDirectory();

			bool btest = ifoldercom.IsiFolderNode(path);
			if (!ifoldercom.IsiFolder(path))
			{
				Console.WriteLine("Making " + path + " an iFolder");
				if (ifoldercom.CreateiFolder(path))
				{
					// verify this path is now an iFolder.
					if (!ifoldercom.IsiFolder(path))
					{
						throw new ApplicationException(path + " was not made an iFolder");
					}

					Console.WriteLine("iFolder created successfully for " + path);

					if (ifoldercom.GetiFolderAclInit())
					{
						string guid, name;
						int rights;

						ifoldercom.GetNextiFolderAce(out guid, out name, out rights);
					}
				}
			}

			ifoldercom.DeleteiFolder(path);*/
		}

		[TearDown]
		public void Cleanup()
		{
		}
	}
}
