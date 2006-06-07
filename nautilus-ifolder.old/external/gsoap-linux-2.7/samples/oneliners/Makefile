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
all:		gmtclient gmtserver.cgi helloclient helloserver.cgi rollclient rollserver.cgi
gmtclient:	gmt.h gmtclient.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) gmt.h
		$(CPP) $(CFLAGS) -o gmtclient gmtclient.cpp soapC.cpp soapClient.cpp $(SOAPCPP) $(LIBS)
gmtserver.cgi:	gmt.h gmtserver.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) gmt.h
		$(CPP) $(CFLAGS) -o gmtserver.cgi gmtserver.cpp soapC.cpp soapServer.cpp $(SOAPCPP) $(LIBS)
helloclient:	hello.h helloclient.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) hello.h
		$(CPP) $(CFLAGS) -o helloclient helloclient.cpp soapC.cpp soapClient.cpp $(SOAPCPP) $(LIBS)
helloserver.cgi:hello.h helloserver.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) hello.h
		$(CPP) $(CFLAGS) -o helloserver.cgi helloserver.cpp soapC.cpp soapServer.cpp $(SOAPCPP) $(LIBS)
rollclient:	roll.h rollclient.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) roll.h
		$(CPP) $(CFLAGS) -o rollclient rollclient.cpp soapC.cpp soapClient.cpp $(SOAPCPP) $(LIBS)
rollserver.cgi:	roll.h rollserver.cpp $(SOAPH) $(SOAPCPP)
		$(GSOAP) roll.h
		$(CPP) $(CFLAGS) -o rollserver.cgi rollserver.cpp soapC.cpp soapServer.cpp $(SOAPCPP) $(LIBS)
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h
distclean:
		rm -f *.o *.wsdl *.xsd *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapC.c soapClient.cpp soapClient.c soapServer.cpp soapServer.c soap*Proxy.h gmtclient gmtserver.cgi helloclient helloserver.cgi rollclient rollserver.cgi
