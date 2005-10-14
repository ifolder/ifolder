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
#include "ifolder-gtk.h"

#include "ifolder-gtk-internal.h"

#include <stdio.h>
#include <stdbool.h>

typedef struct _GeneralPropertyPageWidgets
{
	GtkWidget *name_label;
	GtkWidget *owner_label;
	GtkWidget *location_label;

	GtkWidget *disk_table;
	GtkWidget *used_value;

	GtkWidget *limit_check_button;
	GtkWidget *limit_entry;
	GtkWidget *limit_label;
	GtkWidget *limit_value;
	GtkWidget *limit_unit;

	GtkWidget *avail_label;
	GtkWidget *avail_value;
	GtkWidget *avail_unit;
	
	GtkWidget *disk_usage_bar;
	GtkWidget *disk_usage_frame;
	GtkWidget *disk_usage_full_label;
	GtkWidget *disk_usage_empty_label;
	
	GtkWidget *last_successful_sync;
	GtkWidget *ff_sync_value;
	GtkWidget *sync_interval_value;
} GeneralPropertyPageWidgets;

static void set_graph (GtkProgressBar *disk_usage_bar,
					   long long used_space, long long limit);

static void sync_now_button_callback (GtkButton *button, gpointer user_data);
static void limit_check_button_callback (GtkToggleButton *toggle_button, gpointer user_data);
static void limit_entry_changed_callback (GtkWidget *entry, gpointer user_data);
static void limit_entry_activate_callback (GtkEntry *entry, gpointer user_data);
static gboolean limit_entry_focus_out_callback (GtkWidget *widget, GdkEventFocus *event, gpointer user_data);

static void free_widgets_struct (gpointer data);
static void free_ifolder_disk_space (gpointer data);

static long long ParseCurrentLimit (const gchar *limit_text);

