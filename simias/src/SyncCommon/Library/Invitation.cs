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

namespace Simias.Sync
{
	/// <summary>
	/// Collection Invitation
	/// </summary>
	public class Invitation
	{
		// default root path for collections
		private static string defaultRootPath = Path.Combine(
			Environment.GetFolderPath(
			Environment.SpecialFolder.Personal),
			"My Collections");

		// invitation extension
		private const string extension = ".ifi";

		// invitation fields
		InvitationFields fields;

		/// <summary>
		/// Constructor
		/// </summary>
		public Invitation()
		{
			fields = new InvitationFields();
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="path">The path of the invitation file.</param>
		public Invitation(string path)
		{
			Load(path);
		}

		/// <summary>
		/// Populate the invitation fields from a file.
		/// </summary>
		/// <param name="path">The path of the invitation file.</param>
		public void Load(string path)
		{
			// deserialize
			XmlSerializer serializer = new XmlSerializer(
				typeof(InvitationFields));
				
			TextReader reader = new StreamReader(path);
		
			fields = (InvitationFields)
				serializer.Deserialize(reader);
		
			reader.Close();
		}

		/// <summary>
		/// Save the invitation fields to a file.
		/// </summary>
		/// <param name="path">The path of the invitation file to be created.</param>
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
				typeof(InvitationFields));
			
			TextWriter writer = new StreamWriter(path);
			
			serializer.Serialize(writer, fields);
			
			writer.Close();
		}

		/// <summary>
		/// Create a string representation of the invitation.
		/// </summary>
		/// <returns>A string representation of the invitation.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
	
			InvitationField[] data = (InvitationField[])fields.Data.Clone();

			builder.AppendFormat("Invitation{0}", Environment.NewLine);

			foreach(InvitationField field in data)
			{
				builder.AppendFormat("{0,20}: {1}{2}", field.Key,
					field.Data, Environment.NewLine);
			}

			return builder.ToString();
		}

		#region Properties

		/// <summary>
		/// The name of the Collection
		/// </summary>
		public string CollectionName
		{
			get { return fields["CollectionName"]; }
			set { fields["CollectionName"] = value; }
		}

		/// <summary>
		/// The id of the Collection
		/// </summary>
		public string CollectionId
		{
			get { return fields["CollectionId"]; }
			set { fields["CollectionId"] = value; }
		}

		/// <summary>
		/// The type of the Collection
		/// </summary>
		public string CollectionType
		{
			get { return fields["CollectionType"]; }
			set { fields["CollectionType"] = value; }
		}

		/// <summary>
		/// The rights on the Collection, that have been shared
		/// </summary>
		public string CollectionRights
		{
			get { return fields["CollectionRights"]; }
			set { fields["CollectionRights"] = value; }
		}

		/// <summary>
		/// The authentication identity
		/// </summary>
		public string Identity
		{
			get { return fields["Identity"]; }
			set { fields["Identity"] = value; }
		}

		/// <summary>
		/// The authentication domain
		/// </summary>
		public string Domain
		{
			get { return fields["Domain"]; }
			set { fields["Domain"] = value; }
		}

		/// <summary>
		/// The master host
		/// </summary>
		public string MasterHost
		{
			get { return fields["MasterHost"]; }
			set { fields["MasterHost"] = value; }
		}

		/// <summary>
		/// The master port
		/// </summary>
		public string MasterPort
		{
			get { return fields["MasterPort"]; }
			set { fields["MasterPort"] = value; }
		}

		/// <summary>
		/// The master's public key
		/// </summary>
		public string PublicKey
		{
			get { return fields["PublicKey"]; }
			set { fields["PublicKey"] = value; }
		}

		/// <summary>
		/// An optional invitation message (personal message)
		/// </summary>
		public string Message
		{
			get { return fields["Message"]; }
			set { fields["Message"] = value; }
		}

		/// <summary>
		/// An optional root path for the collection
		/// </summary>
		public string RootPath
		{
			get { return fields["RootPath"]; }
			set { fields["RootPath"] = value; }
		}

		/// <summary>
		/// The sender's name
		/// </summary>
		public string FromName
		{
			get { return fields["FromName"]; }
			set { fields["FromName"] = value; }
		}

		/// <summary>
		/// The sender's email address
		/// </summary>
		public string FromEmail
		{
			get { return fields["FromEmail"]; }
			set { fields["FromEmail"] = value; }
		}

		/// <summary>
		/// The receiver's name
		/// </summary>
		public string ToName
		{
			get { return fields["ToName"]; }
			set { fields["ToName"] = value; }
		}

		/// <summary>
		/// The receiver's email address
		/// </summary>
		public string ToEmail
		{
			get { return fields["ToEmail"]; }
			set { fields["ToEmail"] = value; }
		}

		/// <summary>
		/// The invitation extension
		/// </summary>
		public static string Extension
		{
			get { return extension; }
		}

		/// <summary>
		/// The default root path for new collections
		/// </summary>
		public static string DefaultRootPath
		{
			get { return defaultRootPath; }
		}

		#endregion Properties
	}

	/// <summary>
	/// Invitation fields
	/// </summary>
	[XmlRoot("Invitation")]
	public class InvitationFields
	{
		// fields for fields
		private Hashtable fields;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public InvitationFields()
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
		public InvitationField[] Data
		{
			get
			{
				InvitationField[] array = new InvitationField[fields.Count];

				int i = 0;

				foreach(string key in fields.Keys)
				{
					array[i++] = new InvitationField(key, (string)fields[key]);
				}

				return array;
			}

			set
			{
				InvitationField[] array = value;

				fields = new Hashtable();

				foreach(InvitationField field in array)
				{
					fields.Add(field.Key, field.Data);
				}
			}
		}

		#endregion Properties
	}

	/// <summary>
	/// An invitation field for serialization
	/// </summary>
	public class InvitationField
	{
		private string key;
		private string data;

		/// <summary>
		/// Constructor
		/// </summary>
		public InvitationField() : this(null, null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Field Key</param>
		/// <param name="data">Field Data</param>
		public InvitationField(string key, string data)
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
