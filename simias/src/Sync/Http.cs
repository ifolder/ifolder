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
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Net;
using System.Collections;
using Simias;

using Simias.Event;
using Simias.Client.Event;

using Simias.Storage;
using Simias.Sync;
using Simias.Sync.Delta;
using Simias.Authentication;

namespace Simias.Sync.Http
{
	/// <summary>
	/// Definitions for the http handler.
	/// </summary>
	public class SyncHeaders
	{
		public static string	SyncVersion = "SSyncVersion";
		/// <summary>
		/// 
		/// </summary>
		public static string	CopyOffset = "SSyncOffset";
		/// <summary>
		/// 
		/// </summary>
		public static string	Range = "SSyncRange";
		/// <summary>
		/// The Method that is to be performed.
		/// </summary>
		public static string	Method = "SSyncMethod";
		/// <summary>
		/// 
		/// </summary>
		public static string	Blocks = "SSyncBlocks";
		/// <summary>
		/// 
		/// </summary>
		public static string	BlockSize = "SSyncBlockSize";
		/// <summary>
		/// Used to specify how many objects are sent with the request/response.
		/// </summary>
		public static string	ObjectCount = "SSyncObjects";
		/// <summary>
		/// 
		/// </summary>
		public static string	UserName = "SSyncUser";
		/// <summary>
		/// 
		/// </summary>
		public static string	UserID = "SSyncUserID";
		/// <summary>
		/// 
		/// </summary>
		public static string	CollectionName = "SSyncCollectionName";
		/// <summary>
		/// 
		/// </summary>
		public static string	CollectionID = "SSyncCollectionID";
		/// <summary>
		/// 
		/// </summary>
		public static string	SyncContext = "SSyncContext";
	}

	/// <summary>
	/// 
	/// </summary>
	public enum SyncMethod
	{
		/// <summary>
		/// 
		/// </summary>
		StartSync = 1,
		/// <summary>
		/// 
		/// </summary>
		GetNextInfoList,
		/// <summary>
		/// 
		/// </summary>
		PutNodes,
		/// <summary>
		/// 
		/// </summary>
		GetNodes,
		/// <summary>
		/// 
		/// </summary>
		PutDirs,
		/// <summary>
		/// 
		/// </summary>
		GetDirs,
		/// <summary>
		/// 
		/// </summary>
		DeleteNodes,
		/// <summary>
		/// 
		/// </summary>
		OpenFilePut,
		/// <summary>
		/// 
		/// </summary>
		OpenFileGet,
		/// <summary>
		/// 
		/// </summary>
		GetHashMap,
		/// <summary>
		/// 
		/// </summary>
		PutHashMap,
		/// <summary>
		/// 
		/// </summary>
		ReadFile,
		/// <summary>
		/// 
		/// </summary>
		WriteFile,
		/// <summary>
		/// 
		/// </summary>
		CopyFile,
		/// <summary>
		/// 
		/// </summary>
		CloseFile,
		/// <summary>
		/// 
		/// </summary>
		EndSync,
	}

	/// <summary>
	/// Class used to talk to the HTTP SyncService (SyncHandler.ashx.cs). 
	/// </summary>
	public class HttpSyncProxy
	{
		static string				version = "1.0";
		Collection					collection;
		string						domainId;
		string						url;
		string						userName;
		string						userID;
		WebState					webState;

		/// <summary>
		/// Sorry Russ Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="userName"></param>
		/// <param name="userID"></param>
		public HttpSyncProxy(Collection collection, string userName, string userID)
		{
			this.collection = collection;
			this.domainId = collection.Domain;
			url = collection.MasterUrl.ToString().TrimEnd('/') + "/SyncHandler.ashx";
			this.userName = userName;
			this.userID = userID;
			webState = new WebState(collection.Domain, collection.ID, userID);
		}

