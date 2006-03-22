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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 *  \mainpage iFolder 3 Client API Documentation
 *  \section intro_sec Introduction
 *
 *  This header file contains all the includes you need to write to
 *  libifolder.
 *
 *  \section rules_sec Rules
 *
 *  In general you should follow these rules:
 *  - Release/free items returned from a function.
 *      - For example, if a function returns a iFolderAccount, you should
 *     release the iFolderAccount object when you are finished with it
 *     by calling ifolder_account_release(&ifolder_account).
 * 
 ***********************************************************************/
#ifndef _IFOLDER_CLIENT_H
/* @cond */
#define _IFOLDER_CLIENT_H 1
/* @endcond */

#include "errors.h"
#include "account.h"
#include "ifolder.h"
#include "user.h"
#include "events.h"

#endif
