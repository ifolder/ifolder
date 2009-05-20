/*

soapcpp2.c

Main compiler.

gSOAP XML Web services tools
Copyright (C) 2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.

--------------------------------------------------------------------------------
gSOAP public license.

The contents of this file are subject to the gSOAP Public License Version 1.3
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at
http://www.cs.fsu.edu/~engelen/soaplicense.html
Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
for the specific language governing rights and limitations under the License.

The Initial Developer of the Original Code is Robert A. van Engelen.
Copyright (C) 2000-2004 Robert A. van Engelen, Genivia inc. All Rights Reserved.
--------------------------------------------------------------------------------
GPL license.

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place, Suite 330, Boston, MA 02111-1307 USA

Author contact information:
engelen@genivia.com / engelen@acm.org
--------------------------------------------------------------------------------
*/

#include "soapcpp2.h"

extern int init();
extern int yyparse();
extern FILE *yyin;

int vflag = 0;		/* SOAP version, 0=not set, 1=1.1, 2=1.2 */
int wflag = 0;		/* when set, don't generate WSDL and schema files */
int cflag = 0;		/* when set, generate files with .c extension */
int eflag = 0;		/* when set, use SOAP RPC encoding by default */
int mflag = 0;		/* when set, generate code that requires array/binary classes to explicitly remove malloced array */
int nflag = 0;		/* when set, names the namespaces global struct '%NAME%_namespaces */
int lflag = 0;		/* when set, create library */
int tflag = 0;		/* when set, generates typed messsages (with xsi:type attributes) */
int xflag = 0;		/* when set, excludes imported types */
char dirpath[1024];	/* directory path for generated source files */
char *prefix = "soap";	/* file name prefix for generated source files */
char filename[1024];	/* current file name */
char *importpath = NULL;/* file import path */

/*
IMPORTANT:

The terms and conditions of use of this software do not allow for the removal
of the copyright notice from the main program for visual display. For
integrations with other software, a similar copyright notice must be produced
that is visible to users of the software.
*/

int
main(int argc, char **argv)
{	int i, g;
	char *a;
	strcpy(filename, "<stdin>");
	for (i = 1; i < argc; i++)
	{	a = argv[i];
		if (*a == '-'
#ifdef WIN32
		 || *a == '/'
#endif
		)
		{	g = 1;
			while (g && *++a)
				switch (*a)
				{	case 'c':
						cflag = 1;
						break;
					case 'd':
						a++;
						g = 0;
						if (*a)
							strcpy(dirpath, a);
						else if (i < argc && argv[++i])
							strcpy(dirpath, argv[i]);
						else
							execerror("Option -d requires a directory path");
						if (*dirpath && dirpath[strlen(dirpath)-1] != '/' && dirpath[strlen(dirpath)-1] != '\\')
#ifdef WIN32
							strcat(dirpath, "\\");
#else
							strcat(dirpath, "/");
#endif
						break;
					case 'e':
						eflag = 1;
						break;
					case '?':
					case 'h':
						fprintf(stderr, "Usage: soapcpp2 [-1|-2] [-I path] [-d path] [-p name] [-c] [-i] [-m] [-n] [-t] [file]\n");
						exit(0);
					case 'I':
						a++;
						g = 0;
						if (*a)
							importpath = a;
						else if (i < argc && argv[++i])
							importpath = argv[i];
						else
							execerror("Option -I requires an import path");
						break;
					case 'i':
						break;
					case 'm':
						mflag = 1;
						break;
					case 'n':
						nflag = 1;
						break;
					case 'l':
						lflag = 1;
						break;
					case 't':
						tflag = 1;
						break;
					case 'w':
						wflag = 1;
						break;
					case 'x':
						xflag = 1;
						break;
					case 'p':
						a++;
						g = 0;
						if (*a)
							prefix = a;
						else if (i < argc && argv[++i])
							prefix = argv[i];
						else
							execerror("Option -p requires an output file name prefix");
						break;
					case '1':
						vflag = 1;
						envURI = "http://schemas.xmlsoap.org/soap/envelope/";
						encURI = "http://schemas.xmlsoap.org/soap/encoding/";
						break;
					case '2':
						vflag = 2;
						envURI = "http://www.w3.org/2003/05/soap-envelope";
						encURI = "http://www.w3.org/2003/05/soap-encoding";
						rpcURI = "http://www.w3.org/2003/05/soap-rpc";
						break;
					case 'v':
						fprintf(stderr, "gSOAP compiler version "VERSION". Copyright (C) 2001-2004 Genivia, Inc. All Rights Reserved.\n");
						exit(0);
					default:
            					fprintf(stderr, "soapcpp2: Unknown option %s\n", a);
            					exit(1);
				}
		}
		else if (!(yyin = fopen(argv[i], "r")))
		{	sprintf(errbuf, "Cannot open file \"%s\" for reading", argv[i]);
			execerror(errbuf);
		}
		else
			strcpy(filename, argv[i]);
	}
	fprintf(stderr, "\n**  The gSOAP Stub and Skeleton Compiler for C and C++ "VERSION"\n**  Copyright (C) 2001-2004 Robert van Engelen, Genivia, Inc.\n**  All Rights Reserved. This product is provided \"as is\", without any warranty.\n\n");
	init();
	if (yyparse())
		synerror("skipping the remaining part of the input");
	return errstat();
}
