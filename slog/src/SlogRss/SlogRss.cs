/************************************************************************
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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Web;
using Novell.Collaboration;

namespace Novell.Collaboration
{
    public class SlogRss : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
        	Console.WriteLine("ProcessRequest entered");
        	
        	bool	first = true;
        	
			SlogManager slogManager = SlogManager.Connect();
 			foreach(Slog slog in slogManager)
			{
				if (first == true)
				{	
		            context.Response.ContentType = "text/xml";
		            context.Response.Write("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
					context.Response.Write("<rss version=\"2.0\">");
					first = false;
				}
				
				context.Response.Write("<channel>");
				
				
				context.Response.Write("<title>");
				context.Response.Write(slog.Name);
				context.Response.Write("</title>");

								
				context.Response.Write("<description>");
				context.Response.Write("Ah, we need to add a description property to a slog");
				context.Response.Write("</description>");
				
				foreach(SlogEntry slogEntry in slog)
				{
					context.Response.Write("<item>");
					
					context.Response.Write("<title>");
					context.Response.Write(slogEntry.Title);
					context.Response.Write("</title>");
					
					context.Response.Write("<pubDate>");
					context.Response.Write(slogEntry.Name);
					context.Response.Write("</pubDate>");
					
					context.Response.Write("<description>");
					context.Response.Write(slogEntry.Description);
					context.Response.Write("</description>");
			
					context.Response.Write("</item>");
				}
				
				context.Response.Write("</channel>");
			}
	
			if (first == false)
			{
				context.Response.Write("</rss>");
			}
			else
			{
				// FIXME - set an error
			}
        }

        public bool IsReusable
        {
        	get
        	{
                return true;
            }
        } 
    }
}

