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
 * To compile the stand-alone test, compile the program with:
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
#include <sys/ipc.h>
#include <sys/sem.h>

#include <libxml/tree.h>
#include <libxml/parser.h>
#include <libxml/xpath.h>
#include <libxml/xpathInternals.h>

#include "simias-event-client.h"

/* Turn this on to see debug messages */
#ifdef DEBUG
#define DEBUG_SEC(args) (printf("sec: "), printf args)
#else
#define DEBUG_SEC
#endif

#define WEB_SERVICE_TRUE_STRING		"True"
#define WEB_SERVICE_FALSE_STRING	"False"

#ifndef DARWIN
#if defined(__GNU_LIBRARY__) && !defined(_SEM_SEMUN_UNDEFINED)
/* union semun is defined by including <sys/sem.h> */
#else
/* according to X/OPEN we have to define it ourselves */
union semun {
		int val;
		struct semid_ds *buf;
		unsigned short *array;
		struct seminfo *__buf;
};
#endif
#endif /* DARWIN */

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

/* Tags used for (un)subscribing to events */
#define ACTION_ADD_NODE_CREATED			"AddNodeCreated"
#define ACTION_ADD_NODE_CHANGED			"AddNodeChanged"
#define ACTION_ADD_NODE_DELETED			"AddNodeDeleted"
#define ACTION_ADD_COLLECTION_SYNC		"AddCollectionSync"
#define ACTION_ADD_FILE_SYNC			"AddFileSync"
#define ACTION_ADD_NOTIFY_MESSAGE		"AddNotifyMessage"
#define ACTION_REMOVE_NODE_CREATED		"RemoveNodeCreated"
#define ACTION_REMOVE_NODE_CHANGED		"RemoveNodeChanged"
#define ACTION_REMOVE_NODE_DELETED		"RemoveNodeDeleted"
#define ACTION_REMOVE_COLLECTION_SYNC	"RemoveCollectionSync"
#define ACTION_REMOVE_FILE_SYNC			"RemoveFileSync"
#define ACTION_REMOVE_NOTIFY_MESSAGE	"RemoveNotifyMessage"

/* Tags used for (un)setting event filters */
#define EVENT_FILTER_NAME			"Filter"
#define EVENT_FILTER_TYPE_ATTR_NAME	"type"
#define EVENT_FILTER_COLLECTION_STR	"Collection"
#define EVENT_FILTER_NODE_ID_STR	"NodeID"
#define EVENT_FILTER_NODE_TYPE_STR	"NodeType"

/* File name of the IProcEvent configuration file */
#define CONFIG_FILE_NAME "IProcEvent.cfg"

#define RECEIVE_BUFFER_SIZE 512

#define NUM_OF_ACTION_TYPES 6

