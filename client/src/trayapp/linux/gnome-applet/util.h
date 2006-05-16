/***********************************************************************
 *  iFolder 3 Applet -- Main applet for the iFolder 3 Client
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author(s): Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

#ifndef _IFOLDER3_UTIL_H_
#define _IFOLDER3_UTIL_H_

#include <gtk/gtk.h>

/**
 * Help files
 */
#define IFA_HELP_MAIN_PAGE		"index.html"		/* FIXME: I don't think the main page is 'index.html' */
#define IFA_HELP_PREFERENCES_PAGE	"preferences.html"
#define IFA_HELP_ACCOUNTS_PAGE		"accounts.html"

/**
 * Default spacing/width/etc.
 */
#define IFA_DEFAULT_SECTION_SPACING		20
#define IFA_DEFAULT_SECTION_TITLE_SPACING	5
#define IFA_DEFAULT_BORDER_WIDTH		10

G_BEGIN_DECLS

void ifa_show_help(gchar *page_name);

GdkPixbuf *ifolder_util_load_pixbuf(const gchar *image_name);

G_END_DECLS

#endif /*_IFOLDER3_UTIL_H_*/
