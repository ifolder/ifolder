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
using Simias.Client;
using Simias.Storage;

using Novell.AddressBook;

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
		/// FIXME::This ID shouldn't be static but we don't have
		/// a way to look a domain by id
		/// </summary>
		private string id = "29ebbf9c-d9e8-4471-be80-df3dd90db0ff";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "SimpleServer";
		private string hostAddress;
		private string description = "Simple Server domain";
		private string ownerMember;

		private string firstName;
		private string lastName;
		private string emailAddress;
		private string im;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string serverDocumentPath = "SimpleServer.xml";
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
			log.Debug("  My Address: " + hostAddress);
			Store store = Store.GetStore();


			// Find which member should be the domain owner
			try
			{
				// Load the configuration document from the file.
				serverDoc = new XmlDocument();
				serverDoc.Load(serverDocumentPath);

				XmlElement domainElement = serverDoc.DocumentElement;
				domainName = domainElement.GetAttribute("Name");
				string tmpDescription = "";
				try
				{
					tmpDescription = domainElement.GetAttribute("Description");
				}
				catch{}
				if ( tmpDescription != null )
				{
					description = tmpDescription;
				}

				XmlAttribute attr;
				XmlNode ownerNode = null;
				for (int i = 0; i < domainElement.ChildNodes.Count; i++)
				{
					attr = domainElement.ChildNodes[i].Attributes["Owner"];
					if (attr != null && attr.Value == "true")
					{
						ownerNode = domainElement.ChildNodes[i];
						ownerMember = ownerNode.Attributes["Name"].Value;
						break;
					}
				}

				if ( ownerMember == null || ownerMember == "" )
				{
					throw new Exception("No member with owner status specified");
				}

				//
				// The current member of the local database will be
				// the SimpleServer domain owner
				//

				Member ldbMember = null;
				LocalDatabase ldb = store.GetDatabaseObject();
				
				ICSList memberList = ldb.GetNodesByName( ownerMember );
				foreach( ShallowNode shallowNode in memberList )
				{
					Node cNode = new Node( ldb, shallowNode );
					Simias.Storage.Property simpleProp = 
						cNode.Properties.GetSingleProperty( "SimpleServer" );
					if (simpleProp != null)
					{
						ldbMember = new Member( cNode );
						break;
					}
				}

				if ( ldbMember == null )
				{
					// Create a local member which is the owner of the mDnsDomain
					ldbMember = new Member( ownerMember, Guid.NewGuid().ToString(), Access.Rights.Admin );
					ldbMember.IsOwner = true;

					Simias.Storage.Property simpleProp = new Property( "SimpleServer" , true );
					simpleProp.LocalProperty = true;
					ldbMember.Properties.AddProperty( simpleProp );

					// Save the local database changes.
					ldb.Commit( new Node[] { ldbMember } );
				}

				//
				// Verify the SimpleServer domain exists
				// FIXME:: need to enum domains and look for my special property
				// or maybe add a new type to the domain to know it's a SimpleServer
				// You should only be able to have one SimpleServer domain in a master
				// role per store
				//
			
				Uri localUri = new Uri("http://" + hostAddress + "/simias10");
				Simias.Storage.Domain rDomain = store.GetDomain( this.id );
				if (rDomain == null)
				{
					// Create the mDnsDomain and add an identity mapping.
					/* Reliase when Mike checks in Role changes
					store.AddDomainIdentity(
						ldbMember.ID,
						this.domainName,
						this.id, 
						this.description,
						localUri,
						Simias.Storage.Domain.DomainRole.Master);
					*/

					store.AddDomainIdentity(
						ldbMember.ID,
						this.domainName,
						this.id, 
						this.description,
						localUri);

					rDomain = store.GetDomain( this.id );
					//Node cNode = new Node( this.id, (Simias.Storage.Node) rDomain );
					//cNode.SetType( cNode, "SimpleServer" );
				}

				//
				// Verify the SimpleServer roster
				//

				Roster ssRoster = null;
				Member rMember;
				ArrayList changeList = new ArrayList();

				try
				{
					ssRoster = rDomain.GetRoster( store );
				}
				catch{}
				if ( ssRoster == null )
				{
					ssRoster = new Roster( store, store.GetDomain( this.id ));
					rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
					rMember.IsOwner = true;
					//rMember.Properties.ModifyProperty( "POBox", hostAddress );
					changeList.Add(ssRoster);
					//mdnsRoster.Commit( new Node[] { mdnsRoster, rMember } );
				}
				else
				{
					// Make sure the member exists
					rMember = ssRoster.GetMemberByID( ldbMember.ID );
					if (rMember == null)
					{
						rMember = new Member( ldbMember.Name, ldbMember.ID, Access.Rights.Admin );
						rMember.IsOwner = true;
					}
				}

				//rMember.Properties.ModifyProperty( "POBox", hostAddress );
				changeList.Add(rMember);
				ssRoster.Commit( changeList.ToArray( typeof( Node ) ) as Node[] );

				//
				// Verify the POBox for the local SimpleServer owner
				//
			
				Member pMember;
				Simias.POBox.POBox poBox = null;
				string poBoxName = "POBox:" + this.id + ":" + ldbMember.ID;

				try
				{
					poBox = Simias.POBox.POBox.FindPOBox( store, this.ID, ldbMember.ID );
				}
				catch{}
				if (poBox == null)
				{
					poBox = new Simias.POBox.POBox( store, poBoxName, this.id );
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
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);

				//throw e1;
				// FIXME:: rethrow the exception
			}			
		}

		#region Public Methods

		/// <summary>
		/// Synchronize the SimpleServer.xml members to the Simias member list
		/// </summary>
		public void SynchronizeMembers()
		{
			string	member;
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
			if (store == null)
			{
				log.Error("failed to connect to the Simias store");
				return;
			}

			Novell.AddressBook.Manager abManager = Novell.AddressBook.Manager.Connect();
			if (abManager == null)
			{
				log.Error("failed to connect to the address book");
				return;
			}

			Simias.Storage.Domain ssDomain = null;
			Simias.Storage.Roster ssRoster = null;

			try
			{
				ssDomain = store.GetDomain( this.id );
				ssRoster = ssDomain.GetRoster( store );
			}
			catch(Exception e)
			{
				log.Error("Failed getting the SimpleServer roster");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			if (ssRoster == null)
			{
				return;
			}

			//
			// If this roster isn't already an Address Book, make it one
			//
			if (ssRoster.IsType( ssRoster, "AB:AddressBook" ) == false)
			{
				ssRoster.SetType( ssRoster, "AB:AddressBook" );
				ssRoster.Commit();
			}

			AddressBook systemBook = abManager.GetAddressBook( ssRoster.ID );

			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load(serverDocumentPath);

				XmlElement domainElement = serverDoc.DocumentElement;

				XmlAttribute attr;
				XmlNode ownerNode = null;
				for (int i = 0; i < domainElement.ChildNodes.Count; i++)
				{
					this.firstName = null;
					this.lastName = null;
					this.im = null;
					this.emailAddress = null;

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
								this.firstName = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "Last" )
							{
								this.lastName = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "Email" )
							{
								this.emailAddress = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "IM" )
							{
								this.im = memberNode.ChildNodes[x].InnerText;
								//attr = domainElement.ChildNodes[i].Attributes["Name"];
							}
						}

						memberNode = null;

						//
						// Check if this member already exists
						//

						Simias.Storage.Member ssMember = null;
						try
						{	
							ssMember = ssRoster.GetMemberByName( member );
						}
						catch{}

						if (ssMember != null)
						{
							//
							// First get the contact that's related to this member
							//

							Contact ssContact = null;
							Relationship contactMember = new Relationship( ssRoster.ID, ssMember.ID );
							ICSList results = ssRoster.Search( "Member", contactMember );
							IEnumerator contactEnum = results.GetEnumerator();
							if (contactEnum.MoveNext() == false)
							{
								// This member does not have an associated contact

								// FIXME::Dictionary
								if (firstName != "" ||
									lastName != "" ||
									emailAddress != "" ||
									im != "")
								{
									ssContact = new Contact();
									ssContact.UserID = ssMember.UserID;

									if (firstName != null && lastName != null)
									{
										ssContact.UserName = firstName + " " + lastName;
									}
									else
									{
										ssContact.UserName = member;
									}

									//
									// Setup a relationship from the contact to the member
									// The relationship must be setup this way because normal users
									// will always have read-only access to the system book members
									// and contacts.  For contact self-service a personal contact will
									// be related back to the member and any changes to the personal
									// contact will be reflected back to LDAP in an out-of-band
									// communication.  
									//

									Relationship cRelationship = new Relationship( ssRoster.ID, ssMember.ID );
									ssContact.Properties.ModifyProperty( "Member", cRelationship );
									systemBook.AddContact( ssContact );
									//ssContact.Commit();
								}
							}
							else
							{
								ShallowNode cShallow = (ShallowNode) contactEnum.Current;
								ssContact = systemBook.GetContact( cShallow.ID );
							}
						
							try
							{
								if (ssContact != null)
								{
									this.UpdateContactProperties(ssContact);
									ssContact.Commit();
								}
							}
							catch{}

							ssMember.Properties.ModifyProperty(syncP);
							ssRoster.Commit( ssMember );
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
										Simias.Storage.Access.Rights.ReadOnly);

								// Set the local property sync guid
								ssMember.Properties.ModifyProperty(syncP);
								ssRoster.Commit(ssMember);
							}
							catch
							{
								continue;
							}

							// If no Contact properties exist don't create the contact
							if (firstName == null &&
								lastName == null &&
								im == null &&
								emailAddress == null)
							{
								continue;
							}

							Contact ssContact = new Contact();
							ssContact.UserID = ssMember.UserID;

							if ( firstName != null && lastName != null )
							{
								ssContact.UserName = firstName + " " + lastName;
							}
							else
							{
								ssContact.UserName = member;
							}

							//
							// Setup a relationship from the contact to the member
							// The relationship must be setup this way because normal users
							// will always have read-only access to the system book members
							// and contacts.  For contact self-service a personal contact will
							// be related back to the member and any changes to the personal
							// contact will be reflected back to LDAP in an out-of-band
							// communication.  
							//

							Relationship cRelationship = new Relationship( ssRoster.ID, ssMember.ID );
							ssContact.Properties.ModifyProperty( "Member", cRelationship );

							// Update the contact properties
							this.UpdateContactProperties(ssContact);
							//ssContact.Commit();
							systemBook.AddContact(ssContact);
							ssContact.Commit();
						}	
					}
				}
			}
			catch(Exception e)
			{
				log.Error("Error:" + e.Message);
				errorDuringSync = true;
				//syncException = e;
			}

			// If we didn't have any errors delete any members that
			// don't exist in the xml list

			if (errorDuringSync == false)
			{
				log.Debug("Checking for deleted SimpleServer.xml members");
				ICSList	deleteList = 
					ssRoster.Search( "SyncGuid", syncP.Value, SearchOp.Not_Equal );
				try
				{
					foreach( ShallowNode cShallow in deleteList )
					{
						Node cNode = new Node( ssRoster, cShallow );
						if ( ssRoster.IsType( cNode, "Member" ) == true )
						{	
							try
							{
								Property p = cNode.Properties.GetSingleProperty( "MemberToContact" );
								if ( p != null )
								{
									Simias.Storage.Relationship cRelationship =
										(Simias.Storage.Relationship) p.Value;

									Contact cContact =
										systemBook.GetContact( cRelationship.NodeID );
									if ( cContact != null )
									{
										cContact.Delete();
									}
								}
							}
							catch{}

							// Delete this sucker...
							log.Debug("deleting: " + cNode.Name);
							ssRoster.Commit( ssRoster.Delete( cNode ) );
						}
					}
				}
				catch{}
				log.Debug("Finished checking for deleted SimpleServer.xml members");
			}

			log.Debug( "SynchronizeMembers - finished" );
			return;
		}

		internal void UpdateContactProperties(Contact cContact)
		{
			try
			{
				if ( this.firstName != null && this.lastName != null )
				{
					// System address book only ever has one name so we can just
					// get the preferred name for checking

					Name prefName = cContact.GetPreferredName();
					if (prefName != null)
					{
						if (prefName.Given != this.firstName ||
							prefName.Family != this.lastName)
						{
							prefName.Delete();

							Name cName = new Name(this.firstName, this.lastName);
							cContact.AddName(cName);
						}
					}
					else
					{
						Name cName = new Name(this.firstName, this.lastName);
						cContact.AddName(cName);
					}
				}
			}
			catch{}

			try
			{
				if (this.emailAddress != null)
				{
					//
					// For now the system address book only has one e-mail address
					// an employees work address
					//

					Email prefMail = cContact.GetPreferredEmailAddress();
					if (prefMail != null)
					{
						if (this.emailAddress != prefMail.Address)
						{
							prefMail.Delete();

							Email cMail = 
								new Email(
								EmailTypes.internet | EmailTypes.work | EmailTypes.preferred,
								this.emailAddress);
							cContact.AddEmailAddress(cMail);
						}
					}
					else
					{
						Email cMail = new  
							Email(
								EmailTypes.internet | EmailTypes.work | EmailTypes.preferred, 
								this.emailAddress);
						cContact.AddEmailAddress(cMail);
					}
				}
			}
			catch{}
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
