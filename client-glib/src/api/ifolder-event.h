/***********************************************************************
 *  $RCSfile$
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

#ifndef _IFOLDER_C_EVENT_H_
#define _IFOLDER_C_EVENT_H_

#include <stdarg.h>

#include "ifolder-client.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @file ifolder-event.h
 * @brief Event API
 * 
 * The Event API provides access to the built-in events that occur inside of
 * the iFolder Client.
 * 
 * @section built_in Built-in iFolder Client Events
 * 
 * These are the built-in names of the events the iFolder Client will emit.
 * 
 * @subsection client_events Client Events
 * @li @link client_events_page client-starting @endlink
 * @li @link client_events_page client-started @endlink
 * @li @link client_events_page client-stopping @endlink
 * @li @link client_events_page client-stopped @endlink
 * @li @link client_events_page client-upgrade-available @endlink
 * 
 * @subsection sync_events Synchronization Events
 * @li @link sync_events_page ifolder-dredge-started @endlink
 * 
 * @li @link sync_events_page file-add-detected @endlink
 * @li @link sync_events_page file-delete-detected @endlink
 * @li @link sync_events_page file-modified-detected @endlink
 * 
 * @li @link sync_events_page ifolder-sync-started @endlink
 * @li @link sync_events_page ifolder-sync-paused @endlink
 * @li @link sync_events_page ifolder-sync-succeeded @endlink
 * @li @link sync_events_page ifolder-sync-failed @endlink
 * 
 * @li @link sync_events_page file-deleted @endlink
 * @li @link sync_events_page file-added @endlink
 * @li @link sync_events_page file-modified @endlink
 * 
 * @li @link sync_events_page file-sync-started @endlink
 * @li @link sync_events_page file-sync-paused @endlink
 * @li @link sync_events_page file-sync-succeeded @endlink
 * @li @link sync_events_page file-sync-failed @endlink
 * 
 * @subsection domain_events Domain Events
 * @li @link domain_events_page domain-added @endlink
 * @li @link domain_events_page domain-removed @endlink
 * @li @link domain_events_page domain-host-modified @endlink
 * @li @link domain_events_page domain-logged-in @endlink
 * @li @link domain_events_page domain-logged-out @endlink
 * @li @link domain_events_page domain-needs-credentials @endlink
 * @li @link domain_events_page domain-activated @endlink
 * @li @link domain_events_page domain-inactivated @endlink
 * @li @link domain_events_page domain-new-default @endlink
 * @li @link domain_events_page domain-in-grace-login-period @endlink
 * 
 * @subsection ifolder_events iFolder Events
 * @li @link ifolder_events_page ifolder-connected @endlink
 * @li @link ifolder_events_page ifolder-disconnected @endlink
 * @li @link ifolder_events_page ifolder-created @endlink
 * @li @link ifolder_events_page ifolder-deleted @endlink
 * @li @link ifolder_events_page ifolder-state-changed @endlink
 * @li @link ifolder_events_page ifolder-owner-changed @endlink
 * @li @link ifolder_events_page ifolder-published @endlink
 * @li @link ifolder_events_page ifolder-unpublished @endlink
 * @li @link ifolder_events_page ifolder-member-added @endlink
 * @li @link ifolder_events_page ifolder-member-removed @endlink
 * @li @link ifolder_events_page ifolder-member-rights-modified @endlink
 * 
 * @subsection user_prefs_events User Preferences Events
 * @li @link user_prefs_events_page user-pref-added @endlink
 * @li @link user_prefs_events_page user-pref-deleted @endlink
 * @li @link user_prefs_events_page user-pref-reset @endlink
 * @li @link user_prefs_events_page user-pref-modified @endlink
 * 
 * @section custom_events Custom Events
 * 
 * You can use this API to register, connect to, and emit your own
 * custom-defined events.
 * 
 * You cannot add, remove, or emit a built-in event.  If you attempt to do so,
 * an error will be returned.
 * 
 * @todo Determine whether custom-defined events will span to multiple
 * processes or whether they will just be available inside the process in
 * which they were registered.
 * 
 */

#define IFOLDER_EVENT_LISTENER_CALLBACK(func) ((iFolderEventListenerCallback)func)
typedef void (*iFolderEventListenerCallback)(void);
typedef void (*iFolderEventHandlerFunc)(void);

/**
 * @name Connecting to Events
 * @{
 */

//! Connect an event listener to an event.
/**
 * @param event The name of the event to connect to.
 * @param func The callback function to connect.
 * @param user_data The data to pass to the callback function.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_connect(const char *event, iFolderEventListenerCallback func, void *user_data);

//! Disconnect an event listener from an event.
/**
 * @param event The name of the event to disconnect from.
 * @param func The callback function to disconnect.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_disconnect(const char *event, iFolderEventListenerCallback func);

/*@}*/

/**
 * @name Adding, Removing, and Emitting Events
 * @{
 */

//! Add a new event to the event system.
/**
 * @param event The event to add.
 * @param func The function that will call each event listener when the event
 * is emitted.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_add(const char *event, iFolderEventHandlerFunc func);

//! Remove an event from the event system.
/**
 * @param event The event to remove.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_remove(const char *event);

//! Emit an event
/**
 * The iFolderEventHandlerFunc for the specified event will be called to emit
 * the event to all event listeners.
 * 
 * @param event The event to emit.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_emit(const char *event, ...);

/*@}*/

/**
 * @name Event Listener Access
 * @{
 */

//! Returns iFolderEventListenerCallback functions for an event.
/**
 * This function should be used by iFolderEventHandlerFunc to emit an event to
 * all the listeners.
 * 
 * @param event The event.
 * @param listener_enum Invalid if the call is unsuccessful.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
int ifolder_event_get_listeners(const char *event, iFolderEnumeration *listener_enum);

/*@}*/

#ifdef __cplusplus
}
#endif		/* __cplusplus */

#endif /*_IFOLDER_C_USER_H_*/
