/***********************************************************************
 *  Simias.Mail.MailAddressCollection.cs
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
 *  Author: Per Arneng <pt99par@student.bth.se>
 * 
 ***********************************************************************/

using System;
using System.Text;
using System.Collections;

namespace Simias.Mail {

    // represents a collection of MailAddress objects
    internal class MailAddressCollection : IEnumerable
    {

        protected ArrayList data = new ArrayList();

        public MailAddress this[ int index ] {
            get { return this.Get( index );}
        }

        public int Count { get { return data.Count;}}

        public void Add( MailAddress addr )
        {
            data.Add( addr );
        }
        public MailAddress Get( int index )
        {
            return(MailAddress)data[ index ];
        }

        public IEnumerator GetEnumerator()
        {
            return data.GetEnumerator();
        }


        public override string ToString()
        {

            StringBuilder builder = new StringBuilder();
            for( int i = 0; i <data.Count ; i++ )
            {
                MailAddress addr = this.Get( i );

                builder.Append( addr );

                if( i != ( data.Count - 1 ) ) builder.Append( ",\r\n  " );
            }

            return builder.ToString(); 
        }

        public static MailAddressCollection Parse( string str )
        {

            if( str == null ) throw new ArgumentNullException("Null is not allowed as an address string");

            MailAddressCollection list = new MailAddressCollection();

            string[] parts = str.Split( new char[]{ ',' , ';'} );

            foreach( string part in parts )
            {
                MailAddress add = MailAddress.Parse (part);
                if(add == null)
                    continue;

                list.Add (add);
            }

            return list;
        }

    }

}
