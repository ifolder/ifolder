using System;

using Simias;

namespace Simias.Domain
{
	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		private static readonly string UrlName = "ServiceUrl";

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
		
		public Uri ServiceUrl
		{
			get
			{
				Uri uri = null;

				string uriString = config.Get(UrlName, null);

				if ((uriString != null) && (uriString.Length > 0))
				{
					uri = new Uri(uriString);
				}

				return uri;
			}

			set
			{
				if (value != null) config.Set(UrlName, value.ToString());
			}
		}

		#endregion
	}
}
