COMMENT (); //lint -e762		LEAVE THIS ALONE.
/*(		// LEAVE THIS ALONE.
IF	0	; LEAVE THIS ALONE.
;*****************************************************************************
;*
;*	(C) Copyright 1989-1996 Novell, Inc.
;*	All Rights Reserved.
;*
;*	This program is an unpublished copyrighted work which is proprietary
;*	to Novell, Inc. and contains confidential information that is not
;*	to be reproduced or disclosed to any other person or entity without
;*	prior written consent from Novell, Inc. in each and every instance.
;*
;*	WARNING:  Unauthorized reproduction of this program as well as
;*	unauthorized preparation of derivative works based upon the
;*	program or distribution of copies by sale, rental, lease or
;*	lending are violations of federal copyright laws and state trade
;*	secret laws, punishable by civil and criminal penalties.
;*
;*	INCLUDE FILE THAT CAN BE INCLUDED BOTH IN C AND ASSEMBLY SOURCES
;*
;*	$Workfile:   screen.h  $
;*	$Modtime:   Nov 15 2000 09:29:34  $
;*	$Revision$
;*	$Author$
;*
;*****************************************************************************
;*/

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// %%%%%%%%%%%%%%%%%%% BEGIN C INCLUDE PORTION. %%%%%%%%%%%%%%%%%%%%
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

#ifndef __SCREEN_H__
#define __SCREEN_H__


#include	<mpktypes.h>

#define ScreenDebug		1
#define ScreenDebug1	0

/* Screen flags controlled by the owning engine */
#define KEYBOARD_INPUT_ACTIVE			0x00000001
#define PROCESS_BLOCKED_ON_KEYBOARD		0x00000002
#define PROCESS_BLOCKED_ON_SCREEN		0x00000004
#define INPUT_CURSOR_DISABLED			0x00000008
#define DO_HARDWARE_CURSOR_POSITIONING	0x00000010
#define DO_HARDWARE_UPDATE				0x00000010
#define POP_UP_SCREEN					0x00000020
#define SCREEN_HAS_TITLE_BAR			0x00000040
#define USE_AES_PROCEDURE_FOR_INPUT		0x00000080
#define SCREEN_UPDATE_IN_PROGRESS		0x00000100
#define MS_SCREEN_UPDATE_FLAG			0x00000200
#define MS_CURSOR_UPDATE_FLAG			0x00000400
#define MS_ACTIVATE_IN_PROGRESS_FLAG	0x00000800
#define MS_PENDING_ACTIVATE_FLAG		0x00001000

//
//  Used along with PROCESS_BLOCKED_ON_KEYBOARD.
//

#define	KEYBOARD_THREAD_RESUMED			0x00002000

//
// Screen state controlled by the I/O engine
//

#define NON_SWITCHABLE_SCREEN			0x00000001
#define MIRROR_SCREEN_ACTIVE			0x00000004
#define MIRROR_IMAGE_TRANSMIT_ACTIVE	0x00000008
#define MIRROR_CURSOR_NEEDS_UPDATED		0x00000010
#define MIRROR_IMAGE_NEEDS_UPDATED		0x00000020
#define SCREEN_IS_CLOSED_ON_PRIMARY		0x00000040
#define SCREEN_IS_CLOSED_ON_SECONDARY	0x00000080
#define DUMMY_SCREEN					0x00000100

/* START BLOCK COMMENT
**	#* Scroll directions #/
**	#define SCROLL_UP			1
**	#define SCROLL_DOWN			2
**	#define SCROLL_RIGHT		3
**	#define SCROLL_LEFT			4
END BLOCK COMMENT */

/* Key types */
#define KeyValue(key)	((BYTE)((key) & 0x000000ff))
#define KeyType(key)	((BYTE)(((key) >> 8) & 0x000000ff))
#define KeyStatus(key)  ((BYTE)(((key) >> 16) & 0x000000ff))
#define ScanCode(key)	((BYTE)(((key) >> 24) & 0x000000ff))

