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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Dale Olds <olds@novell.com>
 *
 ***********************************************************************/
using System;
using System.IO;
using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to assist in conflict resolution
/// </summary>
public class Conflict
{
	Collection collection;
	Node node;

	//---------------------------------------------------------------------------
	/// <summary>
	/// constructor, looks a lot like a Node
	/// </summary>
	public Conflict(Collection collection, Node node)
	{
		this.collection = collection;
		this.node = node;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the file name of the temporary file that conflicts with local file for this node
	/// </summary>
	public string ConflictFileName
	{
		get { return "conflict full name here"; }
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the contents of the node that conflicts with this node
	/// </summary>
	public Node ConflictingNode
	{
		get { return null; }
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve conflict and commit 
	/// </summary>
	public void Resolve(bool localChangesWin)
	{
		if (localChangesWin)
		{
			// remove temp file and node property
			// update incarnation to conflict incarn + 1
		}
		else
		{
			// move temp file over visible one
			// move node over this one
		}
		collection.Commit(node);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve conflict and commit 
	/// </summary>
	public void Resolve(string newNodeName)
	{
		// rename node
		// move temp file over visible one
		// move node over this one
		collection.Commit(node);
	}
}

//===========================================================================
}
