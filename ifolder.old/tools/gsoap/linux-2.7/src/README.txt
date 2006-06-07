
The gSOAP 2 compiler sources are Copyright (C) 2000-2003 Robert A. van Engelen,
Genivia inc. All rights reserved.

THIS PART OF THE PACKAGE IS INTENDED TO SUPPORT THE MIGRATION OF gSOAP TO
DIFFERENT PLATFORMS. The code has not been cleaned. No documentation is
enclosed.  Because Web service technology and protocols such as SOAP and WSDL
are changing rapidly, periodic updates will be provided. As a consequence, the
use of this code as part of a larger work cannot be guaranteed to work with
future releases of this software and will most likely fail with future
additions. For questions, please contact the author of the software.

The terms and conditions of use of this software do not allow for the removal
of the copyright notice from the main program for visual display. For
integration with other software, a similar copyright notice must be produced
that is visible to users of the software.

The compiler source distribution contains the following files:
README.txt	This file
Makefile	Unix/linux makefile
MakefileMacOSX	Mac OS X Makefile
soapcpp2.h	Main header file
soapcpp2.c	Main application
symbol2.c	Symbol table handling and code generation module
error2.h	Header file for error2.c
error2.c	Error handling routines
init2.c		Compiler symbol table initialization
soapcpp2_lex.l	Flex/Lex tokens
soapcpp2_yacc.y	Yacc/Bison grammar
