#ifndef __ENCP_H__
#define __ENCP_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1996 Novell, Inc.
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
 *  $Workfile:   encp.h  $
 *  $Modtime:   13 Feb 2002 15:16:50  $
 *  $Revision$
 *  
 ****************************************************************************/

/*	


FILE ENCP.H
	Extended 3.1 NCPs for generic clients

	written by:	Brady Anderson
					March 15, 1990
*/


#define NTCLIENTECO 1
#define CROSSNAMEFIX 1   /* defect 100253657 - RDoxey */

/****************************************************************************/

/* Definitions used for OpenCreate Mode */
#define	OPEN_FILE		0x01
#define	TRUNCATE_FILE	0x02
#define	CREATE_FILE		0x08
#define OPEN_CALLBACK	0x80
#define RO_ACCESS_OK	0x40

#define	OpenCreateFlagsMask	(TRUNCATE_FILE | OPEN_FILE | CREATE_FILE)

/* Definitions used for OpenCreate Action */
#define	FILE_EXISTED	0x01
#define	FILE_CREATED	0x02
#define	FILE_TRUNCATED	0x04
#define	FILE_COMPRESSED	0x08
#define FILE_RO_ACCESS	0x80


/****************************************************************************/
/* Defines used for Trustee scans, adds, and deletes */
#define 	MAX_TRUSTEES	20

/****************************************************************************/
/* Define for search attributes to include all files and subdirectories */
#define 	SEARCH_ATTR_INCLUDE_ALL		0x8000


////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////  DFS Search Defines //////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////
#define	SEARCH_LINK_AWARE					0x20
#define	SEARCH_OPERATE_ON_LINK			0x40
#define	SEARCH_INFO_ON_FIRST_LINK		0x80
#define	SEARCH_LINK_BITS					0xe0

/****************************************************************************/
/* These defines go with the ReturnInfoMask field to determine what will
   be returned in the Netware Information structure */
#define	RFileName				0x00000001
#define	RSize						0x00000002
#define	RAttributes				0x00000004
#define	RDataStreamInfo		0x00000008
#define	RTotalDataStreamSize	0x00000010
#define	REAInfo					0x00000020
#define	RArchiveInfo			0x00000040
#define	RModifyInfo				0x00000080
#define	RCreationInfo			0x00000100
#define	RNameSpaceInfo			0x00000200
#define	RDirectoryInfo			0x00000400
#define	RRights					0x00000800
/* valid only if RNewStyle is set */
#define	RReferenceID			0x00001000
#define	RNSAttribute			0x00002000
#define	RDataStreamActual		0x00004000	/* actual size in sectors */
#define	RDataStreamLogical		0x00008000	/* size from the dir entry */
#define	RLastUpdatedInSeconds	0x00010000	/* seconds relative to yr 2000 */
#define	RDOSName				0x00020000	/* 8.3 DOS Name */
#define	RFlushTime				0x00040000	/* For Drew - flush time of a file */

#define	RParentBaseID			0x00080000	/* req.d by MAC NCP client */
#define	RMacFinderInfo			0x00100000	/* req.d by MAC NCP client */
#define	RSiblingCount			0x00200000	/* req.d by MAC NCP client */
#define	REffectiveRights		0x00400000	/* req.d by MAC NCP client */
#define	RMACDateTimes			0x00800000	/* req.d by MAC NCP client */

#define	RLastAccessedTime		0x01000000	/* req.d by Client 32 -- valid only on file SPD # - Jim A. Nicolet - 6-25-98*/

#define	RDFSLinkInfo			0x02000000	// NSS&DFS only
#define	R64BitFileSize			0x04000000	/* 64 bit file sizes, upper 32 bits masked in TFS*/
#define	RNSSInfoOnSnapshot	0x20000000	// NSS&DFS only
#define	RNSSLargeSizes			0x40000000	// NSS&DFS only

#define	RNewStyle				0x80000000	

/* This is the Netware Information structure which is used to return
   specific information about the entry, if RNewStyle is not set*/
typedef	struct
{
	/* Returned if RSize bit is set */
	LONG	DiskSpaceAllocated;

	/* Returned if RAttribute bit is set */
	LONG	Attributes;
	WORD	Flags;

	/* Returned if RDataStreamInfo bit is set */
	LONG	DataStreamSize;

	/* Returned if RTotalDataStreamSize bit is set */
	LONG	TotalDataStreamsSize;
	WORD	NumberOfDataStreams;

	/* Returned if RCreationInfo bit is set */
	WORD	CreationTime;
	WORD	CreationDate;
	LONG	CreatorsID;

	/* Returned if RModifyInfo bit is set */
	WORD	ModifiedTime;
	WORD	ModifiedDate;
	LONG 	ModifiersID;
	WORD	LastAccessedDate;

	/* Returned if RArchiveInfo bit is set */
	WORD	ArchivedTime;
	WORD	ArchivedDate;
	LONG	ArchiversID;

	/* Returned if RRights bit is set */
	WORD	MaximumRightsMask;

	/* Returned if RDirectoryInfo bit is set */
	LONG	DirectoryEntryNumber;
	LONG	DOSDirectoryEntryNumber;
	LONG	VolumeNumber;

	/* Returned if REAInfo bit is set */
	LONG	ExtendedAttributesValueSize;
	LONG	ExtendedAttributesCount;
	LONG	ExtendedAttributesKeySize;

	/* Returned if RNameSpaceInfo bit is set */
	LONG	CreatorNameSpaceNumber;

}	NetwareInfo;


typedef	struct
{
	/* Returned if RFileName bit is set */
	BYTE	FileNameLength;
	BYTE	FileName[256];		/*max. size is 255 character file name */

} NetwareFileName;



/****************************************************************************/
/* These defines go with the ModifySetMask field to determine which fields will be modified */

#define	MAttributes			0x00000002
#define	MCreationDate		0x00000004
#define	MCreationTime		0x00000008
#define	MCreatorID			0x00000010
#define	MArchiveDate		0x00000020
#define	MArchiveTime		0x00000040
#define	MArchiveID			0x00000080
#define	MModifyDate			0x00000100
#define	MModifyTime			0x00000200
#define	MModifyID			0x00000400
#define	MLastAccess			0x00000800
#define	MRestrict			0x00001000
#define	MMaxSpace			0x00002000


/* This bit is only valid with the 
	F3ModifyNSSpecificInfo api
	NCP 87 19
*/
#define	MName				0x00000001


/* **************************************
#define	MModifyInSeconds	0x00004000
***************************************** */