typedef enum
{
	CLIENT_STATE_INITIALIZING,	/* Used also if client is reconnecting */
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

typedef struct _SimiasEventFuncInfo
{
	SimiasEventFunc		function;
	void *				data;
	struct _SimiasEventFuncInfo *next;
} SimiasEventFuncInfo;

typedef struct _SimiasEventMessage
{
	char *message;
	int length;
	struct _SimiasEventMessage *next;
} SimiasEventMessage;

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
	
	/* Indicates the current amount of data in the buffer to be processed */
	int buffer_length;
	
	/* Contains the address and port information for this client */
	struct sockaddr_in local_sin;

	/**
	 * Callback function for lettng the consumer know about state changes and
	 * errors.
	 */
	SECStateEventFunc state_event_func;
	void *state_event_data;
	
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
	
	/* Thread handle to the process message thread */
	pthread_t process_message_thread;
	
	/** 
	 * Array of SimiasEventFunc pointers stored in a linked-list and indexed by
	 * IPROC_EVENT_ACTION.
	 */
	SimiasEventFuncInfo *event_handlers [NUM_OF_ACTION_TYPES];
	
	/**
	 * Items needed for the tail queue of received messages.
	 */
	SimiasEventMessage *received_messages;
	SimiasEventMessage *last_received_message;
	int received_messages_mutex;	/* Semaphore to protect the message list */
	int received_messages_sem;		/* Semaphore to signal a received message */
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

/* #region Forward declarations for private functions */
static void * sec_thread (void *user_data);
static void * sec_reg_thread (void *user_data);
static void * sec_proc_msg_thread (void *user_data);
static void sec_wait_for_file_change (char *file_path);
static void sec_config_file_changed_callback (void *user_data);
static void sec_shutdown (RealSimiasEventClient *ec, const char *err_msg);
static void sec_report_error (RealSimiasEventClient *ec, const char *err_msg);
static int sec_send_message (RealSimiasEventClient *ec,
							 char * message, int len);
static int sec_get_server_host_address (RealSimiasEventClient *ec,
										struct sockaddr_in *sin);
static char * sec_get_user_profile_dir_path (char *dest_path);
static char * sec_get_config_file_path (char *dest_path);

static void * sec_parse_struct_from_doc (xmlDoc *doc);
static void * sec_create_struct_from_xpath (xmlXPathContext *xpath_ctx);
static int sec_process_message (RealSimiasEventClient *ec, 
								char *message, 
								int length);
static int sec_add_event_handler (RealSimiasEventClient *ec,
								  IPROC_EVENT_ACTION action,
								  SimiasEventFunc function,
								  void * data);
static int sec_remove_event_handler (RealSimiasEventClient *ec,
									 IPROC_EVENT_ACTION action,
									 SimiasEventFunc function);
static int sec_notify_event_handlers (RealSimiasEventClient *ec,
									  IPROC_EVENT_ACTION action,
									  void *event);
static int sec_remove_all_event_handlers (RealSimiasEventClient *ec,
										  IPROC_EVENT_ACTION action);
const char * sec_get_node_type_str (SIMIAS_NODE_TYPE type);

/* Anytime an event struct is returned, it must be freed using this function. */
static void sec_free_event_struct (void *event_struct);

/* #endregion */

/* #region Public Functions */
int sec_init (SimiasEventClient *sec,
			  SECStateEventFunc state_event_func,
			  void *state_event_data)
{
	int i;
	RealSimiasEventClient *ec;
	union semun arg;
	struct sembuf sb = {0, -1, 0};
	key_t received_messages_mutex_key;
	key_t received_messages_sem_key;
	char user_profile_dir [1024];

	ec = malloc (sizeof (RealSimiasEventClient));
	DEBUG_SEC (("sec_init () called\n"));
	memset (ec, '\0', sizeof (RealSimiasEventClient));
	*sec = ec;
	/**
	 * The following macro/function initializes the XML library and checks for
	 * potential API mismatches between the version it was compiled for and the
	 * actual shared library being used.
	 */
	LIBXML_TEST_VERSION
	
	/* NULL out all the linked list of event handler functions */
	for (i = 0; i < NUM_OF_ACTION_TYPES; i++) {
		ec->event_handlers [i] = NULL;
	}
	
	/* moved code to create ec->event_socket into the sec_registerthread */

	
	DEBUG_SEC (("ec->event_socket: %d\n", ec->event_socket));
	
	ec->state = CLIENT_STATE_INITIALIZING;
	
	/* Get received_messages_mutex ready */
	if (sec_get_user_profile_dir_path (user_profile_dir) == NULL) {
		DEBUG_SEC (("Couldn't get config path information\n"));
		return -1;
	}
	
	if ((received_messages_mutex_key = ftok (user_profile_dir, 'M')) == -1) {
		perror ("sec: ftok failed on mutex key");
		return -1;
	}
	if ((ec->received_messages_mutex = 
			semget(received_messages_mutex_key, 1, IPC_CREAT | 0666)) == -1) {
		perror ("sec: semget failed received_messages_mutex");
		return -1;
	}
	
	/* Initialize received_messages_mutex to 1 */
	arg.val = 1;
	if (semctl (ec->received_messages_mutex, 0, SETVAL, arg) == -1) {
		perror ("sec: semctl failed to initialize received_messages_mutex");
		return -1;
	}
	
	/* Get received_messages_sem ready */
	if ((received_messages_sem_key = ftok (user_profile_dir, 'S')) == -1) {
		perror ("sec: ftok failed on mutex key");
		return -1;
	}
	if ((ec->received_messages_sem = 
			semget(received_messages_sem_key, 1, IPC_CREAT | 0666)) == -1) {
		perror ("sec: semget failed received_messages_sem");
		return -1;
	}
	
	/* Initialize received_messages_sem to 0 */
	arg.val = 0;
	if (semctl (ec->received_messages_sem, 0, SETVAL, arg) == -1) {
		perror ("sec: semctl failed to initialize received_messages_sem");
		return -1;
	}

	if ((pthread_create (&(ec->process_message_thread), NULL,
						 sec_proc_msg_thread, ec)) != 0) {
		perror ("sec: could not start process message thread");
		return -1;
	}
	
	/* Save the error handling information. */
	ec->state_event_func = state_event_func;
	ec->state_event_data = state_event_data;
	
	/* Start the event thread waiting for event messages. */
	if ((pthread_create (&(ec->event_thread), NULL, 
						 sec_thread, ec)) != 0) {
		perror ("sec: could not start event thread");
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
	int i;
	union semun arg;
	RealSimiasEventClient *ec = (RealSimiasEventClient *)*sec;
	
	/**
	 * Cleanup the memory used by the link lists storing event handler function
	 * pointers
	 */
	for (i = 0; i < NUM_OF_ACTION_TYPES; i++) {
		if (sec_remove_all_event_handlers (ec, i) != 0) {
			DEBUG_SEC (("sec_cleanup: Error calling sec_remove_all_event_handlers (%d)\n", i));
		}
	}
	
	/* Free the memory being used by SimiasEventClient */
	free (*sec);
	*sec = NULL;
	
	/* Cleanup function for the XML library */
	xmlCleanupParser ();
}

int
sec_register (SimiasEventClient sec)
{
	DEBUG_SEC (("sec_register () called\n"));
	RealSimiasEventClient *ec = (RealSimiasEventClient *)sec;
	/* Don't let registeration happen multiple times */
	if (ec->state == CLIENT_STATE_INITIALIZING) {
		/* Set the states to registering */
		ec->state = CLIENT_STATE_REGISTERING;
		ec->reg_thread_state = REG_THREAD_STATE_INITIALIZING;
		
		/* Start a thread which will process the registration request */
		if ((pthread_create (&(ec->reg_thread), NULL,
							 sec_reg_thread, ec)) != 0) {
			perror ("sec: could not start registration thread");
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
			perror ("sec: getsockname");
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
			perror ("sec: send de-registration message");
		}
	}
	
	sec_shutdown (ec, NULL);
	
	return 0;
}

int
sec_set_event (SimiasEventClient sec, 
			   IPROC_EVENT_ACTION action,
			   bool subscribe,
			   SimiasEventFunc function,
			   void *data)
{
	RealSimiasEventClient *ec = (RealSimiasEventClient *)sec;
	char msg [1024];
	char action_str [256];
	
	switch (action) {
		case ACTION_NODE_CREATED:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_NODE_CREATED : ACTION_REMOVE_NODE_CREATED);
			break;
		case ACTION_NODE_CHANGED:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_NODE_CHANGED : ACTION_REMOVE_NODE_CHANGED);
			break;
		case ACTION_NODE_DELETED:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_NODE_DELETED : ACTION_REMOVE_NODE_DELETED);
			break;
		case ACTION_COLLECTION_SYNC:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_COLLECTION_SYNC : ACTION_REMOVE_COLLECTION_SYNC);
			break;
		case ACTION_FILE_SYNC:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_FILE_SYNC : ACTION_REMOVE_FILE_SYNC);
			break;
		case ACTION_NOTIFY_MESSAGE:
			sprintf (action_str, subscribe ? 
				ACTION_ADD_NOTIFY_MESSAGE : ACTION_REMOVE_NOTIFY_MESSAGE);
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

	/* Send set_event message */
	if (sec_send_message (ec, msg, strlen (msg)) <= 0) {
		/* FIXME: Handle error...no data sent */
		perror ("sec: send set_event message");
	} else {
		if (subscribe) {
			/* Store the event handler function */
			if (sec_add_event_handler (ec, action, function, data) != 0) {
				DEBUG_SEC (("Couldn't add event handler function\n"));
				return -1;
			}
		} else {
			/* Remove the event handler function(s) */
			if (sec_remove_event_handler (ec, action, function) != 0) {
				DEBUG_SEC (("Couldn't remove event handler function(s)\n"));
				return -1;
			}
		}
	}
	
	return 0;
}

