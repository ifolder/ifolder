/****************************************************************************
 |
 |  (C) Copyright 2004 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Modtime:  23Aug04 12:52:40 $
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Command line/Console command declarations for SMS
 ****************************************************************************/

#ifndef _COMMANDLINE_H_
#define _COMMANDLINE_H_

#include<ctype.h>
#include<string.h>
#include<stdlib.h>
#include<stdio.h>


#define MAX_OPTION_SIZE			256

#define	SW_END_OF_SWITCHES	NULL, 0, NULL, NULL, {0, 0}, 0, NULL, 0

/* Switch type constants to be used in cmdLineSwitch_s.type. The application 
	has to use one and only one of the following switch types.*/
#define SWTYPE_VALUE				0x1	/* This switch has an argument */
#define SWTYPE_OPTIONAL_VALUE		0x2	/* This switch has an optional 
										 * argument */
#define SWTYPE_BOOL				0x3	/* This switch is a boolean one*/
#define SWTYPE_INFO					0x4	/* This switch's processor function 
											returns the information to be printed 
											on the screen by allocating memory. 
											The returned info will be freed by 
											the framework. License, Copyright, 
											Version etc., come under this category*/
#define SWTYPE_DESCRIPTION			0x5	/* This switch's processor function 
											returns the description of the program 
											to be printed on the screen by allocating 
											memory. The returned info will be 
											freed by the framework.*/
#define SWTYPE_USAGE				0x6	/* This switch's processor function 
											returns the usage of the program 
											to be printed on the screen by allocating 
											memory. The returned info will be 
											freed by the framework.*/
#define SWTYPE_HELP				0x7 /* The help command. */
#define SWTYPE_MASK			0x0000000F	/* range of type values*/
#define GET_SWTYPE(value)	((value) & SWTYPE_MASK)

/* Constants to indicate the type of value data for a switch */
#define SWVAL_INT			0x00000010	/* a binary int */
#define SWVAL_UINT			0x00000020	/* a binary unsigned int */
#define SWVAL_QUAD			0x00000030	/* a binary QUAD */
#define SWVAL_SQUAD			0x00000040	/* a binary SQUAD */
#define SWVAL_LONG			0x00000050	/* a binary long */
#define SWVAL_ULONG			0x00000060	/* a binary unsigned long */
#define SWVAL_INT16			0x00000070	/* a binary 16 bit int */
#define SWVAL_UINT16		0x00000080	/* a binary 16 bit unsigned int */
#define SWVAL_BYTE			0x00000090	/* a binary BYTE */
#define SWVAL_SBYTE			0x000000a0	/* a binary signed BYTE */
#define SWVAL_CHAR			0x000000b0	/* an ASCII string */
#define SWVAL_STRING		0x000000c0	/* a binary BOOLEAN */
#define SWVAL_MASK			0x000000f0	/* range of data values*/
#define GET_SWVAL(value)	((value) & SWVAL_MASK)

/* Additional options */
#define SWOPT_HAS_NUMERIC_RANGE		0x00010000	/* if set, use the given 
												 * numeric range limits*/
#define SWOPT_INPUT_IN_HEX			0x00020000	/* The input value would appear
												 * in hex and requires 
												 * transformation 	*/
#define SWOPT_HIDDEN				0x00400000	/* if set, don't display the switch
												 * when displaying help */

#define SWOPT_RUNTIME_ONLY			0x00800000	/* suppress switch at STARTUP */
#define SWOPT_STARTUP_ONLY			0x01000000	/* suppress switch at RUNTIME time*/
#define SWOPT_MASK_VALUES			0xffff0000	/* mask for switch options from
												 * the user */

#define GET_SWOPT(value)	((value) & SWOPT_MASK_VALUES)


/* Errors returned */
#define	CMDLN_SUCCESS							0
#define	CMDLN_ERR_OUT_OF_MEMORY			-1

/* One of the switches constructed and sent to CmdLineRegisterSwitches is invalid */
#define 	CMDLN_ERR_INVALID_SWITCH				-2

#define 	CMDLN_ERR_INVALID_ARGUMENT			-3
#define 	CMDLN_ERR_DUPLICATE_SWITCH			-4
#define 	CMDLN_ERR_FILE_NAME_TOO_LONG		-5
#define 	CMDLN_ERR_NOT_INITIALIZED				-6
#define 	CMDLN_ERR_NO_CONFIG_FILE				-7
#define 	CMDLN_ERR_CREATE_LOCK				-8
#define 	CMDLN_ERR_REGISTRATION_FAILED		-9
#define	CMDLN_ERR_INTERNAL_ERROR				-10

