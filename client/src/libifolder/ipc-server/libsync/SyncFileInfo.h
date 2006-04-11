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
#ifndef SYNCFILEINFO_H_
#define SYNCFILEINFO_H_

#include <QSharedData>
#include <QString>
#include <QStringList>
#include <QFileInfo>
#include <QDateTime>
#include <QList>
#include <QTextStream>
#include <QFile>

namespace iFolderCore
{
	
class SyncFileInfoData : public QSharedData
{
friend class SyncFileInfo;

	QString		m_Name;
	QDateTime	m_Created;
	QDateTime	m_Modified;
	qint64		m_Size;
	bool		m_IsDir;
	QString		m_ID;
	qint64		m_ServerVersion;
};

class SyncFileInfo
{
public:
	SyncFileInfo(
		QString name, 
		QDateTime created, 
		QDateTime modified,
		qint64 Size,
		bool isDir,
		QString id,
		qint64	serverVersion);
	
	SyncFileInfo(QFileInfo& fInfo);
	
	virtual ~SyncFileInfo();

	// Getters
	inline QString Name() {return d->m_Name;} const
	inline QDateTime Created() {return d->m_Created;} const
	inline QDateTime Modified() {return d->m_Modified;} const
	inline qint64 Size() {return d->m_Size;} const
	inline bool IsDir() {return d->m_IsDir;} const
	inline QString ID() {return d->m_ID;} const 
	inline qint64 ServerVersion() {return d->m_ServerVersion;} const
	
	// Setters
	inline void SetID(QString id) {d->m_ID = id;}
	inline void SetServerVersion(qint64 sVersion) {d->m_ServerVersion = sVersion;}
	inline void SetModified(QDateTime modified) {d->m_Modified = modified;}
	
	bool SyncFileInfo::Serialize(QTextStream& stream);
	static SyncFileInfo Deserialize(QTextStream& stream);
	inline int Compare(const SyncFileInfo& other) 
	{
		QString s1 = d->m_Name;
		QString s2 = other.d->m_Name;
		return QString::compare(s1, s2);
	}
	
	bool Changed(const SyncFileInfo& other)
	{
		if (d->m_Size != other.d->m_Size || d->m_Modified != other.d->m_Modified)
		{
			return true;	
		}
		return false;
	}
	
	bool operator<(const SyncFileInfo& other) { return d->m_Name < other.d->m_Name; }
	bool operator>(const SyncFileInfo& other) { return d->m_Name > other.d->m_Name; }
	bool operator==(const SyncFileInfo& other) { return d->m_Name > other.d->m_Name; }
	
private:
	QSharedDataPointer<SyncFileInfoData>		d;	
	static QChar m_Separator;
};

//typedef QList<SyncFileInfo> SyncFileInfoList;

class SyncFileInfoList : public QList<SyncFileInfo>
{
public:
	inline SyncFileInfoList() { }
	inline SyncFileInfoList(const SyncFileInfoList &l) : QList<SyncFileInfo>(l) { }
	inline SyncFileInfoList(const QList<SyncFileInfo> &l) : QList<SyncFileInfo>(l) { }
	SyncFileInfoList(const QFileInfoList &l);
	
	int	Save(QFile& file);
	int Restore(QFile& file);
};

}

#endif /*SYNCFILEINFO_H_*/
