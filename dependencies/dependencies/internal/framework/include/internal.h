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
 |		Command line/Console command internal declarations for SMS
 ****************************************************************************/
#ifndef _INTERNAL_H_
#define _INTERNAL_H_

#ifndef FALSE
#define FALSE	0
#endif

#ifndef TRUE
#define TRUE		1
#endif

#if defined linux
#define UINT64	unsigned long long
#define INT64	long long
#elif defined N_PLAT_NLM
#define UINT64	unsigned __int64
#define INT64	__int64
#endif

/* Screen color defines */
#define CYAN			3
#define LCYAN			11
#define LGREEN			10
#define LRED			12
#define WHITE			15
#define YELLOW			14

#define no_argument			0
#define required_argument	1
#define optional_argument	2

#define MAX_FILE_NAME				1024
#define ALL_SWITCHES				-1
#define MAX_OPTION_NAME_LEN		1024

#define SM_CMDLN_IN_LONG_OPTIONS	1
#define SM_CMDLN_IN_SHORT_OPTIONS	2
#define SM_CMDLN_BOOL_NO_COUNTER_PART	1

/* values that outPutMethod takes */
#define NO_KEY_CHOSEN		0
#define CONTINUE_CHOSEN	1
#define ENTER_CHOSEN		2
#define ESCAPE_CHOSEN		3
#define OTHER_KEY_CHOSEN	4

#define NO_OF_ROWS 	25
#define NO_OF_COLS		80
#define LEFT_PANE_SIZE	25
#define TAB_SIZE			8
#define ESC_KEY			27
#define ENTER__KEY		13

#ifdef N_PLAT_NLM
#define ENTER_KEY2			0x02
#define ESCAPE_KEY			0x03
#endif

#define 	CON_CMD_RTAG_NAME	"Sms Console Command RTag"
#ifndef CMD_PASS_TO_NEXT
#define CMD_PASS_TO_NEXT 0x7000002D
#endif

#ifndef CMD_NO_MEMORY
#define CMD_NO_MEMORY 0x70000028
#endif

#ifndef CMD_CMD_EXECUTED
#define CMD_CMD_EXECUTED 0x00000000
#endif

#ifdef linux
#define print(c, s)					printf(s)
#define print1(c, s, a1)			printf(s, a1)
#define print2(c, s, a1, a2)		printf(s, a1, a2)
#define print3(c, s, a1, a2, a3)		printf(s, a1, a2, a3)
#elif defined WIN32
#define print(c, s)					_cprintf(s)
#define print1(c, s, a1)			_cprintf(s, a1)
#define print2(c, s, a1, a2)		_cprintf(s, a1, a2)
#define print3(c, s, a1, a2, a3)		_cprintf(s, a1, a2, a3)
#elif defined N_PLAT_NLM
#define print(c, s)					OutputToScreenWithAttribute(ScrId, c, s)
#define print1(c, s, a1)			OutputToScreenWithAttribute(ScrId, c, s, a1)
#define print2(c, s, a1, a2)		OutputToScreenWithAttribute(ScrId, c, s, a1, a2)
#define print3(c, s, a1, a2, a3)		OutputToScreenWithAttribute(ScrId, c, s, a1, a2, a3)
#define ignore(v)                		((v) = (v))
#endif


char *display_help(cmdLineSwitchState_s *switchState);
char inHexRange(char c);
#ifdef N_PLAT_NLM
UINT64 _atoi64(char *str);
#endif
#if defined WIN32 || defined N_PLAT_NLM
UINT64 atoll(char *str);
#endif
UINT64 hexatoull(char *txt);
INT64 hexatoll(char *txt);
void init_screen(void);
int print_left_pane(char *buffer, int color);
void print_right_pane(char *buffer, int color);
void remove_newlines(char *buf);
void increment_lines(int j);
int is_file_exists(char *file);
int cmdline_init_lock(void);
void cmdline_lock(void);
void cmdline_unlock(void);
void cmdline_deinitlock(void);

struct getopt_s
{
	int count;	/*index into argc array*/
	int position;	/*Position of the character in the argument pointed by 
					count th argc */
	int control;
};

struct option_s
{
	char *name;
	int 	has_arg;
	int 	val;
};

struct aux_long_opt
{
	cmdLineSwitch_s *switch_val;
		
};

#endif /*_INTERNAL_H_*/
