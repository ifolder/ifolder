using System;

namespace Simias.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class ServiceEventArgs : EventArgs
	{
		public static int	TargetAll = 0;
		int					target;
		ServiceEvent		eventType;
		
		/// <summary>
		/// Service Events.
		/// </summary>
		public enum ServiceEvent
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
		/// Constructs a ServiceEventArgs to describe the event.
		/// </summary>
		/// <param name="Target">The Process ID of the service to signal.</param>
		/// <param name="eventType">The event to execute.</param>
		public ServiceEventArgs(int target, ServiceEvent eventType)
		{
			this.target = target;
			this.eventType = eventType;
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
		public ServiceEvent EventType
		{
			get {return eventType;}
		}
	}
}
