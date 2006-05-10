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
#include <common/IFMessages.h>

class IFIPCServer : public QThread
{
//	Q_OBJECT
	
	public:
		IFIPCServer();
//		IFIPCServer(QObject *parent = NULL);
		virtual ~IFIPCServer();
		
		void run();
		void gracefullyExit();
	
		//! Initialize an iFolderMessageHeader before making an IPC request
		void initHeader(iFolderMessageHeader *header, uint messageType);
		
	private:
		//! Make an ipcCall to the server
		int ipcRespond(QString repsonseNamedPipePath, void *response);

		int processMessage(uint messageType, void *message);
		
		int handleRegisterClientRequest(iFolderMessageRegisterClientRequest *message);
		int handleUnregisterClientRequest(iFolderMessageUnregisterClientRequest *message);
		
		int handleDomainAddRequest(iFolderMessageDomainAddRequest *message);
	
		IFNamedPipe *serverNamedPipe;
		bool bExit;
};

#endif /*_IFOLDER_IPC_SERVER_H_*/