GtkWidget *
ifolder_general_property_page_new (const char *ifolder_id)
{
	GtkWidget *vbox, *basic_box, *basic_labels_box;
	GtkWidget *ifolder_image;
	GtkWidget *l;
	GtkWidget *basic_table;
	GtkWidget *disk_section_box;
	GtkWidget *disk_spacer_box;
	GtkWidget *graph_box;
	GtkWidget *graph_label_box;
	
	GtkWidget *sync_section_box, *sync_spacer_box, *server_space_label,
			  *sync_widget_box, *sync_table,
			  *right_box,
			  *sync_now_button;
	
	GeneralPropertyPageWidgets *widgets;
	
	widgets = (GeneralPropertyPageWidgets *)
				malloc (sizeof (GeneralPropertyPageWidgets));
	memset (widgets, 0, sizeof (GeneralPropertyPageWidgets));
	
	vbox = gtk_vbox_new (false, 10);
	gtk_container_set_border_width (GTK_CONTAINER (vbox), 10);

	// Store the iFolder ID with our main GtkWidget (vbox) so that callback
	// functions will be able to perform on the specific iFolder.
	g_object_set_data_full (G_OBJECT (vbox), "ifolder_id",
							(gpointer) strdup (ifolder_id), ifolder_free_char_data);
	g_object_set_data_full (G_OBJECT (vbox), "widgets",
							widgets, free_widgets_struct);
	
	/**
	 * Basic information (Name/Owner/Location)
	 */
	basic_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (vbox), basic_box, false, true, 0);
	
	/* Insert ifolder48.png */
	ifolder_image = gtk_image_new_from_file (IFOLDER_IMAGE_BIG_IFOLDER);
	gtk_misc_set_alignment (GTK_MISC (ifolder_image), 0.5, 0);
	gtk_box_pack_start (GTK_BOX (basic_box), ifolder_image, false, false, 0);
	
	basic_labels_box = gtk_vbox_new (false, 5);
	gtk_box_pack_start (GTK_BOX (basic_box), basic_labels_box, false, true, 0);
	
	widgets->name_label = gtk_label_new ("<span weight=\"bold\"></span>");
	gtk_label_set_use_markup (GTK_LABEL (widgets->name_label), true);
	gtk_misc_set_alignment (GTK_MISC (widgets->name_label), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (basic_labels_box), widgets->name_label, false, true, 5);
	
	/* Create a table to hold the values */
	basic_table = gtk_table_new (2, 2, false);
	gtk_box_pack_start (GTK_BOX (basic_labels_box), basic_table, true, true, 0);
	gtk_table_set_col_spacings (GTK_TABLE (basic_table), 5);
	gtk_table_set_row_spacings (GTK_TABLE (basic_table), 5);
	
	l = gtk_label_new ("<span size=\"small\">Owner:</span>");
	gtk_label_set_use_markup (GTK_LABEL (l), true);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	gtk_table_attach (GTK_TABLE (basic_table), l, 0, 1, 0, 1,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	
	widgets->owner_label = gtk_label_new ("<span size=\"small\"></span>");
	gtk_label_set_use_markup (GTK_LABEL (widgets->owner_label), true);
	gtk_misc_set_alignment (GTK_MISC (widgets->owner_label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (basic_table), widgets->owner_label, 1, 2, 0, 1,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	
	l = gtk_label_new ("<span size=\"small\">Location:</span>");
	gtk_label_set_use_markup (GTK_LABEL (l), true);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	gtk_table_attach (GTK_TABLE (basic_table), l, 0, 1, 1, 2,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	
	widgets->location_label = gtk_label_new ("<span size=\"small\"></span>");
	gtk_label_set_use_markup (GTK_LABEL (widgets->location_label), true);
	gtk_misc_set_alignment (GTK_MISC (widgets->location_label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (basic_table), widgets->location_label, 1, 2, 1, 2,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);

	//------------------------------
	// Disk Space
	//------------------------------
	// create a section box
	disk_section_box = gtk_vbox_new (false, 5);
	gtk_box_pack_start (GTK_BOX (vbox), disk_section_box, false, false, 0);

	l = gtk_label_new ("<span weight=\"bold\">Disk Space on Server</span>");
	gtk_label_set_use_markup (GTK_LABEL (l), true);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (disk_section_box), l, false, true, 0);
	
	// Create an hbox to provide spacing
	disk_spacer_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (disk_section_box), disk_spacer_box, true, true, 0);
	l = gtk_label_new ("");
	gtk_box_pack_start (GTK_BOX (disk_spacer_box), l, false, true, 0);
	
	// create a table to hold the values
	widgets->disk_table = gtk_table_new (3, 3, false);
	gtk_box_pack_start (GTK_BOX (disk_spacer_box), widgets->disk_table, true, true, 0);
	gtk_table_set_col_spacings (GTK_TABLE (widgets->disk_table), 20);
	gtk_table_set_row_spacings (GTK_TABLE (widgets->disk_table), 5);

	// Row 0
	l = gtk_label_new ("Used:");
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	gtk_table_attach (GTK_TABLE (widgets->disk_table), l, 0, 1, 0, 1,
					  GTK_EXPAND | GTK_FILL, 0, 0, 0);
	widgets->used_value = gtk_label_new ("0");
	gtk_misc_set_alignment (GTK_MISC (widgets->used_value), 1, 0.5);
	gtk_table_attach (GTK_TABLE (widgets->disk_table), widgets->used_value, 1, 2, 0, 1,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	l = gtk_label_new ("MB");
	gtk_table_attach (GTK_TABLE (widgets->disk_table), l, 2, 3, 0, 1,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);

	// Row 1
	widgets->limit_unit = gtk_label_new ("MB");
	gtk_table_attach (GTK_TABLE (widgets->disk_table), widgets->limit_unit, 2, 3, 1, 2,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	
	// Row 2
	widgets->avail_label = gtk_label_new ("Available:");
	gtk_misc_set_alignment (GTK_MISC (widgets->avail_label), 0, 0.5);
	gtk_table_attach (GTK_TABLE (widgets->disk_table), widgets->avail_label, 0, 1, 2, 3,
					  GTK_EXPAND | GTK_FILL, 0, 0, 0);
	widgets->avail_value = gtk_label_new ("0");
	gtk_misc_set_alignment (GTK_MISC (widgets->avail_value), 1, 0.5);
	gtk_table_attach (GTK_TABLE (widgets->disk_table), widgets->avail_value, 1, 2, 2, 3,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	widgets->avail_unit = gtk_label_new ("MB");
	gtk_table_attach (GTK_TABLE (widgets->disk_table), widgets->avail_unit, 2, 3, 2, 3,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);

	// Disk Usage Frame
	widgets->disk_usage_frame = gtk_frame_new("");
	gtk_box_pack_start (GTK_BOX (disk_spacer_box), widgets->disk_usage_frame, false, true, 0);
	
	graph_box = gtk_hbox_new (false, 5);
	gtk_container_set_border_width (GTK_CONTAINER (graph_box), 5);
	gtk_container_add (GTK_CONTAINER (widgets->disk_usage_frame), graph_box);

	widgets->disk_usage_bar = gtk_progress_bar_new ();
	gtk_box_pack_start (GTK_BOX (graph_box), widgets->disk_usage_bar, false, true, 0);
	
	gtk_progress_bar_set_orientation (GTK_PROGRESS_BAR (widgets->disk_usage_bar),
									  GTK_PROGRESS_BOTTOM_TO_TOP);
	gtk_progress_bar_set_fraction (GTK_PROGRESS_BAR (widgets->disk_usage_bar),
								   0);
								   
	graph_label_box = gtk_vbox_new (false, 0);
	gtk_box_pack_start (GTK_BOX (graph_box), graph_label_box, false, true, 0);
	
	widgets->disk_usage_full_label = gtk_label_new ("full");
	gtk_misc_set_alignment (GTK_MISC (widgets->disk_usage_full_label), 0, 0);
	gtk_box_pack_start (GTK_BOX (graph_label_box), widgets->disk_usage_full_label, true, true, 0);
	
	widgets->disk_usage_empty_label = gtk_label_new ("empty");
	gtk_misc_set_alignment (GTK_MISC (widgets->disk_usage_empty_label), 0, 1);
	gtk_box_pack_start (GTK_BOX (graph_label_box), widgets->disk_usage_empty_label, true, true, 0);


	//------------------------------
	// Synchronization Information
	//------------------------------
	// create a section box
	sync_section_box = gtk_vbox_new (false, 5);
	gtk_box_pack_start (GTK_BOX (vbox), sync_section_box, false, true, 0);
	
	l = gtk_label_new ("<span weight=\"bold\">Synchronization</span>");
	gtk_label_set_use_markup (GTK_LABEL (l), true);
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0.5);
	gtk_box_pack_start (GTK_BOX (sync_section_box), l, false, true, 0);
	
	// create an hbox to provide spacing
	sync_spacer_box = gtk_hbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (sync_section_box), sync_spacer_box, true, true, 0);
	
	server_space_label = gtk_label_new ("");
	gtk_box_pack_start (GTK_BOX (sync_spacer_box), server_space_label, false, true, 0);
	
	// create a vbox to actually place the widgets in for section
	sync_widget_box = gtk_vbox_new (false, 10);
	gtk_box_pack_start (GTK_BOX (sync_spacer_box), sync_widget_box, true, true, 0);
	
	// create a table to hold the values
	sync_table = gtk_table_new (3, 2, false);
	gtk_box_pack_start (GTK_BOX (sync_widget_box), sync_table, true, true, 0);
	gtk_table_set_col_spacings (GTK_TABLE (sync_table), 20);
	gtk_table_set_row_spacings (GTK_TABLE (sync_table), 5);

	l = gtk_label_new ("Last Successful Synchronization:");
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), l, 0, 1, 0, 1,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);

	widgets->last_successful_sync = gtk_label_new ("N/A");
	gtk_misc_set_alignment (GTK_MISC (widgets->last_successful_sync), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), widgets->last_successful_sync, 1, 2, 0, 1,
					  GTK_EXPAND | GTK_FILL, 0, 0, 0);

	l = gtk_label_new ("Files/Folders to Synchronize:");
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), l, 0, 1, 1, 2,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);
	
	widgets->ff_sync_value = gtk_label_new ("0");
	gtk_misc_set_alignment (GTK_MISC (widgets->ff_sync_value), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), widgets->ff_sync_value, 1, 2, 1, 2,
					  GTK_EXPAND | GTK_FILL, 0, 0, 0);

	l = gtk_label_new ("Automatically Synchronizes Every:");
	gtk_misc_set_alignment (GTK_MISC (l), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), l, 0, 1, 2, 3,
					  GTK_SHRINK | GTK_FILL, 0, 0, 0);

	widgets->sync_interval_value = gtk_label_new ("1 minute");
	gtk_misc_set_alignment (GTK_MISC (widgets->sync_interval_value), 0, 0);
	gtk_table_attach (GTK_TABLE (sync_table), widgets->sync_interval_value, 1, 2, 2, 3,
					  GTK_EXPAND | GTK_FILL, 0, 0, 0);

	right_box = gtk_hbox_new (false, 10);
	gtk_box_pack_end (GTK_BOX (sync_widget_box), right_box, false, false, 0);
	
	sync_now_button = gtk_button_new_with_mnemonic ("Synchronize _Now");
	gtk_box_pack_end (GTK_BOX (right_box), sync_now_button, false, false, 0);
	g_signal_connect (sync_now_button,
					  "clicked",
					  G_CALLBACK (sync_now_button_callback),
					  (gpointer) vbox);

	ifolder_general_property_page_update (vbox, ifolder_id);	

	return vbox;
}

