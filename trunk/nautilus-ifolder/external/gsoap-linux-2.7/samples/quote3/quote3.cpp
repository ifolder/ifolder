#include "soapQuoteProxy.h"	// get proxy
#include "Quote.nsmap"		// get namespace bindings

using namespace std;

class Stock : public Quote	// Quote is the name of the service (see quote3.h and Quote.wsdl)
{ public:
  char *symbol;			// Stock ticker
  Stock() : Quote() { symbol = NULL; };
  Stock(char *ticker) { symbol = ticker; };
  void ticker(char *ticker) { symbol = ticker; };
  float quote() { float q; ns__getQuote(symbol, q); return q; };
};

int main()
{ Quote q;
  float r;
  // Example using the Quote proxy class directly:
  if (q.ns__getQuote("IBM", r) == SOAP_OK)
    cout << r << endl;
  else
    soap_print_fault(q.soap, stderr);
  // Example using the derived Stock proxy class:
  Stock IBM("IBM");
  cout << IBM.quote() << endl;
  return 0;
}
