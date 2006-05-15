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
#ifndef _OPTIONS_H_
#define _OPTIONS_H_

#include <string.h>
#include "glibclient.h"

#define ValidOptionsString "Valid options are\n\n"
#define UsageString "Usage: "
#define ValidTargetString "The valid [Target]s are:\n"
#define InvalidOptionString "Invalid option %s\nTry \'%s %s --%s\' for more information"
#define OptionsString "[Options]"
#define TargetString "[Target]"
#define TargetHelpString "\nUse \'%s [Target] --help\' for information on target options.\n\n"

typedef struct _Option_
{
	char		*ShortName;
	char		*LongName;
	char		*ValueName;
	char		*Description;
	char		*Value;
	bool		Set;
} Option, *POption;

typedef int (*CommandHandler)(struct _Command_ *pCommand, char* extra[]);
typedef struct _Command_
{
	char			*Name;
	int				OptionCount;
	POption			Options;
	CommandHandler	Handler;
} Command, *PCommand;

extern GLIBCLIENT_API int ParseCommand(int argc, char* argv[], int commandCount, PCommand commands);
extern GLIBCLIENT_API void ShowOptionUsage(int count, Option options[]);
extern GLIBCLIENT_API void ShowUsage(char * progName, int count, PCommand commands, bool showOptions);
extern GLIBCLIENT_API int ParseOptions(int argc, char* argv[], int optionCount, Option options[]);
#endif // _OPTIONS_H_