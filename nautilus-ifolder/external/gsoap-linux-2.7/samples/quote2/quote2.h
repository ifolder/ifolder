/*	getQuote example with asynchronous SOAP messaging
*/
//gsoap ns service name:	quote
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	urn:xmethods-delayed-quotes
//gsoap ns service location:	http://services.xmethods.net/soap
ns__getQuote(char *symbol, void dummy);
ns__getQuoteResponse(float Result, void dummy);
