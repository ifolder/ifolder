#region Copyright (c) 2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using NUnit.Util;
using NUnit.Framework;
using Microsoft.Win32;

namespace NUnit.Tests.Util
{
	/// <summary>
	/// Summary description for SettingsGroupTests.
	/// </summary>
	[TestFixture]
	public class SettingsGroupTests
	{
		private RegistryKey testKey;

		public SettingsGroupTests()
		{
		}

		[SetUp]
		public void BeforeEachTest()
		{
			testKey = Registry.CurrentUser.CreateSubKey( "Software\\NunitTest" );
		}

		[TearDown]
		public void AfterEachTest()
		{
			testKey.Close();
			Registry.CurrentUser.DeleteSubKeyTree( "Software\\NunitTest" );
		}

		[Test]
		public void TopLevelSettings()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			Assert.IsNotNull( testGroup );
			Assert.AreEqual( "TestGroup", testGroup.Name );
			Assert.AreEqual( storage, testGroup.Storage );
			
			testGroup.SaveSetting( "X", 5 );
			testGroup.SaveSetting( "NAME", "Charlie" );
			Assert.AreEqual( 5, testGroup.LoadSetting( "X" ) );
			Assert.AreEqual( "Charlie", testGroup.LoadSetting( "NAME" ) );

			testGroup.RemoveSetting( "X" );
			Assert.IsNull( testGroup.LoadSetting( "X" ), "X not removed" );
			Assert.AreEqual( "Charlie", testGroup.LoadSetting( "NAME" ) );

			testGroup.RemoveSetting( "NAME" );
			Assert.IsNull( testGroup.LoadSetting( "NAME" ), "NAME not removed" );
		}

		[Test]
		public void SubGroupSettings()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			SettingsGroup subGroup = new SettingsGroup( "SubGroup", testGroup );
			Assert.IsNotNull( subGroup );
			Assert.AreEqual( "SubGroup", subGroup.Name );
			Assert.IsNotNull( subGroup.Storage );
			Assert.AreEqual( storage, subGroup.Storage.ParentStorage );

			subGroup.SaveSetting( "X", 5 );
			subGroup.SaveSetting( "NAME", "Charlie" );
			Assert.AreEqual( 5, subGroup.LoadSetting( "X" ) );
			Assert.AreEqual( "Charlie", subGroup.LoadSetting( "NAME" ) );

			subGroup.RemoveSetting( "X" );
			Assert.IsNull( subGroup.LoadSetting( "X" ), "X not removed" );
			Assert.AreEqual( "Charlie", subGroup.LoadSetting( "NAME" ) );

			subGroup.RemoveSetting( "NAME" );
			Assert.IsNull( subGroup.LoadSetting( "NAME" ), "NAME not removed" );
		}

		[Test]
		public void TypeSafeSettings()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			
			testGroup.SaveIntSetting( "X", 5);
			testGroup.SaveStringSetting( "Y", "17" );
			testGroup.SaveStringSetting( "NAME", "Charlie");

			Assert.AreEqual( 5, testGroup.LoadSetting("X") );
			Assert.AreEqual( 5, testGroup.LoadIntSetting( "X" ) );
			Assert.AreEqual( "5", testGroup.LoadStringSetting( "X" ) );

			Assert.AreEqual( "17", testGroup.LoadSetting( "Y" ) );
			Assert.AreEqual( 17, testGroup.LoadIntSetting( "Y" ) );
			Assert.AreEqual( "17", testGroup.LoadStringSetting( "Y" ) );

			Assert.AreEqual( "Charlie", testGroup.LoadSetting( "NAME" ) );
			Assert.AreEqual( "Charlie", testGroup.LoadStringSetting( "NAME" ) );
		}

		[Test]
		public void DefaultSettings()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			
			Assert.IsNull( testGroup.LoadSetting( "X" ) );
			Assert.IsNull( testGroup.LoadSetting( "NAME" ) );

			Assert.AreEqual( 5, testGroup.LoadSetting( "X", 5 ) );
			Assert.AreEqual( 6, testGroup.LoadIntSetting( "X", 6 ) );
			Assert.AreEqual( "7", testGroup.LoadStringSetting( "X", "7" ) );

			Assert.AreEqual( "Charlie", testGroup.LoadSetting( "NAME", "Charlie" ) );
			Assert.AreEqual( "Fred", testGroup.LoadStringSetting( "NAME", "Fred" ) );
		}

		[Test, ExpectedException( typeof( FormatException ) )]
		public void BadSetting1()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			testGroup.SaveSetting( "X", "1y25" );

			int x = testGroup.LoadIntSetting( "X" );
		}

		[Test, ExpectedException( typeof( FormatException ) )]
		public void BadSetting2()
		{
			RegistrySettingsStorage storage = new RegistrySettingsStorage( "Test", testKey );
			SettingsGroup testGroup = new SettingsGroup( "TestGroup", storage );
			testGroup.SaveSetting( "X", "1y25" );

			int x = testGroup.LoadIntSetting( "X", 12 );
		}
	}
}