int
sec_set_filter (SimiasEventClient sec, SimiasEventFilter *filter)
{
	RealSimiasEventClient *ec;
	char msg [1024];
	char filter_type_str [256];
	const char *filter_data;
	DEBUG_SEC (("sec_set_filter () entered\n"));
	
	ec = (RealSimiasEventClient *)sec;
	
	switch (filter->type) {
		case EVENT_FILTER_COLLECTION:
			sprintf (filter_type_str, EVENT_FILTER_COLLECTION_STR);
			filter_data = (char *)filter->data;
			break;
		case EVENT_FILTER_NODE_ID:
			sprintf (filter_type_str, EVENT_FILTER_NODE_ID_STR);
			filter_data = (char *)filter->data;
			break;
		case EVENT_FILTER_NODE_TYPE:
			sprintf (filter_type_str, EVENT_FILTER_NODE_TYPE_STR);
			filter_data = 
				sec_get_node_type_str (*((SIMIAS_NODE_TYPE *)filter->data));
			break;
		default:
			/* Don't know what the user is talking about */
			return -1;
	}
	
	if (filter->data) {
		/* Set the filter */
		sprintf (msg,
				 "<%s><%s %s=\"%s\">%s</%s></%s>",
				 EVENT_LISTENER_ELEMENT_NAME,
				 EVENT_FILTER_NAME,
				 EVENT_FILTER_TYPE_ATTR_NAME,
				 filter_type_str,
				 filter_data,
				 EVENT_FILTER_NAME,
				 EVENT_LISTENER_ELEMENT_NAME);
	} else {
		/* Unset the filter */
		sprintf (msg,
				 "<%s><%s %s=\"%s\" /></%s>",
				 EVENT_LISTENER_ELEMENT_NAME,
				 EVENT_FILTER_NAME,
				 EVENT_FILTER_TYPE_ATTR_NAME,
				 filter_type_str,
				 EVENT_LISTENER_ELEMENT_NAME);
	}

	/* Send set_event message */
	if (sec_send_message (ec, msg, strlen (msg)) <= 0) {
		/* FIXME: Handle error...no data sent */
		perror ("sec: send set_filter message");
	}
	
	return 0;
}
/* #endregion */

