#ifndef _NWSIGNAL_H_
#define _NWSIGNAL_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1996-1998 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwsignal.h
==============================================================================
*/
/*
** These symbols are the ANSI ones to force sameness between the public signal.h
** content and the content of this file.  It is assumed that the source file
** which implements signal() and raise will include both this header and
** signal.h, which will force the issue.
*/
#define SIG_IGN (void (*)(int)) 1
#define SIG_DFL (void (*)(int)) 2
#define SIG_ERR (void (*)(int)) 3

#define SIGABRT 1
#define SIGFPE  2
#define SIGILL  3
#define SIGINT  4
#define SIGSEGV 5
#define SIGTERM 6
#define SIGPOLL 7

#define _SIGLAST	500

#ifdef __cplusplus
extern "C"
{
#endif

void  _DefaultSignalHandler( int sig );

#ifdef __cplusplus
}
#endif

#endif