/*The option for which the argument passed is out of range is returned 
	in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_ARG_OUT_OF_RANGE			-11 

/*The option is returned in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_OPTION_REQUIRES_ARG		-12

/*The option is returned in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_INVALID_OPTION				-13

/*The option is returned in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_OPTION_TAKES_NO_ARG		-14

/*The option is returned in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_NOT_RUNTIME_OPTION		-15

/*The option is returned in the 'option' member of CmdLineError_s struct*/
#define 	CMDLN_ERR_NOT_LOADTIME_OPTION		-16 




struct cmdLineSwitch_s;

/* Switch state to be passed to the processes of the option */
typedef struct cmdLineSwitchState_s
{
	char 			isLoadTime;
	char				*switchArg;
	struct cmdLineSwitch_s		*switchData;
} cmdLineSwitchState_s;

/* definition of a routine that can be used to handle the options if the 
available features are not enough */
typedef char* (*cmdLineFn_t)(cmdLineSwitchState_s *switchState);

/* Structure that defines a single command line switch for the parses */
typedef struct cmdLineSwitch_s
{
	char			*name;			/* Switch name, i.e. text used on command 
									 * line to identify the switch */
	int				 type;			/* Type flags for this switch - see above*/
	cmdLineFn_t	 	processor;		/* Parsing function for this switch - NULL 
									 * if no programmed parsing is required */
	void			*value;			/* Address to store the value got from 
									 * command line*/
	union
	{
		struct 
		{
			int			valSize;	/* Size of the buffer pointed to by value */
			int			argSize;	/* Size of the string value on the command line */
		} string;
		struct
		{
			int			minVal;		/* Lower limit for this argument */
			int			maxVal;		/* Upper limit for this argument */
		} numeric;
	} valType;
	int			indexHelp;			/*The index to be given to help messages function
									  * to get the required help string. */
	void			*info;			/* Place to store owner specified 
									 * information about the switch */
	int 			reserved1;		/* reserved by framework and 
								  * should not be used by application. */
} cmdLineSwitch_s;

/* The command line init parameters are passed thru this struct. */
typedef struct CmdLineMod_s 
{
	char *headCmd;	/* The head command registered for command line parsing. */
	char *fileName;  /* The configuration file. */
	char *(*getMessage)(int);	/*The function that returns the help messages for 
							 * corresponding offsets passed. */
	void (*runTimeProcessor)(char*); /* The runtime processor function. 
									* NULL for non netware platforms. */
	int threadGroupId; /*Module's thread group id. 0 for non netware platforms. */
	int nlmHandle; /* Module's nlm handle. 0 for non netware platforms. */
	int help_index_Usage;	/*index to be passed to getMessage function for the 
							 *localised string: "Usage" */
	int help_index_Type;	/*index to be passed to getMessage function to get the 
							 *localised string: "Type" */
	int help_index_TRUE;	/*index to be passed to getMessage function to get the 
							 *localised string: "TRUE" */
	int help_index_FALSE;/*index to be passed to getMessage function to get the 
							 *localised string: "FALSE" */
	int help_index_Value;	/*index to be passed to getMessage function to get the 
							 *localised string: "Value" */
	int help_index_Range;	/*index to be passed to getMessage function to get the 
							 *localised string: "Range" */
	int help_index_switch;	/*index to be passed to getMessage function to get the 
							 *localised string: "Switch" */
	int help_index_Boolean;	/*index to be passed to getMessage function to get the 
							 *localised string: "Boolean" */
	int help_index_StartupOnly;	/*index to be passed to getMessage function to get 
								  * the localised string: "StartupOnly" */
	int help_index_RunTimeOnly;	/*index to be passed to getMessage function to get 
								  * the localised string: "RunTimeOnly" */
	int help_index_Informational;	/*index to be passed to getMessage function to get 
								  * the localised string: "Informational" */
}CmdLineMod_s;

/* The error information is returned through this structure. */
typedef struct CmdLineError_s
{
	int err;
	char *option;
}CmdLineError_s;

int CmdLineInit(CmdLineMod_s *mod);
void CmdLineDeInit(void);
int CmdLineRegisterSwitches(cmdLineSwitch_s *switches);
CmdLineError_s CmdLineProcessData(int argc, char *argv[], char isLoadTime);
CmdLineError_s CmdLineProcessDataByCmdLine(char *cmd_line, char isLoadTime);
CmdLineError_s CmdLineProcessDataByFile(char IsLoadTime);
void CmdLineDisplayHelp(void);

#endif /*_COMMANDLINE_H_*/

