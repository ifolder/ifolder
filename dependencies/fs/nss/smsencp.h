/*	
****************************************************************************
*
* Program Name:  Storage Management Services (NWSMS Lib)
*
* modname: smsencp.h    version: 1.3    date: 12/11/95
* PVCS:        $Revision$   $date$
*
* Date Created:  15 SEPTEMBER 1993
*
* Version:       5.0
*
* Files used:    
*
* Date Modified: 
*
* Modifications: 
*
* Comments:      Paste job until the OS header file encp.h gets proper
*                (include) latches                                          
*
* (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
*
* No part of this file may be duplicated, revised, translated, localized or
* modified in any manner or compiled, linked or uploaded or downloaded to or
* from any computer system without the prior written consent of Novell, Inc.
*
****************************************************************************
*/

#ifndef _SMSENCP_H_INCLUDED       /* smsencp.h header latch */
#define _SMSENCP_H_INCLUDED

#if !defined(IOEngine)
#	define IOEngine	0
#endif
#if !defined(FSEngine)
#	define FSEngine	0
#endif

#ifdef N_PLAT_NLM
#include <config.h>
#include <nwtypes.h>
#include <encp.h>
#define ReadAhead	1	/* for bits.h, see also version.h */
#include <bits.h>
#include <nspace.h>
#elif defined(N_PLAT_GNU)
#include <lcfsdefines.h>
#endif


#if defined(MIGRATED)
#	include <dstruct.h>
#	include <migrate.h>
#endif

#endif                            /* smsencp.h header latch */
