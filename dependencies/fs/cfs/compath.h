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
 | $Modtime: 13 Sep. 2002 $
 |
 | $Workfile: Compath.h $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This file is used to:
 |		Implement the defn. for _NWStoreAsComponentPath function.
 +-------------------------------------------------------------------------*/

#ifndef _COMPATH_H_
#define _COMPATH_H_

#include <smstypes.h>

CCODE _NWStoreAsComponentPath(STRING componentPath,	UINT8  nameSpace,	STRING path,	BOOL pathIsFullyQualified);

#endif /* Header latch */
