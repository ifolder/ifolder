/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Boyd Timothy <btimothy@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Net;
using Gtk;
using Gnome;
using Novell.iFolder;
using Simias.Client;
using Novell.iFolder.Controller;

namespace Novell.iFolder.Nautilus
{
	public class NautilusiFolder
	{
		private static SimiasEventBroker	simiasEventBroker;
		private static iFolderWebService	ifws;
		private static SimiasWebService	simws;
		private static iFolderData		ifdata;
		private static Manager			simiasManager;
		private static DomainController	domainController;
		private static bool				forceShutdown = false;
		
		public static int Main (string[] args)
		{
			// Don't do anything if nothing was specified to do
			if (args.Length == 0)
				return 0;
			for(int i=0;i<args.Length;i++)
				Console.WriteLine(args[i]);

			// Make sure this process is a gnome program
			Gnome.Program program = 
				new Program ("Nautilus-Extension-UI", "0.1.0", Modules.UI, args);
				
			// Get the localized strings loaded
			Util.InitCatalog();
			
			switch (args [0]) {
				case "revert":
					return showRevertiFolderDialog(args);
				case "share":
					return showShareDialog (args);
				case "properties":
					return showPropertiesDialog (args);
				case "help":
					return showHelp (args);
				case "create":
					StartSimias(args);
					return showCreateDialog(args);
			}
			
			program.Run ();
			return 0;
		}

        ///<summary>
        /// Start Simias
        ///</summary>
        ///<param name="args">Array of strings as arguments</param>        
		public static void StartSimias(string[] args)
		{
			bool simiasRunning = false;

			simiasManager = Util.CreateSimiasManager(args);
			
			simiasManager.Start();
			
			string localServiceUrl = simiasManager.WebServiceUri.ToString();
			ifws = new iFolderWebService();
			ifws.Url = localServiceUrl + "/iFolder.asmx";
			LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
			
			simws = new SimiasWebService();
			simws.Url = localServiceUrl + "/Simias.asmx";
			LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			
			while (!simiasRunning)
			{
				try
				{
					ifws.Ping();
					simiasRunning = true;
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				
				if (forceShutdown)
					ForceQuit();
				
				// Wait and ping again
				System.Threading.Thread.Sleep(10);
			}
			
			if (forceShutdown)
				ForceQuit();
			else
			{
				try
				{
					simiasEventBroker = SimiasEventBroker.GetSimiasEventBroker();
					
					// set up to have data ready for events
					ifdata = iFolderData.GetData();
					
					domainController = DomainController.GetDomainController();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					ifws = null;
					ForceQuit();
				}
			}
		}

        ///<summary>
        /// Force quit simias
        ///</summary>
        public static void ForceQuit()
		{
			try
			{
				simiasManager.Stop();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
			
			System.Environment.Exit(-1);
		}

        ///<summary>
        /// Stop Simias
        ///</summary>
        ///<returns>0 on success else -1</returns>
		public static int StopSimias()
		{
			try
			{
				simiasManager.Stop();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return -1;
			}

			return 0;
		}
		
		///<summary>
        /// Display ifolder share dialog
        ///</summary>
        ///<param name="args">String array of arguments</param>
        ///<returns>Status of stop simias else -1</returns>
        private static int showShareDialog (string[] args)        
		{
			if (args.Length < 2) {
				Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}

			StartSimias(args);

			// args[1] is domain ID....

			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1], simiasManager);
			propsDialog.CurrentPage = 1;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();

			return StopSimias();
		}

		private static void RemoveSelectedFolderHandler(string path)
		{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					null,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					Util.GS("Revert this iFolder back to a normal folder?"),
					Util.GS("The folder will still be on your computer, but it will no longer synchronize with the iFolder Server."));

				CheckButton deleteFromServerCB;

				iFolderWeb ifolder = ifws.GetiFolderByLocalPath(path);
				bool IsMaster = (ifolder.CurrentUserID == ifolder.OwnerID);

				if (IsMaster)
					deleteFromServerCB = new CheckButton(Util.GS("Also _delete this iFolder from the server"));
				else
					deleteFromServerCB = new CheckButton(Util.GS("Also _remove my membership from this iFolder"));

