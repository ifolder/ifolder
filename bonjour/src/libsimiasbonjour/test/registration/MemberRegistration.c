#include <stdio.h>
#include <stdlib.h>

#include "simiasbonjour.h"

int main( int argc, char **argv )
{
	DNSServiceRef bHandle;
	DNSServiceErrorType status;

	char pid[128];
	char name[128];
	unsigned char publicKey[512];



/*
	char str1[10];
	sprintf(str1, "hi%c", (unsigned char) 222 );
	*/


	strcpy( pid, "97231a84-c0e8-46ee-a6ca-a2d29fc8589e" );
	strcpy( name, "ABA@BRADY-T41P" );
	strcpy( (char *) publicKey, "<RSAKeyValue><Modulus>yInWIlwi0SsewvMRrgbiULb7xnEDIOVPoA7bzUw8GaO3IAPC7HWCSOUgpFAeUEBtVBtSRQpuWNX7AZq66dy1yExf1qNwKB+aovm8RevxQnHfgPOrrAGyBB9NmqIvbtbPQK599IHzH8R9MkDCpr9nOrSvf2gw3yOUJQ/yt7tRILM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>" );
	printf( "Public Key Length: %d\n", strlen( publicKey ) );

	status =
		RegisterLocalMember(
			pid,
			name,
			8081,
			"/simias10/banderso",
			publicKey,
			&bHandle);
	if ( status == 0 )
	{
		getchar();
		//Sleep(30000);
		status = DeregisterLocalMember( pid, bHandle );
	}
}
