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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;
using Simias.Service;
using Simias.Storage;

namespace Simias.Storage
{
	/// <summary>
	/// Class used to implement an inprocess mutex that protects the log file from reentrancy.
	/// </summary>
	internal class LogMutex
	{
		#region Class Members
		/// <summary>
		/// Table used to keep track of per log file mutexes.
		/// </summary>
		static private Hashtable mutexTable = new Hashtable();
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the Mutex class with default properties.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		public LogMutex( string collectionID )
		{
			lock( typeof( LogMutex ) )
			{
				// See if a mutex already exists for this collection's logfile.
				if ( !mutexTable.ContainsKey( collectionID ) )
				{
					mutexTable.Add( collectionID, new Mutex() );
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the Mutex class with a Boolean value 
		/// indicating whether the calling thread should have initial ownership 
		/// of the mutex.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		/// <param name="initiallyOwned">true to give the calling thread initial 
		/// ownership of the mutex; otherwise, false.</param>
		public LogMutex( string collectionID, bool initiallyOwned )
		{
			bool created = false;
			lock( typeof( LogMutex ) )
			{
				if ( !mutexTable.ContainsKey( collectionID ) )
				{
					mutexTable.Add( collectionID, new Mutex( initiallyOwned ) );
					created = true;
				}
			}

			// If the mutex already existed and the caller specified to acquire
			// the mutex before returning, get it now.
			if ( !created && initiallyOwned )
			{
				WaitOne( collectionID );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Releases all resources held by the current WaitHandle.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		public void Close( string collectionID )
		{
			lock( typeof( LogMutex ) )
			{
				Mutex mutex = mutexTable[ collectionID ] as Mutex;
				if ( mutex != null )
				{
					mutex.Close();
					mutexTable.Remove( collectionID );
				}
			}
		}

		/// <summary>
		/// Releases the mutex once.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		public void ReleaseMutex( string collectionID )
		{
			Mutex mutex = null;

			lock ( typeof( LogMutex ) )
			{
				mutex = mutexTable[ collectionID ] as Mutex;
			}

			if ( mutex == null )
			{
				throw new SimiasException( "Log mutex does not exist for collection " + collectionID );
			}

			mutex.ReleaseMutex();
		}

		/// <summary>
		/// Blocks the current thread until the current WaitHandle receives a signal.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		/// <returns>true if the current instance receives a signal. If the current 
		/// instance is never signaled, WaitOne never returns.</returns>
		public bool WaitOne( string collectionID )
		{
			Mutex mutex = null;

			lock( typeof( LogMutex ) )
			{
				mutex = mutexTable[ collectionID ] as Mutex;
			}

			if ( mutex == null )
			{
				throw new SimiasException( "Log mutex does not exist for collection " + collectionID );
			}

			return mutex.WaitOne();
		}

		/// <summary>
		/// Blocks the current thread until the current WaitHandle receives a signal, 
		/// using 32-bit signed integer to measure the time interval and specifying 
		/// whether to exit the synchronization domain before the wait.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or 
		/// Timeout.Infinite (-1) to wait indefinitely. </param>
		/// <param name="exitContext">true to exit the synchronization domain for the 
		/// context before the wait (if in a synchronized context), and reacquire it; otherwise, false.</param>
		/// <returns>true if the current instance receives a signal; otherwise, false.</returns>
		public bool WaitOne( string collectionID, int millisecondsTimeout, bool exitContext )
		{
			Mutex mutex = null;

			lock( typeof( LogMutex ) )
			{
				mutex = mutexTable[ collectionID ] as Mutex;
			}

			if ( mutex == null )
			{
				throw new SimiasException( "Log mutex does not exist for collection " + collectionID );
			}

			return mutex.WaitOne( millisecondsTimeout, exitContext );
		}

		/// <summary>
		/// Blocks the current thread until the current instance receives a signal, 
		/// using a TimeSpan to measure the time interval and specifying whether to 
		/// exit the synchronization domain before the wait.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection associated 
		/// with the log file.</param>
		/// <param name="timeout">The number of milliseconds to wait, or a TimeSpan 
		/// that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="exitContext">true to exit the synchronization domain for the 
		/// context before the wait (if in a synchronized context), and reacquire it; 
		/// otherwise, false.</param>
		/// <returns>true if the current instance receives a signal; otherwise, false.</returns>
		public bool WaitOne( string collectionID, TimeSpan timeout, bool exitContext )
		{
			Mutex mutex = null;

			lock( typeof( LogMutex ) )
			{
				mutex = mutexTable[ collectionID ] as Mutex;
			}

			if ( mutex == null )
			{
				throw new SimiasException( "Log mutex does not exist for collection " + collectionID );
			}

			return mutex.WaitOne( timeout, exitContext );
		}
		#endregion
	}

	/// <summary>
	/// Class used to queue change log events.
	/// </summary>
	internal class ChangeLogEvent
	{
		#region Class Members
		/// <summary>
		/// Type of change events that are watched for.
		/// </summary>
		public enum ChangeEventType
		{
			/// <summary>
			/// Collection was created. Create a ChangeLogWriter.
			/// </summary>
			CollectionCreate,

			/// <summary>
			/// Collection was deleted. Delete the ChangeLogWriter.
			/// </summary>
			CollectionDelete,

			/// <summary>
			/// Node was created in a collection.
			/// </summary>
			NodeCreate,

			/// <summary>
			/// Node was changed in a collection.
			/// </summary>
			NodeChange,

			/// <summary>
			/// Node was deleted in a collection.
			/// </summary>
			NodeDelete
		}

		/// <summary>
		/// Type of event.
		/// </summary>
		private ChangeEventType type;

		/// <summary>
		/// Context for the event.
		/// </summary>
		private NodeEventArgs args;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the change event type.
		/// </summary>
		public ChangeEventType Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets the event context.
		/// </summary>
		public NodeEventArgs Args
		{
			get { return args; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="type">Type of change long event.</param>
		/// <param name="args">Context for the event.</param>
		public ChangeLogEvent( ChangeEventType type, NodeEventArgs args )
		{
			this.type = type;
			this.args = args;
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that the event context cookie has expired and that
	/// sync must dredge for changes.
	/// </summary>
	public class CookieExpiredException : SimiasException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public CookieExpiredException() :
			base ( "The event context cookie has expired." )
		{
		}

		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="innerException">The exception that cause the cookie to be invalid.</param>
		public CookieExpiredException( Exception innerException ) :
			base ( "The event context cookie has expired.", innerException )
		{
		}
		#endregion
	}


	/// <summary>
	/// Class used as a context object to look up the next set of events for a specified ChangeLogReader
	/// object.
	/// </summary>
	[ Serializable ]
	public class EventContext
	{
		#region Class Members
		/// <summary>
		/// Date and time of event.
		/// </summary>
		private DateTime timeStamp;

		/// <summary>
		/// Assigned event ID.
		/// </summary>
		private ulong recordID;

		/// <summary>
		/// Hint to where the last record was read from in the file. It may not be valid.
		/// </summary>
		private long hint;

		/// <summary>
		/// Used as separator in string representation.
		/// </summary>
		private const char valueSeparator = ':';
		#endregion

		#region Properties
		/// <summary>
		/// Gets the timestamp portion of the context.
		/// </summary>
		internal DateTime TimeStamp
		{
			get { return timeStamp; }
			set { timeStamp = value; }
		}

		/// <summary>
		/// Gets the record ID portion of the context.
		/// </summary>
		internal ulong RecordID
		{
			get { return recordID; }
			set { recordID = value; }
		}

		/// <summary>
		/// Gets the hint of where the last event was read from.
		/// </summary>
		internal long Hint
		{
			get { return hint; }
			set { hint = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="timeStamp">Date and time of event.</param>
		/// <param name="recordID">Assigned event ID.</param>
		/// <param name="hint">Hint to where the last record was read from in the file. It may not be valid.</param>
		internal EventContext( DateTime timeStamp, ulong recordID, long hint )
		{
			this.timeStamp = timeStamp;
			this.recordID = recordID;
			this.hint = hint;
		}

		/// <summary>
		/// Initializes a new instance from a string obtained from ToString
		/// </summary>
		/// <param name="cookie">The string representation of the context.</param>
		public EventContext( string cookie)
		{
			string [] values = cookie.Split(valueSeparator);
			if (values.Length != 3)
			{
				timeStamp = DateTime.MinValue;
				recordID = 0;
				hint = 0;
			}
			else
			{
				timeStamp = new DateTime(long.Parse(values[0]));
				recordID = ulong.Parse(values[1]);
				hint = long.Parse(values[2]);
			}
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Gets a string representation of this context.
		/// Can be used to store this cookie for later use.
		/// </summary>
		/// <returns>A formatted string representing the cookie.</returns>
		public override string ToString()
		{
			return (timeStamp.Ticks.ToString() + valueSeparator + recordID.ToString() + valueSeparator + hint.ToString());
		}
		#endregion
	}

	/// <summary>
	/// Contains the layout of the LogFile header information.
	/// </summary>
	public class LogFileHeader
	{
		#region Class Members
		/// <summary>
		/// Encoded lengths of the object fields.
		/// </summary>
		private const int logFileIDSize = 16;
		private const int maxLogRecordsSize = 4;
		private const int maxFlagsSize = 4;
		private const int lastRecordSize = 8;
		private const int recordLocationSize = 8;

		/// <summary>
		/// This is the total encoded record size.
		/// </summary>
		private const int encodedRecordSize = logFileIDSize + 
											  maxLogRecordsSize + 
											  maxFlagsSize +
											  lastRecordSize +
											  recordLocationSize;

		/// <summary>
		/// Contains the identifier for this log file.
		/// </summary>
		private string logFileID;

		/// <summary>
		/// Maximum number of records to keep persisted in the file.
		/// </summary>
		private uint maxLogRecords;

		/// <summary>
		/// Flags
		/// </summary>
		private uint flags;

		/// <summary>
		/// Last record written to the file. This is just a hint and may
		/// or may not be valid.
		/// </summary>
		private ulong lastRecord;

		/// <summary>
		/// File position of last record written to the file. This is just
		/// a hint and may or may not be valid.
		/// </summary>
		private long recordLocation;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the length of the record.
		/// </summary>
		public int Length
		{
			get { return RecordSize; }
		}

		/// <summary>
		/// Gets or sets the logFileID.
		/// </summary>
		public string LogFileID
		{
			get { return logFileID; }
			set { logFileID = value; }
		}

		/// <summary>
		/// Gets or sets the maximum number of ChangeLog records in the file.
		/// </summary>
		public uint MaxLogRecords
		{
			get { return maxLogRecords; }
			set { maxLogRecords = value; }
		}

		/// <summary>
		/// Returns the size of the LogFileHeader record.
		/// </summary>
		static public int RecordSize
		{
			get { return encodedRecordSize; }
		}

		/// <summary>
		/// Gets or sets the last record written to the file. This is only a
		/// hint and may or may not be valid.
		/// </summary>
		public ulong LastRecord
		{
			get { return lastRecord; }
			set { lastRecord = value; }
		}

		/// <summary>
		/// Gets or sets the file position of the last record in the file.
		/// This is just a hint and may or may not be valid.
		/// </summary>
		public long RecordLocation
		{
			get { return recordLocation; }
			set { recordLocation = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the struct.
		/// </summary>
		/// <param name="ID">Contains the identifier for this log file.</param>
		/// <param name="maxRecords">Maximum number of records to keep persisted in the file.</param>
		public LogFileHeader( string ID, uint maxRecords )
		{
			logFileID = ID;
			maxLogRecords = maxRecords;
			flags = 0;
			lastRecord = 0;
			recordLocation = 0;
		}

		/// <summary>
		/// Initializes a new instance of the struct from an encoded byte array.
		/// </summary>
		/// <param name="encodedRecord">LogFileHeader encoded record.</param>
		public LogFileHeader( byte[] encodedRecord )
		{
			int index = 0;

			byte[] guidArray = new byte[ 16 ];
			Array.Copy( encodedRecord, 0, guidArray, 0, guidArray.Length );
			Guid guid = new Guid( guidArray );
			logFileID = guid.ToString();
			index += logFileIDSize;

			maxLogRecords = BitConverter.ToUInt32( encodedRecord, index );
			index += maxLogRecordsSize;

			flags = BitConverter.ToUInt32( encodedRecord, index );
			index += maxFlagsSize;

			lastRecord = BitConverter.ToUInt64( encodedRecord, index );
			index += lastRecordSize;

			recordLocation = BitConverter.ToInt64( encodedRecord, index );
			index += recordLocationSize;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Converts the object to a formatted byte array.
		/// </summary>
		/// <returns>A formatted byte array containing the LogFileHeader data.</returns>
		public byte[] ToByteArray()
		{
			int index = 0;
			byte[] result = new byte[ RecordSize ];

			// Convert the object members to byte arrays.
			Guid guid = new Guid( logFileID );
			byte[] lfi = guid.ToByteArray();
			byte[] mlr = BitConverter.GetBytes( maxLogRecords );
			byte[] flg = BitConverter.GetBytes( flags );
			byte[] lr = BitConverter.GetBytes( lastRecord );
			byte[] rl = BitConverter.GetBytes( recordLocation );

			// Copy the converted byte arrays to the resulting array.
			Array.Copy( lfi, 0, result, index, lfi.Length );
			index += lfi.Length;

			Array.Copy( mlr, 0, result, index, mlr.Length );
			index += mlr.Length;

			Array.Copy( flg, 0, result, index, flg.Length );
			index += flg.Length;

			Array.Copy( lr, 0, result, index, lr.Length );
			index += lr.Length;

			Array.Copy( rl, 0, result, index, rl.Length );
			index += rl.Length;

			return result;
		}
		#endregion
	}

	/// <summary>
	/// Contains the layout of a ChangeLog record.
	/// </summary>
	public class ChangeLogRecord
	{
		#region Class Members
		/// <summary>
		/// Recordable change log operations.
		/// </summary>
		public enum ChangeLogOp
		{
			/// <summary>
			/// The node exists but no log record has been created.
			/// Do a brute force sync.
			/// </summary>
			Unknown,

			/// <summary>
			/// Node object was created.
			/// </summary>
			Created,

			/// <summary>
			/// Node object was deleted.
			/// </summary>
			Deleted,

			/// <summary>
			/// Node object was changed.
			/// </summary>
			Changed,

			/// <summary>
			/// Node object was renamed.
			/// </summary>
			Renamed
		};

		/// <summary>
		/// Encoded lengths of the object fields.
		/// </summary>
		private const int recordIDSize = 8;
		private const int epochSize = 8;
		private const int nodeIDSize = 16;
		private const int operationSize = 4;
		private const int flagSize = 2;
		private const int masterRevSize = 8;
		private const int slaveRevSize = 8;
		private const int fileLengthSize = 8;
		private const int typeSize = 4;

		/// <summary>
		/// This is the total encoded record size.
		/// </summary>
		private const int encodedRecordSize = recordIDSize + 
											  epochSize + 
											  nodeIDSize + 
											  operationSize + 
											  flagSize +
											  masterRevSize +
											  slaveRevSize +
											  fileLengthSize +
											  typeSize;

		/// <summary>
		/// Record identitifer for this entry.
		/// </summary>
		private ulong recordID;

		/// <summary>
		/// Date and time that event was recorded.
		/// </summary>
		private DateTime epoch;

		/// <summary>
		/// Identifier of Node object that triggered the event.
		/// </summary>
		private string nodeID;

		/// <summary>
		/// Node operation type.
		/// </summary>
		private ChangeLogOp operation;

		/// <summary>
		/// Flags passed to the event.
		/// </summary>
		private ushort flags;

		/// <summary>
		/// Master revision of node.
		/// </summary>
		private ulong masterRev;

		/// <summary>
		/// Local revision of node.
		/// </summary>
		private ulong slaveRev;

		/// <summary>
		/// Length of the file represented by the node if the node is a BaseFileTypeNode.
		/// </summary>
		private long fileLength;

		/// <summary>
		/// Base type of node.
		/// </summary>
		private NodeTypes.NodeTypeEnum type;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the record epoch.
		/// </summary>
		public DateTime Epoch
		{
			get { return epoch; }
			set { epoch = value; }
		}

		/// <summary>
		/// Gets or sets the event ID.
		/// </summary>
		public string EventID
		{
			get { return nodeID; }
			set { nodeID = value; }
		}

		/// <summary>
		/// Gets the length of the record.
		/// </summary>
		public int Length
		{
			get { return RecordSize; }
		}

		/// <summary>
		/// Gets or set the event operation.
		/// </summary>
		public ChangeLogOp Operation
		{
			get { return operation; }
			set { operation = value; }
		}

		/// <summary>
		/// Gets or sets the record ID.
		/// </summary>
		public ulong RecordID
		{
			get { return recordID; }
			set { recordID = value; }
		}

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		public ushort Flags
		{
			get { return flags; }
			set { flags = value; }
		}

		/// <summary>
		/// Gets or sets the master revision value.
		/// </summary>
		public ulong MasterRev
		{
			get { return masterRev; }
			set { masterRev = value; }
		}

		/// <summary>
		/// Gets or sets the slave revision value.
		/// </summary>
		public ulong SlaveRev
		{
			get { return slaveRev; }
			set { slaveRev = value; }
		}

		/// <summary>
		/// Gets or sets the file length value.
		/// </summary>
		public long FileLength
		{
			get { return fileLength; }
			set { fileLength = value; }
		}

		/// <summary>
		/// Gets or sets the base node type.
		/// </summary>
		public NodeTypes.NodeTypeEnum Type
		{
			get { return type; }
			set { type = value; }
		}

		/// <summary>
		/// Returns the size of the ChangeLogRecord.
		/// </summary>
		static public int RecordSize
		{
			get { return encodedRecordSize; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the struct.
		/// </summary>
		/// <param name="operation">Node operation type.</param>
		/// <param name="args">NodeEventArgs object that contains the change information.</param>
		public ChangeLogRecord( ChangeLogOp operation, NodeEventArgs args )
		{
			this.recordID = 0;
			this.epoch = args.TimeStamp;
			this.nodeID = args.ID;
			this.operation = operation;
			this.flags = ( ushort )args.Flags;
			this.masterRev = args.MasterRev;
			this.slaveRev = args.SlaveRev;
			this.fileLength = args.FileSize;
			this.type = ( NodeTypes.NodeTypeEnum )Enum.Parse( typeof( NodeTypes.NodeTypeEnum ), args.Type );
		}

		/// <summary>
		/// Initializes a new instance of the struct from an encoded byte array.
		/// </summary>
		/// <param name="encodedRecord">ChangeLogRecord encoded record.</param>
		public ChangeLogRecord( byte[] encodedRecord ) :
			this( encodedRecord, 0 )
		{
		}

		/// <summary>
		/// Initializes a new instance of the struct from an encoded byte array and index.
		/// </summary>
		/// <param name="encodedRecord">ChangeLogRecord encoded record.</param>
		/// <param name="index">Start index into the byte array.</param>
		public ChangeLogRecord( byte[] encodedRecord, int index )
		{
			recordID = BitConverter.ToUInt64( encodedRecord, index );
			index += recordIDSize;

			epoch = new DateTime( BitConverter.ToInt64( encodedRecord, index ) );
			index += epochSize;

			byte[] guidArray = new byte[ 16 ];
			Array.Copy( encodedRecord, index, guidArray, 0, guidArray.Length );
			Guid guid = new Guid( guidArray );
			nodeID = guid.ToString();
			index += nodeIDSize;

			operation = ( ChangeLogOp )Enum.ToObject( typeof( ChangeLogOp ), BitConverter.ToInt32( encodedRecord, index ) );
			index += operationSize;

			flags = BitConverter.ToUInt16( encodedRecord, index );
			index += flagSize;

			masterRev = BitConverter.ToUInt64( encodedRecord, index );
			index += masterRevSize;

			slaveRev = BitConverter.ToUInt64( encodedRecord, index );
			index += slaveRevSize;

			fileLength = BitConverter.ToInt64( encodedRecord, index );
			index += fileLengthSize;

			type = ( NodeTypes.NodeTypeEnum )Enum.ToObject( typeof( NodeTypes.NodeTypeEnum ), BitConverter.ToInt32( encodedRecord, index ) );
			index += typeSize;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Converts the object to a formatted byte array.
		/// </summary>
		/// <returns>A formatted byte array containing the ChangeLogRecord data.</returns>
		public byte[] ToByteArray()
		{
			int index = 0;
			byte[] result = new byte[ RecordSize ];

			// Convert the object members to byte arrays.
			Guid guid = new Guid( nodeID );
			byte[] nid = guid.ToByteArray();
			byte[] rid = BitConverter.GetBytes( recordID );
			byte[] ep = BitConverter.GetBytes( epoch.Ticks );
			byte[] op = BitConverter.GetBytes( ( int )operation );
			byte[] fl = BitConverter.GetBytes( flags );
			byte[] mr = BitConverter.GetBytes( masterRev );
			byte[] sr = BitConverter.GetBytes( slaveRev );
			byte[] fil = BitConverter.GetBytes( fileLength );
			byte[] tp = BitConverter.GetBytes( ( int )type );

			// Copy the converted byte arrays to the resulting array.
			Array.Copy( rid, 0, result, index, rid.Length );
			index += rid.Length;

			Array.Copy( ep, 0, result, index, ep.Length );
			index += ep.Length;

			Array.Copy( nid, 0, result, index, nid.Length );
			index += nid.Length;

			Array.Copy( op, 0, result, index, op.Length );
			index += op.Length;

			Array.Copy( fl, 0, result, index, fl.Length );
			index += fl.Length;

			Array.Copy( mr, 0, result, index, mr.Length );
			index += mr.Length;

			Array.Copy( sr, 0, result, index, sr.Length );
			index += sr.Length;

			Array.Copy( fil, 0, result, index, fil.Length );
			index += fil.Length;

			Array.Copy( tp, 0, result, index, tp.Length );
			index += tp.Length;

			return result;
		}
		#endregion
	}

	/// <summary>
	/// Class that lets the ChangeLog operate as a thread service.
	/// </summary>
	public class ChangeLog : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ChangeLog ) );

		/// <summary>
		/// Table used to keep track of ChangeLogWriter objects.
		/// </summary>
		private static Hashtable logWriterTable = new Hashtable();
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public ChangeLog()
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a change log writer for the specified collection.
		/// </summary>
		/// <param name="collectionID">The identifier for the collection.</param>
		public void CreateChangeLogWriter( string collectionID )
		{
			lock ( logWriterTable )
			{
				if ( !logWriterTable.ContainsKey( collectionID ) )
				{
					// Allocate a ChangeLogWriter object for this collection and store it in the table.
					logWriterTable.Add( collectionID, new ChangeLogWriter( collectionID ) );
					log.Debug( "Added ChangeLogWriter for collection {0}", collectionID );
				}
			}
		}

		/// <summary>
		/// Deletes a change log writer for the specified collection.
		/// </summary>
		/// <param name="collectionID">The identifier for the collection.</param>
		public void DeleteChangeLogWriter( string collectionID )
		{
			lock ( logWriterTable )
			{
				// Make sure the writer is in the table.
				if ( logWriterTable.ContainsKey( collectionID ) )
				{
					// Remove the ChangeLogWriter object from the table and dispose it.
					ChangeLogWriter logWriter = logWriterTable[ collectionID ] as ChangeLogWriter;
					logWriterTable.Remove( collectionID );

					// Get the path to the file before disposing it.
					string logPath = logWriter.LogFile;
					logWriter.Dispose();

					try { File.Delete( logPath ); } 
					catch {}

					log.Debug( "Deleted ChangeLogWriter for collection {0}", collectionID );
				}
			}
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">Configuration file object that indicates which Collection Store to use.</param>
		public void Start( Configuration config )
		{
			// Get a store object.
			Store store = Store.GetStore();

			// Get all of the collection objects and set up listeners for them.
			foreach (ShallowNode sn in store)
			{
				CreateChangeLogWriter( sn.ID );
			}

			log.Info( "Change Log Service started." );
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			// Remove all of the log writers from the table and dispose them.
			lock ( logWriterTable )
			{
				foreach ( ChangeLogWriter logWriter in logWriterTable.Values )
				{
					logWriter.Dispose();
				}

				// Clear the hashtable.
				logWriterTable.Clear();
			}

			log.Info( "Change Log Service stopped." );
		}
		#endregion
	}

	/// <summary>
	/// Object that records all changes to the Collection Store by listening to the Store events.
	/// </summary>
	internal class ChangeLogWriter : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Default maximum number of records to persist.
		/// </summary>
		private const uint defaultMaxPersistedRecords = 25000;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ChangeLogWriter ) );

		/// <summary>
		/// Collection that the log file belongs to.
		/// </summary>
		private string collectionID;
        
		/// <summary>
		/// Inprocess mutex used to control access to the log file.
		/// </summary>
		private LogMutex mutex;

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Subscribes to Collection Store node events.
		/// </summary>
		private EventSubscriber subscriber;

		/// <summary>
		/// Contains absolute path to the logfile.
		/// </summary>
		private string logFilePath;

		/// <summary>
		/// Contains the next record ID to assign to a ChangeLogRecord.
		/// </summary>
		private ulong recordID = 0;

		/// <summary>
		/// Contains the offset in the file to write.
		/// </summary>
		private long writePosition = LogFileHeader.RecordSize;

		/// <summary>
		/// Contains the last write position boundary.
		/// </summary>
		private long maxWritePosition = ( defaultMaxPersistedRecords * ChangeLogRecord.RecordSize ) + LogFileHeader.RecordSize;

		/// <summary>
		/// Queue used to process change events so the event thread does not have to block.
		/// </summary>
		private Queue eventQueue = new Queue();

		/// <summary>
		/// Flag that indicates if a thread is processing work on the queue.
		/// </summary>
		private bool threadScheduled = false;

		/// <summary>
		/// Contains the header to the log file.
		/// </summary>
		private LogFileHeader logHeader;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the file path for this change log.
		/// </summary>
		public string LogFile
		{
			get { return logFilePath; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="collectionID">Collection identifier to listen for events.</param>
		public ChangeLogWriter( string collectionID )
		{
			this.collectionID = collectionID;

			// Create the mutex that will protect this logfile.
			mutex = new LogMutex( collectionID, true );
			try
			{
				// Get the path to the store managed directory for this collection.
				string logFileDir = Path.Combine( Configuration.GetConfiguration().StorePath, "log" );
				if ( !Directory.Exists( logFileDir ) )
				{
					Directory.CreateDirectory( logFileDir );
				}

				// Build the log file path.
				logFilePath = Path.Combine( logFileDir, collectionID + ".changelog" );

				// Check to see if the file exists.
				if ( !File.Exists( logFilePath ) )
				{
					// Create the file.
					FileStream fs = new FileStream( logFilePath, FileMode.CreateNew, FileAccess.ReadWrite );
					try
					{
						// Create the log file header.
						logHeader = CreateLogFileHeader( fs, collectionID );
					}
					finally
					{
						fs.Close();
					}
				}
				else
				{
					// Open the existing log file.
					FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.ReadWrite );
					try
					{
						// Get the log header.
						logHeader = GetLogFileHeader( fs );

						// Check to see if the log file was shutdown gracefully.
						if ( CheckIntegrity( fs, logHeader, collectionID ) )
						{
							// Setup the current write position.
							SetCurrentWritePosition( fs );
						}
					}
					finally
					{
						fs.Close();
					}
				}

				// Setup the event listeners.
				subscriber = new EventSubscriber(collectionID );
				subscriber.NodeChanged += new NodeEventHandler( OnNodeChange );
				subscriber.NodeCreated += new NodeEventHandler( OnNodeCreate );
				subscriber.NodeDeleted += new NodeEventHandler( OnNodeDelete );
			}
			finally
			{
				mutex.ReleaseMutex( collectionID );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks to see if the file header is valid. If it was not, the file contents are
		/// truncated and reinitialized.
		/// </summary>
		/// <param name="fs">File stream that reference the log file.</param>
		/// <param name="header">The log file header.</param>
		/// <param name="collectionID">ID of the collection being monitored.</param>
		/// <returns>True if the file data is good, otherwise false.</returns>
		private bool CheckIntegrity( FileStream fs, LogFileHeader header, string collectionID )
		{
			bool result = true;

			if ( header.LogFileID != collectionID )
			{
				log.Error( "Log file corrupted. Reinitializing contents." );

				// Truncate the file data.
				fs.SetLength( 0 );
				logHeader = CreateLogFileHeader( fs, collectionID );
				result = false;
			}

			return result;
		}

		/// <summary>
		/// Creates a default log file header and writes it to the log file.
		/// </summary>
		/// <param name="fs">File stream that reference the log file.</param>
		/// <param name="collectionID">ID of collection being monitored.</param>
		/// <returns>A LogFileHeader object.</returns>
		private LogFileHeader CreateLogFileHeader( FileStream fs, string collectionID )
		{
			// Build the new log file header.
			LogFileHeader header = new LogFileHeader( collectionID, defaultMaxPersistedRecords );

			try
			{
				fs.Position = 0;
				fs.Write( header.ToByteArray(), 0, header.Length );
			}
			catch ( IOException e )
			{
				log.Error( "Failed to write log header. " + e.Message );
				throw;
			}

			return header;
		}

		/// <summary>
		/// Looks for the next write position in the log file using the hint saved the last time the file
		/// was written to. If the hint is invalid, then a brute force method is used.
		/// </summary>
		/// <param name="fs">FileStream object that references the log file.</param>
		private bool FindCurrentWritePosition( FileStream fs )
		{
			bool foundPosition = false;
			bool wrapped = false;

			try
			{
				// See if a last position was saved.
				if ( logHeader.RecordLocation >= ( long )LogFileHeader.RecordSize )
				{
					// Make sure that the saved record position in on a ChangeLogRecord boundary.
					if ( ( ( logHeader.RecordLocation - ( long )LogFileHeader.RecordSize ) % ( long )ChangeLogRecord.RecordSize ) == 0 )
					{
						// Using the hint in the log file header, see if the write position still exists in the file.
						if ( logHeader.RecordLocation == ( long )LogFileHeader.RecordSize )
						{
							// There are no records in the log or the log has wrapped back to the beginning.
							if ( fs.Length >= ( LogFileHeader.RecordSize + ChangeLogRecord.RecordSize ) )
							{
								// The file has wrapped, read the entry from the end of the file.
								fs.Position = fs.Length - ChangeLogRecord.RecordSize;
								wrapped = true;
							}
							else
							{
								// There are no entries in the file. No need to do a read.
								writePosition = logHeader.RecordLocation;
								recordID = logHeader.LastRecord;
								foundPosition = true;
							}
						}
						else
						{
							// There is at least one ChangeLogRecord and the file hasn't wrapped.
							fs.Position = logHeader.RecordLocation - ChangeLogRecord.RecordSize;
						}

						// No need to read if the offset was already found.
						if ( foundPosition == false )
						{
							// Read the last valid change log record that was written. 
							byte[] buffer = new byte[ ChangeLogRecord.RecordSize ];
							int bytesRead = fs.Read( buffer, 0, buffer.Length );
							if ( bytesRead > 0 )
							{
								ChangeLogRecord record = new ChangeLogRecord( buffer );
								ulong lastRecordID = record.RecordID;

								if ( record.RecordID == ( logHeader.LastRecord - 1 ) )
								{
									// Position the next for the next read if the event log has rolled over.
									if ( wrapped )
									{
										fs.Position = LogFileHeader.RecordSize;
									}

									// If there is a next record, then the file has rolled over and we need
									// to check the record ID to make sure it is less that the value that we
									// saved. If there is not an other record, then the hint is valid.
									bytesRead = fs.Read( buffer, 0, buffer.Length );
									if ( bytesRead > 0 )
									{
										record = new ChangeLogRecord( buffer );
										if ( lastRecordID > record.RecordID )
										{
											// This is the rollover point. The saved location is valid.
											writePosition = logHeader.RecordLocation;
											recordID = logHeader.LastRecord;
											foundPosition = true;
										}
									}
									else
									{
										// There are no more records and the file hasn't rolled over. The
										// saved location is valid.
										writePosition = logHeader.RecordLocation;
										recordID = logHeader.LastRecord;
										foundPosition = true;
									}
								}
							}
						}
					}
				}
			}
			catch ( IOException e )
			{
				log.Error( "FindCurrentWritePosition():" + e.Message );
			}

			return foundPosition;
		}

		/// <summary>
		/// Gets the log file header for the specified stream.
		/// </summary>
		/// <param name="fs">File stream containing the header.</param>
		/// <returns>A LogFileHeader object from the specified file stream if successful. Otherwise
		/// a null is returned.</returns>
		private LogFileHeader GetLogFileHeader( FileStream fs )
		{
			LogFileHeader logFileHeader = null;

			try
			{
				// Position the file pointer to the beginning of the file.
				fs.Position = 0;

				// Read the data.
				byte[] buffer = new byte[ LogFileHeader.RecordSize ];
				int bytesRead = fs.Read( buffer, 0, buffer.Length );
				if ( bytesRead == buffer.Length )
				{
					logFileHeader = new LogFileHeader( buffer );
				}
			}
			catch ( IOException e )
			{
				log.Error( "Failed to read event log header. {0}", e.Message );
				throw;
			}

			return logFileHeader;
		}

		/// <summary>
		/// Delegate that is called when a Node object has been changed.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeChange( NodeEventArgs args )
		{
			// Don't indicate local events.
			if (((NodeEventArgs.EventFlags)args.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
			{
				// Queue the event and schedule to come back later to process.
				lock ( eventQueue )
				{
					// Add the event to the queue.
					eventQueue.Enqueue( new ChangeLogEvent( ChangeLogEvent.ChangeEventType.NodeChange, args ) );
					
					// See if a thread has already been scheduled to take care of this event.
					if ( threadScheduled == false )
					{
						ThreadPool.QueueUserWorkItem( new WaitCallback( ProcessChangeLogEvent ) );
						threadScheduled = true;
					}
				}
			}
		}

		/// <summary>
		/// Delegate that is called when a Node object has been created.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeCreate( NodeEventArgs args )
		{
			// Don't indicate local events.
			if (((NodeEventArgs.EventFlags)args.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
			{
				// Queue the event and schedule to come back later to process.
				lock ( eventQueue )
				{
					// Add the event to the queue.
					eventQueue.Enqueue( new ChangeLogEvent( ChangeLogEvent.ChangeEventType.NodeCreate, args ) );
					
					// See if a thread has already been scheduled to take care of this event.
					if ( threadScheduled == false )
					{
						ThreadPool.QueueUserWorkItem( new WaitCallback( ProcessChangeLogEvent ) );
						threadScheduled = true;
					}
				}
			}
		}

		/// <summary>
		/// Delegate that is called when a Node object has been deleted.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeDelete( NodeEventArgs args )
		{
			// Don't indicate local events.
			if (((NodeEventArgs.EventFlags)args.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
			{
				// Queue the event and schedule to come back later to process.
				lock ( eventQueue )
				{
					// Add the event to the queue.
					eventQueue.Enqueue( new ChangeLogEvent( ChangeLogEvent.ChangeEventType.NodeDelete, args ) );
					
					// See if a thread has already been scheduled to take care of this event.
					if ( threadScheduled == false )
					{
						ThreadPool.QueueUserWorkItem( new WaitCallback( ProcessChangeLogEvent ) );
						threadScheduled = true;
					}
				}
			}
		}

		/// <summary>
		/// Processes node created, changed and deleted events.
		/// </summary>
		/// <param name="state">Not used.</param>
		private void ProcessChangeLogEvent( object state )
		{
			while ( true )
			{
				ChangeLogEvent work = null;

				// Lock the queue before accessing it to get the work to do.
				lock ( eventQueue )
				{
					if ( eventQueue.Count > 0 )
					{
						work = eventQueue.Dequeue() as ChangeLogEvent;
					}
					else
					{
						threadScheduled = false;
						break;
					}
				}

				switch ( work.Type )
				{
					case ChangeLogEvent.ChangeEventType.NodeChange:
					{
						ChangeLogRecord record = new ChangeLogRecord( ChangeLogRecord.ChangeLogOp.Changed, work.Args );
						WriteLog( record );
						break;
					}

					case ChangeLogEvent.ChangeEventType.NodeCreate:
					{
						ChangeLogRecord record = new ChangeLogRecord( ChangeLogRecord.ChangeLogOp.Created, work.Args );
						WriteLog( record );
						break;
					}
					
					case ChangeLogEvent.ChangeEventType.NodeDelete:
					{
						ChangeLogRecord record = new ChangeLogRecord( ChangeLogRecord.ChangeLogOp.Deleted, work.Args );
						WriteLog( record );
						break;
					}
				}
			}
		}

		/// <summary>
		/// Sets the next write position in the log file.
		/// </summary>
		/// <param name="fs">FileStream object that references the log file.</param>
		private void SetCurrentWritePosition( FileStream fs )
		{
			// See if the hint is valid so we don't have to brute-force the lookup.
			if ( !FindCurrentWritePosition( fs ) )
			{
				try
				{
					// Allocate a buffer to hold the records that are read.
					byte[] buffer = new byte[ ChangeLogRecord.RecordSize * 1000 ];

					// Skip over the file header.
					fs.Position = LogFileHeader.RecordSize;

					// Read the first record.
					int bytesRead = fs.Read( buffer, 0, ChangeLogRecord.RecordSize );
					if ( bytesRead > 0 )
					{
						// Instanitate the first record to compare.
						ChangeLogRecord record1 = new ChangeLogRecord( buffer );
						ChangeLogRecord record2 = null;

						// Read the next bunch of records.
						bytesRead = fs.Read( buffer, 0, buffer.Length );
						while ( bytesRead > 0 )
						{
							int index = 0;
							while ( ( index + ChangeLogRecord.RecordSize ) <= bytesRead )
							{
								// Instantiate the next record so the id's can be compared.
								record2 = new ChangeLogRecord( buffer, index );

								// See if the record id has rolled over.
								if ( record1.RecordID > record2.RecordID )
								{
									// Found the roll over point. Calculate the next write position.
									writePosition = ( fs.Position - bytesRead ) + index;
									recordID = record1.RecordID + 1;
									bytesRead = 0;
									break;
								}
								else
								{
									// Record id's are still increasing.
									index += ChangeLogRecord.RecordSize;
									record1 = record2;
								}
							}

							// If we haven't found the roll over point, keep reading.
							if ( bytesRead > 0 )
							{
								// Read the next buffer full.
								bytesRead = fs.Read( buffer, 0, buffer.Length );
							}
						}

						// There is either only one record in the file or the end of the file has been reached without
						// detecting the rollover point.
						if ( ( record2 == null ) || ( record1 == record2 ) )
						{
							// Next write position is the current position if it isn't at the size limit.
							writePosition = ( fs.Position >= maxWritePosition ) ? LogFileHeader.RecordSize : fs.Position;
							recordID = record1.RecordID + 1;
						}
					}
				}
				catch ( IOException e )
				{
					log.Error( e.Message );
				}
			}
		}

		/// <summary>
		/// Saves the change log header to the file.
		/// </summary>
		private void SetLogFileHeader()
		{
			// Acquire the mutex protecting the log file.
			mutex.WaitOne( collectionID );
			try
			{
				try
				{
					// Open the log file.
					FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.ReadWrite );
					try
					{
						// Initialize the log file header.
						logHeader.LastRecord = recordID;
						logHeader.RecordLocation = writePosition;

						// Position the file pointer to the right position within the file.
						fs.Position = 0;
						fs.Write( logHeader.ToByteArray(), 0, LogFileHeader.RecordSize );
					}
					finally
					{
						fs.Close();
					}
				}
				catch( IOException e )
				{
					log.Error( "Cannot save log file header - {0}", e.Message );
				}
			}
			finally
			{
				mutex.ReleaseMutex( collectionID );
			}
		}

		/// <summary>
		/// Writes the specified ChangeLogRecord to the ChangeLog file.
		/// </summary>
		/// <param name="record">ChangeLogRecord to write to file.</param>
		private void WriteLog( ChangeLogRecord record )
		{
			// Acquire the mutex protecting the log file.
			mutex.WaitOne( collectionID );
			try
			{
				try
				{
					// Open the log file.
					FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.ReadWrite );
					try
					{
						// Add the next ID to the record.
						record.RecordID = recordID;

						// Position the file pointer to the right position within the file.
						fs.Position = writePosition;
						fs.Write( record.ToByteArray(), 0, record.Length );

						// Update the members for the next write operation.
						++recordID;
						writePosition += record.Length;
						if ( writePosition >= maxWritePosition )
						{
							writePosition = LogFileHeader.RecordSize;
						}
					}
					finally
					{
						fs.Close();
					}
				}
				catch( IOException e )
				{
					log.Error( "Lost event - Epoch: {0}, ID: {1}, Operation: {2}. Exception {3}", record.Epoch, record.EventID, record.Operation, e.Message );
				}
			}
			finally
			{
				mutex.ReleaseMutex( collectionID );
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );

			// Save the log file header after disposing the object
			// because we don't want to take anymore events after
			// the log file header is saved.
			SetLogFileHeader();
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					subscriber.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~ChangeLogWriter()      
		{
			Dispose( false );
		}
		#endregion
	}

	/// <summary>
	/// Object that retrieves specified Collection Store changes from the ChangeLog file.
	/// </summary>
	public class ChangeLogReader
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ChangeLogReader ) );

		/// <summary>
		/// Collection that the log file belongs to.
		/// </summary>
		private string collectionID;
        
		/// <summary>
		/// Inprocess mutex used to control access to the log file.
		/// </summary>
		private LogMutex mutex;

		/// <summary>
		/// Contains absolute path to the logfile.
		/// </summary>
		private string logFilePath;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="collection">Collection object to listen for events on.</param>
		public ChangeLogReader( Collection collection )
		{
			collectionID = collection.ID;

			// Create the mutex that will protect this logfile.
			mutex = new LogMutex( collectionID );

			// Build the log file path.
			logFilePath = Path.Combine( Path.Combine( collection.StoreReference.StorePath, "log" ), collection.ID + ".changelog" );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the offset of the next event to be read.
		/// 
		/// NOTE: The entire file must be locked before making this call.
		/// </summary>
		/// <param name="fs">FileStream object associated with the event log file.</param>
		/// <param name="cookie">Event context indicating the next event to be read.</param>
		/// <returns>True if the next offset was found, otherwise false is returned.</returns>
		private bool GetReadPosition( FileStream fs, EventContext cookie )
		{
			bool foundOffset = false;
			int bytesRead = 0;
			byte[] buffer = new byte[ ChangeLogRecord.RecordSize ];

			// Make sure that there is a valid cookie.
			if ( cookie != null )
			{
				try
				{
					// Using the hint in the cookie, see if the read position still exists in the file.
					fs.Position = cookie.Hint;
					bytesRead = fs.Read( buffer, 0, buffer.Length );
					if ( bytesRead > 0 )
					{
						ChangeLogRecord record = new ChangeLogRecord( buffer );
						if ( ( record.RecordID == cookie.RecordID ) && ( ( record.Epoch == cookie.TimeStamp ) || ( cookie.TimeStamp == DateTime.MinValue ) ) )
						{
							// Found the record that we were looking for. If the cookie indicates the no data has
							// ever been read, then position the file pointer back to the first record so it doesn't
							// get skipped. Otherwise, if the record and cookie match exactly, the file pointer is
							// already at the right position to read the next record.
							if ( cookie.TimeStamp == DateTime.MinValue )
							{
								// We have yet to read a record, start at the beginning.
								fs.Position = LogFileHeader.RecordSize;
							}

							foundOffset = true;
						}
					}
					else if ( ( bytesRead == 0 ) && ( cookie.RecordID == 0 ) && ( cookie.TimeStamp == DateTime.MinValue ) )
					{
						fs.Position = LogFileHeader.RecordSize;
						foundOffset = true;
					}
				}
				catch ( IOException e )
				{
					log.Error( "GetReadPosition():" + e.Message );
				}
				catch ( Exception e )
				{
					char[] debugBuffer = new char[ bytesRead ];
					for ( int i = 0; i < bytesRead; ++i )
					{
						debugBuffer[ i ] = Convert.ToChar( buffer[ i ] );
					}

					log.Debug( e, "Failed to get read position" );
					log.Debug( "Cookie: RecordID = {0}, TimeStamp = {1}, Hint = {2}", cookie.RecordID, cookie.TimeStamp, cookie.Hint );
					log.Debug( "Bytes read = {0}, buffer = {1}", bytesRead, debugBuffer );
				}
			}

			return foundOffset;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets an EventContext object that contains the latest event information.
		/// </summary>
		/// <returns>An EventContext object that is up-to-date with the latest information in the event log.</returns>
		public EventContext GetEventContext()
		{
			EventContext cookie = null;

			// Acquire the mutex protecting the log file.
			mutex.WaitOne( collectionID );
			try
			{
				try
				{
					// Open the log file.
					FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.Read );
					try
					{
						// Allocate a buffer to hold the records that are read.
						byte[] buffer = new byte[ ChangeLogRecord.RecordSize * 1000 ];

						// Skip over the file header.
						fs.Position = LogFileHeader.RecordSize;

						// Read the first record.
						int bytesRead = fs.Read( buffer, 0, ChangeLogRecord.RecordSize );
						if ( bytesRead > 0 )
						{
							// Instanitate the first record to compare.
							ChangeLogRecord record1 = new ChangeLogRecord( buffer );
							ChangeLogRecord record2 = null;

							// Read the next bunch of records.
							bytesRead = fs.Read( buffer, 0, buffer.Length );
							while ( bytesRead > 0 )
							{
								int index = 0;
								while ( ( index + ChangeLogRecord.RecordSize ) <= bytesRead )
								{
									// Instantiate the next record so the id's can be compared.
									record2 = new ChangeLogRecord( buffer, index );

									// See if the record id has rolled over.
									if ( record1.RecordID > record2.RecordID )
									{
										// Found the roll over point. Calculate the hint position and create
										// the cookie.
										long hint = ( fs.Position - bytesRead ) + ( index - ChangeLogRecord.RecordSize );
										cookie = new EventContext( record1.Epoch, record1.RecordID, hint );
										bytesRead = 0;
										break;
									}
									else
									{
										// Record id's are still increasing.
										index += ChangeLogRecord.RecordSize;
										record1 = record2;
									}
								}

								// If we haven't found the roll over point, keep reading.
								if ( bytesRead > 0 )
								{
									// Read the next buffer full.
									bytesRead = fs.Read( buffer, 0, buffer.Length );
								}
							}

							// There is either only one record in the file or the end of the file has been reached
							// without detecting a rollover point.
							if ( ( record2 == null ) || ( record1 == record2 ) )
							{
								cookie = new EventContext( record1.Epoch, record1.RecordID, fs.Position - ChangeLogRecord.RecordSize );
							}
						}
						else
						{
							// There are no records in the file yet. Return to use the first record.
							cookie = new EventContext( DateTime.MinValue, 0, fs.Position );
						}
					}
					finally
					{
						fs.Close();
					}
				}
				catch( IOException e )
				{
					log.Error( e.Message );
				}
			}
			finally
			{
				mutex.ReleaseMutex( collectionID );
			}

			return cookie;
		}

		/// <summary>
		/// Gets the events that have been recorded in the ChangeLog from the specified event context.
		/// </summary>
		/// <param name="cookie">Event context received from call to GetEventContext method.</param>
		/// <param name="changeList">Receives a list of ChangeLogRecords that are the changes to the Collection Store.</param>
		/// <returns>True if there is more data to get. Otherwise false is returned.</returns>
		public bool GetEvents( EventContext cookie, out ArrayList changeList )
		{
			long lastRecordOffset;

			// Initialize the out parameter.
			changeList = new ArrayList();
			byte[] buffer = new byte[ ChangeLogRecord.RecordSize * 100 ];
			int bytesRead = 0;

			// Acquire the mutex protecting the log file.
			mutex.WaitOne( collectionID );
			try
			{
				try
				{
					// Open the ChangeLog file.
					FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.Read );
					try
					{
						// Calculate where changes need to start being added based on the EventContext.
						if ( !GetReadPosition( fs, cookie ) )
						{
							// The cookie has been pushed out of the log.
							throw new CookieExpiredException();
						}

						// Read the data.
						bytesRead = fs.Read( buffer, 0, buffer.Length );

						// Calculate the offset to the last record read.
						lastRecordOffset = fs.Position - ChangeLogRecord.RecordSize;
					}
					finally
					{
						fs.Close();
					}
				}
				catch( Exception e )
				{
					throw new CookieExpiredException( e );
				}
			}
			finally
			{
				mutex.ReleaseMutex( collectionID );
			}

			// Make sure that something was read.
			for ( int i = 0; i < bytesRead; i += ChangeLogRecord.RecordSize )
			{
				changeList.Add( new ChangeLogRecord( buffer, i ) );
			}

			// If there were events to pass back, update the cookie.
			if ( changeList.Count > 0 )
			{
				ChangeLogRecord record = ( ChangeLogRecord )changeList[ changeList.Count - 1 ];
				cookie.TimeStamp = record.Epoch;
				cookie.RecordID = record.RecordID;
				cookie.Hint = lastRecordOffset;
			}

			// If less data was read that we had buffer for, we have all of the data.
			return ( bytesRead == buffer.Length ) ? true : false;
		}
		#endregion
	}
}