#define NORMAL_KEY			0x00
#define FUNCTION_KEY		0x01
#define ENTER_KEY			0x02
#define ESCAPE_KEY			0x03
#define BACKSPACE_KEY		0x04
#define DELETE_KEY			0x05
#define INSERT_KEY			0x06
#define CURSOR_UP_KEY		0x07
#define CURSOR_DOWN_KEY		0x08
#define CURSOR_RIGHT_KEY	0x09
#define CURSOR_LEFT_KEY		0x0a
#define CURSOR_HOME_KEY		0x0b
#define CURSOR_END_KEY		0x0c
#define CURSOR_PUP_KEY		0x0d
#define CURSOR_PDOWN_KEY	0x0e

/* Screen manager requests */
#define SWITCH_SCREEN_REQUEST		0x01
#define SELECT_SCREEN_REQUEST		0x02
/* GIVE_SWITCH_SCREEN_REQUEST		0x03 in screen.inc */
#define DOWN_SERVER_REQUEST			0x04


#define RIGHT_SHIFT_KEY		0x01
#define LEFT_SHIFT_KEY		0x02
#define CONTROL_KEY			0x04
#define ALT_KEY				0x08
#define SCROLL_LOCK_STATE	0x10
#define NUM_LOCK_STATE		0x20
#define CAPS_LOCK_STATE		0x40

#if ScreenDebug
#define SCREEN_SIGNATURE	0x34561289L
#endif

#define SCREEN_ROWS			25
#define SCREEN_COLUMNS		80
#define SCREEN_SIZE			(SCREEN_ROWS * SCREEN_COLUMNS * 2)

#define TYPE_AHEAD_BUFFER_SIZE	32
#define COMMAND_HISTORY_SIZE	300

#define SCREEN_PROCESS_STACK_SIZE	2048

struct ScreenStruct
{
#if ScreenDebug
	LONG signature;			/* Debug signiture- Must be 0x34561289 */
#endif

	struct ScreenStruct *previousScreen;
	struct ScreenStruct *nextScreen;
	struct ScreenStruct *popUpOriginalScreen;

	LONG *CLIBScreenStructure;

	BYTE currentPalette;
	BYTE popUpCount;
	BYTE screenList;			/* Used in multi-processor version only */
	BYTE activeCount;
	struct ResourceTagStructure *resourceTag;

	BYTE *screenName;
	BYTE *screenMemory;
	LONG flags;
	LONG state;
	WORD outputCursorPosition;	/* Offset from screenAddress * 2 */
	WORD inputCursorPosition;	/* Offset from 0, 0 */

	/* The position of variables after this point can change with */
	/* new versions of the OS.  They should not be accessed by NLMs */

	/* new stuff with 3.20 */
	BYTE *screenSaveBuffer;
	BYTE *titleBarSaveBuffer;
	LONG titleBarSize;
	BYTE *customDataBuffer;
	LONG customDataSize;
	void *customScreenInfo;

	struct ScreenHandlerStructure *SSScreenHandler;
	WORD screenMode;
	WORD cursorType;
	WORD screenHeight;		/* number of lines on the screen (text mode) */
	WORD screenWidth;		/* displayable characters per line (text mode) */
	LONG screenSize;		/* in bytes */
	LONG (*SSSetCursorMode)(LONG cursorType, LONG screenMode);
	void (*SSEnableCursor)(void);
	void (*SSDisableCursor)(void);
	void (*SSPositionCursor)(WORD position);
	LONG (*SSSetScreenMode)(LONG screenMode, struct ScreenStruct *screenID);

	LONG (*SSSaveFullScreen)(struct ScreenStruct *screenID, BYTE *buffer);
	LONG (*SSRestoreFullScreen)(struct ScreenStruct *screenID, BYTE *buffer);
	LONG (*SSSaveScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE *buffer);
	LONG (*SSRestoreScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE *buffer);
	LONG (*SSFillScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE character, BYTE attribute);
	LONG (*SSFillScreenAreaAttribute)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE attribute);
	LONG (*SSScrollScreenArea)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG height, LONG width, LONG count,
			BYTE newLineAttribute, LONG direction);
	LONG (*SSDisplayScreenLine)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG length, BYTE *textAndAttributes);
	LONG (*SSDisplayScreenText)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG length, BYTE *text);
	LONG (*SSDisplayScreenTextWithAttribute)(struct ScreenStruct *screenID,
			LONG line, LONG column, LONG length, BYTE lineAttribute, BYTE *text);
	LONG (*SSReadScreenCharacter)(struct ScreenStruct *screenID, LONG line,  
            LONG column, BYTE *character);
	LONG (*SSWriteScreenCharacter)(struct ScreenStruct *screenID, LONG line,
            LONG column, BYTE character);
	LONG (*SSWriteScreenCharacterAttribute)(struct ScreenStruct *screenID,
			LONG line, LONG column, BYTE character, BYTE attribute);
	LONG (*SSDisplayScreenTitleBar)(struct ScreenStruct *screenID,
			BYTE *leftText, BYTE *rightText);
	LONG (*SSRemoveScreenTitleBar)(struct ScreenStruct *screenID);
	LONG (*SSNotifyOfCustomDataUpdate)(struct ScreenStruct *screenID);

	LONG reservedArea[6];

/* WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! */
/* From here on the short screen structure does not match the real screen  */
/* structure.  This means that changing this offset will affect  */
/* otherEngineScreenID and transferOffset defined below. */

	WORD protectedInputRegionStart;
	WORD protectedInputRegionEnd;
	WORD initialInputCursorPosition;
	WORD allignmentPadding;

	LONG activeOnThisEngineCount;

	LONG screenSemaphore;	/* Valid when PROCESS_BLOCKED_ON_SCREEN is set */

	LONG keyboardProcessID;	/* Valid when KEYBOARD_INPUT_ACTIVE */

	BYTE typeAheadBufferCount;
	BYTE typeAheadBufferStart;
	BYTE typeAheadBufferEnd;
	BYTE MSScreenEventCount;
	LONG typeAheadBuffer[TYPE_AHEAD_BUFFER_SIZE];

	WORD commandHistoryEnd;
	WORD commandHistoryCurrent;
	BYTE commandHistoryBuffer[COMMAND_HISTORY_SIZE];

	//
	// New fields for rudimentary screen multithreading.
	//

	QUE			ScreenQue;
	SPINLOCK	ScreenSpinLock;

	/*
	 *	Fields for Address Space Information
	 */
	BYTE *AddressSpaceName;
};


struct ScreenInputRoutineStructure
{
	struct ScreenStruct *screenID;
	LONG (*routine)(LONG);
	struct ScreenInputRoutineStructure *next;
	struct ScreenInputRoutineStructure *previous;
	struct ResourceTagStructure *resourceTag;
};

/* Screen Handler structure */
struct ScreenHandlerStructure
{
	struct ScreenHandlerStructure *SHNext;
	struct ResourceTagStructure *SHResourceTag;
	struct LoadDefinitionStructure *SHModuleHandle;
	LONG SHVersion;			/* Version of screenHandlerStructure (1) */
	LONG SHScreenMode;		/* The mode this handler supports */
	WORD SHScreenHeight;		/* number of lines on the screen (text chars) */
	WORD SHScreenWidth;		/* displayable characters per line (text chars) */
	LONG SHScreenSize;		/* in bytes */
	LONG SHTitleBarSize;
	LONG SHCustomDataSize;
	void *SHCustomScreenInfo;
	LONG SHMSEngineUseCount;
	WORD SHNormalCursor;
	BYTE SHReserved[2];
	LONG (*SHSetCursorMode)(LONG cursorType, LONG screenMode);
	void (*SHEnableCursor)(void);
	void (*SHDisableCursor)(void);
	void (*SHPositionCursor)(WORD position);
	LONG (*SHSetScreenMode)(LONG screenMode, struct ScreenStruct *screenID);

	LONG (*SHSaveFullScreen)(struct ScreenStruct *screenID, BYTE *buffer);
	LONG (*SHRestoreFullScreen)(struct ScreenStruct *screenID, BYTE *buffer);
	LONG (*SHSaveScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE *buffer);
	LONG (*SHRestoreScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE *buffer);
	LONG (*SHFillScreenArea)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE character, BYTE attribute);
	LONG (*SHFillScreenAreaAttribute)(struct ScreenStruct *screenID, LONG line,
			LONG column, LONG height, LONG width, BYTE attribute);
	LONG (*SHScrollScreenArea)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG height, LONG width, LONG count,
			BYTE newLineAttribute, LONG direction);
	LONG (*SHDisplayScreenLine)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG length, BYTE *textAndAttributes);
	LONG (*SHDisplayScreenText)(struct ScreenStruct *screenID, LONG line,
            LONG column, LONG length, BYTE *text);
	LONG (*SHDisplayScreenTextWithAttribute)(struct ScreenStruct *screenID,
			LONG line, LONG column, LONG length, BYTE lineAttribute, BYTE *text);
	LONG (*SHReadScreenCharacter)(struct ScreenStruct *screenID, LONG line,  
            LONG column, BYTE *character);
	LONG (*SHWriteScreenCharacter)(struct ScreenStruct *screenID, LONG line,
            LONG column, BYTE character);
	LONG (*SHWriteScreenCharacterAttribute)(struct ScreenStruct *screenID,
			LONG line, LONG column, BYTE character, BYTE attribute);
	LONG (*SHDisplayScreenTitleBar)(struct ScreenStruct *screenID,
			BYTE *leftText, BYTE *rightText);
	LONG (*SHRemoveScreenTitleBar)(struct ScreenStruct *screenID);
	LONG (*SHNotifyOfCustomDataUpdate)(struct ScreenStruct *screenID);
};

