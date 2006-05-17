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

#ifndef _IFOLDER3_WAIT_DIALOG_H_
#define _IFOLDER3_WAIT_DIALOG_H_

#include <stdbool.h>
#include <ifolder-client.h>

G_BEGIN_DECLS

#define IFA_WAIT_DIALOG_TYPE            (ifa_wait_dialog_get_type ())
#define IFA_WAIT_DIALOG(object)         (G_TYPE_CHECK_INSTANCE_CAST ((object), IFA_WAIT_DIALOG_TYPE, IFAWaitDialog))
#define IFA_WAIT_DIALOG_CLASS(klass)    (G_TYPE_CHECK_CLASS_CAST ((klass), IFA_WAIT_DIALOG_TYPE, IFAWaitDialogClass))
#define IFA_IS_WAIT_DIALOG(object)      (G_TYPE_CHECK_INSTANCE_TYPE ((object), IFA_WAIT_DIALOG_TYPE))
#define IFA_IS_WAIT_DIALOG_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((klass), IFA_WAIT_DIALOG_TYPE))
#define IFA_WAIT_DIALOG_GET_CLASS(obj)  (G_TYPE_INSTANCE_GET_CLASS ((obj), IFA_WAIT_DIALOG_TYPE, IFAWaitDialogClass))

typedef struct _IFAWaitDialog        IFAWaitDialog;
typedef struct _IFAWaitDialogClass   IFAWaitDialogClass;

struct _IFAWaitDialog 
{
  GtkDialog parent_instance;

  /*< private >*/
  gpointer private_data;
};

struct _IFAWaitDialogClass 
{
  GtkDialogClass parent_class;
};

typedef enum
{
	IFA_WAIT_DIALOG_NONE,
	IFA_WAIT_DIALOG_CANCEL
} IFAWaitDialogButtonSet;

GType                  ifa_wait_dialog_get_type				(void) G_GNUC_CONST;
GtkWidget             *ifa_wait_dialog_new					(GtkWindow *parent, GdkPixbuf *icon_pixbuf, IFAWaitDialogButtonSet buttonSet, const gchar *title, const gchar *statement, const gchar *secondaryStatement);

G_END_DECLS

#endif /*_IFOLDER3_WAIT_DIALOG_H_*/
