// ifcli.cpp : Defines the entry point for the console application.
//

#include <stdio.h>
#include <string.h>

#include <glib.h>
#include "IFApplication.h"
#include "IFiFolder.h"
#include "IFOptions.h"

int DomainHandler(PCommand command, char* extra[]);
int iFolderHandler(PCommand command, char* extra[]);
int SyncHandler(PCommand command, char* extra[]);
int ConflictHandler(PCommand command, char* extra[]);
int LogHandler(PCommand command, char* extra[]);


Option DomainOptions[] = 
{
	{NULL, "login", "DOMAIN_NAME", "Login to domain", NULL, false},
	{NULL, "logout", "DOMAIN_NAME", "Logout of domain", NULL, false},
	{"a", "add", "URL", "Connect to domain", NULL, false},
	{"r", "remove", "DOMAIN_NAME", "Disconnect from domain", NULL, false},
	{"l", "list", NULL, "List the connected domains", NULL, false},
	{"u", "user", "NAME", "User Name", NULL, false},
	{"p", "password", "PASSWORD", "Password", NULL, false},
	{"v", "view", "NAME", "Get the details of the Domain.", NULL, false},
};
#define DomainOptionsCount sizeof(DomainOptions)/sizeof(Option)

Option iFolderOptions[] = 
{
	{"s", "subscribe", "ID", "Subscribe to the iFolder", NULL, false},
	{"c", "create", "PATH", "Create an iFolder to the specified path.", NULL, false},
	{"r", "revert", "PATH", "Revert to a normal iFolder", NULL, false},
	{"l", "list", NULL, "List the subscribed iFolders", NULL, false},
	{"d", "domain", "NAME", "The domain for the iFolder", NULL, false},
	{"v", "view", "PATH", "Get the iFolder details.", NULL, false},
};
#define iFolderOptionsCount sizeof(iFolderOptions)/sizeof(Option)

Option SyncOptions[] = 
{
	{"b", "block", NULL, "Wait for Synchronization to complete.", NULL, false},
	{"n", "now", NULL, "Synchronize now", NULL, false},
	{"a", "all", NULL, "Syncronize all iFolders", NULL, false},
	{"f", "ifolder", "NAME", "The ifolder to synchronize", NULL, false},
};
#define SyncOptionsCount sizeof(SyncOptions)/sizeof(Option)

Option ConflictOptions[] = 
{
	{"r", "resolve", "PATH", "Resolve the file conflict.", NULL, false},
	{"l", "list", NULL, "List the conflicts.", NULL, false},
	{"s", "server-copy", NULL, "Keep the server's copy.", NULL, false},
	{"f", "ifolder", "NAME", "The ifolder to synchronize", NULL, false},
	{"v", "view", "PATH", "Get the details of the conflict.", NULL, false},
};
#define ConflictOptionsCount sizeof(ConflictOptions)/sizeof(Option)

Option LogOptions[] = 
{
	{"d", "debug", "YES|NO", "Enable/Disable Debug logging.", NULL, false},
	{"l", "level", "NULL", "List the conflicts.", NULL, false},
	{"s", "server-copy", NULL, "Keep the server's copy.", NULL, false},
	{"f", "ifolder", "NAME", "The ifolder to synchronize", NULL, false},
	{"v", "view", "PATH", "Get the details of the conflict.", NULL, false},
};
#define LogOptionsCount sizeof(LogOptions)/sizeof(Option)

Command Commands[] =
{
	{"domain", DomainOptionsCount, DomainOptions, &DomainHandler},
	{"ifolder", iFolderOptionsCount, iFolderOptions, &iFolderHandler},
	{"sync", SyncOptionsCount, SyncOptions, &SyncHandler},
	{"conflict", ConflictOptionsCount, ConflictOptions, &ConflictHandler},
	{"log", LogOptionsCount, LogOptions, &LogHandler},
};
#define CommandCount sizeof(Commands)/sizeof(Command)



int main(int argc, char* argv[])
{
	if (argc == 1)
	{
		ShowUsage(argv[0], CommandCount, Commands, false);
		return 0;
	}

	if (ParseCommand(argc, argv, CommandCount, Commands) != 0)
	{
		return -1;
	}
	
	int loopCount = 1;
	gchar *pPath = NULL;
	
	switch (argc)
	{
	case 3:
		sscanf(argv[2], "%d", &loopCount);
	case 2:
		pPath = argv[1];
		break;
	default:
		printf("Usage : %s {path} [loopcount]", argv[0]);
	}
	
	IFApplication::Initialize();
	gchar c;
	printf("Hit Enter to continue\n");
	scanf("%c", &c);
			
	for (int i = 0; i < loopCount; ++i)
	{
		g_debug("Loop %d", loopCount);
		IFiFolder *piFolder = new IFiFolder(argv[1]);
		piFolder->Sync();
		delete piFolder;
	}
	printf("Hit Enter to continue\n");
	scanf("%c", &c);
}

void PrintEnteredOptions(PCommand command, char* extra[])
{
	int i = 0;
	for (i; i < command->OptionCount; ++i)
	{
		Option *pOption = &command->Options[i];
		if (pOption->Set)
			printf("%s%c%s\n", pOption->LongName == NULL ? pOption->ShortName : pOption->LongName, 
			pOption->Value == NULL ? '\0' : '=', 
			pOption->Value == NULL ? "" : pOption->Value);
	}

	i = 0;
	while (extra[i] != NULL)
		printf("%s ", extra[i++]);
	printf("\n");
}

int DomainHandler(PCommand command, char* extra[])
{
	PrintEnteredOptions(command, extra);
	return 0;
}

int iFolderHandler(PCommand command, char* extra[])
{
	PrintEnteredOptions(command, extra);
	return 0;
}

int SyncHandler(PCommand command, char* extra[])
{
	PrintEnteredOptions(command, extra);
	return 0;
}
int ConflictHandler(PCommand command, char* extra[])
{
	PrintEnteredOptions(command, extra);
	return 0;
}
int LogHandler(PCommand command, char* extra[])
{
	PrintEnteredOptions(command, extra);
	return 0;
}

