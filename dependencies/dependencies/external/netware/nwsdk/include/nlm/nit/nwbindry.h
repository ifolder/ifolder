/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwbindry.h
==============================================================================
*/

#ifndef _NWBINDRY_H_
#define _NWBINDRY_H_

#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>


/**** Bindery security definitions ****/
#define BS_ANY_READ      0x0000 /* Readable by anyone                */
#define BS_LOGGED_READ   0x0001 /* Must be logged in to read         */
#define BS_OBJECT_READ   0x0002 /* Readable by same object or super  */
#define BS_SUPER_READ    0x0003 /* Readable by supervisor only       */
#define BS_BINDERY_READ  0x0004 /* Readable only by the bindery      */
#define BS_ANY_WRITE     0x0000 /* Writeable by anyone               */
#define BS_LOGGED_WRITE  0x0010 /* Must be logged in to write        */
#define BS_OBJECT_WRITE  0x0020 /* Writeable by same object or super */
#define BS_SUPER_WRITE   0x0030 /* Writeable only by the supervisor  */
#define BS_BINDERY_WRITE 0x0040 /* Writeable by the bindery only     */

/* Bindery object type definitions */
#define OT_WILD                        (-1)    /* Matches any type            */
#define OT_UNKNOWN                     0x0000  /* Unknown object type         */
#define OT_USER                        0x0001  /* The object is a "user"      */
#define OT_USER_GROUP                  0x0002  /* A group of users            */
#define OT_GROUP                       0x0002
#define OT_PRINT_QUEUE                 0x0003  /* Services print queues       */
#define OT_FILE_SERVER                 0x0004  /* The object serves files     */
#define OT_JOB_SERVER                  0x0005
#define OT_GATEWAY                     0x0006
#define OT_PRINT_SERVER                0x0007
#define OT_ARCHIVE_QUEUE               0x0008
#define OT_ARCHIVE_SERVER              0x0009  /* Services backup jobs        */
#define OT_JOB_QUEUE                   0x000A
#define OT_ADMINISTRATION              0x000B
#define OT_NAS_SNA_GATEWAY             0x0021
#define OT_REMOTE_BRIDGE_SERVER        0x0024
#define OT_TCPIP_GATEWAY               0x0027
#define OT_TIME_SYNCHRONIZATION_SERVER 0x002D
#define OT_ARCHIVE_SERVER_DYNAMIC_SAP  0x002E
#define OT_ADVERTISING_PRINT_SERVER    0x0047
#define OT_BTRIEVE_VAP                 0x004B
#define OT_NWSQL_VAP                   0x004C
#define OT_PRINT_QUEUE_USER            0x0053

/* Attributes of objects and properties in the bindery */
#define BF_STATIC  0x0000
#define BF_DYNAMIC 0x0001
#define BF_ITEM    0x0000
#define BF_SET     0x0002

/* Maximum lengths of object, properties, (includes terminating null) */
#define BL_OBJECT   48
#define BL_PROPERTY 16
#define BL_PASSWORD 128


#ifdef __cplusplus
extern "C" {
#endif

extern int AddBinderyObjectToSet
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   const char *memberName,
   WORD        memberType
);

extern int ChangeBinderyObjectPassword
(
   const char *objectName,
   WORD        objectType,
   const char *oldPassword,
   const char *newPassword
);

extern int ChangeBinderyObjectSecurity
(
   const char *objectName,
   WORD        objectType,
   BYTE        newObjectSecurity
);

extern int ChangePropertySecurity
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   BYTE        newPropertySecurity
);

extern int CloseBindery
(
   void
);

extern int CreateBinderyObject
(
   const char *objectName,
   WORD        objectType,
   BYTE        objectFlag,
   BYTE        objectSecurity
);

extern int CreateProperty
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   BYTE        propertyFlags,
   BYTE        propertySecurity
);

extern int DeleteBinderyObject
(
   const char *objectName,
   WORD        objectType
);

extern int DeleteBinderyObjectFromSet
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   const char *memberName,
   WORD        memberType
);

extern int DeleteProperty
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName
);

extern int GetBinderyAccessLevel
(
   BYTE       *accessLevel,
   long       *objectID
);

extern int GetBinderyObjectID
(
   const char *objectName,
   WORD        objectType,
   long       *objectID
);

extern int GetBinderyObjectName
(
   long        objectID,
   char       *objectName,
   WORD       *objectType
);

extern int IsBinderyObjectInSet
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   const char *memberName,
   WORD        memberType
);

extern int OpenBindery
(
   void
);

extern int ReadPropertyValue
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   int         segmentNumber,
   BYTE       *propertyValue,
   BYTE       *moreSegments,
   BYTE       *propertyFlags
);

extern int RenameBinderyObject
(
   const char *objectName,
   const char *newObjectName,
   WORD        objectType
);

extern int ScanBinderyObject
(
   const char *searchObjectName,
   WORD        searchObjectType,
   long       *objectID,
   char       *objectName,
   WORD       *objectType,
   char       *objectHasProperties,
   char       *objectFlag,
   char       *objectSecurity
);

extern int ScanProperty
(
   const char *objectName,
   WORD        objectType,
   const char *searchPropertyName,
   long       *sequenceNumber,
   char       *propertyName,
   char       *propertyFlags,
   char       *propertySecurity,
   char       *propertyHasValue,
   char       *moreProperties
);

extern int VerifyBinderyObjectPassword
(
   const char *objectName,
   WORD        objectType,
   const char *password
);

extern int WritePropertyValue
(
   const char *objectName,
   WORD        objectType,
   const char *propertyName,
   int         segmentNumber,
   const BYTE *propertyValue,
   BYTE        moreSegments
);

#ifdef __cplusplus
}
#endif


#endif
