/****************************************************************************
 *
 *                      旼컴컴컴컴컴컴컴컴컴컴컴컴커
 *                      � FOR INTERNAL USE ONLY!!! �
 *                      읕컴컴컴컴컴컴컴컴컴컴컴컴켸
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 * Filename:      TIMECONV.C
 *
 * Date Created:  8 DECEMBER 2000
 *
 * Version:       NetWare 6.0
 *
 * Programmers:   Piyush Kumar Srivastava
 *
 * Files used:    nwsms.h string.h zFriends.h
 *
 * Date Modified: 12/8/00
 *
 * Modifications: For 6-Pack, the time conversion from UTC to DOS and vice-versa
 *				  were needed.
 *
 * Comments:
 
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized or
 * modified in any manner or compiled, linked or uploaded or downloaded to or
 * from any computer system without the prior written consent of Novell, Inc.
 ****************************************************************************/

#include <smstypes.h>
#include <zFriends.h>
#include <timeconvc.h>

void NWConvertUTCToDOSTime(UINT32 *DOSTime, QUAD UTCTime)
{
	*DOSTime = (DOSTime_t)xUTC2dosTime((Time_t)UTCTime);
	return;
}

void NWConvertDOSToUTCTime(QUAD *UTCTime, UINT32 DOSTime)
{
	*UTCTime = (QUAD)xDOS2utcTime((DOSTime_t)DOSTime);
	return;
}

/*****************************************************************************/
