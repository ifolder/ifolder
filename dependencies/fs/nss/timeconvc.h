/****************************************************************************
 *
 *                      旼컴컴컴컴컴컴컴컴컴컴컴컴커
 *                      � FOR INTERNAL USE ONLY!!! �
 *                      읕컴컴컴컴컴컴컴컴컴컴컴컴켸
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 * Filename:      TIMECONV.H
 *
 * Date Created:  27 Aug 2002
 *
 * Version:       NetWare 6.1
 *
 * Programmers:   R.Shyamsundar
 *
 * Files used:    portable.h
 *
 * Date Modified: 
 *
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized or
 * modified in any manner or compiled, linked or uploaded or downloaded to or
 * from any computer system without the prior written consent of Novell, Inc.
 ****************************************************************************/

#ifndef _TIMECONVC_H_
#define _TIMECONVC_H_
//#include <portable.h>

void NWConvertUTCToDOSTime(UINT32 *DOSTime, QUAD UTCTime);
void NWConvertDOSToUTCTime(QUAD *UTCTime, UINT32 DOSTime);

#endif /* Header latch _TIMECONVC_H_ */
