/*****************************************************************************
 *
 *	(C) Copyright 1987-1994 Novell, Inc.
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
 *  Written by : W. Kyle Unice, Drew Major, Howard Davis, Mel Oyler, Dana Henriksen
 *     Compression Algorythum by: Dana M. Henriksen
 * 
 *  $Workfile:   cdcommon.h  $
 *  $Modtime:   21 Aug 1996 19:32:36  $
 *  $Revision$
 *  
 ****************************************************************************/

#ifndef __CDCOMMON_H__
#define __CDCOMMON_H__

typedef struct compressFileHeader_s {

	BYTE	CompressCode;
	BYTE	CompressMajorVersion;
	BYTE	CompressMinorVersion;
	BYTE	Unused;
	LONG	OriginalFileLength;
	LONG	CompressedFileLength;
	LONG	DataTreeNodes;

	LONG	LengthTreeNodes;
	LONG	OffsetTreeNodes;
	LONG	NumberOfHoles;
	LONG	HeaderCheckSum;

} compressFileHeader_t, *compressFileHeader_tp, **compressFileHeader_tpp;

/***************************************************************************
 *                                                                         *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

typedef struct AlgorithmCustomStats_s {

		BYTE	*Description;
		LONG	Value;

} COMPRESS_CUSTOM_STATS, *COMPRESS_CUSTOM_STATS_P;

/***************************************************************************
 *                                                                         *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

typedef struct CompressFileEntry_s {

	struct 	CompressFileEntry_s *next;

	BYTE 	*FileName;				 /* Null terminated name */
	LONG	Volume;
	LONG	PercentageComplete;

} COMPRESS_FILE_ENTRY;

/***************************************************************************
 *                                                                         *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

typedef struct CompressionStats_s {

		BYTE	*CompressionAlgorithmName;
		LONG	CompressionAlogrythmCode;

		LONG	NumberOfCompressions;
		LONG	UnableToCompressCount;
		LONG	TotalSectorsUsedIfNotCompressed;
		LONG	TotalSectorsUsedCompressed;
		LONG	CompressionTickCount;
		LONG	CompressionAbortedCount;
		LONG	CompressionMemoryAllocationFailures;
		LONG	CompressionIOErrors;
		LONG	CompressionFileTooBig;
		LONG	CompressionFileTooSmall;

		COMPRESS_FILE_ENTRY	*FilesBeingCompressed;
		COMPRESS_FILE_ENTRY	*FilesQueuedToBeCompressed;

/* custom statistics */

		LONG	CustomStatsCount;

		COMPRESS_CUSTOM_STATS_P CustomStats;

} COMPRESS_STATS, *COMPRESS_STATS_P;

typedef struct DeCompressionStats_s {

		BYTE	*DecompressionAlgorithmName;
		LONG	DecompressionAlogrythmCode;

		LONG	TotalDecompressionsDone;
		LONG	TotalSectorsDecompressed;
		LONG	DecompressionTickCount;
		LONG	DecompressionAbortedCount;
		LONG 	DecompressionMemoryAllocationFailures;
		LONG	DecompressionFileCorruptionCount;
		LONG	DecompressionIOErrors;

		COMPRESS_FILE_ENTRY	*FilesBeingDecompressed;
		COMPRESS_FILE_ENTRY	*FilesQueuedToBeDecompressed;

/* custom statistics */

		LONG	CustomStatsCount;

		COMPRESS_CUSTOM_STATS_P CustomStats;

} DECOMPRESS_STATS, *DECOMPRESS_STATS_P;

/***************************************************************************
 *                                                                         *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

		/* things you might want to know:
			1. file name
			2. percent completed
			3. stage
			4. deleted file or not
			5. ratio so far
			6.
		*/

/* stage : 0- analyzing file 1-building compressed version */

/* in cdcompi.inc and cduncomi.inc also */

typedef struct CompressStatus_s {
	LONG 	directoryNumber;
	LONG 	compressionStage;
	LONG 	totalIntermediateBlocks;
	LONG 	totalCompressedBlocks;
	LONG 	totalInitialBlocks;
	LONG 	currentIntermediateBlocks;
	LONG 	currentCompressedBlocks;
	LONG 	currentInitialBlocks;
	LONG 	fileFlags;
	LONG 	projectedCompressedSize;  /* in bits */
	LONG 	originalSize; /* in bits */
	LONG	compressVolume;
} CompressStatus_t ;

