/*	router.h

	Simple Web Service message router (relay server)

	Copyright (C) 2000-2002 Robert A. van Engelen. All Rights Reserved.

*/

struct t__Routing
{ char *key;		// key matches SOAPAction or query string or endpoint URL
  char *endpoint;
  char *userid;		// optional HTTP Authorization userid
  char *passwd;		// optional HTTP Authorization passwd
};

struct t__RoutingTable
{ int __size;
  struct t__Routing *__ptr;
};
