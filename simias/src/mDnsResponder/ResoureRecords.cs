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
// This source file contains classes for the different DNS Resource
// Records used by the multi-cast DNS responder.
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.P2p.mDnsResponder
{
	internal class	Resources
	{
		private static readonly log4net.ILog log = 
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		static internal Mutex		resourceMtx = new Mutex(false);
		static	internal ArrayList	resourceList = new ArrayList();
		static	private	ArrayList	serviceList = new ArrayList();

		static public void DumpYourGuts(Question cQuestion)
		{
			log.Info("DumpYourGuts called");
			
			Resources.resourceMtx.WaitOne();
			
			foreach(BaseResource cResource in Resources.resourceList)
			{
				log.Info("");
				log.Info("  RESOURCE");
				log.Info("  Name:      " + cResource.Name);
				log.Info("  Type:      " + cResource.Type);
				log.Info("  TTL:       " + cResource.Ttl.ToString());
				log.Info("  Owner:     " + cResource.Owner.ToString());
				
				if (cResource.Type == mDnsType.hostAddress)
				{
					HostAddress	hostAddr = (HostAddress) cResource;
					
					try
					{
						log.Info("  IP:        " + hostAddr.PrefAddress.ToString());
					}
					catch{}
				}
				else
				if (cResource.Type == mDnsType.serviceLocation)
				{
					ServiceLocation svcLoc = (ServiceLocation) cResource;
					
					log.Info("  Priority:  " + svcLoc.Priority.ToString());
					log.Info("  Weight:    " + svcLoc.Weight.ToString());
					log.Info("  Port:      " + svcLoc.Port.ToString());
					log.Info("  Target:    " + svcLoc.Target);
				}
				else
				if (cResource.Type == mDnsType.ptr)
				{
					Ptr ptr = (Ptr) cResource;
					log.Info("  Target:    " + ptr.Target);
				}
				else
				if (cResource.Type == mDnsType.textStrings)
				{
					TextStrings txt = (TextStrings) cResource;
					foreach(string txtString in txt.GetTextStrings())
					{
						log.Info("  " + txtString);
					}
				}
				log.Info("");
			}
			Resources.resourceMtx.ReleaseMutex();
			log.Info("DumpYourGuts exit");
		}

		static public void DumpLocalRecords()
		{
			log.Info("DumpLocalResources called");
			
			Resources.resourceMtx.WaitOne();
			
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Owner == true)
				{
					log.Info("");
					log.Info("  RESOURCE");
					log.Info("  Name:      " + cResource.Name);
					log.Info("  Type:      " + cResource.Type);
					log.Info("  TTL:       " + cResource.Ttl.ToString());
				
					if (cResource.Type == mDnsType.hostAddress)
					{
						HostAddress	hostAddr = (HostAddress) cResource;
					
						try
						{
							log.Info("  IP:        " + hostAddr.PrefAddress.ToString());
						}
						catch{}
					}
					else
					if (cResource.Type == mDnsType.serviceLocation)
					{
						ServiceLocation svcLoc = (ServiceLocation) cResource;
					
						log.Info("  Priority:  " + svcLoc.Priority.ToString());
						log.Info("  Weight:    " + svcLoc.Weight.ToString());
						log.Info("  Port:      " + svcLoc.Port.ToString());
						log.Info("  Target:    " + svcLoc.Target);
					}
					else
					if (cResource.Type == mDnsType.ptr)
					{
						Ptr ptr = (Ptr) cResource;
						log.Info("  Target:    " + ptr.Target);
					}
					else
					if (cResource.Type == mDnsType.textStrings)
					{
						TextStrings txt = (TextStrings) cResource;
						foreach(string txtString in txt.GetTextStrings())
						{
							log.Info("  " + txtString);
						}
					}
					log.Info("");
				}
				Resources.resourceMtx.ReleaseMutex();
				log.Info("DumpLocalResources exit");
			}
		}
		
		static public void AddHostAddress(HostAddress hostAddr)
		{
			log.Info("AddHostAddress called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == hostAddr.Name &&
					cResource.Type == hostAddr.Type)
				{
					foundOne = true;
					break;
				}
			}

			if (foundOne == false)
			{
				log.Info("   Adding " + hostAddr.Name);
				Resources.resourceList.Add(hostAddr);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		static public void RemoveHostAddress(string host)
		{
			log.Info("RemoveHostAddress called");
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == host &&
					cResource.Type == mDnsType.hostAddress)
				{
					Resources.resourceList.Remove(cResource);
					break;
				}
			}

			Resources.resourceMtx.ReleaseMutex();
		}

		static public HostAddress GetHostAddress(string host)
		{
			log.Info("GetHostAddress called");
			
			HostAddress	rHost = null;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == host &&
					cResource.Type == mDnsType.hostAddress)
				{
					HostAddress cHost = (HostAddress) cResource;
					
					rHost = new HostAddress(cHost.Name, cHost.Ttl, cHost.Type, cHost.Class, false);
					rHost.AddIPAddress(cHost.PrefAddress);
					break;
				}
			}

			Resources.resourceMtx.ReleaseMutex();
			return(rHost);
		}
		
		static public BaseResource GetResource(string name)
		{
			log.Info("GetResource called");
			
			BaseResource	rResource = null;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == name)
				{
					rResource = cResource;
					break;
				}
			}

			Resources.resourceMtx.ReleaseMutex();
			return(rResource);
		}
		
		static public void AddServiceLocation(ServiceLocation serviceLocation)
		{
			log.Info("AddServiceLocation called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == serviceLocation.Name &&
					cResource.Type == serviceLocation.Type)
				{
					foundOne = true;
					
					// Update the record
					ServiceLocation cLocation = (ServiceLocation) cResource;
					cLocation.Port = serviceLocation.Port;
					cLocation.Weight = serviceLocation.Weight;
					cLocation.Priority = serviceLocation.Priority;
					cLocation.Target = serviceLocation.Target;
					break;
				}
			}

			if (foundOne == false)
			{
				log.Info("   Adding " + serviceLocation.Name);
				Resources.resourceList.Add(serviceLocation);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		static public ServiceLocation GetServiceLocation(string service)
		{
			log.Info("GetServiceLocation called");
			
			ServiceLocation	rLocation = null;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == service &&
					cResource.Type == mDnsType.serviceLocation)
				{
					ServiceLocation cLocation = (ServiceLocation) cResource;
					
					rLocation = new ServiceLocation(cLocation.Name, cLocation.Ttl, cLocation.Type, cLocation.Class, false);
					rLocation.Target = cLocation.Target;
					rLocation.Port = cLocation.Port;
					rLocation.Priority = cLocation.Priority;
					rLocation.Weight = cLocation.Weight;
					break;
				}
			}

			Resources.resourceMtx.ReleaseMutex();
			return(rLocation);
		}

		// TODO need to send a dying record out 
		static public void RemoveServiceLocation(ServiceLocation serviceLocation)
		{
			log.Info("RemoveServiceLocation called");
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == serviceLocation.Name &&
					cResource.Type == serviceLocation.Type)
				{
				
					// Update the record
					ServiceLocation cLocation = (ServiceLocation) cResource;
					if (cLocation.Target == serviceLocation.Target)
					{
						Resources.resourceList.Remove(cResource);
						break;
					}
				}
			}

			Resources.resourceMtx.ReleaseMutex();
		}

		static public void AddPtr(Ptr ptr)
		{
			log.Info("AddPtr called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == ptr.Name &&
					cResource.Type == ptr.Type)
				{
					Ptr cPtr = (Ptr) cResource;
					if(cPtr.Target == ptr.Target)
					{
						foundOne = true;
						if (ptr.Ttl == 0)
						{
							log.Info("   Removing " + ptr.Name);
							Resources.resourceList.Remove(cResource);	
						}
							
						break;
					}
				}
			}

			if (foundOne == false)
			{
				if (ptr.Ttl != 0)
				{
					log.Info("   Adding " + ptr.Name);
					Resources.resourceList.Add(ptr);
				}
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		static public void RemovePtr(Ptr ptr)
		{
			log.Info("RemovePtr called");

			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == ptr.Name &&
					cResource.Type == ptr.Type)
				{
					Ptr cPtr = (Ptr) cResource;
					if(cPtr.Target == ptr.Target)
					{
						log.Info("   Removing " + ptr.Name);
						Resources.resourceList.Remove(cResource);	
						break;
					}
				}
			}
			Resources.resourceMtx.ReleaseMutex();
		}

		static public void AddTextStrings(TextStrings txtStrs)
		{
			log.Info("AddTextStrings called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == txtStrs.Name &&
					cResource.Type == txtStrs.Type)
				{
					foundOne = true;
					break;
				}
			}

			if (foundOne == false)
			{
				log.Info("   Adding " + txtStrs.Name);
				Resources.resourceList.Add(txtStrs);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		internal static int PtrToBuffer(Ptr	ptr, int index,	byte[] buffer)
		{
			// Data Length
			short dataLength = (short) ((short) ptr.Target.Length + (short) 1);
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, buffer, index, 2);
			index += 2;

			int last = 0;
			int current = 0;
			string tmpName = ptr.Target;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					buffer[index++] = (byte) (current - last);
					while( last < current )
					{
						buffer[index++] = (byte) tmpName[last++];
					}
				
					current++;
					last = current;
				}
				else
				{	
					current++;
				}
			}
			
			if (last < current)
			{
				buffer[index++] = (byte) (current - last);
				while( last < current )
				{
					buffer[index++] = (byte) tmpName[last++];
				}
			}

			buffer[index++] = 0;
			return(index);			
		}

		internal static int HostAddressToBuffer(HostAddress	hostAddr, int index, byte[] buffer)
		{
			// Data Length
			short dataLength = (short) 4;
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, buffer, index, 2);
			index += 2;

			IPAddress	prefAddress = hostAddr.PrefAddress;
							
			// IP Address
			Buffer.BlockCopy(
				BitConverter.GetBytes(prefAddress.Address),
				0, 
				buffer, 
				index, 
				4);

			index += 4;
			return(index);
		}

		internal static int ServiceLocationToBuffer(ServiceLocation	sl, int index,	byte[] buffer)
		{
			// Data Length
			short dataLength = (short) ((short) sl.Target.Length + (short) 9);
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, buffer, index, 2);
			index += 2;

			short priority = (short) sl.Priority;
			priority = IPAddress.HostToNetworkOrder(priority);
			Buffer.BlockCopy(BitConverter.GetBytes(priority), 0, buffer, index, 2);
			index += 2;

			short weight = (short) sl.Weight;
			weight = IPAddress.HostToNetworkOrder(weight);
			Buffer.BlockCopy(BitConverter.GetBytes(weight), 0, buffer, index, 2);
			index += 2;

			short port = (short) sl.Port; 
			port = IPAddress.HostToNetworkOrder(port);
			Buffer.BlockCopy(BitConverter.GetBytes(port), 0, buffer, index, 2);
			index += 2;

			int last = 0;
			int current = 0;
			string tmpName = sl.Target;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					buffer[index++] = (byte) (current - last);
					while( last < current )
					{
						buffer[index++] = (byte) tmpName[last++];
					}
						
					current++;
					last = current;
				}
				else
				{	
					current++;
				}
			}

			if (last < current)
			{
				buffer[index++] = (byte) (current - last);
				while( last < current )
				{
					buffer[index++] = (byte) tmpName[last++];
				}
			}

			buffer[index++] = 0;
			return(index);
		}
		
		internal
		static
		int
		TextStringsToBuffer(TextStrings	txtStrs, int index,	byte[] buffer)
		{
			int	dataLengthIndex = index;
			index += 2;
			
			short dataLength = 0;
			foreach(string txtString in txtStrs.GetTextStrings())
			{
				dataLength += (short) ((short) txtString.Length + (short) 1);
				buffer[index++] = (byte) txtString.Length;
				for(int i = 0; i < txtString.Length; i++)
				{
					buffer[index++] = (byte) txtString[i];
				}
			}
			
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, buffer, dataLengthIndex, 2);
			
			return(index);			
		}

		internal
		static
		int
		ResponseHeaderToBuffer(
			BaseResource 	cResource,
			short			transactionID, 
			short			flags,
			short			answers,
			byte[]			buffer)
		{
			int	index = 0;
			
			// Transaction ID
			Buffer.BlockCopy(BitConverter.GetBytes(transactionID), 0, buffer, index, 2);
			index += 2;

			// Flags
			Buffer.BlockCopy(BitConverter.GetBytes(flags), 0, buffer, index, 2);
			index += 2;

			// Questions
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, buffer, index, 2);
			index += 2;

			// Answers
			answers = IPAddress.HostToNetworkOrder(answers);
			Buffer.BlockCopy(BitConverter.GetBytes(answers), 0,	buffer, index, 2);
			index += 2;

			// Authorities
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, buffer, index, 2);
			index += 2;

			// Additional
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, buffer, index, 2);
			index += 2;

			// Host Name
			int	last = 0;
			int	current = 0;
			string	tmpName = cResource.Name;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					buffer[index++] = (byte) (current - last);
					while( last < current )
					{
						buffer[index++] = (byte) tmpName[last++];
					}
						
					current++;
					last = current;
				}
				else
				{	
					current++;
				}
			}

			if (last < current)
			{
				buffer[index++] = (byte) (current - last);
				while( last < current )
				{
					buffer[index++] = (byte) tmpName[last++];
				}
			}

			buffer[index++] = 0;

			// Type
			short hostType = (short) cResource.Type;
			hostType = IPAddress.HostToNetworkOrder(hostType);
			Buffer.BlockCopy(BitConverter.GetBytes(hostType), 0, buffer, index, 2);
			index += 2;

			// Class
			short	hostClass = (short) cResource.Class;
			hostClass = IPAddress.HostToNetworkOrder(hostClass);
			Buffer.BlockCopy(BitConverter.GetBytes(hostClass), 0, buffer, index, 2);
			index += 2;

			// Time To Live
			Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(cResource.Ttl)), 0, buffer, index, 4);
			index += 4;
			
			return(index);
		}

		static bool	mDnsStopping = false;
		static Thread maintenanceThread = null;
		static AutoResetEvent maintenanceEvent = null;
	
		internal static int	StartMaintenanceThread()
		{
			log.Info("StartMaintenanceThread called");

			maintenanceEvent = new AutoResetEvent(false);

			maintenanceThread = new Thread(new ThreadStart(Resources.MaintenanceThread));
			maintenanceThread.IsBackground = true;
			maintenanceThread.Start();
			
			log.Info("StartMaintenanceThread finished");
			return(0);
		}

		internal static int	StopMaintenanceThread()
		{
			log.Info("StopMaintenanceThread called");
			mDnsStopping = true;
			maintenanceEvent.Set();
			Thread.Sleep(0);
			if (maintenanceThread != null)
			{
				maintenanceThread.Abort();
				Thread.Sleep(0);
			}
			
			maintenanceEvent.Close();
			log.Info("StopMaintenanceThread finished");
			return(0);
		}

 		// Used to send out the gratutious answers
		internal static void MaintenanceThread()
		{
			byte[]		answer = new byte[4096];
			int			index;
			ushort		flags = 0x0084;
			string		localLabel = "local";
			string		presenceLabel = "_presence";
			string		tcpLabel = "_tcp";

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

			while(maintenanceEvent.WaitOne((Defaults.maintenanceNapTime), false))
			{
				//Thread.Sleep(Defaults.maintenanceNapTime * 1000);
				log.Info("Maintenance thread awake");
				if (mDnsStopping == true)
				{
					return;
				}
				
				Resources.resourceMtx.WaitOne();
				foreach(BaseResource cResource in Resources.resourceList)
				{
					if (cResource.Owner == true)
					{
						if (cResource.Type == mDnsType.hostAddress)
						{
							HostAddress	hostAddr = (HostAddress) cResource;
					
							index =
								Resources.ResponseHeaderToBuffer(
									cResource,
									(short) 0,
									(short) flags,
									1,
									answer);


							// Data Length
							short dataLength = 4;
							dataLength = IPAddress.HostToNetworkOrder(dataLength);
							Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, answer, index, 2);
							index += 2;

							IPAddress	prefAddress = hostAddr.PrefAddress;
							
							// IP Address
							Buffer.BlockCopy(
								//						BitConverter.GetBytes(IPAddress.HostToNetworkOrder(host.IPV4Address)),
								BitConverter.GetBytes(prefAddress.Address),
								0, 
								answer, 
								index, 
								4);

							index += 4;

							try
							{
								server.Send(answer, index);
							}
							catch(Exception e)
							{
								log.Info("Failed sending Host Address record", e);
								continue;
							}
						}
						else
						if (cResource.Type == mDnsType.serviceLocation)
						{
							ServiceLocation svcLoc = (ServiceLocation) cResource;
					
							index = 0;

							// Send the service location answer
							// Transaction ID
							Buffer.BlockCopy(
								BitConverter.GetBytes(0),
								0,
								answer,
								index,
								2);

							index += 2;

							// Flags
							Buffer.BlockCopy(
								BitConverter.GetBytes(flags), 
								0, 
								answer, 
								index, 
								2);
							index += 2;

							// Questions
							Buffer.BlockCopy(
								BitConverter.GetBytes(0),
								0,
								answer,
								index,
								2);

							index += 2;

							// Answers
							short answers = 1;
							answers = IPAddress.HostToNetworkOrder(answers);
							Buffer.BlockCopy(BitConverter.GetBytes(answers), 0,	answer, index, 2);
							index += 2;

							// Authorities
							Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
							index += 2;

							// Additional
							Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
							index += 2;

							int	last = 0;
							int	current = 0;
							string	tmpName = svcLoc.Name;
							Console.WriteLine("Host Name to convert: " + tmpName);
							while( current < tmpName.Length )
							{
								if (tmpName[current] == '.')
								{
									answer[index++] = (byte) (current - last);
									while( last < current )
									{
										answer[index++] = (byte) tmpName[last++];
									}
						
									current++;
									last = current;
								}
								else
								{	
									current++;
								}
							}
							
							if (last < current)
							{
								answer[index++] = (byte) (current - last);
								while( last < current )
								{
									answer[index++] = (byte) tmpName[last++];
								}
							}

							answer[index++] = 0;

							// Type
							short	hostType = (short) svcLoc.Type;
							hostType = IPAddress.HostToNetworkOrder(hostType);
							Buffer.BlockCopy(BitConverter.GetBytes(hostType), 0, answer, index, 2);
							index += 2;

							// Class
							//Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1)), 0, answer, index, 2);
							short	hostClass = (short) svcLoc.Class;
							hostClass = IPAddress.HostToNetworkOrder(hostClass);
							Buffer.BlockCopy(BitConverter.GetBytes(hostClass), 0, answer, index, 2);
							index += 2;

							// Time To Live
							Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(svcLoc.Ttl)), 0, answer, index, 4);
							index += 4;

							// Data Length
							short dataLength = 8;
							dataLength += (short) svcLoc.Target.Length;
							dataLength += 1;
							dataLength = IPAddress.HostToNetworkOrder(dataLength);
							Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, answer, index, 2);
							index += 2;

							Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(svcLoc.Priority)), 0, answer, index, 2);
							index += 2;

							Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(svcLoc.Weight)), 0, answer, index, 2);
							index += 2;

							short port = (short) svcLoc.Port; 
							port = IPAddress.HostToNetworkOrder(port);
							Buffer.BlockCopy(
								BitConverter.GetBytes(port), 0, answer, index, 2);
							index += 2;

							last = 0;
							current = 0;
							tmpName = svcLoc.Target;
							Console.WriteLine("Target to convert: " + tmpName);
							while( current < tmpName.Length )
							{
								if (tmpName[current] == '.')
								{
									answer[index++] = (byte) (current - last);
									while( last < current )
									{
										answer[index++] = (byte) tmpName[last++];
									}
						
									current++;
									last = current;
								}
								else
								{	
									current++;
								}
							}

							if (last < current)
							{
								answer[index++] = (byte) (current - last);
								while( last < current )
								{
									answer[index++] = (byte) tmpName[last++];
								}
							}

							answer[index++] = 0;

							/*
							lOffset += 6;
							Common.BuildDomainName(receiveData, lOffset, ref lOffset, ref target);
							service.Target = target;
									
							offset += dataLength;
							*/

							/*
							// IP Address
							Buffer.BlockCopy(
								//						BitConverter.GetBytes(IPAddress.HostToNetworkOrder(host.IPV4Address)),
								BitConverter.GetBytes(hostAddr.IPAddress),
								0, 
								answer, 
								index, 
								4);

							index += 4;
							*/

							try
							{
								server.Send(answer, index);
							}
							catch(Exception e)
							{
								log.Info("Failed sending Host Address record", e);
								continue;
							}
						}
						else
						if (cResource.Type == mDnsType.ptr)
						{
							Ptr ptr = (Ptr) cResource;
							log.Info("  Target:    " + ptr.Target);
							
							index =
								Resources.ResponseHeaderToBuffer(
									cResource,
									(short) 0,
									(short) flags,
									1,
									answer);
									
							index = PtrToBuffer(ptr, index, answer);
									
							try
							{
								server.Send(answer, index);
							}
							catch(Exception e)
							{
								log.Info("Failed sending Host Address record", e);
								continue;
							}
						}
						else
						if (cResource.Type == mDnsType.textStrings)
						{
							TextStrings txt = (TextStrings) cResource;
							
							index =
								Resources.ResponseHeaderToBuffer(
									cResource,
									(short) 0,
									(short) flags,
									1,
									answer);
									
							index = Resources.TextStringsToBuffer(txt, index, answer);		
							
							try
							{
								server.Send(answer, index);
							}
							catch(Exception e)
							{
								log.Info("Failed sending Host Address record", e);
								continue;
							}
							
						}
						log.Info("");
					}
				}
				Resources.resourceMtx.ReleaseMutex();
			}
		}
	}

	/// <summary>
	/// Summary description for Base Resource
	/// </summary>
	class BaseResource
	{
		#region Class Members
		DateTime			update;
		protected bool		owner = false;
		protected string	name = null;
		protected int		ttl = 0;
		protected mDnsType	dnsType;
		protected mDnsClass dnsClass;
		#endregion

		#region Properties
		public string Name
		{
			get
			{
				try
				{
					return(this.name);
				}
				catch{}
				return("");
			}
		}

		// True if this record's Time-To-Live
		// has expired
		public bool Expired
		{
			get
			{
				return(false);
			}
		}

		// Are we authoritative for this record?
		public bool Owner
		{
			get
			{
				return(owner);
			}
		}

		// Returns the current ttl
		public int Ttl
		{
			get
			{
				return(this.ttl);
			}
		}
		
		public mDnsType Type
		{
			get
			{
				return(this.dnsType);
			}
		}
		
		public mDnsClass Class
		{
			get
			{
				return(this.dnsClass);
			}
		}

		#endregion

		#region Constructors

		public BaseResource(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner)
		{
			this.name = name;
			this.ttl = ttl;
			this.dnsType = dnsType;
			this.dnsClass = dnsClass;
			this.owner = owner;

			this.update = DateTime.Now;
		}
		#endregion

		#region Private Methods
		internal void	Update()
		{
			update = DateTime.Now;
		}
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}

	/// <summary>
	/// Summary description for Host Resource
	/// </summary>
	[Serializable]
	class HostAddress : BaseResource
	{
		#region Class Members
		protected ArrayList	ipAddresses = null;
		#endregion

		#region Properties

		// Returns the preferred address
		public IPAddress PrefAddress
		{
			get
			{
				return((IPAddress) this.ipAddresses[0]);
			}
			
			set
			{
				this.ipAddresses.Remove((IPAddress) value);
				this.ipAddresses.Insert(0, (IPAddress) value);
				this.Update();
			}
		}

		#endregion

		#region Constructors
		public HostAddress(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.ipAddresses = new ArrayList();
		}
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		public void	AddIPAddress(IPAddress ipAddress)
		{
			if (this.ipAddresses.Contains(ipAddress) == false)
			{
				this.ipAddresses.Add(ipAddress);
				this.Update();
			}
		}

		public ArrayList GetIPAddresses()
		{
			ArrayList returnedList = new ArrayList(this.ipAddresses.Count + 1);
			foreach(IPAddress addr in this.ipAddresses)
			{
				returnedList.Add(addr);
			}

			return(returnedList);
		}


		public void	RemoveIPAddress(IPAddress ipAddress)
		{
			this.ipAddresses.Remove(ipAddress);
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Service Resource
	/// </summary>
	class ServiceLocation : BaseResource
	{
		#region Class Members

		/*
		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);
		*/

		ArrayList	nameValues;
		int			port;
		int			priority;
		int			weight;
		string		target = "";
		#endregion

		#region Properties

		public int	Port
		{
			get
			{
				return(this.port);
			}

			set
			{
				this.port = value;
				this.Update();
			}
		}

		public int	Priority
		{
			get
			{
				return(this.priority);
			}

			set
			{
				this.priority = value;
				this.Update();
			}
		}

		public int	Weight
		{
			get
			{
				return(this.weight);
			}

			set
			{
				this.weight = value;
				this.Update();
			}
		}
		
		public string	Target
		{
			get
			{
				return(this.target);
			}
			
			set
			{
				this.target = value;
				this.Update();
			}
		}
		#endregion

		#region Constructors
		/*
		public ServiceLocation() : base()
		{
			this.nameValues = new ArrayList();
		}
		*/

		public ServiceLocation(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.nameValues = new ArrayList();
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods

		public bool	AddNameValue(string name, string nameValue)
		{
			string	full = name;

			if (nameValue != null)
			{
				full += "=" + nameValue;
			}

			this.nameValues.Add(full);
			return(true);
		}

		public bool RemoveNameValue(string name)
		{
			bool	removed = false;
			string	tmpString;

			IEnumerator		enumNames = this.nameValues.GetEnumerator();
			while(enumNames.MoveNext())
			{
				tmpString = (string) enumNames.Current;
				string[] nameS = tmpString.Split('=');
				if(nameS[0] == name)
				{
					this.nameValues.Remove(tmpString);
					removed = true;
					break;
				}
			}

			return(removed);
		}

		public ArrayList GetNameValues()
		{
			return(this.nameValues);
		}
		#endregion
	}

	/// <summary>
	/// Summary description for Ptr
	/// </summary>
	class Ptr : BaseResource
	{
		#region Class Members

		string		target;
		#endregion

		#region Properties

		public string	Target
		{
			get
			{
				return(this.target);
			}
			
			set
			{
				this.target = value;
				this.Update();
			}
		}
		#endregion

		#region Constructors

		public Ptr(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.target = "";			
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods

		#endregion
	}

	/// <summary>
	/// Summary description for TextString
	/// </summary>
	class TextStrings : BaseResource
	{
		#region Class Members
		ArrayList	stringList = null;
		#endregion

		#region Properties
		#endregion

		#region Constructors

		public TextStrings(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.stringList = new ArrayList();
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		public bool	AddTextString(string txtString)
		{
			this.stringList.Add(txtString);
			return(true);
		}

		public bool RemoveTextString(string txtString)
		{
			this.stringList.Remove(txtString);
			return(true);
		}
		
		public bool RemoveAllNameValues()
		{
			this.stringList.Clear();
			return(true);
		}

		public ArrayList GetTextStrings()
		{
			return(this.stringList);
		}
		#endregion
	}
}
