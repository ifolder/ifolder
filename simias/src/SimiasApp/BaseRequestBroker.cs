//
// Mono.ASPNET.BaseRequestBroker
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
// 	Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Copyright 2004 Novell, Inc
//

using System;
using System.Collections;

namespace Mono.ASPNET
{
	public class BaseRequestBroker: MarshalByRefObject, IRequestBroker
	{
		Hashtable requests = new Hashtable ();
		
		internal int RegisterRequest (IWorker worker)
		{
			int result = worker.GetHashCode ();
			lock (requests) {
				requests [result] = worker;
			}

			return result;
		}
		
		internal void UnregisterRequest (int id)
		{
			lock (requests) {
				requests.Remove (id);
			}
		}
		
		public int Read (int requestId, int size, out byte[] buffer)
		{
			buffer = new byte[size];
			IWorker w;
			lock (requests) {
				w = (IWorker) requests [requestId];
			}

			int nread = 0;
			if (w != null)
				nread = w.Read (buffer, 0, size);

			return nread;
		}
		
		public IWorker GetWorker (int requestId)
		{
			lock (requests) {
				return (IWorker) requests [requestId];
			}
		}
		
		public void Write (int requestId, byte[] buffer, int position, int size)
		{
			IWorker worker = GetWorker (requestId);
			if (worker != null)
				worker.Write (buffer, position, size);
		}
		
		public void Close (int requestId)
		{
			IWorker worker = GetWorker (requestId);
			if (worker != null)
				worker.Close ();
		}
		
		public void Flush (int requestId)
		{
			IWorker worker = GetWorker (requestId);
			if (worker != null)
				worker.Flush ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}
	}
}

