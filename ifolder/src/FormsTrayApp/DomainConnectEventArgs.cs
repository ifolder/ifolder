using System;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for DomainConnectEventArgs.
	/// </summary>
	public class DomainConnectEventArgs : EventArgs
	{
		private DomainWeb domainWeb;

		/// <summary>
		/// Constructs a DomainConnectEventArgs object.
		/// </summary>
		public DomainConnectEventArgs(DomainWeb domainWeb)
		{
			this.domainWeb = domainWeb;
		}

		/// <summary>
		/// Gets the DomainWeb object.
		/// </summary>
		public DomainWeb DomainWeb
		{
			get { return domainWeb; }
		}
	}
}
