/* Test app for Simias APIs */
/* right now this is code to test the socket interface into Simias */



#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>

#include <sys/types.h>
#include <sys/timeb.h>

#include "simias.h"

#define TEST_LOOP_COUNT 20

/**
 *	Simias Handle tests
 */
bool simiasHandleTests()
{
	SimiasHandle sHandle;
	int rc = 0;
	int counter = 0;
	bool passed = true;

	for(counter = 0; counter < TEST_LOOP_COUNT; counter++)
	{
		rc = simias_init_local(&sHandle);
		if(!rc)
		{
			rc = simias_ping(sHandle);
			if(rc)
			{
				passed = false;
				break;
			}

			rc = simias_free(&sHandle);
			if(!passed)
			{
				printf("simias_ping failed\n");
				break;
			}
		}
	}

	return passed;
}



/**
 *	Simias Domain tests
 */
bool simiasDomainTests()
{
	SimiasHandle hSimias;
	int rc = 0;
	int counter;
	bool passed = true;

	rc = simias_init_local(&hSimias);
	if(rc)
	{
		printf("Error simias_init_local: %d\n", rc);
		return false;
	}

	for(counter = 0; counter < TEST_LOOP_COUNT; counter++)
	{
		SimiasNodeList hNodeList;
		int nodeCounter, nodeCount = 0;

		rc = simias_get_domains(hSimias, &hNodeList);
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}

		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNode hNode;

			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}
	}

	rc = simias_free(&hSimias);
	if(rc)
	{
		printf("Error simias_free: %d\n", rc);
		passed = false;
	}

	return passed;
}




/**
 *	Simias Collection tests
 */
