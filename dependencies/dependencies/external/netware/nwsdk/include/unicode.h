
/*--------------------------------------------------------------------------

   %name: unicode.h %
   %version: 15 %
   %date_modified: Thu Feb 21 12:47:36 2002 %
   $Copyright:

   Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.

   THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
   TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
   COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
   EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
   WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
   OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
   CRIMINAL AND CIVIL LIABILITY.$

--------------------------------------------------------------------------*/


#if !defined( UNICODE_H )
#define UNICODE_H

#include <stddef.h>     /* size_t */

#ifndef _SIZE_T_DEFINED
#define _SIZE_T_DEFINED
# ifndef _SIZE_T
# define _SIZE_T
#  ifndef __size_t
#  define __size_t
typedef unsigned int size_t;
#  endif
# endif
#endif

#ifdef N_PLAT_OS2
#  define PN_API * N_API     /* OS2 requires different declaration than MS  */
#else
#  define PN_API N_API *
#endif

#if !defined( NTYPES_H )
#  include "ntypes.h"
#endif

#if !defined _CONVERT_H
/*
    Data types
*/
#ifndef UNICODE_TYPE_DEFINED
#define UNICODE_TYPE_DEFINED
typedef unsigned short unicode;		/* Unicode data must be 16 bits   */
#endif       
typedef unicode N_FAR * punicode;
typedef unicode N_FAR * N_FAR * ppunicode;

#define nwunisize(x) (sizeof(x)/sizeof(unicode))

/*
    Converter handle
*/

typedef void *CONVERT;
typedef CONVERT N_FAR * pCONVERT;

#endif


/****************************************************************************/
/*
    Type definitions for converter based APIs
*/

/*
    IBM C/Set compiler requires   "typedef nint (* N_API xxx)"
    MSW and NLM platforms require "typedef nint (N_API * xxx)"
    The PN_API symbol is defined to satisfy both situations.
*/

/*
    Function called when non-mappable bytes are found
*/
typedef nint (PN_API NMBYTE)
(
    pCONVERT             byteUniHandle,/* Handle to Byte <-> Uni converter  */
    punicode             output,       /* Pointer to current output position*/
    nuint                outputLeft,   /* Space left in output buffer       */
    pnuint               outputUsed,   /* Space used in output buffer       */
    nuint8       N_FAR * badInput,     /* Pointer to unmappable bytes       */
    nuint                badInputSize  /* Size of unmappable input          */
);

/*
    Function called when non-mappable unicode characters are found
*/
typedef nint (PN_API NMUNI)
(
    pCONVERT             byteUniHandle,/* Handle to Byte <-> Uni converter  */
    pnuint8              output,       /* Pointer to current output position*/
    nuint                outputLeft,   /* Space left in output buffer       */
    pnuint               outputUsed,   /* Space used in output buffer       */
    unicode      N_FAR * badInput,     /* Ptr to unmappable unicode chars   */
    nuint                badInputSize  /* Size of unmappable input          */
);

/*
    Function called to scan for special byte input
*/
typedef pnuint8 (PN_API SCBYTE)
(
    pCONVERT             byteUniHandle,/* Handle to Byte <-> Uni converter  */
    nuint8       N_FAR * input,        /* Input to scan for special bytes   */
    nint                 scanmax       /* Maximum # of bytes to scan or -1  */
);

/*
    Function called to scan for special Unicode input
*/
typedef punicode (PN_API SCUNI)
(
    pCONVERT              byteUniHandle,/* Handle to Byte <-> Uni converter */
    unicode       N_FAR * input,        /* Input to scan for special chars  */
    nint                  scanmax       /* Maximum # of bytes to scan or -1 */
);

/*
    Function called to parse special byte input
*/
typedef nint (PN_API PRBYTE)
(
    pCONVERT             byteUniHandle,/* Handle to Byte <-> Uni converter  */
    punicode             output,       /* Buffer for Unicode output         */
    nuint                outputleft,   /* Space left in output buffer       */
    pnuint               outputUsed,   /* Space used in output buffer       */
    nuint8       N_FAR * input,        /* Buffer containing byte input      */
    pnuint               inputUsed     /* Number of bytes of input used     */
);

/*
    Function called to parse special Unicode input
*/
typedef nint (PN_API PRUNI)
(
    pCONVERT              byteUniHandle,/* Handle to Byte <-> Uni converter */
    pnuint8               output,       /* Buffer for bytes output          */
    nuint                 outputLeft,   /* Space left in output buffer      */
    pnuint                outputUsed,   /* Space used in output buffer      */
    unicode       N_FAR * input,        /* Buffer containing byte input     */
    pnuint                inputUsed     /* Number of Unicodes of input used */
);


