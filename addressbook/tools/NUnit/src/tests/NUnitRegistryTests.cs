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
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using NUnit.Framework;
using NUnit.Util;

namespace NUnit.Tests.Util
{
	/// <summary>
	/// Summary description for NUnitRegistryTests.
	/// </summary>
	[TestFixture]
	public class NUnitRegistryTests
	{
		public NUnitRegistryTests()
		{
		}

		[Test]
		public void CurrentUser()
		{
			using( RegistryKey key = NUnitRegistry.CurrentUser )
			{
				Assert.IsNotNull( key );
				Assert.AreEqual( @"HKEY_CURRENT_USER\Software\Nascent Software\Nunit", key.Name );
			}
		}

		[Test]
		public void LocalMachine()
		{
			using( RegistryKey key = NUnitRegistry.LocalMachine )
			{
				Assert.IsNotNull( key );
				Assert.AreEqual( @"HKEY_LOCAL_MACHINE\Software\Nascent Software\Nunit", key.Name );
			}
		}

		[Test]
		public void CurrentUserTestMode()
		{
			try
			{
				NUnitRegistry.TestMode = true;
				using( RegistryKey key = NUnitRegistry.CurrentUser )
				{
					Assert.IsNotNull( key );
					Assert.AreEqual( @"HKEY_CURRENT_USER\Software\Nascent Software\Nunit-Test", key.Name );
				}
			}
			finally
			{
				NUnitRegistry.TestMode = false;
			}
		}

		[Test]
		public void LocalMachineTestMode()
		{
			try
			{
				NUnitRegistry.TestMode = true;
				using( RegistryKey key = NUnitRegistry.LocalMachine )
				{
					Assert.IsNotNull( key );
					Assert.AreEqual( @"HKEY_LOCAL_MACHINE\Software\Nascent Software\Nunit-Test", key.Name );
				}
			}
			finally
			{
				NUnitRegistry.TestMode = false;
			}
		}

		[Test]
		public void TestClearRoutines()
		{
			RegistryKey key = null;
			RegistryKey foo = null;
			RegistryKey bar = null;
			RegistryKey footoo = null;

			try
			{
				NUnitRegistry.TestMode = true;
				key = NUnitRegistry.LocalMachine;
				foo = key.CreateSubKey( "foo" );
				bar = key.CreateSubKey( "bar" );
				footoo = foo.CreateSubKey( "foo" );

				key.SetValue("X", 5);
				key.SetValue("NAME", "Joe");
				foo.SetValue("Y", 17);
				bar.SetValue("NAME", "Jennifer");
				footoo.SetValue( "X", 5 );
				footoo.SetValue("NAME", "Charlie" );
				
				NUnitRegistry.ClearTestKeys();

				Assert.AreEqual( 0, key.ValueCount );
				Assert.AreEqual( 0, key.SubKeyCount );
				
			}
			finally
			{
				NUnitRegistry.TestMode = false;
				if ( key != null ) key.Close();
				if ( foo != null ) foo.Close();
				if ( bar != null ) bar.Close();
				if ( footoo != null ) footoo.Close();
			}
		}
	}
}
