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

/**
 * Internal structures
 */
/* Tags used to get the host and port number in the IProcConfig.cfg file. */
#define HOST_TAG "Host"
#define PORT_TAG "Port"

/**
 * Error returned when the Registration thread terminates with a receive
 * outstanding.
 */
#define OWNER_THREAD_TERMINATED 995

/* File name of the IProcEvent configuration file */
#define CONFIG_FILE_NAME "IProcEvent.cfg"

#define RECEIVE_BUFFER_SIZE 2048

typedef enum
{
	CLIENT_STATE_INITIALIZING,
	CLIENT_STATE_REGISTERING,
	CLIENT_STATE_RUNNING,
	CLIENT_STATE_SHUTDOWN
} CLIENT_STATE;

typedef enum
{
	REG_THREAD_STATE_INITIALIZING,
	REG_THREAD_STATE_RUNNING,
	REG_THREAD_STATE_TERMINATED,
	REG_THREAD_STATE_TERMINATION_ACK
} REG_THREAD_STATE;

/**
 * This structure represents the SimiasEventClient typedef declared in the
 * public API.  It is abstracted so the end-user of the API doesn't have to know
 * all the internals of what goes on.  This should make the API more clear.
 */
typedef struct
{
	/* Initialization state of the client */
	CLIENT_STATE state;
	
	/* Socket used to listen for events from the event server */
	int event_socket;
	
	/* Buffer used to receive the socket messages */
	char receive_buffer [RECEIVE_BUFFER_SIZE];
	
	/* Indicates the current amount of data in the buffer to be processed */
	int buffer_length;
	
	/* Contains the address and port information for this client */
	struct sockaddr_in local_sin;

	/* Delegate and context used to indicate an error */	
	void (*error_handler)	(void *user_data);
	
	/**
	 * Tells the state of the registration thread.  This is a work around for
	 * the error that gets thrown when the registration thread exits with a
	 * pending async receive.
	 */
	REG_THREAD_STATE reg_thread_state;

	/* Thread handle to the event thread */
	pthread_t event_thread;
	
	/* Thread handle to the registration thread */
	pthread_t reg_thread;
} RealSimiasEventClient;


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

/* FIXME: Move this into the RealSimiasEventClient structure somehow */
void (*simias_node_created_function) (SimiasNodeEvent *);
void (*simias_node_changed_function) (SimiasNodeEvent *);
void (*simias_node_deleted_function) (SimiasNodeEvent *);

/* #region Forward declarations for private functions */
static void * sec_thread (void *user_data);
static void * sec_reg_thread (void *user_data);
static void sec_wait_for_file_change (char *file_path);
static void sec_config_file_changed_callback (void *user_data);
static void sec_shutdown (RealSimiasEventClient *ec, const char *err_msg);
static void sec_report_error (RealSimiasEventClient *ec, const char *err_msg);
static int sec_send_message (RealSimiasEventClient *ec,
							 char * message, int len);
static int sec_get_server_host_address (RealSimiasEventClient *ec,
										struct sockaddr_in *sin);
static char * sec_get_config_file_path (char *dest_path);

static void * sec_parse_struct_from_doc (xmlDoc *doc);
static void * sec_create_struct_from_xpath (xmlXPathContext *xpath_ctx);
static int sec_process_message (RealSimiasEventClient *ec, 
								char *message, 
								int length);
/* Anytime an event struct is returned, it must be freed using this function. */
static void sec_free_event_struct (void *event_struct);
/* #endregion */