/****************************************************************************/
/*
   Macros used by and returned from converter based API calls 
	(i.e. NWUS*, NWUX*)
*/

/*
   Novell-defined Unicode characters.
   Consult with the Internationalization group before adding to this list.
*/
#define UNI_CHANGE_NAMESPACE    0xf8f4
#define UNI_PREVIOUS_DIR        0xf8f5
#define UNI_CURRENT_DIR         0xf8f6
#define UNI_PATH_SEPARATOR      0xF8F7
#define UNI_VOLUMENAME_ROOT     0xf8f8
#define UNI_VOLUME_ROOT         0xf8f9
#define UNI_NDS_ROOT            0xf8fa
#define UNI_WILD_QMARK          0xf8fb
#define UNI_WILD_ASTERISK       0xf8fc
#define UNI_WILD_AUG_QMARK      0xf8fd
#define UNI_WILD_AUG_ASTERISK   0xf8fe
#define UNI_WILD_AUG_PERIOD     0xf8ff

/*
    Actions to take when an unmappable byte or uni character is encountered.
    Used in SetNoMapAction call.
*/
#define NWU_UNCHANGED_ACTION -1 /* Leave action unchanged                   */
#define NWU_RETURN_ERROR    0   /* Return error code NWU_UNMAPPABLE_CHAR    */
#define NWU_SUBSTITUTE      1   /* Use the current substitution character   */
#define NWU_CALL_HANDLER    2   /* Call the no map handler function         */

/*
    Codes to enable the Scan and Parse handler functions.
    Used in SetScanAction call.
*/
#define NWU_DISABLED        0   /* Disable Scan/Parse functions             */
#define NWU_ENABLED         2   /* Enable  Scan/Parse functions             */

/*
    Flags to pass to NWUXGetCaseConverter to specify whether to load
    a converter which converts to upper, lower or title case.
*/
#define NWU_LOWER_CASE      0   /* Lower case                               */
#define NWU_UPPER_CASE      1   /* Upper case                               */
#define NWU_TITLE_CASE      2   /* Title case                               */

/*
    Flags to pass to NWUXGetNormalizeConverter to specify whether to
    load a converter which converts to pre-composed or de-composed
    unicode characters.
*/
#define NWU_PRECOMPOSED     0   /* Precomposed                              */
#define NWU_DECOMPOSED      1   /* Decomposed                               */

/*
    For use in SetByte/UniFunction calls
*/
#define NWU_UNCHANGED_FUNCTION ((void N_FAR *)-1)
#define NWU_RESET_TO_DEFAULT   NULL

/*
    Error codes.  FFFFFDE0 to FFFFFDFF reserved for new unicode APIs.
*/
#define NWU_NO_CONVERTER         -544 /* Default converter not loaded   */
#define NWU_CONVERTER_NOT_FOUND  -543 /* Converter file was not found   */
#define NWU_TOO_MANY_FILES       -542 /* Too many open files            */
#define NWU_NO_PERMISSION        -541 /* Access to file was denied      */
#define NWU_OPEN_FAILED          -540 /* File open failed               */
#define NWU_READ_FAILED          -539 /* File read failed               */
#define NWU_OUT_OF_MEMORY        -538 /* Insufficient memory            */
#define NWU_CANT_LOAD_CONVERTER  -537 /* Unable to load converter       */
#define NWU_CONVERTER_CORRUPT    -536 /* The converter is invalid       */
#define NWU_NULL_HANDLE          -535 /* Converter handle was NULL      */
#define NWU_BAD_HANDLE           -534 /* Converter handle is invalid    */
#define NWU_HANDLE_MISMATCH      -533 /* Handle doesn't match operation */
#define NWU_UNMAPPABLE_CHAR      -532 /* Unmappable character found     */
#define NWU_RANGE_ERROR          -531 /* Invalid constant passed to fn  */
#define NWU_BUFFER_FULL          -530 /* Buffer too small for output    */
#define NWU_INPUT_MAX            -529 /* Processed max # of input chars */
#define UNI_PARSER_ERROR         -528 /* Error from user-written parser */
#define NWU_OLD_CONVERTER_VERSION -527 /* Outdated converter DLL        */
#define NWU_UNSUPPORTED_AUX_FUNCTION -526 /*  Unsupported AUX function  */
#define NWU_EMBEDDED_NULL        -525 /* Embedded null in len spec string */
#define NWU_GET_CODE_PAGE_FAILED -524 /* Failed to get system cp or cc    */

