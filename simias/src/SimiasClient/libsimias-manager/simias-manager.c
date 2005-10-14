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

#ifdef WIN32
#include <windows.h>
#endif

#include <stdio.h>
#include <io.h>
#include <direct.h>
#include <process.h>
#include <stdarg.h>
#include <stdlib.h>
#include <string.h>

#define MAX_PATH_SIZE				260
#define MAX_STDOUT_BUFFER_SIZE		1024

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
 * Forward declaration of functions.
 */

#ifdef WIN32
static BOOL ReadChildStdoutPipe( Manager *pManager, HANDLE hRead );
static BOOL StartChildProcess( Manager *pManager );
static BOOL StopChildProcess( Manager *pManager );
#endif

static const char *GetDefaultApplicationPath();
static const char *GetDefaultMappingFile();
static void SetWebServiceUri( Manager *pManager, const char *pWebServiceUri );
static void ShowError( const char *format, ... );

void SetApplicationPath( Manager *pManager, const char *pApplicationPath );
void SetDataPath( Manager *pManager, const char *pDataPath );
void SetIsServer( Manager *pManager, char isServer );
void SetWebPort( Manager *pManager, const char *pPort );
void SetShowConsole( Manager *pManager, char showConsole );
void SetVerbose( Manager *pManager, char verbose );


/*
 * Buffer used to show errors.
 */
static char errorBuffer[ 512 ];


#ifdef WIN32

/*
 * Reads the child process's redirected stdout handle to get the output
 * from the child process.
 */
static BOOL ReadChildStdoutPipe( Manager *pManager, HANDLE hRead )
{
	BOOL bStatus;
	char *pBuffer;
	char *pCmd;
	char *pDataPath;
	unsigned long bytesRead = 0;

	/* Allocate a buffer to read the data from. */
	pBuffer = malloc( MAX_STDOUT_BUFFER_SIZE );
	if ( pBuffer != NULL )
	{
		/* Read from the dup handle. */
		bStatus = ReadFile( hRead, pBuffer, MAX_STDOUT_BUFFER_SIZE, &bytesRead, NULL );
		if ( bStatus && ( bytesRead > 0 ) )
		{
			/* NULL append the buffer. */
			pBuffer[ bytesRead ] = '\0';

			/* Copy the first line to the web service uri. */
			pCmd = strchr( pBuffer, '\r' );
			if ( pCmd != NULL )
			{
				*pCmd = '\0';
				SetWebServiceUri( pManager, pBuffer );

				/* Copy the next line to the simias data path */
				/* Skip over the '\r\n'. */
				pDataPath = pCmd + 2;
				pCmd = strchr( pDataPath, '\r' );
				if ( pCmd != NULL )
				{
					*pCmd = '\0';
					SetDataPath( pManager, pDataPath );
				}
			}
		}
		else
		{
			/* No data was written. */
			bStatus = FALSE;
		}

		free( pBuffer );
	}
	else
	{
		ShowError( "Cannot allocate %d bytes in ReadChildStdoutPipe()", MAX_STDOUT_BUFFER_SIZE );
		bStatus = FALSE;
	}

	return bStatus;

}	/*-- End of ReadChildStdoutPipe() --*/

/*
 * Starts the Simias.exe child process.
 */
