using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

namespace FileDelta
{
	[Serializable]
	class ChangeRecord
	{
		public bool		Matched;
		public int		StartBlock;
		public int		EndBlock;
		public byte[]	Data;

		public int Length
		{
			get {return StartBlock;}
			set {StartBlock = value;}
		}
	}

	class ServerFile : FileDelta
	{
		public ServerFile(string file) :
			base(file)
		{
		}
	}

	class ClientFile : FileDelta
	{
		public ClientFile(string file) :
			base(file)
		{
		}
	}

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class FileDelta
	{
		internal		StrongWeakHashtable table = new StrongWeakHashtable();
		string			file;
		const int		BlockSize = 4096;
		BinaryReader	inStream;
		BinaryWriter	outStream;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage : {0} (file1) (file2)", "FileDelta");
				return;
			}
		
			string file1 = Path.GetFullPath(args[0]);
			string file2 = Path.GetFullPath(args[1]);
		
			ServerFile sFile = new ServerFile(file1);
			ClientFile cFile = new ClientFile(file2);
			ArrayList fileStamps = sFile.GetHashStamps();
			ArrayList changes = cFile.GetUploadDeltas(fileStamps);
			ArrayList misses = cFile.GetDownloadDeltas(fileStamps);

			sFile.WriteChanges(changes, @"c:\temp.tmp");

