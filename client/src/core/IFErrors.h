/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
#ifndef _IFErrors_H_
#define _IFErrors_H_

#include <glib.h>

struct _SimiasError_
{
	gchar	*pName;
	int		Code;
};

extern struct _SimiasError_ SimiasErrorToIFError(const gchar *pError);

extern GQuark	IF_CORE_ERROR;

#define IF_SUCCESS		0		// The operation was successful.

#define IF_ERR_ERROR	0xff01ffff	// The operation failed.

#define IF_ERR_ALREADY_INITIALIZED	0xff011000
#define	IF_ERR_NOT_AUTHENTICATED	0xff011001

// Errors returned from simias.
#define IF_ERR_SuccessInGrace		0xff011100
#define IF_ERR_InvalidCertificate	0xff011101
#define IF_ERR_UnknownUser			0xff011102
#define IF_ERR_AmbiguousUser		0xff011103
#define IF_ERR_InvalidCredentials	0xff011104
#define IF_ERR_InvalidPassword		0xff011105
#define IF_ERR_AccountDisabled		0xff011106
#define IF_ERR_AccountLockout		0xff011107
#define IF_ERR_SimiasLoginDisabled	0xff011108
#define IF_ERR_UnknownDomain		0xff011109
#define IF_ERR_InternalException	0xff01110a
#define IF_ERR_MethodNotSupported	0xff01110b
#define IF_ERR_Timeout				0xff01110c




#endif //_IFErrors_H_
