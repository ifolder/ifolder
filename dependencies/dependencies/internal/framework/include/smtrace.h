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
 | $Workfile:   smtrace.h   $
 | $Version:   1.1  $
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |
 |		Contains debug & profile macros, used in smsdebug.h 
 |      Developers SHOULD NOT directly use this file and the macros defined. 
 |
 +-------------------------------------------------------------------------*/

#ifndef _SMTRACE_H_
#define _SMTRACE_H_
 


#ifdef __cplusplus
extern "C" {
#endif

/*  Defines */

/* Defines for SmsDebug2 */

/* users will use DC_MINOR/DC_CRITICAL,  DC_VERBOSE/DC_COMPACT */
#define SD2_CRITICAL	                        0x00000001 
#define SD2_COMPACT 	                        0x00000002

#define SD2_FSTART			                    0x00000004

#define SD2_FEND				                0x00000008
#define SD2_FEND_ONLY_ON_CCODE				    0x00000010
#define SD2_FEND_ON_CCODE_FOR_ANY_FN            0x00000020

#define SD2_FLOG_ERROR			                0x00000040
#define SD2_FAILED_API_ARGUMENTS				0x00000080

#define SD2_FTRACK			                    0x00000100

#define SD2_LINE_NO				                0x00000200
#define SD2_THREAD_ID				            0x00000400
#define SD2_THREAD_NAME				            0x00000800
#define SD2_TIME_STAMP				            0x00001000

#define SD2_PRINT_CONSOLE			            0x00002000
#define SD2_PRINT_FILE			                0x00004000

#define SD2_DEBUG_SMS_DBG			            0x00008000

#define SD2_TRACK_MEMORY				        0x00010000

#define SD2_ALT_DEBUG			                0x00020000 



/*  This bit is reserved and can be used to notify that there 
 *   exists more than 2 SmsDebug variables
 */   
#define SD2_RESERVED			                0x80000000 



/* Defines for SmsProfile */

#define SMSPRO_HEXOUTPUT                        0x80000000





/* Update SMS_DBG_MAX_BUF in SmsDebug.h, when the following are changed */

/*Max String Size used in sms debug*/
#define SMS_DBG_MAX_STRING                      1024

/*todo: Remove hard coding 128. ~128 comes from the format string in FlogError_*/

/*Max String Size used to hold the arguments in sms debug*/
#define SMS_DBG_MAX_ARG_STRING_LENGTH           896 /* (SMS_DEBUG_MAX_STRING - 200)*/

/*Max String Size used to hold the arguments in sms debug*/
#define SMS_DBG_MAX_ARG_STRING                  "896" /* (SMS_DEBUG_MAX_STRING - 200)*/

/* Max length of the function name, to display */
#define SMS_DBG_MAX_FN_NAME_LENGTH              25

/* Max length of the function name, to display */
#define SMS_DBG_MAX_FN_NAME                     "25" 

/* Max length of the line no, to display */
#define SMS_DBG_MAX_LINE_NO_LENGTH              5

/* Max length of the line no, to display */
#define SMS_DBG_MAX_LINE_NO                     "5" 

/* Max length of the Thread Name, to display */
#define SMS_DBG_MAX_THREAD_NAME_LENGTH          25

/* Max length of the Thread Name, to display */
#define SMS_DBG_MAX_THREAD_NAME                 "25"

/* Max length of the Thread ID / function pointer, to display (should be >= 8)*/
#define SMS_DBG_MAX_ID_LENGTH                   8

/* Max length of the Thread ID / function pointer, to display (should be >= 8)*/
#define SMS_DBG_MAX_ID                          "8"

/* Total No of SmsDebug Variables */
#define SMS_DBG_TOTAL_NO_OF_SMSDEBUG_VARS       2  



/*  Function Macros */

#define isAltDebugger()			                (SmsDebug2Var() & SD2_ALT_DEBUG) 

#define isDebugSmsDbg()			                (SmsDebug2Var() & SD2_DEBUG_SMS_DBG)

#define isFStart()			                    (SmsDebug2Var() & SD2_FSTART)
#define isFEnd()			                    (SmsDebug2Var() & SD2_FEND)
#define isFEndOnlyOnCcode()		                (SmsDebug2Var() & SD2_FEND_ONLY_ON_CCODE)
#define isFEndOnCcodeForAnyFn()                 (SmsDebug2Var() & SD2_FEND_ON_CCODE_FOR_ANY_FN)
#define isFLogError()                            (SmsDebug2Var() & SD2_FLOG_ERROR)
#define isFTrack()			                    (SmsDebug2Var() & SD2_FTRACK)

#define isPrintConsole()			            (localSmsDebug2  & SD2_PRINT_CONSOLE)
#define isPrintFile()			                (localSmsDebug2  & SD2_PRINT_FILE)
#define isLineNo()			                    (localSmsDebug2  & SD2_LINE_NO)
#define isThreadId()			                (localSmsDebug2  & SD2_THREAD_ID)
#define isThreadName()			                (localSmsDebug2  & SD2_THREAD_NAME)
#define isTimeStamp()			                (localSmsDebug2  & SD2_TIME_STAMP)

#define isExcluded()                            (!( ((!(SmsDebug & CRITICAL) || (fType & CRITICAL)) && (!(SmsDebug & COMPACT) || (fType & COMPACT))) \
                                                && (SmsDebug & (fType & Sms_AllLayers))== (fType & Sms_AllLayers)))

