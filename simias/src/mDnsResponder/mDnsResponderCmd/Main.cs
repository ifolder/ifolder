// project created on 4/15/04 at 9:12 a
using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;
//using Mono.P2p.mDnsResponder;
using Mono.P2p.mDnsResponderApi;
using log4net;
using log4net.Config;

class mDnsCmd
{
	private static readonly log4net.ILog log = 
		log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	bool				displayCommandHelp = false;
	bool				timeIt = false;
	bool				verbose = false;
	int					timeToLive = 300;
	int					weight = 0;
	int					priority = 0;
	short				port = 0;
	private	ArrayList	ptrList = null;
	private ArrayList	hostList = null;
	private	ArrayList	addressList = null;
	private ArrayList	serviceList = null;
	private ArrayList	textStrings = null;

	public enum Commands : uint
	{
		registerhost = 1,
		registerptr,
		registerservicelocation,
		registertextstrings,
		dumpresources,
		dumplocalresources,
		dumpstats
	}

	public 	Commands		cmd;

	public static int Main(string[] args)
	{
		BasicConfigurator.Configure();
		mDnsCmd	cCmd = new mDnsCmd();

		if (cCmd.ParseArguments(args))
		{
			if(cCmd.ProcessCommands() == false)
			{
				return(-1);
			}
		}
		else
		{
			/*
			if (cAbc.errorMsg != null)
			{
				Console.WriteLine(cAbc.errorMsg);
			}
			*/

			return(-1);
		}

		return(0);

		/*
		if (args.Length >= 1)
		{
			HttpChannel chnl = new HttpChannel();
			ChannelServices.RegisterChannel(chnl);
			
			IRemoteFactory factory = 
				(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"http://localhost:8091/factory.soap");

			if (args[0].ToLower() == "lookupresource" && args.Length >= 2)
			{
				int			status;
				short		rType = 0;
				
				IResourceQuery rq = factory.GetQueryInstance();
				status = rq.LookupResource(args[1], ref rType);
				if (status == 0)
				{
					if (rType == (short) mDnsType.serviceLocation)
					{
			 			int		port = 0;
			 			int		priority = 0;
			 			int		weight = 0;
			 			string 	host = "";
			 			
						rq.LookupServiceLocation(
							"bradys-ifolder._collection._tcp.local",
							ref host,
							ref port,
							ref priority,
							ref weight);
					}
				}
			}
			else
			if (args[0].ToLower() == "registerhost")
			{
				IResourceRegistration rr = factory.GetRegistrationInstance();		
 				rr.RegisterHost("BRADYS-FUNKY-MACX-MACHINE.local", "192.168.1.101");
			}
			else
			if (args[0].ToLower() == "servicelocation")
			{
				IResourceRegistration rr = factory.GetRegistrationInstance();		
 				rr.RegisterServiceLocation("BRADY-T21.local", "bradys-ifolder._collection._tcp.local", 6666, 0, 0);
 			}
 			else
 			if (args[0].ToLower() == "lookup")
 			{
 				string	host = null;
 				int		port = 0;
 				int		priority = 0;
 				int		weight = 0;
 				
				IResourceQuery rq = factory.GetQueryInstance();
				rq.LookupServiceLocation(
					"bradys-ifolder._collection._tcp.local",
					ref host,
					ref port,
					ref priority,
					ref weight);
					
				Console.WriteLine("Host: " + host);
				Console.WriteLine("Port: " + port.ToString());		
 			}
 			else
 			if (args[0].ToLower() == "host")
 			{
 				RHostAddress	ha = null;

				IResourceQuery rq = factory.GetQueryInstance();
				rq.LookupHost("BRADY-T21", ref ha);
					
				Console.WriteLine("IPADDRESS: " + ha.PrefAddress);
 			}
		}
		*/
	}

