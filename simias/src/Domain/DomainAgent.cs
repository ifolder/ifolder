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
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));

		private static readonly string SectionName = "Domain";

		/// <summary>
		/// The suggested service url for the current machine.
		/// </summary>
		private static readonly Uri DefaultServiceUrl = (new UriBuilder("http",
			MyDns.GetHostName(), 6346, EndPoint)).Uri;

		private static readonly string UrlKeyName = "Service Url";
		
		/// <summary>
		/// The enabled state of using the domain service.
		/// </summary>
		private static readonly bool DefaultEnabled = false;

		private static readonly string EnabledKeyName = "Enabled";
		
		private Configuration config;

		public DomainAgent(Configuration config)
		{
			this.config = config;
			log.Info("Domain Service Enabled: {0}", Enabled);
			log.Info("Domain Service Url: {0}", ServiceUrl);
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

		public Uri ServiceUrl
		{
			get { return new Uri(config.Get(SectionName, UrlKeyName, DefaultServiceUrl.ToString())); }

			set { config.Set(SectionName, UrlKeyName, value.ToString()); }
		}

		public bool Enabled
		{
			get { return bool.Parse(config.Get(SectionName, EnabledKeyName, DefaultEnabled.ToString())); }

			set { config.Set(SectionName, EnabledKeyName, value.ToString()); }
		}

		#endregion
	}
}
