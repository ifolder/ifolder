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

using log4net;
using log4net.spi;

namespace Simias
{
	/// <summary>
	/// Simias Log Interface
	/// </summary>
	public interface ISimiasLog
	{
		void Debug(string format, params object[] args);
		void Info(string format, params object[] args);
		void Warn(string format, params object[] args);
		void Error(string format, params object[] args);
		void Fatal(string format, params object[] args);

		void Debug(Exception e, string format, params object[] args);
		void Info(Exception e, string format, params object[] args);
		void Warn(Exception e, string format, params object[] args);
		void Error(Exception e, string format, params object[] args);
		void Fatal(Exception e, string format, params object[] args);
	}
}