/* Structure used for Modify File Information Call */
typedef	struct
{
	/* Set if the MAttributes bit is set in the ModifyMask field */
	WORD	FileAttributes;
	BYTE	FileMode;
	BYTE	FileXAttributes;

	/* Set if the MCreationDate bit is set */
	WORD	CreationDate;

	/* Set if the MCreationTime bit is set */
	WORD	CreationTime;

	/* Set if the MCreatorID bit is set */
	LONG	CreatorsID;

	/* Set if the MModifyDate bit is set */
	WORD	ModifiedDate;

	/* Set if the MModifyTime bit is set */
	WORD	ModifiedTime;

	/* Set if the MModifyID bit is set */
	LONG 	ModifiersID;

	/* Set if the MArchiveDate bit is set */
	WORD	ArchivedDate;

	/* Set if the MArchiveTime bit is set */
	WORD	ArchivedTime;

	/* Set if the MArchiveID bit is set */
	LONG	ArchiversID;

	/* Set if the MLastAccess bit is set */
	WORD	LastAccessedDate;

	/* Set if the MRestrict bit is set */
	WORD	RestrictionGrantMask;
	WORD	RestrictionRevokeMask;

	/* Set if the MMaxSpace bit is set */
	int	MaximumSpace;

/********************************************************************************
	LONG	ModifyInRelativeSeconds;
******************************************************************************* */

} ModifyInfo;

/****************************************************************************/
typedef	struct
{
	BYTE	Volume;
	LONG	DirectoryBaseOrHandle;		  /* Directory Base | Short Directory Handle*/
	BYTE	HandleFlag;
	BYTE	PathComponentCount;
	BYTE	PathString[300];
} NWHandlePath;

/* Handle Flags Definition */
#define	HFShortDirectoryHandle						0x00
#define	HFDirectoryBase								0x01
#define	HFNoHandlePresent	 						0xFF

typedef	struct
{
	BYTE	Volume;
	LONG	DirectoryBaseOrHandle;		  /* Directory Base | Short Directory Handle*/
	BYTE	HandleFlag;
	BYTE	PathComponentCount;
} NWRenMoveHandlePath;

/****************************************************************************/
typedef	struct
{
	WORD	HugeOffset;
	WORD	HugeLength;
} HugeInfoStruct;

/****************************************************************************/
/* File Handle to Directory Base NCP structure */

/* 
	HandleInfoLevel
		0 - Handle Info (Vol,DirBase,Stream#)
		1 - Handle Info with Name Space (Vol,DirBase,Stream#)
		2 - File info from handle
		3 - Dir info from 1 byte handle
*/
typedef struct
{
	BYTE	NWFileHandle[6];
	BYTE	HandleInfoLevel;
	BYTE	NameSpace;	/* only used when HandleInfoLevel == 1 */
}HandleToDirBaseReq;

typedef struct
{
	BYTE DirectoryHandle;
	BYTE Pad[5];
	BYTE	HandleInfoLevel;
	BYTE Pad1;
} DirHandleInfoReq;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DataStream;
}HandleToDirBaseRep;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
	LONG	DataStream;
}FileHandleInfoRep;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
}DirHandleInfoRep;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
	LONG	DataStream;
	LONG	ParentDirectoryNumber;
	LONG	ParentDOSDirectoryNumber;
}FileHandleInfoParentRep;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
	LONG	ParentDirectoryNumber;
	LONG	ParentDOSDirectoryNumber;
}DirHandleInfoParentRep;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
	LONG	ParentDirectoryNumber;
	LONG	ParentDOSDirectoryNumber;
} DIRINFOANDPARENT;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
	LONG	DataStream;
	LONG	ParentDirectoryNumber;
	LONG	ParentDOSDirectoryNumber;
} FILEINFOANDPARENT;

/****************************************************************************/
/* CreateOpen Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				OpenCreateFlags;
	WORD				SearchAttributes;
	LONG				ReturnInfoMask;  
	LONG				CreateAttributes;
	WORD				DesiredAccessRights;
	NWHandlePath	NPathInfo;
}CreateOpenRequest;

/* CreateOpen2 Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DataStream;
	BYTE				OpenCreateFlags;
	BYTE				Xunused;
	WORD				SearchAttributes;
	WORD				XXunused;
	LONG				ReturnInfoMask;  
	LONG				CreateAttributes;
	WORD				DesiredAccessRights;
	NWHandlePath	NPathInfo;
}CreateOpenRequest2;

/* CreateOpen Reply NCP structure */
typedef struct
{
	LONG					FileHandle;
	BYTE					OpenCreateAction;
	BYTE					OCRetFlags;
	NetwareInfo			NInfo;
	NetwareFileName	FInfo;
}CreateOpenReply;
/****************************************************************************/
typedef	struct
{
	LONG	CCFileHandle;
	BYTE	CCFunction;
}CallbackControlRequest;


/****************************************************************************/
typedef	struct
{
	BYTE			 NameSpace;
	BYTE			 Reserved;
	NWHandlePath NPathInfo;
}SearchInitRequest;

typedef struct
{
	BYTE 			VolumeNumber;	 	/* The following three fields make up the	*/
	LONG			DirectoryNumber;	/* nine byte search sequence */
	LONG			EntryNumber;
}SearchInitReply;


/****************************************************************************/
typedef	struct
{
	BYTE			NameSpace;
	BYTE			DataStream;
	WORD			SearchAttributes;
	LONG			ReturnInfoMask;
	BYTE			SVolume;
	LONG			SDirectoryNumber;
	LONG			SEntryNumber;
	BYTE			SearchPatternLength;
 	BYTE			SearchPattern[255];
}SearchContRequest;


typedef struct
{
	BYTE					SVolume;
	LONG					SDirectoryNumber;
	LONG					SEntryNumber;
	BYTE					Reserved;
	NetwareInfo			NInfo;
	NetwareFileName	FInfo;
}SearchContReply;


/****************************************************************************/
/* Rename File or Directory Request NCP structure */

/* Bits used in the RenameFlag to turn ON these options */
#define	RRenameToMyself		0x01
#define	RCompatabilityFlag	0x02
#define	RNameOnlyFlag			0x04

typedef	struct
{
	BYTE						NameSpace;
	BYTE						RenameFlag;
	WORD						SearchAttributes;
	NWRenMoveHandlePath	SrcPathInfo;
	NWRenMoveHandlePath	DstPathInfo;
	BYTE						PInfo;
}RenameRequest;

/* Only a completion code will be returned on reply */



/****************************************************************************/
/* Trustees Structure */
typedef	struct
{
	LONG	ObjectID;
	WORD	TrusteeRights;
}Trustees;


/****************************************************************************/
/* Scan File or Directory for Trustees Request NCP structure */
typedef	struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	WORD				SearchAttribute;
	LONG				ScanSequence;
	NWHandlePath 	NPathInfo;
}ScanTrusteesRequest;


/* Scan File or Directory for Trustees Reply NCP structure */
typedef	struct
{
	LONG				NextScanSequence;
	WORD				ObjectIDCount;
	Trustees			STrustees[MAX_TRUSTEES];
}ScanTrusteesReply;



