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
using Simias.Event;
using Simias.Service;
using Simias.Storage;

namespace Simias.Storage
{
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
		internal EventContext( string cookie)
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
		/// Log file states.
		/// </summary>
		public enum LogState
		{
			/// <summary>
			/// The log file has been closed.
			/// </summary>
			Closed,

			/// <summary>
			/// The log file is open.
			/// </summary>
			Opened
		};

		/// <summary>
		/// Encoded lengths of the object fields.
		/// </summary>
		private const int logFileIDSize = 16;
		private const int maxLogRecordsSize = 4;
		private const int maxFlagsSize = 4;

		/// <summary>
		/// This is the total encoded record size.
		/// </summary>
		private const int encodedRecordSize = logFileIDSize + maxLogRecordsSize + maxFlagsSize;

		// File states that indicate a graceful shutdown.
		private const uint opened = 1;
		private const uint closed = 0;

		/// <summary>
		/// Contains the identifier for this log file.
		/// </summary>
		private string logFileID;

		/// <summary>
		/// Maximum number of records to keep persisted in the file.
		/// </summary>
		private uint maxLogRecords;

		/// <summary>
		/// Flags used to tell if the service went down nicely.
		/// </summary>
		private uint flags;
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
		/// Gets or sets the log state.
		/// </summary>
		public LogState State
		{
			get { return ( flags == opened ) ? LogState.Opened : LogState.Closed; }
			set { flags = ( value == LogState.Opened ) ? opened : closed; }
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

			// Copy the converted byte arrays to the resulting array.
			Array.Copy( lfi, 0, result, index, lfi.Length );
			index += lfi.Length;

			Array.Copy( mlr, 0, result, index, mlr.Length );
			index += mlr.Length;

			Array.Copy( flg, 0, result, index, flg.Length );
			index += flg.Length;

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

		/// <summary>
		/// This is the total encoded record size.
		/// </summary>
		private const int encodedRecordSize = recordIDSize + epochSize + nodeIDSize + operationSize + flagSize;

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
		/// <param name="epoch">Date and time that event was recorded.</param>
		/// <param name="nodeID">Identifier of Node object that triggered the event.</param>
		/// <param name="operation">Node operation type.</param>
		/// <param name="flags">Flags passed to the event.</param>
		public ChangeLogRecord( DateTime epoch, string nodeID, ChangeLogOp operation, ushort flags )
		{
			this.recordID = 0;
			this.epoch = epoch;
			this.nodeID = nodeID;
			this.operation = operation;
			this.flags = flags;
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
		/// Configuration object for the Collection Store.
		/// </summary>
		private Configuration config;

		/// <summary>
		/// Table used to keep track of ChangeLogWriter objects.
		/// </summary>
		private Hashtable logWriterTable = new Hashtable();

		/// <summary>
		/// Subscribes to Collection Store events.
		/// </summary>
		private EventSubscriber subscriber;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public ChangeLog()
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Delegate that is called when a Node object has been created.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnCollectionCreate( NodeEventArgs args )
		{
			// If the Node ID matches the Collection ID then this is a collection event.
			if ( args.Collection == args.Node )
			{
				lock ( logWriterTable )
				{
					if ( !logWriterTable.ContainsKey( args.Collection ) )
					{
						// Allocate a ChangeLogWriter object for this collection and store it in the table.
						logWriterTable.Add( args.Collection, new ChangeLogWriter( config, args.Collection ) );
						log.Debug( "Added ChangeLogWriter for collection {0}", args.Collection );
					}
				}
			}
		}

		/// <summary>
		/// Delegate that is called when a Node object has been deleted.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnCollectionDelete( NodeEventArgs args )
		{
			// If the Node ID matches the Collection ID then this is a collection event.
			if ( args.Collection == args.Node )
			{
				lock ( logWriterTable )
				{
					// Make sure the writer is in the table.
					if ( logWriterTable.ContainsKey( args.Collection ) )
					{
						// Remove the ChangeLogWriter object from the table and dispose it.
						ChangeLogWriter logWriter = logWriterTable[ args.Collection ] as ChangeLogWriter;
						logWriterTable.Remove( args.Collection );
						logWriter.Dispose();
						log.Debug( "Deleted ChangeLogWriter for collection {0}", args.Collection );
					}
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
			this.config = config;

			// Setup the event listeners.
			subscriber = new EventSubscriber(  );
			subscriber.NodeCreated += new NodeEventHandler( OnCollectionCreate );
			subscriber.NodeDeleted += new NodeEventHandler( OnCollectionDelete );

			// Get a store object.
			Store store = Store.GetStore();

			// Get all of the collection objects and set up listeners for them.
			foreach (ShallowNode sn in store)
			{
				Collection c = new Collection(store, sn);
				lock ( logWriterTable )
				{
					// Make sure the entry isn't already in the table.
					if ( !logWriterTable.ContainsKey( c.ID ) )
					{
						// Allocate a ChangeLogWriter object for this collection and store it in the table.
						logWriterTable.Add( c.ID, new ChangeLogWriter( config, c.ID ) );
						log.Debug( "Added ChangeLogWriter for collection {0}", c.ID );
					}
				}
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
			// Dispose the collection listeners.
			subscriber.Dispose();

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
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="config">Store configuration file.</param>
		/// <param name="collectionID">Collection identifier to listen for events.</param>
		public ChangeLogWriter( Configuration config, string collectionID )
		{
			// Get the path to the store managed directory for this collection.
			string logFileDir = Path.Combine( config.StorePath, "log" );
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
				FileStream fs = new FileStream( logFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None );
				try
				{
					// Create the log file header.
					CreateLogFileHeader( fs, collectionID );
				}
				finally
				{
					fs.Close();
				}
			}
			else
			{
				// Open the existing log file.
				FileStream fs = new FileStream( logFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None );
				try
				{
					// Check to see if the log file was shutdown gracefully.
					if ( CheckIntegrity( fs, collectionID ) )
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
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks to see if the file was gracefully shutdown. If it was not, the file contents are
		/// truncated and reinitialized.
		/// </summary>
		/// <param name="fs">File stream that reference the log file.</param>
		/// <param name="collectionID">ID of the collection being monitored.</param>
		/// <returns>True if the file data is good, otherwise false.</returns>
		private bool CheckIntegrity( FileStream fs, string collectionID )
		{
			LogFileHeader logHeader = GetLogFileHeader( fs );
			if ( ( logHeader.LogFileID == collectionID ) && ( logHeader.State == LogFileHeader.LogState.Closed ) )
			{
				// Set the status to opened.
				WriteLogFileStatus( fs, LogFileHeader.LogState.Opened );
				return true;
			}
			else
			{
				log.Error( "Log file corrupted. Reinitializing contents." );

				// Truncate the file data.
				fs.SetLength( 0 );
				CreateLogFileHeader( fs, collectionID );
				return false;
			}
		}

		/// <summary>
		/// Creates a default log file header and writes it to the log file.
		/// </summary>
		/// <param name="fs">File stream that reference the log file.</param>
		/// <param name="collectionID">ID of collection being monitored.</param>
		private void CreateLogFileHeader( FileStream fs, string collectionID )
		{
			// Build the new log file header.
			LogFileHeader header = new LogFileHeader( collectionID, defaultMaxPersistedRecords );
			header.State = LogFileHeader.LogState.Opened;

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
		}

		/// <summary>
		/// Gets the log file header for the specified stream.
		/// </summary>
		/// <param name="fs">File stream containing the header.</param>
		/// <returns>A LogFileHeader object from the specified file stream if successful. Otherwise
		/// a null is returned.</returns>
		private LogFileHeader GetLogFileHeader( FileStream fs )
		{
			LogFileHeader logHeader = null;

			try
			{
				// Position the file pointer to the beginning of the file.
				fs.Position = 0;

				// Read the data.
				byte[] buffer = new byte[ LogFileHeader.RecordSize ];
				int bytesRead = fs.Read( buffer, 0, buffer.Length );
				if ( bytesRead == buffer.Length )
				{
					logHeader = new LogFileHeader( buffer );
				}
			}
			catch ( IOException e )
			{
				log.Error( "Failed to read event log header. {0}", e.Message );
				throw;
			}

			return logHeader;
		}

		/// <summary>
		/// Delegate that is called when a Node object has been changed.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeChange( NodeEventArgs args )
		{
			ChangeLogRecord record = new ChangeLogRecord( DateTime.Now, args.ID, ChangeLogRecord.ChangeLogOp.Changed, ( ushort )args.Flags );
			WriteLog( record );
		}

		/// <summary>
		/// Delegate that is called when a Node object has been created.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeCreate( NodeEventArgs args )
		{
			ChangeLogRecord record = new ChangeLogRecord( DateTime.Now, args.ID, ChangeLogRecord.ChangeLogOp.Created, ( ushort )args.Flags );
			WriteLog( record );
		}

		/// <summary>
		/// Delegate that is called when a Node object has been deleted.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		private void OnNodeDelete( NodeEventArgs args )
		{
			ChangeLogRecord record = new ChangeLogRecord( DateTime.Now, args.ID, ChangeLogRecord.ChangeLogOp.Deleted, ( ushort )args.Flags );
			WriteLog( record );
		}

		/// <summary>
		/// Opens the event log file, retrying if the file is currently in use.
		/// </summary>
		/// <returns>A FileStream object associated with the event log file.</returns>
		private FileStream OpenLogFile()
		{
			FileStream fs = null;

			// Stay in the loop until the file is opened or we tried the maximum number of times.
			for ( int i = 0; ( fs == null ) && ( i < 10 ); ++i )
			{
				try
				{
					// Open the log file.
					fs = new FileStream( logFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None );
				}
				catch ( IOException )
				{
					// Wait for a moment before trying to open the file again.
					Thread.Sleep( 100 );
				}
			}

			return fs;
		}

		/// <summary>
		/// Sets the next write position in the log file.
		/// </summary>
		/// <param name="fs">FileStream object that references the log file.</param>
		private void SetCurrentWritePosition( FileStream fs )
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

		/// <summary>
		/// Writes the specified ChangeLogRecord to the ChangeLog file.
		/// </summary>
		/// <param name="record">ChangeLogRecord to write to file.</param>
		private void WriteLog( ChangeLogRecord record )
		{
			// Open the log file.
			FileStream fs = OpenLogFile();
			if ( fs != null )
			{
				try
				{
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
					catch ( IOException e )
					{
						log.Error( "Failed to write event to event log. Epoch: {0}, ID: {1}, Operation: {2} {3}", record.Epoch, record.EventID, record.Operation, e.Message );
					}
				}
				finally
				{
					fs.Close();
				}
			}
			else
			{
				log.Error( "Failed to open event log file. Lost event - Epoch: {0}, ID: {1}, Operation: {2}", record.Epoch, record.EventID, record.Operation );
			}
		}

		/// <summary>
		/// Write the log file status out to the log file.
		/// </summary>
		/// <param name="status">Status of the log file.</param>
		private void WriteLogFileStatus( LogFileHeader.LogState status )
		{
			// Open the log file.
			FileStream fs = OpenLogFile();
			if ( fs != null )
			{
				try
				{
					WriteLogFileStatus( fs, status );
				}
				finally
				{
					fs.Close();
				}
			}
			else
			{
				log.Error( "Failed to open event log file. Lost status - status: {0}", status );
			}
		}

		/// <summary>
		/// Write the log file status out to the log file.
		/// </summary>
		/// <param name="fs">FileStream object that references the log file.</param>
		/// <param name="status">Status of the log file.</param>
		private void WriteLogFileStatus( FileStream fs, LogFileHeader.LogState status )
		{
			try
			{
				// Get the LogFileHeader.
				LogFileHeader logHeader = GetLogFileHeader( fs );
				if ( logHeader != null )
				{
					// Set the status in the header.
					logHeader.State = status;

					// Position the file pointer to the right position within the file.
					fs.Position = 0;
					fs.Write( logHeader.ToByteArray(), 0, logHeader.Length );
				}
				else
				{
					log.Error( "Failed to read log file header." );
				}
			}
			catch ( IOException e )
			{
				log.Error( "Failed to write log file status. {0}", e.Message );
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

					// Write out to the log file that it closed nicely.
					WriteLogFileStatus( LogFileHeader.LogState.Closed );
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

			// Make sure that there is a valid cookie.
			if ( cookie != null )
			{
				try
				{
					byte[] buffer = new byte[ ChangeLogRecord.RecordSize ];

					// Using the hint in the cookie, see if the read position still exists in the file.
					fs.Position = cookie.Hint;
					int bytesRead = fs.Read( buffer, 0, buffer.Length );
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
			}

			return foundOffset;
		}

		/// <summary>
		/// Opens the event log file, retrying if the file is currently in use.
		/// </summary>
		/// <returns>A FileStream object associated with the event log file.</returns>
		private FileStream OpenLogFile()
		{
			FileStream fs = null;

			// Stay in the loop until the file is opened or we tried the maximum number of times.
			for ( int i = 0; ( fs == null ) && ( i < 10 ); ++i )
			{
				try
				{
					// Open the log file.
					fs = new FileStream( logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read );
				}
				catch ( IOException e )
				{
					log.Error( "Failed to open log file. {0}", e.Message );

					// Wait for a moment before trying to open the file again.
					Thread.Sleep( 100 );
				}
			}

			return fs;
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
			
			// Open the event log file.
			FileStream fs = OpenLogFile();
			if ( fs != null )
			{
				try
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
					catch ( IOException e )
					{
						log.Error( e.Message );
					}
				}
				finally
				{
					fs.Close();
				}
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
			bool moreData = true;

			// Initialize the out parameter.
			changeList = new ArrayList();
			byte[] buffer = new byte[ ChangeLogRecord.RecordSize * 100 ];
			int bytesRead = 0;

			// Open the ChangeLog file.
			FileStream fs = OpenLogFile();
			if ( fs != null )
			{
				try
				{
					try
					{
						// Calculate where changes need to start being added based on the EventContext.
						if ( GetReadPosition( fs, cookie ) )
						{
							// Read the data.
							bytesRead = fs.Read( buffer, 0, buffer.Length );
						}
						else
						{
							// The cookie has been pushed out of the log.
							throw new CookieExpiredException();
						}

						long lastRecordOffset = fs.Position - ChangeLogRecord.RecordSize;

						// Done reading from the file. Close it.
						fs.Close();

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
						moreData = ( bytesRead == buffer.Length ) ? true : false;
					}
					catch ( IOException e )
					{
						throw new CookieExpiredException( e );
					}
				}
				finally
				{
					fs.Close();
				}
			}
			else
			{
				throw new CookieExpiredException();
			}

			return moreData;
		}
		#endregion
	}
}
