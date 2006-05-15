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
#include "IFiFolder.h"

IFiFolder::IFiFolder(gchar *pPath) :
	m_RootDir(pPath)
{
}

IFiFolder::~IFiFolder(void)
{
}

IFiFolder* IFiFolder::Create(gchar *pPath)
{
	return new IFiFolder(pPath);
}

gboolean IFiFolder::IsiFolder(gchar *pPath)
{
	return false;
}
	

int IFiFolder::Revert()
{
	return 0;
}
int IFiFolder::Connect()
{
	return 0;
}
int IFiFolder::Sync()
{
	m_RootDir.DetectChanges();
	return 0;
}
int IFiFolder::Stop()
{
	return 0;
}
