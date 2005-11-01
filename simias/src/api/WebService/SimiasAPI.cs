/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Novell.Security.ClientPasswordManager;

using Simias;
using Simias.Client;
using Simias.DomainServices;
using Simias.Storage;
using Simias.Sync;
using Simias.Security.Web.AuthenticationService;
//using Simias.POBox;

namespace Simias.Web
{
	/// <summary>
	/// SimiasResult holds the results to the WebService calls
	/// </summary>
	public struct SimiasResult
	{
		public int code;
		public string result;
		public string exception;
	}

	/// <summary>
	/// This is the core of the SimiasServce.  All of the methods in the
	/// web service are implemented here.
	/// </summary>
	[WebService(
	Namespace="http://novell.com/simiasapi/web/",
	Name="SimiasAPIService",
	Description="Web Service providing access to SimiasAPI")]
	public class SimiasAPIService : WebService
	{
		/// <summary>
		/// Creates the SimiasService and sets up logging
		/// </summary>
		public SimiasAPIService()
		{
		}

		/// <summary>
		/// WebMethod that allows a client to ping the service to see
		/// if it is up and running
		/// </summary>
		[WebMethod(EnableSession=true, Description="Allows a client to ping to make sure the Web Service is up and running")]
		[SoapDocumentMethod]
		public void Ping()
		{
			// Nothing to do here, just return
		}


		/// <summary>
		/// WebMethod that gets all domains
		/// </summary>
		[WebMethod(EnableSession=true, Description="Returns all domains in Simias with optional only slaves")]
		[SoapDocumentMethod]
		public string GetDomains()
		{
			StringBuilder result = new StringBuilder();
			result.Append("<?xml version=\"1.0\"?>");
			result.Append("<ObjectList>");

			try
			{
				Store store = Store.GetStore();

				ICSList domainList = store.GetDomainList();

				foreach( ShallowNode sn in domainList )
				{
					Collection c = store.GetCollectionByID(sn.CollectionID);
					Node node = c.GetNodeByID(sn.ID);

					result.Append(node.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
				}
			}
			catch(Exception e) 
			{
				return e.ToString();
			}
			
			result.Append("</ObjectList>");

			return result.ToString();
		}

		/// <summary>
		/// WebMethod that gets collections
		/// </summary>
		[WebMethod(EnableSession=true, Description="Returns all collections in Simias with optional only slaves")]
		[SoapDocumentMethod]
		public string GetCollections()
		{
			StringBuilder result = new StringBuilder();
			result.Append("<?xml version=\"1.0\"?>");
			result.Append("<ObjectList>");

			try
			{
				Store store = Store.GetStore();

				ICSList domainList = store.GetDomainList();

				foreach( ShallowNode sn in domainList )
				{
                    ICSList collectionlist = store.GetCollectionsByDomain(sn.ID);

                    foreach( ShallowNode csn in collectionlist )
                    {
                        Collection c = store.GetCollectionByID(csn.CollectionID);

                        result.Append(c.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                    }
				}
			}
			catch(Exception e) 
			{
				return e.ToString();
			}
			
			result.Append("</ObjectList>");

			return result.ToString();
		}

		/// <summary>
		/// WebMethod that gets collections
		/// </summary>
		[WebMethod(EnableSession=true, Description="Returns all collections in Simias with optional only slaves")]
		[SoapDocumentMethod]
		public string GetCollectionsInDomain(string domainID)
		{
			StringBuilder result = new StringBuilder();
			result.Append("<?xml version=\"1.0\"?>");
			result.Append("<ObjectList>");

			try
			{
				Store store = Store.GetStore();

                ICSList collectionlist = store.GetCollectionsByDomain(domainID);

                foreach( ShallowNode csn in collectionlist )
                {
                    Collection c = store.GetCollectionByID(csn.CollectionID);

                    result.Append(c.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                }
			}
			catch(Exception e) 
			{
				return e.ToString();
			}
			
			result.Append("</ObjectList>");

			return result.ToString();
		}

		/// <summary>
		/// WebMethod that gets collections
		/// </summary>
		[WebMethod(EnableSession=true, Description="Returns all collections in Simias with optional only slaves")]
		[SoapDocumentMethod]
		public string GetCollectionsByType(string type)
		{
			StringBuilder result = new StringBuilder();
			result.Append("<?xml version=\"1.0\"?>");
			result.Append("<ObjectList>");

			try
			{
				Store store = Store.GetStore();

				ICSList domainList = store.GetDomainList();

				foreach( ShallowNode sn in domainList )
				{
                    ICSList collectionlist = store.GetCollectionsByDomain(sn.ID);

                    foreach( ShallowNode csn in collectionlist )
                    {
                        Collection col = store.GetCollectionByID(csn.CollectionID);
                        if (col.IsType(col, type))
                        {
                            result.Append(col.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                        }
                    }
				}
			}
			catch(Exception e) 
			{
				return e.ToString();
			}
			
			result.Append("</ObjectList>");

			return result.ToString();
		}

		/// <summary>
		/// WebMethod that gets collections
		/// </summary>
		[WebMethod(EnableSession=true, Description="Returns all collections in Simias with optional only slaves")]
		[SoapDocumentMethod]
		public string GetCollectionsInDomainByType(string domainID, string type)
		{
			StringBuilder result = new StringBuilder();
			result.Append("<?xml version=\"1.0\"?>");
			result.Append("<ObjectList>");

			try
			{
				Store store = Store.GetStore();

                ICSList collectionlist = store.GetCollectionsByDomain(domainID);

                foreach( ShallowNode csn in collectionlist )
                {
                    Collection col = store.GetCollectionByID(csn.CollectionID);
                    if (col.IsType(col, type))
                    {
                        result.Append(col.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                    }
                }
			}
			catch(Exception e) 
			{
				return e.ToString();
			}
			
			result.Append("</ObjectList>");

			return result.ToString();
		}


        /// <summary>
        /// WebMethod that gets nodes from a collection
        /// </summary>
        [WebMethod(EnableSession=true, 
                   Description="Returns all nodes in a collection")]
        [SoapDocumentMethod]
        public string GetNodes(string collectionID, string type)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<?xml version=\"1.0\"?>");
            result.Append("<ObjectList>");

            try
            {
                Store store = Store.GetStore();

                Collection col = store.GetCollectionByID(collectionID);

                if(type.Length > 0)
                {
                    ICSList nodeList = col.GetNodesByType(type);
                    foreach( ShallowNode sn in nodeList )
                    {
                        Node node = col.GetNodeByID(sn.ID);
                        result.Append(node.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                    }
                }
                else
                {
                    foreach( ShallowNode sn in col )
                    {
                        Node node = col.GetNodeByID(sn.ID);
                        result.Append(node.Properties.ToString(false).Replace("<ObjectList>", "").Replace("</ObjectList>", ""));
                    }
                }
            }
            catch(Exception e) 
            {
                return e.ToString();
            }

            result.Append("</ObjectList>");

            return result.ToString();
        }



	}
}




