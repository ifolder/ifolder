/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwextatt.h
==============================================================================
*/
          
#ifndef _NWEXTATT_H_
#define _NWEXTATT_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

#include <npackon.h>

#define CRITICAL_ATTRIBUTE_FLAG 0x00000080
#define RESERVED_FLAGS_MASK     0x0000FFFF
#define USER_FLAGS_MASK         0xFFFF0000

  /*------------------------------------------------------------------*
   * T_enumerateEAnoKey is the structure returned in the dataBuffer   *
   * by EnumerateEA if a empty key (or NULL) is specified.  The       *
   * EAsInReply output parameter tells how many T_enumerateEAnoKey    *
   * structures are in the dataBuffer.                                *
   *------------------------------------------------------------------*/
  
typedef struct tagT_enumerateEAnoKey
{
   LONG  valueLength;   /* length of entire EA */
   WORD  keyLength;
   LONG  accessFlags;
   char  keyValue[1];   /* length of this field is given by keyLength */
} T_enumerateEAnoKey;

  /*----------------------------------------------------------------------*
   * T_enumerateEAwithKey is the structure returned in the dataBuffer     *
   * by EnumerateEA if a non-empty key is specified.  In this case the    *
   * EAsInReply output parameter will return one and there will only be   *
   * one T_enumerateEAwithKey structure in the dataBuffer.                *
   *----------------------------------------------------------------------*/
typedef struct tagT_enumerateEAwithKey
{
   LONG  valueLength;   /* length of entire EA */
   WORD  keyLength;
   LONG  accessFlags;
   LONG  keyExtants;
   LONG  valueExtants;  /* EA extents */
   char  keyValue[1];   /* length of this field is given by keyLength */
} T_enumerateEAwithKey;

#include <npackoff.h>

#ifdef __cplusplus
extern "C" {
#endif

extern int CloseEA
(
   int handle
);

extern int CopyEA
(
   const char *srcPath, 
   const char *destPath,
   int         destVolumeNumber,
   LONG        destDirectoryNumber,
   LONG       *EAcount,
   LONG       *EAdataSize,
   LONG       *EAkeySize
);
                              
extern int EnumerateEA
(
   int         handle,
   const char *keyBuffer,
   BYTE       *dataBuffer,
   LONG        dataBufferSize,
   int         startPosition,
   LONG       *dataSize,
   LONG       *EAsInReply
);

extern int GetEAInfo
(
   int   handle,
   LONG *totalEAs,
   LONG *totalDataSizeOfEAs,
   LONG *totalKeySizeOfEAs
);
                 
extern int OpenEA
(
   const char *path, 
   LONG        reserved
);
                 
extern int ReadEA
(
   int         handle,
   const char *keyBuffer,
   char       *dataBuffer,
   LONG        dataBufferSize,
   LONG       *accessFlags
);
                 
extern int WriteEA
(
   int         handle,
   const char *keyBuffer,
   const char *dataBuffer,
   LONG        dataBufferSize,
   LONG        accessFlags
);
      
#ifdef __cplusplus
}
#endif

#endif