void
ifolder_general_property_page_update (GtkWidget *property_page,
									  const char *ifolder_id)
{
	GeneralPropertyPageWidgets *widgets;
	char *saved_ifolder_id;
	int sync_interval = 0;
	char sync_interval_value[256];
	iFolder *ifolder;
	int rc;
	char markup_text[2048];
	int objects_to_sync;
	iFolderDiskSpace *disk_space = NULL;

	printf ("ifolder_general_property_page_update: %s\n", ifolder_id);
	
	rc = ifolder_get (&ifolder, ifolder_id);
	if (rc != 0)
	{
		printf ("ifolder_get() failed with return code: %d\n", rc);
		return;
	}

	printf ("ifolder_get() succeeded!\n");

	widgets = (GeneralPropertyPageWidgets *)
			  g_object_get_data (G_OBJECT (property_page), "widgets");

printf ("1\n");
	// Set the new ifolder_id
	g_object_set_data_full (G_OBJECT (property_page), "ifolder_id",
							(gpointer) strdup (ifolder_id), ifolder_free_char_data);
	
printf ("2\n");
	if (ifolder->last_sync_time)
	{
printf ("2.1: %s\n", ifolder->last_sync_time);
		gtk_label_set_text (GTK_LABEL (widgets->last_successful_sync),
							ifolder->last_sync_time);
	}
	else
	{
printf ("2.2\n");
		gtk_label_set_text (GTK_LABEL (widgets->last_successful_sync),
							"N/A");
	}
	
printf ("3\n");
	if (ifolder_get_objects_to_sync (&objects_to_sync, ifolder_id) == 0)
	{
		sprintf (markup_text, "%d", objects_to_sync);
		gtk_label_set_text (GTK_LABEL (widgets->ff_sync_value), markup_text);
	}
	else
	{
		gtk_label_set_text (GTK_LABEL (widgets->ff_sync_value), "N/A");
	}

printf ("4\n");
	if (ifolder->sync_interval <= 0)
	{
printf ("4.1\n");
		if (ifolder_get_default_sync_interval (&sync_interval) != 0)
		{
printf ("4.2\n");
			sync_interval = 0;
		}
	}
	else
	{
printf ("4.3\n");
		sync_interval = ifolder->sync_interval;
	}

	if (sync_interval > 0)
	{
printf ("4.4\n");
		sync_interval = sync_interval / 60; // Convert to minutes
	}

printf ("5\n");
	sprintf (sync_interval_value, "%d %s", sync_interval, "minute(s)");
	gtk_label_set_text (GTK_LABEL (widgets->sync_interval_value),
						sync_interval_value);

printf ("6\n");
	if (ifolder->name)
	{
		sprintf (markup_text, "<span weight=\"bold\">%s</span>", ifolder->name);
		gtk_label_set_markup (GTK_LABEL (widgets->name_label), markup_text);
	}
	if (ifolder->owner)
	{
		sprintf (markup_text, "<span size=\"small\">%s</span>", ifolder->owner);
		gtk_label_set_markup (GTK_LABEL (widgets->owner_label), markup_text);
	}
	if (ifolder->unmanaged_path)
	{
		sprintf (markup_text, "<span size=\"small\">%s</span>", ifolder->unmanaged_path);
		gtk_label_set_markup (GTK_LABEL (widgets->location_label), markup_text);
	}
	
	if (ifolder_get_disk_space (&disk_space, ifolder_id) != 0)
		disk_space = NULL;
	
	// Check to see if the current user is the owner of this iFolder
	// and if they are, allow them to set the quota of the iFolder.
	if (ifolder->current_user_id && ifolder->owner_id
		&& strcmp (ifolder->current_user_id, ifolder->owner_id) == 0)
	{
		if (widgets->limit_check_button == NULL)
		{
			widgets->limit_check_button =
				gtk_check_button_new_with_mnemonic ("_Set Quota:");
			g_signal_connect (widgets->limit_check_button,
							  "toggled",
							  G_CALLBACK (limit_check_button_callback),
							  (gpointer) property_page);
			
			gtk_table_attach (GTK_TABLE (widgets->disk_table),
							  widgets->limit_check_button, 0, 1, 1, 2,
							  GTK_SHRINK | GTK_FILL, 0, 0, 0);
			
			widgets->limit_entry = gtk_entry_new ();
			g_signal_connect (widgets->limit_entry,
							  "changed",
							  G_CALLBACK (limit_entry_changed_callback),
							  (gpointer) property_page);
			g_signal_connect (widgets->limit_entry,
							  "activate",
							  G_CALLBACK (limit_entry_activate_callback),
							  (gpointer) property_page);
			g_signal_connect (widgets->limit_entry,
							  "focus-out-event",
							  G_CALLBACK (limit_entry_focus_out_callback),
							  (gpointer) property_page);
			gtk_entry_set_width_chars (GTK_ENTRY (widgets->limit_entry), 6);
			gtk_entry_set_max_length (GTK_ENTRY (widgets->limit_entry), 10);
			gtk_entry_set_alignment (GTK_ENTRY (widgets->limit_entry), 1);
			gtk_table_attach (GTK_TABLE (widgets->disk_table),
							  widgets->limit_entry, 1, 2, 1, 2,
							  GTK_SHRINK | GTK_FILL, 0, 0, 0);
			gtk_widget_show_all (widgets->limit_check_button);
			gtk_widget_show_all (widgets->limit_entry);
		}
		else
		{
			gtk_widget_show (widgets->limit_check_button);
			gtk_widget_show (widgets->limit_entry);
		}
		
		if (widgets->limit_label != NULL)
		{
			gtk_widget_hide (widgets->limit_label);
			gtk_widget_hide (widgets->limit_value);
		}
	}
	else
	{
		if (widgets->limit_label == NULL)
		{
			widgets->limit_label = gtk_label_new ("Quota:");
			gtk_misc_set_alignment (GTK_MISC (widgets->limit_label), 0, 0.5);
			gtk_table_attach (GTK_TABLE (widgets->disk_table),
							  widgets->limit_label, 0, 1, 1, 2,
							  GTK_EXPAND | GTK_FILL, 0, 0, 0);
			
			widgets->limit_value = gtk_label_new ("0");
			gtk_misc_set_alignment (GTK_MISC (widgets->limit_value), 1, 0.5);
			gtk_table_attach (GTK_TABLE (widgets->disk_table),
							  widgets->limit_value, 1, 2, 1, 2,
							  GTK_EXPAND | GTK_FILL, 0, 0, 0);
			
			gtk_widget_show_all (widgets->limit_label);
			gtk_widget_show_all (widgets->limit_value);
		}
		else
		{
			gtk_widget_show (widgets->limit_label);
			gtk_widget_show (widgets->limit_value);
		}
		
		if (widgets->limit_check_button != NULL)
		{
			gtk_widget_hide (widgets->limit_check_button);
			gtk_widget_hide (widgets->limit_entry);
		}
	}
	
	if (disk_space != NULL)
	{
		g_object_set_data_full (G_OBJECT (property_page), "disk_space",
								(gpointer) disk_space, free_ifolder_disk_space);

		int tmp_val;
		
		if (disk_space->limit == 0)
		{
			gtk_widget_set_sensitive (widgets->limit_unit, FALSE);
			gtk_widget_set_sensitive (widgets->avail_label, FALSE);
			gtk_widget_set_sensitive (widgets->avail_value, FALSE);
			gtk_widget_set_sensitive (widgets->avail_unit, FALSE);
			gtk_widget_set_sensitive (widgets->disk_usage_bar, FALSE);
			gtk_widget_set_sensitive (widgets->disk_usage_frame, FALSE);
			gtk_widget_set_sensitive (widgets->disk_usage_full_label, FALSE);
			gtk_widget_set_sensitive (widgets->disk_usage_empty_label, FALSE);
			
			if (widgets->limit_check_button != NULL)
			{
				gtk_toggle_button_set_active (
					GTK_TOGGLE_BUTTON (widgets->limit_check_button),
					FALSE);
				gtk_widget_set_sensitive (widgets->limit_entry, FALSE);
				gtk_entry_set_text (GTK_ENTRY (widgets->limit_entry), "0");
			}
			
			if (widgets->limit_label != NULL)
			{
				gtk_widget_set_sensitive (widgets->limit_label, FALSE);
				gtk_widget_set_sensitive (widgets->limit_value, FALSE);
				gtk_label_set_text (GTK_LABEL (widgets->limit_value), "0");
			}
			
			gtk_label_set_text (GTK_LABEL (widgets->avail_value), "0");
		}
		else
		{
			gtk_widget_set_sensitive (widgets->limit_unit, TRUE);
			gtk_widget_set_sensitive (widgets->avail_label, TRUE);
			gtk_widget_set_sensitive (widgets->avail_value, TRUE);
			gtk_widget_set_sensitive (widgets->avail_unit, TRUE);
			gtk_widget_set_sensitive (widgets->disk_usage_bar, TRUE);
			gtk_widget_set_sensitive (widgets->disk_usage_frame, TRUE);
			gtk_widget_set_sensitive (widgets->disk_usage_full_label, TRUE);
			gtk_widget_set_sensitive (widgets->disk_usage_empty_label, TRUE);
			
			if (widgets->limit_check_button != NULL)
			{
				gtk_toggle_button_set_active (
					GTK_TOGGLE_BUTTON (widgets->limit_check_button),
					TRUE);
				gtk_widget_set_sensitive (widgets->limit_entry, TRUE);
				tmp_val = (int)(disk_space->limit / (1024 * 1024));
				sprintf (markup_text, "%d", tmp_val);
				gtk_entry_set_text (GTK_ENTRY (widgets->limit_entry), markup_text);
			}
			
			if (widgets->limit_label != NULL)
			{
				gtk_widget_set_sensitive (widgets->limit_label, TRUE);
				gtk_widget_set_sensitive (widgets->limit_value, TRUE);
				tmp_val = (int)(disk_space->limit / (1024 * 1024));
				sprintf (markup_text, "%d", tmp_val);
				gtk_label_set_text (GTK_LABEL (widgets->limit_value), markup_text);
			}
			
			tmp_val = (int)(disk_space->available_space / (1024 * 1024));
			sprintf (markup_text, "%d", tmp_val);
			gtk_label_set_text (GTK_LABEL (widgets->avail_value), markup_text);
		}
		
		// FIXME: Update the graph
		set_graph (GTK_PROGRESS_BAR (widgets->disk_usage_bar),
				   disk_space->used_space, disk_space->limit);

		// Add one because there is no iFolder that is zero
		if (disk_space->used_space == 0)
		{
			gtk_label_set_text (GTK_LABEL (widgets->used_value), "0");
		}
		else
		{
			tmp_val = (int)(disk_space->used_space / (1024 * 1024)) + 1;
			sprintf (markup_text, "%d", tmp_val);
			gtk_label_set_text (GTK_LABEL (widgets->used_value), markup_text);
		}
	}

//	if (disk_space)
//		free (disk_space);

	ifolder_free (&ifolder);
}

