/***********************************************************************
 *  Simias.Mail.SmtpResponse.cs
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
 *  Author:
 *         Per Arneng (pt99par@student.bth.se)
 * 
 ***********************************************************************/

using System;

namespace Simias.Mail {


    /// this class represents the response from the smtp server
    internal class SmtpResponse
    {

        private string rawResponse;
        private int statusCode;
        private string[] parts;

    /// use the Parse method to create instances
        protected SmtpResponse()
        {
        }

    /// the smtp status code FIXME: change to Enumeration?
        public int StatusCode {
            get { return statusCode;}
            set { statusCode = value;}
        }

    /// the response as it was recieved
        public string RawResponse {
            get { return rawResponse;}
            set { rawResponse = value;}
        }

    /// the response as parts where ; was used as delimiter
        public string[] Parts {
            get { return parts;}
            set { parts = value;}
        }

    /// parses a new response object from a response string
        public static SmtpResponse Parse( string line )
        {
            SmtpResponse response = new SmtpResponse();

            if( line.Length < 4 )
                throw new SmtpException( "Response is to short " + 
                        line.Length + ".");

            if( ( line[ 3 ] != ' ' ) && ( line[ 3 ] != '-' ) )
                throw new SmtpException( "Response format is wrong.(" + 
                        line + ")" );

        // parse the response code
            response.StatusCode = Int32.Parse( line.Substring( 0 , 3 ) );

        // set the rawsponse
            response.RawResponse = line;

        // set the response parts
            response.Parts = line.Substring( 0 , 3 ).Split( ';' );

            return response;
        }
    }

}
