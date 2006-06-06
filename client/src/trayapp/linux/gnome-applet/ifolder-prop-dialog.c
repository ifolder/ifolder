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

#include <stdlib.h>
#include <string.h>
#include <gtk/gtk.h>
#include <gdk/gdkkeysyms.h>

//#if !GTK_CHECK_VERSION(2,6,0)
//#include <gnome.h>
//#endif
#include "util.h"

#include "ifolder-prop-dialog.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFAiFolderPropDialogPrivate IFAiFolderPropDialogPrivate;
struct _IFAiFolderPropDialogPrivate 
{
	GtkWindow 		*parentWindow;

	gboolean		realized;
	gboolean		controlKeyPressed;
	
	iFolder			*ifolder;
	
	GtkWidget		*notebook;
	
	/**
	 * General Page
	 */
	GtkWidget		*generalPage;
	GtkWidget		*basicTable;
	GtkWidget		*nameLabel;
	GtkWidget		*ownerLabel;
	GtkWidget		*locationLabel;
	GtkWidget		*accountLabel;
	
	GtkWidget		*usedValue;
	GtkWidget		*diskTable;
	GtkWidget		*limitLabel;
	GtkWidget		*limitCheckButton;
	GtkWidget		*limitValue;
	GtkWidget		*limitEntry;
	GtkWidget		*limitUnit;
	GtkWidget		*availLabel;
	GtkWidget		*availValue;
	GtkWidget		*availUnit;
	
	GtkWidget		*diskUsageBar;
	GtkWidget		*diskUsageFrame;
	GtkWidget		*diskUsageFullLabel;
	GtkWidget		*diskUsageEmptyLabel;
	
	GtkWidget		*lastSuccessfulSync;
	GtkWidget		*ffSyncValue;
	GtkWidget		*syncIntervalValue;
	
	GtkWidget		*syncNowButton;
	
	/**
	 * Sharing Page
	 */
	GtkWidget		*sharingPage;
	
	/**
	 * History Page
	 */
	GtkWidget		*historyPage;
	
	/**
	 * Unsynchronized Items Page
	 */
	GtkWidget		*unsynchronizedItemsPage;
};

static GdkPixbuf *WINDOW_ICON		= NULL;

#define IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_IFOLDER_PROP_DIALOG_TYPE, IFAiFolderPropDialogPrivate))

static void                 ifa_ifolder_prop_dialog_finalize       (GObject            *object);

G_DEFINE_TYPE (IFAiFolderPropDialog, ifa_ifolder_prop_dialog, GTK_TYPE_DIALOG)

/* Forward Declarations */

static void load_pixbufs (void);

static void                 ifa_ifolder_prop_dialog_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_ifolder_prop_dialog_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_ifolder_prop_dialog_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);

static void on_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd);
static void on_hide (GtkWidget *widget, IFAiFolderPropDialog *ipd);
static gboolean on_key_press (GtkWidget *widget, GdkEventKey *key, IFAiFolderPropDialog *ipd);

static GtkWidget * create_notebook (IFAiFolderPropDialog *ipd);
static void create_buttons (IFAiFolderPropDialog *ipd);

static GtkWidget * create_general_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_sharing_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_history_page (IFAiFolderPropDialog *ipd);
static GtkWidget * create_unsynchronized_items_page (IFAiFolderPropDialog *ipd);

static void on_general_page_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd);

static void on_sync_now_clicked (GtkButton *button, IFAiFolderPropDialog *ipd);

static void
ifa_ifolder_prop_dialog_class_init (IFAiFolderPropDialogClass *klass)
{
	GObjectClass *object_class;
	GtkWidgetClass *widget_class;
	GtkWindowClass *window_class;
	  
	g_message ("ifa_ifolder_prop_dialog_class_init()");
		
	object_class = (GObjectClass *)klass;
	widget_class = (GtkWidgetClass *)klass;
	window_class = (GtkWindowClass *)klass;
		
	object_class->set_property = ifa_ifolder_prop_dialog_set_property;
	object_class->get_property = ifa_ifolder_prop_dialog_get_property;
	
	object_class->finalize = ifa_ifolder_prop_dialog_finalize;
	
	widget_class->style_set = ifa_ifolder_prop_dialog_style_set;
	
	load_pixbufs ();
	
	g_type_class_add_private (object_class, sizeof (IFAiFolderPropDialogPrivate));
}

