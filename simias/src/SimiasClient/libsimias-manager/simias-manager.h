/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright  Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
#ifndef _simias_manager_h_
#define _simias_manager_h_

//typedef void *	Manager;

/*
 * Method:	Allocates a Manager object and populates it with the defaults.
 * Returns:	A pointer to a Manager object.
 */
Manager *AllocateManager();

/*
 * Method:	Allocates a Manager object and populates it with the specified
 * command line arguments.
 * Returns:	A pointer to a Manager object.
 */
Manager *AllocateManagerWithArgs( int argsLength, const char *args[] );

/*
 * Frees the specified Manager object.
 */
void FreeManager( Manager *pManager );

/*
 * Gets the path to the Simias.exe application.
 */
const char *GetApplicationPath( Manager *pManager );

/*
 * Gets the path to the simias data directory.
 */
const char *GetDataPath( Manager *pManager );

/*
 * Gets whether to run in a server configuration.
 */
char GetIsServer( Manager *pManager );

/*
 * Gets the web service listener port.
 */
int GetPort( Manager *pManager );

/*
 * Gets whether to show console output for the Simias process.
 */
char GetShowConsole( Manager *pManager );

/*
 * Gets the url for the local web service.
 */
const char *GetWebServiceUrl( Manager *pManager );

/*
 * Gets whether to print extra informational messages.
 */
char GetVerbose( Manager *pManager );

/*
 * Sets a new path to the Simias.exe application.
 */
void SetApplicationPath( Manager *pManager, const char *pApplicationPath );

/*
 * Sets a new path to the simias data directory.
 */
void SetDataPath( Manager *pManager, const char *pDataPath );

/*
 * Sets whether to run in a server configuration.
 */
void SetIsServer( Manager *pManager, char isServer );

/*
 * Sets a new web service listener port.
 */
void SetPort( Manager *pManager, int port );

/*
 * Sets whether to show console output for the Simias process.
 */
void SetShowConsole( Manager *pManager, char showConsole );

/*
 * Sets whether to print extra informational messages.
 */
void SetVerbose( Manager *pManager, char verbose );

#endif	/*-- _simias_manager_h_ --*/