
The 'extras' directory contains contributions and other useful additions.

All contributions are covered by the gSOAP public license, unless specifically
stated in the source. The following authors provided the contributions
included in this directory:

fault.cpp contributed by A. Kelly
logging.cpp contributed by M. Helmick
WinInet support contributed by J. Kastanowitz and B. Thiesfield
mod_gsoap contributed by Christian Aberger and others

ckdb.h ckdb.c			Simple Cookie database manager (store/load)
ckdbtest.h ckdbtest.c		Test code for Simple Cookie database manager
fault.cpp			Print SOAP Fault messages to C++ streams
GSoapWinInet.h GSoapWinInet.cpp	WinInet support
httpmd5.h httpmd5.c		HTTP Content-MD5 support
httpmd5test.h httpmd5test.c	Example HTTP Content-MD5 test application
logging.cpp			Log send, receive, and trace messages on streams
plugin.h plugin.c		Example gSOAP plugin to view SOAP messages
mod_gsoap			Directory with mod_gsoap modules for Apache

Please refer to the Christian's Web site for installation intructions for the
Apache and IIS modules for gSOAP (mod_gsoap directory):

http://mx.aberger.at/SOAP/

The soapdefs.h file contains supporting declarations (see notes in the sources).

Note: string.cpp in older distributions is obsolete and replaced with stl.h
and stl.cpp in the top directory.
