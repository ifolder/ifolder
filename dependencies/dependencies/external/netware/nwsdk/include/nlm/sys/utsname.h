#ifndef _SYS_UTSNAME_H_
#define _SYS_UTSNAME_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  sys/utsname.h
==============================================================================
*/

#define _SYS_NMLN    260     /* at least 257 to support Internet names */

struct utsname
{
   char sysname[_SYS_NMLN];  /* name of operating system implementation      */
   char release[_SYS_NMLN];  /* major/minor release level of implementation  */
   char version[_SYS_NMLN];  /* revision level of implementation             */
   char nodename[_SYS_NMLN]; /* implementation-specific communications net   */
   char machine[_SYS_NMLN];  /* name of hardware underneath operating system */
   char library[_SYS_NMLN];  /* version and level of CLib.NLM                */
};


#ifdef __cplusplus
extern "C" {
#endif

int uname( struct utsname *name );

#ifdef __cplusplus
}
#endif

#endif