		/// <summary>
		/// Gets an Http Request object with the specified Sync method set.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		private HttpWebRequest GetRequest(SyncMethod method)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			webState.InitializeWebRequest(request);
			request.ContentType = "application/octet-stream";
			request.Method = "POST";
			WebHeaderCollection headers = request.Headers;
			headers.Add(SyncHeaders.SyncVersion, version);
			headers.Add(SyncHeaders.Method, method.ToString());
			headers.Add(SyncHeaders.UserName, userName);
			headers.Add(SyncHeaders.UserID, userID);
			headers.Add(SyncHeaders.CollectionName, collection.Name);
			headers.Add(SyncHeaders.CollectionID, collection.ID);
			headers.Add(Simias.Security.Web.AuthenticationService.Login.DomainIDHeader, domainId);
			return request;
		}

		/// <summary>
		/// Return the exception and include the returned details.
		/// </summary>
		/// <param name="response">The response from the server.</param>
		/// <returns>An Exception.</returns>
		private SimiasException GetException(HttpWebResponse response)
		{
			string message;
			if (response.ContentLength != 0 && response.ContentType == "text/html")
			{
				StreamReader reader = new StreamReader(response.GetResponseStream());
				try
				{
					char[] info = new char[response.ContentLength];
					reader.Read(info, 0, info.Length);
					message = string.Format("{0} \n {1}", response.StatusDescription, info);
					return new SimiasException(message);
				}
				catch {/* Fall through and return the default exception.*/ }
				finally
				{
					reader.Close();
				}
			}
			return new SimiasException(response.StatusDescription);
		}
		
