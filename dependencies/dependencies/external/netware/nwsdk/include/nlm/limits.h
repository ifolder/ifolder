#ifndef _LIMITS_H_
#define _LIMITS_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  limits.h
==============================================================================
*/
#define PAGESIZE     4096              /* granularity of memory mapping et al.*/

#define CHAR_BIT     8                 /* max number of bits in a 'char'      */
#define SCHAR_MIN    (-128)            /* min value of a 'signed char'        */
#define SCHAR_MAX    127               /* max value of a 'signed char'        */
#define UCHAR_MAX    255               /* max value of an 'unsigned char'     */

#if '\xff' < 0
# define CHAR_MIN    SCHAR_MIN         /* min value of a 'char'               */
# define CHAR_MAX    SCHAR_MAX         /* max value of a 'char'               */
#else
# define CHAR_MIN    0
# define CHAR_MAX    UCHAR_MAX
#endif

#define MB_LEN_MAX   5                 /* max bytes in multibyte character    */

#define SHRT_MIN     (-32768)          /* min value of 'short int'            */
#define SHRT_MAX     32767             /* max value of 'short int'            */
#define USHRT_MAX    65535             /* max value of 'unsigned short int'   */
#define LONG_MIN     (-2147483647-1)   /* min value of 'long int'             */
#define LONG_MAX     2147483647        /* max value of 'long int'             */
#define ULONG_MAX    4294967295        /* max value of 'unsigned long int'    */
#define INT_MIN      LONG_MIN          /* min value of 'int'                  */
#define INT_MAX      LONG_MAX          /* max value of 'int'                  */
#define UINT_MAX     ULONG_MAX         /* max value of 'unsigned int'         */

#ifndef SSIZE_MAX
# define SSIZE_MAX   INT_MAX           /* max value of a 'ssize_t'            */
#endif

#ifndef TZNAME_MAX
# define TZNAME_MAX  8                 /* max characters in time zone name    */
#endif

#define PIPE_BUF     512               /* length of pipe data integrity       */


#endif
