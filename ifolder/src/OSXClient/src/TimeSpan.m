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
	if( (seconds > SECONDS_IN_DAY) &&
		(seconds % SECONDS_IN_DAY) == 0)
	{
		return (seconds/SECONDS_IN_DAY);
	}
	if( (seconds > SECONDS_IN_HOUR) &&
		(seconds % SECONDS_IN_HOUR) == 0)
	{
		return (seconds/SECONDS_IN_HOUR);
	}
	if( (seconds > SECONDS_IN_MINUTE) &&
		(seconds % SECONDS_IN_MINUTE) == 0)
	{
		return (seconds/SECONDS_IN_MINUTE);
	}
	return seconds;
}

+(NSString *)getTimeSpanUnits:(long)seconds
{
	if( (seconds > SECONDS_IN_DAY) &&
		(seconds % SECONDS_IN_DAY) == 0)
	{
		if(seconds/SECONDS_IN_DAY == 1)
			return @"day";
		else
			return @"days";
	}
	if( (seconds > SECONDS_IN_HOUR) &&
		(seconds % SECONDS_IN_HOUR) == 0)
	{
		if(seconds/SECONDS_IN_HOUR == 1)
			return @"hour";
		else
			return @"hours";
	}
	if( (seconds > SECONDS_IN_MINUTE) &&
		(seconds % SECONDS_IN_MINUTE) == 0)
	{
		if(seconds/SECONDS_IN_MINUTE == 1)
			return @"minute";
		else
			return @"minutes";
	}
	if(seconds == 1)
		return @"second";
	else
		return @"seconds";
}


@end
