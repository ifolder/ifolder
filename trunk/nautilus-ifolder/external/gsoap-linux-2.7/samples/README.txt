
EXAMPLES

License details: see below.

Examples are included in the 'samples' directory. The examples are:

calc.h calcclient.c calcserver.c:	 Simple calculator client and server
ck.h ckclient.cpp ckserver.cpp:		 Cookie client and server example
dime.h dimeclt.cpp dimesrv.cpp:		 Client and image server using DIME
factory.h factory.cpp			 Remote object factory and simple ORB
factorytest.h factorytest.cpp		 Test client for remote object factory
googleapi.h googleapi.c			 Google Web API client
listing.h listing.cpp:			 XMethod service listing client
localtime.h localtime.c:		 Get localtime DOC/LIT client example
lu.h lumat.cpp luclient.cpp luserver.cpp:Linear solver example client+server
magic.h magicclient.cpp magicserver.cpp: Magic Squares client and server
magic.h mtmagicserver.cpp:		 Multi-threaded Magic Squares server
mybubble.h mybubble.c:			 MyBubble client
oneliners:				 Directory with one-line services
polymorph.h polymorph.cpp:		 Polymorphic object exchange
quote.h quote.c				 Get delayed stock quote
quote2.h quote2.c			 Get delayed stock quote (SOAP messages)
quote3.h quote3.cpp			 Get delayed stock quote (Stock class)
quote4.h quote4.cpp			 Get delayed stock quote using XML DOM
quotex.h quotex.cpp:			 Combined client/server example
rss.h rss.c				 Obtain RSS feeds
ssl.h sslclient.c sslserver.c:		 SSL example (requires OpenSSL)
uddi.h uddi.cpp:			 UDDI client
varparam.h varparam.cpp:		 Variable polymorphic parameters example
webserver:				 Stand-alone Web server

To build the example services and clients, type "make" in the 'samples'
directory (Sun Solaris users should type "make -f MakefileSolaris").

DISCLAIMER

WE TRY OUR BEST TO PROVIDE YOU WITH "REAL-WORLD" EXAMPLES BUT WE CANNOT
GUARANTEE THAT ALL CLIENT EXAMPLES CAN CONNECT TO THIRD PARTY WEB SERVICES
WHEN THESE SERVICES ARE DOWN OR HAVE BEEN REMOVED.

MS WINDOWS

For Windows users, the archive includes 'magic_VC' and 'quote_VC' Visual
Studio projects in the 'samples' directory.

SSL

To try the SSL-secure SOAP server, install OpenSSL and change the occurrences
of "linprog2.cs.fsu.edu" in sslclient.c and sslserver.c to the machine name
(or machine IP) you are using. Example .pem files are included but you need to
create your own .pem files (see OpenSSL documentation).

The sslclient and sslserver codes can then be build as follows:

soapcpp2 -c ssl.h
gcc -DWITH_OPENSSL -o sslclient sslclient.c stdsoap2.c soapC.c soapClient.c -lssl -lcrypto
gcc -DWITH_OPENSSL -o sslserver sslserver.c stdsoap2.c soapC.c soapServer.c -lssl -lcrypto -lpthread

COPYRIGHT AND LICENSE

The gSOAP examples in the 'samples' directory are distributed under the GPL.
A commercial license is available from Genivia, Inc. Please contact:
contact@genivia.com

gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.

--------------------------------------------------------------------------------
This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A
PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place, Suite 330, Boston, MA 02111-1307 USA

Author contact information:
engelen@genivia.com / engelen@acm.org
--------------------------------------------------------------------------------