/* These are the keyboard state flags returned by the call to  */
/* GetKeyboardStatus and also returned in the KeyStatus field by GetKey. */

#define RIGHT_SHIFT_KEY_DOWN	0x01
#define LEFT_SHIFT_KEY_DOWN		0x02
#define CONTROL_KEY_DOWN		0x04
#define ALT_KEY_DOWN			0x08
#define SCROLL_LOCK_ON			0x10
#define NUM_LOCK_ON				0x20
#define CAPS_LOCK_ON			0x40

/* Screen Types */
#define SCREEN_TYPE_TTY					0
#define SCREEN_TYPE_MONOCHROME			1
#define SCREEN_TYPE_DUAL_MODE			2
#define SCREEN_TYPE_CGA					3
#define SCREEN_TYPE_EGA					4
#define SCREEN_TYPE_VGA					5

/* Screen Modes */
#define SCREEN_MODE_TTY					0
#define SCREEN_MODE_80X25				1
#define SCREEN_MODE_80X43				2
#define SCREEN_MODE_80X50				3
#define SCREEN_MODE_D					0x0D
#define SCREEN_MODE_E					0x0E
#define SCREEN_MODE_F					0x0F
#define SCREEN_MODE_10					0x10
#define SCREEN_MODE_11					0x11
#define SCREEN_MODE_12					0x12
#define SCREEN_MODE_13					0x13

/* Cursor Types */
#define CURSOR_NORMAL					0x0C0B
#define CURSOR_THICK					0x0C09
#define CURSOR_BLOCK					0x0C00
#define CURSOR_TOP						0x0400

/*
 * Screen Activity Types 
 */
#define SCREEN_ACTIVITY_TEXT_SIZE	12
/*--------------------------------------------------------------------------**
** MOVED TO PROCDEFS.H - EXPORTED BY Jim A. Nicolet 11-15-2000              **
** // Screen Activity Error Levels                                          **
** //#define SAEL_OK						0			// Screen Activity             **
** //#define SAEL_INFO					1                                       **
** //#define SAEL_WARNING				2                                       **
** //#define SAEL_ERROR					3                                       **
**                                                                          **
** // Definitions                                                           **
** //extern void InitializeActivity(BYTE	*text);                           **
** //extern void TerminateActivity(BYTE *terminationText, UINT	errorLevel); **
** //                                                                       **
**--------------------------------------------------------------------------*/

#define	GET_NEXT_SCREEN(X)			((X)->nextScreen)

extern LONG RunningProcess;
extern LONG KeyboardProcessID;
extern struct ScreenStruct *systemConsoleScreen;
extern struct ScreenStruct *loggerConsoleScreen;

extern LONG AppScreenLock;

extern struct ScreenHandlerStructure defaultScreenHandler;
extern struct ScreenHandlerStructure *screenHandlerList;

extern struct ScreenStruct *screenListHead;
extern struct ScreenStruct *screenListTail;
extern struct ScreenStruct *activeScreen;
extern LONG screenManagerProcessID;
extern BYTE screenManagerKeyType;

extern struct ScreenInputRoutineStructure *inputRoutineListHead;
extern struct ResourceTagStructure *OSScreenRTag;

extern BYTE *ScreenAddress;
extern BYTE isColorScreen;
extern BYTE bellWasRung;
extern BYTE normalAttributeByte;
extern BYTE reverseAttributeByte;
extern BYTE titleBarSaveBuffer[];

