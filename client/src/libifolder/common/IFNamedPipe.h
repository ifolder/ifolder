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

#ifndef _IFOLDER_NAMED_PIPE_H_
#define _IFOLDER_NAMED_PIPE_H_

#include <QString>

//! An abstraction for named pipes
/**
 * Named pipes are used as the IPC mechanism in the iFolder Client and this
 * class provides an abstraction to the innards of how they work since they are
 * different on the three platforms we need to support.
 */
class iFolderNamedPipe
{
	public:
		//! Specifies the type of named pipe to use
		/**
		 * Used by the iFolderNamedPipe construtor to specify which kind of
		 * named pipe to use.  It is, by design, a one-way pipe.
		 */
		enum PermissionType
		{
			ReadOnly,	/*!< A one-way named pipe that can be read from */
			WriteOnly	/*!< A one-way named pipe that can be written to */
		};

		/**
		 * @param pathName the absolute path to the named pipe
		 */
		iFolderNamedPipe(QString pathName, PermissionType permissionType);
		virtual ~iFolderDomain();
		
		//! Open up the named pipe for reading or writing
		/**
		 * Opens the named pipe in the mode specified by the constructor.
		 * 
		 * @param createIfNeeded if true, this will create the named pipe if
		 * it does not exist.  This is false by default.
		 * @returns a file descriptor to read or write to or -1 if an error
		 * occurs.
		 */
		int open(bool createIfNeeded = false);
		
		//! Force the named pipe to be closed
		/**
		 * This is called in the destructor if not called explicitly.
		 */
		int close();
		
	private:
		//! Used by open() to create a named pipe when needed.
		int create();
};

#endif /*_IFOLDER_NAMED_PIPE_H_*/