static
void set_graph (GtkProgressBar *disk_usage_bar,
				long long used_space, long long limit)
{
	if (limit == 0)
	{
		gtk_progress_bar_set_fraction (disk_usage_bar, 0);
		return;
	}
	
	if (limit < used_space)
		gtk_progress_bar_set_fraction (disk_usage_bar, 1);
	else
		gtk_progress_bar_set_fraction (disk_usage_bar,
			((double)used_space / (double)limit));
}

static void
sync_now_button_callback (GtkButton *button, gpointer user_data)
{
	GtkWidget *vbox;
	char *ifolder_id;
	
	vbox = (GtkWidget *)user_data;
	
	ifolder_id = g_object_get_data (G_OBJECT (vbox), "ifolder_id");
	
	printf ("sync_now_button_callback: %s\n", ifolder_id);
	
	if (ifolder_sync_now (ifolder_id) != 0)
	{
		printf ("SynciFolderNow(\"%s\") failed\n", ifolder_id);
	}
}

static void
limit_check_button_callback (GtkToggleButton *toggle_button,
							 gpointer user_data)
{
	GtkWidget *vbox;
	GeneralPropertyPageWidgets *widgets;
	char *ifolder_id;
	iFolderDiskSpace *disk_space;
	long long current_limit;
	
	vbox = (GtkWidget *)user_data;
	
	widgets = (GeneralPropertyPageWidgets *)
				g_object_get_data (G_OBJECT (vbox), "widgets");
	
	if (gtk_toggle_button_get_active (toggle_button))
	{
		gtk_widget_set_sensitive (widgets->limit_unit, TRUE);
		gtk_widget_set_sensitive (widgets->avail_label, TRUE);
		gtk_widget_set_sensitive (widgets->avail_value, TRUE);
		gtk_widget_set_sensitive (widgets->avail_unit, TRUE);
		gtk_widget_set_sensitive (widgets->limit_entry, TRUE);
		gtk_widget_set_sensitive (widgets->disk_usage_bar, TRUE);
		gtk_widget_set_sensitive (widgets->disk_usage_frame, TRUE);
		gtk_widget_set_sensitive (widgets->disk_usage_full_label, TRUE);
		gtk_widget_set_sensitive (widgets->disk_usage_empty_label, TRUE);
	}
	else
	{
		gtk_widget_set_sensitive (widgets->limit_unit, FALSE);
		gtk_widget_set_sensitive (widgets->avail_label, FALSE);
		gtk_widget_set_sensitive (widgets->avail_value, FALSE);
		gtk_widget_set_sensitive (widgets->avail_unit, FALSE);
		gtk_widget_set_sensitive (widgets->limit_entry, FALSE);
		gtk_widget_set_sensitive (widgets->disk_usage_bar, FALSE);
		gtk_widget_set_sensitive (widgets->disk_usage_frame, FALSE);
		gtk_widget_set_sensitive (widgets->disk_usage_full_label, FALSE);
		gtk_widget_set_sensitive (widgets->disk_usage_empty_label, FALSE);
		
		gtk_entry_set_text (GTK_ENTRY (widgets->limit_entry), "0");
		
		// if the current value is not the same as the
		// read value, we need to save the current value
		disk_space = (iFolderDiskSpace *)
					 g_object_get_data (G_OBJECT (vbox), "disk_space");
		
		current_limit = ParseCurrentLimit (gtk_entry_get_text
										   (GTK_ENTRY (widgets->limit_entry)));
		if (current_limit != disk_space->limit)
		{
			// Save the Limit
			printf ("Saving the limit: %lld\n", current_limit);

			ifolder_id = g_object_get_data (G_OBJECT (vbox), "ifolder_id");

			if (ifolder_save_disk_space_limit (ifolder_id, current_limit) != 0)
				printf ("Error saving the disk space limit\n");
		}
	}
}

