// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteParameterCollection.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Data;
using System.Collections;

namespace Simias.Storage.Provider.Sqlite
{
	/// <summary>
	/// 
	/// </summary>
        public class SqliteParameterCollection : IDataParameterCollection,
                IList
        {
                ArrayList numeric_param_list = new ArrayList ();
                Hashtable named_param_hash = new Hashtable ();

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
                public IEnumerator GetEnumerator ()
                {
                        throw new NotImplementedException ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="parameterName"></param>
                public void RemoveAt (string parameterName)
                {
                        if (!named_param_hash.Contains (parameterName))
                                throw new ApplicationException ("Parameter " + parameterName + " not found");

                        numeric_param_list.RemoveAt ((int) named_param_hash[parameterName]);
                        named_param_hash.Remove (parameterName);

                        RecreateNamedHash ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="param"></param>
                public void RemoveAt (SqliteParameter param)
                {
                        RemoveAt (param.ParameterName);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="index"></param>
                public void RemoveAt (int index)
                {
                        RemoveAt (((SqliteParameter) numeric_param_list[index]).ParameterName);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="o"></param>
			/// <returns></returns>
                int IList.IndexOf (object o)
                {
                        return IndexOf ((SqliteParameter) o);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="parameterName"></param>
			/// <returns></returns>
                public int IndexOf (string parameterName)
                {
                        return (int) named_param_hash[parameterName];
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="param"></param>
			/// <returns></returns>
                public int IndexOf (SqliteParameter param)
                {
                        return IndexOf (param.ParameterName);
                }

                bool IList.Contains (object value)
                {
                        return Contains ((SqliteParameter) value);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="parameterName"></param>
			/// <returns></returns>
                public bool Contains (string parameterName)
                {
                        return named_param_hash.Contains (parameterName);
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="param"></param>
			/// <returns></returns>
                public bool Contains (SqliteParameter param)
                {
                        return Contains (param.ParameterName);
                }

                object IList.this[int index] {
                        get {
                                return this[index];
                        }
                        set {
                                CheckSqliteParam (value);
                                this[index] = (SqliteParameter) value;
                        }
                }

                object IDataParameterCollection.this[string parameterName] {
                        get {
                                return this[parameterName];
                        }
                        set {
                                CheckSqliteParam (value);
                                this[parameterName] = (SqliteParameter) value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public SqliteParameter this[string parameterName] {
                        get {
                                return this[(int) named_param_hash[parameterName]];
                        }
                        set {
                                if (this.Contains (parameterName))
                                        numeric_param_list[(int) named_param_hash[parameterName]] = value;
                                else          // uhm, do we add it if it doesn't exist? what does ms do?
                                        Add (value);
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public SqliteParameter this[int parameterIndex] {
                        get {
                                return (SqliteParameter) numeric_param_list[parameterIndex];
                        }
                        set {
                                numeric_param_list[parameterIndex] = value;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
                public int Add (object value)
                {
                        CheckSqliteParam (value);
                        SqliteParameter sqlp = (SqliteParameter) value;
                        if (named_param_hash.Contains (sqlp.ParameterName))
                                throw new DuplicateNameException ("Parameter collection already contains given value.");

                        named_param_hash[value] = numeric_param_list.Add (value);

                        return (int) named_param_hash[value];
                }

                // IList

			/// <summary>
			/// 
			/// </summary>
			/// <param name="param"></param>
			/// <returns></returns>
                public SqliteParameter Add (SqliteParameter param)
                {
                        Add ((object)param);
                        return param;
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name"></param>
			/// <param name="value"></param>
			/// <returns></returns>
                public SqliteParameter Add (string name, object value)
                {
                        return Add (new SqliteParameter (name, value));
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="name"></param>
			/// <param name="type"></param>
			/// <returns></returns>
                public SqliteParameter Add (string name, DbType type)
                {
                        return Add (new SqliteParameter (name, type));
                }

			/// <summary>
			/// 
			/// </summary>
                public bool IsFixedSize {
                        get {
                                return false;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public bool IsReadOnly {
                        get {
                                return false;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public void Clear ()
                {
                        numeric_param_list.Clear ();
                        named_param_hash.Clear ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="index"></param>
			/// <param name="value"></param>
                public void Insert (int index, object value)
                {
                        CheckSqliteParam (value);
                        if (numeric_param_list.Count == index) {
                                Add (value);
                                return;
                        }

                        numeric_param_list.Insert (index, value);
                        RecreateNamedHash ();
                }

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
                public void Remove (object value)
                {
                        CheckSqliteParam (value);
                        RemoveAt ((SqliteParameter) value);
                }

                // ICollection

            /// <summary>
            /// 
            /// </summary>
    			public int Count {
                        get {
                                return numeric_param_list.Count;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public bool IsSynchronized {
                        get {
                                return false;
                        }
                }

			/// <summary>
			/// 
			/// </summary>
                public object SyncRoot {
                        get {
                                return null;
                        }
                }


			/// <summary>
			/// 
			/// </summary>
			/// <param name="array"></param>
			/// <param name="index"></param>
				public void CopyTo (Array array, int index)
                {
                        throw new NotImplementedException ();
                }

                private void CheckSqliteParam (object value)
                {
                        if (!(value is SqliteParameter))
                                throw new InvalidCastException ("Can only use SqliteParameter objects");
                }

                private void RecreateNamedHash ()
                {
                        for (int i = 0; i < numeric_param_list.Count; i++) {
                                named_param_hash[((SqliteParameter) numeric_param_list[i]).ParameterName] = i;
                        }
                }
        }
}
