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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Data;
using System.Text;
using Simias.Storage.Provider;
using System.Runtime.InteropServices;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// The ResultSet Implementation for the Sqlite provider.
	/// </summary>
	public class SqliteResultSet : MarshalByRefObject, IResultSet
	{
		private bool AlreadyDisposed = false;
		private bool includeCid;
		IDataReader	Reader;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="reader">IDataReader to read the data from this resultset.</param>
		/// <param name="includeCid">If true include the collection ID.</param>
		public SqliteResultSet(IDataReader reader, bool includeCid)
		{
			this.includeCid = includeCid;
			Reader = reader;
			if (Reader != null)
			{
				if (!Reader.Read())
				{
					Dispose(false);
				}
			}
		}

		/// <summary>
		/// Destructor
		/// </summary>
		~SqliteResultSet()
		{
			Dispose(true);
		}

		#region IObjectIterator Members

		/// <summary>
		/// Translates any reserved XML characters into the proper format for inclusion in a XML string.
		/// </summary>
		/// <param name="normal">String to translate.</param>
		/// <returns>Translated string.</returns>
		private string TranslateStringToXml(string normal)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in normal)
			{
				switch (c)
				{
					case '&':
						sb.Append("&amp;");
						break;

					case '<':
						sb.Append("&lt;");
						break;

					case '>':
						sb.Append("&gt;");
						break;

					case '\"':
						sb.Append("&quot;");
						break;

					case '\'':
						sb.Append("&#39;");
						break;

					default:
						sb.Append(c);
						break;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Method to return the next set of objects.
		/// All the objects that can fit in the buffer will be returned.
		/// returns false when no more objects exist.
		/// </summary>
		/// <param name="buffer">Buffer used to return the objects.</param>
		/// <returns>Count - number of caracters in buffer.</returns>
		public int GetNext(ref char[] buffer)
		{
			// TODO:  Add FlaimObjectIterator.GetNext implementation
			if (AlreadyDisposed || Reader == null)
			{
				return 0;
			}
			else
			{
				int length = buffer.Length;
				int offset = 0;
				string startListTag = "<" + XmlTags.ObjectListTag + ">";
				string endListTag = "</" + XmlTags.ObjectListTag + ">";
				if (length > startListTag.Length)
				{
					int stringLen = startListTag.Length;
					startListTag.CopyTo(0, buffer, offset, stringLen);
					offset += stringLen;
					length -= stringLen;
					// Save space for the end tag.
					length -= endListTag.Length;

					while (length > 0)
					{
						string id = Reader[0].ToString();
						string name = Reader[1].ToString();
						string type = Reader[2].ToString();
						string cid;
						if (includeCid)
						{
							cid = string.Format(" {0}=\"{1}\"", XmlTags.CIdAttr, TranslateStringToXml(Reader[3].ToString()));
						}
						else
						{
							cid = "";
						}

						string objectXml = string.Format("<{0} {1}=\"{2}\" {3}=\"{4}\" {5}=\"{6}\"{7}/>", 
								XmlTags.ObjectTag,
								XmlTags.IdAttr,
								TranslateStringToXml(id),
								XmlTags.NameAttr,
								TranslateStringToXml(name),
								XmlTags.TypeAttr,
								TranslateStringToXml(type),
								cid);

						stringLen = objectXml.Length;
						if (length > stringLen)
						{
							objectXml.CopyTo(0, buffer, offset, stringLen);
							offset += stringLen;
							length -= stringLen;
						}
						else
						{
							// We are out of space.
							break;
						}
						if (!Reader.Read())
						{
							Dispose();
							break;
						}
					}
					endListTag.CopyTo(0, buffer, offset, endListTag.Length);
				}
				
				return buffer.Length - length;
			}
		}
		
		/// <summary>
		/// Set the Index to the specified offset from the origin.
		/// </summary>
		/// <param name="origin">The origin to move from</param>
		/// <param name="offset">The offset to move the index by.</param>
		/// <returns>True if successful.</returns>
		public bool SetIndex(IndexOrigin origin, int offset)
		{
			return false;
		}

		/// <summary>
		/// Property to get the count of available objects.
		/// </summary>
		public int Count
		{
			get
			{
				return 0;
			}
		}

		#endregion

		/// <summary>
		/// Method to cleanup resources held.
		/// </summary>
		/// <param name="inFinalize">True when called from finalizer.</param>
		private void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				AlreadyDisposed = true;
				Reader.Close();
				Reader.Dispose();
			
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Method to cleanup resources held.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
