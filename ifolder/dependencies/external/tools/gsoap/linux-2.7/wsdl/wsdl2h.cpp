/*

wsdl2h.cpp

WSDL parser and converter to gSOAP header file format

--------------------------------------------------------------------------------
gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.
This software is released under one of the following two licenses:
GPL or Genivia's license for commercial use.
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
A commercial use license is available from Genivia, Inc., contact@genivia.com
--------------------------------------------------------------------------------

Build:
	soapcpp2 -ipwsdl wsdl.h
	g++ -o wsdl2h wsdl2h.cpp types.cpp service.cpp wsdl.cpp schema.cpp wsdlC.cpp stdsoap2.cpp
	
TODO:
	Resolve relative versus absolute import paths for reading imported WSDL/schema (use URL local addresses)
	Do not generate abstract complexTypes, but include defs in derived types
	Handle simpleType derivation from base64
	Option to define base class for all classes, e.g. -b xsd__anyType
	Look into improving xs:choice and xs:any

*/

#include "types.h"
#include "service.h"

static void options(int argc, char **argv);

int cflag = 0,
    eflag = 0,
    fflag = 0,
    lflag = 0,
    mflag = 0,
    pflag = 0,
    sflag = 0,
    vflag = 0;

char *infile = NULL,
     *outfile = NULL,
     *mapfile = "typemap.dat",
     *proxy_host = NULL;

int proxy_port = 8080;

FILE *stream = stdout;

const char *prefix_name = "ns";

char elementformat[]   = "    %-35s  %-30s";
char pointerformat[]   = "    %-35s *%-30s";
char attributeformat[] = "   @%-35s  %-30s";
char vectorformat[]    = "    std::vector<%-22s> *%-30s";
char arrayformat[]     = "    %-35s *__ptr%-25s";
char sizeformat[]      = "    %-35s  __size%-24s";
char schemaformat[]    = "//gsoap %-4s schema %s:\t%s\n";
char serviceformat[]   = "//gsoap %-4s service %s:\t%s %s\n";
char paraformat[]      = "    %-35s %s%s";
char anonformat[]      = "    %-35s _%s%s";

char copyrightnotice[] = "\n**  The gSOAP WSDL parser for C and C++ "VERSION"\n**  Copyright (C) 2001-2004 Robert van Engelen, Genivia, Inc.\n**  All Rights Reserved. This product is provided \"as is\", without any warranty.\n**  This software is released under one of the following two licenses:\n**  GPL or Genivia's license for commercial use.\n\n";

char licensenotice[]   = "\n\
--------------------------------------------------------------------------------\n\
gSOAP XML Web services tools\n\
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.\n\
\n\
GPL license.\n\
\n\
This program is free software; you can redistribute it and/or modify it under\n\
the terms of the GNU General Public License as published by the Free Software\n\
Foundation; either version 2 of the License, or (at your option) any later\n\
version.\n\
\n\
This program is distributed in the hope that it will be useful, but WITHOUT ANY\n\
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A\n\
PARTICULAR PURPOSE. See the GNU General Public License for more details.\n\
\n\
You should have received a copy of the GNU General Public License along with\n\
this program; if not, write to the Free Software Foundation, Inc., 59 Temple\n\
Place, Suite 330, Boston, MA 02111-1307 USA\n\
\n\
Author contact information:\n\
engelen@genivia.com / engelen@acm.org\n\
--------------------------------------------------------------------------------\n\
A commercial use license is available from Genivia, Inc., contact@genivia.com\n\
--------------------------------------------------------------------------------\n";

int main(int argc, char **argv)
{ fprintf(stderr, copyrightnotice);
  options(argc, argv);
  wsdl__definitions definitions;
  if (infile)
  { if (!outfile)
    { if (strncmp(infile, "http://", 7))
      { const char *s = strrchr(infile, '.');
        if (s && (!strcmp(s, ".wsdl") || !strcmp(s, ".gwsdl") || !strcmp(s, ".xsd")))
        { outfile = estrdup(infile);
          outfile[s - infile + 1] = 'h';
          outfile[s - infile + 2] = '\0';
        }
        else
        { outfile = (char*)emalloc(strlen(infile) + 3);
          strcpy(outfile, infile);
          strcat(outfile, ".h");
        }
      }
    }
  }
  if (outfile)
  { stream = fopen(outfile, "w");
    if (!stream)
    { fprintf(stderr, "Cannot write to %s\n", outfile);
      exit(1);
    }
    fprintf(stderr, "Saving %s\n\n", outfile);
  }
  definitions.read(infile);
  if (definitions.error())
  { definitions.print_fault();
    exit(1);
  }
  Definitions def;
  def.compile(definitions);
  if (outfile)
  { fclose(stream);
    fprintf(stderr, "To complete the process, compile with:\nsoapcpp2 %s\n\n", outfile);
  }
  return 0;
}