static void
ifa_ifolder_prop_dialog_init (IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv;
	GtkWidget *vbox;
	
	g_message ("ifa_ifolder_prop_dialog_init()");

	/* Data */
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	ipd->private_data = priv;
	
	priv->realized = FALSE;
	
//	gtk_window_set_default_size (GTK_WINDOW (ipd), 500, 400);
//	gtk_widget_set_size_request (GTK_WIDGET (ipd), 500, 400);
	
	gtk_window_set_icon (GTK_WINDOW (ipd), WINDOW_ICON);
	gtk_window_set_position (GTK_WINDOW (ipd), GTK_WIN_POS_CENTER_ON_PARENT);
	

	/* Widgets */
	gtk_widget_push_composite_child ();
	
	vbox = gtk_vbox_new (FALSE, 12);
	gtk_box_pack_start (GTK_BOX (GTK_DIALOG (ipd)->vbox), vbox, TRUE, TRUE, 0);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), IFA_DEFAULT_BORDER_WIDTH);

	gtk_box_pack_start (GTK_BOX (vbox), create_notebook (ipd), FALSE, FALSE, 0);

	gtk_widget_show (vbox);
	
	gtk_widget_pop_composite_child ();

	create_buttons (ipd);
}

static void
ifa_ifolder_prop_dialog_finalize (GObject *object)
{
	IFAiFolderPropDialog *ipd = IFA_IFOLDER_PROP_DIALOG (object);
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);

	g_debug ("IFAiFolderPropDialog::ifa_ifolder_prop_dialog_finalize()");

	
/*
	if (priv->domain_host_modified_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_host_modified_cb_id);
	if (priv->domain_logged_in_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_logged_in_cb_id);
	if (priv->domain_logged_out_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_logged_out_cb_id);
	if (priv->domain_activated_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_activated_cb_id);
	if (priv->domain_inactivated_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_inactivated_cb_id);
	if (priv->domain_new_default_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_new_default_cb_id);
	if (priv->domain_removed_cb_id > 0)
		g_signal_handler_disconnect (ifolder_client, priv->domain_removed_cb_id);
*/
	
	G_OBJECT_CLASS (ifa_ifolder_prop_dialog_parent_class)->finalize (object);
}

