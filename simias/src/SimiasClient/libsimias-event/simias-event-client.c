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
 
/**
 * Right now, this can be compiled by typing:
 * 
 * 		gcc `xml2-config --libs --cflags` -o event-client simias-event-client.c
 */
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <sys/un.h>
#include <unistd.h>
#include <netdb.h>

#include <libxml/tree.h>
#include <libxml/parser.h>
#include <libxml/xpath.h>
#include <libxml/xpathInternals.h>

#include "simias-event-client.h"

#define WEB_SERVICE_TRUE_STRING		"True"
#define WEB_SERVICE_FALSE_STRING	"False"

#define MAX_PENDING_LISTENS			5

/**
 * Structures to represent 1-level-deep XML
 */
typedef struct
{
	char *host;
	char *port;
} SimiasEventServerConfig;

static char * sec_server_config_elements [] = {
	"Host",
	"Port",
	NULL
};

/* EventRegistration Message */
#define REGISTRATION_TOP_ELEMENT_NAME	"EventRegistration"
#define REGISTRATION_HOST_ATTR_NAME		"host"
#define REGISTRATION_PORT_ATTR_NAME		"port"

/* Event Listener Tags */
#define EVENT_LISTENER_ELEMENT_NAME		"EventListener"
#define EVENT_ELEMENT_NAME				"Event"
#define EVENT_ACTION_ATTR_NAME			"action"


#define SEC_EVENT_TYPE_XPATH "//Event/@type"

static char * sec_node_event_elements [] = {
	"Action",
	"Time",
	"Source",
	"Collection",
	"Type",
	"EventID",
	"Node",
	"Flags",
	"MasterRev",
	"SlaveRev",
	"FileSize",
	NULL
};

static char * sec_collection_sync_event_elements [] = {
	"Name",
	"ID",
	"Action",
	"Successful",
	NULL
};

static char * sec_file_sync_event_elements [] = {
	"CollectionID",
	"ObjectType",
	"Delete",
	"Name",
	"Size",
	"SizeToSync",
	"SizeRemaining",
	"Direction",
	NULL
};

static char * sec_notify_event_elements [] = {
	"Message",
	"Time",
	"Type",
	NULL
};

typedef enum
{
	SIMIAS_OBJECT_SERVER_CONFIG,
	SIMIAS_OBJECT_NODE_EVENT,
	SIMIAS_OBJECT_COLLECTION_SYNC_EVENT,
	SIMIAS_OBJECT_FILE_SYNC_EVENT,
	SIMIAS_OBJECT_NOTIFY_EVENT
} SimiasObjectType;

/* FIXME: Temporary #defines */
#define SIMIAS_EVENT_SERVER_CONFIG_PATH "/home/boyd/.local/share/IProcEvent.cfg"
#define SIMIAS_EVENT_SERVER_HOST "127.0.0.1"
#define SIMIAS_EVENT_SERVER_PORT 5432

/* #region Forward declarations for private functions */
static void * sec_thread (void *user_data);
static void * sec_reg_thread (void *user_data);
static void sec_wait_for_file_change (char *file_path);
static void sec_config_file_changed_callback (void *user_data);
static void sec_shutdown (SimiasEventClient *ec, const char *err_msg);
static void sec_report_error (SimiasEventClient *ec, const char *err_msg);
static int sec_send_message (SimiasEventClient *ec,
							 char * message, int len);
static int sec_get_server_host_address (SimiasEventClient *ec,
										struct sockaddr_in *sin);
static char * sec_get_config_file_path (char *dest_path);

static void * sec_parse_struct_from_doc (xmlDoc *doc);
static char * sec_create_xml_from_struct (const char *top_level_element_name, 
										  char **struct_ptr,
										  char **node_names);
static void * sec_create_struct_from_xpath (xmlXPathContext *xpath_ctx);
/* #endregion */

