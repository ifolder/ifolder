/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#include "gaim-domain.h"
#include <simias.h>

/* Gaim Includes */
#include "account.h"
#include "blist.h"

#include <simiasgaimStub.h>
#include <simiasgaim.nsmap>

static char *the_soap_url = NULL;

/* Forward Declarations of Static Functions */
static void sync_account(gpointer data, gpointer user_data);
static void sync_buddy(gpointer data, gpointer user_data);

static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(gboolean reread_config);

/* Function Implementation */
int
gaim_domain_add_gaim_buddy(const char *account_name, const char *account_proto,
						   const char *buddy_name, const char *alias,
						   const char *ip_addr, const char *ip_port)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__AddGaimBuddy req;
	struct _ns1__AddGaimBuddyResponse resp;
	
	soap_url = get_soap_url(FALSE);
	if (!soap_url) {
		return -1;
	}
	
	/* Setup the Request */
	req.accountName		= (char *)account_name;
	req.accountProto	= (char *)account_proto;
	req.buddyName		= (char *)buddy_name;
	req.alias			= (char *)alias;
	req.ipAddr			= (char *)ip_addr;
	req.ipPort			= (char *)ip_port;
	
	init_gsoap(&soap);
	soap_call___ns1__AddGaimBuddy(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return -1;
	}
	
	cleanup_gsoap(&soap);
	
	return 0;
}

int
gaim_domain_update_gaim_buddy(const char *account_name, 
								  const char *account_proto,
								  const char *buddy_name,
								  const char *alias,
								  const char *ip_addr,
								  const char *ip_port)
{
	/**
	 * Since we're privy to the implementation of the webservice, we know we can
	 * just call gaim_domain_add_gaim_buddy() (since it just calls an update if
	 * the buddy already exists.
	 */
	return gaim_domain_add_gaim_buddy(account_name, account_proto, buddy_name,
									  alias, ip_addr, ip_port);
}

/**
 * This function is used to loop through ALL of the buddies in the buddy list
 * when Gaim first starts up (and Simias is running) or when Gaim has been
 * running and Simias comes online.
 * 
 * The information in Gaim breaks any ties (i.e., it wins on the conflict
 * resolution).  All changes to information about Gaim buddies should be done
 * by the user in Gaim.  The Gaim Roster list in Simias should not be editable
 * in iFolder/The Client Application.
 */
void
sync_buddy_with_simias_roster(gpointer key, gpointer value, gpointer user_data)
{
	GaimBuddy *buddy = (GaimBuddy *)value;
	
	g_print("Adding/Updating Buddy in Gaim Domain Roster: %s\n",
			gaim_buddy_get_alias(buddy));

	gaim_domain_add_gaim_buddy(
		gaim_account_get_username(buddy->account),
		gaim_account_get_protocol_id(buddy->account),
		buddy->name,
		gaim_buddy_get_alias(buddy),
		"",		/* FIXME: Get IP Address */
		"");	/* FIXME: Get IP Port */

	/**
	 * FIXME: Implement PSEUDOCODE in sync_buddy_with_simias_roster
	 * 
	 * PSEUDOCODE:
	 * 
	 * if (gaim buddy already exists in Simias Gaim Roster) {
	 * 		- Update the Simias Gaim Roster with any changes in the Buddy List
	 * 		- The Gaim Buddy List wins any conflicts
	 * 
	 * 		- If we already have an IP Address stored in Simias for this user,
	 * 		  send a [simias:ping-request] message so we can make sure that the
	 * 		  IP Address reflects what is current for any buddies who are
	 * 		  currently online.
	 * 
	 * 		- Check all the collections in Simias which this buddy may have been
	 * 		  added to and we've never sent an invitation for.  Send out a
	 * 		  [simias:invitation-request] message to the buddy for any that
	 * 		  match this condition.
	 * } else {
	 * 		- This is a buddy that has never been added to The Simias Gaim
	 * 		  Roster so add the buddy to the roster.
	 * }
	 */
}

char *
gaim_domain_get_po_box_id()
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetGaimPOBoxID req;
	struct _ns1__GetGaimPOBoxIDResponse resp;
	char *po_box_id = NULL;
	
	soap_url = get_soap_url(FALSE);
	if (!soap_url) {
		return NULL;
	}
	
	init_gsoap(&soap);
	soap_call___ns1__GetGaimPOBoxID(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return NULL;
	}
	
	/* Get the POBox ID */
	po_box_id = strdup(resp.GetGaimPOBoxIDResult);
	
	cleanup_gsoap(&soap);
	
	return po_box_id;
}

/* Sync Buddies to Simias Gaim Domain Roster */
int
sync_buddies_to_simias(GList *gaim_accounts)
{
	/**
	 * Loop through the buddies in every account and call:
	 *     gaim_domain_add_gaim_buddy()
	 */
	if (gaim_accounts) {
		return -1;
	}

	g_list_foreach(gaim_accounts, sync_account, NULL);
	
	return 0;
}

static void
sync_account(gpointer data, gpointer user_data)
{
}

static void
sync_buddy(gpointer data, gpointer user_data)
{
}

/* Utility functions for gSOAP */
static char *
get_soap_url(gboolean reread_config)
{
	char *url;
	char gaim_domain_url[512];
	int err;
	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (err == SIMIAS_SUCCESS) {
		sprintf(gaim_domain_url, "%s/GaimDomainService.asmx", url);
		free(url);
		the_soap_url = strdup(gaim_domain_url);
		/* FIXME: Figure out who and when this should ever be freed */
	}
	
	return NULL;
}

/**
 * gSOAP
 */
static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, simiasgaim_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}
