/****************************************************************************
 *
 *                      旼컴컴컴컴컴컴컴컴컴컴컴컴커
 *                      � FOR INTERNAL USE ONLY!!! �
 *                      읕컴컴컴컴컴컴컴컴컴컴컴컴켸
 *
 * Program Name:  Storage Management Services (NWSMS Lib)
 *
 *   modname: wildpat.c    version: 2.1    date: 06/21/96
 *   PVCS:        $Revision$   $date$
 *
 * Date Created:  25 APRIL 1990
 *
 * Version:       3.11
 *
 * Programmers:   Del Robins
 *
 * Files used:    nwsms.h
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

#include <internal.h> 
#include <nwsms.h>

#if defined(NETWARE_V320)
	#undef __INLINE_FUNCTIONS__
#endif
#include <string.h>
#include <nwlocale.h>

void NWStoreAsWildPath(
    STRING packetPtr,
    UINT8  nameSpace,
    STRING path)
{
    char *buffPtr;
	 int i;

	 if (nameSpace is NWNAME_SPACE_MAC)
	 {
#if defined(NETWARE_V320) || defined(NETWARE_V312)
		strcpy(packetPtr + 1, path);
    	*packetPtr = (UINT8 )strlen(packetPtr + 1);
#else
		strcpy(packetPtr, path);
#endif
	 }

	 else
	{

    	buffPtr   = packetPtr;
#	if defined(NETWARE_V320) || defined(NETWARE_V312)
    	buffPtr++;
#	endif

    	while (*path)
    	{
    		switch (*path)
        	{
        	case  '*':
        		
			case 0xFF:
				
            	*buffPtr++ = 0xFF;
            	*buffPtr++ = *path++;
            	break;

        	default:
                                for (i = NWCharType((unsigned char)*(path)); i > 0; --i)
				{
				
       	    	*buffPtr++ = *path++;
				}
                
           	break;
        	}
    	}
		
    	*buffPtr = 0;

//#	if defined(NETWARE_V320) || defined(NETWARE_V312)
    	*packetPtr = (UINT8 )strlen(packetPtr + 1);
    	/*
packetPtr++;
*packetPtr++=0XFF;
*packetPtr++=*path;
packetPtr=0;
packetPtr--;
packetPtr--;
*packetPtr=(UINT8 )strlen(packetPtr + 2);
*/



//#	endif
//#	endif
	}
}

/****************************************************************************/
/****************************************************************************/

