/***********************************************************************
 *  $RCSfile$
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;

namespace Simias.Agent
{
	/// <summary>
	/// Simias AgentFactory Class
	/// </summary>
	public sealed class AgentFactory
	{
		string storePath;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public AgentFactory()
			: this(null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="storePath">The collection store path.</param>
		public AgentFactory(string storePath)
		{
			this.storePath = storePath;
		}

		/// <summary>
		/// Get the default invitation agent.
		/// </summary>
		/// <returns>An invitation agent.</returns>
		public IInviteAgent GetInviteAgent()
		{
			// default invitation agent
			IInviteAgent agent = GetInviteAgent("EmailInviteAgent",
				"Simias.Agent.EmailInviteAgent");

			return agent;
		}

		/// <summary>
		/// Get the specified invitation agent.
		/// </summary>
		/// <param name="assembly">The assembly that contains the invitation agent.</param>
		/// <param name="type">The class type of the invitation agent.</param>
		/// <returns>An invitation agent.</returns>
		private IInviteAgent GetInviteAgent(string assembly, string type)
		{
			// create an instance of the Invitation Agent
			IInviteAgent agent = (IInviteAgent)
				Activator.CreateInstance(assembly, type).Unwrap();

			// initialize the agent
			agent.StorePath = this.storePath;

			return agent;
		}

		#region Properties
		
		/// <summary>
		/// The collection store path.
		/// </summary>
		public string StorePath
		{
			get { return storePath; }
		}

		#endregion Properties
	}
}
