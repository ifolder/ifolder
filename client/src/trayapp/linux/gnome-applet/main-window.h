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

#ifndef _IFOLDER3_MAIN_WINDOW_H_
#define _IFOLDER3_MAIN_WINDOW_H_

#include <ifolder-client.h>

G_BEGIN_DECLS

#define IFA_MAIN_WINDOW_TYPE            (ifa_main_window_get_type ())
#define IFA_MAIN_WINDOW(object)         (G_TYPE_CHECK_INSTANCE_CAST ((object), IFA_MAIN_WINDOW_TYPE, IFAMainWindow))
#define IFA_MAIN_WINDOW_CLASS(klass)    (G_TYPE_CHECK_CLASS_CAST ((klass), IFA_MAIN_WINDOW_TYPE, IFAMainWindowClass))
#define IFA_IS_MAIN_WINDOW(object)      (G_TYPE_CHECK_INSTANCE_TYPE ((object), IFA_MAIN_WINDOW_TYPE))
#define IFA_IS_MAIN_WINDOW_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((klass), IFA_MAIN_WINDOW_TYPE))
#define IFA_MAIN_WINDOW_GET_CLASS(obj)  (G_TYPE_INSTANCE_GET_CLASS ((obj), IFA_MAIN_WINDOW_TYPE, IFAMainWindowClass))

typedef struct _IFAMainWindow        IFAMainWindow;
typedef struct _IFAMainWindowClass   IFAMainWindowClass;

struct _IFAMainWindow 
{
  GtkDialog parent_instance;

  /*< private >*/
  gpointer private_data;
};

struct _IFAMainWindowClass 
{
  GtkDialogClass parent_class;
};

GType			ifa_main_window_get_type (void) G_GNUC_CONST;
IFAMainWindow	*ifa_get_main_window();
void			ifa_show_main_window();
void			ifa_hide_main_window();



G_END_DECLS

#endif /*_IFOLDER3_MAIN_WINDOW_H_*/
