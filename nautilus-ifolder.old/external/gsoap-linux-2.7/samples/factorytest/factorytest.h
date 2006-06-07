/*	factorytest.h

	Client-side remote object factory definitions

	Copyright (C) 2000-2002 Robert A. van Engelen. All Rights Reserved.
*/

//gsoap ns service name:	factorytest
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
//  Remote factory objects
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

////////////////////////////////////////////////////////////////////////////////
//
//  Remote factory method interfaces
//
////////////////////////////////////////////////////////////////////////////////

int ns__create(enum t__object object, char *name, enum t__status &status);

int ns__lookup(enum t__object object, char *name, enum t__status &status);

int ns__rename(char *name, enum t__status &status);

int ns__release(enum t__status &status);

////////////////////////////////////////////////////////////////////////////////
//
//  Rewote adder method interfaces
//
////////////////////////////////////////////////////////////////////////////////

int ns__set(double val, enum t__status &status);

int ns__get(double &val);

int ns__add(double val, enum t__status &status);

////////////////////////////////////////////////////////////////////////////////
//
//  Remote counter method interfaces
//
////////////////////////////////////////////////////////////////////////////////

int ns__inc(enum t__status &status);

