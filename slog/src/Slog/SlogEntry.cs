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
		internal static string	dateProperty = "SLOG:Date";
		internal static string	titleProperty = "SLOG:Title";
		internal static string	entryProperty = "SLOG:Entry";
		internal static string	userIDProperty = "SLOG:UserID";
		internal static string	categoryProperty = "SLOG:Category";
		internal static string 	commentsProperty = "SLOG:Comments";
		
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
		/// PublishDate
		/// Indiates when the slog entry/item was published
		/// All dates must conform to RFC 822 date and time specification.
		/// Ex. Sat, 07 Sep 2002 00:00:01 GMT
		/// Note: This property is optional
		/// </summary>
		public string PublishDate
		{
			get
			{
				// FIXME should just get the creation date of the node
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
		/// The title of the item.
		/// Note: this property is mandatory
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
		/// Description
		/// The item synopsis
		/// Note: this property is mandatory
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
		
		/// <summary>
		/// Author
		/// UserID or email address of the author
		/// of the slog entry/item
		/// Note: this property is optional
		/// </summary>
		public string Author
		{
			get
			{
				try
				{
					// FIXME
					return(this.Properties.GetSingleProperty(userIDProperty).ToString());
				}
				catch{}
				return("");
			}
		}

		/// <summary>
		/// Category
		/// The category the slog entry/item belongs to.
		/// ex. Harley Davidson Enthusiast
		/// Note: this property is optional
		/// </summary>
		public string Category
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(categoryProperty).ToString());
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
						this.Properties.ModifyProperty(categoryProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(categoryProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Comments
		/// URL of a page for comments relating to this entry/item.
		/// ex. http://www.myblog.org/comments-cgi?entry_id=290
		/// Note: this property is optional
		/// </summary>
		public string Comments
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(commentsProperty).ToString());
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
						this.Properties.ModifyProperty(commentsProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(commentsProperty);
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
