using System;
using System.Collections;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for Defaults
	/// </summary>
	class Defaults
	{
		#region Class Members
		static internal int			timeToLive = 300;		// 5 minutes
		static internal int			ptrTimeToLive = 7200;	// 2 hours

		// Number of seconds the maintenance thread sleeps
		//static internal int			maintenanceNapTime = 120;
		static internal int			maintenanceNapTime = 30;

		static internal int			sendBufferSize = 32768;
		#endregion

		#region Properties
		#endregion

		#region Constructors
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}
}
