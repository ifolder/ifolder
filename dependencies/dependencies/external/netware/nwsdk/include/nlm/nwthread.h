#ifndef _NWTHREAD_H_
#define _NWTHREAD_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwthread.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


/* values for __action_code used with ExitThread() */
#define TSR_THREAD                  (-1)
#define EXIT_THREAD                 0
#define EXIT_NLM                    1

/* values for __mode used with spawnxx() */
#define P_WAIT                      0
#define P_NOWAIT                    1
#define P_OVERLAY                   2
#define P_NOWAITO                   4
#define P_SPAWN_IN_CURRENT_DOMAIN   8

#define NO_CONTEXT                  0
#define USE_CURRENT_CONTEXT         1

/* stack defines */
#define MIN_STACKSIZE               16384
#define DEFAULT_STACKSIZE           16384


struct AESProcessStructure
{                                            /* set by or to... */
   struct AESProcessStructure *ALink;        /* AES */
   LONG              AWakeUpDelayAmount;     /* ticks to wait; unchanged */
   LONG              AWakeUpTime;            /* AES */
   void              (*AProcessToCall)(void *);/* function to call; unchanged */
   LONG              ARTag;                  /* resource tag; unchanged */
   LONG              AOldLink;               /* NULL */
};

typedef struct WorkToDoStructure
{                                        
   struct WorkToDoStructure   *Link;
   void                       (*workProcedure)();
   LONG                       WorkResourceTag;
   LONG                       PollCountAmount;              
   LONG                       PollCountWhen;                 
   void                       (*userProcedure)();
   void                       *dataPtr;
   LONG                       destThreadGroup;
} WorkToDo;

typedef void (*CLEANUP)( int );


#ifdef __cplusplus
extern "C"
{
#endif

/* custom data area variables... */
extern void *threadCustomDataPtr;
extern LONG threadCustomDataSize;
extern void *threadGroupCustomDataPtr;
extern LONG threadGroupCustomDataSize;

/* prototypes... */
int   AtUnload( void (*func)( void )); 
int   BeginThread( void (*func)( void * ), void *stackP,
         unsigned int stackSize, void *arg );
int   BeginThreadGroup( void (*func)( void * ), void *stackP,
         unsigned int stackSize, void *arg ); 
int   Breakpoint( int arg ); 
void  CancelNoSleepAESProcessEvent(
         struct AESProcessStructure *EventNode ); 
void  CancelSleepAESProcessEvent(
         struct AESProcessStructure *EventNode );
int   ClearNLMDontUnloadFlag( int NLMID );
void  delay( unsigned milliseconds );
int   EnterCritSec( void );
int   ExitCritSec( void );
void  ExitThread( int action_code, int termination_code ); 
unsigned int   FindNLMHandle( const char *NLMFileName );
char  *getcmd( char *cmdLine ); 
unsigned int   GetNLMHandle( void ); 
int   GetNLMID( void ); 
int   GetNLMIDFromNLMHandle( int NLMHandle ); 
int   GetNLMIDFromThreadID( int threadID, char *fileName );
int   GetNLMNameFromNLMID( int NLMID, char *fileName, char *description ); 
int   GetNLMNameFromNLMHandle( int  NLMHandle, char *LDFileName, char *LDName );
int   GetThreadContextSpecifier( int threadID ); 
int   GetThreadGroupID( void ); 
LONG  __GetThreadIDFromPCB( int PCB );
LONG  GetThreadHandicap( int threadID );
int   GetThreadID( void );
int   GetThreadName( int threadID, char *tName );
int   MapNLMIDToHandle( int NLMID );
CLEANUP  PopThreadCleanup( int execute );
CLEANUP  PopThreadGroupCleanup( int execute );
int   PushThreadCleanup( CLEANUP func );
int   PushThreadGroupCleanup( CLEANUP func );
int   RenameThread( int threadID, const char *newName );
int   ResumeThread( int threadID ); 
int   ReturnNLMVersionInfoFromFile( const BYTE *pathName, LONG *majorVersion,
         LONG *minorVersion, LONG *revision, LONG *year, LONG *month, LONG *day,
         BYTE *copyrightString, BYTE *description ); 
int   ReturnNLMVersionInformation( int NLMHandle, LONG *majorVersion,
         LONG *minorVersion, LONG *revision, LONG *year, LONG *month, LONG *day,
         BYTE *copyrightString, BYTE *description );
void  ScheduleNoSleepAESProcessEvent(
      struct AESProcessStructure *EventNode );
void  ScheduleSleepAESProcessEvent(
      struct AESProcessStructure *EventNode );
int   ScheduleWorkToDo( void (*ProcedureToCall)( void *data, WorkToDo
         *workToDo ), void *workData, WorkToDo *workToDo );
int   SetNLMDontUnloadFlag( int NLMID );
int   SetNLMID( int newNLMID );
int   SetThreadContextSpecifier( int threadID, int contextSpecifier );
int   SetThreadGroupID( int newThreadGroupID );
void  SetThreadHandicap( int threadID, int handicap );
int   spawnlp( int mode, const char *path, char *arg0, ... );
int   spawnvp( int mode, const char *path, char **argv );
int   SuspendThread( int threadID );
void  ThreadSwitch( void );
void  ThreadSwitchLowPriority( void );
void  ThreadSwitchWithDelay( void );

#ifdef __cplusplus
}
#endif


#endif
