/****************************************************************************
 |
 |  (C) Copyright 2002 Novell, Inc.
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
 | $Author$
 | $Modtime:  15Jun04 13:14:35 $
 |
 | $Workfile:  SmsDebug.h   $
 | $Version:   1.1  $
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Macros for debug and profile trace.
 +-------------------------------------------------------------------------*/

#ifndef _SMSDEBUG_H_
#define _SMSDEBUG_H_
 

#include <smtrace.h>

#ifdef __cplusplus
extern "C" {
#endif




/*  For functions, that deal with passwords/security related or that require no debug info or that are 
 *  called while holding spinlocks etcetera 
 */
 
#define NO_DEBUG_INFO                 0x00000000 






/*
 *	FTYPE Values
 */

/* CRITICALITY wise: Choose only one of the below */

#define CRITICAL	                    0x00000001

#define MINOR	                        0x00000000



/* VERBOSITY wise: Choose only one of the below */

#define COMPACT 	                    0x00000002

#define VERBOSE			                0x00000000


/* LAYER wise: Choose only one of the below */

#define INTERFACE_FUNCTION              0x00000004

#define ALL_LAYERS					INTERFACE_FUNCTION


/* In Out parameters for the interface of the module */
#define INTERFACE_ARGS                  0x80000000



/*	DebugControl Values for FTrack. */


/* CRITICALITY wise */

#define DC_MINOR 		                0x00000000
#define DC_CRITICAL 		            0x00000001



/* VERBOSITY wise */

#define DC_VERBOSE			            0x00000000
#define DC_COMPACT			            0x00000002






/*	Other Defines. */

 /* Screen Color Defines */
#define CYAN			                3
#define LGREY			                7
#define LCYAN			                11
#define LGREEN			                10
#define LRED			                12
#define WHITE			                15
#define YELLOW			                14


/* Defines used in SMSPrintHexMem function */

#define OUTPUT_FORMAT       "%-.08X "
#define OUTPUT_LENGTH       9
#define NEWLINE_COUNT       (OUTPUT_LENGTH * 8)  





/*	Other Macros */

/* Total no of bytes that can be sent to Ftracks ( includes msg and all args ) */

/* todo: Remove hard coding 67. 67 comes from the format string in Ftracks */
#define SMS_DBG_MAX_BUF_LENGTH          829         /* SMS_DBG_MAX_STRING - 67 */

/* Total no of bytes that can be sent to Ftracks ( includes msg and all args ) */
#define SMS_DBG_MAX_BUF                 "828"       /* SMS_DBG_MAX_STRING - 67 -1 */




/*	Structure Declarations and Typedefs */


#ifdef N_PLAT_NLM
typedef long (*SMS_DbgHeadCmd)(struct ScreenStruct *scrID, char *cmd, struct StackFrame *stackFrame);

/*
*   IMP NOTE: 
*       The OS will truncate the arguments to the last 8 characters if the argument
*       'appears' to be a valid hexadecimal value. Take care this when designing the 
*       command and when parsing.
*
*   Ex1:
*       If the the actual command line is,
*       cmd asdasd asd 1234567890A 
*     then we will get the cmd line argument as 
*           asdasd asd 4567890A 
*
*   Ex2:
*       If the the actual command line is,
*       cmd asd "1234567890" asdasd
*     then we will get the cmd line argument as 
*           asd "1234567890" asdasd
*
*   Ex3:
*       If the the actual command line is,
*       cmd asdasd 12345678901 asd
*     then we will get the cmd line argument as 
*           asdasd 45678901 asd
*/

typedef struct _SMS_DEBUG_EXTN_
{
	char			*name;			/* Name of the extension, e.g if cmd is SMSDBG then DBG */
	SMS_DbgHeadCmd	 processor;		/* Processor function for the command */
	char			*usageString;	/* Usage string to include command and parameters to pass */
	char			*helpString;	/* Help string to be displayed in the debugger */
} SMS_DEBUG_EXTN;


#endif

#ifdef N_PLAT_NLM
struct ModSpecific
{
	 /* The module name */
	char                *Sms_ModSpecificDbgModName;

	 /* The debug file location for module */
	char                *Sms_ModSpecificDbgFileName; 

	 /* The debugger console command spedific to module */
	char                *Sms_ModSpecificDbgHeadCmd;

	/* All the layers in SmsDebug are represented by this bit map*/
	int 				Sms_AllLayers;

	 /* The debugger console command spedific to module */
	unsigned int              Sms_ModSpecificDbgModNlmHandle;

	 /* The thread group context of the module's main thread */
	unsigned int	                Sms_ModSpecificClibThreadGroupContext;

	 /* The array of debug extn structures to register with Alt Debugger. The last element contains all NULLs */
	SMS_DEBUG_EXTN	        *Sms_ModSpecificDbgCommands;

	/* The address of the sample enabled symbol (used along with the map file to resolve 
   	the address in the logs to their corresponding names, in release builds) */
	void                     *Sms_ModSpecificSampleEnabledSymbol;

	/* Name of the above symbol */
	char                     *Sms_ModSpecificSampleEnabledSymbolName;

};
#else
struct ModSpecific
{
	 /* The module name */
	char                *Sms_ModSpecificDbgModName;

	 /* The debug file location for module */
	char                *Sms_ModSpecificDbgFileName; 

	 /* The debugger console command spedific to module */
	char                *Sms_ModSpecificDbgHeadCmd;

	/* All the layers in SmsDebug are represented by this bit map*/
	int 				Sms_AllLayers;
};
#endif



/*	Extern Variables and functions SPECIFIC to the modules, defined by them. */


/* The module's custom debug init and deinit */
struct ModSpecific* InitCustomDebug(void);
void DeInitCustomDebug(void);

/*  The module's custom debug start info is returned thru this function. 
 *   This function is called at the beginning of the logging and whatever 
 *   returned is logged at the start of the log file. Return NULL string if
 *   there is no such info, not NULL.                                    
 */    
char *GetCustomDebugStartInfoBuffer(void) ;

/*  The buffer returned is freed by calling this function.              */
void ReleaseCustomDebugStartInfoBuffer(char *) ;



/*	Extern Variables useful for users of SmsDebug */

/* To know whether SmsDebug services can be used or not. */
extern char             IsSmsDebugInitialised ;



/*	Function Macros */


/* To debug memory (/enable tagging for allocations etc.,) */
#define isTrackMemory()			                (SmsDebug2Var() & SD2_TRACK_MEMORY)

/* Maintains the state of tracking memory for that variable */
#define SetTrackMemory(Var)                     (SmsDebug2Var() & SD2_TRACK_MEMORY ? (Var |= SD2_TRACK_MEMORY) : (Var &= ~SD2_TRACK_MEMORY))

/* Enables the tracking of memory */
#define EnableTrackMemory()                     SmsDebug2Var() |= SD2_TRACK_MEMORY




/*	Macros which help in time/space optimizations */

/* useful to skip big chunks of debug code (converting parameters from unicode etc., for FLogError), if not enabled*/
#define isFailedApiArgs()                       (SmsDebug2Var() & SD2_FAILED_API_ARGUMENTS)

/* useful to decide whether to compact the info */
#define isDebugCompact()			            (FTYPE & SmsDebugVar() && COMPACT & SmsDebug2Var() )

/* useful to skip big chunks of debug code (converting parameters from unicode or extracting info (like name) from structures etc., ), if not enabled*/
#define isInterfaceArgs()                       (SmsDebugVar() & INTERFACE_ARGS)


/*	Function macros - Profile & Debug */


#ifdef SMSPROFILE64

#define FStart()        __int64 _smsProfileStart, _smsProfileEnd;                                   \
                        if(SmsProfile & (FTYPE & SMS_ALL_LAYERS) && SmsProfile & FTYPE )            \
                        {                                                                           \
                            _smsProfileStart = LightUltraResolution64();                            \
                        }                                                                  



/* TODO:   compute the time for some console printf and fprintfs, used here, and take the avg and decr.. .*/
/* TODO:   Option to print decimal output for 64 bit */

#define FEnd(cCode)     if(SmsProfile & (FTYPE & SMS_ALL_LAYERS) && SmsProfile & FTYPE )            \
                        {                                                                           \
                            _smsProfileEnd  = LightUltraResolution64();                             \
                            _smsProfileEnd -= _smsProfileStart;                                     \
                            ConsolePrintf("%s : %x :%x%x \n", FNAME, kCurrentThread(), *((unsigned int*)&(_smsProfileEnd)+1), *(unsigned int*)&(_smsProfileEnd));   \
                        }



#define FLogError(Failed_NON_SMS_API_Name, ErrNo, ArgListString )                                   \
                        if(SmsDebugVar() & FTYPE && isFLogError())                                       \
                            FlogError_(FNAME, __LINE__, Failed_NON_SMS_API_Name, (unsigned int)ErrNo, ArgListString);


#define FTrack(Feature_To_Debug, DebugControl, msg)                     
#define FTrack1(Feature_To_Debug, DebugControl, msg, arg1)             
#define FTrack2(Feature_To_Debug, DebugControl, msg, arg1, arg2)       
#define FTrack3(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3)   
#define FTrack4(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4)   
#define FTrack5(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4, arg5)   

#elif defined (SMSPROFILE32)

#define FStart()        unsigned int _smsProfileStart, _smsProfileEnd;                                   \
                        if(SmsProfile & (FTYPE & SMS_ALL_LAYERS) && SmsProfile & FTYPE )            \
                        {                                                                           \
                            _smsProfileStart = LightHighResolution32();                             \
                        }                                                                  



/* TODO:   compute the time for some console printf and fprintfs, used here, and take the avg and decr.. .*/
#define FEnd(cCode)     if(SmsProfile & (FTYPE & SMS_ALL_LAYERS) && SmsProfile & FTYPE )            \
                        {                                                                           \
                            _smsProfileEnd  = LightHighResolution32();                              \
                            _smsProfileEnd -= _smsProfileStart;                                     \
                            ConsolePrintf(SmsProfile & SMSPRO_HEXOUTPUT ? "%s : %x :%x \n" : "%s : %x :%d \n", FNAME, kCurrentThread(), _smsProfileEnd);    \
                        }



#define FLogError(Failed_NON_SMS_API_Name, ErrNo, ArgListString )                                   \
                        if(SmsDebugVar() & FTYPE && isFLogError())                                       \
                            FlogError_(FNAME, __LINE__, Failed_NON_SMS_API_Name, (unsigned int)ErrNo, ArgListString);


#define FTrack(Feature_To_Debug, DebugControl, msg)                     
#define FTrack1(Feature_To_Debug, DebugControl, msg, arg1)             
#define FTrack2(Feature_To_Debug, DebugControl, msg, arg1, arg2)       
#define FTrack3(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3)   
#define FTrack4(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4)   
#define FTrack5(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4, arg5)   


#elif defined (SMSDEBUG)

#define FLogError(Failed_NON_SMS_API_Name, ErrNo, ArgListString )                                   \
                        if(SmsDebugVar() & FTYPE && SmsDebugVar() & (FTYPE & SMS_ALL_LAYERS) && isFLogError())                                       \
                            FlogError_(FNAME, __LINE__, Failed_NON_SMS_API_Name, (unsigned int)ErrNo, ArgListString);

#define FStart()        if(SmsDebugVar() & FTYPE && isFStart())     Fstart_(FNAME, FTYPE)


#define FEnd(cCode)     if(SmsDebugVar() && (isFEndOnCcodeForAnyFn() || ((SmsDebugVar() & FTYPE) && (isFEnd()||isFEndOnlyOnCcode())))) Fend_(FNAME, cCode, FTYPE)


#define FTrack(Feature_To_Debug, DebugControl, msg)                                                 \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack_(FNAME, __LINE__, msg, DebugControl, FTYPE); 

#define FTrack1(Feature_To_Debug, DebugControl, msg, arg1)                                          \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack1_(FNAME, __LINE__, msg, (void*)(arg1), DebugControl, FTYPE); 

#define FTrack2(Feature_To_Debug, DebugControl, msg, arg1, arg2)                                    \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack2_(FNAME, __LINE__, msg, (void*)(arg1), (void*)(arg2), DebugControl, FTYPE);

#define FTrack3(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3)                              \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack3_(FNAME, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3), DebugControl, FTYPE); 

#define FTrack4(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4)                        \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack4_(FNAME, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3), (void*)(arg4), DebugControl, FTYPE); 

#define FTrack5(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4, arg5)                  \
                        if(SmsDebugVar() & Feature_To_Debug && isFTrack())                               \
                            Ftrack5_(FNAME, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3 ),(void*)(arg4 ),(void*)(arg5), DebugControl, FTYPE); 

