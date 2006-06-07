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

struct channel
{ char *title;
  char *link;
  char *language;
  char *copyright;
  char *description;
  struct image *image;
  int __size;
  struct item *item;
};

struct item
{ char *title;
  char *link;
  char *description;
};

struct image
{ char *title;
  char *url;
  char *link;
  int width	0:1 = 0;	// optional, default value = 0
  int height	0:1 = 0;	// optional, default value = 0
  char *description;
};

struct rss
{ @char *version = "0.91";
  struct channel channel;
};
