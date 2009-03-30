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
all:		sslclient sslserver
sslclient:	ssl.h sslclient.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c ssl.h
		$(CC) -DWITH_OPENSSL $(CFLAGS) -o sslclient sslclient.c soapC.c soapClient.c $(SOAPC) -lssl -lcrypto $(LIBS)
sslserver:	ssl.h sslserver.c $(SOAPH) $(SOAPC)
		$(GSOAP) -c ssl.h
		$(CC) -DWITH_OPENSSL $(CFLAGS) -o sslserver sslserver.c soapC.c soapServer.c $(SOAPC) -lssl -lcrypto -lpthread $(LIBS)
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h sslclient sslserver