static void
ifa_ifolder_prop_dialog_set_property (GObject      *object, 
			       guint         prop_id, 
			       const GValue *value, 
			       GParamSpec   *pspec)
{
  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
load_pixbufs (void)
{
	if (WINDOW_ICON == NULL)
		WINDOW_ICON = ifa_load_pixbuf ("ifolder16.png");
}

static void
ifa_ifolder_prop_dialog_get_property (GObject    *object, 
			       guint       prop_id, 
			       GValue     *value, 
			       GParamSpec *pspec)
{
  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
dialog_style_set (GtkWidget *widget,
		  GtkStyle *previous_style,
		  gpointer data)
{
/*
  GtkDialog *dialog;

  dialog = GTK_DIALOG (widget);
*/

  /* Override the style properties with HIG-compliant spacings.  Ugh.
   * http://developer.gnome.org/projects/gup/hig/1.0/layout.html#layout-dialogs
   * http://developer.gnome.org/projects/gup/hig/1.0/windows.html#alert-spacing
   */

/*
  gtk_box_set_spacing (GTK_BOX (dialog->vbox), 12);

  gtk_container_set_border_width (GTK_CONTAINER (dialog->action_area), 0);
  gtk_box_set_spacing (GTK_BOX (dialog->action_area), 6);
*/
}

static void
ifa_ifolder_prop_dialog_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
/*
  if (GTK_WIDGET_CLASS (ifa_ifolder_prop_dialog_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_ifolder_prop_dialog_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
*/
}

GtkWidget *
ifa_ifolder_prop_dialog_new (GtkWindow *parent, iFolder *ifolder)
{
	IFAiFolderPropDialogPrivate *priv;
	gchar *tmpStr;
	
	g_return_val_if_fail (IFOLDER_IS_IFOLDER (ifolder), NULL);

	g_message ("ifa_ifolder_prop_dialog_new()...calling g_object_new()");
	IFAiFolderPropDialog *ipd = IFA_IFOLDER_PROP_DIALOG (g_object_new (IFA_IFOLDER_PROP_DIALOG_TYPE, NULL));
	
	g_message ("done calling g_object_new()");
	
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE(ipd);

	priv->parentWindow = parent;
	if (parent != NULL)
	{
		gtk_window_set_transient_for (GTK_WINDOW (ipd), parent);
		gtk_window_set_destroy_with_parent (GTK_WINDOW (ipd), TRUE);
		gtk_window_set_modal (GTK_WINDOW (ipd), TRUE);
	}
	
	priv->ifolder = ifolder;

	tmpStr = g_markup_printf_escaped ("%s %s", ifolder_get_name (ifolder), _("Properties"));
	gtk_window_set_title (GTK_WINDOW (ipd), tmpStr);
	g_free (tmpStr);
	
	gtk_dialog_set_has_separator (GTK_DIALOG (ipd), FALSE);
	gtk_window_set_resizable (GTK_WINDOW (ipd), FALSE);
	gtk_dialog_set_default_response (GTK_DIALOG (ipd), GTK_RESPONSE_OK);
	gtk_window_set_modal (GTK_WINDOW (ipd), TRUE);
	gtk_window_set_type_hint (GTK_WINDOW (ipd), GDK_WINDOW_TYPE_HINT_NORMAL);

/*
	g_signal_connect (ipd, "response", G_CALLBACK (on_ipd_response), ipd);
	g_signal_connect (ipd, "key-release-event", G_CALLBACK (on_key_release), ipd);
	g_signal_connect (ipd, "response", G_CALLBACK (on_response), ipd);
*/
	g_signal_connect (ipd, "realize", G_CALLBACK (on_realize), ipd);
	g_signal_connect (ipd, "hide", G_CALLBACK (on_hide), ipd);
	g_signal_connect (ipd, "key-press-event", G_CALLBACK (on_key_press), ipd);
	
	return GTK_WIDGET (ipd);
}

static void
on_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv;
	
	g_debug ("IFAiFolderPropDialog::on_realize()");
	
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	/* FIXME: Register for domain events that could affect this window */
	
	priv->realized = TRUE;
}

static void
on_hide (GtkWidget *widget, IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	g_debug ("IFAiFolderPropDialog::on_hide()");

	gtk_widget_destroy (GTK_WIDGET (ipd));
}

static gboolean
on_key_press (GtkWidget *widget, GdkEventKey *key, IFAiFolderPropDialog *ipd)
{
	switch (key->keyval)
	{
		case GDK_F1:
///			on_help (NULL, ipd);
			break;
		default:
			return FALSE;	/* propogate the event on to other key-press handlers */
			break;
	}
	
	return TRUE; /* don't propogate the event on to other key-press handlers */
}

static GtkWidget *
create_notebook (IFAiFolderPropDialog *ipd)
{
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	priv->notebook = gtk_notebook_new ();
	
	priv->generalPage = create_general_page (ipd);
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), priv->generalPage, gtk_label_new (_("General")));
	g_signal_connect (priv->generalPage, "realize", G_CALLBACK (on_general_page_realize), ipd);

	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_sharing_page (ipd), gtk_label_new (_("Sharing")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_history_page (ipd), gtk_label_new (_("History")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_unsynchronized_items_page (ipd), gtk_label_new (_("Unsynchronized Items")));
	gtk_widget_show_all (priv->notebook);
	
	return priv->notebook;
}

