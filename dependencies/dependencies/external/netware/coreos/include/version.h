/*****************************************************************************
 *
 *	(C) Copyright 1989-1993 Novell, Inc.
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
 *  $release$
 *  $modname: version.h$
 *  $version: 1.1$
 *  $date: Thu, Jun 6, 1996$
 *  $nokeywords$
 *  
 ****************************************************************************/

/* counter part in version.inc */
#define IOEngine	0
#define FSEngine	0

// !!! Turn this flag off in the released version !!!
#define DebugSymbolsEnabledFlag	0

#define IncludeDeadCode 0
#define Instrumented 0
#define UnLoadRouter 1
#define AlphaTest 0
#define InLineAssemblyEnabled			1
#define Auditing	1
#define AssemblyKernel	1
#define DataMigrationServices	1
#define ReadAhead	1
#define FourMegabytePages		1
#define FastWorkToDoSupport		1
#define SMPSupport				1
#define TurboMemSync			0

/****************************************************************************/
/****************************************************************************/

#if (IOEngine || FSEngine)
#define ALPHA_FLAG	0
#define ALPHA_MAJOR	2
#define ALPHA_MINOR	2

#define BETA_FLAG	0
#define BETA_MAJOR	3
#define BETA_MINOR	0
#else
#define ALPHA_FLAG	0
#define ALPHA_MAJOR	2
#define ALPHA_MINOR	2

#define BETA_FLAG	0
#define BETA_MAJOR	3
#define BETA_MINOR	0
#endif