/* functions */
extern struct ScreenStruct *GetDebugScreenID(void);

ERROR UnblockThreadBlockedOnKeyboardWithResume(THREAD ThreadHandle, ADDR ScreenHandle);

ERROR UnblockThreadBlockedOnScreenWithResume(THREAD ThreadHandle, ADDR ScreenHandle);

// These are used only in the kernel
LONG DirectUnformattedOutputToScreen(struct ScreenStruct *screenID, char *controlString);
LONG DirectOutputToScreen(struct ScreenStruct *screenID, void *controlString, ...);
LONG DirectOutputToScreenWithAttribute(
		struct ScreenStruct *screenID,
		BYTE attribute,
		void *controlString,
		...);
void DirectOutputToScreenInMargins(
	struct ScreenStruct *screenID,	/* Screen to be used	*/
	BYTE	*string,				/* String to be printed	*/
	int		 leftMargin,			/* Left margin			*/
	int		 rightMargin);			/* Right margin			*/


#endif // __SCREEN_H__

#if		0		// LEAVE THIS ALONE.
ELSE	; LEAVE THIS ALONE.

;****************************************************************************
;**************** BEGIN ASSEMBLY INCLUDE PORTION OF FILE. *******************
;****************************************************************************

ScreenDebug = 1

; Screen flags controlled by the owning engine
KEYBOARD_INPUT_ACTIVE			EQU		00000001h
PROCESS_BLOCKED_ON_KEYBOARD		EQU		00000002h
PROCESS_BLOCKED_ON_SCREEN		EQU		00000004h
INPUT_CURSOR_DISABLED			EQU		00000008h
DO_HARDWARE_CURSOR_POSITIONING	EQU		00000010h
DO_HARDWARE_UPDATE				EQU		00000010h
POP_UP_SCREEN					EQU		00000020h
SCREEN_HAS_TITLE_BAR			EQU		00000040h
USE_AES_PROCEDURE_FOR_INPUT		EQU		00000080h
SCREEN_UPDATE_IN_PROGRESS		EQU		00000100h
MS_SCREEN_UPDATE_FLAG			EQU		00000200h
MS_CURSOR_UPDATE_FLAG			EQU		00000400h
MS_ACTIVATE_IN_PROGRESS_FLAG	EQU		00000800h
MS_PENDING_ACTIVATE_FLAG		EQU		00001000h
KEYBOARD_THREAD_RESUMED			EQU		00002000H

; Screen state controlled by the I/O engine
NON_SWITCHABLE_SCREEN		equ	00000001h
MIRROR_SCREEN_ACTIVE		equ	00000004h
MIRROR_IMAGE_TRANSMIT_ACTIVE	equ	00000008h
MIRROR_CURSOR_NEEDS_UPDATED	equ	00000010h
MIRROR_IMAGE_NEEDS_UPDATED	equ	00000020h
SCREEN_IS_CLOSED_ON_PRIMARY	equ	00000040h
SCREEN_IS_CLOSED_ON_SECONDARY	equ	00000080h
DUMMY_SCREEN			equ	00000100h

;;;; Screen scroll directions
;;;SCROLL_UP			equ	1
;;;SCROLL_DOWN			equ	2
;;;SCROLL_RIGHT			equ	3
;;;SCROLL_LEFT			equ	4

;Key types
NORMAL_KEY		equ	00h
FUNCTION_KEY		equ	01h
ENTER_KEY		equ	02h
ESCAPE_KEY		equ	03h
BACKSPACE_KEY		equ	04h
DELETE_KEY		equ	05h
INSERT_KEY		equ	06h
CURSOR_UP_KEY		equ	07h
CURSOR_DOWN_KEY		equ	08h
CURSOR_RIGHT_KEY	equ	09h
CURSOR_LEFT_KEY		equ	0Ah
CURSOR_HOME_KEY		equ	0Bh
CURSOR_END_KEY		equ	0Ch
CURSOR_PUP_KEY		equ	0Dh
CURSOR_PDOWN_KEY	equ	0Eh

;Screen manager requests
SWITCH_SCREEN_REQUEST		equ	01h
SELECT_SCREEN_REQUEST		equ	02h
GIVE_SWITCH_SCREEN_REQUEST	equ	03h
DOWN_SERVER_REQUEST		equ	04h

