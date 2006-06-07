
#include "quoteServiceProxy.h"
#include "rateServiceProxy.h"
#include "calcServiceObject.h"

#include "quote.nsmap"
#include "rate.nsmap"
#include "calc.nsmap"

using namespace std;

int main(int argc, char *argv[])
{ if (argc <= 1)
  { calc::Service calc;
    return calc.serve();
  }
  quote::Service quote;
  float q;
  if (quote.ns__getQuote(argv[1], q))
    soap_print_fault(quote.soap, stderr);
  else
  { if (argc > 2)
    { rate::Service rate;
      float r;
      if (rate.ns__getRate("us", argv[2], r))
        soap_print_fault(rate.soap, stderr);
      else
        q *= r;
    }
    cout << argv[1] << ": " << q << endl;
  }
  return 0;
}

namespace calc {

int ns__add(struct soap *soap, double a, double b, double *result)
{ *result = a + b;
  return SOAP_OK;
}

int ns__sub(struct soap *soap, double a, double b, double *result)
{ *result = a - b;
  return SOAP_OK;
}

int ns__mul(struct soap *soap, double a, double b, double *result)
{ *result = a * b;
  return SOAP_OK;
}

int ns__div(struct soap *soap, double a, double b, double *result)
{ *result = a / b;
  return SOAP_OK;
}

}
