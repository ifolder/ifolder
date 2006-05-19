/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
#include <stdio.h>

#include "IFOptions.h"

Option help = {"?", "help", NULL, "Display this help", NULL, false};

int InternalParseOptions(int argc, char* argv[], int optionCount, Option options[], int argOffset);

int ParseCommand(int argc, char *argv[], int commandCount, PCommand commands)
{
	char *progName = argv[0];
	char *commandString = argv[1];
	int i;
	for (i = 0; i < commandCount; ++i)
	{
		PCommand command = &commands[i];
		if (strcmp(commandString, command->Name) == 0)
		{
			InternalParseOptions(argc, argv, command->OptionCount, command->Options, 2);
			return command->Handler(command, argv);
		}
	}
	if ((strlen(commandString) > 2) && ((strcmp(commandString + 2, help.LongName) == 0) || (strcmp(commandString + 1, help.ShortName) == 0)))
		ShowUsage(progName, commandCount, commands, true);
	else
		ShowUsage(progName, commandCount, commands, false);

	return 0;
}

void ShowOptionUsage(int count, Option options[])
{
	int i = 0;
	for (i; i < count; i++)
	{
		Option opt = options[i];
		printf("\t%c%s%s%s%s%c%s\n\t\t\t\t%s\n", 
			opt.ShortName != NULL ? '-' : ' ',
			opt.ShortName != NULL ? opt.ShortName : " ",
			opt.ShortName != NULL && opt.LongName != NULL ? ", " : "  ",
			opt.LongName == NULL ? "" : "--",
			opt.LongName == NULL ? "" : opt.LongName,
			opt.ValueName == NULL ? '\0' : '=',
			opt.ValueName == NULL ? "" : opt.ValueName,
			opt.Description);
	}
	printf("\t    --%s\n\t\t\t\t%s\n", help.LongName, help.Description);
}

void ShowUsage(char * progName, int count, PCommand commands, bool showOptions)
{
	int i;
	printf(UsageString);
	printf("%s %s %s...\n\n", progName, TargetString, OptionsString);
	printf(ValidTargetString);
	for (i=0; i < count; ++i)
	{
		PCommand command = &commands[i];
		printf("\t%s\n", command->Name);
	}
	printf(TargetHelpString, progName);

	if (showOptions)
	{
		for (i = 0; i < count; ++i)
		{
			PCommand command = &commands[i];
			if (command->Options != NULL)
			{
				printf("%s %s\n", command->Name, OptionsString);
				ShowOptionUsage(command->OptionCount, command->Options);
			}
			printf("\n");
		}
	}
}

int ParseOptions(int argc, char* argv[], int optionCount, Option options[])
{
	return InternalParseOptions(argc, argv, optionCount, options, 1);
}


int InternalParseOptions(int argc, char* argv[], int optionCount, Option options[], int argOffset)
{
	bool haveOption = false;
	int argIndex = argOffset;
	int	nextNonOption = 0;
	
	while (argIndex < argc)
	{
		char *pArg = argv[argIndex++];
		size_t charIndex = 0;
		if (pArg[charIndex++] == '-')
		{
			char ch;
			size_t nameLen;
			while ((ch = pArg[charIndex++]) != '\0')
			{
				// Check to see if this is a long option.
				Option *pOption = NULL;
				if (ch == '-')
				{
					// This is a long option find option.
					char *plopt = pArg + 2;
					if (strlen(plopt) == 0)
					{
						// we have a -- with no options leave the rest of the args as extra.
						while (argIndex < argc)
							argv[nextNonOption++] = argv[argIndex++];
						break;
					}
					int optionIndex = 0;
					for (optionIndex; optionIndex < optionCount; ++optionIndex)
					{
						Option *pTmpOpt = &options[optionIndex];
						nameLen = strlen(pTmpOpt->LongName) + 2;
						if (strncmp(plopt, pTmpOpt->LongName, nameLen - 2) == 0)
						{
							pOption = &options[optionIndex];
							break;
						}
					}
					if (pOption == NULL)
					{
						// Check if this is the help option.
						if (strcmp(plopt, help.LongName) == 0)
						{
							ShowOptionUsage(optionCount, options);
							return 0;
						}
					}
					charIndex = strlen(pArg);
				}
				else
				{
					// This is a short option.
					int optionIndex = 0;
					nameLen = charIndex; // include the -
					for (optionIndex; optionIndex < optionCount; ++optionIndex)
					{
						if ((options[optionIndex].ShortName != NULL) && 
							(strchr(options[optionIndex].ShortName, ch) != NULL))
						{
							pOption = &options[optionIndex];
							break;
						}
					}
				}
				if (pOption != NULL)
				{
					pOption->Set = true;
					haveOption = true;
					if (pOption->ValueName != NULL)
					{
						size_t argLen = strlen(pArg);
						if ((argLen > nameLen) && (pArg[nameLen] == '='))
						{
							pOption->Value = &pArg[nameLen+1];
							charIndex = strlen(pArg);
						}
						else
						{
							char *pValue = argIndex < argc ? argv[argIndex + 1] : NULL;
							if (pValue[0] != '-')
							{
								pOption->Value = pValue;
								argIndex++;
							}
						}
					}
				}
				else 
				{
					printf(InvalidOptionString, pArg, argv[0], argv[1], help.LongName);
				}
			}
		}
		else
		{
			// This is a non option.
			argv[nextNonOption++] = pArg;
		}
	}
	if (!haveOption)
		ShowOptionUsage(optionCount, options);
		argv[nextNonOption] = NULL;
	return 0;
}
