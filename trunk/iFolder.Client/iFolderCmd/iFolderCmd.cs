// project created on 4/17/2006 at 10:44 AM
using System;

using iFolder.Client;

class iFolderCmd
{
	private string hostUrl = "ifolder3pilot.novell.com";
	private string user = String.Empty;
	private string pwd = String.Empty;
	private string command = String.Empty;
	private string action = String.Empty;
	private string domainid = String.Empty;
	private string ifolderid = String.Empty;
	private string localpath = String.Empty;
	private string name = String.Empty;
	private string description = String.Empty;
	private bool savePassword = false;

	private void DisplayUsage()
	{
		Console.WriteLine( "iFolderCmd command (domain|ifolder|run)" );
		Console.WriteLine( "   domain list");
		Console.WriteLine( "   domain add --host <host name> --user <user name> --pwd <user's password> --save-password" );
		Console.WriteLine( "   domain remove --domainid <domain's unique ID>" );
		Console.WriteLine( "" );
		Console.WriteLine( "   ifolder list-available --domainid <domain's unique ID>");
		Console.WriteLine( "   ifolder list-subscribed --domainid <domain unique ID>" );
		Console.WriteLine( "   ifolder create --domainid <domain unique ID> --name <iFolder's name> --description <iFolder description>" );
		Console.WriteLine( "   ifolder delete --domainid <domain unique ID> --ifolderid <ifolder unique ID>" );
		Console.WriteLine( "   ifolder subscribe --domainid <domain's unique ID> --ifolderid <ifolder's unique id> --pwd <user's password> --path <local file system path>" );
		Console.WriteLine( "   ifolder unsubscribe --ifolderid <ifolder's unique id>" );
		Console.WriteLine( "" );
		Console.WriteLine( "   run --domain <domainid:password>" );
	}
	
	private void ParseArguments( string[] args )
	{
		if ( args.Length > 0 )
		{
			command = args[0].ToLower();
			if ( args.Length > 1 )
			{
				action = args[1].ToLower();
			}

			for( int i = 1; i < args.Length; i++ )
			{
				switch( args[ i ].ToLower() )
				{
					case "--name":
					{
						if ( i + 1 < args.Length )
						{
							name = args[ ++i ];
						}
						break;
					}

					case "--description":
					{
						if ( i + 1 < args.Length )
						{
							description = args[ ++i ];
						}
						break;
					}

					case "--domainid":
					{
						if ( i + 1 < args.Length )
						{
							domainid = args[ ++i ];
						}
						break;
					}

					case "--ifolderid":
					{
						if ( i + 1 < args.Length )
						{
							ifolderid = args[ ++i ];
						}
						break;
					}

					case "--path":
					{
						if ( i + 1 < args.Length )
						{
							localpath = args[ ++i ];
						}
						break;
					}

					case "--host":
					{
						if ( i + 1 < args.Length )
						{
							hostUrl = args[ ++i ];
						}
						break;
					}
					
					case "--user":
					{
						if ( i + 1 < args.Length )
						{
							user = args[ ++i ];
						}
						break;
					}
					
					case "--pwd":
					{
						if ( i + 1 < args.Length )
						{
							pwd = args[ ++i ];
						}
						break;
					}

					case "--save-password":
					{
						savePassword = true;
						break;
					}

					case "--domain":
					{
						if ( i + 1 < args.Length )
						{
							string[] comps = args[ ++i ].Split( new char[] { ':' } );
							domainid = comps[ 0 ];
							if ( comps.Length > 1 )
							{
								pwd = comps[ 1 ];
							}
						}
						break;
					}
				}
			}
		}
	}

