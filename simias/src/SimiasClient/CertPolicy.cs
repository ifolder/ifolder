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
 *
 ***********************************************************************/
using System;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using Simias.Client;

namespace Simias.Client
{
	/// <summary>
	/// Summary description for CertPolicy.
	/// </summary>
	public class CertPolicy : ICertificatePolicy
	{
		internal class CertificateState
		{
			internal X509Certificate	Certificate;
			internal bool				Accepted;

			internal CertificateState(X509Certificate certificate, bool accepted)
			{
				this.Certificate = certificate;
				this.Accepted = accepted;
			}
		}

		#region Class Members

		static public Hashtable CertTable = new Hashtable();

		/// <summary>
		/// The default certificate policy.
		/// </summary>
		private ICertificatePolicy defaultCertPolicy;

		private  enum    CertificateProblem  : uint
		{
			CertEXPIRED                   = 0x800B0101,
			CertVALIDITYPERIODNESTING     = 0x800B0102,
			CertROLE                      = 0x800B0103,
			CertPATHLENCONST              = 0x800B0104,
			CertCRITICAL                  = 0x800B0105,
			CertPURPOSE                   = 0x800B0106,
			CertISSUERCHAINING            = 0x800B0107,
			CertMALFORMED                 = 0x800B0108,
			CertUNTRUSTEDROOT             = 0x800B0109,
			CertCHAINING                  = 0x800B010A,
			CertREVOKED                   = 0x800B010C,
			CertUNTRUSTEDTESTROOT         = 0x800B010D,
			CertREVOCATION_FAILURE        = 0x800B010E,
			CertCN_NO_MATCH               = 0x800B010F,
			CertWRONG_USAGE               = 0x800B0110,
			CertUNTRUSTEDCA               = 0x800B0112
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public CertPolicy()
		{
			defaultCertPolicy = ServicePointManager.CertificatePolicy;
			ServicePointManager.CertificatePolicy = this;
		}

		#endregion

		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static byte[] GetCertificate(string host)
		{
			CertificateState cs = CertTable[host] as CertificateState;
			if (cs != null)
			{
				return cs.Certificate.GetRawCertData();
			}
			return null;
		}

		/// <summary>
		/// Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		/// <param name="persist">If true save in store.</param>
		public static void StoreCertificate(byte[] certificate, string host, bool persist)
		{
			CertTable[host] = new CertificateState(new X509Certificate(certificate), true);
			if (persist)
			{
				// Save the cert in the store.
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void LoadCertsFromStore()
		{
		}

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
			bool honorCert = true;

			if ((certificateProblem == 0) || (CertificateProblem.CertEXPIRED.Equals(certificateProblem)))
			{
				honorCert = true;
			}
			else
			{
				CertificateState cs = CertTable[srvPoint.Address.Host] as CertificateState;
				if (cs != null && cs.Accepted)
				{
					if (cs.Certificate.GetCertHash() == certificate.GetCertHash())
					{
						honorCert = true;
					}
				}
				else
				{
					// This is a new cert replace the certificate.
					CertTable[srvPoint.Address.Host] = new CertificateState(certificate, false);
				}
			}
			return honorCert;
		}

		#endregion
	}
}
