/******************************************************************************

  %name: nwsm.h %
  %version: 5 %
  %date_modified: Mon Oct 18 11:01:32 1999 %
  $Copyright:

  Copyright (c) 1989-1996 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/
#if ! defined ( NWSM_H )
#define NWSM_H

#if ! defined ( NTYPES_H )
#include "ntypes.h"
#endif

#if ! defined ( NWAPIDEF_H )
#include "nwapidef.h"
#endif

#if ! defined ( NWCALDEF_H )
#include "nwcaldef.h"
#endif

#if ! defined ( NWNAMSPC_H )
#include "nwnamspc.h"
#endif

#include "npackon.h"

#ifdef __cplusplus
extern "C" {
#endif

#define LOAD_COULD_NOT_FIND_FILE            1
#define LOAD_ERROR_READING_FILE             2
#define LOAD_NOT_NLM_FILE_FORMAT            3
#define LOAD_WRONG_NLM_FILE_VERSION         4
#define LOAD_REENTRANT_INITIALIZE_FAILURE   5
#define LOAD_CAN_NOT_LOAD_MULTIPLE_COPIES   6
#define LOAD_ALREADY_IN_PROGRESS            7
#define LOAD_NOT_ENOUGH_MEMORY              8
#define LOAD_INITIALIZE_FAILURE             9
#define LOAD_INCONSISTENT_FILE_FORMAT       10
#define LOAD_CAN_NOT_LOAD_AT_STARTUP        11
#define LOAD_AUTO_LOAD_MODULES_NOT_LOADED   12
#define LOAD_UNRESOLVED_EXTERNAL            13
#define LOAD_PUBLIC_ALREADY_DEFINED         14
#define LOAD_XDC_DATA_ERROR                 15
#define LOAD_NOT_OS_DOMAIN                  16

N_EXTERN_LIBRARY( NWCCODE )
NWSMLoadNLM
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * loadCommand
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMLoadNLM2
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * loadCommand,
   pnuint32            loadNLMReturnCode
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMUnloadNLM
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * NLMName
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMMountVolume
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * volumeName,
   pnuint32            volumeNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMDismountVolumeByNumber
(
   NWCONN_HANDLE     connHandle,
   nuint16           volumeNumber
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMDismountVolumeByName
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * volumeName
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMAddNSToVolume
(
   NWCONN_HANDLE     connHandle,
   nuint16           volNumber,
   nuint8            namspc
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMSetDynamicCmdStrValue
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * setCommandName,
   const nstr8 N_FAR * cmdValue
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMSetDynamicCmdIntValue
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * setCommandName,
   nuint32             cmdValue
);

N_EXTERN_LIBRARY( NWCCODE )
NWSMExecuteNCFFile
(
   NWCONN_HANDLE       connHandle,
   const nstr8 N_FAR * NCFFileName
);

#include "npackoff.h"
#ifdef __cplusplus
}
#endif
#endif
