using System;
using System.IO;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Class used to set up the state for a sync pass.
	/// </summary>
	public class StartSyncInfo
	{
		/// <summary>
		/// The collection to sync.
		/// </summary>
		public string			CollectionID;
		/// <summary>
		/// The sync context.
		/// </summary>
		public string			Context;
		/// <summary>
		/// True if only changes since last sync are wanted.
		/// </summary>
		public bool				ChangesOnly;
		/// <summary>
		/// True if the client has changes. Used to Determine if there is work.
		/// </summary>
		public bool				ClientHasChanges;
		/// <summary>
		/// The Status of this sync.
		/// </summary>
		public StartSyncStatus	Status;
		/// <summary>
		/// The access allowed to the collection.
		/// </summary>
		public Access.Rights	Access;

		/// <summary>
		/// Constructor.
		/// </summary>
		public StartSyncInfo()
		{
			Context = "";
		}

		/// <summary>
		/// Constructs a SyncStartInfo from a serialized object.
		/// </summary>
		/// <param name="reader"></param>
		public StartSyncInfo(BinaryReader reader)
		{
			CollectionID = new Guid(reader.ReadBytes(16)).ToString();
			Context = reader.ReadString();
			ChangesOnly = reader.ReadBoolean();
			ClientHasChanges = reader.ReadBoolean();
			Status = (StartSyncStatus)reader.ReadByte();
			Access = (Access.Rights)reader.ReadByte();
		}

		/// <summary>
		/// Serializes this instance into a stream.
		/// </summary>
		/// <param name="writer">The stream to serialize to.</param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(new Guid(CollectionID).ToByteArray());
			writer.Write(Context);
			writer.Write(ChangesOnly);
			writer.Write(ClientHasChanges);
			writer.Write((byte)Status);
			writer.Write((byte)Access);
		}

	}
}
