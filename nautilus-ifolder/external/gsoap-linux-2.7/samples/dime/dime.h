//gsoap ns service name:	dime
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	http://websrv.cs.fsu.edu/~engelen/dime.wsdl
//gsoap ns service location:	http://localhost:8085

//gsoap ns schema  namespace:	urn:dime
class ns__Data
{ unsigned char *__ptr; /* points to data */
  int __size;		/* size of data */
  char *id;		/* dime attachment ID (set to NULL to obtain unique cid) */
  char *type;		/* dime attachment content type */
  char *options;	/* dime attachment options (optional) */
  ns__Data();
  struct soap *soap;	/* soap context that created this instance */
};
class arrayOfData	/* SOAP array of data */
{ ns__Data *__ptr;
  int __size;
  arrayOfData();
  arrayOfData(struct soap*, int);
  virtual ~arrayOfData();
  int size();
  void resize(int);
  ns__Data& operator[](int) const;
  struct soap *soap;
};
class arrayOfName	/* SOAP array of strings */
{ char **__ptr;
  int __size;
  arrayOfName();
  arrayOfName(struct soap*, int);
  virtual ~arrayOfName();
  int size();
  void resize(int);
  char*& operator[](int) const;
  struct soap *soap;
};
int ns__putData(arrayOfData *data, arrayOfName *names);
int ns__getData(arrayOfName *names, arrayOfData *data);
int ns__getImage(char *name, ns__Data &image);
