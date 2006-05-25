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

#ifndef IFOLDER_CLIENT_H
#define IFOLDER_CLIENT_H

#include "ifolder-types.h"
#include "ifolder-errors.h"
#include "ifolder-domain.h"
#include "ifolder.h"
//#include "ifolder-user.h"
#include "ifolder-user-prefs.h"

#ifdef __cplusplus
extern "C"
{
#endif		/* __cplusplus */

/**
 * @mainpage iFolder 3.6 Client API for C
 * 
 * @section intro_sec Introduction
 * 
 * @todo Add main page documentation for the C API
 * 
 * iFolder is a simple and secure storage solution that can increase your
 * productivity by enabling you to back up, access and manage your personal
 * files-from anywhere, at any time. Once you have installed iFolder, you
 * simply save your files locally-as you have always done-and iFolder
 * automatically updates the files on a network server and delivers them to the
 * other machines you use.
 * 
 * A wiki page is maintained for iFolder at http://www.ifolder.com/
 * 
 * @section install_sec Installation
 * 
 * @subsection step1 Step 1: Get the Source Code
 * 
 * Get the source code from http://www.ifolder.com
 * 
 * @subsection step2 Step 2: Build the Source Code
 * 
 */

/**
 * @file ifolder-client.h
 * @brief Main Client API (start here)
 * 
 * @link client_events_page Client Events @endlink
 * @link sync_events_page Synchronization Events @endlink
 * 
 * A process can only control only ONE instance of the iFolder Client.  When
 * your program loads, call ifolder_client_initialize().  Before you exit,
 * make sure you call ifolder_client_uninitialize() before your program exits.
 */
G_BEGIN_DECLS

#define IFOLDER_DEFAULT_CONFIG_FILE_NAME	"ifolder3.ini"

#define IFOLDER_CLIENT_TYPE				(ifolder_client_get_type())
#define IFOLDER_CLIENT(obj)				(G_TYPE_CHECK_INSTANCE_CAST ((obj), IFOLDER_CLIENT_TYPE, iFolderClient))
#define IFOLDER_CLIENT_CLASS(klass)		(G_TYPE_CHECK_CLASS_CAST ((klass), IFOLDER_CLIENT_TYPE, iFolderClientClass))
#define IFOLDER_IS_CLIENT(obj)			(G_TYPE_CHECK_INSTANCE_TYPE ((obj), IFOLDER_CLIENT_TYPE))
#define IFOLDER_IS_CLIENT_CLASS(klass)	(G_TYPE_CHECK_CLASS_TYPE ((klass), IFOLDER_CLIENT_TYPE))
#define IFOLDER_CLIENT_GET_CLASS(obj)	(G_TYPE_INSTANCE_GET_CLASS ((obj), IFOLDER_CLIENT_TYPE, iFolderClientClass))

/* GObject support */
GType ifolder_client_get_type (void) G_GNUC_CONST;

/**
 * Enumerations
 */
/**
 * @todo Do we need any more states than these?
 */
typedef enum
{
	IFOLDER_CLIENT_STATE_INITIALIZING,	/*!< The client is initializing */
	IFOLDER_CLIENT_STATE_STOPPED,		/*!< Synchronization has been stopped for the client */
	IFOLDER_CLIENT_STATE_IDLE,			/*!< The client is idle (in between synchronization cycles) */
	IFOLDER_CLIENT_STATE_PAUSED,			/*!< The client is paused in the middle of a synchronization process */
	IFOLDER_CLIENT_STATE_SYNCHRONIZING,	/*!< The client is actively synchronizing an iFolder */
	IFOLDER_CLIENT_STATE_UNINITIALIZING	/*!< The client is uninitializing */
} iFolderClientState;

/**
 * Method definitions
 */

/**
 * @name Main Client API
 */
/*@{*/

//! Initialize the iFolder Client.
/**
 * This must be called before using the iFolder Client API.
 * 
 * @param data_path (Optional) The local file system path that should be used
 * to store metadata, control files, etc. for the iFolder Client.  This allows
 * multiple instances of the iFolder Client to run on the same computer by the
 * same user.
 * @return IFOLDER_SUCCESS if the call was successful.
 */
iFolderClient *ifolder_client_initialize(const gchar *data_path, GError **error);

//! Uninitialize the iFolder Client
/**
 * This must be called when you are finished using the iFolder Client (usually
 * just before your process exits).
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_client_uninitialize(iFolderClient *client, GError **error);

//! Returns the current state of the iFolder Client.
/**
 * @return the current state of the iFolder Client.
 */
iFolderClientState ifolder_client_get_state(iFolderClient *client);

/*@}*/

/**
 * @name Synchronization Control
 * 
 * @todo Review these functions with Calvin, Russ, and anyone else who should
 * give input on this.  Trent?
 * 
 * Control synchronization for the entire client by using one of these
 * functions.
 * 
 * To synchronize a single iFolder, use:
 * 
 * @li ifolder_sync_now()
 */
/*@{*/

//! Start synchronization for all iFolders.
/**
 * The main iFolder process should call this function when it first loads to
 * start a synchronization process.  If this method is subsequently called, it
 * will queue a complete synchronization of all iFolders.
 * 
 * If this function is called again before a full synchronization completes,
 * one of the following will be returned:
 * 
 * @li IFOLDER_ERR_FULL_SYNC_ALREADY_RUNNING
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_client_start_synchronization(iFolderClient *client, GError **error);

//! Stop all synchronization
/**
 * @todo Determine if a call to ifolder_start_sync(x, true) would be allowed
 * when the client is stopped.
 * 
 * Immediately stops all synchronization processes that are running.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_client_stop_synchronization(iFolderClient *client, GError **error);

//! Resume synchronization after being paused
/**
 * This will resume synchronization without causing the iFolder that was
 * synchronizing to check for local changes again.  It also does not re-
 * synchronize iFolders that had already synchronized in the previous
 * synchronization cycle.
 * 
 * @return IFOLDER_SUCCESS if the call was successful.
 */
void ifolder_client_resume_synchronization(iFolderClient *client, GError **error);

/*@}*/

/**
 * @name Domain Access
 */
/*@{*/

/**
 * The returned GSList should be freed by the caller.
 */
GSList *ifolder_client_get_all_domains(iFolderClient *client, GError **error);

/**
 * The returned GSList should be freed by the caller.
 */
GSList *ifolder_client_get_all_active_domains(iFolderClient *client, GError **error);
iFolderDomain *ifolder_client_get_default_domain(iFolderClient *client, GError **error);

iFolderDomain *ifolder_client_add_domain(iFolderClient *client, const gchar *host_address, const gchar *user_name, const gchar *password, gboolean remember_password, gboolean make_default, GError **error);
void ifolder_client_remove_domain(iFolderClient *client, iFolderDomain *domain, GError **error);

/*@}*/

/** @page client_events_page Client Events

@events
 @event client-starting
 @event client-started
 @event client-stopping
 @event client-stopped
 @event client-upgrade-available
@endevents

<hr>

@eventdef client-starting
 @eventproto
void (*client_starting)(void);
 @endeventproto
 @eventdesc
  Emitted when the main client process is starting up.
@endeventdef

@eventdef client-started
 @eventproto
void (*client_started)(void);
 @endeventproto
 @eventdesc
  Emitted when the main client process has started.
  
  @todo Note: Make sure that the implementation of the API automatically
  reconnects all event handlers that were already registered with the
  new client process.  This should be completely transparent to the consumer
  of the API.
@endeventdef

@eventdef client-stopping
 @eventproto
void (*client_stopping)(void);
 @endeventproto
 @eventdesc
  Emitted when the main client process is shutting down.  You should clean up
  memory as needed.
@endeventdef

@eventdef client-stopped
 @eventproto
void (*client_stopped)(void);
 @endeventproto
 @eventdesc
  Emitted just before the client shuts down entirely.
@endeventdef

@eventdef client-upgrade-available
 @eventproto
void (*client_upgrade_available)(const iFolderDomain domain, const char *version);
 @endeventproto
 @eventdesc
  Emitted when a client upgrade is detected after logging in to a domain.
 @param domain The domain which has a new client available for download.
 @param version The version of the new client.
@endeventdef

*/

/** @page sync_events_page Synchronization Events

@events
 @event ifolder-dredge-started

 @event file-add-detected
 @event file-delete-detected
 @event file-modified-detected

 @event ifolder-sync-started
 @event ifolder-sync-paused
 @event ifolder-sync-succeeded
 @event ifolder-sync-failed

 @event file-added
 @event file-deleted
 @event file-modified

 @event file-sync-started
 @event file-sync-paused
 @event file-sync-succeeded
 @event file-sync-failed
@endevents

<hr>

@eventdef ifolder-dredge-started
 @eventproto
void (*ifolder_dredge_started)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted at the beginning of a synchronization cycle when the client checks an iFolder for local changes.
 @param ifolder The iFolder that is being synchronized.
@endeventdef

@eventdef file-add-detected
 @eventproto
void (*file_add_detected)(const iFolder ifolder, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted during an iFolder dredge when a new file is detected.
 @param ifolder The iFolder that is being synchronized.
 @param file_name The name of the file that was added.
@endeventdef

@eventdef file-delete-detected
 @eventproto
void (*file_delete_detected)(const iFolder ifolder, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted during an iFolder dredge when a known file no longer exists locally.
 @param ifolder The iFolder that is being synchronized.
 @param file_name The name of the file that was deleted.
@endeventdef

@eventdef file-modified-detected
 @eventproto
void (*file_modified_detected)(const iFolder ifolder, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted during an iFolder dredge when a known file is detected to be modified.
 @param ifolder The iFolder that is being synchronized.
 @param file_name The name of the file that was modified.
@endeventdef

@eventdef ifolder-sync-started
 @eventproto
void (*ifolder_sync_started)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder synchronization has begun.
 @param ifolder The iFolder that is being synchronized.
@endeventdef

@eventdef ifolder-sync-paused
 @eventproto
void (*ifolder_sync_paused)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder synchronization is paused.
 @param ifolder The iFolder that is being synchronized.
@endeventdef

@eventdef ifolder-sync-succeeded
 @eventproto
void (*ifolder_sync_succeeded)(const iFolder ifolder);
 @endeventproto
 @eventdesc
  Emitted when an iFolder synchronization completed successfully.
 @param ifolder The iFolder that is being synchronized.
@endeventdef

@eventdef ifolder-sync-failed
 @eventproto
void (*ifolder_sync_failed)(const iFolder ifolder, const iFolderSyncFailureType failure);
 @endeventproto
 @eventdesc
  Emitted when an iFolder synchronization failed.
 @param ifolder The iFolder that is being synchronized.
 @param failure The reason the synchronization failed.
@endeventdef

@eventdef file-added
 @eventproto
void (*file_added)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file in an iFolder is added.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file that was added.
@endeventdef

@eventdef file-deleted
 @eventproto
void (*file_deleted)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file in an iFolder is deleted.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file that was deleted.
@endeventdef

@eventdef file-modified
 @eventproto
void (*file_modified)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file in an iFolder is modified.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file that was modified.
@endeventdef

@eventdef file-sync-started
 @eventproto
void (*file_sync_started)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file synchronization is started.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file.
@endeventdef

@eventdef file-sync-paused
 @eventproto
void (*file_sync_paused)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file synchronization is paused.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file.
@endeventdef

@eventdef file-sync-succeeded
 @eventproto
void (*file_sync_succeeded)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name);
 @endeventproto
 @eventdesc
  Emitted when a file synchronization succeeded.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file.
@endeventdef

@eventdef file-sync-failed
 @eventproto
void (*file_sync_failed)(const iFolder ifolder, const iFolderSyncDirection direction, const char *file_name, const iFolderSyncFailureType failure);
 @endeventproto
 @eventdesc
  Emitted when a file synchronization failed.
 @param ifolder The iFolder that is being synchronized.
 @param direction The direction of the current synchronization.
 @param file_name The name of the file.
 @param failure The reason the synchronization failed.
@endeventdef

*/

G_END_DECLS

#ifdef __cplusplus
}
#endif		/* __cplusplus */


#endif /* IFOLDER_CLIENT_H */
