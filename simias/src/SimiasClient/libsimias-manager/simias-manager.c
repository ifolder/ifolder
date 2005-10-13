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

#include <stdio.h>
#include <io.h>
#include <direct.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>

#define MAX_PATH_SIZE				260

#define WIN_ALL_USERS_PROFILE		"ALLUSERSPROFILE"
#define MAPPING_FILE				"SimiasDirectoryMapping"
#define DEFAULT_LINUX_MAPPING_DIR	"/etc/simias/"
#define DEFAULT_WINDOWS_MAPPING_DIR	"\\Application Data\\Simias\\"

/**
 * Structure used to hold the default settings and user specified settings.
 */
typedef struct _Manager_
{
	const char	*applicationPath;
	const char	*simiasDataPath;
	const char	*webServiceUri;
	const char	*port;

	union
	{
		struct _flags_
		{
			char	IsServer	: 1;
			char	ShowConsole	: 1;
			char	Verbose		: 1;

		} flags;

		char	zeroFlags;	
	};
	
} Manager;

/*
 * Forward declaration of static functions.
 */

static const char *GetDefaultApplicationPath();
static const char *GetDefaultMappingFile();
static void ShowError( const char *format, ... );

void SetApplicationPath( Manager *pManager, const char *pApplicationPath );
void SetDataPath( Manager *pManager, const char *pDataPath );
void SetIsServer( Manager *pManager, char isServer );
void SetPort( Manager *pManager, const char *pPort );
void SetShowConsole( Manager *pManager, char showConsole );
void SetVerbose( Manager *pManager, char verbose );


/*
 * Buffer used to show errors.
 */
static char errorBuffer[ 512 ];


/*
 * Gets the default path to the Simias.exe file.
 */
static const char *GetDefaultApplicationPath()
{
	const char *pDefaultPath;
	char *pApplicationPath = NULL;
	char *pTempPath;
	FILE *fp;
	intptr_t hFile;
	struct _finddata_t fileData;
	unsigned long length = 0;

	/* See if the DefaultMappingFile is in the current directory. */
	if ( ( hFile = _findfirst( MAPPING_FILE, &fileData ) ) != -1 )
	{
		pTempPath = malloc( strlen( fileData.name ) + 1 );
		if ( pTempPath != NULL )
		{
			strcpy( pTempPath, fileData.name );
			length = fileData.size;
		}
		else
		{
			ShowError( "Could not allocate %d bytes in GetDefaultApplicationPath()", strlen( fileData.name ) + 1 );
		}

		_findclose( hFile );
	}
	else
	{
		/* The file does not exist in the current directory. Look for it in the
		 * default place.
		 */
		pDefaultPath = GetDefaultMappingFile();
		if ( ( hFile = _findfirst( pDefaultPath, &fileData ) ) != -1 )
		{
			pTempPath = malloc( strlen( pDefaultPath ) + 1 );
			if ( pTempPath != NULL )
			{
				strcpy( pTempPath, pDefaultPath );
				length = fileData.size;
			}
			else
			{
				ShowError( "Could not allocate %d bytes in GetDefaultApplicationPath()", strlen( pDefaultPath ) + 1 );
			}

			_findclose( hFile );
		}
		else
		{
			ShowError( "Cannot file default mapping file %s", pDefaultPath );
		}
	}

	/* See if a path to the file was found. */
	if ( pTempPath != NULL )
	{
		/* Open the mapping file and extract the path. */
		if ( ( fp = fopen( pTempPath, "rb" ) ) != NULL )
		{
			/* Allocate space to read the string into. */
			pApplicationPath = malloc( length + 1 );
			if ( pApplicationPath != NULL )
			{
				if ( fgets( pApplicationPath, length, fp ) != NULL )
				{
					/* Remove the '\n' at the end if there is one. */
					if ( pApplicationPath[ length - 2 ] == 0x0D )
					{
						// Windows
						pApplicationPath[ length - 2 ] = '\0';
					}
					else if ( pApplicationPath[ length - 1 ] == 0x0A )
					{
						// Linux
						pApplicationPath[ length - 1 ] = '\0';
					}
				}
				else
				{
					free( pApplicationPath );
					pApplicationPath = NULL;
				}
			}
			else
			{
				ShowError( "Could not allocate %d bytes in GetDefaultApplicationPath()", length + 1 );
			}

			fclose( fp );
		}
		else
		{
			ShowError( "Cannot open mapping file %s", pTempPath );
		}

		free( pTempPath );
	}

	return pApplicationPath;
}

