/***********************************************************************
 *  Provider.cs - A helper class for providers.
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
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;
using Simias;
//using Simias.Storage.Provider.Flaim;

namespace Simias.Storage.Provider
{	
	/// <summary>
	/// Represents a Collection Store Provider.
	/// </summary>
	public abstract class Provider
	{
		#region Constant Definitions.

		/// <summary>
		/// Xml Tag for a list of objects.
		/// </summary>
		public const string ObjectListTag = "ObjectList";
		/// <summary>
		/// Xml Tag for an object definition.
		/// </summary>
		public const string ObjectTag = "Object";
		/// <summary>
		/// Xml tag for a property definition.
		/// </summary>
		public const string PropertyTag = "Property";
		/// <summary>
		/// Xml attribute for name.
		/// </summary>
		public const string NameAttr = "name";
		/// <summary>
		/// Xml attribute for id.
		/// </summary>
		public const string IdAttr = "id";
		/// <summary>
		/// Xml attribute for type.
		/// </summary>
		public const string TypeAttr = "type";

		/// <summary>
		/// Xml attribute for property flags.
		/// </summary>
		public const string FlagsAttr = "flags";
		
		/// <summary>
		/// The name property for an object.
		/// </summary>
		public const string ObjectName = "Display Name";
		/// <summary>
		/// The type property for an object.
		/// </summary>
		public const string ObjectType = "Object Type";
		/// <summary>
		/// The id property for an object.
		/// </summary>
		public const string ObjectId = "GUID";

		/// <summary>
		/// Property that describes the collection that a node belongs to.
		/// </summary>
		public const string CollectionId = "CollectionId";

		/// <summary>
		/// The syntax values that are supported by the store.
		/// </summary>
		public class Syntax
		{
			/// <summary>
			/// Represents text; that is, a series of Unicode characters. A string is a sequential 
			/// collection of Unicode characters, typically used to represent text, while a String 
			/// is a sequential collection of System.Char objects that represents a string. The 
			/// value of the String is the content of the sequential collection, and the value is 
			/// immutable.
			/// </summary>
			public const string String = "String";

			/// <summary>
			/// Represents an 8-bit signed integer value. The SByte value type represents 
			/// integers with values ranging from negative 128 to positive 127.
			/// </summary>
			public const string SByte = "SByte";
				
			/// <summary>
			/// Represents an 8-bit unsigned integer value. The Byte value type represents 
			/// unsigned integers with values ranging from 0 to 255.
			/// </summary>
			public const string Byte = "Byte"; 

			/// <summary>
			/// Represents a 16-bit signed integer value. The Int16 value type represents signed 
			/// integers with values ranging from negative 32768 through positive 32767.
			/// </summary>
			public const string Int16 = "Int16"; 

			/// <summary>
			/// Represents a 16-bit unsigned integer value. The UInt16 value type represents 
			/// unsigned integers with values ranging from 0 to 65535.
			/// </summary>
			public const string UInt16 = "UInt16";

			/// <summary>
			/// Represents a 32-bit signed integer value. The Int32 value type represents signed 
			/// integers with values ranging from negative 2,147,483,648 through positive 2,147,483,647.
			/// </summary>
			public const string Int32 = "Int32";

			/// <summary>
			/// Represents a 32-bit unsigned integer value. The UInt32 value type represents unsigned 
			/// integers with values ranging from 0 to 4,294,967,295.
			/// </summary>
			public const string UInt32 = "UInt32";

			/// <summary>
			/// Represents a 64-bit signed integer value. The Int64 value type represents integers
			/// with values ranging from negative 9,223,372,036,854,775,808 through 
			/// positive 9,223,372,036,854,775,807.
			/// </summary>
			public const string Int64 = "Int64";

			/// <summary>
			/// Represents a 64-bit unsigned integer value. The UInt64 value type represents unsigned 
			/// integers with values ranging from 0 to 18,446,744,073,709,551,615.
			/// </summary>
			public const string UInt64 = "UInt64";

			/// <summary>
			/// Represents a Unicode character. The Char value type represents a Unicode character,
			/// also called a Unicode code point, and is implemented as a 16-bit number ranging in
			/// value from hexadecimal 0x0000 to 0xFFFF.
			/// </summary>
			public const string Char = "Char";

			/// <summary>
			/// Represents a single-precision floating point number. The Single value type represents a 
			/// single-precision 32-bit number with values ranging from negative 3.402823e38 to 
			/// positive 3.402823e38, as well as positive or negative zero, PositiveInfinity, 
			/// NegativeInfinity, and not a number (NaN).
			/// </summary>
			public const string Single = "Single";

			/// <summary>
			/// Represents a Boolean value. Instances of this type have values of either true or false.
			/// </summary>
			public const string Boolean = "Boolean";

			/// <summary>
			/// Represents an instant in time, typically expressed as a date and time of day. The DateTime 
			/// value type represents dates and times with values ranging from 12:00:00 midnight, 
			/// January 1, 0001 Anno Domini (Common Era) to 11:59:59 P.M., December 31, 9999 A.D. (C.E.)
			/// </summary>
			public const string DateTime = "DateTime";

			/// <summary>
			/// Provides an object representation of a uniform resource identifier (URI) and easy access 
			/// to the parts of the URI. A URI is a compact representation of a resource available to 
			/// your application on the Internet.
			/// </summary>
			public const string Uri = "Uri";

			/// <summary>
			/// Represents an XML document. This class implements the W3C Document Object Model (DOM) 
			/// Level 1 Core and the Core DOM Level 2. The DOM is an in-memory (cache) tree representation 
			/// of an XML document and enables the navigation and editing of this document.
			/// </summary>
			public const string XmlDocument = "XmlDocument";

			/// <summary>
			/// Represents a time interval.  The value of an instance of TimeSpan represents a period of time. 
			/// That value is the number of ticks contained in the instance and can range from Int64.MinValue 
			/// to Int64.MaxValue. A tick is the smallest unit of time that can be specified, and is equal to 
			/// 100 nanoseconds. Both the specification of a number of ticks and the value of a TimeSpan can 
			/// be positive or negative.
			/// </summary>
			public const string TimeSpan = "TimeSpan";
		}

		private const string CFG_Section = "StoreProvider";
		private const string CFG_Path = "Path";
		private const string CFG_Assembly = "Assembly";
		private const string CFG_TypeName = "Type";
		private const string CFG_Version = "Version";
		private const string StoreName = ".simias";

		#endregion

		#region Errors

		/// <summary>
		/// The defined errors.
		/// </summary>
		public enum Error
		{
			/// <summary>
			/// The call completed successfully.
			/// </summary>
			OK = 0,
			/// <summary>
			/// Invalid DataBase Version.
			/// </summary>
			Version = -1,
			/// <summary>
			/// Failed to create object.
			/// </summary>
			Create = -2,
			/// <summary>
			/// The Object already exists.
			/// </summary>
			Exists = -3,
			/// <summary>
			/// Invalid Object Format.
			/// </summary>
			Format = -4,
			/// <summary>
			/// Failed to delete object.
			/// </summary>
			Delete = -5,
			/// <summary>
			/// Transaction Error.
			/// </summary>
			Transaction = -6,
			/// <summary>
			/// Open DB Error.
			/// </summary>
			Open = -7,
		}

		#endregion

		#region Static Methods.
		/// <summary>
		/// Connect to the default provider.
		/// </summary>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(out bool created)
		{
			return LoadProvider(DefaultPath, out created);
		}

		/// <summary>
		/// Connect to the default provider.
		/// </summary>
		/// <param name="path">Path to the store.</param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(string path, out bool created)
		{
			path = Provider.fixupPath(path);
			//if (!path.Equals(DbPath))
			//	Simias.Configuration.BaseConfigPath = path;
			//DbPath = path;
			//return (Connect("FlaimProvider.dll", "Simias.Storage.Provider.Flaim.FlaimProvider", path, out created));
			return (LoadProvider(path, out created));
			//return (Connect("FsProvider.dll", "Simias.Storage.Provider.Fs.FsProvider", path, out created));
		}
		
		/// <summary>
		/// Connect to the specified provider.
		/// </summary>
		/// <param name="assembly">Path to the assembly that contains the provider.</param>
		/// <param name="providerType">Name of the class that implements the IProvider interface.</param>
		/// <param name="path"></param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(string assembly, string providerType, string path, out bool created)
		{
			path = Provider.fixupPath(path);
			//if (!path.Equals(DbPath))
			//	Simias.Configuration.BaseConfigPath = path;
			Assembly = assembly;
			TypeName = providerType;
			//DbPath = path;
			return (LoadProvider(path, out created));
		}
		
		/// <summary>
		/// Connect to the specified provider.
		/// </summary>
		/// <param name="path">The path to the DB.</param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		private static IProvider LoadProvider(string path, out bool created)
		{
			string assembly = Assembly;
			string providerType = TypeName;
			//string path = DbPath;
			created = false;
			IProvider provider = null;

			// Load the assembly and find our provider.
			Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assembly));
			Type pType = null;
			Type[] types = pAssembly.GetExportedTypes();
			foreach (Type t in types)
			{
				if (t.FullName.Equals(providerType))
				{
					pType = t;
					break;
				}
			}

			// If we did not find our type return a null.
			if (pType == null)
			{
				return (null);
			}

			object[] args = {path};
			object[] activationAttrs = null;
			
			/* This needs to be commented out till I can fix the service code.
			if (Environment.OSVersion.Platform.Equals(PlatformID.Win32NT))
			{
				// Try to create the mutex.
				bool createdMutex;
				string name = "FlaimService_Mutex";

				Mutex mutex = new Mutex(true, name, out createdMutex);
			
				if (!createdMutex)
				{
					// The service is running attach to the remote service.
					object[] parameters = {path};
					activationAttrs = new object[] {new UrlAttribute ("tcp://localhost:1234")};
					//RemotingConfiguration.RegisterActivatedClientType(pType, "tcp://localHost:1234");
				}
				else
				{
					mutex.ReleaseMutex();
				}
			}
			*/

			provider = (IProvider)pAssembly.CreateInstance(providerType, false, 0, null, args, null, activationAttrs);

			try
			{
				provider.OpenStore();
			}
			catch
			{
				try
				{
					provider.CreateStore();
					// Set the Provider in the configuration.
					Assembly = assembly;
					TypeName = providerType;
					//DbPath = path;
					created = true;
				}
				catch (Exception e)
				{
					throw;
//					Console.WriteLine(e.Message);
//					Console.WriteLine(e.StackTrace);
				}
			}
			
			return (provider);
		}

		/// <summary>
		/// Called to remove the Database path.
		/// </summary>
		public static void Delete(string path)
		{
			//string path = DbPath;
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		/// <summary>
		/// Gets and Sets the Provider Version.
		/// </summary>
		public static string Version
		{
			get
			{
				string version = "0";
				return (Simias.Configuration.Get(CFG_Section, CFG_Version, version));
			}
			set
			{
				Simias.Configuration.Set(CFG_Section, CFG_Version, value);
			}
		}

		/// <summary>
		/// Gets and Sets the Assembly that implements the provider instance used.
		/// </summary>
		public static string Assembly
		{
			get
			{
				string assembly = "SqliteProvider.dll";
				return (Simias.Configuration.Get(CFG_Section, CFG_Assembly, assembly));
			}
			set
			{
				Simias.Configuration.Set(CFG_Section, CFG_Assembly, value);
			}
		}

		/// <summary>
		/// Gets and Sets the Class Type of the implemented provider.
		/// </summary>
		public static string TypeName
		{
			get
			{
				string providerType = "Simias.Storage.Provider.Sqlite.SqliteProvider";
				return (Simias.Configuration.Get(CFG_Section, CFG_TypeName, providerType));
			}
			set
			{
				Simias.Configuration.Set(CFG_Section, CFG_TypeName, value);
			}
		}

		/*
		/// <summary>
		/// Gets and Sets the database path of the implemented provider.
		/// </summary>
		public static string DbPath
		{
			get
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (path == null || path.Length == 0)
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}
				path = Provider.fixupPath(path);
				return (Simias.Configuration.Get(CFG_Section, CFG_Path, path));
			}
			set
			{
				Simias.Configuration.Set(CFG_Section, CFG_Path, value);
			}
		}
		*/

		/// <summary>
		/// Gets the default database path.
		/// </summary>
		public static string DefaultPath
		{
			get
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (path == null || path.Length == 0)
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}
				path = Provider.fixupPath(path);
				return (path);
			}
		}

		private static string fixupPath(string path)
		{
			path = Path.Combine(path, StoreName);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}

		#endregion
	}
}
