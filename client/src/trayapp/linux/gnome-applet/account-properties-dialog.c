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

#include "account-properties-dialog.h"

/*@todo Remove this when gettext is added */
#define _

extern iFolderClient *ifolder_client;

typedef struct _IFAAccountPropsDialogPrivate IFAAccountPropsDialogPrivate;
struct _IFAAccountPropsDialogPrivate 
{
	GtkWidget 	*parentWindow;

	iFolderDomain	*domain;

	gboolean	controlKeyPressed;
	
	/**
	 * Global Widgets
	 */
	GtkWidget 	*notebook;
	GtkWidget	*enableAccountButton;
	GtkWidget	*defaultAccountButton;

	/**
	 * Widgets for the Server Page
	 */
	GtkWidget	*serverPage;
	GtkWidget	*serverNameLabel;
	GtkWidget	*serverAddressEntry;
	GtkWidget	*serverDescriptionTextView;
	gboolean	bServerAddressChanged;

	/**
	 * Widgets for the Identity Page
	 */
	GtkWidget	*identityPage;
	GtkWidget	*userNameLabel;
	GtkWidget	*passwordEntry;
	GtkWidget	*rememberPasswordButton;
	gboolean	bPasswordChanged;

	/**
	 * Widgets for the Disk Space Page
	 */
	GtkWidget	*diskSpacePage;
	GtkWidget	*quotaTotalLabel;
	GtkWidget	*quotaUsedLabel;
	GtkWidget	*quotaAvailableLabel;
	GtkWidget	*quotaGraph;
	
	/**
	 * Signal Handler IDs
	 */
	gulong		domain_host_modified_cb_id;
	gulong		domain_logged_in_cb_id;
	gulong		domain_logged_out_cb_id;
	gulong		domain_activated_cb_id;
	gulong		domain_inactivated_cb_id;
	gulong		domain_new_default_cb_id;
	gulong		domain_removed_cb_id;
	
	gulong		on_enable_account_toggled_cb_id;
	gulong		on_default_account_toggled_cb_id;

	gulong		on_server_address_changed_cb_id;
	gulong		on_server_address_focus_out_cb_id;

	gulong		on_password_entry_changed_cb_id;
	gulong		on_password_entry_focus_out_cb_id;
	gulong		on_remember_password_toggled_cb_id;
};

#define IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE(obj) (G_TYPE_INSTANCE_GET_PRIVATE ((obj), IFA_ACCOUNT_PROPS_DIALOG_TYPE, IFAAccountPropsDialogPrivate))

static void                 ifa_account_props_dialog_finalize       (GObject            *object);

G_DEFINE_TYPE (IFAAccountPropsDialog, ifa_account_props_dialog, GTK_TYPE_DIALOG)

/* Forward Declarations */

static void                 ifa_account_props_dialog_get_property   (GObject            *object,
							     guint               prop_id,
							     GValue             *value,
							     GParamSpec         *pspec);
static void                 ifa_account_props_dialog_set_property   (GObject            *object,
							     guint               prop_id,
							     const GValue       *value,
							     GParamSpec         *pspec);
static void                 ifa_account_props_dialog_style_set      (GtkWidget          *widget,
			                                     GtkStyle           *previous_style);

GtkWidget * create_notebook (IFAAccountPropsDialog *apd);
GtkWidget * create_server_page (IFAAccountPropsDialog *apd);
GtkWidget * create_identity_page (IFAAccountPropsDialog *apd);
GtkWidget * create_disk_space_page (IFAAccountPropsDialog *apd);

GtkWidget * create_global_check_buttons (IFAAccountPropsDialog *apd);

static void on_dialog_response (GtkDialog *dialog, gint arg1, IFAAccountPropsDialog *apd);
static gboolean on_key_press (GtkWidget *widget, GdkEventKey *event, IFAAccountPropsDialog *apd);
static gboolean on_key_release (GtkWidget *widget, GdkEventKey *event, IFAAccountPropsDialog *apd);
static void on_realize (GtkWidget *widget, IFAAccountPropsDialog *apd);
static void on_response (GtkDialog *dialog, gint response_id, IFAAccountPropsDialog *apd);

static void on_server_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd);
static void on_identity_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd);
static void on_disk_space_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd);

static void close_window (IFAAccountPropsDialog *apd);

static void	init_global_check_buttons (IFAAccountPropsDialog *apd);