				deleteFromServerCB.Sensitive = simws.GetDomainInformation(ifolder.DomainID).Authenticated;
				dialog.ExtraWidget = deleteFromServerCB;

				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
						if (ifolder != null)
						{
							ifws.RevertiFolder(ifolder.ID);
							if(deleteFromServerCB.Active)
							{
								if (IsMaster)
								{
									ifws.DeleteiFolder(ifolder.DomainID, ifolder.ID);
								}
								else
								{
									ifws.DeclineiFolderInvitation(ifolder.DomainID, ifolder.ID);
								}
							}

						}
					}
					catch(Exception e)
					{
						iFolderExceptionDialog ied =
							new iFolderExceptionDialog(
								null,
								e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
					}
				}
		}

		private static int showRevertiFolderDialog (string[] args)
		{
			if (args.Length < 2) {
				Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}
			StartSimias(args);
			RemoveSelectedFolderHandler(args[1]);
			return StopSimias();
		}
		
        ///<summary>
        /// Display properties dialog
        ///</summary>
        ///<param name="args">String array of arguments needed</param>
        ///<returns>Status of stop simias else -1</returns>
		private static int showPropertiesDialog (string[] args)
		{
			if (args.Length < 2) {
				Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}
			
			StartSimias(args);
			
			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1], simiasManager);
			propsDialog.CurrentPage = 0;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();

			return StopSimias();
		}

		///<summary>
	        /// Creation of iFolder Dialog  
        	///</summary>
	        ///<param name="args">String array of arguments needed</param>
		private static int showCreateDialog (string[] args)
                {

			DomainInformation[] domains = domainController.GetDomains();
                        if (domains.Length <= 0) return 0;        
                        string domainID = domains[0].ID;        
                        DomainInformation defaultDomain = domainController.GetDefaultDomain();
                        if (defaultDomain != null)
                                domainID = defaultDomain.ID;

			string path = args[1];
                        DragCreateDialog cd = new DragCreateDialog(null, domains, domainID, path, ifws);
			int rc = 0;
                        do
                        {
                        	rc = cd.Run();
				cd.Hide();

                                if (rc == (int)ResponseType.Ok)
                                {
					ShowPassPhraseDialog(  path, cd.DomainID, cd.Encrypted);
					break;
				} 
			}while(rc == (int)ResponseType.Ok); 
		
                        return 0;
                }
		
        ///<summary>
        /// Display Help
        ///</summary>
        ///<param name="args">String array of argument</param>
        ///<returns>0 on completion</returns>
		private static int showHelp (string[] args)
		{
			Util.ShowHelp(Util.HelpMainPage, null);
			return 0;
		}

		///<summary>
        /// Display passphrase dialog
        ///</summary>
        ///<param name="path">Path of ifolder</param>
        ///<param name="domain_id">Id of the domain</param>
        ///<param name="encrypted">Enctypred or not</param>
        ///<returns>0 on success else 1</returns>
		private static int ShowPassPhraseDialog(string path, string domain_id, bool encrypted)
		{
			Console.WriteLine(" The corresponding c# file is called");
			if(!encrypted)
			{
				iFolderWeb newiFolder = ifws.CreateiFolderInDomain(path, domain_id);
				if( newiFolder != null)
				{
					Console.WriteLine("Created iFolder");
					return 0;
				}
				Console.WriteLine("Error creating iFolder");
					return 1;
			}
			bool passphraseSet = false;
			try
			{
				passphraseSet = simws.IsPassPhraseSet(domain_id);
			}
			catch(Exception ex)
			{
				// Unable to create iFolder. Need to login..
			}
			bool passPhraseEntered = false;
			if(passphraseSet == true)
			{
				bool rememberOption = simws.GetRememberOption(domain_id);
				string passphrasecheck;
				passphrasecheck = simws.GetPassPhrase( domain_id);
				if(passphrasecheck == null || passphrasecheck == "")
					passPhraseEntered = ShowVerifyDialog( domain_id, simws);
				else
					passPhraseEntered = true;
			}
			else
			{
				passPhraseEntered = ShowEnterPassPhraseDialog(domain_id, simws);
			}
			if( passPhraseEntered == true)
			{
				iFolderWeb newiFolder = ifws.CreateiFolderInDomainEncr(path, domain_id, !encrypted, "BlowFish", simws.GetPassPhrase(domain_id));
				if( newiFolder != null)
					return 0;
				else
					return 1;
			}
//			string[] array = domainController.GetRAList( domain_id);
			return 0;
		}

        ///<summary>
        /// Display enter passphrase dialog
        ///</summary>
        ///<param name="DomainID">ID of the domain</param>
        ///<param name="simws">Reference to simias web service</param>
        ///<returns>Status else false</returns>
		private static bool ShowEnterPassPhraseDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;	
			EnterPassPhraseDialog epd = new EnterPassPhraseDialog(DomainID, simws);
			try
			{
			do
			{
				result = epd.Run();
				epd.Hide();
                                if( result == (int)ResponseType.Cancel || result == (int) ResponseType.DeleteEvent)
                                        break;
				if( epd.PassPhrase != epd.RetypedPassPhrase )
				{
					Console.WriteLine("PassPhrases do not match");
					// show an error message
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("PassPhrase mismatch"),
						Util.GS("The PassPhrase and retyped Passphrase are not same"),
						Util.GS("Enter the passphrase again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
				else
					break;
			}while( result != (int)ResponseType.Cancel);
                        if( result == (int)ResponseType.Cancel || result ==(int)ResponseType.DeleteEvent)
                        {
                                status = false;
                                simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
                        }
			
			else if( epd.PassPhrase == epd.RetypedPassPhrase)
			{
				// Check the recovery agent
				string publicKey = "";
				Status passPhraseStatus = simws.SetPassPhrase( DomainID, epd.PassPhrase, epd.RecoveryAgent, publicKey);
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					status = true;
					simws.StorePassPhrase( DomainID, epd.PassPhrase, CredentialType.Basic, epd.ShouldSavePassPhrase);
				}
				else 
				{
					// error setting the passphrase
					status = false;
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Error setting the Passphrase"),
						Util.GS("Unable to set the passphrase"),
						Util.GS("Try again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
			}
			}
			catch(Exception ex)
			{
                                iFolderMsgDialog dialog = new iFolderMsgDialog(
                                                                       null,
                                                                       iFolderMsgDialog.DialogType.Error,
                                                                       iFolderMsgDialog.ButtonSet.None,
                                                                       Util.GS("Unable to set the passphrase"),
                                                                       Util.GS(ex.Message),
                                                                       Util.GS("Please enter the passphrase again"));
                                dialog.Run();
                                dialog.Hide();
                                dialog.Destroy();
                                dialog = null;
				return false;
			}
			return status;
		}

        ///<summary>
        /// Show password verify dialog
        ///</summary>
        ///<param name="DomainID">ID of the domain</param>
        ///<param name="simws">Reference to simias web service</param>
        ///<returns>Status else false</returns>
		private static bool ShowVerifyDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;
			Status passPhraseStatus= null;
			VerifyPassPhraseDialog vpd = new VerifyPassPhraseDialog();
			// vpd.TransientFor = this;
			try
			{
			do
			{
				result = vpd.Run();
				vpd.Hide();
				// Verify PassPhrase..  If correct store passphrase and set a local property..
				if( result == (int)ResponseType.Ok)
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, vpd.PassPhrase);
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						// Display an error Message
						Console.WriteLine("Invalid Passphrase");
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Invalid Passphrase"),
							Util.GS("The Passphrase entered is invalid"),
							Util.GS("Please re-enter the passphrase"));
							dialog.Run();
							dialog.Hide();
							dialog.Destroy();
							dialog = null;
						passPhraseStatus = null;
					}
					else if(passPhraseStatus.statusCode == StatusCodes.Success)
						break;
				}
			}while( result != (int)ResponseType.Cancel && result !=(int)ResponseType.DeleteEvent);
			if( result == (int)ResponseType.Cancel || result == (int)ResponseType.DeleteEvent)
			{
				try
				{
					simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
					status = false;
				}
				catch(Exception e)
				{
					return false;
				}
			}
			else if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
			{
				try
				{
					simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
					status = true;
				}
				catch(Exception ex) 
				{
					return false;
				}
			}
			}
			catch(Exception e)
			{
				return false;
			}
			return status;
		}

        ///<summary>
        /// Check whether passphrase is available or not
        ///</summary>
        ///<param name="selectedDomain">Selected domain ID</param>
        ///<returns>True if passphrase available else false</returns>
		private static bool IsPassPhraseAvailable(string selectedDomain)
		{
			bool passPhraseStatus = false;;
			bool passphraseStatus = simws.IsPassPhraseSet(selectedDomain);
			if(passphraseStatus == true)
			{
				// if passphrase not given during login
				string passphrasecheck = simws.GetPassPhrase(selectedDomain);
				if( passphrasecheck == null || passphrasecheck =="")
				{
					passPhraseStatus = ShowVerifyDialog(selectedDomain, simws);
				}
				else
				{
					passPhraseStatus = true;
				}
			}
			else
			{
				// if passphrase is not set
				passPhraseStatus = ShowEnterPassPhraseDialog(selectedDomain, simws);
			}
			return passPhraseStatus;
		}
	}
}