/* #region Public Functions */
static int
sec_init (SimiasEventClient *ec, void *error_handler)
{
printf ("SEC: sec_init () called\n");	
	/**
	 * The following macro/function initializes the XML library and checks for
	 * potential API mismatches between the version it was compiled for and the
	 * actual shared library being used.
	 */
	LIBXML_TEST_VERSION
	
	/* Create a socket to listen for events from the event server. */
	if ((ec->event_socket = socket (PF_INET, SOCK_STREAM, 0)) < 0) {
		perror ("simias-event-client: listen socket");
		return -1;
	}
	
	/* Create a socket to send messages to the event server. */
	if ((ec->message_socket = socket (PF_INET, SOCK_STREAM, 0)) < 0) {
		perror ("simias-event-client: send socket");
		return -1;
	}

	ec->state = CLIENT_STATE_INITIALIZING;
	ec->reg_thread_state = REG_THREAD_STATE_INITAL;
	
	/* Save the error handling information. */
	ec->error_handler = error_handler;
	
	/* Start the event thread waiting for event messages. */
	if ((pthread_create (&(ec->event_thread), NULL, 
						 sec_thread, ec)) != 0) {
		perror ("simias-event-client: could not start event thread");
		return -1;
	}

	return 0;
}

/**
 * Any cleanup should be done in this function.
 */
static int
sec_cleanup (SimiasEventClient *ec)
{
	/* Cleanup function for the XML library */
	xmlCleanupParser ();
}

static int
sec_register (SimiasEventClient *ec)
{
printf ("SEC: sec_register () called\n");	

	/* Don't let registeration happen multiple times */
	if (ec->reg_thread_state == REG_THREAD_STATE_INITAL) {
		/* Set the states to registering */
		ec->reg_thread_state = REG_THREAD_STATE_INITIALIZING;
		
		/* Start a thread which will process the registration request */
		if ((pthread_create (&(ec->reg_thread), NULL,
							 sec_reg_thread, ec)) != 0) {
			perror ("simias-event-client: could not start registration thread");
			return -1;
		}
		
		if (pthread_join (ec->reg_thread, NULL) != 0) {
			perror ("simias-event-client: could not call pthread_join on the registration thread");
			return -1;
		}
	} else {
printf ("SEC: sec_register: could not start thread\n");	
	}
	
	return 0;
}

static int
sec_deregister (SimiasEventClient *ec)
{
	char reg_msg [4096];
	struct sockaddr_in my_sin;
	int my_sin_addr_len;
	char addr_str [32];
	char port_str [32];

	if (ec->state == CLIENT_STATE_RUNNING) {

		my_sin_addr_len = sizeof (struct sockaddr_in);
		if (getsockname (ec->event_socket, 
						 (struct sockaddr *)&my_sin, 
						 &my_sin_addr_len) != 0) {
			perror ("simias-event-client: getsockname");
			return -1;
		}
		
		sprintf (addr_str, "%s", inet_ntoa (my_sin.sin_addr));
		sprintf (port_str, "%d", my_sin.sin_port);

		/* Format the XML for the de-registration message */
		sprintf (reg_msg, 
			"<%s %s=\"%s\" %s=\"%s\">%s</%s>",
			REGISTRATION_TOP_ELEMENT_NAME,
			REGISTRATION_HOST_ATTR_NAME,
			addr_str,
			REGISTRATION_PORT_ATTR_NAME,
			port_str,
			WEB_SERVICE_FALSE_STRING,
			REGISTRATION_TOP_ELEMENT_NAME);
		
		/* Send de-registration message */
		if (sec_send_message (ec, reg_msg, strlen (reg_msg)) <= 0) {
			/* FIXME: Handle error...no data sent */
			perror ("simias-event-client send de-registration message");
		}
	}
	
	sec_shutdown (ec, NULL);
	ec->error_handler = NULL;
	
	return 0;
}

