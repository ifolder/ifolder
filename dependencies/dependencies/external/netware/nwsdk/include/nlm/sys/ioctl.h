#ifndef _SYS_IOCTL_H_
#define _SYS_IOCTL_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  (C) Copyright 1982, 1985, 1986 Regents of the University of California.
=  All rights reserved. The Berkeley software License Agreement specifies the
=  terms and conditions for redistribution.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/ioctl.h
==============================================================================
*/
#include <sys/filio.h>
#include <sys/sockio.h>

/* for most I_- defines, include stropts.h */

/*
** Where 'fd' is a pipe, this command returns the number of bytes that can be
** written on a pipe without blocking. Analogous to I_NREAD which is also valid
** for a pipe.
*/
#define I_NWRITE     101

/*
** Where 'fd' is a pipe, this command permits the caller to change its alloc-
** ated buffer size. Any data written that has not been already read prior to
** this operation is lost and the buffer is cleared.
*/
#define I_SETBUF     102

#ifdef __cplusplus
extern "C" {
#endif

int ioctl( int fd, int command, ... );

#ifdef __cplusplus
}
#endif

#endif