/*
 * Gets the default location where the DirectoryMappingFile exists.
 * The pointer returned from this function does not need to be freed.
 */
static const char *GetDefaultMappingFile()
{
	static char DefaultMappingPath[ MAX_PATH_SIZE + 1 ] = {'\0'};
	char *pProfile;

	/* Only do this once. */
	if ( DefaultMappingPath[ 0 ] == '\0' )
	{

#ifdef WIN32

		if ( ( pProfile = getenv( WIN_ALL_USERS_PROFILE ) ) != NULL )
		{
			strcpy( DefaultMappingPath, pProfile );
			strcat( DefaultMappingPath, DEFAULT_WINDOWS_MAPPING_DIR );
			strcat( DefaultMappingPath, MAPPING_FILE );
		}
		else
		{
			ShowError( "Cannot get environment variable %s", WIN_ALL_USERS_PROFILE );
		}

#else

		strcpy( DefaultMappingPath, DEFAULT_LINUX_MAPPING_DIR );
		strcat( DefaultMappingPath, MAPPING_FILE );

#endif

	}

	return ( const char * )DefaultMappingPath;
}

/*
 * Parses the command line arguments and sets the values in the object.
 */
static void ParseConfigurationParameters( Manager *pManager, int argsLength, const char *args[] )
{
	int i;

	/* Skip the first argument as it will be the program name. */
	for ( i = 1; i < argsLength; ++i )
	{
		if ( !_stricmp( args[ i ], "-p" ) || !_stricmp( args[ i ], "--port" ) )
		{
			if ( ( i + 1 ) < argsLength )
			{
				SetPort( pManager, args[ ++i ] );
			}
			else
			{
				ShowError( "Invalid command line parameters. No port or range was specified" );
			}
		}
		else if ( !_stricmp( args[ i ], "-d" ) || !_stricmp( args[ i ], "--datadir" ) )
		{
			if ( ( i + 1 ) < argsLength )
			{
				SetDataPath( pManager, args[ ++i ] );
			}
			else
			{
				ShowError( "Invalid command line parameters. No store path was specified" );
			}
		}
		else if ( !_stricmp( args[ i ], "-a" ) || !_stricmp( args[ i ], "--apppath" ) )
		{
			if ( ( i + 1 ) < argsLength )
			{
				SetApplicationPath( pManager, args[ ++i ] );
			}
			else
			{
				ShowError( "Invalid command line parameters. No application path was specified" );
			}
		}
		else if ( !_stricmp( args[ i ], "-i" ) || !_stricmp( args[ i ], "--isserver" ) )
		{
			pManager->flags.IsServer = 1;
		}
		else if ( !_stricmp( args[ i ], "-s" ) || !_stricmp( args[ i ], "--showconsole" ) )
		{
			pManager->flags.ShowConsole = 1;
		}
		else if ( !_stricmp( args[ i ], "-v" ) || !_stricmp( args[ i ], "--verbose" ) )
		{
			pManager->flags.Verbose = 1;
		}
	}
}

/*
 * Displays errors to the console.
 */
static void ShowError( const char *format, ... )
{
	char buffer[ 256 ];
	va_list list;

	va_start( list, format );
	vsprintf( buffer, format, list );
	va_end( list );

	perror( buffer );
}




/*
 * Method:	Allocates a Manager object and populates it with the defaults.
 * Returns:	A pointer to a Manager object.
 */
Manager *AllocateManager()
{
	Manager *pManager = malloc( sizeof( Manager ) );
	if ( pManager != NULL )
	{
		pManager->applicationPath = GetDefaultApplicationPath();
		pManager->simiasDataPath = NULL;
		pManager->webServiceUri = NULL;
		pManager->port = NULL;
		pManager->zeroFlags = 0;
	}

	return pManager;

}	/*-- End of AllocateManager() --*/

/*
 * Method:	Allocates a Manager object and populates it with the specified
 * command line arguments.
 * Returns:	A pointer to a Manager object.
 */
