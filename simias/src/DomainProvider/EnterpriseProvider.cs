/***********************************************************************
 *  $RCSfile$
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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Web;

using Simias.Authentication;
using Simias.Client;
using Simias.Service;
using Simias.Storage;

namespace Simias
{
	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	internal class SearchState : IDisposable
	{
		#region Class Members
		/// <summary>
		/// The default timeout for the search context.
		/// </summary>
		static private int defaultTimeout = 60 * 1000;

		/// <summary>
		/// Table used to keep track of outstanding search entries.
		/// </summary>
		static private Hashtable searchTable = new Hashtable();

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle used to store and recall this context object.
		/// </summary>
		private string contextHandle = Guid.NewGuid().ToString();

		/// <summary>
		/// Identifier for the domain that is being searched.
		/// </summary>
		private string domainID;

		/// <summary>
		/// Object used to iteratively return the members from the domain.
		/// </summary>
		private ICSEnumerator enumerator;

		/// <summary>
		/// Timer that will be used to clean up the entry if it is orphaned.
		/// </summary>
		private Timer timer;

		/// <summary>
		/// Time out value for outstanding searches.
		/// </summary>
		private int searchTimeout;
		#endregion

		#region Properties
		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Gets the context handle for this object.
		/// </summary>
		public string ContextHandle
		{
			get { return contextHandle; }
		}

		/// <summary>
		/// Gets the domain ID for the domain that is being searched.
		/// </summary>
		public string DomainID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Gets the search iterator.
		/// </summary>
		public ICSEnumerator Enumerator
		{
			get { return enumerator; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		public SearchState( string domainID, ICSEnumerator enumerator ) :
			this( domainID, enumerator, SearchState.defaultTimeout )
		{
		}

		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		/// <param name="searchTimeout">Amount of time in milliseconds that the search context remains valid.</param>
		public SearchState( string domainID, ICSEnumerator enumerator, int searchTimeout )
		{
			this.domainID = domainID;
			this.enumerator = enumerator;
			this.searchTimeout = searchTimeout;
			this.timer = new Timer( new TimerCallback( SearchOrphaned ), this, searchTimeout, Timeout.Infinite );

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Resets the timer to the the specified timeout interval.
		/// </summary>
		private void Reset()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			timer.Change( searchTimeout, Timeout.Infinite );
		}

		/// <summary>
		/// Timer routine used to clean up orphaned searches.
		/// </summary>
		/// <param name="timerState">State that indicates which context has timed out.</param>
		private void SearchOrphaned( Object timerState )
		{
			SearchState searchState = timerState as SearchState;
			searchState.Dispose();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a search context object that contains the state information for an outstanding search.
		/// </summary>
		/// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
		/// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
		static public SearchState GetSearchState( string contextHandle )
		{
			SearchState searchState = null;

			lock ( searchTable )
			{
				searchState = searchTable[ contextHandle ] as SearchState;
			}

			if ( searchState != null )
			{
				// Reset the timer.
				searchState.Reset();
			}

			return searchState;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			lock ( searchTable )
			{
				// Remove the search context from the table and dispose it.
				searchTable.Remove( contextHandle );
			}

			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					enumerator.Dispose();
					timer.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~SearchState()      
		{
			Dispose( false );
		}
		#endregion
	}

	/// <summary>
	/// Implementation of the IDomainProvider Service for Simias Enterprise
	/// Server.
	/// </summary>
	public class EnterpriseProvider : IDomainProvider, IThreadService
	{
		#region Class Members
		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Enterprise Domain Provider";
		static private string providerDescription = "Domain Provider for Simias Enterprise";

		/// <summary>
		/// Store object.
		/// </summary>
		static private Store store = Store.GetStore();
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the host address property from the domain object.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object containing the host address for the
		/// domain if successful. Otherwise returns a null.</returns>
		private Uri GetDomainHostAddress( string domainID )
		{
			Uri hostAddress = null;

			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				Property p = domain.Properties.FindSingleValue( PropertyTags.HostAddress );
				if ( p != null )
				{
					hostAddress = p.Value as Uri;
				}
			}

			return hostAddress;
		}
		#endregion

		#region IDomainProvider Members

		/// <summary>
		/// Gets the name of the domain provider.
		/// </summary>
		public string Name
		{
			get { return providerName; }
		}

		/// <summary>
		/// Gets the description of the domain provider.
		/// </summary>
		public string Description
		{
			get { return providerDescription; }
		}

		/// <summary>
		/// Performs authentication to the specified domain.
		/// </summary>
		/// <param name="domain">Domain to authenticate to.</param>
		/// <param name="httpContext">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		public Authentication.Status Authenticate( Domain domain, HttpContext httpContext )
		{
			return new Authentication.Status( Authentication.StatusCodes.Success );
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		/// <summary>
		public void DeleteLocation( string domainID, string collectionID )
		{
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( Object searchContext )
		{
			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext as String );
			if ( searchState != null )
			{
				searchState.Dispose();
			}
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, out Object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;

			// Start the search for all members of the domain.
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				SearchState searchState = new SearchState( domainID, domain.GetMemberList().GetEnumerator() as ICSEnumerator );
				searchContext = searchState.ContextHandle;
				moreEntries = FindNextDomainMembers( ref searchContext, out memberList, count );
			}

			return moreEntries;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Name of attribute to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, out Object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;

			// Start the search for the specific members of the domain.
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				ICSList list = domain.Search( attributeName, searchString, operation );
				SearchState searchState = new SearchState( domainID, list.GetEnumerator() as ICSEnumerator );
				searchContext = searchState.ContextHandle;
				moreEntries = FindNextDomainMembers( ref searchContext, out memberList, count );
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for all domain members started by calling the FindFirstDomainMembers method.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref Object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext as String );
			if ( searchState != null )
			{
				// Get the domain being searched.
				Domain domain = store.GetDomain( searchState.DomainID );
				if ( domain != null )
				{
					// Allocate a list to hold the member objects.
					ArrayList tempList = new ArrayList( count );
					ICSEnumerator enumerator = searchState.Enumerator;
					while( ( count > 0 ) && enumerator.MoveNext() )
					{
						// The enumeration returns ShallowNode objects.
						ShallowNode sn = enumerator.Current as ShallowNode;
						if ( sn.Type == NodeTypes.MemberType )
						{
							tempList.Add( new Member( domain, sn ) );
							--count;
						}
					}

					if ( tempList.Count > 0 )
					{
						memberList = tempList.ToArray( typeof ( Member ) ) as Member[];
						moreEntries = ( count == 0 ) ? true : false;
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		public bool OwnsDomain( string domainID )
		{
			Domain domain = store.GetDomain( domainID );
			return ( ( domain != null ) && domain.IsType( domain, "Enterprise" ) ) ? true : false;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID )
		{
			return OwnsDomain( domainID ) ? GetDomainHostAddress( domainID ) : null;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that the
		/// collection belongs to.</param>
		/// <param name="collectionID">Identifier of the collection to
		/// find the network location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string collectionID )
		{
			return OwnsDomain( domainID ) ? GetDomainHostAddress( domainID ) : null;
		}

		/// <summary>
		/// Returns the network location of where to create a collection.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will own the 
		/// collection.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being created.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string userID, string collectionID )
		{
			return OwnsDomain( domainID ) ? GetDomainHostAddress( domainID ) : null;
		}

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			return OwnsDomain( domainID ) ? GetDomainHostAddress( domainID ) : null;
		}
		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">Configuration file object that indicates which Collection Store to use.</param>
		public void Start( Configuration conf )
		{
			// Register with the domain provider service.
			DomainProvider.RegisterProvider( this );
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			// Unregister with the domain provider service.
			DomainProvider.Unregister( this );
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom( int message, string data )
		{
		}

		#endregion
	}
}
