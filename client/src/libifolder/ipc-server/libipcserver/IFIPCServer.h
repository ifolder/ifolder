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

#ifndef _IFOLDER_IPC_SERVER_H_
#define _IFOLDER_IPC_SERVER_H_

//#include <QObject>
#include <QThread>

#include <common/IFNamedPipe.h>

class iFolderIPCServer : public QThread
{
//	Q_OBJECT
	
	public:
		iFolderIPCServer(/*QObject *parent*/);
		virtual ~iFolderIPCServer();
		
		void run();
		void gracefullyExit();
	
	private:
		int processMessage(int messageType, void *message);
	
		iFolderNamedPipe *serverNamedPipe;
		bool bExit;
};

#endif /*_IFOLDER_IPC_SERVER_H_*/

