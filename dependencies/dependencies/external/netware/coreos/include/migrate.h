#ifndef __MIGRATE_H__
#define __MIGRATE_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   MIGRATE.H  $
 *  $Modtime:   Aug 29 1998 10:04:20  $
 *  $Revision$
 *  
 ****************************************************************************/

/*********************************************************************************
 * Program Name:  NetWare 386
 *
 * Filename:	  migrate.h
 *
 * Date Created:  November 11, 1991
 *
 * Version:		  1.0
 *
 * Programmers:	  Jim A. Nicolet
 *
 * Files used:
 *
 * Date Modified: 
 *
 * Modifications: 
 *
 * Comments: 
 *				 
 *
 ****************************************************************************/

#define	ERR_INVALID_SM_OPTION						239
#define	ERR_INVALID_SUPPORT_MODULE_ID				240
#define	ERR_SUPPORT_MODULE_ALREADY_REGISTERED	241
#define	ERR_SUPPORT_MODULE_CREATE_FAILED			242
#define	ERR_SUPPORT_MODULE_CLOSE_FAILED			243
#define	ERR_SM_WRITE_NO_SPACE						244
#define	ERR_SM_WRITE_IO_ERROR						245
#define	ERR_SM_READ_IO_ERROR							246
#define	ERR_SUPPORT_MODULE_OPEN_FAILED			247
#define	ERR_SUPPORT_MODULE_DELETE_FAILED			248

#define	NO_OPERATION_IN_PROGRESS	0
#define	MIGRATE_OPERATION		1
#define	DEMIGRATE_OPERATION	2
#define	PEEK_OPERATION			3

#define	SalvageMode				1
#define	NormalMode				0

#define	RootMode					0x55
#define	MigrationRootSignature		0xafde5714

#define	DefaultRdWrSupportModuleID	0x64646d6d

/* ioFlags definitions for when a SM registers */
#define	AllowSaveKeyBit					0x00000001
#define	AllowRWDefaultModuleBit			0x00000002
#define	AllowModuleToBeBoundBit			0x00000004
#define	NotifyIfSaveKeyBitOnRenameMove	0x00001000	/* SPD# 81202 */

#define	GetFullPathBit					0x40000000	/* used in the rtdm */
#define	SMLoadedSuccessfullyFlag		0x80000000

/* Flags definition for the INWput routine */
#define	SetSaveKeyBit						0x00000001
#define	MigrateDataNoDeleteBit			0x00008000

/* Minimim Version of the rtdm I allow to work */
#define	RTDMMajorVersionNumber	0x1
#define	RTDMMinorVersionNumber	0x6

typedef	struct	{
	LONG	sFlag;
	LONG	sLookUpNumber;
	LONG	sAttributes;
	LONG	sModifyDateAndTime;
	LONG	sArchiveDateAndTime;
	LONG	sCreateDateAndTime;
	LONG	sLastUpdatedInRelativeSeconds;
	LONG	sReferenceCount;
} SaveKeyStruct;

typedef	struct	{
	LONG	mFlink;
	LONG	mBlink;
	LONG	mOp;	/* 1 - migrate, 2 - de-migrate, 3 - peek */
	LONG	mSemaphore;
	LONG	mVol;
	LONG	mBase;
	LONG	mActive;
} mH;

typedef	struct	{
	LONG	VFlag;
	LONG	DataStreamNumber;
	LONG	DataStreamSize;
	LONG	FileHandle;
	LONG	CompressStreamSize; /* Set to DataStreamSize if not compressed */
}DMHANDLE;

typedef	struct	{
	LONG	MyStation;
	DMHANDLE	dmh[MaximumNumberOfDataStreams];
} DMHS;

typedef	struct	{
	LONG FileSystemTypeID;
	LONG Volume;
	LONG DOSDirEntry;
	LONG OwnerDirEntry;
	LONG OwnerNameSpace;
	BYTE OwnerFileName[256];	/* 255 + 1 len byte */
} DMFSINFO;

typedef	struct	{
	struct ExtendedDirectoryStructure EDS;
	LONG	SMID;
	BYTE	keyData[108];
} DMKEY;

typedef	struct	{
	LONG	OldParent;
	LONG	OldDirNumber;
	LONG	OldNameSpace;
	LONG	OldAttributes;
	LONG	OldLookUpEntryNumber;
	DMKEY	OldKey;
	BYTE 	OldFileName[256];	/* 255 + 1 len byte */
} OLDDMINFO;

