#ifndef _NWCONIO_H_
#define _NWCONIO_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwconio.h
==============================================================================
*/
#ifndef N_PLAT_NLM
# define N_PLAT_NLM
#endif
#include <ntypes.h>
#include <nwtypes.h>

/* attributes that may be set via SetScreenAttributes() */
#define DONT_AUTO_ACTIVATE    0x01  /* avoids autoactivation when screens are */
                                    /* created, but no other screens exist    */
#define DONT_SWITCH_SCREEN    0x02  /* avoids screen being switched           */
#define DONT_CHECK_CTRL_CHARS 0x10  /* turns off ^C and ^S processing         */
#define AUTO_DESTROY_SCREEN   0x20  /* avoids "Press any key to close screen  */
#define POP_UP_SCREEN         0x40  /* for a pop up screen                    */
#define UNCOUPLED_CURSORS     0x80  /* for distinct input & output cursors    */

/* more screen attribute values returned by GetScreenInfo() */
#define HAS_A_CLIB_HANDLE            0x00000100
#define _KEYBOARD_INPUT_ACTIVE       0x00010000
#define _PROCESS_BLOCKED_ON_KEYBOARD 0x00020000
#define _PROCESS_BLOCKED_ON_SCREEN   0x00040000
#define _INPUT_CURSOR_DISABLED       0x00080000
#define _SCREEN_HAS_TITLE_BAR        0x00400000
#define _NON_SWITCHABLE_SCREEN       0x01000000


#ifdef __cplusplus
extern "C"
{
#endif

extern int	getch( void );
extern int	getche( void );
extern int	kbhit( void );
extern int	putch( int c );
extern int	ungetch( int c );
extern char *cgets( char *buf );
extern int	CheckIfScreenDisplayed( int screenHandle, long waitFlag );
extern void clrscr( void );
extern void ConsolePrintf( const char *format, ... );
extern void CopyToScreenMemory( WORD height, WORD width, const BYTE *Rect,
					WORD beg_x, WORD beg_y );
extern void CopyFromScreenMemory( WORD height, WORD width, BYTE *Rect,
					WORD beg_x, WORD beg_y );
extern int	CoupleInputOutputCursors( void );
extern int	cputs( const char *buf );
extern int	cprintf( const char *fmt, ... );
extern int	CreateScreen( const char *screenName, BYTE attr );
extern int	cscanf( const char *fmt, ... );
extern int	DecoupleInputOutputCursors( void );
extern int	DestroyScreen( int screenHandle );
extern int	DisplayInputCursor( void );
extern int	DisplayScreen( int screenHandle );
extern int	DropPopUpScreen( int screenHandle );
extern int	GetCurrentScreen( void );
extern BYTE GetCursorCouplingMode( void );
extern WORD GetCursorShape( BYTE *startline, BYTE *endline );
extern WORD GetCursorSize( BYTE *firstline, BYTE *lastline );
extern int	GetPositionOfOutputCursor( WORD *rowP, WORD *columnP );
extern int	__GetScreenID( int screenHandle );
extern int	GetScreenInfo( int handle, char *name, LONG *attr );
extern int	GetSizeOfScreen( WORD *heightP, WORD *widthP );
extern void gotoxy( WORD col, WORD row );
extern int	HideInputCursor( void );
extern int	IsColorMonitor( void );
extern int	PressAnyKeyToContinue( void );
extern int	PressEscapeToQuit( void );
extern void RingTheBell( void );
extern int	ScanScreens( int LastScreenID, char *name, LONG *attr );
extern int	ScrollScreenRegionDown( int firstLine, int numLines );
extern int	ScrollScreenRegionUp( int firstLine, int numLines );
extern BYTE SetAutoScreenDestructionMode( BYTE newMode );
extern BYTE SetCtrlCharCheckMode( BYTE newMode );
extern BYTE SetCursorCouplingMode( BYTE newMode );
extern WORD SetCursorShape( BYTE startline, BYTE endline );
extern int	SetCurrentScreen( int screenHandle );
extern int	SetInputAtOutputCursorPosition( void );
extern int	SetOutputAtInputCursorPosition( void );
extern int	SetPositionOfInputCursor( WORD row, WORD column );
extern LONG SetScreenAreaAttribute( LONG line, LONG column, LONG numLines,
				LONG numColumns, LONG attr );
extern int	SetScreenAttributes( LONG mask, LONG attr );
extern LONG	SetScreenCharacterAttribute( LONG line, LONG column, LONG attr );
extern int	SetScreenRegionAttribute( int firstLine, int numLines, BYTE attr );
extern WORD	wherex( void );
extern WORD	wherey( void );

#ifdef __cplusplus
}
#endif


#endif