#else  /* Release Build  */

#define FLogError(Failed_NON_SMS_API_Name, ErrNo, ArgListString )                                   \
                        if(SmsDebugVar() & FTYPE && SmsDebugVar() & (FTYPE & SMS_ALL_LAYERS) && isFLogError())\
                            FlogError_(FPTR, __LINE__, Failed_NON_SMS_API_Name, (unsigned int)ErrNo, ArgListString);

#define FStart()        if(SmsDebugVar() & FTYPE && isFStart())     Fstart_(FPTR, FTYPE)


#define FEnd(cCode)     if(SmsDebugVar() && (isFEndOnCcodeForAnyFn() || ((SmsDebugVar() & FTYPE) && (isFEnd()||isFEndOnlyOnCcode())))) Fend_(FPTR, cCode, FTYPE)



#define FTrack(Feature_To_Debug, DebugControl, msg)                                                 \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack_(FPTR, __LINE__, msg, DebugControl, FTYPE); 

#define FTrack1(Feature_To_Debug, DebugControl, msg, arg1)                                          \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack1_(FPTR, __LINE__, msg, (void*)(arg1), DebugControl, FTYPE); 

#define FTrack2(Feature_To_Debug, DebugControl, msg, arg1, arg2)                                    \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack2_(FPTR, __LINE__, msg, (void*)(arg1), (void*)(arg2), DebugControl, FTYPE); 

