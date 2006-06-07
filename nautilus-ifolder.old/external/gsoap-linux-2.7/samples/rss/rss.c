/*

RSS 0.91 and 2.0

Retrieve RSS feeds.

Usage: rss URL
Prints RSS content.

--------------------------------------------------------------------------------
gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.
This software is released under one of the following two licenses:
GPL or Genivia's license for commercial use.
--------------------------------------------------------------------------------
GPL license.

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place, Suite 330, Boston, MA 02111-1307 USA

Author contact information:
engelen@genivia.com / engelen@acm.org
--------------------------------------------------------------------------------
A commercial use license is available from Genivia, Inc., contact@genivia.com
--------------------------------------------------------------------------------
*/

#include "soapH.h"

#define STR(s) (s?s:"")

#define DEFAULT_ENDPOINT "http://www.xmethods.net/interfaces/rss"

#define MAX_REDIRECTS 10

int main(int argc, char **argv)
{ struct soap *soap = soap_new();
  const char *endpoint;
  struct rss *rss;
  int i;
  /* RSS 0.91 has no namespaces */
  soap->namespaces = NULL;
  /* and no encoding style */
  soap->encodingStyle = NULL;
  /* get URL of RSS feed */
  if (argc >= 2)
    endpoint = argv[1];
  else
    endpoint = DEFAULT_ENDPOINT;
  /* connect and parse HTTP header (max HTTP redirects) */
  for (i = 0; i < MAX_REDIRECTS; i++)
  { /* HTTP GET and parse HTTP header */
    if (soap_connect_command(soap, SOAP_GET, endpoint, NULL)
     || soap_begin_recv(soap))
    { if ((soap->error >= 301 && soap->error <= 303) || soap->error == 307)
        endpoint = soap_strdup(soap, soap->endpoint); /* HTTP redirects */
      else
      { soap_print_fault(soap, stderr);
        exit(soap->error);
      }
    }
    else
      break;
  }
  /* parse RSS */
  rss = soap_get_rss(soap, NULL, "rss", NULL);
  /* close connection */
  soap_end_recv(soap);
  soap_closesock(soap);
  if (rss)
  { if (!strcmp(rss->version, "0.91") || !strcmp(rss->version, "2.0"))
    { printf("Title: %s\n", STR(rss->channel.title));
      printf("Link: %s\n", STR(rss->channel.link));
      printf("Language: %s\n", STR(rss->channel.language));
      printf("Description: %s\n", STR(rss->channel.description));
      if (rss->channel.image)
      { printf("Image title: %s\n", STR(rss->channel.image->title));
        printf("Image url: %s\n", STR(rss->channel.image->url));
        printf("Image link: %s\n", STR(rss->channel.image->link));
        printf("Image dimensions: %d x %d\n", rss->channel.image->width, rss->channel.image->height);
        printf("Image description: %s\n", STR(rss->channel.image->description));
      }
      for (i = 0; i < rss->channel.__size; i++)
      { printf("\n%3d Title: %s\n", i+1, STR(rss->channel.item[i].title));
        printf("    Link: %s\n", STR(rss->channel.item[i].link));
        printf("    Description: %s\n", STR(rss->channel.item[i].description));
      }
      printf("\nCopyright: %s\n", STR(rss->channel.copyright));
    }
    else
      fprintf(stderr, "RSS 0.91 or 2.0 content expected\n");
  }
  else
    soap_print_fault(soap, stderr);
  soap_end(soap);
  soap_done(soap);
  soap_free(soap);
  return 0;
}

/* Don't need a namespace table. We put an empty one here to avoid link errors */
struct Namespace namespaces[] = { {NULL, NULL} };