	void DisplayUsage()
	{
		Console.WriteLine("mDnsResponderCmd [command] <parameters> -t -v");
		Console.WriteLine("    registerhost -h <hostname> -a <address>");
		Console.WriteLine("    registerptr  -p <ptr name> -h <target host> -l <TTL in seconds>");
		Console.WriteLine("    registerservicelocation -h <hostname> -s <servicename> -p <port> -w <weight> -r <pRiority>");
		Console.WriteLine("    registertextstrings -s <servicename> -t <textstring>");
		Console.WriteLine("    dumpresources");
		Console.WriteLine("    dumplocalresources");
		Console.WriteLine("	   dumpstats");
		Console.WriteLine("   -t = time the command");
		Console.WriteLine("   -v = verbose");
		return;
	}

	private bool ParseArguments(string [] args)
	{
		if (args.Length == 0)
		{
			DisplayUsage();
			return(false);
		}

		string firstArg = args[0].ToLower();
		if (firstArg == "--help")
		{
			DisplayUsage();
			return(false);
		}

		// Which command
		switch(firstArg)
		{
			case "registerhost":
				this.cmd = Commands.registerhost;
				break;
					
			case "registerptr":
				this.cmd = Commands.registerptr;
				break;
					
			case "registerservicelocation":
				this.cmd = Commands.registerservicelocation;
				break;
					
			case "registertextstrings":
				this.cmd = Commands.registertextstrings;
				break;

			case "dumpresources":
				this.cmd = Commands.dumpresources;
				break;

			case "dumplocalresources":
				this.cmd = Commands.dumplocalresources;
				break;
					
			default:
				log.Error("invalid command");
				return(false);
		}

		for(int i = 1; i < args.Length; i++)
		{
			string cParm = args[i].ToLower();

			if (cParm == "--help")
			{
				this.displayCommandHelp = true;
			}
			else
			if (args[i].Length == 2)
			{
				if (cParm == "-d" &&
					Commands.registertextstrings == this.cmd)
				{
					// Get the host used for the command
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							this.AddService(args[i]);
						}
					}
				}
				else
				if (cParm == "-h")
				{
					// Get the host used for the command
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							this.AddHost(args[i]);
						}
					}
				}
				else
				if (args[i] == "-p")
				{
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							if (Commands.registerptr == this.cmd)
							{
								this.AddPtr(args[i]);
							}
							else
							{
								this.port = Convert.ToInt16(args[i], 10);
							}
						}
					}
				}
				else
				if (args[i] == "-s")
				{
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							if (Commands.registertextstrings == this.cmd)
							{
								this.AddTextStrings(args[i]);
							}
							else
							if (Commands.registerservicelocation == this.cmd)
							{
								this.AddService(args[i]);
							}
						}
					}
				}
				else
				if (args[i] == "-a")
				{
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							this.AddAddress(args[i]);
						}
					}
				}
				else
				if (args[i] == "-l")
				{
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							this.timeToLive = Convert.ToInt32(args[i], 10);
						}
					}
				}
				else
				if (args[i] == "-w")
				{
					if (i + 1 <= args.Length)
					{
						if (args[++i] != null)
						{
							this.weight = Convert.ToInt32(args[i], 10);
						}
					}
				}
				else
				if (args[i] == "-t")
				{	
					this.timeIt = true;
				}
				else
					if (args[i] == "-v")
				{
					this.verbose = true;
				}
			}
		}

		return(true);
	}

	public void AddAddress(string addr)
	{
		if (this.addressList == null)
		{
			this.addressList = new ArrayList();
		}

		this.addressList.Add(addr);
	}

	public void AddHost(string host)
	{
		if (this.hostList == null)
		{
			this.hostList = new ArrayList();
		}

		this.hostList.Add(host);
	}
	public void AddPtr(string ptr)
	{
		if (this.ptrList == null)
		{
			this.ptrList = new ArrayList();
		}

		this.ptrList.Add(ptr);
	}

	public void AddService(string svc)
	{
		if (this.serviceList == null)
		{
			this.serviceList = new ArrayList();
		}

		this.serviceList.Add(svc);
	}

	public void AddTextStrings(string txtStr)
	{
		if (this.textStrings == null)
		{
			this.textStrings = new ArrayList();
		}

		this.textStrings.Add(txtStr);
	}

	public bool	ProcessCommands()
	{
		//AddressBook	cAddressBook = null;
		//Contact		cContact = null;
		//Manager		abManager;
		DateTime	start = DateTime.Now;

		HttpChannel chnl = new HttpChannel();
		ChannelServices.RegisterChannel(chnl);
			
		IRemoteFactory factory = 
			(IRemoteFactory) Activator.GetObject(
			typeof(IRemoteFactory),
			"http://localhost:8091/factory.soap");

		if (this.cmd == Commands.registerptr)
		{
			if (verbose == true)
			{
				log.Info("Command::registerptr");
			}

			try
			{
				if (ptrList != null && ptrList[0] != null &&
					hostList != null && hostList[0] != null)
				{
					IResourceRegistration rr = factory.GetRegistrationInstance();		
					rr.RegisterPointer((string) ptrList[0], (string) hostList[0]);
				}
			}
			catch{}
		}
		else
		if (this.cmd == Commands.registerhost)
		{
			if (verbose == true)
			{
				log.Info("Command::registerhost");
			}

			try
			{
				if (hostList != null && hostList[0] != null &&
					addressList != null && addressList[0] != null)
				{
					IResourceRegistration rr = factory.GetRegistrationInstance();		
					rr.RegisterHost((string) hostList[0], (string) addressList[0]);
				}
			}
			catch{}
		}
		else
		if (this.cmd == Commands.registerservicelocation)
		{
			if (verbose == true)
			{
				log.Info("Command::registerservicelocation");
			}

			try
			{
				if (hostList != null && hostList[0] != null &&
					serviceList != null && serviceList[0] != null)
				{
					IResourceRegistration rr = factory.GetRegistrationInstance();		
					rr.RegisterServiceLocation((string) hostList[0], (string) serviceList[0], (int) this.port, this.priority, this.weight);
				}
				else
				{
					log.Info("missing parameters");
				}
			}
			catch{}
		}
		else
		if (this.cmd == Commands.registertextstrings)
		{
			if (verbose == true)
			{
				log.Info("Command::registertextstrings");
			}

			try
			{
				if (hostList != null && hostList[0] != null &&
					textStrings != null && textStrings[0] != null)
				{
					string[] txtStrings = new string[textStrings.Count];
					
					int	i = 0;
					foreach(string ss in textStrings)
					{
						txtStrings[i++] = ss;
					}

					IResourceRegistration rr = factory.GetRegistrationInstance();		
					rr.RegisterTextStrings((string) hostList[0], (string[]) txtStrings);
				}
				else
				{
					log.Info("missing parameters");
				}
			}
			catch{}
		}
		else
		if (this.cmd == Commands.dumpresources)
		{
			if (verbose == true)
			{
				log.Info("Command::dumpresources");
			}

			try
			{
				ImDnsLog dl = factory.GetLogInstance();		
				dl.DumpResourceRecords();
			}
			catch{}
		}

		if (this.cmd == Commands.dumplocalresources)
		{
			if (verbose == true)
			{
				log.Info("Command::dumplocalresources");
			}

			try
			{
				ImDnsLog dl = factory.GetLogInstance();		
				dl.DumpLocalRecords();
			}
			catch{}
		}
				
		if (timeIt == true)
		{
			DateTime stop = DateTime.Now;

			// Difference in days, hours, and minutes.
			TimeSpan ts = stop - start;
			log.Info("");
			log.Info("Command completed in: ");
			log.Info("   " + ts.TotalSeconds.ToString() + " seconds");
			log.Info("   " + ts.TotalMilliseconds.ToString() + " milliseconds");
		}
			
		return(true);
	}


}