/*
===============================================================================
Novell Software Developer Kit Sample Code License

Copyright (C) 2003-2004 Novell, Inc.  All Rights Reserved.

THIS WORK IS SUBJECT TO U.S. AND INTERNATIONAL COPYRIGHT LAWS AND TREATIES.  
USE AND REDISTRIBUTION OF THIS WORK IS SUBJECT TO THE LICENSE AGREEMENT 
ACCOMPANYING THE SOFTWARE DEVELOPMENT KIT (SDK) THAT CONTAINS THIS WORK.  
PURSUANT TO THE SDK LICENSE AGREEMENT, NOVELL HEREBY GRANTS TO DEVELOPER 
A ROYALTY-FREE, NON-EXCLUSIVE LICENSE TO INCLUDE NOVELL'S SAMPLE CODE IN ITS 
PRODUCT.  NOVELL GRANTS DEVELOPER WORLDWIDE DISTRIBUTION RIGHTS TO MARKET, 
DISTRIBUTE, OR SELL NOVELL'S SAMPLE CODE AS A COMPONENT OF DEVELOPER'S PRODUCTS.
NOVELL SHALL HAVE NO OBLIGATIONS TO DEVELOPER OR DEVELOPER'S CUSTOMERS WITH 
RESPECT TO THIS CODE.

NAME OF FILE:
	smuni.h

PURPOSE/COMMENTS:
	xplat unicode and utf8 premitives header file

NDK COMPONENT NAME AND VERSION:
	SMS Developer Components

LAST MODIFIED DATE: 
	16 July 2004

===============================================================================
*/
 
#ifndef _SMUNICODE_H
#define _SMUNICODE_H

#include<stddef.h>
#include <smstypes.h>

/* this files will contain all XPLAT ( NetWare and Linux) Unicode and UTF8 related functions*/

#ifdef __cplusplus
extern "C" {
#endif

#ifdef N_PLAT_NLM
typedef int UniRuleTable_t;
#endif

#ifndef _UNICODE_T
#define _UNICODE_T
#if defined (N_PLAT_UNIX)
#include <wchar.h>
		typedef wchar_t unicode_t;
#else
		typedef unsigned short unicode_t; 
#endif
#endif

#ifndef _UTF8_T
#define _UTF8_T
typedef char utf8_t;	
#endif

#pragma pack(push, 1 )
typedef struct
{
        UINT32          nameSpace;
        UINT32          fileSystem;
} UNICODE_CONTEXT;
#pragma pack(pop)


#if defined (N_PLAT_UNIX)
/* Macroed functions */

/* WCS strings */
#define unilen(a) 			(wcslen((a)))
#define unicpy(a,b)			(wcscpy((a), (b)))
#define unicat(a, b)		(wcscat((a), (b)))
#define unincat(a, b, c)	(wcsncat((a), (b), (c)))
#define unichr(a, b)		(wcschr((a), (b)))
#define unirchr(a, b)		(wcsrchr((a), (b)))
#define unipbrk(a, b)		(wcspbrk((a), (b)))
#define unincpy(a, b, c)	(wcsncpy((a), (b), (c)))
#define unicspn(a, b)		(wcscspn((a), (b)))
#define unistr(a, b)		(wcsstr((a), (b)))
#define unicmp(a, b)		(wcscmp((a), (b)))
#define unincmp(a, b,c)		(wcsncmp((a), (b), (c)))
#define uniicmp(a, b)		(wcscasecmp((a), (b)))
#define uninicmp(a,b,c)		SMuninicmp( (a), (b), (c))
#define unirev(a) 			SMunirev(a)

#endif
#define unidup(a) 			SMunidup(a)
#define uniupr(a) 			SMuniupr(a)


/*Returns a reverse string.  */
unicode_t * SMunirev (unicode_t *str ) ; 

/* Returns a duplicate string. Calling fucntion need to free the memory */
unicode_t * SMunidup ( const unicode_t *str ) ; 

/*Converts unicode string to upper case  */
unicode_t * SMuniupr ( unicode_t *str ) ;

/* Returns the no of bytes of the first character in the UTF8 string */
unsigned int SMutf8Size(const utf8_t *string);

/* Returns pointer to the next UTF8 character */
utf8_t *  SMutf8next ( const utf8_t *string ) ;

/*Reverses the UTF8 string and returns the same */
utf8_t *  SMutf8rev (utf8_t* string);

/* Duplicates the UTF8 string and returns the same */
utf8_t *  SMutf8dup (const utf8_t *str);

/* Converts the UTF8 string to upper case and returns the same */
utf8_t *  SMutf8upr ( utf8_t *string ) ;

/* Returns the number of UTF8 characters in the UTF8 string */
size_t    SMutf8len    ( const utf8_t *string );

/* Concatenates two utf8 strings dest and src */
utf8_t *  SMutf8cat    ( utf8_t *dest, const  utf8_t  *src );

/*Concatenates the first n characters of src with dest */
utf8_t *  SMutf8ncat    ( utf8_t  *dest, const  utf8_t *src, size_t n ); 

/* Compares two utf8 strings */
int       SMutf8cmp ( const utf8_t *s1, const utf8_t *s2 ) ;

/* Searches for the first occurance of string as2 in as1 */
utf8_t*   SMutf8str   ( const utf8_t *as1, const utf8_t *as2 );

/* Compares two utf8 strings */
int       SMutf8cmp     ( const  utf8_t  *s1, const  utf8_t  *s2 );

/* Case ignore Compares two utf8 strings */
int       SMutf8icmp     ( const  utf8_t  *s1, const  utf8_t  *s2 ); 

/* Case ignore Compares the first n characters of two utf8 strings */
int       SMutf8nicmp     (const  utf8_t *s1, const  utf8_t  *s2, unsigned int n);

/* Searches and returns the first occurence of character ch in the string  */
utf8_t *  SMutf8chr ( const utf8_t *string, const utf8_t *ch ) ;

/* Searches and returns the last occurence of character ch in the string  */
utf8_t *  SMutf8rchr ( const utf8_t *string, const utf8_t *ch ) ;

/* Converts the Local string to Unicode and returns the number of bytes converted (excluding the null terminator) */
int SMuni2loc ( char *dest, size_t *destLen, const unicode_t *src, size_t srcLen, UNICODE_CONTEXT* uContext);

/* Converts the Local string to Unicode and returns the number of bytes converted (excluding the null terminator) */
int SMloc2uni (unicode_t *dest, size_t *destLen, const char *src, size_t srcLen, UNICODE_CONTEXT* uContext) ;

 /* Converts Unicode strings to UTF8 format */
int SMuni2utf8 (char *dest, const unicode_t *src, size_t*destBufSize);

/* Converts UTF8 strings to Unicode format  */

int SMutf82uni (unicode_t *dest, const char *src, size_t *destBufSize);

/* Converts a string from the local code page to UTF-8. */
int SMloc2utf8(utf8_t * dest, size_t *destLen,const char * src, int srclen, UNICODE_CONTEXT *uCtx) ;

/* Converts a string from the UTF8 to Locale code page   */
int SMutf82loc( char * dest, size_t* destLen, const utf8_t * src,  int  srcLen, UNICODE_CONTEXT *uCtx);

#ifdef N_PLAT_NLM
/*Import unicode APIs*/
void SMImportUniAPIs(UINT32 nlmHandle);

/*Unimport unicode APIs*/
void SMUnImportUniAPIs(UINT32 nlmHandle);
#endif

#ifdef __cplusplus
}
#endif

#endif
