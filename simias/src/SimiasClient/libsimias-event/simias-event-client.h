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

#ifndef SIMIAS_EVENT_CLIENT_H
#define SIMIAS_EVENT_CLIENT_H

#include <stdbool.h>

/**
 * SimiasEventClient is the main structure that is used to make the calls to the
 * Event Server.  In the sec_init function, memory is allocated for the
 * structure and in sec_cleanup, it is freed.
 */
typedef void * SimiasEventClient;

typedef enum
{
	SEC_STATE_EVENT_CONNECTED,
	SEC_STATE_EVENT_DISCONNECTED,
	SEC_STATE_EVENT_ERROR
} SEC_STATE_EVENT;

/**
 * Actions that indicate what to do with the simias events
 */
typedef enum
{
	ACTION_NODE_CREATED = 0,
	ACTION_NODE_CHANGED,
	ACTION_NODE_DELETED,
	ACTION_COLLECTION_SYNC,
	ACTION_FILE_SYNC,
	ACTION_NOTIFY_MESSAGE
} IPROC_EVENT_ACTION;

/**
 * Used to specify only certain types of events should be sent to the client.
 * These filters only apply to Simias "node" events.  Sync events cannot be
 * filtered.
 */
typedef enum
{
	/* Subscribe to all changes in the specified collection */
	EVENT_FILTER_COLLECTION,
	
	/* Subscribe to all changes to the specified node */
	EVENT_FILTER_NODE_ID,
	
	/* Subscribe to all changes to nodes of the specified type */
	EVENT_FILTER_NODE_TYPE
} IPROC_EVENT_FILTER_TYPE;

/**
 * This enum is for convenience when calling sec_set_filter
 * when the type of filter is set to EVENT_FILTER_NODE_TYPE.
 */
typedef enum
{
	NODE_TYPE_BASE_FILE,
	NODE_TYPE_COLLECTION,
	NODE_TYPE_DIR,
	NODE_TYPE_DOMAIN,
	NODE_TYPE_FILE,
	NODE_TYPE_IDENTITY,
	NODE_TYPE_LINK,
	NODE_TYPE_LOCAL_DATABASE,
	NODE_TYPE_MEMBER,
	NODE_TYPE_NODE,
	NODE_TYPE_POLICY,
	NODE_TYPE_ROSTER,
	NODE_TYPE_STORE_FILE,
	NODE_TYPE_TOMBSTONE
} SIMIAS_NODE_TYPE;

typedef struct
{
	/* Type of event filter */
	IPROC_EVENT_FILTER_TYPE type;
	
	/**
	 * Data that is used to filter the events.
	 * 
	 * Valid values for data (based on value of type):
	 * 
	 * EVENT_FILTER_COLLECTION: data should be a char * set to the Collection ID
	 * EVENT_FILTER_NODE_ID: data should be a char * set to the Node ID
	 * EVENT_FILTER_NODE_TYPE: data should be a SIMAS_NODE_TYPE *
	 */
	void *data;
} SimiasEventFilter;

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
	char *connected;
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
	char *status;
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
 * Callback function prototypes for Simias Events.
 * 
 * These prototypes specify the type of functions that should be passed to
 * sec_set_event () function when registering for event actions.
 * 
 * ACTION_*_NODE_* actions should use SimiasNodeEventFunc
 * ACTION_*_COLLECTION_SYNC actions should use SimiasCollectionSyncEventFunc
 * ACTION_*_FILE_SYNC actions should use SimiasFileSyncEventFunc
 * ACTION_*_NODE_* actions should use SimiasNotifyEventFunc
 * 
 * event:	the SimiasNodeEvent received from the server.
 * data:	data passed to the function, set when calling sec_set_event ().
 * Returns:	these functions should return 0 if successful, otherwise, -1.
 */
typedef int (*SimiasNodeEventFunc) (SimiasNodeEvent *event, void *data);
typedef int (*SimiasCollectionSyncEventFunc) (SimiasCollectionSyncEvent *, void *data);
typedef int (*SimiasFileSyncEventFunc) (SimiasFileSyncEvent *, void *data);
typedef int (*SimiasNotifyEventFunc) (SimiasNotifyEvent *, void *data);

/* Prototype for convenience */
typedef int (*SimiasEventFunc) (void *, void *);

/**
 * Callback function prototype for Simias Event Client State
 */
typedef int (*SECStateEventFunc) (SEC_STATE_EVENT state_event,
								  const char *message,
								  void *data);

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
int sec_init (SimiasEventClient *sec, SECStateEventFunc state_event_func, 
			  void *state_event_data);

/**
 * Cleans up the Simias Event Client
 * 
 * This function should be called when the application is no longer using the
 * Simias Event Client.
 */
int sec_cleanup (SimiasEventClient *sec);

/**
 * Registers this client with the server to listen for Simias events.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
int sec_register (SimiasEventClient sec);

/**
 * Deregisters this client with the server
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
int sec_deregister (SimiasEventClient sec);

/**
 * Subscribe or unsubscribe to the specified event.
 * 
 * This function should only be called once a SEC_STATE_EVENT_CONNECTED
 * has been received by the SECStateEventFunc registered during sec_init.
 *
 * sec:			The Simias Event Client.
 * action:		The action that should be listened for.
 * subscribe:	true to subscribe, false to unsubscribe
 * function:	The callback function that should be called when the event
 * 				action is received.  If function is NULL when subscribe is false
 * 				all functions associated with the specified action will be
 * 				removed.
 * data:		Custom data that will be passed to the callback function.
 */
int sec_set_event (SimiasEventClient sec,
				   IPROC_EVENT_ACTION action,
				   bool subscribe,
				   SimiasEventFunc function,
				   void *data);

/**
 * Set or unset the specified filter for the subscriber.
 * 
 * sec:		The Simias Event Client.
 * filter:	A filter which limits the type of filters that should be sent back
 * 			back to the client from the event server.  If the filter should be
 * 			cleared, filter->data should be set to NULL.
 * 
 * Returns -1 if there was an error, otherwise 0.
 */
int sec_set_filter (SimiasEventClient sec, SimiasEventFilter *filter);

#endif /* SIMIAS_EVENT_CLIENT_H */
