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
#define SOCKET_PATH "echo_socket"

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
 * Actions that indicate what to do with the simias events
 */
typedef enum
{
	ACTION_ADD_NODE_CREATED,
	ACTION_ADD_NODE_CHANGED,
	ACTION_ADD_NODE_DELETED,
	ACTION_ADD_COLLECTION_SYNC,
	ACTION_ADD_FILE_SYNC,
	ACTION_ADD_NOTIFY_MESSAGE,
	ACTION_REMOVE_NODE_CREATED,
	ACTION_REMOVE_NODE_CHANGED,
	ACTION_REMOVE_NODE_DELETED,
	ACTION_REMOVE_COLLECTION_SYNC,
	ACTION_REMOVE_FILE_SYNC,
	ACTION_REMOVE_NOTIFY_MESSAGE
} IPROC_EVENT_ACTION;

typedef struct
{
	/* Initialization state of the client */
	CLIENT_STATE state;
	
	/* Socket used to listen for events from the event server */
	int event_socket;
	
	/* Socket used to send messages to the event server */
	int message_socket;
	
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
} SimiasEventClient;

/**
 * Event Structures
 */
 
/* Node Event */
typedef struct
{
	char *event_type;
	char *action;
	char *time;
	char *source;
	char *collection;
	char *type;
	char *event_id;
	char *node;
	char *flags;
	char *master_rev;
	char *slave_rev;
	char *file_size;
} SimiasNodeEvent;

/* Collection Sync Event */
typedef struct 
{
	char *event_type;
	char *name;
	char *id;
	char *action;
	char *successful;
} SimiasCollectionSyncEvent;

/* File Sync Event */
typedef struct 
{
	char *event_type;
	char *collection_id;
	char *object_type;
	char *delete_str;
	char *name;
	char *size;
	char *size_to_sync;
	char *size_remaining;
	char *direction;
} SimiasFileSyncEvent;

/* Notify Event */
typedef struct 
{
	char *event_type;
	char *message;
	char *time;
	char *type;
} SimiasNotifyEvent;

/**
 * Public Functions
 */

/**
 * Initializes the Simias Event Client
 * 
 * param: error_handler Delegate that gets called if an error occurs.  A NULL
 * may be passed in if the application does not care to be notified of errors.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_init (SimiasEventClient *ec,
									 void *error_handler);

/**
 * Cleans up the Simias Event Client
 * 
 * This function should be called when the application is no longer using the
 * Simias Event Client.
 */
static int sec_cleanup (SimiasEventClient *ec);

/**
 * Registers this client with the server to listen for Simias events.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_register (SimiasEventClient *ec);

/**
 * Deregisters this client with the server
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_deregister (SimiasEventClient *ec);

/**
 * Start subscribing to or unsubscribing from the specified event.
 *
 * param action: Action to take regarding the event.
 * param handler: Callback function that gets called when the specified event
 * happens or is to be removed.
 */
static int sec_set_event (SimiasEventClient *ec, 
						  IPROC_EVENT_ACTION action,
						  void **handler);