		/// <summary>
		/// Start a sync pass.
		/// </summary>
		/// <param name="si">The StartSyncInfo to control the sync.</param>
		public void StartSync(ref StartSyncInfo si)
		{
			HttpWebRequest request = GetRequest(SyncMethod.StartSync);
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			si.Serialize(writer);
			writer.Close();

			HttpWebResponse response = null;
			try
			{
				response = (HttpWebResponse)request.GetResponse();
			}
			catch( WebException webEx )
			{
				HttpWebResponse resp = webEx.Response as HttpWebResponse;
				if ( resp != null )
				{
					if ( resp.StatusCode == HttpStatusCode.BadRequest )
					{
						log.Debug( "Russ Fix ME!" );
						Collection collection = Store.GetStore().GetCollectionByID( si.CollectionID );
						new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( collection.Domain, collection.ID ) );
					}
				}

				throw webEx;
			}

			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					CookieCollection cc = request.CookieContainer.GetCookies(new Uri(url));
					foreach (Cookie cookie in cc)
					{
						cookie.Expired = true;
					}
					throw GetException(response);
				}
				// Now get the StartSyncInfo back;
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				si = new StartSyncInfo(reader);
				request.CookieContainer.Add(response.Cookies);
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Returns the next set of NodeInfos.
		/// </summary>
		/// <returns></returns>
		public SyncNodeInfo[] GetNextInfoList(out string context)
		{
			HttpWebRequest request = GetRequest(SyncMethod.GetNextInfoList);
			request.ContentLength = 0;
			request.KeepAlive = false;
			request.GetRequestStream().Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Get the context;
				context = response.Headers.Get(SyncHeaders.SyncContext);
				// Get the size of the returned array.
				int count = int.Parse(response.Headers.Get(SyncHeaders.ObjectCount));
				if (count == 0)
					return null;
				SyncNodeInfo[] infoArray = new SyncNodeInfo[count];
				// Now get the StartSyncInfo back;
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				for (int i = 0; i < count; i++)
				{
					infoArray[i] = new SyncNodeInfo(reader);
				}
				return infoArray;
			}
			finally
			{
				response.Close();
			}
		}
		
		/// <summary>
		/// Sync the supplied nodes to the server.
		/// </summary>
		/// <param name="nodes">The nodes to sync.</param>
		/// <returns>The status of the sync.</returns>
		public SyncNodeStatus[] PutNodes(SyncNode[] nodes)
		{
			HttpWebRequest request = GetRequest(SyncMethod.PutNodes);
			return PutNodes(request, nodes);
		}
		
		/// <summary>
		/// Download the nodes from the server.
		/// </summary>
		/// <param name="nids">The nodes to sync down.</param>
		/// <returns>The nodes requested.</returns>
		public SyncNode[] GetNodes(string[] nids)
		{
			HttpWebRequest request = GetRequest(SyncMethod.GetNodes);
			return GetNodes(request, nids);
		}

		/// <summary>
		/// Sync the Directory nodes to the server.
		/// </summary>
		/// <param name="nodes">The nodes to sync.</param>
		/// <returns>The status of the sync.</returns>
		public SyncNodeStatus[] PutDirs(SyncNode[] nodes)
		{
			HttpWebRequest request = GetRequest(SyncMethod.PutDirs);
			return PutNodes(request, nodes);
		}

		/// <summary>
		/// Sync the supplied nodes to the server. The method is already set.
		/// This is used by Generic and Directory nodeTypes.
		/// </summary>
		/// <param name="request">The HttpWebRequest to use.</param>
		/// <param name="nodes">The nodes to sync.</param>
		/// <returns>The status of the sync.</returns>
		private SyncNodeStatus[] PutNodes(HttpWebRequest request, SyncNode[] nodes)
		{
			WebHeaderCollection headers = request.Headers;
			// Get the length to send.
			headers.Add(SyncHeaders.ObjectCount, nodes.Length.ToString());
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			foreach (SyncNode sNode in nodes)
			{
				sNode.Serialize(writer);
			}
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the status.
				int count = int.Parse(response.Headers.Get(SyncHeaders.ObjectCount));
				SyncNodeStatus[] status = new SyncNodeStatus[count];
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				for (int i = 0; i < status.Length; ++i)
				{
					status[i] = new SyncNodeStatus(reader);
				}
				return status;
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Download the Directory nodes from the server.
		/// </summary>
		/// <param name="nids">The nodes to sync down.</param>
		/// <returns>The nodes requested.</returns>
		public SyncNode[] GetDirs(string[] nids)
		{
			HttpWebRequest request = GetRequest(SyncMethod.GetDirs);
			return GetNodes(request, nids);
		}

		/// <summary>
		/// Download the nodes from the server. The method is already set.
		/// This is used by Generic and Directory nodeTypes.
		/// </summary>
		/// <param name="request">The request object.</param>
		/// <param name="nids">The nodes to sync down.</param>
		/// <returns>The nodes requested.</returns>
		private SyncNode[] GetNodes(HttpWebRequest request, string[] nids)
		{
			WebHeaderCollection headers = request.Headers;
			request.ContentLength = 16 * nids.Length;
			headers.Add(SyncHeaders.ObjectCount, nids.Length.ToString());
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			foreach (string nid in nids)
			{
				writer.Write(new Guid(nid).ToByteArray());
			}
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get Nodes.
				SyncNode[] nodes = new SyncNode[nids.Length];
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				for (int i = 0; i < nodes.Length; ++i)
				{
					nodes[i] = new SyncNode(reader);
				}
				return nodes;
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Delete the nodes on the server.
		/// </summary>
		/// <param name="nodeIDs">The array of node IDs to delete.</param>
		/// <returns>An array of status codes.</returns>
		public SyncNodeStatus[] DeleteNodes(string[] nodeIDs)
		{
			HttpWebRequest request = GetRequest(SyncMethod.DeleteNodes);
			WebHeaderCollection headers = request.Headers;
			// Get the length to send.
			request.ContentLength = nodeIDs.Length * 16;
			headers.Add(SyncHeaders.ObjectCount, nodeIDs.Length.ToString());
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			foreach (string nodeID in nodeIDs)
			{
				writer.Write(new Guid(nodeID).ToByteArray());
			}
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the status.
				int count = int.Parse(response.Headers.Get(SyncHeaders.ObjectCount));
				SyncNodeStatus[] status = new SyncNodeStatus[count];
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				for (int i = 0; i < count; ++i)
				{
					status[i] = new SyncNodeStatus(reader);
				}
				return status;
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Opens the file to sync up to the server.
		/// </summary>
		/// <param name="node">The node to sync.</param>
		/// <returns>The Status of the sync.</returns>
		public SyncStatus OpenFilePut(SyncNode node)
		{
			HttpWebRequest request = GetRequest(SyncMethod.OpenFilePut);
			WebHeaderCollection headers = request.Headers;
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			node.Serialize(writer);
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the Status.
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				return (SyncStatus)reader.ReadByte();
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Opens the file to sync down from the server.
		/// </summary>
		/// <param name="nodeID">The ID of the node.</param>
		/// <returns>The node that represents the file.</returns>
		public SyncNode OpenFileGet(string nodeID)
		{
			HttpWebRequest request = GetRequest(SyncMethod.OpenFileGet);
			WebHeaderCollection headers = request.Headers;
			request.ContentLength = 16;
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			writer.Write(new Guid(nodeID).ToByteArray());
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the SyncNode.
				if (response.ContentLength != 0)
				{
					BinaryReader reader = new BinaryReader(response.GetResponseStream());
					return new SyncNode(reader);
				}
				return null;
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Get the HashMap for the opened file.
		/// </summary>
		/// <returns>The HashData[]</returns>
		public HashData[] GetHashMap()
		{
			HttpWebRequest request = GetRequest(SyncMethod.GetHashMap);
			request.ContentLength = 0;
			request.GetRequestStream().Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the HashData.
				int count = int.Parse(response.Headers.Get(SyncHeaders.ObjectCount));
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				return HashMap.DeSerializeHashMap(reader, count);
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Put the HashMap up to the server.
		/// </summary>
		/// <param name="fStream">The stream of data to create the HashMap of.</param>
		/// <returns>The Status.</returns>
		public SyncStatus PutHashMap(StreamStream fStream)
		{
			HttpWebRequest request = GetRequest(SyncMethod.PutHashMap);
			request.Headers.Add(SyncHeaders.ObjectCount, HashMap.GetBlockCount(fStream.Length).ToString());
			fStream.Position = 0;
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			HashMap.SerializeHashMap(fStream, writer);
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the Status.
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				return (SyncStatus)reader.ReadByte();
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Reads the specified blocks from the server.
		/// </summary>
		/// <param name="seg">The range of blocks to read.</param>
		/// <param name="blockSize">The block size.</param>
		/// <returns>The response to the read. The data is in the responseStream.
		/// This response must be closed.</returns>
		public HttpWebResponse ReadFile(DownloadSegment seg, int blockSize)
		{
			HttpWebRequest request = GetRequest(SyncMethod.ReadFile);
			WebHeaderCollection headers = request.Headers;
			headers.Add(SyncHeaders.BlockSize, blockSize.ToString());
			request.ContentLength = DownloadSegment.InstanceSize;
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			seg.Serialize(writer);
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
			{
				return response;
			}
			// If we get here we had an error.
			response.Close();
			throw GetException(response);
		}

		/// <summary>
		/// Write the file data to the server.
		/// </summary>
		/// <param name="stream">The stream containing the data.</param>
		/// <param name="offset">The offset to write at.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void WriteFile(StreamStream stream, long offset, int count)
		{
			HttpWebRequest request = GetRequest(SyncMethod.WriteFile);
			WebHeaderCollection headers = request.Headers;
			request.ContentLength = count;
			headers.Add(SyncHeaders.Range, offset.ToString() + "-" + ((long)(offset + count)).ToString());
			Stream rStream = request.GetRequestStream();
			int bytesRead = stream.Read(rStream, count);
			if (bytesRead != count)
				throw new SimiasException("Could not write all data.");
			rStream.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Called to copy data from the original file on the server to the new file.
		/// </summary>
		/// <param name="copyArray">The array of blocks and offsets to copy from the original file.</param>
		public void CopyFile(ArrayList copyArray)
		{
			HttpWebRequest request = GetRequest(SyncMethod.CopyFile);
			WebHeaderCollection headers = request.Headers;
			headers.Add(SyncHeaders.ObjectCount, copyArray.Count.ToString());
			headers.Add(SyncHeaders.BlockSize, HashData.BlockSize.ToString());
			request.ContentLength = copyArray.Count * BlockSegment.InstanceSize;
			// TODO: This is a kluge to work around a Apache/mod_ssl memory problem.
			byte[] copyData = new byte[request.ContentLength];
			BinaryWriter writer = new BinaryWriter(new MemoryStream(copyData));
			//BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			foreach (BlockSegment seg in copyArray)
			{
				seg.Serialize(writer);
			}
			writer.Close();
			Stream rStream = request.GetRequestStream();
			rStream.Write(copyData, 0, copyData.Length);
			rStream.Close();
			// TODO: End.

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Called to close a downloaded file.
		/// </summary>
		public void CloseFile()
		{
			CloseFile(false);
		}

		/// <summary>
		/// Called to close the file being synced.
		/// </summary>
		/// <param name="commit">True if the file should be commited.</param>
		/// <returns>The sync status.</returns>
		public SyncNodeStatus CloseFile(bool commit)
		{
			HttpWebRequest request = GetRequest(SyncMethod.CloseFile);
			WebHeaderCollection headers = request.Headers;
			request.ContentLength = 1;
			BinaryWriter writer = new BinaryWriter(request.GetRequestStream());
			writer.Write(commit);
			writer.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
				// Now get the status.
				BinaryReader reader = new BinaryReader(response.GetResponseStream());
				return new SyncNodeStatus(reader);
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Called to end this sync cycle.
		/// </summary>
		public void EndSync()
		{
			HttpWebRequest request = GetRequest(SyncMethod.EndSync);
			request.ContentLength = 0;
			request.GetRequestStream().Close();
			request.KeepAlive = false;
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw GetException(response);
				}
			}
			finally
			{
				response.Close();
			}
		}
	}

	/// <summary>
	/// Class to Handle the request made via the HTTP handler.
	/// </summary>
	public class HttpService
	{
		SyncService service;

		/// <summary>
		/// Start the sync pass.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		/// <param name="session">The Http session.</param>
		public void StartSync(HttpRequest request, HttpResponse response, HttpSessionState session)
		{
			string userID = request.Headers.Get(SyncHeaders.UserID);
			BinaryReader reader = new BinaryReader(request.InputStream);
			StartSyncInfo si = new StartSyncInfo(reader);
			service = new SyncService();
			service.Start(ref si, userID, session.SessionID);
			response.ContentType = "application/octet-stream";
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			si.Serialize(writer);
			writer.Close();
		}

		/// <summary>
		/// Get the next batch of nodes.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void GetNextInfoList(HttpRequest request, HttpResponse response)
		{
			int count = 100;
			string context;
			SyncNodeInfo[] infoArray = service.NextNodeInfoList(ref count, out context);
			response.ContentType = "application/octet-stream";
			response.AddHeader(SyncHeaders.SyncContext, context);
			response.AddHeader(SyncHeaders.ObjectCount, count.ToString());
			if (count > 0)
			{
				BinaryWriter writer = new BinaryWriter(response.OutputStream);
				for (int i = 0; i < count; i++)
				{
					infoArray[i].Serialize(writer);
				}
				writer.Close();
			}
		}
		
		
		/// <summary>
		/// Store the nodes in the Simias store.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void PutNodes(HttpRequest request, HttpResponse response)
		{
			string sCount = request.Headers.Get(SyncHeaders.ObjectCount);
			if (sCount == null)
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}
		
			int count = int.Parse(sCount);
			BinaryReader reader = new BinaryReader(request.InputStream);
			SyncNode[] nodes = new SyncNode[count];
			for (int i = 0; i < count; ++i)
			{
				nodes[i] = new SyncNode(reader);
			}
			SyncNodeStatus[] statusArray = service.PutNonFileNodes(nodes);
			// Now write the status back to the client.
			response.ContentType = "application/octet-stream";
			response.AddHeader(SyncHeaders.ObjectCount, statusArray.Length.ToString());
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			foreach (SyncNodeStatus status in statusArray)
			{
				status.Serialize(writer);
			}
			writer.Close();
		}
		
		/// <summary>
		/// Get the nodes from the store.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void GetNodes(HttpRequest request, HttpResponse response)
		{
			string sCount = request.Headers.Get(SyncHeaders.ObjectCount);
			if (sCount == null)
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}
		
			int count = int.Parse(sCount);
			BinaryReader reader = new BinaryReader(request.InputStream);
			string[] nids = new string[count];
			for (int i = 0; i < count; ++i)
			{
				nids[i] = new Guid(reader.ReadBytes(16)).ToString();
			}
			SyncNode[] nodes = service.GetNonFileNodes(nids);
			// Now write the nodes back to the client.
			response.ContentType = "application/octet-stream";
			response.AddHeader(SyncHeaders.ObjectCount, nodes.Length.ToString());
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			foreach (SyncNode node in nodes)
			{
				node.Serialize(writer);
			}
			writer.Close();
		}

		/// <summary>
		/// Store the directory nodes in the Simias store.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void PutDirs(HttpRequest request, HttpResponse response)
		{
			string sCount = request.Headers.Get(SyncHeaders.ObjectCount);
			if (sCount == null)
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}
		
			int count = int.Parse(sCount);
			BinaryReader reader = new BinaryReader(request.InputStream);
			SyncNode[] nodes = new SyncNode[count];
			for (int i = 0; i < count; ++i)
			{
				nodes[i] = new SyncNode(reader);
			}
			SyncNodeStatus[] statusArray = service.PutDirs(nodes);
			// Now write the status back to the client.
			response.ContentType = "application/octet-stream";
			response.AddHeader(SyncHeaders.ObjectCount, statusArray.Length.ToString());
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			foreach (SyncNodeStatus status in statusArray)
			{
				status.Serialize(writer);
			}
			writer.Close();
		}

		/// <summary>
		/// Get the directory nodes from the store.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void GetDirs(HttpRequest request, HttpResponse response)
		{
			GetNodes(request, response);
		}

		/// <summary>
		/// Delete the nodes from the store.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void DeleteNodes(HttpRequest request, HttpResponse response)
		{
			string sCount = request.Headers.Get(SyncHeaders.ObjectCount);
			if (sCount == null)
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}
		
			int count = int.Parse(sCount);
			BinaryReader reader = new BinaryReader(request.InputStream);
			string[] nids = new string[count];
			for (int i = 0; i < count; ++i)
			{
				nids[i] = new Guid(reader.ReadBytes(16)).ToString();
			}
			SyncNodeStatus[] statusArray = service.DeleteNodes(nids);
			// Now write the status back to the client.
			response.ContentType = "application/octet-stream";
			response.AddHeader(SyncHeaders.ObjectCount, statusArray.Length.ToString());
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			foreach (SyncNodeStatus status in statusArray)
			{
				status.Serialize(writer);
			}
			writer.Close();
		}

		/// <summary>
		/// Open the file for an upload.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void OpenFilePut(HttpRequest request, HttpResponse response)
		{
			BinaryReader reader = new BinaryReader(request.InputStream);
			SyncNode node = new SyncNode(reader);
			SyncStatus status = service.PutFileNode(node);
			response.ContentType = "application/octet-stream";
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			writer.Write((byte)status);
			writer.Close();
		}

		/// <summary>
		/// Open the file for a download.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void OpenFileGet(HttpRequest request, HttpResponse response)
		{
			BinaryReader reader = new BinaryReader(request.InputStream);
			string nodeID = new Guid(reader.ReadBytes(16)).ToString();
			SyncNode node = service.GetFileNode(nodeID);
			response.ContentType = "application/octet-stream";
			if (node != null)
			{
				BinaryWriter writer = new BinaryWriter(response.OutputStream);
				node.Serialize(writer);
				writer.Close();
			}
		}

		/// <summary>
		/// Get the hashMap for this file.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		public void GetHashMap(HttpRequest request, HttpResponse response)
		{
			response.ContentType = "application/octet-stream";
			int entryCount;
			byte[] hashMap = service.GetHashMap(out entryCount);
			if (hashMap != null)
			{
				response.AddHeader(SyncHeaders.ObjectCount, entryCount.ToString());
				response.StatusCode = (int)HttpStatusCode.OK;
				response.OutputStream.Write(hashMap, 0, hashMap.Length);
				response.OutputStream.Close();
				return;
			}
			response.AddHeader(SyncHeaders.ObjectCount, "0");
		}

		public void PutHashMap(HttpRequest request, HttpResponse response)
		{
			return;
		}

		/// <summary>
		/// Read the specified bytes from the open file.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void ReadFile(HttpRequest request, HttpResponse response)
		{
			string sBlockSize = request.Headers.Get(SyncHeaders.BlockSize);
			if (sBlockSize != null)
			{
				int blockSize = int.Parse(sBlockSize);
				BinaryReader reader = new BinaryReader(request.InputStream);
				DownloadSegment seg = new DownloadSegment(reader);
				
				// Now send the data back;
				response.ContentType = "application/octet-stream";
				response.BufferOutput = false;
				byte[] buffer = new byte[blockSize];
				Stream outStream = response.OutputStream;
				int readSize = (seg.EndBlock - seg.StartBlock +1) * blockSize;
                int bytesRead = service.Read(outStream, seg.StartBlock * blockSize, readSize);
				outStream.Close();
			}
			else
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
		}

		/// <summary>
		/// Write the specified bytes to the opened file.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void WriteFile(HttpRequest request, HttpResponse response)
		{
			long offset, size;
			if (GetRange(request, out offset, out size))
			{
				service.Write(request.InputStream, offset, (int)size);
			}
			else
				response.StatusCode = (int)HttpStatusCode.BadRequest;
		}

		/// <summary>
		/// Copy the specified range from the orignal file to the new file.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void CopyFile(HttpRequest request, HttpResponse response)
		{
			string sCount = request.Headers.Get(SyncHeaders.ObjectCount);
			string sBlockSize = request.Headers.Get(SyncHeaders.BlockSize);
			if (sCount == null || sBlockSize == null)
			{
				response.StatusCode = (int)HttpStatusCode.BadRequest;
				return;
			}
			
			int count = int.Parse(sCount);
			int blockSize = int.Parse(sBlockSize);
			BinaryReader reader = new BinaryReader(request.InputStream);
			for (int i = 0; i < count; ++i)
			{	
				BlockSegment bSeg = new BlockSegment(reader);
				service.Copy(bSeg.StartBlock * blockSize, bSeg.Offset, blockSize * (bSeg.EndBlock - bSeg.StartBlock + 1));
			}
		}

		/// <summary>
		/// Close the open file and commit if specified.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void CloseFile(HttpRequest request, HttpResponse response)
		{
			BinaryReader reader = new BinaryReader(request.InputStream);
			bool commit = reader.ReadBoolean();
			SyncNodeStatus status = service.CloseFileNode(commit);
			response.Buffer = true;
			response.ContentType = "application/octet-stream";
			BinaryWriter writer = new BinaryWriter(response.OutputStream);
			status.Serialize(writer);
			writer.Close();
		}

		/// <summary>
		/// Stop this sync cycle.
		/// </summary>
		/// <param name="request">The HttpRequest.</param>
		/// <param name="response">The HttpResponse.</param>
		public void EndSync(HttpRequest request, HttpResponse response)
		{
			service.Stop();
		}

		/// <summary>
		/// Return the file offset and size to read or write.
		/// </summary>
		/// <param name="Request">The request.</param>
		/// <param name="offset">The file offset.</param>
		/// <param name="size">The size.</param>
		/// <returns>True if range found.</returns>
		private bool GetRange(HttpRequest Request, out long offset, out long size)
		{
			string range = Request.Headers.Get(SyncHeaders.Range);
			if (range != null)
			{
				string[] values = range.Split('-');
				if (values.Length == 2)
				{
					offset = long.Parse(values[0]);
					size = long.Parse(values[1]) - offset;
					return true;
				}
			}
			
			offset = size = 0;
			return false;
		}

		/// <summary>
		/// Return the file offset and size to copy.
		/// </summary>
		/// <param name="Request">The request.</param>
		/// <param name="oldOffset">The original file offset.</param>
		/// <param name="offset">The file offset.</param>
		/// <param name="size">The size.</param>
		/// <returns>True if range found.</returns>
		private bool GetRange(HttpRequest Request, out long oldOffset, out long offset, out long size)
		{
			if (GetRange(Request, out offset, out size))
			{
				string cOffset = Request.Headers.Get(SyncHeaders.CopyOffset);
				if (cOffset != null)
				{
					oldOffset = long.Parse(cOffset);
					return true;
				}
			}
			oldOffset = size = 0;
			return false;
		}
	}
}
