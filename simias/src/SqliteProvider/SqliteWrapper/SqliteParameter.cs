// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteParameter.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Data;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// 
	/// </summary>
        public class SqliteParameter : IDbDataParameter
        {
                string name;
                DbType type;
                string source_column;
                ParameterDirection direction;
                DataRowVersion row_version;
                object param_value;
                byte precision;
                byte scale;
                int size;

			/// <summary>
			/// 
			/// </summary>
                public SqliteParameter ()
                {
                        type = DbType.String;
                        direction = ParameterDirection.Input;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name_in"></param>
			/// <param name="type_in"></param>
                public SqliteParameter (string name_in, DbType type_in)
                {
                        name = name_in;
                        type = type_in;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name_in"></param>
			/// <param name="param_value_in"></param>
                public SqliteParameter (string name_in, object param_value_in)
                {
                        name = name_in;
                        type = DbType.String;
                        param_value = param_value_in;
                        direction = ParameterDirection.Input;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name_in"></param>
			/// <param name="type_in"></param>
			/// <param name="size_in"></param>
                public SqliteParameter (string name_in, DbType type_in, int size_in)
                        : this (name_in, type_in)
                {
                        size = size_in;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name_in"></param>
			/// <param name="type_in"></param>
			/// <param name="size"></param>
			/// <param name="src_column"></param>
                public SqliteParameter (string name_in, DbType type_in, int size, string src_column)
                        : this (name_in ,type_in)
                {
                        source_column = src_column;
                }

			/// <summary>
			/// 
			/// </summary>
                public DbType DbType {
                        get {
                                return type;
                        }
                        set {
                                type = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public ParameterDirection Direction {
                        get {
                                return direction;
                        }
                        set {
                                direction = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public bool IsNullable {
                        get {
                                // uhh..
                                return true;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public string ParameterName {
                        get {
                                return name;
                        }
                        set {
                                name = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public string SourceColumn {
                        get {
                                return source_column;
                        }
                        set {
                                source_column = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public DataRowVersion SourceVersion {
                        get {
                                return row_version;
                        }
                        set {
                                row_version = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public object Value {
                        get {
                                return param_value;
                        }
                        set {
                                param_value = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public byte Precision {
                        get {
                                return precision;
                        }
                        set {
                                precision = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public byte Scale {
                        get {
                                return scale;
                        }
                        set {
                                scale = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public int Size {
                        get {
                                return size;
                        }
                        set {
                                size = value;
                        }
                }
        }
}
