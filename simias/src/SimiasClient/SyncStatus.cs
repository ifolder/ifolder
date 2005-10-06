/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/

namespace Simias.Client
{

	#region SyncStatus

	/// <summary>
	/// The status codes for a sync attempt.
	/// </summary>
	public enum SyncStatus : byte
	{
		/// <summary>
		/// The operation was successful.
		/// </summary>
		Success,
		/// <summary>
		/// There was an error.
		/// </summary>
		Error,
		/// <summary> 
		/// node update was aborted due to update from other client 
		/// </summary>
		UpdateConflict,
		/// <summary> 
		/// node update was completed, but temporary file could not be moved into place
		/// </summary>
		FileNameConflict,
		/// <summary> 
		/// node update was probably unsuccessful, unhandled exception on the server 
		/// </summary>
		ServerFailure,
		/// <summary> 
		/// node update is in progress 
		/// </summary>
		InProgess,
		/// <summary>
		/// The File is in use.
		/// </summary>
		InUse,
		/// <summary>
		/// The Server is busy.
		/// </summary>
		Busy,
		/// <summary>
		/// The client passed invalid data.
		/// </summary>
		ClientError,
		/// <summary>
		/// The policy doesnot allow this file.
		/// </summary>
		Policy,
		/// <summary>
		/// Insuficient rights for the operation.
		/// </summary>
		Access,
		/// <summary>
		/// The collection is Locked.
		/// </summary>
		Locked,
		/// <summary>
		/// The disk quota doesn't allow this file.
		/// </summary>
		PolicyQuota,
		/// <summary>
		/// The size policy doesn't allow this file.
		/// </summary>
		PolicySize,
		/// <summary>
		/// The type policy doesn't allow this file.
		/// </summary>
		PolicyType,
		/// <summary>
		/// The disk is full.
		/// </summary>
		DiskFull,
		/// <summary>
		/// The Object is readonly.
		/// </summary>
		ReadOnly,
	}

	#endregion
}
