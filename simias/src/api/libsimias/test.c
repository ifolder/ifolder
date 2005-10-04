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
				printf("Simias Ping %d:%d\n", counter, counter2);
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

	return passed;
}


int main(int argc, char **argv)
{
	bool	passedTest = true;
	bool	allTests = true;
	
	struct timeb startTime;
	struct timeb stopTime;

	printf("Test: simiasHandleTests()\n");
	passedTest = simiasHandleTests();
	if(passedTest)
		printf("Test: simiasHandleTests() - PASS\n");
	else
	{
		printf("Test: simiasHandleTests() - FAILED\n");
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



