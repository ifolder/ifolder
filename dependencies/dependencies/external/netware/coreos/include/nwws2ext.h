/******************************************************************************

  $Workfile:   nwws2ext.h  $
  $Revision$
  $Modtime::   Jan 07 1998 13:02:16                       $
  $Copyright:

  Copyright (c) 1997 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

// This file contains proposed extensions to the Winsock 2 specification to
// support Novell's implementation of namespace providers.

#ifndef _NWWS2EXT_H
#define _NWWS2EXT_H


//___[ Manifest constants ]________________________________________________________________________

// Proposed output flag for deregistered services
#ifndef RESULT_IS_DEREGISTERED
#define RESULT_IS_DEREGISTERED 0x0002
#endif

// Proposed output flag for containers
#ifndef RESULT_IS_CONTAINER
#define RESULT_IS_CONTAINER 0x0004
#endif

// Values used to indicate an attribute list in the blob
#define WS_ATTRLIST_ASCII    0xb10bea1a     // blob contains ASCII strings
#define WS_ATTRLIST_UNICODE  0xb10bea10     // blob contains UNICODE strings

//	Name Spaces
// Extends definitions in WINSOCK2.H

#define NS_BINDERY						(4)
#define NS_SLP                      (5)

// Predefined BLOB Value Types
// Extends Predefined Value Types in winnt.h
//
//#define REG_SZ                   ( 1 )   // Unicode NULL terminated string
//#define REG_BINARY               ( 3 )   // Free form binary
//#define REG_DWORD                ( 4 )   // 32-bit number

#define REG_BOOL							( 11 )  // Boolian value; TRUE or FALSE
#define REG_KEYWORD						( 12 )  // Keyword with no value

//___[ Macros ]____________________________________________________________________________________

#define IS_ASCII_BLOBATTRLIST(p) (WS_ATTRLIST_ASCII == (p)->dwSignature)
#define IS_UNICODE_BLOBATTRLIST(p) (WS_ATTRLIST_UNICODE == (p)->dwSignature)
#define IS_BLOBATTRLIST(p) (IS_ASCII_BLOBATTRLIST(p) || IS_UNICODE_BLOBATTRLIST(p))

//
// After a successful return from WSALookupServiceNext, use the following macro to convert the
// offsets in the WSABLOBATTRLIST to pointers:
//
//    ADJUST_BLOB_POINTERS(resultSet->lpBlob);
// 
// If there is no blob, or it is not a WSABLOBATTRLIST, the macro will do nothing.
//
#define ADJUST_BLOB_POINTERS(p) { \
   LPWSABLOBATTRLIST pBase; \
   if ((p) && (pBase = (LPWSABLOBATTRLIST)(p)->pBlobData) && IS_BLOBATTRLIST(pBase)) { \
      DWORD i; \
      ADJUST_BLOB_PTR(pBase->lpAttributes, LPWSAATTRINFO); \
      for (i = 0; i < pBase->dwAttrCount; i++) \
         ADJUST_BLOB_PTR(pBase->lpAttributes[i].lpszName, LPTSTR), \
         ADJUST_BLOB_PTR(pBase->lpAttributes[i].lpValue, LPBYTE); \
   } \
}
#define ADJUST_BLOB_PTR(p,type) (p ? p = (type)(((LPBYTE)pBase) + (DWORD)(p)):0)

//___[ Type definitions ]__________________________________________________________________________

typedef WSANSCLASSINFO WSAATTRINFO, *LPWSAATTRINFO;

// Structure of a blob containing an attribute list
typedef struct _WSABlobAttrList {
   DWORD           dwSignature;     // Identifies the blob as an attribute list
   DWORD           dwAttrCount;     // Number of attributes present
   LPWSAATTRINFO   lpAttributes;    // Pointer to attribute array
} WSABLOBATTRLIST, *LPWSABLOBATTRLIST;

#endif // _NWWS2EXT_H

//_________________________________________________________________________________________________
//_________________________________________________________________________________________________
