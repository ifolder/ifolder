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

#include "wait-dialog.h"

/*@todo Remove this when gettext is added */
#define _

typedef struct _IFAWaitDialogPrivate IFAWaitDialogPrivate;
struct _IFAWaitDialogPrivate 
{
	IFAWaitDialogButtonSet	buttonSet;
	GtkWidget				*progressBar;
	GtkWidget				*image;
	guint					progressBarTimeout;
	gboolean				bHideCalled;
	GtkWidget				*statement;
	GtkWidget				*secondaryStatement;
};

#define IFA_WAIT_DIALOG_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_WAIT_DIALOG_TYPE, IFAWaitDialogPrivate))

static void                 ifa_wait_dialog_finalize       (GObject            *object);
static void					dialog_realized					(GtkWidget *widget, gpointer user_data);
static void                 close_cb                        (IFAWaitDialog     *wait_dialog);
static gboolean update_progress (IFAWaitDialog *wait_dialog);
static gboolean pulse_progress_bar (IFAWaitDialog *wait_dialog);
				 
G_DEFINE_TYPE (IFAWaitDialog, ifa_wait_dialog, GTK_TYPE_DIALOG)

static void                 ifa_wait_dialog_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_wait_dialog_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_wait_dialog_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);


static void
ifa_wait_dialog_class_init (IFAWaitDialogClass *klass)
{
  GObjectClass *object_class;
  GtkWidgetClass *widget_class;
  GtkDialogClass *dialog_class;
	
  object_class = (GObjectClass *)klass;
  widget_class = (GtkWidgetClass *)klass;
  dialog_class = (GtkDialogClass *)klass;
	
  object_class->set_property = ifa_wait_dialog_set_property;
  object_class->get_property = ifa_wait_dialog_get_property;

  object_class->finalize = ifa_wait_dialog_finalize;

  widget_class->style_set = ifa_wait_dialog_style_set;

  g_type_class_add_private (object_class, sizeof (IFAWaitDialogPrivate));
}

static void
ifa_wait_dialog_init (IFAWaitDialog *wait_dialog)
{
	IFAWaitDialogPrivate *priv;
	GtkWidget *vbox, *hbox, *textVBox;

	/* Data */
	priv = IFA_WAIT_DIALOG_GET_PRIVATE (wait_dialog);
	wait_dialog->private_data = priv;

	priv->buttonSet = IFA_WAIT_DIALOG_NONE;
	priv->progressBarTimeout = 0;
	priv->bHideCalled = FALSE;
	
	gtk_dialog_set_has_separator (GTK_DIALOG (wait_dialog), FALSE);
	gtk_window_set_resizable (GTK_WINDOW (wait_dialog), FALSE);
	gtk_window_set_modal (GTK_WINDOW (wait_dialog), TRUE);
	gtk_window_set_decorated (GTK_WINDOW (wait_dialog), FALSE);
//	gtk_window_set_has_frame (GTK_WINDOW (wait_dialog), FALSE);
  
	/* Widgets */
	gtk_widget_push_composite_child ();
	vbox = gtk_vbox_new (FALSE, 0);
	
	gtk_box_pack_start (GTK_BOX (GTK_DIALOG (wait_dialog)->vbox), vbox, TRUE, TRUE, 0);

	hbox = gtk_hbox_new(FALSE, 10);
	gtk_box_pack_start (GTK_BOX (vbox), hbox, true, true, 0);
	gtk_container_set_border_width (GTK_CONTAINER (hbox), 10);
	
	priv->image = gtk_image_new();
	gtk_box_pack_start (GTK_BOX (hbox), priv->image, false, false, 0);
	
	textVBox = gtk_vbox_new(FALSE, 10);
	gtk_box_pack_end (GTK_BOX (hbox), textVBox, true, true, 0);
	
	priv->statement = gtk_label_new(NULL);
	gtk_label_set_line_wrap (GTK_LABEL (priv->statement), TRUE);
	gtk_label_set_use_markup (GTK_LABEL (priv->statement), TRUE);
	gtk_label_set_selectable (GTK_LABEL (priv->statement), FALSE);
	GTK_WIDGET_UNSET_FLAGS (priv->statement, GTK_CAN_FOCUS);
	gtk_misc_set_alignment (GTK_MISC (priv->statement), 0, 0);
	gtk_box_pack_start (GTK_BOX (textVBox), priv->statement, false, false, 0);

	priv->secondaryStatement = gtk_label_new(NULL);
	gtk_label_set_line_wrap (GTK_LABEL (priv->secondaryStatement), TRUE);
	gtk_label_set_selectable (GTK_LABEL (priv->statement), FALSE);
	GTK_WIDGET_UNSET_FLAGS (priv->statement, GTK_CAN_FOCUS);
	gtk_misc_set_alignment (GTK_MISC (priv->statement), 0, 0);
	gtk_box_pack_start (GTK_BOX (textVBox), priv->secondaryStatement, true, true, 8);
	
	priv->progressBar = gtk_progress_bar_new();
	gtk_box_pack_start (GTK_BOX (vbox), priv->progressBar, true, false, 8);
	gtk_progress_bar_set_activity_blocks (GTK_PROGRESS_BAR (priv->progressBar), 20);
	gtk_progress_bar_set_orientation (GTK_PROGRESS_BAR (priv->progressBar), GTK_PROGRESS_LEFT_TO_RIGHT);
	gtk_progress_bar_set_pulse_step (GTK_PROGRESS_BAR (priv->progressBar), 0.05);

	gtk_widget_show (vbox);
	gtk_widget_show (priv->image);
	gtk_widget_show (hbox);
	gtk_widget_show (textVBox);
	gtk_widget_show (priv->statement);
	gtk_widget_show (priv->secondaryStatement);
	gtk_widget_show (priv->progressBar);
	
	gtk_widget_pop_composite_child ();
	
	/* force defaults */
}

