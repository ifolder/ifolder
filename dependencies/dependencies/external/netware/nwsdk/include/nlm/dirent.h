#ifndef _DIRENT_H_
#define _DIRENT_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  dirent.h
==============================================================================
*/

#ifndef _INO_T
#  define _INO_T
typedef long ino_t;
#endif

#ifndef _DEV_T
#  define _DEV_T
typedef long dev_t;
#endif

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <npackon.h>

typedef struct dirent 
{  
   unsigned long  d_attr;                 /* entry's attribute              */
   unsigned short d_time;                 /* entry's modification time      */
   unsigned short d_date;                 /* entry's modification date      */
   long           d_size;                 /* entry's size - files only      */
   ino_t          d_ino;                  /* serial number                  */
   dev_t          d_dev;                  /* volume number                  */
   unsigned long  d_cdatetime;            /* creation date and time         */
   unsigned long  d_adatetime;            /* last access date - files only  */
   unsigned long  d_bdatetime;            /* last archive date and time     */
   long           d_uid;                  /* owner id (object id)           */
   unsigned long  d_archivedID;           /* obj ID that last archived file */
   unsigned long  d_updatedID;            /* obj ID that last updated file  */
   char           d_nameDOS[13];          /* entry's DOS namespace name     */
   unsigned short d_inheritedRightsMask;  /* entry's inherited rights mask  */
   unsigned char  d_originatingNameSpace; /* entry's creating name space    */

   /* the next two fields are only valid in ScanErasedFiles()               */
   unsigned long  d_ddatetime;            /* deleted date/time              */
   unsigned long  d_deletedID;            /* deleted ID                     */

   /*----------------- new fields starting in v4.11 ----------------------- */
   char           d_name[255+1];          /* entry's namespace name         */
} DIR;

#include <npackoff.h>

#ifdef __cplusplus
extern "C"
{
#endif

int   closedir( DIR *dirp );
int   closedir_510( DIR *dirp );
DIR   *opendir( const char *pathName );
DIR   *opendir_411( const char *pathName );
DIR   *readdir( DIR *dirp );
DIR   *readdir_411( DIR *dirp );
void   rewinddir( DIR *dirp );

/* Novell-defined function... */
int   SetReaddirAttribute(DIR *dirp, unsigned long newAttribute );

#ifdef __cplusplus
}
#endif

#define closedir(path)  closedir_510(path)
#define opendir(path)   opendir_411(path)
#define readdir(dirp)   readdir_411(dirp)

#endif
