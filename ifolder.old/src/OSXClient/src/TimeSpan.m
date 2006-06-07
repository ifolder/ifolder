/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

#import "TimeSpan.h"

#define SECONDS_IN_MINUTE	(long)60
#define SECONDS_IN_HOUR		(long)60*60
#define SECONDS_IN_DAY		(long)60*60*24

@implementation TimeSpan

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
