// ifcli.cpp : Defines the entry point for the console application.
//

#include <stdio.h>
#include <string.h>

#include <glib.h>
#include "IFApplication.h"
#include "IFDomain.h"
#include "IFiFolder.h"
#include "IFOptions.h"

int DomainHandler(PCommand command, char* extra[]);
int iFolderHandler(PCommand command, char* extra[]);
int SyncHandler(PCommand command, char* extra[]);
int ConflictHandler(PCommand command, char* extra[]);
int LogHandler(PCommand command, char* extra[]);

static gchar* DomainName = NULL;
static gboolean iFolderName = NULL;
static gboolean LogCmd = false;

static GOptionEntry AppOptions[] = 
{
	{"domain", 'd', 0, G_OPTION_ARG_STRING, &DomainName, "Domain to operate on.", "DOMAIN_NAME"},
	{"ifolder", 'i', 0, G_OPTION_ARG_STRING, &iFolderName, "iFolder to operate on", "IFOLDER_NAME"},
	{"log", 'l', 0, G_OPTION_ARG_NONE, &LogCmd, "Logging Operation", NULL},
	{ NULL }
};

static gboolean doLogin = false;
static gboolean doLogout = false;
static gboolean doAdd = false;
static gboolean doRemove = false;
static gboolean doList = false;
static gboolean doView = false;
static gchar* HostName = NULL;
static gchar* UserName = NULL;
static gchar* Password = NULL;

static GOptionEntry DomainOptions[] =
{
	{"login", 'i', 0, G_OPTION_ARG_NONE, &doLogin, "Login to the domain", NULL},
	{"logout", 'o', 0, G_OPTION_ARG_NONE, &doLogout, "Logout of the domain", NULL},
	{"add", 'a', 0, G_OPTION_ARG_NONE, &doAdd, "Add domain to known domains", NULL},
	{"remove", 'r', 0, G_OPTION_ARG_NONE, &doRemove, "Remove domain from known domains", NULL},
	{"list", 'l', 0, G_OPTION_ARG_NONE, &doList, "List the domains that have been added.", NULL},
	{"view", 'v', 0, G_OPTION_ARG_NONE, &doView, "Get the details of the Domain.", NULL},
	{"host", 'h', 0, G_OPTION_ARG_STRING, &HostName, "Host | Host:port", "HOST"},
	{"user", 'u', 0, G_OPTION_ARG_STRING, &UserName, "User Name", "USER_NAME"},
	{"password", 'p', 0, G_OPTION_ARG_STRING, &Password, "Password", "PASSWORD"},
	{ NULL }
};

static gboolean ifoSubscribe = false;
static gboolean ifoCreate = false;
static gboolean ifoRemove = false;
static gboolean ifoList = false;
static gboolean ifoView = false;
static gchar* iFolderID = NULL;

static GOptionEntry iFolderOptions[] = 
{
	{"subscribe", 's', 0, G_OPTION_ARG_NONE, &ifoSubscribe, "Subscribe to the iFolder", NULL},
	{"create", 'c', 0, G_OPTION_ARG_NONE, &ifoCreate, "Create an iFolder to the specified path.", NULL},
	{"revert", 'r', 0, G_OPTION_ARG_NONE, &ifoRemove, "Revert to a normal iFolder", NULL},
	{"list", 'l', 0, G_OPTION_ARG_NONE, &ifoList, "List the subscribed iFolders", NULL},
	{"view", 'v', 0, G_OPTION_ARG_NONE, &ifoView, "Get the iFolder details.", NULL},
	{"domain", 'd', 0, G_OPTION_ARG_STRING, &DomainName, "Domain to which the ifolder belongs.", "DOMAIN_NAME"},
	{"name", 'n', 0, G_OPTION_ARG_STRING, &iFolderName, "Name of the iFolder.", "IFOLDER_NAME"},
	{"id", 'i', 0, G_OPTION_ARG_STRING, &iFolderID, "ID of the iFolder.", "IFOLDER_ID"},
	{ NULL }
};

/*

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
*/


int main(int argc, char* argv[])
{
	char c;
	printf("Hit Enter to continue\n");
	scanf("%c", &c);
	
	GError *error = NULL;
	GOptionContext* clContext = g_option_context_new(NULL);
	g_option_context_add_main_entries(clContext, AppOptions, NULL);
	GOptionGroup* domainGroup = g_option_group_new("domain", "Domain Options", "Options for operating on a domain", NULL, NULL);
	g_option_group_add_entries(domainGroup, DomainOptions);
	g_option_context_add_group(clContext, domainGroup);
	GOptionGroup* iFolderGroup = g_option_group_new("ifolder", "iFolder Options", "Options for operating on an iFolder", NULL, NULL);
	g_option_group_add_entries(iFolderGroup, iFolderOptions);
	g_option_context_add_group(clContext, iFolderGroup);
	if (!g_option_context_parse(clContext, &argc, &argv, &error))
	{
		printf("%s\n", error->message);
		g_clear_error(&error);
	}
	else
	{
		if (!IFApplication::Initialize(&error))
		{
			printf("%s\n", error->message);
			g_clear_error(&error);
		}
		if (iFolderName != NULL)
		{
		}
		else if (DomainName != NULL)
		{
			if (doLogin)
			{
				if (UserName == NULL)
				{
				}
				if (Password == NULL)
				{
				}
				IFDomain* pDomain = IFDomain::GetDomainByName(DomainName);
				if (pDomain != NULL)
				{
					GError *error = NULL;
					if (!pDomain->Login(Password, &error))
					{
						if (error != NULL)
						{
							g_warning(error->message);
							g_clear_error(&error);
						}
					}
					else
					{
						printf("Login Successful\n");
					}
				}
				
			}
			if (doLogout)
			{
			}
			if (doAdd)
			{
				if (HostName == NULL)
				{
					printf("Enter Host : ");
				}
				if (UserName == NULL)
				{
					printf("Enter User Name : ");
				}
				if (Password == NULL)
				{
					printf("Enter Password : ");
					fgetc(stdin);
				}
				IFDomain* pDomain = IFDomain::Add(UserName, Password, HostName, &error);
				if (pDomain == NULL)
				{
					if (error != NULL)
					{
						printf("%s\n", error->message);
						g_clear_error(&error);
					}
				}
				else
				{
					printf("Domain Add Successful\n");
					printf("Name : %s\n", pDomain->m_Name);
					printf("ID   : %s\n", pDomain->m_ID);
				}
			}
			if (doRemove)
			{
			}
			if (doList)
			{
			}
			if (doView)
			{
			}
		}
		else if (LogCmd)
		{
		}
	}
	g_option_context_free(clContext);
	printf("Hit Enter to continue\n");
	scanf("%c", &c);
	
	/*
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
	*/
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
	/*
	if (DomainOptions[DO_login].Set)
	{
		if (!DomainOptions[DO_user].Set || !DomainOptions[DO_password].Set)
		{
		}
	}
	else if (DomainOptions[DO_logout].Set)
	{
	}
	else if (DomainOptions[DO_add].Set)
	{
		if (!DomainOptions[DO_user].Set || !DomainOptions[DO_password].Set)
		{
			printf("UserName or Password is missing\n");
		}
	}
	else if (DomainOptions[DO_remove].Set)
	{
	}
	else if (DomainOptions[DO_list].Set)
	{
	}
	else if (DomainOptions[DO_view].Set)
	{
	}
	PrintEnteredOptions(command, extra);
	*/
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

