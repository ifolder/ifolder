#ifndef _SIGNAL_H_
#define _SIGNAL_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  signal.h
==============================================================================
*/

#define SIG_IGN            (void (*)(int)) 1
#define SIG_DFL            (void (*)(int)) 2
#define SIG_ERR            (void (*)(int)) 3

#define SIGABRT            1
#define SIGFPE             2
#define SIGILL             3
#define SIGINT             4
#define SIGSEGV            5
#define SIGTERM            6
#define SIGPOLL            7

/* currently unimplemented POSIX-mandated signals */
#define SIGKILL            101
#define SA_NOCLDSTOP       102
#define SIGALRM            103
#define SIGCHILD           104
#define SIGCONT            105
#define SIGHUP             106
#define SIGPIPE            107
#define SIGQUIT            108
#define SIGSTOP            109
#define SIGTSTP            110
#define SIGTTIN            111
#define SIGTTOU            112
#define SIGUSR1            113
#define SIGUSR2            114
#define SIG_BLOCK          115
#define SIG_SETMASK        116
#define SIG_UNBLOCK        117

/* Novell-defined signals */
#define SIG_FINI           500
#define SIG_IPBIND         501
#define SIG_IPUNBIND       502
#define SIG_IPXBIND        503
#define SIG_IPXUNBIND      504

#define SIG_IPREGISTER     505
#define SIG_IPUNREGISTER   506
#define SIG_IPXREGISTER    507
#define SIG_IPXUNREGISTER  508

#define SIG_LOCALECHANGE   510

typedef int sig_atomic_t;

#ifdef __cplusplus
extern "C"
{
#endif

int   raise( int );
void  (*signal( int sig, void (*func)( int ) )) ( int );

#ifdef __cplusplus
}
#endif

#endif
