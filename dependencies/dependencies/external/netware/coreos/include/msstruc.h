#ifndef __MSSTRUC_H__
#define __MSSTRUC_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
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
 *  $Workfile:   msstruc.h  $
 *  $Modtime:   Sep 02 1999 16:31:34  $
 *  $Revision$
 *
 ****************************************************************************/

/*********************************************************************************
 * Program Name:  NetWare 386
 *
 * Filename:	  msstruc.h
 *
 * Date Created:  September 30, 1991
 *
 * Version:		  1.0
 *
 * Programmers:	  Jim A. Nicolet
 *
 * Files used:
 *
 * Date Modified: 
 *
 * Modifications: 
 *
 * Comments: This file contains function call prototypes for Migrate Services
 *
 ****************************************************************************/

typedef struct
{
	LONG Hflag;
	LONG DataStreamNumber;
	LONG FileHandle;
} HSTR;

typedef struct
{
	LONG DataStreamNumber;
	LONG DataSize;
} DMINFO;


typedef struct
{
	LONG FileSystemTypeID;
	LONG VolumeID;
	LONG Volume;
	LONG PrimaryKey;
	LONG SecondaryKey;
	BYTE *FileName;
} FSINFO;

/****************************************************************************/
/****************************************************************************/


#endif /* __MSSTRUC_H__ */
