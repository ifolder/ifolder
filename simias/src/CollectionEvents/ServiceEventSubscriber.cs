using System;

namespace Simias.Event
{
	/// <summary>
	/// Class to listen to Service events.
	/// </summary>
	public class ServiceEventSubscriber : MarshalByRefObject, IDisposable
	{
		#region Events
		/// <summary>
		/// Delegate used to control services in the system.
		/// </summary>
		public event ServiceEventHandler ServiceControl;
		#endregion

		#region Private Fields
		EventBroker broker;
		string		userName;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to listen for service events.
		/// </summary>
		/// <param name="domain">The domain for this publisher obtained from the CollectionStore.</param>
		public ServiceEventSubscriber(Configuration conf, string domain)
		{
			userName = System.Environment.UserName;
			enabled = true;
			alreadyDisposed = false;
			
			EventBroker.RegisterClientChannel(conf, domain);
			broker = new EventBroker();
			broker.ServiceControl += new ServiceEventHandler(OnServiceControl);
		}

		
		/// <summary>
		/// Finalizer.
		/// </summary>
		~ServiceEventSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		/// <summary>
		/// Gets the User That this service listener is running as.
		/// </summary>
		public string UserName
		{
			get {return userName;}
		}
		#endregion

		#region Callbacks

		/// <summary>
		/// Callback used to control services in the Simias System.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		//[OneWay]
		public void OnServiceControl(ServiceEventArgs args)
		{
			if (ServiceControl != null && userName == args.UserName)
			{
				Delegate[] cbList = ServiceControl.GetInvocationList();
				foreach (ServiceEventHandler cb in cbList)
				{
					try 
					{ 
						cb(args);
					}
					catch 
					{
						// Remove the offending delegate.
						ServiceControl -= cb;
					}
				}
			}
		}

		#endregion

		#region Private Methods
		
		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					broker.ServiceControl -= new ServiceEventHandler(OnServiceControl);
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
		}

		#endregion

		#region MarshalByRefObject overrides

		/// <summary>
		/// This object should not time out.
		/// </summary>
		/// <returns></returns>
		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup any resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