typedef	struct	{
	struct ExtendedDirectoryStructure EDS;
	LONG	SMID;
	LONG	RootDirBase;
	LONG	RootDateAndTime;
	BYTE	RootNameLength;
	BYTE	RootName[12];
	BYTE	keyData[87];
} ROOTKEY;

typedef	struct	{
	void	*spLink;
	LONG	amountMigrated;
	LONG	amountDeMigrated;
	LONG	SupportModuleID;
	LONG	slotNumber;
	LONG	filesMigrated;
	LONG	filesLimbo;	/* demigrated - but still have key */
} SMSN;

/* NCP 90 128 */
typedef struct
{
	LONG Vol;
	LONG DirBase;
	LONG NameSpace;
	LONG SMID;
	LONG Flags;
} MoveDataReq;

typedef	struct
{
	BYTE SubFunctionCode;
	MoveDataReq mdr;
} ncp90_128;

/* NCP 90 129 */
typedef struct
{
	LONG Vol;
	LONG DirBase;
	LONG NameSpace;
} FileInfoReq;

typedef	struct
{
	BYTE SubFunctionCode;
	FileInfoReq fir;
} ncp90_129;

typedef struct
{
	LONG rSMID;
	LONG rRestoreTime;
	LONG rDataStreams;	/* stream sizes follow */
} FileInfoRep;

/* NCP 90 130 */
typedef struct
{
	LONG Vol;
	LONG SupModID;
} VolInfoReq;

typedef	struct
{
	BYTE SubFunctionCode;
	VolInfoReq vir;
} ncp90_130;

typedef struct
{
	LONG rFilesMigrated;
	LONG rTtlDataSizeMigrated;
	LONG rSpaceUsed;
	LONG rLimboUsed;
	LONG rSpaceMigrated;
	LONG rFilesLimbo;
} VolInfoRep;

/* NCP 90 131 */
typedef struct
{
	LONG rStatus;
	LONG rMajorVersion;
	LONG rMinorVersion;
	LONG rSMRegs;
} DMStatusRep;

/* NCP 90 132 */
typedef struct
{
	LONG InfoLevel;
	LONG SMID;
} SMInfoReq;

typedef	struct
{
	BYTE SubFunctionCode;
	SMInfoReq smir;
} ncp90_132;

typedef struct
{
	LONG rIOStatus;
	LONG rInfoBlockSize;
	LONG rAvailSpace;
	LONG rUsedSpace;
	BYTE rSMString;	/* 128 length limit, Info block follows string */
} Info0Rep;

typedef struct
{
	LONG rSMRegs; /* id of sm's follow */
} Info1Rep;

typedef struct
{
	BYTE NameLength; /* sm name*/
} Info2Rep;

/* NCP 90 133 */
typedef struct
{
	LONG Vol;
	LONG DirBase;
	LONG NameSpace;
} GetDataReq;

typedef	struct
{
	BYTE SubFunctionCode;
	GetDataReq gdr;
} ncp90_133;

/* NCP 90 134 */
typedef struct
{
	LONG Action;
	LONG SMID;
} GetSetIDReq;

typedef	struct
{
	BYTE SubFunctionCode;
	GetSetIDReq gsidr;
} ncp90_134;

typedef struct
{
	LONG rSMID;
} GetSetIDRep;


/* NCP 90 135 */
typedef struct
{
	LONG SupModID;
	LONG Vol;
	LONG TargetDirectoryBase;
} SMCapacityReq;

typedef struct
{
	LONG SMBlockSizeinSectors;
	LONG SMTotalBlocks;	/* -1 means unlimited */
	LONG SMUsedBlocks;
} SMCapacityRep;

typedef	struct
{
	BYTE SubFunctionCode;
	SMCapacityReq capreq;
} ncp90_135;

typedef	struct
{
	BYTE SubFunctionCode;
	LONG verb;
	BYTE verbData;
} ncp90_136;


extern	LONG	DMpresent;

extern	LONG	ChangeDMLookUpKeyToSalvage(
				LONG Volume,
				LONG edsNumber,
				LONG oldDirectoryEntryNumber,
				LONG newSalvageEntryNumber);

extern	LONG	ChangeDMLookUpKeyToNormal(
				LONG Volume,
				LONG edsNumber,
				LONG oldSalvageEntryNumber,
				LONG newDirectoryEntryNumber);

