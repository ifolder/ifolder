GSOAP=../../soapcpp2
SOAPH=../../stdsoap2.h
SOAPC=../../stdsoap2.c
SOAPCPP=../../stdsoap2.cpp
CC=gcc
CPP=g++
LIBS=-lpthread -lz -lssl -lcrypto
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I../..
CMFLAGS=-DWITH_COOKIES -DWITH_GZIP -DWITH_OPENSSL
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
all:		webserver
webserver:	webserver.h webserver.c httpget.o options.o logging.o $(SOAPH) $(SOAPC)
		$(GSOAP) -c webserver.h
		$(CC) $(CFLAGS) -o webserver webserver.c httpget.o options.o logging.o soapC.c soapClient.c soapServer.c $(SOAPC) $(LIBS)
options.o:	opt.h options.h options.c
		$(GSOAP) -cnpopt opt.h
		$(CC) $(CFLAGS) -c options.c
logging.o:	logging.h logging.c
		$(CC) $(CFLAGS) -c logging.c
httpget.o:	httpget.h httpget.c
		$(CC) $(CFLAGS) -c httpget.c
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soapClientLib.c soapClientLib.cpp soapServerLib.c soapServerLib.cpp soap*Proxy.h webserver
