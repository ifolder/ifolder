/******************************************************************************

  %name: nwbindry.h %
  %version: 7 %
  %date_modified: Tue Feb 10 13:24:44 2004 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWBINDRY_H )
#define NWBINDRY_H

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NUNICODE_H )
#include "nunicode.h"
#endif


#ifdef __cplusplus
extern "C" {
#endif

/* Bindery object types (in HIGH-LOW order) */
#define OT_WILD                 0xFFFF
#define OT_UNKNOWN              0x0000
#define OT_USER                 0x0100
#define OT_USER_GROUP           0x0200
#define OT_PRINT_QUEUE          0x0300
#define OT_FILE_SERVER          0x0400
#define OT_JOB_SERVER           0x0500
#define OT_GATEWAY              0x0600
#define OT_PRINT_SERVER         0x0700
#define OT_ARCHIVE_QUEUE        0x0800
#define OT_ARCHIVE_SERVER       0x0900
#define OT_JOB_QUEUE            0x0A00
#define OT_ADMINISTRATION       0x0B00
#define OT_NAS_SNA_GATEWAY      0x2100
#define OT_REMOTE_BRIDGE_SERVER 0x2600
#define OT_TCPIP_GATEWAY        0x2700
#define OT_TREE_NAME            0x7802

/* Extended bindery object types */
#define OT_TIME_SYNCHRONIZATION_SERVER 0x2D00
#define OT_ARCHIVE_SERVER_DYNAMIC_SAP  0x2E00
#define OT_ADVERTISING_PRINT_SERVER    0x4700
#define OT_BTRIEVE_VAP                 0x5000
#define OT_PRINT_QUEUE_USER            0x5300


/* Bindery object and property flags */
#define BF_STATIC   0x00
#define BF_DYNAMIC  0x01
#define BF_ITEM     0x00
#define BF_SET      0x02

/*********  Bindery object and property security access levels  **********/
#define BS_ANY_READ      0x00   /* Readable by anyone                */
#define BS_LOGGED_READ   0x01   /* Must be logged in to read         */
#define BS_OBJECT_READ   0x02   /* Readable by same object or super  */
#define BS_SUPER_READ    0x03   /* Readable by supervisor only       */
#define BS_BINDERY_READ  0x04   /* Readable only by the bindery      */
#define BS_ANY_WRITE     0x00   /* Writeable by anyone               */
#define BS_LOGGED_WRITE  0x10   /* Must be logged in to write        */
#define BS_OBJECT_WRITE  0x20   /* Writeable by same object or super */
#define BS_SUPER_WRITE   0x30   /* Writeable only by the supervisor  */
#define BS_BINDERY_WRITE 0x40   /* Writeable by the bindery only     */

N_EXTERN_LIBRARY( NWCCODE )
NWVerifyObjectPassword
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * password
);

N_EXTERN_LIBRARY( NWCCODE )
NWDisallowObjectPassword
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * disallowedPassword
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeObjectPassword
(
  NWCONN_HANDLE       conn,
  const nstr8 N_FAR * objName,
  nuint16             objType,
  const nstr8 N_FAR * oldPassword,
  const nstr8 N_FAR * newPassword
);

N_EXTERN_LIBRARY( NWCCODE )
NWReadPropertyValue
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName,
   nuint8              segmentNum,
   pnuint8             segmentData,
   pnuint8             moreSegments,
   pnuint8             flags
);

N_EXTERN_LIBRARY( NWCCODE )
NWWritePropertyValue
(
   NWCONN_HANDLE        conn,
   const nstr8  N_FAR * objName,
   nuint16              objType,
   const nstr8  N_FAR * propertyName,
   nuint8               segmentNum,
   const nuint8 N_FAR * segmentData,
   nuint8               moreSegments
);

N_EXTERN_LIBRARY( NWCCODE )
NWAddObjectToSet
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName,
   const nstr8 N_FAR * memberName,
   nuint16             memberType
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteObjectFromSet
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   pnstr8              propertyName,
   pnstr8              memberName,
   nuint16             memberType
);

