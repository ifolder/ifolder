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

#ifndef _IFOLDER_H_
#define _IFOLDER_H_

#include <QString>
#include <QList>

#include "IFUser.h"
#include "IFChangeEntry.h"

/**
 * @file IFiFolder.h
 * @brief iFolder API (API for individual iFolders)
 */

class IFiFolder
{
	public:
		~IFiFolder();

		enum MemberRights {Deny, ReadOnly, ReadWrite, Admin};

		void publish();
		void unPublish();
		void setDescription(QString *newDescription);
		QList<IFUser> getMembers(int index, int count);
		void setMemberRights(IFUser member, MemberRights rights);
		void addMember(IFUser member, MemberRights rights);
		void removeMember(IFUser member);
		void setOwner(IFUser member);
		QList<IFChangeEntry> getChanges(int index, int count);
		QList<IFChangeEntry> getFileChanges(QString *relativePath, int index, int count);
		
	private:
		IFiFolder();	

		friend class IFDomain;
};

#endif /*_IFOLDER_H_*/
