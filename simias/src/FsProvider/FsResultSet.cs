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
using System.Collections;
using System.IO;
using Simias.Storage.Provider;
using System.Runtime.InteropServices;

namespace Simias.Storage.Provider.Fs
{
	/// <summary>
	/// Summary description for FsObjectIterator.
	/// </summary>
	public class FsResultSet : MarshalByRefObject, IResultSet
	{
		private bool AlreadyDisposed = false;
		int			count;
		Queue		resultQ;
        
		/// <summary>
		/// Creates a resultset from the queue.
		/// </summary>
		/// <param name="queue"></param>
		public FsResultSet(Queue queue)
		{
			resultQ = queue;
			count = resultQ.Count;
		}

		#region IObjectIterator Members

		/// <summary>
		/// Method to return the next set of objects.
		/// All the objects that can fit in the buffer will be returned.
		/// returns false when no more objects exist.
		/// </summary>
		/// <param name="buffer">Buffer used to return the objects.</param>
		/// <returns>true - objects returned. false - no more objects</returns>
		public int GetNext(ref char[] buffer)
		{
			if (AlreadyDisposed || resultQ.Count == 0)
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

					while (resultQ.Count != 0 && length > 0)
					{
						string objectXml = (string)resultQ.Peek();
						stringLen = objectXml.Length;
						if (length > stringLen)
						{
							resultQ.Dequeue();
							objectXml.CopyTo(0, buffer, offset, stringLen);
							offset += stringLen;
							length -= stringLen;
						}
						else
						{
							// We are out of space.
							break;
						}
					}
					endListTag.CopyTo(0, buffer, offset, endListTag.Length);
				}
				if (length == 0)
				{
					Dispose();
				}
				return buffer.Length - length;
			}
		}

		/// <summary>
		/// Property to get the count of available objects.
		/// </summary>
		public int Count
		{
			get
			{
				if (AlreadyDisposed)
				{
					return 0;
				}
				else
				{
					return count;
				}
			}
		}



		#endregion

		#region IDisposable Members

		/// <summary>
		/// Method to cleanup any file system resources held.
		/// </summary>
		public void Dispose()
		{
			AlreadyDisposed = true;
		}

		#endregion
	}
}