static long long
ParseCurrentLimit (const gchar *limit_text)
{
	long long size_limit;
	
	if (limit_text == NULL || strlen (limit_text) == 0)
		size_limit = 0;
	else
	{
		size_limit = atol (limit_text);
	}
	
	size_limit = size_limit * 1024 * 1024;
printf ("ParseCurrentLimit() returning: %lld\n", size_limit);
	return size_limit;
}

static void
limit_entry_changed_callback (GtkWidget *entry, gpointer user_data)
{
	GtkWidget *vbox;
	GeneralPropertyPageWidgets *widgets;
	iFolderDiskSpace *disk_space;
	long long current_limit;
	int tmp_val;
	long long result;
	char avail_value_text[2048];
	
	vbox = (GtkWidget *)user_data;
	
	widgets = (GeneralPropertyPageWidgets *)
				g_object_get_data (G_OBJECT (vbox), "widgets");
	disk_space = (iFolderDiskSpace *)
				 g_object_get_data (G_OBJECT (vbox), "disk_space");

	printf ("limit_entry_changed_callback\n");
	
	current_limit = ParseCurrentLimit (gtk_entry_get_text
									   (GTK_ENTRY (widgets->limit_entry)));
	
	if (current_limit == 0)
		gtk_label_set_text (GTK_LABEL (widgets->avail_value), "0");
	else
	{
		result = current_limit - disk_space->used_space;
		if (result < 0)
			tmp_val = 0;
		else
			tmp_val = (int)(result / (1024 * 1024));
		
		sprintf (avail_value_text, "%d", tmp_val);
		gtk_label_set_text (GTK_LABEL (widgets->avail_value),
							avail_value_text);
	}

	set_graph (GTK_PROGRESS_BAR (widgets->disk_usage_bar),
			   disk_space->used_space, current_limit);
}

