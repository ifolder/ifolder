using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace Simias.Storage
{
	/// <summary>
	/// Summary description for StorageBase.
	/// </summary>
	public class StorageBase : System.ComponentModel.Component
	{
		#region Class Members
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected System.ComponentModel.Container components = null;

		/// <summary>
		/// Variable that tells if the object instance is still useable.
		/// </summary>
		protected bool isDisposed = false;
		#endregion

		#region Constructor / Disposer
		public StorageBase()
		{
			// Keep a container that we can put our managed objects into
			// so we can free them later at dispose time.
			components = new System.ComponentModel.Container();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if ( components != null )
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
			isDisposed = true;
		}
		#endregion
	}
}
