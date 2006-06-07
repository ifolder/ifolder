#include "soapH.h"
using namespace std;
int main(int argc, char **argv)
{ soap *soap = soap_new1(SOAP_DOM_NODE); // enable deserialization of application data
  if (argc <= 1)
  { fprintf(stderr, "Usage: quote4 <ticker>\n");
    exit(1);
  }
  soap_dom_element envelope(soap, "http://schemas.xmlsoap.org/soap/envelope/", "Envelope");
  soap_dom_element body(soap, "http://schemas.xmlsoap.org/soap/envelope/", "Body");
  soap_dom_attribute encodingStyle(soap, "http://schemas.xmlsoap.org/soap/envelope/", "encodingStyle", "http://schemas.xmlsoap.org/soap/encoding/");
  soap_dom_element request(soap, "urn:xmethods-delayed-quotes", "getQuote");
  soap_dom_element symbol(soap, NULL, "symbol", argv[1]);
  soap_dom_element response(soap);
  envelope.add(body);
  body.add(encodingStyle);
  body.add(request);
  request.add(symbol);
  cout << "Request message:" << endl << envelope << endl;
  soap_mark_xsd__anyType(soap, &envelope);
  if (soap_connect(soap, "http://services.xmethods.net/soap", "")
   || soap_put_xsd__anyType(soap, &envelope, NULL, NULL)
   || soap_end_send(soap)
   || soap_begin_recv(soap)
   || !soap_get_xsd__anyType(soap, &response, NULL, NULL) // returns pointer when successful
   || soap_end_recv(soap)
   || soap_closesock(soap))
  { soap_print_fault(soap, stderr);
    soap_print_fault_location(soap, stderr);
  }
  else
  { cout << "Response message:" << endl << response << endl;
    for (soap_dom_element::iterator walker = response.find(SOAP_TYPE_xsd__float); walker != response.end(); ++walker)
      cout << "Quote = " << *(xsd__float*)(*walker).node << endl;
  }
  soap_destroy(soap);
  soap_end(soap);
  soap_done(soap);
  free(soap);
  return 0;
}
SOAP_NMAC struct Namespace namespaces[] =
{
  {"SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/", "http://www.w3.org/*/soap-envelope"},
  {"SOAP-ENC", "http://schemas.xmlsoap.org/soap/encoding/", "http://www.w3.org/*/soap-encoding"},
  {"xsi", "http://www.w3.org/2001/XMLSchema-instance", "http://www.w3.org/*/XMLSchema-instance"},
  {"xsd", "http://www.w3.org/2001/XMLSchema", "http://www.w3.org/*/XMLSchema"},
  {NULL, NULL}
};

