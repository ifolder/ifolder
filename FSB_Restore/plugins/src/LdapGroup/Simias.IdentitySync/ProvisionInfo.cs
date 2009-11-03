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
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

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