static int
sec_set_event (SimiasEventClient *ec, IPROC_EVENT_ACTION action, void **handler)
{
	char msg [4096];
	char action_str [256];
	
	/* FIXME: Implement sec_set_event */
	switch (action) {
		case ACTION_ADD_NODE_CREATED:
			sprintf (action_str, "AddNodeCreated");
			break;
		case ACTION_ADD_NODE_CHANGED:
			sprintf (action_str, "AddNodeChanged");
			break;
		case ACTION_ADD_NODE_DELETED:
			sprintf (action_str, "AddNodeDeleted");
			break;
		case ACTION_ADD_COLLECTION_SYNC:
			sprintf (action_str, "AddCollectionSync");
			break;
		case ACTION_ADD_FILE_SYNC:
			sprintf (action_str, "AddFileSync");
			break;
		case ACTION_ADD_NOTIFY_MESSAGE:
			sprintf (action_str, "AddNotifyMessage");
			break;
		case ACTION_REMOVE_NODE_CREATED:
			sprintf (action_str, "RemoveNodeCreated");
			break;
		case ACTION_REMOVE_NODE_CHANGED:
			sprintf (action_str, "RemoveNodeChanged");
			break;
		case ACTION_REMOVE_NODE_DELETED:
			sprintf (action_str, "RemoveNodeDeleted");
			break;
		case ACTION_REMOVE_COLLECTION_SYNC:
			sprintf (action_str, "RemoveCollectionSync");
			break;
		case ACTION_REMOVE_FILE_SYNC:
			sprintf (action_str, "RemoveFileSync");
			break;
		case ACTION_REMOVE_NOTIFY_MESSAGE:
			sprintf (action_str, "RemoveNotifyMessage");
			break;
		default:
			/* Don't know what the user is talking about */
			return -1;
	}
	
	sprintf (msg, 
		"<%s><%s %s=\"%s\" /></%s>",
		EVENT_LISTENER_ELEMENT_NAME,
		EVENT_ELEMENT_NAME,
		EVENT_ACTION_ATTR_NAME,
		action_str,
		EVENT_LISTENER_ELEMENT_NAME);

	/* Send registration message */
	if (sec_send_message (ec, msg, strlen (msg)) <= 0) {
		/* FIXME: Handle error...no data sent */
		perror ("simias-event-client send registration message");
	}
	
	return 0;
}
/* #endregion */

/* #region Private Functions */
static void *
sec_thread (void *user_data)
{
	SimiasEventClient *ec = (SimiasEventClient *)user_data;
	int new_s, sin_size, len;
	char my_host_name [512];
	struct hostent *hp;
	char err_msg [2048];
	char buf [256];
	
printf ("SEC: sec_thread () called\n");	
	
	sprintf (my_host_name, "127.0.0.1");
	
	hp = gethostbyname (my_host_name);
	if (!hp) {
		perror ("simias-event-client (server): gethostbyname");
		sprintf (err_msg, "gethostbyname (\"%s\") failed in sec_thread ()", 
																my_host_name);
		sec_shutdown (ec, err_msg);
		return NULL;
	}

	ec->local_sin.sin_family = AF_INET;
	bcopy (hp->h_addr, (char *)&(ec->local_sin.sin_addr), hp->h_length);
	ec->local_sin.sin_port = 0;	/* Use any unused port at random */
	
	if (bind (ec->event_socket,
			  (struct sockaddr *)&(ec->local_sin),
			  sizeof (ec->local_sin)) != 0) {
		perror ("simias-event-client (server): bind");
		sprintf (err_msg, "bind () failed in sec_thread ()");
		sec_shutdown (ec, err_msg);
		return NULL;
	}

printf ("SEC: sec_thread: bind () complete\n");	
	
	if (listen (ec->event_socket, MAX_PENDING_LISTENS) != 0) {
		perror ("simias-event-client (server): listen");
		sprintf (err_msg, "listen () failed in sec_thread ()");
		sec_shutdown (ec, err_msg);
		return NULL;
	}

printf ("SEC: sec_thread: listen () called\n");	

	ec->state = CLIENT_STATE_RUNNING;
	
	sin_size = sizeof (struct sockaddr_in);

	while (ec->state != CLIENT_STATE_SHUTDOWN)
	{
		new_s = accept (ec->event_socket, 
						(struct sockaddr *)&(ec->local_sin), 
						&sin_size);
printf ("SEC: sec_thread: accept () called\n");	
		if (new_s < 0) {
			perror ("simias-event-client (server): accept");
			sprintf (err_msg, "accept () failed in sec_thread ()");
			sec_shutdown (ec, err_msg);
			return NULL;
		}
		
		while (len = recv (new_s, buf, sizeof (buf), 0)) {
printf ("SEC: sec_thread: recv () called\n");	
			fputs (buf, stdout);
		}
				
		close (new_s);
	}
}

