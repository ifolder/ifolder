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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Text;

namespace Simias.DomainService.Web
{
	/// <summary>
	/// Provision information
	/// </summary>
	[Serializable]
	public class ProvisionInfo
	{
		/// <summary>
		/// User ID
		/// </summary>
		public string UserID;

		/// <summary>
		/// PO Box Collection ID
		/// </summary>
		public string POBoxID;

		/// <summary>
		/// PO Box Collection Name
		/// </summary>
		public string POBoxName;

		/// <summary>
		/// The unique ID for the member node for this user.
		/// </summary>
		public string MemberNodeID;

		/// <summary>
		/// The name of the member object in the POBox collection.
		/// </summary>
		public string MemberNodeName;

		/// <summary>
		/// The rights that the member has in the POBox collection.
		/// </summary>
		public string MemberRights;

		/// <summary>
		/// Constructor
		/// </summary>
		public ProvisionInfo()
		{
		}

		/// <summary>
		/// Create a string representation
		/// </summary>
		/// <returns>A string representation</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			
			string newLine = Environment.NewLine;

			builder.AppendFormat("Provision Information{0}", newLine);
			builder.AppendFormat("  User ID          : {0}{1}", this.UserID, newLine);
			builder.AppendFormat("  PO Box ID        : {0}{1}", this.POBoxID, newLine);
			builder.AppendFormat("  PO Box Name      : {0}{1}", this.POBoxName, newLine);
			builder.AppendFormat("  Member Node ID   : {0}{1}", this.MemberNodeID, newLine);
			builder.AppendFormat("  Member Node Name : {0}{1}", this.MemberNodeName, newLine);
			builder.AppendFormat("  Member Rights    : {0}{1}", this.MemberRights, newLine);

			return builder.ToString();
		}
	}
}
