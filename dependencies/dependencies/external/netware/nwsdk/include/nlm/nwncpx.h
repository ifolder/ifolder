#ifndef _NWNCPX_H_
#define _NWNCPX_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwncpx.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* ---------------------------------------------------------------------------
** Note: This file now holds the contents of nwncpext.h.
** ---------------------------------------------------------------------------
*/

#define MAX_NCP_EXTENSION_NAME_BYTES 33

#define BEGIN_SCAN_NCP_EXTENSIONS    0xFFFFFFFF
#define REPLY_BUFFER_IS_FRAGGED      0xFFFFFFFF

#define CONNECTION_BEING_RESTARTED   0x01101001 
#define CONNECTION_BEING_KILLED      0x02202002 
#define CONNECTION_BEING_LOGGED_OUT  0x03303003 
#define CONNECTION_BEING_FREED       0x04404004 


typedef struct NCPExtensionClient NCPExtensionClient;

struct NCPExtensionClient
{
   LONG connection;
   LONG task;
};

typedef struct FragElement FragElement;

struct FragElement
{
   void *ptr;
   LONG size;
};

typedef struct NCPExtensionMessageFrag NCPExtensionMessageFrag;

struct NCPExtensionMessageFrag
{
   LONG totalMessageSize;
   LONG fragCount;
   struct FragElement fragList[4];
};


#ifdef __cplusplus
extern "C" {
#endif

extern int NWDeRegisterNCPExtension
(
   void *queryData
);

extern int NWGetNCPExtensionInfo
(
   const char *NCPExtensionName,
   LONG *NCPExtensionID,
   BYTE *majorVersion,
   BYTE *minorVersion,
   BYTE *revision,
   void *queryData
);

extern int NWGetNCPExtensionInfoByID
(
   LONG NCPExtensionID,
   char *NCPExtensionName,
   BYTE *majorVersion,
   BYTE *minorVersion,
   BYTE *revision,
   void *queryData
);

extern int NWRegisterNCPExtension
(
   const char *NCPExtensionName,
   BYTE      (*NCPExtensionHandler)(
      struct NCPExtensionClient *NCPExtensionClient,
      void *requestData,
      LONG requestDataLen,
      void *replyData,
      LONG *replyDataLen),
   void      (*ConnectionEventHandler)(
      LONG connection,
      LONG eventType),
   void      (*ReplyBufferManager)(
      struct NCPExtensionClient *NCPExtensionClient,
      void *replyBuffer),
   BYTE        majorVersion,
   BYTE        minorVersion,
   BYTE        revision,
   void      **queryData
);

extern int NWRegisterNCPExtensionByID
(
   LONG        NCPExtensionID,
   const char *NCPExtensionName,
   BYTE      (*NCPExtensionHandler)(
      struct NCPExtensionClient *NCPExtensionClient,
      void *requestData,
      LONG requestDataLen,
      void *replyData,
      LONG *replyDataLen),
   void      (*ConnectionEventHandler)(
      LONG connection,
      LONG eventType),
   void      (*ReplyBufferManager)(
      struct NCPExtensionClient *NCPExtensionClient,
      void *replyBuffer),
   BYTE        majorVersion,
   BYTE        minorVersion,
   BYTE        revision,
   void      **queryData
);

extern int NWScanNCPExtensions
(
   LONG *NCPExtensionID,
   char *NCPExtensionName,
   BYTE *majorVersion,
   BYTE *minorVersion,
   BYTE *revision,
   void *queryData
);

extern int NWSendNCPExtensionFraggedRequest
(
   LONG                                 NCPExtensionID,
   const struct NCPExtensionMessageFrag *requestFrag,
   struct NCPExtensionMessageFrag       *replyFrag
);

extern int NWSendNCPExtensionRequest
(
   LONG        NCPExtensionID,
   const void *requestData,
   LONG        requestDataLen,
   void       *replyData,
   LONG       *replyDataLen
);

#ifdef __cplusplus
}
#endif


#endif
