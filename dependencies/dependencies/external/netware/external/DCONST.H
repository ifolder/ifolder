/***************************************************************************
 *
 * (C) Unpublished Copyright of Novell, Inc.  All Rights Reserved.
 *
 * No part of this file may be duplicated, revised, translated, localized
 * or modified in any manner or compiled, linked or uploaded or downloaded
 * to or from any computer system without the prior written consent of
 * Novell, Inc.
 *
 *  $release$
 *  $modname: dconst.h$
 *  $version: 1.2$
 *  $date: Wed, Sep 7, 1994$
 *  $nokeywords$
 *
 ***************************************************************************/

#ifndef __DCONST_H
#define __DCONST_H

/* maximum number of characters in names, not including terminator */
#define MAX_RDN_CHARS 			128
#define MAX_DN_CHARS				256
#define MAX_SCHEMA_NAME_CHARS	32
#define MAX_TREE_NAME_CHARS	32
#define MAX_SAP_NAME_CHARS		47

/* maximum size of names, including terminator */
#define MAX_RDN_BYTES			(2*(MAX_RDN_CHARS + 1))
#define MAX_DN_BYTES				(2*(MAX_DN_CHARS + 1))
#define MAX_SCHEMA_NAME_BYTES	(2*(MAX_SCHEMA_NAME_CHARS + 1))

#define MAX_ASN1_NAME			32
#define MAX_VALUE					(63U * 1024U)

#define DELIM_WILD				'*'	/* delimiter in filters */
#define DELIM_VALUE				'='	/* delimiter in names */
#define DELIM_DV					'+'	/* delimiter in names */

/* delimiters in dot form names */
#define DELIM_DF_RDN				'.'
#define DELIM_DF_ESCAPE			'\\'

/* delimiters in slash form names */
#define DELIM_SF_RDN				'\\'
#define DELIM_SF_ESCAPE			'\''
#define DELIM_SF_RELATIVE		'.'

/* unicode characters not accepted by DS */
#define UNI_BYTE_ORDER_MARK			0xfeff
#define UNI_SWAPPED_BYTE_ORDER_MARK	0xfffe
#define UNI_REPLACEMENT_CHAR			0xfffd
#define UNI_NON_CHAR						0xffff

/* special entry names in ACLs */
#define DS_ROOT_NAME				"[Root]"
#define DS_PUBLIC_NAME			"[Public]"
#define DS_MASK_NAME				"[Inheritance Mask]"
#define DS_CREATOR_NAME			"[Creator]"		/* can only be used in AddEntry */
#define DS_SELF_NAME				"[Self]"			/* can only be used in AddEntry */

/* special attribute names in ACLs */
#define DS_ALL_ATTRS_NAME		"[All Attributes Rights]"
#define DS_SMS_RIGHTS_NAME		"[SMS Rights]"
#define DS_ENTRY_RIGHTS_NAME	"[Entry Rights]"

typedef enum SYNTAX
{
	SYN_UNKNOWN, 					/* 0  */
	SYN_DIST_NAME, 				/* 1  */
	SYN_CE_STRING, 				/* 2  */
	SYN_CI_STRING, 				/* 3  */
	SYN_PR_STRING, 				/* 4  */
	SYN_NU_STRING,					/* 5  */
	SYN_CI_LIST, 					/* 6  */
	SYN_BOOLEAN, 					/* 7  */
	SYN_INTEGER, 					/* 8  */
	SYN_OCTET_STRING, 			/* 9  */
	SYN_TEL_NUMBER,				/* 10 */
	SYN_FAX_NUMBER, 				/* 11 */
	SYN_NET_ADDRESS, 				/* 12 */
	SYN_OCTET_LIST, 				/* 13 */
	SYN_EMAIL_ADDRESS,			/* 14 */
	SYN_PATH,						/* 15 */
	SYN_REPLICA_POINTER, 		/* 16 */
	SYN_OBJECT_ACL, 				/* 17 */
	SYN_PO_ADDRESS,				/* 18 */
	SYN_TIMESTAMP, 				/* 19 */
	SYN_CLASS_NAME, 				/* 20 */
	SYN_STREAM, 					/* 21 */
	SYN_COUNTER, 					/* 22 */
	SYN_BACK_LINK,					/* 23 */
	SYN_TIME,						/* 24 */
	SYN_TYPED_NAME,				/* 25 */
	SYN_HOLD,						/* 26 */
	SYN_INTERVAL,					/* 27 */
	SYNTAX_COUNT					/* 28 */
} SYNTAX;

typedef enum NAME_SPACE_TYPE
{
	DS_DOS, DS_MACINTOSH, DS_UNIX, DS_FTAM, DS_OS2
} NAME_SPACE_TYPE;

typedef enum REPLICA_TYPE 
{
	RT_MASTER, RT_SECONDARY, RT_READONLY, RT_SUBREF, RT_COUNT
} REPLICA_TYPE;

typedef enum REPLICA_STATE
{
	RS_ON, RS_NEW_REPLICA, RS_DYING_REPLICA,
	RS_LOCKED, RS_CRT_0, RS_CRT_1,
	RS_TRANSITION_ON,
	RS_SS_0 = 48, RS_SS_1,
	RS_JS_0 = 64, RS_JS_1, RS_JS_2,
	RS_MS_0 = 80,
	RS_COUNT
} REPLICA_STATE;

typedef enum NET_ADDRESS_TYPE 
{
	NT_IPX, NT_IP, NT_SDLC, NT_TOKENRING_ETHERNET, NT_OSI, NT_APPLETALK, NT_COUNT
} NET_ADDRESS_TYPE;

typedef enum NCP_SERVER_STATUS
{
	DS_UNKNOWN, DS_DOWN, DS_UP
} NCP_SERVER_STATUS;

#define IPX_ADDRESS_LEN	12

typedef enum EMAIL_ADDRESS_TYPE 
{
	SMF70 = 1, SMF71, SMTP, X400, SNADS, PROFS
} EMAIL_ADDRESS_TYPE;

/*---------------------------------------------------------------------------
 * definitions used by Access Control
 */
#define DS_ENTRY_BROWSE			0x00000001L
#define DS_ENTRY_ADD				0x00000002L
#define DS_ENTRY_DELETE			0x00000004L
#define DS_ENTRY_RENAME			0x00000008L
#define DS_ENTRY_SUPERVISOR	0x00000010L

#define DS_ENTRY_MASK			(DS_ENTRY_BROWSE | DS_ENTRY_ADD \
										| DS_ENTRY_DELETE | DS_ENTRY_RENAME \
										| DS_ENTRY_SUPERVISOR)

#define DS_ATTR_COMPARE			0x00000001L
#define DS_ATTR_READ				0x00000002L
#define DS_ATTR_WRITE			0x00000004L
#define DS_ATTR_SELF				0x00000008L
#define DS_ATTR_SUPERVISOR		0x00000020L

#define DS_ATTR_MASK				(DS_ATTR_COMPARE | DS_ATTR_READ | DS_ATTR_WRITE \
										| DS_ATTR_SELF | DS_ATTR_SUPERVISOR)

/*===========================================================================*/
#endif
