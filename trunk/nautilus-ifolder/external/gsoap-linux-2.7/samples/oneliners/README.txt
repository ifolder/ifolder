This directory contains a number of "one-liners": gSOAP Web Services and client
applications that are only one line long (not counting the usual #includes).

The programs are small, but they can do a number of useful things:

gmt.h, gmtclient.cpp, gmtserver.cpp:		return current time in GMT
hello.h, helloclient.cpp, helloserver.cpp:	Hello World!
roll.h, rollclient.cpp, rollserver.cpp:		roll of a die

You can use the client programs right away after compilation, because they
connect to our server. To run the server examples you have to install them
as CGI applications.
