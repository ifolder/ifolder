#ifndef _STRING_H_
#define _STRING_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  string.h
==============================================================================
*/

#ifndef NULL
# define NULL  0
#endif

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

#ifdef __cplusplus
extern "C"
{
#endif

/* ISO/ANSI C defined functions... */
void    *memchr( const void *, int, size_t );
int     memcmp( const void *, const void *, size_t );
void    *memcpy( void *, const void *, size_t );
void    *memmove( void *, const void *, size_t );
void    *memset( void *, int, size_t );
char    *strcpy( char *, const char * );
char    *strcat( char *, const char * );
char    *strchr( const char *, int );
int     strcmp( const char *, const char * );
int     strcoll( const char *, const char * );
size_t   strcspn( const char *, const char * );
char    *strerror( int );
size_t   strlen( const char * );
char    *strncat( char *, const char *, size_t );
int     strncmp( const char *, const char *, size_t );
char    *strncpy( char *, const char *, size_t );
char    *strpbrk( const char *, const char * );
char    *strrchr( const char *, int );
size_t   strspn( const char *, const char * );
char    *strstr( const char *, const char * );
char    *strtok( char *, const char * );
size_t   strxfrm( char *, const char *, size_t );

/* POSIX-defined additions... */
char  *strtok_r( char *, const char *, char ** ); 

/* nonstandard functions... */
int   memicmp( const void *, const void *, size_t ); 
int   strcmpi( const char *, const char * ); 
int   stricmp( const char *, const char * ); 
char  *strdup( const char * ); 
char  *strlist( char *, const char *, ... ); 
char  *strlwr( char * );
int   strnicmp( const char *, const char *, size_t ); 
char  *strnset( char *, int, size_t ); 
char  *strrev( char * ); 
char  *strset( char *, int ); 
char  *strupr( char * ); 
void  swab( const void *, void *, size_t ); 
void  swaw( const void *, void *, size_t );

#ifdef __cplusplus
}
#endif

#endif
