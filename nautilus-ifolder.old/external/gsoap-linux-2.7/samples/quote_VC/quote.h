//gsoap ns service name:	quote
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	urn:xmethods-delayed-quotes
//gsoap ns service location:	http://services.xmethods.net/soap

int ns__getQuote(char *symbol, float *Result);
