namespace quote {

//gsoap ns service name:	Service
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service location:	http://services.xmethods.net/soap
//gsoap ns service namespace:	urn:xmethods-delayed-quotes

//gsoap ns service method-action: getQuote ""
int ns__getQuote(char *symbol, float &Result);

}
