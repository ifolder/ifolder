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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/
#include "simias.h"
#include "simiasweb.h"

#include <simiasStub.h>
#include <simias.nsmap>

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef DEBUG
#define SIMIAS_DEBUG(args) (printf("libsimias: "), printf args)
#else
#define SIMIAS_DEBUG
#endif

#if defined(WIN32)
#define DIR_SEP "\\"
#else
#define DIR_SEP "/"
#endif

/* Global Variables */
static char *the_soap_url = NULL;

/* Foward Declarations */
static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(bool reread_config);

/* Function Implementations */
int
simias_get_local_service_url(char **url)
{
	char user_profile_dir[1024];
	char simias_config_file_path[1024];
	FILE *simias_conf_file;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return SIMIAS_ERROR_NO_USER_PROFILE;
	}

	SIMIAS_DEBUG((stderr, "User Profile Dir: %s\n", user_profile_dir));

	sprintf(simias_config_file_path, "%s%sSimias.config",
			user_profile_dir, DIR_SEP);
	
	SIMIAS_DEBUG((stderr, "Simias Config File: %s\n", simias_config_file_path));

	/* Attempt to open the file */
	simias_conf_file = fopen(simias_config_file_path, "r");
	if (!simias_conf_file) {
		SIMIAS_DEBUG((stderr, "Error opening \"%s\"\n", simias_config_file_path));
		return SIMIAS_ERROR_OPENING_CONFIG_FILE;
	}

	*url = parse_local_service_url(simias_conf_file);

	fclose(simias_conf_file);
	
	if (!(*url)) {
		SIMIAS_DEBUG((stderr, "Couldn't find Local Service URL in \"%s\"\n",
					 simias_config_file_path));
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	return SIMIAS_SUCCESS;
}


/******************************************************************************
 * gSOAP Wrappers for WebService Calls                                        *
 ******************************************************************************/
