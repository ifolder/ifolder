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
#include "SyncFileInfo.h"

namespace iFolderCore
{
	
QChar SyncFileInfo::m_Separator = '|';

SyncFileInfo::SyncFileInfo(
	QString name, 
	QDateTime created, 
	QDateTime modified,
	qint64 size,
	bool isDir,
	QString id,
	qint64	serverVersion)
{
	d = new SyncFileInfoData();
	d->m_Name = name;
	d->m_Created = created;
	d->m_Modified = modified;
	d->m_Size = size;
	d->m_IsDir = isDir;
	d->m_ID = id;
	d->m_ServerVersion = serverVersion;
}

SyncFileInfo::SyncFileInfo(QFileInfo& fInfo)
{
	d = new SyncFileInfoData();
	d->m_Name = fInfo.fileName();
	d->m_Created = fInfo.created();
	d->m_Modified = fInfo.lastModified();
	d->m_Size = fInfo.size();
	d->m_IsDir = fInfo.isDir();
	d->m_ID = "";
	d->m_ServerVersion = 0;
}

SyncFileInfo::~SyncFileInfo()
{
}

bool SyncFileInfo::Serialize(QTextStream& stream)
{
	stream << d->m_Name;
	stream << m_Separator + d->m_Created.toString(Qt::ISODate);
	stream << m_Separator + d->m_Modified.toString(Qt::ISODate);
	stream << m_Separator << d->m_Size;
	stream << m_Separator << d->m_IsDir;
	stream << m_Separator + d->m_ID;
	stream << m_Separator << d->m_ServerVersion;
	stream << endl;
	return true;
}

SyncFileInfo SyncFileInfo::Deserialize(QTextStream& stream)
{
	QString line = stream.readLine();
	QStringList list = line.split(m_Separator);
	if (list.size() == 7)
	{
		SyncFileInfo  sfi(
			list.at(0),
			QDateTime::fromString(list.at(1), Qt::ISODate),
			QDateTime::fromString(list.at(2), Qt::ISODate),
			list.at(3).toLongLong(),
			list.at(4).toLong(),
			list.at(5),
			list.at(6).toLongLong());
		return sfi;
	}
}

SyncFileInfoList::SyncFileInfoList(const QFileInfoList& list)
{
	foreach (QFileInfo fInfo, list)
		append(SyncFileInfo(fInfo));
}

int	SyncFileInfoList::Save(QFile& file)
{
	if (file.open(QFile::WriteOnly | QFile::Truncate))
	{
		QTextStream outs(&file);
		foreach (SyncFileInfo info, *this)
			info.Serialize(outs);		
		file.close();
	}
	return 0;
}
	
int SyncFileInfoList::Restore(QFile& file)
{
	if (file.open(QFile::ReadOnly))
	{
		QTextStream ins(&file);
		while (!ins.atEnd())
			append(SyncFileInfo::Deserialize(ins));
		file.close();
		return 0;
	}
	return -1;
}

}
