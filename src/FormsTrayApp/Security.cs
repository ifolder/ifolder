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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Security.
	/// </summary>
	public class Security
	{
		public Security()
		{
		}

		#region Win32 API
		private static readonly int SECURITY_BUILTIN_DOMAIN_RID = 0x00000020;
		private static readonly int DOMAIN_ALIAS_RID_USERS = 0x00000221;
		private static readonly int DACL_SECURITY_INFORMATION = 0x00000004;
		private static readonly int ERROR_INSUFFICIENT_BUFFER = 122;
		private static readonly int SECURITY_DESCRIPTOR_REVISION = 1;
		private static readonly int ACL_REVISION = 2;
		private static readonly int INHERITED_ACE = 0x10;
		private static readonly uint MAXDWORD = 0xffffffff;
		public static readonly uint GENERIC_READ = 0x80000000;
		public static readonly uint GENERIC_WRITE = 0x40000000;
		public static readonly uint GENERIC_EXECUTE = 0x20000000;
		public static readonly int OBJECT_INHERIT_ACE = 0x1;
		public static readonly int CONTAINER_INHERIT_ACE = 0x2;
		private static readonly int SE_DACL_AUTO_INHERIT_REQ = 0x0100;
		private static readonly int SE_DACL_AUTO_INHERITED = 0x0400;
		private static readonly int SE_DACL_PROTECTED = 0x1000;

		[DllImport("advapi32.dll")]
		static extern bool AllocateAndInitializeSid(ref SID_IDENTIFIER_AUTHORITY pAuthority, byte subAuthorityCount, int subAuthority0, int subAuthority1, int subAuthority2, int subAuthority3, int subAuthority4, int subAuthority5, int subAuthority6, int subAuthority7, ref IntPtr pSid);

		[DllImport("advapi32.dll")]
		static extern void FreeSid(IntPtr pSid);

		[DllImport("advapi32.dll", CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Unicode)]
		static extern bool GetFileSecurity(string fileName, int RequestedInformation, IntPtr pSecurityDescriptor, int length, out int lengthNeeded);

		[DllImport("advapi32.dll", CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Unicode)]
		static extern bool SetFileSecurity(string lpFileName, int SecurityInformation, ref SECURITY_DESCRIPTOR pSecurityDescriptor);

		[DllImport("advapi32.dll", CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		static extern bool GetSecurityDescriptorDacl(IntPtr pSecurityDescriptor, out bool bDaclPresent, ref IntPtr pDacl, out bool bDaclDefaulted);

		[DllImport("advapi32.dll")]
		static extern int GetLengthSid(IntPtr pSid);

		[DllImport("advapi32.dll")]
		static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);

		[DllImport("advapi32.dll")]
		static extern bool GetAce(IntPtr pAcl, int dwAceIndex, ref IntPtr pAce);

		[DllImport("advapi32.dll")]
		static extern bool EqualSid(IntPtr pSid1, IntPtr pSid2);

		[DllImport("advapi32.dll")]
		static extern bool AddAce(IntPtr pAcl, int dwAceRevision, uint dwStartingAceIndex, IntPtr pAceList, int nAceListLength);

		[DllImport("advapi32.dll")]
		static extern bool AddAccessAllowedAce(IntPtr pAcl, int dwAceRevision, uint AccessMask, IntPtr pSid);
		
		[DllImport("advapi32.dll")]
		static extern bool AddAccessAllowedAceEx(IntPtr pAcl, int dwAceRevision, int AceFlags, uint AccessMask,	IntPtr pSid);

		[DllImport("advapi32.dll")]
		static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR pSecurityDescriptor, bool bDaclPresent, IntPtr pDacl, bool bDaclDefaulted);

		[DllImport("advapi32.dll")]
		static extern bool GetSecurityDescriptorControl(IntPtr pSecurityDescriptor,	out short pControl, out int lpdwRevision);

		[DllImport("advapi32.dll")]
		static extern bool SetSecurityDescriptorControl(ref SECURITY_DESCRIPTOR pSecurityDescriptor,	short ControlBitsOfInterest, short ControlBitsToSet);

		[StructLayout(LayoutKind.Sequential)]
		public struct ACCESS_ALLOWED_ACE 
		{
			public ACE_HEADER Header;
			public int Mask;
			public int SidStart;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ACE_HEADER 
		{
			public byte AceType;
			public byte AceFlags;
			public short AceSize;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SID_IDENTIFIER_AUTHORITY 
		{
			public byte Value0;
			public byte Value1;
			public byte Value2;
			public byte Value3;
			public byte Value4;
			public byte Value5;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ACL
		{
			public byte AclRevision;
			public byte Sbz1;
			public ushort AclSize;
			public ushort AceCount;
			public ushort Sbz2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_DESCRIPTOR 
		{
			public byte Revision;
			public byte Sbz1;
			public short Control;
			public IntPtr Owner;
			public IntPtr Group;
			public IntPtr Sacl;
			public IntPtr Dacl;
		}
		#endregion


		/// <summary>
		/// Adds an ACE (access control entry) for the Users group to the specified directory.
		/// </summary>
		/// <param name="Path">The path to add the ACE to.</param>
		/// <param name="AceFlags">The flags to set in the ACE.</param>
		/// <param name="AccessMask">The rights to allow in the ACE.</param>
		/// <returns><b>True</b> if the ACE was successfully set; otherwise, <b>False</b>.</returns>
		public bool SetAccess(string Path, int AceFlags, uint AccessMask)
		{
			IntPtr pSid = IntPtr.Zero;
			IntPtr fileSD = IntPtr.Zero;
			IntPtr pNewAcl = IntPtr.Zero;
			try
			{
				// Create the SID for the Users group.
				SID_IDENTIFIER_AUTHORITY ntAuthority;
				ntAuthority.Value0 = ntAuthority.Value1 = ntAuthority.Value2 = ntAuthority.Value3 = ntAuthority.Value4 = 0;
				ntAuthority.Value5 = 5;
				if (!AllocateAndInitializeSid(
					ref ntAuthority,
					2,
					SECURITY_BUILTIN_DOMAIN_RID,
					DOMAIN_ALIAS_RID_USERS,
					0, 0, 0, 0, 0, 0,
					ref pSid))
				{
					return false;
				}

				// Get the size of the security descriptor for the directory.
				int sdLength = 0;
				if (!GetFileSecurity(Path, DACL_SECURITY_INFORMATION, fileSD, 0, out sdLength))
				{
					if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
					{
						return false;
					}

					// Allocate the security descriptor
					fileSD = Marshal.AllocHGlobal(sdLength);

					// Get the security descriptor for the directory.
					if (!GetFileSecurity(Path, DACL_SECURITY_INFORMATION, fileSD, sdLength, out sdLength))
					{
						return false;
					}

					// Get DACL from the old SD.
					bool bDaclPresent;
					bool bDaclDefaulted;
					IntPtr aclPtr = IntPtr.Zero;
					if (!GetSecurityDescriptorDacl(fileSD, out bDaclPresent, ref aclPtr, out bDaclDefaulted))
					{
						return false;
					}

					// Put the data in an ACL structure.
					MemoryMarshaler mm = new MemoryMarshaler(aclPtr);
					ACL acl = (ACL)mm.ParseStruct(typeof(ACL));
					
					// Compute size needed for the new ACL.
					ACCESS_ALLOWED_ACE accessAllowedAce = new ACCESS_ALLOWED_ACE();
					int n1 = Marshal.SizeOf(accessAllowedAce);
					int n2 = Marshal.SizeOf(n1);
					int cbNewAcl = acl.AclSize + 12 /*sizeof(ACCESS_ALLOWED_ACE)*/ + GetLengthSid(pSid) - 4 /*sizeof(int)*/;
		
					// Allocate memory for new ACL.
					pNewAcl = Marshal.AllocHGlobal(cbNewAcl);
		
					// Initialize the new ACL.
					if (!InitializeAcl(pNewAcl, cbNewAcl, ACL_REVISION))
					{
						return false;
					}
		
					// If DACL is present, copy all the ACEs from the old DACL
					// to the new DACL.
					uint newAceIndex = 0;
					IntPtr acePtr = IntPtr.Zero;
					int CurrentAceIndex = 0;
					if (bDaclPresent && (acl.AceCount > 0))
					{
						for (CurrentAceIndex = 0; CurrentAceIndex < acl.AceCount; CurrentAceIndex++)
						{
							// Get the ACE.
							if (!GetAce(aclPtr, CurrentAceIndex, ref acePtr))
							{
								return false;
							}

							// Put the data in an ACCESS_ALLOWED_ACE structure.
							mm.Ptr = acePtr;
							accessAllowedAce = (ACCESS_ALLOWED_ACE)mm.ParseStruct(typeof(ACCESS_ALLOWED_ACE));

							// Check if it is a non-inherited ACE.
							if ((accessAllowedAce.Header.AceFlags & INHERITED_ACE) == INHERITED_ACE)
								break;

							// Get the memory that holds the SID.
							mm.Ptr = acePtr;
							mm.Advance(8);

							// If the SID matches, don't add the ACE.
							if (EqualSid(pSid, mm.Ptr))
								continue;

							// Add the ACE to the new ACL.
							if (!AddAce(pNewAcl, ACL_REVISION, MAXDWORD, acePtr, accessAllowedAce.Header.AceSize))
							{
								return false;
							}

							newAceIndex++;
						}
					}
		
					// Add the access-allowed ACE to the new DACL.
					if (!AddAccessAllowedAceEx(pNewAcl, ACL_REVISION, AceFlags, AccessMask, pSid))
					{
						return false;
					}

					// Copy the rest of inherited ACEs from the old DACL to the new DACL.
					if (bDaclPresent && (acl.AceCount > 0))
					{
						for (; CurrentAceIndex < acl.AceCount; CurrentAceIndex++)
						{
							// Get the ACE.
							if (!GetAce(aclPtr, CurrentAceIndex, ref acePtr))
							{
								return false;
							}

							// Put the data in an ACCESS_ALLOWED_ACE structure.
							mm.Ptr = acePtr;
							accessAllowedAce = (ACCESS_ALLOWED_ACE)mm.ParseStruct(typeof(ACCESS_ALLOWED_ACE));

							// Add the ACE to the new ACL.
							if (!AddAce(pNewAcl, ACL_REVISION, MAXDWORD, acePtr, accessAllowedAce.Header.AceSize))
							{
								return false;
							}
						}
					}

					// Create a new security descriptor to set on the directory.
					SECURITY_DESCRIPTOR newSD;
					newSD.Revision = (byte)SECURITY_DESCRIPTOR_REVISION;
					newSD.Sbz1 = 0;
					newSD.Control = 0;
					newSD.Owner = IntPtr.Zero;
					newSD.Group = IntPtr.Zero;
					newSD.Sacl = IntPtr.Zero;
					newSD.Dacl = IntPtr.Zero;

					// Set the new DACL to the new SD.
					if (!SetSecurityDescriptorDacl(ref newSD, true, pNewAcl, false))
					{
						return false;
					}
		
					// Copy the old security descriptor control flags 
					short controlBitsOfInterest = 0;
					short controlBitsToSet = 0;
					short oldControlBits = 0;
					int revision = 0;
		
					if (!GetSecurityDescriptorControl(fileSD, out oldControlBits, out revision))
					{
						return false;
					}
		
					if ((oldControlBits & SE_DACL_AUTO_INHERITED) == SE_DACL_AUTO_INHERITED)
					{
						controlBitsOfInterest = (short)(SE_DACL_AUTO_INHERIT_REQ | SE_DACL_AUTO_INHERITED);
						controlBitsToSet = controlBitsOfInterest;
					}
					else if ((oldControlBits & SE_DACL_PROTECTED) == SE_DACL_PROTECTED)
					{
						controlBitsOfInterest = (short)SE_DACL_PROTECTED;
						controlBitsToSet = controlBitsOfInterest;
					}
		
					if (controlBitsOfInterest > 0)
					{
						if (!SetSecurityDescriptorControl(ref newSD, controlBitsOfInterest, controlBitsToSet))
						{
							return false;
						}
					}
		
					// Set the new SD to the File.
					if (!SetFileSecurity(Path, DACL_SECURITY_INFORMATION, ref newSD))
					{
						return false;
					}
				}
			}
			finally
			{
				if (pSid != IntPtr.Zero)
				{
					FreeSid(pSid);
				}

				if (fileSD != IntPtr.Zero)
				{
					//GlobalFree(fileSD);
					Marshal.FreeHGlobal(fileSD);
				}

				if (pNewAcl != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(pNewAcl);
				}
			}

			return true;
		}
	}

	/// <summary>
	/// A class used to marshal memory referenced by a pointer.
	/// </summary>
	public class MemoryMarshaler
	{
		private IntPtr ptr;

		/// <summary>
		/// Constructs a MemoryMarshaler object.
		/// </summary>
		/// <param name="ptr">The pointer to the memory.</param>
		public MemoryMarshaler(IntPtr ptr)
		{
			this.ptr = ptr;
		}

		/// <summary>
		/// Advance the memory pointer by cbLength bytes.
		/// </summary>
		/// <param name="cbLength">The number of bytes to advance the memory pointer.</param>
		public void Advance(int cbLength)
		{
			long p = ptr.ToInt64();
			p += cbLength;
			ptr = (IntPtr)p;
		}

		/// <summary>
		/// Parse the memory to a structure of type.
		/// </summary>
		/// <param name="type">The type of the structure to parse to.</param>
		/// <returns>An object of the specified type.</returns>
		public object ParseStruct(System.Type type)
		{
			return ParseStruct(type, true);
		}

		/// <summary>
		/// Parse the memory to a structure of type.
		/// </summary>
		/// <param name="type">The type of structure to parse to.</param>
		/// <param name="moveOffset">Indicates if the memory pointer should be advanced.</param>
		/// <returns>An object of the specified type.</returns>
		public object ParseStruct(System.Type type, bool moveOffset)
		{
			object o = Marshal.PtrToStructure(ptr, type);
			if (moveOffset)
				Advance(Marshal.SizeOf(type));
			return o;
		}

		/// <summary>
		/// Gets/sets the memory pointer.
		/// </summary>
		public IntPtr Ptr
		{
			get { return ptr; }
			set	{ ptr = value; }
		}
	}
}
