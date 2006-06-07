//gsoap ns service name:	XMethodsQuery
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	http://www.xmethods.net/interfaces/query.wsdl
//gsoap ns service location:	http://www.xmethods.net/interfaces/query

//gsoap ns schema namespace: http://www.xmethods.net/interfaces/query
//gsoap t schema namespace: http://www.xmethods.net/interfaces/query.xsd

class t__ServiceSummary
{ public:
  char *name;
  char *id;
  char *shortDescription;
  char *wsdlURL;
  char *publisherID;
};

class t__ServiceDetail
{ public:
  char *name;
  char *id;
  char *shortDescription;
  char *description;
  char *implementationID;
  char *email;
  char *wsdlURL;
  char *infoURL;
  char *discussionURL;
  char *notes;
  char *tmodelID;
  char *publisherID;
  char *uuid;
};

class t__IDNamePair
{ public:
  char *id;
  char *name;
};

class ArrayOfServiceSummary
{ public:
  t__ServiceSummary *__ptr;
  int __size;
  void print() const;
};

class ArrayOfIDNamePair
{ public:
  t__IDNamePair *__ptr;
  int __size;
  void print() const;
};

//gsoap ns service method-action: getAllServiceSummaries ""
ns__getAllServiceSummaries(ArrayOfServiceSummary&);

//gsoap ns service method-action: getServiceSummariesByPublisher ""
ns__getServiceSummariesByPublisher(char *publisherID, ArrayOfServiceSummary&);

//gsoap ns service method-action: getServiceDetail ""
ns__getServiceDetail(char *id, t__ServiceDetail&);

//gsoap ns service method-action: getAllServiceNames ""
ns__getAllServiceNames(ArrayOfIDNamePair&);

//gsoap ns service method-action: getServiceNamesByPublisher ""
ns__getServiceNamesByPublisher(char *publisherID, ArrayOfIDNamePair&);

