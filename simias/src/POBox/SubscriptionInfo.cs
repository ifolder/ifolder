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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Text;

namespace Simias.POBox
{
	/// <summary>
	/// Collection Subscription Information 
	/// </summary>
	public class SubscriptionInfo
	{
		// subscription info extension
		private const string extension = ".csi";

		// subscription info fields
		SubscriptionInfoFields fields;

		/// <summary>
		/// Constructor
		/// </summary>
		public SubscriptionInfo()
		{
			fields = new SubscriptionInfoFields();
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="path">The path of the subscription info file.</param>
		public SubscriptionInfo(string path)
		{
			Load(path);
		}

		/// <summary>
		/// Populate the subscription info fields from a file.
		/// </summary>
		/// <param name="path">The path of the subscription info file.</param>
		public void Load(string path)
		{
			// deserialize
			XmlSerializer serializer = new XmlSerializer(
				typeof(SubscriptionInfoFields));
				
			TextReader reader = new StreamReader(path);
		
			fields = (SubscriptionInfoFields)
				serializer.Deserialize(reader);
		
			reader.Close();
		}

		/// <summary>
		/// Save the subscription info fields to a file.
		/// </summary>
		/// <param name="path">The path of the subscription info file to be created.</param>
		public void Save(string path)
		{
			// check directory
			string dir = Path.GetDirectoryName(path);

			if ((dir.Length > 0) && (!Directory.Exists(dir)))
			{
				Directory.CreateDirectory(dir);
			}

			// serialize
			XmlSerializer serializer = new XmlSerializer(
				typeof(SubscriptionInfoFields));
			
			TextWriter writer = new StreamWriter(path);
			
			serializer.Serialize(writer, fields);
			
			writer.Close();
		}

		/// <summary>
		/// Create a string representation of the subscription info.
		/// </summary>
		/// <returns>A string representation of the subscription info.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
	
			SubscriptionInfoField[] data = (SubscriptionInfoField[])fields.Data.Clone();

			builder.AppendFormat("SubscriptionInfo{0}", Environment.NewLine);

			foreach(SubscriptionInfoField field in data)
			{
				builder.AppendFormat("{0,20}: {1}{2}", field.Key,
					field.Data, Environment.NewLine);
			}

			return builder.ToString();
		}

		#region Properties

		/// <summary>
		/// The subscription (node) id (optional)
		/// </summary>
		public string ID
		{
			get { return fields["ID"]; }
			set { fields["ID"] = value; }
		}

		/// <summary>
		/// The authentication domain id
		/// </summary>
		public string Domain
		{
			get { return fields["Domain"]; }
			set { fields["Domain"] = value; }
		}

		/// <summary>
		/// The collection id
		/// </summary>
		public string Collection
		{
			get { return fields["Collection"]; }
			set { fields["Collection"] = value; }
		}

		/// <summary>
		/// The post office service URL
		/// </summary>
		public Uri Url
		{
			get { return new Uri(fields["Url"]); }
			set { fields["Url"] = value.ToString(); }
		}

		#endregion Properties
	}

	/// <summary>
	/// SubscriptionInfo fields
	/// </summary>
	[XmlRoot("SubscriptionInfo")]
	public class SubscriptionInfoFields
	{
		// fields for fields
		private Hashtable fields;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public SubscriptionInfoFields()
		{
			fields = new Hashtable();
		}

		#region Properties

		/// <summary>
		/// Data index property
		/// </summary>
		[XmlIgnore]
		public string this[string key]
		{
			get { return (string)fields[key]; }
			set { fields[key] = value; }
		}

		/// <summary>
		/// An inivation field array for serialization
		/// </summary>
		[XmlElement("Field")]
		public SubscriptionInfoField[] Data
		{
			get
			{
				SubscriptionInfoField[] array = new SubscriptionInfoField[fields.Count];

				int i = 0;

				foreach(string key in fields.Keys)
				{
					array[i++] = new SubscriptionInfoField(key, (string)fields[key]);
				}

				return array;
			}

			set
			{
				SubscriptionInfoField[] array = value;

				fields = new Hashtable();

				foreach(SubscriptionInfoField field in array)
				{
					fields.Add(field.Key, field.Data);
				}
			}
		}

		#endregion Properties
	}

	/// <summary>
	/// An subscription info field for serialization
	/// </summary>
	public class SubscriptionInfoField
	{
		private string key;
		private string data;

		/// <summary>
		/// Constructor
		/// </summary>
		public SubscriptionInfoField() : this(null, null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Field Key</param>
		/// <param name="data">Field Data</param>
		public SubscriptionInfoField(string key, string data)
		{
			this.key = key;
			this.data = data;
		}

		/// <summary>
		/// Field Key
		/// </summary>
		public string Key
		{
			get { return key; }
			set { key = value; }
		}

		/// <summary>
		/// Field Data
		/// </summary>
		public string Data
		{
			get { return data; }
			set { data = value; }
		}
	}
}
