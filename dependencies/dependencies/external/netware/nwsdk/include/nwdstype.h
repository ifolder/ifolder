/******************************************************************************

  %name: nwdstype.h %
  %version: 4 %
  %date_modified: Wed Dec 18 12:08:04 1996 %
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
#if ! defined ( NWDSTYPE_H )
#define NWDSTYPE_H

#ifndef NWDSCCODE
#define NWDSCCODE    int
#endif

/*******************************************************************/
/*                                                                 */
/*    !!!!! The following types have been obsoleted !!!!!!!!!      */
/*                                                                 */
/*   The following have been obsoleted - use types found in        */
/*   ntypes.h                                                      */
/*                                                                 */
/*   ntypes.h contains equivalent types for each of the typedefs   */
/*   listed below.  For example "uint32" is "nuint32" in ntypes.h  */
/*                                                                 */
/*   These typedefs also conflicted with defines in the WinSock2   */
/*   headers on NetWare.  The decision was made to obsolete these  */
/*   types to eliminate conflicts.                                 */ 
/*******************************************************************/
#ifdef INCLUDE_OBSOLETE

typedef unsigned long uint32;
typedef signed long int32;
typedef unsigned short uint16;
typedef signed short int16;
typedef unsigned char uint8;
typedef signed char int8;

#ifndef NWUNSIGNED
#define NWUNSIGNED unsigned
#endif

#endif /* #ifdef INCLUDE_OBSOLETE */

#endif /* NWDSTYPE_H */
