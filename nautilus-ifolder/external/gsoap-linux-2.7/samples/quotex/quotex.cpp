/*	quotex.cpp
	This example is both a SOAP service and a client application.
	As a CGI program, it will serve currency-converted stock quote requests.
	As a client, it will return the currency-converted stock quote given as
	arguments to the program on the command-line. For example
	> quotex AOL uk
*/

#include "soapH.h"	// include generated proxy and SOAP support
#include "quotex.nsmap"	// include generated namespace map file

const char endpoint[] = "http://websrv.cs.fsu.edu/~engelen/quotex.cgi";

int main(int argc, char **argv)
{ struct soap *soap = soap_new();
  float q;
  if (argc <= 2)
  { soap->user = soap_new(); // pass a new gSOAP environment which we need to make server-side client calls
    soap_serve(soap);	// serve request
    soap_destroy((struct soap*)soap->user);
    soap_end((struct soap*)soap->user);
    soap_done((struct soap*)soap->user);
    free(soap->user);
    soap->user = NULL;
  }
  else if (soap_call_ns3__getQuote(soap, endpoint, NULL, argv[1], argv[2], q) == 0)
    printf("\nCompany %s: %f (%s)\n", argv[1], q, argv[2]);
  else
  { soap_print_fault(soap, stderr);
    soap_print_fault_location(soap, stderr);
  }
  soap_destroy(soap);
  soap_end(soap);
  soap_done(soap);
  free(soap);
  return 0;
}

int ns3__getQuote(struct soap *soap, char *symbol, char *country, float &result)
{ float q, r;
  // soap->user contains an environment that we can use to make calls that do not interfere with the current service environment
  if (soap_call_ns1__getQuote((struct soap*)soap->user, "http://services.xmethods.net/soap", NULL, symbol, q) == 0 &&
      soap_call_ns2__getRate((struct soap*)soap->user, "http://services.xmethods.net/soap", NULL, "us", country, r) == 0)
  { result = q*r;
    return SOAP_OK;
  }
  soap_receiver_fault(soap, *soap_faultstring((struct soap*)soap->user), NULL);
  return SOAP_FAULT;	// pass soap fault messages on to the client of this app
}

/*	Since this app is a combined client-server, it is easy to put it together with
 * 	one header file that describes all remote methods. However, as a consequence we
 *	have to implement the methods that are not ours. Since these implementations are
 * 	never called, we can make them dummies.
 */

int ns1__getQuote(struct soap *soap, char *symbol, float &Result)
{ return SOAP_NO_METHOD; } // dummy: will never be called
int ns2__getRate(struct soap *soap, char *country1, char *country2, float &Result)
{ return SOAP_NO_METHOD; } // dummy: will never be called

