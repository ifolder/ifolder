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
using System.Runtime.Serialization;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Collection Store Provider exception.
	/// </summary>
	[Serializable]
	public class CSPException : System.Exception
	{
		string errorString = "CSPError";
		Provider.Error error;


		private CSPException(SerializationInfo s, StreamingContext c) :
			base(s, c)
		{
			error = (Provider.Error)s.GetInt32(errorString);
		}
		/// <summary>
		/// Constructor to create a CSPException.
		/// </summary>
		/// <param name="message">The string describing the exception.</param>
		/// <param name="error">The int representing the exception.</param>
		public CSPException(string message, Provider.Error error) :
			base(message)
		{
			this.error = error;
		}

		/// <summary>
		/// Overriden method to allow this object to be deserialized.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="c"></param>
		public override void GetObjectData(SerializationInfo s, StreamingContext c)
		{
			base.GetObjectData (s, c);
			s.AddValue(errorString, error);	
		}


		/// <summary>
		/// Overriden property to return the string message for the exception.
		/// </summary>
		public override string Message
		{
			get {return base.Message + " Error = " + string.Format("{0:X8}", (int)error) + this.StackTrace;}
		}

	}
}
