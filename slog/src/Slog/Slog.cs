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
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Simias.Storage;

namespace Novell.Collaboration
{
	/// 
	/// <summary>
	/// Summary description for Slog.
	/// </summary>
	public class Slog : Collection
	{
		#region Class Members
		private static string	descriptionProperty = "SLOG:Description";
		private static string	linkProperty = "SLOG:Link";
		private static string	languageProperty = "SLOG:Language";
		private static string	copyrightProperty = "SLOG:Copyright";
		private static string	editorProperty = "SLOG:Editor";
		private static string	webmasterProperty = "SLOG:Webmaster";
		private static string	generatorProperty = "SLOG:Generator";
		private static string	cloudProperty = "SLOG:Cloud";
		private static string	ttlProperty = "SLOG:TTL";
		private static string	ratingProperty = "SLOG:Rating";
						
		#endregion

		#region Properties
		
		/// <summary>
		/// Title
		/// Name of the slog/channel.
		/// Note: this property is mandatory
		/// </summary>
		public string Title
		{
			get
			{
				try
				{
					return(this.Name);
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
						this.Name = value;
					}
				}
				catch{}
			}
		}
				
								
		/// <summary>
		/// Description
		/// Phrase or sentence describing the slog/channel
		/// Note: this property is mandatory
		/// </summary>
		public string Description
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(descriptionProperty).ToString());
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
						this.Properties.ModifyProperty(descriptionProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(descriptionProperty);
					}
				}
				catch{}
			}
		}
		
		/// <summary>
		/// Link
		/// The URL to the HTML website corresponding to the slog/channel.
		/// Ex. http://www.novell.com/bloggers/
		/// </summary>
		public string Link
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(linkProperty).ToString());
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
						this.Properties.ModifyProperty(linkProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(linkProperty);
					}
				}
				catch{}
			}
		}
		
		/// <summary>
		/// Language
		/// The language the channel/slog is written in.
		/// This property allows aggregators to group/exclude channels by language.
		/// Use valid W3C values.
		/// Ex. en-us
		/// Note: This property is optional
		/// </summary>
		public string Language
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(languageProperty).ToString());
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
						this.Properties.ModifyProperty(languageProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(languageProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Copyright
		/// Copyright notice for the content of the channel.
		/// Ex. Copyright 2004, Novell, Inc.
		/// Note: This property is optional
		/// </summary>
		public string Copyright
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(copyrightProperty).ToString());
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
						this.Properties.ModifyProperty(copyrightProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(copyrightProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// ManagingEditor
		/// Email address for the person responsible for
		/// editorial content.
		/// Ex. banderso@novell.com (Brady Anderson)
		/// Note: This property is optional
		/// </summary>
		public string ManagingEditor
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(editorProperty).ToString());
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
						this.Properties.ModifyProperty(editorProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(editorProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Webmaster
		/// Email address for person responsible for technical
		/// issues relating to the slog/channel.
		/// Ex. cgaisford@novell.com (Calvin Gaisford)
		/// Note: This property is optional
		/// </summary>
		public string Webmaster
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(webmasterProperty).ToString());
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
						this.Properties.ModifyProperty(webmasterProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(webmasterProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// PublishDate
		/// The publication date for the content in the Slog/channel.
		/// For example, the New York Times publishes on a daily basis
		/// the publication date flips once every 24 hours.
		/// All dates must conform to RFC 822 date and time specification.
		/// Ex. Sat, 07 Sep 2002 00:00:01 GMT
		/// Note: This property is optional
		/// </summary>
		public string PublishDate
		{
			get
			{
				// FIXME
				try
				{
					return("");
				}
				catch{}
				return("");
			}
		}

		/// <summary>
		/// LastBuildDate
		/// The last time the content of the Slog/channel changed.
		/// All dates must conform to RFC 822 date and time specification.
		/// Ex. Sat, 07 Sep 2002 00:00:01 GMT
		/// Note: This property is optional
		/// </summary>
		public string LastBuildDate
		{
			get
			{
				// FIXME
				try
				{
					return("");
				}
				catch{}
				return("");
			}
		}

		/// <summary>
		/// Generator
		/// Indicates the program used to generate the Slog/channel.
		/// Ex. Slogger v1.0
		/// Note: This property is optional
		/// </summary>
		public string Generator
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(generatorProperty).ToString());
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
						this.Properties.ModifyProperty(generatorProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(generatorProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Cloud
		/// Allows processes to register with a cloud so they can
		/// be notified of updates to this Slog.
		/// ex. <cloud domain="rpc.sys.com" port="80" path="/RPC2"
		///      registerProcedure="pingMe" protocol="soap" />
		/// Note: This property is optional
		///
		/// BUGBUG This property may not belong in the store but 
		/// should be supported in the SlogRss feed component.
		/// </summary>
		public string Cloud
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(cloudProperty).ToString());
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
						this.Properties.ModifyProperty(cloudProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(cloudProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Ttl
		/// Time-To-Live in minutes.
		/// Recommendation of how long a channel can be cached
		/// before refreshing from the source
		/// Note: This property is optional
		/// </summary>
		public string Ttl
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(ttlProperty).ToString());
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
						this.Properties.ModifyProperty(ttlProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(ttlProperty);
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// Rating
		/// The PICS rating for this channel
		/// Note: This property is optional
		/// </summary>
		public string Rating
		{
			get
			{
				try
				{
					return(this.Properties.GetSingleProperty(ratingProperty).ToString());
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
						this.Properties.ModifyProperty(ratingProperty, (string) value);
					}
					else
					{
						this.Properties.DeleteProperties(ratingProperty);
					}
				}
				catch{}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a Slog Collection
		/// </summary>
		/// <param name="store">The store for this Slog.</param>
		/// <param name="name">The friendly name that of this Slog.</param>
		public Slog(Store store, string name) :
			base(store, name, store.DefaultDomain)
		{
			// Set the type of the collection.
			this.SetType(this, typeof(Slog).Name);
		}


		/// <summary>
		/// Constructor for creating a Slog object from an exising node.
		/// </summary>
		/// <param name="store">The store object for this Slog.</param>
		/// <param name="node">The node object to construct the Slog.</param>
		public Slog(Store store, Node node)
			: base(store, node)
		{
			// Make sure this collection has our store propery
			if (!this.IsType( this, typeof(Slog).Name ) )
			{
				// Raise an exception here
				throw new ApplicationException( "Invalid Slog collection." );
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that can iterate through the ICSList.
		/// </summary>
		/// <returns>An IEnumerator object.</returns>
		public new IEnumerator GetEnumerator()
		{
			ICSList results = this.Search(	PropertyTags.Types, 
											typeof(SlogEntry).Name, 
											SearchOp.Equal );

			EntryEnumerator eEnumerator = 
					new EntryEnumerator(this, results.GetEnumerator());
			return eEnumerator;
		}
		#endregion
	}

	/// <summary>
	/// Class used for enumerating contacts
	/// </summary>
	public class EntryEnumerator : IEnumerator
	{
		#region Class Members
		private IEnumerator		entryEnum = null;
		private Slog			slog = null;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to instantiate this object by means of an enumerator.
		/// </summary>
		/// 
		internal EntryEnumerator(Slog s, IEnumerator enumerator)
		{
			this.slog = s;
			this.entryEnum = enumerator;
		}
		#endregion

		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position
		/// </summary>
		public void Reset()
		{
			entryEnum.Reset();
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				try
				{
					ShallowNode sNode = (ShallowNode) entryEnum.Current;
					return(new SlogEntry(slog, sNode));
				}
				catch{}
				return(null);
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			return entryEnum.MoveNext();
		}
		#endregion
	}
}


