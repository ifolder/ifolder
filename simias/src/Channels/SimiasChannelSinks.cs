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
 *  Author: Rob
 *
 ***********************************************************************/

using System;

namespace Simias.Channels
{
	/// <summary>
	/// Simias Channel Sinks
	/// </summary>
	[Flags]
	public enum SimiasChannelSinks
	{
		/// <summary>
		/// Specifies a soap style formatter sink.
		/// </summary>
		Soap		= 0x0001,

		/// <summary>
		/// Specifies a binary style formatter sink.
		/// </summary>
		Binary		= 0x0002,

		/// <summary>
		/// Specifies a security sink.
		/// </summary>
		Security	= 0x0004,

		/// <summary>
		/// Specifies a sniffer sink.
		/// </summary>
		Sniffer		= 0x0008,
	};

}