if ScreenDebug
SCREEN_SIGNATURE	equ	34561289h
endif




SCREEN_ROWS		equ	25
SCREEN_COLUMNS		equ	80
SCREEN_SIZE		equ	(SCREEN_ROWS * SCREEN_COLUMNS * 2)

TYPE_AHEAD_BUFFER_SIZE	equ	32
COMMAND_HISTORY_SIZE	equ	300

if ScreenDebug

ScreenStruct	struc

	signature			dd	?

	previousScreen			dd	?
	nextScreen			dd	?
	popUpOriginalScreen		dd	?

	CLIBScreenStructure		dd	?

	currentPalette			db	?
	popUpCount			db	?
	screenList			db	?
	activeCount			db	?
	resourceTag			dd	?

	screenName			dd	?
	screenMemory			dd	?
	flags				dd	?
	state				dd	?
	outputCursorPosition		dw	?
	inputCursorPosition		dw	?

;	The position of variables after this point can change with
;	new versions of the OS.  They should not be accessed by NLMs

;	/* new stuff with 3.20 */
	screenSaveBuffer			dd	?
	titleBarSaveBuffer			dd	?
	titleBarSize				dd	?
	customDataBuffer			dd	?
	customDataSize				dd	?
	customScreenInfo			dd	?

	SSScreenHandler				dd	?
	SSscreenMode				dw	?
	SScursorType				dw	?
	SSscreenHeight				dw	?
	SSscreenWidth				dw	?
	SSscreenSize				dd	?
	SSSetCursorMode				dd	?
	SSEnableCursor				dd	?
	SSDisableCursor				dd	?
	SSPositionCursor			dd	?
	SSSetScreenMode				dd	?
	SSSaveFullScreen			dd	?
	SSRestoreFullScreen			dd	?
	SSSaveScreenArea			dd	?
	SSRestoreScreenArea			dd	?
	SSFillScreenArea			dd	?
	SSFillScreenAreaAttribute		dd	?
	SSScrollScreenArea			dd	?
	SSDisplayScreenLine			dd	?
	SSDisplayScreenText			dd	?
	SSDisplayScreenTextWithAttribute	dd	?
	SSReadScreenCharacter			dd	?
	SSWriteScreenCharacter			dd	?
	SSWriteScreenCharacterAttribute		dd	?
	SSDisplayScreenTitleBar			dd	?
	SSRemoveScreenTitleBar			dd	?
	SSNotifyOfCustomDataUpdate		dd	?

	reservedArea				dd	6 dup (?)

; WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING!
; From here on the short screen structure does not match the real screen 
; structure.  This means that changing this offset will affect 
; otherEngineScreenID and transferOffset defined below.
	protectedInputRegionStart 	dw	?
	protectedInputRegionEnd		dw	?
	initialInputCursorPosition 	dw	?
	allignmentPadding		dw	?

	activeOnThisEngineCount		dd	?

	screenSemaphore			dd	?

	keyboardInputProcessID		dd	?

	typeAheadBufferCount		db	?
	typeAheadBufferStart		db	?
	typeAheadBufferEnd		db	?
	reserved1			db	?
	typeAheadBuffer			dd	TYPE_AHEAD_BUFFER_SIZE dup (?)

	commandHistoryEnd		dw	?
	commandHistoryCurrent		dw	?
	commandHistoryBuffer		db	COMMAND_HISTORY_SIZE dup (?)

ScreenStruct	ends

else	;NOT ScreenDebug

ScreenStruct	struc

	previousScreen			dd	?
	nextScreen			dd	?
	popUpOriginalScreen		dd	?

	CLIBScreenStructure		dd	?

	currentPalette			db	?
	popUpCount			db	?
	screenList			db	?
	activeCount			db	?
	resourceTag			dd	?

	screenName			dd	?
	screenMemory			dd	?
	flags				dd	?
	state				dd	?
	outputCursorPosition		dw	?
	inputCursorPosition		dw	?

;	The position of variables after this point can change with
;	new versions of the OS.  They should not be accessed by NLMs

