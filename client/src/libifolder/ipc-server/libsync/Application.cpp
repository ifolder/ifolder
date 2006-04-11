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
*  Author(s): Russ Young <ryoung@novell.com>
*
***********************************************************************/
#include "Application.h"

#include <QDir>

namespace iFolderCore
{
	
	QString		Application::m_DataPath;
	bool		Application::m_Initialized = false;
	QMutex		Application::m_Mutex;
	

int Application::Initialize()
{
	return Initialize(QDir::homePath() + "/.ifolder3");
}

int Application::Initialize(QString dataPath)
{
	m_Mutex.lock();
	if (!m_Initialized)
	{
		m_DataPath = dataPath;
		QDir dataDir(m_DataPath);
		if (!dataDir.exists())
			dataDir.mkpath(m_DataPath);
		m_Initialized = true;
	}
	m_Mutex.unlock();
	
	return 0;
}

}