#define FTrack3(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3)                              \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack3_(FPTR, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3), DebugControl, FTYPE); 

#define FTrack4(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4)                        \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack4_(FPTR, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3), (void*)(arg4), DebugControl, FTYPE);

#define FTrack5(Feature_To_Debug, DebugControl, msg, arg1, arg2, arg3, arg4, arg5)                  \
                        if(SmsDebugVar() & Feature_To_Debug  && isFTrack())                              \
                            Ftrack5_(FPTR, __LINE__, msg, (void*)(arg1), (void*)(arg2), (void*)(arg3 ),(void*)(arg4 ),(void*)(arg5), DebugControl, FTYPE); 


#endif              /* End of        #ifdef SMSPROFILE64  */







/*=================================================================================
 *=================================================================================
 *	Function Declarations
 *=================================================================================
 *=================================================================================*/


/* Ascii to Hexadecimal */
unsigned int atoh(char *s);


/*  Prints the given memory into a string in hex while inserting newlines after every 'NEWLINE_COUNT' th char, optionally */
char *SMSPrintHexMem(void *mem, int len, unsigned short insertNewLine, int sizeOfOutBuf, char *outBuf);

void RegisterDebugExtn(void);
void UnRegisterDebugExtn(void);

unsigned long SmsDebugVar(void);
unsigned long SmsDebug2Var(void);