#define NWU_ILLEGAL_UTF8_CHARACTER -506 /* Cannot convert UTF8 char to Uni*/
#define NWU_INSUFFICIENT_BUFFER    -500


/*
   Error codes for translator based APIs (i.e. NW prefix)
*/
#define UNI_ALREADY_LOADED   -489  /* Already loaded another country or code page */
#define UNI_FUTURE_OPCODE    -490  /* Rule table has unimplimented rules*/
#define UNI_NO_SUCH_FILE     -491  /* No such file or directory */
#define UNI_TOO_MANY_FILES   -492  /* Too many files already open */
#define UNI_NO_PERMISSION    -493  /* Permission denied on file open */
#define UNI_NO_MEMORY        -494  /* Not enough memory */
#define UNI_LOAD_FAILED      -495  /* NWLoadRuleTable failed, don't know why */
#define UNI_HANDLE_BAD       -496  /* Rule table handle was bad */
#define UNI_HANDLE_MISMATCH  -497  /* Rule table handle doesn't match operation*/
#define UNI_RULES_CORRUPT    -498  /* Rule table is corrupt */
#define UNI_NO_DEFAULT       -499  /* No default rule and no 'No map' character*/
#define UNI_INSUFFICIENT_BUFFER -500
#define UNI_OPEN_FAILED      -501  /* Open failed in NWLoadRuleTable */
#define UNI_NO_LOAD_DIR      -502  /* Load directory could not be determined */
#define UNI_BAD_FILE_HANDLE  -503  /* File handle was bad */
#define UNI_READ_FAILED      -504  /* File read of rule table failed */
#define UNI_TRANS_CORRUPT    -505  /* Translator is corrupt */

#define UNI_ILLEGAL_UTF8_CHARACTER -506 /* Illegal UTF-8 character encountered */


