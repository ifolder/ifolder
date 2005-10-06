/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;

namespace Simias.mDns
{
	/// <summary>
	/// Summary description for Dnsd.
	/// </summary>
	public class Types
	{
		// possible error code values 
		public enum kErrorType : int
		{
			kDNSServiceErr_NoError             = 0,
			kDNSServiceErr_Unknown             = -65537,       /* 0xFFFE FFFF */
			kDNSServiceErr_NoSuchName          = -65538,
			kDNSServiceErr_NoMemory            = -65539,
			kDNSServiceErr_BadParam            = -65540,
			kDNSServiceErr_BadReference        = -65541,
			kDNSServiceErr_BadState            = -65542,
			kDNSServiceErr_BadFlags            = -65543,
			kDNSServiceErr_Unsupported         = -65544,
			kDNSServiceErr_NotInitialized      = -65545,
			kDNSServiceErr_AlreadyRegistered   = -65547,
			kDNSServiceErr_NameConflict        = -65548,
			kDNSServiceErr_Invalid             = -65549,
			kDNSServiceErr_Firewall            = -65550,
			kDNSServiceErr_Incompatible        = -65551,        /* client library incompatible with daemon */
			kDNSServiceErr_BadInterfaceIndex   = -65552,
			kDNSServiceErr_Refused             = -65553,
			kDNSServiceErr_NoSuchRecord        = -65554,
			kDNSServiceErr_NoAuth              = -65555,
			kDNSServiceErr_NoSuchKey           = -65556,
			kDNSServiceErr_NATTraversal        = -65557,
			kDNSServiceErr_DoubleNAT           = -65558,
			kDNSServiceErr_BadTime             = -65559
			/* mDNS Error codes are in the range
				 * FFFE FF00 (-65792) to FFFE FFFF (-65537) */
		};

		/* General flags used in functions defined below */
		public enum kDNSServiceFlags : int
		{
			kDNSServiceFlagsMoreComing          = 0x1,
			/* MoreComing indicates to a callback that at least one more result is
			 * queued and will be delivered following immediately after this one.
			 * Applications should not update their UI to display browse
			 * results when the MoreComing flag is set, because this would
			 * result in a great deal of ugly flickering on the screen.
			 * Applications should instead wait until until MoreComing is not set,
			 * and then update their UI.
			 * When MoreComing is not set, that doesn't mean there will be no more
			 * answers EVER, just that there are no more answers immediately
			 * available right now at this instant. If more answers become available
			 * in the future they will be delivered as usual.
			 */

			kDNSServiceFlagsAdd                 = 0x2,
			kDNSServiceFlagsDefault             = 0x4,
			/* Flags for domain enumeration and browse/query reply callbacks.
			 * "Default" applies only to enumeration and is only valid in
			 * conjuction with "Add".  An enumeration callback with the "Add"
			 * flag NOT set indicates a "Remove", i.e. the domain is no longer
			 * valid.
			 */

			kDNSServiceFlagsNoAutoRename        = 0x8,
			/* Flag for specifying renaming behavior on name conflict when registering
			 * non-shared records. By default, name conflicts are automatically handled
			 * by renaming the service.  NoAutoRename overrides this behavior - with this
			 * flag set, name conflicts will result in a callback.  The NoAutorename flag
			 * is only valid if a name is explicitly specified when registering a service
			 * (i.e. the default name is not used.)
			 */

			kDNSServiceFlagsShared              = 0x10,
			kDNSServiceFlagsUnique              = 0x20,
			/* Flag for registering individual records on a connected
			 * DNSServiceRef.  Shared indicates that there may be multiple records
			 * with this name on the network (e.g. PTR records).  Unique indicates that the
			 * record's name is to be unique on the network (e.g. SRV records).
			 */

			kDNSServiceFlagsBrowseDomains       = 0x40,
			kDNSServiceFlagsRegistrationDomains = 0x80,
			/* Flags for specifying domain enumeration type in DNSServiceEnumerateDomains.
			 * BrowseDomains enumerates domains recommended for browsing, RegistrationDomains
			 * enumerates domains recommended for registration.
			 */

			kDNSServiceFlagsLongLivedQuery      = 0x100,
			/* Flag for creating a long-lived unicast query for the DNSServiceQueryRecord call. */

			kDNSServiceFlagsAllowRemoteQuery    = 0x200,
			/* Flag for creating a record for which we will answer remote queries
			 * (queries from hosts more than one hop away; hosts not directly connected to the local link).
			 */

			kDNSServiceFlagsForceMulticast      = 0x400
			/* Flag for signifying that a query or registration should be performed exclusively via multicast DNS,
			 * even for a name in a domain (e.g. foo.apple.com.) that would normally imply unicast DNS.
			 */
		};

		public Types()
		{
		}
	}
}
