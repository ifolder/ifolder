using System;
using Mono.P2p.mDnsResponder;

namespace Simias.Service.mDnsService
{
	/// <summary>
	/// Summary description for mDnsService.
	/// </summary>
	public class mDnsService : BaseProcessService
	{
		static mDnsService service;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			service = new mDnsService();
			service.Run();
		}

		protected override void Start()
		{
			// TODO: Need to issue a ping call to see if the mDnsResponder
			// is already running
			Responder.Startup("");
		}

		protected override void Resume()
		{
			// Don't support Resume
		}

		protected override void Pause()
		{
			// Don't support Pause
		}

		protected override void Custom(int message, string data)
		{
			// Don't support Custom
		}

		protected override void Stop()
		{
			Responder.Shutdown();
		}
	}
}
