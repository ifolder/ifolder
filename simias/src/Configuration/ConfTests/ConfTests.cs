using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Simias;

namespace Simias.Tests
{
	[TestFixture]
	public class ConfTests
	{

		[TestFixtureSetUp]
		public void Init()
		{
			Console.WriteLine("");
			Console.WriteLine("=== Setting up Configuration Tests ===");
			Configuration.BaseConfigPath = "./";
		}

		[Test]
		public void SetThenGet()
		{
			String str1, str2;

			str1 = "SetThenGet Test Value";

			Console.WriteLine("=== Running SetThenGet ===");

			Configuration.Set("Section1", "SetThenGetKey", str1);
			str2 = Configuration.Get("Section1", "SetThenGetKey", "Default SetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("SetThenGet didn't return matching values");
		}

		[Test]
		public void GetThenGet()
		{
			String str1, str2;

			str1 = "GetThenGet Test Value";

			Console.WriteLine("=== Running GetThenGet ===");

			str1 = Configuration.Get("Section1", "GetThenGetKey", str1);
			str2 = Configuration.Get("Section1", "GetThenGetKey", "Default GetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("GetThenGet didn't return matching values");
		}

		[Test]
		public void DefaultSetThenGet()
		{
			String str1, str2;

			str1 = "DefaultSetThenGet Test Value";

			Console.WriteLine("=== Running DefaultSetThenGet ===");

			Configuration.Set("DefaultSetThenGetKey", str1);
			str2 = Configuration.Get("DefaultSetThenGetKey", "Default DefaultSetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("DefaultSetThenGet didn't return matching values");
		}

		[Test]
		public void DefaultGetThenGet()
		{
			String str1, str2;

			str1 = "DefaultGetThenGet Test Value";

			Console.WriteLine("=== Running DefaultGetThenGet ===");

			Configuration.Set("DefaultGetThenGetKey", str1);
			str2 = Configuration.Get("DefaultGetThenGetKey", "Default DefaultGetThenGet Test Value");
			if(str1 != str2)
				throw new Exception("DefaultGetThenGet didn't return matching values");
		}

		[Test]
		public void MassiveSetThenGetTest()
		{
			String str1;

			Console.WriteLine("=== Running MassiveSetThenGetTest ===");

			for(int x=0; x < 50; x++)
			{
				String key = "Key" + x;
				String value = "TestValueForTheMassiveSetTest" + x;
				Configuration.Set("MassiveSection", key, value);
				Console.WriteLine("Setting Key : " + key);
			}

			for(int x=0; x < 50; x++)
			{
				String key = "Key" + x;
				String value = "TestValueForTheMassiveSetTest" + x;
				String newValue = Configuration.Get("MassiveSection", key, "This is bogus, the value should be here" + x);
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
			Configuration.Set("FunkyCharacterDudesSection", key, value);
			Console.WriteLine("Setting Key   = {0}", key);
			Console.WriteLine("Setting Value = {0}", value);

			String newValue = Configuration.Get("FunkyCharacterDudesSection", key, "If you see this value, we are going to fail");
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

			Configuration.Set("SetThenGetKey", "Some Value that should be overwritten");
			Configuration.Set("SetThenGetKey", "Some Value that should be overwritten222");
			Configuration.Set("SetThenGetKey", "Some Value that should be overwritten333");
			Configuration.Set("SetThenGetKey", "Some Value that should be overwritten444");
			Configuration.Set("SetThenGetKey", "Some Value that should be overwritten555");
			Configuration.Set("SetThenGetKey", "This should be the final value");
			str2 = Configuration.Get("SetThenGetKey", "Some value that should be ingored");
			if(str2 != "This should be the final value")
				throw new Exception("MultipleSetThenGet didn't return matching values");
		}


		[TestFixtureTearDown]
		public void Cleanup()
		{
			Console.WriteLine("=== Cleaning up Configuration Tests ===");
		}
	}

	public class Tests
	{
		static void Main()
		{
			ConfTests tests = new ConfTests();
			tests.Init();
			tests.SetThenGet();
			tests.GetThenGet();
			tests.DefaultSetThenGet();
			tests.DefaultGetThenGet();
			tests.MassiveSetThenGetTest();
			tests.FunkyCharacterDudesTest();
			tests.MultipleSetThenGet();
			tests.Cleanup();
		}
	}
}


