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
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;

namespace Simias.Identity
{
	/// <summary>
	/// Class for determining the capabilities of a user identity provider
	/// </summary>
	public class UserProviderCaps
	{
		/// <summary>
		/// Value used to determine if the provider supports external synchronizing.
		/// </summary>
		public bool ExternalSync;

		/// <summary>
		/// Value used to determine if the provider supports creating objects.
		/// </summary>
		public bool CanCreate;

		/// <summary>
		/// Value used to determine if the provider supports deleting objects.
		/// </summary>
		public bool CanDelete;

		/// <summary>
		/// Value used to determine if the provider supports modifying objects.
		/// </summary>
		public bool CanModify;
	}
	
	/// <summary>
	/// Interface for an external identity/user provider
	/// </summary>
	public interface IUserProvider
	{
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		string Description { get; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Guid" mandatory="false">Guid associated to the user.</param>
		/// <param name="Username" mandatory="true">User or short name for the new user.</param>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <param name="Firstname" mandatory="false">First or given name associated to the user.</param>
		/// <param name="Lastname" mandatory="false">Last or family name associated to the user.</param>
		/// <param name="Fullname" mandatory="false">Full or complete name associated to the user.</param>
		/// <param name="Distinguished" mandatory="false">Distinguished name of the user.</param>
		/// <returns>RegistrationStatus</returns>
		RegistrationInfo
		Create(
			string Guid,
			string Username,
			string Password,
			string Firstname,
			string Lastname,
			string Fullname,
			string Distinguished );
		
		/// <summary>
		/// Method to delete a user from the external identity/user database.
		/// Some external systems may not allow deletion of users.
		/// </summary>
		/// <param name="Username">Name of the user to delete from the external system.</param>
		/// <returns>true - successful  false - failed</returns>
		bool Delete( string Username );
		
		/// <summary>
		/// Method to retrieve the capabilities of a user identity
		/// provider.
		/// </summary>
		/// <returns>providers capabilities</returns>
		UserProviderCaps GetCapabilities();

		/// <summary>
		/// Method to set/reset a user's password
		/// Note: This method will be replaced when the self-service
		/// framework is designed and implemented.
		/// </summary>
		/// <param name="Username" mandatory="true">Username to set the password on.</param>
		/// <param name="Password" mandatory="true">New password.</param>
		/// <returns>true - successful</returns>
		bool SetPassword( string Username, string Password );
		
		/// <summary>
		/// Method to verify a user's password
		/// </summary>
		/// <param name="Username">User to verify the password against</param>
		/// <param name="Password">Password to verify</param>
		/// <param name="status">Structure used to pass additional information back to the user.</param>
		/// <returns>true - Valid password, false Invalid password</returns>
		bool VerifyPassword( string Username, string Password, Simias.Authentication.Status status );
		#endregion
	}
}	
