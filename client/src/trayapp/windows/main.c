
#include <gtk/gtk.h>

#include <ifolder-client.h>
#include "../linux/gnome-applet/preferences-window.h"
#include "../linux/gnome-applet/main-window.h"
#include "../linux/gnome-applet/util.h"

iFolderClient *ifolder_client = NULL;

static void showMainWindow( GtkWidget *widget, gpointer data )
{
	ifa_show_main_window();
}

static void showPreferences( GtkWidget *widget, gpointer data )
{
	ifa_show_preferences_window(0);
}

static gboolean delete_event( GtkWidget *widget,
                              GdkEvent  *event,
                              gpointer   data )
{
    /* If you return FALSE in the "delete_event" signal handler,
     * GTK will emit the "destroy" signal. Returning TRUE means
     * you don't want the window to be destroyed.
     * This is useful for popping up 'are you sure you want to quit?'
     * type dialogs. */

    g_print ("delete event occurred\n");

    /* Change TRUE to FALSE and the main window will be destroyed with
     * a "delete_event". */

    return FALSE;
}

/* Another callback */
static void destroy( GtkWidget *widget,
                     gpointer   data )
{
    gtk_main_quit ();
}

int main( int   argc,
          char *argv[] )
{
	GError *err = NULL;

    /* GtkWidget is the storage type for widgets */
    GtkWidget *window;
    GtkWidget *prefsButton, *quitButton, *mainButton;

	g_thread_init(NULL);

    gtk_init (&argc, &argv);

	if (argc < 2)
	{
		g_message("FIXME: Determine what the default config directory should be");
		ifolder_client = ifolder_client_initialize(NULL, &err);
	}
	else
	{
		g_message("FIXME: Validate that argv[1] is a valid directory...or at least that the syntax is correct.");
		ifolder_client = ifolder_client_initialize(argv[1], &err);
	}

    /* create a new window */
    window = gtk_window_new (GTK_WINDOW_TOPLEVEL);

	gtk_window_set_title(GTK_WINDOW(window), "iFolder 2.5 Client");
    
    g_signal_connect (G_OBJECT (window), "delete-event",
		      G_CALLBACK (delete_event), NULL);
    
    g_signal_connect (G_OBJECT (window), "destroy",
		      G_CALLBACK (destroy), NULL);
    
    /* Sets the border width of the window. */
    gtk_container_set_border_width (GTK_CONTAINER (window), 10);
    
	/* Creates a new button with the label "Hello World". */
	mainButton = gtk_button_new_with_label ("Main");

	g_signal_connect (G_OBJECT (mainButton), "clicked",
			  G_CALLBACK (showMainWindow), NULL);

	/* Creates a new button with the label "Hello World". */
    prefsButton = gtk_button_new_with_label ("Preferences");
    
    g_signal_connect (G_OBJECT (prefsButton), "clicked",
		      G_CALLBACK (showPreferences), NULL);

    /* Creates a new button with the label "Hello World". */
    quitButton = gtk_button_new_with_label ("Quit");
    
    g_signal_connect (G_OBJECT (quitButton), "clicked",
					  G_CALLBACK (showPreferences), NULL);

	g_signal_connect_swapped (G_OBJECT (quitButton), "clicked",
							  G_CALLBACK (gtk_widget_destroy),
							  G_OBJECT (window));

	GtkWidget *winBox = gtk_vbox_new(false, 7);
	gtk_container_set_border_width(GTK_CONTAINER(winBox), 7);

	gtk_window_set_position(GTK_WINDOW(window), GTK_WIN_POS_CENTER);
	gtk_container_add(GTK_CONTAINER(window), winBox);
	
	gtk_box_pack_start(GTK_BOX(winBox), mainButton, true, true, 0);
	gtk_box_pack_start(GTK_BOX(winBox), prefsButton, true, true, 0);
	gtk_box_pack_start(GTK_BOX(winBox), quitButton, true, true, 0);

    gtk_widget_show_all (window);

    gtk_main ();

	ifolder_client_uninitialize(ifolder_client, &err);

    return 0;
}

void
ifa_quit_ifolder ()
{
    gtk_main_quit ();
}