static GtkWidget *
create_general_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox, *basicBox, *ifolderImage, *basicLabelsBox, *label,
			  *diskSectionBox, *diskSpacerBox, *graphBox, *graphLabelBox,
			  *syncSectionBox, *syncSpacerBox, *syncWidgetBox, *syncTable,
			  *rightBox;
	GdkPixbuf *ifolderPixbuf;
	gchar *tmpStr;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 10);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), IFA_DEFAULT_BORDER_WIDTH);

	basicBox = gtk_hbox_new (FALSE, 10);
	gtk_box_pack_start (GTK_BOX (vbox), basicBox, FALSE, TRUE, 0);
	
	ifolderPixbuf = ifa_load_pixbuf ("ifolder48.png");
	ifolderImage = gtk_image_new_from_pixbuf (ifolderPixbuf);
	gtk_misc_set_alignment (GTK_MISC (ifolderImage), 0.5, 0);
	
	gtk_box_pack_start (GTK_BOX (basicBox), ifolderImage, FALSE, FALSE, 0);
	
	basicLabelsBox = gtk_vbox_new (FALSE, 5);
	gtk_box_pack_start (GTK_BOX (basicBox), basicLabelsBox, FALSE, TRUE, 0);
	
	priv->nameLabel = gtk_label_new (NULL);
	gtk_label_set_use_markup (GTK_LABEL (priv->nameLabel), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (priv->nameLabel), FALSE);
	gtk_misc_set_alignment (GTK_MISC (priv->nameLabel), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (basicLabelsBox), priv->nameLabel, FALSE, TRUE, 5);
	
	/* Create a table to hold the values */
	priv->basicTable = gtk_table_new (3, 2, FALSE);
	gtk_box_pack_start (GTK_BOX (basicLabelsBox), priv->basicTable, TRUE, TRUE, 0);
	gtk_table_set_col_spacings(GTK_TABLE(priv->basicTable), 5);
	gtk_table_set_row_spacings(GTK_TABLE(priv->basicTable), 5);
	

	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", _("Owner:"));
	label = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_label_set_use_markup (GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), label, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->ownerLabel = gtk_label_new (NULL);
	gtk_label_set_use_markup (GTK_LABEL (priv->ownerLabel), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (priv->ownerLabel), FALSE);
	gtk_misc_set_alignment (GTK_MISC (priv->ownerLabel), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), priv->ownerLabel, 1,2, 0,1,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
					 
	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", _("Location:"));
	label = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_label_set_use_markup (GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), label, 0,1, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->locationLabel = gtk_label_new (NULL);
	gtk_label_set_use_markup (GTK_LABEL (priv->locationLabel), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (priv->locationLabel), FALSE);
	gtk_misc_set_alignment (GTK_MISC (priv->locationLabel), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), priv->locationLabel, 1,2, 1,2,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", _("Account:"));
	label = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_label_set_use_markup (GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), label, 0,1, 2,3,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->accountLabel = gtk_label_new (NULL);
	gtk_label_set_use_markup (GTK_LABEL (priv->accountLabel), TRUE);
	gtk_label_set_use_underline (GTK_LABEL (priv->accountLabel), FALSE);
	gtk_misc_set_alignment (GTK_MISC (priv->accountLabel), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->basicTable), priv->accountLabel, 1,2, 2,3,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	/**
	 * Disk Space
	 */
	/* create a section box */
	diskSectionBox = gtk_vbox_new (FALSE, IFA_DEFAULT_SECTION_TITLE_SPACING);
	gtk_box_pack_start (GTK_BOX (vbox), diskSectionBox, FALSE, TRUE, 0);
	tmpStr = g_markup_printf_escaped ("<span weight=\"bold\">%s</span>", _("Disk Space on Server"));
	label = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_label_set_use_markup (GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (diskSectionBox), label, FALSE, TRUE, 0);
	
	/* hbox to provide spacing */
	diskSpacerBox = gtk_hbox_new (FALSE, 10);
	gtk_box_pack_start (GTK_BOX (diskSectionBox), diskSpacerBox, TRUE, TRUE, 0);
	label = gtk_label_new (NULL);
	gtk_box_pack_start (GTK_BOX (diskSpacerBox), label, FALSE, TRUE, 0);
	
	/* create a table to hold the values */
	priv->diskTable = gtk_table_new (3, 3, FALSE);
	gtk_box_pack_start (GTK_BOX (diskSpacerBox), priv->diskTable, TRUE, TRUE, 0);
	gtk_table_set_col_spacings(GTK_TABLE(priv->basicTable), 20);
	gtk_table_set_row_spacings(GTK_TABLE(priv->basicTable), 5);

	label = gtk_label_new (_("Used:"));
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->diskTable), label, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->usedValue = gtk_label_new (NULL);
	gtk_misc_set_alignment (GTK_MISC (priv->usedValue), 1, 0.5);
	gtk_table_attach (GTK_TABLE (priv->diskTable), priv->usedValue, 1,2, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	label = gtk_label_new (_("MB:"));
	gtk_table_attach (GTK_TABLE (priv->diskTable), label, 2,3, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->limitUnit = gtk_label_new (_("MB:"));
	gtk_table_attach (GTK_TABLE (priv->diskTable), priv->limitUnit, 2,3, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->availLabel = gtk_label_new (_("Available:"));
	gtk_misc_set_alignment (GTK_MISC (priv->availLabel), 0, 0.5);
	gtk_table_attach (GTK_TABLE (priv->diskTable), priv->availLabel, 0,1, 2,3,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->availValue = gtk_label_new (NULL);
	gtk_misc_set_alignment (GTK_MISC (priv->availValue), 1, 0.5);
	gtk_table_attach (GTK_TABLE (priv->diskTable), priv->availValue, 1,2, 2,3,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->availUnit = gtk_label_new (_("MB:"));
	gtk_table_attach (GTK_TABLE (priv->diskTable), priv->availUnit, 2,3, 2,3,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	
	priv->diskUsageFrame = gtk_frame_new (NULL);
	gtk_box_pack_start (GTK_BOX (diskSpacerBox), priv->diskUsageFrame, FALSE, TRUE, 0);
	graphBox = gtk_hbox_new (FALSE, 5);
	gtk_container_set_border_width (GTK_CONTAINER (graphBox), 5);
	gtk_container_add (GTK_CONTAINER (priv->diskUsageFrame), graphBox);
	
	priv->diskUsageBar = gtk_progress_bar_new ();
	gtk_box_pack_start (GTK_BOX (graphBox), priv->diskUsageBar, FALSE, TRUE, 0);
	
	gtk_progress_bar_set_orientation (GTK_PROGRESS_BAR (priv->diskUsageBar), GTK_PROGRESS_BOTTOM_TO_TOP);
	gtk_progress_bar_set_fraction (GTK_PROGRESS_BAR (priv->diskUsageBar), 0);
	
	graphLabelBox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (graphBox), graphLabelBox, FALSE, TRUE, 0);
	
	priv->diskUsageFullLabel = gtk_label_new (_("full"));
	gtk_misc_set_alignment (GTK_MISC (priv->diskUsageFullLabel), 0, 0);
	gtk_box_pack_start (GTK_BOX (graphLabelBox), priv->diskUsageFullLabel, TRUE, TRUE, 0);
	
	priv->diskUsageEmptyLabel = gtk_label_new (_("empty"));
	gtk_misc_set_alignment (GTK_MISC (priv->diskUsageEmptyLabel), 0, 1);
	gtk_box_pack_start (GTK_BOX (graphLabelBox), priv->diskUsageEmptyLabel, TRUE, TRUE, 0);
	
	/**
	 * Synchronization Information
	 */
	/* create a section box */
	syncSectionBox = gtk_vbox_new (FALSE, IFA_DEFAULT_SECTION_TITLE_SPACING);
	gtk_box_pack_start (GTK_BOX (vbox), syncSectionBox, FALSE, TRUE, 0);
	tmpStr = g_markup_printf_escaped ("<span weight=\"bold\">%s</span>", _("Synchronization"));
	label = gtk_label_new (tmpStr);
	g_free (tmpStr);
	gtk_label_set_use_markup (GTK_LABEL (label), TRUE);
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (syncSectionBox), label, FALSE, TRUE, 0);
	
	/* hbox to provide spacing */
	syncSpacerBox = gtk_hbox_new (FALSE, 10);
	gtk_box_pack_start (GTK_BOX (syncSectionBox), syncSpacerBox, TRUE, TRUE, 0);
	label = gtk_label_new (NULL);
	gtk_box_pack_start (GTK_BOX (syncSpacerBox), label, FALSE, TRUE, 0);
	
	syncWidgetBox = gtk_vbox_new (FALSE, 10);
	gtk_box_pack_start (GTK_BOX (syncSpacerBox), syncWidgetBox, TRUE, TRUE, 0);
	
	/* create a table to hold the values */
	syncTable = gtk_table_new (3, 2, FALSE);
	gtk_box_pack_start (GTK_BOX (syncWidgetBox), syncTable, TRUE, TRUE, 0);
	gtk_table_set_homogeneous (GTK_TABLE (syncTable), FALSE);
	gtk_table_set_col_spacings(GTK_TABLE(syncTable), 20);
	gtk_table_set_row_spacings(GTK_TABLE(syncTable), 5);
	
	label = gtk_label_new (_("Last Successful Synchronization:"));
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (syncTable), label, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->lastSuccessfulSync = gtk_label_new (_("N/A"));
	gtk_misc_set_alignment (GTK_MISC (priv->lastSuccessfulSync), 0, 0.5);
	gtk_table_attach_defaults (GTK_TABLE (syncTable), priv->lastSuccessfulSync, 1,2, 0,1);
	
	label = gtk_label_new (_("Files/Folders to Synchronize:"));
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (syncTable), label, 0,1, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->ffSyncValue = gtk_label_new ("0");
	gtk_misc_set_alignment (GTK_MISC (priv->ffSyncValue), 0, 0.5);
	gtk_table_attach_defaults (GTK_TABLE (syncTable), priv->ffSyncValue, 1,2, 1,2);
	
	label = gtk_label_new (_("Automatically Synchronizes Every:"));
	gtk_misc_set_alignment (GTK_MISC (label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (syncTable), label, 0,1, 2,3,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	priv->syncIntervalValue = gtk_label_new ("1 minute(s)");
	gtk_misc_set_alignment (GTK_MISC (priv->syncIntervalValue), 0, 0.5);
	gtk_table_attach_defaults (GTK_TABLE (syncTable), priv->syncIntervalValue, 1,2, 2,3);
	
	rightBox = gtk_hbox_new (FALSE, 10);
	gtk_box_pack_end (GTK_BOX (syncWidgetBox), rightBox, FALSE, FALSE, 0);
	
	priv->syncNowButton = gtk_button_new_with_mnemonic (_("Synchronize _Now"));
	gtk_box_pack_end (GTK_BOX (rightBox), priv->syncNowButton, FALSE, FALSE, 0);
	g_signal_connect (priv->syncNowButton, "clicked", G_CALLBACK (on_sync_now_clicked), ipd);
	
	return vbox;
}

static GtkWidget *
create_sharing_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static GtkWidget *
create_history_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static GtkWidget *
create_unsynchronized_items_page (IFAiFolderPropDialog *ipd)
{
	GtkWidget *vbox;
	IFAiFolderPropDialogPrivate *priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	vbox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (vbox), gtk_label_new ("FIXME: Implement me!"), TRUE, TRUE, 0);

	return vbox;	
}

static void
create_buttons (IFAiFolderPropDialog *ipd)
{
	gtk_dialog_add_button (GTK_DIALOG (ipd), GTK_STOCK_CLOSE, GTK_RESPONSE_OK);
}

static void
on_general_page_realize (GtkWidget *widget, IFAiFolderPropDialog *ipd)
{
	gchar *tmpStr;
	IFAiFolderPropDialogPrivate *priv;
	iFolderDomain *domain;
	
	priv = IFA_IFOLDER_PROP_DIALOG_GET_PRIVATE (ipd);
	
	tmpStr = g_markup_printf_escaped ("<span weight=\"bold\">%s</span>", ifolder_get_name (priv->ifolder));
	gtk_label_set_markup (GTK_LABEL (priv->nameLabel), tmpStr);
	g_free (tmpStr);

	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", ifolder_get_owner_full_name (priv->ifolder));
	gtk_label_set_markup (GTK_LABEL (priv->ownerLabel), tmpStr);
	g_free (tmpStr);

	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", ifolder_get_local_path (priv->ifolder));
	gtk_label_set_markup (GTK_LABEL (priv->locationLabel), tmpStr);
	g_free (tmpStr);

	domain = ifolder_get_domain (priv->ifolder);
	tmpStr = g_markup_printf_escaped ("<span size=\"small\">%s</span>", ifolder_domain_get_name (domain));
	gtk_label_set_markup (GTK_LABEL (priv->accountLabel), tmpStr);
	g_free (tmpStr);
	
	g_message ("FIXME: Read and set all the quota and synchronization time information");
}

static void
on_sync_now_clicked (GtkButton *button, IFAiFolderPropDialog *ipd)
{
	g_message ("FIXME: Implement IFAiFolderPropDialog::on_sync_now_clicked()");
}