bool simiasCollectionTests()
{
	SimiasHandle hSimias;
	int rc = 0;
	int counter;
	bool passed = true;

	rc = simias_init_local(&hSimias);
	if(rc)
	{
		printf("Error simias_init_local: %d\n", rc);
		return false;
	}

	for(counter = 0; counter < TEST_LOOP_COUNT; counter++)
	{
		SimiasNodeList hNodeList;
		int nodeCounter, nodeCount = 0;


		// GetCollections

		rc = simias_get_collections(hSimias, &hNodeList);
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}
			
		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNode hNode;
			
			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}


		// GetCollections by Type

		rc = simias_get_collections_by_type(hSimias, 
											&hNodeList,
											"iFolder");
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}

		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNode hNode;

			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}


		// GetCollections for domain


		rc = simias_get_domains(hSimias, &hNodeList);
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}

		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNodeList hNodeList2;
			int nodeCounter2, nodeCount2 = 0;
			SimiasNode hNode;

			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_get_collections_for_domain(hSimias, &hNodeList2, 
												   simias_node_get_id(hNode));
			if(rc)
			{
				printf("Error simias_get_collections: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_nodelist_get_node_count(hNodeList2, &nodeCount2);
			if(rc)
			{
				printf("Error simias_nodelist_get_node_count: %d\n", rc);
				passed = false;
				break;
			}

			for(nodeCounter2 = 0; nodeCounter2 < nodeCount2; nodeCounter2++)
			{
				SimiasNode hNode2;

				rc = simias_nodelist_extract_node(hNodeList2, &hNode2,
														nodeCounter2);
				if(rc)
				{
					printf("Error simias_nodelist_extract_node: %d\n", rc);
					passed = false;
					break;
				}

				rc = simias_node_free(&hNode2);
				if(rc)
				{
					printf("Error simias_node_free: %d\n", rc);
					passed = false;
					break;
				}
			}

			rc = simias_nodelist_free(&hNodeList2);
			if(rc)
			{
				printf("Error simias_nodelist_free: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}




		// GetCollections for domain by type


		rc = simias_get_domains(hSimias, &hNodeList);
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}

		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNodeList hNodeList2;
			int nodeCounter2, nodeCount2 = 0;
			SimiasNode hNode;

			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_get_collections_for_domain_by_type(hSimias, &hNodeList2, 
													simias_node_get_id(hNode),
													"iFolder");
			if(rc)
			{
				printf("Error simias_get_collections: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_nodelist_get_node_count(hNodeList2, &nodeCount2);
			if(rc)
			{
				printf("Error simias_nodelist_get_node_count: %d\n", rc);
				passed = false;
				break;
			}

			for(nodeCounter2 = 0; nodeCounter2 < nodeCount2; nodeCounter2++)
			{
				SimiasNode hNode2;

				rc = simias_nodelist_extract_node(hNodeList2, &hNode2,
														nodeCounter2);
				if(rc)
				{
					printf("Error simias_nodelist_extract_node: %d\n", rc);
					passed = false;
					break;
				}

				rc = simias_node_free(&hNode2);
				if(rc)
				{
					printf("Error simias_node_free: %d\n", rc);
					passed = false;
					break;
				}
			}

			rc = simias_nodelist_free(&hNodeList2);
			if(rc)
			{
				printf("Error simias_nodelist_free: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}
	}

	rc = simias_free(&hSimias);
	if(rc)
	{
		printf("Error simias_free: %d\n", rc);
		passed = false;
	}

	return passed;
}




/**
 *	Simias Node tests
 */
bool simiasNodeTests()
{
	SimiasHandle hSimias;
	int rc = 0;
	int counter;
	bool passed = true;

	rc = simias_init_local(&hSimias);
	if(rc)
	{
		printf("Error simias_init_local: %d\n", rc);
		return false;
	}

	for(counter = 0; counter < TEST_LOOP_COUNT; counter++)
	{
		SimiasNodeList hNodeList;
		int nodeCounter, nodeCount = 0;


		// GetNodes (from all collections)

		rc = simias_get_collections(hSimias, &hNodeList);
		if(rc)
		{
			printf("Error simias_get_collections: %d\n", rc);
			passed = false;
			break;
		}

		rc = simias_nodelist_get_node_count(hNodeList, &nodeCount);
		if(rc)
		{
			printf("Error simias_nodelist_get_node_count: %d\n", rc);
			passed = false;
			break;
		}

			
		for(nodeCounter = 0; nodeCounter < nodeCount; nodeCounter++)
		{
			SimiasNodeList hNodeList2;
			int nodeCounter2, nodeCount2 = 0;
			SimiasNode hNode;

			rc = simias_nodelist_extract_node(hNodeList, &hNode,
													nodeCounter);
			if(rc)
			{
				printf("Error simias_nodelist_extract_node: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_get_nodes(hSimias, 
								  &hNodeList2,
								  simias_node_get_id(hNode) );
			if(rc)
			{
				printf("Error simias_get_nodes: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_nodelist_get_node_count(hNodeList2, &nodeCount2);
			if(rc)
			{
				printf("Error simias_nodelist_get_node_count: %d\n", rc);
				passed = false;
				break;
			}

			for(nodeCounter2 = 0; nodeCounter2 < nodeCount2; nodeCounter2++)
			{
				SimiasNode hNode2;
				int propertyCount, pCounter;

				rc = simias_nodelist_extract_node(hNodeList2, &hNode2,
														nodeCounter2);
				if(rc)
				{
					printf("Error simias_nodelist_extract_node: %d\n", rc);
					passed = false;
					break;
				}

				propertyCount = simias_property_get_count(hNode2);
		//		printf("Properties on Node: %d\n", propertyCount);

				for(pCounter = 0; pCounter < propertyCount; pCounter++)
				{
					SimiasProperty hProperty;

					rc = simias_property_extract_property(hNode2, 
														&hProperty,
														pCounter);
					if(rc)
					{
						printf("Error simias_property_extract_property: %d\n", rc);
						passed = false;
						break;
					}

					rc = simias_property_free(&hProperty);
					if(rc)
					{
						printf("Error simias_property_free: %d\n", rc);
						passed = false;
						break;
					}
				}

				rc = simias_node_free(&hNode2);
				if(rc)
				{
					printf("Error simias_node_free: %d\n", rc);
					passed = false;
					break;
				}
			}

			rc = simias_nodelist_free(&hNodeList2);
			if(rc)
			{
				printf("Error simias_nodelist_free: %d\n", rc);
				passed = false;
				break;
			}

			rc = simias_node_free(&hNode);
			if(rc)
			{
				printf("Error simias_node_free: %d\n", rc);
				passed = false;
				break;
			}
		}

		rc = simias_nodelist_free(&hNodeList);
		if(rc)
		{
			printf("Error simias_nodelist_free: %d\n", rc);
			passed = false;
			break;
		}
	}

	rc = simias_free(&hSimias);
	if(rc)
	{
		printf("Error simias_free: %d\n", rc);
		passed = false;
	}

	return passed;
}







int main(int argc, char **argv)
{
	bool	passedTest = true;
	bool	allTests = true;
	
//	struct timeb startTime;
//	struct timeb stopTime;

	printf("Test: simiasHandleTests()\n");
	passedTest = simiasHandleTests();
	if(passedTest)
		printf("Test: simiasHandleTests() - PASS\n");
	else
	{
		printf("Test: simiasHandleTests() - FAILED\n");
		allTests = false;
	}


	printf("Test: simiasDomainTests()\n");
	passedTest = simiasDomainTests();
	if(passedTest)
		printf("Test: simiasDomainTests() - PASS\n");
	else
	{
		printf("Test: simiasDomainTests() - FAILED\n");
		allTests = false;
	}


	printf("Test: simiasCollectionTests()\n");
	passedTest = simiasCollectionTests();
	if(passedTest)
		printf("Test: simiasCollectionTests() - PASS\n");
	else
	{
		printf("Test: simiasCollectionTests() - FAILED\n");
		allTests = false;
	}

	printf("Test: simiasNodeTests()\n");
	passedTest = simiasNodeTests();
	if(passedTest)
		printf("Test: simiasNodeTests() - PASS\n");
	else
	{
		printf("Test: simiasNodeTests() - FAILED\n");
		allTests = false;
	}


	printf("\n");
	printf("-------------------------------------------------------------\n");
	if(allTests)
		printf(" All Simias Tests Pass\n");
	else
		printf("One or more Simias Tests Failed\n");
	printf("-------------------------------------------------------------\n");
	
	return 0;
}



