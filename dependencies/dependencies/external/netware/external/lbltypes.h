/*****************************************************************************
 *	
 *  (C) Copyright 1997-1998 Novell, Inc.
 *  All Rights Reserved.
 *
 *  This program is an unpublished copyrighted work which is proprietary
 *  to Novell, Inc. and contains confidential information that is not
 *  to be reproduced or disclosed to any other person or entity without
 *  prior written consent from Novell, Inc. in each and every instance.
 *
 *  WARNING:  Unauthorized reproduction of this program as well as
 *  unauthorized preparation of derivative works based upon the
 *  program or distribution of copies by sale, rental, lease or
 *  lending are violations of federal copyright laws and state trade
 *  secret laws, punishable by civil and criminal penalties.
 *	
 ****************************************************************************/
#include <dtypes.h>


/* Internal Labels Versions */

#define MA_LABEL_ITYPE_CORRUPT 255
#define MA_LABEL_ITYPE_NOVOL 254
#define MA_LABEL_ITYPE_UNKNOWN 253

/****************************************************************************/
//	Internal Structures between NDS and MASV
/****************************************************************************/
#define MA_LABEL_VER 1
#define MA_LABEL_SIZE 8

typedef struct MALabelStruct 
{
	uint8 version;		/* MA_LABEL_VERSION */
	uint8 size;			/* number of uint32 words */
	uint8 secLevel;	/* Secrecy levels	*/
	uint8 intLevel;	/* Integrity levels	*/
	uint32 secCats;	/* secCats */
	uint32 free1;		/* Reserved (0) future secCats */
	uint32 free2;		/* Reserved (0) future secCats */
	uint32 intCats;	/* Integrity categories */
	uint32 free3;		/* Reserved (0) future integrity cats */
	uint32 secSing;	/* Reserved (0) Singluar Security Category*/
	uint32 intSing;	/* Reserved (0) Singluar Integriy Category*/	
} MALabel;

typedef struct MARangeStruct
{
	uint32 rangeType;	/* Range flag (NWMA_CLR... not needed but handy) */
	MALabel readL;
	MALabel writeL;
} MARange;

/****************************************************************************/
/****************************************************************************/