;	/* new stuff with 3.20 */
	screenSaveBuffer			dd	?
	titleBarSaveBuffer			dd	?
	titleBarSize				dd	?
	customDataBuffer			dd	?
	customDataSize				dd	?
	customScreenInfo			dd	?

	SSScreenHandler				dd	?
	SSscreenMode				dw	?
	SScursorType				dw	?
	SSscreenHeight				dw	?
	SSscreenWidth				dw	?
	SSscreenSize				dd	?
	SSSetCursorMode				dd	?
	SSEnableCursor				dd	?
	SSDisableCursor				dd	?
	SSPositionCursor			dd	?
	SSSetScreenMode				dd	?
	SSSaveFullScreen			dd	?
	SSRestoreFullScreen			dd	?
	SSSaveScreenArea			dd	?
	SSRestoreScreenArea			dd	?
	SSFillScreenArea			dd	?
	SSFillScreenAreaAttribute		dd	?
	SSScrollScreenArea			dd	?
	SSDisplayScreenLine			dd	?
	SSDisplayScreenText			dd	?
	SSDisplayScreenTextWithAttribute	dd	?
	SSReadScreenCharacter			dd	?
	SSWriteScreenCharacter			dd	?
	SSWriteScreenCharacterAttribute		dd	?
	SSDisplayScreenTitleBar			dd	?
	SSRemoveScreenTitleBar			dd	?
	SSNotifyOfCustomDataUpdate		dd	?

	reservedArea				dd	6 dup (?)

; WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING!
; From here on the short screen structure does not match the real screen 
; structure.  This means that changing this offset will affect 
; otherEngineScreenID and transferOffset defined below.
	protectedInputRegionStart 	dw	?
	protectedInputRegionEnd		dw	?
	initialInputCursorPosition 	dw	?

	screenSemaphore			dd	?

	keyboardInputProcessID		dd	?

	typeAheadBufferCount		db	?
	typeAheadBufferStart		db	?
	typeAheadBufferEnd		db	?
	reserved1			db	?
	typeAheadBuffer			dd	TYPE_AHEAD_BUFFER_SIZE dup (?)

	commandHistoryEnd		dw	?
	commandHistoryCurrent		dw	?
	commandHistoryBuffer		db	COMMAND_HISTORY_SIZE dup (?)

ScreenStruct	ends

endif	;NOT ScreenDebug

otherEngineScreenID	equ	(OFFSET protectedInputRegionStart)
transferOffset		equ	(OFFSET otherEngineScreenID + 4)
;
; These are the keyboard state flags returned by the call to 
; GetKeyboardStatus and also returned in the KeyStatus field by GetKey.
;

RIGHT_SHIFT_KEY_DOWN		equ	01h
LEFT_SHIFT_KEY_DOWN		equ	02h
CONTROL_KEY_DOWN		equ	04h
ALT_KEY_DOWN			equ	08h
SCROLL_LOCK_ON			equ	10h
NUM_LOCK_ON			equ	20h
CAPS_LOCK_ON			equ	40h

;* Screen Types
SCREEN_TYPE_TTY			equ	0
SCREEN_TYPE_MONOCHROME		equ	1
SCREEN_TYPE_DUAL_MODE		equ	2
SCREEN_TYPE_CGA			equ	3
SCREEN_TYPE_EGA			equ	4
SCREEN_TYPE_VGA			equ	5

;* Screen Modes
SCREEN_MODE_TTY			equ	0
SCREEN_MODE_80X25		equ	1
SCREEN_MODE_80X43		equ	2
SCREEN_MODE_80X50		equ	3
SCREEN_MODE_D			equ	0Dh
SCREEN_MODE_E			equ	0Eh
SCREEN_MODE_F			equ	0Fh
SCREEN_MODE_10			equ	10h
SCREEN_MODE_11			equ	11h
SCREEN_MODE_12			equ	12h
SCREEN_MODE_13			equ	13h

;* Cursor Types
CURSOR_NORMAL			equ	0C0Bh
CURSOR_THICK			equ	0C09h
CURSOR_BLOCK			equ	0C00h
CURSOR_TOP			equ	0400h

;*****************************************************************************
;*****************************************************************************

ENDIF		; LEAVE THIS ALONE.
COMMENT();	// LEAVE THIS ALONE.
#endif		// LEAVE THIS ALONE.

/* (	; LEAVE THIS ALONE.
; END ; REMOVE THE FIRST SEMICOLON IF THIS FILE ENDS A COMPILATION UNIT IN A .386 FILE.
;*/		// LEAVE THIS ALONE.