static void
ifa_wait_dialog_finalize (GObject *object)
{
	IFAWaitDialog *wait_dialog = IFA_WAIT_DIALOG (object);
	IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;
	
	g_message ("ifa_wait_dialog_finalize()");

	priv->bHideCalled = TRUE;
	if (priv->progressBarTimeout > 0)
	{
		g_source_remove (priv->progressBarTimeout);
		priv->progressBarTimeout = 0;
	}
	
	G_OBJECT_CLASS (ifa_wait_dialog_parent_class)->finalize (object);
}

static void
ifa_wait_dialog_set_property (GObject      *object, 
			       guint         prop_id, 
			       const GValue *value, 
			       GParamSpec   *pspec)
{
  IFAWaitDialog *wait_dialog = IFA_WAIT_DIALOG (object);
  IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;

  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
ifa_wait_dialog_get_property (GObject    *object, 
			       guint       prop_id, 
			       GValue     *value, 
			       GParamSpec *pspec)
{
  IFAWaitDialog *wait_dialog = IFA_WAIT_DIALOG (object);
  IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;
	
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
  GtkDialog *dialog;

  dialog = GTK_DIALOG (widget);

  /* Override the style properties with HIG-compliant spacings.  Ugh.
   * http://developer.gnome.org/projects/gup/hig/1.0/layout.html#layout-dialogs
   * http://developer.gnome.org/projects/gup/hig/1.0/windows.html#alert-spacing
   */

  gtk_container_set_border_width (GTK_CONTAINER (dialog->vbox), 12);
  gtk_box_set_spacing (GTK_BOX (dialog->vbox), 12);

  gtk_container_set_border_width (GTK_CONTAINER (dialog->action_area), 0);
  gtk_box_set_spacing (GTK_BOX (dialog->action_area), 6);
}

static void
ifa_wait_dialog_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
  if (GTK_WIDGET_CLASS (ifa_wait_dialog_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_wait_dialog_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
}

/**
 * ifa_wait_dialog_new:
 *
 * Creates a new #IFAWaitDialog.
 *
 * Returns: a newly created #IFAWaitDialog
 *
 * Since: 2.6
 */
GtkWidget *
/*ifa_wait_dialog_new (void)*/
ifa_wait_dialog_new(GtkWindow *parent, GdkPixbuf *icon_pixbuf, IFAWaitDialogButtonSet buttonSet, const gchar *title, const gchar *statement, const gchar *secondaryStatement)
{
	IFAWaitDialogPrivate *priv;
	gchar *statementMarkup;
	IFAWaitDialog *dialog = IFA_WAIT_DIALOG (g_object_new (IFA_WAIT_DIALOG_TYPE, NULL));
	
	priv = IFA_WAIT_DIALOG_GET_PRIVATE(dialog);

 	if (parent != NULL)
		gtk_window_set_transient_for (GTK_WINDOW (dialog), parent);
	
	if (icon_pixbuf != NULL)
		gtk_image_set_from_pixbuf (GTK_IMAGE (priv->image), icon_pixbuf);
	
	g_message("FIXME: Do something with the buttonSet");
	
	if (title != NULL)
		gtk_window_set_title (GTK_WINDOW (dialog), title);
	
	if (statement != NULL)
	{
		statementMarkup = g_markup_printf_escaped ("<span weight=\"bold\" size=\"larger\">%s</span>", statement);
		gtk_label_set_markup (GTK_LABEL (priv->statement), statementMarkup);
		g_free (statementMarkup);
	}
	
	if (secondaryStatement != NULL)
		gtk_label_set_text (GTK_LABEL (priv->secondaryStatement), secondaryStatement);
	
	g_signal_connect (dialog, "realize", G_CALLBACK(dialog_realized), NULL);

	return GTK_WIDGET (dialog);
}

static void
dialog_realized(GtkWidget *widget, gpointer user_data)
{
	/* start the progress bar moving */
	g_timeout_add (250, (GSourceFunc)update_progress, IFA_WAIT_DIALOG(widget));
}

static void 
close_cb (IFAWaitDialog *wait_dialog)
{
  IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;

  gtk_widget_hide (GTK_WIDGET (wait_dialog));
}

static gboolean
update_progress (IFAWaitDialog *wait_dialog)
{
	IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;
	
	if (priv->bHideCalled)
		return FALSE;	/* the dialog is closing so stop the timer */
	
	g_idle_add ((GSourceFunc)pulse_progress_bar, wait_dialog);
	
	return TRUE; /* keep the timeout going */
}

static gboolean
pulse_progress_bar (IFAWaitDialog *wait_dialog)
{
	IFAWaitDialogPrivate *priv = (IFAWaitDialogPrivate *)wait_dialog->private_data;

	gtk_progress_bar_pulse (GTK_PROGRESS_BAR (priv->progressBar));
	
	return FALSE; /* prevent GLib from automatically calling this again */
}