static BOOL StartChildProcess( Manager *pManager )
{
	BOOL bStatus;
	char *pArgs;
	PROCESS_INFORMATION pi;
	STARTUPINFO si;

	/* Allocate space for the argument string. */
	pArgs = malloc( 1024 );
	if ( pArgs != NULL )
	{
		/* First parameter is the application name */
		strcpy( pArgs, "\"" );
		strcat( pArgs, pManager->applicationPath );
		strcat( pArgs, "\"" );

		/* Set the data path if specified */
		if ( pManager->simiasDataPath != NULL )
		{
			strcat( pArgs, " --datadir \"" );
			strcat( pArgs, pManager->simiasDataPath );
			strcat( pArgs, "\"" );
		}

		/* Set the port if specified */
		if ( pManager->port != NULL )
		{
			strcat( pArgs, " --port " );
			strcat( pArgs, pManager->port );
		}

		/* Set the configuration type */
		if ( pManager->flags.IsServer )
		{
			strcat( pArgs, " --runasserver" );
		}

		/* Set whether to show the output console */
		if ( pManager->flags.ShowConsole )
		{
			strcat( pArgs, " --showconsole" );
		}

		/* Set whether to show the extra information */
		if ( pManager->flags.Verbose )
		{
			strcat( pArgs, " --verbose" );
		}

		/* Initialize the input structures. */
		memset( &pi, 0, sizeof( pi ) );
		memset( &si, 0, sizeof( si ) );
		si.cb = sizeof( si );

		/* Create the child process. */
		bStatus = CreateProcess( NULL, pArgs, NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi );
		if ( bStatus )
		{
			/* Wait for the process to terminate then close the handles. */
			WaitForSingleObject( pi.hProcess, INFINITE );
			CloseHandle( pi.hProcess );
			CloseHandle( pi.hThread );
		}
		else
		{
			ShowError( "Failed to create Simias.exe process - %d.", GetLastError() );
		}

		free( pArgs );
	}
	else
	{
		ShowError( "Cannot allocate %d bytes in StartChildProcess()", 1024 );
		bStatus = FALSE;
	}

	return bStatus;

}	/*-- End of StartChildProcess() --*/

/*
 * Stops the Simias.exe child process.
 */
static BOOL StopChildProcess( Manager *pManager )
{
	BOOL bStatus;
	char *pArgs;
	PROCESS_INFORMATION pi;
	STARTUPINFO si;

	/* Allocate space for the argument string. */
	pArgs = malloc( 1024 );
	if ( pArgs != NULL )
	{
		/* First parameter is the application name */
		strcpy( pArgs, "\"" );
		strcat( pArgs, pManager->applicationPath );
		strcat( pArgs, "\"" );

		/* Set the data path if specified */
		if ( pManager->simiasDataPath != NULL )
		{
			strcat( pArgs, " --datadir \"" );
			strcat( pArgs, pManager->simiasDataPath );
			strcat( pArgs, "\"" );
		}

		/* Set whether to show the extra information */
		if ( pManager->flags.Verbose )
		{
			strcat( pArgs, " --verbose" );
		}

		/* Add the stop command. */
		strcat( pArgs, " --stop" );

		/* Initialize the input structures. */
		memset( &pi, 0, sizeof( pi ) );
		memset( &si, 0, sizeof( si ) );
		si.cb = sizeof( si );

		/* Create the child process. */
		bStatus = CreateProcess( NULL, pArgs, NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi );
		if ( bStatus )
		{
			/* Wait for the process to terminate then close the handles. */
			WaitForSingleObject( pi.hProcess, INFINITE );
			CloseHandle( pi.hProcess );
			CloseHandle( pi.hThread );
		}
		else
		{
			ShowError( "Failed to create Simias.exe process - %d.", GetLastError() );
		}

		free( pArgs );
	}
	else
	{
		ShowError( "Cannot allocate %d bytes in StopChildProcess()", 1024 );
		bStatus = FALSE;
	}

	return bStatus;

}	/*-- End of StopChildProcess() --*/

#endif	/*-- WIN32 --*/

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

}	/*-- End of GetDefaultApplicationPath() --*/

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

}	/*-- End of GetDefaultMappingFile() --*/

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
				SetWebPort( pManager, args[ ++i ] );
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

}	/*-- End of ParseConfigurationParameters() --*/

/*
 * Sets a new uri in the webServiceUri.
 */
static void SetWebServiceUri( Manager *pManager, const char *pWebServiceUri )
{
	const char *pTemp = pManager->webServiceUri;

	pManager->webServiceUri = malloc( strlen( pWebServiceUri ) + 1 );
	if ( pManager->webServiceUri != NULL )
	{
		strcpy( ( char * )pManager->webServiceUri, pWebServiceUri );
		if ( pTemp != NULL )
		{
			free( ( char * )pTemp );
		}
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetWebServiceUri()", strlen( pWebServiceUri ) + 1 );
		pManager->webServiceUri = pTemp;
	}

}	/*-- End of SetWebServiceUri() --*/

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

}	/*-- End of ShowError() --*/




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

}	/*-- End of FreeManager() --*/

