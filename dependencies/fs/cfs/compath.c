/****************************************************************************
 *
 *                      +--------------------------+
 *                      | FOR INTERNAL USE ONLY!!! |
 *                      +--------------------------+
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 *   modname: compath.c    version: 2.5    date: 11/12/96
 *   PVCS:        $Revision$   $date$
 *
 * Date Created:  25 APRIL 1990
 *
 * Version:       3.11
 *
 * Programmers:   Del Robins
 *
 * Files used:    nwsms.h & string.h
 *
 * Date Modified: 
 *
 * Modifications: 
 *
 * Comments:      
 *
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized or
 * modified in any manner or compiled, linked or uploaded or downloaded to or
 * from any computer system without the prior written consent of Novell, Inc.
 ****************************************************************************/

#include <compath.h>
#include <nwsms.h>
//#include <nwlocale.h>


void *ImportSymbol(int, char *);	/* from CLIB */
typedef UINT32 (* ImportedSym)();

CCODE _NWStoreAsComponentPath(
	STRING componentPath,
	UINT8  nameSpace,
	STRING path,
	NWBOOLEAN pathIsFullyQualified)
{
#if defined(NETWARE_V320)
static UINT32 (*GetDOSUpperCaseTable)() = 0;
static BYTE DOSUpperCaseTable[256] = {0};
extern UINT32 tsaNLMHandle;
#elif defined(NETWARE_V312)
extern BYTE DOSUpperCaseTable[];
#endif
	char *buffPtr, ch, count, *countByte, *lenByte, levels;
	int   fillLength, totalPathLength, i;


	buffPtr = componentPath;
	if (!path)
	  *buffPtr = 0;

	else
	{
		countByte	  	= buffPtr++;
		lenByte		 	= buffPtr++;
		levels		  	= 0;
		count			= 0;
		totalPathLength = 0;
		if (pathIsFullyQualified)
		{   
			while (*path)
			{
			    
				switch (*path)
				{
				case  0:
					levels++;
					break;

				case '\\':
				case '/':
					if (nameSpace == NWNAME_SPACE_MAC)
					{
						count++;
						ch = *path++;
						totalPathLength++;
						*buffPtr++ = ch;
					}
					else
					{
						path++;
						if (count)
						{
							levels++;
							*lenByte = count;
							lenByte = buffPtr++;
							totalPathLength++;
							count = 0;
						}
					}
					break;

				case ':':
					if (nameSpace == NWNAME_SPACE_MAC)
					{
						path++;
						if (count)
						{
						 	levels++;
						 	*lenByte = count;
						 	lenByte = buffPtr++;
						 	totalPathLength++;
						 	count = 0;
						}
					}
					else
					{
						/* In nfs a colon is valid in a file name,
						   we only want to use the first colon as a
						   separator, so if levels is > 0, just copy
							the character
						*/
						if (levels)
						{
							count++;
							*buffPtr++ = *path++;
							totalPathLength++;
						}
						else 
						{
							path++;
						 	levels++;
						 	*lenByte = count;
						 	lenByte = buffPtr++;
						 	totalPathLength++;
						 	count = 0;
						}
					}
					break;

				case 0xFF:
					if (nameSpace == DOSNameSpace)
					{
						/* The character 0xFF is the OSs break character,
						to really indicate an 0xFF, it must be preceded
						or escaped by another 0xFF */

						count +=2;
						*buffPtr++ = *path;
						*buffPtr++ = *path++;
						totalPathLength += 2;
					}
					else
					{
						count++;
						*buffPtr++ = *path++;
						totalPathLength++;
					}
					break;

				case 0x10:
				case 0x11:
				case 0x12:
			#if defined(NETWARE_V320)
				if (!GetDOSUpperCaseTable)
				{
					GetDOSUpperCaseTable = ImportSymbol(tsaNLMHandle, "GetDOSUpperCaseTable");
					if (GetDOSUpperCaseTable)
						GetDOSUpperCaseTable(DOSUpperCaseTable);
					else
					/* if can't get table, don't map up */
					{
						count++;
						*buffPtr++ = *path++;
						totalPathLength++;
						break;
					}

				}
			#endif
					count++;
			#if !defined(NETWARE_V311)
					if (nameSpace == NWNAME_SPACE_DOS)
						*buffPtr++ = DOSUpperCaseTable[*path++];
					else
			#endif
						*buffPtr++ = *path++;
					totalPathLength++;
					break;

				default:
                                        for (i = NWCharType((unsigned char) *(path)); i > 0; --i)
					{
						count++;
						*buffPtr++ = *path++;
						totalPathLength++;
					}
					break;
				}
		
				/*... this check is to see that we don't overwrite memory...*/
				if (totalPathLength >= NW_COMPONENT_PATH_LEN)
					return (NWERR_FS_PATH_TOO_LONG);
			}
		}
		else
		{  
			//ConsolePrintf("relative path \r\n");
			while (*path)
			{
			   // ConsolePrintf("%c.",*path);
				switch (*path)
				{
				case  0:
					levels++;
					break;

				case 0xFF:
						if (nameSpace == DOSNameSpace)
						{
							/* The character 0xFF is the OSs break character,
							to really indicate an 0xFF, it must be preceded
							or escaped by another 0xFF */

							count +=2;
							*buffPtr++ = *path;
							*buffPtr++ = *path++;
							totalPathLength += 2;
						}
						else
						{
							count++;
							*buffPtr++ = *path++;
							totalPathLength++;
						}
						break;

				case 0x10:
				case 0x11:
				case 0x12:
			#if defined(NETWARE_V320)
					if (!GetDOSUpperCaseTable)
					{
						GetDOSUpperCaseTable = ImportSymbol(tsaNLMHandle, "GetDOSUpperCaseTable");
						if (GetDOSUpperCaseTable)
							GetDOSUpperCaseTable(DOSUpperCaseTable);
						else
						/* if can't get table, don't map up */
						{
							count++;
							*buffPtr++ = *path++;
							totalPathLength++;
							break;
						}

					}
			#elif defined(NETWARE_V312)
					count++;
					if (nameSpace == NWNAME_SPACE_DOS)
						*buffPtr++ = DOSUpperCaseTable[*path++];
					else
						*buffPtr++ = *path++;
					totalPathLength++;
					break;
			#endif

				default:
                                        for (i = NWCharType((unsigned char)*(path)); i > 0; --i)
					{
						count++;
						*buffPtr++ = *path++;
						totalPathLength++;
					}
					break;
				}
				/*... this check is to see that we don't overwrite memory...*/
				if (totalPathLength >= NW_COMPONENT_PATH_LEN)
					return (NWERR_FS_PATH_TOO_LONG);
			}

		}

		if (count)
		{
			*lenByte = count;
			levels++;
		}

		*countByte = (char)levels;

		fillLength  = NW_COMPONENT_PATH_LEN - (buffPtr - countByte);
		buffPtr	+= fillLength+1;
	}
	 //ConsolePrintf("\r\n");
	return (0);
}

