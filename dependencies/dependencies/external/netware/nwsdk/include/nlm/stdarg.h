#ifndef _STDARG_H_
#define _STDARG_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  stdarg.h
==============================================================================
*/

typedef char *va_list[1];

#define va_start(ap, parmN) ((ap)[0] = (char *) &(parmN) + \
      ((sizeof(parmN) + sizeof(int) - 1) & ~(sizeof(int) - 1)), (void) 0)

#define va_arg(ap, type) ((ap)[0] += \
      ((sizeof(type) + sizeof(int) - 1) & ~(sizeof(int) - 1)), \
      (*(type *) ((ap)[0] - \
      ((sizeof(type) + sizeof(int) - 1) & ~(sizeof(int) - 1)))))

#define va_end(ap) ((ap)[0] = 0, (void) 0)

#endif
