using System;
using Mono.P2p.mDnsResponder;

namespace Simias.Service.mDnsService
{
	/// <summary>
	/// Summary description for mDnsService.
	/// </summary>
	public class mDnsService : IThreadService
	{
		public void Start(Simias.Configuration conf)
		{
			// TODO: Need to issue a ping call to see if the mDnsResponder
			// is already running
			Responder.Startup("");
		}

		public void Resume()
		{
			// Don't support Resume
		}

		public void Pause()
		{
			// Don't support Pause
		}

		public void Custom(int message, string data)
		{
			// Don't support Custom
		}

		public void Stop()
		{
			Responder.Shutdown();
		}
	}
}
