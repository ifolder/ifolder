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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using Simias;
using Simias.Storage;

namespace Novell.Collaboration
{
	/// <summary>
	/// Summary description for a SlogEntry.
	/// A SlogEntry is equivalent to a Node in the collection store.
	/// </summary>
	public class SlogEntry : Node
	{
		#region Class Members
		public static string	dateProperty = "SLOG:Date";
		public static string	titleProperty = "SLOG:Title";
		public static string	entryProperty = "SLOG:Entry";
		public static string	userIDProperty = "SLOG:UserID";
		#endregion

		#region Properties
		/// <summary>
		/// UserID
		/// !NOTE! Doc incomplete
		/// </summary>
		public string UserID
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(userIDProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(userIDProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(userIDProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Date
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Date
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(dateProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(dateProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(dateProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Title
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Title
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(titleProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(titleProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(titleProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Title
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Description
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(entryProperty).ToString());
				}
				catch{}
				return("");
			}

			set
			{
				try
				{
					if (value != null)
					{
						this.Properties.ModifyProperty(entryProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(entryProperty);
					}
				}
				catch{}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Simple SlogEntry constructor
		/// </summary>
		public SlogEntry(Slog slog, string name) : base(name)
		{
			slog.SetType(this, typeof(SlogEntry).Name);
		}

		/// <summary>
		/// Simple SlogEntry constructor
		/// </summary>
		public SlogEntry(Slog slog, ShallowNode sNode) : base(slog, sNode)
		{
		}

		#endregion
	}
}
