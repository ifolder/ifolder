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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;

using Simias;
using Simias.Storage;

namespace Simias.Location
{
	/// <summary>
	/// Location Providers
	/// </summary>
	public class LocationProviderList : IEnumerable, IEnumerator
	{
		/// <summary>
		/// Location service node name
		/// </summary>
		public static readonly string LocationServiceNodeName = "Location Service";
		
		/// <summary>
		/// Location provider list property name
		/// </summary>
		public static readonly string LocationProviderListPropertyName = "Location Providers";
		
		private Store store;
		private Collection collection;
		private Node node;
		private MultiValuedList list;
		private IEnumerator enumerator;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration">The Simias configuration object.</param>
		public LocationProviderList(Configuration configuration)
		{
			store = Store.GetStore();

			collection = store.GetDatabaseObject();

			// check for node
			node = collection.GetSingleNodeByName(LocationServiceNodeName);

			if (node == null)
			{
				// create default node
				node = new Node(LocationServiceNodeName);
				collection.Commit(node);

				Add(typeof(DefaultLocationProvider));
				Add(typeof(mDnsLocationProvider));
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collection">The configuration collection.</param>
		private LocationProviderList(Collection collection)
		{
			this.store = collection.StoreReference;
			this.collection = collection;
		}

		/// <summary>
		/// Add a location provider to the top of the provider list.
		/// </summary>
		/// <param name="type">The location provider class type.</param>
		public void Add(Type type)
		{
			ArrayList current = new ArrayList();

			node = collection.GetSingleNodeByName(LocationServiceNodeName);
			list = node.Properties.GetProperties(LocationProviderListPropertyName);
			
			foreach(Property p in list)
			{
				current.Add(p.Value);
			}
			
            node.Properties.ModifyProperty(LocationProviderListPropertyName, type.AssemblyQualifiedName);
			
			foreach(string item in current)
			{
				node.Properties.AddProperty(LocationProviderListPropertyName, item);
			}

			collection.Commit(node);
		}

		#region IEnumerable Members

		/// <summary>
		/// Get the location provider enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return new LocationProviderList(collection);
		}

		#endregion

		#region IEnumerator Members

		/// <summary>
		/// Reset the location provider enumeration.
		/// </summary>
		public void Reset()
		{
			node = collection.GetSingleNodeByName(LocationServiceNodeName);

			list = node.Properties.GetProperties(LocationProviderListPropertyName);

			enumerator = list.GetEnumerator();
		}

		/// <summary>
		/// The current location provider.
		/// </summary>
		public object Current
		{
			get { return Type.GetType(((enumerator.Current as Property).Value) as String); }
		}

		/// <summary>
		/// Move to the next location provider.
		/// </summary>
		/// <returns>true if the enumerator successfully moved to the next location provider.</returns>
		public bool MoveNext()
		{
			if (enumerator == null)
			{
				Reset();
			}

			return enumerator.MoveNext();
		}

		#endregion
	}
}
