//<%@ WebService Language="c#" %>

using System.Web;

namespace Novell.Collaboration
{
    public class SlogRss : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            //context.Response.Write("Hello World!");
            
            // Kewl now we can slog away
            context.Response.Write("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
			context.Response.Write("<rss version=\"2.0\">");
			context.Response.Write("</rss>");
			
			context.Response.Write("<channel>");
			context.Response.Write("<title>iFolderlogue</title>");
			context.Response.Write("<description>The voices of iFolder development</description>");
			context.Response.Write("<item>");
			context.Response.Write("<title>");
			context.Response.Write("Brady Anderson: Simias III");
			context.Response.Write("</title>");
			
			context.Response.Write("<description>");
			context.Response.Write("Today Mike Lasky released version 3 of the Simias architecture document");
			context.Response.Write("</description>");
			
			context.Response.Write("</item>");
			 			
			context.Response.Write("</channel>");
			context.Response.Write("</rss>");
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

