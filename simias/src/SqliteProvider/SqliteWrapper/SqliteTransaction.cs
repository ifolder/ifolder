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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Data;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// Summary description for SqliteTransaction.
	/// </summary>
	public class SqliteTransaction : IDbTransaction
	{
		SqliteConnection sqliteConn;
		SqliteCommand command;
		IsolationLevel isolationLevel;
		static string beginTransaction = "BEGIN";
		static string rollBack = "ROLLBACK";
		static string commit = "COMMIT";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conn"></param>
		public SqliteTransaction(SqliteConnection conn)
		{
			isolationLevel = IsolationLevel.ReadCommitted;
			sqliteConn = conn;
			command = conn.CreateCommand();
			command.CommandText = beginTransaction;
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="il"></param>
		public SqliteTransaction(SqliteConnection conn, IsolationLevel il) :
			this(conn)
		{
			isolationLevel = il;
		}
		
		#region IDbTransaction Members

		/// <summary>
		/// 
		/// </summary>
		public void Rollback()
		{
			command.CommandText = rollBack;
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Commit()
		{
			command.CommandText = commit;
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// 
		/// </summary>
		public IDbConnection Connection
		{
			get
			{
				return sqliteConn;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public System.Data.IsolationLevel IsolationLevel
		{
			get
			{
				return isolationLevel;
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			command.Dispose();
		}

		#endregion
	}
}
