#include "soapXMethodsQueryProxy.h"
#include "XMethodsQuery.nsmap"

int main(int argc, char** argv)
{ XMethodsQuery query;
  ArrayOfIDNamePair services;
  printf("Content-type: text/html\r\n\r\n<html><h1>Most Recent Xmethods Service Listing</h1><pre>\n");
  if (!query.ns__getAllServiceNames(services))
    services.print();
  else
  { soap_print_fault(query.soap, stderr);
    soap_print_fault_location(query.soap, stderr);
  }
  printf("</pre></html>\n");
  return 0;
}

void ArrayOfIDNamePair::print() const
{ for (int i = 0; i < __size; i++)
  { t__IDNamePair &pair = __ptr[i];
    char *id = pair.id;
    char *name = pair.name;
    printf("<a href=\"http://www.xmethods.net/ve2/ViewListing.po?key=%s\">%s</a>\n", id?id:"", name?name:"?");
  }
}

void ArrayOfServiceSummary::print() const
{ for (int i = 0; i < __size; i++)
  { t__ServiceSummary &summary = __ptr[i];
    char *id = summary.id;
    char *name = summary.name;
    char *shortDescription = summary.shortDescription;
    char *wsdlURL = summary.wsdlURL;
    printf("<a href=\"http://www.xmethods.net/ve2/ViewListing.po?key=%s\">%s</a> \"%s\" <a href=\"%s\">%s</a>\n", id?id:"", name?name:"?", shortDescription?shortDescription:"", wsdlURL?wsdlURL:"", wsdlURL?wsdlURL:"no WSDL");
  }
}
