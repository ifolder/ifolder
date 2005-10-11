/* Sample Shell app for Simias APIs */


#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include "simias.h"

// Globals for shell environment
SimiasHandle hSimias;
SimiasNode	 hDomain;
SimiasNode	 hCurNode;
int level = 0;


int doList(void);
int doCD(char *name);


int doCD(char *name)
{
	int rc = 0;
	SimiasNodeList hNodeList;
	SimiasNode		hNewNode;

	printf("Changing into %s\n", name);
	if(strncmp(name, "..", 2) == 0)
	{
		if(level == 0)
			return 0;
		else if(level == 1)
		{
			printf("Level is one\n");
			if(hCurNode != NULL)
				simias_node_free(hCurNode);
			hCurNode = NULL;
			hDomain = NULL;
			level = 0;
			return 0;
		}
		else if(level == 2)
		{
			printf("Level is two\n");
			simias_node_free(hCurNode);
			hCurNode = hDomain;
			level = 1;
			return 0;
		}
	}


	// if level is 0, we are at the top
	if(level == 0)
	{
		printf("Getting domain list...\n");
		rc = simias_get_domains(hSimias, &hNodeList);
		if(rc)
			return rc;
	}
	else if(level == 1)
	{
		rc = simias_get_collections_for_domain_by_type(hSimias, 
											   &hNodeList, 
											   simias_node_get_id(hCurNode),
												"Collection");
		if(rc)
			return rc;
	}
	else if(level == 2)
	{
		printf("Error: the object %s has no containment\n", 
					simias_node_get_name(hCurNode));
		return 0;
	}


	rc = simias_nodelist_extract_node_by_name(hNodeList,
											  &hNewNode,
											  name);
	if(rc)
	{
		printf("Error extracting node by name\n");
		simias_nodelist_free(&hNodeList);
		return rc;
	}

	if(hNewNode != NULL)
	{
		if(level == 1)
			hDomain = hCurNode;
		else if(level != 0)
			simias_node_free(hCurNode);
		level++;
		hCurNode = hNewNode;
	}

	rc = simias_nodelist_free(&hNodeList);
	if(rc)
		return rc;
}



int doList(void)
{
	int rc = 0;
	SimiasNodeList hNodeList;
	int nodeCounter, nodeCount = 0;

	if(level == 0)
	{
		rc = simias_get_domains(hSimias, &hNodeList);
		if(rc)
			return rc;
	}
	else if(level == 1)
	{
		rc = simias_get_collections_for_domain(hSimias, 
											   &hNodeList, 
											   simias_node_get_id(hCurNode));
		if(rc)
			return rc;
	}
	else
	{
		rc = simias_get_nodes(hSimias, 
								&hNodeList, 
								simias_node_get_id(hCurNode));
		if(rc)
			return rc;
	}

	rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
	if(rc)
	{
		simias_nodelist_free(&hNodeList);
		return rc;
	}

	for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
	{
		SimiasNode hNode;

		rc = simias_nodelist_extract_node(hNodeList, &hNode,
												nodeCounter);
		if(rc)
		{
			simias_nodelist_free(&hNodeList);
			return rc;
		}

		printf("%s\n", simias_node_get_name(hNode));

		rc = simias_node_free(&hNode);
		if(rc)
		{
			simias_nodelist_free(&hNodeList);
			return rc;
		}
	}

	rc = simias_nodelist_free(&hNodeList);
	if(rc)
		return rc;
}


int shell_prompt()
{
	char buffer[256];
	bool reprompt = true;

	while(reprompt)
	{
		if(hCurNode == NULL)
			printf("simias [root]: ");
		else
			printf("simias [%s]: ", simias_node_get_name(hCurNode));

		memset(buffer, 0, 256);
		fgets(buffer, 256, stdin);

		if(strncmp(buffer, "ls", 2) == 0)
			doList();
		if(strncmp(buffer, "cd", 2) == 0)
		{
			char *tmpName;
			tmpName = strchr(buffer, ' ');
			if(tmpName != NULL)
			{
				char *name;
				tmpName = ++tmpName;
				name = strchr(tmpName, '\r');
				if(name != NULL)
					*name = 0;
				name = strchr(tmpName, '\n');
				if(name != NULL)
					*name = 0;
				name = tmpName;
				doCD(name);
			}
		}
		else if(strncmp(buffer, "quit", 4) == 0)
			reprompt = false;
		else if(strncmp(buffer, "q", 1) == 0)
			reprompt = false;
	}
}


int main(int argc, char **argv)
{
	printf("Connecting to simias...");
	hSimias = NULL;
	hDomain = NULL;
	hCurNode = NULL;
	level = 0;

	int rc = simias_init_local(&hSimias);
	if(rc)
		return rc;

	printf("done\n");
	shell_prompt();

	rc = simias_free(&hSimias);
	if(rc)
	{
		printf("Error simias_free: %d\n", rc);
	}
	
	return 0;
}