/****************************************************************************/
/* Add Trustee or Trustees to File or Directory Request NCP structure */
typedef	struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	WORD				SearchAttributes;
	WORD				TrusteeRightsMask;
	WORD				ObjectIDCount;
	NWHandlePath 	NPathInfo;
	Trustees			ATrustees[MAX_TRUSTEES];
}AddTrusteesRequest;


/****************************************************************************/
/* Delete Trustee or Trustees from a File or Directory Request NCP structure */
typedef	struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	WORD				ObjectIDCount;
	NWHandlePath 	NPathInfo;
	Trustees			DTrustees[MAX_TRUSTEES];
}DeleteTrusteesRequest;


/****************************************************************************/
/* Obtain File/SubDirectory Info Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DstNameSpace;
	WORD				SearchAttributes;
	LONG				ReturnInfoMask;
	NWHandlePath	NPathInfo;
}ObtainInfoRequest;

/* Obtain File/SubDirectory Info Reply NCP structure */
typedef	struct
{
	NetwareInfo			NInfo;
	NetwareFileName	FInfo;
}ObtainInfoReply;


/****************************************************************************/
/* Modify File/SubDirectory Information Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	WORD				SearchAttributes;
	LONG				ModifyMask;
	ModifyInfo		MInfo;
	NWHandlePath	NPathInfo;
}ModifyInfoRequest;


/****************************************************************************/
/* Delete File Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	WORD				SearchAttributes;
	NWHandlePath	NPathInfo;
}DeleteFileRequest;
												

/****************************************************************************/
/* Allocate Directory Handle Request NCP Structure */
typedef	struct
{
	BYTE				NameSpace;
	BYTE				DstNameSpace;	/* valid only if bit 15 of alloc mode is set */
	WORD				AllocateMode;
	NWHandlePath	NPathInfo;
} AllocateDirRequest;


/* Allocate Directory Handle Reply NCP Structure */
typedef	struct
{
	BYTE				NewDirectoryHandle;
	BYTE				VolumeNumber;
	BYTE				Reserved[4];
} AllocateDirReply;

typedef	struct
{
	LONG		Volume;
	LONG		DirectoryBase;
	LONG		DOSDirectoryBase;
	LONG		NameSpace;
	BYTE		NewDirectoryHandle;
} AllocateDirReply2;


/****************************************************************************/
/* Set Directory Handle Request NCP Structure */
typedef	struct
{
	BYTE				NameSpace;
	BYTE				DataStream;
	BYTE				DstDirHandle;
	BYTE				Flags;
	NWHandlePath	NPathInfo;
} SetDirRequest;


/* Set Directory Handle only returns a ccode for the reply */

typedef	struct
{
	LONG		Volume;
	LONG		DirectoryBase;
	LONG		DOSDirectoryBase;
	LONG		NameSpace;
} SetDirReply2;



/****************************************************************************/
/* Scan Salvagable Files Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DataStream;
	LONG				ReturnInfoMask;
	LONG				ScanSequence;
	NWHandlePath	NPathInfo;
}ScanSalvageRequest;


/* Scan Salvagable Files Reply NCP structure */
typedef struct
{
	LONG					NextScanSequence;
	LONG					DeletedDateAndTime;
	LONG					DeletorID;
	LONG					DeletorVolume;
	LONG					DeletorDirectoryBase;
	NetwareInfo			NInfo;
	NetwareFileName	FInfo;
}ScanSalvageReply;

/****************************************************************************/
/* Recover Salvagable File Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	LONG				Sequence;
	LONG				Volume;
	LONG				DirectoryBase;
	BYTE				NewFileNameLength;
	BYTE				NewFileName[255];
}RecoverSalvageRequest;



/****************************************************************************/
	/* Purge Salvagable File Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				Reserved;
	LONG				Sequence;
	LONG				Volume;
	LONG				DirectoryBase;
}PurgeSalvageRequest;


/****************************************************************************/
/* Get Name Space Specific File/SubDirectory Information Request NCP structure */
typedef struct
{
	BYTE				SrcNameSpace;
	BYTE				DstNameSpace;
	BYTE				Reserved;
	BYTE				VolumeNumber;
	LONG				DirectoryBase;
	LONG				NSInfoBitMask;
}GetNSInfoRequest;

/* Get Name Space Specific File/SubDirectory Info Reply NCP structure */
typedef	struct
{
	BYTE				NSData[512];
}GetNSInfoReply;


/****************************************************************************/
/* Modify Name Space Specific Information */
typedef struct
{
	BYTE				SrcNameSpace;
	BYTE				DstNameSpace;
	BYTE				VolumeNumber;
	LONG				DirectoryBase;
	LONG				ModifyMask;
	BYTE				ModifyData[512];
}ModifyNSInfoRequest;

/****************************************************************************/
/* Search File/SubDirectory Set Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DataStream;
	WORD				SearchAttributes;
	LONG				ReturnInfoMask;
	WORD				ReturnInfoCount;
	BYTE				SVolume;
	LONG				SDirectoryNumber;
	LONG				SEntryNumber;
	BYTE				SearchPatternLength;
	BYTE				SearchPattern[255];
}SearchSetRequest;

/* Search File/SubDirectory Set Reply NCP structure */
typedef	struct
{
	BYTE					SVolume;
	LONG					SDirectoryNumber;
	LONG					SEntryNumber;
	BYTE					MoreEntries;
	WORD					InfoCount;
	NetwareInfo			NInfo;
}SearchSetReply;



/****************************************************************************/
/* Get Path from Handle Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DirectoryHandle;
}GetPathRequest;


/****************************************************************************/
/* Get Path from Handle Reply NCP structure */
typedef struct
{
	NetwareFileName	FInfo;
}GetPathReply;


/****************************************************************************/
/* Get Directory Base and Volume # Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DstNameSpace;
	WORD				DstNameSpaceID;	/*ID = 'Jn', then DstNameSpace is valid*/
	NWHandlePath	NPathInfo;
}GetDirBaseRequest;

/****************************************************************************/
/* Get Directory Base and Volume # NCP structure */
typedef struct
{
	LONG				NSDirectoryBase;
	LONG				DOSDirectoryBase;
	BYTE				VolumeNumber;
}GetDirBaseReply;


/****************************************************************************/
/* Query Name Space Info */
typedef struct
{
	BYTE				NameSpace;
	BYTE				VolumeNumber;
}QueryNSInfoRequest;

typedef struct
{
	LONG				FixedFieldsMask;
	LONG				VariableFieldsMask;
	LONG				HugeFieldsMask;
	WORD				FixedBitsDefined;
	WORD				VariableBitsDefined;
	WORD				HugeBitsDefined;
	LONG				FieldsLengthTable[32];
}QueryNSInfoReply;


/****************************************************************************/
/* Get Name Spaces Loaded List From Volume Number NCP Structure */
typedef struct
{
	WORD				Reserved;
	BYTE				VolumeNumber;
}GetNSLLRequest;

typedef struct
{
	WORD				NumberOfNameSpacesLoaded;
	BYTE				NameSpaceLoadList[MaximumNumberOfNameSpaces];
}GetNSLLReply;


