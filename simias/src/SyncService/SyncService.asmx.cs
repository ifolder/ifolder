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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using Simias.Storage;

namespace Simias.Sync.Web
{
	/// <summary>
	/// The Sync Web Service Class.
	/// </summary>
	[WebService(Namespace = "http://novell.com/simias/sync/",
		 Name = "Simias Sync Service",
		 Description = "Web Service providing Syncronization to Simias")]
	public class SyncWebService : System.Web.Services.WebService
	{
		SyncService Service
		{
			get { return (SyncService)Session["SyncService"]; }
			set { Session["SyncService"] = value; }
		}

		/// <summary>
		/// start sync of this collection -- perform basic role checks.
		/// </summary>
		/// <param name="si">The start info used to setup this sync.</param>
		/// <param name="user">This is temporary.  Needs to be removed when Security is added.</param>
		/// <returns>The SyncNodeStamps for the sync.</returns>
		[WebMethod(EnableSession = true)]
		[SoapRpcMethod]
		public SyncNodeStamp[] Start(SyncStartInfo si, string user, out SyncStartInfo siout)
		{
			SyncService ss = new SyncService();
			Service = ss;
			Session.Timeout = 5;
			SyncNodeStamp[] nodes = ss.Start(ref si, user);
			siout = si;
			// TODO:
			// If we have work we need to get a lock.
			return nodes;
		}

		/// <summary>
		/// Called when Syncronization has completed for this collection.
		/// This call abandons the session.
		/// </summary>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public void Stop()
		{
			Service.Stop();
			Session.Abandon();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public bool KeepAlive()
		{
			if (Service != null)
				return true;
			return false;
		}

		/// <summary>
		/// simple version string, also useful to check remoting
		/// </summary>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public string Version()
		{
			return Service.Version;
		}

		/// <summary>
		/// Takes an array of non-file nodes and writes them to the store.
		/// </summary>
		/// <returns>An array of the failed nodes.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNodeStatus[] PutNodes(SyncNode[] nodes)
		{
			return Service.PutNonFileNodes(nodes);
		}

		/// <summary>
		/// gets an array of small nodes
		/// </summary>
		/// <returns>An Array of non-file nodes.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNode[] GetNodes(string[] nids)
		{
			return Service.GetNonFileNodes(nids);
		}

		/// <summary>
		/// gets an array of dir nodes
		/// </summary>
		/// <returns>An Array of non-file nodes.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNode[] GetDirs(string[] nids)
		{
			return Service.GetNonFileNodes(nids);
		}

		/// <summary>
		/// Takes an array of non-file nodes and writes them to the store.
		/// </summary>
		/// <returns>An array of the failed nodes.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNodeStatus[] PutDirs(SyncNode[] nodes)
		{
			return Service.PutDirs(nodes);
		}

		/// <summary>
		/// Put the node that represents the file to the server. This call is made to begin
		/// an upload of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="node">The node to put to ther server.</param>
		/// <returns>True if successful.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public bool PutFileNode(SyncNode node)
		{
			return Service.PutFileNode(node);
		}

		/// <summary>
		/// Get the node that represents the file. This call is made to begin
		/// a download of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="nodeID">The node to get.</param>
		/// <returns>The SyncNode.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNode GetFileNode(string nodeID)
		{
			return Service.GetFileNode(nodeID);
		}

		/// <summary>
		/// Delete the nodes listed.
		/// </summary>
		/// <param name="nodeIDs">An array of IDs of the nodes to delete.</param>
		/// <returns>The status of the deletes.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNodeStatus[] DeleteNodes(string[] nodeIDs)
		{
			return Service.DeleteNodes(nodeIDs);
		}

		/// <summary>
		/// Get a HashMap of the file.
		/// </summary>
		/// <param name="blockSize">The block size to be hashed.</param>
		/// <returns>The HashMap.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public HashData[] GetHashMap(int blockSize)
		{
			return Service.GetHashMap(blockSize);
		}

		/// <summary>
		/// Write the included data to the new file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset in the new file of where to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public void Write(byte[] buffer, long offset, int count)
		{
			Service.Write(buffer, offset, count);
		}

		/// <summary>
		/// Copy data from the old file to the new file.
		/// </summary>
		/// <param name="oldOffset">The offset in the old (original file).</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public void Copy(long oldOffset, long offset, int count)
		{
			Service.Copy(oldOffset, offset, count);
		}

		/// <summary>
		/// Read data from the currently opened file.
		/// </summary>
		/// <param name="buffer">Byte array of bytes read.</param>
		/// <param name="offset">The offset to begin reading.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public int Read(out byte[] buffer, long offset, int count)
		{
			return Service.Read(out buffer, offset, count);
		}

		/// <summary>
		/// Close the current file.
		/// </summary>
		/// <param name="commit">True: commit the filenode and file.
		/// False: Abort the changes.</param>
		/// <returns>Returns the status of the sync.</returns>
		[WebMethod(EnableSession = true)]
        [SoapRpcMethod]
		public SyncNodeStatus CloseFileNode(bool commit)
		{
			return Service.CloseFileNode(commit);
		}
	}
}