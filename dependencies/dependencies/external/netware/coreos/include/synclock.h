/*****************************************************************************
 *
 *	(C) Copyright 1989-1994 Novell, Inc.
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
 *  $Workfile:   synclock.h  $
 *  $Modtime:   27 Aug 1998 16:32:26  $
 *  $Revision$
 *  
 ****************************************************************************/


#ifndef	SYNCLOCK_H_INCLUDED

/************************************************************************/
/* Definition for OS Time Zone string												*/
/************************************************************************/
#define MAX_TIME_ZONE_STRING_LENGTH		80

/************************************************************************/
/* Structure and declarations used by clients									*/
/************************************************************************/
/*

	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC
	WARNING:  Any changes must be mirrored in SYNCLOCK.INC

*/

typedef	struct	Synchronized_Clock_T
{
	LONG	clock[2];				/* [0] = whole seconds, [1] = fractional*/
	LONG	statusFlags;
	LONG	adjustmentCount;
	LONG	adjustment[2];			/* [0] = whole seconds, [1] = fractional*/
	LONG	grossCorrection[2];		/* [0] = whole seconds, [1] = fractional*/
	LONG	tickIncrement[2];		/* [0] = whole seconds, [1] = fractional*/
	LONG	stdTickIncrement[2];	/* [0] = whole seconds, [1] = fractional*/
	LONG	eventOffset[2];			/* [0] = whole seconds, [1] = ignored fractional*/
	LONG	eventTime;				/* whole seconds only */
	LONG	daylight;				/* 1 if daylight savings time name is in timeZoneString*/
	long	timezoneOffset;			/* seconds to UTC;LocalTime+timezone = UTC*/
	long	tzname[2];				/* offset to normal and daylight timezone names in timeZoneString*/
	char	timeZoneString[MAX_TIME_ZONE_STRING_LENGTH];	/* The actual time zone string represented by above values */
	long	daylightOffset;			/* additional offset during Daylight Savings Time*/
	long	daylightOnOff;			/* 0 = not in daylight savings time (OFF), nonzero = ON */
	LONG	startDSTime;			/* When does Daylight Savings time begin*/
	LONG	stopDSTime;				/* When does Daylight Savings time stop */
	}	Synchronized_Clock_T;

typedef	LONG	clockAndStatus[3];

/************************************************************************/
/* Flag definitions for getting and setting clock fields						*/
/************************************************************************/

#define	SYNCCLOCK_CLOCK_BIT					0x00000001L
#define	SYNCCLOCK_TICK_INCREMENT_BIT		0x00000002L
#define	SYNCCLOCK_ADJUSTMENT_BIT			0x00000004L
#define	SYNCCLOCK_GROSS_CORRECTION_BIT		0x00000008L
#define	SYNCCLOCK_ADJUSTMENT_COUNT_BIT		0x00000010L
#define	SYNCCLOCK_STATUS_BIT				0x00000020L
#define	SYNCCLOCK_STD_TICK_BIT				0x00000040L
#define	SYNCCLOCK_EVENT_TIME_BIT			0x00000080L
#define	SYNCCLOCK_EVENT_OFFSET_BIT			0x00000100L
#define	SYNCCLOCK_HARDWARE_CLOCK_BIT		0x00000200L
#define	SYNCCLOCK_RESERVED1_BIT				0x00000400L
#define	SYNCCLOCK_DAYLIGHT_BIT				0x00000800L
#define	SYNCCLOCK_TIMEZONE_OFFSET_BIT		0x00001000L
#define	SYNCCLOCK_TZNAME_BIT				0x00002000L
#define	SYNCCLOCK_TIMEZONE_STR_BIT			0x00004000L
#define	SYNCCLOCK_DAYLIGHT_OFFSET_BIT		0x00008000L
#define	SYNCCLOCK_DAYLIGHT_ON_OFF_BIT		0x00010000L
#define	SYNCCLOCK_START_DST_BIT				0x00020000L
#define	SYNCCLOCK_STOP_DST_BIT				0x00040000L
#define	SYNCCLOCK_ALL_DEFINED_BITS		   (0x0007FFFFL & ~SYNCCLOCK_HARDWARE_CLOCK_BIT)

/************************************************************************/
/* Bits used in the TIMESYNC status word - eventOffset[1]					*/
/************************************************************************/

#define	TIMESYNC_TA_CANCEL		0x80000000
#define	TIMESYNC_TA_ORIGINATE	0x10000000
#define	TIMESYNC_TA_SRC_MASK		0x00FFFFFF
#define	TIMESYNC_TA_COMPLETED	0x40000000

/************************************************************************/

/************************************************************************/
/* Bit masks used with 32 bit clock status										*/
/************************************************************************/

#define	CLOCK_IS_SYNCHRONIZED				0X00000001L
#define	CLOCK_IS_NETWORK_SYNCHRONIZED		0x00000002L
#define	CLOCK_SYNCHRONIZATION_IS_ACTIVE		0x00000004L
#define	CLOCK_EXTERNAL_SYNC_ACTIVE			0x00000008L
#define	CLOCK_STATUS_EXTRN_SYNC_TYPE		0x0000F000L
#define	CLOCK_STATUS_SERVER_TYPE			0x00000F00L

/************************************************************************/
/* function prototypes	-- include procdefs.h to get these					*/
/* 								or define needssyncclockprotottypes 			*/
/************************************************************************/

#ifdef needssyncclockprototypes
extern	void	GetSyncClockFields(LONG bitMap, Synchronized_Clock_T *aClock);
extern	void	SetSyncClockFields(LONG bitMap, Synchronized_Clock_T *aClock);
extern	LONG	GetCurrentClock(clockAndStatus *dataPtr);
#endif

/************************************************************************/

#define	SYNCLOCK_H_INCLUDED
#endif

/****************************************************************************/
/****************************************************************************/

