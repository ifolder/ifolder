using System;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for GenericEntry.
	/// </summary>
	public class GenericEntry
	{
		private bool add;
		private bool remove;

		public GenericEntry()
		{
			remove = false;
			add = false;
		}

		#region Properties
		public bool Add
		{
			get
			{
				return add;
			}
			set
			{
				add = value;
			}
		}

		public bool Remove
		{
			get
			{
				return remove;
			}
			set
			{
				remove = value;
			}
		}
		#endregion
	}
}
