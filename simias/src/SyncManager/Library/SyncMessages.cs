using System;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Service Custom Messages
	/// </summary>
	public enum SyncMessages
	{
		/// <summary>
		/// Start the sync cycle for all collections immediately.
		/// </summary>
		/// <remarks>
		/// The data is ignored.
		/// </remarks>
		SyncAllNow,

		/// <summary>
		/// Start the sync cycle for a specific collection immediately.
		/// </summary>
		/// <remarks>
		/// The data is the id of the collection.
		/// </remarks>
		SyncCollectionNow,
	}
}
