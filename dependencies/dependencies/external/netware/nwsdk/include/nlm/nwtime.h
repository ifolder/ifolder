#ifndef _NWTIME_H_
#define _NWTIME_H_
/*============================================================================
=  NetWare C NLM Runtime Library source code
=
=  Unpublished Copyright (C) 1997 by Novell, Inc. All rights reserved.
=
=  No part of this file may be duplicated, revised, translated, localized or
=  modified in any manner or compiled, linked or uploaded or downloaded to or
=  from any computer system without the prior written consent of Novell, Inc.
=
=  nwtime.h
==============================================================================
*/
#include <nwtypes.h>

#ifndef CLOCK_IS_SYNCHRONIZED
# define CLOCK_IS_SYNCHRONIZED           0x01
# define _CLOCKANDSTATUS_T
typedef LONG clockAndStatus[3];
#endif

#ifndef CLOCK_IS_NETWORK_SYNCHRONIZED
# define CLOCK_IS_NETWORK_SYNCHRONIZED   0x02
#endif

#ifndef CLOCK_SYNCHRONIZATION_IS_ACTIVE
# define CLOCK_SYNCHRONIZATION_IS_ACTIVE 0x04
#endif


#ifndef _TIME_T
# define _TIME_T
typedef long time_t;
#endif

#include <npackon.h>

#ifdef NW_MAX_PORTABILITY
/*
** The following macros are established to get and put time and date infor-
** mation from and to the DOS time and date structures in a portable way.
** The other method (using bit fields with expected names) is clearer, but
** isn't usually portable according to ANSI.
*/
# define GET_HOUR_FROM_TIME(packed_time)     (((packed_time) & 0xf800) >> 11)
# define GET_MINUTE_FROM_TIME(packed_time)   (((packed_time) & 0x07e0) >> 5)
# define GET_BISECOND_FROM_TIME(packed_time) ((packed_time) & 0x001f)

# define GET_YEAR_FROM_DATE(packed_date)     (((packed_date) & 0xfe00) >> 9)
# define GET_MONTH_FROM_DATE(packed_date)    (((packed_date) & 0x01e0) >> 5)
# define GET_DAY_FROM_DATE(packed_date)      ((packed_date) & 0x001f)

# define PUT_HOUR_IN_TIME(hour, packed_time)                 \
        packed_time |= (WORD)(((hour) << 11) & 0xf800)
# define PUT_MINUTE_IN_TIME(minute, packed_time)             \
        packed_time |= (WORD)(((minute) << 5) & 0x07e0)
# define PUT_BISECOND_IN_TIME(bisecond, packed_time)         \
        packed_time |= (WORD)((bisecond) & 0x001f)

# define PUT_YEAR_IN_DATE(year, packed_date)                 \
        packed_date |= (WORD)(((year) << 9) & 0xfe00)
# define PUT_MONTH_IN_DATE(month, packed_date)               \
        packed_date |= (WORD)(((month) << 5) & 0x01e0)
# define PUT_DAY_IN_DATE(day, packed_date)                   \
        packed_date |= (WORD)((day) & 0x001f)

struct _DOSTime { WORD packed_time; };
struct _DOSDate { WORD packed_date; };

#else

struct _DOSTime
{
   WORD  bisecond : 5;     /* two second increments (0 - 29) */
   WORD  minute   : 6;     /* 0 - 59                         */
   WORD  hour     : 5;     /* 0 - 23                         */
};

struct _DOSDate
{
   WORD  day          : 5; /* 1 - 31                     */
   WORD  month        : 4; /* 1 - 12                     */
   WORD  yearsSince80 : 7; /* years since 1980 (0 - 119) */
};

#endif

/*
** These are copies of the original, Intel-use-only structures.
*/
struct __DOSTime
{
   WORD  bisecond : 5;     /* two second increments (0 - 29) */
   WORD  minute   : 6;     /* 0 - 59                         */
   WORD  hour     : 5;     /* 0 - 23                         */
};

struct __DOSDate
{
   WORD  day          : 5; /* 1 - 31                     */
   WORD  month        : 4; /* 1 - 12                     */
   WORD  yearsSince80 : 7; /* years since 1980 (0 - 119) */
};

/*
** These unions are useful to avoid type coercions.
*/
typedef union DOSDate
{
   WORD            Date;
   struct _DOSDate date;
} DOSDate;

typedef union DOSTime
{
   WORD            Time;
   struct _DOSTime time;
} DOSTime;

typedef union DOSDateAndTime
{
   LONG DOSDateTime;

   struct
   {
      WORD Time;
      WORD Date;
   } DOS;

   struct
   {
      struct _DOSTime time;
      struct _DOSDate date;
   } dos;
} DOSDateAndTime;

#ifndef _STRUCT_TM
#define _STRUCT_TM
struct tm
{
   int   tm_sec;              /* seconds after the minute--[0, 59]           */
   int   tm_min;              /* minutes after the hour--[0, 59]             */
   int   tm_hour;             /* hours since midnight--[0, 23]               */
   int   tm_mday;             /* days of the month--[1, 31]                  */
   int   tm_mon;              /* months since January--[0, 11]               */
   int   tm_year;             /* years since 1900--[0, 99]                   */
   int   tm_wday;             /* days since Sunday--[0, 6]                   */
   int   tm_yday;             /* days since first of January--[0, 365]       */
   int   tm_isdst;            /* Daylight Savings Time flag--[-, 0, +]       */
};                            /* (+ in effect, 0 if not, - if unknown)       */
#endif

#include <npackoff.h>

#ifdef __cplusplus
extern "C"
{
#endif

/* prototypes... */
time_t   _ConvertDOSTimeToCalendar( LONG dateTime );
void     _ConvertTimeToDOS( time_t calendarTime,
            struct _DOSDate *filDatP, struct _DOSTime *filTimP );
void     GetClockStatus( clockAndStatus _dataPtr );
LONG     GetCurrentTicks( void );
LONG     GetHighResolutionTimer( void );
LONG     GetSuperHighResolutionTimer( void );
LONG     NWGetHighResolutionTimer(void);
LONG     NWGetSuperHighResolutionTimer(void);
size_t	NWLstrftime(char *string, size_t maxSize, const char *format, const struct tm *timePtr);
time_t  *__get_altzone( void );
int     *__get_daylight( void );
time_t  *__get_daylightOffset( void );
int     *__get_daylightOnOff( void );
time_t  *__get_timezone( void );
void    SecondsToTicks(LONG Seconds, LONG TenthsOfSeconds, LONG *Ticks);
void    TicksToSeconds(LONG Ticks, LONG *Seconds, LONG *TenthsOfSeconds);

#ifdef __cplusplus
}
#endif


#define altzone        (*__get_altzone())
#define timezone       (*__get_timezone())      /* offset in seconds from GMT */
#define daylight       (*__get_daylight())      /* means tzname[1] is valid */
#define daylightOffset (*__get_daylightOffset())/* offset in seconds to DST */
#define daylightOnOff  (*__get_daylightOnOff()) /* current status of DST */


#endif
