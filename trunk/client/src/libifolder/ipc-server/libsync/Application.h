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
#ifndef APPLICATION_H_
#define APPLICATION_H_

#include <QString>
#include <QMutex>

//#include "../ifolder-errors.h"

namespace iFolderCore
{

class Application
{
public:
	static int Initialize();
	static int Initialize(QString dataPath);

	// Getters
	inline static QString DataPath() {return m_DataPath;}
	
private:
	static QString		m_DataPath;
	static bool		m_Initialized;
	static QMutex		m_Mutex;
};

}

#endif /*APPLICATION_H_*/
