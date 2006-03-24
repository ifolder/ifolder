#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include <ifolder-client.h>

static int print_syntax(void);

static int run_trayapp(void);
static void run_internal_test(void);
static void run_ipc_test(void);

static void run_api_tests(void);

static int passes = 0;
static int fails  = 0;

int main(int argc, char *argv[])
{
	int err;
	int i;
	
	if (argc != 2)
		return print_syntax();
	else
	{
		/**
		 * Determine whether to:
		 *	1. Run as the TrayApp (do nothing and wait for IPC clients)
		 *	2. Run tests as the TrayApp (exercise the "internal" libifolder calls)
		 *	3. Run tests as an IPC Client (exercise the "IPC" libifolder calls)
		 */
		if (strcmp(argv[1], "trayapp") == 0)
			return run_trayapp();
		else if (strcmp(argv[1], "internal") == 0)
			run_internal_test();
		else if (strcmp(argv[1], "ipc") == 0)
			run_ipc_test();
		else
			return print_syntax();
	}

	printf("Test complete:\n");
	printf("\tPasses: %d\n", passes);
	printf("\tFails:  %d\n", fails);
	
	if (fails > 0)
		return 0 - fails;

	return 0;
}

int
print_syntax(void)
{
	printf("\nSyntax: testlibifolder <trayapp|internal|ipc>\n\n");
	printf("\ttrayapp  - Run as the TrayApp (do nothing and wait\n");
	printf("\t           for IPC clients)\n");
	printf("\tinternal - Run tests as the TrayApp (exercise the\n");
	printf("\t           \"internal\" libifolder API)\n");
	printf("\tipc      - Run tests as an IPC Client (exercise the\n");
	printf("\t           \"IPC\" libifolder API)\n\n");
		
	return -1;
}

int
run_trayapp(void)
{
	int err;
	
	err = ifolder_client_initialize(true);
	if (err != IFOLDER_SUCCESS)
		return err;
	
	printf("Running as TrayApp.  You can connect IPC clients now.\n\n");
	printf("Press <ENTER> to quit.\n");
	getchar();
	
	return ifolder_client_uninitialize();
}

void
run_internal_test(void)
{
	int err;
	
	err = ifolder_client_initialize(true);
	if (err != IFOLDER_SUCCESS)
	{
		printf("ifolder_client_initialize(true) returned an error: %d\n", err);
		return;
	}
	
	printf("Running internal API tests...\n");
	
	run_api_tests();
	
	ifolder_client_uninitialize();
}

void
run_ipc_test(void)
{
	int err;
	
	err = ifolder_client_initialize(false);
	if (err != IFOLDER_SUCCESS)
	{
		printf("ifolder_client_initialize(false) returned an error: %d\n", err);
		return;
	}
	
	printf("Running IPC API tests...\n");
	
	run_api_tests();
	
	ifolder_client_uninitialize();
}

void
run_api_tests(void)
{
	int err;
	iFolderAccount account;
	
	/* Add an account */
	printf("Adding an account...");
	err = ifolder_account_add("192.168.1.123", "btimothy", "mypassword", true, &account);
	if (err != IFOLDER_SUCCESS)
	{
		fails++;
		printf("Failed! %d\n", err);
	}
	else
	{
		passes++;
		printf("Done!\n");
		printf("Account: %s\n", ifolder_account_get_id(account));
		printf("            Name: %s\n", ifolder_account_get_name(account));
		printf("     Description: %s\n", ifolder_account_get_description(account));
		printf("         Version: %s\n", ifolder_account_get_version(account));
		printf("    Host Address: %s\n", ifolder_account_get_host_address(account));
		printf("    Machine Name: %s\n", ifolder_account_get_machine_name(account));
		printf("       OS Version: %s\n", ifolder_account_get_os_version(account));
		printf("        User Name: %s\n", ifolder_account_get_user_name(account));
		printf("       Is Default: %s\n", ifolder_account_is_default(account) ? "Yes" : "No");
		printf("        Is Active: %s\n", ifolder_account_is_active(account) ? "Yes" : "No");
		
		err = ifolder_account_release(&account);
		if (err == IFOLDER_SUCCESS)
			passes++;
		else
		{
			fails++;
			printf("Failed to release memory used by iFolderAccount: %d\n", err);
		}
	}
}
