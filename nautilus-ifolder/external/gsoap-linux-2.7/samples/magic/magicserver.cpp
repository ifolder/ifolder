#include "soapH.h"
#include "magic.nsmap"

////////////////////////////////////////////////////////////////////////////////
//
//	Magic Squares Server
//
////////////////////////////////////////////////////////////////////////////////

// Install as a CGI application.
// Alternatively, run from command line with arguments IP (which must be the
// IP of the current machine you are using) and PORT to run this as a
// stand-alone server on a port. For example:
// > magicserver.cgi 18081 &
// To let 'magic' talk to this service, change the URL in magic.cpp into
// "http://localhost:18081"

int main(int argc, char **argv)
{ struct soap soap;
  int m, s;
  soap_init(&soap);
  // soap.accept_timeout = 60; // die if no requests are made within 1 minute
  if (argc < 2)
  { soap_serve(&soap);
    soap_destroy(&soap);
    soap_end(&soap);	// clean up 
  }
  else
  { m = soap_bind(&soap, NULL, atoi(argv[1]), 100);
    if (m < 0)
    { soap_print_fault(&soap, stderr);
      exit(1);
    }
    fprintf(stderr, "Socket connection successful %d\n", m);
    for (int i = 1; ; i++)
    { s = soap_accept(&soap);
      if (s < 0)
      { if (soap.errnum)
          soap_print_fault(&soap, stderr);
	else
	  fprintf(stderr, "Server timed out\n");
	break;
      }
      fprintf(stderr, "%d: accepted %d IP=%d.%d.%d.%d ... ", i, s, (int)(soap.ip>>24)&0xFF, (int)(soap.ip>>16)&0xFF, (int)(soap.ip>>8)&0xFF, (int)soap.ip&0xFF);
      soap_serve(&soap);	// process RPC skeletons
      fprintf(stderr, "served\n");
      soap_destroy(&soap);
      soap_end(&soap);	// clean up 
    }
  }
  soap_done(&soap);
  return 0;
}

////////////////////////////////////////////////////////////////////////////////
//
//	Magic Square Algorithm
//
////////////////////////////////////////////////////////////////////////////////

int ns1__magic(struct soap *soap, int n, matrix *square)
{ int i, j, k, l, key = 2;
  if (n < 1)
    return soap_sender_fault(soap, "Negative or zero size", "<error xmlns=\"http://tempuri.org/\">The input parameter must be positive</error>");
  if (n > 100)
    return soap_sender_fault(soap, "size > 100", "<error xmlns=\"http://tempuri.org/\">The input parameter must not be too large</error>");
  square->resize(n, n);
  for (i = 0; i < n; i++)
    for (j = 0; j < n; j++)
      (*square)[i][j] = 0;
  i = 0;
  j = (n-1)/2;
  (*square)[i][j] = 1;
  while (key <= n*n)
  { if (i-1 < 0)
      k = n-1;
    else
      k = i-1;
    if (j-1 < 0)
      l = n-1;
    else
      l = j-1;
    if ((*square)[k][l])
      i = (i+1) % n;
    else
    { i = k;
      j = l;
    }
    (*square)[i][j] = key;
    key++;
  }
  return SOAP_OK;
}

////////////////////////////////////////////////////////////////////////////////
//
//	Class vector Methods
//
////////////////////////////////////////////////////////////////////////////////

vector::vector()
{ __ptr = 0;
  __size = 0;
}

vector::vector(int n)
{ __ptr = (int*)soap_malloc(soap, n*sizeof(int));
  __size = n;
}

vector::~vector()
{ soap_unlink(soap, this); // not required, but just to make sure if someone calls delete on this
}

void vector::resize(int n)
{ int *p;
  if (__size == n)
    return;
  p = (int*)soap_malloc(soap, n*sizeof(int));
  if (__ptr)
  { for (int i = 0; i < (n <= __size ? n : __size); i++)
      p[i] = __ptr[i];
    soap_unlink(soap, __ptr);
    free(__ptr);
  }
  __size = n;
  __ptr = p;
}

int& vector::operator[](int i) const
{ if (!__ptr || i < 0 || i >= __size)
    fprintf(stderr, "Array index out of bounds\n");
  return (__ptr)[i];
}

////////////////////////////////////////////////////////////////////////////////
//
//	Class matrix Methods
//
////////////////////////////////////////////////////////////////////////////////

matrix::matrix()
{ __ptr = 0;
  __size = 0;
}

matrix::matrix(int rows, int cols)
{ __ptr = soap_new_vector(soap, rows);
  for (int i = 0; i < cols; i++)
    __ptr[i].resize(cols);
  __size = rows;
}

matrix::~matrix()
{ soap_unlink(soap, this); // not required, but just to make sure if someone calls delete on this
}

void matrix::resize(int rows, int cols)
{ int i;
  vector *p;
  if (__size != rows)
  { if (__ptr)
    { p = soap_new_vector(soap, rows);
      for (i = 0; i < (rows <= __size ? rows : __size); i++)
      { if (this[i].__size != cols)
          (*this)[i].resize(cols);
	(p+i)->__ptr = __ptr[i].__ptr;
	(p+i)->__size = cols;
      }
      for (; i < rows; i++)
        __ptr[i].resize(cols);
    }
    else
    { __ptr = soap_new_vector(soap, rows);
      for (i = 0; i < rows; i++)
        __ptr[i].resize(cols);
      __size = rows;
    }
  }
  else
    for (i = 0; i < __size; i++)
      __ptr[i].resize(cols);
}

vector& matrix::operator[](int i) const
{ if (!__ptr || i < 0 || i >= __size)
    fprintf(stderr, "Array index out of bounds\n");
  return __ptr[i];
}
