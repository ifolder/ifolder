/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/
namespace Novell.iFolder
{
	using System;
	using System.IO;

	public class DirectoryEntry
	{
		private Boolean			isDirectory;
		private DirectoryInfo	dirInfo;
		private FileInfo		fileInfo;

		public DirectoryEntry(DirectoryInfo dInfo)
		{
			isDirectory = true;
			dirInfo = dInfo;
		}

		public DirectoryEntry(FileInfo fInfo)
		{
			isDirectory = false;
			fileInfo = fInfo;
		}
		
		private DirectoryEntry()
		{
			isDirectory = true;
		}

		public DirectoryInfo GetDirectoryInfo()
		{
			return dirInfo;
		}

		public FileInfo GetFileInfo()
		{
			return fileInfo;
		}

		public string FullName
		{
			get
			{
				if(isDirectory)
					return(dirInfo.FullName);
				else
					return(fileInfo.FullName);
			}
		}

		public string Name
		{
			get
			{
				if(isDirectory)
					return(dirInfo.Name);
				else
					return(fileInfo.Name);
			}
		}

		public Boolean IsDirectory
		{
			get
			{
				return isDirectory;
			}
		}
	}
}
