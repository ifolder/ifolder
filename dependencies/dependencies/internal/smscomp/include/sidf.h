/*
====================================================================================
Novell Software Developer Kit Sample Code License

Copyright (C) 2003-2004 Novell, Inc.  All Rights Reserved.

THIS WORK IS SUBJECT TO U.S. AND INTERNATIONAL COPYRIGHT LAWS AND TREATIES.  
USE AND REDISTRIBUTION OF THIS WORK IS SUBJECT TO THE LICENSE AGREEMENT 
ACCOMPANYING THE SOFTWARE DEVELOPMENT KIT (SDK) THAT CONTAINS THIS WORK.  
PURSUANT TO THE SDK LICENSE AGREEMENT, NOVELL HEREBY GRANTS TO DEVELOPER 
A ROYALTY-FREE, NON-EXCLUSIVE LICENSE TO INCLUDE NOVELL'S SAMPLE CODE IN ITS 
PRODUCT.  NOVELL GRANTS DEVELOPER WORLDWIDE DISTRIBUTION RIGHTS TO MARKET, 
DISTRIBUTE, OR SELL NOVELL'S SAMPLE CODE AS A COMPONENT OF DEVELOPER'S PRODUCTS.
NOVELL SHALL HAVE NO OBLIGATIONS TO DEVELOPER OR DEVELOPER'S CUSTOMERS WITH 
RESPECT TO THIS CODE.

NAME OF FILE: 
			sidf.h

PURPOSE/COMMENTS:
			Header file defining types, macros and constants for manipulating SIDF-compliant data fields 

NDK COMPONENT NAME AND VERSION:
			SMS Developer Components

LAST MODIFIED DATE:
			22 Jan 2004 
====================================================================================
*/


#ifndef _SIDF_H_INCLUDED          /* sidf.h header Latch */
#define _SIDF_H_INCLUDED


#define SIDF_SYNC_DATA                 0x5AA5
#define SIDF_FIELD_HEADER_SIZE         14
#define SIDF_INVALID_OFFSET            0xFFFFFFFF
#define SIDFSTREAM_CSTR_FAILURE(c)     {dstrSIDFStream(c); return(NULL);}

#define SIDF_MIN_BLANK_SECTION_SIZE    32

/* (SIDFField) types and constants*/

#pragma pack(push, 1)

typedef struct                         /* SIDFField type definition */
{
	UINT32      fid;
	UINT32      size;
	UINT32      control;
	void       *data;
} SIDFField;

#pragma pack(pop)

#define FIELD_CTL_NULL_FID             0x00000000
#define FIELD_CTL_FORMAT1              0x00000100
#define FIELD_CTL_FORMAT2              0x00000200
#define FIELD_CTL_FORMAT3              0x00000400
#define FIELD_CTL_VARIABLE_SIZE        0x00000800
#define FIELD_CTL_DATA_IS_HUGE         0x00001000
#define FIELD_CTL_SECTION_BEGIN        0x00002000
#define FIELD_CTL_SECTION_END          0x00004000
#define FIELD_CTL_CALCULATE_CRC        0x00008000
#define FIELD_CTL_WAS_FOUND            0x00010000
#define FIELD_CTL_DATA_IS_UINT16       0x00000001
#define FIELD_CTL_DATA_IS_UINT32       0x00000002
#define FIELD_CTL_DATA_IS_UINT64       0x00000003
#define FIELD_CTL_DATA_IS_STRING       0x00000004
#define FIELD_CTL_DATA_IS_STREAM       0x00000005
#define FIELD_CTL_DATA_IS_TIMESTAMP    0x00000006

#define FIELD_CTL_DATATYPE_MASK        0x000000FF
#define FIELD_CTL_SIZE_FORMATS         0x00000F00
#define FIELD_CTL_FORMAT_HEADER        (FIELD_CTL_FORMAT1 | \
                                        FIELD_CTL_SECTION_BEGIN)
#define FIELD_CTL_WRITE_UINT32         (FIELD_CTL_FORMAT1 | \
                                        FIELD_CTL_DATA_IS_UINT32)
#define FIELD_CTL_SMALL_STRING         (FIELD_CTL_FORMAT1 | \
                                        FIELD_CTL_DATA_IS_STRING)


/* (SIDFField) macros */

#define SIDF_FieldType(c)         ((c) & FIELD_CTL_DATATYPE_MASK)
#define SIDF_ClearFieldType(c)    ((c) &= ~FIELD_CTL_DATATYPE_MASK)
#define SIDF_ResetFieldType(c,t)  ((c)=((c) & ~FIELD_CTL_DATATYPE_MASK)|t)

#define UINT32SigBytes(n)         (((n) & 0xFF000000) ? 4 :\
                                   ((n) & 0x00FF0000) ? 3 :\
                                   ((n) & 0x0000FF00) ? 2 : 1)

#define SIDF_SizeOfFID(f)         UINT32SigBytes(f)

#define SIDF_SizeOfUINT32(n)      (((n) & 0xFFFF0000) ? 4 : \
                                   ((n) & 0x0000FF00) ? 2 : 1)

#define IsFixedLong(f)            (((f) & 0xFFFF0000)        &&             \
                                   (((f) & 0xF000) == 0xF000))
#define IsFixedShort(f)           ((((f) & 0xFF000000) == 0) &&             \
                                   (((f) & 0xC0) == 0x40))
#define LongFixedSz(f)            (1 << (((f)&0x700) >> 8))
#define ShortFixedSz(f)           (1 << ((f)&0x07))

#define SIDF_GetFixedSize(f)      (IsFixedLong(f) ? LongFixedSz(f) :        \
                                   IsFixedShort(f) ? ShortFixedSz(f) : 0)
/*
#define SIDF_IsFixedSize(f)       ((f&0xFF000000) ? ((f&0xF000) == 0xF000) :\
                                   (!(f&0xFF0000)) ? (f & 0x40)            :\
                                   (f & 0x400000)  ? (f & 0x40)            :\
                                   ((f & 0xF000) == 0xF000))

#define SIDF_GetFixedSize(f)      ((!(f&0xFFFF0000) || (!(f&0xFF000000) && \
                                   (f&0x400000))) ? (1 << (f&0x07))      : \
                                   (1 << ((f&0x700) >> 8)))
*/

#define SIDF_OffsetToEndSize(c)   ((c & FIELD_CTL_DATA_IS_UINT32)? 4 :\
                                   (c & FIELD_CTL_DATA_IS_UINT64)? 8 : 2)

#define SIDF_SizeFormat1(s)       ((s) & 0x7F)

#define SIDF_SizeFormat2(s)       (((s) & 0xFFFF0000) ? 0x82 : \
                                   ((s) & 0x0000FF00) ? 0x81 : 0x80)

#define SIDF_SizeFormat3(d)       ((*(BYTE *) (d) | 0xC0))


#endif                                /* Header Latch */