/* #region Private Functions */
static void *
sec_thread (void *user_data)
{
	RealSimiasEventClient *ec = (RealSimiasEventClient *)user_data;
	char recv_buf [RECEIVE_BUFFER_SIZE];
	char *buffer;
	char *temp_buffer;
	SimiasEventMessage *message;
	int bytes_received;
	int buffer_length;
	int stated_message_length;
	int message_length;
	int bytes_to_process;
	int buffer_index;
	bool b_first_read;
	struct sembuf sb = {0, -1, 0};
	
	DEBUG_SEC (("sec_thread () called\n"));
	
	b_first_read = true;
	
	/* Wait until the register thread marks us as registered before continuing */
	while (ec->state != CLIENT_STATE_RUNNING) {
		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			return NULL;
		}
		DEBUG_SEC (("sec_thread () waiting for register thread to complete\n"));
		sleep (2);
	}
	
	buffer = NULL;
	buffer_length = 0;

	while ((bytes_received = recv (ec->event_socket, 
								   recv_buf, RECEIVE_BUFFER_SIZE, 0)) > 0) {

//printf ("\n=====Dumping %d bytes received:=====\n", bytes_received);
//int i;
//for (i = 0; i < bytes_received; i++) {
//	putchar (recv_buf [i]);
//}
//printf ("\n=====SEC: recv(): Dump complete=====\n\n");

		if (buffer == NULL) {
			DEBUG_SEC (("recv (): Creating new buffer of %d bytes\n", bytes_received));
			buffer = malloc (bytes_received);
			if (!buffer) {
				DEBUG_SEC (("Out of memory!\n"));
				break;
			}
			
			memcpy (buffer, recv_buf, bytes_received);
			buffer_length = bytes_received;
			
//printf ("\n-----Dumping %d bytes added to new buffer:-----\n", bytes_received);
//for (i = 0; i < bytes_received; i++) {
//	putchar (buffer [i + 4]);
//}
//printf ("\n-----SEC: recv(): Dump complete-----\n\n");
		} else {
			DEBUG_SEC (("recv (): Adding %d bytes to existing buffer of %d bytes\n", bytes_received, buffer_length));
			temp_buffer = malloc (bytes_received + buffer_length);
			if (!temp_buffer) {
				DEBUG_SEC (("Out of memory!\n"));
				break;
			}
			
			/* Concatenate the old buffer bytes with the new bytes */
			memcpy (temp_buffer, buffer, buffer_length);
			memcpy (temp_buffer + buffer_length, recv_buf, bytes_received);
			
			/* Free the memory from the old buffer */
			free (buffer);
			buffer = temp_buffer;
			buffer_length += bytes_received;
		}
		
		/**
		 * Once the code reaches here, buffer and buffer_length represent all
		 * the data we've received up to this point.
		 */
		bytes_to_process = buffer_length;
		buffer_index = 0;
		while (bytes_to_process > 0) {
			/* There needs to be at least 4 bytes for the stated message length */
			if (bytes_to_process >= 4) {
				/* Get the length of the message.  Add in the prepended length. */
				stated_message_length = *((int *)(buffer + buffer_index));

//if (stated_message_length == 0) {
//	printf ("SEC: recv (): Stated message length cannot be 0!\n");
//	break;
//} else {
//	printf ("SEC: recv (): Stated message length = %d\n", stated_message_length);				
//}
				
				message_length = stated_message_length + 4;

				/* See if the entire message is inside the buffer */
				if (bytes_to_process >= message_length) {
					message = malloc (sizeof (SimiasEventMessage));
					
					/* Process the message received from the Event Server */
					message->message = malloc (sizeof (char) * (stated_message_length + 1));
					message->message [stated_message_length] = '\0';
					memcpy (message->message, 
							 &(buffer [buffer_index + 4]), 
							 stated_message_length);
					message->length = stated_message_length;
					message->next = NULL;
					
					/**
					 * Make sure we have a write lock on the received_messages
					 * list so that we're the only one that modifies it.
					 */
					if (!b_first_read) {
						sb.sem_op = -1;
						if (semop (ec->received_messages_mutex, &sb, 1) == -1) {
							perror ("sec: error getting control of semaphore");
							break;
						}
					} else {
						b_first_read = false;
					}
					
					if (ec->last_received_message == NULL) {
						/* No messages in the queue */
						ec->received_messages = ec->last_received_message = message;
					} else {
						ec->last_received_message->next = message;
						ec->last_received_message = message;
					}
					
					/* Release the semaphore lock */
					sb.sem_op = 1;
					if (semop (ec->received_messages_mutex, &sb, 1) == -1) {
						perror ("sec: error releasing control of semaphore");
						break;
					}
					
					/**
					 * Increment the received_messages_sem to denote there's
					 * another message available in received_messages.
					 */
					sb.sem_op = 1;
					if (semop (ec->received_messages_sem, &sb, 1) == -1) {
						perror ("sec: error incrementing message count with semaphore");
						break;
					}

//printf ("\n*****Dumping message to send to process_message:*****\n");
//for (i = 0; i < stated_message_length; i++) {
//	putchar (message [i]);
//}
//printf ("\n*****Dump complete*****\n\n");

//					sec_process_message (ec, message, stated_message_length);
//					free (message);

					/* Update the buffer_index */
					buffer_index += message_length;
					bytes_to_process -= message_length;					
				} else {
					/* Cannot process the message until we receive more data */
					break;
				}
			} else {
				break;
			}
		}
		
		/* Update the buffer to only contain data that still needs processing */
		if (bytes_to_process == 0) {
			DEBUG_SEC (("recv (): All bytes in buffer processed.\n"));
			/* The buffer is empty and should be freed and nulled */
			free (buffer);
			buffer = NULL;
			buffer_length = 0;
		} else {
			DEBUG_SEC (("recv (): %d bytes in buffer remain.\n", bytes_to_process));
			/* A new buffer needs to be setup to store the remaining bytes */
			temp_buffer = malloc (bytes_to_process);
			memcpy (temp_buffer, buffer + buffer_index, bytes_to_process);
			free (buffer);
			buffer = temp_buffer;
			buffer_length = bytes_to_process;
		}
	}

	/**
	 * If len == -1, there was some type of socket error.
	 * If len == 0, the Simias Event Server has properly shutdown.
	 * 
	 * In either case, we need to put the client into a reconnect state so
	 * that the client can reconnect with the server when it becomes
	 * available again.
	 */
	DEBUG_SEC (("*=*=*=*=*=*= RECONNECT REQUIRED: recv () error =*=*=*=*=*=*\n"));
	if (sec_reconnect (ec) != 0) {
		sec_shutdown (ec, "Could not reconnect the Simias Event Client");
	}
}
		
