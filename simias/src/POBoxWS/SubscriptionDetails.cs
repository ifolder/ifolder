using System;

namespace Novell.Simias.POBox.Web
{
	/// <summary>
	/// Subscription Details
	/// </summary>
	[Serializable]
	public class SubscriptionDetails
	{
		/// <summary>
		/// The subscription collection dir node id
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// The subscription collection dir node name
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// The subscription collection service URL
		/// </summary>
		public string CollectionUrl;

		public SubscriptionDetails()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
