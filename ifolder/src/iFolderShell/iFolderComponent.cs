/***********************************************************************
 *  iFolderComponent.cs - A COM interface that exposes iFolder methods.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using Novell.iFolder;
using Simias.Storage;
using Novell.AddressBook;
using Novell.iFolder.FormsBookLib;

namespace Novell.iFolder.iFolderCom
{
	public interface IiFolderComponent
	{
		String Description{get; set;}
		bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderPropInit();
		bool GetNextiFolderProp(out string name, out string val);
		bool GetiFolderAclInit();
		bool GetNextiFolderAce(out string guid, out string name, out int rights);
		void ShareiFolder(string id, int rights, bool invite);
		void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, bool modal);
		bool InvokeContactPickerDlg();
		bool GetNextAddedItem(out string guid, out string name);
	}

	/// <summary>
	/// Summary description for iFolderComponent.
	/// </summary>
	[
		ClassInterface(ClassInterfaceType.None),
		GuidAttribute("AA81D832-3B41-497c-B508-E9D02F8DF421")
	]
	public class iFolderComponent : IiFolderComponent
	{
		static private iFolderManager manager = null;//= Manager.Connect();
		private iFolder ifolder;
		private iFolderFile ifolderfile;
		private ICSEnumerator propEnumerator;
		private ICSEnumerator aclEnumerator;

		static private AddressBook.Manager abManager = null;
		private Novell.AddressBook.AddressBook addressBook = null;

		private IEnumerator items;
//		public iFolderComponent(Uri location)
//		{
//			manager= iFolderManager.Connect(location);
//		}

		public iFolderComponent()
		{
			//
			// TODO: Add constructor logic here
			//
			System.Diagnostics.Debug.WriteLine("In iFolderComponent()");

			try
			{
				if (manager == null)
				{
					manager= iFolderManager.Connect();
				}

				// Connect to the address book manager
				if (abManager == null)
				{
					abManager = Novell.AddressBook.Manager.Connect();
				}

				// Open the default address book
				if (addressBook == null)
				{
					addressBook = abManager.OpenDefaultAddressBook();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public String Description
		{
			get { return ifolderfile.Description; }
			set
			{
				ifolderfile.Description = value;

				// TODO - move this so that the commit can be done once at the end of
				// a bunch of modifies.
				ifolderfile.ThisNode.Commit();
			}
		}

		public bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				return manager.CanBeiFolder(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return false;
		}

		public bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				if (manager.IsiFolder(path))
				{
					this.ifolder = manager.GetiFolderByPath(path);
					this.ifolderfile = this.ifolder.GetiFolderFileByName(path);
					return true;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return false;
		}

		public bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				if (IsiFolder(path))
				{
					return this.ifolder.IsShareable();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return false;
		}

		public bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				ifolder= manager.CreateiFolder(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return (ifolder != null);
		}

		public void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				manager.DeleteiFolderByPath(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			System.Diagnostics.Debug.WriteLine("In GetiFolderNode()");

			try
			{
				foreach(iFolder ifolder in manager)
				{
//					string rootPath= ifolder.LocalPath + Path.DirectorySeparatorChar.ToString();
					if (path.StartsWith(ifolder.LocalPath))
					{
						ifolderfile= ifolder.GetiFolderFileByName(path);
						if (ifolderfile != null)
						{
							System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning true");
							return true;
						}

						break;
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning false");

			return false;
		}

		public bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			return manager.IsPathIniFolder(path);
		}

		public bool GetiFolderPropInit()
		{
			// Set up the enumerator to get the Properties on the Node.
			propEnumerator = ( ICSEnumerator )ifolderfile.ThisNode.Properties.GetEnumerator();

			return (propEnumerator != null);
		}

		public bool GetNextiFolderProp(out string name, out string val)
		{
			if (propEnumerator.MoveNext())
			{
				Property p = (Property)propEnumerator.Current;
				name = new string(p.Name.ToCharArray());
				val = new string(p.Value.ToString().ToCharArray());
				return true;
			}
			else
			{
				propEnumerator.Dispose();
				name = null;
				val = null;
				return false;
			}
		}

		public bool GetiFolderAclInit()
		{
			// Get the access control list for the collection.
			aclEnumerator = (ICSEnumerator)ifolder.GetShareAccess().GetEnumerator();

			return (aclEnumerator != null);
		}

		public bool GetNextiFolderAce(out string guid, out string name, out int rights)
		{
			name = null;
			if (aclEnumerator.MoveNext())
			{
				AccessControlEntry ace = (AccessControlEntry)aclEnumerator.Current;
				guid = new string(ace.Id.ToString().ToCharArray());
				rights = (int)ace.Rights;
				
				if (!ace.WellKnown)
				{
					// Get the user name from the store.
					name = GetContact(guid);
				}

				return true;
			}
			else
			{
				aclEnumerator.Dispose();
				guid = null;
				rights = 0;
				return false;
			}
		}

		public void ShareiFolder(string id, int rights, bool invite)
		{
			ifolder.Share(id, (Access.Rights)rights, invite);
		}

		private string GetContact(string userID)
		{
			// Retrieve the contact
			try
			{
				Contact myContact = addressBook.GetContactByIdentity(userID);
				return myContact.UserName;
			}
			catch (Exception)
			{
				// If there is a problem resolving, just return the ID.
				return userID;
			}
		}

		public void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, bool modal)
		{
			string windowName = "Advanced iFolder Properties for " + Path.GetFileName(path);

			// Search for existing window and bring it to foreground ...
			Win32Util.Win32Window win32Window = Win32Util.Win32Window.FindWindow(null, windowName);
			if (win32Window != null)
			{
				win32Window.BringWindowToTop();
			}
			else
			{
				iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
				ifolderAdvanced.Name = path;
				ifolderAdvanced.Text = windowName;
				ifolderAdvanced.ABManager = abManager;
				ifolderAdvanced.CurrentiFolder = manager.GetiFolderByPath(path);
				ifolderAdvanced.LoadPath = dllPath;

				if (modal)
				{
					ifolderAdvanced.ShowDialog();
				}
				else
				{
					ifolderAdvanced.Show();
				}
			}
		}

		public bool InvokeContactPickerDlg()
		{
			ContactPicker picker = new ContactPicker();
			picker.CurrentManager = abManager;
			DialogResult result = picker.ShowDialog();
			if (result == DialogResult.OK)
			{
				ArrayList contactList = picker.GetContactList;
				if (contactList.Count > 0)
				{
					items = contactList.GetEnumerator();
					return true;
				}
			}

			return false;
		}

        // TODO - is there a better way to do this???
		public bool GetNextAddedItem(out string guid, out string name)
		{
			if (items.MoveNext())
			{
				Contact contact = (Contact)items.Current;
				name = contact.UserName;
				guid = contact.Identity;
				return true;
			}
			else
			{
				guid = null;
				name = null;
				return false;
			}
		}
	}
}
