// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteDataReader.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//
//  Russ Young (Novell) Added new class to use Step Data Reader.
//  

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// 
	/// </summary>
	public class SqliteDataReader : MarshalByRefObject,
		IEnumerable, IDataReader, IDisposable, IDataRecord
        {
		/// <summary>
		/// 
		/// </summary>
                protected SqliteCommand command;
		/// <summary>
		/// 
		/// </summary>
                protected ArrayList rows;
		/// <summary>
		/// 
		/// </summary>
                protected ArrayList columns;
		/// <summary>
		/// 
		/// </summary>
                protected Hashtable column_names;
		/// <summary>
		/// 
		/// </summary>
                protected int current_row;
		/// <summary>
		/// 
		/// </summary>
                protected bool closed;
		/// <summary>
		/// 
		/// </summary>
                protected bool reading;
		/// <summary>
		/// 
		/// </summary>
                protected int records_affected;

                internal SqliteDataReader (SqliteCommand cmd)
                {
                        command = cmd;
                        rows = new ArrayList ();
                        columns = new ArrayList ();
                        column_names = new Hashtable ();
                        closed = false;
                        current_row = -1;
                        reading = true;
                }

				internal virtual unsafe SqliteCommand.SqliteError ExecRead(string sql, [In, Out] byte **errmsg)
				{
					sbyte		**result;
					int			nrow = 0;
					int			ncolumn = 0;
					SqliteCommand.SqliteError	err = SqliteCommand.SqliteError.OK;

					try
					{
						err = sqlite_get_table(
							command.Connection.Handle,
							sql,
							&result,
							ref nrow,
							ref ncolumn,
							errmsg);

						if (err == SqliteCommand.SqliteError.OK)
						{
							if (nrow != 0)
							{
								// cache names of columns if we need to
								int i;
								for (i = 0; i < ncolumn; i++) 
								{
									string col = new String (result[i]);
									columns.Add (col);
									column_names[col.ToLower ()] = i;
								}

								ArrayList data_row = new ArrayList (nrow);
								int currentCol = 1;
								for (; i < (nrow + 1) * ncolumn; i++) 
								{
									if (result[i] != ((sbyte *)0)) 
									{
										data_row.Add(new String (result[i]));
									} 
									else 
									{
										data_row.Add(null);
									}
									if (currentCol == ncolumn)
									{
										rows.Add (data_row);
										data_row = new ArrayList(nrow);
										currentCol = 1;
									}
									else
									{
										currentCol++;
									}
								}
							}
							sqlite_free_table(result);
						}
					}
					catch// (Exception e)
					{
						//Console.WriteLine(e.Message);
						//Console.WriteLine(e.StackTrace);
					}

					return (err);
				}

				[DllImport("sqlite")]
				unsafe static extern SqliteCommand.SqliteError sqlite_get_table (IntPtr handle, string sql, [In, Out] sbyte ***result_ptr,
				ref int nrow, ref int ncolumn, [In, Out] byte **errstr_ptr);

				[DllImport("sqlite")]
				unsafe static extern void sqlite_free_table (sbyte **result_ptr);

                internal void ReadingDone ()
                {
                        records_affected = command.NumChanges ();
                        reading = false;
                }

		/// <summary>
		/// 
		/// </summary>
                public virtual void Close ()
                {
                        closed = true;
                }

		/// <summary>
		/// 
		/// </summary>
                public void Dispose ()
                {
                        // nothing to do
                }

		IEnumerator IEnumerable.GetEnumerator () {
			return new DbEnumerator (this);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public DataTable GetSchemaTable () {
			
			// We sort of cheat here since sqlite treats all types as strings
			// we -could- parse the table definition (since that's the only info
			// that we can get out of sqlite about the table), but it's probably
			// not worth it.

			DataTable dataTableSchema  = null;

			DataColumn dc;
			DataRow schemaRow;

			// only create the schema DataTable if
			// there is fields in a result set due
			// to the result of a query; otherwise,
			// a null needs to be returned
			if(this.FieldCount > 0) {
				
				dataTableSchema = new DataTable ();
				
				dataTableSchema.Columns.Add ("ColumnName", typeof (string));
				dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
				dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
				dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
				dataTableSchema.Columns.Add ("NumericScale", typeof (int));
				dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
				dataTableSchema.Columns.Add ("IsKey", typeof (bool));
				dc = dataTableSchema.Columns["IsKey"];
				dc.AllowDBNull = true; // IsKey can have a DBNull
				dataTableSchema.Columns.Add ("BaseCatalogName", typeof (string));
				dataTableSchema.Columns.Add ("BaseColumnName", typeof (string));
				dataTableSchema.Columns.Add ("BaseSchemaName", typeof (string));
				dataTableSchema.Columns.Add ("BaseTableName", typeof (string));
				dataTableSchema.Columns.Add ("DataType", typeof(Type));
				dataTableSchema.Columns.Add ("AllowDBNull", typeof (bool));
				dataTableSchema.Columns.Add ("ProviderType", typeof (int));
				dataTableSchema.Columns.Add ("IsAliased", typeof (bool));
				dataTableSchema.Columns.Add ("IsExpression", typeof (bool));
				dataTableSchema.Columns.Add ("IsIdentity", typeof (bool));
				dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (bool));
				dataTableSchema.Columns.Add ("IsRowVersion", typeof (bool));
				dataTableSchema.Columns.Add ("IsHidden", typeof (bool));
				dataTableSchema.Columns.Add ("IsLong", typeof (bool));
				dataTableSchema.Columns.Add ("IsReadOnly", typeof (bool));

				for (int i = 0; i < this.FieldCount; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();
										
					schemaRow["ColumnName"] = columns[i];
					schemaRow["ColumnOrdinal"] = i + 1;
					
					// FIXME: how do you determine the column size
					//        using SQL Lite?
					int columnSize = 8192; // pulled out of the air
					schemaRow["ColumnSize"] = columnSize;
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;

					schemaRow["IsUnique"] = false;
					schemaRow["IsKey"] = DBNull.Value;
					
					schemaRow["BaseCatalogName"] = "";
					
					schemaRow["BaseColumnName"] = columns[i];
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";

					// FIXME: don't know how to determine
					// the .NET type based on the
					// SQL Lite data type
					// Use string
					schemaRow["DataType"] = typeof(string);

					schemaRow["AllowDBNull"] = true;
					
					// FIXME: don't know how to get the
					//  SQL Lite data type
					int providerType = 0; // out of the air
					schemaRow["ProviderType"] = providerType;
					
					schemaRow["IsAliased"] = false;
					schemaRow["IsExpression"] = false;
					schemaRow["IsIdentity"] = false;
					schemaRow["IsAutoIncrement"] = false;
					schemaRow["IsRowVersion"] = false;
					schemaRow["IsHidden"] = false;
					schemaRow["IsLong"] = false;
					schemaRow["IsReadOnly"] = false;
					
					schemaRow.AcceptChanges();
					
					dataTableSchema.Rows.Add (schemaRow);
				}
			}
			return dataTableSchema;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
                public virtual bool NextResult ()
                {
                        current_row++;
                        if (current_row < rows.Count)
                                return true;
                        return false;
                }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
                public virtual bool Read ()
                {
                        return NextResult ();
                }

		/// <summary>
		/// 
		/// </summary>
                public int Depth {
                        get {
                                return 0;
                        }
                }

		/// <summary>
		/// 
		/// </summary>
                public bool IsClosed {
                        get {
                                return closed;
                        }
                }

		/// <summary>
		/// 
		/// </summary>
                public int RecordsAffected {
                        get {
                                return records_affected;
                        }
                }

                // sqlite callback
                internal unsafe int SqliteCallback (int c, int argc, sbyte **argv, sbyte **colnames)
                {
                        // cache names of columns if we need to
                        if (column_names.Count == 0) {
                                for (int i = 0; i < argc; i++) {
                                        string col = new String (colnames[i]);
                                        columns.Add (col);
                                        column_names[col.ToLower ()] = i;
                                }
                        }

                        ArrayList data_row = new ArrayList (argc);
                        for (int i = 0; i < argc; i++) {
                                if (argv[i] != ((sbyte *)0)) {
                                        data_row.Add(new String (argv[i]));
                                } else {
                                        data_row.Add(null);
                                }
                        }
                        rows.Add (data_row);
                        return 0;
                }

                //
                // IDataRecord getters
                //

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public bool GetBoolean (int i)
                {
                        return Convert.ToBoolean ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public byte GetByte (int i)
                {
                        return Convert.ToByte ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="fieldOffset"></param>
		/// <param name="buffer"></param>
		/// <param name="bufferOffset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public long GetBytes (int i, long fieldOffset, byte[] buffer, 
                               int bufferOffset, int length)
                {
                        throw new NotImplementedException ();
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public char GetChar (int i)
                {
                        return Convert.ToChar ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="fieldOffset"></param>
		/// <param name="buffer"></param>
		/// <param name="bufferOffset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public long GetChars (int i, long fieldOffset, char[] buffer, 
                               int bufferOffset, int length)
                {
                        throw new NotImplementedException ();
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public IDataReader GetData (int i)
                {
                        // sigh.. in the MSDN docs, it says that "This member supports the
                        // .NET Framework infrastructure and is not nitended to be used
                        // directly from your code." -- so why the hell is it in the public
                        // interface?
                        throw new NotImplementedException ();
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string GetDataTypeName (int i)
                {
                        return "text"; // SQL Lite data type
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public DateTime GetDateTime (int i)
                {
                        return Convert.ToDateTime ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public decimal GetDecimal (int i)
                {
                        return Convert.ToDecimal ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public double GetDouble (int i)
                {
                        return Convert.ToDouble ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Type GetFieldType (int i)
                {
                        return System.Type.GetType ("System.String"); // .NET data type
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public float GetFloat (int i)
                {
                        return Convert.ToSingle ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Guid GetGuid (int i)
                {
                        throw new NotImplementedException ();
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public short GetInt16 (int i)
                {
                        return Convert.ToInt16 ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public int GetInt32 (int i)
                {
                        return Convert.ToInt32 ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public long GetInt64 (int i)
                {
                        return Convert.ToInt64 ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string GetName (int i)
                {
                        return (string) columns[i];
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int GetOrdinal (string name)
                {
                        return (int) column_names[name];
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string GetString (int i)
                {
                        return ((string) ((ArrayList) rows[current_row])[i]);
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public object GetValue (int i)
                {
                        return ((ArrayList) rows[current_row])[i];
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public int GetValues (object[] values)
                {
                        int num_to_fill = System.Math.Min (values.Length, columns.Count);
                        for (int i = 0; i < num_to_fill; i++) {
                                if (((ArrayList) rows[current_row])[i] != null) {
                                        values[i] = ((ArrayList) rows[current_row])[i];
                                } else {
                                        values[i] = DBNull.Value;
                                }
                        }
                        return num_to_fill;
                }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public bool IsDBNull (int i)
                {
                        if (((ArrayList) rows[current_row])[i] == null)
                                return true;
                        return false;
                }

		/// <summary>
		/// 
		/// </summary>
		public int FieldCount {
                        get {
                                if (current_row == -1 || current_row == rows.Count)
                                        return 0;
                                return columns.Count;
                        }
                }

		/// <summary>
		/// 
		/// </summary>
		public object this[string name] {
                        get {
                                return ((ArrayList) rows[current_row])[(int) column_names[name]];
                        }
                }
		
		/// <summary>
		/// 
		/// </summary>
		public object this[int i] {
                        get {
                                return ((ArrayList) rows[current_row])[i];
                        }
                }
        }

	/// <summary>
	/// 
	/// </summary>
	public class SqliteStepDataReader : SqliteDataReader
	{
		IntPtr	virtualMachine;

		internal SqliteStepDataReader(SqliteCommand cmd) :
			base(cmd)
		{
			virtualMachine = IntPtr.Zero;
		}

		internal override unsafe SqliteCommand.SqliteError ExecRead(string sql, [In, Out] byte **errmsg)
		{
			SqliteCommand.SqliteError err = SqliteCommand.SqliteError.OK;
			sbyte	*pSqlTail;

			err = sqlite_compile(
				command.Connection.Handle,
				sql,
				&pSqlTail,
				out virtualMachine,
				errmsg);

			return err;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Close ()
		{
			if (!closed)
			{
				unsafe
				{
					SqliteCommand.SqliteError err = SqliteCommand.SqliteError.OK;
					sbyte	*pErrmsg = null;
			
					if (virtualMachine != IntPtr.Zero)
					{
						err = sqlite_finalize(virtualMachine, &pErrmsg);
						virtualMachine = IntPtr.Zero;
					}

					if (pErrmsg != null)
					{
						sqliteFree(pErrmsg);
					}
				}
				closed = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool NextResult ()
		{
			unsafe
			{
				SqliteCommand.SqliteError err = SqliteCommand.SqliteError.OK;
				int	nColumn = 0;
				sbyte	**pValues;
				sbyte	**pNames;

				err = sqlite_step(
					virtualMachine,
					ref nColumn,
					&pValues,
					&pNames);

				if (err == SqliteCommand.SqliteError.Row)
				{
					// cache names of columns if we need to
					if (column_names.Count == 0) 
					{
						for (int i = 0; i < nColumn; i++) 
						{
							string col = new String (pNames[i]);
							columns.Add (col);
							column_names[col.ToLower ()] = i;
						}
					}

					ArrayList data_row = new ArrayList (nColumn);
					for (int i = 0; i < nColumn; i++) 
					{
						if (pValues[i] != ((sbyte *)0)) 
						{
							data_row.Add(new String (pValues[i]));
						} 
						else 
						{
							data_row.Add(null);
						}
					}
					rows.Insert(0, data_row);
					current_row = 0;;
					return true;
				}
				Close();
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool Read ()
		{
			return NextResult ();
		}

		[DllImport("sqlite")]
		unsafe static extern SqliteCommand.SqliteError sqlite_compile (IntPtr handle, string sql, [In, Out] sbyte **pzTail,
			out IntPtr pVm, [In, Out] byte **errstr_ptr);

		[DllImport("sqlite")]
		unsafe static extern SqliteCommand.SqliteError sqlite_step (IntPtr pVm, ref int nCol, [In, Out] sbyte ***pValues, [In, Out] sbyte ***pNames);

		[DllImport("sqlite")]
		unsafe static extern SqliteCommand.SqliteError sqlite_finalize(IntPtr pVm, [In, Out] sbyte **pErrMsg);

		[DllImport ("sqlite")]
		unsafe static extern void sqliteFree (void *ptr);
		
	}
}
