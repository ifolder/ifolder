// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteConnection.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;
using System.Data;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// 
	/// </summary>
	public class SqliteConnection : IDbConnection
	{
		string conn_str;
		string db_file;
		int db_mode;
		IntPtr sqlite_handle;
		
		ConnectionState state;

		/// <summary>
		/// 
		/// </summary>
		public SqliteConnection ()
		{
			db_file = null;
			db_mode = 0644;
			state = ConnectionState.Closed;
			sqlite_handle = IntPtr.Zero;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connstring"></param>
		public SqliteConnection (string connstring)
			: this ()
		{
			ConnectionString = connstring;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose ()
		{
			Close ();
		}

		/// <summary>
		/// 
		/// </summary>
		public string ConnectionString
		{
			get 
			{
				return conn_str;
			}
			set 
			{
				if (value == null) 
				{
					Close ();
					conn_str = null;
					return;
				}

				if (value != conn_str) 
				{
					Close ();
					conn_str = value;

					db_file = null;
					db_mode = 0644;

					string[] conn_pieces = value.Split (',');
					foreach (string piece in conn_pieces) 
					{
						piece.Trim ();
						string[] arg_pieces = piece.Split ('=');
						if (arg_pieces.Length != 2) 
						{
							throw new InvalidOperationException ("Invalid connection string");
						}
						string token = arg_pieces[0].ToLower ();
						string tvalue = arg_pieces[1];
						string tvalue_lc = arg_pieces[1].ToLower ();
						if (token == "uri") 
						{
							if (tvalue_lc.StartsWith ("file://")) 
							{
								db_file = tvalue.Substring (6);
							} 
							else if (tvalue_lc.StartsWith ("file:")) 
							{
								db_file = tvalue.Substring (5);
							} 
							else if (tvalue_lc.StartsWith ("/")) 
							{
								db_file = tvalue;
							} 
							else 
							{
								throw new InvalidOperationException ("Invalid connection string: invalid URI");
							}
						} 
						else if (token == "mode") 
						{
							db_mode = Convert.ToInt32 (tvalue);
						}
					}

					if (db_file == null) 
					{
						throw new InvalidOperationException ("Invalid connection string: no URI");
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int ConnectionTimeout
		{
			get 
			{
				return 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string Database
		{
			get 
			{
				return db_file;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public ConnectionState State
		{
			get 
			{
				return state;
			}
		}

		internal IntPtr Handle
		{
			get 
			{
				return sqlite_handle;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Open ()
		{
			if (conn_str == null) 
			{
				throw new InvalidOperationException ("No database specified");
			}

			if (state != ConnectionState.Closed) 
			{
				return;
			}

			string errmsg;
			sqlite_handle = sqlite_open (db_file, db_mode, out errmsg);

			if (errmsg != null) 
			{
				throw new ApplicationException (errmsg);
			}

			state = ConnectionState.Open;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Close ()
		{
			if (state != ConnectionState.Open) 
			{
				return;
			}

			state = ConnectionState.Closed;

			sqlite_close (sqlite_handle);
			sqlite_handle = IntPtr.Zero;
		}

		/// <summary>
		/// 
		/// </summary>
		public int LastInsertRowId 
		{
			get 
			{
				return sqlite_last_insert_rowid (Handle);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="databaseName"></param>
		public void ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public SqliteCommand CreateCommand ()
		{
			return new SqliteCommand (null, this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IDbTransaction BeginTransaction ()
		{
			return new SqliteTransaction(this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public IDbTransaction BeginTransaction (IsolationLevel il)
		{
			return new SqliteTransaction(this, il);
		}

		internal void StartExec ()
		{
			// use a mutex here
			state = ConnectionState.Executing;
		}

		internal void EndExec ()
		{
			state = ConnectionState.Open;
		}

		[DllImport("sqlite")]
		static extern IntPtr sqlite_open (string dbname, int db_mode, out string errstr);

		[DllImport("sqlite")]
		static extern void sqlite_close (IntPtr sqlite_handle);

		[DllImport("sqlite")]
		static extern int sqlite_last_insert_rowid (IntPtr sqlite_handle);
	}
}
