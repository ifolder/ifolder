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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;

namespace Simias.POBox
{
	/// <summary>
	/// Invitation states.
	/// </summary>
	public enum InvitationState
	{
		/// <summary>
		/// The invitation has been created but not sent.
		/// </summary>
		Pending,

		/// <summary>
		/// The invitation has been sent.
		/// </summary>
		Invited,

		/// <summary>
		/// The invitation, a request to join, has been sent.
		/// </summary>
		Posted,

		/// <summary>
		/// The invitation, a request to join, has been rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The invitation has been accepted.
		/// </summary>
		Accepted,

		/// <summary>
		/// The invitation has been declined.
		/// </summary>
		Declined
	};

	/// <summary>
	/// An Invitation object is a specialized message used for inviting someone to a team space.
	/// </summary>
	public class Invitation : Message
	{
		#region Class Members
		/// <summary>
		/// The name of the property storing the InvitationState.
		/// </summary>
		public const string InviteState = "InvitationState";
		#endregion

		#region Constructors
		/// <summary>
		/// Instantiates an Invitation object.
		/// </summary>
		/// <param name="invitationName">The friendly name of the invitation.</param>
		/// <param name="messageType">The type of the message.</param>
		public Invitation(string invitationName, MessageType messageType) :
			base (invitationName, messageType)
		{
			this.Properties.AddProperty(InviteState, InvitationState.Pending);
		}
		#endregion
	}
}
