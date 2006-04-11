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

#ifndef _IFOLDER_IPC_CLIENT_H_
#define _IFOLDER_IPC_CLIENT_H_

//#include <QObject>
#include <QThread>

#include <common/IFNamedPipe.h>
#include <common/IFMessages.h>

class iFolderIPCClient : public QThread
{
//	Q_OBJECT
	
	public:
		iFolderIPCClient();
//		iFolderIPCServer(QObject *parent = NULL);
		virtual ~iFolderIPCClient();
		
		//! This must be called before the thread is started
		/**
		 * If this call does not return IFOLDER_SUCCESS, the
		 * QThread.run() will do nothing.
		 */
//		int registerClient();
		int init();
		int registerClient();
		void run();
		void gracefullyExit();

		//! Initialize an iFolderMessageHeader before making an IPC request
		void initHeader(iFolderMessageHeader *header, uint messageType);
		
	private:
		//! Make an ipcCall to the server
		int ipcCall(void *request, void **response);

		int processMessage(int messageType, void *message);

		int ipcCallRegisterClient(iFolderNamedPipe *serverNamedPipe, iFolderNamedPipe *tempNamedPipe, iFolderMessageRegisterClientRequest *request, iFolderMessageRegisterClientResponse **response);
		
		int unregisterClient();
		int ipcCallUnregisterClient(iFolderNamedPipe *serverNamedPipe, iFolderNamedPipe *tempNamedPipe, iFolderMessageUnregisterClientRequest *request, iFolderMessageUnregisterClientResponse **response);
	
		iFolderNamedPipe *clientNamedPipe;
		char clientNamedPipePath[NAMED_PIPE_PATH_MAX];
		bool bExit;
		bool bInitialized;
};

#endif /*_IFOLDER_IPC_CLIENT_H_*/

