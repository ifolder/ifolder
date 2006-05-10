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
       static public void ProcessChannel(HttpContext ctx, Slog slog)
       {
        	Console.WriteLine("ProcessChannel entered");
        	
			ctx.Response.Write("<title>");
			ctx.Response.Write(slog.Title);
			ctx.Response.Write("</title>");
				
			if (slog.Description != "")
			{			
				ctx.Response.Write("<description>");
				ctx.Response.Write(slog.Description);
				ctx.Response.Write("</description>");
			}

			if (slog.Link != "")
			{			
				ctx.Response.Write("<link>");
				ctx.Response.Write(slog.Link);
				ctx.Response.Write("</link>");
			}

			if (slog.Language != "")
			{			
				ctx.Response.Write("<language>");
				ctx.Response.Write(slog.Language);
				ctx.Response.Write("</language>");
			}

			if (slog.Copyright != "")
			{			
				ctx.Response.Write("<copyright>");
				ctx.Response.Write(slog.Copyright);
				ctx.Response.Write("</copyright>");
			}

			if (slog.ManagingEditor != "")
			{			
				ctx.Response.Write("<managingEditor>");
				ctx.Response.Write(slog.ManagingEditor);
				ctx.Response.Write("</managingEditor>");
			}
			
			if (slog.Webmaster != "")
			{			
				ctx.Response.Write("<webmaster>");
				ctx.Response.Write(slog.Webmaster);
				ctx.Response.Write("</webmaster>");
			}
			
			if (slog.PublishDate != "")
			{			
				ctx.Response.Write("<pubDate>");
				ctx.Response.Write(slog.PublishDate);
				ctx.Response.Write("</pubDate>");
			}
			
			if (slog.LastBuildDate != "")
			{			
				ctx.Response.Write("<lastBuildDate>");
				ctx.Response.Write(slog.LastBuildDate);
				ctx.Response.Write("</lastBuildDate>");
			}
			
			if (slog.Generator != "")
			{			
				ctx.Response.Write("<generator>");
				ctx.Response.Write(slog.Generator);
				ctx.Response.Write("</generator>");
			}
													
			if (slog.Cloud != "")
			{			
				ctx.Response.Write("<cloud>");
				ctx.Response.Write(slog.Cloud);
				ctx.Response.Write("</cloud>");
			}

			if (slog.Ttl != "")
			{			
				ctx.Response.Write("<ttl>");
				ctx.Response.Write(slog.Ttl);
				ctx.Response.Write("</ttl>");
			}
																																																																																																																																																																						
			if (slog.Rating != "")
			{			
				ctx.Response.Write("<rating>");
				ctx.Response.Write(slog.Rating);
				ctx.Response.Write("</rating>");
			}
			
        	Console.WriteLine("ProcessChannel exit");
 		}       	
    
       static public void ProcessItem(HttpContext ctx, SlogEntry entry)
       {
        	Console.WriteLine("ProcessItem entered");
        	
        	// An RSS item must have at least a Description or 
        	// a title
        	if (entry.Description == "" && entry.Title == "")
        	{
        		return;
        	}
        	
			ctx.Response.Write("<item>");
					
			if (entry.Title != "")
			{
				ctx.Response.Write("<title>");
				ctx.Response.Write(entry.Title);
				ctx.Response.Write("</title>");
			}
			
			if (entry.Name != "")
			{		
				ctx.Response.Write("<pubDate>");
				//ctx.Response.Write(entry.PublishDate);
				ctx.Response.Write(entry.Name);
				ctx.Response.Write("</pubDate>");
			}
			
			if (entry.Description != "")
			{		
				ctx.Response.Write("<description>");
				ctx.Response.Write(entry.Description);
				ctx.Response.Write("</description>");
			}
			
			try
			{
				if (entry.Author != "")
				{
					ctx.Response.Write("<author>");
					ctx.Response.Write(entry.Author);
					ctx.Response.Write("</author>");
				}
			}
			catch{}

			try
			{
				// FIXME - category needs to be exposed as a 
				// multi-valued property
				if (entry.Category != "")
				{
					ctx.Response.Write("<category>");
					ctx.Response.Write(entry.Category);
					ctx.Response.Write("</category>");
				}
			}
			catch{}

			try
			{
				if (entry.Comments != "")
				{
					ctx.Response.Write("<comments>");
					ctx.Response.Write(entry.Comments);
					ctx.Response.Write("</comments>");
				}
			}
			catch{}

			try
			{
				ctx.Response.Write("<guid>");
				//ctx.Response.Write(ctx.Request.Path);
				ctx.Response.Write(ctx.Request.Url);
				ctx.Response.Write("?node=");
				ctx.Response.Write(entry.ID);
				ctx.Response.Write("</guid>");
			}
			catch{}
												
			ctx.Response.Write("</item>");
        	Console.WriteLine("ProcessItem exit");
       }	
    
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
				SlogRss.ProcessChannel(context, slog);
				
				foreach(SlogEntry slogEntry in slog)
				{
					SlogRss.ProcessItem(context, slogEntry);
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

