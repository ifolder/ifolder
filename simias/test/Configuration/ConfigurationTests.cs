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
 *			Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/
 
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Simias;

namespace Simias.Tests
{
	[TestFixture]
	public class ConfigurationTests
	{
		private Configuration config;

		[TestFixtureSetUp]
		public void Init()
		{
			Console.WriteLine("");
			Console.WriteLine("=== Setting up Configuration Tests ===");
			config = Configuration.CreateDefaultConfig("./");
		}

		[Test]
		public void SetThenGet()
		{
			String str1, str2;

			str1 = "SetThenGet Test Value";

			Console.WriteLine("=== Running SetThenGet ===");

			config.Set("Section1", "SetThenGetKey", str1);
			str2 = config.Get("Section1", "SetThenGetKey", "Default SetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("SetThenGet didn't return matching values");
		}

		[Test]
		public void GetThenGet()
		{
			String str1, str2;

			str1 = "GetThenGet Test Value";

			Console.WriteLine("=== Running GetThenGet ===");

			str1 = config.Get("Section1", "GetThenGetKey", str1);
			str2 = config.Get("Section1", "GetThenGetKey", "Default GetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("GetThenGet didn't return matching values");
		}

		[Test]
		public void DefaultSetThenGet()
		{
			String str1, str2;

			str1 = "DefaultSetThenGet Test Value";

			Console.WriteLine("=== Running DefaultSetThenGet ===");

			config.Set("DefaultSetThenGetKey", str1);
			str2 = config.Get("DefaultSetThenGetKey", "Default DefaultSetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("DefaultSetThenGet didn't return matching values");
		}

		[Test]
		public void DefaultGetThenGet()
		{
			String str1, str2;

			str1 = "DefaultGetThenGet Test Value";

			Console.WriteLine("=== Running DefaultGetThenGet ===");

			config.Set("DefaultGetThenGetKey", str1);
			str2 = config.Get("DefaultGetThenGetKey", "Default DefaultGetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("DefaultGetThenGet didn't return matching values");
		}

		[Test]
		public void MassiveSetThenGetTest()
		{
			Console.WriteLine("=== Running MassiveSetThenGetTest ===");

			for(int x=0; x < 50; x++)
			{
				String key = "Key" + x;
				String value = "TestValueForTheMassiveSetTest" + x;
				config.Set("MassiveSection", key, value);
				Console.WriteLine("Setting Key : " + key);
			}

			for(int x=0; x < 50; x++)
			{
				String key = "Key" + x;
				String value = "TestValueForTheMassiveSetTest" + x;
				String newValue = config.Get("MassiveSection", key, "This is bogus, the value should be here" + x);
				if(newValue != value)
					throw new Exception("MassiveSetThenGetTest didn't pass");
			}
		}

		[Test]
		public void FunkyCharacterDudesTest()
		{
			Console.WriteLine("=== Running FunkyCharacterDudesTest ===");

			String key = "funnkykeyname";
			String value = "<test>!@#$%^&*()_+=-}{[]\\|:;'\"?/><></test>";
			config.Set("FunkyCharacterDudesSection", key, value);
			Console.WriteLine("Setting Key   = {0}", key);
			Console.WriteLine("Setting Value = {0}", value);

			String newValue = config.Get("FunkyCharacterDudesSection", key, "If you see this value, we are going to fail");
			Console.WriteLine("Read Key   = {0}", key);
			Console.WriteLine("Read Value = {0}", newValue);
			if(newValue != value)
				throw new Exception("FunkyCharacterDudesTest didn't pass");
		}

		[Test]
		public void MultipleSetThenGet()
		{
			String str2;

			Console.WriteLine("=== Running MultipleSetThenGet ===");

			config.Set("SetThenGetKey", "Some Value that should be overwritten");
			config.Set("SetThenGetKey", "Some Value that should be overwritten222");
			config.Set("SetThenGetKey", "Some Value that should be overwritten333");
			config.Set("SetThenGetKey", "Some Value that should be overwritten444");
			config.Set("SetThenGetKey", "Some Value that should be overwritten555");
			config.Set("SetThenGetKey", "This should be the final value");
			str2 = config.Get("SetThenGetKey", "Some value that should be ingored");
			if(str2 != "This should be the final value")
				throw new Exception("MultipleSetThenGet didn't return matching values");
		}

		[Test]
		public void SetThenGetSectionsWithSameChildren()
		{
			Console.WriteLine("=== Running SetThenGetSectionsWithSameChildren ===");
			string sectionAstuff = "SectionA's stuff";
			string sectionBstuff = "SectionB's stuff";
			string sectionCstuff = "SectionC's stuff";
			string keyName = "stuff";

			config.Set("SectionA", keyName, sectionAstuff);
			config.Set("SectionB", keyName, sectionBstuff);
			config.Set("SectionC", keyName, sectionCstuff);
			string testString = config.Get("SectionA", keyName, "This value shouldn't matter");
			if (testString != sectionAstuff)
			{
				throw new Exception("SetThenGetSectionsWithSameChildren didn't return the correct value");
			}

			testString = config.Get("SectionB", keyName, "This value shouldn't matter");
			if (testString != sectionBstuff)
			{
				throw new Exception("SetThenGetSectionsWithSameChildren didn't return the correct value");
			}

			testString = config.Get("SectionC", keyName, "This value shouldn't matter");
			if (testString != sectionCstuff)
			{
				throw new Exception("SetThenGetSectionsWithSameChildren didn't return the correct value");
			}
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			Console.WriteLine("=== Cleaning up Configuration Tests ===");
		}
	}
}