static void *
sec_reg_thread (void *user_data)
{
	/* FIXME: Implement sec_reg_thread */
	SimiasEventClient *ec = (SimiasEventClient *)user_data;
	struct sockaddr_in sin;
	struct sockaddr_in my_sin;
	int my_sin_addr_len;
	char reg_msg [4096];
	char ip_addr [128];
	int b_connected = 0;
	char addr_str [32];
	char port_str [32];
	
printf ("SEC: sec_reg_thread () called\n");	

	/**
	 * Wait until the event client thread is running before continuing.  If we
	 * don't wait, we will not be able to know what port the client is listening
	 * on for messages back from the server.
	 */
	while (ec->state != CLIENT_STATE_RUNNING) {

printf ("SEC: sec_reg_thread: waiting for client to start\n");	

		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			return NULL;
		}

		/* FIXME: Use something else besides a sleep () here */
		sleep (1);
	}
	
	my_sin_addr_len = sizeof (struct sockaddr_in);
	if (getsockname (ec->event_socket, 
					 (struct sockaddr *)&my_sin, 
					 &my_sin_addr_len) != 0) {
		perror ("simias-event-client: getsockname");
		return NULL;
	}
	
	sprintf (addr_str, "%s", inet_ntoa (my_sin.sin_addr));
	sprintf (port_str, "%d", my_sin.sin_port);
	fprintf (stderr, "Client listening on %s:%s\n", addr_str, port_str);
	
	/* Stay in this loop until we connect */
	while (!b_connected && (ec->state != CLIENT_STATE_SHUTDOWN)) {
		if ((sec_get_server_host_address (ec, &sin)) != 0) {
			/* FIXME: Handle error */
		}
		
		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			/* FIXME: Figure out what to do here */
			return;
		}
		
		/* Connect to the server */
		if (connect (ec->message_socket, 
					 (struct sockaddr *)&sin, 
					 sizeof (sin)) == 0) {
			b_connected = 1;

			ec->reg_thread_state = REG_THREAD_STATE_RUNNING;
			
			/* Format the XML for registration message */
			sprintf (reg_msg, 
				"<%s %s=\"%s\" %s=\"%s\">%s</%s>",
				REGISTRATION_TOP_ELEMENT_NAME,
				REGISTRATION_HOST_ATTR_NAME,
				addr_str,
				REGISTRATION_PORT_ATTR_NAME,
				port_str,
				WEB_SERVICE_TRUE_STRING,
				REGISTRATION_TOP_ELEMENT_NAME);
			
			/* Send registration message */
			if (sec_send_message (ec, reg_msg, strlen (reg_msg)) <= 0) {
				/* FIXME: Handle error...no data sent */
				perror ("simias-event-client send registration message");
			}
		} else {
			/* FIXME: Handle the error here */
			perror ("simias-event-client connect");
			
			sleep (2);
		}
	}
	
	ec->reg_thread_state = REG_THREAD_STATE_TERMINATED;
}

static void
sec_wait_for_file_change (char *file_path)
{
	/* FIXME: Implement this with polling using the stat () call so that we are portable on Linux, Windows, and Mac */
}

static void
sec_config_file_changed_callback (void *user_data)
{
}

static void
sec_shutdown (SimiasEventClient *ec, const char *err_msg)
{
	/* FIXME: Figure out how to lock on *ec */
	if (ec->state != CLIENT_STATE_SHUTDOWN) {
		ec->state = CLIENT_STATE_SHUTDOWN;
		
		/* Signal the registration thread to shutdown */
		if (ec->reg_thread_state == REG_THREAD_STATE_INITIALIZING) {
			/* FIXME: Signal a shutdown */
		}
		
		/* Close the socket if it is still open */
		if (ec->event_socket >= 0) {
			close (ec->event_socket);
		}
		
		/* Close the socket if it is still open */
		if (ec->message_socket >= 0) {
			close (ec->message_socket);
		}
	}
	
	/* Inform the application if an error occurred */
	if (err_msg != NULL) {
		sec_report_error (ec, err_msg);
	}
}

/**
 * Sends an error message to the application's error handler if one was
 * specified.
 */
static void
sec_report_error (SimiasEventClient *ec, const char *err_msg)
{
	if (ec->error_handler != NULL) {
		ec->error_handler ((char *)err_msg);
	}
}

/**
 * Returns the number of bytes that were sent or -1 if there were errors.
 */
