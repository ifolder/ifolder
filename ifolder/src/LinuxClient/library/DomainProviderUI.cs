/***********************************************************************
 *  $RCSfile: DomainProviderUI.cs,v $
 *
 *  Copyright (C) 2005 Novell, Inc.
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
using System.IO;
using System.Reflection;

using Gtk;

using Simias.Client;

using Novell.iFolder;
 
namespace Novell.iFolder.DomainProvider
{
	public class DomainProviderUI
	{
		static private DomainProviderUI instance = null;
		
		private Hashtable registeredProviders = new Hashtable();

//		private const string DllFiles = "*DomainProviderUI*.dll";
		private const string DllFiles = "*.dll";
		
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
		
		static public DomainProviderUI GetDomainProviderUI()
		{
			lock(typeof(DomainProviderUI))
			{
				if (instance == null)
					instance = new DomainProviderUI();
			
				return instance;
			}
		}
		
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
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
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