N_EXTERN_LIBRARY( NWCCODE )
NWIsObjectInSet
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName,
   const nstr8 N_FAR * memberName,
   nuint16             memberType
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanProperty
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   pnstr8              searchPropertyName,
   pnuint32            iterHandle,
   pnstr8              propertyName,
   pnuint8             propertyFlags,
   pnuint8             propertySecurity,
   pnuint8             valueAvailable,
   pnuint8             moreFlag
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectID
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   pnuint32            objID
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectDiskSpaceLeft
(
   NWCONN_HANDLE  conn,
   nuint32        objID,
   pnuint32       systemElapsedTime,
   pnuint32       unusedDiskBlocks,
   pnuint8        restrictionEnforced
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectName
(
   NWCONN_HANDLE  conn,
   nuint32        objID,
   pnstr8         objName,
   pnuint16       objType
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanObject
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * searchName,
   nuint16             searchType,
   pnuint32            objID,
   pnstr8              objName,
   pnuint16            objType,
   pnuint8             hasPropertiesFlag,
   pnuint8             objFlags,
   pnuint8             objSecurity
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetBinderyAccessLevel
(
   NWCONN_HANDLE  conn,
   pnuint8        accessLevel,
   pnuint32       objID
);

N_EXTERN_LIBRARY( NWCCODE )
NWCreateProperty
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName,
   nuint8              propertyFlags,
   nuint8              propertySecurity
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteProperty
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangePropertySecurity
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   const nstr8 N_FAR * propertyName,
   nuint8              newPropertySecurity
);

N_EXTERN_LIBRARY( NWCCODE )
NWCreateObject
(
   NWCONN_HANDLE  conn,
   pnstr8         objName,
   nuint16        objType,
   nuint8         objFlags,
   nuint8         objSecurity
);

N_EXTERN_LIBRARY( NWCCODE )
NWDeleteObject
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType
);

N_EXTERN_LIBRARY( NWCCODE )
NWRenameObject
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * oldObjName,
   const nstr8 N_FAR * newObjName,
   nuint16             objType
);

N_EXTERN_LIBRARY( NWCCODE )
NWChangeObjectSecurity
(
   NWCONN_HANDLE       conn,
   const nstr8 N_FAR * objName,
   nuint16             objType,
   nuint8              newObjSecurity
);

N_EXTERN_LIBRARY( NWCCODE )
NWOpenBindery
(
   NWCONN_HANDLE  conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWCloseBindery
(
   NWCONN_HANDLE conn
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanObjectTrusteePaths
(
   NWCONN_HANDLE  conn,
   nuint32        objID,
   nuint16        volNum,
   pnuint16       iterHandle,
   pnuint8        accessRights,
   pnstr8         dirPath
);

N_EXTERN_LIBRARY( NWCCODE )
NWScanObjectTrusteePathsExt
(
   NWCONN_HANDLE  conn,
   nuint32        objID,
   nuint16        volNum,
   pnuint16       iterHandle,
   pnuint8        accessRights,
   pnstr8         dirPath1506
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectEffectiveRights
(
   NWCONN_HANDLE       conn,
   nuint32             objID,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   pnuint16            rightsMask
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectEffectiveRightsExt
(
   NWCONN_HANDLE       conn,
   nuint32             objID,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   nuint8              buNameSpace,
   pnuint16            rightsMask
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectEffectiveRights2
(
   NWCONN_HANDLE       conn,
   nuint32             objID,
   NWDIR_HANDLE        dirHandle,
   const nstr8 N_FAR * path,
   pnuint16            rightsMask
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesBeginA
(
   nuint32        luObjectType,
	pnuint32       pluHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesNextA
(
   nuint32        luHandle,
	pnuint32       pluLenBuffer,
	pnstr8         strBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesEndA
(
   nuint32        luHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesBeginW
(
   nuint32        luObjectType,
	pnuint32       pluHandle
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesNextW
(
   nuint32        luHandle,
	pnuint32       pluLenBuffer,
	punicode       strBuffer
);

N_EXTERN_LIBRARY( NWCCODE )
NWGetObjectNamesEndW
(
   nuint32        luHandle
);

#ifdef __cplusplus
}
#endif

#endif