/* Wrapper for GetDomains */
int
simias_get_domains(bool only_slaves, SimiasDomainInfo **ret_domainsA[])
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetDomains req;
	struct _ns1__GetDomainsResponse resp;
	SimiasDomainInfo **domainInfosA;
	SimiasDomainInfo *domain;
	int num_of_domains = 0;
	int i = 0;
	struct ns1__ArrayOfDomainInformation *array_of_domain_infos;
	struct ns1__DomainInformation **domainsA;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(true);
	if (!soap_url) {
		return -1;
	}

	/* Setup the Request */
	req.onlySlaves = only_slaves ? true_ : false_;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
		soap.userid = username;
		soap.passwd = password;
	}
	soap_call___ns1__GetDomains(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return SIMIAS_ERROR_IN_SOAP_CALL;
	}

	/* Allocate memory to return the domain information in */
	array_of_domain_infos = resp.GetDomainsResult;
	if (array_of_domain_infos) {
		num_of_domains = array_of_domain_infos->__sizeDomainInformation;
		if (num_of_domains >= 0) {
			domainInfosA = malloc(sizeof(SimiasDomainInfo *)
									 * (num_of_domains + 1));
			if (!domainInfosA) {
				/* Out of Memory error */
				cleanup_gsoap(&soap);
				return SIMIAS_ERROR_OUT_OF_MEMORY;
			}
			
			domainsA = array_of_domain_infos->DomainInformation;
			
			/* Populate the memory */
			for (i = 0; i < num_of_domains; i++) {
				/* Malloc a new SimiasDomainInfo */
				domain = malloc(sizeof(SimiasDomainInfo));
				if (!domain) {
					/* Out of Memory Error */
					return SIMIAS_ERROR_OUT_OF_MEMORY;
				}
				/* Type */
				switch (domainsA[i]->Type) {
					case ns1__DomainType__Master:
						domain->type = SIMIAS_DOMAIN_TYPE_MASTER;
						break;
					case ns1__DomainType__Slave:
						domain->type = SIMIAS_DOMAIN_TYPE_SLAVE;
						break;
					case ns1__DomainType__Local:
						domain->type = SIMIAS_DOMAIN_TYPE_LOCAL;
						break;
					case ns1__DomainType__None:
					default:
						domain->type = SIMIAS_DOMAIN_TYPE_NONE;
				}
				
				/* Active */
				if (domainsA[i]->Active == true_) {
					domain->active = true;
				} else {
					domain->active = false;
				}

				/* Name */
				if (domainsA[i]->Name)
					domain->name = strdup(domainsA[i]->Name);
				else
					domain->name = strdup("");
				
				/* Description */
				if (domainsA[i]->Description)
					domain->description = strdup(domainsA[i]->Description);
				else
					domain->description = strdup("");

				/* ID */
				if (domainsA[i]->ID)
					domain->id = strdup(domainsA[i]->ID);
				else
					domain->id = strdup("");

				/* MemberUserID */
				if (domainsA[i]->MemberUserID)
					domain->member_user_id = strdup(domainsA[i]->MemberUserID);
				else
					domain->member_user_id = strdup("");

				/* MemberName */
				if (domainsA[i]->MemberName)
					domain->member_name = strdup(domainsA[i]->MemberName);
				else
					domain->member_name = strdup("");

				/* RemoteUrl */
				if (domainsA[i]->RemoteUrl)
					domain->remote_url = strdup(domainsA[i]->RemoteUrl);
				else
					domain->remote_url = strdup("");

				/* POBoxID */
				if (domainsA[i]->POBoxID)
					domain->po_box_id = strdup(domainsA[i]->POBoxID);
				else
					domain->po_box_id = strdup("");

				/* Host */
				if (domainsA[i]->Host)
					domain->host = strdup(domainsA[i]->Host);
				else
					domain->host = strdup("");
				
				/* IsSlave */
				if (domainsA[i]->IsSlave == true_) {
					domain->is_slave = true;
				} else {
					domain->is_slave = false;
				}
				
				/* IsDefault */
				if (domainsA[i]->IsDefault == true_) {
					domain->is_default = true;
				} else {
					domain->is_default = false;
				}
				
				/* Add this to the Array */
				domainInfosA[i] = domain;
			}

			/* NULL-terminate domainInfosA */
			domainInfosA[i] = 0x0;
			
			*ret_domainsA = domainInfosA;
		}
	} else {
		printf("array_of_domain_infos is NULL\n");
	}
	
	cleanup_gsoap(&soap);
	
	return SIMIAS_SUCCESS;
}

int
simias_free_domains(SimiasDomainInfo **domainsA[])
{
	SimiasDomainInfo *curr_domain;
	int i = 0;
	
	if (!*domainsA) {
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	curr_domain = (*domainsA)[i];
	while (curr_domain) {
		/* First free all the char *'s */
		free(curr_domain->name);
		free(curr_domain->description);
		free(curr_domain->id);
		free(curr_domain->member_user_id);
		free(curr_domain->member_name);
		free(curr_domain->remote_url);
		free(curr_domain->po_box_id);
		free(curr_domain->host);
		free(curr_domain);
		
		curr_domain = (*domainsA)[++i];
	}
	
	free(*domainsA);

	return SIMIAS_SUCCESS;
}


/**
 * gSOAP
 */
static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, simias_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}

static char *
get_soap_url(bool reread_config)
{
	char *url;
	char simias_domain_url[512];
	int err;
	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (err == SIMIAS_SUCCESS) {
		sprintf(simias_domain_url, "%s/Simias.asmx", url);
		free(url);
		if (the_soap_url)
			free(the_soap_url);
		the_soap_url = strdup(simias_domain_url);
		/* FIXME: Figure out who and when this should ever be freed */
	} else {
		printf("simias_get_local_service_url() returned: %d\n", err);
	}
	
	return the_soap_url;
}