static void on_domain_host_modified (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_logged_in (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_logged_out (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_activated (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_inactivated (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_new_default (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);
static void on_domain_removed (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd);

static void on_enable_account_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd);
static void on_default_account_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd);

static void on_server_address_changed (GtkEntry *entry, IFAAccountPropsDialog *apd);
static gboolean on_server_address_focus_out (GtkWidget *widget, GdkEventFocus *event, IFAAccountPropsDialog *apd);

static void on_password_entry_changed (GtkEntry *entry, IFAAccountPropsDialog *apd);
static gboolean on_password_entry_focus_out (GtkWidget *widget, GdkEventFocus *event, IFAAccountPropsDialog *apd);
static void on_remember_password_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd);

static gboolean save_server_address (IFAAccountPropsDialog *apd);
static gboolean save_password (IFAAccountPropsDialog *apd);

static void
ifa_account_props_dialog_class_init (IFAAccountPropsDialogClass *klass)
{
  GObjectClass *object_class;
  GtkWidgetClass *widget_class;
  GtkDialogClass *dialog_class;
  
  g_message ("ifa_account_props_dialog_class_init()");
	
  object_class = (GObjectClass *)klass;
  widget_class = (GtkWidgetClass *)klass;
  dialog_class = (GtkDialogClass *)klass;
	
  object_class->set_property = ifa_account_props_dialog_set_property;
  object_class->get_property = ifa_account_props_dialog_get_property;

  object_class->finalize = ifa_account_props_dialog_finalize;

  widget_class->style_set = ifa_account_props_dialog_style_set;

  g_type_class_add_private (object_class, sizeof (IFAAccountPropsDialogPrivate));
}

static void
ifa_account_props_dialog_init (IFAAccountPropsDialog *account_props_dialog)
{
	IFAAccountPropsDialogPrivate *priv;
	GtkWidget *vbox;
	
	g_message ("ifa_account_props_dialog_init()");

	/* Data */
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (account_props_dialog);
	account_props_dialog->private_data = priv;

	/* Widgets */
	gtk_widget_push_composite_child ();
	vbox = gtk_vbox_new (FALSE, 12);
	
	gtk_box_pack_start (GTK_BOX (GTK_DIALOG (account_props_dialog)->vbox), vbox, TRUE, TRUE, 0);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), IFA_DEFAULT_BORDER_WIDTH);

	gtk_box_pack_start (GTK_BOX (vbox), create_notebook(account_props_dialog), true, true, 0);
	gtk_box_pack_start (GTK_BOX (vbox), create_global_check_buttons(account_props_dialog), false, false, 0); 

	gtk_widget_show (vbox);
	
	gtk_widget_pop_composite_child ();
	
	gtk_dialog_add_button (GTK_DIALOG (account_props_dialog), GTK_STOCK_CLOSE, GTK_RESPONSE_OK);
}

static void
ifa_account_props_dialog_finalize (GObject *object)
{
	IFAAccountPropsDialog *account_props_dialog = IFA_ACCOUNT_PROPS_DIALOG (object);
	IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (account_props_dialog);

	g_message ("ifa_account_props_dialog_finalize()");

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
	
	G_OBJECT_CLASS (ifa_account_props_dialog_parent_class)->finalize (object);
}

static void
ifa_account_props_dialog_set_property (GObject      *object, 
			       guint         prop_id, 
			       const GValue *value, 
			       GParamSpec   *pspec)
{
  IFAAccountPropsDialog *account_props_dialog = IFA_ACCOUNT_PROPS_DIALOG (object);
  IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (account_props_dialog);

  switch (prop_id) 
    {
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
      break;
    }
}

static void
ifa_account_props_dialog_get_property (GObject    *object, 
			       guint       prop_id, 
			       GValue     *value, 
			       GParamSpec *pspec)
{
  IFAAccountPropsDialog *account_props_dialog = IFA_ACCOUNT_PROPS_DIALOG (object);
  IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (account_props_dialog);
	
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

/*  gtk_container_set_border_width (GTK_CONTAINER (dialog->vbox), 12);*/
  gtk_box_set_spacing (GTK_BOX (dialog->vbox), 12);

  gtk_container_set_border_width (GTK_CONTAINER (dialog->action_area), 0);
  gtk_box_set_spacing (GTK_BOX (dialog->action_area), 6);
}

static void
ifa_account_props_dialog_style_set (GtkWidget *widget,
			    GtkStyle  *previous_style)
{
  if (GTK_WIDGET_CLASS (ifa_account_props_dialog_parent_class)->style_set)
    GTK_WIDGET_CLASS (ifa_account_props_dialog_parent_class)->style_set (widget, previous_style);

  dialog_style_set (widget, previous_style, NULL);
}