int InitSMSDebug(struct ModSpecific* (*InitCustomDebug)(void), 
				char* (*GetCustomDebugStartInfoBuffer)(void), 
				void (*ReleaseCustomDebugStartInfoBuffer)(char *));
void DeInitSMSDebug(void (*DeInitCustomDebug)(void));
void UpdateSMSDebug(char* (*GetCustomDebugStartInfoBuffer)(void), 
				void (*ReleaseCustomDebugStartInfoBuffer)(char *));

unsigned long SetSmsDebug(unsigned int val, char* (*GetCustomDebugStartInfoBuffer)(void), 
				void (*ReleaseCustomDebugStartInfoBuffer)(char *));
unsigned long SetSmsDebug2(unsigned int val, char* (*GetCustomDebugStartInfoBuffer)(void), 
				void (*ReleaseCustomDebugStartInfoBuffer)(char *));

unsigned long SMSDBG_RestartFileLogging(char *oldLocation,
						  					    char *newLocation,
					char* (*GetCustomDebugStartInfoBuffer)(void), 
				void (*ReleaseCustomDebugStartInfoBuffer)(char *));


/*	Error Numbers for SMS DEBUG */

#define     SMSDBG_ERROR_CODE(err)          (0xFFE10000L | err)

#define     SMSDBG_ERR_NO_FILE_LOGGING      SMSDBG_ERROR_CODE(0xFFFF)
#define     SMSDBG_ERR_OUT_OF_MEMORY        SMSDBG_ERROR_CODE(0xFFFE)
#define     SMSDBG_ERR_NOT_INITIALIZED      SMSDBG_ERROR_CODE(0xFFFD)





#ifdef __cplusplus
}
#endif

#endif

/***************************************************************************/
