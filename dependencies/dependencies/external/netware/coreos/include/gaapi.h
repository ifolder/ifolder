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

#ifndef __GAAPI_H
#define __GAAPI_H

#ifdef __cplusplus
# define EXTERNC extern "C"
#else
# define EXTERNC
#endif

#if defined(WIN32)
# define GAMSEXPORT __declspec(dllexport)
# define GAMSIMPORT __declspec(dllimport)
#else
# define GAMSEXPORT
# define GAMSIMPORT
#endif

#ifdef __GAMS_SOURCE__
# define GAMS__PORT GAMSEXPORT
#else
# define GAMS__PORT GAMSIMPORT
#endif

#define GAMSAPI EXTERNC GAMS__PORT

/****************************************************************************/
/****************************************************************************/
#endif