/*
 * Gets the path to the Simias.exe application.
 */
const char *GetApplicationPath( Manager *pManager )
{
	return pManager->applicationPath;

}	/*-- End of GetApplicationPath() --*/

/*
 * Gets the path to the simias data directory.
 */
const char *GetDataPath( Manager *pManager )
{
	return pManager->simiasDataPath;

}	/*-- End of GetDataPath() --*/

/*
 * Gets whether to run in a server configuration.
 */
char GetIsServer( Manager *pManager )
{
	return ( char )( pManager->flags.IsServer ? 1 : 0 );

}	/*-- End of GetIsServer() --*/

/*
 * Gets the web service listener port or range.
 */
const char *GetWebPort( Manager *pManager )
{
	return pManager->port;

}	/*-- End of GetWebPort() --*/

/*
 * Gets whether to show console output for the Simias process.
 */
char GetShowConsole( Manager *pManager )
{
	return ( char )( pManager->flags.ShowConsole ? 1 : 0 );

}	/*-- End of GetShowConsole() --*/

/*
 * Gets the url for the local web service.
 */
const char *GetWebServiceUrl( Manager *pManager )
{
	return pManager->webServiceUri;

}	/*-- End of GetWebServiceUrl() --*/

/*
 * Gets whether to print extra informational messages.
 */
char GetVerbose( Manager *pManager )
{
	return ( char )(pManager->flags.Verbose ? 1 : 0 );

}	/*-- End of GetVerbose() --*/

/*
 * Sets a new path to the Simias.exe application.
 */
void SetApplicationPath( Manager *pManager, const char *pApplicationPath )
{
	const char *pTemp = pManager->applicationPath;

	pManager->applicationPath = malloc( strlen( pApplicationPath ) + 1 );
	if ( pManager->applicationPath != NULL )
	{
		strcpy( ( char * )pManager->applicationPath, pApplicationPath );
		if ( pTemp != NULL )
		{
			free( ( char * )pTemp );
		}
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetApplicationPath()", strlen( pApplicationPath ) + 1 );
		pManager->applicationPath = pTemp;
	}

}	/*-- End of SetApplicationPath() --*/

/*
 * Sets a new path to the simias data directory.
 */
void SetDataPath( Manager *pManager, const char *pDataPath )
{
	const char *pTemp = pManager->simiasDataPath;

	pManager->simiasDataPath = malloc( strlen( pDataPath ) + 1 );
	if ( pManager->simiasDataPath != NULL )
	{
		strcpy( ( char * )pManager->simiasDataPath, pDataPath );
		if ( pTemp != NULL )
		{
			free( ( char * )pTemp );
		}
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetDataPath()", strlen( pDataPath ) + 1 );
		pManager->simiasDataPath = pTemp;
	}

}	/*-- End of SetDataPath() --*/

/*
 * Sets whether to run in a server configuration.
 */
void SetIsServer( Manager *pManager, char isServer )
{
	pManager->flags.IsServer = isServer ? 1 : 0;

}	/*-- End of SetIsServer() --*/

/*
 * Sets a new web service listener port or range.
 */
void SetWebPort( Manager *pManager, const char *pPort )
{
	const char *pTemp = pManager->port;

	pManager->port = malloc( strlen( pPort ) + 1 );
	if ( pManager->port != NULL )
	{
		strcpy( ( char * )pManager->port, pPort );
		if ( pTemp != NULL )
		{
			free( ( char * )pTemp );
		}
	}
	else
	{
		ShowError( "Could not allocate %d bytes in SetWebPort()", strlen( pPort ) + 1 );
		pManager->port = pTemp;
	}

}	/*-- End of SetWebPort() --*/

/*
 * Sets whether to show console output for the Simias process.
 */
void SetShowConsole( Manager *pManager, char showConsole )
{
	pManager->flags.ShowConsole = showConsole ? 1 : 0;

}	/*-- End of SetShowConsole() --*/

/*
 * Sets whether to print extra informational messages.
 */
void SetVerbose( Manager *pManager, char verbose )
{
	pManager->flags.Verbose = verbose ? 1 : 0;

}	/*-- End of SetVerbose() --*/

