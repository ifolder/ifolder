/****************************************************************************
File:    FLAIM.H
Title:   Applications Include File
Owner:   FLAIM Team
Tabs:    4,3

	Copyright © 1991-2005 Novell, Inc. All Rights Reserved.
	Use and redistribution of this work is subject to the developer license
	agreement through which this work is made available.
	THIS WORK MAY NOT BE REVISED OR MODIFIED WITHOUT THE PRIOR WRITTEN CONSENT
	OF NOVELL, INC.  THE WORK IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
	EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL NOVELL OR THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
	ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE WORK OR
	THE USE OR OTHER DEALINGS IN THE WORK.
-----------------------------------------------------------------------------
Desc: This include file contains the structure definitions and prototypes
		needed by an application to interface with FLAIM.

 * $Log$
 * Revision 1.2  2005/05/02 23:42:56  ryoung65
 * Fixed the member list that would crash after waiting for awhile and then trying to read more members.
 * The flaim handle was being freed because a thread went away.
 * https://bugzilla.novell.com/show_bug.cgi?id=81217
 *
 * Revision 1.1  2004/12/02 21:21:10  cgaisford
 * added flaim for macosx
 *
 * Revision 4.131.4.43.2.8  2004/11/18 00:13:04  rmiller
 * Minor changes so we could build on MacOS
 *
 * Revision 4.131.4.43.2.7  2004/09/21 16:24:43  dius
 * DEFECT 000384423 - Restoreing incremental files fixes.
 *
 * Revision 4.131.4.43.2.6  2004/09/10 16:58:27  dius
 * DEFECT000382731 - additionally a fix to separate the picket fence and check record
 * processing from the standard FLM_DEBUG code.  Added FLM_CHECK_RECORD
 * and FLM_PICKET_FENCE to include these debug capabilities.  Tested in both debug and
 * release code.
 *
 * Revision 4.131.4.43.2.5  2004/08/19 20:40:36  dius
 * Changes to fix defect 380253.
 *
 * Revision 4.131.4.43.2.4  2004/07/14 20:00:24  rmiller
 * Set FLM_ERROR_BASE to 0
 *
 * Revision 4.131.4.43.2.3  2004/07/13 17:07:28  dius
 * Added FERR_PASSWD_INVALID error code.
 *
 * Revision 4.131.4.43.2.2  2004/07/12 22:17:33  dius
 * Added password parameter to FlmDbOpen - task 1613
 *
 * Revision 4.131.4.43.2.1  2004/07/09 22:10:14  dius
 * Separated the code to generate a key in an upgrade and added
 * bHaveEncKey to the pFile so we can override the bAllowLimited
 * flag if needed.
 *
 * Revision 4.131.4.43  2004/05/13 22:03:08  rmiller
 * Added the size_t typedef for HPUX only
 *
 * Revision 4.131.4.42  2004/05/06 16:09:11  rmiller
 * More changes to operator new.  This time for AIX.
 *
 * Revision 4.131.4.41  2004/05/04 23:20:18  rmiller
 * Used FLM_UNIX in a few places where I should have used FLM_HPUX
 *
 * Revision 4.131.4.40  2004/05/04 23:17:45  rmiller
 * Various changes to get flaim building on hp-ux.
 *
 * Revision 4.131.4.39  2004/05/03 15:37:33  dius
 * Updates to support the backup/restore with password
 *
 * Revision 4.131.4.38  2004/04/27 20:26:55  dius
 * Removed unused error codes.
 *
 * Revision 4.131.4.37  2004/04/26 22:26:30  dius
 * Bug fix - set the encLength and encId in getFieldInfo.
 *
 * Revision 4.131.4.36  2004/04/20 17:34:42  dius
 * Made F_Rfl a friend to FlmRecord.  Removed FlmRecord methods
 * that were no longer needed as a result.
 *
 * Revision 4.131.4.35  2004/04/14 22:30:48  dius
 * inlined a few functions as per Code review.  Added methods locateFieldByPosition
 * and locateLastField to the FlmRecord class.
 *
 * Revision 4.131.4.34  2004/04/14 19:04:43  dius
 * Added encryption flags to NODE and fixed the GedPutXXX
 * functions as per code review.  Import based on the encryption flags.
 *
 * Revision 4.131.4.33  2004/04/14 16:39:04  dius
 * Code review changes
 *
 * Revision 4.131.4.32  2004/04/13 16:52:07  dsanders
 * Fixed up some tabs.
 *
 * Revision 4.131.4.31  2004/04/12 17:40:10  dius
 * Added encrypted value to the NODE structure.  Also added
 * the method insertAt to the FlmRecord class.
 *
 * Revision 4.131.4.30  2004/04/09 19:48:18  dius
 * Added private methods in FlmRecords that reduced the
 * number of times we need to call getFieldPionter from
 * within a FlmRecord.
 *
 * Revision 4.131.4.29  2004/04/07 23:35:21  dius
 * Optimized a few parameters being passed in getFieldInfo
 *
 * Revision 4.131.4.28  2004/04/02 20:56:43  rmiller
 * Fixed a warning about a redefinition of function tokenIsNum
 *
 * Revision 4.131.4.27  2004/04/01 16:38:16  dius
 * Merged getEncFieldInfo and getFieldInfo.  Added two parameters
 * to getFieldInfo.
 *
 * Revision 4.131.4.26  2004/03/23 21:34:57  dius
 * Added method setEncHeader to FlmRecord.
 *
 * Revision 4.131.4.25  2004/03/18 16:30:40  dius
 * New private versions of getEncFlags, setEncFlags and getEncryptionID
 * that use the FlmField pointer.
 *
 * Revision 4.131.4.24  2004/03/17 17:07:44  dius
 * Added uiEncId to all FlmRecord::setXXX and FlmRecord::getXXX methods.
 *
 * Revision 4.131.4.23  2004/03/13 01:05:00  ahodgkinson
 * Support for background index deletion.  Also performed misc. cleanup.
 *
 * Revision 4.131.4.22  2004/03/12 21:32:13  dius
 * Added a few new error codes.  Added optional parameter
 * to all FlmRecord field setters.
 *
 * Revision 4.131.4.21  2004/03/09 23:50:58  dius
 * Rename getImportDataPtr to allocStorageSpace. Removed getNewEncDataPtr.
 *
 * Revision 4.131.4.20  2004/03/09 20:33:30  rmiller
 * New error code: FERR_ENCRYPTION_UNAVAILABLE
 * Also removed an extra getDataPtr from FlmRecord
 *
 * Revision 4.131.4.19  2004/03/08 16:50:31  dsanders
 * Defect 346120.  Added support during a structural rebuild for setting
 * SMI USTRING index key limits on indexes that do not already have limits.
 *
 * Revision 4.131.4.18  2004/03/05 20:17:24  dius
 * Removed exportComplete().
 *
 * Revision 4.131.4.17  2004/03/05 19:51:38  dius
 * Changed getExportDataPtr to getDataPtr.
 *
 * Revision 4.131.4.16  2004/03/05 19:43:39  dius
 * Removed importComplete
 *
 * Revision 4.131.4.15  2004/03/05 18:58:18  dius
 * Renamed getImportDataPtr to allocStorageSpace.
 *
 * Revision 4.131.4.14  2004/03/04 19:30:48  dius
 * New getNewEncDataPtr function. Changed FLM_ENC_FLD_OVERHEAD
 *  to 8 instead of 10. The encrypted data length is now two bytes.
 *
 * Revision 4.131.4.13  2004/03/02 19:44:42  dius
 * New calling format for getImportDataPtr.  Moved NICI codes to
 * new location.
 *
 * Revision 4.131.4.12  2004/03/01 22:54:33  dius
 * no message
 *
 * Revision 4.131.4.11  2004/02/27 19:55:07  dius
 * Added some methods for handling encrypted fields to the FlmRecord,
 *  fixed up some comments and added a few fields to the EGDCOM NODE
 * structure.
 *
 * Revision 4.131.4.10  2004/02/25 23:26:41  ahodgkinson
 * Merged changes made to the FLAIM 4 branch.
 *
 * Revision 4.131.4.9  2004/02/25 22:46:53  rmiller
 * Backup and restore functions now accept a password and will shroud and
 * unshroud the database key.
 *
 * Revision 4.131.4.8  2004/02/25 18:15:24  dius
 * Put some of the encryption methods of the FlmRecord into
 * an ifdef USE_NICI clause.
 *
 * Revision 4.131.4.7  2004/02/25 16:42:29  dius
 * Added isEncryptedField, getEncryptedDataLength, getEncryptionID
 *  and encryptField to FlmRecord class.
 *
 * Revision 4.131.4.6  2004/02/24 00:11:48  dius
 * New NICI error Code.
 *
 * Revision 4.131.4.5  2004/02/20 18:45:13  rmiller
 * Removed all the password stuff from the database open code since it's
 * no longer necessary.
 *
 * Revision 4.131.4.4  2004/02/18 20:57:37  dius
 * New error code.
 *
 * Revision 4.131.4.3  2004/02/18 16:05:17  dius
 * Added code for parsing EncDef records
 *
 * Revision 4.131.4.2  2004/02/10 22:02:30  rmiller
 * Encryption stuff related to FlmDbOpen, FlmDbRewrapKeys and FlmGetWrapMethod
 *
 * Revision 4.131.4.1  2004/02/02 22:31:48  rmiller
 * Added a few variables needed for encryption support to FDB and FSYSDATA
 *
 * Revision 4.131  2004/01/26 18:18:25  ahodgkinson
 * Changes to support a FLAIM memory allocator designed to reduce
 * memory fragmentation.
 *
 * Revision 4.130  2003/12/06 00:26:15  ahodgkinson
 * Created new FlmGetConfig options to determine if dynamic cache
 * adjustment is supported.
 *
 * Revision 4.129  2003/12/04 18:42:49  dsanders
 * Moved FlmDbClose prototype into flaim.h
 *
 * Revision 4.128  2003/12/04 18:29:37  dsanders
 * Moved unicode functions and f_sprintf to flaim.h
 *
 * Revision 4.127  2003/12/02 23:28:01  ahodgkinson
 * Changes to support a user-provided memory allocator.  Also restructured
 * the code so that flaim.h is the only header file that consumers of FLAIM
 * need in order to use FLAIM.
 *
 * Revision 4.126  2003/09/12 21:19:25  ahodgkinson
 * Added support for comment fields.  Got rid of unused routines.
 *
 * Revision 4.125  2003/09/08 23:16:42  ahodgkinson
 * Changes to get rid of NetWare SDK dependencies.
 *
 * Revision 4.124  2003/09/03 19:07:23  ahodgkinson
 * Fixed Watcom compiler warnings.
 *
 * Revision 4.123  2003/09/02 22:28:11  ahodgkinson
 * Changes to support f_new.
 *
 * Revision 4.122  2003/08/22 19:34:10  ahodgkinson
 * Eliminated all NetWare CLIB dependencies.
 *
 * Revision 4.121  2003/06/23 18:50:42  dsanders
 * Defect 340306.  Modified prototype for FlmDbTransCommit to return bEmpty flag.
 * (Same as revision 4.118.2.2 for falconsp2)
 *
 * Revision 4.120  2003/06/20 15:12:58  rmiller
 * Changes from India to enable dynamic cache adjustment on HP-UX
 * (Changes have been approved by Daniel)
 *
 * Revision 4.119  2003/06/04 21:25:58  ahodgkinson
 * Fixed RCS log messages.
 *
 *    Rev 4.118   30 Jan 2003 15:06:40   rmiller
 * Defect 100301891: Allow Flaim to reuse DRN's in the dictionary container
 * 
 *    Rev 4.117   06 Jan 2003 13:16:58   andy
 * Defect 323887.  Optimized SCACHE so that new blocks can be written quickly.
 * 
 *    Rev 4.116   12 Dec 2002 13:46:34   andy
 * Defect 322651.  Needed to gather stats on the number of dirty blocks and
 * log blocks.
 * 
 *    Rev 4.115   04 Dec 2002 16:50:58   andy
 * Defect 321768.  Cache optimizations -- we now maintain a list of free
 * cache blocks that can be quickly used and we also allow borrowing of
 * cache memory between block and record cache.
 * 
 *    Rev 4.114   21 Nov 2002 09:47:02   dss
 * Defect 320562.  Changes to allow 64 bits for statistics data - monitoring.
 * 
 *    Rev 4.113   13 Nov 2002 13:05:52   andy
 * Defect 500281151.  Changes to improve background indexing performance.
 * 
 *    Rev 4.112   01 Nov 2002 14:37:38   dss
 * Defect 315174.  Modifications to allow group committing of transactions to
 * the roll-forward log.
 * 
 *    Rev 4.111   23 Aug 2002 17:27:12   rmiller
 * Defect 311290  Back out support for dynamic cache limits on AIX.  (The
 * system call we were using to get the memory info isn't available on AIX
 * 4.3, so we're back to using static cache limits for all versions of AIX.)
 * 
 *    Rev 4.110   21 Aug 2002 15:32:28   rmiller
 * Changes to allow us to use dynamic cache limits on AIX
 * 
 *    Rev 4.109   19 Jul 2002 10:25:04   rmiller
 * Defect 307381: Changes so that http stuff will work on Unix platforms
 * 
 *    Rev 4.108   30 May 2002 14:45:00   dss
 * Defect #302209.  Added FLM_MAX_DIRTY_CACHE option for FlmConfig.
 * 
 *    Rev 4.107   13 May 2002 10:38:30   andy
 * Defect 500277125.  Added FDB_GET_MUST_CLOSE_RC.
 * 
 *    Rev 4.106   22 Apr 2002 11:34:22   andy
 * Defect 299319.  Added FDB_GET_FFILE_ID option to FlmDbGetConfig.
 * 
 *    Rev 4.105   29 Mar 2002 11:39:34   andy
 * Defect 296790.  Added uiWaitTruncateTime to the CHECKPOINT_INFO struct.
 * 
 *    Rev 4.104   29 Mar 2002 11:11:08   dius
 * Added a new type to the enum eFlmFuncs called FLM_DB_LOGHDR to
 * be used in the database operations web page.
 * 
 *    Rev 4.103   28 Mar 2002 11:11:56   andy
 * Added FLM_DB_REMOVE to the FlmFuncs list.
 * 
 *    Rev 4.102   27 Mar 2002 12:54:32   andy
 * Added FlmDbGetLockType API.  Did some misc. cleanup.
 * 
 *    Rev 4.101   15 Mar 2002 09:52:48   dss
 * Added prototype for FlmParseQuery.
 * 
 *    Rev 4.100   08 Mar 2002 17:01:08   andy
 * Changes to use new name table object.
 * 
 *    Rev 4.99   08 Mar 2002 11:33:12   dss
 * Added in some tag names for area definitions that were still in use.
 * 
 *    Rev 4.98   08 Mar 2002 10:59:48   dss
 * Got rid of FNTABLE structure.  Created F_NameTable class.
 * 
 *    Rev 4.97   01 Mar 2002 15:38:20   dius
 * Added prototyp for flmGetCPInfo.
 * 
 *    Rev 4.96   28 Feb 2002 16:56:06   dss
 * Defect #500274049.  Added FLM_RESOLVE_UNK option for FlmCursorAddOp.  Added
 * uiFlags parameter to FlmCursorAddOp.  Got rid of some unused flags.
 * Added FlmCursorReleaseResources API.
 * 
 *    Rev 4.95   11 Feb 2002 11:12:54   dss
 * Added FLM_QUERY_MAX.
 * 
 *    Rev 4.94   30 Jan 2002 10:06:18   andy
 * Defect 290770.  Support for "must close" flag on the FDB.
 * 
 *    Rev 4.93   25 Jan 2002 12:52:38   andy
 * Defect 290531.  Added FlmDbGetConfig option for retrieving the next
 * incremental backup sequence number.  Also added abortFile() method
 * to F_Restore.
 * 
 *    Rev 4.92   24 Jan 2002 17:13:34   dss
 * Defect #500269961.  Changed DEFAULT_CACHE_ADJUST_MAX to 0xE0000000.
 * 
 *    Rev 4.91   16 Jan 2002 17:05:50   dss
 * Added includes of fpackon.hpp and fpackoff.hpp
 * 
 *    Rev 4.90   16 Jan 2002 14:54:02   andy
 * Added FLM_UNKNOWN_FUNC to the eFlmFuncs table.
 * 
 *    Rev 4.89   15 Jan 2002 18:16:50   rmiller
 * Added FLM_IMPORT_HTTP_SYMS, FLM_UMIMPORT_HTTP_SYMS, FLM_REGISTER_HTTP_URL,
 * and FLM_DEREGISTER_HTTP_URL to the eFlmConfigTypes enum.
 * 
 *    Rev 4.88   15 Jan 2002 13:21:38   andy
 * Added C/S support for FlmGetThreadInfo.
 * 
 *    Rev 4.87   11 Jan 2002 10:54:50   rmiller
 * Overhauling the HTTP subsystem
 * 
 *    Rev 4.86   07 Jan 2002 10:57:22   andy
 * Defect 287732.  Support for persistent index suspend.
 * 
 *    Rev 4.85   18 Dec 2001 14:07:04   rmiller
 * Added FLM_HTTPD to eFlmConfigTypes
 * 
 *    Rev 4.84   12 Dec 2001 13:29:10   dss
 * Got rid of #if 0'd areas of code.  Added FDB_SET_APP_DATA option for
 * FlmDbConfig.  Added FDB_GET_APP_DATA option for FlmDbGetConfig.  Got rid
 * of error handler callback typedef and functions for setting them.
 * 
 *    Rev 4.83   12 Dec 2001 10:03:08   andy
 * Changes to use new F_TheadMgr object.  Added FlmGetThreadInfo API.
 * 
 *    Rev 4.82   21 Nov 2001 16:06:28   dss
 * Modified prototypes to allow passing of a data directory - so data files
 * can be put on a volume separate from the rollback files.
 * 
 *    Rev 4.81   19 Nov 2001 10:51:40   andy
 * Added support for ECache and prefetch on database open.
 * 
 *    Rev 4.80   11 Oct 2001 12:41:44   dss
 * Support for cross-container indexes.  Also added support for indexing
 * callback that can modify the record being indexed when an index is
 * being added.
 * 
 *    Rev 4.79   24 Aug 2001 08:36:12   dss
 * Added FLM_GENERAL_MESSAGE.
 * 
 *    Rev 4.78   23 Aug 2001 17:04:36   dss
 * Modified release method of F_Logger class to unlock the mutex before
 * deleting the object.
 * 
 *    Rev 4.77   21 Aug 2001 14:29:32   andy
 * Needed to declare the destructors in F_Logger and F_LogMessage to be
 * virtual.
 * 
 *    Rev 4.76   20 Aug 2001 14:22:02   andy
 * Added more methods to F_LogMessage.
 * 
 *    Rev 4.75   20 Aug 2001 09:07:32   andy
 * Added FLM_TRANSACTION_MESSAGE to the FlmLogMessageType enum.
 * 
 *    Rev 4.74   17 Aug 2001 11:19:36   andy
 * Added initial support for FLAIM message logging.
 * 
 *    Rev 4.73   09 Aug 2001 14:54:06   andy
 * Changed the status method of the F_Restore class to include a uiTransId
 * parameter.
 * 
 *    Rev 4.72   07 Aug 2001 10:56:02   andy
 * Added FO_DONT_RESUME_INDEXING flag.
 * 
 *    Rev 4.71   17 Jul 2001 13:49:16   andy
 * Defect 273195.  Added support for Win64.
 * 
 *    Rev 4.70   21 Jun 2001 09:01:58   brutt
 * FDB_GET_LOCK_WAITERS memory should be delete[]ed not just deleted.
 * 
 *    Rev 4.69   15 Jun 2001 13:54:12   andy
 * Defect 270666.  Added support for retrying failed operations during a
 * database restore.
 * 
 *    Rev 4.68   15 Jun 2001 10:30:26   andy
 * Defect 270610.  Improved FCURSOR_GET_FLM_IX to return information about
 * queries that use multiple indexes.
 * 
 *    Rev 4.67   29 May 2001 11:40:20   dss
 * Defect #268758.  Added defines for configuring file extend size.
 * 
 *    Rev 4.66   23 May 2001 15:30:10   dss
 * Defect #268437.  Renamed FlmDbConvert to FlmDbUpgrade.  Also changed
 * FLM_DB_CONVERT to FLM_DB_UPGRADE.
 * 
 *    Rev 4.65   23 May 2001 14:26:28   andy
 * Defect 268437.  Changes to support logging upgrade operations in the RFL.
 * 
 *    Rev 4.64   18 May 2001 09:16:44   dss
 * Defect #268007.  Added the RESTORE_REDUCE define.
 * 
 *    Rev 4.63   20 Apr 2001 14:21:46   cbenson
 * fixed outdated comment
 * 
 *    Rev 4.62   13 Apr 2001 10:13:10   andy
 * Defect 265487.  Added support for lock priorities.
 * 
 *    Rev 4.61   10 Apr 2001 16:34:00   dss
 * Defect #265156.  Added bKeepRflFiles and bLogAbortedTransToRfl flags to
 * CREATE_OPTS structure.  Modified FlmDbOpen to NOT return create options.
 * Got rid of option for FlmDbGetConfig to get create options.
 * 
 *    Rev 4.60   03 Apr 2001 15:00:34   dss
 * Modified so that DEFAULT_RFL_MAX_FILE_SIZE is now F_MAXIMUM_FILE_SIZE.
 * 
 *    Rev 4.59   03 Apr 2001 14:05:40   andy
 * Added FlmBackupGetConfig prototype.
 * 
 *    Rev 4.58   02 Apr 2001 09:47:06   andy
 * Changed the default block size to 4096.  Also added szDestFileName to
 * the DB_COPY_INFO structure.
 * 
 *    Rev 4.57   30 Mar 2001 13:05:18   dss
 * Modified FlmDbGetConfig to have three value parameters instead of just one.
 * 
 *    Rev 4.56   30 Mar 2001 11:02:50   andy
 * Changed FlmCursorPrev to pass FLM_CURSOR_PREV function ID to
 * flmCurPerformRead rather than FLM_CURSOR_NEXT.
 * 
 *    Rev 4.55   27 Mar 2001 11:04:32   andy
 * Changes to support 8 terabyte databases.
 * 
 *    Rev 4.54   20 Mar 2001 13:35:46   andy
 * Added FlmDbGetRflFileName.
 * 
 *    Rev 4.53   19 Mar 2001 13:52:38   dss
 * Added callback and bOverwriteDestOk flag to FlmDbRename.
 * 
 *    Rev 4.52   19 Mar 2001 12:25:48   dss
 * Added prototype for FlmDbRename.
 * 
 *    Rev 4.51   16 Mar 2001 16:34:58   andy
 * Removed uiDbVersion parameter from openRflFile -- the F_FSRestore object
 * will determine the version of the database by reading the file header.
 * Changed close method of F_Restore to return an RCODE.
 * 
 *    Rev 4.50   16 Mar 2001 08:45:16   dss
 * Added prototype for FlmDbGetUnknownStreamObj.
 * 
 *    Rev 4.49   15 Mar 2001 17:32:12   andy
 * Removed the status callback parameter from FlmDbRestore -- all status
 * will be provided via the status method of the restore object.  Added
 * uiDbVersion parameter to openRflFile method of F_Restore -- this is needed
 * by the F_FSRestore implementation for determining the path of the RFL
 * files.
 * 
 *    Rev 4.48   15 Mar 2001 09:34:58   andy
 * Added puiIncSeqNum parameter to FlmDbBackup.
 * 
 *    Rev 4.47   12 Mar 2001 16:36:16   dss
 * Added option for FlmDbConfig to automatically turn off keeping of RFL files.
 * Also added options for FLmDbGetConfig to query what the flag is currently
 * set to.  Also modified to allow checkpoint to be forced for the reason that
 * the RFL volume is bad.
 * 
 *    Rev 4.46   05 Mar 2001 16:38:16   dss
 * Got rid of FlmDbDelete - didn't know we had FlmDbRemove.  Moved FlmDbRemove
 * to fdbremov.cpp.
 * 
 *    Rev 4.45   05 Mar 2001 13:06:24   dss
 * Added prototype for FlmDbDelete.
 * 
 *    Rev 4.44   05 Mar 2001 09:36:52   dss
 * Added FDB_KEEP_ABORTED_TRANS_IN_RFL option for FlmDbConfig.
 * 
 *    Rev 4.43   02 Mar 2001 16:58:30   andy
 * Removed FO_DOING_RESTORE flag.
 * 
 *    Rev 4.42   27 Feb 2001 14:49:38   dss
 * Defect #260805. (Same as 260812 for ver41).  Modified to have FLM_CLOSE_FILE
 * option for FlmConfig.
 * (Same as revision 4.36 in ver42 code base).
 * 
 *    Rev 4.41   22 Feb 2001 09:44:08   dss
 * Changed RECOVER_ to RESTORE_.  Also added F_UnknownStream abstract base
 * class.
 * 
 *    Rev 4.40   21 Feb 2001 15:38:16   andy
 * Changes to use the F_Restore object.
 * 
 *    Rev 4.39   16 Feb 2001 14:39:24   andy
 * Added F_Recovery virtual base class.
 * 
 *    Rev 4.38   16 Feb 2001 10:41:34   dss
 * Defect #259907.  Added FLM_DELETING_STATUS for status hook callback.
 * (Merged in revision 4.35 from ver42 code base).
 * 
 *    Rev 4.37   16 Feb 2001 10:28:56   andy
 * More changes for hot, continuous backup.  Added FlmDbBackupBegin and
 * FlmDbBackupEnd.
 * 
 *    Rev 4.36   13 Feb 2001 14:14:30   dss
 * Defect 259641.  Added FDB_SET_APP_VERSION.
 * (Merged in revision 4.34 from ver42 code base).  Also added some more
 * defines for FlmDbConfig pertaining to hot continuous backup.
 * 
 *    Rev 4.35   06 Feb 2001 12:50:12   andy
 * Added FLM_VER_4_3.
 * 
 *    Rev 4.34   02 Feb 2001 16:57:54   andy
 * First round of changes for HCB (Hot, Continuous Backup).
 * 
 *    Rev 4.33   03 Jan 2001 15:47:40   andy
 * Added FDB_DISABLE_SANITY_CHECKING and F_TRANS_HEADER_SIZE defines.
 * 
 *    Rev 4.32   22 Dec 2000 11:11:24   dss
 * Cache checking and protection:  Added configuration options for configuring
 * these things on and off.
 * 
 *    Rev 4.31   18 Dec 2000 13:17:16   andy
 * Added FLM_DONT_KILL_TRANS flag to be passed into FlmDbTransBegin.  This
 * flag is used to indicate that the checkpoint thread should not attempt
 * to kill a read transaction.  This is used by FlmDbBackup.
 * 
 *    Rev 4.30   11 Dec 2000 16:24:06   andy
 * Changes for AIX.
 * 
 *    Rev 4.29   01 Nov 2000 11:02:16   dss
 * Added FCURSOR_SET_ABS_POS, FCURSOR_GET_ABS_POS, FCURSOR_GET_ABS_COUNT, and
 * FCURSOR_GET_ABS_POSITIONABLE.
 * 
 *    Rev 4.28   01 Nov 2000 09:27:40   swp
 * New positioning tag for the index definition.
 * 
 *    Rev 4.27   13 Oct 2000 08:59:28   andy
 * Added FlmCursorSetKeyRange.
 * 
 *    Rev 4.26   29 Sep 2000 09:26:10   dss
 * Got rid of FLM_NOT_EXISTS, FLM_NOT_CONTAINS, FLM_NOT_MATCH_BEGIN,
 * FLM_NOT_MATCH, and FLM_NOT_MATCH_END operators.
 * 
 *    Rev 4.25   28 Sep 2000 16:53:24   dss
 * Added FLM_NOT_MATCH_OP, FLM_NOT_MATCH_BEGIN_OP, and FLM_NOT_MATCH_END_OP.
 * These are strictly for internal use only.
 * 
 *    Rev 4.24   27 Sep 2000 16:16:20   dss
 * Added FCURSOR_SAVE_POSITION and FCURSOR_RESTORE_POSITION.
 * 
 *    Rev 4.23   27 Sep 2000 12:55:10   dss
 * Added uiDrnCost to OPT_INFO structure.
 * 
 *    Rev 4.22   25 Sep 2000 14:29:04   andy
 * Added FlmDbBackup, FlmDbRestore, and FlmDbRemove.  Added optional
 * parameter to FlmDbTransBegin that allows the first 2K of the
 * file header to be returned to the application.
 * 
 *    Rev 4.21   22 Sep 2000 15:12:46   dss
 * Got rid of FLM_FLD_ID and FLM_REC_FLD.
 * 
 *    Rev 4.20   22 Sep 2000 11:31:02   swp
 * Changed prototype for flmCurPerformRead
 * 
 *    Rev 4.19   21 Sep 2000 14:01:38   dss
 * Changed various things for cursors.  Modified to use leftTruncated and
 * rightTruncated instead of substring stuff.
 * 
 *    Rev 4.18   25 Aug 2000 11:37:10   swp
 * Removed FlmGetItemId() API.  Still need to remove old area tags.
 * 
 *    Rev 4.17   10 Aug 2000 10:54:30   swp
 * Defect 244199 - changed doKeyMatch to container 0, FLM_TRUE or FLM_FALSE.
 * 
 *    Rev 4.16   31 Jul 2000 10:40:06   dss
 * Defect #243334.  Added m_bInDbList field to FlmBlob class.
 * 
 *    Rev 4.15   25 Jul 2000 12:39:18   brutt
 * DEFECT 242787.  Changed the way out-of-memory simulation is done.
 * Added gv_FlmSysData.uiOutOfMemSimEnabledFlag, which if turned on
 * through FlmConfig, will simulate out of memory.  This will allow us to
 * turn it on or off at runtime.
 * 
 *    Rev 4.14   20 Jul 2000 13:13:36   dss
 * Defect #242280.  Added FLM_FLUSH_IO option for FlmConfig - also added
 * DEFAULT_FLUSH_IO define.
 * 
 *    Rev 4.13   30 Jun 2000 14:30:50   blj
 * changed values of FLM_UNICODE_VAL and FLM_TEXT_VAL (moved up to fill in
 * gap in qTypes enum).
 * 
 *    Rev 4.12   28 Jun 2000 13:49:20   dss
 * Got rid of specificity scores.
 * 
 *    Rev 4.11   28 Jun 2000 13:03:12   dss
 * Added a comment about FLM_SANITYLEVEL.
 * 
 *    Rev 4.10   28 Jun 2000 11:37:16   dss
 * Cleaned up some comments.  Changed FLM_DFS to FLM_DIRECT_IO.
 * 
 *    Rev 4.9   28 Jun 2000 10:59:30   dss
 * Got rid of ability to set and get server lock manager flag - we always
 * use the server lock manager.
 * 
 *    Rev 4.8   26 Jun 2000 16:11:08   blj
 * removed granularity tag - which was a QF option.
 * 
 *    Rev 4.7   21 Jun 2000 14:42:58   andy
 * Added DEFAULT_BLOCK_CACHE_PERCENTAGE define.
 * 
 *    Rev 4.6   21 Jun 2000 09:23:10   andy
 * Removed FLM_STAT conditional.
 * 
 *    Rev 4.5   19 Jun 2000 13:59:50   dss
 * Modified FlmShutdown to be a void function.
 * 
 *    Rev 4.4   19 Jun 2000 11:08:50   dss
 * Got rid of password parameter.
 * 
 *    Rev 4.3   16 Jun 2000 17:40:08   blj
 * removed hShare.
 * 
 *    Rev 4.2   16 Jun 2000 17:28:18   swp
 * Changed the FlmBlob class to only support referenced blobs.
 * 
 *    Rev 4.1   16 Jun 2000 16:54:06   blj
 * Version 4.1
 * 
 *    Rev 3.175   16 Jun 2000 16:11:24   andy
 * Added FSHARE_BLOCK_CACHE_PERCENTAGE.
 * 
 *    Rev 3.174   16 Jun 2000 09:39:28   blj
 * removed GetMaintSeqNum & CursorAddIxSelectCB functions and removed unused
 * cursor config options.
 * 
 *    Rev 3.173   14 Jun 2000 12:41:50   blj
 * removed unused db config option (get dict id).
 * 
 *    Rev 3.172   13 Jun 2000 13:56:56   andy
 * Defect 238499.  Changes for UNIX to get physical/avail memory.
 * 
 *    Rev 3.171   13 Jun 2000 11:24:00   blj
 * removed FlmDataPrep api.
 * 
 *    Rev 3.170   09 Jun 2000 15:30:26   blj
 * removed wp prefix structure and the no encrypt element within the create opt
 * 
 *    Rev 3.169   09 Jun 2000 10:43:08   dss
 * Got rid of candidate set IDs in cursor APIs.  Also got rid of some other
 * unused FlmCursorXXX apis as well as support for external fields.
 * 
 *    Rev 3.168   09 Jun 2000 09:16:20   blj
 * removed old unused defines/prototypes/comments.
 * 
 *    Rev 3.167   02 Jun 2000 09:12:42   swp
 * Removal of old wp defines.
 * 
 *    Rev 3.166   01 Jun 2000 10:01:46   dss
 * Modified to ifdef on UNIX.
 * 
 *    Rev 3.165   01 Jun 2000 09:32:32   blj
 * removed unused API FlmPathParse.
 * 
 *    Rev 3.164   31 May 2000 11:47:36   blj
 * removed yield hook functions.
 * 
 *    Rev 3.163   30 May 2000 16:12:06   swp
 * Removed pascal and ngw_value definitions.
 * 
 *    Rev 3.162   26 May 2000 17:24:52   swp
 * Removed IO_PATH
 * 
 *    Rev 3.161   24 May 2000 16:29:42   swp
 * Remove FLM_MIN and max.
 * 
 *    Rev 3.160   24 May 2000 13:42:22   swp
 * Removed VOID_p references.
 * 
 *    Rev 3.159   24 May 2000 12:32:16   swp
 * Moved asserts to ftk.hpp.
 * 
 *    Rev 3.158   17 May 2000 14:38:22   andy
 * Defect 232584.  Removed FlmGetKeys (unused).
 * 
 *    Rev 3.157   01 May 2000 09:57:48   blj
 * defect 232888 - added new index complete event.
 * 
 *    Rev 3.156   26 Apr 2000 07:22:20   dss
 * Defect #231810.  Added FLM_SET_RESULT and FLM_GET_RESULT macros.
 * 
 *    Rev 3.155   17 Apr 2000 12:04:16   swp
 * Defect 229195 - backed out change for dropping case in key retrieve.
 * 
 *    Rev 3.154   14 Apr 2000 15:51:52   dss
 * Defect #232010.  Added some FCURSOR_xxx options, removed those no longer in
 * use.  Removed bFetchRecs and bDoView from OPT_INFO structure.
 * 
 *    Rev 3.153   13 Apr 2000 13:07:52   dss
 * Defect #84650.  Needed to be able to set the record validator and status
 * hook callbacks into the cursor, not just the DB.
 * 
 *    Rev 3.152   13 Apr 2000 12:40:06   swp
 * Defect 229195 - add flag to strip out case bits.
 * 
 *    Rev 3.151   11 Apr 2000 13:44:46   dss
 * Defect #84650.  Added FCURSOR_AT_BOF and FCURSOR_AT_EOF so SMI can call
 * FLAIM to determine where a cursor is positioned.
 * 
 *    Rev 3.150   03 Apr 2000 12:19:32   dss
 * Defect #224828.  Added FLM_EXISTS_OP and FLM_NOT_EXISTS_OP.
 * 
 *    Rev 3.149   28 Mar 2000 14:55:02   andy
 * Defect 229961.  Added support for FDB_GET_LOCK_WAITERS_EX option on
 * FlmDbGetConfig.
 * 
 *    Rev 3.148   27 Mar 2000 15:47:28   dss
 * Defect #226128.  Various changes to support embedded user predicates.
 * 
 *    Rev 3.147   27 Mar 2000 14:55:54   andy
 * Defect 228966. Changes for 64-bit.
 * 
 *    Rev 3.146   17 Feb 2000 13:57:02   blj
 * Defect 225444 - fixed solaris inline function compiler warnings.
 * 
 *    Rev 3.145   16 Feb 2000 14:20:12   swp
 * Defect 223419 - index method a bool was an uint.
 * 
 *    Rev 3.144   10 Feb 2000 08:28:52   blj
 * Defect 223496 - inline function changes.
 * 
 *    Rev 3.143   08 Feb 2000 14:36:34   andy
 * Defect 223597.  Needed to include fansi.hpp.
 * 
 *    Rev 3.142   31 Jan 2000 09:26:52   swp
 * New API to get index status and resume and pause indexing.
 * 
 *    Rev 3.141   25 Jan 2000 14:54:44   dss
 * Added FLM_DONT_INSERT_IN_CACHE flag for FlmRecordAdd.
 * 
 *    Rev 3.140   12 Jan 2000 12:43:00   swp
 * Defect 219171 - added FlmDbConvert tag.
 * 
 *    Rev 3.139   12 Jan 2000 10:57:02   blj
 * defect #215743 - added do extended data check option.
 * 
 *    Rev 3.138   10 Jan 2000 13:00:54   dss
 * Defect #219053.  Added prototype for FlmStorage2UINT32.
 * 
 *    Rev 3.137   21 Dec 1999 15:51:32   dss
 * Defect #212233.  Added APIs for setting dynamic and hard cache limits and
 * for getting memory information.  Also added FLM_xxx enums for getting
 * and setting cache cleanup, cache adjust, and unused cleanup intervals.
 * 
 *    Rev 3.136   17 Dec 1999 15:40:12   swp
 * Defect 203283 - Flaim version 4.00 .
 * 
 *    Rev 3.135   10 Dec 1999 11:53:14   andy
 * Added FlmPathBuild and added extern "C" where appropriate.
 * 
 *    Rev 3.134   08 Dec 1999 09:54:30   blj
 * made macro for FlmStorage2INT.
 * 
 *    Rev 3.133   07 Dec 1999 14:54:38   blj
 * Defect #201695 - replaced SQ_INFO struct with OPT_INFO. Also moved fdefrec.
 * include file and added make werr macros.
 * 
 *    Rev 3.132   30 Nov 1999 12:56:40   blj
 * removed shared dict defines.
 * 
 *    Rev 3.131   22 Nov 1999 15:43:42   blj
 * Changed export/import method names.
 * 
 *    Rev 3.130   19 Nov 1999 13:24:14   swp
 * Moved major toolkit include files into FLAIM.  Moved out typedefs 
 * into ftypes.hpp.  New language routines.
 * 
 *    Rev 3.129   08 Nov 1999 09:11:02   dss
 * Added options to FlmDbConfig.
 * 
 *    Rev 3.128   04 Nov 1999 17:34:46   andy
 * Added several fuction tags to eFlmFuncs that were needed in fhooks.
 * 
 *    Rev 3.127   04 Nov 1999 16:52:32   blj
 * Added back in missing functions (blobs) and made several FlmBlob methods 
 * inline, and also removed extra space within file.
 * 
 *    Rev 3.126   04 Nov 1999 14:38:48   dss
 * Got rid of FLM_DIAG_FILE_PATH for diagnostics.
 * 
 *    Rev 3.125   04 Nov 1999 09:12:56   swp
 * Remove all references to session and transaction handles.
 * 
 *    Rev 3.124   18 Oct 1999 12:32:52   swp
 * Moved the record functionality of FlmKeyRetrieve into FlmRecordRetrieve.
 * 
 *    Rev 3.123   15 Oct 1999 10:30:52   jrd
 * Added FLM_FLD_RESET define for the get field callback.
 * 
 *    Rev 3.122   14 Oct 1999 15:38:06   blj
 * Removed FT_ defines and FDATATYPE enum.
 * 
 *    Rev 3.121   11 Oct 1999 09:56:04   jrd
 * Added parameter to the cusor field callback to return the record in which the
 * field was found.
 * 
 *    Rev 3.120   08 Oct 1999 09:05:56   andy
 * Added RCACHE_STATE structure.
 * 
 *    Rev 3.119   05 Oct 1999 09:13:32   blj
 * Fixed values of FO_ defines.
 * 
 *    Rev 3.118   04 Oct 1999 09:22:48   dss
 * Modified CHK_RECORD structure to use a FlmRecordSet object.
 * 
 *    Rev 3.117   04 Oct 1999 09:16:16   blj
 * getClassID changes (moved defines).
 * 
 *    Rev 3.116   30 Sep 1999 17:04:46   jrd
 * Added hdb parameter to field and compare callbacks, and added
 * FCURSOR_PREFER_RECS define to FlmCursorConfig.
 * 
 *    Rev 3.115   29 Sep 1999 13:22:26   blj
 * Changed FLMUINT8 parameters to use FLMUINT (on Storage convert funcs).
 * 
 *    Rev 3.114   28 Sep 1999 12:59:16   andy
 * Changed NODE_p members of FLM_UPDATE_EVENT to FlmRecord *.
 * 
 *    Rev 3.113   28 Sep 1999 12:43:02   andy
 * Removed FlmTransAbort.
 * 
 *    Rev 3.112   28 Sep 1999 11:07:00   andy
 * Removed phTransRV parameter from FlmSessionTransBegin.
 * 
 *    Rev 3.111   27 Sep 1999 14:22:16   blj
 * Removed defines and functions for WP wdstr, 51, & 60.
 * 
 *    Rev 3.110   27 Sep 1999 10:38:36   jrd
 * Made changes for new FLAIM record class.
 * 
 *    Rev 3.0   23 Dec 1997 09:39:56   dss
 * New file for SLICK project
 * 
 *    Rev 2.0   12 Mar 1997 11:29:04   andy
 * GroupWise 5.1 Shipping Code
 * 
*****************************************************************************/
#ifndef  FLAIM_H
#define  FLAIM_H

	#if defined( XFLAIM_H) && defined( SUPPORT_OLD_XFLAIM_NAMES)
		#error Cannot support old X-FLAIM typedefs, etc. when including flaim.h
	#endif

	#ifndef FLM_PLATFORM_CONFIGURED
		#define FLM_PLATFORM_CONFIGURED

	// Determine the build platform

	#undef FLM_WIN32
	#undef FLM_WIN64
	#undef FLM_NLM
	#undef FLM_UNIX
	#undef FLM_AIX
	#undef FLM_LINUX
	#undef FLM_SOLARIS
	#undef FLM_SPARC
	#undef FLM_OSF
	#undef FLM_HPUX
	#undef FLM_MACOSX
	#undef FLM_BIG_ENDIAN

	#if defined( __NETWARE__) || defined( NLM) || defined( N_PLAT_NLM)
		#define FLM_NLM
	#elif defined( _WIN64)
		#define FLM_WIN32
		#define FLM_WIN64
	#elif defined( _WIN32)
		#define FLM_WIN32
	#elif defined( _AIX)
		#define FLM_UNIX
		#define FLM_AIX
		#define FLM_BIG_ENDIAN
	#elif defined( linux)
		#define FLM_UNIX
		#define FLM_LINUX
	#elif defined( sun)
		#define FLM_UNIX
		#define FLM_SOLARIS
		#if defined( sparc) || defined( __sparc)
			#define FLM_SPARC
				#define FLM_BIG_ENDIAN
		#endif
	#elif defined( __osf__)
		#define FLM_UNIX
		#define FLM_OSF
			#define FLM_BIG_ENDIAN
	#elif defined( __hpux) || defined( hpux)
		#define FLM_UNIX
		#define FLM_HPUX
			#define FLM_BIG_ENDIAN
	#elif defined( __ppc__) && defined( __APPLE__)
		#define FLM_UNIX
		#define FLM_MACOSX
		#define FLM_BIG_ENDIAN
	#else
			#error Platform architecture is undefined.
	#endif

	// Debug or release build?

	#ifndef FLM_DEBUG
		#if defined( DEBUG) || (defined( PRECHECKIN) && PRECHECKIN != 0)
			#define FLM_DEBUG
		#endif
	#endif

	// Alignment

	#if( defined( FLM_WIN32) && !defined( FLM_WIN64)) || defined( FLM_NLM)
			#define FLM_ALLOC_ALIGN			0x0003
			#define FLM_ALIGN_SIZE			4
	#elif defined( FLM_UNIX) || defined( FLM_WIN64)
			#define FLM_ALLOC_ALIGN			0x0007
			#define FLM_ALIGN_SIZE			8
	#else
		#error Platform not supported
	#endif

	// Basic type definitions

	#if defined( FLM_UNIX)
		typedef unsigned long		FLMUINT;
		typedef long					FLMINT;
		typedef unsigned char		FLMBYTE;
		typedef unsigned short		FLMUNICODE;

		typedef unsigned long long	FLMUINT64;
		typedef unsigned int   		FLMUINT32;
		typedef unsigned short		FLMUINT16;
		typedef unsigned char  		FLMUINT8;
		typedef			  long long	FLMINT64;
		typedef			  int	  		FLMINT32;
		typedef          short		FLMINT16;
		typedef signed   char		FLMINT8;
		#if defined( __x86_64__) || defined( FLM_AIX) || defined( FLM_HPUX) || defined(FLM_MACOSX)
			typedef unsigned long			FLMSIZET;
		#else
			typedef unsigned 					FLMSIZET;
		#endif
	#else

		#if defined( FLM_WIN64)
			typedef unsigned __int64	FLMUINT;
			typedef __int64				FLMINT;
			typedef unsigned long		FLMSIZET;
		#else
			typedef unsigned long		FLMUINT;
			typedef long					FLMINT;
		#endif

		typedef unsigned char			FLMBYTE;
		typedef unsigned short int		FLMUNICODE;

		typedef unsigned int   			FLMUINT32;
		typedef unsigned short int		FLMUINT16;
		typedef unsigned char  			FLMUINT8;
		typedef signed int   			FLMINT32;
		typedef signed short int		FLMINT16;
		typedef signed char				FLMINT8;
		typedef unsigned					FLMSIZET;

		#if defined( __MWERKS__)
			typedef unsigned long long		FLMUINT64;
			typedef long long					FLMINT64;
		#else
			typedef unsigned __int64 		FLMUINT64;
			typedef __int64 					FLMINT64;
		#endif

	#endif

		typedef FLMINT								RCODE;
		typedef FLMINT								FLMBOOL;

	#endif

	// Disable certain compiler warnings

	#ifdef FLM_WIN32

		// For some reason, Windows emits a warning when the packing
		// is changed.

		#pragma warning( disable : 4103)

		// Unreferenced inline function

		#pragma warning( disable : 4514)

	#endif

	#if !defined( FLM_UNIX) && !defined( FLM_WIN64)
		#pragma pack( push, 1)
	#endif

	#ifndef NULL
		#define NULL   0
	#endif

	#ifndef TRUE
		#define TRUE   1
	#endif

	#ifndef FALSE
		#define FALSE  0
	#endif

	typedef void *				F_MUTEX;
	typedef void *				F_SEM;
	#define F_MUTEX_NULL		NULL

	#define F_MAXIMUM_FILE_SIZE		0xFFFC0000
	#define F_FILENAME_SIZE				256
	#define F_PATH_MAX_SIZE				256

	#ifndef RC_OK
		#define RC_OK( rc)						((rc) == 0)
	#endif

	#ifndef RC_BAD
		#define RC_BAD( rc)						((rc) != 0)
	#endif

	#define FLM_ERROR_BASE(e)					((RCODE)((int)((e))))
	#define FERR_OK						0

	#define FIRST_FLAIM_ERROR					FLM_ERROR_BASE( 0xC001)		// First (lowest) error currently defined
	#define FERR_BOF_HIT							FLM_ERROR_BASE( 0xC001)		// Beginning of file hit
	#define FERR_EOF_HIT							FLM_ERROR_BASE( 0xC002)		// End of file hit
	#define FERR_END								FLM_ERROR_BASE( 0xC003)		// End of file for gedcom routine.  This is an internal error
	#define FERR_EXISTS							FLM_ERROR_BASE( 0xC004)		// Record already exists
	#define FERR_FAILURE							FLM_ERROR_BASE( 0xC005)		// Internal failure
	#define FERR_NOT_FOUND						FLM_ERROR_BASE( 0xC006)		// A record, key, or key reference was not found
	#define FERR_BAD_DICT_ID					FLM_ERROR_BASE( 0xC007)		// Invalid dictionary record number -- outside unreserved range
	#define FERR_BAD_CONTAINER					FLM_ERROR_BASE( 0xC008)		// Invalid Container Number
	#define FERR_NO_ROOT_BLOCK					FLM_ERROR_BASE( 0xC009)		// LFILE does not have a root block.
																				// Always handled internally - never returned to application
	#define FERR_BAD_DRN							FLM_ERROR_BASE( 0xC00A)		// Cannot pass a zero DRN into modify or delete or 0xFFFFFFFF into add
	#define FERR_BAD_FIELD_NUM					FLM_ERROR_BASE( 0xC00B)		// Bad field number in record being added
	#define FERR_BAD_FIELD_TYPE				FLM_ERROR_BASE( 0xC00C)		// Bad field type in record being added
	#define FERR_BAD_HDL							FLM_ERROR_BASE( 0xC00D)		// Request contained bad db handle
	#define FERR_BAD_IX							FLM_ERROR_BASE( 0xC00E)		// Invalid Index Number Given
	#define FERR_BACKUP_ACTIVE					FLM_ERROR_BASE( 0xC00F)		// Operation could not be completed - a backup is being performed
	#define FERR_SERIAL_NUM_MISMATCH			FLM_ERROR_BASE( 0xC010)		// Comparison of serial numbers failed
	#define FERR_BAD_RFL_DB_SERIAL_NUM		FLM_ERROR_BASE( 0xC011)		// Bad database serial number in RFL file header
	#define FERR_BTREE_ERROR					FLM_ERROR_BASE( 0xC012)		// The B-Tree in the file system is bad
	#define FERR_BTREE_FULL						FLM_ERROR_BASE( 0xC013)		// The B-tree in the file system is full
	#define FERR_BAD_RFL_FILE_NUMBER			FLM_ERROR_BASE( 0xC014)		// Bad RFL file number in RFL file header
	#define FERR_CANNOT_DEL_ITEM				FLM_ERROR_BASE( 0xC015)		// Cannot delete field definitions
	#define FERR_CANNOT_MOD_FIELD_TYPE		FLM_ERROR_BASE( 0xC016)		// Cannot modify a field's type
	#define FERR_CONV_BAD_DEST_TYPE			FLM_ERROR_BASE( 0xC018)		// Bad destination type specified for conversion
	#define FERR_CONV_BAD_DIGIT				FLM_ERROR_BASE( 0xC019)		// Non-numeric digit found in text to numeric conversion
	#define FERR_CONV_BAD_SRC_TYPE			FLM_ERROR_BASE( 0xC01A)		// Bad source type specified for conversion
	#define FERR_RFL_FILE_NOT_FOUND			FLM_ERROR_BASE( 0xC01B)		// Could not open an RFL file.
	#define FERR_CONV_DEST_OVERFLOW			FLM_ERROR_BASE( 0xC01C)		// Destination buffer not large enough to hold converted data
	#define FERR_CONV_ILLEGAL					FLM_ERROR_BASE( 0xC01D)		// Illegal conversion -- not supported
	#define FERR_CONV_NULL_SRC					FLM_ERROR_BASE( 0xC01E)		// Source cannot be a NULL pointer in conversion
	#define FERR_CONV_NULL_DEST				FLM_ERROR_BASE( 0xC01F)		// Destination cannot be a NULL pointer in conversion
	#define FERR_CONV_NUM_OVERFLOW			FLM_ERROR_BASE( 0xC020)		// Numeric overflow (GT upper bound) converting to numeric type
	#define FERR_CONV_NUM_UNDERFLOW			FLM_ERROR_BASE( 0xC021)		// Numeric underflow (LT lower bound) converting to numeric type
	#define FERR_DATA_ERROR						FLM_ERROR_BASE( 0xC022)		// Data in the database is invalid
	#define FERR_DD_ERROR						FLM_ERROR_BASE( 0xC024)		// Internal logical DD compromised
	#define FERR_INVALID_FILE_SEQUENCE		FLM_ERROR_BASE( 0xC025)		// Inc. backup file provided during a restore is invalid
	#define FERR_ILLEGAL_OP						FLM_ERROR_BASE( 0xC026)		// Illegal operation for database
	#define FERR_DUPLICATE_DICT_REC			FLM_ERROR_BASE( 0xC027)		// Duplicate dictionary record found
	#define FERR_CANNOT_CONVERT				FLM_ERROR_BASE( 0xC028)		// Condition occurred which prevents database conversion
	#define FERR_UNSUPPORTED_VERSION			FLM_ERROR_BASE( 0xC029)		// Db version is an unsupported ver of FLAIM (ver 1.2)
	#define FERR_FILE_ER							FLM_ERROR_BASE( 0xC02A)		// File error in a gedcom routine
	#define FERR_BAD_FIELD_LEVEL				FLM_ERROR_BASE( 0xC02B)		// Invalid field level
	#define FERR_GED_BAD_RECID					FLM_ERROR_BASE( 0xC02C)		// Bad record ID syntax
	#define FERR_GED_BAD_VALUE					FLM_ERROR_BASE( 0xC02D)		// Bad or ambiguous/extra value in GEDCOM
	#define FERR_GED_MAXLVLNUM					FLM_ERROR_BASE( 0xC02E)		// Exceeded GED_MAXLVLNUM in gedcom routines
	#define FERR_GED_SKIP_LEVEL				FLM_ERROR_BASE( 0xC02F)		// Bad GEDCOM tree structure -- level skipped
	#define FERR_ILLEGAL_TRANS					FLM_ERROR_BASE( 0xC030)		// Attempt to start an illegal type of transaction
	#define FERR_ILLEGAL_TRANS_OP				FLM_ERROR_BASE( 0xC031)		// Illegal operation for transaction type
	#define FERR_INCOMPLETE_LOG				FLM_ERROR_BASE( 0xC032)		// Incomplete log record encountered during recovery
	#define FERR_INVALID_BLOCK_LENGTH		FLM_ERROR_BASE( 0xC033)		// Invalid Block Length
	#define FERR_INVALID_TAG					FLM_ERROR_BASE( 0xC034)		// Invalid tag name
	#define FERR_KEY_NOT_FOUND					FLM_ERROR_BASE( 0xC035)		// A key|reference is not found -- modify/delete error
	#define FERR_VALUE_TOO_LARGE				FLM_ERROR_BASE( 0xC036)		// Value too large
	#define FERR_MEM								FLM_ERROR_BASE( 0xC037)		// General memory allocation error
	#define FERR_BAD_RFL_SERIAL_NUM			FLM_ERROR_BASE( 0xC038)		// Bad serial number in RFL file header
	#define FERR_NEWER_FLAIM					FLM_ERROR_BASE( 0xC03A)		// Running old code on a newer database code must be upgraded
	#define FERR_CANNOT_MOD_FIELD_STATE		FLM_ERROR_BASE( 0xC03B)		// Attempted to change a field state illegally
	#define FERR_NO_MORE_DRNS					FLM_ERROR_BASE( 0xC03C)		// The highest DRN number has already been used in an add
	#define FERR_NO_TRANS_ACTIVE				FLM_ERROR_BASE( 0xC03D)		// Attempted to updated DB outside transaction
	#define FERR_NOT_UNIQUE						FLM_ERROR_BASE( 0xC03E)		// Found Duplicate key for unique index
	#define FERR_NOT_FLAIM						FLM_ERROR_BASE( 0xC03F)		// Opened a file that was not a FLAIM file
	#define FERR_NULL_RECORD					FLM_ERROR_BASE( 0xC040)		// NULL Record cannot be passed to add or modify
	#define FERR_NO_HTTP_STACK					FLM_ERROR_BASE( 0xC041)		// No http stack was loaded
	#define FERR_OLD_VIEW						FLM_ERROR_BASE( 0xC042)		// While reading was unable to get previous version of block
	#define FERR_PCODE_ERROR					FLM_ERROR_BASE( 0xC043)		// The integrity of the dictionary PCODE in the database has been compromised
	#define FERR_PERMISSION						FLM_ERROR_BASE( 0xC044)		// Invalid permission for file operation
	#define FERR_SYNTAX							FLM_ERROR_BASE( 0xC045)		// Dictionary record has improper syntax
	#define FERR_CALLBACK_FAILURE				FLM_ERROR_BASE( 0xC046)		// Callback failure
	#define FERR_TRANS_ACTIVE					FLM_ERROR_BASE( 0xC047)		// Attempted to close DB while transaction was active
	#define FERR_RFL_TRANS_GAP					FLM_ERROR_BASE( 0xC048)		// A gap was found in the transaction sequence in the RFL
	#define FERR_BAD_COLLATED_KEY				FLM_ERROR_BASE( 0xC049)		// Something in collated key is bad.
	#define FERR_UNSUPPORTED_FEATURE			FLM_ERROR_BASE( 0xC04A)		// Attempting a feature that is not supported for the database version.
	#define FERR_MUST_DELETE_INDEXES			FLM_ERROR_BASE( 0xC04B)		// Attempting to delete a container that has indexes defined for it.  Indexes must be deleted first.
	#define FERR_RFL_INCOMPLETE				FLM_ERROR_BASE( 0xC04C)		// RFL file is incomplete.
	#define FERR_CANNOT_RESTORE_RFL_FILES	FLM_ERROR_BASE( 0xC04D)		// Cannot restore RFL files - not using multiple RFL files.
	#define FERR_INCONSISTENT_BACKUP			FLM_ERROR_BASE( 0xC04E)		// A problem (corruption, etc.) was detected in a backup set
	#define FERR_BLOCK_CHECKSUM				FLM_ERROR_BASE( 0xC04F)		// Block checksum error
	#define FERR_ABORT_TRANS					FLM_ERROR_BASE( 0xC050)		// Attempted operation after a critical error - should abort transaction
	#define FERR_NOT_RFL							FLM_ERROR_BASE( 0xC051)		// Attempted to open RFL file which was not an RFL file
	#define FERR_BAD_RFL_PACKET				FLM_ERROR_BASE( 0xC052)		// RFL packet was bad
	#define FERR_DATA_PATH_MISMATCH			FLM_ERROR_BASE( 0xC053)		// Bad data path specified to open database
	#define FERR_HTTP_REGISTER_FAILURE		FLM_ERROR_BASE( 0xC054)		// FlmConfig( FLM_HTTP_REGISTER_URL) failed
	#define FERR_HTTP_DEREG_FAILURE			FLM_ERROR_BASE( 0xC055)		// FlmConfig( FLM_HTTP_DEREGISTER_URL) failed
	#define FERR_IX_FAILURE						FLM_ERROR_BASE( 0xC056)		// Indexing process failed, non-unique data was found when a unique index was being created
	#define FERR_HTTP_SYMS_EXIST				FLM_ERROR_BASE( 0xC057)		// Tried to import new http related symbols before unimporting the old ones
	#define FERR_FILE_EXISTS					FLM_ERROR_BASE( 0xC059)		// Attempt to create a database or store file, but the file already exists
	#define FERR_SYM_RESOLVE_FAIL				FLM_ERROR_BASE( 0xC05A)		// Call to SAL_ModResolveSym failed.
	#define FERR_BAD_SERVER_CONNECTION		FLM_ERROR_BASE( 0xC05B)		// Connection to FLAIM server is bad
	#define FERR_CLOSING_DATABASE				FLM_ERROR_BASE( 0xC05C)		// Database is being closed due to a critical erro
	#define FERR_INVALID_CRC					FLM_ERROR_BASE( 0xC05D)		// CRC could not be verified.
	#define FERR_KEY_OVERFLOW					FLM_ERROR_BASE( 0xC05E)		// Key generated by the record causes the max key size to be exceeded
	#define FERR_NOT_IMPLEMENTED				FLM_ERROR_BASE( 0xC05F)		// function not implemented (possibly client/server)
	#define FERR_MUTEX_OPERATION_FAILED		FLM_ERROR_BASE( 0xC060)		// Mutex operation failed
	#define FERR_MUTEX_UNABLE_TO_LOCK		FLM_ERROR_BASE( 0xC061)		// Unable to get the mutex lock
	#define FERR_SEM_OPERATION_FAILED		FLM_ERROR_BASE( 0xC062)		// Semaphore operation failed
	#define FERR_SEM_UNABLE_TO_LOCK			FLM_ERROR_BASE( 0xC063)		// Unable to get the semaphore lock
	#define FERR_BAD_REFERENCE					FLM_ERROR_BASE( 0xC069)		// Bad reference in the dictionary
	#define FERR_DRIVER_NOT_FOUND				FLM_ERROR_BASE( 0xC06C)		// Could not find a matching driver in machine's area definition
	#define FERR_BAD_DRIVER_PATH				FLM_ERROR_BASE( 0xC06D)		// Tried one or more driver directory paths and failed
	#define FERR_UNALLOWED_UPGRADE			FLM_ERROR_BASE( 0xC070)		// FlmDbUpgrade cannot upgrade the database
	#define FERR_ID_RESERVED					FLM_ERROR_BASE( 0xC074)		// Attempted to use a dictionary ID that has been reserved at a lower level
	#define FERR_CANNOT_RESERVE_ID			FLM_ERROR_BASE( 0xC075)		// Attempted to reserve a dictionary ID that has been used at a higher level
	#define FERR_DUPLICATE_DICT_NAME			FLM_ERROR_BASE( 0xC076)		// Dictionary record with duplicate name found
	#define FERR_CANNOT_RESERVE_NAME			FLM_ERROR_BASE( 0xC077)		// Attempted to reserve a dictionary name that is in use
	#define FERR_BAD_DICT_DRN					FLM_ERROR_BASE( 0xC078)		// Attempted to add, modify, or delete a dictionary DRN >= FLM_RESERVED_TAG_NUMS
	#define FERR_CANNOT_MOD_DICT_REC_TYPE	FLM_ERROR_BASE( 0xC079)		// Cannot modify a dictionary item into another type of item, must delete then add
	#define FERR_PURGED_FLD_FOUND				FLM_ERROR_BASE( 0xC07A)		// Record contained 'purged' field
	#define FERR_DUPLICATE_INDEX				FLM_ERROR_BASE( 0xC07B)		// Duplicate index
	#define FERR_TOO_MANY_OPEN_FILES			FLM_ERROR_BASE( 0xC07C)		// No session file handles could be closed - in to increase via FlmSetMaxPhysOpens()
	#define FERR_ACCESS_DENIED					FLM_ERROR_BASE( 0xC07D)		// Access Denied from setting in log header
	#define FERR_CACHE_ERROR					FLM_ERROR_BASE( 0xC07F)		// Cache Block is somehow corrupt
	#define FERR_BLOB_MISSING_FILE			FLM_ERROR_BASE( 0xC081)		// Missing BLOB file on add/modify
	#define FERR_NO_REC_FOR_KEY				FLM_ERROR_BASE( 0xC082)		// Record pointed to by an index key is missing
	#define FERR_DB_FULL							FLM_ERROR_BASE( 0xC083)		// Database is full, cannot create more blocks
	#define FERR_TIMEOUT							FLM_ERROR_BASE( 0xC084)		// Query operation timed out
	#define FERR_CURSOR_SYNTAX					FLM_ERROR_BASE( 0xC085)		// Cursor operation had improper syntax
	#define FERR_THREAD_ERR						FLM_ERROR_BASE( 0xC086)		// Thread Error
	#define FERR_SYS_CHECK_FAILED				FLM_ERROR_BASE( 0xC087)		// File system is not supported for the requested operation
	#define FERR_EMPTY_QUERY					FLM_ERROR_BASE( 0xC088)		// Warning: Query has no results
	#define FERR_INDEX_OFFLINE					FLM_ERROR_BASE( 0xC089)		// Warning: Index is offline and being rebuilt
	#define FERR_TRUNCATED_KEY					FLM_ERROR_BASE( 0xC08A)		// Warning: Can't evaluate truncated key against selection criteria
	#define FERR_INVALID_PARM					FLM_ERROR_BASE( 0xC08B)		// Invalid parm
	#define FERR_USER_ABORT						FLM_ERROR_BASE( 0xC08C)		// User or application aborted the operation
	#define FERR_RFL_DEVICE_FULL				FLM_ERROR_BASE( 0xC08D)		// No space on RFL device for logging
	#define FERR_MUST_WAIT_CHECKPOINT		FLM_ERROR_BASE( 0xC08E)		// Must wait for a checkpoint before starting transaction - due to disk problems - usually in RFL volume.
	#define FERR_NAMED_SEMAPHORE_ERR			FLM_ERROR_BASE( 0xC08F)		// Something bad happened with the named semaphore class (F_NamedSemaphore)
	#define FERR_LOAD_LIBRARY					FLM_ERROR_BASE( 0xC090)		// Failed to load a shared library module
	#define FERR_UNLOAD_LIBRARY				FLM_ERROR_BASE( 0xC091)		// Failed to unload a shared library module
	#define FERR_IMPORT_SYMBOL					FLM_ERROR_BASE( 0xC092)		// Failed to import a symbol from a shared library module
	#define FERR_BLOCK_FULL						FLM_ERROR_BASE( 0xC093)		// Destination block for insert is full
	#define FERR_BAD_BASE64_ENCODING			FLM_ERROR_BASE( 0xC094)		// Could not perform Base64 encoding
	#define FERR_MISSING_FIELD_TYPE			FLM_ERROR_BASE( 0xC095)		// Adding a dictionary without a field type.

	/*============================================================================
							IO Errors
	============================================================================*/

	#define FERR_IO_ACCESS_DENIED				FLM_ERROR_BASE( 0xC201)		// Access denied. Caller is not allowed access to a file.
	#define FERR_IO_BAD_FILE_HANDLE			FLM_ERROR_BASE( 0xC202)		// Bad file handle
	#define FERR_IO_COPY_ERR					FLM_ERROR_BASE( 0xC203)		// Copy error
	#define FERR_IO_DISK_FULL					FLM_ERROR_BASE( 0xC204)		// Disk full
	#define FERR_IO_END_OF_FILE				FLM_ERROR_BASE( 0xC205)		// End of file
	#define FERR_IO_OPEN_ERR					FLM_ERROR_BASE( 0xC206)		// Error opening file
	#define FERR_IO_SEEK_ERR					FLM_ERROR_BASE( 0xC207)		// File seek error
	#define FERR_IO_MODIFY_ERR					FLM_ERROR_BASE( 0xC208)		// File modify error
	#define FERR_IO_PATH_NOT_FOUND			FLM_ERROR_BASE( 0xC209)		// Path not found
	#define FERR_IO_TOO_MANY_OPEN_FILES		FLM_ERROR_BASE( 0xC20A)		// Too many files open
	#define FERR_IO_PATH_TOO_LONG				FLM_ERROR_BASE( 0xC20B)		// Path too long
	#define FERR_IO_NO_MORE_FILES				FLM_ERROR_BASE( 0xC20C)		// No more files in directory
	#define FERR_DELETING_FILE					FLM_ERROR_BASE( 0xC20D)		// Had error deleting a file
	#define FERR_IO_FILE_LOCK_ERR				FLM_ERROR_BASE( 0xC20E)		// File lock error
	#define FERR_IO_FILE_UNLOCK_ERR			FLM_ERROR_BASE( 0xC20F)		// File unlock error
	#define FERR_IO_PATH_CREATE_FAILURE		FLM_ERROR_BASE( 0xC210)		// Path create failed
	#define FERR_IO_RENAME_FAILURE			FLM_ERROR_BASE( 0xC211)		// File rename failed
	#define FERR_IO_INVALID_PASSWORD			FLM_ERROR_BASE( 0xC212)		// Invalid file password
	#define FERR_SETTING_UP_FOR_READ			FLM_ERROR_BASE( 0xC213)		// Had error setting up to do a read
	#define FERR_SETTING_UP_FOR_WRITE		FLM_ERROR_BASE( 0xC214)		// Had error setting up to do a write
	#define FERR_IO_AT_PATH_ROOT				FLM_ERROR_BASE( 0xC215)		// Currently positioned at the path root level
	#define FERR_INITIALIZING_IO_SYSTEM		FLM_ERROR_BASE( 0xC216)		// Had error initializing the file system
	#define FERR_FLUSHING_FILE					FLM_ERROR_BASE( 0xC217)		// Had error flushing a file
	#define FERR_IO_INVALID_PATH				FLM_ERROR_BASE( 0xC218)		// Invalid path
	#define FERR_IO_CONNECT_ERROR				FLM_ERROR_BASE( 0xC219)		// Failed to connect to a remote network resource
	#define FERR_OPENING_FILE					FLM_ERROR_BASE( 0xC21A)		// Had error opening a file
	#define FERR_DIRECT_OPENING_FILE			FLM_ERROR_BASE( 0xC21B)		// Had error opening a file for direct I/O
	#define FERR_CREATING_FILE					FLM_ERROR_BASE( 0xC21C)		// Had error creating a file
	#define FERR_DIRECT_CREATING_FILE		FLM_ERROR_BASE( 0xC21D)		// Had error creating a file for direct I/O
	#define FERR_READING_FILE					FLM_ERROR_BASE( 0xC21E)		// Had error reading a file
	#define FERR_DIRECT_READING_FILE			FLM_ERROR_BASE( 0xC21F)		// Had error reading a file using direct I/O
	#define FERR_WRITING_FILE					FLM_ERROR_BASE( 0xC220)		// Had error writing to a file
	#define FERR_DIRECT_WRITING_FILE			FLM_ERROR_BASE( 0xC221)		// Had error writing to a file using direct I/O
	#define FERR_POSITIONING_IN_FILE			FLM_ERROR_BASE( 0xC222)		// Had error positioning within a file
	#define FERR_GETTING_FILE_SIZE			FLM_ERROR_BASE( 0xC223)		// Had error getting file size
	#define FERR_TRUNCATING_FILE				FLM_ERROR_BASE( 0xC224)		// Had error truncating a file
	#define FERR_PARSING_FILE_NAME			FLM_ERROR_BASE( 0xC225)		// Had error parsing a file name
	#define FERR_CLOSING_FILE					FLM_ERROR_BASE( 0xC226)		// Had error closing a file
	#define FERR_GETTING_FILE_INFO			FLM_ERROR_BASE( 0xC227)		// Had error getting file information
	#define FERR_EXPANDING_FILE				FLM_ERROR_BASE( 0xC228)		// Had error expanding a file (using direct I/O)
	#define FERR_GETTING_FREE_BLOCKS			FLM_ERROR_BASE( 0xC229)		// Had error getting free blocks from file system
	#define FERR_CHECKING_FILE_EXISTENCE	FLM_ERROR_BASE( 0xC22A)		// Had error checking if a file exists
	#define FERR_RENAMING_FILE					FLM_ERROR_BASE( 0xC22B)		// Had error renaming a file
	#define FERR_SETTING_FILE_INFO			FLM_ERROR_BASE( 0xC22C)		// Had error setting file information

	/*============================================================================
							Encryption / Decryption Errors
	============================================================================*/
	#define FERR_NICI_CONTEXT					FLM_ERROR_BASE( 0xC301)		// Failed to obtain a NICI context
	#define FERR_NICI_FIND_INIT				FLM_ERROR_BASE( 0xC302)		// CCS_FindInit failed
	#define FERR_NICI_FIND_OBJECT				FLM_ERROR_BASE( 0xC303)		// CCS_FindObject failed
	#define FERR_NICI_WRAPKEY_NOT_FOUND		FLM_ERROR_BASE( 0xC304)		// Could not locate a wrapping key
	#define FERR_NICI_ATTRIBUTE_VALUE		FLM_ERROR_BASE( 0xC305)		// CCS_AttributeVAlue failed
	#define FERR_NICI_BAD_ATTRIBUTE			FLM_ERROR_BASE( 0xC306)		// Invalid attribute
	#define FERR_NICI_BAD_RANDOM				FLM_ERROR_BASE( 0xC307)		// CCS_GetRandom failed
	#define FERR_NICI_WRAPKEY_FAILED			FLM_ERROR_BASE( 0xC309)		// CCS_WrapKey failed
	#define FERR_NICI_GENKEY_FAILED			FLM_ERROR_BASE( 0xC30A)		// CCS_GenerateKey failed
	#define FERR_REQUIRE_PASSWD				FLM_ERROR_BASE( 0xC30B)		// Password required to unwrap key
	#define FERR_NICI_SHROUDKEY_FAILED		FLM_ERROR_BASE( 0xC30C)		// CCS_pbeShroudPrivateKey failed
	#define FERR_NICI_UNSHROUDKEY_FAILED	FLM_ERROR_BASE( 0xC30D)		// CCS_pbdUnshroudPrivateKey failed
	#define FERR_NICI_UNWRAPKEY_FAILED		FLM_ERROR_BASE( 0xC30E)		// CCS_UnrapKey failed
	#define FERR_NICI_ENC_INIT_FAILED		FLM_ERROR_BASE( 0xC30F)		// CCS_DataEncryptInit failed
	#define FERR_NICI_ENCRYPT_FAILED			FLM_ERROR_BASE( 0xC310)		// CCS_DataEncrypt failed
	#define FERR_NICI_DECRYPT_INIT_FAILED	FLM_ERROR_BASE( 0xC311)		// CCS_DataDecryptInit failed
	#define FERR_NICI_DECRYPT_FAILED			FLM_ERROR_BASE( 0xC312)		// CCS_DataDecrypt failed
	#define FERR_NICI_INIT_FAILED				FLM_ERROR_BASE( 0xC313)		// CCS_Init falied.
	#define FERR_NICI_KEY_NOT_FOUND			FLM_ERROR_BASE( 0xC314)		// Could not locate encryption/decryption key
	#define FERR_NICI_INVALID_ALGORITHM		FLM_ERROR_BASE( 0xC315)		// Unsupported NICI ecncryption algorithm
	#define FERR_FLD_NOT_ENCRYPTED			FLM_ERROR_BASE( 0xC316)		// FlmRecord field is not encrypted
	#define FERR_CANNOT_SET_KEY				FLM_ERROR_BASE( 0xC317)		// Attempted to set a key on record add
	#define FERR_MISSING_ENC_TYPE				FLM_ERROR_BASE( 0xC318)		// Adding EncDef record without Encryption type.
	#define FERR_CANNOT_MOD_ENC_TYPE			FLM_ERROR_BASE( 0xC319)		// Attempting to change the encryption type
	#define FERR_MISSING_ENC_KEY				FLM_ERROR_BASE( 0xC31A)		// Modified EncDef record is missing a key.
	#define FERR_CANNOT_CHANGE_KEY			FLM_ERROR_BASE( 0xC31B)		// Attempt to modify the key in an EncDef record.
	#define FERR_BAD_ENC_KEY					FLM_ERROR_BASE( 0xC31C)
	#define FERR_CANNOT_MOD_ENC_STATE		FLM_ERROR_BASE( 0xC31D)
	#define FERR_DATA_SIZE_MISMATCH			FLM_ERROR_BASE( 0xC31E)
	#define FERR_ENCRYPTION_UNAVAILABLE		FLM_ERROR_BASE( 0xC31F)
	#define FERR_PURGED_ENCDEF_FOUND			FLM_ERROR_BASE( 0xC320)
	#define FERR_FLD_NOT_DECRYPTED			FLM_ERROR_BASE( 0xC321)
	#define FERR_BAD_ENCDEF_ID					FLM_ERROR_BASE( 0xC322)
	#define FERR_PBE_ENCRYPT_FAILED			FLM_ERROR_BASE( 0xC323)
	#define FERR_DIGEST_FAILED					FLM_ERROR_BASE( 0xC324)
	#define FERR_DIGEST_INIT_FAILED			FLM_ERROR_BASE( 0xC325)
	#define FERR_EXTRACT_KEY_FAILED			FLM_ERROR_BASE( 0xC326)
	#define FERR_INJECT_KEY_FAILED			FLM_ERROR_BASE( 0xC327)
	#define FERR_PBE_DECRYPT_FAILED			FLM_ERROR_BASE( 0xC328)
	#define FERR_PASSWD_INVALID				FLM_ERROR_BASE( 0xC329)
	

	/*============================================================================
							Server TCP/IP Errors
	============================================================================*/

	#define FERR_SVR_NOIP_ADDR					FLM_ERROR_BASE( 0xC900)		// IP address not found
	#define FERR_SVR_SOCK_FAIL					FLM_ERROR_BASE( 0xC901)		// IP socket failure
	#define FERR_SVR_CONNECT_FAIL				FLM_ERROR_BASE( 0xC902)		// TCP/IP connection failure
	#define FERR_SVR_BIND_FAIL					FLM_ERROR_BASE( 0xC903)		// The TCP/IP services on your system may not be configured or installed.  If this POA is not to run Client/Server, use the /notcpip startup switch or disable TCP/IP through the NWADMIN snapin
	#define FERR_SVR_LISTEN_FAIL				FLM_ERROR_BASE( 0xC904)		// TCP/IP listen failed
	#define FERR_SVR_ACCEPT_FAIL				FLM_ERROR_BASE( 0xC905)		// TCP/IP accept failed
	#define FERR_SVR_SELECT_ERR				FLM_ERROR_BASE( 0xC906)		// TCP/IP select failed
	#define FERR_SVR_SOCKOPT_FAIL				FLM_ERROR_BASE( 0xC907)		// TCP/IP socket operation failed
	#define FERR_SVR_DISCONNECT				FLM_ERROR_BASE( 0xC908)		// TCP/IP disconnected
	#define FERR_SVR_READ_FAIL					FLM_ERROR_BASE( 0xC909)		// TCP/IP read failed
	#define FERR_SVR_WRT_FAIL					FLM_ERROR_BASE( 0xC90A)		// TCP/IP write failed
	#define FERR_SVR_READ_TIMEOUT				FLM_ERROR_BASE( 0xC90B)		// TCP/IP read timeout
	#define FERR_SVR_WRT_TIMEOUT				FLM_ERROR_BASE( 0xC90C)		// TCP/IP write timeout
	#define FERR_SVR_ALREADY_CLOSED			FLM_ERROR_BASE( 0xC90D)		// Connection already closed
	#define LAST_FLAIM_ERROR					FLM_ERROR_BASE( 0xC90D)		// Last error currently defined

	/***************************************************************************
	*                        Forward Declarations
	***************************************************************************/

	class FlmRecord;
	class FlmRecordSet;
	class F_LogMessage;
	class F_FileHdl;
	class F_ListItem;
	class F_ListMgr;

	/***************************************************************************
	*                             FLAIM Types
	***************************************************************************/

	typedef void *					HFDB;          // Pointer to a FLAIM Database
		#define HFDB_NULL          NULL
	typedef void *					HFCURSOR;      // Pointer to a FLAIM Cursor
		#define HFCURSOR_NULL      NULL
	typedef void *             HFBLOB;        // Pointer to a FLAIM BLOB
		#define HFBLOB_NULL        NULL
	typedef void **		      HFBLOB_p;      // Handle to a FLAIM BLOB
	typedef void *					HFBACKUP;		// Database backup handle
		#define HFBACKUP_NULL	NULL

	typedef struct node *		NODE_p;        // GEDCOM Node (field)
	typedef NODE_p  *				NODE_pp;       // Pointer to a NODE pointer


	/*
	Struct:  Pool Memory Block Structure (MBLK)
	Desc:    To be used by some but not required by your favorite alloc routines.
	*        The memory block structure is usually allocated BEFORE
	*        the PMM structure.  This allows a nice way to destroy all
	*        memory as well as the memory pool structure in one swoop.
	*/

	typedef struct MBLK
	{
		MBLK *		pPrevBlk;         // Points to the previous block
		FLMUINT		uiBlkSize;			// This block's size
		FLMUINT		uiFreeOfs;			// Free offset in the block
		FLMUINT		uiFreeSize;			// Amount of free mem left in block
	} MBLK;


	/*
	Struct: 	POOL_STATS
	Desc:		Structure that is used my "Smart" pools to determine optimal
				block sizes. 
	Note:		When uiAllocBytes reaches wrap size, the current optimal 
  				block size will be determined and uiAllocBytes will be set
				to (optimal block size * 100)and uiCount will be set to 100.
	*/
	typedef struct
	{
		FLMUINT	uiAllocBytes;			// Total number of bytes requested from
												// GedPoolAlloc & GedPoolCalloc calls
		FLMUINT	uiCount;					// Number of Free/Resets performed on 
												// the pool
	} POOL_STATS;

	/*
	Struct:  Pool Memory Manager (PMM)
	Desc:    The PMM is the root structure used to track memory allocations
	*        for GEDCOM.
	*/
	typedef struct
	{
		MBLK *			lblk;
		FLMUINT			uiBlkSize;
		FLMUINT			uiBytesAllocated;
		POOL_STATS *	pPoolStats;
	} PMM, POOL, * PMM_p, * POOL_p;

	void GedPoolInit(
			PMM_p			pmm,
			FLMUINT 		uiBlkSize);

	void * GedPoolCalloc(
		PMM_p 			pmm,
  		FLMUINT			uiSize);

	void * GedPoolAlloc(
		PMM_p				pmm,
		FLMUINT			udSize);

	RCODE GedPoolFree(
		PMM_p				pmm);

	RCODE GedPoolReset(
		PMM_p				pmm,
		void *			ptr);

	void * GedPoolMark(
		PMM_p				pmm);

	/****************************************************************************
	Struct:  CREATE_OPTS       Create Options
	Desc:    This structure is used as a parameter to FlmDbCreate to specify
	*        the create options for a database.  It is returned when calling
	*        FlmDbOpen.
	****************************************************************************/
	typedef struct
	{
		FLMUINT		uiBlockSize;
	#define DEFAULT_BLKSIZ              4096

		FLMUINT		uiVersionNum;
	#define FLM_VER_3_0						301
	#define FLM_VER_3_02						302
	#define FLM_VER_3_10						310
	#define FLM_VER_4_0						400
	#define FLM_VER_4_3						430
	#define FLM_VER_4_31						431	// Added last committed trans ID to
															// the log header
	#define FLM_VER_4_50						450	// Added ability to create cross-
															// container indexes.
	#define FLM_VER_4_51						451	// Added ability to permanently 
															// suspend indexes
	#define FLM_VER_4_52						452	// Added ability to delete indexes
															// in the background
	#define FLM_VER_4_60						460	// Added support for encrypted attributes
	#define FLM_CURRENT_VERSION_NUM		FLM_VER_4_60
	#define FLM_CURRENT_VER_STR			"4.60"

		FLMUINT		uiMinRflFileSize;			// Minimum bytes per RFL file
	#define DEFAULT_MIN_RFL_FILE_SIZE	((FLMUINT)100 * (FLMUINT)1024 * (FLMUINT)1024)
		FLMUINT		uiMaxRflFileSize;			// Maximum bytes per RFL file
	#define DEFAULT_MAX_RFL_FILE_SIZE	F_MAXIMUM_FILE_SIZE
		FLMBOOL		bKeepRflFiles;				// Keep RFL files?
	#define DEFAULT_KEEP_RFL_FILES_FLAG	FALSE
		FLMBOOL		bLogAbortedTransToRfl;	// Log aborted transactions to RFL?
	#define DEFAULT_LOG_ABORTED_TRANS_FLAG	FALSE

		FLMUINT		uiDefaultLanguage;
		FLMUINT		uiAppMajorVer;			// The applications major version number
		FLMUINT		uiAppMinorVer;			// The applications minor version number

	} CREATE_OPTS, * CREATE_OPTS_p;

	/*===========================================================================
	Desc:		This is a pure virtual base class that all other FLAIM classes 
				inherit from.
	===========================================================================*/
	class F_Base
	{
	public:

		F_Base()
		{ 
			m_ui32RefCnt = 1;	
		}

		virtual ~F_Base()
		{
		}

		inline FLMUINT getRefCount( void)
		{
			return( m_ui32RefCnt);
		}

		void * operator new(
			FLMSIZET			uiSize)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new(
			FLMSIZET			uiSize,
			const char *	pszFile,
			int				iLine)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new[](
			FLMSIZET			uiSize)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new[](
			FLMSIZET			uiSize,
			const char *	pszFile,
			int				iLine)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void operator delete(
			void *			ptr);

		void operator delete[](
			void *			ptr);

	#if defined( FLM_DEBUG) && !defined( __WATCOMC__)
		void operator delete(
			void *			ptr,
			char *			file,
			int				line);
	#endif

	#if defined( FLM_DEBUG) && !defined( __WATCOMC__)
		void operator delete[](
			void *			ptr,
			char *			file,
			int				line);
	#endif

		inline FLMUINT AddRef( void)
		{
			m_ui32RefCnt++;
			return m_ui32RefCnt;
		}

		FLMUINT Release( void);

	protected:

		FLMUINT32		m_ui32RefCnt;

	friend class F_FileHdlPage;
	friend class F_FileHdlMgrPage;
	};
