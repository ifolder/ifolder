#include <funpointer.h>
#include <tsajob.h>

FS_API_TABLE NSSAPITable;
FS_API_TABLE CFSAPITable;
FS_API_TABLE VFSAPITable;
void PopulateAPITable()
{
	//NSS
	NSSAPITable.filesysType						= 0x00000003;
	NSSAPITable.FS_SetFileDirSearchAttributes	= FS_SetFileDirSearchAttributes;
	NSSAPITable.FS_InitScan						= FS_InitScan;	
	NSSAPITable.FS_EndScan						= FS_EndScan;
	NSSAPITable.FS_ReInitScan					= FS_ReInitScan;
	NSSAPITable.FS_Close						= FS_Close;
	NSSAPITable.FS_SetSparseStatus				= FS_SetSparseStatus;
	NSSAPITable.FS_ConvertOpenMode				= FS_ConvertOpenMode;
	NSSAPITable.FS_Open							= FS_Open;
	NSSAPITable.FS_CheckCOWOnName 				= FS_CheckCOWOnName;
	NSSAPITable.JOB_GetInfoAboutFirstDataSet	= JOB_GetInfoAboutFirstDataSet;
	NSSAPITable.FS_ScanNextFileEntry 			= FS_ScanNextFileEntry;
	NSSAPITable.FS_ReInitScan 					= FS_ReInitScan;
	NSSAPITable.FS_ScanNextDirectoryEntry 		= FS_ScanNextDirectoryEntry;
	NSSAPITable.FS_GetFileNameSpaceUNIName 		= FS_GetFileNameSpaceUNIName;
	NSSAPITable.FS_SetFileEntryInfo				= FS_SetFileEntryInfo;
	NSSAPITable.FS_GetDirSpaceRestriction		= FS_GetDirSpaceRestriction;
	NSSAPITable.FS_ScanTrustees					= FS_ScanTrustees;
	NSSAPITable.FS_GetFilePrimaryUNIName		= FS_GetFilePrimaryUNIName;
	NSSAPITable.FS_GetNameSpaceEntryName       = FS_GetNameSpaceEntryName;
	NSSAPITable.FS_Create						= FS_Create; 
	NSSAPITable.FS_Write						= FS_Write;  
	NSSAPITable.FS_SetFileSize					= FS_SetFileSize;  
	NSSAPITable.FS_ModifyInfo					= FS_ModifyInfo;  
	NSSAPITable.FS_FileExcludedByScanControl	= FS_FileExcludedByScanControl;
	NSSAPITable.FS_GetCompressedFileSize		= FS_GetCompressedFileSize;
	NSSAPITable.FS_AllocateModifyInfo			= FS_AllocateModifyInfo;
	NSSAPITable.FS_SetCompressedFileSize		=FS_SetCompressedFileSize;
	NSSAPITable.FS_AddTrustee					=FS_AddTrustee;
	NSSAPITable.FS_DeleteTrustees				=FS_DeleteTrustees;
	NSSAPITable.FS_Read							=FS_Read;
	NSSAPITable.FS_GetInfo						=FS_GetInfo;
	NSSAPITable.FS_RestoreInitScan				=FS_RestoreInitScan;
	NSSAPITable.FS_SetReadyFileDirForRestore	=FS_SetReadyFileDirForRestore;	
#ifdef N_PLAT_NLM
	NSSAPITable.FS_CreateMigratedDirectory		=FS_CreateMigratedDirectory;
#endif
	NSSAPITable.FS_OpenAndRename				= FS_OpenAndRename;
	NSSAPITable.FS_SetPrimaryNameSpace		= FS_SetPrimaryNameSpace;
	NSSAPITable.FS_SetNameSpaceName				= FS_SetNameSpaceName; 
	NSSAPITable.FS_GenericGetFileNameSpaceName	= FS_GetFileNameSpaceUNIName;	
	NSSAPITable.FS_DeleteSetPurge				= FS_DeleteSetPurge;
	NSSAPITable.FS_DeleteFile					= FS_DeleteFile;

	//NSSAPITable.FS_CreateMigratedFile			=FS_CreateMigratedFile;
	//NSSAPITable.FS_VerifyMigrationKey			=FS_VerifyMigrationKey;
#ifdef N_PLAT_NLM
	//CFS
	CFSAPITable.filesysType						= 0x00000001;
	CFSAPITable.FS_SetFileDirSearchAttributes	= FS_CFS_SetFileDirSearchAttributes;
	CFSAPITable.FS_InitScan						= FS_CFS_InitScan;	
	CFSAPITable.FS_EndScan						= FS_CFS_EndScan;
	CFSAPITable.FS_ReInitScan					= FS_CFS_ReInitScan;
	CFSAPITable.FS_Close						= FS_CFS_Close;
	CFSAPITable.FS_SetSparseStatus				= FS_CFS_SetSparseStatus;
	CFSAPITable.FS_ConvertOpenMode				= FS_CFS_ConvertOpenMode;
	CFSAPITable.FS_Open							= FS_CFS_Open;
	CFSAPITable.FS_CheckCOWOnName 				= FS_CFS_CheckCOWOnName;
	CFSAPITable.JOB_GetInfoAboutFirstDataSet 	= CFS_JOB_GetInfoAboutFirstDataSet;
	CFSAPITable.FS_ScanNextFileEntry 			= FS_CFS_ScanNextFileEntry;
	CFSAPITable.FS_ReInitScan 					= FS_CFS_ReInitScan;
	CFSAPITable.FS_ScanNextDirectoryEntry 		= FS_CFS_ScanNextDirectoryEntry;
	CFSAPITable.FS_GetFileNameSpaceUNIName 		= FS_CFS_GetFileNameSpaceUNIName;
	CFSAPITable.FS_SetFileEntryInfo				= FS_CFS_SetFileEntryInfo;
	CFSAPITable.FS_GetDirSpaceRestriction		= FS_CFS_GetDirSpaceRestriction;
	CFSAPITable.FS_ScanTrustees					= FS_CFS_ScanTrustees;
	CFSAPITable.FS_GetFilePrimaryUNIName		= FS_CFS_GetFilePrimaryUNIName;
	CFSAPITable.FS_GetNameSpaceEntryName       = FS_CFS_GetNameSpaceEntryName;
	CFSAPITable.FS_Create						= FS_CFS_CreateAndOpen; 
	CFSAPITable.FS_Write						= FS_CFS_Write; 
	CFSAPITable.FS_SetFileSize					= FS_CFS_SetFileSize;
	CFSAPITable.FS_ModifyInfo					= FS_CFS_ModifyInfo; 
	CFSAPITable.FS_FileExcludedByScanControl 	= FS_CFS_FileExcludedByScanControl;
	CFSAPITable.FS_GetCompressedFileSize		= FS_CFS_GetCompressedFileSize;
	CFSAPITable.FS_AllocateModifyInfo			= FS_CFS_AllocateModifyInfo;
	CFSAPITable.FS_DeleteTrustees				=FS_CFS_DeleteTrustees;
	CFSAPITable.FS_AddTrustee					=FS_CFS_AddTrustee;
	CFSAPITable.FS_SetCompressedFileSize		=FS_CFS_SetCompressedFileSize;
	CFSAPITable.FS_CreateMigratedDirectory		=FS_CFS_CreateMigratedDirectory;
	CFSAPITable.FS_OpenAndRename				= FS_CFS_OpenAndRename;
	CFSAPITable.FS_SetPrimaryNameSpace		= FS_CFS_SetPrimaryNameSpace;
	CFSAPITable.FS_SetNameSpaceName				= FS_CFS_SetNameSpaceName;
	CFSAPITable.FS_GenericGetFileNameSpaceName	= FS_CFS_GenericGetFileNameSpaceName;
	//CFSAPITable.FS_CreateMigratedFile			=FS_CFS_CreateMigratedFile;
	//CFSAPITable.FS_VerifyMigrationKey			= FS_CFS_VerifyMigrationKey;
#elif N_PLAT_GNU	
	VFSAPITable.filesysType						= 0x00000003;
	VFSAPITable.FS_SetFileDirSearchAttributes	= FS_VFS_SetFileDirSearchAttributes;
	VFSAPITable.FS_InitScan						= FS_VFS_InitScan;	
	VFSAPITable.FS_EndScan						= FS_VFS_EndScan;
	VFSAPITable.FS_ReInitScan					= FS_VFS_ReInitScan;
	VFSAPITable.FS_Close						= FS_VFS_Close;
	VFSAPITable.FS_SetSparseStatus				= FS_VFS_SetSparseStatus;
	VFSAPITable.FS_ConvertOpenMode				= FS_VFS_ConvertOpenMode;
	VFSAPITable.FS_Open							= FS_VFS_Open;
	VFSAPITable.FS_CheckCOWOnName 				= FS_VFS_CheckCOWOnName;
	VFSAPITable.JOB_GetInfoAboutFirstDataSet	= JOB_GetInfoAboutFirstDataSet;
	VFSAPITable.FS_ScanNextFileEntry 			= FS_VFS_ScanNextFileEntry;
	VFSAPITable.FS_ReInitScan 					= FS_VFS_ReInitScan;
	VFSAPITable.FS_ScanNextDirectoryEntry 		= FS_VFS_ScanNextDirectoryEntry;
	VFSAPITable.FS_GetFileNameSpaceUNIName 		= FS_VFS_GetFileNameSpaceUNIName;
	VFSAPITable.FS_SetFileEntryInfo				= FS_SetFileEntryInfo;
	VFSAPITable.FS_GetDirSpaceRestriction		= FS_VFS_GetDirSpaceRestriction;
	VFSAPITable.FS_ScanTrustees					= FS_VFS_ScanTrustees;
	VFSAPITable.FS_GetFilePrimaryUNIName		= FS_VFS_GetFilePrimaryUNIName;
	VFSAPITable.FS_GetNameSpaceEntryName 		= FS_VFS_GetNameSpaceEntryName;
	VFSAPITable.FS_Create						= FS_VFS_Create; 
	VFSAPITable.FS_Write						= FS_VFS_Write;  
	VFSAPITable.FS_SetFileSize					= FS_VFS_SetFileSize;  
	VFSAPITable.FS_ModifyInfo					= FS_VFS_ModifyInfo;  
	VFSAPITable.FS_FileExcludedByScanControl	= FS_FileExcludedByScanControl;
	VFSAPITable.FS_GetCompressedFileSize		= FS_VFS_GetCompressedFileSize;
	VFSAPITable.FS_AllocateModifyInfo			= FS_AllocateModifyInfo;
	VFSAPITable.FS_SetCompressedFileSize		=FS_SetCompressedFileSize;
	VFSAPITable.FS_AddTrustee					=FS_VFS_AddTrustee;
	VFSAPITable.FS_DeleteTrustees				=FS_VFS_DeleteTrustees;
	VFSAPITable.FS_OpenAndRename				= FS_VFS_OpenAndRename;
	VFSAPITable.FS_SetPrimaryNameSpace			= FS_SetPrimaryNameSpace;
	VFSAPITable.FS_SetNameSpaceName				= FS_SetNameSpaceName;
	VFSAPITable.FS_GenericGetFileNameSpaceName	= FS_VFS_GetFilePrimaryUNIName;
	VFSAPITable.FS_Read							=FS_VFS_Read;
	VFSAPITable.FS_GetInfo						=FS_VFS_GetInfo;
	VFSAPITable.FS_RestoreInitScan				=FS_VFS_RestoreInitScan;
	VFSAPITable.FS_SetReadyFileDirForRestore	=FS_VFS_SetReadyFileDirForRestore;	
	VFSAPITable.FS_DeleteSetPurge				= FS_VFS_DeleteSetPurge;
	VFSAPITable.FS_DeleteFile					= FS_VFS_DeleteFile;
#endif
}

