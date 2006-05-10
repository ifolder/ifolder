/***********************************************************************
 *  $RCSfile: Certificate.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web;
using System.Xml;

namespace iFolder.Client
{
	// bugbug temporary.  Need to create a certificate store
	// prompt user to accept invalid certs etc.
	public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
	{
		public TrustAllCertificatePolicy()
		{}

		public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem )
		{
			return true;
		}
	}
}
