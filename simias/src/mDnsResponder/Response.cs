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
// This source file is responsible for building and sending
// multi-cast DNS responses.
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

using log4net;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for DnsRequest
	/// </summary>
	class mDnsResponse
	{
		private static readonly log4net.ILog log = 
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region Class Members

		private			byte[]		buffer;
		private			int			index;
		private			ushort		flags = 0x0084;
		private			int			transactionID;
		private			ArrayList	questionList;
		private			ArrayList	answerList;
		private			ArrayList	authorityList;
		private			ArrayList	additionalList;
		private			UdpClient	mDnsClient;
		#endregion

		#region Properties

		/// <summary>
		/// !NOTE! Doc incomplete
		/// </summary>
		public int TransactionID
		{
			get
			{
				return(this.transactionID);
			}

			set
			{
				this.transactionID = value;
			}
		}
		
		public int	Answers
		{
			get
			{
				try
				{
					if (this.answerList != null)
					{
						return(this.answerList.Count);
					}
				}
				catch{}
				return(0);
			}
		}

		public int	Authorities
		{
			get
			{
				try
				{
					if (this.authorityList != null)
					{
						return(this.authorityList.Count);
					}
				}
				catch{}
				return(0);
			}
		}

		public int	Additionals
		{
			get
			{
				try
				{
					if (this.additionalList != null)
					{
						return(this.additionalList.Count);
					}
				}
				catch{}
				return(0);
			}
		}

		public int Flags
		{
			get
			{
				return(this.flags);
			}
		}
		#endregion

		#region Constructors
		public mDnsResponse()
		{
			this.FinalConstruction();
		}

		public mDnsResponse(UdpClient cUdpClient)
		{
			this.FinalConstruction();
			this.mDnsClient = cUdpClient;
		}

		#endregion

		#region Private Methods
		internal void	FinalConstruction()
		{
			this.questionList = null;
			this.answerList = null;
			this.authorityList = null;
			this.additionalList = null;
			this.buffer = null;
			this.index = 0;
		}

		internal int BaseResourceToBuffer(BaseResource cResource)
		{
			// Host Name
			int	last = 0;
			int	current = 0;
			string	tmpName = cResource.Name;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					this.buffer[this.index++] = (byte) (current - last);
					while( last < current )
					{
						this.buffer[this.index++] = (byte) tmpName[last++];
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
				this.buffer[this.index++] = (byte) (current - last);
				while( last < current )
				{
					this.buffer[this.index++] = (byte) tmpName[last++];
				}
			}

			this.buffer[this.index++] = 0;

			// Type
			short hostType = (short) cResource.Type;
			hostType = IPAddress.HostToNetworkOrder(hostType);
			Buffer.BlockCopy(BitConverter.GetBytes(hostType), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Class
			/*
			short	hostClass = (short) cResource.Class;
			hostClass = IPAddress.HostToNetworkOrder(hostClass);
			*/

			int	hostClass = 0x00000180;
			Buffer.BlockCopy(BitConverter.GetBytes(hostClass), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Time To Live
			Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(cResource.Ttl)), 0, this.buffer, this.index, 4);
			this.index += 4;
			return(0);
		}

		internal int HostAddressToBuffer(HostAddress hostAddr)
		{
			// Data Length
			short dataLength = (short) 4;
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, this.buffer, this.index, 2);
			this.index += 2;

			IPAddress	prefAddress = hostAddr.PrefAddress;
							
			// IP Address
			Buffer.BlockCopy(
				BitConverter.GetBytes(hostAddr.PrefAddress.Address),
				0, 
				this.buffer, 
				this.index, 
				4);

			this.index += 4;
			return(0);
		}

		internal int PtrToBuffer(Ptr ptr)
		{
			int		last = 0;
			int		current = 0;

			// Data Length
			// Add one for the beginning component length and one for the zero terminator
			short dataLength = (short) ((short) ptr.Target.Length + (short) 2);
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, this.buffer, this.index, 2);
			this.index += 2;

			string	tmpName = ptr.Target;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					this.buffer[this.index++] = (byte) (current - last);
					while( last < current )
					{
						this.buffer[this.index++] = (byte) tmpName[last++];
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
				this.buffer[this.index++] = (byte) (current - last);
				while( last < current )
				{
					this.buffer[this.index++] = (byte) tmpName[last++];
				}
			}

			this.buffer[this.index++] = 0;
			return(0);			
		}

		internal int ResourceToBuffer(BaseResource cResource)
		{
			this.BaseResourceToBuffer(cResource);
			if (cResource.Type == mDnsType.hostAddress)
			{
				HostAddressToBuffer((HostAddress) cResource);
			}
			else
			if (cResource.Type == mDnsType.ptr)
			{
				PtrToBuffer((Ptr) cResource);
			}
			else
			if (cResource.Type == mDnsType.serviceLocation)
			{
				ServiceLocationToBuffer((ServiceLocation) cResource);
			}
			else
			if (cResource.Type == mDnsType.textStrings)
			{
				TextStringsToBuffer((TextStrings) cResource);
			}
			return(0);
		}

		internal int ResponseHeaderToBuffer()
		{
			short tmpValue;

			// Transaction ID
			Buffer.BlockCopy(BitConverter.GetBytes(this.transactionID), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Flags
			Buffer.BlockCopy(BitConverter.GetBytes(this.flags), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Questions
			// FIXME - for now I don't think questions will always be zero for a response
			Buffer.BlockCopy(BitConverter.GetBytes(0), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Answers
			if (this.answerList != null)
			{
				tmpValue = IPAddress.HostToNetworkOrder((short) this.answerList.Count);
			}
			else
			{
				tmpValue = 0;
			}
			Buffer.BlockCopy(BitConverter.GetBytes(tmpValue), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Authorities
			if (this.authorityList != null)
			{
				tmpValue = IPAddress.HostToNetworkOrder((short) this.authorityList.Count);
			}
			else
			{
				tmpValue = 0;
			}
			Buffer.BlockCopy(BitConverter.GetBytes(tmpValue), 0, this.buffer, this.index, 2);
			this.index += 2;

			// Additional
			if (this.additionalList != null)
			{
				tmpValue = IPAddress.HostToNetworkOrder((short) this.additionalList.Count);
			}
			else
			{
				tmpValue = 0;
			}
			Buffer.BlockCopy(BitConverter.GetBytes(tmpValue), 0, this.buffer, this.index, 2);
			this.index += 2;

			return(0);
		}

		internal int ServiceLocationToBuffer(ServiceLocation sl)
		{
			// Data Length
			short dataLength = (short) ((short) sl.Target.Length + (short) 8);
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, this.buffer, this.index, 2);
			this.index += 2;

			short priority = (short) sl.Priority;
			priority = IPAddress.HostToNetworkOrder(priority);
			Buffer.BlockCopy(BitConverter.GetBytes(priority), 0, this.buffer, this.index, 2);
			this.index += 2;

			short weight = (short) sl.Weight;
			weight = IPAddress.HostToNetworkOrder(weight);
			Buffer.BlockCopy(BitConverter.GetBytes(weight), 0, this.buffer, this.index, 2);
			this.index += 2;

			//short port = (short) sl.Port; 
			short port = IPAddress.HostToNetworkOrder((short) sl.Port);
			Buffer.BlockCopy(BitConverter.GetBytes(port), 0, this.buffer, this.index, 2);
			this.index += 2;

			int last = 0;
			int current = 0;
			string tmpName = sl.Target;
			while( current < tmpName.Length )
			{
				if (tmpName[current] == '.')
				{
					this.buffer[this.index++] = (byte) (current - last);
					while( last < current )
					{
						this.buffer[this.index++] = (byte) tmpName[last++];
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
				this.buffer[this.index++] = (byte) (current - last);
				while( last < current )
				{
					this.buffer[this.index++] = (byte) tmpName[last++];
				}
			}

			this.buffer[this.index++] = 0;
			return(0);
		}

		internal int TextStringsToBuffer(TextStrings txtStrs)
		{
			int	dataLengthIndex = this.index;
			this.index += 2;
			
			short dataLength = 0;
			foreach(string txtString in txtStrs.GetTextStrings())
			{
				dataLength += (short) ((short) txtString.Length + (short) 1);
				this.buffer[this.index++] = (byte) txtString.Length;
				for(int i = 0; i < txtString.Length; i++)
				{
					this.buffer[this.index++] = (byte) txtString[i];
				}
			}
			
			dataLength = IPAddress.HostToNetworkOrder(dataLength);
			Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, this.buffer, dataLengthIndex, 2);
			
			return(0);			
		}
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods

		public	void	AddAdditional(BaseResource cResource)
		{
			if (this.additionalList == null)
			{
				this.additionalList = new ArrayList();
			}

			this.additionalList.Add(cResource);
		}

		public	void	AddAnswer(BaseResource	cResource)
		{
			if (this.answerList == null)
			{
				this.answerList = new ArrayList();
			}

			this.answerList.Add(cResource);
		}

		public	void	AddAuthority(BaseResource cResource)
		{
			if (this.authorityList == null)
			{
				this.authorityList = new ArrayList();
			}

			this.authorityList.Add(cResource);
		}

		public void ClearAnswers()
		{
			if (this.answerList != null)
			{
				this.answerList.Clear();
			}
		}

		public void ClearAdditionals()
		{
			if (this.additionalList != null)
			{
				this.additionalList.Clear();
			}
		}

		public void ClearAuthorities()
		{
			if (this.authorityList != null)
			{
				this.authorityList.Clear();
			}
		}

		public	void Send()
		{
			// Make sure we have something to send
			if (this.answerList == null &&
				this.authorityList == null &&
				this.additionalList == null)
			{
				return;
			}

			if (this.mDnsClient == null)
			{
				return;
			}

			this.index = 0;
			if (this.buffer == null)
			{
				this.buffer = new byte[Defaults.sendBufferSize];
			}

			this.ResponseHeaderToBuffer();

			if (this.answerList != null)
			{
				foreach(BaseResource cResource in this.answerList)
				{
					this.ResourceToBuffer(cResource);
				}
			}

			if (this.authorityList != null)
			{
				foreach(BaseResource cResource in this.authorityList)
				{
					this.ResourceToBuffer(cResource);
				}
			}

			if (this.additionalList != null)
			{
				foreach(BaseResource cResource in this.additionalList)
				{
					this.ResourceToBuffer(cResource);
				}
			}

			try
			{
				this.mDnsClient.Send(this.buffer, this.index);
			}
			catch(Exception e)
			{
				log.Debug("Failed sending a mDnsResponse", e);
			}
		}

		#endregion
	}
}
