#ifndef _NWINTXX_H_
#define _NWINTXX_H_

#ifdef _INTEGRAL_MAX_BITS

#if !defined(INT8) && _INTEGRAL_MAX_BITS >= 8
#define INT8 __int8
#endif

#if !defined(INT16) && _INTEGRAL_MAX_BITS >= 16
#define INT16 __int16
#endif

#if !defined(INT32) && _INTEGRAL_MAX_BITS >= 32
#define INT32 __int32
#endif

#if !defined(INT64) && _INTEGRAL_MAX_BITS >= 64
#define INT64 __int64
#endif

#if !defined(INT128) && _INTEGRAL_MAX_BITS >= 128
#define INT128 __int128
#endif

#else /*_INTEGRAL_MAX_BITS*/

#ifndef INT8
#define INT8 char
#endif

#ifndef INT16
#define INT16 short
#endif

#ifndef INT32
#define INT32 long
#endif

#if !defined(INT64) && defined(_LONG_LONG)
#define INT64 long long
#endif

#endif /*_INTEGRAL_MAX_BITS*/

#endif /*_NWINTXX_H_*/
