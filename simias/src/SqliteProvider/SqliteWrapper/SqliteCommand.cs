// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteCommand.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Data;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// 
	/// </summary>
        public class SqliteCommand : IDbCommand
        {
                SqliteConnection parent_conn;
//                SqliteTransaction transaction;
                IDbTransaction transaction;
                string sql;
                int timeout;
                CommandType type;
                UpdateRowSource upd_row_source;
                SqliteParameterCollection sql_params;

			/// <summary>
			/// 
			/// </summary>
        		public SqliteCommand ()
                {
					sql = "";
					sql_params = new SqliteParameterCollection ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="sqlText"></param>
			/// <param name="dbConn"></param>
                public SqliteCommand (string sqlText, SqliteConnection dbConn) :
					this()
                {
                        sql = sqlText;
						parent_conn = dbConn;
						CommandTimeout = 2000;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="sqlText"></param>
			/// <param name="dbConn"></param>
			/// <param name="trans"></param>
                public SqliteCommand (string sqlText, SqliteConnection dbConn, IDbTransaction trans) :
					this(sqlText, dbConn)
                {
					transaction = trans;
                }

			/// <summary>
			/// 
			/// </summary>
                public void Dispose ()
                {
                }

			/// <summary>
			/// 
			/// </summary>
                public string CommandText {
                        get {
                                return sql;
                        }
                        set {
							    sql = value;
                        }
                }

                // note that we could actually implement
                // a timeout with sqlite, but setting up a signal to interrupt us after
                // a certain amount of time, but it's probably not worth the effort
			/// <summary>
			/// 
			/// </summary>
                public int CommandTimeout {
                        get {
                                return timeout;
                        }
                        set {
                                timeout = value;
								sqlite_busy_timeout(parent_conn.Handle, timeout);
                        }
                }

                IDbConnection IDbCommand.Connection {
                        get {
                                return parent_conn;
                        }
                        set {
                                if (!(value is SqliteConnection)) {
                                        throw new InvalidOperationException ("Can't set Connection to something other than a SqliteConnection");
                                }
                                parent_conn = (SqliteConnection) value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public SqliteConnection Connection {
                        get {
                                return parent_conn;
                        }
                        set {
                                parent_conn = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public CommandType CommandType {
                        get {
                                return type;
                        }
                        set {
                                type = value;
                        }
                }

                IDataParameterCollection IDbCommand.Parameters {
                        get {
                                return Parameters;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public SqliteParameterCollection Parameters {
                        get {
                                return sql_params;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public IDbTransaction Transaction {
                        get {
                                return transaction;
                        }
                        set {
                                transaction = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public UpdateRowSource UpdatedRowSource {
                        get {
                                return upd_row_source;
                        }
                        set {
                                upd_row_source = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public void Prepare ()
                {
                }

			/// <summary>
			/// 
			/// </summary>
                public void Cancel ()
                {
                }

                IDbDataParameter IDbCommand.CreateParameter ()
                {
                        return CreateParameter ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
                public SqliteParameter CreateParameter ()
                {
                        return new SqliteParameter ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
                public int ExecuteNonQuery ()
                {
                        int rows_affected;
                        SqliteDataReader r = ExecuteReader (CommandBehavior.Default, false, out rows_affected);
                        return rows_affected;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
                public object ExecuteScalar ()
                {
                        SqliteDataReader r = ExecuteReader ();
                        if (r == null || !r.Read ()) {
                                return null;
                        }
                        object o = r[0];
                        r.Close ();
                        return o;
                }

                IDataReader IDbCommand.ExecuteReader ()
                {
                        return ExecuteReader ();
                }

                IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
                {
                        return ExecuteReader (behavior);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
                public SqliteDataReader ExecuteReader ()
                {
                        return ExecuteReader (CommandBehavior.Default);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="behavior"></param>
			/// <returns></returns>
                public SqliteDataReader ExecuteReader (CommandBehavior behavior)
                {
                        int r;
                        return ExecuteReader (behavior, true, out r);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="behavior"></param>
			/// <param name="want_results"></param>
			/// <param name="rows_affected"></param>
			/// <returns></returns>
			public SqliteDataReader ExecuteReader (CommandBehavior behavior, bool want_results, out int rows_affected)
			{
				SqliteDataReader reader = null;
				SqliteError err;

				parent_conn.StartExec ();

				string msg = "";
				unsafe 
				{
					byte *msg_result;

					try 
					{
						if (want_results) 
						{
							//reader = new SqliteStepDataReader(this);
							reader = new SqliteDataReader(this);

							/*
							reader = new SqliteDataReader (this);
							SqliteCallbackFunction cbf = new SqliteCallbackFunction(reader.SqliteCallback);
							err = sqlite_exec (parent_conn.Handle,
									sql,
									cbf,
									0, &msg_result);
							*/
							

							err = (SqliteError)reader.ExecRead(sql, &msg_result);
							reader.ReadingDone ();
						}
						else 
						{
							err = sqlite_exec (parent_conn.Handle,
								sql,
								null,
								0, &msg_result);
						}
					} 
					finally 
					{
						parent_conn.EndExec ();
					}

					if (msg_result != null)
					{
						StringBuilder sb = new StringBuilder ();

						for (byte *y = msg_result; *y != 0; y++)
							sb.Append ((char) *y);
						msg = sb.ToString ();

						sqliteFree (msg_result);
					}
				}

				if (err != SqliteError.OK)
				{
					throw new ApplicationException ("Sqlite error " + msg);
				}

				rows_affected = NumChanges ();
				return reader;
			}


                internal int NumChanges () {
                        return sqlite_changes (parent_conn.Handle);
                }

                internal unsafe delegate int SqliteCallbackFunction (int c, int argc, sbyte **argv, sbyte **colnames);

                [DllImport("sqlite")]
                unsafe static extern SqliteError sqlite_exec (IntPtr handle, string sql, SqliteCallbackFunction callback,
							      int user_data, [In, Out] byte **errstr_ptr);

		[DllImport ("sqlite")]
		unsafe static extern void sqliteFree (void *ptr);
		
                [DllImport("sqlite")]
                static extern int sqlite_changes (IntPtr handle);

			[DllImport("sqlite")]
			static extern void sqlite_busy_timeout(IntPtr handle, int ms);

                internal enum SqliteError : int {
                        OK,
                        Error,
                        Internal,
                        Perm,
                        Abort,
                        Busy,
                        Locked,
                        NoMem,
                        ReadOnly,
                        Interrupt,
                        IOErr,
                        Corrupt,
                        NotFound,
                        Full,
                        CantOpen,
                        Protocol,
                        Empty,
                        Schema,
                        TooBig,
                        Constraint,
                        Mismatch,
                        Misuse,
						Nolfs,
						Auth,
						Format,
						Range,
						Row = 100,
						Done = 101
                }
        }
}
