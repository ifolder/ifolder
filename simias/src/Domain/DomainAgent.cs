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

using Simias;

namespace Simias.Domain
{
	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		private static readonly string SectionName = "Domain";
		private static readonly string UrlKeyName = "ServiceUrl";

		private Configuration config;

		public DomainAgent(Configuration config)
		{
			this.config = config;
		}

		public IDomainService Connect()
		{
			if (ServiceUrl == null) return null;

			string url = ServiceUrl.ToString();

			return (IDomainService)Activator.GetObject(typeof(IDomainService), url);
		}

		#region Properties
		
		public static string EndPoint
		{
			get { return "DomainService.rem"; }
		}

		public static int Port
		{
			get { return 6346; }
		}

		public Uri ServiceUrl
		{
			get
			{
				Uri uri = null;

				string uriString = config.Get(SectionName, UrlKeyName, null);

				if ((uriString != null) && (uriString.Length > 0))
				{
					UriBuilder ub = new UriBuilder(uriString);
					ub.Path = EndPoint;
					ub.Port = Port;
					uri = ub.Uri;
				}

				return uri;
			}

			set
			{
				if (value != null)
				{
					config.Set(SectionName, UrlKeyName, value.ToString());
				}
			}
		}

		#endregion
	}
}
