using System;
using System.Collections;

namespace Simias.Gaim
{
	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	public class GaimDomainSearchContext
	{
		#region Class Members
		private String id;
		private ArrayList buddies;
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

		public ArrayList Buddies
		{
			get
			{
				return buddies;
			}
			set
			{
				if (value != null)
				{
					buddies = value;
				}
			}
		}

		public int Count
		{
			get
			{
				if (buddies != null)
				{
					return buddies.Count;
				}
				else
					return 0;
			}
		}
		#endregion

		#region Public Methods
		#endregion
	}
}
