/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

//
// Simple command line driven test client for issuing multi-cast
// DNS reqeusts
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

using log4net;
using log4net.Config;

namespace mDnsClient
{
	/// <summary>
	/// Summary description for mDnsTestClient
	/// </summary>
	class mDnsTestClient
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			byte[]		dnsRequest = new byte[8192];
			int			index = 0;
			ushort		flags = 0x0001;
			short		requestType = 0;
//			short		requestClass = -32767;
			short		questions;
			ushort		requestClass = 1;
//			UdpClient	client = null;
			string		qDomain = "*";
			string		localLabel = ".local";

			if (args[0] == "--help")
			{
				Console.WriteLine("mDnsClient <dns domain> -t <type of resource>");
				Console.WriteLine("   -t <haddress, host, ptr, service, txt>");
				return;
			}

			if (args.Length >= 1)
			{
				qDomain = args[0];
			}

			if (args.Length >= 3)
			{
				if (args[1] == "-t")
				{
					switch(args[2])
					{
						case "haddress":
							requestType = 1;
							break;

						case "host":
							requestType = 30;
							break;

						case "ptr":
							requestType = 12;
							break;

						case "service":
							requestType = 33;
							break;

						case "txt":
							requestType = 16;
							break;

						case "all":
							requestType = 255;
							break;
							
						case "guts":
							requestType = 1189;
							break;
					}
				}
			}

			BasicConfigurator.Configure();
			if (log.IsInfoEnabled) log.Info("mDnsClient::Main called");

			Socket client = null;
//			IPEndPoint iep = new IPEndPoint(5353, 5353);
			IPEndPoint iep = null;
			EndPoint ep = (EndPoint) iep;

	
			try
			{
				client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				iep = new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353);
				//iep.LocalEndPoint = 5353;
				
				
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReuseAddress,
					true);

				client.Bind(iep);
				client.SetSocketOption(
					SocketOptionLevel.IP, 
					SocketOptionName.AddMembership,
					new MulticastOption(IPAddress.Parse("224.0.0.251")));

			}
			catch(Exception e)
			{
				if (log.IsDebugEnabled) log.Debug("Failed binding socket");
				if (log.IsDebugEnabled) log.Debug(e.Message);
				if (log.IsDebugEnabled) log.Debug(e.StackTrace);
				return;
			}

			/*
			try
			{
				// Setup an endpoint to multi-cast datagrams
				client = new UdpClient("224.0.0.251", 5353);
			}
			catch(Exception e)
			{
				if (log.IsDebugEnabled) log.Debug(e.Message);
				if (log.IsDebugEnabled) log.Debug(e.StackTrace);
				return;
			}
			*/

			// Transaction ID
			Buffer.BlockCopy(BitConverter.GetBytes(11), 0, dnsRequest, index, 2);
			index += 2;

			// Flags
			//flags = 0x0001; // recursion
			flags = 0x0000;
			Buffer.BlockCopy(BitConverter.GetBytes(flags), 0, dnsRequest, index, 2);
			index += 2;

			// Questions
			questions = IPAddress.HostToNetworkOrder((short) 1);
			Buffer.BlockCopy(BitConverter.GetBytes(questions), 0, dnsRequest, index, 2);
			index += 2;

			// Answers
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Authorities
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Additional
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Next comes the question section of the packet

			if (args[0] != null)
			{
				int	last = 0;
				int	current = 0;
					
				while( current < args[0].Length )
				{
					if (args[0][current] == '.')
					{
						dnsRequest[index++] = (byte) (current - last);
						while( last < current )
						{
							dnsRequest[index++] = (byte) args[0][last++];
						}
						
						current++;
						last = current;
					}
					else
					{	
						current++;
					}
				}
					
				dnsRequest[index++] = (byte) (current - last);
				while (last < current)
				{
					dnsRequest[index++] = (byte) args[0][last++];
				}
			}
			else
			{
				dnsRequest[index++] = (byte) qDomain.Length;
				for(int i = 0; i < qDomain.Length; i++)
				{
					dnsRequest[index++] = (byte) qDomain[i];
				}

				dnsRequest[index++] = (byte) localLabel.Length;
				for(int i = 0; i < localLabel.Length; i++)
				{
					dnsRequest[index++] = (byte) localLabel[i];
				}
			}

			dnsRequest[index++] = 0;

			// Type
			requestType = (short) IPAddress.HostToNetworkOrder(requestType);
			Buffer.BlockCopy(BitConverter.GetBytes(requestType), 0, dnsRequest, index, 2);
			index += 2;

			// Class
			requestClass = (ushort) IPAddress.HostToNetworkOrder((ushort) 0x8001);
			requestClass = 0x0100;
			Buffer.BlockCopy(BitConverter.GetBytes(requestClass), 0, dnsRequest, index, 2);
			index += 2;
			
			try
			{
				//client.Send(dnsRequest, 0, index, SocketFlags.None);
				client.SendTo(dnsRequest, index, SocketFlags.None, iep);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			
			
			/*
			Thread.Sleep(1000);

			index = 0;
			// Transaction ID
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Flags
			flags = 0x0010;
			Buffer.BlockCopy(BitConverter.GetBytes(flags), 0, dnsRequest, index, 2);
//			Buffer.BlockCopy(BitConverter.GetBytes(flags), 0, dnsRequest, index, 2);
			index += 2;

			// Questions
			questions = IPAddress.HostToNetworkOrder((short) 1);
			Buffer.BlockCopy(BitConverter.GetBytes(questions), 0, dnsRequest, index, 2);
			index += 2;

			// Answers
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Authorities
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Additional
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, dnsRequest, index, 2);
			index += 2;

			// Next comes the question section of the packet

			if (args[0] != null)
			{
				dnsRequest[index++] = (byte) args[0].Length;
				for(int i = 0; i < args[0].Length; i++)
				{
					dnsRequest[index++] = (byte) args[0][i];
				}
			}
			else
			{
				dnsRequest[index++] = (byte) qDomain.Length;
				for(int i = 0; i < qDomain.Length; i++)
				{
					dnsRequest[index++] = (byte) qDomain[i];
				}

				dnsRequest[index++] = (byte) localLabel.Length;
				for(int i = 0; i < localLabel.Length; i++)
				{
					dnsRequest[index++] = (byte) localLabel[i];
				}
			}

			dnsRequest[index++] = 0;

			// Type
			//requestType = (short) IPAddress.HostToNetworkOrder(requestType);
			Buffer.BlockCopy(BitConverter.GetBytes(requestType), 0, dnsRequest, index, 2);
			index += 2;

			// Class
			requestClass = (ushort) IPAddress.HostToNetworkOrder((ushort) 1);
			requestClass = 0x0100;
			Buffer.BlockCopy(BitConverter.GetBytes(requestClass), 0, dnsRequest, index, 2);
			index += 2;

			try
			{
//				client.Send(dnsRequest, 0, index, SocketFlags.None);
				client.SendTo(dnsRequest, index, SocketFlags.None, iep);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			*/

			if (client != null)
			{
				client.Close();
			}
		}
	}
}
