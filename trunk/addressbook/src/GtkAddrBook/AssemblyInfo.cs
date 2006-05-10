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

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.0.0")]
	[assembly: SatelliteContractVersion("1.0.0.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.0.0")]
	[assembly: SatelliteContractVersion("1.0.0.0")]
	[assembly: ComCompatibleVersion(1, 0, 0, 0)]
	[assembly: TypeLibVersion(1, 10)]
#endif

[assembly: AssemblyTitle("Novell.AddressBook.UI.gtk.dll")]
[assembly: AssemblyDescription("Novell.AddressBook.UI.gtk.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("iFolder development team")]
[assembly: AssemblyProduct("iFolder")]
[assembly: AssemblyCopyright("(c) 2004 Novell, Inc.")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(false)]
[assembly: AssemblyDefaultAlias("Novell.AddressBook.UI.gtk.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: ComVisible(false)]

//[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("../addressbook-snakeoil.keys")]
