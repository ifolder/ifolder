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

using System;
using System.Collections;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;

//using Novell.AddressBook;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Class to initialize/verify a SimpleServer domain in the Collection Store
	/// </summary>
	public class Domain
	{
		#region Class Members

		/// <summary>
		/// GUID for this SimpleServer domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Simple Server";
		private string hostAddress;
		private string description = "Simple Server domain";
		private string ownerMember;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string serverDocumentPath = "../../etc/SimpleServer.xml";
		private XmlDocument serverDoc;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the SimpleServer domain's unique ID
		/// </summary>
		public string ID
		{
			get { return(this.id); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's friendly ID
		/// </summary>
		public string Name
		{
			get { return(this.domainName); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's description
		/// </summary>
		public string Description
		{
			get { return(this.description); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's host address
		/// </summary>
		public string Host
		{
			get { return(this.hostAddress); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		internal Domain( bool init )
		{
			if ( init == true )
			{
				this.Init();
			}
		}

		/// <summary>
		/// Constructor for creating a new mDnsDomain object.
		/// </summary>
		/// <param name="description">String that describes this domain.</param>
		internal Domain( bool init, string description ) 
		{
			this.description = description;

			if ( init == true )
			{
				this.Init();
			}
		}
		#endregion

		internal void Init()
		{
			hostAddress = MyDns.GetHostName();
			Store store = Store.GetStore();

			// Find which member should be the domain owner
			try
			{
				// Load the configuration document from the file.
				serverDoc = new XmlDocument();
				serverDoc.Load( serverDocumentPath );

				XmlElement domainElement = serverDoc.DocumentElement;
				domainName = domainElement.GetAttribute( "Name" );
				string tmpDescription = "";
				try
				{
					tmpDescription = domainElement.GetAttribute( "Description" );
				}
				catch{}
				if ( tmpDescription != null )
				{
					description = tmpDescription;
				}

				XmlAttribute attr;
				XmlNode ownerNode = null;
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					attr = domainElement.ChildNodes[i].Attributes["Owner"];
					if ( attr != null && attr.Value == "true" )
					{
						ownerNode = domainElement.ChildNodes[i];
						ownerMember = ownerNode.Attributes["Name"].Value;
						break;
					}
				}

				if ( ownerMember == null || ownerMember == "" )
				{
					throw new SimiasException( "No member with owner status specified" );
				}

				//
				// Verify the SimpleServer domain exists or create it.
				//
			
				Simias.Storage.Domain rDomain = 
					this.GetSimpleServerDomain( true, ownerMember );

				if ( rDomain == null )
				{
					log.Error( "Couldn't create or verify SimpleServer domain" );
					return;
				}

				//
				// Make this domain the default
				//

				store.DefaultDomain = rDomain.ID;

				//
				// Verify the POBox for the local SimpleServer owner
				//
			
				/*
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + rDomain.ID + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, rDomain.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, rDomain.ID );
					poBox.Role = SyncRoles.Master;
					poBox.CreateMaster = false;

					pMember = 
						new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
					pMember.IsOwner = true;
					poBox.Commit(new Node[] { poBox, pMember });
				}
				else
				{
					// verify member in POBox
					pMember = poBox.GetMemberByID( ldbMember.ID );
					if (pMember == null)
					{
						pMember = 
							new Member( ldbMember.Name, ldbMember.ID, Access.Rights.ReadWrite );
						pMember.IsOwner = true;
						poBox.Commit(new Node[] { pMember });

					}
				}
				*/
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				//throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		/// <summary>
		/// Method to get the Simias simple server domain
		/// If the the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerName is ignored
		/// </summary>
		internal Simias.Storage.Domain GetSimpleServerDomain( bool create, string ownerName )
		{
			//
			//  Check if the SimpleServer domain exists in the store
			//

			Simias.Storage.Domain ssDomain = null;
			Store store = Store.GetStore();

			try
			{
				foreach( ShallowNode sNode in store.GetDomainList() )
				{
					Simias.Storage.Domain tmpDomain = store.GetDomain( sNode.ID );
					Property p = tmpDomain.Properties.GetSingleProperty( "SimpleServer" );
					if ( p != null && (bool) p.Value == true )
					{
						ssDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
				}

				if ( ssDomain == null && create == true )
				{
					this.id = Guid.NewGuid().ToString();
					//Uri localUri = new Uri("http://" + MyDns.GetHostName() + "/simias10");

					// Create the simple server domain.
					ssDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName, 
							this.id,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
                            Simias.Storage.Domain.ConfigurationType.ClientServer );

					// This needs to be added to allow the enterprise location provider
					// to be able to resolve this domain.
					ssDomain.SetType( ssDomain, "Enterprise" );

					// Set that this is a simple server domain.
					Property p = new Property( "SimpleServer", true );
					p.LocalProperty = true;
					ssDomain.Properties.ModifyProperty( p );

					// Create the owner member for the domain.
					Member member = 
						new Member(
							ownerName, 
							Guid.NewGuid().ToString(), 
							Access.Rights.Admin );

					member.IsOwner = true;
					ssDomain.Commit( new Node[] { ssDomain, member } );

					// Create the name mapping.
					store.AddDomainIdentity( ssDomain.ID, member.UserID );
				}
			}
			catch( Exception gssd )
			{
				log.Error( gssd.Message );
				log.Error( gssd.StackTrace );
			}

			return ssDomain;
		}

		#region Public Methods

		/// <summary>
		/// Synchronize the SimpleServer.xml members to the Simias member list
		/// </summary>
		public void SynchronizeMembers()
		{
			string member;
			string firstName;
			string lastName;

			log.Debug( "SynchronizeMembers - called" );

			//
			// Create a sync iteration guid which will be stamped
			// in all matching objects as a local property
			//

			Property syncP = new Property("SyncGuid", Guid.NewGuid().ToString());
			syncP.LocalProperty = true;
			bool errorDuringSync = false;

			//
			// First verify the System Book exists in Simias
			//

			Store store = Store.GetStore();
			if ( store == null )
			{
				log.Error( "failed to connect to the Simias store" );
				return;
			}

			/*
			Novell.AddressBook.Manager abManager = Novell.AddressBook.Manager.Connect();
			if ( abManager == null )
			{
				log.Error("failed to connect to the address book");
				return;
			}
			*/

			Simias.Storage.Domain ssDomain = null;

			try
			{
				ssDomain = this.GetSimpleServerDomain( false, "" );
			}
			catch{}

			//
			// If this domain isn't already an Address Book, make it one
			//

			/*
			if (ssDomain.IsType( ssDomain, "AB:AddressBook" ) == false)
			{
				ssDomain.SetType( ssDomain, "AB:AddressBook" );
				ssDomain.Commit();
			}
			

			AddressBook systemBook = abManager.GetAddressBook( ssDomain.ID );
			*/

			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load( serverDocumentPath );

				XmlElement domainElement = serverDoc.DocumentElement;

				XmlAttribute attr;
				//XmlNode ownerNode = null;
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					firstName = null;
					lastName = null;

					attr = domainElement.ChildNodes[i].Attributes["Name"];
					if (attr != null)
					{
						XmlNode cNode = domainElement.ChildNodes[i];
						member = cNode.Attributes["Name"].Value;

						//
						// Retrieve the contact properties from SimpleServer.xml
						//

						XmlNode memberNode = domainElement.ChildNodes[i];
						for ( int x = 0; x < memberNode.ChildNodes.Count; x++ )
						{
							if ( memberNode.ChildNodes[x].Name == "First" )
							{
								firstName = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "Last" )
							{
								lastName = memberNode.ChildNodes[x].InnerText;
							}
						}

						memberNode = null;

						//
						// Check if this member already exists
						//

						Simias.Storage.Member ssMember = null;
						try
						{	
							ssMember = ssDomain.GetMemberByName( member );
						}
						catch{}

						if ( ssMember != null )
						{
							// Check if the password has changed
							XmlAttribute pwdAttr = 
								domainElement.ChildNodes[i].Attributes["Password"];
							if ( pwdAttr != null )
							{
								Property password = ssMember.Properties.GetSingleProperty( "SS:Password" );
								if ( password != null )
								{
									if ( password.Value as string != pwdAttr.Value as string )
									{
										password.Value = pwdAttr.Value;
										password.LocalProperty = true;
										ssMember.Properties.ModifyProperty( password );
									}
								}
								else
								{
									password = new Property( "SS:Password", pwdAttr.Value as string );
									password.LocalProperty = true;
									ssMember.Properties.ModifyProperty( password );
								}
							}
							else
							{
								ssMember.Properties.DeleteProperties( "SS:Password" );
							}

							//
							// Not sure if I modify a property with the same
							// value that already exists will force a node
							// update and consequently a synchronization so I'll
							// check just to be sure.
							//

							// First name change?
							if ( firstName != null )
							{
								if ( ssMember.Given != firstName )
								{
									ssMember.Given = firstName;
								}
							}
							else
							{
								ssMember.Given = firstName;
							}

							// Last name change?
							if ( lastName != null )
							{
								if ( ssMember.Family != lastName )
								{
									ssMember.Family = lastName;
								}
							}
							else
							{
								ssMember.Given = lastName;
							}

							if ( ssMember.FN == null )
							{
								ssMember.FN = ssMember.Given + " " + ssMember.Family;
							}

							ssMember.Properties.ModifyProperty( syncP );
							ssDomain.Commit( ssMember );
						}
						else
						{
							//
							// The member didn't exist so let's create it
							//

							try
							{
								// Create a new member and then contact
								ssMember = new
									Member(
										member,
										Guid.NewGuid().ToString(), 
										Simias.Storage.Access.Rights.ReadOnly,
										firstName,
										lastName);

								// Set the local property sync guid
								ssMember.Properties.ModifyProperty(syncP);

								// Get the password
								XmlAttribute pwdAttr = 
									domainElement.ChildNodes[i].Attributes["Password"];
								if ( pwdAttr != null )
								{
									Property pwd = new Property( "SS:Password", pwdAttr.Value );
									pwd.LocalProperty = true;
									ssMember.Properties.ModifyProperty( pwd );
								}

								ssDomain.Commit( ssMember );
							}
							catch
							{
								continue;
							}
						}	
					}
				}
			}
			catch(Exception e)
			{
				log.Error( "Error:" + e.Message );
				log.Error( e.StackTrace );
				errorDuringSync = true;
				//syncException = e;
			}

			// If we didn't have any errors delete any members that
			// don't exist in the xml list

			if ( errorDuringSync == false )
			{
				log.Debug("Checking for deleted SimpleServer.xml members");
				ICSList	deleteList = 
					ssDomain.Search( "SyncGuid", syncP.Value, SearchOp.Not_Equal );
				try
				{
					foreach( ShallowNode cShallow in deleteList )
					{
						Node cNode = new Node( ssDomain, cShallow );
						if ( ssDomain.IsBaseType( cNode, "Member" ) == true )
						{	
							// Delete this sucker...
							log.Debug("deleting: " + cNode.Name);
							ssDomain.Commit( ssDomain.Delete( cNode ) );
						}
					}
				}
				catch{}
				log.Debug("Finished checking for deleted SimpleServer.xml members");
			}

			log.Debug( "SynchronizeMembers - finished" );
			return;
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.domainName;
		}
		#endregion
	}
}
