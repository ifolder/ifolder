#ifndef __TIMER_H__
#define __TIMER_H__
/*****************************************************************************
 *
 *	(C) Copyright 1987-1994 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   timer.h  $
 *  $Modtime:   27 Aug 1998 16:21:06  $
 *  $Revision$
 *  
 ****************************************************************************/

struct	TimerDataStructure
{
	struct TimerDataStructure *TLink;
	void (*TCallBackProcedure)(LONG parameter); /* set by the caller; unchanged */
	LONG TCallBackEBXParameter;			/* set by the caller; unchanged */
	LONG TCallBackWaitTime;				/* set by the caller; unchanged; looked at only when the call is made. */
	struct ResourceTagStructure *TResourceTag; /* set by the caller; unchanged */
	LONG TWorkWakeUpTime;
	LONG TSignature;
};

LONG ConvertSecondsRelativeToDOS(int SecondsRelativeToYear2000);
int ConvertDOSToSecondsRelative(LONG DOSDateAndTime);
int ConvertDOSDateTimeToSeconds( LONG DOSDateAndTime );
int GetCurrentSecondsRelative( void );
int GetZeroSecondsRelative( void );

typedef struct
{
	BYTE CurrentYear;
	BYTE CurrentMonth;
	BYTE CurrentDay;
	BYTE CurrentHour;
	BYTE CurrentMinute;
	BYTE CurrentSecond;
	BYTE CurrentWeekDay;
} timeVector;


/****************************************************************************/
/****************************************************************************/


#endif /* __TIMER_H__ */
