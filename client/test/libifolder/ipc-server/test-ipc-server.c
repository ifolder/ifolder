#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include <ifolder-client.h>

static int run_trayapp(void);

int main(int argc, char *argv[])
{
	return run_trayapp();
}

int
run_trayapp(void)
{
	int err;
	
	err = ifolder_client_initialize();
	if (err != IFOLDER_SUCCESS)
		return err;
	
	printf("Running as TrayApp.  You can connect IPC clients now.\n\n");
	printf("Press <ENTER> to quit.\n");
	getchar();
	
	return ifolder_client_uninitialize();
}

