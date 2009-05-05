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
using System.Text;
using System.Runtime.InteropServices;

using Simias.Client;

namespace Novell.iFolderCom
{
	/// <summary>
	/// A class used to represent a domain in a list.
	/// </summary>
	[ComVisible(false)]
	public class DomainItem
	{
		private string name;
		private string id;
        private string host;
        private string url;

		/// <summary>
		/// Constructs a DomainInfo object.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <param name="ID">The ID of the object.</param>
		public DomainItem(string name, string ID)
		{
			this.name = name;
			this.id = ID;
            this.host = null;
		}

        /// <summary>
        /// Constructs a DomainInfo object.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="ID">The ID of the object.</param>
        /// <param name="host">The Host IP of the object.</param>
        public DomainItem(string name, string ID, string host)
        {
            this.name = name;
            this.id = ID;
            this.host = host;
        }

        /// <summary>
        /// Constructs a DomainInfo object.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="ID">The ID of the object.</param>
        /// <param name="host">The Host IP of the object.</param>
        /// <param name="url">The Host URL of the domain</param>
        public DomainItem(string name, string ID, string host, string url)
        {
            this.name = name;
            this.id = ID;
            this.host = host;
            this.url = url;
        }

		#region Properties
		/// <summary>
		/// Gets/sets the name of the domain object.
		/// </summary>
		public string Name
		{
			get { return name;	}
			set { name = value; }
		}

        /// <summary>
        /// Gets/sets the Url of the domain object.
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        /// <summary>
        /// Gets/sets the name of the domain object.
        /// </summary>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

		/// <summary>
		/// Gets the ID of the domain object.
		/// </summary>
		public string ID
		{
			get	{ return id; }
		}
		#endregion

		/// <summary>
		/// Gets a string representation of the Domain object.
		/// </summary>
		/// <returns>A string representing the name of the Domain object.</returns>
		public override string ToString()
		{
            if (this.host != null)
                return String.Format(name + " - " + host);
            else
            return name;
		}
	}
}
