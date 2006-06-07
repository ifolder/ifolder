/*	event.cpp

	C++-style client

	Events based on asynchronous one-way SOAP messaging using HTTP
	keep-alive for persistent connections. C++ style with Proxy object.

	Copyright (C) 2000-2002 Robert A. van Engelen. All Rights Reserved.

	Compile:
	soapcpp2 event.h
	c++ -o event event.cpp stdsoap2.cpp soapC.cpp soapClient.cpp

	Run (first start the event handler on localhost port 18000):
	event

*/

#include "soapEventProxy.h"
#include "Event.nsmap"

int main()
{ Event e;
  soap_set_omode(e.soap, SOAP_IO_KEEPALIVE);
  if (e.handle(EVENT_A))
    soap_print_fault(e.soap, stderr);
  if (e.handle(EVENT_B))
    soap_print_fault(e.soap, stderr);
  /* connection should not be kept alive after the last call: be nice to the server and tell it now */
  soap_clr_omode(e.soap, SOAP_IO_KEEPALIVE);
  if (e.handle(EVENT_C))
   soap_print_fault(e.soap, stderr);
  return 0;
}
