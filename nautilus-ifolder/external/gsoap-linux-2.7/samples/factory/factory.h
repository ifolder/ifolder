/*	factory.h

	Server-side remote object factory definitions

	Server code: factory.cpp

	This header file contains all the class declarations of the remote
	objects to support serialization of these objects for server-side
	state management (simple save/load operations are implemented).

	The remote object factory uses a lease-based system. Remote objects
	are purged from the pool when the lease expires (see LEASETERM in
	factory.cpp). Supports inheritance.

	Compile:
	soapcpp2 factory.h
	c++ -o factory factory.cpp stdsoap2.cpp soapC.cpp soapServer.cpp

	Run (e.g. in the background)
	factory <port>
	where <port> is a available port number, e.g. 18085

	Copyright (C) 2000-2002 Robert A. van Engelen. All Rights Reserved.
*/

//gsoap ns service name:	factory
//gsoap ns service style:	rpc
//gsoap ns service encoding:	encoded
//gsoap ns service namespace:	http://websrv.cs.fsu.edu/~engelen/factory.wsdl
//gsoap ns service location:	http://localhost:18085

//gsoap ns schema namespace: urn:factoryService
//gsoap t schema namespace: urn:factoryTypes
//gsoap h schema namespace: urn:factoryHandles

////////////////////////////////////////////////////////////////////////////////
//
//  SOAP Header: used to exchange stateful object handles
//
////////////////////////////////////////////////////////////////////////////////

struct SOAP_ENV__Header
{ mustUnderstand unsigned int h__handle;
};

////////////////////////////////////////////////////////////////////////////////
//
//  Server-side root class
//
////////////////////////////////////////////////////////////////////////////////

enum t__object				// object types:
{ ROOT,					// t__root object
  ADDER,				// t__adder object
  COUNTER				// t__counter object
};

enum t__status				// remote object status:
{ FACTORY_OK,					// ok
  FACTORY_INVALID,				// invalid handle (wrong type of object or lease expired)
  FACTORY_NOTFOUND,				// lookup operation not successful
  FACTORY_RETRY					// cannot create new object: try later
};

class t__root
{ public:
  enum t__object object;		// object type
  char *name;				// object name for lookup operation (optional)
  unsigned int handle;			// internal handle
  time_t lease;				// lease expiration time
  t__root();
  virtual ~t__root();
  virtual void renew();			// to renew lease
};

////////////////////////////////////////////////////////////////////////////////
//
//  Server-side adder class derived from root
//
////////////////////////////////////////////////////////////////////////////////

class t__adder: public t__root
{ public:
  double val;				// current value of the adder
  void set(double val);			// to set the adder
  double get();				// to get value of the adder
  void add(double val);			// to add a value to the adder
};

////////////////////////////////////////////////////////////////////////////////
//
//  Server-side counter class derived from adder
//
////////////////////////////////////////////////////////////////////////////////

class t__counter: public t__adder
{ public:
  void inc();				// to increment the counter
};

////////////////////////////////////////////////////////////////////////////////
//
//  Remote factory method interfaces
//
////////////////////////////////////////////////////////////////////////////////

//gsoap ns service method-header-part: create h__handle
int ns__create(enum t__object object, char *name, enum t__status &status);

//gsoap ns service method-header-part: lookup h__handle
int ns__lookup(enum t__object object, char *name, enum t__status &status);

//gsoap ns service method-header-part: rename h__handle
int ns__rename(char *name, enum t__status &status);

//gsoap ns service method-header-part: release h__handle
int ns__release(enum t__status &status);

////////////////////////////////////////////////////////////////////////////////
//
//  Rewote adder method interfaces
//
////////////////////////////////////////////////////////////////////////////////

//gsoap ns service method-header-part: set h__handle
int ns__set(double val, enum t__status &status);

//gsoap ns service method-header-part: get h__handle
int ns__get(double &val);

//gsoap ns service method-header-part: add h__handle
int ns__add(double val, enum t__status &status);

////////////////////////////////////////////////////////////////////////////////
//
//  Remote counter method interfaces
//
////////////////////////////////////////////////////////////////////////////////

//gsoap ns service method-header-part: inc h__handle
int ns__inc(enum t__status &status);