			foreach (ChangeRecord change in changes)
			{
				if (change.Matched)
				{
					Console.WriteLine("Found Match Block {0} to Block {1}", change.StartBlock, change.EndBlock);
				}
				else
				{
					Console.WriteLine("Found change size = {0}", change.Length);
					if (change.Length < 80)
					{
						foreach(byte b in change.Data)
						{
							char c = (char)b;
							Console.Write(c);
						}
						Console.WriteLine();
					}
				}
			}
		}

		public FileDelta(string file)
		{
			this.file = file;
		}

		public ArrayList GetUploadDeltas(ArrayList originalStamps)
		{
			table.Add(originalStamps);
			ArrayList changes = new ArrayList();

			BinaryReader	reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
			int				bytesRead = BlockSize * 16;
			byte[]			buffer = new byte[BlockSize * 16];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			ChangeRecord	change;
			int				startByte = 0;
			int				endByte = 0;
			int				endOfLastMatch = 0;
			byte			dropByte = 0;
								
			while (bytesRead != 0)
			{
				bytesRead = reader.Read(buffer, readOffset, bytesRead - readOffset);
				if (bytesRead == 0)
					break;
				bytesRead = bytesRead == 0 ? bytesRead : bytesRead + readOffset;
				
				if (bytesRead >= BlockSize)
				{
					endByte = startByte + BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(BlockSize, dropByte, buffer[endByte]);
						ArrayList entryList = table[entry.WeakHash];
						if (entryList != null)
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							int eIndex = entryList.IndexOf(entry);
							if (eIndex != -1)
							{
								entry = (HashEntry)entryList[eIndex];
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									change = new ChangeRecord();
									change.Matched = false;
									change.Length = startByte - endOfLastMatch;
									change.Data = new byte[change.Length];
									Array.Copy(buffer, endOfLastMatch, change.Data, 0, change.Length);
									changes.Add(change);
								}
								startByte = endByte + 1;
								endByte = startByte + BlockSize - 1;
								endOfLastMatch = startByte;
								recomputeWeakHash = true;

								change = changes.Count == 0 ? null : (ChangeRecord)changes[changes.Count - 1];
								if (change != null && change.Matched && change.EndBlock == entry.BlockNumber -1)
								{
									change.EndBlock = entry.BlockNumber;
								}
								else
								{
									// Save the matched block.
									change = new ChangeRecord();
									change.Matched = true;
									change.StartBlock = entry.BlockNumber;
									change.EndBlock = entry.BlockNumber;
									changes.Add(change);
								}
								continue;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					// We need to copy any data that has not been saved.
					if (endOfLastMatch == 0)
					{
						// We don't want to send to large of a buffer create a ChangeRecord
						// for the data in the buffer.
						change = new ChangeRecord();
						change.Matched = false;
						change.Length = startByte - endOfLastMatch;
						change.Data = new byte[change.Length];
						Array.Copy(buffer, endOfLastMatch, change.Data, 0, change.Length);
						changes.Add(change);
						endOfLastMatch = startByte;
					}
					readOffset = bytesRead - endOfLastMatch;
					Array.Copy(buffer, endOfLastMatch, buffer, 0, readOffset);
					startByte = startByte - endOfLastMatch; //0;
					endOfLastMatch = 0;
					endByte = readOffset - 1;
				}
				else
				{
					endByte = bytesRead - 1;
					break;
				}
			}

			// Get the remaining changes.
			if (endOfLastMatch != endByte)//== 0 && endByte != 0)
			{
				change = new ChangeRecord();
				change.Matched = false;
				change.Length = endByte - endOfLastMatch + 1;
				change.Data = new byte[change.Length];
				Array.Copy(buffer, endOfLastMatch, change.Data, 0, change.Length);
				changes.Add(change);
			}

			reader.Close();
			return changes;
		}

		public ArrayList GetDownloadDeltas(ArrayList originalStamps)
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Add(originalStamps);
			bool[] hits = new bool[originalStamps.Count];
			ArrayList missing = new ArrayList();

			BinaryReader	reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
			int				bytesRead = BlockSize * 16;
			byte[]			buffer = new byte[BlockSize * 16];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			int				startByte = 0;
			int				endByte = 0;
			byte			dropByte = 0;
								
			while (bytesRead != 0)
			{
				bytesRead = reader.Read(buffer, readOffset, bytesRead - readOffset);
				if (bytesRead == 0)
					break;
				bytesRead = bytesRead == 0 ? bytesRead : bytesRead + readOffset;
				
				if (bytesRead >= BlockSize)
				{
					endByte = startByte + BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(BlockSize, dropByte, buffer[endByte]);
						ArrayList entryList = table[entry.WeakHash];
						if (entryList != null)
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							int eIndex = entryList.IndexOf(entry);
							if (eIndex != -1)
							{
								entry = (HashEntry)entryList[eIndex];
								// We found a match save the match;
								hits[entry.BlockNumber] = true;

								startByte = endByte + 1;
								endByte = startByte + BlockSize - 1;
								recomputeWeakHash = true;
								continue;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					readOffset = bytesRead - startByte;
					Array.Copy(buffer, startByte, buffer, 0, readOffset);
					startByte = 0;
				}
				else
				{
					break;
				}
			}

			ChangeRecord change = null;
			for (int i = 0; i < hits.Length; ++i)
			{
				if (hits[i] == true)
				{
					if (change != null && (change.EndBlock == i -1))
					{
						change.EndBlock = i;
					}
					else
					{
						change = new ChangeRecord();
						change.Matched = true;
						change.StartBlock = i;
						change.EndBlock = i;
						missing.Add(change);
					}
				}

			}

			reader.Close();
			return missing;
		}

		/// <summary>
		/// Writes the new file based on The ChangeRecord array.
		/// </summary>
		/// <param name="changes"></param>
		public void WriteChanges(ArrayList changes, string outFile)
		{
			inStream = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
			outStream = new BinaryWriter(File.Open(outFile, FileMode.CreateNew, FileAccess.Write, FileShare.None));
			foreach(ChangeRecord change in changes)
			{
				if (change.Matched)
				{
					byte[] buffer = new byte[BlockSize];
					inStream.BaseStream.Position = change.StartBlock * BlockSize;
					for (int i = change.StartBlock; i <= change.EndBlock; ++i)
					{
						int bytesRead = inStream.Read(buffer, 0, BlockSize);
						outStream.Write(buffer, 0, BlockSize);
					}
				}
				else
				{
					// Write the bytes to the output stream.
					outStream.Write(change.Data, 0, change.Length);
				}
			}
			inStream.Close();
			outStream.Close();
		}
	
		public ArrayList GetHashStamps()
		{
			BinaryReader	reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
			FileInfo		fi = new FileInfo(file);
			int				blockCount = (int)(fi.Length / BlockSize) + 1;
			ArrayList		list = new ArrayList(blockCount);
			byte[]			buffer = new byte[BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			// Compute the hash codes.
			while ((bytesRead = reader.Read(buffer, 0, BlockSize)) != 0)
			{
				HashEntry entry = new HashEntry();
				entry.WeakHash = wh.ComputeHash(buffer, 0, (UInt16)bytesRead);
				entry.StrongHash =  sh.ComputeHash(buffer, 0, bytesRead);
				entry.BlockNumber = currentBlock++;
				list.Add(entry);
			}

			reader.Close();

			return list;
		}
	}

	struct HashEntry
	{
		internal int	BlockNumber;
		internal UInt32	WeakHash;
		internal byte[]	StrongHash;

		public override bool Equals(object obj)
		{
			if (this.WeakHash == ((HashEntry)obj).WeakHash)
			{
				for (int i = 0; i < StrongHash.Length; ++i)
				{
					if (StrongHash[i] != ((HashEntry)obj).StrongHash[i])
						return false;
				}
				return true;
			}
			return false;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}
	}

	class WeakHash
	{
		UInt16 a;
		UInt16 b;

		internal UInt32 ComputeHash(byte[] buffer, int offset, UInt16 count)
		{
			a = 0;
			b = 0;
			UInt16 l = (UInt16)(count);
			
			for (int i = offset; i < offset + count; ++i)
			{
				a += buffer[i];
				b += (UInt16)(l-- * buffer[i]);
			}

			return a + (UInt32)(b * 0x10000);
		}

		internal UInt32 RollHash(int count, byte dropValue, byte addValue)
		{
			a = (UInt16)(a - dropValue + addValue);
			b = (UInt16)(b - ((count) * dropValue) + a);
			return a + (UInt32)(b * 0x10000);
		}
	}

	class StrongHash
	{
		MD5		md5 = new MD5CryptoServiceProvider();
			
		internal byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			return md5.ComputeHash(buffer, offset, count);
		}
	}

	class StrongWeakHashtable
	{
		Hashtable table = new Hashtable();

		public void Add(HashEntry entry)
		{
			lock (this)
			{
				ArrayList entryArray;
				entryArray = (ArrayList)table[entry.WeakHash];
				if (entryArray == null)
				{
					entryArray = new ArrayList();
					table.Add(entry.WeakHash, entryArray);
				}
				entryArray.Add(entry);
			}
		}

		public void Add(ArrayList entryList)
		{
			foreach (HashEntry entry in entryList)
			{
				Add(entry);
			}
		}

		public ArrayList this[UInt32 weakHash]
		{
			get
			{
				return (ArrayList)table[weakHash];
			}
		}
	}
}