static int
sec_send_message (SimiasEventClient *ec, char * message, int len)
{
	int sent_length;
	char err_msg [2048];
	void *real_message;
	
	/* Cannot send if we haven't registered */
	if (ec->reg_thread_state == REG_THREAD_STATE_INITIALIZING) {
		fprintf (stderr, "Cannot send message to server.  Registration hasn't completed.\n");
		return 0;
	}
	
	real_message = (void *)malloc (len + 4);
	if (!real_message) {
		fprintf (stderr, "Out of memory\n");
		return 0;
	}
	
	*((int *)real_message) = len;
	sprintf (real_message + 4, "%s", message);
	printf ("Sending: %s\n", real_message + 4);
	
	sent_length = send (ec->message_socket, real_message, len + 4, 0);
	
	free (real_message);
	
	if (sent_length == -1) {
		sprintf (err_msg,
				 "Failed to send message to server.  Socket error: %s",
				 strerror (errno));
		sec_shutdown (ec, err_msg);
	} else {
		printf ("Just sent %d bytes to the server\n", sent_length);
	}
	
	return (int) sent_length;
}

/**
 * Initializes the sockaddr_in structure to contain the host where the
 * event service is running.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int
sec_get_server_host_address (SimiasEventClient *ec, 
											 struct sockaddr_in *sin)
{
	char config_file_path [1024];
	struct stat file_stat;
	char err_msg [2048];
	int b_addr_read = 0;
	xmlDoc *doc;
	SimiasEventServerConfig *server_config;
	
	/* See if the local application data directory has been created */
	if ((sec_get_config_file_path (config_file_path)) == NULL) {
		sec_shutdown (ec, "Could not get the config file path");
		return -1;
	}

	if ((stat (config_file_path, &file_stat)) != 0) {
		if (errno == ENOENT) {	/* A component of the config_file_path doesn't exist */
			/* FIXME: Create the directories needed for the file */
		} else {
			/* Some other bad error happened */
			sprintf (err_msg,
					 "Could not stat config file.  Real error: %s",
					 strerror (errno));
			sec_shutdown (ec, err_msg);
			return -1;
		}
	}
	
	/**
	 * Wait until the service is listening.  Then get the contents of the
	 * configuration file.  If the server happens to be writing to the file
	 * while this process is reading, an error will occur and this process will
	 * try again after a short wait interval.
	 */
	while (!b_addr_read) {
		if ((stat (config_file_path, &file_stat)) == 0) {
			/* Attempt to read the XML config file. */
			doc = xmlReadFile (config_file_path, NULL, 0);
			if (doc != NULL) {

				server_config = 
					sec_parse_struct_from_doc (doc);
				if (server_config == NULL) {
					goto HANDLE_XML_READ_CONFIG_ERROR;
				}

				/* Fill out the sockaddr_in structure */				
				bzero ((char *)sin, sizeof (struct sockaddr_in));
				sin->sin_family = AF_INET;
				sin->sin_addr.s_addr = 
					inet_addr (((SimiasEventServerConfig *)server_config)->host);
				sin->sin_port = 
					htons (atoi (((SimiasEventServerConfig *)server_config)->port));
					
				b_addr_read = 1;
				
				free (server_config);
				xmlFreeDoc (doc);
			} else {
				/* Extenuating circumstances allow for this goto :) */
				HANDLE_XML_READ_CONFIG_ERROR:

				/* Wait and then try again if the service hasn't been shutdown */
				sec_wait_for_file_change (config_file_path);
				if (ec->state == CLIENT_STATE_SHUTDOWN) {
					break; /* out of the while loop */
				}
			}
		} else {
			if (errno != ENOENT) {
				/* The call to stat () returned something worse than a file not existing */
				sprintf (err_msg,
						 "Could not stat config file.  Real error: %s",
						 strerror (errno));
				sec_shutdown (ec, err_msg);
				return -1;
			}
			
			/* Wait and then try again if the service hasn't been shutdown */
			sec_wait_for_file_change (config_file_path);
			if (ec->state == CLIENT_STATE_SHUTDOWN) {
				break; /* out of the while loop */
			}
		}
	}
	
	if (ec->state == CLIENT_STATE_SHUTDOWN) {
		return -1;
	}
	
	return 0;
}