static void
limit_entry_activate_callback (GtkEntry *entry, gpointer user_data)
{
	GtkWidget *vbox;
	GeneralPropertyPageWidgets *widgets;
	char *ifolder_id;
	long long current_limit;
	
	vbox = (GtkWidget *)user_data;
	
	widgets = (GeneralPropertyPageWidgets *)
				g_object_get_data (G_OBJECT (vbox), "widgets");
	ifolder_id = g_object_get_data (G_OBJECT (vbox), "ifolder_id");

	printf ("limit_entry_activate_callback\n");
	
	current_limit = ParseCurrentLimit (gtk_entry_get_text
									   (GTK_ENTRY (widgets->limit_entry)));
	// Save the Limit
	printf ("Saving the limit: %lld\n", current_limit);

	if (ifolder_save_disk_space_limit (ifolder_id, current_limit) != 0)
		printf ("Error saving the disk space limit\n");
}

static gboolean
limit_entry_focus_out_callback (GtkWidget *widget, GdkEventFocus *event, gpointer user_data)
{
	GtkWidget *vbox;
	GeneralPropertyPageWidgets *widgets;
	char *ifolder_id;
	long long current_limit;
	
	vbox = (GtkWidget *)user_data;
	
	widgets = (GeneralPropertyPageWidgets *)
				g_object_get_data (G_OBJECT (vbox), "widgets");
	ifolder_id = g_object_get_data (G_OBJECT (vbox), "ifolder_id");

	printf ("limit_entry_focus_out_callback\n");
	
	current_limit = ParseCurrentLimit (gtk_entry_get_text
									   (GTK_ENTRY (widgets->limit_entry)));
	// Save the Limit
	printf ("Saving the limit: %lld\n", current_limit);

	if (ifolder_save_disk_space_limit (ifolder_id, current_limit) != 0)
		printf ("Error saving the disk space limit\n");

	return FALSE;
}

static void
free_widgets_struct (gpointer data)
{
	GeneralPropertyPageWidgets *widgets;
	
printf ("free_widgets_struct\n");
	widgets = (GeneralPropertyPageWidgets *)data;
	if (widgets)
		free (widgets);
}

static void
free_ifolder_disk_space (gpointer data)
{
	iFolderDiskSpace *disk_space;
	
printf ("free_ifolder_disk_space\n");
	disk_space = (iFolderDiskSpace *)data;
	if (disk_space)
		free (disk_space);
}