/****************************************************************************/
/* Get Huge NS Information */
typedef struct
{
	BYTE				NameSpace;
	BYTE				VolumeNumber;
	LONG				DirectoryBase;
	LONG				HugeMask;
	BYTE				HugeStateInfo[16];
} GetHugeNSRequest;

typedef struct
{
	BYTE				NextHugeStateInfo[16];
	LONG				HugeDataLength;
	BYTE				HugeData[512];
} GetHugeNSReply;


/****************************************************************************/
/* Set Huge NS Information */
typedef struct
{
	BYTE				NameSpace;
	BYTE				VolumeNumber;
	LONG				DirectoryBase;
	LONG				HugeMask;
	BYTE				HugeStateInfo[16];
	LONG				HugeDataLength;
	BYTE				HugeData[480];
} SetHugeNSRequest;

typedef struct
{
	BYTE				NextHugeStateInfo[16];
	LONG				HugeDataUsed;
} SetHugeNSReply;

/****************************************************************************/

#define	LastComponentIsAFile		1

#define	cookieValidBits		LastComponentIsAFile

typedef	struct
{
	WORD	cookieFlags;
	LONG	Vol;
	LONG	Base;
}PathCookie;

/* Get Full Path String Request NCP structure */
typedef struct
{
	BYTE				SrcNS;
	BYTE				DstNS;
	PathCookie		pCookie;
	NWHandlePath	NPathInfo;
}GetFullPathStringReq;

/****************************************************************************/
/* Get Full Path String Reply NCP structure */
typedef struct
{
	PathCookie		repCookie;
	WORD				repComponentSize;
	WORD				repComponentCount;
	BYTE				ComponentBuffer;
}GetFullPathStringRep;
/****************************************************************************/
/* Get Effective Directory Rights Request NCP structure */
typedef struct
{
	BYTE				NameSpace;
	BYTE				DstNameSpace;
	WORD				SearchAttributes;
	LONG				ReturnInfoMask;
	NWHandlePath	NPathInfo;
}GetEffectiveRightsRequest;

/* Get Effective Directory Rights Reply NCP structure */
typedef	struct
{
	WORD					MyEffectiveRights;
	NetwareInfo			NInfo;
	NetwareFileName	FInfo;
}GetEffectiveRightsReply;

typedef struct
{
	LONG	Volume;
	LONG	DirectoryBase;
	LONG	DOSDirectoryBase;
	LONG	NameSpace;
} F3ALLOCDIRHANDLEINFO;

/****************************************************************************/
/* Modify DOS Attributes on a File or SubDirectory */
typedef struct
{
	BYTE	NameSpace;
	BYTE	Flags;
	WORD	SearchAttributes;
	LONG	AttributesMask;
	LONG	Attributes;		
	NWHandlePath	NPathInfo;
}ModifyDOSAttributesRequest;

typedef struct
{
	LONG foundItems;
	LONG changedItems;
	LONG ValidFlag; /* indicates whether the newAttributes field is valid */
	LONG NewAttributes; /* when Allow Wild Cards is set, only valid for first match */
}ModifyDOSAttributesReply;

/****************************************************************************/
/* Log File */
typedef struct
{
	BYTE	NameSpace;
	BYTE	reserved;
	WORD	reserved2;
	LONG	LogFileFlags;
	LONG	WaitTime;
	NWHandlePath	NPathInfo;
}LogFileRequest;

#define	LockFileImmediatelyBit		0x00000001	/* LOCKBIT in locks.h */
#define CallBackRequestedBit		0x00008000

typedef struct
{
	BYTE	NameSpace;
	BYTE	reserved;
	WORD	reserved2;
	NWHandlePath	NPathInfo;
}ClearOrReleaseFileRequest;
/****************************************************************************/
/* Get Directory Space Restriction */

typedef struct 
{
	BYTE 			NameSpace;
	BYTE 			pad[2];
	NWHandlePath	NPathInfo;
} GetDirSpaceRestrictionReq_t ;
/****************************************************************************/
/* FUNCTION PROTOTYPES FOR GENPRIM.386 */

extern LONG	GenericGetVolumeNumber(
					BYTE *string,
					LONG NameSpace,
					BYTE **path,
					BYTE *volume);

/****************************************************************************/
/* FUNCTION PROTOTYPES FOR FILER3.C */

extern	LONG F3OpenCreate(
				LONG	station,
				LONG	task,
				LONG	Volume,
				LONG	PathCount,
				LONG	PathBase,
				BYTE	*PathString,
				LONG	NameSpace,
				LONG	DataStream,
				LONG	OpenCreateFlags,
				LONG	SearchAttributes,
				LONG	CreateAttributes,
				LONG	DesiredAccessRights,
				LONG	ReturnInfoMask,
				LONG	*FileHandle,
				BYTE	*OpenCreateAction,
				NetwareInfo			*NInfo,
				NetwareFileName	*FInfo,
				LONG	*rLen);

#if CROSSNAMEFIX
extern	LONG F3OpenCreateEx(
				LONG	station,
				LONG	task,
				LONG	Volume,
				LONG	PathCount,
				LONG	PathBase,
				BYTE	*PathString,
				LONG	NameSpace,
				LONG	DataStream,
				LONG	OpenCreateFlags,
				LONG	SearchAttributes,
				LONG	CreateAttributes,
				LONG	DesiredAccessRights,
				LONG	ReturnInfoMask,
				LONG	*FileHandle,
				BYTE	*OpenCreateAction,
				NetwareInfo			*NInfo,
				NetwareFileName	*FInfo,
				LONG	*rLen,
				LONG	MixedModeFlag);
#endif

extern	LONG F3InitFileSearch(
		LONG	station,
		LONG	NameSpace,
		LONG Volume,
		LONG PathCount,
		LONG PathBase,
		BYTE *PathString,
		LONG	*VolumeNumber,
		LONG	*DirectoryNumber,
		LONG	*EntryNumber);

extern	LONG F3ContinueFileSearch(
		LONG	station,
		LONG	NameSpace,
		LONG	SearchAttributes,
		LONG	Volume,
		LONG	DirectoryNumber,
		LONG	EntryNumber,
		BYTE	*SearchPattern,	/* length preceded string */
		LONG	ReturnInfoMask,
		LONG	*NextEntryNumber,
		NetwareInfo	*NInfo,
		NetwareFileName *FInfo,
		LONG	*rLen);


extern	LONG F3RenameFile(
		LONG	station,
		LONG	task,
		LONG	NameSpace,
		LONG	RenameFlag,
		LONG	SearchAttributes,
		LONG SrcVolume,
		LONG SrcPathCount,
		LONG SrcPathBase,
		BYTE *SrcPathString,
		LONG DstVolume,
		LONG DstPathCount,
		LONG DstPathBase,
		BYTE *DstPathString);

