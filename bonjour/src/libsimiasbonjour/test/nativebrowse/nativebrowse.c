#include <stdio.h>
#include <stdlib.h>
#include "simiasbonjour.h"

/*
static void DNSSD_API browse_reply(DNSServiceRef client, DNSServiceFlags flags, uint32_t ifIndex, DNSServiceErrorType errorCode,
	const char *replyName, const char *replyType, const char *replyDomain, void *context)
*/

static
void
DNSSD_API 
BrowseCallback(
    DNSServiceRef                       sdRef,
    DNSServiceFlags                     flags,
    uint32_t                            ifIndex,
    DNSServiceErrorType                 errorCode,
    const char                          *serviceName,
    const char                          *regtype,
    const char                          *replyDomain,
    void                                *context)
{
	DNSServiceErrorType status;
	MemberInfo			info;

	status = GetMemberInfo( (char *) serviceName, &info );
	if ( status == kDNSServiceErr_NoError )
	{
		printf( " Friendly Name: %s\n", info.Name );
		printf( " Service Path:  %s\n", info.ServicePath );
		printf( " Host Name:     %s\n", info.HostName );
	}
}


int main( int argc, char **argv )
{
	DNSServiceRef bHandle;
	DNSServiceErrorType status = kDNSServiceErr_NoError;

	status = BrowseMembersInit( BrowseCallback, &bHandle );
	if ( status == kDNSServiceErr_NoError )
	{
		do 
		{
			status = BrowseMembers( bHandle, 300 );
		} while ( status == kDNSServiceErr_NoError );

		BrowseMembersShutdown( bHandle );
	}
}



