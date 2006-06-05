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

#ifndef _IFOLDER3_IFOLDER_PROP_DIALOG_H_
#define _IFOLDER3_IFOLDER_PROP_DIALOG_H_

#include <ifolder-client.h>

G_BEGIN_DECLS

#define IFA_IFOLDER_PROP_DIALOG_TYPE            (ifa_ifolder_prop_dialog_get_type ())
#define IFA_IFOLDER_PROP_DIALOG(object)         (G_TYPE_CHECK_INSTANCE_CAST ((object), IFA_IFOLDER_PROP_DIALOG_TYPE, IFAiFolderPropDialog))
#define IFA_IFOLDER_PROP_DIALOG_CLASS(klass)    (G_TYPE_CHECK_CLASS_CAST ((klass), IFA_IFOLDER_PROP_DIALOG_TYPE, IFAiFolderPropDialogClass))
#define IFA_IS_IFOLDER_PROP_DIALOG(object)      (G_TYPE_CHECK_INSTANCE_TYPE ((object), IFA_IFOLDER_PROP_DIALOG_TYPE))
#define IFA_IS_IFOLDER_PROP_DIALOG_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((klass), IFA_IFOLDER_PROP_DIALOG_TYPE))
#define IFA_IFOLDER_PROP_DIALOG_GET_CLASS(obj)  (G_TYPE_INSTANCE_GET_CLASS ((obj), IFA_IFOLDER_PROP_DIALOG_TYPE, IFAiFolderPropDialogClass))

typedef struct _IFAiFolderPropDialog        IFAiFolderPropDialog;
typedef struct _IFAiFolderPropDialogClass   IFAiFolderPropDialogClass;

struct _IFAiFolderPropDialog 
{
  GtkDialog parent_instance;

  /*< private >*/
  gpointer private_data;
};

struct _IFAiFolderPropDialogClass 
{
  GtkDialogClass parent_class;
};

GType			ifa_ifolder_prop_dialog_get_type (void) G_GNUC_CONST;
GtkWidget		*ifa_ifolder_prop_dialog_new (GtkWindow *parent, iFolder *ifolder);

G_END_DECLS

#endif /*_IFOLDER3_IFOLDER_PROP_DIALOG_H_*/
