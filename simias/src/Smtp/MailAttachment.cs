/***********************************************************************
 *  Simias.Mail.MailAttachment.cs
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
 *         Lawrence Pit (loz@cable.a2000.nl)
 *         Per Arneng (pt99par@student.bth.se)
 * 
 ***********************************************************************/
using System;

namespace Simias.Mail
{
    public class MailAttachment
    {
        private string filename;
        private MailEncoding encoding;

        public MailAttachment (string filename) : 
        this (filename, MailEncoding.Base64) 
        {
        }

        public MailAttachment (string filename, MailEncoding encoding) 
        {
            this.filename = filename;
            this.encoding = encoding;
            try
            {
                System.IO.File.OpenRead (filename).Close ();
            }
            catch(Exception e)
            {
                throw new Exception("Cannot find file: '" + 
                        filename + "'." );
            }
        }

            // Properties
        public string Filename 
        {
            get { return filename;} 
        }

        public MailEncoding Encoding 
        {
            get { return encoding;} 
        }       

    }

} //namespace System.Web.Mail