/*--------------------------------------------------------
	    User-provided allocator routines for use by FLAIM
**-------------------------------------------------------*/
typedef void * (* FLM_ALLOC_FUNC)(
	FLMUINT				uiSize);

typedef void * (* FLM_REALLOC_FUNC)(
	void *				pvOldPtr,
	FLMUINT				uiNewSize);

typedef void (* FLM_FREE_FUNC)(
	void *				pvPtr);

typedef unsigned int (* FLM_SIZEOF_FUNC)(
	void *				ppvPtr);

typedef void (* FLM_RELOC_FUNC)(
	void *				pvOldAlloc,
	void *				pvNewAlloc);

typedef FLMBOOL (* FLM_CAN_RELOC_FUNC)(
	void *				pvOldAlloc);

typedef struct
{
	FLM_ALLOC_FUNC			fnAlloc;
	FLM_REALLOC_FUNC		fnRealloc;
	FLM_FREE_FUNC			fnFree;
	FLM_SIZEOF_FUNC		fnSizeof;

	FLM_ALLOC_FUNC			fnBlockCacheAlloc;
	FLM_REALLOC_FUNC		fnBlockCacheRealloc;
	FLM_FREE_FUNC			fnBlockCacheFree;
	FLM_SIZEOF_FUNC		fnBlockCacheSizeof;

	FLM_ALLOC_FUNC			fnRecCacheAlloc;
	FLM_REALLOC_FUNC		fnRecCacheRealloc;
	FLM_FREE_FUNC			fnRecCacheFree;
	FLM_SIZEOF_FUNC		fnRecCacheSizeof;

} F_Allocator;

	/*--------------------------------------------------------
			 Name Table Function Structures
	**-------------------------------------------------------*/

	typedef struct
	{
		FLMUNICODE *	puzTagName;
		FLMUINT			uiTagNum;
		FLMUINT			uiType;
		FLMUINT			uiSubType;
	} FLM_TAG_INFO;

	class F_NameTable : public F_Base
	{
	public:

		F_NameTable();

		~F_NameTable();

		void clearTable( void);

		RCODE setupFromDb(
			HFDB	hDb);

		FLMBOOL getNextTagNumOrder(
			FLMUINT *		puiNextPos,
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiNameBufSize,
			FLMUINT *		puiTagNum = NULL,
			FLMUINT *		puiType = NULL,
			FLMUINT *		puiSubType = NULL);

		FLMBOOL getNextTagNameOrder(
			FLMUINT *		puiNextPos,
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiNameBufSize,
			FLMUINT *		puiTagNum = NULL,
			FLMUINT *		puiType = NULL,
			FLMUINT *		puiSubType = NULL);

		FLMBOOL getFromTagType(
			FLMUINT			uiType,
			FLMUINT *		puiNextPos,
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiNameBufSize,
			FLMUINT *		puiTagNum = NULL,
			FLMUINT *		puiSubType = NULL);

		FLMBOOL getFromTagNum(
			FLMUINT			uiTagNum,
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiNameBufSize,
			FLMUINT *		puiType = NULL,
			FLMUINT *		puiSubType = NULL);

		FLMBOOL getFromTagName(
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT *		puiTagNum,
			FLMUINT *		puiType = NULL,
			FLMUINT *		puiSubType = NULL);

		FLMBOOL getFromTagTypeAndName(
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiType,
			FLMUINT *		puiTagNum,
			FLMUINT *		puiSubType = NULL);

		RCODE addTag(
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiTagNum,
			FLMUINT			uiType,
			FLMUINT			uiSubType,
			FLMBOOL			bCheckDuplicates = TRUE);

		void sortTags( void);

	private:

		RCODE allocTag(
			FLMUNICODE *		puzTagName,
			FLMBYTE *			pszTagName,
			FLMUINT				uiTagNum,
			FLMUINT				uiType,
			FLMUINT				uiSubType,
			FLM_TAG_INFO **	ppTagInfo);

		RCODE reallocSortTables(
			FLMUINT	uiNewTblSize);

		void copyTagName(
			FLMUNICODE *	puzDestTagName,
			FLMBYTE *		pszDestTagName,
			FLMUINT			uiDestBufSize,
			FLMUNICODE *	puzSrcTagName);

		FLM_TAG_INFO * findTagByName(
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT *		puiInsertPos = NULL);

		FLM_TAG_INFO * findTagByNum(
			FLMUINT			uiTagNum,
			FLMUINT *		puiInsertPos = NULL);

		FLM_TAG_INFO * findTagByTypeAndName(
			FLMUNICODE *	puzTagName,
			FLMBYTE *		pszTagName,
			FLMUINT			uiType,
			FLMUINT *		puiInsertPos = NULL);

		RCODE insertTagInTables(
			FLM_TAG_INFO *	pTagInfo,
			FLMUINT			uiTagNameTblInsertPos,
			FLMUINT			uiTagTypeAndNameTblInsertPos,
			FLMUINT			uiTagNumTblInsertPos);

		PMM						m_pmm;
		FLM_TAG_INFO **		m_ppSortedByTagName;
		FLM_TAG_INFO **		m_ppSortedByTagNum;
		FLM_TAG_INFO **		m_ppSortedByTagTypeAndName;
		FLMUINT					m_uiTblSize;
		FLMUINT					m_uiNumTags;
		FLMBOOL					m_bTablesSorted;
	};

	/*--------------------------------------------------------
			 Index status and suspend and resume definitions.
	**-------------------------------------------------------*/

	#define RECID_UNDEFINED		0xFFFFFFFF

	typedef struct
	{
		FLMUINT			uiIndexNum;					// Index number
		FLMBOOL			bSuspended;					// Active or suspended
		FLMUINT			uiStartTime;				// Start time of the offline process or zero.
		FLMUINT			uiLastRecordIdIndexed;	// If RECID_UNDEFINED then index is online,
															// otherwise this is the value of the last 
															// record ID (or record ID hole) that was
															// indexed.
		FLMUINT			uiKeysProcessed;			// Keys processed for offline thread.
		FLMUINT			uiRecordsProcessed;		// Records processed for offline thread.
		FLMUINT			uiTransactions;			// Number of transactions started by the
															// indexing thread
	} FINDEX_STATUS;

	typedef enum
	{
		QOPT_NONE = 0,
		QOPT_USING_INDEX,
		QOPT_USING_PREDICATE,
		QOPT_SINGLE_RECORD_READ,
		QOPT_PARTIAL_CONTAINER_SCAN,
		QOPT_FULL_CONTAINER_SCAN
	} qOptTypes;

	// Structure used on GetConfig's FCURSOR_GET_OPT_INFO_LIST option

	typedef struct
	{
		qOptTypes	eOptType;				// Type of optimization done
		FLMUINT		uiCost;					// Cost calculated for sub-query
		FLMUINT		uiDrnCost;				// DRN cost for sub-query.
		FLMUINT		uiIxNum;					// Index used to execute query if
													// eOptType == QOPT_USING_INDEX
		FLMBOOL		bDoRecMatch;			// Record must be retrieved to exe
													// query.  Only valid if eOptType
													// is QOPT_USING_INDEX.
		FLMUINT		bDoKeyMatch;			// Must match against index keys.  Only
													// valid if eOptType == QOPT_USING_INDEX.
		FLMUINT		uiDrn;					// DRN to read if eOptType ==
													// QOPT_SINGLE_RECORD_READ
	} OPT_INFO, * OPT_INFO_p;

	typedef struct
	{
		FLMUINT		uiMaxBytes;
		FLMUINT		uiTotalBytesAllocated;
		FLMUINT		uiCount;
		FLMUINT		uiOldVerCount;
		FLMUINT		uiOldVerBytes;
		FLMUINT		uiCacheHits;
		FLMUINT		uiCacheHitLooks;
		FLMUINT		uiCacheFaults;
		FLMUINT		uiCacheFaultLooks;
	} FLM_CACHE_USAGE;

	typedef struct
	{
		FLMUINT64	ui64TotalExtendedMemory;
		FLMUINT64	ui64RemainingExtendedMemory;
		FLMUINT64	ui64TotalBytesAllocated;
		FLMUINT64	ui64CacheHits;
		FLMUINT64	ui64CacheFaults;
	} FLM_ECACHE_USAGE;

	typedef struct
	{
		FLMBOOL				bDynamicCacheAdjust;
		FLMUINT				uiCacheAdjustPercent;
		FLMUINT				uiCacheAdjustMin;
		FLMUINT				uiCacheAdjustMax;
		FLMUINT				uiCacheAdjustMinToLeave;
		FLMUINT				uiDirtyCount;
		FLMUINT				uiDirtyBytes;
		FLMUINT				uiNewCount;
		FLMUINT				uiNewBytes;
		FLMUINT				uiLogCount;
		FLMUINT				uiLogBytes;
		FLMUINT				uiFreeBytes;
		FLMUINT				uiFreeCount;
		FLMUINT				uiReplaceableCount;
		FLMUINT				uiReplaceableBytes;
		FLM_CACHE_USAGE	RecordCache;
		FLM_CACHE_USAGE	BlockCache;
		FLM_ECACHE_USAGE	ECache;
	} FLM_MEM_INFO;

	typedef struct
	{
		FLMUINT		uiThreadId;
		FLMUINT		uiThreadGroup;
		FLMUINT		uiAppId;
		FLMUINT		uiStartTime;
		FLMBYTE *	pszThreadName;
		FLMBYTE *	pszThreadStatus;
	} F_THREAD_INFO;

	typedef struct
	{
		FLMUINT		uiThreadId;		// Thread Id
		HFDB			hDb;				// Database handle
		FLMUINT		uiTransID;		// Transaction ID
		RCODE			rc;				// Return code
	} FLM_TRANS_EVENT, * FLM_TRANS_EVENT_p;

	typedef struct
	{
		FLMUINT		uiThreadId;		// Thread Id
		HFDB			hDb;				// Database handle
		FLMUINT		uiTransID;		// Transaction ID
		RCODE			rc;				// Return code
		FLMUINT		uiDrn;			// DRN
		FLMUINT		uiContainer;	// Container ID
		FlmRecord *	pNewRecord;		// New record (adds, modifies)
		FlmRecord *	pOldRecord;		// Old record (modifies, deletes)
	} FLM_UPDATE_EVENT, * FLM_UPDATE_EVENT_p;

	typedef struct
	{
		FLMBOOL		bRunning;
		FLMUINT		uiRunningTime;
		FLMBOOL		bForcingCheckpoint;
		FLMUINT		uiForceCheckpointRunningTime;
		FLMINT		iForceCheckpointReason;
			#define			CP_TIME_INTERVAL_REASON				1
			#define			CP_SHUTTING_DOWN_REASON				3
			#define			CP_RFL_VOLUME_PROBLEM				4
		FLMBOOL		bWritingDataBlocks;
		FLMUINT		uiLogBlocksWritten;
		FLMUINT		uiDataBlocksWritten;
		FLMUINT		uiDirtyCacheBytes;
		FLMUINT		uiBlockSize;
		FLMUINT		uiWaitTruncateTime;
	} CHECKPOINT_INFO, * CHECKPOINT_INFO_p;

	typedef struct
	{
		FLMUINT		uiThreadId;
		FLMUINT		uiTime;				// For lock holder, this is the
												// time the lock was obtained.
												// For the lock waiter, this is the
												// time he started waiting for the lock.
	} LOCK_USER, * LOCK_USER_p;

	typedef struct
	{
		HFDB        hDb;              // Current Database
		FLMUINT     uiRecId;          // DRN of the current record.
		FLMUINT     uiContainer;      // Current Container Number
		FlmRecord * pRecord;          // Current Record 
		void *		pvField;				// Current field within the record
	} SWEEP_INFO, * SWEEP_INFO_p;

	typedef struct
	{
		HFDB     hDb;							// Database handle.
		FLMUINT  uiContainerNum;			// Container number.
		FLMUINT  uiIndexNum;					// Index being used. Zero if none.
		FLMUINT  uiProcessedCnt;			// Items processed
		FLMUINT  uiMatchedCnt;				// Records that passed criteria.
		FLMUINT	uiNumRejectedByCallback;// Records rejected by callback.
		FLMUINT	uiDupsEliminated;			// Duplicates eliminated.
		FLMUINT	uiKeysTraversed;			// Keys traversed
		FLMUINT	uiKeysRejected;			// Keys rejected
		FLMUINT	uiRefsTraversed;			// References traversed.
		FLMUINT	uiRefsRejected;			// References rejected.
		FLMUINT	uiRecsFetchedForEval;	// Records fetched
		FLMUINT	uiRecsRejected;			// Records rejected
		FLMUINT	uiRecsFetchedForView;	// Records retrieved after key passes.
		FLMUINT	uiRecsNotFound;			// Records not found.
	} FCURSOR_SUBQUERY_STATUS;

	/*===========================================================================
								Defines for FLAIM Functions

	Desc: Used by FLAIM's processing hooks to determine severity of error conditions.
	===========================================================================*/
	typedef enum
	{
		FLM_UNKNOWN_FUNC,
		FLM_BLOB_CREATE_REFERENCE,
		FLM_BLOB_READ,
		FLM_BLOB_SEEK,
		FLM_BLOB_EXPORT,
		FLM_BLOB_IMPORT,
		FLM_BLOB_PURGE,

		FLM_CURSOR_COMPARE_DRNS,
		FLM_CURSOR_CONFIG,
		FLM_CURSOR_FIRST,
		FLM_CURSOR_FIRST_DRN,
		FLM_CURSOR_GET_CONFIG,
		FLM_CURSOR_LAST,
		FLM_CURSOR_LAST_DRN,
		FLM_CURSOR_MOVE_RELATIVE,
		FLM_CURSOR_NEXT,
		FLM_CURSOR_NEXT_DRN,
		FLM_CURSOR_PREV,
		FLM_CURSOR_PREV_DRN,
		FLM_CURSOR_REC_COUNT,
		FLM_CURSOR_SET_INDEX,
		FLM_CURSOR_SET_KEY_RANGE,
		FLM_CURSOR_VALIDATE,
		FLM_CURSOR_INIT,
		FLM_CURSOR_FREE,

		FLM_DB_CHECK,
		FLM_DB_CHECKPOINT,
		FLM_DB_CLOSE,
		FLM_DB_UPGRADE,
		FLM_DB_CREATE,
		FLM_DB_GET_COMMIT_CNT,
		FLM_DB_GET_CONFIG,
		FLM_DB_GET_LOCK_INFO,
		FLM_DB_GET_LOCK_TYPE,
		FLM_DB_GET_TRANS_ID,
		FLM_DB_GET_TRANS_TYPE,
		FLM_DB_LOCK,
		FLM_DB_OPEN,
		FLM_DB_REDUCE_SIZE,
		FLM_DB_SWEEP,
		FLM_DB_TRANS_ABORT,
		FLM_DB_TRANS_BEGIN,
		FLM_DB_TRANS_COMMIT,
		FLM_DB_UNLOCK,

		FLM_GET_ITEM_ID,
		FLM_INDEX_GET_NEXT,
		FLM_INDEX_STATUS,			// Cannot name FLM_INDEX_STATUS
		FLM_INDEX_RESUME,
		FLM_INDEX_SUSPEND,

		FLM_KEY_RETRIEVE,
		FLM_RESERVE_NEXT_DRN,
		FLM_RECORD_ADD,
		FLM_RECORD_MODIFY,
		FLM_RECORD_DELETE,
		FLM_RECORD_RETRIEVE,

		FLM_DB_REMOVE,
		FLM_DB_LOGHDR,			// Used to display File Log Header in a closed file

		// Always insert new funcs before LAST_FLM_FUNC 
		LAST_FLM_FUNC                 
	} eFlmFuncs;

	typedef struct
	{
		HFDB			hDb;				// Handle to current record's db
		FLMUINT		uiRecId;			// Database DRN for current record
		NODE_p		pRec;				// Record
		FLMUINT		uiContainerId;	// Database Container for current record
	} REC_CONTEXT;

	typedef struct
	{
		HFBLOB		hBlob;		// Handle to the BLOB.
		REC_CONTEXT	RecContext;	// Includes dwRecId, wContainerId
		eFlmFuncs	FlmFunction;// The FLAIM function value.
	} BLOB_CONTEXT;

	typedef struct
	{
		FLMUINT64		ui64BytesToCopy;
		FLMUINT64		ui64BytesCopied;
		FLMBOOL			bNewSrcFile;
		FLMBYTE			szSrcFileName[ F_PATH_MAX_SIZE];
		FLMBYTE			szDestFileName[ F_PATH_MAX_SIZE]; 
	} DB_COPY_INFO, * DB_COPY_INFO_p;

	typedef struct
	{
		FlmRecord *		pRecord;			// Record to examine to see if it contains
												// dictionary information.
		FLMUINT			uiContainer;	// Container the record came from.
		FLMUINT			uiDrn;			// DRN of record.
		FlmRecordSet *	pDictRecSet;	// Set of dictionary records returned.
	} CHK_RECORD, * CHK_RECORD_p;

	typedef struct
	{
		FLMUINT		uiDrn;			// Current DRN being processed
		FLMUINT		uiLastDrn;		// Highest DRN in this container
		FLMUINT		uiContainer;	// Container being processed
	} DB_CONVERT_INFO, * DB_CONVERT_INFO_p;

	typedef struct
	{
		FLMUINT64		ui64BytesToDo;
		FLMUINT64		ui64BytesDone;
	} DB_BACKUP_INFO, * DB_BACKUP_INFO_p;

	typedef struct
	{
		FLMBYTE			szSrcFileName [F_PATH_MAX_SIZE];
		FLMBYTE			szDstFileName [F_PATH_MAX_SIZE];
	} DB_RENAME_INFO, * DB_RENAME_INFO_p;

	typedef enum
	{
		FLM_LOCK_NONE,
		FLM_LOCK_EXCLUSIVE,
		FLM_LOCK_SHARED
	} FLOCK_TYPE;

	typedef struct
	{
		FLOCK_TYPE	eCurrLockType;			// Current lock type (FLM_LOCK_NONE,
													// FLM_LOCK_EXCLUSIVE, or FLM_LOCK_SHARED)
		FLMUINT		uiThreadId;				// Thread ID of thread that has the lock,
													// if lock is an exclusive lock.
		FLMUINT		uiNumExclQueued;		// Number of pending exclusive locks.
		FLMUINT		uiNumSharedQueued;	// Number of pending shared locks.
		FLMUINT		uiPriorityCount;		// Number of locks queued with priority
													// >= the requested priority - see
													// FlmDbGetLockInfo.
	} FLOCK_INFO;

	typedef struct
	{
		FLMUINT64		ui64BytesToDo;
		FLMUINT64		ui64BytesDone;
	} BYTE_PROGRESS;

	typedef struct node
	{
		NODE_p      next;					// child, next sib, or uncle (compare levels)
		NODE_p      prior;            // parent, prior sib, or nephew (compare levels)
		FLMBYTE *   value;
		FLMUINT     uiLength;			// true len of value (exclusive of end NULL)
		FLMUINT16   ui16TagNum;			// Tag number (short version of DRN)
		FLMUINT8    ui8Level;			// hierarchy level (0 = root)
		FLMUINT8		ui8Type;				// value's type defined below

		// Define the allowable field types

		#define FLM_TEXT_TYPE			0
		#define FLM_NUMBER_TYPE			1
		#define FLM_BINARY_TYPE			2
		#define FLM_CONTEXT_TYPE		3
		#define FLM_REAL_TYPE			4		// float (SREAL) or double (LREAL)
		#define FLM_DATE_TYPE			5		// Date: year, month, day in month
		#define FLM_TIME_TYPE			6		// Time: hour, minute, second, hundredth
		#define FLM_TMSTAMP_TYPE		7		// Time stamp
		#define FLM_BLOB_TYPE			8		// Blob type - internal or external

		#define FLM_NUM_OF_TYPES		9		// Number of field types supported

		#define FLM_DEFAULT_TYPE	FLM_CONTEXT_TYPE

		#define FLM_DATA_LEFT_TRUNCATED	0x10	// Data is left truncated
		#define FLM_DATA_RIGHT_TRUNCATED	0x20	// Data is right truncated.
		#define HAS_REC_SOURCE     0x40			// Node has HFDB, container, and
															// RecId immediatly following the node.
		#define HAS_REC_ID         0x80			// Node has Record Id (4 bytes) immediately
															// following the node.
		FLMUINT		uiEncFlags;						// Encryption flags
		FLMUINT		uiEncLength;					// The length of the encrypted data
		FLMUINT		uiEncId;							// The DRN of the encryption record
		FLMBYTE *	pucEncValue;					// The encrypted value
	} NODE;

	typedef struct Rec_Key
	{
		FlmRecord *	pKey;
		Rec_Key *	pNextKey;
	} REC_KEY;

	FLMUINT FlmLanguage(
		FLMBYTE  *	pLanguageCode);

	void FlmGetLanguage(
		FLMINT		iLangNum,
		FLMBYTE  *	pLanguageCode);

	/*--------------------------------------------------------
			 FLAIM Record stuff
	**-------------------------------------------------------*/

	#define FLM_MAX_FIELD_VAL_SIZE			((FLMUINT)65535)

	/*============================================================================
	Class: 	FlmRecordSet
	Desc: 	This class allows a user to keep a set of FlmRecords.
	============================================================================*/
	class FlmRecordSet : public F_Base
	{
	public:

		FlmRecordSet()
		{
			m_iCurrRec = -1;
			m_ppRecArray = NULL;
			m_iRecArraySize = 0;
			m_iTotalRecs = 0;
		}

		~FlmRecordSet();

		// Insert a FlmRecord into the set.
		RCODE insert( FlmRecord * pRecord);

		// Clear all records in the set
		void clear( void);

		// Methods for navigating the set
		FlmRecord * first( void)
		{
			if (!m_iTotalRecs)
			{
				return( NULL);
			}
			m_iCurrRec = 0;
			return( m_ppRecArray [0]);
		}

		FlmRecord * last( void)
		{
			if (!m_iTotalRecs)
			{
				return( NULL);
			}
			m_iCurrRec = m_iTotalRecs - 1;
			return( m_ppRecArray [m_iCurrRec]);
		}

		FlmRecord * next( void);

		FlmRecord * prev( void)
		{
			if (m_iCurrRec - 1 < 0)
			{
				m_iCurrRec = -1;
				return( NULL);
			}
			m_iCurrRec--;
			return( m_ppRecArray [m_iCurrRec]);
		}

		FLMINT count( void)
		{	
			return m_iTotalRecs;
		}

	private:

		FLMINT			m_iCurrRec;
		FlmRecord **	m_ppRecArray;
		FLMINT			m_iRecArraySize;
		FLMINT			m_iTotalRecs;
	};

	/*============================================================================
	Class: 	FlmUserPredicate
	Desc: 	Abstract base class which provides the interface that
				FLAIM uses to allow an application to embed predicates
				in a query.
	============================================================================*/
	class FlmUserPredicate : public F_Base
	{
	public:

		// Method that returns the search cost of this object in providing
		// records for a query.

		virtual RCODE searchCost(
			HFDB			hDb,			// Handle to database - DO NOT REMEMBER - could
											// change from one call to the next!
			FLMBOOL		bNotted,		// Flag indicating predicate has been notted.
			FLMBOOL		bExistential,// Flag indicating predicate is "existential" if
											// TRUE, "universal" if FALSE.
			FLMUINT *	puiCost,		// cost is returned here
			FLMUINT *	puiDrnCost,	// DRN cost is returned here
			FLMUINT *	puiTestRecordCost,// Test record cost is returned here.
			FLMBOOL *	pbPassesEmptyRec
			) = 0;

		// Method that returns the cost of testing ALL record.

		virtual RCODE testAllRecordCost(
			HFDB			hDb,			// Handle to database
			FLMUINT *	puiCost		// cost is returned here
			) = 0;

		// Position to and return the first record that satisfies the predicate.

		virtual RCODE firstRecord(
			HFDB				hDb,		// Handle to database - DO NOT REMEMBER - could
											// change from one call to the next!
			FLMUINT *		puiDrn,	// Returns record's DRN if non-NULL.
			FlmRecord **	ppRecord	// Returns pointer to record if non-NULL.
			) = 0;

		// Position to and return the last record that satisfies the predicate.

		virtual RCODE lastRecord(
			HFDB				hDb,		// Handle to database - DO NOT REMEMBER - could
											// change from one call to the next!
			FLMUINT *		puiDrn,	// Returns record's DRN if non-NULL.
			FlmRecord **	ppRecord	// Returns pointer to record if non-NULL.
			) = 0;

		// Position to and return the next record that satisfies the predicate.
		// If no prior positioning has been done,
		// position to and return the first record.

		virtual RCODE nextRecord(
			HFDB				hDb,		// Handle to database - DO NOT REMEMBER - could
											// change from one call to the next!
			FLMUINT *		puiDrn,	// Returns record's DRN if non-NULL.
			FlmRecord **	ppRecord	// Returns pointer to record if non-NULL.
			) = 0;

		// Position to and return the previous record that satisfies the predicate.
		// If no prior positioning has been done,
		// position to and return the last record.

		virtual RCODE prevRecord(
			HFDB				hDb,		// Handle to database - DO NOT REMEMBER - could
											// change from one call to the next!
			FLMUINT *		puiDrn,	// Returns record's DRN if non-NULL.
			FlmRecord **	ppRecord	// Returns pointer to record if non-NULL.
			) = 0;

		// Test a record to see if it passes the criteria of the predicate.

		virtual RCODE testRecord(
			HFDB			hDb,				// Handle to database - DO NOT REMEMBER.
												// Could change from one call to the next!
			FlmRecord *	pRecord,			// Pointer to record to be tested.
			FLMUINT		uiDrn,			// DRN of record to be tested.
			FLMUINT *	puiResult		// Result is returned here.  FLM_FALSE should
												// be returned if predicate is false, FLM_TRUE
												// unknown.
			) = 0;

		// Return index being used, 0 if none.

		virtual FLMUINT getIndex( 
			FLMUINT *	puiIndexInfo) = 0;

		// Return a copy of the object.  Result set should be
		// emptied, score should be unset - only the predicate
		// should be preserved.
		// Returns NULL if the copy fails.

		virtual FlmUserPredicate * copy( void) = 0;

		// Return predicate's FLAIM cursor handle, if any.  NOTE: This
		// MUST return a non-null cursor if canPosByPredicate returns
		// TRUE!  It does not have to if it returns FALSE.

		virtual HFCURSOR getCursor( void) = 0;

		// Position this predicate to the same position as
		// another predicate.

		virtual RCODE positionTo(
			HFDB						hDb,
			FlmUserPredicate *	pPredicate) = 0;

		// Save current position of predicate.

		virtual RCODE savePosition( void) = 0;

		// Restore last saved position of predicate.

		virtual RCODE restorePosition( void) = 0;

		// Determine if predicate is absolute positionable

		virtual RCODE isAbsPositionable(
			HFDB			hDb,
			FLMBOOL *	pbIsAbsPositionable) = 0;

		// Get absolute position count

		virtual RCODE getAbsCount(
			HFDB			hDb,
			FLMUINT *	puiCount) = 0;

		// Get absolute position

		virtual RCODE getAbsPosition(
			HFDB			hDb,
			FLMUINT *	puiPosition) = 0;

		// Set absolute position

		virtual RCODE positionToAbs(
			HFDB			hDb,
			FLMUINT		uiPosition,
			FLMBOOL		bFallForward,
			FLMUINT		uiTimeLimit,
			FLMUINT *	puiPosition,
			FLMUINT *	puiDrn) = 0;

		virtual void releaseResources( void) = 0;
	};

	/*===========================================================================
						Cursor Typedefs and Function Prototypes
	===========================================================================*/

	#define FLM_FALSE			(1)
	#define FLM_TRUE			(2)
	#define FLM_UNK			(4)

	typedef enum
	{
		NO_TYPE = 0,				// 0 (internal use only)

	// WARNING: Don't renumber below _VAL enums without 
	// redoing gv_DoValAndDictTypesMatch table

		FLM_BOOL_VAL = 1,			// 1
		FLM_UINT32_VAL,			// 2 
		FLM_INT32_VAL,				// 3
		FLM_REAL_VAL,				// 4
		FLM_REC_PTR_VAL,			// 5
		FLM_TIME_VAL,				// 6
		FLM_DATE_VAL,				// 7
		FLM_TMSTAMP_VAL,			// 8
		FLM_BINARY_VAL,			// 9
		FLM_STRING_VAL,			// 10	
		FLM_UNICODE_VAL,			// 11
		FLM_TEXT_VAL,				//	12 (internal FLAIM text type)

			// Enums for internal use
			FIRST_VALUE = FLM_BOOL_VAL,	
			LAST_VALUE = FLM_TEXT_VAL,	

		FLM_FLD_PATH = 25,		// 25
		FLM_CB_FLD,					// 26

		// NOTE: These operators MUST stay in this order - this order is assumed
		// by the precedence table - see fqstack.cpp

		FLM_AND_OP = 100,			// 100
		FLM_OR_OP,					// 101
		FLM_NOT_OP,					// 102
		FLM_EQ_OP,					// 103
		FLM_MATCH_OP,				// 104
		FLM_MATCH_BEGIN_OP,		// 105
		FLM_MATCH_END_OP,			// 106
		FLM_CONTAINS_OP,			// 107
		FLM_NE_OP,					// 108
		FLM_LT_OP,					// 109
		FLM_LE_OP,					// 110
		FLM_GT_OP,					// 111
		FLM_GE_OP,					// 112
		FLM_BITAND_OP,				// 113
		FLM_BITOR_OP,				// 114
		FLM_BITXOR_OP,				// 115
		FLM_MULT_OP,				// 116
		FLM_DIV_OP,					// 117
		FLM_MOD_OP,					// 118
		FLM_PLUS_OP,				// 119
		FLM_MINUS_OP,				// 120
		FLM_NEG_OP,					// 121
		FLM_LPAREN_OP,				// 122
		FLM_RPAREN_OP,				// 123
		FLM_UNKNOWN,	 			// 124
		FLM_USER_PREDICATE,		// 125
		FLM_EXISTS_OP,				// 126 For internal use only - key generation

			// Enums for internal use
			FIRST_OP				= FLM_AND_OP,
			FIRST_LOG_OP		= FIRST_OP,
			LAST_LOG_OP			= FLM_NOT_OP,
			FIRST_COMPARE_OP	= FLM_EQ_OP,
			LAST_COMPARE_OP	= FLM_GE_OP,
			FIRST_ARITH_OP		= FLM_BITAND_OP,
			LAST_ARITH_OP		= FLM_MINUS_OP,
			LAST_OP				= LAST_ARITH_OP

	} QTYPES;


	/*--------------------------------------------------------
				Cursor Function Prototypes
	**-------------------------------------------------------*/

	RCODE FlmParseQuery(
		HFCURSOR			hCursor,
		F_NameTable *	pNameTable,
		FLMBYTE *		pszQueryCriteria);

	RCODE FlmCursorClone(
		HFCURSOR		hSource,
		HFCURSOR *  phCursor);

	RCODE FlmCursorConfig(
		HFCURSOR    hCursor,
		FLMUINT     uiType,
		void *	   Value1,
		void *	   Value2);

		// Predefined values for cursor config types

		#define FCURSOR_CLEAR_QUERY			2
		#define FCURSOR_GEN_POS_KEYS			3
		#define FCURSOR_SET_HDB					4
		#define FCURSOR_SET_FLM_IX				5
		#define FCURSOR_SET_OP_TIME_LIMIT	6
		#define FCURSOR_SET_PERCENT_POS		7
		#define FCURSOR_SET_POS					8
		#define FCURSOR_SET_POS_FROM_DRN		9
		#define FCURSOR_SET_REC_TYPE			10
		#define FCURSOR_RETURN_KEYS_OK		11
		#define FCURSOR_NO_INVISIBLE_TRANS	12
		#define FCURSOR_DO_INVISIBLE_TRANS	13
		#define FCURSOR_DISCONNECT				14
		#define FCURSOR_ALLOW_DUPS				15
		#define FCURSOR_ELIMINATE_DUPS		16
		#define FCURSOR_SET_REC_VALIDATOR	17
		#define FCURSOR_SET_STATUS_HOOK		18
		#define FCURSOR_SAVE_POSITION			19
		#define FCURSOR_RESTORE_POSITION		20
		#define FCURSOR_SET_ABS_POS			21

		// Predefined values for uiIxNum, on FCURSOR_SET_FLM_IX

		#define FLM_WEIGH_THE_INDEXES			FLM_WILD_TAG
		#define FLM_SELECT_INDEX            FLM_WEIGH_THE_INDEXES
		#define FLM_NO_INDEX                0

	RCODE FlmCursorGetConfig(
		HFCURSOR    hCursor,
		FLMUINT     uiType,
		void *	   Value1,
		void *	   Value2);

		// Predefined values for get config types
		#define FCURSOR_GET_OPT_INFO_LIST		3
		#define FCURSOR_GET_FLM_IX					4
			#define HAVE_NO_INDEX					0
			#define HAVE_ONE_INDEX					1
			#define HAVE_ONE_INDEX_MULT_PARTS	2
			#define HAVE_MULTIPLE_INDEXES			3
		#define FCURSOR_GET_OPT_INFO				5
		#define FCURSOR_GET_PERCENT_POS			6
		#define FCURSOR_GET_REC_TYPE				9
		#define FCURSOR_GET_FLAGS					12
		#define FCURSOR_GET_STATE					13
		#define FCURSOR_GET_POSITIONABLE			14
		#define FCURSOR_AT_BOF						15
		#define FCURSOR_AT_EOF						16
		#define FCURSOR_GET_ABS_POSITIONABLE	17
		#define FCURSOR_GET_ABS_POS				18
		#define FCURSOR_GET_ABS_COUNT				19

		// Predefined values for optimization info (FCURSOR_GET_OPT_INFO)

		#define FCURSOR_USING_FROM_KEY		0x01
		#define FCURSOR_USING_UNTIL_KEY		0x02
		#define FCURSOR_USING_MULT_KEYS		0x04

		// Predefined values for query state (FCURSOR_GET_STATE)

		#define FCURSOR_HAVE_CRITERIA			0x01
		#define FCURSOR_EXPECTING_OPERATOR	0x02
		#define FCURSOR_QUERY_COMPLETE		0x04
		#define FCURSOR_QUERY_OPTIMIZED		0x08
		#define FCURSOR_READ_PERFORMED		0x10

	RCODE FlmCursorSetOrderIndex(
		HFCURSOR		hCursor,
		FLMUINT *	puiFieldPaths,		
		FLMUINT *	puiIndexRV);

	RCODE   FlmCursorSetMode(
		HFCURSOR    hCursor,
		FLMUINT		uiFlags);

		// Predefined values for text comparison modes

		#define FLM_NOWILD         0x01
		#define FLM_WILD           0x02
		#define FLM_NOCASE         0x04
		#define FLM_CASE           0x08

		// Predefined values for wFlags (see FlmCursorAddField, etc.)

		#define FLM_USE_DEFAULT_VALUE	0x20
		#define FLM_SINGLE_VALUED		0x40
			
		// Predefined values for text conversions

		#define FLM_NO_SPACE			0x1000
		#define FLM_NO_DASH			0x2000
		#define FLM_NO_UNDERSCORE	0x4000
		#define FLM_MIN_SPACES		0x8000

	RCODE FlmCursorCleanup(
		HFCURSOR *	phCursor);

	void FlmCursorReleaseResources(
		HFCURSOR	hCursor);

	RCODE FlmCursorFree(
		HFCURSOR *	phCursor);

	RCODE FlmCursorInit(
		HFDB        hDb,
		FLMUINT     uiContId,
		HFCURSOR *  phCursor);

	RCODE FlmCursorValidate(
		HFCURSOR    hCursor);

	RCODE FlmCursorCurrent(
		HFCURSOR    hCursor,
		FlmRecord **ppRecord);

	RCODE FlmCursorCurrentDRN(
		HFCURSOR		hCursor,
		FLMUINT *	pDrn);

	RCODE FlmCursorMoveRelative(
		HFCURSOR			hCursor,
		FLMINT *			piPosition,
		FlmRecord **	ppRecNode);

	RCODE FlmCursorRecCount(
		HFCURSOR		hCursor,
		FLMUINT *	puiCount);

	RCODE FlmCursorCompareDRNs(
		HFCURSOR		hCursor,
		FLMUINT		uiDRN1,
		FLMUINT		uiDRN2,
		FLMUINT		uiTimeLimit,
		FLMINT *		piCmpResultRV,
		FLMBOOL *	pbTimedOutRV,
		FLMUINT *	puiCountRV);
			
	RCODE FlmCursorTestRec(
		HFCURSOR    hCursor,
		FlmRecord * pRecord,
		FLMBOOL *	pbIsMatch);

	RCODE FlmCursorTestDRN(
		HFCURSOR    hCursor,
		FLMUINT     uiDRN,
		FLMBOOL *	pbIsMatch);

	// Predefined value for no time limit

	#define		FLM_NO_LIMIT		0xFFFF
			
	RCODE FlmCursorAddField(
		HFCURSOR		hCursor,
		FLMUINT		uiFldId,
		FLMUINT		uiFlags);
			
	// Predefined values for special fields

	#define FLM_RECID_FIELD    FLM_DRN_TAG
	#define FLM_ANY_FIELD      FLM_WILD_TAG

	RCODE FlmCursorAddFieldPath(
		HFCURSOR			hCursor,
		FLMUINT *		psFldPath,
		FLMUINT			uiFlags);
			
	RCODE FlmCursorAddUserPredicate(
		HFCURSOR					hCursor,
		FlmUserPredicate *	pPredicate);

	typedef RCODE ( * CURSOR_GET_FIELD_CB)(
		void *			pvUserData,						
		FlmRecord *		pRecord,
		HFDB				hDb,
		FLMUINT *		puiFldPath,
		FLMUINT			uiAction,
			#define FLM_FLD_FIRST			1
			#define FLM_FLD_NEXT				2
			#define FLM_FLD_CLEANUP			3
			#define FLM_FLD_VALIDATE		4
			#define FLM_FLD_RESET			5
		FlmRecord **	ppFieldRecRV,
		void **			ppFieldRV,
		FLMUINT *		puiResult);

	RCODE FlmCursorAddFieldCB(
		HFCURSOR			hCursor,
		FLMUINT *		puiFldPath,
		FLMUINT			uiFlags,
		FLMBOOL			bValidateOnly,
		CURSOR_GET_FIELD_CB	fnGetField,
		void *			pvUserData,
		FLMUINT			uiUserDataLen);

	RCODE FlmCursorAddOp(
		HFCURSOR			hCursor,
		QTYPES			eOperator,
		FLMUINT			uiFlags = 0);
		
	RCODE FlmCursorAddValue(
		HFCURSOR			hCursor,
		QTYPES			eValType,
		void *			pVal,
		FLMUINT			uiValLen);

	/*--------------------------------------------------------
			 Initialization and Termination Routines.
	**-------------------------------------------------------*/

	RCODE FlmStartup(
		F_Allocator *	pAllocator = NULL);

	void FlmShutdown( void);

	typedef enum
	{
		FLM_CLOSE_UNUSED_FILES,		// FlmConfig.  Close all files that have not
											// been used for the specified number of
											// seconds.
											//		Value1 (FLMUINT)	Seconds
		FLM_CLOSE_ALL_FILES,			// FlmConfig.  Close all available file handles
											// as well as all used files as as they are
											// available.  Files opened after this call
											// will not be immediately closed after use.
		FLM_OPEN_THRESHOLD,			// FlmConfig. Maximum number of file handles
											// available.
											//		Value1 (FLMUINT)	Maximum
											// FlmGetConfig. Returns threshhold.
											//		Value1 (FLMUINT *)
		FLM_OPEN_FILES,				// FlmGetConfig.  Returns number of open files.
											//		Value1 (FLMUINT *)
		FLM_CACHE_LIMIT,				// FlmConfig.  Set maximum cache size in bytes.
											//		Value1 (FLMUINT)	Cache size.
		FLM_SCACHE_DEBUG,				// FlmConfig.  Enable/disable cache debugging.
											//		Value1 (FLMBOOL)	TRUE=enable, FALSE=disable
		FLM_START_STATS,				// FlmConfig. Start gathering statistics.
		FLM_STOP_STATS,				// FlmConfig. Stop gathering statistics.
		FLM_RESET_STATS,				// FlmConfig. Reset statistics.
		FLM_TMPDIR,						// FlmConfig. Set temporary directory.
											//		Value1 (FLMBYTE *)	Directory name.
		FLM_SANITYLEVEL,				// FlmConfig. Set sanity checking level.
											//		Value1 (FLMUINT)	Check level, must be:
											//			FLM_NO_CHECK,
											//			FLM_BASIC_CHECK, 
											//			FLM_INTERMEDIATE_CHECK,
											//			FLM_EXTENSIVE_CHECK.
											// FlmGetConfig.  Get sanity checking level.
											//		Value1 (FLMUINT *)
		FLM_MAX_CP_INTERVAL,			// FlmConfig. Set maximum seconds between
											// checkpoints.
											//		Value1 (FLMUINT)	Seconds
											// FlmGetConfig. Returns maximum seconds.
											//		Value1 (FLMUINT *)
		FLM_BLOB_EXT,					// FlmConfig. Set BLOB override file extension.
											//		Value1 (FLMBYTE *)	File extension.  NULL
											//									or empty string
											//									disables override.
											// FlmGetConfig. Returns current override value.
											//		Value1 (FLMBYTE *)	Should point to at
											//									lease a 4 byte buffer.
		FLM_MAX_TRANS_SECS,			// FlmConfig.  Set maximum transaction time limit.
											// Used to determine whether a transaction should
											// be killed.
											//		Value1 (FLMUINT)	Seconds
											// FlmGetConfig. Returns seconds.
											//		Value1 (FLMUINT *)
		FLM_MAX_TRANS_INACTIVE_SECS,
											// FlmConfig. Set maximum time a transaction can
											// be inactive before it will be killed.
											//		Value1 (FLMUINT)	Seconds
											// FlmGetConfig.  Returns seconds.
											//		Value1 (FLMUINT *)
		FLM_CACHE_ADJUST_INTERVAL,	// FlmConfig. Set interval for dynamically
											// adjusting cache limit.
											//		Value1 (FLMUINT)	Seconds.
											// FlmGetConfig.  Returns interval.
											//		Value1 (FLMUINT *)
		FLM_CACHE_CLEANUP_INTERVAL,// FlmConfig.l Set interval for dynamically
											// cleaning out old cache blocks and records.
											//		Value1 (FLMUINT)	Seconds
											// FlmGetConfig.  Returns interval.
											//		Value1 (FLMUINT *)
		FLM_UNUSED_CLEANUP_INTERVAL,
											// FlmConfig. Set interval for cleaning up
											// unused structures.
											//		Value1 (FLMUINT)	Seconds
											// FlmGetConfig.  Returns interval.
											//		Value1 (FLMUINT *)
		FLM_MAX_UNUSED_TIME,			// FlmConfig.  Set maximum time for an item
											// to be unused.
											//		Value1 (FLMUINT)	Seconds
											//	FlmGetConfig.  Returns maximum unused time.
											//		Value1 (FLMUINT *)
		FLM_BLOCK_CACHE_PERCENTAGE,// FlmConfig.  Percentage of cache to be
											// allocated to the block cache.
											//		Value1 (FLMUINT)	Percent (0 to 100)
											// FlmGetConfig.  Returns percentage.
											//		Value1 (FLMUINT *)
		FLM_OUT_OF_MEM_SIMULATION,	// FlmConfig - Enable/Disable out-of-memory
											// simulation.
											//		Value1	(FLMUINT)	Zero=Disable
											//									Non-zero=Enable
											// FlmGetConfig.  Return simulation flag.
											//		Value1	(FLMBOOL *)
		FLM_CACHE_CHECK,				// FlmConfig - Enable or disable cache checking.
											//		Value1	(FLMUINT)	Zero=Disable
											//									Non-zero=Enable
											// FlmGetConfig. Return cache checking flag.
											//		Value1	(FLMBOOL *)
		FLM_CLOSE_FILE,				// FlmConfig - Close a DB file.
											//		Value1	(FLMBYTE *)	pszDbFileName
		FLM_LOGGER,						// FlmConfig - Specify the logger object.
											//		Value1	(F_Logger *) pLogger
		FLM_USE_ESM,					// FlmConfig - Enable or disable use of ESM
											// (Extended Server Memory)
											//		Value1	(FLMUINT)	Zero=Disable
											//									Non-zero=Enable
											// FlmGetConfig. Return ESM mode.
											//		Value1	(FLMBOOL *)
		FLM_ASSIGN_HTTP_SYMS,		// Assigns all the function pointers in the
											// gv_FlmSysData.Httpparms struct.
											//		Value1	HTTPCONFIGPARAMS  * A pointer to
											// the struct containing all the needed function
											//	pointers.
											//		Value2	N/A
		FLM_UNASSIGN_HTTP_SYMS,		// Set all of the http function pointers to NULL
											//		Value1	N/A
											//		Value2	N/A
		FLM_REGISTER_HTTP_URL,		// Tell the HTTP server that we're ready
											//	to service requests for for some
											// set of URL's
											// Value1	FLMBYTE *	'Top-level' name for
											// the sub-tree that this callback will
											// service (ex: coredb)
											//	Value2	N/A
		FLM_DEREGISTER_HTTP_URL,	// Tell the HTTP server to stop calling
											//	our callback function when a request
											// for the specified URL comes in.
											// Value1	FLMBYTE *	'Top-level' name for
											// the sub-tree that we don't want to service
											// anymore (ex: coredb)
											//	Value2	N/A
		FLM_KILL_DB_HANDLES,			// Invalidate open database handles, forcing
											// the database to (eventually) be closed.
											// Value1	FLMBYTE *	Database path
											// Value2	FLMBYTE *	Data file path
											// NOTE: Passing NULL for Value1 and Value2
											// will cause all active database handles to
											// be invalidated.
		FLM_QUERY_MAX,					// Set the maximum number of queries to save.
											// Value1	FLMUINT		Maximum queries.
											//	FlmGetConfig.  Returns maximum queries.
											//		Value1 (FLMUINT *)
		FLM_MAX_DIRTY_CACHE,			// FlmConfig - Specify maximum dirty cache.
											//		Value1	(FLMUINT) Maximum dirty cache
											//		Value2	(FLMUINT) Low dirty cache
											// FlmGetConfig. Return maximum dirty cache.
											//		Value1	(FLMUINT *)
		FLM_DYNA_CACHE_SUPPORTED	// FlmGetConfig. Returns TRUE if the current
											// platform supports dynamic cache adjustments
											//		Value1	(FLMBOOL *)
	} eFlmConfigTypes;

	// Defaults for certain settable items

	#define DEFAULT_MAX_CP_INTERVAL				180
	#define DEFAULT_MAX_TRANS_SECS				2400
	#define DEFAULT_MAX_TRANS_INACTIVE_SECS	30
	#define DEFAULT_CACHE_ADJUST_PERCENT		70
	#define DEFAULT_CACHE_ADJUST_MIN				(16 * 1024 * 1024)
	#define DEFAULT_CACHE_ADJUST_MAX				0xE0000000
	#define DEFAULT_CACHE_ADJUST_MIN_TO_LEAVE	0
	#define DEFAULT_CACHE_ADJUST_INTERVAL		15
	#define DEFAULT_CACHE_CLEANUP_INTERVAL		15
	#define DEFAULT_UNUSED_CLEANUP_INTERVAL	2
	#define DEFAULT_MAX_UNUSED_TIME				120
	#define DEFAULT_BLOCK_CACHE_PERCENTAGE		50
	#define DEFAULT_FILE_EXTEND_SIZE				(8192 * 1024)

	// Values for FLM_SANITYLEVEL

	#define FLM_NO_CHECK								1
	#define FLM_BASIC_CHECK							2
	#define FLM_INTERMEDIATE_CHECK				3
	#define FLM_EXTENSIVE_CHECK					4

	RCODE FlmSetDynamicMemoryLimit(
		FLMUINT			uiCacheAdjustPercent,
		FLMUINT			uiCacheAdjustMin,
		FLMUINT			uiCacheAdjustMax,
		FLMUINT			uiCacheAdjustMinToLeave);

	RCODE FlmSetHardMemoryLimit(
		FLMUINT			uiPercent,
		FLMBOOL			bPercentOfAvail,
		FLMUINT			uiMin,
		FLMUINT			uiMax,
		FLMUINT			uiMinToLeave);

	void FlmGetMemoryInfo(
		FLM_MEM_INFO *	pMemInfo);

	RCODE FlmConfig(
		eFlmConfigTypes	eConfigType,
		void *		Value1,
		void *		Value2);

	RCODE FlmGetConfig(
		eFlmConfigTypes	eConfigType,
		void *	   Value);

	RCODE FlmGetThreadInfo(
		PMM_p					pPool,
		F_THREAD_INFO **	ppThreadInfo,
		FLMUINT *			puiNumThreads,
		FLMBYTE *			pszUrl = NULL);

	/*============================================================================
	Statistics
	============================================================================*/

	typedef struct
	{
		FLMUINT64	ui64Count;			// Number of times operation was performed
		FLMUINT64	ui64ElapMilli;		// Total elapsed time (milliseconds) for
												// the operations.
	} COUNT_TIME_STAT, * COUNT_TIME_p;

	typedef struct
	{
		FLMUINT64	ui64Count;			// Number of times operation was performed
		FLMUINT64	ui64TotalBytes;	// Total number of bytes involved in the
												// operations
		FLMUINT64	ui64ElapMilli;		// Total elapsed time (milliseconds) for the
												// operations.
	} DISKIO_STAT, * DISKIO_STAT_p;

	typedef struct
	{
		COUNT_TIME_STAT	CommittedTrans;	// Transactions committed
		COUNT_TIME_STAT	AbortedTrans;		// Transactions aborted
		COUNT_TIME_STAT	InvisibleTrans;	// Invisible Transactions
	} RTRANS_STATS, * RTRANS_STATS_p;

	typedef struct
	{
		COUNT_TIME_STAT	CommittedTrans;	// Transactions committed
		COUNT_TIME_STAT	GroupCompletes;	// Group completes.
		FLMUINT64			ui64GroupFinished;// Transactions finished in group
		COUNT_TIME_STAT	AbortedTrans;		// Transactions aborted
	} UTRANS_STATS, * UTRANS_STATS_p;

	typedef struct
	{
		DISKIO_STAT		BlockReads;						// Statistics on block reads
		DISKIO_STAT		OldViewBlockReads;			// Statistics on old view
																// block reads.
		FLMUINT			uiBlockChkErrs;				// Number of times we had
																// check errors reading
																// blocks
		FLMUINT			uiOldViewBlockChkErrs;		// Number of times we had
																// check errors reading an
																// old view of a block
		FLMUINT			uiOldViewErrors;				// Number of times we had an
																// old view error when
																// reading
		DISKIO_STAT		BlockWrites;					// Statistics on Block writes
	} BLOCKIO_STATS, * BLOCKIO_STATS_p;

	typedef struct
	{
		FLMBOOL			bHaveStats;						// Flag indicating whether or
																// not there are statistics
																// for this LFILE
		BLOCKIO_STATS	RootBlockStats;				// Block I/O statistics for root
																// blocks
		BLOCKIO_STATS	MiddleBlockStats;				// Block I/O statistics for
																// blocks that are not root
																// blocks or leaf blocks
		BLOCKIO_STATS	LeafBlockStats;				// Block I/O statistics for leaf
																// blocks
		FLMUINT64		ui64BlockSplits;				// Number of block splits that
																// have occurred in this logical
																// file
		FLMUINT64		ui64BlockCombines;			// Number of block combines that
																// have occurred in this
																// logical file
		FLMUINT			uiLFileNum;						// Logical file number
		FLMUINT			uiDictNum;						// Dictionary this LFILE is
																// defined in - only set for
																// indexes
		FLMUINT			uiFlags;							// Flags for logical file
	#define					LFILE_IS_INDEX			0x80
																// This bit, if set, indicates
																// that the logical file is
																// an index. If not set, the
																// logical file is a
																// container
	#define					LFILE_TYPE_UNKNOWN	0x40
																// This bit, if set, indicates
																// that the logical file type is
																// not currently known
	#define					LFILE_LEVEL_MASK		0x0F	
																// Lower four bits contain the
																// number of levels in the
																// logical file's B-Tree
	} LFILE_STATS, * LFILE_STATS_p;

	typedef struct
	{
		FLMBYTE *		pszDbName;						// Database name - from pFile.
		FLMBOOL			bHaveStats;						// Flag indicating whether or
																// not there are statistics
																// for this database
		RTRANS_STATS	ReadTransStats;				// Read Transaction
																// Statistics
		UTRANS_STATS	UpdateTransStats;				// Update Transaction
																// Statistics
		FLMUINT64		ui64NumCursors;				// Number of times a cursor
																// was initialized on this
																// database
		FLMUINT64		ui64NumCursorReads;			// Number of cursor operations
																// that have been performed on
																// this database (next, prev,
																// first, last, current)
		COUNT_TIME_STAT	RecordAdds;					// Number of record adds that
																// have been performed on this
																// database
		COUNT_TIME_STAT	RecordDeletes;				// Number of record deletes that
																// have been performed on this
																// database
		COUNT_TIME_STAT	RecordModifies;			// Number of record modifies
																// that have been performed on
																// this database
		FLMUINT64		ui64NumRecordReads;			// Number of record reads
																// performed on this database that
																// were not cursor reads
		FLMUINT			uiLFileAllocSeq;				// Allocation sequence number
																// for LFILE array
		LFILE_STATS *	pLFileStats;					// Pointer to LFILE statistics
																// array
		FLMUINT			uiLFileStatArraySize;		// Size of LFILE statistics
																// array
		FLMUINT			uiNumLFileStats;				// Number of elements in LFILE
																// array currently in use
		BLOCKIO_STATS	LFHBlockStats;					// Block I/O statistics for
																// LFH blocks
		BLOCKIO_STATS	AvailBlockStats;				// Block I/O statistics for
																// AVAIL blocks

		// Write statistics

		DISKIO_STAT		LogHdrWrites;					// Statistics on log header
																// writes
		DISKIO_STAT		LogBlockWrites;				// Statistics on log block
																// writes
		DISKIO_STAT		LogBlockRestores;				// Statistics on log block
																// restores

		// Read statistics

		DISKIO_STAT		LogBlockReads;					// Statistics on log block
																// reads
		FLMUINT			uiLogBlockChkErrs;			// Number of times we had
																// checksum errors reading
																// blocks from the rollback
																// log
		FLMUINT			uiReadErrors;					// Number of times we got read
																// errors
		FLMUINT			uiWriteErrors;					// Number of times we got write
																// errors

		// Lock statistics

		COUNT_TIME_STAT	NoLocks;						// Times when no lock was held
		COUNT_TIME_STAT	WaitingForLock;			// Time waiting for lock
		COUNT_TIME_STAT	HeldLock;					// Time holding lock

	} DB_STATS, * DB_STATS_p;

	typedef struct
	{
		F_MUTEX			hMutex;							// Handle to semaphore for
																// controlling access and
																// updating of this structure
		DB_STATS *		pDbStats;						// Pointer to array of database
																// statistics
		FLMUINT			uiDBAllocSeq;					// Allocation sequence number
																// for database statistics
		FLMUINT			uiDbStatArraySize;			// Size of the database statistics
																// array
		FLMUINT			uiNumDbStats;					// Number of elements in the
																// database statistics array that
																// are currently in use
		FLMBOOL			bCollectingStats;				// Flag indicating whether or
																// not we are currently
																// collecting statistics
		FLMUINT			uiStartTime;					// Time we started collecting
																// statistics
		FLMUINT			uiStopTime;						// Time we stopped collecting
																// statistics
	} FLM_STATS;

	RCODE FlmGetStats(
		FLM_STATS *	pFlmStats);

	void FlmFreeStats(
		FLM_STATS *	pFlmStats);

	/*--------------------------------------------------------
			 FLAIM Event Notification structures and routines.
	**-------------------------------------------------------*/

	typedef enum
	{
		F_EVENT_LOCKS,
		F_EVENT_UPDATES,
		F_MAX_EVENT_CATEGORIES
	} FEventCategory;

	typedef enum
	{
		F_EVENT_LOCK_WAITING,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_LOCK_GRANTED,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_LOCK_SUSPENDED,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_LOCK_RESUMED,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_LOCK_RELEASED,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_LOCK_TIMEOUT,		// pvEventData1 == hDb, pvEventData2 == thread id
		F_EVENT_BEGIN_TRANS,			// pEventData1 == FLM_TRANS_EVENT *
		F_EVENT_COMMIT_TRANS,		// pEventData1 == FLM_TRANS_EVENT *
		F_EVENT_ABORT_TRANS,			// pEventData1 == FLM_TRANS_EVENT *
		F_EVENT_ADD_RECORD,			// pEventData1 == FLM_UPDATE_EVENT *
		F_EVENT_MODIFY_RECORD,		// pEventData1 == FLM_UPDATE_EVENT *
		F_EVENT_DELETE_RECORD,		// pEventData1 == FLM_UPDATE_EVENT *
		F_EVENT_RESERVE_DRN,			// pEventData1 == FLM_UPDATE_EVENT *
		F_EVENT_INDEXING_COMPLETE, // pEventData1 (FLMUINT) - index number, 
											// pEventData2 (FLMUINT) - last drn indexed, 
											//  if zero then the index is now online.
		F_MAX_EVENT_TYPES				// Should always be last.
	} FEventType;

	typedef void *					HFEVENT;
	#define HFEVENT_NULL       NULL

	typedef void (* FEVENT_CB)(
		FEventType	eEventType,
		void *		pvAppData,
		void *		pvEventData1,
		void *		pvEventData2);

	RCODE FlmRegisterForEvent(
		FEventCategory	eCategory,
		FEVENT_CB		fnEventCB,
		void *			pvAppData,
		HFEVENT *		phEventRV);

	void FlmDeregisterForEvent(
		HFEVENT *	phEventRV);

	/*--------------------------------------------------------
			Open, Create, Close and Convert Database Routines.
	**-------------------------------------------------------*/

	RCODE FlmDbCreate(
		FLMBYTE *	   pszDbFileName,
		FLMBYTE *		pszDataDir,
		FLMBYTE *		pszRflDir,
		FLMUINT        uiOpenFlags,
		FLMBYTE *	   pDictFileName,
		FLMBYTE *      pszDictBuf,
		CREATE_OPTS_p  pCreateOpts,
		HFDB *			phDb);

		// Defines used by FlmDbCreate and FlmDbOpen

		#define FO_SHARE									0
		#define FO_ONLINE									0x0020
		#define FO_DONT_REDO_LOG						0x0040
		#define FO_DONT_RESUME_BACKGROUND_THREADS	0x0080
		#define FO_DO_LOGICAL_CHECK					0x0100
		#define FO_DO_EXTENDED_DATA_CHECK			0x0200	// Used only in DbCheck.
		#define FO_ALLOW_LIMITED						0X0400	// Used only in FlmDbOpen

	RCODE FlmDbOpen(
		FLMBYTE *      pszDbFileName,
		FLMBYTE *		pszDataDir,
		FLMBYTE *		pszRflDir,
		FLMUINT			uiOpenFlags,
		FLMBYTE *		pszPassword,
		HFDB *			phDbRV);

	RCODE FlmDbClose(
		HFDB *			phDbRV);

	typedef void (* COMMIT_FUNC)(
		HFDB				hDb,
		void *			pvUserData);

	RCODE FlmDbConfig(
		HFDB				hDb,
		FLMUINT			uiType,
		void *			Value1,
		void *			Value2);

		// uiType values for FlmDbConfig

		#define FDB_CLEAR_OVERRIDE					1
		#define FDB_SANITYLEVEL						2
		#define FDB_SET_APP_VERSION				3
		#define FDB_RFL_KEEP_FILES					4
		#define FDB_RFL_DIR							5
		#define FDB_RFL_FILE_LIMITS				6
		#define FDB_RFL_ROLL_TO_NEXT_FILE		7
		#define FDB_KEEP_ABORTED_TRANS_IN_RFL	8
		#define FDB_AUTO_TURN_OFF_KEEP_RFL		9
		#define FDB_FILE_EXTEND_SIZE				10
		#define FDB_SET_APP_DATA					11
		#define FDB_SET_COMMIT_CALLBACK			12

		// Flags for overriding global settings

		#define FDB_OVRD_SANITY						((FLMUINT)(0x00000001))
		#define FDB_DISABLE_SANITY_CHECKING		((FLMUINT)(0x00000002))

	void FlmFreeMem(
		void *		pMem);

	RCODE FlmDbGetConfig(
		HFDB				hDb,
		FLMUINT			uiType,
		void *			Value1,
		void *			Value2 = NULL,
		void *			Value3 = NULL);

		// uiType flags for FlmDbGetConfig

		#define	FDB_GET_VERSION									0x0001
		#define	FDB_GET_BLKSIZ										0x0002
		#define	FDB_GET_DEFAULT_LANG								0x0003
		#define	FDB_GET_PATH										0x0011
		#define	FDB_GET_TRANS_ID									0x0012
		#define	FDB_GET_CHECKPOINT_INFO							0x0013
		// Value1 is CHECKPOINT_INFO *
		// Value2 and Value3 are ignored
		#define	FDB_GET_LOCK_HOLDER								0x0014
		// Value1 is LOCK_USER *
		// Value2 and Value3 are ignored
		#define	FDB_GET_LOCK_WAITERS								0x0015
		// Value1 is LOCK_USER ** (Caller must free
		// using FlmFreeMem( &ptr).  May return NULL.
		// Value2 and Value3 are ignored
		#define	FDB_GET_LOCK_WAITERS_EX							0x0016
		// Value1 is FlmLockInfo *
		// Value2 and Value3 are ignored
		#define	FDB_GET_RFL_DIR									0x0017
		// Value1 is FLMBYTE[ F_MAX_PATH_SIZE]
		// Value2 and Value3 are ignored
		#define	FDB_GET_RFL_FILE_NUM								0x0018
		// Value1 is FLMUINT *
		// Value2 and Value3 are ignored
		#define	FDB_GET_RFL_HIGHEST_NU							0x0019
		// Value1 is FLMUINT *			
		// Value2 and Value3 are ignored
		#define	FDB_GET_RFL_FILE_SIZE_LIMITS					0x001A
		// Value1 is FLMUINT *
		// Value2 is FLMUINT *
		// Value3 is ignored
		#define	FDB_GET_RFL_KEEP_FLAG							0x001B
		// Value1 is FLMBOOL *
		// Value2 and Value3 are ignored
		#define	FDB_GET_LAST_BACKUP_TRANS_ID					0x001C
		// Value1 is FLMUINT *
		// Value2 and Value3 are ignored
		#define	FDB_GET_BLOCKS_CHANGED_SINCE_BACKUP			0x001D
		// Value1 is FLMUINT *
		// Value2 and Value3 are ignored
		#define	FDB_GET_SERIAL_NUMBER							0x001E
		// Value1 is FLMBYTE[ F_SERIAL_NUM_SIZE]
		// Value2 and Value3 are ignored
		#define	FDB_GET_AUTO_TURN_OFF_KEEP_RFL_FLAG			0x001F
		// Value1 is FLMBOOL *
		// Value2 and Value3 are ignored
		#define	FDB_GET_KEEP_ABORTED_TRANS_IN_RFL_FLAG		0x0020
		// Value1 is FLMBOOL *
		// Value2 and Value3 are ignored
		#define FDB_GET_SIZES										0x0021
		// Value1 is FLMUINT64 *	Returns DB size
		// Value2 is FLMUINT64 *	Returns rollback size
		// Value3 is FLMUINT64 *	Returns RFL size
		#define FDB_GET_FILE_EXTEND_SIZE							0x0022
		// Value1 is FLMUINT *		Returns file extend size
		// Value2 and Value3 are ignored.
		#define FDB_GET_APP_DATA									0x0023
		// Value1 is a void **		Returns app object for DB
		#define FDB_GET_NEXT_INC_BACKUP_SEQ_NUM				0x0024
		// Value1 is a FLMUINT *	Returns the sequence number of
		//									the next incremental backup
		#define FDB_GET_DICT_SEQ_NUM								0x0025
		// Value1 is a FLMUINT *	Returns the sequence number of
		//									the dictionary
		#define FDB_GET_FFILE_ID									0x0026
		// Value1 is a FLMUINT *	Returns the ID of the FDB's FFILE
		#define FDB_GET_MUST_CLOSE_RC								0x0027
		// Value1 is an RCODE *		Returns the error that caused the
		//									"must close" flag to be set

	class FlmLockInfo : public F_Base
	{
	public:

		virtual FLMBOOL setLockCount(		// Return TRUE to continue, FALSE to stop
			FLMUINT		uiTotalLocks) = 0;

		virtual FLMBOOL addLockInfo(		// Return TRUE to continue, FALSE to stop
			FLMUINT		uiLockNum,			// Position in queue (0 = lock holder,
													// 1 ... n = lock waiter)
			FLMUINT		uiThreadID,			// Thread ID of the lock holder/waiter
			FLMUINT		uiTime) = 0;		// For the lock holder, this is the
													// time when the lock was obtained.
													// For a lock waiter, this is the time
													// that the waiter was placed in the queue.
													// Both times are presented in milliseconds.
	};

	/*------------------------------------------------------
		FLAIM Processing Hooks (call-backs)
	-------------------------------------------------------*/

	typedef RCODE (* QUERY_EXTENDER_HOOK)(
		HFCURSOR			hCursor,
		FLMBOOL			bHaveQuery,
		void *			UserData);

	// The REC_VALIDATOR_HOOK callback function has eight
	// parameters (see FLAIM.H):
	//
	//    Type        Name           Description
	//
	//    FLMUINT     uiFlmFuncId     [IN] Current FLAIM operation.
	//                               (See FlmFuncs enum in FLAIM.H
	//                               for a complete list.)
	//
	//    HFDB        hDb            [IN] Handle to the current
	//                               database.
	//
	//    FLMUINT     uiContainerId   [IN] ID of the current container.
	//
	//    NODE_p      pRecord        [IN] Pointer to the current
	//                               record.
	//
	//    NODE_p      pOldRecord     [IN] Pointer to the original
	//                               record.  This value will be
	//                               NULL unless the current
	//                               operation is a record modify.
	//
	//    void *   UserData       [IN] User-defined data.
	//
	//    RCODE *     pRCode         [OUT] Return code to be
	//                               returned by the API which
	//                               invoked the callback.
	//
	// The REC_VALIDATOR_HOOK callback returns an RCODE.  In the case
	// of an update operation, the RCODE returned by the callback
	// is ignored.  However, in the case of a read operation, the
	// return code is evaluated.  If it is FERR_OK, the current
	// record is returned to the application.  Otherwise, the record
	// is not returned, but the read operation is allowed to continue.
	//
	// If *pRCode is FERR_OK, the routine which invoked the callback
	// will continue the current operation.  Otherwise, the operation
	// will be terminate and *pRCode will be returned to the application.

	typedef FLMBOOL (* REC_VALIDATOR_HOOK)(
		eFlmFuncs		eFlmFuncId,
		HFDB				hDb,
		FLMUINT			uiContainerId,
		FlmRecord *		pRecord,
		FlmRecord *		pOldRecord,
		void *			UserData,
		RCODE *			pRCode);


	typedef RCODE (* IX_CALLBACK)(
		HFDB				hDb,
		FLMUINT			uiIndexNum,
		FLMUINT			uiContainerNum,
		FLMUINT			uiDrn,
		FlmRecord *		pInputRecord,
		FlmRecord **	ppModifiedRecord,
		void *			UserData);

	typedef RCODE (* STATUS_HOOK)(
		FLMUINT			uiStatusType,
		void *			Parm1,
		void *			Parm2,
		void *			UserData);

		// Options for uiStatusType

		#define FLM_INDEXING_STATUS			2
   		// Index processing status
			// Parm1 (FLMUINT) == Records Processed
			// Parm2 (FLMUINT) == Keys Processed
		#define FLM_DELETING_STATUS			3
   		// Index processing status
			// Parm1 (FLMUINT) == Blocks deleted.
			// Parm2 (FLMUINT) == Block size.
		#define FLM_SWEEP_STATUS				5
			// FlmDbSweep() status
			// Parm1 (SWEEP_INFO *) == info structure from FlmDbSweep.
			// Parm2 (FLMUINT) == defines for EACH_CONTAINER, ...
		#define FLM_CHECK_STATUS				7
			// Check Status returned from FlmDbCheck
			// Parm1 DB_CHECK_PROGRESS *
			// Parm2 Not used
		#define FLM_LOCK_STATUS					11
			// Parm1 Not Used
			// Parm2 (FLMUINT) == lock state, 1 == lock file, 2 == unlock file.
		#define FLM_SWEEP_DB_ERR_STATUS	12
			// Error status returned from FlmDbSweep
			// Parm1 (RCODE) == error code
			// Parm2 (SWEEP_INFO *) == info struct from FlmDbSweep
		#define FLM_SUBQUERY_STATUS			13
			// Parm1 (FCURSOR_SUBQUERY_STATUS *), Info about the sub-query.
		#define FLM_BLOB_ERROR_STATUS 17
			// This callback is made when an error is hit in open, create, read or
			// write BLOB code.  The more common error codes are: 
			//		BCEF_ERR_BAD_DATA - Data length in the node is invalid.
			//		FERR_BLOB_MISSING_FILE - The file for the external BLOB is missing.

			// Parm1 (BLOB_CONTEXT *)
			// Parm2 (RCODE) - Current error code
		#define FLM_DB_COPY_STATUS			21
			// This callback is made when FlmDbCopy is called to report progress.
			// Parm1 (DB_COPY_INFO *)
			// Parm2 - not used
		#define FLM_REBUILD_STATUS			22
			// Rebuild Status returned from FlmDbRebuild
			// Parm1 REBUILD_INFO *
			// Parm2 Not used
		#define FLM_PROBLEM_STATUS			23
			// Problem Status returned from FlmDbCheck
			// Parm1 CORRUPT_INFO *
			// Parm2 FLMBOOL *	// Callback returns a flag indicating whether
										// or not the error should be fixed.
		#define FLM_CHECK_RECORD_STATUS	24
			// Check Record Status returned from FlmDbRebuild
			// Parm1 (CHK_RECORD *)
			// Parm2 Not used
		#define FLM_EXAMINE_RECORD_STATUS	25
			// Examine Record Status returned from FlmDbRebuild
			// This status gives the callback function a chance to examine each
			//		non-dictionary record in the rebuilt DB.  All return information
			//		from the callback function (including the return code) is ignored.
			// Parm1 (CHK_RECORD *)
			// Parm2 Not used
		#define FLM_DB_CONVERT_STATUS			26
			// This callback is made when FlmDbConvert is called to report progress.
			// The 3_0 to 3_10 and 3_10 to 3_0 conversions call.  May be called
			// for every record or every non-data b-tree block.
			// Parm1 (DB_CONVERT_INFO *)

		#define FLM_DB_BACKUP_STATUS			27
			// This callback is made when FlmDbBackup is called to report progress.
			// Parm1 (DB_BACKUP_INFO *)
		#define FLM_DB_RENAME_STATUS			28
			// This callback is made when FlmDbRename is called to report progress.
			// Parm1 (DB_RENAME_INFO *)
		#define FLM_DELETING_KEYS				29
   		// Index processing status while deleting a container
			// Parm1 (FLMUINT) == Index Number.
			// Parm2 (FLMUINT) == Elements traversed.
		#define FLM_REBUILD_ADD_DICT_REC_STATUS	30
			// Called for each dictionary record recovered by the rebuild code
			// prior to re-adding the record to the database.  The application
			// can change the record, if desired.
			// Parm1 (FlmRecord *)
			// Parm2 Not used

	typedef RCODE (* BACKER_WRITE_HOOK)( 
		void *		pvBuffer,
		FLMUINT		uiBytesToWrite,
		void *		pvUserData);

	/*--------------------------------------------------------
			Index operations.
	**-------------------------------------------------------*/

	RCODE FlmIndexStatus(
		HFDB					hDb,
		FLMUINT				uiIndexNum,
		FINDEX_STATUS *	pIndexStatus);

	RCODE FlmIndexGetNext(
		HFDB			hDb,
		FLMUINT *	puiIndexNum);

	RCODE FlmIndexSuspend(
		HFDB			hDb,
		FLMUINT		uiIndexNum);

	RCODE FlmIndexResume(
		HFDB			hDb,
		FLMUINT		uiIndexNum);

	/*--------------------------------------------------------
					 Maintenance Thread Info
	**-------------------------------------------------------*/

	typedef struct
	{
		FLMUINT			uiDoing;
			#define FLM_MAINT_UNKNOWN					0x00000000
			#define FLM_MAINT_IDLE						0x00000001
			#define FLM_MAINT_LOOKING_FOR_WORK		0x00000002
			#define FLM_MAINT_WAITING_FOR_LOCK		0x00000003
			#define FLM_MAINT_ENDING_TRANS			0x00000004
			#define FLM_MAINT_TERMINATED				0x00000005
			#define FLM_MAINT_FREEING_BLOCKS			0x00000006
		FLMUINT64		ui64BlocksFreed;
	} FMAINT_STATUS;

	RCODE FlmMaintenanceStatus(
		HFDB					hDb,
		FMAINT_STATUS *	pMaintStatus);

	/*--------------------------------------------------------
					 Error Handling Routines
	**-------------------------------------------------------*/

	FLMBOOL FlmErrorIsFileCorrupt(
		RCODE       rc);

	FLMBYTE * FlmErrorString(
		RCODE       rc);

	RCODE FlmGetDiagInfo(
		HFDB			hDb,
		FLMUINT     uiDiagCode,
		void *      pvDiagInfo);

		// Below are the allowable values for uiDiagCode, on FlmGetDiagInfo,
		// and the errors that can generate the diagnosic information

		#define FLM_DIAG_INDEX_NUM       0x0001      // NOTE:  pvDiagInfo == FLMUINT *
			// FERR_NOT_UNIQUE - returns index number of non-unique index
		#define FLM_DIAG_DRN             0x0002      // NOTE:  pvDiagInfo == FLMUINT *
			// FERR_SYNTAX - dictionary syntax error info, returns dict rec DRN
			// FERR_INVALID_TAG - returns drn of last valid dict record processed
			// FERR_DUPLICATE_DICT_REC - returns drn of record with duplicate ID.
			// FERR_DUPLICATE_DICT_NAME - returns drn of record with duplicate name.
			// FERR_ID_RESERVED - returns drn of reserved ID.
			// FERR_CANNOT_RESERVE_ID - returns drn of ID that cannot be reserved.
			// FERR_CANNOT_RESERVE_NAME - returns drn of name that cannot be reserved.
			// FERR_BAD_DICT_DRN - returns dictionary DRN that was bad.
		#define FLM_DIAG_FIELD_NUM       0x0004      // NOTE:  pvDiagInfo == FLMUINT *
			// FERR_SYNTAX - more dict syntax error info, returns field dict field num
			// FERR_BAD_FIELD_NUM - returns invalid field number in record
		#define FLM_DIAG_AREA_ID         0x0020      // NOTE: pvDiagInfo == FLMUINT *
			// Area ID that was not defined or for which a machine was not defined.
		#define FLM_DIAG_FIELD_TYPE      0x0040      // NOTE: pvDiagInfo == FLMUINT *
			// Field type for field that was not defined
			// FERR_BAD_FIELD_NUM
		#define FLM_DIAG_ENC_ID				0x0080		// NOTE: pvDiagInfo == FLMUINT *
			// Encryption Id that was not defined
			// FERR_PURGED_ENCDEF_FOUND - Purged encryption definition used

	FLMBOOL   FlmDbUsingCS(
		HFDB			hDb);

	/*--------------------------------------------------------
					 Transaction Routines.
	**-------------------------------------------------------*/

	// Defines used for 'uiTransType' parameter

	#define FLM_NO_TRANS				0
	#define FLM_UPDATE_TRANS		1
	#define FLM_READ_TRANS			2

	#define FLM_DONT_KILL_TRANS	0x10
	#define FLM_DONT_POISON_CACHE	0x20

	#define FLM_GET_TRANS_FLAGS(uiTransType) \
		(((uiTransType) & 0xF0))

	#define FLM_GET_TRANS_TYPE(uiTransType) \
		(((uiTransType) & 0x0F))
		
	// Defines used for uiMaxLockWait parameter

	#define FLM_NO_TIMEOUT			0xFF

	RCODE FlmDbLock(
		HFDB				hDb,
		FLOCK_TYPE		eLockType,
		FLMINT			iPriority,
		FLMUINT			uwTimeout);

	RCODE FlmDbUnlock(
		HFDB				hDb);

	RCODE FlmDbGetLockType(
		HFDB				hDb,
		FLOCK_TYPE *	peLockType,
		FLMBOOL *		pbImplicit);

	RCODE FlmDbGetLockInfo(
		HFDB				hDb,
		FLMINT			iPriority,
		FLOCK_INFO *	pLockInfo);

	RCODE FlmDbCheckpoint(
		HFDB				hDb,
		FLMUINT			uwTimeout);

	RCODE FlmDbTransBegin(
		HFDB        hDb,
		FLMUINT		uiTransType,
		FLMUINT		uiMaxLockWait,
		FLMBYTE *	pszHeader = NULL);

	#define F_TRANS_HEADER_SIZE		2048	// Size of buffer required for pszHeader
														// parameter of FlmDbTransBegin

	RCODE FlmDbTransCommit(
		HFDB			hDb,
		FLMBOOL *	pbEmpty = NULL);

	RCODE FlmDbTransAbort(
		HFDB			hDb);

	RCODE FlmDbGetTransType(
		HFDB        hDb,
		FLMUINT *	puiTransTypeRV);

	RCODE FlmDbGetTransId(
		HFDB        hDb,
		FLMUINT *	uiTransNum);

	RCODE FlmDbGetCommitCnt(
		HFDB        hDb,
		FLMUINT *	seqNum);

	/*--------------------------------------------------------
		 ADD, MODIFY, DELETE and READ record and key routines.
	**-------------------------------------------------------*/

	#define FLM_AUTO_TRANS				0x0100	// Value ORed into wAutoTrans parameter
	#define FLM_DO_IN_BACKGROUND		0x0400	// Value ORed into uiAutoTrans parameter
	#define FLM_DONT_INSERT_IN_CACHE	0x0800	// Value ORed into uiAutoTrans parameter
	#define FLM_SUSPENDED				0x1000	// Value ORed into wAutoTrans parameter

	RCODE FlmRecordAdd(
		HFDB			hDb,
		FLMUINT		uiContainerNum,
		FLMUINT *	puiDrnRV,
		FlmRecord *	pRecord,
		FLMUINT		uiAutoTrans);

	RCODE FlmRecordModify(
		HFDB			hDb,
		FLMUINT		uiContainerNum,
		FLMUINT		uiDrn,
		FlmRecord *	pRecord,
		FLMUINT		uiAutoTrans);

	RCODE FlmRecordDelete(
		HFDB			hDb,
		FLMUINT		uiContainerNum,
		FLMUINT		uiDrn,
		FLMUINT		uiAutoTrans);

	RCODE FlmReserveNextDrn(
		HFDB			hDb,
		FLMUINT		uiContainer,
		FLMUINT *	puiDrnRV);

	RCODE FlmFindUnusedDictDrn(
		HFDB			hDb,
		FLMUINT		uiStartDrn,
		FLMUINT		uiEndDrn,
		FLMUINT *	puiDrnRV);

		// uiFlag or uiFlags values in FlmRecordRetrieve or FlmKeyRetrieve

		#define FO_INCL      0x10
		#define FO_EXCL      0x20
		#define FO_EXACT     0x40
		#define FO_KEY_EXACT	0x80
		#define FO_FIRST		0x100
		#define FO_LAST		0x200

	RCODE FlmRecordRetrieve(
		HFDB				hDb,
		FLMUINT			uiContainer,
		FLMUINT			uiDrn,
		FLMUINT			uiFlag,
		FlmRecord * *	ppRecord,
		FLMUINT *		puiDrnRV);

	RCODE	FlmKeyRetrieve(
		HFDB				hDb,
		FLMUINT			uiIndex,
		FLMUINT			uiContainer,
		FlmRecord *		pKeyTree,
		FLMUINT			uiRefDrn,
		FLMUINT			uiFlags,
		FlmRecord * *	ppRecordRV,
		FLMUINT *		puiDrnRV);

	/*--------------------------------------------------------
					 Misc. FLAIM Routines.
	**-------------------------------------------------------*/
		
	RCODE FlmGetItemName(
		HFDB				hDb,
		FLMUINT			uiItemId,
		FLMUINT			uiNameBufSize,
		FLMBYTE *		pszNameBuf);

	RCODE FlmDbReduceSize(
		HFDB				hDb,
		FLMUINT			uiCount,
		FLMUINT *		puiCountRV);

	RCODE FlmDbSweep(
		HFDB				hDb,
		FLMUINT			uiSweepMode,
		FLMUINT			uiCallbackFreq,
		STATUS_HOOK		fnStatusHook,
		void *			UserData);

		// Options for wSweepMode

		#define SWEEP_CHECKING_FLDS   0x01     // look for 'checking' field/record states.
		#define SWEEP_PURGED_FLDS     0x02     // remove 'purged' items.
		#define SWEEP_STATS           0x04     // only calls the STATUS_HOOK

		// Options for wCallbackFreq

		#define EACH_CONTAINER        0x02     // calls fnStatusHook on each container
		#define EACH_RECORD           0x04     // calls fnStatusHook on each record
		#define EACH_FIELD            0x08     // calls fnStatusHook on each field
		#define EACH_CHANGE           0x10     // calls when a 'checking' or 'purged'
															// field is being changed 'check' -> 'unused'
															// and deleting 'purged' fields

	RCODE FlmDbCopy(
		FLMBYTE *		pszSrcDbName,
		FLMBYTE *		pszSrcDataDir,
		FLMBYTE *		pszSrcRflDir,
		FLMBYTE *		pszDestDbName,
		FLMBYTE *		pszDestDataDir,
		FLMBYTE *		pszDestRflDir,
		STATUS_HOOK		fnStatusCallback,
		void *			UserData);

	FLMINT FlmStrCmp(
		FLMUINT			uiCompFlags,	// For uiCompFlags use the defines for 
												// case, space, dash, underscore in query.
		FLMUINT			uiLanguage,
		FLMUNICODE *	uzStr1,
		FLMUNICODE *	uzStr2);

	RCODE FlmDbUpgrade(
		HFDB			hDb,		
		STATUS_HOOK	fnStatusCallback,
		void *		UserData);

	RCODE FlmEnableEncryption(
		HFDB				hDb,
		FLMBYTE **		ppucWrappingKeyRV,		// This returned buffer must be freed by the caller
		FLMUINT32 *		pui32KeyLen);

	RCODE FlmDbWrapKey(
		HFDB				hDb,
		FLMBYTE *		pszPassword);				// May be NULL to wrap the key in the server key

	typedef enum
	{
		// These values are stored in the header of the 
		// backup, so do not change their values.
		FLM_FULL_BACKUP = 0,
		FLM_INCREMENTAL_BACKUP
	} FBackupType;

	RCODE FlmDbBackupBegin(
		HFDB			hDb,
		FBackupType	eBackupType,
		FLMUINT		uiTransType,
		FLMUINT		uiMaxLockWait,
		HFBACKUP *	phBackup);

	RCODE FlmBackupGetConfig(
		HFBACKUP			hBackup,
		FLMUINT			uiType,
		void *			Value1,
		void *			Value2 = NULL);

		// uiType flags for FlmBackupGetConfig

		#define	FBAK_GET_BACKUP_TRANS_ID				0x0001
		// Value1 is FLMUINT *
		#define	FBAK_GET_LAST_BACKUP_TRANS_ID			0x0002
		// Value1 is FLMUINT *

	RCODE FlmDbBackup(
		HFBACKUP					hBackup,
		FLMBYTE *				pszBackupPath,
		FLMBYTE *				pszPassword,
		BACKER_WRITE_HOOK		fnWrite,
		STATUS_HOOK				fnStatus,
		void *					pvUserData,
		FLMUINT *				puiIncSeqNum);

	RCODE FlmDbBackupEnd(
		HFBACKUP *				phBackup);

	RCODE FlmDbRemove(
		FLMBYTE *		pszDbName,
		FLMBYTE *		pszDataDir,
		FLMBYTE *		pszRflDir,
		FLMBOOL			bRemoveRflFiles);

	RCODE FlmDbRename(
		FLMBYTE *		pszDbName,
		FLMBYTE *		pszDataDir,
		FLMBYTE *		pszRflDir,
		FLMBYTE *		pszNewDbName,
		FLMBOOL			bOverwriteDestOk,
		STATUS_HOOK		fnStatusCallback,
		void *			UserData);

	/*===========================================================================
	Class:	F_UnknownStream
	Desc:		This object is used to read "unknown" data from the RFL or to
				write unknown data to the RFL.
	===========================================================================*/
	class F_UnknownStream : public F_Base
	{
	public:

		virtual RCODE read(
			FLMUINT			uiLength,				// Number of bytes to read
			void *			pvBuffer,				// Buffer to place read bytes into
			FLMUINT *		puiBytesRead) = 0;	// [out] Number of bytes read

		virtual RCODE write(
			FLMUINT			uiLength,				// Number of bytes to write
			void *			pvBuffer) = 0;

		virtual RCODE close( void) = 0;			// Reads to the end of the
															// stream and discards any
															// remaining data (if input stream).
	};

	RCODE FlmDbGetUnknownStreamObj(
		HFDB						hDb,
		F_UnknownStream **	ppUnknownStream);

	RCODE FlmDbGetRflFileName(
		HFDB 					hDb,
		FLMUINT				uiFileNum,
		FLMBYTE *			pszFileName);

	/*--------------------------------------------------------
				BLOB Class, Functions and Definitions
	**-------------------------------------------------------*/

	// FlmBlob::create uiBlobType values

	#define BLOB_UNKNOWN_TYPE        0     // Unknown (binary) type

	// FlmBlob::create uiFlags values

	#define BLOB_OWNED_REFERENCE_FLAG   0x10     // Set when BLOB is owned
	#define BLOB_UNOWNED_REFERENCE_FLAG 0x1000   // Set for BLOB reference

	/*
		BLOB uiRights values.
		RDONLY_FLAG:		Opens external BLOB file in FlmBlob::open().
		RDWR_FLAG:			Opens external BLOB file in FlmBlob::open().
		DELAY_OPEN_FLAG:	Opens external BLOB file on first read.
								Checks the BLOB file for existance in FlmBlob::open().
		SAFE_MODE_FLAG:	Does not open or check external BLOB in FlmBlob::open().
								Opens the BLOB in read-only mode when reading.
	*/

	#define FLM_BLOB_RDONLY_FLAG			0x01
	#define FLM_BLOB_RDWR_FLAG				0x02
	#define FLM_BLOB_DELAY_OPEN_FLAG		0x04
	#define FLM_BLOB_SAFE_MODE_FLAG		0

	/*============================================================================
	Class: 	FlmBlob
	Desc: 	The FlmBlob class provides support for database Binary Large Objects
				(BLOB). This class replaces the old 'C' FlmBlobXxx functions.
	NOTE:		Initially the only minimal BLOB support is being provided. 
	============================================================================*/
	class FlmBlob : public F_Base
	{
	public:

		FlmBlob()
		{
		}

		virtual ~FlmBlob()
		{
		}

		virtual RCODE create( 
			HFDB			hDb, 
			FLMBYTE *	pFilePath, 
			FLMBOOL		bOwned = FALSE) = 0;

		virtual RCODE open( 
			HFDB			hDb, 
			FLMUINT		uiRights) = 0;

		virtual RCODE close( void) = 0;

		virtual FLMBYTE * getImportDataPtr(
			FLMUINT		uiLength) = 0;

		virtual FLMUINT getDataLength( void) = 0;
		
		virtual FLMBYTE * getDataPtr( void) = 0;
		
		virtual FLMINT compareFileName(
			FLMBYTE *		pszFileName) = 0;

		virtual RCODE buildFileName(
			FLMBYTE *		pszFileName) = 0;
	};

	RCODE FlmAllocBlob(
		FlmBlob **		ppBlob);

	/*
	*** Recovery status types
	*/

	#define RESTORE_BEGIN_TRANS						1
		// Value1 (FLMUINT): Transaction start time
	#define RESTORE_COMMIT_TRANS						2
	#define RESTORE_ABORT_TRANS						3
	#define RESTORE_ADD_REC								4
		// Value1 (FLMUINT): Container
		// Value2 (FLMUINT): DRN
		// Value3 (FlmRecord *): Record pointer
	#define RESTORE_DEL_REC								5
		// Value1 (FLMUINT): Container
		// Value2 (FLMUINT): DRN
	#define RESTORE_MOD_REC								6
		// Value1 (FLMUINT): Container
		// Value2 (FLMUINT): DRN
		// Value3 (FlmRecord *): Record pointer (modified record)
	#define RESTORE_RESERVE_DRN						7
		// Value1 (FLMUINT): Container
		// Value2 (FLMUINT): DRN
	#define RESTORE_INDEX_SET							8
		// Value1 (FLMUINT): Index
		// Value2 (FLMUINT): Start DRN
		// Value3 (FLMUINT): End DRN
	#define RESTORE_PROGRESS							9
		// Value1 (BYTE_PROGRESS *)
	#define RESTORE_REDUCE								10
		// Value1 (FLMUINT): Count
	#define RESTORE_UPGRADE								11
		// Value1 (FLMUINT): Current DB Version
		// Value2 (FLMUINT): New DB Version
	#define RESTORE_ERROR								12
		// Value1 (FLMUINT): RCODE
	#define RESTORE_INDEX_SUSPEND						13
		// Value1 (FLMUINT): Index number
	#define RESTORE_INDEX_RESUME						14
		// Value1 (FLMUINT): Index number
	#define RESTORE_BLK_CHAIN_DELETE					15
		// Value1 (FLMUINT): Tracker DRN
		// Value2 (FLMUINT): Block count
	#define RESTORE_WRAP_KEY							16
		// Value1 (FLMUINT): DBKey Length
	#define RESTORE_ENABLE_ENCRYPTION				17
		// Value1 (FLMUINT): DBKey Length

	/*
	*** Recovery actions
	*/

	#define RESTORE_ACTION_CONTINUE					0 // Continue recovery
	#define RESTORE_ACTION_STOP						1 // Stop recovery
	#define RESTORE_ACTION_SKIP						2 // Skip operation (future)
	#define RESTORE_ACTION_RETRY						3 // Retry the operation

	/*
	*** F_Restore class definition
	*/

	/*===========================================================================
	Class:	F_Restore (virtual base class)
	Desc:		The F_Restore class is used when performing database recovery
				after a crash or restore operation.
	===========================================================================*/
	class F_Restore : public F_Base
	{
	public:

		virtual ~F_Restore()
		{
		};

		virtual RCODE openBackupSet( void) = 0;

		virtual RCODE openRflFile(					// Open an RFL file.
			FLMUINT			uiFileNum) = 0;

		virtual RCODE openIncFile(					// Open an incremental backup file
			FLMUINT			uiFileNum) = 0;

		virtual RCODE read(
			FLMUINT			uiLength,				// Number of bytes to read
			void *			pvBuffer,				// Buffer to place read bytes into
			FLMUINT *		puiBytesRead) = 0;	// [out] Number of bytes read

		virtual RCODE close( void) = 0;			// Close the current file

		virtual RCODE abortFile( void) = 0;		// Abort processing the file
															// and close file handles, etc.

		virtual RCODE processUnknown(				// Process an unknown object
			F_UnknownStream *		pUnkStrm) = 0;

		virtual RCODE status(						// Status "callback"
			FLMUINT			uiStatusType,			// See status types (above)
			FLMUINT			uiTransId,
			void *			pvValue1,
			void *			pvValue2,
			void *			pvValue3,
			FLMUINT *		puiAction) = 0;	
	};

	RCODE FlmDbRestore(
		FLMBYTE *				pszDbPath,
		FLMBYTE *				pszDataDir,
		FLMBYTE *				pszBackupPath,
		FLMBYTE *				pszRflDir,
		FLMBYTE *				pszPassword,
		F_Restore *				pRestoreObj);

	/*
	*** FLAIM message logging
	*/

	typedef enum
	{
		FLM_QUERY_MESSAGE,
		FLM_TRANSACTION_MESSAGE,
		FLM_GENERAL_MESSAGE,
		FLM_NUM_MESSAGE_TYPES
	} FlmLogMessageType;

	typedef enum
	{
		FLM_BLACK = 0,
		FLM_BLUE,
		FLM_GREEN,
		FLM_CYAN,
		FLM_RED,
		FLM_PURPLE,
		FLM_BROWN,
		FLM_LIGHTGRAY,
		FLM_DARKGRAY,
		FLM_LIGHTBLUE,
		FLM_LIGHTGREEN,
		FLM_LIGHTCYAN,
		FLM_LIGHTRED,
		FLM_LIGHTPURPLE,
		FLM_YELLOW,
		FLM_WHITE,
		FLM_NUM_COLORS,
		FLM_CURRENT_COLOR
	} FlmColorType;

	/*===========================================================================
	Class:	F_Logger
	===========================================================================*/
	class F_Logger : public F_Base
	{
	public:

		// Constructor

		F_Logger();

		// Destructor

		virtual ~F_Logger();

		// Pure virtual functions that must be implemented

		virtual F_LogMessage * beginMessage(
			FlmLogMessageType	eMsgType) = 0;

		// Functions implemented in the base class

		void enableMessageType(
			FlmLogMessageType	eMsgType);

		void enableAllMessageTypes( void);

		void disableMessageType(
			FlmLogMessageType	eMsgType);

		void disableAllMessageTypes( void);

		FLMBOOL messageTypeEnabled(
			FlmLogMessageType	eMsgType);

		void lockLogger( void);

		void unlockLogger( void);

		// setupLogger should allocate the mutex.

		RCODE setupLogger( void);

		FLMBOOL loggerIsSetup( void);

	private:

		// Private methods and variables

		F_MUTEX		m_hMutex;
		FLMBOOL		m_bSetupCalled;
		FLMBOOL *	m_pbEnabledList;
	};

	/*===========================================================================
	Class:	F_LogMessage
	===========================================================================*/
	class F_LogMessage : public F_Base
	{
	public:

		F_LogMessage()
		{
			m_uiBackColors = 0;
			m_uiForeColors = 0;
			m_eCurrentForeColor = FLM_LIGHTGRAY;
			m_eCurrentBackColor = FLM_BLACK;
		}

		virtual ~F_LogMessage()
		{
		}

		// Pure virtual functions

		virtual void changeColor(
			FlmColorType	eForeColor,
			FlmColorType	eBackColor) = 0;

		virtual void appendString(
			FLMBYTE *	pszStr) = 0;

		virtual void newline( void) = 0;

		virtual void endMessage( void) = 0;

		// Public methods

		void pushForegroundColor( void);

		void popForegroundColor( void);

		void pushBackgroundColor( void);

		void popBackgroundColor( void);

		FlmColorType getForegroundColor()
		{
			return( m_eCurrentForeColor);
		}

		FlmColorType getBackgroundColor()
		{
			return( m_eCurrentBackColor);
		}

		void setColor(
			FlmColorType	eForeColor,
			FlmColorType	eBackColor);

	private:

	#define F_MAX_COLOR_STACK_SIZE		8
		FlmColorType		m_eBackColors[ F_MAX_COLOR_STACK_SIZE];
		FlmColorType		m_eForeColors[ F_MAX_COLOR_STACK_SIZE];
		FLMUINT				m_uiBackColors;
		FLMUINT				m_uiForeColors;
		FlmColorType		m_eCurrentBackColor;
		FlmColorType		m_eCurrentForeColor;
	};

	/*--------------------------------------------------------
			 Storage and Native conversion functions
	**-------------------------------------------------------*/

	#define F_MAX_NUM_BUF		12

	RCODE FlmUINT2Storage(
		FLMUINT			uiNum, 
		FLMUINT *		puiLength, 
		FLMBYTE *		pBuffer);

	RCODE FlmINT2Storage(
		FLMINT			iNum, 
		FLMUINT *		puiLength, 
		FLMBYTE *		pBuffer);

	RCODE FlmStorage2UINT(
		FLMUINT			uiType,
		FLMUINT 			uiLength, 
		FLMBYTE *		pBuffer,
		FLMUINT *		puiNum);

	RCODE FlmStorage2UINT32(
		FLMUINT			uiType,
		FLMUINT 			uiLength, 
		FLMBYTE *		pBuffer,
		FLMUINT32 *		pui32Num);

	RCODE FlmStorage2INT(
		FLMUINT			uiType,
		FLMUINT 			uiLength, 
		FLMBYTE *		pBuffer,
		FLMINT *			piNum);

	RCODE FlmUnicode2Storage(
		FLMUNICODE *	puzStr,
		FLMUINT *		puiLength,
		FLMBYTE *		pBuffer);

	FLMUINT FlmGetUnicodeStorageLength(
		FLMUNICODE *	puzStr);

	RCODE FlmStorage2Unicode(
		FLMUINT			uiType,
		FLMUINT			uiBufLength,
		FLMBYTE *		pBuffer,
		FLMUINT *		puiStrBufLen,
		FLMUNICODE *	puzStrBuf);

	RCODE flmGetUnicode(
		FLMUINT			uiType,
		FLMUINT			uiBufLength,
		FLMBYTE *		pBuffer,
		FLMUINT *		puiStrBufLen,
		FLMUNICODE *	puzStrBuf);

	#define FlmStorage2Unicode( a, b, c, d, e) \
		flmGetUnicode( a, b, c, d, e)

	RCODE FlmGetUnicodeLength(
		FLMUINT		uiType,
		FLMUINT		uiBufLength,
		FLMBYTE *	pBuffer,
		FLMUINT *	puiUniLength);

	RCODE FlmNative2Storage(
		FLMBYTE *		pszStr,
		FLMUINT *		puiLength,
		FLMBYTE *		pBuffer);

	FLMUINT FlmGetNativeStorageLength(
		FLMBYTE *		pszStr);

	RCODE FlmStorage2Native(
		FLMUINT			uiType,
		FLMUINT			uiBufLength,
		FLMBYTE *		pBuffer,
		FLMUINT *		puiStrBufLen,
		FLMBYTE *		puzStrBuf);

	FLMUINT FlmGetNativeLength(
		FLMUINT			uiType,
		FLMUINT			uiBufLength,
		FLMBYTE *		pBuffer);

	/**************************************************************************
	*                  FLAIM Dictionary Tag Numbers 
	*
	* FLAIM Database Dictionary and Internal Tags
	*
	* FLAIM TEAM NOTES:
	*  1) These numbers cannot be changed for backward compatibility reasons.                     *
	*  2) IF ANY NEW TAGS ARE INSERTED - Then you MUST change the database
	*   version number, because old databases will become invalid.....
	*
	***************************************************************************/

	#define FLM_RESERVED_TAG_NUMS							32000

	// Special purpose container and index numbers

	#define FLM_DICT_CONTAINER							32000
	#define FLM_LOCAL_DICT_CONTAINER					FLM_DICT_CONTAINER
	#define FLM_DATA_CONTAINER							32001
	#define FLM_TRACKER_CONTAINER						32002
	#define FLM_DICT_INDEX								32003

	#define FLM_MISSING_FIELD_TAG						32049		// Used on CursorAddField call
	#define FLM_WILD_TAG									32050    // Wild card - matches everything

	// Range where unregistered fields begin

	#define FLM_FREE_TAG_NUMS							32769
	#define FLM_UNREGISTERED_TAGS						32769

	/*===========================================================================
								Dictionary Record Field Numbers

	WARNINGS:
		1) These numbers cannot be changed for backward compatibility reasons.
		2) Any Changes Made to any '_TAG' defines must be reflected in
			FlmDictTags table found in fntable.cpp
	===========================================================================*/

	#define FLM_TAGS_START								32100
	#define FLM_DICT_FIELD_NUMS						FLM_TAGS_START
	#define TS												FLM_TAGS_START

	#define FLM_FIELD_TAG								(TS +  0)
	#define FLM_FIELD_TAG_NAME							"Field"
	#define FLM_INDEX_TAG								(TS +  1)
	#define FLM_INDEX_TAG_NAME							"Index"
	#define FLM_TYPE_TAG									(TS +  2)
	#define FLM_TYPE_TAG_NAME							"Type"
	#define FLM_COMMENT_TAG								(TS +  3)
	#define FLM_COMMENT_TAG_NAME						"Comment"
	#define FLM_CONTAINER_TAG							(TS +  4)
	#define FLM_CONTAINER_TAG_NAME					"Container"
	#define FLM_LANGUAGE_TAG							(TS +  5)
	#define FLM_LANGUAGE_TAG_NAME						"Language"
	#define FLM_OPTIONAL_TAG							(TS +  6)
	#define FLM_OPTIONAL_TAG_NAME						"Optional"
	#define FLM_UNIQUE_TAG								(TS +  7)
	#define FLM_UNIQUE_TAG_NAME						"Unique"
	#define FLM_KEY_TAG									(TS +  8)
	#define FLM_KEY_TAG_NAME							"Key"
	#define FLM_REFS_TAG									(TS +  9)
	#define FLM_REFS_TAG_NAME							"Refs"
	#define FLM_ENCDEF_TAG								(TS + 10)
	#define FLM_ENCDEF_TAG_NAME						"EncDef"
	#define FLM_DELETE_TAG								(TS + 11)
	#define FLM_DELETE_TAG_NAME						"Delete"
	#define FLM_BLOCK_CHAIN_TAG						(TS + 12)
	#define FLM_BLOCK_CHAIN_TAG_NAME					"BlockChain"
	//#define FLM_NU_13_TAG								(TS + 13)
	//#define FLM_NU_14_TAG								(TS + 14)
	//#define FLM_NU_15_TAG								(TS + 15)
	//#define FLM_NU_16_TAG								(TS + 16)
	#define FLM_AREA_TAG									(TS + 17)
	#define FLM_AREA_TAG_NAME							"Area"
	//#define FLM_NU_18_TAG								(TS + 18)
	//#define FLM_NU_19_TAG								(TS + 19)
	//#define FLM_NU_20_TAG								(TS + 20)
	//#define FLM_NU_21_TAG								(TS + 21)
	//#define FLM_NU_22_TAG								(TS + 22)
	//#define FLM_NU_23_TAG								(TS + 23)  
	//#define FLM_NU_24_TAG								(TS + 24)  
	#define FLM_STATE_TAG								(TS + 25)
	#define FLM_STATE_TAG_NAME							"State"
	#define FLM_BLOB_TAG									(TS + 26)
	#define FLM_BLOB_TAG_NAME							"Blob"
	#define FLM_THRESHOLD_TAG							(TS + 27)
	#define FLM_THRESHOLD_TAG_NAME					"Threshold"
	//#define FLM_NU_28_TAG								(TS + 28)
	#define FLM_SUFFIX_TAG								(TS + 29)
	#define FLM_SUFFIX_TAG_NAME						"Suffix"
	#define FLM_SUBDIRECTORY_TAG						(TS + 30)
	#define FLM_SUBDIRECTORY_TAG_NAME				"Subdirectory"
	#define FLM_RESERVED_TAG							(TS + 31)
	#define FLM_RESERVED_TAG_NAME						"Reserved"
	#define FLM_SUBNAME_TAG								(TS + 32)
	#define FLM_SUBNAME_TAG_NAME						"Subname"
	#define FLM_NAME_TAG									(TS + 33)
	#define FLM_NAME_TAG_NAME							"Name"
	//#define FLM_NU_34_TAG								(TS + 34)
	//#define FLM_NU_35_TAG								(TS + 35) 
	#define FLM_BASE_TAG									(TS + 36)
	#define FLM_BASE_TAG_NAME							"Base"
	//#define FLM_NU_37_TAG								(TS + 37)
	#define FLM_CASE_TAG									(TS + 38)
	#define FLM_CASE_TAG_NAME							"Case"
	//#define FLM_NU_39_TAG								(TS + 39)
	#define FLM_COMBINATIONS_TAG						(TS + 40)
	#define FLM_COMBINATIONS_TAG_NAME				"Combinations"
	#define FLM_COUNT_TAG								(TS + 41)
	#define FLM_COUNT_TAG_NAME							"Count"
	#define FLM_POSITIONING_TAG						(TS + 42)
	#define FLM_POSITIONING_TAG_NAME					"Positioning"
	//#define FLM_NU_43_TAG								(TS + 43)
	#define FLM_PAIRED_TAG								(TS + 44)
	#define FLM_PAIRED_TAG_NAME						"Paired"
	#define FLM_PARENT_TAG								(TS + 45)
	#define FLM_PARENT_TAG_NAME						"Parent"
	#define FLM_POST_TAG									(TS + 46)
	#define FLM_POST_TAG_NAME							"Post"
	#define FLM_REQUIRED_TAG							(TS + 47)
	#define FLM_REQUIRED_TAG_NAME						"Required"
	#define FLM_USE_TAG									(TS + 48)
	#define FLM_USE_TAG_NAME							"Use"
	#define FLM_FILTER_TAG								(TS + 49)
	#define FLM_FILTER_TAG_NAME						"Filter"
	#define FLM_LIMIT_TAG								(TS + 50)
	#define FLM_LIMIT_TAG_NAME							"Limit"
	//#define FLM_NU_51_TAG								(TS + 51)
	//#define FLM_NU_52_TAG								(TS + 52)
	//#define FLM_NU_53_TAG								(TS + 53)
	#define FLM_DICT_TAG									(TS + 54)
	#define FLM_DICT_TAG_NAME							"Dict"
	//#define FLM_NU_55_TAG								(TS + 55)
	//#define FLM_NU_56_TAG								(TS + 56)
	//#define FLM_NU_57_TAG								(TS + 57)
	//#define FLM_NU_58_TAG								(TS + 58)
	//#define FLM_NU_59_TAG								(TS + 59)  
	//#define FLM_NU_60_TAG								(TS + 60) 
	//#define FLM_NU_61_TAG								(TS + 61)
	//#define FLM_NU_62_TAG								(TS + 62)
	//#define FLM_NU_63_TAG								(TS + 63)
	//#define FLM_NU_64_TAG								(TS + 64)
	//#define FLM_NU_65_TAG								(TS + 65)
	//#define FLM_NU_66_TAG								(TS + 66)
	//#define FLM_NU_67_TAG								(TS + 67)
	//#define FLM_NU_68_TAG								(TS + 68)
	//#define FLM_NU_69_TAG								(TS + 69)
	#define FLM_RECINFO_TAG								(TS + 70)
	#define FLM_RECINFO_TAG_NAME						"RecInfo"
	#define FLM_DRN_TAG									(TS + 71)
	#define FLM_DRN_TAG_NAME							"Drn"
	#define FLM_DICT_SEQ_TAG							(TS + 72)
	#define FLM_DICT_SEQ_TAG_NAME						"DictSeq"
	#define FLM_LAST_CONTAINER_INDEXED_TAG			(TS + 73)
	#define FLM_LAST_CONTAINER_INDEXED_TAG_NAME	"LastContainerIndexed"
	#define FLM_LAST_DRN_INDEXED_TAG					(TS + 74)
	#define FLM_LAST_DRN_INDEXED_TAG_NAME			"LastDrnIndexed"
	#define FLM_ONLINE_TRANS_ID_TAG					(TS + 75)
	#define FLM_ONLINE_TRANS_ID_TAG_NAME			"OnlineTransId"
	#define FLM_LAST_DICT_FIELD_NUM					(TS + 75)

	/*============================================================================
	Dictionary Record Definitions - below are comments that document valid 
	dictionary objects and their structure.
	============================================================================*/

	/*
	Field Definition
	Desc: The below syntax is used to define a field within a database dictionary
			container.

	0 [@<ID>@] field <name>          # FLM_FIELD_TAG
	| 1 type <below>                 # FLM_TYPE_TAG
			{context|number|text|binary|real|date|time|tmstamp|blob}]
	[ 1 state <below>                # FLM_STATE_TAG - what is the state of the field
			{ *active                  #  The field is active (being used).
			| checking                 #  User request to determine if field is used.
												#  This is done by calling FlmDbSweep.
			| unused                   #  Result of 'checking'. Field is not used and
												#  maybe deleted. Note: a field in this state
												#  may still have other dictionary item that
												#  are referencing it.
			| purge}]                  #  Remove all fld occurances, and delete def.
	*/
	
	/*
	 Encryption Definition
	 Desc: The below syntax is used to define an encryption definition record.
	 0 [@<ID>@] EncDef <name>		# FLM_ENCDEF_TAG
	 	1 type <below>						# FLM_TYPE_TAG
	 		{ des3 | aes }
	 */

	/*
	Container Definition
	Desc: The below syntax is used to define a container within a database dictionary
			container.

	0 [@<ID>@] container <name>      # FLM_CONTAINER_TAG
	*/

	/*
	Area Definition
	Desc: An area allows the application to define a logical location that
			blob files can be placed.

	0 [@<ID>@] area <name>           # FLM_AREA_TAG
	[{1 base* 0* | <ID>}             # FLM_BASE_TAG - 0 = same area a DB location
	  [ 2 subdirectory <string>]     # FLM_SUBDIRECTORY_TAG
			3 subd ...
	|{1 machine pc|mac|unix}...      # FLM_MACHINE_TAG
	  [ 2 order <driver>[;<driver>]...] # FLM_ORDER_TAG
		 2 driver .* | <tag>          # FLM_DRIVER_TAG
		 [ 3 directory <string>]      # FLM_DIRECTORY_TAG
	]
	[ 1 blob]                        # FLM_BLOB_TAG
	  [ 2 options compress,encrypt,checksum]  # FLM_OPTIONS_TAG
	  [ 2 threshold 1* | <#>]        # FLM_THRESHOLD_TAG - k-bytes before moved to
												#  external file
	[ 1 subname]                     # FLM_SUBNAME_TAG - create/use subdir named:
	  [ 2 prefix* "SDIR_"* | <str>   # FLM_PREFIX_TAG - 1-5 character prefix
	  [ 2 suffix* 1* | <#>           # FLM_SUFFIX_TAG - max < 4k, dir suffix
	*/

	/*
	Reserved Dictionary Record
	Desc: Allows user to reserve a typeless ID in a dictionary.

	0 [@<ID>@] reserved <name>       # FLM_RESERVED_TAG
	*/

	/*
	Index Definition
	Desc: Below is the syntax that is used to define a FLAIM index.

	0 [@<ID>@] index <psName>        # FLM_INDEX_TAG
	[ 1 area** 0** | <ID> ]          # FLM_AREA_TAG - QF files area, 0 = "same as DB"
	[ 1 container* DEFAULT* | <ID> ] # FLM_CONTAINER_TAG - indexes span only one container
	[ 1 count KEYS &| REFS* ]        # FLM_COUNT_TAG - key count of keys and/or refs
	[ 1 language* US* | <language> ] # FLM_LANGUAGE_TAG - for full-text parsing and/or sorting

	  1 key [EACHWORD]					# FLM_KEY_TAG - 'use' defaults based on type
	  [ 2 base <ID>]                 # FLM_BASE_TAG - base rec/field for fields below
	  [ 2 combinations*              # FLM_COMBINATIONS_TAG - how to handle repeating fields
			ALL | NORMALIZED* ]
	  [ 2 post]                      # FLM_POST_TAG - case-flags post-pended to key
	  [ 2 required*]                 # FLM_REQUIRED_TAG - key value is required
	  [ 2 unique]                    # FLM_UNIQUE_TAG - key has only 1 reference
	  { 2 <field> }...               # FLM_FIELD_TAG - compound key if 2 or more
		 [ 3 case* mixed* | upper]    # FLM_CASE_TAG - text-only, define chars case
		 [ 3 <field>]...              # FLM_FIELD_TAG - alternate field(s)
		 [ 3 paired** ]               # FLM_PAIRED_TAG - add field ID to key
		 [ 3 optional*                # FLM_OPTIONAL_TAG - component's value is optional
		  |3 required ]               # FLM_REQUIRED_TAG - component's value is required
		{[ 3 use* eachword**|value*|field]} # FLM_USE_TAG
		{[ 3 filter minspace|nodash|nounderscore|minspaces]} # FLM_FILTER_TAG
		 [ 3 limit {256 | limit}]

	<field> ==
	  n field <field path>           #  path identifies field -- maybe "based"
	  [ m type <data type>]          # FLM_TYPE_TAG - only for ixing unregistered fields
	*/


	/*============================================================================
			
	NON-Dictionary Record Definitions - the following record definitions are for
	internal FLAIM usage.

	============================================================================*/

	/*
	Deleted BLOB Tracker Record
	Desc: BLOB tracker records are found in the FLM_TRACKER_CONTAINER.
			It records blob files that are on ready to be deleted. 
			BDELETE records are single-field records.  The fields are BLOB fields.

	0 bdelete <BLOB>                 # FLM_BLOB_DELETE_TAG - single-field record;
												#  DRN > 65,536
	*/

	/*
	Record Info Record - for EXPORT/IMPORT
	Desc: The Record Info Record is currently only used in export/import files.
			It contains the record information for each exported record.

	0 recinfo                        # FLM_RECINFO_TAG
	  1 drn <#>                      # FLM_DRN_TAG - DRN for the record
	[ 1 dseq <#>]                    # FLM_DICT_SEQ_TAG - dictionary sequence ID for the record
	*/

	RCODE FlmKeyBuild(
		HFDB        hDb,
		FLMUINT     uiIxNum,
		FLMUINT		uiContainer,
		FlmRecord *	pKeyTree,
		FLMUINT		uiFlags,
		FLMBYTE *   pKeyBuf,
		FLMUINT *   puiKeyLenRV);

	#if FLM_ALIGN_SIZE == 8
		typedef FLMUINT32		FIELDLINK;
	#else
		typedef FLMUINT16		FIELDLINK;
	#endif

	/*============================================================================
	Struct: 	FlmField
	============================================================================*/
	typedef struct
	{
		FLMUINT32	ui32DataOffset;
		FLMUINT16	ui16FieldID;
		FLMUINT8		ui8DataLen;
		FLMUINT8		ui8TypeAndLevel;

			// Bits 0 - 2 used for type

			#define FLD_TEXT_TYPE				0x00
			#define FLD_NUMBER_TYPE				0x01
			#define FLD_BINARY_TYPE				0x02
			#define FLD_CONTEXT_TYPE			0x03
			#define FLD_BLOB_TYPE				0x04

			// Bits 3 - 4 used for flags

			#define FLD_DATA_LEFT_TRUNCATED	0x08
			#define FLD_DATA_RIGHT_TRUNCATED	0x10

			// Bits 5 - 7 for level (max level is 7)

		FIELDLINK	uiPrev;
		FIELDLINK	uiNext;

	} FlmField;

	/*============================================================================
	Desc: 	Class which provides the record interface that FLAIM uses to
				access and manipulate all records.
	============================================================================*/
	class FlmRecord : public F_Base
	{
	public:

		#define RCA_READ_ONLY_FLAG				0x00000001
		#define RCA_CACHED						0x00000002
		#define RCA_OK_TO_DELETE				0x00000004
		#define RCA_OLD_VERSION					0x00000008

		FlmRecord();

		~FlmRecord();

		void * operator new(
			FLMSIZET			uiSize)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new(
			FLMSIZET			uiSize,
			const char *	pszFile,
			int				iLine)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new[](
			FLMSIZET			uiSize)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void * operator new[](
			FLMSIZET			uiSize,
			const char *	pszFile,
			int				iLine)
		#if !defined( FLM_NLM)
			throw()
		#endif
			;

		void operator delete(
			void *			ptr);

		void operator delete[](
			void *			ptr);

	#if defined( FLM_DEBUG) && !defined( __WATCOMC__)
		void operator delete(
			void *			ptr,
			char *			file,
			int				line);
	#endif

	#if defined( FLM_DEBUG) && !defined( __WATCOMC__)
		void operator delete[](
			void *			ptr,
			char *			file,
			int				line);
	#endif

		inline FLMUINT AddRef( void)
		{
			return( AddRef( FALSE));
		}

		inline FLMUINT Release( void)
		{
			return( Release( FALSE));
		}

		FlmRecord * copy( void);

		RCODE clear(
			FLMBOOL			bReleaseMemory = FALSE);

		RCODE getINT( 
			void *			pvField,
			FLMINT *			piNumber);

		RCODE getUINT( 
			void *			pvField,
			FLMUINT *		puiNumber);

		RCODE getUINT32(
			void *			pvField,
			FLMUINT32 *		pui32Number);

		inline RCODE getUnicodeLength( 
			void *			pvField,
			FLMUINT *		puiLength)
		{
			if( pvField)
			{
				return( FlmGetUnicodeLength( 
					getDataType( pvField), getDataLength( pvField), 
					getDataPtr( pvField), puiLength));
			}

			return( FERR_NOT_FOUND);
		}

		RCODE getUnicode( 
			void *			pvField,
			FLMUNICODE *	pUnicode, 
			FLMUINT *		puiBufLen);

		inline RCODE getNativeLength( 
			void *			pvField,
			FLMUINT *		puiLength)
		{
			if( pvField)
			{
				*puiLength = FlmGetNativeLength( 
									getDataType( pvField), getDataLength( pvField),
									getDataPtr( pvField));
				return FERR_OK;
			}

			return( FERR_NOT_FOUND);
		}

		RCODE getNative( 
			void *			pvField,
			FLMBYTE *		pszString, 
			FLMUINT *		puiBufLen);

		inline RCODE getBinaryLength( 
			void *			pvField,
			FLMUINT *		puiLength)
		{
			if( pvField)
			{
				*puiLength = getDataLength( pvField);
				return FERR_OK;
			}

			return( FERR_NOT_FOUND);
		}

		RCODE getRecPointer(
			void *			pvField,
			FLMUINT *		puiRecPointer);

		inline RCODE getRecPointer32( 
			void *			pvField, 
			FLMUINT32 *		pui32RecPointer)
		{
			FLMUINT	uiRecPointer;
			RCODE		rc;
			
			rc = getRecPointer( pvField, &uiRecPointer);
			*pui32RecPointer = (FLMUINT32)uiRecPointer;

			return( rc);
		}

		RCODE getBinary(
			void *			pvField,
			void *			pvBuf,
			FLMUINT *		puiBufLen);

		RCODE getBlob(
			void *			pvField,
			FlmBlob **		ppBlob);

		RCODE setINT(
			void *			pvField,
			FLMINT			iNumber,
			FLMUINT			uiEncId = 0);

		RCODE setUINT( 
			void *			pvField,
			FLMUINT			uiNumber,
			FLMUINT			uiEncId = 0);

		RCODE setRecPointer(
			void *			pvField,
			FLMUINT			uiRecPointer,
			FLMUINT			uiEncId = 0);

		RCODE setUnicode(
			void *			pvField,
			FLMUNICODE *	puzUnicode,
			FLMUINT			uiEncId = 0);

		RCODE setNative(
			void *			pvField,
			FLMBYTE *		pszString,
			FLMUINT			uiEncId = 0);

		RCODE setBinary(
			void *			pvField,
			void *			pvBuf,
			FLMUINT			uiBufLen,
			FLMUINT			uiEncId = 0);

		RCODE setBlob( 
			void *			pvField,
			FlmBlob *		pBlob,
			FLMUINT			uiEncId = 0);

		#define INSERT_PREV_SIB			1
		#define INSERT_NEXT_SIB			2
		#define INSERT_FIRST_CHILD		3
		#define INSERT_LAST_CHILD		4

		RCODE insert(
			void *			pvField,
			FLMUINT			uiInsertAt, 
			FLMUINT			uiFieldID,
			FLMUINT			uiDataType,
			void **			ppvField);

		RCODE insertLast( 
			FLMUINT			uiLevel,
			FLMUINT			uiFieldID, 
			FLMUINT			uiDataType,
			void **			ppvField);

		inline RCODE remove(
			void *			pvField)
		{
			return remove( getFieldPointer( pvField));
		}

		inline void * root( void)
		{
			if( m_uiFldTblOffset)
			{
				return( (void *)1);
			}

			return( NULL);
		}

		inline void * nextSibling(
			void *			pvField)
		{
			return( pvField 
						? getFieldVoid( nextSiblingField( 
								getFieldPointer( pvField)))
						: NULL);
		}

		inline void * firstChild( 
			void *			pvField)
		{
			return( pvField 
						? getFieldVoid( firstChildField( 
								getFieldPointer( pvField)))
						: NULL);
		}

		inline void * next(
			void *			pvField)
		{
			return( pvField 
						? getFieldVoid( nextField( 
								getFieldPointer( pvField)))
						: NULL);
		}

		inline void * prev(
			void *			pvField)
		{
			return( pvField 
						? getFieldVoid( prevField( 
								getFieldPointer( pvField)))
						: NULL);
		}

		void * prevSibling( 
			void *			pvField);

		inline void * lastChild( 
			void *			pvField)
		{
			return( getFieldVoid( 
				lastChildField( getFieldPointer( pvField))));
		}

		inline void * parent( 
			void *			pvField)
		{
			return parent( getFieldPointer( pvField));
		}

		#define SEARCH_TREE		1	
		#define SEARCH_FOREST	2

		void * find(
			void *			pvStartField,
			FLMUINT			uiFieldID,
			FLMUINT			uiOccur = 1, 
			FLMUINT			uiFindOption = SEARCH_FOREST);

		void * find( 
			void *			pvStartField,
			FLMUINT *		puiFieldPath,
			FLMUINT			uiOccur = 1, 
			FLMUINT			uiFindOption = SEARCH_FOREST);

		inline FLMUINT getLevel(
			void *			pvField)
		{
			return( getFieldLevel( getFieldPointer( pvField)));
		}

		inline FLMUINT getFieldID(
			void *			pvField)
		{
			return( getFieldPointer( pvField)->ui16FieldID);
		}

		inline void setFieldID(
			void *			pvField,
			FLMUINT			uiFieldID)
		{
			if( uiFieldID)
			{
				getFieldPointer( pvField)->ui16FieldID = (FLMUINT16)uiFieldID;
			}
		}

		inline FLMUINT getDataType(
			void *			pvField)
		{
			return( getFieldDataType( getFieldPointer( pvField)));
		}

		inline FLMUINT getDataLength(
			void *			pvField)
		{
			return( getFieldDataLength( getFieldPointer( pvField)));
		}

		inline FLMBOOL hasChild( 
			void *			pvField)
		{
			return( firstChildField( 
				getFieldPointer( pvField)) != NULL) ? TRUE : FALSE;
		}

		inline FLMBOOL isLast( 
			void *			pvField)
		{
			return( nextField( 
				getFieldPointer( pvField)) == NULL) ? TRUE : FALSE;
		}

		inline void setRightTruncated( 
			void *			pvField,
			FLMBOOL			bTrueFalse)
		{
			setRightTruncated( getFieldPointer( pvField),
									 bTrueFalse);
		}

		inline FLMBOOL isRightTruncated(
			void *			pvField)
		{
			return( isRightTruncated( getFieldPointer( pvField)));
		}

		inline void setLeftTruncated( 
			void *			pvField,
			FLMBOOL			bTrueFalse)
		{
			setLeftTruncated( getFieldPointer( pvField),
									bTrueFalse);
		}

		inline FLMBOOL isLeftTruncated( 
			void *			pvField)
		{
			return( isLeftTruncated( getFieldPointer( pvField)));
		}

		inline RCODE getFieldInfo(
			void *			pvField,
			FLMUINT *		puiFieldID, 
			FLMUINT *		puiLevel, 
			FLMUINT *		puiDataType,	
			FLMUINT *		puiLength,
			FLMUINT *		puiEncLength,
			FLMUINT *		puiEncId)
		{
			FlmField *	pField = getFieldPointer( pvField);

			*puiFieldID = pField->ui16FieldID;
			*puiLevel	= getLevel( pvField);
			*puiLength	= getDataLength( pvField);
			*puiDataType = getDataType( pvField);

			if (isEncryptedField( pField))
			{
				if (puiEncLength)
				{
					*puiEncLength = getEncryptedDataLength( pField);
				}

				if (puiEncId)
				{
					*puiEncId = getEncryptionID( pField);
				}
			}
			else
			{
				if (puiEncLength)
				{
					*puiEncLength = 0;
				}
				if (puiEncId)
				{
					*puiEncId = 0;
				}
			}

			return FERR_OK;
		}

		RCODE preallocSpace( 
			FLMUINT			uiFieldCount,
			FLMUINT			uiDataSize);

		RCODE allocStorageSpace(
			void *			pvField,
			FLMUINT			uiDataType,
			FLMUINT			uiLength,
			FLMUINT			uiEncLength,
			FLMUINT			uiEncId,
			FLMUINT			uiFlags,
			FLMBYTE **		ppDataPtr,
			FLMBYTE **		ppEncDataPtr);

		FLMUINT getTotalMemory( void);

		inline FLMUINT getFreeMemory( void)
		{
			return( ((m_uiFldTblSize - m_uiFldTblOffset) * sizeof( FlmField)) +
				(m_uiAvailFields * sizeof( FlmField)) +
				(getDataBufSize() - m_uiDataBufOffset));
		}

		RCODE compressMemory( void);

		inline FLMUINT getID( void)
		{
			return( m_uiRecordID);
		}

		inline void setID(
			FLMUINT			uiRecordID)
		{
			m_uiRecordID = uiRecordID;
		}

		inline FLMUINT getContainerID( void)
		{
			return( m_uiContainerID);
		}

		inline void setContainerID(
			FLMUINT			uiContainerID)
		{
			m_uiContainerID = uiContainerID;
		}

		RCODE importRecord(
			F_FileHdl *		pFileHdl,
			F_NameTable *	pNameTable);

		RCODE importRecord(
			FLMBYTE **		ppBuffer,
			FLMUINT			uiBufSize,
			F_NameTable *	pNameTable);

		RCODE importRecord(
			NODE_p			pNode);

		RCODE exportRecord(
			HFDB				hDb,
			POOL *			pPool,
			NODE_p *			ppNode);

		inline FLMBOOL isReadOnly( void)
		{
			return( (m_uiFlags & RCA_READ_ONLY_FLAG) ? TRUE : FALSE);
		}

		inline FLMBOOL isCached( void)
		{
			return( (m_uiFlags & RCA_CACHED) ? TRUE : FALSE);
		}

		inline FLMBOOL isOldVersion( void)
		{
			return( (m_uiFlags & RCA_OLD_VERSION) ? TRUE : FALSE);
		}

		inline FLMBYTE * getDataPtr(
			void *			pvField)
		{
			return( getDataPtr( getFieldPointer( pvField)));
		}

		inline FLMBOOL isEncryptedField(
			void *			pvField)
		{
			return isEncryptedField( getFieldPointer( pvField));
		}

		inline FLMBYTE * getEncryptionDataPtr(
			void *			pvField)
		{
			return( getEncryptionDataPtr( getFieldPointer( pvField)));
		}

		inline FLMUINT getEncryptedDataLength(
			void *			pvField)
		{
			return getEncryptedDataLength( getFieldPointer(pvField));
		}
		
		FLMUINT getEncryptionID( 
			void *			pvField)
		{
			return getEncryptionID( getFieldPointer( pvField));
		}
			
		FLMUINT getEncFlags(
			void *			pvField)
		{
			return getEncFlags( getFieldPointer( pvField));
		}
		
		void setEncFlags(
			void *			pvField,
			FLMUINT			uiFlags)
		{
			setEncFlags( getFieldPointer( pvField), uiFlags);
		}

		void * locateFieldByPosition(
			FLMUINT			uiPosition);

		
		RCODE checkRecord( void);

		RCODE checkField(
			FlmField *			pFld);

#define FLD_HAVE_ENCRYPTED_DATA		0x01
#define FLD_HAVE_DECRYPTED_DATA		0x02
#define FLD_PICKET_FENCE_SIZE			8		// Only used in debug builds in encrypted fields
														// Represents the size of two picket fences.
#define FLD_RAW_FENCE					"RAWD"
#define FLD_ENC_FENCE					"ENCD"

	private:

		FLMUINT AddRef( 
			FLMBOOL			bMutexLocked);

		FLMUINT Release( 
			FLMBOOL			bMutexLocked);

		void * parent( 
			FlmField *		pField);

		inline FLMUINT getFieldLevel(
			FlmField *		pField)
		{
			return( (pField->ui8TypeAndLevel & 0xE0) >> 5);
		}

		inline void setReadOnly( void)
		{
			m_uiFlags |= RCA_READ_ONLY_FLAG;
		}

		inline void setCached( void)
		{
			m_uiFlags |= RCA_CACHED;
		}

		inline void clearCached( void)
		{
			m_uiFlags &= ~RCA_CACHED;
		}

		inline void setOldVersion( void)
		{
			m_uiFlags |= RCA_OLD_VERSION;
		}

		inline void clearOldVersion( void)
		{
			m_uiFlags &= ~RCA_OLD_VERSION;
		}

		void * getFieldVoid(
			FlmField *		pField);

		FlmField * getFieldPointer(
			void *			pvField);

		FLMBYTE * getDataPtr(
			FlmField *		pField);

		FlmField * nextSiblingField(
			FlmField *		pField);

		FlmField * lastSubTreeField( 
			FlmField *		pField);

		RCODE createField( 
			FlmField *		pPrevField,
			FlmField **		ppNewField); 

		RCODE removeFields(
			FlmField *		pFirstField,
			FlmField *		pLastField = NULL);

		RCODE copyFields( 
			FlmField *		pSrcFields);

		RCODE getNewDataPtr(
			FlmField *		pField,
			FLMUINT			uiDataType,
			FLMUINT			uiNewLength,
			FLMUINT			uiNewEncLength,
			FLMUINT			uiEncId,
			FLMUINT			uiFlags,
			FLMBYTE **		ppDataPtr,
			FLMBYTE **		ppEncDataPtr);

		inline FlmField * firstChildField( 
			FlmField *		pField)
		{
			FLMUINT	uiLevel = getLevel( getFieldVoid( pField));

			return ((pField = nextField( pField)) != NULL && 
						getLevel( getFieldVoid( pField)) > uiLevel)
							? pField
							: NULL;
		}

		FlmField * lastChildField(
			FlmField *		pField);

		FlmField * getFirstField( void);

		FlmField * getLastField( void);

		FlmField * prevField( 
			FlmField *		pField);

		FlmField * nextField(
			FlmField *		pField);

		RCODE setFieldLevel(
			FlmField *		pField,
			FLMUINT			uiLevel);

		void setFieldDataType(
			FlmField *		pField,
			FLMUINT			uiDataType);

		inline FLMUINT getFieldDataType(
			FlmField *			pField)
		{
			FLMUINT	uiFldType;

			if( (uiFldType = pField->ui8TypeAndLevel & 0x07) <= 3)
			{
				return( uiFldType);
			}

			return( FLM_BLOB_TYPE);
		}

		FLMUINT getFieldDataLength(
			FlmField *			pField);

		inline FlmField * getFieldTable( void)
		{
			return( (FlmField *)(m_pucBuffer + FLM_ALIGN_SIZE));
		}

		inline FLMUINT getDataBufSize( void)
		{
			return( (FLMUINT)((m_pucBuffer + m_uiBufferSize) - 
				(FLMBYTE *)(&(getFieldTable()[ m_uiFldTblSize]))));
		}

		inline FLMBYTE * getDataBufPtr( void)
		{
			return( (FLMBYTE *)(&(getFieldTable()[ m_uiFldTblSize])));
		}

		RCODE compactMemory( void);

		FLMBOOL isEncryptedField(
			FlmField *			pField);

		FLMBYTE * getEncryptionDataPtr(
			FlmField *		pField);
			
		FLMUINT getEncryptedDataLength(
			FlmField *		pField);

		FLMUINT getEncryptionID( 
			FlmField *			pField);
			
		FLMUINT getEncFlags(
			FlmField *			pField);
		
		void setEncFlags(
			FlmField *		pField,
			FLMUINT			uiFlags);
			
		void setEncHeader(
			FLMBYTE *		pBuffer,
			FLMUINT			uiFlags,
			FLMUINT			uiEncId,
			FLMUINT			uiNewLength,
			FLMUINT			uiEncNewLength);

		void setRightTruncated( 
			FlmField *		pField,
			FLMBOOL			bTrueFalse);

		void setLeftTruncated( 
			FlmField *		pField,
			FLMBOOL			bTrueFalse);

		inline FLMBOOL isRightTruncated(
			FlmField *		pField)
		{
			return( pField->ui8TypeAndLevel & 
							FLD_DATA_RIGHT_TRUNCATED
						? TRUE 
						: FALSE);
		}

		inline FLMBOOL isLeftTruncated(
			FlmField *		pField)
		{
			return( pField->ui8TypeAndLevel & 
							FLD_DATA_LEFT_TRUNCATED
						? TRUE 
						: FALSE);
		}
		
		RCODE remove(
			FlmField *		pField);

		FLMUINT		m_uiContainerID;
		FLMUINT		m_uiRecordID;
		FLMUINT		m_uiFlags;
		FLMBYTE *	m_pucBuffer;
		FLMUINT		m_uiBufferSize;
		FLMUINT		m_uiFldTblSize;
		FLMUINT		m_uiFldTblOffset;
		FLMUINT		m_uiDataBufOffset;
		FLMBOOL		m_bHolesInData;
		FLMUINT		m_uiAvailFields;
		FIELDLINK	m_uiFirstAvail;

		friend struct FlmRecordExt;
		friend class F_Rfl;
	};

	// Define FlmDefaultRec for existing applications that do a new
	// on it

	#define FlmDefaultRec	FlmRecord

	RCODE flmCurPerformRead(
		eFlmFuncs		eFlmFuncId,
		HFCURSOR 		hCursor,
		FLMBOOL			bReadForward,
		FLMBOOL			bSetFirst,
		FLMUINT *		puiSkipCount,
		FlmRecord **	ppRecord,
		FLMUINT *		puiDrn );

	/*
	Desc : Positions the cursor to the first item in a set defined by a cursor
			 and retrieves the corresponding record from the database.
	*/
	inline RCODE FlmCursorFirst(
		HFCURSOR 			hCursor,
				// [IN] Handle to a cursor.
		FlmRecord **		ppRecord)
				// [OUT] Pointer to a FlmRecord.  *ppRecord will be non-NULL if the
				// call is successful. Otherwise, the value of *ppRecord will be NULL.
	{
		return flmCurPerformRead( FLM_CURSOR_FIRST, 
			hCursor, TRUE, TRUE, NULL, ppRecord, NULL);
	}

	/*
	Desc : Positions the cursor to the last item in a set defined by a cursor
			 and retrieves the corresponding record from the database.
	*/
	inline RCODE FlmCursorLast(
		HFCURSOR				hCursor,
				// [IN] Handle to a cursor.
		FlmRecord **		ppRecord)
				// [OUT] Pointer to a FlmRecord.  *ppRecord will be non-NULL if the
				// call is successful. Otherwise, the value of *ppRecord will be NULL.
	{
		return flmCurPerformRead( FLM_CURSOR_LAST, hCursor,
			FALSE, TRUE, NULL, ppRecord, NULL);
	}

	/*
	Desc : Positions the cursor to the next item in a set defined by a cursor
			 and retrieves the corresponding record from the database.
	*/
	inline RCODE FlmCursorNext(
		HFCURSOR				hCursor,
				// [IN] Handle to a cursor.
		FlmRecord **		ppRecord)
				// [OUT] Pointer to a FlmRecord.  *ppRecord will be non-NULL if the
				// call is successful. Otherwise, the value of *ppRecord will be NULL.
	{
		return flmCurPerformRead( FLM_CURSOR_NEXT, hCursor, 
			TRUE, FALSE, NULL, ppRecord, NULL);
	}

	/*
	Desc : Positions the cursor to the previous item in a set defined by a cursor
			 and retrieves the corresponding record from the database.
	*/
	inline RCODE FlmCursorPrev(
		HFCURSOR				hCursor,
				// [IN] Handle to a cursor.
		FlmRecord **		ppRecord)
				// [OUT] Pointer to a FlmRecord.  *ppRecord will be non-NULL if the
				// call is successful. Otherwise, the value of *ppRecord will be NULL.
	{
		return flmCurPerformRead( FLM_CURSOR_PREV, hCursor,
			FALSE, FALSE, NULL, ppRecord, NULL);
	}

	/*
	Desc : Positions the cursor to the first item in a set defined by a cursor
			 and returns the DRN of the corresponding record.
	*/
	inline RCODE FlmCursorFirstDRN(
		HFCURSOR 	hCursor,
				// [IN] Handle to a cursor.
		FLMUINT *	puiDrn)
				// [OUT] Pointer to a DRN.  If the call is successful, the value
				// of *puiDrn will be the DRN of the first record.
	{
		return flmCurPerformRead( FLM_CURSOR_FIRST_DRN, hCursor, 
			TRUE, TRUE, NULL, NULL, puiDrn);
	}

	/*
	Desc : Positions the cursor to the last item in a set defined by a cursor
			 and returns the DRN of the corresponding record.
	*/
	inline RCODE FlmCursorLastDRN(
		HFCURSOR 	hCursor,
				// [IN] Handle to a cursor.
		FLMUINT *	puiDrn)
				// [OUT] Pointer to a DRN.  If the call is successful, the value
				// of *puiDrn will be the DRN of the last record.
	{
		return flmCurPerformRead( FLM_CURSOR_LAST_DRN, hCursor, 
			FALSE, TRUE, NULL, NULL, puiDrn);
	}

	/*
	Desc : Positions the cursor to the next item in a set defined by a cursor
			 and returns the DRN of the corresponding record.
	*/
	inline RCODE FlmCursorNextDRN(
		HFCURSOR		hCursor,
				// [IN] Handle to a cursor.
		FLMUINT *	puiDrn)
				// [OUT] Pointer to a DRN.  If the call is successful, the value
				// of *puiDrn will be the DRN of the next record.
	{
		return flmCurPerformRead( FLM_CURSOR_NEXT_DRN, hCursor, 
			TRUE, FALSE, NULL, NULL, puiDrn);
	}

	/*
	Desc : Positions the cursor to the previous item in a set defined by a cursor
			 and returns the DRN of the corresponding record.
	*/
	inline RCODE FlmCursorPrevDRN(
		HFCURSOR		hCursor,
				// [IN] Handle to a cursor.
		FLMUINT *	puiDrn)
				// [OUT] Pointer to a DRN.  If the call is successful, the value
				// of *puiDrn will be the DRN of the previous record.
	{
		return flmCurPerformRead( FLM_CURSOR_PREV_DRN, hCursor,
			FALSE, FALSE, NULL, NULL, puiDrn);
	}

	// CheckDb Error Codes

	#define FLM_BAD_CHAR							1
	#define FLM_BAD_ASIAN_CHAR					2
	#define FLM_BAD_CHAR_SET					3
	#define FLM_BAD_TEXT_FIELD					4
	#define FLM_BAD_NUMBER_FIELD				5
	#define FLM_BAD_CONTEXT_FIELD				6
	#define FLM_BAD_FIELD_TYPE					7
	#define FLM_BAD_IX_DEF						8
	#define FLM_MISSING_REQ_KEY_FIELD		9
	#define FLM_BAD_TEXT_KEY_COLL_CHAR		10
	#define FLM_BAD_TEXT_KEY_CASE_MARKER	11
	#define FLM_BAD_NUMBER_KEY					12
	#define FLM_BAD_CONTEXT_KEY				13
	#define FLM_BAD_BINARY_KEY					14
	#define FLM_BAD_DRN_KEY						15
	#define FLM_BAD_KEY_FIELD_TYPE			16
	#define FLM_BAD_KEY_COMPOUND_MARKER		17
	#define FLM_BAD_KEY_POST_MARKER			18
	#define FLM_BAD_KEY_POST_BYTE_COUNT		19
	#define FLM_BAD_KEY_LEN						20
	#define FLM_BAD_LFH_LIST_PTR				21
	#define FLM_BAD_LFH_LIST_END				22
	#define FLM_BAD_PCODE_LIST_END			23
	#define FLM_BAD_BLK_END						24
	#define FLM_KEY_COUNT_MISMATCH			25
	#define FLM_REF_COUNT_MISMATCH			26
	#define FLM_BAD_CONTAINER_IN_KEY			27
	#define FLM_BAD_BLK_HDR_ADDR				28
	#define FLM_BAD_BLK_HDR_LEVEL				29
	#define FLM_BAD_BLK_HDR_PREV				30
	#define FLM_BAD_BLK_HDR_NEXT				31
	#define FLM_BAD_BLK_HDR_TYPE				32
	#define FLM_BAD_BLK_HDR_ROOT_BIT			33
	#define FLM_BAD_BLK_HDR_BLK_END			34
	#define FLM_BAD_BLK_HDR_LF_NUM			35
	#define FLM_BAD_AVAIL_LIST_END			36
	#define FLM_BAD_PREV_BLK_NEXT				37
	#define FLM_BAD_FIRST_ELM_FLAG			38
	#define FLM_BAD_LAST_ELM_FLAG				39
	#define FLM_BAD_LEM							40
	#define FLM_BAD_ELM_LEN						41
	#define FLM_BAD_ELM_KEY_SIZE				42
	#define FLM_BAD_ELM_PKC_LEN				43
	#define FLM_BAD_ELM_KEY_ORDER				44
	#define FLM_BAD_ELM_KEY_COMPRESS			45
	#define FLM_BAD_CONT_ELM_KEY				46
	#define FLM_NON_UNIQUE_FIRST_ELM_KEY	47
	#define FLM_BAD_ELM_FLD_OVERHEAD			48
	#define FLM_BAD_ELM_FLD_LEVEL_JUMP		49
	#define FLM_BAD_ELM_FLD_NUM				50
	#define FLM_BAD_ELM_FLD_LEN				51
	#define FLM_BAD_ELM_FLD_TYPE				52
	#define FLM_BAD_ELM_END						53
	#define FLM_BAD_PARENT_KEY					54
	#define FLM_BAD_ELM_DOMAIN_SEN			55
	#define FLM_BAD_ELM_BASE_SEN				56
	#define FLM_BAD_ELM_IX_REF					57
	#define FLM_BAD_ELM_ONE_RUN_SEN			58
	#define FLM_BAD_ELM_DELTA_SEN				59
	#define FLM_BAD_ELM_DOMAIN					60
	#define FLM_BAD_LAST_BLK_NEXT				61
	#define FLM_BAD_FIELD_PTR					62
	#define FLM_REBUILD_REC_EXISTS			63
	#define FLM_REBUILD_KEY_NOT_UNIQUE		64
	#define FLM_NON_UNIQUE_ELM_KEY_REF		65
	#define FLM_OLD_VIEW							66
	#define FLM_COULD_NOT_SYNC_BLK			67
	#define FLM_IX_REF_REC_NOT_FOUND			68
	#define FLM_IX_KEY_NOT_FOUND_IN_REC		69
	#define FLM_DRN_NOT_IN_KEY_REFSET		70
	#define FLM_BAD_BLK_CHECKSUM				71
	#define FLM_BAD_LAST_DRN					72
	#define FLM_BAD_FILE_SIZE					73
	#define FLM_BAD_AVAIL_BLOCK_COUNT		74
	#define FLM_BAD_DATE_FIELD					75
	#define FLM_BAD_TIME_FIELD					76
	#define FLM_BAD_TMSTAMP_FIELD				77
	#define FLM_BAD_DATE_KEY					78
	#define FLM_BAD_TIME_KEY					79
	#define FLM_BAD_TMSTAMP_KEY				80
	#define FLM_BAD_BLOB_FIELD					81
	#define FLM_BAD_PCODE_IXD_TBL				82
	#define FLM_DICT_REC_ADD_ERR				83
	#define FLM_NUM_CORRUPT_ERRORS			84

	typedef struct
	{
		FLMUINT64	ui64BytesUsed;
		FLMUINT64	ui64ElementCount;
		FLMUINT64 	ui64ContElementCount;
		FLMUINT64 	ui64ContElmBytes;
		FLMUINT		uiBlockCount;
		FLMINT		iErrCode;
		FLMUINT		uiNumErrors;
	} BLOCK_INFO;

	typedef struct
	{
		FLMINT		iErrCode;
		FLMUINT		uiErrLocale;
	#define				LOCALE_NONE			0
	#define				LOCALE_LFH_LIST	1
	#define				LOCALE_PCODE_LIST	2
	#define				LOCALE_AVAIL_LIST	3
	#define				LOCALE_B_TREE		4
	#define				LOCALE_PCODE_TBLS	5
	#define				LOCALE_INDEX		6
		FLMUINT		uiErrLfNumber;
		FLMUINT		uiErrLfType;
		FLMUINT		uiErrBTreeLevel;
		FLMUINT		uiErrBlkAddress;
		FLMUINT		uiErrParentBlkAddress;
		FLMUINT		uiErrElmOffset;
		FLMUINT		uiErrDrn;
		FLMUINT		uiErrElmRecOffset;
		FLMUINT		uiErrFieldNum;
		FLMBYTE *	pBlk;

		// Index corruption information

		FlmRecord *	pErrIxKey;
		FlmRecord *	pErrRecord;
		REC_KEY *	pErrRecordKeyList;

	} CORRUPT_INFO;

	typedef struct
	{
		void *			AppArg;
		FLMINT			iCheckPhase;
	#define					CHECK_GET_DICT_INFO	1
	#define					CHECK_B_TREE			2
	#define					CHECK_AVAIL_BLOCKS	3
	#define					CHECK_DICT_PCODE		4
	#define					CHECK_RS_SORT			5
		FLMBOOL			bStartFlag;
		FLMUINT			uiNumLFs;
		FLMUINT			uiCurrLF;
		FLMUINT			uiLfNumber;
		FLMUINT			uiLfType;
		FLMUINT64		ui64BytesExamined;
		FLMUINT			uiNumProblemsFixed;
		FLMBOOL			bStructCorrupt;
		FLMBOOL			bLogicCorrupt;
		FLMUINT			uiLogicalCorruptions;
		FLMUINT			uiLogicalRepairs;
		FLMUINT64		ui64FileSize;
		FLMUINT			uiNumFields;
		FLMUINT			uiNumIndexes;
		FLMUINT			uiNumContainers;
		FLMUINT			uiNumLogicalFiles;
		BLOCK_INFO		AvailBlocks;
		BLOCK_INFO		LFHBlocks;
		BLOCK_INFO		PcodeBlocks;

		// Index check progress

		FLMBOOL			bUniqueIndex;				// Is this a unique index?
		FLMUINT64		ui64NumKeys;				// Number of keys in the result set
		FLMUINT64		ui64NumDuplicateKeys;	// Number of duplicate keys generated
		FLMUINT64		ui64NumKeysExamined;		// Number of keys checked
		FLMUINT64		ui64NumKeysNotFound;		// Extra keys found in indexes
		FLMUINT64		ui64NumRecKeysNotFound;	// Keys missing from indexes
		FLMUINT64		ui64NumNonUniqueKeys;	// Non-unique keys in indexes
		FLMUINT64		ui64NumConflicts;			// # of non-corruption conflicts
		FLMUINT64		ui64NumRSUnits;			// Number of rset sort items
		FLMUINT64		ui64NumRSUnitsDone;		// Number of rset items sorted

		// Internal use only

		void *			pvDbInfo;

	} DB_CHECK_PROGRESS;

	typedef struct
	{
		FLMINT			iDoingFlag;
	#define					REBUILD_GET_BLK_SIZ		1
	#define					REBUILD_RECOVER_DICT		2
	#define					REBUILD_RECOVER_DATA		3
		FLMBOOL			bStartFlag;
		FLMUINT64		ui64FileSize;
		FLMUINT64		ui64BytesExamined;
		FLMUINT			uiTotRecs;
		FLMUINT			uiRecsRecov;
	} REBUILD_INFO;

	FLMBYTE * FlmVerifyErrToStr(
		FLMINT			iErrCode);

	RCODE FlmDbCheck(
		HFDB						hDb,
		FLMBYTE *				pszDbFileName,
		FLMBYTE *				pszDataDir,
		FLMBYTE *				pszRflDir,
		FLMUINT					uiFlags,
		PMM *						pmm,
		DB_CHECK_PROGRESS *	pDbStatsRV,
		STATUS_HOOK				fnStatusHook,
		void *					AppArg);

	RCODE FlmDbRebuild(
		FLMBYTE *		pszSourceDbPath,
		FLMBYTE *		pszSourceDataDir,
		FLMBYTE *		pszDestDbPath,
		FLMBYTE *		pszDestDataDir,
		FLMBYTE *		pszDestRflDir,
		FLMBYTE *		pDictPath,
		CREATE_OPTS *	pCreateOpts,
		FLMUINT *		puiTotRecsRV,
		FLMUINT *		puiRecsRecovRV,
		STATUS_HOOK		fnStatusHook,
		void *			StatusData);

	#define F_MAXIMUM_FILE_SIZE	0xFFFC0000
	#define F_FILENAME_SIZE			256
	#define F_PATH_MAX_SIZE			256

	void f_pathParse(
		FLMBYTE *		pszPath,
		FLMBYTE *		pszServer,
		FLMBYTE *		pszVolume,
		FLMBYTE *		pszDirPath,
		FLMBYTE *		pszFileName);

	RCODE f_pathReduce(
		FLMBYTE *		pszSourcePath,
		FLMBYTE *		pszDestPath,
		FLMBYTE *		pszString);

	RCODE f_pathAppend(
		FLMBYTE *		pszPath,
		FLMBYTE *		pszPathComponent);

	RCODE f_pathToStorageString(
		FLMBYTE *		pPath,
		FLMBYTE *		pszString);

	void f_pathCreateUniqueName(
		FLMUINT *		puiTime,
		FLMBYTE *		pFileName,
		FLMBYTE *		pFileExt,
		FLMBYTE *		pHighChars,
		FLMBOOL			bModext);

	FLMBOOL f_doesFileMatch(
		FLMBYTE *		pszFileName,
		FLMBYTE *		pszTemplate);

	/*
	*** Directories
	*/

	class F_DirHdl : public F_Base
	{
	public:

		virtual ~F_DirHdl()
		{
		}

		virtual RCODE OpenDir(
			FLMBYTE *	pDirPath,
			char *		pszPattern) = 0;

		virtual RCODE Next( void) = 0;
													
		virtual FLMBYTE * CurrentItemName( void) = 0;

		virtual FLMUINT CurrentItemSize( void) = 0;

		virtual FLMBOOL CurrentItemIsDir( void) = 0;

		virtual void CurrentItemPath(
			FLMBYTE  *	pPath) = 0;
	};

	RCODE FlmAllocDirHdl(
		F_DirHdl **		ppDirHdl);

	/*
	*** List item
	*/

	typedef struct
	{
		F_ListItem *		pPrevItem;			// Prev ListItem
		F_ListItem *		pNextItem;			// Next ListItem
		FLMUINT				uiListCount;		// Number of items within a list. This 
														// element is not used when found within
														// a ListItem (only used in ListMgr)
	} F_ListNode;

	class F_ListItem : public F_Base
	{
	protected:

		F_ListMgr *		m_pListMgr;				// List that this item is linked into.
		FLMUINT			m_uiLNodeCnt;			// Number of LNODEs
		F_ListNode *	m_pLNodes;				// List of LNODES that this item is apart of.
														// Call F_List::GetListCount to determine how 
														// many LNODEs this item has.
		FLMBOOL			m_bInList;

		F_ListItem()
		{
			m_pListMgr = NULL;
			m_pLNodes = NULL;
			m_uiLNodeCnt = 0;
			m_bInList = FALSE;
		}

		virtual ~F_ListItem();

		RCODE Setup(								// Finish setup operation on this ListItem
			F_ListMgr *		pList,				// List manager to use
			F_ListNode *	pLNodes,				// Array of LNODEs to be used
			FLMUINT			uiLNodeCnt);		// Number of F_ListNodes supplied.

		RCODE RemoveFromList(					// Remove this list item from all lists.
			FLMUINT		uiList = 0);			// Which list to remove item from
														// To remove item from all lists pass in
														// FLM_ALL_LISTS define.

		// List Traversal Methods

		inline F_ListItem * GetNextListItem(
			FLMUINT		uiList = 0)
		{
			return( m_pLNodes[ uiList].pNextItem);
		}

		inline F_ListItem * GetPrevListItem(
			FLMUINT		uiList = 0)
		{
			return( m_pLNodes[ uiList].pPrevItem);
		}

		// List Modification Methods

		inline F_ListItem * SetNextListItem(
			FLMUINT				uiList,	
			F_ListItem *		pNewNext)
		{
			F_ListNode *	pLNode;

			pLNode = &m_pLNodes[ uiList];
			pLNode->pNextItem = pNewNext;

			return pNewNext;
		}

		inline F_ListItem * SetPrevListItem(
			FLMUINT				uiList,	
			F_ListItem *		pNewPrev)
		{
			F_ListNode *	pLNode;

			pLNode = &m_pLNodes[ uiList];
			pLNode->pPrevItem = pNewPrev;

			return pNewPrev;
		}

		friend class F_ListMgr;
		friend class F_FileHdlPage;
		friend class F_FileHdlMgr;
		friend class F_ObjRefTracker;
	};

	/*
	*** File handle
	*/

	class F_FileHdl : public F_ListItem
	{
	public:

		virtual ~F_FileHdl()
		{
		}

		virtual RCODE Close( void) = 0;				// Close a file - The destructor will call this
																// This is used to obtain an error code.
													
		virtual RCODE Create(							// Create a new file.
			FLMBYTE *	pIoPath,							// File to be created
			FLMUINT		uiIoFlags) = 0;				// Access and Mode Flags

		virtual RCODE CreateUnique(					// Create a new file (with a unique file name).
			FLMBYTE *	pIoPath,							// Directory where the file is to be created
			FLMBYTE *	pszFileExtension,				// Extension to be used on the new file.
			FLMUINT		uiIoFlags) = 0;				// Access and Mode Flags

		virtual RCODE Open(								// Initiates access to an existing file.
			FLMBYTE *	pIoPath,							// File to be opened
			FLMUINT		uiIoFlags) = 0;				// Access and Mode Flags

		virtual RCODE Flush( void) = 0;				// Flushes a file's buffers to disk

		virtual RCODE Read(								// Reads a buffer of data from a file
			FLMUINT		uiOffset,						// Offset to being reading at.
			FLMUINT		uiLength,						// Number of bytes to read
			void *		pvBuffer,						// Buffer to place read bytes into
			FLMUINT *	puiBytesRead) = 0;			// [out] number of bytes read

		virtual RCODE Seek(								// Moves the current position in the file
			FLMUINT		uiOffset,						// Offset to seek to
			FLMINT		iWhence,							// Location to apply sdwOffset to.
			FLMUINT *	puiNewOffset) = 0;			// [out] new file offset

		virtual RCODE Size(								// Returns to size of the open file.
			FLMUINT *	puiSize) = 0;					// [out] size of the file

		virtual RCODE Tell(								// Returns to current position of the file
																// pointer in the open file.
			FLMUINT *	puiOffset) = 0;				// [out] current file position

		virtual RCODE Truncate(							// Decreases the size of a file.
			FLMUINT		uiSize) = 0;					// Size to truncate the file to.

		virtual RCODE Write(								// Writes a buffer of data to a file.
			FLMUINT		uiOffset,						// Offset to seek to.
			FLMUINT		uiLength,						// Number of bytes to write.
			void *		pvBuffer,						// Buffer that contains bytes to be written
			FLMUINT *	puiBytesWritten) = 0;		// Number of bytes written.
	};

	RCODE FlmAllocFileHandle(
		F_FileHdl **		ppFileHandle);

	/*
	*** File flags
	*/

	#define F_IO_CURRENT_POS	0xFFFFFFFF

	#define F_IO_RDONLY				0x0001
	#define F_IO_RDWR					0x0002
	#define F_IO_TRUNC				0x0004
	#define F_IO_EXCL					0x0008
	#define F_IO_CREATE_DIR			0x0010
	#define F_IO_SH_DENYRW			0x0020
	#define F_IO_SH_DENYWR			0x0040
	#define F_IO_SH_DENYNONE		0x0080
	#define F_IO_DIRECT				0x0100
	#define F_IO_DELETE_ON_CLOSE	0x0200

	// File Positioning Definitions

	#define F_IO_SEEK_SET		0	// Beginning of File
	#define F_IO_SEEK_CUR		1	// Current File Pointer Position
	#define F_IO_SEEK_END		2	// End of File

	/*
	*** File system
	*/

	class F_FileSystem : public F_Base
	{
	public:

		virtual ~F_FileSystem()
		{
		}

		virtual RCODE Open(
			FLMBYTE *		pFilePath,					// Name of file to be opened.
			FLMUINT			uiIoFlags,					// Access and Mode flags.
			F_FileHdl **	ppFileHdl) = 0;			// Returns open file handle object.

		virtual RCODE Create(							// Create a new file handle
			FLMBYTE *		pFilePath,					// Name of file to be created
			FLMUINT			uiIoFlags,					// Access amd Mode flags
			F_FileHdl **	ppFileHdl) = 0;			// Returns open file handle object.

		virtual RCODE OpenDir(							// Open a directory
			FLMBYTE *		pDirPath,					// Directory to be opened.
			char *			pszPattern,					// File name pattern.
			F_DirHdl **		ppDirHdl) = 0;				// Returns open directory handle
																// object.

		virtual RCODE CreateDir(						// Create a directory
			FLMBYTE *		pDirPath) = 0;				// Directory to be created.

		virtual RCODE RemoveDir(						// Remove a directory
			FLMBYTE *		pDirPath,					// Directory to be removed.
			FLMBOOL			bClear = FALSE) = 0;		// OK to delete files if dir is not empty?

		virtual RCODE Exists(							// See if a file or directory exists.
			FLMBYTE *		pPath) = 0;					// Name of file or directory to check.

		virtual FLMBOOL IsDir(							// See if a path is a directory.
			FLMBYTE *		pPath) = 0;					// Name of path to check.

		virtual RCODE GetTimeStamp(					// Get the date/time when the file
																// was last updated.
			FLMBYTE *		pPath,						// Path to file
			FLMUINT *		puiTimeStamp) = 0;		// Buffer in which time stamp is 
																// returned.

		virtual RCODE Delete(							// Delete a file or directory
			FLMBYTE *		pPath) = 0;					// Name of file or directory to delete.

		virtual RCODE Rename(							// Rename a file.
			FLMBYTE *		pFilePath,					// File to be renamed
			FLMBYTE *		pNewFilePath) = 0;		// New file name

		virtual RCODE Copy(								// Copy a file.
			FLMBYTE *		pSrcFilePath,				// Name of source file to be copied.
			FLMBYTE *		pDestFilePath,				// Name of destination file.
			FLMBOOL			bOverwrite,					// Overwrite destination file?
			FLMUINT *		puiBytesCopied) = 0;		// Number of bytes copied.
	};

	RCODE FlmAllocFileSystem(
		F_FileSystem **		ppFileSystem);

	FLMINT f_sprintf(
		char *			pszDestStr,
		const char *	pszFormat,
		...);

	#define f_min(a, b) \
		((a) < (b) ? (a) : (b))

	#define f_max(a, b) \
		((a) < (b) ? (b) : (a))

	#define f_swap( a, b, tmp) \
		((tmp) = (a), (a) = (b), (b) = (tmp))

	FLMBYTE * f_wtoa(
		FLMINT16			i16Value,
		FLMBYTE *		ptr);

	FLMBYTE * f_dtoa(
		FLMINT			iValue,
		FLMBYTE *		ptr);

	FLMBYTE * f_uwtoa(
		FLMUINT16		ui16Value,
		FLMBYTE *		ptr);

	FLMBYTE * f_udtoa(
		FLMUINT			uiValue,
		FLMBYTE *		ptr);

	FLMINT f_atoi(
		FLMBYTE *		ptr);

	FLMINT f_atol(
		FLMBYTE *		ptr);

	FLMINT f_atod(
		FLMBYTE *		ptr);

	FLMUINT f_atoud(
		FLMBYTE *		ptr);

	FLMINT f_unicmp(
		FLMUNICODE *	puzStr1,
		FLMUNICODE *	puzStr2);

	FLMUINT f_unilen(
		FLMUNICODE *	puzStr);

	FLMINT f_unincmp(
		FLMUNICODE *	puzStr1,
		FLMUNICODE *	puzStr2,
		FLMUINT			uiLen);

	FLMINT f_uninativecmp(
		FLMUNICODE *	puzStr1,
		char *			pszStr2);

	FLMINT f_uninativencmp(
		FLMUNICODE *	puzStr1,
		char *			pszStr2,
		FLMUINT			uiCount);

	FLMUNICODE * f_unicpy(
		FLMUNICODE *	puzDestStr,
		FLMUNICODE *	puzSrcStr);

	void f_nativetounistrcpy(
		FLMUNICODE *	puzDestBuf,
		FLMBYTE *		pszSrcBuf);

	FLMBOOL tokenIsNum(
		FLMBYTE *	pszToken,
		FLMUINT *	puiNum);

	inline void f_align32(
		FLMBYTE *		pucStart,
		FLMBYTE **		pucCur)
	{
		FLMBYTE *	pucTmp = *pucCur;
		FLMUINT		uiSize;

		uiSize = sizeof( FLMUINT32) - (pucTmp - pucStart) % sizeof( FLMUINT32);

		if( uiSize != sizeof( FLMUINT32))
		{
			*pucCur = pucTmp + uiSize;
		}
	}

	#if !defined( FLM_UNIX) && !defined( FLM_WIN64)
		#pragma pack(pop)
	#endif

#endif