static char *
sec_get_config_file_path (char *dest_path)
{
	/* FIXME: Implement sec_get_config_file_path */
	return strcpy (dest_path, SIMIAS_EVENT_SERVER_CONFIG_PATH);
}

static void *
sec_parse_struct_from_doc (xmlDoc *doc)
{
	xmlXPathContext	*xpath_ctx;
	xmlXPathObject	*xpath_obj;

	void *return_struct = NULL;

	/* Create xpath evaluation context */
	xpath_ctx = xmlXPathNewContext (doc);
	if (xpath_ctx == NULL) {
		fprintf (stderr, "Unable to create a new XPath context\n");
		return NULL;
	}

	return_struct = sec_create_struct_from_xpath (xpath_ctx);
												   
	/* Free the global variables that may have been allocated by the parser */
	xmlCleanupParser ();
	
	return return_struct;
}

/**
 * Get the strings stored in the XML by parsing out the data with XPath
 * expressions
 */
static void *
sec_create_struct_from_xpath (xmlXPathContext *xpath_ctx)
{
	void *data_struct = NULL;
	char **element_names = NULL;
	char *element_name;
	char xpath_expr [256];
	char **struct_ptr = NULL;
	xmlXPathObject *xpath_obj;
	xmlNodeSet *node_set;
	xmlChar *event_type;	/* unsigned char */
	xmlNode *cur_node;
	xmlChar *cur_node_val;
	int num_of_nodes;
	int i, struct_pos;
	
	struct_pos = 0;

	/* Look for the type attribute of the Event tag */
	xpath_obj = xmlXPathEvalExpression (SEC_EVENT_TYPE_XPATH, xpath_ctx);
	if (xpath_obj->nodesetval == NULL) {
		/* Must be dealing with Server Config (host & port) */
		data_struct = (SimiasEventServerConfig *)
						malloc (sizeof (SimiasEventServerConfig));
		element_names = sec_server_config_elements;
		
		struct_ptr = (char **)data_struct;
	} else {
		/* Determine the type */
		node_set = xpath_obj->nodesetval;
		num_of_nodes = (node_set) ? node_set->nodeNr : 0;
		if (num_of_nodes != 1) {
			fprintf (stderr, "Number of attributes on Event/@type nodes is not one (1): %d\n", num_of_nodes);
			xmlFree (xpath_obj);
			return NULL;
		}
		cur_node = node_set->nodeTab [0];
		if (cur_node->type != XML_ATTRIBUTE_NODE) {
			fprintf (stderr, "XPath of \"%s\" did not return an XML_ATTRIBUTE_NODE\n", SEC_EVENT_TYPE_XPATH);
			xmlFree (xpath_obj);
			return NULL;
		}
		
		/* Get the event type attribute value */
		event_type = xmlNodeGetContent (cur_node);
		if (event_type == NULL) {
			fprintf (stderr, "Could not get the value of Event/@type\n");
			xmlFree (xpath_obj);
			return NULL;
		}
		
		fprintf (stdout, "Parsing event from XML: %s\n", event_type);
		if (!strcmp ("NodeEventArgs", event_type)) {
			data_struct = (SimiasNodeEvent *)
							malloc (sizeof (SimiasNodeEvent));
			element_names = sec_node_event_elements;
		} else if (!strcmp ("CollectionSyncEventArgs", event_type)) {
			data_struct = (SimiasCollectionSyncEvent *)
							malloc (sizeof (SimiasCollectionSyncEvent));
			element_names = sec_collection_sync_event_elements;
		} else if (!strcmp ("FileSyncEventArgs", event_type)) {
			data_struct = (SimiasFileSyncEvent *)
							malloc (sizeof (SimiasFileSyncEvent));
			element_names = sec_file_sync_event_elements;
		} else if (!strcmp ("NotifyEventArgs", event_type)) {
			data_struct = (SimiasNotifyEvent *)
							malloc (sizeof (SimiasNotifyEvent));
			element_names = sec_notify_event_elements;
		}
		
		/**
		 * Save type off.  Since "event_type" is always the first element of
		 * all the "event" structures, save it off and increment the struct_pos..
		 */
		struct_ptr = (char **)data_struct;
		struct_ptr [struct_pos] = strdup (event_type);
		struct_pos++;

		xmlFree (event_type);
		xmlFree (xpath_obj);
	}
		
	/* Set element_name to the first element in element_names */
	element_name = element_names [0];

	/* Loop through the element_names and set the data in the data_struct */	
	for (i = 0; element_name; i++) {
		/* Construct the expression */
		sprintf (xpath_expr, "//%s", element_name);
		
		/* Evaluate the expression */
		xpath_obj = xmlXPathEvalExpression (xpath_expr, xpath_ctx);
		if (xpath_obj == NULL) {
			fprintf (stderr, "Unable to evaluate XPath expression: \"%s\"\n", xpath_expr);
			return NULL;
		}
		
		node_set = xpath_obj->nodesetval;
		num_of_nodes = (node_set) ? node_set->nodeNr : 0;
		
		if (num_of_nodes != 1) {
			fprintf (stderr, 
					 "Number of nodes found with XPath expression \"%s\" (expected only 1): %d\n",
					 xpath_expr,
					 num_of_nodes);
			xmlFree (xpath_obj);
			return NULL;
		}
		
		/* Should be a XML_ELEMENT_NODE */
		cur_node = node_set->nodeTab [0];
		
		if (cur_node->type != XML_ELEMENT_NODE) {
			fprintf (stderr, "Not XML_ELEMENT_NODE: %s\n", cur_node->name);
			xmlFree (xpath_obj);
			return NULL;
		}

		cur_node_val = xmlNodeGetContent (cur_node);
		if (cur_node_val == NULL) {
			fprintf (stderr, "Element contained no data: %s\n", cur_node->name);
			xmlFree (xpath_obj);
			return NULL;
		}

		struct_ptr [struct_pos] = strdup (cur_node_val);
		struct_pos++;
		
		xmlFree (cur_node_val);
		xmlFree (xpath_obj);

		/* Advance to the next XPath expression */
		element_name = element_names [i + 1];
	}
	
	return data_struct;
}