GtkWidget *
ifa_account_props_dialog_new(GtkWindow *parent, iFolderDomain *domain)
{
	IFAAccountPropsDialogPrivate *priv;
	gchar *tmpStr;
	g_message ("ifa_account_props_dialog_new()...calling g_object_new()");
	IFAAccountPropsDialog *dialog = IFA_ACCOUNT_PROPS_DIALOG (g_object_new (IFA_ACCOUNT_PROPS_DIALOG_TYPE, NULL));
	
	g_message ("done calling g_object_new()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE(dialog);

	priv->domain = domain;
	priv->controlKeyPressed = FALSE;
	priv->bServerAddressChanged = FALSE;
	priv->bPasswordChanged = FALSE;

	priv->domain_host_modified_cb_id	= 0;
	priv->domain_logged_in_cb_id		= 0;
	priv->domain_logged_out_cb_id		= 0;
	priv->domain_activated_cb_id		= 0;
	priv->domain_inactivated_cb_id		= 0;
	priv->domain_new_default_cb_id		= 0;
	priv->domain_removed_cb_id			= 0;

	priv->on_enable_account_toggled_cb_id		= 0;
	priv->on_default_account_toggled_cb_id		= 0;

	priv->on_server_address_changed_cb_id		= 0;
	priv->on_server_address_focus_out_cb_id		= 0;

	priv->on_password_entry_changed_cb_id		= 0;
	priv->on_password_entry_focus_out_cb_id		= 0;
	priv->on_remember_password_toggled_cb_id	= 0;

	if (parent != NULL)
	{
		gtk_window_set_transient_for (GTK_WINDOW (dialog), parent);
		gtk_window_set_destroy_with_parent (GTK_WINDOW (dialog), TRUE);
		gtk_window_set_modal (GTK_WINDOW (dialog), TRUE);
	}
	
	tmpStr = g_markup_printf_escaped ("%s %s", ifolder_domain_get_name (domain), _("Properties"));
	gtk_window_set_title (GTK_WINDOW (dialog), tmpStr);
	g_free (tmpStr);
	
	gtk_dialog_set_has_separator (GTK_DIALOG (dialog), FALSE);
	gtk_window_set_resizable (GTK_WINDOW (dialog), FALSE);
	gtk_window_set_modal (GTK_WINDOW (dialog), FALSE);
	gtk_window_set_type_hint (GTK_WINDOW (dialog), GDK_WINDOW_TYPE_HINT_NORMAL);
	gtk_dialog_set_default_response (GTK_DIALOG (dialog), GTK_RESPONSE_OK);
//	gtk_window_set_decorated (GTK_WINDOW (dialog), FALSE);
//	gtk_window_set_has_frame (GTK_WINDOW (dialog), FALSE);
	g_signal_connect (dialog, "response", G_CALLBACK (on_dialog_response), dialog);
	g_signal_connect (dialog, "key-press-event", G_CALLBACK (on_key_press), dialog);
	g_signal_connect (dialog, "key-release-event", G_CALLBACK (on_key_release), dialog);
	g_signal_connect (dialog, "realize", G_CALLBACK (on_realize), dialog);
	g_signal_connect (dialog, "response", G_CALLBACK (on_response), dialog);
	
	g_signal_connect (priv->serverPage, "realize", G_CALLBACK (on_server_page_realize), dialog);
	g_signal_connect (priv->identityPage, "realize", G_CALLBACK (on_identity_page_realize), dialog);
	g_signal_connect (priv->diskSpacePage, "realize", G_CALLBACK (on_disk_space_page_realize), dialog);

	return GTK_WIDGET (dialog);
}

iFolderDomain *
ifa_account_props_dialog_get_domain (IFAAccountPropsDialog *dialog)
{
	IFAAccountPropsDialogPrivate *priv;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (dialog);

	return priv->domain;
}

GtkWidget *
create_notebook (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->notebook = gtk_notebook_new ();

	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_server_page(apd), gtk_label_new (_("System")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_identity_page(apd), gtk_label_new (_("Identity")));
	gtk_notebook_append_page (GTK_NOTEBOOK (priv->notebook), create_disk_space_page(apd), gtk_label_new (_("Disk Space")));
	gtk_widget_show_all (priv->notebook);

	return priv->notebook;
}

GtkWidget *
create_server_page (IFAAccountPropsDialog *apd)
{
	GtkWidget *table;
	GtkWidget *l;
	IFAAccountPropsDialogPrivate *priv;
	GtkRequisition req;
	GtkWidget *sw;
	GtkTextBuffer *textBuffer;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->serverPage = gtk_vbox_new (FALSE, 0);

	table = gtk_table_new(6, 2, FALSE);
	gtk_box_pack_start (GTK_BOX (priv->serverPage), table, true, true, 0);
	gtk_table_set_col_spacings(GTK_TABLE(table), 12);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);

	/**
	 * Row 1
	 */
	l = gtk_label_new (_("Name:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	priv->serverNameLabel = gtk_label_new (NULL);
	gtk_table_attach (GTK_TABLE (table), priv->serverNameLabel, 1,2, 0,1,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_label_set_use_underline (GTK_LABEL (priv->serverNameLabel), FALSE);
	gtk_misc_set_alignment(GTK_MISC(priv->serverNameLabel), 0, 0.5);

	/**
	 * Row 2
	 */
	l = gtk_label_new (_("Address:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 1,2,
					(GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	
	priv->serverAddressEntry = gtk_entry_new ();
	gtk_table_attach (GTK_TABLE (table), priv->serverAddressEntry, 1,2, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_label_set_mnemonic_widget (GTK_LABEL (l), priv->serverAddressEntry);

	/**
	 * Row 3
	 */
	l = gtk_label_new (_("You can change the address if you are offline"));
	gtk_table_attach (GTK_TABLE (table), l, 1,2, 2,3,
					(GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_label_set_line_wrap (GTK_LABEL (l), TRUE);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	gtk_widget_set_sensitive (l, FALSE);
	gtk_widget_size_request (priv->serverAddressEntry, &req);
	gtk_widget_set_size_request (l, req.width, -1);
	
	/**
	 * Row 4
	 */
	sw = gtk_scrolled_window_new(NULL, NULL);
	gtk_table_attach (GTK_TABLE (table), sw, 0,2, 3,4,
					(GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					(GtkAttachOptions)0,
					0,0);
	gtk_scrolled_window_set_shadow_type(GTK_SCROLLED_WINDOW(sw), GTK_SHADOW_ETCHED_IN);
	gtk_scrolled_window_set_policy(GTK_SCROLLED_WINDOW(sw), GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);

	priv->serverDescriptionTextView = gtk_text_view_new ();
	gtk_text_view_set_wrap_mode (GTK_TEXT_VIEW (priv->serverDescriptionTextView), GTK_WRAP_WORD);
	gtk_text_view_set_editable (GTK_TEXT_VIEW (priv->serverDescriptionTextView), FALSE);
	gtk_widget_set_sensitive (priv->serverDescriptionTextView, FALSE);
	gtk_text_view_set_cursor_visible (GTK_TEXT_VIEW (priv->serverDescriptionTextView), FALSE);
	gtk_text_view_set_left_margin (GTK_TEXT_VIEW (priv->serverDescriptionTextView), 5);
	gtk_text_view_set_right_margin (GTK_TEXT_VIEW (priv->serverDescriptionTextView), 5);
	gtk_container_add(GTK_CONTAINER(sw), priv->serverDescriptionTextView);

	return priv->serverPage;
}

GtkWidget *
create_identity_page (IFAAccountPropsDialog *apd)
{
	GtkWidget *table;
	GtkWidget *l;
	IFAAccountPropsDialogPrivate *priv;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->identityPage = gtk_vbox_new (FALSE, 0);

	table = gtk_table_new(3, 2, FALSE);
	gtk_box_pack_start (GTK_BOX (priv->identityPage), table, true, true, 0);
	gtk_table_set_col_spacings(GTK_TABLE(table), 12);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);

	/**
	 * Row 1 (User Name)
	 */
	l = gtk_label_new (_("User Name:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	priv->userNameLabel = gtk_label_new (NULL);
	gtk_table_attach (GTK_TABLE (table), l, 1,2, 0,1,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_label_set_use_underline (GTK_LABEL (l), FALSE);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	
	/**
	 * Row 2 (User Password)
	 */
	l = gtk_label_new (_("_Password:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);
	gtk_label_set_use_underline (GTK_LABEL (l), TRUE);

	priv->passwordEntry = gtk_entry_new ();
	gtk_table_attach (GTK_TABLE (table), priv->passwordEntry, 1,2, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_entry_set_visibility (GTK_ENTRY (priv->passwordEntry), FALSE);
	gtk_label_set_mnemonic_widget (GTK_LABEL (l), priv->passwordEntry);
	
	/**
	 * Row 3 (Remember Password Checkbox)
	 */
	priv->rememberPasswordButton = gtk_check_button_new_with_mnemonic (_("_Remember password"));
	gtk_table_attach (GTK_TABLE (table), priv->rememberPasswordButton, 1,2, 2,3,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);

	return priv->identityPage;
}

GtkWidget *
create_disk_space_page (IFAAccountPropsDialog *apd)
{
	GtkWidget *table;
	GtkWidget *l;
	IFAAccountPropsDialogPrivate *priv;
	GtkWidget *graphFrame;
	GtkWidget *graphBox;
	GtkWidget *graphLabelBox;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->diskSpacePage = gtk_vbox_new (FALSE, 0);

	table = gtk_table_new(3, 3, FALSE);
	gtk_box_pack_start (GTK_BOX (priv->diskSpacePage), table, true, true, 0);
	gtk_table_set_col_spacings(GTK_TABLE(table), 12);
	gtk_table_set_row_spacings(GTK_TABLE(table), 6);
	gtk_container_set_border_width(GTK_CONTAINER(table), 12);

	/**
	 * Row 1 (Quota)
	 */
	l = gtk_label_new (_("Quota:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 0,1,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	priv->quotaTotalLabel = gtk_label_new (NULL);
	gtk_table_attach (GTK_TABLE (table), priv->quotaTotalLabel, 1,2, 0,1,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(priv->quotaTotalLabel), 0, 0.5);
	
	/**
	 * Row 2 (Used)
	 */
	l = gtk_label_new (_("Used:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 1,2,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	priv->quotaUsedLabel = gtk_label_new (NULL);
	gtk_table_attach (GTK_TABLE (table), priv->quotaUsedLabel, 1,2, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(priv->quotaUsedLabel), 0, 0.5);
	
	/**
	 * Row 3 (Available)
	 */
	l = gtk_label_new (_("Available:"));
	gtk_table_attach (GTK_TABLE (table), l, 0,1, 2,3,
					 (GtkAttachOptions)(GTK_EXPAND | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0.5);

	priv->quotaAvailableLabel = gtk_label_new (NULL);
	gtk_table_attach (GTK_TABLE (table), priv->quotaAvailableLabel, 1,2, 1,2,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_misc_set_alignment(GTK_MISC(priv->quotaAvailableLabel), 0, 0.5);
	
	/**
	 * Disk Space Graph
	 */
	graphFrame = gtk_frame_new (NULL);
	gtk_table_attach (GTK_TABLE (table), graphFrame, 2,3, 0,3,
					 (GtkAttachOptions)(GTK_SHRINK | GTK_FILL),
					 (GtkAttachOptions)0,
					 0,0);
	gtk_frame_set_shadow_type (GTK_FRAME (graphFrame), GTK_SHADOW_ETCHED_OUT);
	
	graphBox = gtk_hbox_new (FALSE, 5);
	gtk_container_set_border_width (GTK_CONTAINER (graphBox), 5);
	
	gtk_container_add (GTK_CONTAINER (graphFrame), graphBox);
	
	priv->quotaGraph = gtk_progress_bar_new ();
	gtk_box_pack_start (GTK_BOX (graphBox), priv->quotaGraph, FALSE, TRUE, 0);

	gtk_progress_bar_set_orientation (GTK_PROGRESS_BAR (priv->quotaGraph), GTK_PROGRESS_BOTTOM_TO_TOP);
	gtk_progress_bar_set_pulse_step (GTK_PROGRESS_BAR (priv->quotaGraph), 0.10);
	gtk_progress_bar_set_fraction (GTK_PROGRESS_BAR (priv->quotaGraph), 0);
	
	graphLabelBox = gtk_vbox_new (FALSE, 0);
	gtk_box_pack_start (GTK_BOX (graphBox), graphLabelBox, FALSE, TRUE, 0);
	
	l = gtk_label_new (_("full"));
	gtk_misc_set_alignment(GTK_MISC(l), 0, 0);
	gtk_box_pack_start (GTK_BOX (graphLabelBox), l, TRUE, TRUE, 0);
	
	l = gtk_label_new (_("empty"));
	gtk_misc_set_alignment(GTK_MISC(l), 0, 1);
	gtk_box_pack_start (GTK_BOX (graphLabelBox), l, TRUE, TRUE, 0);
	
	return priv->diskSpacePage;
}

GtkWidget *
create_global_check_buttons (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	GtkWidget *vbox;

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	vbox = gtk_vbox_new (FALSE, 0);
	
	priv->enableAccountButton = gtk_check_button_new_with_mnemonic (_("_Automatically connect"));
	gtk_box_pack_start (GTK_BOX (vbox), priv->enableAccountButton, FALSE, FALSE, 0);
	
	priv->defaultAccountButton = gtk_check_button_new_with_mnemonic (_("Account is _default"));
	gtk_box_pack_start (GTK_BOX (vbox), priv->defaultAccountButton, FALSE, FALSE, 0);
	
	return vbox;
}

static void
on_dialog_response (GtkDialog *dialog, gint arg1, IFAAccountPropsDialog *apd)
{
	g_message ("FIXME: Implement on_dialog_response() in account-properties.c");
	gtk_widget_hide (GTK_WIDGET (apd));
	gtk_widget_destroy (GTK_WIDGET (apd));
}

static gboolean
on_key_press (GtkWidget *widget, GdkEventKey *event, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	gboolean stop_other_handlers = true;
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	switch(event->keyval)
	{
		case GDK_Escape:
			close_window (apd);
			break;
		case GDK_Control_L:
		case GDK_Control_R:
			priv->controlKeyPressed = true;
			stop_other_handlers = false;
			break;
		case GDK_W:
		case GDK_w:
			if (priv->controlKeyPressed)
				close_window (apd);
			else
				stop_other_handlers = false;
			break;
		default:
			stop_other_handlers = false;
			break;
	}

	return stop_other_handlers;
}

static gboolean
on_key_release (GtkWidget *widget, GdkEventKey *event, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	gboolean stop_other_handlers = false;
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	switch(event->keyval)
	{
		case GDK_Control_L:
		case GDK_Control_R:
			priv->controlKeyPressed = false;
			break;
		default:
			break;
	}

	return stop_other_handlers;
}

static void
on_realize (GtkWidget *widget, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_message ("FIXME: Implement IFAAccountPropsDialog::on_realize()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	init_global_check_buttons (apd);

	/* Register for domain events that could affect this dialog */
	priv->domain_host_modified_cb_id = g_signal_connect (ifolder_client, "domain-host-modified", G_CALLBACK (on_domain_host_modified), apd);
	priv->domain_logged_in_cb_id = g_signal_connect (ifolder_client, "domain-logged-in", G_CALLBACK (on_domain_logged_in), apd);
	priv->domain_logged_out_cb_id = g_signal_connect (ifolder_client, "domain-logged-out", G_CALLBACK (on_domain_logged_out), apd);
	priv->domain_activated_cb_id = g_signal_connect (ifolder_client, "domain-activated", G_CALLBACK (on_domain_activated), apd);
	priv->domain_inactivated_cb_id = g_signal_connect (ifolder_client, "domain-inactivated", G_CALLBACK (on_domain_inactivated), apd);
	priv->domain_new_default_cb_id = g_signal_connect (ifolder_client, "domain-new-default", G_CALLBACK (on_domain_new_default), apd);
	priv->domain_removed_cb_id = g_signal_connect (ifolder_client, "domain-removed", G_CALLBACK (on_domain_removed), apd);
	
	/* Widget signal handlers */
	priv->on_enable_account_toggled_cb_id		= g_signal_connect (priv->enableAccountButton, "toggled", G_CALLBACK (on_enable_account_toggled), apd);
	priv->on_default_account_toggled_cb_id		= g_signal_connect (priv->defaultAccountButton, "toggled", G_CALLBACK (on_default_account_toggled), apd);

	priv->on_server_address_changed_cb_id		= g_signal_connect (priv->serverAddressEntry, "changed", G_CALLBACK (on_server_address_changed), apd);
	priv->on_server_address_focus_out_cb_id		= g_signal_connect (priv->serverAddressEntry, "focus-out", G_CALLBACK (on_server_address_focus_out), apd);

	priv->on_password_entry_changed_cb_id		= g_signal_connect (priv->passwordEntry, "changed", G_CALLBACK (on_password_entry_changed), apd);
	priv->on_password_entry_focus_out_cb_id		= g_signal_connect (priv->passwordEntry, "focus-out", G_CALLBACK (on_password_entry_focus_out), apd);
	priv->on_remember_password_toggled_cb_id	= g_signal_connect (priv->rememberPasswordButton, "toggled", G_CALLBACK (on_remember_password_toggled), apd);
}

static void
on_response (GtkDialog *dialog, gint response_id, IFAAccountPropsDialog *apd)
{
	/* FIXME: Figure out why this is NOT being called! */
	g_message ("FIXME: Implement IFAAccountPropsDialog::on_response()");
}

static void
on_server_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	GtkTextBuffer *textBuffer;
	
	g_message ("FIXME: Implement IFAAccountPropsDialog::on_server_page_realize()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (ifolder_domain_get_name (priv->domain) != NULL)
		gtk_label_set_text (GTK_LABEL (priv->serverNameLabel), ifolder_domain_get_name (priv->domain));
		
	if (ifolder_domain_get_host_address (priv->domain) != NULL)
		gtk_entry_set_text (GTK_ENTRY (priv->serverAddressEntry), ifolder_domain_get_host_address (priv->domain));
		
	if (ifolder_domain_is_authenticated (priv->domain))
	{
		gtk_widget_set_sensitive (priv->serverAddressEntry, FALSE);
		gtk_entry_set_editable (GTK_ENTRY (priv->serverAddressEntry), FALSE);
	}
	else
	{
		gtk_widget_set_sensitive (priv->serverAddressEntry, TRUE);
		gtk_entry_set_editable (GTK_ENTRY (priv->serverAddressEntry), TRUE);
	}

	if (ifolder_domain_get_description (priv->domain) != NULL)
	{
		textBuffer = gtk_text_buffer_new (NULL);
		gtk_text_buffer_set_text (textBuffer, ifolder_domain_get_description (priv->domain), -1);
		gtk_text_view_set_buffer (GTK_TEXT_VIEW (priv->serverDescriptionTextView), textBuffer);
	}
}

static void
on_identity_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_message ("FIXME: Implement IFAAccountPropsDialog::on_identity_page_realize()");

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	if (ifolder_domain_get_user_name (priv->domain) != NULL)
		gtk_label_set_text (GTK_LABEL (priv->userNameLabel), ifolder_domain_get_user_name (priv->domain));
	
	g_debug ("FIXME: Read in the user's password in IFAAccountPropsDialog::on_identity_page_realize()");
	/*
	if (ifolder_domain_get_user_password (priv->domain) != NULL)
	{
		gtk_entry_set_text (GTK_ENTRY (priv->passwordEntry), ifolder_domain_get_user_password (priv->domain));
		
		gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->rememberPasswordButton), TRUE);
	}
	else
	{
		gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->rememberPasswordButton), FALSE);
	}
	*/
}

static void
on_disk_space_page_realize (GtkWidget *widget, IFAAccountPropsDialog *apd)
{
	g_message ("FIXME: Implement IFAAccountPropsDialog::on_disk_space_page_realize()");
/*
			DiskSpace ds = ifdata.GetUserDiskSpace(domain.MemberUserID);

			if(ds == null)
			{
				QuotaTotalLabel.Text = Util.GS("N/A");
				QuotaUsedLabel.Text = Util.GS("N/A");
				QuotaAvailableLabel.Text = Util.GS("N/A");
				QuotaGraph.Fraction = 0;
			}
			else
			{
				int tmpValue;

				if(ds.Limit == 0)
				{
					QuotaTotalLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.Limit / (1024 * 1024));
					QuotaTotalLabel.Text =
						string.Format("{0} {1}", tmpValue, Util.GS("MB"));
				}

				if(ds.UsedSpace == 0)
				{
					QuotaUsedLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
					QuotaUsedLabel.Text =
						string.Format("{0} {1}", tmpValue, Util.GS("MB"));
				}

				if(ds.AvailableSpace == 0)
				{
					QuotaAvailableLabel.Text = Util.GS("N/A");
				}
				else
				{
					tmpValue = (int)(ds.AvailableSpace / (1024 * 1024));
					QuotaAvailableLabel.Text =
						string.Format("{0} {1}",tmpValue, Util.GS("MB"));
				}

				if(ds.Limit == 0)
				{
					QuotaGraph.Fraction = 0;
				}
				else
				{
					if(ds.Limit < ds.UsedSpace)
						QuotaGraph.Fraction = 1;
					else
						QuotaGraph.Fraction = ((double)ds.UsedSpace) / 
												((double)ds.Limit);
				}
			}
*/
}

static void
close_window (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (priv->bServerAddressChanged)
		if (!save_server_address (apd))
			return;
	
	if (priv->bPasswordChanged)
		if (!save_password (apd))
			return;
	
	gtk_widget_hide (GTK_WIDGET (apd));
	gtk_widget_destroy (GTK_WIDGET (apd));
}

static void
init_global_check_buttons (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_message ("FIXME: Implement IFAAccountPropsDialog::init_global_check_buttons()");

	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->enableAccountButton), ifolder_domain_is_active (priv->domain));
	gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->defaultAccountButton), ifolder_domain_is_default (priv->domain));
	gtk_widget_set_sensitive (priv->defaultAccountButton, !(ifolder_domain_is_default (priv->domain)));
}

static void
on_domain_host_modified (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_message ("IFAAccountPropsDialog::on_domain_host_modified()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		if (priv->on_server_address_changed_cb_id > 0)
			g_signal_handler_disconnect (ifolder_client, priv->on_server_address_changed_cb_id);
		
		gtk_entry_set_text (GTK_ENTRY (priv->serverAddressEntry), ifolder_domain_get_host_address (domain));
		priv->bServerAddressChanged = FALSE;
		
		if (priv->on_server_address_changed_cb_id > 0)
			priv->on_server_address_changed_cb_id = g_signal_connect (ifolder_client, "changed", G_CALLBACK (on_server_address_changed), apd);
	}
}

static void
on_domain_logged_in (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_debug ("IFAAccountPropsDialog::on_domain_logged_in()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	/* Make sure that the domain event is for the domain this dialog is showing */
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		/* Reset the values to the original */
		if (priv->on_server_address_changed_cb_id > 0)
			g_signal_handler_disconnect (ifolder_client, priv->on_server_address_changed_cb_id);
		
		gtk_entry_set_text (GTK_ENTRY (priv->serverAddressEntry), ifolder_domain_get_host_address (domain));
		priv->bServerAddressChanged = FALSE;
		
		if (priv->on_server_address_changed_cb_id > 0)
			priv->on_server_address_changed_cb_id = g_signal_connect (ifolder_client, "changed", G_CALLBACK (on_server_address_changed), apd);
			
		/* Prevent the user from modifying the address */
		gtk_widget_set_sensitive (priv->serverAddressEntry, FALSE);
		gtk_entry_set_editable (GTK_ENTRY (priv->serverAddressEntry), FALSE);
	}
}

static void
on_domain_logged_out (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_debug ("IFAAccountPropsDialog::on_domain_logged_out()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	/* Make sure that the domain event is for the domain this dialog is showing */
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		/* Reset the values to the original */
		if (priv->on_server_address_changed_cb_id > 0)
			g_signal_handler_disconnect (ifolder_client, priv->on_server_address_changed_cb_id);
		
		gtk_entry_set_text (GTK_ENTRY (priv->serverAddressEntry), ifolder_domain_get_host_address (domain));
		priv->bServerAddressChanged = FALSE;
		
		if (priv->on_server_address_changed_cb_id > 0)
			priv->on_server_address_changed_cb_id = g_signal_connect (ifolder_client, "changed", G_CALLBACK (on_server_address_changed), apd);
			
		/* Allow the user to modify the address */
		gtk_widget_set_sensitive (priv->serverAddressEntry, TRUE);
		gtk_entry_set_editable (GTK_ENTRY (priv->serverAddressEntry), TRUE);
	}
}

static void
on_domain_activated (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	g_debug ("FIXME: IFAAccountPropsDialog::on_domain_activated()");
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		if (!gtk_toggle_button_get_active (GTK_TOGGLE_BUTTON (priv->enableAccountButton)))
		{
			if (priv->on_enable_account_toggled_cb_id > 0)
				g_signal_handler_disconnect (ifolder_client, priv->on_enable_account_toggled_cb_id);
			
			gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->enableAccountButton), TRUE);
			
			if (priv->on_enable_account_toggled_cb_id > 0)
				priv->on_enable_account_toggled_cb_id = g_signal_connect (priv->enableAccountButton, "toggled", G_CALLBACK (on_enable_account_toggled), apd);
		}
	}
}

static void
on_domain_inactivated (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	g_debug ("FIXME: IFAAccountPropsDialog::on_domain_inactivated()");
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		if (gtk_toggle_button_get_active (GTK_TOGGLE_BUTTON (priv->enableAccountButton)))
		{
			if (priv->on_enable_account_toggled_cb_id > 0)
				g_signal_handler_disconnect (ifolder_client, priv->on_enable_account_toggled_cb_id);
			
			gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->enableAccountButton), FALSE);
			
			if (priv->on_enable_account_toggled_cb_id > 0)
				priv->on_enable_account_toggled_cb_id = g_signal_connect (priv->enableAccountButton, "toggled", G_CALLBACK (on_enable_account_toggled), apd);
		}
	}
}

static void
on_domain_new_default (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;

	g_debug ("IFAAccountPropsDialog::on_domain_new_default()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		/* This domain was just made the new default */
		if (priv->on_default_account_toggled_cb_id > 0)
			g_signal_handler_disconnect (ifolder_client, priv->on_default_account_toggled_cb_id);
			
		gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->defaultAccountButton), TRUE);
		gtk_widget_set_sensitive (priv->defaultAccountButton, FALSE);
		
		if (priv->on_default_account_toggled_cb_id > 0)
			priv->on_default_account_toggled_cb_id = g_signal_connect (priv->defaultAccountButton, "toggled", G_CALLBACK (on_default_account_toggled), apd);
	}
	else if (ifolder_domain_is_default (priv->domain))
	{
		/* This domain is no longer the default */
		if (priv->on_default_account_toggled_cb_id > 0)
			g_signal_handler_disconnect (ifolder_client, priv->on_default_account_toggled_cb_id);
			
		gtk_toggle_button_set_active (GTK_TOGGLE_BUTTON (priv->defaultAccountButton), FALSE);
		gtk_widget_set_sensitive (priv->defaultAccountButton, TRUE);
		
		if (priv->on_default_account_toggled_cb_id > 0)
			priv->on_default_account_toggled_cb_id = g_signal_connect (priv->defaultAccountButton, "toggled", G_CALLBACK (on_default_account_toggled), apd);
	}
}

static void
on_domain_removed (iFolderClient *client, iFolderDomain *domain, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (strcmp (ifolder_domain_get_id (domain), ifolder_domain_get_id (priv->domain)) == 0)
	{
		/* Close and destroy this window since this domain is no longer valid */
		close_window (apd);
	}
}

static void
on_enable_account_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd)
{
	GError *err;
	IFAAccountPropsDialogPrivate *priv;
	GtkWidget *errDialog;
	
g_message ("IFAAccountPropsDialog::on_enable_account_toggled()");
	err = NULL;
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	if (gtk_widget_is_focus (GTK_WIDGET (togglebutton)))
	{
		if (gtk_toggle_button_get_active (togglebutton) != ifolder_domain_is_active (priv->domain))
		{
			if (gtk_toggle_button_get_active (togglebutton))
				ifolder_domain_activate (priv->domain, &err);
			else
				ifolder_domain_inactivate (priv->domain, &err);
			
			if (err)
			{
				errDialog = gtk_message_dialog_new (GTK_WINDOW (apd), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_WARNING, GTK_BUTTONS_CLOSE, err->message);
				g_clear_error (&err);
				gtk_dialog_run (GTK_DIALOG (errDialog));
				gtk_widget_destroy (errDialog);
				
				if (priv->on_enable_account_toggled_cb_id > 0)
					g_signal_handler_disconnect (ifolder_client, priv->on_enable_account_toggled_cb_id);
				
				gtk_toggle_button_set_active (togglebutton, !(gtk_toggle_button_get_active (togglebutton)));
				
				if (priv->on_enable_account_toggled_cb_id > 0)
					priv->on_enable_account_toggled_cb_id = g_signal_connect (priv->enableAccountButton, "toggled", G_CALLBACK (on_enable_account_toggled), apd);
			}
		}
	}
}

static void
on_default_account_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd)
{
	GError *err;
	IFAAccountPropsDialogPrivate *priv;
	GtkWidget *errDialog;
	
g_debug ("IFAAccountPropsDialog::on_default_account_toggled()");
	err = NULL;
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	if (gtk_widget_is_focus (GTK_WIDGET (togglebutton)))
	{
		/**
		 * The defaultAccountButton is not sensitive (enabled) if this
		 * is the default account, so the only state that needs to be
		 * handled here is when a user activates the check button.
		 */
		if (!gtk_toggle_button_get_active (togglebutton)) return; /* This condition should never be hit. */
		
		ifolder_domain_set_default (priv->domain, &err);
		if (err)
		{
			errDialog = gtk_message_dialog_new (GTK_WINDOW (apd), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_WARNING, GTK_BUTTONS_CLOSE, err->message);
			g_clear_error (&err);
			gtk_dialog_run (GTK_DIALOG (errDialog));
			gtk_widget_destroy (errDialog);

			if (priv->on_default_account_toggled_cb_id > 0)
				g_signal_handler_disconnect (ifolder_client, priv->on_default_account_toggled_cb_id);
			
			gtk_toggle_button_set_active (togglebutton, !(gtk_toggle_button_get_active (togglebutton)));
			
			if (priv->on_default_account_toggled_cb_id > 0)
				priv->on_default_account_toggled_cb_id = g_signal_connect (priv->defaultAccountButton, "toggled", G_CALLBACK (on_default_account_toggled), apd);
		}
	}
}

static void
on_server_address_changed (GtkEntry *entry, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_debug ("IFAAccountPropsDialog::on_server_address_changed()");
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->bServerAddressChanged = TRUE;
}

static gboolean
on_server_address_focus_out (GtkWidget *widget, GdkEventFocus *event, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_debug ("IFAAccountPropsDialog::on_server_address_focus_out()");
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (priv->bServerAddressChanged)
		save_server_address (apd);
		
	return FALSE;
}

static void
on_password_entry_changed (GtkEntry *entry, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_debug ("IFAAccountPropsDialog::on_password_entry_changed()");
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);

	priv->bPasswordChanged = TRUE;
}

static gboolean
on_password_entry_focus_out (GtkWidget *widget, GdkEventFocus *event, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_debug ("IFAAccountPropsDialog::on_password_entry_focus_out()");
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (priv->bPasswordChanged)
		save_password (apd);
		
	return FALSE;
}

static void
on_remember_password_toggled (GtkToggleButton *togglebutton, IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	
	g_debug ("IFAAccountPropsDialog::on_remember_password_toggled()");
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	if (gtk_widget_is_focus (GTK_WIDGET (togglebutton)))
		save_password (apd);
}

static gboolean
save_server_address (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	const gchar *serverAddress;
	GtkWidget *errDialog;
	GError *err = NULL;
	gboolean bHostAddressUpdated = FALSE;
	
	g_debug ("IFAAccountPropsDialog::save_server_address()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	serverAddress = gtk_entry_get_text (GTK_ENTRY (priv->serverAddressEntry));
	if (serverAddress == NULL || strlen (serverAddress) == 0)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (apd), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, _("The server address cannot be empty.  Please enter an address for the server."));
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		return FALSE;
	}
	
	ifolder_domain_change_host_address (priv->domain, serverAddress, &err);
	if (err)
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (apd), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
		g_clear_error (&err);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		return FALSE;
	}
	
	priv->bServerAddressChanged = FALSE;
	
	return TRUE;
}

static gboolean
save_password (IFAAccountPropsDialog *apd)
{
	IFAAccountPropsDialogPrivate *priv;
	const gchar *password;
	GtkWidget *errDialog;
	GError *err = NULL;
	gboolean bHostAddressUpdated = FALSE;
	
	g_debug ("IFAAccountPropsDialog::save_password()");
	
	priv = IFA_ACCOUNT_PROPS_DIALOG_GET_PRIVATE (apd);
	
	password = gtk_entry_get_text (GTK_ENTRY (priv->passwordEntry));
	if (strlen (password) == 0)
		ifolder_domain_set_credentials (priv->domain, NULL, IFOLDER_CREDENTIAL_TYPE_NONE, &err);
	else
		ifolder_domain_set_credentials (priv->domain, password, IFOLDER_CREDENTIAL_TYPE_BASIC, &err);

	if (err)	
	{
		errDialog = gtk_message_dialog_new (GTK_WINDOW (apd), GTK_DIALOG_DESTROY_WITH_PARENT, GTK_MESSAGE_ERROR, GTK_BUTTONS_CLOSE, err->message);
		gtk_dialog_run (GTK_DIALOG (errDialog));
		gtk_widget_destroy (errDialog);
		return FALSE;
	}
	
	priv->bPasswordChanged = FALSE;
	
	return TRUE;
}

