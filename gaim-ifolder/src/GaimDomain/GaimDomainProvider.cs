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
 *  Author: Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

// shorty
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.Gaim
{
	/// <summary>
	/// Class to support Simias' IDomainProvider interface
	/// </summary>
	public class GaimDomainProvider : IDomainProvider
	{
		#region Class Members
		private string providerName = "Gaim Domain Provider";
		private string description = "Simias Domain Provider for the Gaim Workgroup Domain";
		private Hashtable searchContexts = new Hashtable();
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the location provider.
		/// </summary>
		public string Name
		{
			get { return( providerName ); }
		}

		/// <summary>
		/// Gets the description of the location provider.
		/// </summary>
		public string Description
		{
			get { return( description ); }
		}
		#endregion

		#region Constructors

		public GaimDomainProvider()
		{
			log.Debug( "Instantiated" );
		}

		#endregion

		#region IDomainProvider Implementation
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
		public Authentication.Status Authenticate( Simias.Storage.Domain domain, HttpContext ctx )
		{
			string gaimSessionTag = "gaim";

			Simias.Storage.Member member = null;

			// Assume failure
			Simias.Authentication.Status status = 
				new Simias.Authentication.Status( SCodes.InvalidCredentials );

			// Gaim domain requires session support
			if ( ctx.Session != null )
			{
				GaimSession gaimSession;

				// State should be 1
				string memberID = ctx.Request.Headers[ "gaim-member" ];
				if ( memberID == null || memberID == "" )
				{
					return status;
				}

				member = domain.GetMemberByID( memberID );
				if ( member == null )
				{
					return status;
				}

				status.UserName = member.Name;
				status.UserID = member.UserID;

				gaimSession = ctx.Session[ gaimSessionTag ] as GaimSession;
				if ( gaimSession == null )
				{
					gaimSession = new GaimSession();
					gaimSession.MemberID = member.UserID;
					gaimSession.State = 1;

					// Fixme
					gaimSession.OneTimePassword = DateTime.UtcNow.Ticks.ToString();
					GaimBuddy buddy = GaimDomain.GetBuddyByUserID( member.UserID );
					if (buddy != null)
					{
						RSACryptoServiceProvider credential = buddy.GetCredentialByUserID( member.UserID );
						if ( credential != null )
						{
							byte[] oneTime = new UTF8Encoding().GetBytes( gaimSession.OneTimePassword );
							byte[] encryptedText = credential.Encrypt( oneTime, false );
	
							// Set the encrypted one time password in the response
							ctx.Response.AddHeader(
								"gaim-secret",
								Convert.ToBase64String( encryptedText ) );
								
							ctx.Session[ gaimSessionTag ] = gaimSession;
						}
					}
				}
				else
				if ( status.UserID == gaimSession.MemberID )
				{
					// State should be 1
					string encodedSecret = ctx.Request.Headers[ "gaim-secret" ];
					if ( encodedSecret != null && encodedSecret != "" )
					{
						UTF8Encoding utf8 = new UTF8Encoding();
						string decodedString = 
							utf8.GetString( Convert.FromBase64String( encodedSecret ) );

						// Check it...
						if ( decodedString.Equals( gaimSession.OneTimePassword ) == true )
						{
							status.statusCode = SCodes.Success;
							gaimSession.State = 2;
						}
					}
					else
					{
						// Fixme
						gaimSession.OneTimePassword = DateTime.UtcNow.Ticks.ToString();
						gaimSession.State = 1;

						GaimBuddy buddy = GaimDomain.GetBuddyByUserID( member.UserID );
						if (buddy != null)
						{
							RSACryptoServiceProvider credential = buddy.GetCredentialByUserID( member.UserID );
							if ( credential != null )
							{
								try
								{
									byte[] oneTime = new UTF8Encoding().GetBytes( gaimSession.OneTimePassword );
									byte[] encryptedText = credential.Encrypt( oneTime, false );
	
									// Set the encrypted one time password in the response
									ctx.Response.AddHeader(
										"gaim-secret",
										Convert.ToBase64String( encryptedText ) );
								}
								catch( Exception encr )
								{
									log.Debug( encr.Message );
									log.Debug( encr.StackTrace );
								}
							}
						}
					}
				}
			}

			return status;
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		public void DeleteLocation( string domainID, string collectionID )
		{
			// GaimDomain does not keep location state
			// so wave it on through
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( string searchContext )
		{
			if (searchContext == null) return;

			if (searchContexts.Contains(searchContext))
			{
				searchContexts.Remove(searchContext);
			}
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, int count, out string searchContext, out Member[] memberList, out int total )
		{
			return FindFirstDomainMembers(domainID, PropertyTags.FullName, String.Empty, SearchOp.Contains, count, out searchContext, out memberList, out total);
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out string searchContext, out Member[] memberList, out int total )
		{
			// Ignore the domainID since we only ever have one domain to deal with

			ArrayList members = new ArrayList();
			ArrayList allMembers = new ArrayList();
			searchContext = null;
			total = 0;

			GaimBuddy[] buddies =
				GaimDomain.SearchForBuddies(mapSimiasAttribToGaim(attributeName),
											searchString,
											operation);
			if (buddies != null && buddies.Length > 0)
			{
				total = buddies.Length;
				foreach (GaimBuddy buddy in buddies)
				{
					string[] machineNames = buddy.MachineNames;
					for (int i = 0; i < machineNames.Length; i++)
					{
						Member member = GaimDomain.FindBuddyInDomain(buddy, machineNames[i]);
						if (member == null)
						{
							member = new Member(buddy.GetSimiasMemberName(machineNames[i]),
												buddy.GetSimiasUserID(machineNames[i]),
												Simias.Storage.Access.Rights.ReadWrite,
												null, null);
												
							// Use the Alias so the User Selector shows the buddy alias
							string alias = buddy.Alias;
							if (alias != null)
							{
								member.FN = string.Format("{0} ({1})", alias, machineNames[i]);
							}
						}
	
						if (members.Count < count)
						{
							// We haven't exceeded the requested search size
	
							members.Add(member);
						}
	
						// Save all the buddies for later
						allMembers.Add(member);
					}
				}
			}

			memberList = members.ToArray(typeof(Member)) as Member[];

			if (allMembers.Count > 0)
			{
				GaimDomainSearchContext newSearchContext = new GaimDomainSearchContext();
				newSearchContext.Members = allMembers;
				newSearchContext.CurrentIndex = members.Count;
				searchContext = newSearchContext.ID;
				lock (searchContexts.SyncRoot)
				{
					searchContexts.Add(searchContext, newSearchContext);
				}
			}

			return members.Count < allMembers.Count ? true : false;
		}

		/// <summary>
		/// Continues the search for all domain members started by calling the FindFirstDomainMembers method.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			bool bMoreEntries = false;
			ArrayList members = new ArrayList();
			memberList = null;

			if (searchContext == null)
				throw new ArgumentNullException("searchContext cannot be null when calling FindNextDomainMembers");

			lock (searchContexts.SyncRoot)
			{
				if (!searchContexts.Contains(searchContext))
					return false;

				GaimDomainSearchContext gaimDomainSearchContext = (GaimDomainSearchContext)searchContexts[searchContext];
				
				int i = gaimDomainSearchContext.CurrentIndex;

				while (i < gaimDomainSearchContext.Count)
				{
					Member member = (Member)gaimDomainSearchContext.Members[i];
					if (member != null)
					{
						if (members.Count < count)
						{
							members.Add(member);
						}
						else
						{
							bMoreEntries = true;
							break;
						}
					}
					
					i++;
				}

				if (i >= gaimDomainSearchContext.Count)
				{
					// Position the index at the end
					gaimDomainSearchContext.CurrentIndex = gaimDomainSearchContext.Count - 1;
				}
				else
				{
					gaimDomainSearchContext.CurrentIndex = i;
				}
			}

			if (members.Count > 0)
			{
				memberList = members.ToArray(typeof (Member)) as Member[];
			}

			return bMoreEntries;
		}

		/// <summary>
		/// Continues the search for domain members from a previous cursor.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindPreviousDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			bool bMoreEntries = false;
			ArrayList members = new ArrayList();
			memberList = null;

			if (searchContext == null)
				throw new ArgumentNullException("searchContext cannot be null when calling FindPreviousDomainMembers");

			lock (searchContexts.SyncRoot)
			{
				if (!searchContexts.Contains(searchContext))
					return false;

				GaimDomainSearchContext gaimDomainSearchContext = (GaimDomainSearchContext)searchContexts[searchContext];
				
				int i = gaimDomainSearchContext.CurrentIndex - 1;

				while (i >= 0)
				{
					Member member = (Member)gaimDomainSearchContext.Members[i];
					if (member != null)
					{
						if (members.Count < count)
						{
							members.Add(member);
						}
						else
						{
							bMoreEntries = true;
							break;
						}
					}
					
					i--;
				}

				if (i < 0)
				{
					gaimDomainSearchContext.CurrentIndex = 0;
				}
				else
				{
					gaimDomainSearchContext.CurrentIndex = i;
				}
			}

			if (members.Count > 0)
			{
				memberList = members.ToArray(typeof (Member)) as Member[];
			}

			return bMoreEntries;
		}

		/// <summary>
		/// Continues the search from the specified record location for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindSeekDomainMembers( ref string searchContext, int offset, int count, out Member[] memberList )
		{
			memberList = null;

			if (searchContext == null)
				throw new ArgumentNullException("searchContext cannot be null when calling FindSeekDomainMembers");
			lock (searchContexts.SyncRoot)
			{
				if (!searchContexts.Contains(searchContext))
					return false;

				GaimDomainSearchContext gaimDomainSearchContext = (GaimDomainSearchContext)searchContexts[searchContext];
				
				if (offset < 0 || offset >= gaimDomainSearchContext.Count)
					throw new IndexOutOfRangeException("offset is out of bounds with the total number of members available in the search");
					
				gaimDomainSearchContext.CurrentIndex = offset;
				
				return FindNextDomainMembers(ref searchContext, count, out memberList);
			}
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
			log.Debug( "OwnsDomain called" );
			return ( domainID.ToLower() == Simias.Gaim.GaimDomain.ID ) ? true : false;
		}

		/// <summary>
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		public void PreCommit( string domainID, Member member )
		{
			// Ignore domainID...since it's always the same

			GaimBuddy buddy = GaimDomain.GetBuddyByUserID(member.UserID);
			if (buddy == null)
			{
				log.Debug("PreCommit() called on a member that no longer exists in blist.xml");
				return;
			}
			
//			// If we make it this far, we've got everything we need to add onto the member object.
//			Simias.Storage.Property p =
//				new Property("Gaim:MungedID", buddy.MungedID);
//			p.LocalProperty = true;
//			member.Properties.AddProperty(p);
			
			// Gaim Account Name
			Simias.Storage.Property p = new Property("Gaim:AccountName", buddy.AccountName);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Account Protocol
			p = new Property("Gaim:AccountProto", buddy.AccountProtocolID);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);
			
			// Gaim Screenname
			p = new Property("Gaim:Screenname", buddy.Name);
			p.LocalProperty = true;
			member.Properties.AddProperty(p);

			// Buddy Simias UserID
//			if (buddy.SimiasUserID != null)
//			{
//				p = new Property("Gaim:SimiasUserID", buddy.SimiasUserID);
//				p.LocalProperty = true;
//				member.Properties.AddProperty(p);
//			}

			string machineName = GaimBuddy.ParseMachineName(member.Name);
			if (machineName != null)
			{
				// Buddy Simias URL
				string simiasURL = buddy.GetSimiasURL(machineName);
				if (simiasURL != null)
				{
					p = new Property("Gaim:SimiasURL", simiasURL);
					p.LocalProperty = true;
					member.Properties.AddProperty(p);
				}
			}
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
			log.Debug( "ResolveLocation(domainID) called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
				locationUri = MemberToUri( GaimDomain.GetDomain(), member );
			}

			return locationUri;
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
			log.Debug( "ResolveLocation called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Store store = Store.GetStore();
					Simias.Storage.Domain domain = store.GetDomain(domainID.ToLower());

					Simias.POBox.POBox poBox =
						Simias.POBox.POBox.FindPOBox(store, domainID, domain.GetCurrentMember().UserID);

					Subscription sub = poBox.GetSubscriptionByCollectionID(collectionID);
					locationUri = SubscriptionFromIDToUri(sub.FromIdentity);

					if (locationUri == null)
					{
						Collection collection = Store.GetStore().GetCollectionByID( collectionID );
						Member member = collection.GetMemberByID(collection.Owner.UserID);
						if ( member != null )
						{
							locationUri = MemberToUri(domain, member);
						}
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
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
			log.Debug( "ResolveLocation with userID called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Member member = domain.GetMemberByID(userID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
		}

		/// <summary>
		/// Returns the network location of where to the POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			log.Debug( "ResolvePOBoxLocation called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Member member = domain.GetMemberByID(userID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;

		}

		/// <summary>
		/// Sets a new host address for the domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain for network address
		/// to be changed.</param>
		/// <param name="hostLocation">A Uri object containing the new network
		/// address for the domain.</param>
		public void SetHostLocation( string domainID, Uri hostLocation )
		{
			// Not needed by gaim. Just here to satisify the interface requirement.
		}

		#endregion

		#region Private Methods
		private Uri MemberToUri(Simias.Storage.Domain domain, Member member)
		{
			Uri locationUri = null;
			
			if (domain == null || member == null) return null;			

			// Since we store the Buddy's SimiasURL right in the Simias Store,
			// we can just retrieve the information from the database.
			Simias.Storage.PropertyList pList = member.Properties;
			Simias.Storage.Property p = pList.GetSingleProperty("Gaim:SimiasURL");
			if (p == null) return null;
			
			locationUri = new Uri((string) p.Value);

			return locationUri;
		}

		private Uri SubscriptionFromIDToUri(string fromID)
		{
			Uri locationUri = null;

			GaimBuddy buddy = GaimDomain.GetBuddyByUserID(fromID);
			if (buddy != null)
			{
				string simiasURL = buddy.GetSimiasURLByUserID(fromID);
				if (simiasURL != null)
				{
					locationUri = new Uri(simiasURL);
				}
			}

			return locationUri;
		}

		private string mapSimiasAttribToGaim(string attributeName)
		{
//			switch(attributeName)
//			{
//				case "Given":
					return "ScreenName";
//				case "Family":
//				case "FN":
//				default:
//					return "Alias";
//			}
		}
		#endregion

		private class GaimSession
		{
			public string	MemberID;
			public string	OneTimePassword;
			public int		State;

			public GaimSession()
			{
			}
		}
	}
}