/**
 * The returned char * must be freed by the caller.
 */
static char *
sec_create_xml_from_struct (const char *top_level_element_name,
							char **struct_ptr, 
							char **node_names)
{
	/* FIXME: Find a better way to create the XML than just a very large char buffer */
	char xml_buf [4096];
	char element_tag [128];
	int i;
	
	/* Start the top-level element */
	sprintf (element_tag, "<%s>", top_level_element_name);
	strcpy (xml_buf, element_tag);
	
	/* Add the child elements */
	for (i = 0; node_names [i]; i++) {
		sprintf (element_tag, "<%s>", node_names [i]);
		
		strcat (xml_buf, struct_ptr [i]);
		
		sprintf (element_tag, "</%s>", node_names [i]);
	}
	
	/* Close the top-level element */
	sprintf (element_tag, "</%s>", top_level_element_name);
	strcat (xml_buf, element_tag);
	
	return strdup (xml_buf);
}
/* #endregion */

/* #region Testing */
int main (int argc, char *argv[])
{
	SimiasEventClient ec;
	char buf [256];
	
	if (sec_init (&ec, NULL) != 0) {
		fprintf (stderr, "sec_init failed\n");
		return -1;
	}
	
	if (sec_register (&ec) != 0) {
		fprintf (stderr, "sec_register failed\n");
		return -1;
	}
	
	printf ("Registration complete\n");
	
	/* Ask to listen to some events by calling sec_set_event () */
	sec_set_event (&ec, ACTION_ADD_NODE_CREATED, NULL);
	sec_set_event (&ec, ACTION_ADD_NODE_CHANGED, NULL);
	sec_set_event (&ec, ACTION_ADD_NODE_DELETED, NULL);
	sec_set_event (&ec, ACTION_ADD_COLLECTION_SYNC, NULL);
	sec_set_event (&ec, ACTION_ADD_FILE_SYNC, NULL);
	sec_set_event (&ec, ACTION_ADD_NOTIFY_MESSAGE, NULL);
	
	fprintf (stdout, "Press <Enter> to stop the client...");
	fgets (buf, sizeof (buf), stdin);
	
	if (sec_deregister (&ec) != 0) {
		fprintf (stderr, "sec_deregister failed\n");
		return -1;
	}

	if (sec_cleanup (&ec) != 0) {
		fprintf (stderr, "sec_cleanup failed\n");
		return -1;
	}
	
	return 0;
}
