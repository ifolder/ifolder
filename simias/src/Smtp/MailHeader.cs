/***********************************************************************
 *  Simias.Mail.MailHeader.cs
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
using System.Collections;
using System.Collections.Specialized;

namespace Simias.Mail {


    // This class represents the header of a mail with
    // all the header fields.
    internal class MailHeader
    {

        protected NameValueCollection data = new NameValueCollection();

        public string To {
            get { return data[ "To" ];}
            set { data[ "To" ] = value;}
        }

        public string From {
            get { return data[ "From" ];}
            set { data[ "From" ] = value;}
        }

        public string Cc {
            get { return data[ "Cc" ];}
            set { data[ "Cc" ] = value;}
        }

        public string Bcc {
            get { return data[ "Bcc" ];}
            set { data[ "Bcc" ] = value;}
        }

        public string Subject {
            get { return data[ "Subject" ];}
            set { data[ "Subject" ] = value;}
        }

        public string Importance {
            get { return data[ "Importance" ];}
            set { data[ "Importance" ] = value;}
        }

        public string Priority {
            get { return data[ "Priority" ];}
            set { data[ "Priority" ] = value;}
        }

        public string MimeVersion {
            get { return data[ "Mime-Version" ];}
            set { data[ "Mime-Version" ] = value;}
        }

        public string ContentType {
            get { return data[ "Content-Type" ];}
            set { data[ "Content-Type" ] = value;}
        } 

        public string ContentTransferEncoding{
            get { return data[ "Content-Transfer-Encoding" ];}
            set { data[ "Content-Transfer-Encoding" ] = value;}
        } 

        public string ContentDisposition {
            get { return data[ "Content-Disposition" ];}
            set { data[ "Content-Disposition" ] = value;}
        } 

        public string ContentBase {
            get { return data[ "Content-Base" ];}
            set { data[ "Content-Base" ] = value;}
        }

        public string ContentLocation {
            get { return data[ "Content-Location" ];}
            set { data[ "Content-Location" ] = value;}
        }   


        public NameValueCollection Data {
            get { return data;} 
        }

    }

}
