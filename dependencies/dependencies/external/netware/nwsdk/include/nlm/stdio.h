#ifndef _STDIO_H_
#define _STDIO_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  stdio.h
==============================================================================
*/
#include <stdarg.h>

#ifndef NULL
# define NULL 0
#endif

#ifndef EOF
# define EOF (-1)
#endif

#ifndef _SIZE_T
# define _SIZE_T
typedef unsigned int size_t;
#endif

typedef long fpos_t;

#ifndef _WCHAR_T
# define _WCHAR_T
typedef unsigned short  wchar_t;
#endif

#ifndef _WINT_T
# define _WINT_T
typedef long wint_t;
#endif

/* values for fseek()' whence argument */
#define SEEK_SET     0        /* add 'offset' to beginning of file        */
#define SEEK_CUR     1        /* add 'offset' to current position in file */
#define SEEK_END     2        /* add 'offset' to end of file              */

/* miscellaneous definitions */
#define FOPEN_MAX    20       /* at least this many FILEs available       */
#define BUFSIZ       1024     /* (extreme) default buffer size            */
#define FILENAME_MAX 1024     /* max number of characters in a path name  */

/* definitions for tmpnam() and tmpfil() */
#define TMP_MAX      100000   /* "_T-00000.TMP" to "_T-99999.TMP"         */
#define L_tmpnam     13       /* 8 + 1 + 3 + 1 (always DOS namespace)     */

/* values for field '_flag' in FILE below */
#define _IOREAD      0x0001   /* currently reading                        */
#define _IOWRT       0x0002   /* currently writing                        */
#define _IORW        0x0004   /* opened for reading and writing           */
#define _IOBIN       0x0008   /* binary file (O_BINARY)                   */
#define _IONBF       0x0010   /* unbuffered (e.g.: stdout and stderr)     */
#define _IOLBF       0x0020   /* line buffered (e.g.: stdin)              */
#define _IOFBF       0x0040   /* fully buffered (most files)              */
#define _IOEOF       0x0080   /* EOF reached on read                      */
#define _IOERR       0x0100   /* I/O error from system                    */
#define _IOBUF       0x0200   /* stdio code malloc()'d this buffer        */
#define _IOTMP       0x0400   /* was a temporary file by tmpfile()        */

typedef struct _iobuf         /* file stream structure                    */
{
   unsigned long  _signature; /* identifies this structure                */
   signed long    _avail;     /* available (unused/unread) room in buffer */
   unsigned char  *_ptr;      /* next character from/to here in buffer    */
   unsigned char  *_base;     /* the buffer (not really)                  */
   unsigned long  _oflag;     /* pre-CLib.NLM v4.11 compatibility         */
   unsigned long  _file;      /* file descriptor                          */
   unsigned long  _flag;      /* state of stream                          */
   unsigned char  _buf[4];    /* fake, micro buffer as a fall-back        */
   unsigned long  _env;       /* Macintosh or UNIX text file signature    */
} FILE;

#ifdef __cplusplus
extern "C"
{
#endif

/* ISO/ANSI C defined functions... */
void     clearerr( FILE * );
int      fclose( FILE * );
int      feof( FILE * );
int      ferror( FILE * );
int      fflush( FILE * );
int      fgetc( FILE * );
int      fgetpos( FILE *, fpos_t * );
char     *fgets( char *, int, FILE * );
FILE     *fopen( const char *, const char * );
int      fprintf( FILE *, const char *, ... );
int      fputc( int, FILE * );
int      fputs( const char *, FILE * );
size_t   fread( void *, size_t, size_t, FILE * );
FILE     *freopen( const char *, const char *, FILE * );
int      fscanf( FILE *, const char *, ... );
int      fseek( FILE *fp, long offset, int whence );
int      fsetpos( FILE *, const fpos_t * );
long     ftell( FILE * );
size_t   fwrite( const void *, size_t, size_t, FILE * );
int      getc( FILE * );
int      getchar( void );
char     *gets( char * );
void     perror( const char * );
int      printf( const char *, ... );
int      putc( int, FILE * );
int      putchar( int );
int      puts( const char * );
int      remove( const char * );
int      rename( const char *, const char * );
void     rewind( FILE * );
int      scanf( const char *, ... );
void     setbuf( FILE *, char * );
int      setvbuf( FILE *, char *, int, size_t );
int      sprintf( char *, const char *, ... );
int      sscanf( const char *, const char *, ... );
FILE     *tmpfile( void );
char     *tmpnam( char * );
int      ungetc( int, FILE * );
int      vfprintf( FILE *, const char *, va_list );
int      vfscanf( FILE *, const char *, va_list );
int      vprintf( const char *, va_list );
int      vscanf( const char *, va_list );
int      vsprintf(char *, const char *, va_list );
int      vsscanf(const char *, const char *, va_list );

/* POSIX-defined additions... */
FILE  *fdopen( int, const char * );
int   fileno( FILE * );

/* nonstandard functions... */
char *cgets( char * );
int   cprintf( const char *, ... );
int   cputs( const char * );
int   cscanf( const char *, ... );
int   fcloseall( void );
int   fgetchar( void );
int   flushall( void );
int   fputchar( int );
int   getch( void );
int   getche( void );
int   putch( int );
int   ungetch( int );
int   vcprintf( const char *, va_list );
int   vcscanf( const char *, va_list );

/* Novell-defined, enabled functions... */
int   NWcprintf( const char *, ... );
int   NWfprintf( FILE *, const char *, ... );
int   NWprintf( const char *, ... );
int   NWsprintf( char *, const char *, ... );
int   NWvcprintf( const char *, va_list ); 
int   NWvfprintf( FILE *, const char *, va_list );
int   NWvprintf( const char *, va_list );
int   NWvsprintf( char *, const char *, va_list );

/*
** For the following support, open the file without 'b' in the mode. Additions
** for transparent Macintosh text file support ('\r' on lines) and additions
** for transparent UNIX text file support ('\n' on lines).
*/
int   IsMacintoshTextFile( FILE * );
int   SetMacintoshTextMode( FILE * );
int   UnsetMacintoshTextMode( FILE * );    /* back to '\r\n' */
int   is_unix_text_file( FILE * );
int   set_unix_text_mode( FILE * );
int   unset_unix_text_mode( FILE * );      /* back to '\r\n' */

/* functions underlying macro support... */
FILE  **__get_stdin( void ); 
FILE  **__get_stdout( void ); 
FILE  **__get_stderr( void );

#ifdef __cplusplus
}
#endif

/* defined as macros in both ISO C and ISO C++ */

#define stdin       (*__get_stdin())
#define stdout      (*__get_stdout())
#define stderr      (*__get_stderr())

#ifdef __cplusplus

inline int getc( FILE *fp )        { return fgetc(fp);       }
inline int putc( int c, FILE *fp ) { return fputc(c, fp);    }
inline int getchar( void )         { return getc(stdin);     }
inline int putchar( int c )        { return putc(c, stdout); }

#else

#define getchar()   getc(stdin)
#define putchar(c)  putc((c), stdout)
#define getc(fp)    fgetc(fp)
#define putc(c, fp) fputc((c), (fp))
#define fileno(fp)  (fileno)(fp)

#endif

#endif
