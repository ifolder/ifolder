/*============================================================================
=
=  NetWare NLM Library source code
=
=  Unpublished Copyright (C) 1995 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  io.h
==============================================================================
*/
#ifdef _FIND_OLD_HEADERS_
# error This is an obsolete, Novell SDK header!
#else
# include <fcntl.h>
# include <unistd.h>
# include <nwfattr.h>

# define STDIN_HANDLE   0        /* use POSIX STDIN_FILENO */
# define STDOUT_HANDLE  1        /* use POSIX STDOUT_FILENO */
# define STDERR_HANDLE  2        /* use POSIX STDERR_FILENO */

# define ACCESS_RD      0x0004   /* use POSIX R_OK */
# define ACCESS_WR      0x0002   /* use POSIX W_OK */
# define ACCESS_XQ      0x0001   /* use POSIX X_OK */

#endif
