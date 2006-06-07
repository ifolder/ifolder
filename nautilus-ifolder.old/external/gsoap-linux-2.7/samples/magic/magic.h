//gsoap ns1 service name:	magic
//gsoap ns1 service style:	rpc
//gsoap ns1 service encoding:	encoded
//gsoap ns1 service namespace:	http://websrv.cs.fsu.edu/~engelen/magic.wsdl
//gsoap ns1 service location:	http://websrv.cs.fsu.edu/~engelen/magicserver.cgi
//gsoap ns1 service documentation: Demo Magic Squares service

//gsoap ns1 schema namespace: urn:MagicSquare

typedef int xsd__int;

class vector
{ public:
  xsd__int *__ptr;
  int __size;
  struct soap *soap;
  vector();
  vector(int);
  virtual ~vector();
  void resize(int);
  int& operator[](int) const;
};

class matrix
{ public:
  vector *__ptr;
  int __size;
  struct soap *soap;
  matrix();
  matrix(int, int);
  virtual ~matrix();
  void resize(int, int);
  vector& operator[](int) const;
};

int ns1__magic(xsd__int rank, matrix *result);
