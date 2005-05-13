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
 *  Author: Mike Lasky <mlasky@novell.com>
 *			Russ Young
 *
 ***********************************************************************/
using System;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using Simias.Client;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for CertPolicy.
	/// </summary>
	public class CertPolicy : ICertificatePolicy
	{
		#region Constructor

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public CertPolicy()
		{
			ServicePointManager.CertificatePolicy = this;
		}

		#endregion

		#region ICertificatePolicy Members

		/// <summary>
		/// Implements the application certificate validation policy.
		/// </summary>
		/// <param name="srvPoint">The ServicePoint that will use the certificate.</param>
		/// <param name="certificate">The certificate to validate.</param>
		/// <param name="request">The request that received the certificate.</param>
		/// <param name="certificateProblem">The problem encountered when using the certificate.</param>
		/// <returns>True if the certificate is to be honored. Otherwise, false is returned.</returns>
		public bool CheckValidationResult( ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem )
		{
			return true;
		}

		#endregion
	}
}
