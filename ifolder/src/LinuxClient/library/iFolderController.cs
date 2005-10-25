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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;

using Novell.iFolder.Events;

using Simias.Client;

namespace Novell.iFolder.Controller
{
	public class iFolderController
	{
		/// <summary>
		/// Member that ensures single instance
		/// </summary>
		private static iFolderController instance = null;

		/// <summary>
		/// Member that provides acces to the iFolder Web Service
		/// </summary>
		private iFolderWebService ifws = null;


		/// <summary>
		/// Member that provides acces to the Simias Web Service
		/// </summary>
		private SimiasWebService simws = null;

		/// <summary>
		/// Hashtable to hold the ifolders
		/// </summary>
		private Hashtable keyediFolders = null;

		/// <summary>
		/// Hashtable to hold the subscription to ifolder map
		/// </summary>
		private Hashtable keyedSubscriptions = null;

		/// <summary>
		/// Member to provide access to domain events and data
		/// </summary>
		private DomainController domainController = null;
		
		///
		/// Events
		///
		public event iFolderAddedEventHandler iFolderAdded;
		public event iFolderDeletedEventHandler iFolderDeleted;
		public event iFolderChangedEventHandler iFolderChanged;
		public event iFolderInvitationReceivedEventHandler iFolderInvitationReceived;
		
		private iFolderController(Manager simiasManager)
		{
			string localServiceUrl = simiasManager.WebServiceUri.ToString();
			try
			{
				ifws = new iFolderWebService();
				ifws.Url = localServiceUrl + "/iFolder.asmx";
				LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				ifws = null;
				throw new Exception("Unable to create ifolder web service in iFolderController");
			}
			try
			{
				simws = new SimiasWebService();
				simws.Url = localServiceUrl + "/Simias.asmx";
				LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service in iFolderController");
			}

			keyediFolders = new Hashtable();
			keyedSubscriptions = new Hashtable();

			domainController = DomainController.GetDomainController(simiasManager);
			if (domainController != null)
			{
				domainController.DomainLoggedIn +=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut +=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
			}
		}
		
		public static iFolderController GetiFolderController(Manager simiasManager)
		{
			lock (typeof(iFolderController))
			{
				if (instance == null)
				{
					instance = new iFolderController(simiasManager);
				}
			
				return instance;
			}
		}
		
		private void OnDomainLoggedInEvent(object sender, DomainEventArgs args)
		{
Console.WriteLine("iFolderController.OnDomainLoggedInEvent() entered");
		}

		private void OnDomainLoggedOutEvent(object sender, DomainEventArgs args)
		{
Console.WriteLine("iFolderController.OnDomainLoggedOutEvent() entered");
		}
	}
}