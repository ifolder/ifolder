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
 *  Author: Russ Young
 * 
 ***********************************************************************/
using System;
using System.Net;
using System.Web;
using System.Web.SessionState;
using Simias.Storage;

namespace Simias.Sync.Web
{
	public class SyncHandler : IHttpHandler, IRequiresSessionState
	{
		
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest Request = context.Request;
			HttpResponse Response = context.Response;
			// Set no cache-ing.
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			SyncService service = (SyncService)context.Session["SyncService"];
			if (service != null)
			{
				string httpMethod = Request.HttpMethod;
				SyncHttp.Operation op = (SyncHttp.Operation)Enum.Parse(typeof(SyncHttp.Operation), Request.Headers.Get(SyncHttp.SyncOperation), true);
				if (string.Compare(httpMethod, "Get", true) == 0)
				{
					// Determine What work we need to do.
					Response.ContentType = "application/octet-stream";
					switch (op)
					{
						case SyncHttp.Operation.Read:
						{
							long offset;
							long size;
							if (GetRange(Request, out offset, out size))
							{
								byte[] buffer;
								service.Read(out buffer, offset, (int)size);
								Response.BinaryWrite(buffer);
								Response.End();
								//Response.WriteFile((service.GetReadHandle(), offset, size);
							}
							else
							{
								Response.StatusCode = (int)HttpStatusCode.BadRequest;
							}
							break;
						}
						default:
							Response.StatusCode = (int)HttpStatusCode.BadRequest;
							break;
					}
				}
				else if (string.Compare(httpMethod, "Post", true) == 0)
				{
					// Determine What work we need to do.
					switch (op)
					{
						case SyncHttp.Operation.Write:
						{
							long offset, size;
							if (GetRange(Request, out offset, out size))
							{
								service.Write(Request.InputStream, offset, (int)size);
								Response.StatusCode = (int)HttpStatusCode.OK;
							}
							break;
						}
						case SyncHttp.Operation.Copy:
						{
							long oldOffset, offset, size;
							if (GetRange(Request, out oldOffset, out offset, out size))
							{
								service.Copy(oldOffset, offset, (int)size);
								Response.StatusCode = (int)HttpStatusCode.OK;
							}
							break;
						}
						case SyncHttp.Operation.Read:
						{
							string sBlockCount = Request.Headers.Get(SyncHttp.SyncBlocks);
							string sBlockSize = Request.Headers.Get(SyncHttp.BlockSize);
							if (sBlockCount != null && sBlockSize != null)
							{
								int blockCount = int.Parse(sBlockCount);
								int blockSize = int.Parse(sBlockSize);
								long[] fileMap = new long[blockCount];
								byte[] input = new byte[Request.ContentLength];
								Request.InputStream.Read(input, 0, Request.ContentLength);
								int readOffset = 0;
								for (int i = 0; i < blockCount; ++i)
								{
									fileMap[i] = BitConverter.ToInt64(input,readOffset);
									readOffset += 8;
								}

								// Now send the data back;
								byte[] outBuffer = new byte[blockSize];
								long offset = 0;
								for (int i = 0; i < blockCount; ++i)
								{
									if (fileMap[i] == -1)
									{
										int bytesRead = service.Read(out outBuffer, offset, blockSize);
										if (bytesRead != blockSize && bytesRead != 0)
										{
											byte[] tempArray = new byte[bytesRead];
											Array.Copy(outBuffer, tempArray, bytesRead);
											Response.BinaryWrite(tempArray);
										}
										else if (bytesRead != 0)
										{
											Response.BinaryWrite(outBuffer);
										}
									}
									offset += blockSize;
								}
							}
							else
							{
								Response.StatusCode = (int)HttpStatusCode.BadRequest;
							}
							break;
						}
						default:
							Response.StatusCode = (int)HttpStatusCode.BadRequest;
							break;
					}
				}
				else
				{
					Response.StatusCode = (int)HttpStatusCode.BadRequest;
				}
			}
			else
			{
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
			//Response.WriteFile("Web.Config");
		}

		public bool IsReusable
		{
			// To enable pooling, return true here.
			// This keeps the handler in memory.
			get { return true; }
		}

		/// <summary>
		/// Return the file offset and size to read or write.
		/// </summary>
		/// <param name="Request">The request.</param>
		/// <param name="offset">The file offset.</param>
		/// <param name="size">The size.</param>
		/// <returns>True if range found.</returns>
		private bool GetRange(HttpRequest Request, out long offset, out long size)
		{
			string range = Request.Headers.Get(SyncHttp.SyncRange);
			if (range != null)
			{
				string[] values = range.Split('-');
				if (values.Length == 2)
				{
					offset = long.Parse(values[0]);
					size = long.Parse(values[1]) - offset;
					return true;
				}
			}
			
			offset = size = 0;
			return false;
		}

		/// <summary>
		/// Return the file offset and size to copy.
		/// </summary>
		/// <param name="Request">The request.</param>
		/// <param name="oldOffset">The original file offset.</param>
		/// <param name="offset">The file offset.</param>
		/// <param name="size">The size.</param>
		/// <returns>True if range found.</returns>
		private bool GetRange(HttpRequest Request, out long oldOffset, out long offset, out long size)
		{
			if (GetRange(Request, out offset, out size))
			{
				string cOffset = Request.Headers.Get(SyncHttp.CopyOffset);
				if (cOffset != null)
				{
					oldOffset = long.Parse(cOffset);
					return true;
				}
			}
			oldOffset = size = 0;
			return false;
		}
	}
}