Manager *AllocateManagerWithArgs( int argsLength, const char *args[] )
{
	Manager *pManager = AllocateManager();
	if ( pManager != NULL )
	{
		ParseConfigurationParameters( pManager, argsLength, args );
	}

	return pManager;

}	/*-- End of AllocateManagerWithArgs() --*/

/*
 * Frees the specified Manager object.
 */
void FreeManager( Manager *pManager )
{
	if ( pManager != NULL )
	{
		if ( pManager->applicationPath != NULL )
		{
			free( ( char * )pManager->applicationPath );
		}

		if ( pManager->simiasDataPath != NULL )
		{
			free( ( char * )pManager->simiasDataPath );
		}

		if ( pManager->webServiceUri != NULL )
		{
			free( ( char * )pManager->webServiceUri );
		}

		if ( pManager->port != NULL )
		{
			free( ( char * )pManager->port );
		}

		/* Clear out the Manager structure before freeing the memory. */
		memset( pManager, 0, sizeof( Manager ) );
		free( pManager );
	}
}

/*
 * Gets the path to the Simias.exe application.
 */
const char *GetApplicationPath( Manager *pManager )
{
	return pManager->applicationPath;
}

/*
 * Gets the path to the simias data directory.
 */
const char *GetDataPath( Manager *pManager )
{
	return pManager->simiasDataPath;
}

/*
 * Gets whether to run in a server configuration.
 */
char GetIsServer( Manager *pManager )
{
	return ( char )( pManager->flags.IsServer ? 1 : 0 );
}

/*
 * Gets the web service listener port or range.
 */
const char *GetPort( Manager *pManager )
{
	return pManager->port;
}

/*
 * Gets whether to show console output for the Simias process.
 */
char GetShowConsole( Manager *pManager )
{
	return ( char )( pManager->flags.ShowConsole ? 1 : 0 );
}

/*
 * Gets the url for the local web service.
 */
const char *GetWebServiceUrl( Manager *pManager )
{
	return pManager->webServiceUri;
}

/*
 * Gets whether to print extra informational messages.
 */
char GetVerbose( Manager *pManager )
{
	return ( char )(pManager->flags.Verbose ? 1 : 0 );
}

/*
 * Sets a new path to the Simias.exe application.
 */
void SetApplicationPath( Manager *pManager, const char *pApplicationPath )
{
	const char *pTemp = pManager->applicationPath;

	pManager->applicationPath = malloc( strlen( pApplicationPath + 1 ) );
	if ( pManager->applicationPath != NULL )
	{
		strcpy( ( char * )pManager->applicationPath, pApplicationPath );
		free( ( char * )pTemp );
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetApplicationPath()", strlen( pApplicationPath ) + 1 );
		pManager->applicationPath = pTemp;
	}
}

/*
 * Sets a new path to the simias data directory.
 */
void SetDataPath( Manager *pManager, const char *pDataPath )
{
	const char *pTemp = pManager->simiasDataPath;

	pManager->simiasDataPath = malloc( strlen( pDataPath + 1 ) );
	if ( pManager->simiasDataPath != NULL )
	{
		strcpy( ( char * )pManager->simiasDataPath, pDataPath );
		free( ( char * )pTemp );
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetDataPath()", strlen( pDataPath ) + 1 );
		pManager->simiasDataPath = pTemp;
	}
}

/*
 * Sets whether to run in a server configuration.
 */
void SetIsServer( Manager *pManager, char isServer )
{
	pManager->flags.IsServer = isServer ? 1 : 0;
}

/*
 * Sets a new web service listener port or range.
 */
void SetPort( Manager *pManager, const char *pPort )
{
	const char *pTemp = pManager->port;

	pManager->port = malloc( strlen( pPort + 1 ) );
	if ( pManager->port != NULL )
	{
		strcpy( ( char * )pManager->port, pPort );
		free( ( char * )pTemp );
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetPort()", strlen( pPort ) + 1 );
		pManager->port = pTemp;
	}
}

/*
 * Sets whether to show console output for the Simias process.
 */
void SetShowConsole( Manager *pManager, char showConsole )
{
	pManager->flags.ShowConsole = showConsole ? 1 : 0;
}

/*
 * Sets whether to print extra informational messages.
 */
void SetVerbose( Manager *pManager, char verbose )
{
	pManager->flags.Verbose = verbose ? 1 : 0;
}


