//gsoap ns1 service namespace:	urn:xmethods-delayed-quotes
//gsoap ns1 service style:	rpc
//gsoap ns1 service encoding:	encoded
int ns1__getQuote(char *symbol, float &Result);

//gsoap ns2 service namespace:	urn:xmethods-CurrencyExchange
//gsoap ns2 service style:	rpc
//gsoap ns2 service encoding:	encoded
int ns2__getRate(char *country1, char *country2, float &Result);

//gsoap ns3 service name:	quotex
//gsoap ns3 service style:	rpc
//gsoap ns3 service encoding:	encoded
//gsoap ns3 service location:	http://www.cs.fsu.edu/~engelen/quotex.cgi
//gsoap ns3 service namespace:	http://www.cs.fsu.edu/~engelen/quotex.wsdl
//gsoap ns3 schema  namespace:	urn:quotex
int ns3__getQuote(char *symbol, char *country, float &result);
