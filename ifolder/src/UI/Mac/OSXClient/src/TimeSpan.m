/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*       These functions are used to calculate and convert seconds to appropirate 
* time units.
*
*******************************************************************************/

#import "TimeSpan.h"

#define SECONDS_IN_MINUTE	(long)60
#define SECONDS_IN_HOUR		(long)60*60
#define SECONDS_IN_DAY		(long)60*60*24

@implementation TimeSpan

//============================================================
// getTimeSpanValue
// Convert the time span from seconds to minutes or hours or days
//============================================================
+(long)getTimeSpanValue:(long)seconds
{
	long secondsValue = seconds;
	long secDay = SECONDS_IN_DAY;
	long secHour = SECONDS_IN_HOUR;
	long secMinute = SECONDS_IN_MINUTE;

	if( (seconds >= secDay) &&
		((seconds % secDay) == 0) )
	{
		secondsValue = seconds/secDay;
	}
	else if( (seconds >= secHour) &&
			 ((seconds % secHour) == 0) )
	{
		secondsValue = seconds/secHour;
	}
	else if( (seconds >= secMinute) &&
		     ((seconds % secMinute) == 0) )
	{
		secondsValue = seconds/secMinute;
	}

	return secondsValue;
}

//============================================================
// getTimeSpanUnits
// Get the units after converting seconds 
//============================================================
+(NSString *)getTimeSpanUnits:(long)seconds
{
	long secDay = SECONDS_IN_DAY;
	long secHour = SECONDS_IN_HOUR;
	long secMinute = SECONDS_IN_MINUTE;

	if( (seconds >= secDay) &&
		(seconds % secDay) == 0)
	{
		if(seconds/secDay == 1)
			return NSLocalizedString(@"day", @"Time span for one day, sync in 1 \"day\"");
		else
			return NSLocalizedString(@"days", @"Time span for multiple days, sync in 5 \"days\"");
	}
	if( (seconds >= secHour) &&
		(seconds % secHour) == 0)
	{
		if(seconds/secHour == 1)
			return NSLocalizedString(@"hour", @"Time span for one hour, sync in 1 \"hour\"");
		else
			return NSLocalizedString(@"hours", @"Time span for multiple hours, sync in 5 \"hours\"");
	}
	if( (seconds >= secMinute) &&
		(seconds % secMinute) == 0)
	{
		if(seconds/secMinute == 1)
			return NSLocalizedString(@"minute", @"Time span for one minute, sync in 1 \"minute\"");
		else
			return NSLocalizedString(@"minutes", @"Time span for multiple minutes, sync in 5 \"minutes\"");
	}
	if(seconds == 1)
		return NSLocalizedString(@"second",  @"Time span for one second, sync in 1 \"second\"");
	else
		return NSLocalizedString(@"seconds", @"Time span for multiple seconds, sync in 5 \"seconds\"");
}


//============================================================
// getTimeInded
// Get the index instead of string after converting
//============================================================
+(int)getTimeIndex:(long)seconds
{
	long secDay = SECONDS_IN_DAY;
	long secHour = SECONDS_IN_HOUR;
	long secMinute = SECONDS_IN_MINUTE;

	if( (seconds >= secDay) &&
		(seconds % secDay) == 0)
	{
		return 0;
	}
	if( (seconds >= secHour) &&
		(seconds % secHour) == 0)
	{
		return 1;
	}
	if( (seconds >= secMinute) &&
		(seconds % secMinute) == 0)
	{
		return 2;
	}
	return 3;
}

//============================================================
// getSeconds
// Convert the valude and index received to seconds
//============================================================
+(long)getSeconds:(long)value withIndex:(int)index
{
	long seconds = value;
	switch(index)
	{
		case 0:
			seconds = value * SECONDS_IN_DAY;
			break;
		case 1:
			seconds = value * SECONDS_IN_HOUR;
			break;
		case 2:
			seconds = value * SECONDS_IN_MINUTE;
			break;
	}
	return seconds;
}



@end