	public static void Main( string[] args )
	{
		iFolderCmd cmd = new iFolderCmd();
		if ( args.Length > 0 )
		{
			cmd.ParseArguments( args );
		}
		else
		{
			cmd.DisplayUsage();
			return;
		}

		if ( cmd.command == "domain" )
		{
			if ( cmd.action == "list" )
			{
				Domain[] domains = iFolder.Client.Domain.GetDomains();
				if ( domains.Length > 0 )
				{
					Console.WriteLine( "Configured Domains" );
				}
				foreach( Domain domain in domains )
				{
					Console.WriteLine();
					Console.WriteLine( "Name:        " + domain.Name );
					Console.WriteLine( "ID:          " + domain.ID );
					Console.WriteLine( "Description: " + domain.Description );
					Console.WriteLine( "Host:        " + domain.Host.Address );
					Console.WriteLine( "User:        " + domain.User );
					Console.WriteLine( "UserID:      " + domain.UserID );
				}
			}
			else if ( cmd.action == "add" )
			{
				try
				{
					iFolder.Client.Host host = new iFolder.Client.Host( cmd.hostUrl );
					Domain ifolderDomain = iFolder.Client.Domain.Add( host, cmd.user, cmd.pwd, true, true );
					Console.WriteLine( "Domain Added" );
					Console.WriteLine( "   ID: " + ifolderDomain.ID );
					Console.WriteLine( "   Name: " + ifolderDomain.Name );
					if ( ifolderDomain.Description != null && ifolderDomain.Description != String.Empty )
					{
						Console.WriteLine( "   Description: " + ifolderDomain.Description );
					}
				}
				catch( System.Exception addex )
				{
					Console.WriteLine( "Exception generated while adding a domain" );
					Console.WriteLine( addex.Message );
				}
			}
			else if ( cmd.action == "remove" && cmd.domainid != String.Empty )
			{
				try
				{
					iFolder.Client.Domain.Remove( cmd.domainid );
					Console.WriteLine( "Domain: " + cmd.domainid + " successfully removed" );
				}
				catch( iFolder.Client.UnknownDomainException ude )
				{
					Console.WriteLine( "Failed to find: " + ude.DomainID );
				}
				catch( System.Exception dr )
				{
					Console.WriteLine( "An exception was generated removing domain: " + cmd.domainid );
					Console.WriteLine( dr.Message );
				}
			}
		}
		else if ( cmd.command == "ifolder" )
		{
			if ( cmd.action == "list-available" )
			{
				Domain domain = 
					( cmd.domainid != String.Empty ) 
						? iFolder.Client.Domain.GetDomainByID( cmd.domainid )
						: iFolder.Client.Domain.GetDefaultDomain(); 

				iFolder.Client.iFolder[] ifolders = iFolder.Client.iFolder.GetAvailable( domain );
				if ( ifolders.Length > 0 )
				{
					Console.WriteLine( "Available iFolders" );
					foreach( iFolder.Client.iFolder ifldr in ifolders )
					{
						Console.WriteLine();
						Console.WriteLine( "Name:        " + ifldr.Name );
						Console.WriteLine( "ID:          " + ifldr.ID );
						Console.WriteLine( "Description: " + ifldr.Description );
						Console.WriteLine( "Access:      " + ifldr.Rights.ToString() );
						Console.WriteLine( "Owner ID:    " + ifldr.OwnerID );
						Console.WriteLine( "Owner Name:  " + ifldr.OwnerName );
					}
				}
			}
			if ( cmd.action == "list-subscribed" )
			{
				iFolder.Client.iFolder[] ifolders = iFolder.Client.iFolder.GetSubscribed();
				if ( ifolders.Length > 0 )
				{
					Console.WriteLine( "Subscribed iFolders" );
					foreach( iFolder.Client.iFolder ifldr in ifolders )
					{
						Console.WriteLine();
						Console.WriteLine( "Name:        " + ifldr.Name );
						Console.WriteLine( "ID:          " + ifldr.ID );
						Console.WriteLine( "Description: " + ifldr.Description );
						Console.WriteLine( "Access:      " + ifldr.Rights.ToString() );
						Console.WriteLine( "Local Path:  " + ifldr.LocalPath );
						Console.WriteLine( "Owner ID:    " + ifldr.OwnerID );
						Console.WriteLine( "Owner Name:  " + ifldr.OwnerName );
					}
				}
			}
			else if ( cmd.action == "subscribe" )
			{
				Domain domain = 
					( cmd.domainid != String.Empty ) 
						? iFolder.Client.Domain.GetDomainByID( cmd.domainid )
						: iFolder.Client.Domain.GetDefaultDomain(); 

				if ( cmd.ifolderid != String.Empty )
				{
					iFolder.Client.iFolder subscribediFolder =
						iFolder.Client.iFolder.Subscribe( domain, cmd.ifolderid );
				}
				else
				{
					Console.WriteLine( "Missing ifolderid command line parameter" );
				}
			}
			else if ( cmd.action == "create" )
			{
				Domain domain = 
					( cmd.domainid != String.Empty ) 
						? iFolder.Client.Domain.GetDomainByID( cmd.domainid )
						: iFolder.Client.Domain.GetDefaultDomain(); 

				if ( cmd.name != String.Empty )
				{
					iFolder.Client.iFolder creatediFolder =
						iFolder.Client.iFolder.Create( domain, cmd.name, cmd.description, String.Empty );
				}
				else
				{
					Console.WriteLine( "Missing \"name\" command line parameter" );
				}
			}
			else if ( cmd.action == "delete" )
			{
				Domain domain = 
					( cmd.domainid != String.Empty ) 
					? iFolder.Client.Domain.GetDomainByID( cmd.domainid )
					: iFolder.Client.Domain.GetDefaultDomain(); 

				if ( cmd.ifolderid != String.Empty )
				{
					iFolder.Client.iFolder.Delete( domain, cmd.ifolderid );
				}
				else
				{
					Console.WriteLine( "Missing \"ifolderid\" command line parameter" );
				}
			}
		}
	}
}