GSOAP=../../soapcpp2
SOAPH=../../stdsoap2.h
SOAPC=../../stdsoap2.c
SOAPCPP=../../stdsoap2.cpp
CC=gcc
CPP=g++
LIBS=
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I../..
CMFLAGS=
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
all:		main
main:		main.cpp envC.o quoteClientLib.o rateClientLib.o calcServerLib.o stdsoap2.o $(SOAPH)
		$(CPP) $(CFLAGS) -o main main.cpp envC.o quoteClientLib.o rateClientLib.o calcServerLib.o stdsoap2.o $(LIBS)
stdsoap2.o:	$(SOAPCPP) $(SOAPH)
		$(CPP) -DWITH_NONAMESPACES $(CFLAGS) -c $(SOAPCPP)
envC.o:         env.h header.h fault.h
		$(GSOAP) -penv env.h
		$(CPP) $(CFLAGS) -c envC.cpp
quoteClientLib.cpp:     quote.h
		$(GSOAP) -n quote.h
quoteClientLib.o:       quoteClientLib.cpp
		$(CPP) $(CFLAGS) -c quoteClientLib.cpp
rateClientLib.cpp:      rate.h
		$(GSOAP) -n rate.h
rateClientLib.o:        rateClientLib.cpp
		$(CPP) $(CFLAGS) -c rateClientLib.cpp
calcServerLib.cpp:      calc.h
		$(GSOAP) -n calc.h
calcServerLib.o:        calcServerLib.cpp
		$(CPP) $(CFLAGS) -c calcServerLib.cpp
clean:
		rm -f *.o *H.h *Stub.h *C.cpp *Client.cpp *Server.cpp *ClientLib.cpp *ServerLib.cpp *Proxy.h *Object.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log *H.h *Stub.h *C.cpp *Client.cpp *Server.cpp *ClientLib.cpp *ServerLib.cpp *Proxy.h *Object.h
