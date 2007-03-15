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
 *	Author: Boyd Timothy <btimothy@novell.com>
 ***********************************************************************/

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

			// Make sure this process is a gnome program
			Gnome.Program program = 
				new Program ("Nautilus-Extension-UI", "0.1.0", Modules.UI, args);
				
			// Get the localized strings loaded
			Util.InitCatalog();
			
			switch (args [0]) {
				case "share":
					return showShareDialog (args);
				case "properties":
					return showPropertiesDialog (args);
				case "help":
					return showHelp (args);
				case "create":
					StartSimias(args);
					return ShowPassPhraseDialog(args[1], args[2], (args[3] == "true"));
			}
			
			program.Run ();
			return 0;
		}
		
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
		
		private static int showHelp (string[] args)
		{
			Util.ShowHelp(Util.HelpMainPage, null);
			return 0;
		}
		
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

		private static bool ShowEnterPassPhraseDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;	
			EnterPassPhraseDialog epd = new EnterPassPhraseDialog(DomainID);
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
				Status passPhraseStatus = simws.SetPassPhrase( DomainID, Util.PadString(epd.PassPhrase, 16)/*epd.PassPhrase*/, epd.RecoveryAgent, publicKey);
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					status = true;
					simws.StorePassPhrase( DomainID, Util.PadString(epd.PassPhrase, 16)/*epd.PassPhrase*/, CredentialType.Basic, epd.ShouldSavePassPhrase);
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
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, Util.PadString(vpd.PassPhrase, 16)/*vpd.PassPhrase*/);
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
					simws.StorePassPhrase( DomainID, Util.PadString(vpd.PassPhrase, 16)/*vpd.PassPhrase*/, CredentialType.Basic, vpd.ShouldSavePassPhrase);
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
//			return false;
			return status;
		}

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

		/*

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
                                //      to be removed
                                                break;
                                        }
                                        else if(passPhraseStatus.statusCode == StatusCodes.Success)
                                                break;
                                }
                        }while( result != (int)ResponseType.Cancel && result !=(int)ResponseType.DeleteEvent);
                        if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
                        {
                                status = true;
                                try
                                {
                                        simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
                                }
                                catch(Exception ex)
                                {
                                        return true;
                                }
                        }
                        else //if(result == (int)ResponseType.Cancel)
                        {
                                Console.WriteLine(" cancelled passphrase entry");
                                try
                                {
                                        simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
                                        string passphrasecheck;
                                        passphrasecheck = simws.GetPassPhrase(DomainID);
                                        if(passphrasecheck == "")
                                                Console.WriteLine(" Cancel clicked at the time of login-- confirmed");
                                        else
                                                Console.WriteLine(" cancel clicked is not confirmed");
                                }
                                catch(Exception e)
                                {
                                        return true;
                                }
                        }
                        }
                        catch(Exception e)
                        {
                                return true;
                        }
                        return true;
//                      return status;
                }

                private static bool ShowEnterPassPhraseDialog(string DomainID, SimiasWebService simws)
                {
                        bool status = false;
                        int result;
                        EnterPassPhraseDialog epd = new EnterPassPhraseDialog(DomainID);
                        try
                        {
                        do
                        {
                                result = epd.Run();
                                epd.Hide();
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

                        if( epd.PassPhrase == epd.RetypedPassPhrase)
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
                        catch(Exception e)
                        {
                                return true;
                        }
                        return true;
//                      return status;
                }

	}
	*/
}
