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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

// This class is a temporary hack to get around the problems that
// exist with a Mutex and multiple processes
// It's slow, it's dirty, but it's damn solid

// TODO: Take this out once Paul puts in the platform defines. Mac is broken until then.
#define LINUX

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Simias
{
	/// <summary>
	/// Class used to control cross process synchronization.
	/// </summary>
	public class SimiasMutex
	{
		#region Class Members
#if MONO
		// TODO: Remove for debug purposes only.
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( SimiasMutex ) );

		[Flags]
		enum Operation
		{
			LOCK_SH = 1,
			LOCK_EX = 2,
			LOCK_NB = 4,
			LOCK_UN = 8
		};

#if LINUX
        private static int O_CREAT = 0100;
#elif DARWIN
        private static int O_CREAT = 0x200;
#endif
		private static int Success = 0;
		private static string dirPath = null;
		private static string lockDir = "Locks";
		private string name;
		private int acquired;
		private int fd;
		private int threadID;

		[DllImport ("libc")]
	static extern int open(string name, int flags);

		[DllImport ("libc")]
        static extern int close(int fd);

		[DllImport ("libc")]
		static extern int flock(int fd, int operation);
#else
		private Mutex mux;
#endif
		private bool disposed = true;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="name">Name of the cross process mutex.</param>
		public SimiasMutex(string name)
		{
#if MONO
			this.name = name;
			this.acquired = 0;

			if (dirPath == null)
			{
				// Setup the path to the Lock directory.
				dirPath = Path.Combine(Configuration.GetConfiguration().StorePath, lockDir);
				if (!Directory.Exists(dirPath))
				{
					Directory.CreateDirectory(dirPath);
					log.Debug("Created lock path {0}", dirPath);
				}
			}

			// Build the path to the mutex file.
			this.name = Path.Combine(dirPath, name);

			// Try to create the file.
			log.Debug("Creating mutex: {0}", name);
            fd = open(this.name, O_CREAT);
			if (fd == -1)
			{
				throw new IOException("Failed to create mutex " + this.name);
			}

			disposed = false;
#else
			mux = new Mutex(false, name);
#endif
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Disposes of the mutex
		/// </summary>
		public void Close()
		{
#if MONO
			GC.SuppressFinalize(this);
			Dispose(false);
#else
			mux.Close();
#endif
		}

		/// <summary>
		/// Waits to acquire the mutext.
		/// </summary>
		public void WaitOne()
		{
#if MONO
			lock (this)
			{
				if (acquired != 0)
				{
					log.Debug("In WaitOne() - Mutex is acquired");
					if (Thread.CurrentThread.GetHashCode() == threadID)
					{
						// We already own the lock just bump the count.
						log.Debug("Mutex already acquired by thread {0}", threadID);
						acquired++;
						return;
					}
				}
			}

			// If we get here we need to acquire the lock.
			int error = flock(fd, (int)Operation.LOCK_EX);
			if (error == Success)
			{
				lock (this)
				{
					acquired++;
					threadID = Thread.CurrentThread.GetHashCode();
					log.Debug("Acquired mutex by thread {0}", threadID );
				}
			}
			else
			{
				throw new SimiasException("Wait for mutex failed.");
			}
#else
			mux.WaitOne();
#endif
		}

		/// <summary>
		/// Releases the mutex.
		/// </summary>
		public void ReleaseMutex()
		{
#if MONO
			bool needToRelease = false;
			lock (this)
			{
				if (acquired != 0)
				{
					log.Debug("In ReleaseMutext() - Mutex is acquired");
					if (Thread.CurrentThread.GetHashCode() == threadID)
					{
						// Release the mutex.
						if (--acquired == 0)
						{
							log.Debug("NeedToRelease is true" );
							needToRelease = true;
						}
					}
				}
			}

			if (needToRelease)
			{
				int error = flock(fd, (int)Operation.LOCK_UN);
				if (error != Success)
				{
					lock (this)
					{
						log.Debug("Failed to release mutex");
						++acquired;
					}

					// This thread does not own the mutex. Exception.
					throw new ApplicationException("Failed To release mutex.");				}
			}
#else
			mux.ReleaseMutex();
#endif
		}
		#endregion

		#region Disposable Methods
		/// <summary>
		/// Finalizer for the class.
		/// </summary>
		~SimiasMutex()
		{
			Dispose(true);
		}

		/// <summary>
		/// Disposes of owned resources.
		/// </summary>
		/// <param name="inFinalizer">Set to true if this routine was called from the finalizer.</param>
		private void Dispose(bool inFinalizer)
		{
			if (!disposed)
			{
				disposed = true;
#if MONO
				// We need to close the file
                close(fd);
				log.Debug("File handle closed");
#endif
			}
		}
		#endregion
	}
}

