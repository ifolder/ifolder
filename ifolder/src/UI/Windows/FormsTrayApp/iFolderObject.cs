/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;

using Novell.iFolder.Web;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// iFolder states.
	/// </summary>
	public enum iFolderState
	{
		Initial,
		/// <summary>
		/// The Normal state.
		/// </summary>
		Normal,

		/// <summary>
		/// The Synchronizing state.
		/// </summary>
		Synchronizing,

		/// <summary>
		/// The FailedSync state.
		/// </summary>
		FailedSync,

		/// <summary>
		/// Synchronizing with the local store.
		/// </summary>
		SynchronizingLocal,

		/// <summary>
		/// Unable to connect to the server.
		/// </summary>
		Disconnected,

        /// <summary>
        /// Passphrase not provided, encrypted folder wont sync.
        /// </summary>
        NoPassphrase,

        /// <summary>
        /// State represent when revert and deletion of ifolder is in progress.
        /// </summary>
        RevertAndDelete,

        /// <summary>
        /// State represent when sync is disabled for ifolder
        /// </summary>
        SyncDisabled
	}

	/// <summary>
	/// Summary description for iFolderObject.
	/// </summary>
	public class iFolderObject
	{
		#region Class Members

		private iFolderWeb ifolderWeb;
		private iFolderState ifolderState;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs an iFolderObject object.
		/// </summary>
		/// <param name="ifolderWeb">The iFolderWeb object to base this object on.</param>
		/// <param name="ifolderState">The state of the iFolder.</param>
		public iFolderObject(iFolderWeb ifolderWeb, iFolderState ifolderState)
		{
			this.ifolderWeb = ifolderWeb;
			this.ifolderState = ifolderState;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets/sets the iFolderWeb object.
		/// </summary>
		public iFolderWeb iFolderWeb
		{
			get { return ifolderWeb; }
			set { ifolderWeb = value; }
		}

		/// <summary>
		/// Gets/sets the iFolderState for this object.
		/// </summary>
		public iFolderState iFolderState
		{
			get { return ifolderState; }
			set { ifolderState = value; }
		}

		/// <summary>
		/// Gets the identifier of the iFolder.
		/// </summary>
		public string ID
		{
			get { return ifolderWeb.ID; }
		}

		#endregion
	}
}
