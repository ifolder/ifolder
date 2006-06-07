//gsoap ns service name:	Quote
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	urn:xmethods-delayed-quotes
//gsoap ns service location:	http://services.xmethods.net/soap

//gsoap ns service method-action: getQuote ""
int ns__getQuote(char *symbol, float &Result);
