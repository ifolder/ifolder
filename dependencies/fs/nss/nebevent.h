/****************************************************************************
 |
 |  (C) Copyright 2001 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 Storage Management Services (SMS)
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:  29 Sep 2003	$
 |
 | $Workfile: NEBEvent.h	$
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Define prototypes for NEBEvent.c file
 ****************************************************************************/

#ifndef _NEBEVENT_H_
#define _NEBEVENT_H_

#include <neb.h>
#include <nwapidef.h>
#include <zOmni.h>
#include <tsaunicode.h>
#include <smstypes.h>

typedef struct NEB_RENAMER_LIST
{
	NINT	entryExitID;
	unicode	oldName[NW_MAX_VOLUME_NAME_LEN];
	struct NEB_RENAMER_LIST	*next;
} TSA_NEB_RENAMER_LIST;

CCODE TSA_NEBEventRegister();
void TSA_NEBEventUnRegister();
void TSA_NEBVolumeRenameProducerRegistration(struct EventBlock *evBlk);
void TSA_NEBVolumeRenameEventEntry(		struct EventBlock	*evBlk );
void TSA_NEBVolumeRenameEventExit(struct EventBlock	*evBlk );


#endif /* Header latch - _NEBEVENT_H_ */