extern	LONG F3ScanForTrustees(
		LONG	station,
		LONG	NameSpace,
		LONG Volume,
		LONG PathCount,
		LONG PathBase,
		BYTE *PathString,
		LONG	ScanSequence,
		LONG	MaxTrusteeCount,	/* Maximum Number of Trustees to return */
		LONG	*NextScanSequence,
		LONG	*ObjectIDCount,
		Trustees	*TPtr);

extern	LONG F3ObtainFileInfo(
		LONG	station,
		LONG	NameSpace,
		LONG Volume,
		LONG PathCount,
		LONG PathBase,
		BYTE *PathString,
		LONG	DstNameSpace,
		LONG	ReturnInfoMask,
		NetwareInfo	 *NInfo,
		NetwareFileName	*FInfo,
		LONG *rLen);

extern	LONG F3ModifyInfo(
		LONG	station,
		LONG	task,
		LONG Volume,
		LONG PathCount,
		LONG PathBase,
		BYTE *PathString,
		LONG	NameSpace,
		LONG	SearchAttributes,
		LONG	ModifyMask,
		ModifyInfo	*MInfo);

extern LONG F3ModifyDOSAttributes(
		LONG Station,
		LONG Task,
		LONG Volume,
		LONG PathBase,
		BYTE *PathString,
		LONG PathCount,
		LONG NameSpace,
		LONG SearchAttributes,
		LONG Flags,
		LONG Mask,
		LONG newAttributes,
		LONG *validFlag,
		LONG *resultentAttributes,
		LONG *foundItems,
		LONG *changedItems);


extern	LONG	F3EraseFile(
		LONG	station,
		LONG	task,
		LONG Volume,
		LONG PathCount,
		LONG PathBase,
		BYTE *PathString,
		LONG	NameSpace,
		LONG	SearchAttributes);

extern	LONG	F3SetDirHandle(
		LONG	station,
		LONG	Volume,
		LONG	PathCount,
		LONG	PathBase,
		BYTE	*PathString,
		LONG	NameSpace,
		LONG	DstDirHandle,
		F3ALLOCDIRHANDLEINFO *info);

extern	LONG	F3AddTrustees(
		LONG	station,
		LONG	NameSpace,
		LONG	Volume,
		LONG	PathCount,
		LONG	PathBase,
		BYTE	*PathString,
		LONG	TrusteeRightsMask,
		LONG	ObjectIDCount,
		Trustees	*TPtr);

extern	LONG	F3DeleteTrustees(
		LONG	station,
		LONG	NameSpace,
		LONG	Volume,
		LONG	PathCount,
		LONG	PathBase,
		BYTE	*PathString,
		LONG	ObjectIDCount,
		Trustees	*TPtr);

extern	LONG	F3AllocDirHandle(
		LONG	station,
		LONG	task,
		LONG	Volume,
		LONG	PathCount,
		LONG	PathBase,
		BYTE	*PathString,
		LONG	NameSpace,
		LONG	AllocateMode, 
		LONG	DstNameSpace,
		LONG	*DirectoryHandle,
		F3ALLOCDIRHANDLEINFO *info);

extern	LONG	F3ScanSalvagedFiles(
		LONG	station,
		LONG	NameSpace,
		LONG	Volume,
		LONG	PathCount,
		LONG	PathBase,
		BYTE	*PathString,
		LONG	ScanSequence,
		LONG	ReturnInfoMask,
		LONG	*NextScanSequence,
		LONG	*DeletedDateAndTime,
		LONG	*DeletorID,
		LONG	*DeletedDirectoryBase,
		NetwareInfo	*NInfo,
		NetwareFileName	*FInfo,
		LONG	*rLen);

extern	LONG	F3RecoverSalvagedFiles(
		LONG	station,
		LONG	NameSpace,
		LONG	Sequence,
		LONG	Volume,
		LONG	DirectoryBase,
		BYTE	*FileName);		/* length preceded */

extern	LONG	F3PurgeSalvageableFile(
		LONG	station,
		LONG	NameSpace,
		LONG	Sequence,
		LONG	Volume,
		LONG	DirectoryBase);

extern	LONG	F3GetNSSpecificInfo(
	LONG	station,
	LONG	SrcNameSpace,
	LONG	DstNameSpace,
	LONG	VolumeNumber,
	LONG	DirectoryBase,
	LONG	NSInfoBitMask,
	LONG	*NSInfoLength,
	BYTE	*NSInfo);

extern	LONG	F3ModifyNSSpecificInfo(
	LONG	station,
	LONG	task,
	LONG	DataPacketLength,		/* Size of data packet minus IPX header and fixed length data*/
	LONG	SrcNameSpace,
	LONG	DstNameSpace,
	LONG	VolumeNumber,
	LONG	DirectoryBase,
	LONG	ModifyMask,
	BYTE	*MInfo);

extern	LONG	F3SearchSet(
	LONG	station,
	LONG	NameSpace,
	LONG	SearchAttributes,
	int	NumberOfEntries,		/* return info count -- maximum # of entries to return */
	LONG	Volume,
	LONG	DirectoryNumber,
	LONG	EntryNumber,
	BYTE	*SearchPattern,		/* length preceded */
	LONG	ReturnInfoMask,
	LONG	*NewEntryNumber,
	LONG	*MoreEntriesFlag,
	LONG	*EntriesCount,
	LONG	*InfoLength,	/* preset to contain buffer length -- rtn actual length */
	BYTE	*Info);

extern	LONG	F3GetPathString(
	LONG	Station,
	LONG	NameSpace,
	LONG	DirectoryHandle,
	NetwareFileName	*FInfo);

extern	LONG	F3GetDirBase(
	LONG	station,
	LONG	SrcNameSpace,
	LONG	Volume,
	LONG	PathCount,
	LONG	PathBase,
	BYTE	*PathString,
	LONG	DstNameSpace,
	LONG	*DirectoryBase,
	LONG	*DOSDirectoryBase);

extern	LONG	F3QueryNameSpaceInfo(
	LONG	NameSpace,
	LONG	VolumeNumber,
	LONG	*FixedFieldsMask,
	LONG	*VariableFieldsMask,
	LONG	*HugeFieldsMask,
	LONG	*FixedBitsDefined,
	LONG	*VariableBitsDefined,
	LONG	*HugeBitsDefined,
	LONG	*FLTable);	/* must contain room for 32 longs */

extern	LONG	F3GetNameSpaceList(
	LONG	VolumeNumber,
	LONG	*NumOfLoadedNameSpaces,
	BYTE	*NSInfo);

extern	LONG	F3GetHugeInfo(
	LONG	Station,
	LONG	Task,
	LONG	NameSpace,
	LONG	VolumeNumber,
	LONG	DirectoryBase,
	LONG	HugeMask,
	BYTE	*HSInfo,		/* huge state info */
	LONG	*HugeDataLength,
	BYTE	*NHSInfo,		/* next huge state info */
	BYTE	*HInfo);

