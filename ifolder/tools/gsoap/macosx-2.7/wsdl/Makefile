GSOAP=../soapcpp2
SOAPH=../stdsoap2.h
SOAPC=../stdsoap2.c
SOAPCPP=../stdsoap2.cpp
CC=gcc
CPP=g++
LIBS=-lm
COFLAGS=-O1
# COFLAGS=-DDEBUG -g
CWFLAGS=-Wall
CIFLAGS=-I..
CMFLAGS=
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
wsdl2h:		wsdlC.o wsdl.o schema.o soap.o types.o service.o wsdl2h.cpp $(SOAPCPP)
		$(CPP) $(CFLAGS) -o wsdl2h wsdl2h.cpp wsdlC.o wsdl.o schema.o soap.o types.o service.o $(SOAPCPP) $(LIBS)
		cp -f wsdl2h ..
wsdlC.o:	wsdlC.cpp
		$(CPP) -c $(CFLAGS) wsdlC.cpp
wsdlC.cpp:	schema.h soap.h mime.h dime.h http.h wsdl.h includes.h imports.h
		$(GSOAP) -ipwsdl wsdl.h
types.o:	types.h types.cpp
		$(CPP) -c $(CFLAGS) types.cpp
service.o:	types.h service.h service.cpp
		$(CPP) -c $(CFLAGS) service.cpp
wsdl.o:		wsdl.h wsdl.cpp includes.h imports.h
		$(CPP) -c $(CFLAGS) wsdl.cpp
schema.o:	schema.h schema.cpp includes.h imports.h
		$(CPP) -c $(CFLAGS) schema.cpp
soap.o:		soap.h soap.cpp includes.h imports.h
		$(CPP) -c $(CFLAGS) soap.cpp
secure:
		touch wsdl2h.cpp
		make wsdl2h_secure
wsdl2h_secure:	wsdlC.o wsdl.o schema.o types.o service.o wsdl2h.cpp $(SOAPCPP)
		$(CPP) $(CFLAGS) -o wsdl2h wsdl2h.cpp wsdlC.o wsdl.o schema.o types.o service.o $(SOAPCPP) -lcrypto -lssl
		cp -f wsdl2h ..
clean:		
		rm -f *.o wsdlH.h wsdlStub.h wsdlC.cpp wsdlClient.cpp wsdlServer.cpp wsdlClientLib.cpp wsdlServerLib.cpp
distclean:		
		rm -f *.o *.wsdl. *.xsd *.xml *.nsmap wsdl2h wsdlH.h wsdlStub.h wsdlC.cpp wsdlClient.cpp wsdlServer.cpp wsdlClientLib.cpp wsdlServerLib.cpp