static void *
sec_reg_thread (void *user_data)
{
	RealSimiasEventClient *ec = (RealSimiasEventClient *)user_data;
	struct sockaddr_in sin;
	struct sockaddr_in my_sin;
	int my_sin_addr_len;
	char reg_msg [4096];
	char ip_addr [128];
	bool b_connected = false;
	char addr_str [32];
	char port_str [32];
	char config_file_path [1024];
	
	DEBUG_SEC (("sec_reg_thread () called\n"));

	/* Stay in this loop until we connect */
	while (!b_connected && (ec->state != CLIENT_STATE_SHUTDOWN)) {
		if ((sec_get_server_host_address (ec, &sin)) != 0) {
			/* FIXME: Handle error */
		}
		
		if (ec->state == CLIENT_STATE_SHUTDOWN) {
			return;
		}
		
		/* Create a socket to communicate with the event server on */
		if ((ec->event_socket = socket (PF_INET, SOCK_STREAM, 0)) < 0) {
			perror ("sec: client event socket");
			return NULL;
		}
		
		/* Connect to the server */
		if (connect (ec->event_socket, 
					 (struct sockaddr *)&sin, 
					 sizeof (struct sockaddr)) == 0) {
			/* Determine what port the client is listening on (implicit bind) */
			my_sin_addr_len = sizeof (struct sockaddr_in);
			if (getsockname (ec->event_socket, 
							 (struct sockaddr *)&my_sin, 
							 &my_sin_addr_len) != 0) {
				perror ("sec: getsockname");
				return NULL;
			}
			
			sprintf (addr_str, "%s", inet_ntoa (my_sin.sin_addr));
			sprintf (port_str, "%d", my_sin.sin_port);
			DEBUG_SEC (("Client listening on %s:%s\n", addr_str, port_str));

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
				perror ("sec: send registration message");
			} else {
				ec->state = CLIENT_STATE_RUNNING;
				b_connected = true;
	
				/**
				 * Notify the state_event_function that we are now connected to
				 * the Simias Event Server.
				 */
				if (ec->state_event_func) {
					ec->state_event_func (SEC_STATE_EVENT_CONNECTED, 
										  NULL,
										  ec->state_event_data);
				}
			}
		} else {
		
			/* Close the old socket if it is still open */
			if (ec->event_socket) {
				close (ec->event_socket);
				ec->event_socket = 0;
			}
			
			/* FIXME: Handle the error here */
			perror ("sec: connect");
			
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

static void *
sec_proc_msg_thread (void *user_data)
{
//	struct sembuf sops;
	SimiasEventMessage *message;
	struct sembuf sb = {0, -1, 0};
	RealSimiasEventClient *ec = (RealSimiasEventClient *)user_data;
	
	DEBUG_SEC (("sec_proc_msg_thread () entered\n"));
	
//	sops.sem_num = 1;
//	sops.sem_op
	
	while (ec->state != CLIENT_STATE_SHUTDOWN) {
		/* Don't do anything unless there's a message (resource) available */
		DEBUG_SEC (("proc_msg_thread: Waiting for signal of available message\n"));
		sb.sem_op = -1;
		if (semop (ec->received_messages_sem, &sb, 1) == -1) {
			perror ("sec: proc_msg_thread: error getting control of received_messages_sem");
			continue;
		}
		
		DEBUG_SEC (("proc_msg_thread: signal of available message received\n"));
		
		/* Get control of the received messages semaphore */
		sb.sem_op = -1;
		if (semop (ec->received_messages_mutex, &sb, 1) == -1) {
			perror ("sec: proc_msg_thread: error getting control of received_messages_mutex");
			continue;
		}
					
		DEBUG_SEC (("proc_msg_thread: got lock on received_messages\n"));
		
		/* If there's a received message remove it from the list */
		if (ec->received_messages == NULL) {
			/* FIXME: Release the semaphore */
			sb.sem_op = 1;
			if (semop (ec->received_messages_mutex, &sb, 1) == -1) {
				perror ("sec: proc_msg_thread: error releasing control of received_messages_mutex");
				continue;
			}
		
			continue;
		}

		/* There's at least one message in the queue */
		message = ec->received_messages;
		ec->received_messages = ec->received_messages->next;
		
		/* Make sure last_received_message is correct if list is now empty */
		if (ec->received_messages == NULL) {
			ec->last_received_message = NULL;
		}
		
		/* FIXME: Release the semaphore */
		sb.sem_op = 1;
		if (semop (ec->received_messages_mutex, &sb, 1) == -1) {
			perror ("sec: proc_msg_thread: error releasing control of received_messages_mutex");
			continue;
		}
		
		DEBUG_SEC (("proc_msg_thread: released lock on received_messages\n"));
		
		/* Process the message */
		sec_process_message (ec, message->message, message->length);
		
		/* Free the memory used by the message */
		free (message->message);
		free (message);
	}

	DEBUG_SEC (("sec_proc_msg_thread () exiting\n"));
}

int
sec_reconnect (RealSimiasEventClient *ec)
{
	DEBUG_SEC (("sec_reconnect () called\n"));
	/**
	 * Prevent this from executing more than once if the recv () and the send ()
	 * happen to try to start this up at the same time.
	 */
	if (ec->state == CLIENT_STATE_INITIALIZING) {
		return 0;	/* Prevent the second caller from doing anything */
	}

	/**
	 * Set the state to CLIENT_STATE_INITIALIZING so that anyone else who tries
	 * to call this function will not actually perform a reconnect more than
	 * once.
	 */
	ec->state = CLIENT_STATE_INITIALIZING;

	/* Close the old socket if it is still open */
	if (ec->event_socket) {
		close (ec->event_socket);
	}

	/**
	 * Notify the state_event_function that we are now disconnected from the
	 * Simias Event Server.
	 */
	if (ec->state_event_func) {
		ec->state_event_func (SEC_STATE_EVENT_DISCONNECTED, 
							  NULL,
							  ec->state_event_data);
	}
	
	/* Create a new socket to communicate with the event server on */
	if ((ec->event_socket = socket (PF_INET, SOCK_STREAM, 0)) < 0) {
		perror ("sec: client event socket");
		return -1;
	}
	
	/* Start the event thread waiting for event messages. */
	if ((pthread_create (&(ec->event_thread), NULL, 
						 sec_thread, ec)) != 0) {
		perror ("sec: reconnect: could not start event thread");
		return -1;
	}

	if (sec_register ((SimiasEventClient)ec) != 0) {
		sec_shutdown (ec, "Could not re-register the Simias Event Client");
		return -1;
	}

	return 0;
}

static void
sec_wait_for_file_change (char *file_path)
{
	/* FIXME: Implement this with polling using the stat () call so that we are portable on Linux, Windows, and Mac */
	sleep (2);
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
	if (ec->state_event_func != NULL) {
		if (ec->state_event_func (SEC_STATE_EVENT_ERROR, err_msg,
								ec->state_event_data) != 0) {
			DEBUG_SEC (("Error calling state_event_func\n"));
		}
						
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
	
	real_message = (void *)malloc ((sizeof (char) * len) + 5);
	if (!real_message) {
		DEBUG_SEC (("Out of memory\n"));
		return 0;
	}

	memset (real_message, '\0', len + 5);
	
	*((int *)real_message) = len;
	sprintf (real_message + 4, "%s", message);
	DEBUG_SEC (("Sending (socket: %d): %s\n", ec->event_socket, real_message + 4));
	
	sent_length = send (ec->event_socket, real_message, len + 4, 0);

	DEBUG_SEC (("Socket after send (): %d\n", ec->event_socket));
	
	free (real_message);
	
	if (sent_length == -1) {
		perror ("SEC: send () error:");
		sprintf (err_msg,
				 "Failed to send message to server.  Socket error: %s",
				 strerror (errno));
		/**
		 * Something must be bad with the socket so we need to reconnect to the
		 * Simias Event Server.
		 */
		DEBUG_SEC (("*=*=*=*=*=*= RECONNECT REQUIRED: send () error =*=*=*=*=*=*\n"));
		if (sec_reconnect (ec) != 0) {
			sec_shutdown (ec, "Could not reconnect the Simias Event Client");
		}
	} else {
		DEBUG_SEC (("Just sent %d bytes to the server\n", sent_length));
	}
	
	return sent_length;
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
sec_get_user_profile_dir_path (char *dest_path)
{
#if defined(WIN32)
	char *user_profile;
	/* Build the configuration file path. */
	user_profile = getenv("USERPROFILE");
	if (user_profile == NULL || strlen (user_profile) <= 0) {
		DEBUG_SEC (("Could not get the USERPROFILE directory\n"));
		return NULL;
	}

	sprintf (dest_path, user_profile);
#else
	char *home_dir;
	char dot_local_path [512];
	char dot_local_share_path [512];
	
	home_dir = getenv ("HOME");
	if (home_dir == NULL || strlen (home_dir) <= 0) {
		DEBUG_SEC (("Could not get the HOME directory\n"));
		return NULL;
	}
	
	/**
	 * Create the directories if they don't already exist.  Ignore any errors if
	 * they already do exist.
	 */
	sprintf (dot_local_path, "%s%s", home_dir, "/.local");
	sprintf (dot_local_share_path, "%s%s", home_dir, "/.local/share");
	if (((mkdir(dot_local_path, 0777) == -1) && (errno != EEXIST)) ||
		 ((mkdir(dot_local_share_path, 0777) == -1) && (errno != EEXIST )))
	{
		perror ("Cannot create '~/.local/share' directory");
		return NULL;
	}
	
	sprintf (dest_path, dot_local_share_path);
#endif	/* WIN32 */

	return dest_path;
}

static char *
sec_get_config_file_path (char *dest_path)
{
	char user_profile_dir [1024];
	if (sec_get_user_profile_dir_path (user_profile_dir) == NULL) {
		DEBUG_SEC (("Could not get base dir for config file\n"));
		return NULL;
	}

#if defined(WIN32)
	sprintf (dest_path,
			 "%s\\Local Settings\\Application Data\\IProcEvent.cfg",
			 user_profile_dir);
#else
	sprintf (dest_path, "%s/IProcEvent.cfg", user_profile_dir);
#endif	/* WIN32 */

	return dest_path;
}

static int
sec_process_message (RealSimiasEventClient *ec, char *message, int length)
{
	xmlDoc *doc;
	void *message_struct;
	char **struct_ptr;
	int err = 0;

	/* Construct an xmlDoc from the message */	
	xmlInitParser ();
	doc = xmlReadMemory (message, length, "message.xml", NULL, 0);
	if (doc != NULL) {
		message_struct = sec_parse_struct_from_doc (doc);
		if (message_struct == NULL) {
			DEBUG_SEC (("Struct couldn't be parsed from message\n"));
			return -1;
		}
		
		struct_ptr = (char **)message_struct;
		
		/* Call the right handler based on the type of message received */
		if (strcmp ("NodeEventArgs", struct_ptr [0]) == 0) {
			SimiasNodeEvent *event = (SimiasNodeEvent *)message_struct;
			DEBUG_SEC (("NodeEventArgs received\n"));

			if (strcmp ("NodeCreated", event->action) == 0) {
				err = sec_notify_event_handlers (ec, ACTION_NODE_CREATED, event);
			} else if (strcmp ("NodeChanged", event->action) == 0) {
				err = sec_notify_event_handlers (ec, ACTION_NODE_CHANGED, event);
			} else if (strcmp ("NodeDeleted", event->action) == 0) {
				err = sec_notify_event_handlers (ec, ACTION_NODE_DELETED, event);
			}
		} else if (strcmp ("CollectionSyncEventArgs", struct_ptr [0]) == 0) {
			SimiasCollectionSyncEvent *event = (SimiasCollectionSyncEvent *)
												message_struct;
			DEBUG_SEC (("CollectionSyncEventArgs message received\n"));
			err = sec_notify_event_handlers (ec, ACTION_COLLECTION_SYNC, event);
		} else if (strcmp ("FileSyncEventArgs", struct_ptr [0]) == 0) {
			SimiasFileSyncEvent *event = (SimiasFileSyncEvent *)message_struct;
			DEBUG_SEC (("FileSyncEventArgs message received\n"));
			err = sec_notify_event_handlers (ec, ACTION_FILE_SYNC, event);
		} else if (strcmp ("NotifyEventArgs", struct_ptr [0]) == 0) {
			SimiasNotifyEvent *event = (SimiasNotifyEvent *)message_struct;
			DEBUG_SEC (("NotifyEventArgs message received\n"));
			err = sec_notify_event_handlers (ec, ACTION_NOTIFY_MESSAGE, event);
		}
		
		sec_free_event_struct (message_struct);
		xmlFreeDoc (doc);
		
		if (err) {
			DEBUG_SEC (("Error occurred in sec_notify_event_handlers\n"));
			return -1;
		}
	} else {
		DEBUG_SEC (("Invalid XML received from event server\n"));
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
		DEBUG_SEC (("Unable to create a new XPath context\n"));
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
			DEBUG_SEC (("Number of attributes on Event/@type nodes is not one (1): %d\n", num_of_nodes));
			xmlFree (xpath_obj);
			return NULL;
		}
		cur_node = node_set->nodeTab [0];
		if (cur_node->type != XML_ATTRIBUTE_NODE) {
			DEBUG_SEC (("XPath of \"%s\" did not return an XML_ATTRIBUTE_NODE\n", SEC_EVENT_TYPE_XPATH));
			xmlFree (xpath_obj);
			return NULL;
		}
		
		/* Get the event type attribute value */
		event_type = xmlNodeGetContent (cur_node);
		if (event_type == NULL) {
			DEBUG_SEC (("Could not get the value of Event/@type\n"));
			xmlFree (xpath_obj);
			return NULL;
		}
		
		DEBUG_SEC (("Parsing event from XML: %s\n", event_type));
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
			DEBUG_SEC (("Unable to evaluate XPath expression: \"%s\"\n", xpath_expr));
			return NULL;
		}
		
		node_set = xpath_obj->nodesetval;
		num_of_nodes = (node_set) ? node_set->nodeNr : 0;
		
		if (num_of_nodes != 1) {
			DEBUG_SEC (("Number of nodes found with XPath expression \"%s\" (expected only 1): %d\n",
					 xpath_expr,
					 num_of_nodes));
			xmlFree (xpath_obj);
			return NULL;
		}
		
		/* Should be a XML_ELEMENT_NODE */
		cur_node = node_set->nodeTab [0];
		
		if (cur_node->type != XML_ELEMENT_NODE) {
			DEBUG_SEC (("Not XML_ELEMENT_NODE: %s\n", cur_node->name));
			xmlFree (xpath_obj);
			return NULL;
		}

		cur_node_val = xmlNodeGetContent (cur_node);
		if (cur_node_val == NULL) {
			DEBUG_SEC (("Element contained no data: %s\n", cur_node->name));
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
		DEBUG_SEC (("Freeing NodeEventArgs\n"));
		element_names = sec_node_event_elements;
	} else if (!strcmp ("CollectionSyncEventArgs", struct_ptr [0])) {
		DEBUG_SEC (("Freeing CollectionSyncEventArgs\n"));
		element_names = sec_collection_sync_event_elements;
	} else if (!strcmp ("FileSyncEventArgs", struct_ptr [0])) {
		DEBUG_SEC (("Freeing FileSyncEventArgs\n"));
		element_names = sec_file_sync_event_elements;
	} else if (!strcmp ("NotifyEventArgs", struct_ptr [0])) {
		DEBUG_SEC (("Freeing NotifyEventArgs\n"));
		element_names = sec_notify_event_elements;
	} else {
		DEBUG_SEC (("Freeing unknown event type (memory leak possible)\n"));
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

static int
sec_add_event_handler (RealSimiasEventClient *ec,
					   IPROC_EVENT_ACTION action,
					   SimiasEventFunc function,
					   void * data)
{
	SimiasEventFuncInfo *func_info;

	/* function must NOT be null */
	if (!function) {
		return -1;
	}
	
	/**
	 * If the function already exists in the list, remove it.
	 */
	sec_remove_event_handler (ec, action, function);
	
	func_info = malloc (sizeof (SimiasEventFuncInfo));
	if (!func_info) {
		DEBUG_SEC (("sec_add_event_handler: Out of memory\n"));
		return -1;
	}
	
	func_info->function	= function;
	func_info->data		= data;
	func_info->next		= ec->event_handlers [action];
	
	/* Place this new handler as the first item in the list */
	ec->event_handlers [action] = func_info;

	return 0;
}

/**
 * Returns 0 if at least one function was removed or -1 otherwise.
 */
static int
sec_remove_event_handler (RealSimiasEventClient *ec, 
						  IPROC_EVENT_ACTION action,
						  SimiasEventFunc function)
{
	SimiasEventFuncInfo *curr_func, *prev_func;
	bool b_func_removed = false;
	
	/* If function is NULL, remove ALL functions for the specified action */
	if (function == NULL) {
		return sec_remove_all_event_handlers (ec, action);
	} else {
		/**
		 * Search through the list for the specified SimiasEventFunc, remove it,
		 * and free the memory used by the SimiasEventFuncInfo.
		 */
		prev_func = NULL;
		
		for (curr_func = ec->event_handlers [action]; curr_func;
			 prev_func = curr_func, curr_func = curr_func->next) {
			if (curr_func->function == function) {
				if (prev_func == NULL) {
					/* This is the first func in the list */
					ec->event_handlers [action] = curr_func->next;
				} else {
					prev_func->next = curr_func->next;
				}
	
				b_func_removed = true;
				free (curr_func);
				break;	/* out of the for loop */
			}
		}
	}
	
	if (!b_func_removed) {
		return -1;
	}
	
	return 0;
}

static int
sec_notify_event_handlers (RealSimiasEventClient *ec,
						   IPROC_EVENT_ACTION action,
						   void *event)
{
	SimiasEventFuncInfo *curr_func;
	bool b_error_occurred = false;
	DEBUG_SEC (("sec_notify_event_handlers () called\n"));
	/**
	 * Iterate through all the event handlers for the specified action and call
	 * them with the event and user-specified data.
	 */
	for (curr_func = ec->event_handlers [action]; curr_func; 
		 curr_func = (SimiasEventFuncInfo *)curr_func->next) {
		if (curr_func->function (event, curr_func->data) != 0) {
			b_error_occurred = true;
		}
	}
	
	if (b_error_occurred) {
		return -1;
	}
	
	return 0;
}

static int
sec_remove_all_event_handlers (RealSimiasEventClient *ec,
							   IPROC_EVENT_ACTION action)
{
	SimiasEventFuncInfo *curr_func, *temp_func;

	/**
	 * Remove all the event handlers for the specified action */
	for (curr_func = ec->event_handlers [action]; curr_func; ) {
		 temp_func = curr_func->next;
		 free (curr_func);
		 curr_func = temp_func;
	}
	
	ec->event_handlers [0] = NULL;

	return 0;
}

const char *
sec_get_node_type_str (SIMIAS_NODE_TYPE type)
{
	switch (type) {
		case NODE_TYPE_BASE_FILE:
			return "BaseFileNode";
		case NODE_TYPE_COLLECTION:
			return "Collection";
		case NODE_TYPE_DIR:
			return "DirNode";
		case NODE_TYPE_DOMAIN:
			return "Domain";
		case NODE_TYPE_FILE:
			return "FileNode";
		case NODE_TYPE_IDENTITY:
			return "Identity";
		case NODE_TYPE_LINK:
			return "LinkNode";
		case NODE_TYPE_LOCAL_DATABASE:
			return "LocalDatabase";
		case NODE_TYPE_MEMBER:
			return "Member";
		case NODE_TYPE_NODE:
			return "Node";
		case NODE_TYPE_POLICY:
			return "Policy";
		case NODE_TYPE_ROSTER:
			return "Roster";
		case NODE_TYPE_STORE_FILE:
			return "StoreFileNode";
		case NODE_TYPE_TOMBSTONE:
			return "Tombstone";
		default:
			return "UnknownNode";
	}
}
/* #endregion */

/* #region Testing */

int
simias_node_event_callback (SimiasNodeEvent *event, void *data)
{
	printf ("sec-test: SimiasNodeEvent:\n");
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
	
	return 0;
}

int
simias_collection_sync_event_callback (SimiasCollectionSyncEvent *event, void *data)
{
	printf ("sec-test: SimiasCollectionSyncEvent:\n");
	printf ("\t%s: %s\n", "event_type", event->event_type);
	printf ("\t%s: %s\n", "name", event->name);
	printf ("\t%s: %s\n", "id", event->id);
	printf ("\t%s: %s\n", "action", event->action);
	printf ("\t%s: %s\n", "successful", event->successful);
	
	return 0;
}

int
simias_file_sync_event_callback (SimiasFileSyncEvent *event, void *data)
{
	printf ("sec-test: SimiasCollectionSyncEvent:\n");
	printf ("\t%s: %s\n", "event_type", event->event_type);
	printf ("\t%s: %s\n", "collection_id", event->collection_id);
	printf ("\t%s: %s\n", "object_type", event->object_type);
	printf ("\t%s: %s\n", "delete_str", event->delete_str);
	printf ("\t%s: %s\n", "name", event->name);
	printf ("\t%s: %s\n", "size", event->size);
	printf ("\t%s: %s\n", "size_to_sync", event->size_to_sync);
	printf ("\t%s: %s\n", "size_remaining", event->size_remaining);
	printf ("\t%s: %s\n", "direction", event->direction);
	
	return 0;
}

int
simias_notify_event_callback (SimiasNotifyEvent *event, void *data)
{
	printf ("sec-test: SimiasCollectionSyncEvent:\n");
	printf ("\t%s: %s\n", "event_type", event->event_type);
	printf ("\t%s: %s\n", "message", event->message);
	printf ("\t%s: %s\n", "time", event->time);
	printf ("\t%s: %s\n", "type", event->type);
	
	return 0;
}

int
sec_state_event_callback (SEC_STATE_EVENT state_event, const char *message, void *data)
{
	SimiasEventClient *ec = (SimiasEventClient *)data;
	SIMIAS_NODE_TYPE node_type;
	SimiasEventFilter event_filter;
	
	switch (state_event) {
		case SEC_STATE_EVENT_CONNECTED:
			printf ("sec-test: Connected Event\n");

			/* Ask to listen to some events by calling sec_set_event () */
			sec_set_event (*ec, ACTION_NODE_CREATED, true, (SimiasEventFunc)simias_node_event_callback, NULL);
			sec_set_event (*ec, ACTION_NODE_CHANGED, true, (SimiasEventFunc)simias_node_event_callback, NULL);
			sec_set_event (*ec, ACTION_NODE_DELETED, true, (SimiasEventFunc)simias_node_event_callback, NULL);
			sec_set_event (*ec, ACTION_COLLECTION_SYNC, true, (SimiasEventFunc)simias_collection_sync_event_callback, NULL);
			sec_set_event (*ec, ACTION_FILE_SYNC, true, (SimiasEventFunc)simias_file_sync_event_callback, NULL);
			sec_set_event (*ec, ACTION_NOTIFY_MESSAGE, true, (SimiasEventFunc)simias_notify_event_callback, NULL);
			
			/* Setup a filter to only get Collections back when it's a NodeEvent */
			node_type = NODE_TYPE_COLLECTION;
			event_filter.type = EVENT_FILTER_NODE_TYPE;
			event_filter.data = &node_type;
			sec_set_filter (*ec, &event_filter);

			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			printf ("sec-test: Disconnected Event\n");

			break;
		case SEC_STATE_EVENT_ERROR:
			if (message) {
				printf ("sec-test: Error in Simias Event Client: %s\n", message);
			} else {
				printf ("sec-test: An unknown error occurred in Simias Event Client\n");
			}
			break;
		default:
			printf ("sec-test: An unknown Simias Event Client State Event occurred\n");
	}
	return 0;
}

/*
// CRG- this must be commented out for OS X to build correctly
int
main (int argc, char *argv[])
{
	SimiasEventClient ec;
	CLIENT_STATE state;
	char buf [256];
	
	if (sec_init (&ec, sec_state_event_callback, &ec) != 0) {
		printf ("sec-test: sec_init failed\n");
		return -1;
	}
	
	if (sec_register (ec) != 0) {
		printf ("sec-test: sec_register failed\n");
		return -1;
	}
	
	printf ("sec-test: Registration complete\n");
	
	printf ("sec-test: Press <Enter> to stop the client...");
	fgets (buf, sizeof (buf), stdin);
	
	if (sec_deregister (ec) != 0) {
		printf ("sec-test: sec_deregister failed\n");
		return -1;
	}

	if (sec_cleanup (&ec) != 0) {
		printf ("sec-test: sec_cleanup failed\n");
		return -1;
	}
	
	return 0;
}
*/

/* #endregion */