extern	LONG	F3SetHugeInfo(
	LONG	Station,
	LONG	Task,
	LONG	NameSpace,
	LONG	VolumeNumber,
	LONG	DirectoryBase,
	LONG	HugeMask,
	BYTE	*HSInfo,
	LONG	HugeDataLength,
	BYTE	*HInfo,
	BYTE	*NHSInfo,
	LONG	*HugeDataUsed);

extern	LONG	F3GetFullPathString(
	LONG	Station,
	LONG	SrcNameSpace,
	LONG	Volume,
	LONG	PathBase,
	LONG	DstNameSpace,
	PathCookie	*pCookie,
	LONG	ComponentBufferSize,
	LONG	*ComponentCount,
	LONG	*ComponentSize,
	BYTE	*ComponentBuffer);

extern	LONG	F3GetEffectiveDirectoryRights(
	LONG	station,
	LONG	NameSpace,
	LONG	Volume,
	LONG	PathCount,
	LONG	PathBase,
	BYTE	*PathString,
	LONG	DstNameSpace,
	LONG	ReturnInfoMask,
	WORD	*MyEffectiveRights,
	NetwareInfo	*NInfo,
	NetwareFileName	*FInfo,
	LONG	*rLen);

extern	LONG F3GetFileHandleInfoAndParent(LONG, LONG, void *);
extern	LONG F3GetDirHandleInfoAndParent(LONG, LONG, void *);

extern	LONG GetNetWareInfo(
				LONG Volume,
				LONG Station,
				LONG DirEntry,
				LONG DOSDirEntry,
				LONG NameSpace,
				LONG ReturnInfoMask,
				struct DirectoryStructure *OtherDir,
				struct DirectoryStructure *DOSDir,
				void *info,
				LONG *rLen,
//;;BEGIN SPD 138431 - OPNSIZFX.NLM Change 33 of 34 - TDR 5/23/97
//				LONG infoLen);
				LONG infoLen,
				LONG *FileHandle);
//;;END SPD 138431 - OPNSIZFX.NLM Change 33 of 34 - TDR 5/23/97

extern LONG	GenericFillReplyBuffer(
				LONG	Volume,
				LONG	Station,
				LONG	DirEntry,
				LONG	DOSDirEntry,
				LONG	NameSpace,
				LONG	ReturnInfoMask,
				struct DirectoryStructure	*OtherDir,
				struct DirectoryStructure	*DOSDir,
				NetwareInfo	*rep,
				NetwareFileName	*frep,
//;;BEGIN SPD 138431 - OPNSIZFX.NLM Change 34 of 34 - TDR 5/23/97
//				LONG	*rLen,//);
				LONG	*rLen,
				LONG	*FileHandle);
//;;END SPD 138431 - OPNSIZFX.NLM Change 34 of 34 - TDR 5/23/97

extern LONG GenParsePath(
							LONG	NameSpace,
						 	LONG	Station,
						 	NWHandlePath *NPathInfo,
						 	LONG	*Volume,
							LONG 	*PathCount,
						 	LONG	*PathBase,
							BYTE	**PathString);

#if CROSSNAMEFIX
extern LONG GenParsePathEx(
							LONG	NameSpace,
						 	LONG	Station,
						 	NWHandlePath *NPathInfo,
						 	LONG	*Volume,
							LONG 	*PathCount,
						 	LONG	*PathBase,
							BYTE	**PathString,
							LONG 	*MixedModeFlag);
#endif

extern LONG GenFullParsePath(
							LONG	NameSpace,
						 	LONG	Station,
						 	NWHandlePath *NPathInfo,
							LONG	cookieFlags,
						 	LONG	*Volume,
						 	LONG	*PathBase);

extern LONG GenParseCompactPath(
							LONG NameSpace,
							LONG Station,
							NWRenMoveHandlePath *NPathInfo,
							BYTE *String,
							LONG *Volume,
							LONG *PathCount,
							LONG *PathBase,
							BYTE **PathString,
							BYTE **NewString);

/****************************************************************************/
/* Function Prototypes for NLM interface to the Generic NCPs. */
/* These Functions can be exported */
extern LONG	GenNSOpenCreateFile(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							BYTE					OpenCreateFlags,
							WORD					SearchAttributes,
							WORD					DesiredAccessRights,
							LONG					CreateAttributes,
							LONG					ReturnInfoMask,
							LONG					*FileHandle,
							BYTE					*OpenCreateAction,
							NetwareInfo			*NInfo,
							BYTE				 	*FileName);


extern LONG	GenNSInitFileSearch(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							BYTE					*VolumeNumber,		  /* Search Seq. */
							LONG					*DirectoryNumber,
							LONG					*EntryNumber);
							

extern LONG	GenNSContinueFileSearch(
							LONG					Station,
							BYTE					NameSpace,
							BYTE					DataStream,
							WORD					SearchAttributes,
							LONG					ReturnInfoMask,
							BYTE					SVolume,		 			/* Search Seq. */
							LONG					SDirectoryNumber,
							LONG					SEntryNumber,
							BYTE					*SearchPattern,
							LONG					*NewSEntryNumber,
							NetwareInfo			*NInfo,
							BYTE					*FileName);

/* replaced GenNSRename -- which had coding errors in it. */
extern	LONG	GenNSRenameMove(
			LONG	Station,
			LONG	Task,
			BYTE	NameSpace,
			BYTE	RenameFlag,
			WORD	SearchAttributes,
			NWRenMoveHandlePath 	*SrcPathInfo,	/* special structure for rename */
			NWRenMoveHandlePath	*DstPathInfo,
			BYTE	*PInfo);	/* path info for both src & dst */

extern LONG	GenNSObtainInfo(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					SrcNameSpace,
							BYTE					DstNameSpace,
							WORD					SearchAttributes,
							LONG					ReturnInfoMask,
							NetwareInfo			*NInfo,
							BYTE					*FileName);
							

							
extern LONG	GenNSModifyInfo(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					SearchAttributes,
							LONG					ModifyMask,
							ModifyInfo			*MInfo);


extern LONG GenNSErase(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					SearchAttributes);


extern LONG	GenNSAllocDirHandle(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					AllocateMode,
							BYTE					*NewDirectoryHandle,
							BYTE					*VolumeNumber);

extern LONG	GenNSAllocateDirHandle(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					SrcNameSpace,
							WORD					AllocateMode,
							BYTE					DstNameSpace,
							BYTE					*NewDirectoryHandle,
							BYTE					*VolumeNumber);

extern LONG	GenNSGetDirBase(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					Reserved, /* if Reserved == 'Jn' then DstNameSpace is valid*/
							LONG					*NSDirectoryBase,
							LONG					*DOSDirectoryBase,
							BYTE					*VolumeNumber,
							BYTE					DstNameSpace);

