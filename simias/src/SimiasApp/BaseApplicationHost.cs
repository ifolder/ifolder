
// Mono.ASPNET.BaseApplicationHost
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Copyright 2004 Novell, Inc
//
using System;

namespace Mono.ASPNET
{
	public class BaseApplicationHost : MarshalByRefObject, IApplicationHost
	{
		string path;
		string vpath;
		IRequestBroker requestBroker;
		EndOfRequestHandler endOfRequest;
		ApplicationServer appserver;
		
		public BaseApplicationHost ()
		{
			endOfRequest = new EndOfRequestHandler (EndOfRequest);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler (OnUnload);
		}

		public void OnUnload (object o, EventArgs args)
		{
			appserver.DestroyHost (this);
		}

		public override object InitializeLifetimeService ()
		{
			return null; // who wants to live forever?
		}

		public ApplicationServer Server {
			get { return appserver; }
			set { appserver = value; }
		}
		
		public string Path {
			get {
				if (path == null)
					path = AppDomain.CurrentDomain.GetData (".appPath").ToString ();

				return path;
			}
		}

		public string VPath {
			get {
				if (vpath == null)
					vpath =  AppDomain.CurrentDomain.GetData (".appVPath").ToString ();

				return vpath;
			}
		}

		public AppDomain Domain {
			get { return AppDomain.CurrentDomain; }
		}
		
		public IRequestBroker RequestBroker
		{
			get { return requestBroker; }
			set { requestBroker = value; }
		}
		
		protected void ProcessRequest (MonoWorkerRequest mwr)
		{
			if (!mwr.ReadRequestData ()) {
				EndOfRequest (mwr);
				return;
			}
			
			mwr.EndOfRequestEvent += endOfRequest;
			mwr.ProcessRequest ();
		}

		public void EndOfRequest (MonoWorkerRequest mwr)
		{
			try {
				mwr.CloseConnection ();
			} catch {
			} finally {
				((BaseRequestBroker) requestBroker).UnregisterRequest (mwr.RequestId);
			}
		}
	}
}

