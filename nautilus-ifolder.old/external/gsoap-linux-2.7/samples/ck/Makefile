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
all:		ckclient ckserver
ckclient:	ck.h ckclient.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) ck.h
		$(CPP) -DWITH_COOKIES $(CFLAGS) -o ckclient ckclient.cpp soapC.cpp soapClient.cpp $(SOAPCPP) $(LIBS)
ckserver:	ck.h ckserver.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) ck.h
		$(CPP) -DWITH_COOKIES $(CFLAGS) -o ckserver ckserver.cpp soapC.cpp soapServer.cpp $(SOAPCPP) $(LIBS)
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h ckclient ckserver