extern LONG	RegisterRTDataMigrationNLM(
		LONG	Station,
		LONG	(*addr[])(),
		BYTE	*ID,
		LONG	majorVersion,
		LONG	minorVersion);

extern LONG DeRegisterRTDataMigrationNLM(
		LONG Station,
		BYTE *DMTAG,
		LONG ForceFlag);

extern LONG MoveFileDataToDM(
		LONG Station,
		LONG vol,
		LONG DOSDirBase,
		LONG SupportModuleID,
		LONG flags);

extern LONG MoveFileDataFromDM(
		LONG Station,
		LONG vol,
		LONG DOSDirBase);

extern LONG DMFileInformation(
		LONG Station,
		LONG vol,
		LONG DOSDirBase,
		LONG *validDataStreams,
		LONG *supportModuleID,
		LONG *estRetrievalTime,
		LONG *info);

extern LONG VolumeDMInformation(
		LONG Station,
		LONG vol,
		LONG SMID,
		LONG *numOfFiles,
		LONG *ttlMigratedDataSize,
		LONG *spaceUsed,
		LONG *limboUsed,
		LONG *migratedFiles,
		LONG *limboFiles);

LONG	RealTimeDataMigratorInfo(
		LONG	Station,
		LONG	*MigratorPresentFlag,
		LONG	*majorVersion,
		LONG	*minorVersion,
		LONG	*NumberOfLoadedSupportModules);

extern LONG PeekDMFileData(
		LONG Station,
		LONG vol,
		LONG DOSDirBase,
		LONG DataStream,
		LONG NoWaitFlag,
		LONG startingSector,
		LONG SectorsToRead,
		BYTE *buf,
		LONG *sectorsRead,
		LONG *bytesRead,
		LONG	*NoWaitReason);

extern	LONG	RegisterDMSupportModule(
		LONG	ioFlag,
		LONG	(*addr[])(),
		BYTE	*SupportModuleName,
		LONG	SupportModuleID,
		LONG	MaxSectorsXF,
		LONG	*SlotNumber);

extern	LONG	DeRegisterDMSupportModule(
		LONG	SupportModuleID,
		BYTE	*SupportModuleName,
		LONG	SlotNumber);

extern	LONG	DMSupportModuleInformation(
		LONG	InformationLevel,
		LONG	SupportModuleID,
		void	*returnInfo,
		LONG	*rtnInfoLen);

extern	LONG	IsDataMigrationAllowed( LONG Volume);

/* ************************************************************************ */
/* ************************************************************************ */
/* ************************************************************************ */
extern	LONG	CheckForActiveMigrationHandle( LONG Vol, LONG Base, LONG *operation);
extern	LONG	RemovemH( LONG Vol, mH *rmop);
extern	LONG	AddmH( LONG Vol, LONG Base, LONG Operation, LONG *newMop);

extern	LONG	GetMigratedData(
				LONG Station,
				mH	*mop,
				LONG edsNumber,
				BYTE *DOSFileName);

extern	LONG	DeleteMigratedFile(
					LONG Volume,
					LONG DirectoryEntryNumber,
					LONG edsNumber,
					LONG sectors,
					LONG attributes,
					BYTE *DOSFileName);

extern	LONG	VerifyMigratedFileForLimbo(
					LONG	vol,
					LONG	DOSEntryNumber,
					LONG	kNumber);

extern	LONG	GetSupportModuleID(
				LONG vol,
				LONG DOSEntryNumber,
				LONG eaNumber,
				LONG *SMID);

extern	LONG	GetSetDefaultSupportModule(
					LONG Station,
					LONG action,
					LONG SMID,
					LONG *rtnSMID);

extern	LONG	DMVerifyRenameOrMove(
				LONG Volume,
				LONG NameSpace,
				LONG TargetSubDirectoryEntry,
				struct DirectoryStructure *Dir[],
				LONG *OldEntryNumber[],
				BYTE *NewName,
				OLDDMINFO *old);

extern	void	DMRenameOrMove(
				LONG Volume,
				LONG NameSpace,
				LONG TargetSubDirectoryEntry,
				OLDDMINFO *old,
				BYTE *NewName,
				LONG NewEntryNumber);

extern	LONG	RTDMRequest(
				LONG Station,
				LONG requestLength,
				LONG requestVerb,
				void *requestData,
				LONG *answerLength,
				void *answer);

/****************************************************************************/
/****************************************************************************/


#endif /* __MIGRATE_H__ */
