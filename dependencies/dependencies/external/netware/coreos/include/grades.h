/*****************************************************************************
 *
 *  (C) Copyright 1998 Novell, Inc.
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

#if !defined GRADES_H
#define GRADES_H

/***********************************************************************/
/* Graded Authentication Categories  					 		              */ 
/***********************************************************************/

/* Workstation/Platform Properties */
#define GA_CONN_UNAUTHENTICATED	0x80000000	/* Connection is not authenticated */	
#define GA_SOFTWARE_TCB			   0x40000000	/* Software TCB (e.g., WinNT, UNIX, ..., C2 class) */
#define GA_HARDWARE_TCB			   0x20000000	/* Hardware TCB (e.g., Trusted Workstation) includes software TCB */
#define GA_RESERVED_4		   	0x10000000

/* Authentication Method Developer */
#define GA_NOVELL_DEVELOPED	   0x08000000	/* Novell developed method includes partner developed */
#define GA_PARTNER_DEVELOPED	   0x04000000	/* Novell partner developed method */
#define GA_RESERVED_7				0x02000000	
#define GA_RESERVED_8				0x01000000	

/* Authentication Method Type */
#define GA_KNOWLEDGE				   0x00800000	/* User Knowledge */
#define GA_POSSESSION			   0x00400000	/* User Possession */
#define GA_CHARACTERISTIC		   0x00200000	/* User Characteristic */
#define GA_CLIENT_POSSESION	   0x00100000	/* Client Possession */

/* Authentication Method Characteristics */
#define GA_SNOOP_REPLAY			   0x00080000	/* Resists passive snoop & replay attacks */
#define GA_MAN_IN_MIDDLE		   0x00040000	/* Resists active man_in_middle attacks */
#define GA_VERIFIER_SPOOF		   0x00020000	/* Resists active verify spoof attacks */
#define GA_AUTH_DEV_COMPROMISE	0x00010000	/* Resists authentication device compromise attacks */

#define GA_VERIFIER_COMPROMISE	0x00008000	/* Resists verifier compromise attacks */
#define GA_SESSION_HIJACKING  	0x00004000	/* Resists session hijacking attacks */
#define GA_CHANNEL_OBSERVATION  	0x00002000	/* Resists observation of data transported over the channel */

/* Reserved */
#define GA_RESERVED_20	   		0x00001000

#define GA_RESERVED_21	   		0x00000800
#define GA_RESERVED_22	   		0x00000400
#define GA_RESERVED_23	   		0x00000200
#define GA_RESERVED_24	   		0x00000100

#define GA_RESERVED_25	   		0x00000080
#define GA_RESERVED_26	   		0x00000040
#define GA_RESERVED_27	   		0x00000020
#define GA_RESERVED_28	   		0x00000010

#define GA_RESERVED_29	   		0x00000008
#define GA_RESERVED_30	   		0x00000004
#define GA_RESERVED_31  	 		0x00000002
#define GA_RESERVED_32	   		0x00000001

/* Grade Masks */
#define GA_WS_GRADE_MASK 		   0xf0000000

#define GA_METHOD_GRADE_MASK 	   0x0fffffff
#define GA_METHOD_DEV_MASK 	   0x0f000000
#define GA_METHOD_TYPE_MASK 	   0x00f00000
#define GA_METHOD_CHAR_MASK		0x000fffff

/* Pre-defined Authentication Grades */
#define MA_GRADE_UNAUTHENTICATED (GA_CONN_UNAUTHENTICATED)

#define MA_GRADE_WEAK            (GA_KNOWLEDGE | GA_NOVELL_DEVELOPED )
#define MA_GRADE_SSL             (GA_CLIENT_POSSESION | GA_SNOOP_REPLAY | GA_NOVELL_DEVELOPED )
#define MA_GRADE_NDSV4           (GA_KNOWLEDGE | GA_CLIENT_POSSESION | GA_SNOOP_REPLAY | GA_MAN_IN_MIDDLE | GA_VERIFIER_COMPROMISE | GA_SESSION_HIJACKING | GA_NOVELL_DEVELOPED)
#define MA_GRADE_SCARD       		(GA_POSSESSION | MA_GRADE_NDSV4)
#define MA_GRADE_ENCRYPT_SSL 		(GA_CHANNEL_OBSERVATION | MA_GRADE_SSL)
#define MA_GRADE_ENCRYPT_SCARD 	(GA_CHANNEL_OBSERVATION | MA_GRADE_SMARTCARD)

#endif /* GRADES_H */