/* #region Public Functions */
int
sec_init (SimiasEventClient *sec, void *error_handler)
{
printf ("SEC: sec_init () called\n");
	RealSimiasEventClient *ec = malloc (sizeof (RealSimiasEventClient));
	*sec = ec;
	/**
	 * The following macro/function initializes the XML library and checks for
	 * potential API mismatches between the version it was compiled for and the
	 * actual shared library being used.
	 */
	LIBXML_TEST_VERSION
	
	/* Create a socket to communicate with the event server on */
	if ((ec->event_socket = socket (PF_INET, SOCK_STREAM, 0)) < 0) {
		perror ("simias-event-client: client event socket");
		return -1;
	}
	
	ec->state = CLIENT_STATE_INITIALIZING;
	
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
int
sec_cleanup (SimiasEventClient *sec)
{
	/* Free the memory being used by SimiasEventClient */
	free (*sec);
	*sec = NULL;
	
	/* Cleanup function for the XML library */
	xmlCleanupParser ();
}

int
sec_register (SimiasEventClient sec)
{
printf ("SEC: sec_register () called\n");	
	RealSimiasEventClient *ec = (RealSimiasEventClient *)sec;
	/* Don't let registeration happen multiple times */
	if (ec->state == CLIENT_STATE_INITIALIZING) {
		/* Set the states to registering */
		ec->state = CLIENT_STATE_REGISTERING;
		ec->reg_thread_state = REG_THREAD_STATE_INITIALIZING;
		
		/* Start a thread which will process the registration request */
		if ((pthread_create (&(ec->reg_thread), NULL,
							 sec_reg_thread, ec)) != 0) {
			perror ("simias-event-client: could not start registration thread");
			return -1;
		}
	} 
	
	return 0;
}

int
sec_deregister (SimiasEventClient sec)
{
	RealSimiasEventClient *ec = (RealSimiasEventClient *)sec;
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

int
sec_set_event (SimiasEventClient sec, IPROC_EVENT_ACTION action, void (*handler)(void *))
{
	RealSimiasEventClient *ec = (RealSimiasEventClient *)sec;
	char msg [4096];
	char action_str [256];
	
	/* FIXME: Implement sec_set_event */
	switch (action) {
		case ACTION_ADD_NODE_CREATED:
			sprintf (action_str, "AddNodeCreated");
			simias_node_created_function = handler;
			break;
		case ACTION_ADD_NODE_CHANGED:
			sprintf (action_str, "AddNodeChanged");
			simias_node_changed_function = handler;
			break;
		case ACTION_ADD_NODE_DELETED:
			sprintf (action_str, "AddNodeDeleted");
			simias_node_deleted_function = handler;
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
	RealSimiasEventClient *ec = (RealSimiasEventClient *)user_data;
	int new_s, sin_size, len, real_length;
	char my_host_name [512];
	struct hostent *hp;
	char err_msg [2048];
	char buf [4096];
	char * real_message = NULL;
	
printf ("SEC: sec_thread () called\n");	

	/* Wait until the register thread marks us as registered before continuing */
	while (ec->state != CLIENT_STATE_RUNNING) {
		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			return NULL;
		}
		printf ("SEC: sec_thread () waiting for register thread to complete\n");
		sleep (2);
	}
	
	while (ec->state != CLIENT_STATE_SHUTDOWN)
	{
		while (len = recv (ec->event_socket, buf, sizeof (buf), 0)) {
printf ("SEC: sec_thread: recv () called\n");
			real_length = *((int *)buf);
			printf ("real_length: %d\n", real_length);
			if (real_length > 0 && (real_length == (len - 4))) {
				real_message = malloc (sizeof (char) * real_length);
				
				strncpy (real_message, buf + 4, real_length);
				
				printf ("Message received:\n\n%s\n\n", real_message);
				
				sec_process_message (ec, real_message, real_length);
				
				free (real_message);
			} else {
				printf ("SEC: recv () returned %d bytes\n", len);
			}
		}
	}
}

static void *
sec_reg_thread (void *user_data)
{
	/* FIXME: Implement sec_reg_thread */
	RealSimiasEventClient *ec = (RealSimiasEventClient *)user_data;
	struct sockaddr_in sin;
	struct sockaddr_in my_sin;
	int my_sin_addr_len;
	char reg_msg [4096];
	char ip_addr [128];
	int b_connected = 0;
	char addr_str [32];
	char port_str [32];
	char config_file_path [1024];
	
printf ("SEC: sec_reg_thread () called\n");	

	/* Stay in this loop until we connect */
	while (!b_connected && (ec->state != CLIENT_STATE_SHUTDOWN)) {
		if ((sec_get_server_host_address (ec, &sin)) != 0) {
			/* FIXME: Handle error */
		}
		
		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			return;
		}
		
		/* Connect to the server */
		if (connect (ec->event_socket, 
					 (struct sockaddr *)&sin, 
					 sizeof (sin)) == 0) {
			b_connected = 1;

			/* Determine what port the client is listening on (implicit bind) */
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

			ec->state = CLIENT_STATE_RUNNING;
		} else {
			/* FIXME: Handle the error here */
			perror ("simias-event-client connect");
			
			/**
			 * See if this is a case of the server not listening on the socket
			 * anymore.  It may have gone down hard and left the configuration
			 * file with an invalid socket.  Keep watching the file until the
			 * socket changes and it can be connected.
			 */
			if ((sec_get_config_file_path (config_file_path)) == NULL) {
				sec_shutdown (ec, "Could not get the config file path");
				return NULL;
			}

			/* This is a blocking call */			
			sec_wait_for_file_change (config_file_path);
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
sec_shutdown (RealSimiasEventClient *ec, const char *err_msg)
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
sec_report_error (RealSimiasEventClient *ec, const char *err_msg)
{
	if (ec->error_handler != NULL) {
		ec->error_handler ((char *)err_msg);
	}
}

/**
 * Returns the number of bytes that were sent or -1 if there were errors.
 */
static int
sec_send_message (RealSimiasEventClient *ec, char * message, int len)
{
	int sent_length;
	char err_msg [2048];
	void *real_message;
	
	real_message = (void *)malloc (len + 4);
	if (!real_message) {
		fprintf (stderr, "Out of memory\n");
		return 0;
	}
	
	*((int *)real_message) = len;
	sprintf (real_message + 4, "%s", message);
	printf ("Sending: %s\n", real_message + 4);
	
	sent_length = send (ec->event_socket, real_message, len + 4, 0);
	
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
sec_get_server_host_address (RealSimiasEventClient *ec, 
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
			xmlInitParser ();
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
					inet_addr (server_config->host);
				sin->sin_port = 
					htons (atoi (server_config->port));
					
				b_addr_read = 1;
				
				free (server_config->host);
				free (server_config->port);
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

static int
sec_process_message (RealSimiasEventClient *ec, char *message, int length)
{
	xmlDoc *doc;
	void *message_struct;
	char **struct_ptr;

	/* Construct an xmlDoc from the message */	
	xmlInitParser ();
	doc = xmlReadMemory (message, length, "message.xml", NULL, 0);
	if (doc != NULL) {
		message_struct = sec_parse_struct_from_doc (doc);
		if (message_struct == NULL) {
			fprintf (stderr, "SEC: Struct couldn't be parsed from message\n");
			return -1;
		}
		
		struct_ptr = (char **)message_struct;
		
		/* Call the right handler based on the type of message received */
		if (strcmp ("NodeEventArgs", struct_ptr [0]) == 0) {
			SimiasNodeEvent *event = (SimiasNodeEvent *)message_struct;
			printf ("NodeEventArgs received\n");

			if (strcmp ("NodeCreated", event->action) == 0) {
				if (simias_node_created_function != NULL)
					simias_node_created_function (event);
			} else if (strcmp ("NodeChanged", event->action) == 0) {
				if (simias_node_changed_function != NULL)
					simias_node_changed_function (event);
			} else if (strcmp ("NodeDeleted", event->action) == 0) {
				if (simias_node_deleted_function != NULL)
					simias_node_deleted_function (event);
			}
		} else if (strcmp ("CollectionSyncEventArgs", struct_ptr [0]) == 0) {
			printf ("CollectionSyncEventArgs message received\n");
		} else if (strcmp ("FileSyncEventArgs", struct_ptr [0]) == 0) {
			printf ("FileSyncEventArgs message received\n");
		} else if (strcmp ("NotifyEventArgs", struct_ptr [0]) == 0) {
			printf ("NotifyEventArgs message received\n");
		}
		
		sec_free_event_struct (message_struct);
		xmlFreeDoc (doc);
	} else {
		fprintf (stderr, "SEC: Invalid XML received from event server\n");
		return -1;
	}
	
	return 0;
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

static void
sec_free_event_struct (void *event_struct)
{
	char **element_names = NULL;
	char *element_name = NULL;
	char **struct_ptr;
	int i, struct_pos;

	struct_ptr = (char **)event_struct;

	/* Determine the type of struct we're dealing with */
	if (!strcmp ("NodeEventArgs", struct_ptr [0])) {
printf ("SEC: Freeing NodeEventArgs\n");
		element_names = sec_node_event_elements;
	} else if (!strcmp ("CollectionSyncEventArgs", struct_ptr [0])) {
printf ("SEC: Freeing CollectionSyncEventArgs\n");
		element_names = sec_collection_sync_event_elements;
	} else if (!strcmp ("FileSyncEventArgs", struct_ptr [0])) {
printf ("SEC: Freeing FileSyncEventArgs\n");
		element_names = sec_file_sync_event_elements;
	} else if (!strcmp ("NotifyEventArgs", struct_ptr [0])) {
printf ("SEC: Freeing NotifyEventArgs\n");
		element_names = sec_notify_event_elements;
	} else {
printf ("SEC: Freeing unknown event type (memory leak possible)\n");
		free (event_struct);
		return;
	}
	
	element_name = element_names [0];
	
	for (i = 0; element_name; i++) {
		free (struct_ptr [i]);

		/* Advance to the next XPath expression */
		element_name = element_names [i + 1];
	}
	
	free (event_struct);
}
/* #endregion */

/* #region Testing */

void
print_simias_node_event (SimiasNodeEvent *event)
{
	printf ("\t%s: %s\n", "action", event->action);
	printf ("\t%s: %s\n", "time", event->time);
	printf ("\t%s: %s\n", "source", event->source);
	printf ("\t%s: %s\n", "collection", event->collection);
	printf ("\t%s: %s\n", "type", event->type);
	printf ("\t%s: %s\n", "event_id", event->event_id);
	printf ("\t%s: %s\n", "node", event->node);
	printf ("\t%s: %s\n", "flags", event->flags);
	printf ("\t%s: %s\n", "master_rev", event->master_rev);
	printf ("\t%s: %s\n", "slave_rev", event->slave_rev);
	printf ("\t%s: %s\n", "file_size", event->file_size);
}

void 
simias_node_created_callback (SimiasNodeEvent *event)
{
	printf ("simias_node_created_callback () entered\n");
	
	print_simias_node_event (event);
}

void
simias_node_changed_callback (SimiasNodeEvent *event)
{
	printf ("simias_node_changed_callback () entered\n");
	
	print_simias_node_event (event);
}

void
simias_node_deleted_callback (SimiasNodeEvent *event)
{
	printf ("simias_node_deleted_callback () entered\n");
	
	print_simias_node_event (event);
}

int
main (int argc, char *argv[])
{
	SimiasEventClient ec;
	char buf [256];
	
	if (sec_init (&ec, NULL) != 0) {
		fprintf (stderr, "sec_init failed\n");
		return -1;
	}
	
	if (sec_register (ec) != 0) {
		fprintf (stderr, "sec_register failed\n");
		return -1;
	}
	
	printf ("Registration complete\n");
	
	/**
	 * Until the message queue is implemented, we need to wait for the client
	 * to be running before continuing
	 */
	while (((RealSimiasEventClient *)ec)->state != CLIENT_STATE_RUNNING) {
		if (((RealSimiasEventClient *)ec)->state == CLIENT_STATE_SHUTDOWN) {
			fprintf (stderr, "Shutdown initiated prematurely\n");
			return -1;
		}
		
		fprintf (stdout, "Test code: Waiting for client to be running\n");
		sleep (1);
	}
	
	/* Ask to listen to some events by calling sec_set_event () */
	sec_set_event (ec, ACTION_ADD_NODE_CREATED, (void *)&simias_node_created_callback);
	sec_set_event (ec, ACTION_ADD_NODE_CHANGED, (void *)&simias_node_changed_callback);
	sec_set_event (ec, ACTION_ADD_NODE_DELETED, (void *)&simias_node_deleted_callback);
	sec_set_event (ec, ACTION_ADD_COLLECTION_SYNC, NULL);
	sec_set_event (ec, ACTION_ADD_FILE_SYNC, NULL);
	sec_set_event (ec, ACTION_ADD_NOTIFY_MESSAGE, NULL);
	
	fprintf (stdout, "Press <Enter> to stop the client...");
	fgets (buf, sizeof (buf), stdin);
	
	if (sec_deregister (ec) != 0) {
		fprintf (stderr, "sec_deregister failed\n");
		return -1;
	}

	if (sec_cleanup (&ec) != 0) {
		fprintf (stderr, "sec_cleanup failed\n");
		return -1;
	}
	
	return 0;
}
