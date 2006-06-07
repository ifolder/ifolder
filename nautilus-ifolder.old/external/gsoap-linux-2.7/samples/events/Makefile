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
all:		event handler
event:		event.h event.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c event.h
		$(CC) $(CFLAGS) -o event event.c soapC.c soapClient.c $(SOAPC) $(LIBS)
handler:	event.h handler.c $(SOAPH) $(SOAPC)
		$(GSOAP) event.h
		$(CC) $(CFLAGS) -o handler handler.c soapC.c soapClient.c soapServer.c $(SOAPC) -lpthread $(LIBS)
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h event handler
