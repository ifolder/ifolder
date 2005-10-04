/* Test app for Simias APIs */
/* right now this is code to test the socket interface into Simias */



#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>

#include <sys/types.h>
#include <sys/timeb.h>

#include "simias.h"


/**
 *	Simias Handle tests
 */
bool simiasHandleTests()
{
	SimiasHandle sHandle;
	int rc = 0;
	int counter, counter2 = 0;
	bool passed = true;

	for(counter = 0; counter < 20; counter++)
	{
		rc = simias_init_local(&sHandle);
		if(!rc)
		{
			for(counter2 = 0; counter2 < 200; counter2++)
			{
				printf("simias ping: %d:%d\n", counter, counter2);
				rc = simias_ping(sHandle);
				if(rc)
				{
					passed = false;
					break;
				}
			}
			rc = simias_free(&sHandle);
			if(!passed)
				break;
		}
	}
	printf("\n");

	return passed;
}



/**
 *	Simias Handle tests
 */
bool simiasDomainTests()
{
	SimiasHandle hSimias;
	int rc = 0;
	int counter;
	bool passed = true;

	rc = simias_init_local(&hSimias);
	if(!rc)
	{
		for(counter = 0; counter < 20000; counter++)
		{
			SimiasNodeList hNodeList;

			rc = simias_get_domains(hSimias, &hNodeList);
			if(!rc)
			{
				rc = simias_nodelist_free(&hNodeList);
			}

			printf("simias_get_domains: %d\n", counter);

			if(rc)
			{
				passed = false;
				break;
			}
		}
		rc = simias_free(&hSimias);
	}

	printf("\n");

	return passed;
}


int main(int argc, char **argv)
{
	bool	passedTest = true;
	bool	allTests = true;
	
	struct timeb startTime;
	struct timeb stopTime;
/*

	printf("Test: simiasHandleTests()\n");
	passedTest = simiasHandleTests();
	if(passedTest)
		printf("Test: simiasHandleTests() - PASS\n");
	else
	{
		printf("Test: simiasHandleTests() - FAILED\n");
		allTests = false;
	}
*/
	printf("Test: simiasDomainTests()\n");
	passedTest = simiasDomainTests();
	if(passedTest)
		printf("Test: simiasDomainTests() - PASS\n");
	else
	{
		printf("Test: simiasDomainTests() - FAILED\n");
		allTests = false;
	}

	printf("\n\n");
	printf("-------------------------------------------------------------\n");
	if(allTests)
		printf(" All Simias Tests Pass\n");
	else
		printf("One or more Simias Tests Failed\n");
	printf("-------------------------------------------------------------\n");
	
	return 0;
}



