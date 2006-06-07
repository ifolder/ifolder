The gSOAP WSDL parser and importer

INSTRUCTIONS

The gSOAP WSDL parser converts WSDL into a gSOAP header file for processing
with the gSOAP soapcpp2 compiler to generate client stubs and server skeletons.

Example:

$ wsdl2h -o Amazon.h http://soap.amazon.com/schemas/AmazonWebServices.wsdl

$ soapcpp2 Amazon.h

The generated Amazon.h includes the definitions of data types and service
operations of the Amazon Web service. To develop a C++ client application, you
can use the generated 'soapAmazonSearchBindingProxy.h' class and
'AmazonSearchBinding.nsmap' XML namespace table to access the Amazon Web
service. Both need to be '#include'-d in your source. Then compile and link the
soapC.cpp, soapClient.cpp, and stdsoap2.cpp sources to complete the build.
More information can be found in the gSOAP documentation.

When parsing a WSDL, the output file name is the WSDL input file name with
extension '.h' instead of '.wsdl'. When an input file is absent or a WSDL file
from a Web location is accessed, the header output will be produced on the
standard output. An input file may also contain a schema and will be handled as
such.

INPUT

wsdl2h reads from standard input or the file name provided at the command line:

wsdl2h [options] [-o outfile.h] [infile.wsdl]

Valid input file formats are .wsdl and .xsd (schema) files.

OUTPUT

The output file is a gSOAP-formatted header file. The header file syntax is
augmented with annotations reflecting WSDL and schema-specific bindings and
validation constraints.

We suggest the use of Doxygen (www.doxygen.org) to produce documented for the
generated header file. However, we STRONGLY recommend user to inspect the
generated header file first for warnings and other annotations indicating
potential problems.

Note that Doxygen's license model does not infinge on your ownership of the
gSOAP source code output when you purchased a commercial license.

COMMAND OPTIONS

-c	generate pure C header file code
-e	enum names will not be prefixed
-f	generate flat C++ class hierarchy for schema extensions
-m	create modules for separate compilation
-nname	use name as the base namespace prefix name instead of 'ns'
-ofile	output to file
-p	create polymorphic types with C++ inheritance with base xsd__anyType
-rhost:port
	connect via proxy host and port
-s	do not generate STL code (no std::string and no std::vector)
-tfile	use type map file instead of the default file typemap.dat
-v	verbose output
-?	help

DOCUMENTATION

See soapdoc2.pdf for documentation.

TYPEMAP FILE

The 'typemap.dat' file can be used to provide custom type mappings for binding
XML schema types to C and/or C++ types. The WSDL parser 'wsdl2h' can be used
without the 'typemap.dat' file, because and internal table is used to associate
XML schema types to C or C++ types (for C, use the -c option).

The 'typemap.dat' file also allows you to change the generation of the 'ns1',
'ns2', 'ns3', ... namespace prefixes with custom names.

INSTALLATION

Type 'make' in the 'wsdl' directory to build wsdl2h. You must first install the
gSOAP package and build the gSOAP compiler soapcpp2 to rebuild the wsdl2h WSDL
parser.

USING SSL FOR HTTPS

You must build the WSDL parser with 'make secure' to build an SSL-enabled
version of wsdl2h that can access HTTPS secure sites.

LICENSE

The gSOAP WSDL parser 'wsdl2h' and source code are released under the GPL.
See gpl.txt for more details. A commercial license is available from Genivia.
Please contact Genivia (contact@genivia.com) for more details.

COPYRIGHT NOTICE

gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.
