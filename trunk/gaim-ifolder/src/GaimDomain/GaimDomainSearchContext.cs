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
		private ArrayList members;
		private int currentIndex;
		#endregion

		#region Constructor
		public GaimDomainSearchContext()
		{
			id = Guid.NewGuid().ToString();
			currentIndex = 0;
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

		public ArrayList Members
		{
			get
			{
				return members;
			}
			set
			{
				if (value != null)
				{
					members = value;
				}
			}
		}

		public int Count
		{
			get
			{
				if (members != null)
				{
					return members.Count;
				}
				else
					return 0;
			}
		}
		
		public int CurrentIndex
		{
			get
			{
				return currentIndex;
			}
			set
			{
				currentIndex = value;
			}
		}
		#endregion

		#region Public Methods
		#endregion
	}
}