#define isExcluded2()                           (SmsDebug & (fType & Sms_AllLayers)) && (!((!(SmsDebug2 & SD2_CRITICAL) || (debugControl & SD2_CRITICAL)) && (!(SmsDebug2 & SD2_COMPACT) || (debugControl & SD2_COMPACT))))



#define ReSetPrintFile()			            (localSmsDebug2 = SmsDebug2 & ~SD2_PRINT_FILE)
#define SetPrintFile()			                (localSmsDebug2 = SmsDebug2 | SD2_PRINT_FILE)





#ifdef N_PLAT_NLM
#pragma aux LightUltraResolution64 =                \
             "rdtsc"                                \
             value   [edx eax]                      \
             modify [edx eax];
#endif

/*  Extern Variables  */

extern unsigned int SmsProfile;

/*  Function Prototypes  */

void Ftrack_(void * function,  long lineNo, char* msg, unsigned int DebugControl, unsigned int fType);
void Ftrack1_(void * function,  long lineNo, char* format, void* arg1, unsigned int DebugControl, unsigned int fType);
void Ftrack2_(void * function,  long lineNo, char* format, void* arg1, void *arg2, unsigned int DebugControl, unsigned int fType);
void Ftrack3_(void * function,  long lineNo, char* format, void* arg1, void *arg2, void*arg3 , unsigned int DebugControl, unsigned int fType);
void Ftrack4_(void * function,  long lineNo, char* format, void* arg1, void *arg2, void*arg3 , void *arg4, unsigned int DebugControl, unsigned int fType);
void Ftrack5_(void * function,  long lineNo, char* format, void* arg1, void *arg2, void*arg3 , void *arg4, void *arg5, unsigned int DebugControl, unsigned int fType);
void FlogError_(void * function,  long lineNo,  char *failedAPIName,  unsigned int errNo, char *);
void Fend_(void *function, unsigned long cCode, unsigned int fType);
void Fstart_(void *function, unsigned int fType);

#ifdef N_PLAT_NLM
__int64 GetProcessorOperatingFrequency(void); /* can be used when we want the time instead of clock ticks */

__int64 LightUltraResolution64(void);

unsigned int LightHighResolution32(void);

#define Print64(var) ConsolePrintf("%-8.8X%-8.8X", *((unsigned int*)&(var)+1), *(unsigned int*)&(var))
#endif


#ifdef __cplusplus
}
#endif

#endif

/***************************************************************************/
