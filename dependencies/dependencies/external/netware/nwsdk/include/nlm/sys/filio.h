#ifndef _SYS_FILIO_H_
#define _SYS_FILIO_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/filio.h
==============================================================================
*/

#define FIONREAD     1  /* get count of bytes to read (readable) */
#define FIONBIO      2  /* set/clear nonblocking I/O */
#define FIOGETNBIO   3  /* get nonblocking I/O status */

#endif
