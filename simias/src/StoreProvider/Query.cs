/***********************************************************************
 *  Query.cs - A query class for providers.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Represents a query into a Collection Store Provider.
	/// </summary>
	[Serializable]
	public class Query
	{
		string collectionId;
		string property;
		Operator operation;
		string value;
		string type;

		/// <summary>
		/// Search Operators.
		/// </summary>
		public enum Operator
		{
			/// <summary>
			/// Equal operator.
			/// </summary>
			Equal,
			/// <summary>
			/// Not Equal operator.
			/// </summary>
			Not_Equal,
			/// <summary>
			/// Begins with operator.
			/// </summary>
			Begins,
			/// <summary>
			/// Ends with operator.
			/// </summary>
			Ends,
			/// <summary>
			/// Contain operator.
			/// </summary>
			Contains,
			/// <summary>
			/// Greater than operator.
			/// </summary>
			Greater,
			/// <summary>
			/// Less than operator.
			/// </summary>
			Less,
			/// <summary>
			/// Greater than or equal to operator.
			/// </summary>
			Greater_Equal,
			/// <summary>
			/// Less than or equal to operator.
			/// </summary>
			Less_Equal,
			/// <summary>
			/// Existence of a property.
			/// </summary>
			Exists
		};
		
		/// <summary>
		/// Construct a Query object that can be used to perform a search.
		/// </summary>
		/// <param name="collectionId">The Collection to perform the search in.</param>
		/// <param name="property">The property that contains the value.</param>
		/// <param name="op">The operator used for the match criteria.</param>
		/// <param name="value">The value to match.</param>
		/// <param name="type">The Type of the value to search for.</param>
		public Query(string collectionId, string property, Operator op, string value, string type)
		{
			this.collectionId = collectionId;
			this.property = property;
			this.operation = op;
			this.type = type;

			if (type == Provider.Syntax.Boolean)
			{
				this.value = ( String.Compare(value, "true", true) == 0) ? "1" : "0";
			}
			else
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Construct a Query that can be used to perform a global query.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		public Query(string property, Operator op, string value, string type) :
			this(null, property, op, value, type)
		{
		}

		/// <summary>
		/// Property to get the collectionId.
		/// </summary>
		public string CollectionId
		{
			get
			{
				return collectionId;
			}
		}

		/// <summary>
		/// Read only property to get the Store property name for this query.
		/// </summary>
		public string Property
		{
			get
			{
				return property;
			}
		}

		/// <summary>
		/// Read only property to get the query operation.
		/// </summary>
		public Operator Operation
		{
			get
			{
				return operation;
			}
		}

		/// <summary>
		/// Read only property that gets the string value for this query.
		/// </summary>
		public string Value
		{
			get
			{
				return value;
			}
		}

		/// <summary>
		/// Read only property that gets the value type.
		/// </summary>
		public string Type
		{
			get
			{
				return type;
			}
		}
	}
}