extern LONG	GenNSGetNameSpaceList(
							BYTE					VolumeNumber,
							WORD					*NumberNSLoaded,
							BYTE					*NSLoadedList);	/* points to a buffer at least MaximumNumberOfNameSpaces big */

extern LONG GenNSScanSalvageableFiles(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							BYTE					DataStream,
							LONG					ReturnInfoMask,
							LONG					ScanSequence,
							LONG					*NextScanSequence,
							LONG					*DeletedDateAndTime,
							LONG					*DeletorID,
							LONG					*DeletorVolume,
							LONG					*DeletorDirectoryBase,
							NetwareInfo			*NInfo,
							BYTE					*FileName);


extern LONG GenNSRecoverSalvageableFile(
							LONG					Station,
							LONG					NameSpace,
							LONG					Sequence,
							LONG					Volume,
							LONG					DirectoryBase,
							BYTE					*NewFileName);


								
extern LONG GenNSPurgeSalvageableFile(
							LONG					Station,
							LONG					NameSpace,
							LONG					Sequence,
							LONG					Volume,
							LONG					DirectoryBase);


extern LONG	GenNSGetPathString(
							LONG					Station,
							BYTE					NameSpace,
							BYTE					DirectoryHandle,
							BYTE					*PathString);							


extern LONG	GenNSScanForTrustees(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					SearchAttribute,
							LONG					ScanSequence,
							LONG					*NextScanSequence,
							WORD					*ObjectIDCount,
							Trustees				*STrustees);
							

extern LONG	GenNSAddTrustees(
							LONG					Station,
							NWHandlePath 		*NPathInfo,
							BYTE					NameSpace,
							WORD					SearchAttributes,
							WORD					TrusteeRightsMask,
							WORD					ObjectIDCount,
							Trustees				*ATrustees);
							

extern LONG GenNSDeleteTrustees(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							WORD					ObjectIDCount,
							Trustees				*DTrustees);


extern LONG	GenNSSetDirHandle(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							BYTE					DataStream,
							BYTE					DstDirHandle);


extern LONG	GenNSQueryNameSpaceInfo(
							BYTE					NameSpace,
							BYTE					VolumeNumber,
							LONG					*FixedFieldsMask,
							LONG					*VariableFieldsMask,
							LONG					*HugeFieldsMask,
							WORD					*FixedBitsDefined,
							WORD					*VariableBitsDefined,
							WORD					*HugeBitsDefined,
							LONG					*FieldsLengthTable); /* pointer to space for 32 longs */


extern LONG	GenNSGetSpecificInfo(
							LONG 					Station,
							BYTE					SrcNameSpace,
							BYTE					DstNameSpace,
							BYTE					VolumeNumber,
							LONG					DirectoryBase,
							LONG					NSInfoBitMask,
							BYTE					*NSData);


extern LONG	GenNSModifySpecificInfo(
							LONG 					Station,
							LONG 					Task,
							BYTE					SrcNameSpace,
							BYTE					DstNameSpace,
							BYTE					VolumeNumber,
							LONG					DirectoryBase,
							LONG					ModifyMask,
							WORD					ModifyDataLength,
							BYTE					*ModifyData);

extern LONG	GetHugeInformation(
							LONG	Station,
							LONG	Task,
							BYTE	NameSpace,
							BYTE	VolumeNumber,
							LONG	DirectoryBase,
							LONG	HugeMask,
							BYTE	*HugeStateInfo,
							BYTE	*HugeData,
							LONG	*HugeDataLength,			
							BYTE	*NextHugeStateInfo);

extern LONG	SetHugeInformation(
							LONG	Station,
							LONG	Task,
							BYTE	NameSpace,
							BYTE	VolumeNumber,
							LONG	DirectoryBase,
							LONG	HugeMask,
							BYTE	*HugeStateInfo,
							LONG	HugeDataLength,
							BYTE	*HugeData,
							BYTE	*NextHugeStateInfo,
							LONG	*HugeDataUsed);

extern LONG	GenNSGetFullPathString(
							LONG					Station,
							BYTE					SourceNameSpace,
							BYTE					DestinationNameSpace,
							NWHandlePath		*NPathInfo,	
							PathCookie			*NLMCookie,
							LONG					NewPathBufferSize,
							BYTE					*NewPathBuffer,
							WORD					*NewPathComponentSize,
							WORD					*NewPathComponentCount);

extern LONG	GenNSEffectiveDirectoryRights(
							LONG					Station,
							NWHandlePath		*NPathInfo,
							BYTE					SrcNameSpace,
							BYTE					DstNameSpace,
							WORD					SearchAttributes,
							LONG					ReturnInfoMask,
							WORD					*EffectiveRights,					
							NetwareInfo			*NInfo,
							BYTE					*FileName);

extern LONG	GenNSRename(
							LONG	Station,
							LONG	Task,
							NWHandlePath 	*SrcPathInfo,
							NWHandlePath	*DstPathInfo,
							BYTE	NameSpace,
							WORD	SearchAttributes);

extern LONG	GenNSOpenCreate(
							LONG					Station,
							LONG					Task,
							NWHandlePath		*NPathInfo,
							BYTE					NameSpace,
							BYTE					DataStream,
							BYTE					OpenCreateFlags,
							WORD					SearchAttributes,
							WORD					DesiredAccessRights,
							LONG					CreateAttributes,
							LONG					ReturnInfoMask,
							LONG					*FileHandle,
							BYTE					*OpenCreateAction,
							NetwareInfo			*NInfo,
							BYTE				 	*FileName);

/****************************************************************************/
/****************************************************************************/
/* ************************ NCP 87 (Enhanced NCP) has subfunctions ************************** */

typedef struct NCPPathHandle	/* used in CASE 87 Enhanced NCPs*/
{
	BYTE	Volume;
	LONG	DirectoryBaseOrHandle;
	BYTE	HandleFlag;
	BYTE	PCC;
	BYTE	PS;				/* Path componenets start here.*/
									/* PathString may not be prenet if component count is 0*/
} NCPHandlePath;

typedef struct NCPShortHandlePath	/* used in CASE 87 Enhanced NCPs*/
{	/* Just like the HandlePath but doesn't include the actual path stuff*/
	BYTE	Volume;
	LONG	DirectoryBaseOrHandle;
	BYTE	HandleFlag;
	BYTE	PCC;		/*Path Component Count */
} NCPShortHandlePath;

/* ************************************************************************ */
/* ************************************************************************ */
/* ************************************************************************ */
/* ************************************************************************ */

/* ************************ ncp 90 10 ************************** */
typedef	struct	{
	BYTE	Skip[37];
	WORD	SubFuncLen;
	BYTE	Subfunction;
	LONG 	Volume;
	LONG	DirBase;
	LONG	NameSpace;
} ncp90_10;

/* ************************ ncp 90 11 ************************** */
typedef	struct	{
	BYTE	Skip[37];
	WORD	SubFuncLen;
	BYTE	Subfunction;
	LONG	Handle;
} ncp90_11;

