/******************************************************************************

  %name: nwapidef.h %
  %version: 5 %
  %date_modified: Thu Jan 16 15:50:00 1997 %
  $Copyright:

  Copyright (c) 1989-1995 Novell, Inc.  All Rights Reserved.                      

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/

#if ! defined ( NWAPIDEF_H )
#define NWAPIDEF_H

/* Miscellaneous string lengths (constant) */
/* NOTE: These max values include a byte for null  */
#define NW_MAX_USER_NAME_LEN              49
#define NW_MAX_VOLUME_NAME_LEN            17
#define NW_MAX_SERVER_NAME_LEN            49
#define NW_MAX_TREE_NAME_LEN              33
#define NW_MAX_SERVICE_TYPE_LEN           49

/* Miscellaneous unicode string sizes in bytes (constant) */

#define NW_MAX_USER_NAME_BYTES              2 * NW_MAX_USER_NAME_LEN
#define NW_MAX_VOLUME_NAME_BYTES            2 * NW_MAX_VOLUME_NAME_LEN
#define NW_MAX_SERVER_NAME_BYTES            2 * NW_MAX_SERVER_NAME_LEN
#define NW_MAX_TREE_NAME_BYTES              2 * NW_MAX_TREE_NAME_LEN
#define NW_MAX_SERVICE_TYPE_BYTES           2 * NW_MAX_SERVICE_TYPE_LEN

/* PrintFlags (nuint16 value) */
#define NW_PRINT_FLAG_RELEASE             0x0001
#define NW_PRINT_FLAG_SUPPRESS_FF         0x0002
#define NW_PRINT_FLAG_TEXT_FILE           0x0004
#define NW_PRINT_FLAG_PRINT_BANNER        0x0008
#define NW_PRINT_FLAG_NOTIFY              0x0010

/* Print string lengths (constant) */
#define NW_MAX_JOBDESCR_LEN               50
#define NW_MAX_FORM_NAME_LEN              13
#define NW_MAX_BANNER_NAME_LEN            13
#define NW_MAX_QUEUE_NAME_LEN             65

/* Client Types : these are returned by NWGetClientType */
#define NW_NETX_SHELL   1
#define NW_VLM_REQ      2
#define NW_CLIENT32     3
#define NW_NT_REQ       4
#define NW_OS2_REQ      5
#define NW_NLM_REQ      6

#endif  /* NWAPIDEF_INC */
