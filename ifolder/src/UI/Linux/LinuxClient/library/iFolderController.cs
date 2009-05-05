/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Boyd Timothy <btimothy@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

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
		
        /// <summary>
        /// Constructor
        /// </summary>
		private iFolderController()
		{
			string localServiceUrl = Simias.Client.Manager.LocalServiceUrl.ToString();
			try
			{
				ifws = new iFolderWebService();
				ifws.Url = localServiceUrl + "/iFolder.asmx";
				LocalService.Start(ifws);
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
				LocalService.Start(simws);
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service in iFolderController");
			}

			keyediFolders = new Hashtable();
			keyedSubscriptions = new Hashtable();

			domainController = DomainController.GetDomainController();
			if (domainController != null)
			{
				domainController.DomainLoggedIn +=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut +=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
			}
		}
		
        /// <summary>
        /// Get iFolder Controller
        /// </summary>
        /// <returns>iFolder Controller</returns>
		public static iFolderController GetiFolderController()
		{
			lock (typeof(iFolderController))
			{
				if (instance == null)
				{
					instance = new iFolderController();
				}
			
				return instance;
			}
		}
		
		private void OnDomainLoggedInEvent(object sender, DomainEventArgs args)
		{
//Debug.PrintLine("iFolderController.OnDomainLoggedInEvent() entered");
		}

		private void OnDomainLoggedOutEvent(object sender, DomainEventArgs args)
		{
//Debug.PrintLine("iFolderController.OnDomainLoggedOutEvent() entered");
		}
	}
}