#ifdef __cplusplus
extern "C" {
#endif

/****************************************************************************/
/*  
    Unicode converter prototypes - These APIs are preferred over the older
	 non-converter counterparts (i.e. NWUnicodeToLocal, NWLocalToUnicode, etc.)
*/
/*
    These are the Standard API's
*/
N_EXTERN_LIBRARY(nint) NWUSStandardUnicodeInit
(                               /* Initialize standard converters           */
    void
);

N_EXTERN_LIBRARY(nint) NWUSStandardUnicodeOverride
(                               /* Replace standard converter.              */
    nuint     codepage
);

N_EXTERN_LIBRARY(void) NWUSStandardUnicodeRelease
(                               /* Release the standard converters          */
    void
);

N_EXTERN_LIBRARY(nint) NWUSGetCodePage
(                               /* Get the native code page and country     */
    pnuint    pCodePage,
    pnuint    pCountry
);

/* NOTE:  The actualLength parameter returned by the conversion routines
          does *not* include the null terminator.
*/

N_EXTERN_LIBRARY(nint) NWUSByteToUnicode
(                               /* Convert bytes to Unicode                 */
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer. Or 0   */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUSByteToUnicodePath
(                               /* Convert bytes to Unicode for file path   */
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer. Or 0   */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUSLenByteToUnicode
(                               /* Convert bytes to Unicode                 */
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer. Or 0   */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    nint                 inLength,       /* Input str length in bytes or -1 */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUSLenByteToUnicodePath
(                               /* Convert bytes to Unicode for file path   */
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of  output buffer. Or 0  */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    nint                 inLength,       /* Input str length in bytes or -1 */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToByte
(                               /* Convert Unicode to bytes                 */
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToBytePath
(                               /* Convert Unicode to bytes for file path   */
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToUntermByte
(                               /* Convert Unicode to bytes                 */
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer        */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToUntermBytePath
(                               /* Convert Unicode to bytes for file path   */
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer        */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToLowerCase
(                               /* Convert Unicode to lower case            */
    punicode              lowerCaseOutput,/* Buffer for lower cased output  */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in uni chars */
);

N_EXTERN_LIBRARY(nint) NWUSUnicodeToUpperCase
(                               /* Convert Unicode to upper case            */
    punicode              upperCaseOutput,/* Buffer for upper cased output  */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in uni chars */
);

/*
    These are the Extended API's
*/
N_EXTERN_LIBRARY(nint) NWUXLoadByteUnicodeConverter
(                               /* Load a Byte <-> Unicode converter        */
    nuint            codepage,     /* Codepage number                       */
    pCONVERT N_FAR * byteUniHandle /* Converter handle returned here        */
);

N_EXTERN_LIBRARY(nint) NWUXLoadCaseConverter
(                               /* Load a Unicode -> Case converter         */
    nuint            caseFlag,   /* Want upper, lower or title casing?      */
    pCONVERT N_FAR * caseHandle  /* Converter handle returned here          */
);

N_EXTERN_LIBRARY(nint) NWUXLoadCollationConverter
(                               /* Load a Unicode -> Collation converter    */
    nuint            countryCode,    /* Country code for this locale        */
    pCONVERT N_FAR * collationHandle /* Converter handle returned here      */
);

N_EXTERN_LIBRARY(nint) NWUXLoadNormalizeConverter
(                               /* Load a Unicode -> Normalized converter   */
    nuint            preDeFlag,      /* Want precomposed or decomposed flag?*/
    pCONVERT N_FAR * normalizeHandle /* Converter handle returned here      */
);

N_EXTERN_LIBRARY(nint) NWUXUnloadConverter
(                               /* Release a converter from memory          */
    pCONVERT  converterHandle   /* Handle to converter to be released       */
);

N_EXTERN_LIBRARY(nint) NWUXByteToUnicode
(                               /* Convert bytes to Unicode                 */
    pCONVERT             byteUniHandle,  /* Handle to Byte <-> Uni converter*/
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer. Or 0   */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUXByteToUnicodePath
(                               /* Convert bytes to Unicode for file path   */
    pCONVERT             byteUniHandle,  /* Handle to Byte <-> Uni converter*/
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer. Or 0   */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUXLenByteToUnicode
(                               /* Convert bytes to Unicode                 */
    pCONVERT             byteUniHandle,  /* Handle to Byte <-> Uni converter*/
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer         */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    nint                 inLength,       /* Input str length in bytes or -1 */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUXLenByteToUnicodePath
(                               /* Convert bytes to Unicode for file path   */
    pCONVERT             byteUniHandle,  /* Handle to Byte <-> Uni converter*/
    punicode             unicodeOutput,  /* Buffer for resulting Unicode    */
    nuint                outputBufferLen,/* Length of output buffer         */
    const nuint8 N_FAR * byteInput,      /* Buffer for input bytes          */
    nint                 inLength,       /* Input str length in bytes or -1 */
    pnuint               actualLength    /* Length of results in uni chars  */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToByte
(                               /* Convert Unicode to bytes                 */
    pCONVERT              byteUniHandle, /* Handle to Byte <-> Uni converter*/
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer        */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToBytePath
(                               /* Convert Unicode to bytes for file path   */
    pCONVERT              byteUniHandle, /* Handle to Byte <-> Uni converter*/
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToUntermByte
(                               /* Convert Unicode to bytes                 */
    pCONVERT              byteUniHandle, /* Handle to Byte <-> Uni converter*/
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer        */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToUntermBytePath
(                               /* Convert Unicode to bytes for file path   */
    pCONVERT              byteUniHandle, /* Handle to Byte <-> Uni converter*/
    pnuint8               byteOutput,     /* Buffer for output bytes        */
    nuint                 outputBufferLen,/* Length of output buffer        */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in bytes     */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToCase
(                               /* Convert to upper, lower or title case    */
    pCONVERT              caseHandle,     /* Handle to converter            */
    punicode              monocasedOutput,/* Buffer for output              */
    nuint                 outputBufferLen,/* Length of output buffer. Or 0  */
    const unicode N_FAR * unicodeInput,   /* Buffer for Unicode input       */
    pnuint                actualLength    /* Length of results in uni chars */
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToCollation
(                               /* Convert Unicode to Collation weights     */
    pCONVERT              collationHandle, /* Handle to converter           */
    punicode              collationWeights,/* Buffer for collation weights  */
    nuint                 outputBufferLen, /* Length of output buffer. Or 0 */
    const unicode N_FAR * unicodeInput,    /* Buffer for Unicode input      */
    pnuint                actualLength     /* Length of results in uni chars*/
);

N_EXTERN_LIBRARY(nint) NWUXUnicodeToNormalized
(                               /* Convert Unicode to normalized            */
    pCONVERT              normalizeHandle, /* Handle to converter           */
    punicode              normalizedOutput,/* Buffer for normalized output  */
    nuint                 outputBufferLen, /* Length of output buffer. Or 0 */
    const unicode N_FAR * unicodeInput,    /* Buffer for Unicode input      */
    pnuint                actualLength     /* Length of results in uni chars*/
);

N_EXTERN_LIBRARY(nint) NWUXGetCharSize
(                               /* Convert Unicode to bytes for file path   */
    pCONVERT             byteUniHandle,/* Handle to Byte <-> Uni converter  */
    const nuint8 N_FAR * byteInput,    /* Ptr to single or double-byte char */
    pnuint               pCharSize     /* # bytes in character (1 or 2)     */
);

N_EXTERN_LIBRARY(nint) NWUXSetNoMapAction
(                               /* Set action to be taken for no map chars  */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    nint      noMapByteAction,  /* Action to take for unmappable bytes      */
    nint      noMapUniAction    /* Action to take for unmappable unicode    */
);

N_EXTERN_LIBRARY(nint) NWUXGetNoMapAction
(                               /* Get action to be taken for no map chars  */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    pnint     noMapByteAction,  /* Action to take for unmappable bytes      */
    pnint     noMapUniAction    /* Action to take for unmappable unicode    */
);

N_EXTERN_LIBRARY(nint) NWUXSetScanAction
(                               /* Enable or disable scan/parse functions   */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    nint      scanByteAction,   /* Set action for scan/parse byte functions */
    nint      scanUniAction     /* Set action for scan/parse uni functions  */
);

N_EXTERN_LIBRARY(nint) NWUXGetScanAction
(                               /* Get status of scan/parse functions       */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    pnint     scanByteAction,   /* Status of scan/parse byte functions      */
    pnint     scanUniAction     /* Status of scan/parse uni functions       */
);

N_EXTERN_LIBRARY(nint) NWUXSetSubByte
(                               /* Set substitution byte for converter      */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    nuint8    substituteByte    /* Byte to be substituted                   */
);

N_EXTERN_LIBRARY(nint) NWUXGetSubByte
(                               /* Get substitution byte for converter      */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    pnuint8   substituteByte    /* Substitution byte returned here          */
);

N_EXTERN_LIBRARY(nint) NWUXSetSubUni
(                               /* Set substitute uni char for converter    */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    unicode   substituteUni     /* Unicode character to be substituted      */
);

N_EXTERN_LIBRARY(nint) NWUXGetSubUni
(                               /* Get substitute uni char for converter    */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    punicode  substituteUni     /* Substitution unicode char returned here  */
);

N_EXTERN_LIBRARY(nint) NWUXSetByteFunctions
(                               /* Set up unmappable byte handling          */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    NMBYTE    noMapByteFunc,    /* Function called for unmappable bytes     */
    SCBYTE    scanByteFunc,     /* Byte scanning function                   */
    PRBYTE    parseByteFunc     /* Byte parsing function                    */
);

N_EXTERN_LIBRARY(nint) NWUXGetByteFunctions
(                               /* Get unmappable byte handling functions   */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    NMBYTE N_FAR *noMapByteFunc,/* Handler function returned here           */
    SCBYTE N_FAR *scanByteFunc, /* Byte scanning function                   */
    PRBYTE N_FAR *parseByteFunc /* Byte parsing function                    */
);

N_EXTERN_LIBRARY(nint) NWUXSetUniFunctions
(                               /* Set up unmappable character handling     */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    NMUNI     noMapUniFunc,     /* Function called for unmappable uni chars */
    SCUNI     scanUniFunc,      /* Unicode scanning function                */
    PRUNI     parseUniFunc      /* Unicode parsing function                 */
);

N_EXTERN_LIBRARY(nint) NWUXGetUniFunctions
(                               /* Set up unmappable unicode char handling  */
    pCONVERT  byteUniHandle,    /* Handle to a Byte <-> Unicode converter   */
    NMUNI N_FAR *noMapUniFunc,  /* Function called for unmappable uni chars */
    SCUNI N_FAR *scanUniFunc,   /* Unicode scan function                    */
    PRUNI N_FAR *parseUniFunc   /* Unicode parse function                   */
);

N_EXTERN_LIBRARY(nint) NWUXEnableOemEuro
(
   pCONVERT  convert            /* Set up converter to use the NW OEM Euro  */
);

N_EXTERN_LIBRARY(nint) NWUXResetConverter
(
    pCONVERT  convert           /* Reset a converter to default state       */
);

/****************************************************************************/

/*
    Table based Unicode/Local text conversion APIs. The converter based
	 APIs are preferred over these.
*/
N_EXTERN_LIBRARY(nint) NWInitUnicodeTables
(
   nint countryCode,
   nint codePage
);

N_EXTERN_LIBRARY(NWRCODE) NWLSetPrimaryUnicodeSearchPath
(
   const nstr N_FAR *strSearchPath
);

N_EXTERN_LIBRARY(nint) NWFreeUnicodeTables
(
   void
);

N_EXTERN_LIBRARY(nint) NWLoadRuleTable
(
   pnstr ruleTableName,           /* Name of the rule table              */
   pnptr ruleHandle               /* Where to put the rule table handle  */
);

N_EXTERN_LIBRARY(nint) NWUnloadRuleTable
(
   nptr        ruleHandle         /* Rule table handle                   */
);


#if defined N_PLAT_NLM

/* NWUSByteToUnicode or NWUXByteToUnicode are preferred */
N_EXTERN_LIBRARY(nint)
NWLocalToUnicode                    /* Convert local to Unicode            */
(
   nptr               ruleHandle,    /* Rule table handle                   */
   punicode           dest,          /* Buffer for resulting Unicode        */
   nuint32            maxLen,        /* Size of results buffer              */
   const void N_FAR * src,           /* Buffer with source local code       */
   unicode            noMap,         /* No map character                    */
   pnuint             len,           /* Number of unicode chars in output   */
   nuint32   allowNoMapFlag    /* Flag indicating default map is allowable */
);

/* NWUSUnicodeToByte or NWUXUnicodeToByte are preferred */
N_EXTERN_LIBRARY(nint)
NWUnicodeToLocal                    /* Convert Unicode to local code       */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   nptr                  dest,       /* Buffer for resulting local code     */
   nuint32               maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   nuint8                noMap,      /* No Map character                    */
   pnuint                len,        /* Number of bytes in output           */
   nuint32   allowNoMapFlag    /* Flag indicating default map is allowable */
);

#if !defined(EXCLUDE_UNICODE_NLM_COMPATIBILITY_MACROS)
#define NWLocalToUnicode(P1,P2,P3,P4,P5,P6) NWLocalToUnicode(P1,P2,P3,P4,P5,P6, 1)
#define NWUnicodeToLocal(P1,P2,P3,P4,P5,P6) NWUnicodeToLocal(P1,P2,P3,P4,P5,P6, 1)
#endif

/* If I could make size_t be nuint32 for N_PLAT_NLM all of the functions */
/* below here could be single sourced.                                   */
#if 0
N_EXTERN_LIBRARY(nint)
NWUnicodeToCollation                /* Convert Unicode to collation        */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   punicode              dest,       /* Buffer for resulting Unicode weights*/
   nuint32               maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   unicode               noMap,      /* No map character                    */
   pnuint32              len         /* Number of unicode chars in output   */
);

N_EXTERN_LIBRARY(nint)
NWUnicodeCompare                    /* Compare two unicode characters      */
(
   nptr           ruleHandle,       /* Rule table handle                   */
   unicode        chr1,             /* 1st character                       */
   unicode        chr2              /* 2nd character                       */
);

N_EXTERN_LIBRARY(nint)
NWUnicodeToMonocase                 /* Convert Unicode to collation        */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   punicode              dest,       /* Buffer for resulting Unicode weights*/
   nuint32               maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   pnuint32              len         /* Number of unicode chars in output   */
);

#endif

#else   /*  not N_PLAT_NLM  */

/* NWUSByteToUnicode or NWUXByteToUnicode are preferred */
N_EXTERN_LIBRARY(nint)
NWLocalToUnicode                    /* Convert local to Unicode            */
(
   nptr                 ruleHandle, /* Rule table handle                   */
   punicode             dest,       /* Buffer for resulting Unicode        */
   size_t               maxLen,     /* Size of results buffer              */
   const nuint8 N_FAR * src,        /* Buffer with source local code       */
   unicode              noMap,      /* No map character                    */
   size_t       N_FAR * len         /* Number of unicode chars in output   */
);

/* NWUSUnicodeToByte or NWUXUnicodeToByte are preferred */
N_EXTERN_LIBRARY(nint)
NWUnicodeToLocal                    /* Convert Unicode to local code       */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   pnuint8               dest,       /* Buffer for resulting local code     */
   size_t                maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   unsigned char         noMap,      /* No Map character                    */
   size_t        N_FAR * len         /* Number of bytes in output           */
);

#endif    /* not N_PLAT_NLM */

N_EXTERN_LIBRARY(nint)
NWUnicodeToCollation                /* Convert Unicode to collation        */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   punicode              dest,       /* Buffer for resulting Unicode weights*/
   size_t                maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   unicode               noMap,      /* No map character                    */
   size_t        N_FAR * len         /* Number of unicode chars in output   */
);

N_EXTERN_LIBRARY(nint)
NWUnicodeCompare                    /* Compare two unicode characters      */
(
   nptr           ruleHandle,       /* Rule table handle                   */
   unicode        chr1,             /* 1st character                       */
   unicode        chr2              /* 2nd character                       */
);

N_EXTERN_LIBRARY(nint)
NWUnicodeToMonocase                 /* Convert Unicode to collation        */
(
   nptr                  ruleHandle, /* Rule table handle                   */
   punicode              dest,       /* Buffer for resulting Unicode weights*/
   size_t                maxLen,     /* Size of results buffer              */
   const unicode N_FAR * src,        /* Buffer with source Unicode          */
   size_t        N_FAR * len         /* Number of unicode chars in output   */
);


/*
 *    Functions that work with XLate Tables
 */

#if defined N_PLAT_DOS && defined N_UNI_NEW_TABLES

#  define N_UNI_LOAD_MONOCASE     0x0001
#  define N_UNI_LOAD_COLLATION    0x0002

N_EXTERN_LIBRARY(nint) NWLInitXlateTables
(
   nint codePage,
   nflag8 flags
);

N_EXTERN_LIBRARY(nint) NWLFreeXlateTables
(
   void
);

N_EXTERN_LIBRARY(nint) NWLLoadXlateTable
(
   pnstr ruleTableName,           /* Name of the rule table              */
   pnptr ruleHandle               /* Where to put the rule table handle  */
);

N_EXTERN_LIBRARY(nint) NWLUnloadXlateTable
(
   const void N_FAR * ruleHandle         /* Rule table handle                   */
);

#  define NWInitUnicodeTables(CountryCode, CodePage)  \
      NWLInitXlateTables(                                               \
                           CodePage,                                    \
                           N_UNI_LOAD_MONOCASE | N_UNI_LOAD_COLLATION   \
                        )
#  define NWFreeUnicodeTables   NWLFreeXlateTables
#  define NWLoadRuleTable       NWLLoadXlateTable
#  define NWUnloadRuleTable     NWLUnloadXlateTable

#endif


N_EXTERN_LIBRARY(nint) NWGetUnicodeToLocalHandle
(
   pnptr handle
);

N_EXTERN_LIBRARY(nint) NWGetLocalToUnicodeHandle
(
   pnptr handle
);

N_EXTERN_LIBRARY(nint) NWGetMonocaseHandle
(
   pnptr handle
);

N_EXTERN_LIBRARY(nint) NWGetCollationHandle
(
   pnptr handle
);

/****************************************************************************/

/*
    Redefine these functions to use the new unicode API monocase routines.
*/
#ifdef N_PLAT_NLM
# define uniicmp(s1, s2)  nwusuniicmp(s1, s2)
# define uninicmp(s1, s2, l) nwusuninicmp(s1, s2, l)
#endif


/*
    Unicode string functions that work like those in string.h
*/

N_EXTERN_LIBRARY(punicode) unicat    /* Corresponds to strcat    */
(
   punicode              s1,       /* Original string                     */
   const unicode N_FAR * s2        /* String to be appended               */
);

N_EXTERN_LIBRARY(punicode) unichr    /* Corresponds to strchr    */
(
   const unicode N_FAR * s,        /* String to be scanned                */
   unicode               c         /* Character to be found               */
);

N_EXTERN_LIBRARY(punicode) unicpy    /* Corresponds to strcpy    */
(
   punicode              s1,       /* Destination string                  */
   const unicode N_FAR * s2        /* Source string                       */
);

N_EXTERN_LIBRARY(size_t) unicspn     /* Corresponds to strcspn   */
(
   const unicode N_FAR * s1,       /* String to be scanned                */
   const unicode N_FAR * s2        /* Character set                       */
);

N_EXTERN_LIBRARY(size_t) unilen      /* Corresponds to strlen    */
(
   const unicode N_FAR * s         /* String to determine length of       */
);

N_EXTERN_LIBRARY(punicode) unincat   /* Corresponds to strncat   */
(
   punicode              s1,       /* Original string                     */
   const unicode N_FAR * s2,       /* String to be appended               */
   size_t                n         /* Maximum characters to be appended   */
);

N_EXTERN_LIBRARY(punicode) unincpy   /* Corresponds to strncpy   */
(
   punicode              s1,       /* Destination string                  */
   const unicode N_FAR * s2,       /* Source string                       */
   size_t                n         /* Maximum length                      */
);

N_EXTERN_LIBRARY(punicode) uninset   /* Corresponds to strnset   */
(
   punicode s,            /* String to be modified               */
   unicode  c,            /* Fill character                      */
   size_t   n             /* Maximum length                      */
);

N_EXTERN_LIBRARY(punicode) unipbrk   /* Corresponds to strpbrk   */
(
   const unicode N_FAR * s1,       /* String to be scanned                */
   const unicode N_FAR * s2        /* Character set                       */
);

N_EXTERN_LIBRARY (punicode) unipcpy    /* Corresponds to strpcpy */
(
   punicode              s1,       /* Destination string                  */
   const unicode N_FAR * s2        /* Source string                       */
);

N_EXTERN_LIBRARY(punicode) unirchr   /* Corresponds to strrchr   */
(
   const unicode N_FAR * s,        /* String to be scanned                */
   unicode               c         /* Character to be found               */
);

N_EXTERN_LIBRARY(punicode) unirev    /* Corresponds to strrev    */
(
   punicode s             /* String to be reversed               */
);

N_EXTERN_LIBRARY(punicode) uniset    /* Corresponds to strset    */
(
   punicode s,            /* String to modified                  */
   unicode  c             /* Fill character                      */
);

N_EXTERN_LIBRARY(size_t) unispn      /* Corresponds to strspn    */
(
   const unicode N_FAR * s1,       /* String to be tested                 */
   const unicode N_FAR * s2        /* Character set                       */
);

N_EXTERN_LIBRARY(punicode) unistr    /* Corresponds to strstr    */
(
   const unicode N_FAR * s1,       /* String to be scanned                */
   const unicode N_FAR * s2        /* String to be located                */
);

N_EXTERN_LIBRARY(punicode) unitok    /* Corresponds to strtok    */
(
   punicode              s1,       /* String to be parsed                 */
   const unicode N_FAR * s2        /* Delimiter values                    */
);

N_EXTERN_LIBRARY(nint) uniicmp       /* Corresponds to stricmp   */
(
   const unicode N_FAR * s1,       /* 1st string to be compared           */
   const unicode N_FAR * s2        /* 2nd string to be compared           */
);

N_EXTERN_LIBRARY(nint) uninicmp      /* Corresponds to strnicmp  */
(
   const unicode N_FAR * s1,       /* 1st string to be compared           */
   const unicode N_FAR * s2,       /* 2nd string to be compared           */
   size_t                len       /* Maximum length                      */
);

N_EXTERN_LIBRARY(nint) unicmp        /* Unicode compare          */
(
   const unicode N_FAR * s1,
   const unicode N_FAR * s2
);

N_EXTERN_LIBRARY(nint) unincmp       /* Unicode length compare  */
(
   const unicode N_FAR * s1,
   const unicode N_FAR * s2,
   size_t                len
);

N_EXTERN_LIBRARY(size_t) unisize     /* Corresponds to sizeof    */
(
   const unicode N_FAR * s
);

/*
 * UTF-8  <--> Unicode Conversion APIS
 */
N_EXTERN_LIBRARY(nint) NWLUnicodeToUTF8
(
   const unicode N_FAR * uniStr,
   nuint                 maxSize,
   pnuint8               utf8Str,
   pnuint                utf8Size
);

N_EXTERN_LIBRARY(nint) NWLUTF8ToUnicode
(
   const nuint8 N_FAR * utf8Str,
   nuint                maxSize,
   punicode             uniStr,
   pnuint               uniSize,
   ppnstr               badSequence
);

N_EXTERN_LIBRARY(nint) NWLUTF8ToUnicodeSize
(
   const nuint8 N_FAR * utf8Str,
   pnuint               size
);

N_EXTERN_LIBRARY(nuint) NWLUnicodeToUTF8Size
(
   const unicode N_FAR * uniStr
);

#ifdef __cplusplus
}
#endif

#endif

/****************************************************************************/

