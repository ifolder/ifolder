using System;

namespace Simias.Gaim
{
	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	public class GaimDomainSearchContext
	{
		#region Class Members
		private String id;
		#endregion

		#region Constructor
		public GaimDomainSearchContext()
		{
			id = Guid.NewGuid().ToString();
		}
		#endregion

		#region Properties
		public string ID
		{
			get
			{
				return id;
			}
		}
		#endregion
	}
}
