#ifndef _NWDYNARR_H_
#define _NWDYNARR_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwdynarr.h
==============================================================================
*/

#ifndef NULL
#  define NULL 0
#endif

#define DYN_FREE  0
#define DYN_INUSE 1 /* may be any non-zero value */


#ifndef _SIZE_T
#  define _SIZE_T
typedef unsigned int size_t;
#endif

  /*------------------------------------------------------------------------*
   * Model of a Dynamic array element definition.  A program would not      *
   * actually use this structure but rather one suited to it's requirements.*
   * This structure just illustrates that a DynArray's element may have     *
   * any structure, however the first field must be four bytes and it       *
   * must be non-zero whenever that element is in use                       *
   *------------------------------------------------------------------------*/
typedef struct tagT_DYNARRAY
{
   unsigned int DYNinUse; /* non-zero means entry is use       */
                          /* zero means free                   */
                          /* application may use this field as */
                          /* long as value is always non-zero  */

   /* application may define other fields */
} T_DYNARRAY;


#ifdef __cplusplus
extern "C"
{
#endif

  /*-----------------------------------------------------------------------*
   * Model DynArray Block definition (DAB).  A DAB describes a DynArray.   *
   * Since a DAB points to a DynArray element, which is developer defined, *
   * the DAB must be customized for a particular use.  The GEN macros below*
   * do this automatically so their use is recommended.                    *
   *                                                                       *
   * A DAB must always be passed to the DynArray functions.                *
   *-----------------------------------------------------------------------*/
typedef struct tagT_DYNARRAY_BLOCK
{
   void *DABarrayP;
   int   DABnumSlots;
   int   DABelementSize;
   void *(*DABrealloc)(void *, size_t);
   int   DABgrowAmount;
   int   DABnumEntries;
} T_DYNARRAY_BLOCK;

  /*-------------------------------------------------------------------------*
   * DynArray function prototypes.  Note that the macros defined below are   *
   * necessary to prevent compile errors since a T_DYNARRAY_BLOCK is only    *
   * a model.                                                                *
   *-------------------------------------------------------------------------*/
extern int AllocateDynArrayEntry
(
   T_DYNARRAY_BLOCK *dabP
);

extern int AllocateGivenDynArrayEntry
(
   T_DYNARRAY_BLOCK *dabP,
   int               ndx
);

extern int DeallocateDynArrayEntry
(
   T_DYNARRAY_BLOCK *dabP,
   int               ndx
);

#ifdef __cplusplus
}
#endif


  /*-------------------------------------------------------------------------*
   * Macros to prevent compile errors when the DynArray functions are used   *
   *-------------------------------------------------------------------------*/
#define AllocateDynArrayEntry( dabP ) \
        AllocateDynArrayEntry( ((T_DYNARRAY_BLOCK *)dabP) )

#define AllocateGivenDynArrayEntry( dabP, ndx ) \
        AllocateGivenDynArrayEntry( ((T_DYNARRAY_BLOCK *)dabP), ndx )

#define DeallocateDynArrayEntry( dabP, ndx ) \
        DeallocateDynArrayEntry( ((T_DYNARRAY_BLOCK *)dabP), ndx )

/*----------------------------------------------------------------------------*
 *                                                                            *
 *   GEN_DYNARRAY_BLOCK - macro for generating a DAB                          *
 *                                                                            *
 *   Usage:                                                                   *
 *                                                                            *
 *      GEN_DYNARRAY_BLOCK( elementType, varName, defDec )                    *
 *                                                                            *
 *   Arguments:                                                               *
 *                                                                            *
 *      elementType -  the C type of the element (int, struct,                *
 *                     typedef, etc.)                                         *
 *                                                                            *
 *      varName     -  the name of the variable being declared                *
 *                     as a dynamic array                                     *
 *                                                                            *
 *      defDec      -  may be DECLARE, DEFINE, or INIT                        *
 *                                                                            *
 *         DECLARE  -  declares the specified variable as the                 *
 *                     type of DAB specified.  Generates:                     *
 *                        struct varName##Struct varName                      *
 *                                                                            *
 *         DEFINE( realloc, growAmount ) - defines and initializes the        *
 *                     specified variable as the type of DAB specified.       *
 *                                                                            *
 *                     GEN_DYNARRAY_BLOCK generates:                          *
 *                                                                            *
 *                        struct varName##Struct                              *
 *                           {                                                *
 *                           elementType   *DABarrayP;                        *
 *                           int            DABnumSlots;                      *
 *                           int            DABelementSize;                   *
 *                           void          *(*DABrealloc) (void *, size_t);   *
 *                           int            DABgrowAmount;                    *
 *                           int            DABnumEntries;                    *
 *                           } varName = {NULL, 0, elementSize, realloc,      *
 *                                        growAmount, 0 }                     *
 *                                                                            *
 *         INIT( realloc, growAmount ) - initializes the (already defined)    *
 *                        DAB. Generates: struct varName##Struct varName =    *
 *                           {NULL, 0, elementSize, realloc, growAmount, 0 }  *
 *                                                                            *
 *         Parms for the DEFINE and INIT parameters:                          *
 *                                                                            *
 *            realloc - is the reallocation function to use when the dynarray *
 *                      is expanded; normally this is "realloc"               *
 *                                                                            *
 *            growAmount - is the amount to expand the dynarray by if         *
 *                         AllocateDynArrayEntry expands the array            *
 *                                                                            *
 *----------------------------------------------------------------------------*/

#define GEN_DYNARRAY_BLOCK( elementType, varName, defDec ) \
   struct varName##Struct                                  \
   __EXIST_##defDec                                        \
   (                                                       \
      {                                                    \
      elementType *DABarrayP;                              \
      int          DABnumSlots;                            \
      int          DABelementSize;                         \
      void        *(*DABrealloc)(void *, size_t);          \
      int          DABgrowAmount;                          \
      int          DABnumEntries;                          \
      }                                                    \
   )                                                       \
      varName __##defDec , sizeof( elementType ) )


  /*------------------------------------*
   * GEN_DYNARRAY_BLOCK helper macros   *
   *------------------------------------*/

#define __DECLARE ___DECLARE( ignore
#define ___DECLARE( ignore1, ignore2 )
#define __EXIST_DECLARE( body ) body

#define __DEFINE( realloc, growAmount ) ___DEFINE( realloc, growAmount
#define ___DEFINE( realloc, growAmount, elementSize ) = {NULL, 0, \
   elementSize, realloc, growAmount, 0 }
#define __EXIST_DEFINE( ignore1, ignore2 ) ___EXIST_DEFINE
#define ___EXIST_DEFINE( body ) body

#define __INIT( realloc, growAmount ) ___INIT( realloc, growAmount
#define ___INIT( realloc, growAmount, elementSize ) = {NULL, 0, elementSize,\
   realloc, growAmount, 0 }
#define __EXIST_INIT( ignore1, ignore2 ) ___EXIST_INIT
#define ___EXIST_INIT( body )


#endif
