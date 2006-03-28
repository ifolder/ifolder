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
 ***********************************************************************/

#ifndef IFOLDERCHANGEENTRY_H_
#define IFOLDERCHANGEENTRY_H_

#include <QObject>
#include <QString>
//#include <QTime>

class iFolderChangeEntry : public QObject
{
	Q_OBJECT
	
//	Q_PROPERTY(QTime time READ time)
	Q_PROPERTY(ChangeEntryType type READ type)
	Q_PROPERTY(QString id READ id)
	Q_PROPERTY(QString name READ name)
	Q_PROPERTY(QString userID READ userID)
	Q_PROPERTY(QString userFullName READ userFullName)
	
	public:
		~iFolderChangeEntry();
		
		enum ChangeEntryType {Add, Modify, Delete, Unknown};
		
//		QTime time();
		ChangeEntryType type();
		QString id();
		QString name();
		QString userID();
		QString userFullName();
	
	private:
		iFolderChangeEntry(QObject *parent = 0);

		friend class iFolder;
};

#endif /*IFOLDERCHANGEENTRY_H_*/
