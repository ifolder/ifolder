using System;

namespace Simias.Service
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class ProcessServiceTest : BaseProcessService
	{
		static ProcessServiceTest service;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			service = new ProcessServiceTest();
			service.Run();
		}

		protected override void Start()
		{
		}

		protected override void Stop()
		{
		}

		protected override void Pause()
		{
		}

		protected override void Resume()
		{
		}

		protected override void Custom(int messageId, string message)
		{
		}
	}
}
