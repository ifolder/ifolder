/* ====================================================================
 * The Apache Software License, Version 1.1
 *
 * Copyright (c) 2002 Daniel Lopez Ridruejo.
 *           (c) 2002 Ximian, Inc.
 *           All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 *
 * 3. The end-user documentation included with the redistribution,
 *    if any, must include the following acknowledgment:
 *       "This product includes software developed by 
 *        Daniel Lopez Ridruejo (daniel@rawbyte.com) and
 *        Ximian Inc. (http://www.ximian.com)"
 *    Alternately, this acknowledgment may appear in the software itself,
 *    if and wherever such third-party acknowledgments normally appear.
 *
 * 4. The name "mod_mono" must not be used to endorse or promote products 
 *    derived from this software without prior written permission. For written
 *    permission, please contact daniel@rawbyte.com.
 *
 * 5. Products derived from this software may not be called "mod_mono",
 *    nor may "mod_mono" appear in their name, without prior written
 *    permission of Daniel Lopez Ridruejo and Ximian Inc.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL DANIEL LOPEZ RIDRUEJO OR
 * ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 * ====================================================================
 *
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Math = System.Math;

namespace Mono.ASPNET
{
	public class MapPathEventArgs : EventArgs
	{
		string path;
		string mapped;
		bool isMapped;

		public MapPathEventArgs (string path)
		{
			this.path = path;
			isMapped = false;
		}

		public string Path {
			get { return path; }
		}
		
		public bool IsMapped {
			get { return isMapped; }
		}

		public string MappedPath {
			get { return mapped; }
			set {
				mapped = value;
				isMapped = (value != null && value != "");
			}
		}
	}

	public delegate void MapPathEventHandler (object sender, MapPathEventArgs args);
	public delegate void EndOfRequestHandler (MonoWorkerRequest request);
	
	public abstract class MonoWorkerRequest : SimpleWorkerRequest
	{
		IApplicationHost appHostBase;
		Encoding encoding;
		byte [] queryStringBytes;
		string hostVPath;
		string hostPath;

		public MonoWorkerRequest (IApplicationHost appHost)
			: base (String.Empty, String.Empty, null)
		{
			if (appHost == null)
				throw new ArgumentNullException ("appHost");

			appHostBase = appHost;
		}

		public event MapPathEventHandler MapPathEvent;
		public event EndOfRequestHandler EndOfRequestEvent;
		
		string HostPath {
			get { 
				if (hostPath == null)
					hostPath = appHostBase.Path;

				return hostPath;
			}
		}

		string HostVPath {
			get { 
				if (hostVPath == null)
					hostVPath = appHostBase.VPath;

				return hostVPath;
			}
		}

		protected virtual Encoding Encoding {
			get {
				if (encoding == null)
					encoding = new UTF8Encoding (false);

				return encoding;
			}

			set { encoding = value; }
		}

		public override string GetAppPath ()
		{
			return HostVPath;
		}

		public override string GetAppPathTranslated ()
		{
			return HostPath;
		}

		public override string GetFilePathTranslated ()
		{
			return MapPath (GetFilePath ());
		}

		public override string GetLocalAddress ()
		{
			return "localhost";
		}

		public override string GetServerName ()
		{
			string hostHeader = GetKnownRequestHeader(HeaderHost);
			if (hostHeader == null || hostHeader.Length == 0) {
				hostHeader = GetLocalAddress ();
			} else {
				int colonIndex = hostHeader.IndexOf (':');
				if (colonIndex > 0) {
					hostHeader = hostHeader.Substring (0, colonIndex);
				} else if (colonIndex == 0) {
					hostHeader = GetLocalAddress ();
				}
			}
			return hostHeader;
		}


		public override int GetLocalPort ()
		{
			return 0;
		}

		public override byte [] GetPreloadedEntityBody ()
		{
			return null;
		}

		public override byte [] GetQueryStringRawBytes ()
		{
			if (queryStringBytes == null) {
				string queryString = GetQueryString ();
				if (queryString != null)
					queryStringBytes = Encoding.GetBytes (queryString);
			}

			return queryStringBytes;
		}

		string DoMapPathEvent (string path)
		{
			if (MapPathEvent != null) {
				MapPathEventArgs args = new MapPathEventArgs (path);
				foreach (MapPathEventHandler evt in MapPathEvent.GetInvocationList ()) {
					evt (this, args);
					if (args.IsMapped)
						return args.MappedPath;
				}
			}

			return null;
		}
		
		public override string MapPath (string path)
		{
			string eventResult = DoMapPathEvent (path);
			if (eventResult != null)
				return eventResult;

			if (path == null || path.Length == 0 || path == HostVPath)
				return HostPath.Replace ('/', Path.DirectorySeparatorChar);

			if (path [0] == '~' && path.Length > 2 && path [1] == '/')
				path = path.Substring (1);

			int len = HostVPath.Length;
			if (path == HostVPath || path.StartsWith (HostVPath + "/"))
				path = path.Substring (len + 1);

			if (path.Length > 0 && path [0] == '/')
				path = path.Substring (1);

			return Path.Combine (HostPath, path.Replace ('/', Path.DirectorySeparatorChar));
		}

		protected abstract bool GetRequestData ();

		public bool ReadRequestData ()
		{
			return GetRequestData ();
		}

		public void ProcessRequest ()
		{
			HttpRuntime.ProcessRequest (this);
		}

		public override void EndOfRequest ()
		{
			if (EndOfRequestEvent != null)
				EndOfRequestEvent (this);
		}

		public override void SendCalculatedContentLength (int contentLength)
		{
			//FIXME: Should we ignore this for apache2?
			SendUnknownResponseHeader ("Content-Length", contentLength.ToString ());
		}

		public override void SendKnownResponseHeader (int index, string value)
		{
			if (HeadersSent ())
				return;

			string headerName = HttpWorkerRequest.GetKnownResponseHeaderName (index);
			SendUnknownResponseHeader (headerName, value);
		}

		private void SendStream (Stream stream, long offset, long length)
		{
			if (offset < 0 || length <= 0)
				return;
			
			long stLength = stream.Length;
			if (offset + length > stLength)
				length = stLength - offset;

			if (offset > 0)
				stream.Seek (offset, SeekOrigin.Begin);

			byte [] fileContent = new byte [8192];
			int count = fileContent.Length;
			while (length > 0 && (count = stream.Read (fileContent, 0, count)) != 0) {
				SendResponseFromMemory (fileContent, count);
				length -= count;
				count = (int) Math.Min (length, fileContent.Length);
			}
		}

		public override void SendResponseFromFile (string filename, long offset, long length)
		{
			Stream file = null;
			try {
				file = File.OpenRead (filename);
				SendStream (file, offset, length);
			} finally {
				if (file != null)
					file.Close ();
			}
		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length)
		{
			Stream file = null;
			try {
				file = new FileStream (handle, FileAccess.Read);
				SendStream (file, offset, length);
			} finally {
				if (file != null)
					file.Close ();
			}
		}
	}
}

