/***********************************************************************
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/
#ifndef IFOLDER_GTK_H
#define IFOLDER_GTK_H 1

#include <gtk/gtk.h>

#define IFOLDER_IMAGE_BIG_IFOLDER	"/opt/novell/ifolder3/share/ifolder3/images/ifolder48.png"

#define IFOLDER_USER_PIXBUF			"/opt/novell/ifolder3/share/ifolder3/images/ifolderuser.png"
#define IFOLDER_INVITED_USER_PIXBUF	"/opt/novell/ifolder3/share/ifolder3/images/inviteduser.png"
#define IFOLDER_OWNER_PIXBUF		"/opt/novell/ifolder3/share/ifolder3/images/currentuser.png"

GtkWidget *ifolder_general_property_page_new (const char *ifolder_id);

/**
 * This function allows the application that uses this property page to update
 * the iFolder that's currently being displayed.  You can either pass in the
 * same iFolder ID that your originally passed in, or a different one.  Every
 * affected Widget will be updated.
 */
void ifolder_general_property_page_update (GtkWidget *property_page,
										   const char *ifolder_id);

GtkWidget *ifolder_sharing_property_page_new (const char *ifolder_id);
void ifolder_sharing_property_page_update (GtkWidget *property_page,
										   const char *ifolder_id);

#endif
