//gsoap ns service name:	Event service is a simple remote event handler
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	http://www.cs.fsu.edu/~engelen/event.wsdl
//gsoap ns service location:	http://localhost:18000

//gsoap ns schema namespace: urn:event

enum ns__event { EVENT_A, EVENT_B, EVENT_C, EVENT_Z };

//gsoap ns service method-action: handle "event"
//gsoap ns service method-documentation: handle handles asynchronous events
int ns__handle(enum ns__event event, void);
