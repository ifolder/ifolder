using System;

namespace Simias.Event
{
	/// <summary>
	/// Service Events.
	/// </summary>
	public enum ServiceControl
	{
		/// <summary>
		/// The service should shutdown.
		/// </summary>
		Shutdown = 1,
		/// <summary>
		/// The service should reconfigure.
		/// </summary>
		Reconfigure = 2
	};

	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class ServiceEventArgs : CollectionEventArgs
	{
		/// <summary>
		/// Used to target all services.
		/// </summary>
		public static int	TargetAll = 0;
		int					target;
		ServiceControl		controlEvent;
		string				userName;
		
		/// <summary>
		/// Constructs a ServiceEventArgs to describe the event.
		/// </summary>
		/// <param name="target">The Process ID of the service to signal.</param>
		/// <param name="controlEvent">The event to execute.</param>
		public ServiceEventArgs(int target, ServiceControl controlEvent) :
			base(EventType.ServiceControl)
		{
			this.target = target;
			this.controlEvent = controlEvent;
			this.userName = System.Environment.UserName;
		}

		/// <summary>
		/// Gets the target process ID.
		/// </summary>
		public int Target
		{
			get {return target;}
		}

		/// <summary>
		/// Gets the event type.
		/// </summary>
		public ServiceControl ControlEvent
		{
			get {return controlEvent;}
		}
	}
}