////////////////////////////////////////////////////////////////////////////////
//
//	Parse command line options
//
////////////////////////////////////////////////////////////////////////////////

static void options(int argc, char **argv)
{ int i;
  for (i = 1; i < argc; i++)
  { char *a = argv[i];
    if (*a == '-'
#ifdef WIN32
     || *a == '/'
#endif
    )
    { int g = 1;
      while (g && *++a)
      { switch (*a)
        { case 'c':
            cflag = 1;
       	    break;
	  case 'e':
	    eflag = 1;
	    break;
	  case 'f':
	    fflag = 1;
	    break;
	  case 'l':
	    lflag = 1;
	    break;
	  case 'm':
	    mflag = 1;
	    break;
          case 'n':
            a++;
            g = 0;
            if (*a)
              prefix_name = a;
            else if (i < argc && argv[++i])
              prefix_name = argv[i];
            else
              fprintf(stderr, "wsdl2h: Option -n requires a prefix name argument");
	    break;
          case 'o':
            a++;
            g = 0;
            if (*a)
              outfile = a;
            else if (i < argc && argv[++i])
              outfile = argv[i];
            else
              fprintf(stderr, "wsdl2h: Option -o requires an output file argument");
	    break;
	  case 'p':
	    pflag = 1;
	    break;
	  case 'r':
            a++;
            g = 0;
            if (*a)
              proxy_host = a;
            else if (i < argc && argv[++i])
              proxy_host = argv[i];
            else
              fprintf(stderr, "wsdl2h: Option -r requires a proxy host:port argument");
            if (proxy_host)
	    { char *s = (char*)emalloc(strlen(proxy_host + 1));
	      strcpy(s, proxy_host);
	      proxy_host = s;
	      s = strchr(proxy_host, ':');
	      if (s)
	      { proxy_port = soap_strtol(s + 1, NULL, 10);
	        *s = '\0';
	      }
	    }
	    break;
	  case 's':
	    sflag = 1;
	    break;
          case 't':
            a++;
            g = 0;
            if (*a)
              mapfile = a;
            else if (i < argc && argv[++i])
              mapfile = argv[i];
            else
              fprintf(stderr, "wsdl2h: Option -t requires a type map file argument");
	    break;
	  case 'v':
	    vflag = 1;
	    break;
          case '?':
          case 'h':
            fprintf(stderr, "Usage: wsdl2h [-c|-e|-f|-s] [-m] [-n name] [-p] [-r host:port] [-v] [-t typemapfile.dat] [-o outfile.h] [infile.xsd|infile.wsdl|http://...]\n");
            exit(0);
          default:
            fprintf(stderr, "wsdl2h: Unknown option %s\n", a);
            exit(1);
        }
      }
    }
    else
     infile = argv[i];
  }
}

////////////////////////////////////////////////////////////////////////////////
//
//	Namespaces
//
////////////////////////////////////////////////////////////////////////////////

struct Namespace namespaces[] =
{
  {"SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/", "http://www.w3.org/*/soap-envelope"},
  {"SOAP-ENC", "http://schemas.xmlsoap.org/soap/encoding/", "http://www.w3.org/*/soap-encoding"},
  {"xsi", "http://www.w3.org/2001/XMLSchema-instance"},
  {"xsd", ""}, // http://www.w3.org/2001/XMLSchema"}, // don't use this, it might conflict with xs
  {"xml", "http://www.w3.org/XML/1998/namespace"},
  {"xs", "http://www.w3.org/2001/XMLSchema", "http://www.w3.org/*/XMLSchema" },
  {"http", "http://schemas.xmlsoap.org/wsdl/http/"},
  {"soap", "http://schemas.xmlsoap.org/wsdl/soap/", "http://schemas.xmlsoap.org/wsdl/soap*/"},
  {"mime", "http://schemas.xmlsoap.org/wsdl/mime/"},
  {"dime", "http://schemas.xmlsoap.org/ws/2002/04/dime/wsdl/", "http://schemas.xmlsoap.org/ws/*/dime/wsdl/"},
  {"wsdl", "http://schemas.xmlsoap.org/wsdl/"},
  {"gwsdl", "http://www.gridforum.org/namespaces/2003/03/gridWSDLExtensions"},
  {"sd", "http://www.gridforum.org/namespaces/2003/03/serviceData"},
  {NULL, NULL}
};