/* ************************ ncp 90 12 ************************** */
typedef	struct	{
	BYTE	Skip[37];
	WORD	SubFuncLen;
	BYTE	Subfunction;
	BYTE	NetWareFileHandle[6];
	LONG	SuggestedFileSize;
} ncp90_12;

typedef	struct	{
	LONG	OldFileSize;
	LONG	NewFileSize;
} ncp90_12_reply;

/****************************************************************************/
/****************************************************************************/


extern void	ASyncWaitCallBack(
			WORD connectionNumber,
			LONG CompletionCode,
			LONG Task);

/*	define the data structures.	*/

extern struct ASyncLockWaitStructure	*ASyncDoneList;
extern struct ASyncLockWaitStructure	*ATimeOutList;

/*	define AStatus values	*/

#define Inserting	0
#define Waiting		1
#define Done		2
#define Pending		3
#define NeedsFree	4
#define OCAcked		5


/*	define the wait type values	*/

#define PhysicalRecordLock		0
#define NonPhysicalRecordLock	1
#define OpenCallbackType		2

struct ASyncLockWaitStructure
{
	struct ASyncLockWaitStructure *ALink;
	LONG AStation;
	LONG ATask;
	LONG AHandle;
	LONG ACompletionCode;
	LONG ALastTime;
	LONG AWaitThread;
	LONG ATimeOutTime;
	struct ASyncLockWaitStructure *ATOLink;
	BYTE AStatus;
	BYTE AWaitType;
};


extern LONG (*DupFileHook)(
			struct RequestPacketStructure *Request,
			struct ReplyProceduresStructure *RP,
			LONG reserved, LONG PacketSize,
			BYTE *Answer, LONG AnswerBufferLength);

/****************************************************************************/
/****************************************************************************/

#define	ssflReturnFileName			0x00000001
#define	psflPurgeAll		0x00000001

/* ncp 87 41 */
typedef struct
{
	BYTE				NameSpace;
	BYTE				reserved;
	WORD				ControlFlags;
	LONG				ScanSequence;
	NWHandlePath	NPathInfo;
} ScanSalvFileList;

typedef struct
{
	LONG NextScanSequence;
	LONG PurgeBase;
	LONG ScanItems;
	BYTE ScanInfo;
} ScanSalvFileListRep;

/* ncp 87 42 */
typedef struct
{
	BYTE NameSpace;
	BYTE reserved;
	WORD ControlFlags;
	LONG Volume;
	LONG PurgeBase;
	LONG PurgeCount;
	LONG PurgeList;	/* PurgeList[PurgeCount] */
} PurgeSalvFileList;

typedef struct
{
	LONG PurgedCount;
	LONG PurgedList;	/* PurgedListpPurgedCount] */	
} PurgeSalvFileListRep;


/* NCP 87 43 */
typedef struct
{
	BYTE	reserved[3];
	BYTE	queryFlag;	/* set to 0x1 if just querying for current rights */
	LONG	fileHandle;
	LONG	removeRights;
} RevokeFileHandleRightsReq;

typedef struct
{
	LONG	fileHandle;
	LONG	currentRights;
} RevokeFileHandleRightsRep;

/* NCP 87 64 */
typedef struct
{
	LONG	FileHandle;				/* hi - lo*/
	UINT64	StartingByteOffset;		/* hi - lo*/
	WORD	BytesToRead;			/* hi - lo*/
} ReadFile64Req;

typedef struct
{
	WORD     BytesActuallyRead;		/* hi - lo*/
	BYTE     Data;
} ReadFile64Rep;

/* NCP 87 65 */
typedef struct
{
	LONG	FileHandle;				  /* hi - lo*/
	UINT64	StartingByteOffset;		  /* hi - lo*/
	WORD	BytesToWrite;			  /* hi - lo*/
	BYTE 	WriteBuffer;
} WriteFile64Req;

/* NCP 87 66 */
typedef struct
{
	LONG	FileHandle; 			  /* hi-lo*/
} GetFileSize64Req;

typedef struct
{
	UINT64	CurrentFileSize; 		  /* lo - hi*/
} GetFileSize64Rep;

/* NCP 87 67 */
typedef struct
{
	LONG	LockFlag;				/* lo - hi*/
	LONG	FileHandle;	  			/* hi - lo*/
	UINT64	LockAreaStartOffset;	/* hi - lo*/
	UINT64	LockAreaLength;			/* hi - lo*/
	LONG	LockTimeout;			/* hi - lo*/
} LogPhysicalRecord64Req;

/* NCP 87 68 */
typedef struct
{
	LONG	FileHandle;	  			/* hi - lo*/
	UINT64	LockAreaStartOffset;	/* hi - lo*/
	UINT64	LockAreaLength;			/* hi - lo*/
} ReleasePhysicalRecord64Req;

/* NCP 87 69 */
typedef struct
{
	LONG	FileHandle;	  			/* hi - lo*/
	UINT64	LockAreaStartOffset;	/* hi - lo*/
	UINT64	LockAreaLength;			/* hi - lo*/
} ClearPhysicalRecord64Req;

#if NTCLIENTECO
/* NCP 87 44 */
typedef struct
{
	BYTE	reserved[2];
	BYTE	volumeNumber;
	BYTE	nameSpace;
	LONG	directoryNumber;
	WORD	oldAccessRights;
	WORD	newAccessRights;
	LONG	fileHandle;
} UpdateFileHandleRightsReq;

typedef struct
{
   LONG     fileHandle;
   LONG     currentRights;
} UpdateFileHandleRightsRep;

/* in LOCKS1.C */
extern LONG UpdateFileHandleRights(
					LONG Station,
					LONG Task,
					LONG VolumeNumber,
					LONG NameSpace,
					LONG DirectoryNumber,
					LONG OldAccessRights,
					LONG NewAccessRights,
					LONG FileHandle,
					LONG *newRights);
#endif

/* in LOCKS1.C */
extern LONG RevokeFileHandleRights(
					LONG Station,
					LONG Task,
					LONG FileHandle,
					LONG QueryFlag,
					LONG removeRights,
					LONG *newRights);

extern LONG F3ScanSalvageFileList(
					LONG Station,
					LONG NameSpace,
					LONG Volume,
					LONG PathCount,
					LONG PathBase,
					BYTE *PathString,
					LONG ControlFlags,
					LONG ScanSequence,
					LONG *NextScanSequence,
					LONG *PurgeBase,
					BYTE *info,
					LONG *itemCount,
					LONG *listSize);

extern LONG F3PurgeSalvageFileList(
					LONG Station,
					LONG NameSpace,
					LONG Volume,
					LONG PurgeBase,
					LONG ControlFlags,
					LONG *PurgeCount,
					LONG *PurgeList,
					LONG *PurgeCCodeCount,
					LONG *PurgeCCodeList);



#endif /* __ENCP_H__ */