/* state : 0- starting 1-building trees 2-decompressing */

typedef struct DecompressStatus_s {
	LONG 	directoryNumber;
	LONG 	totalBlocksToDecompress;
	LONG 	currentBlock;
	LONG 	state;
	LONG	decompressVolume;
} DecompressStatus_t;

typedef struct DecompressStatusNode_s {
	struct DecompressStatusNode_s *next;
	struct DecompressStatusNode_s *prev;
	DecompressStatus_t status;
} DecompressStatusNode_t ;


/***************************************************************************
 *                                                                         *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

typedef struct CompressAlgorithm_s {
		struct CompressAlgorithm_s *next;
		LONG (*CompressFile)( LONG inHandle, LONG outHandle,
			LONG tempHandle, LONG inFileSize, LONG volume,
			LONG *compressionHandle, BYTE *fileName, 
			CompressStatus_t *compressStatus );
		void (*GetStats)();
		BYTE CompressionCode;
		struct ResourceTagStructure *ResourceTag;
} COMPRESS_ALGO, *COMPRESS_ALGO_P;

typedef struct DecompressAlgorithm_s {
		struct	DecompressAlgorithm_s *next;
		LONG	(*DecompressFile)( LONG inHandle, LONG outHandle,
				LONG *decompressHandle, BYTE *fileName, LONG volume, 
				LONG directoryEntry );
		void	(*GetStats)();
		BYTE	DecompressionCode;
		struct ResourceTagStructure *ResourceTag;
} DECOMPRESS_ALGO, *DECOMPRESS_ALGO_P;


/***************************************************************************
 *
 * 
 *
 ***************************************************************************/

#define SWAP_LONG( x )  (long)\
   (( (unsigned long) ((unsigned long)((unsigned long)(x) & (unsigned long)0xFF000000) >> 24)) \
  | ( (unsigned long) ((unsigned long)((unsigned long)(x) & (unsigned long)0x00FF0000) >> 8 )) \
  | ( (unsigned long) ((unsigned long)((unsigned long)(x) & (unsigned long)0x0000FF00) << 8 )) \
  | ( (unsigned long) ((unsigned long)((unsigned long)(x) & (unsigned long)0x000000FF) << 24)))

/************************************************************************
 *
 *	 Major and minor versions.
 *
 ************************************************************************/

#define COMPRESS_MAJOR_VERSION 1
#define COMPRESS_MINOR_VERSION 0

/*************************************************************************
*
*  These bits indicate whether the file was compressed on a low hi
*  machine or a hi-low machine.  This bit is on or off in the compressCode
*
*************************************************************************/

#define NETWARE_COMPRESS_LOHI         0x01
#define NETWARE_COMPRESS_HILO         0x02
#define ARJ_COMPRESS_LOHI			  0x03
#define ARJ_COMPRESS_HILO             0x04

extern DecompressStatusNode_t *VolumeDecompressFilesTail[];
extern DecompressStatusNode_t *VolumeDecompressFiles[];
extern CompressStatus_t   *VolumeCurrentCompressingFile[];

extern LONG	VolumeCompressHighTickHigh[]; /* high order dword of total */
extern LONG	VolumeCompressHighTickCount[];/* high resolution tick count spent in compression */
extern LONG VolumeCompressByteInCount[];
extern LONG VolumeCompressByteOutCount[];
extern LONG VolumeCompressHighByteInCount[];
extern LONG VolumeCompressHighByteOutCount[];

extern LONG	VolumeDecompressHighTickHigh[];
extern LONG	VolumeDecompressHighTickCount[];
extern LONG VolumeDecompressByteInCount[];
extern LONG VolumeDecompressByteOutCount[];
extern LONG VolumeDecompressHighByteInCount[];
extern LONG VolumeDecompressHighByteOutCount[];

#define	CompressionDisabled	0
#define	CompressionMounting	1
#define	CompressionEnabled	2
#define	CompressionDismounting	3

extern BYTE CompressionEnabledTable[];

#endif /* __CDCOMMON_H__ */

/****************************************************************************/
/****************************************************************************/

