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
	public enum InvitationState
	{
		Pending,
		Invited,
		Posted,
		Rejected,
		Accepted,
		Declined
	};

	/// <summary>
	/// An Invitation object is a specialized message used for inviting someone to a team space.
	/// </summary>
	public class Invitation : Message
	{
		/// <summary>
		/// Instantiates an Invitation object.
		/// </summary>
		public Invitation(string invitationName) :
			base (invitationName)
		{
		}
	}
}
