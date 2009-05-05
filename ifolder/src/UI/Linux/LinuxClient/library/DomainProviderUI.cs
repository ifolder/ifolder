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
using System.IO;
using System.Reflection;

using Gtk;

using Simias.Client;

using Novell.iFolder;
 
namespace Novell.iFolder.DomainProvider
{
    /// <summary>
    /// class DomainProviderUI
    /// </summary>
	public class DomainProviderUI
	{
		static private DomainProviderUI instance = null;
		
		private Hashtable registeredProviders = new Hashtable();

//		private const string DllFiles = "*DomainProviderUI*.dll";
		private const string DllFiles = "*.dll";
		
        /// <summary>
        /// Gets the count of registered providers
        /// </summary>
		public int Count
		{
			get
			{
				return registeredProviders.Count;
			}
		}
		
        public IDomainProviderUI[] Providers
		{
			get
			{
				lock(typeof(DomainProviderUI))
				{
					IDomainProviderUI[] providers =
						new IDomainProviderUI[registeredProviders.Count];
					
					ICollection icol = registeredProviders.Values;
					icol.CopyTo(providers, 0);
				
					return providers;
				}
			}
		}
		
        /// <summary>
        /// Get Domain Provider UI
        /// </summary>
        /// <returns>DomainProviderUI</returns>
		static public DomainProviderUI GetDomainProviderUI()
		{
			lock(typeof(DomainProviderUI))
			{
				if (instance == null)
					instance = new DomainProviderUI();
			
				return instance;
			}
		}
		
        /// <summary>
        /// GetProvider For ID
        /// </summary>
        /// <param name="domainID">Domain ID</param>
        /// <returns>IDomainProviderUI</returns>
		public IDomainProviderUI GetProviderForID(string domainID)
		{
			lock(typeof(DomainProviderUI))
			{
				IDomainProviderUI provider = null;
				
				if (registeredProviders.Contains(domainID))
					provider = (IDomainProviderUI)registeredProviders[domainID];
				
				return provider;
			}
		}
		
//		public Dialog CreatePropertiesDialog(string domainID, Window parent)
//		{
//			Dialog dialog = null;

//			IDomainProviderUI provider = GetDomainProvider(domainID);
//			if (provider != null)
//			{
//				dialog = provider.CreatePropertiesDialog(parent);
//			}
			
//			return dialog;
//		}
		
        /// <summary>
        /// Constructor
        /// </summary>
		private DomainProviderUI()
		{
			lock(this)
			{
				registeredProviders = new Hashtable();
				
				LoadProviders();
			}
		}

		/// <summary>
		/// Search $prefix/lib/*.dll for implementors of IDomainProviderUI and
		/// add them to the registeredProviders Hashtable.
		///	</summary>
		private void LoadProviders()
		{
			string[] libFileList = null;

			if (Directory.Exists(Util.PluginsPath))
				libFileList = Directory.GetFiles(Util.PluginsPath, DllFiles);
			
			if (libFileList == null || libFileList.Length == 0) return;
			
			foreach(string libFile in libFileList)
			{
				ArrayList providers = LoadProviders(libFile);
				if (providers != null)
				{
					foreach(IDomainProviderUI provider in providers)
					{
						registeredProviders[provider.ID] = provider;
					}
				}
			}
		}
		
		/// <summary>
		/// Attempt to load the specified library and look for all
		/// classes that implement IDomainProviderUI.  Validate each
		/// IDomainProviderUI using reflection.
		/// </summary>
		private ArrayList LoadProviders(string libFile)
		{
			lock(typeof(DomainProviderUI))
			{
				ArrayList providers = new ArrayList();

				try
				{
					Assembly assembly = Assembly.LoadFrom(libFile);
					if (assembly != null)
					{
						foreach(Type type in assembly.GetTypes())
						{
							IDomainProviderUI potentialProvider = null;
							try
							{
								potentialProvider =
									(IDomainProviderUI)assembly.CreateInstance(type.FullName);
							}
							catch{}
							
							if (potentialProvider != null &&
								IsProviderValid(potentialProvider))
								providers.Add((IDomainProviderUI)potentialProvider);
						}
						
					}
				}
				catch(Exception e)
				{
					Debug.PrintLine(e.Message);
					Debug.PrintLine(e.StackTrace);
				}

				return providers;
			}
		}
		
		/// <summary>
		/// Make sure that all the expected properties and methods exist
		/// </summary>
		private bool IsProviderValid(IDomainProviderUI potentialProvider)
		{
			// FIXME: Implement DomainProviderUI.IsProviderValid
			return true;
		}
		
//		private IDomainProviderUI GetDomainProvider(string domainID)
//		{
//			IDomainProviderUI provider = null;
			
//			lock(typeof(DomainProviderUI))
//			{
//				// See if there is a provider mapping for this domain
//				if (domainProviderTable.ContainsKey(domainID))
//					provider = (IDomainProviderUI)registeredProviders[domainID];
//			}
			
//			return provider;
//		}
	}
} 
