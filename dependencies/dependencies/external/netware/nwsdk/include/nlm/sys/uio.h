#ifndef _SYS_UIO_H_
#define _SYS_UIO_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/uio.h
==============================================================================
*/

struct iovec
{
   char  *iov_base;
   int   iov_len;
};

#endif
