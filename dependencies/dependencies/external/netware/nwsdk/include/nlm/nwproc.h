#ifndef __nwproc_h__
#define __nwproc_h__
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwproc.h
==============================================================================
*/

#ifndef _PID_T
# define _PID_T
typedef long   pid_t;            /* process ID */
#endif

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

/* prototypes... */
pid_t CreateChildProcess( void *func, const void *threadName,
			const void *cmdLine, void *arg, void *stack, size_t stackSize,
			int stdfds[3], int clearenv, const void *procName, int enableApp );
int   KillChildProcess( pid_t pid );
int   WaitOnChildProcess( pid_t pid, int *statloc, int options );

#endif
