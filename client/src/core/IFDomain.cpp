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
#include "IFDomain.h"

IFDomain::IFDomain(utf8string name, utf8string id, utf8string url)
{
	m_Name = name;
	m_ID = id;
	m_Url = url;
}

IFDomain::~IFDomain(void)
{
}

IFDomain IFDomain::Add(utf8string name, utf8string id, utf8string url)
{
	return IFDomain(name, id, url);
}

int IFDomain::Remove()
{
	return 0;
}

int IFDomain::Login()
{
	return 0;
}

int IFDomain::Logout()
{
	return 0;
}