/*
 * Starts the simias process running.
 */
const char *Start( Manager *pManager )
{
	const char *pWebServiceUri = NULL;

#ifdef WIN32

	BOOL bStatus;
	HANDLE hReadStdout;
	HANDLE hReadStdoutDup;
	HANDLE hSaveStdout;
	HANDLE hWriteStdout;
	SECURITY_ATTRIBUTES sa;

	/* Setup the pipe to be inheritable */
	sa.nLength = sizeof( SECURITY_ATTRIBUTES );
	sa.bInheritHandle = TRUE;
	sa.lpSecurityDescriptor = NULL;

	/* Save the current stdout handle. */
	hSaveStdout = GetStdHandle( STD_OUTPUT_HANDLE );

	/* Create a pipe for the child process's STDOUT. */
	bStatus = CreatePipe( &hReadStdout, &hWriteStdout, &sa, 0 );
	if ( bStatus )
	{
		/* Set the write-end of the pipe to be STDOUT for this process. */
		bStatus = SetStdHandle( STD_OUTPUT_HANDLE, hWriteStdout );
		if ( bStatus )
		{
			/* Create a noninheiritable read handle and close the inheritable read handle. */
			bStatus = DuplicateHandle( 
				GetCurrentProcess(), 
				hReadStdout, 
				GetCurrentProcess(),
				&hReadStdoutDup,
				0,
				FALSE,
				DUPLICATE_SAME_ACCESS | DUPLICATE_CLOSE_SOURCE );

			if ( bStatus )
			{
				/* Create the child process. */
				bStatus = StartChildProcess( pManager );
				if ( bStatus )
				{
					/* Restore the saved stdout handle. */
					bStatus = SetStdHandle( STD_OUTPUT_HANDLE, hSaveStdout );
					if ( !bStatus )
					{
						ShowError( "Failed to restore stdout handle." );
					}

					/* Close the write stdout handle. */
					CloseHandle( hWriteStdout );

					/* Read from the dup handle. */
					bStatus = ReadChildStdoutPipe( pManager, hReadStdoutDup );
					if ( bStatus )
					{
						/* Return the uri on success. */
						pWebServiceUri = pManager->webServiceUri;
					}
				}

				/* Close the dup read handle. */
				CloseHandle( hReadStdoutDup );
			}
			else
			{
				ShowError( "Could not duplicate read handle - %d.", GetLastError() );
			}
		}
		else
		{
			ShowError( "Could not redirect the stdout handle - %d.", GetLastError() );
		}
	}
	else
	{
		ShowError( "Failed to create redirected stdout handle - %d", GetLastError() );
	}

#else

	const char *ppArgs[ 9 ];
	const char **ppCurArg = &ppArgs[ 0 ];
	intptr_t status;

	/* First parameter is the application name */
	*ppCurArg++ = pManager->applicationPath;

	/* Set the data path if specified */
	if ( pManager->simiasDataPath != NULL )
	{
		*ppCurArg++ = "--datadir";
		*ppCurArg++ = pManager->simiasDataPath;
	}

	/* Set the port if specified */
	if ( pManager->port != NULL )
	{
		*ppCurArg++ = "--port";
		*ppCurArg++ = pManager->port;
	}

	/* Set the configuration type */
	if ( pManager->flags.IsServer )
	{
		*ppCurArg++ = "--runasserver";
	}

	/* Set whether to show the output console */
	if ( pManager->flags.ShowConsole )
	{
		*ppCurArg++ = "--showconsole";
	}

	/* Set whether to show the extra information */
	if ( pManager->flags.Verbose )
	{
		*ppCurArg++ = "--verbose";
	}

	/* End of the argument list */
	*ppCurArg = NULL;

	status = _spawnv( _P_WAIT, pManager->applicationPath, ppArgs );
	if ( status == 0 )
	{
	}

#endif

	return pWebServiceUri;

}	/*-- End of Start() --*/

/*
 * Stops the simias process from running.
 */
int Stop( Manager *pManager )
{
	int status;

#ifdef WIN32

	/* Stop the child process. */
	status = StopChildProcess( pManager ) ? 1 : 0;

#else
#endif

	return status;

}	/*-- End of Stop() --*/

