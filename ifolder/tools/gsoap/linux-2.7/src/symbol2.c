/*

symbol2.c

Symbol table handling, type graph handling, and code generation.

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
Copyright (C) 2000-2003 Robert A. van Engelen, Genivia inc. All Rights Reserved.
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
#include "soapcpp2_yacc.h"

char *envURI = "http://schemas.xmlsoap.org/soap/envelope/";
char *encURI = "http://schemas.xmlsoap.org/soap/encoding/";
char *rpcURI = "http://www.w3.org/2003/05/soap-rpc";
char *xsiURI = "http://www.w3.org/2001/XMLSchema-instance";
char *xsdURI = "http://www.w3.org/2001/XMLSchema";
char *tmpURI = "http://tempuri.org";

static	Symbol *symlist = (Symbol*) 0;	/* pointer to linked list of symbols */
static	Symbol *nslist = (Symbol*) 0;	/* pointer to linked list of namespace prefix symbols */

static	Tnode *Tptr[TYPES];

Service *services = NULL;

FILE *fout, *fhead, *fclient, *fserver, *fheader, *flib, *fmatlab, *fmheader;

int typeNO = 1;	/* unique no. assigned to all types */

static int is_anytype_flag = 0; /* anytype is used */

/*
install - add new symbol
*/
Symbol *
install(const char *name, Token token)
{ Symbol *p;
  p = (Symbol*)emalloc(sizeof(Symbol));
  p->name = emalloc(strlen(name)+1);
  strcpy(p->name, name);
  p->token = token;
  p->next = symlist;
  symlist = p;
  return p;
}

/*
lookup - search for an identifier's name. If found, return pointer to symbol table entry. Return pointer 0 if not found.
*/
Symbol *
lookup(const char *name)
{ Symbol *p;
  for (p = symlist; p; p = p->next)
    if (!strcmp(p->name, name))
      return p;
  return NULL;
}

/*
gensymidx - generate new symbol from base name and index
*/
Symbol *
gensymidx(const char *base, int idx)
{ char buf[1024];
  Symbol *s;
  sprintf(buf, "%s_%d", base, idx);
  s = lookup(buf);
  if (s)
    return s;
  return install(buf, ID);
}

/*
gensym - generate new symbol from base name
*/
Symbol *
gensym(const char *base)
{ static int num = 1;
  return gensymidx(base, num++);
}

/*
mktable - make a new symbol table with a pointer to a previous table
*/
Table*
mktable(Table *table)
{	Table	*p;
	p = (Table*)emalloc(sizeof(Table));
	p->sym = lookup("/*?*/");
	p->list = (Entry*) 0;
	if (table == (Table*) 0)
		p->level = INTERNAL;
	else	p->level = table->level+1;
	p->prev = table;
	return p;
}

/*
mkmethod - make a new method by calling mktype
*/
Tnode*
mkmethod(Tnode *ret, Table *args)
{	FNinfo *fn = (FNinfo*)emalloc(sizeof(FNinfo));
	fn->ret = ret;
	fn->args = args;
	return mktype(Tfun, fn, 0);
}

/*
freetable - free space by removing a table
*/
void
freetable(Table *table)
{	Entry	*p, *q;
	if (table == (Table*) 0)
		return;
	for (p = table->list; p != (Entry*) 0; p = q) {
		q = p->next;
		free(p);
	}
	free(table);
}

/*
unlinklast - unlink last entry added to table
*/
Entry *
unlinklast(Table *table)
{	Entry	**p, *q;
	if (table == (Table*)0)
		return (Entry*)0;
	for (p = &table->list; *p != (Entry*)0 && (*p)->next != (Entry*)0;
	     p = &(*p)->next);
	q = *p;
	*p = (Entry*)0;
	return q;
}

/*
enter - enter a symbol in a table. Error if already in the table
*/
Entry	*
enter(table, sym)
Table	*table;
Symbol	*sym;
{ Entry	*p, *q = NULL;
  for (p = table->list; p; q = p, p = p->next)
    if (p->sym == sym && p->info.typ->type != Tfun)
    { sprintf(errbuf, "Duplicate declaration of %s (line %d)", sym->name, p->lineno);
      semerror(errbuf);
      return p;
    }
  p = (Entry*)emalloc(sizeof(Entry));
  p->sym = sym;
  p->info.typ = NULL;
  p->info.sto = Snone;
  p->info.hasval = False;
  p->info.minOccurs = 1;
  p->info.maxOccurs = 1;
  p->info.offset = 0;
  p->level = table->level;
  p->lineno = yylineno;
  p->next = NULL;
  if (!q)
    table->list = p;
  else
    q->next = p;
  return p;
}

/*
entry - return pointer to table entry of a symbol
*/
Entry	*entry(table, sym)
Table	*table;
Symbol	*sym;
{ Table	*t;
  Entry	*p;
  for (t = table; t; t = t->prev)
    for (p = t->list; p; p = p->next)
      if (p->sym == sym)
	return p;
  return NULL;
}

/*
reenter - re-enter a symbol in a table.
*/
Entry	*
reenter(table, sym)
Table	*table;
Symbol	*sym;
{ Entry	*p, *q = NULL;
  for (p = table->list; p; q = p, p = p->next)
    if (p->sym == sym)
      break;
  if (p && p->next)
  { if (q)
      q->next = p->next;
    else
      table->list = p->next;
    for (q = p->next; q->next; q = q->next)
      ;
    q->next = p;
    p->next = NULL;
  }
  return p;
}

Entry *
enumentry(Symbol *sym)
{ Table	*t;
  Entry	*p, *q;
  for (t = enumtable; t; t = t->prev)
    for (p = t->list; p; p = p->next)
      if (q = entry(p->info.typ->ref, sym))
	return q;
  return NULL;
}

char *get_mxClassID(Tnode*);
char *c_ident(Tnode*);
char *c_storage(Storage);
char *c_init(Entry*);
char *c_type(Tnode*);
char *c_type_id(Tnode*, char*);
char *xsi_type_cond(Tnode*, int);
char *xsi_type(Tnode*);
char *xsi_type_cond_u(Tnode*, int);
char *xsi_type_u(Tnode*);
char *the_type(Tnode*);
char *wsdl_type(Tnode*, char*);
char *base_type(Tnode*, char*);
char *xml_tag(Tnode*);
char *ns_qualifiedElement(Tnode*);
char *ns_qualifiedAttribute(Tnode*);
char *ns_convert(char*);
char *ns_add(char*, char*);
char *ns_remove(char*);
char *ns_remove2(char*);
char *res_remove(char*);
char *ns_overridden(Table*, Entry*);

int has_class(Tnode*);
int has_getter(Tnode*);
int has_setter(Tnode*);
int has_ns(Tnode*);
int has_ns_t(Tnode*);
int has_ns_eq(char*, char*);
char *ns_of(char*);
char *prefix_of(char*);
int has_offset(Tnode*);
int reflevel(Tnode *typ);
Tnode* reftype(Tnode *typ);
int is_response(Tnode*);
Entry *get_response(Tnode*);
int is_primitive_or_string(Tnode*);
int is_primitive(Tnode*);
Entry *is_discriminant(Tnode*);
Entry *is_dynamic_array(Tnode*);
int is_binary(Tnode*);
int is_hexBinary(Tnode*);
int is_string(Tnode*);
int is_wstring(Tnode*);
int is_stdstring(Tnode*);
int is_stdwstring(Tnode*);
int is_stdstr(Tnode*);
int get_dimension(Tnode*);
char *has_soapref(Tnode*);
int is_document(const char*);
int is_literal(const char*);

char *xsi_type_Tarray(Tnode*);    
char *xsi_type_Darray(Tnode*);    

void matlab_def_table(Table *,Tnode *);
void def_table(Table *,Tnode *);
void out_generate(Tnode *);
int no_of_var(Tnode*);
char *pointer_stuff(Tnode*);
void in_defs(Table*);
void in_defs2(Table*);
void out_defs(Table*);
void mark_defs(Table*);
void in_attach(Table*);
void out_attach(Table*);
void serialize(Tnode*);
void mark(Tnode*);
void defaults(Tnode*);
void soap_put(Tnode*);
void soap_out(Tnode*);
void soap_out_Darray(Tnode *);
void soap_get(Tnode*);
void soap_in(Tnode*); 
void soap_in_Darray(Tnode *);
void soap_instantiate_class(Tnode *);
int get_Darraydims(Tnode *typ);

void soap_serve(Table*);
void generate_proto(Table*, Entry*);
void generate_call(Table*, Entry*);
void generate_server(Table*, Entry*);
void generate_header(Table*);
void generate_schema(Table*);
void gen_schema(FILE*,Table*,char*,char*,int,int,char*,char*,char*,char*);
/* void gen_schema_element_names(FILE *fd, Table *t, Entry *p, char *ns, char *ns1); */
void gen_schema_elements_attributes(FILE *fd, Table *t, char *ns, char *ns1, char *encoding, char *style);
void gen_schema_elements(FILE *fd, Tnode *p, char *ns, char *ns1);
int gen_schema_element(FILE *fd, Entry *q, char *ns, char *ns1);
void gen_schema_attributes(FILE *fd, Tnode *p, char *ns, char *ns1);
void gen_wsdl(FILE*,Table*,char*,char*,char*,char*,char*,char*,char*);
void gen_nsmap(FILE*,Symbol*,char*);
void gen_proxy(FILE*,Table*,Symbol*,char*,char*,char*,char*,char*);
void gen_object(FILE*,Table*,Symbol*,char*,char*,char*,char*,char*);
void gen_data(char*,Table*,char*,char*,char*,char*,char*,char*);
FILE *gen_env(char*,char*,int,Table*,char*,char*,char*,char*,char*,char*);
void gen_field(FILE*,int,Entry*,char*,char*,char*);
void gen_val(FILE*,int,Tnode*,char*,char*,char*);
void gen_atts(FILE*,Table*,char*);

/*
mktype - make a (new) type with a reference to additional information and the
width in bytes required to store objects of that type. A pointer to the
type is returned which can be compared to check if types are identical.
*/
Tnode *
mktype(Type type, void *ref, int width)
{	Tnode	*p;
	int t = 0;
	if (transient != -2 || type > Ttime)
	  t = transient;
	if (type != Tstruct && type != Tclass)
	for (p = Tptr[type]; p != (Tnode*) 0; p = p->next)
		if (p->ref == ref && p->sym == (Symbol*) 0 && p->width == width && p->transient == t)
			return p;	/* type alrady exists in table */
	p = (Tnode*)emalloc(sizeof(Tnode));	/* install new type */
	p->type = type;
	p->ref = ref;
	p->id = lookup("/*?*/");
	p->base = NULL;
	p->sym = (Symbol*)0;
	p->response = (Entry*)0;
	p->width = width;
	p->generated = False;
	p->wsdl = False;
	p->next = Tptr[type];
	p->transient = t;
	p->imports = imports;
	p->pattern = NULL;
	p->minLength = 0;
	p->maxLength = -1;
	p->num = typeNO++;
	Tptr[type] = p;
	return p;
}

Tnode *
mksymtype(Tnode *typ, Symbol *sym)
{	Tnode *p;
	p = (Tnode*)emalloc(sizeof(Tnode));	/* install new type */
	p->type = typ->type;
	p->ref = typ->ref;
	p->id = typ->id;
	p->sym = sym;
	p->response = (Entry*)0;
	p->width = typ->width;
	p->generated = True; /* copy of existing (generated) type */
	p->wsdl = False;
	p->next = Tptr[typ->type];
	p->transient = transient;
	p->imports = imports;
	p->pattern = NULL;
	p->minLength = 0;
	p->maxLength = -1;
	p->num = typeNO++;
	Tptr[typ->type] = p;
	return p;
}

Tnode *
mktemplate(Tnode *typ, Symbol *id)
{	Tnode *p;
	for (p = Tptr[Ttemplate]; p; p = p->next)
		if (p->ref == typ && p->id == id && p->transient == transient)
			return p;	/* type alrady exists in table */
	p = (Tnode*)emalloc(sizeof(Tnode));	/* install new type */
	p->type = Ttemplate;
	p->ref = typ;
	p->id = id;
	p->sym = NULL;
	p->response = (Entry*)0;
	p->width = 0;
	p->generated = False; /* copy of existing (generated) type */
	p->wsdl = False;
	p->next = Tptr[Ttemplate];
	p->transient = transient;
	p->imports = imports;
	p->pattern = NULL;
	p->minLength = 0;
	p->maxLength = -1;
	p->num = typeNO++;
	Tptr[Ttemplate] = p;
	return p;
}

/*	DO NOT REMOVE OR ALTER (SEE LICENCE AGREEMENT AND COPYING.txt)	*/
void
copyrightnote(FILE *fd, char *fn)
{ fprintf(fd, "/* %s\n   Generated by gSOAP "VERSION" from %s\n   Copyright (C) 2001-2004 Genivia, Inc. All Rights Reserved.\n   This software is released under one of the following three licenses:\n   GPL, the gSOAP public license, or Genivia's license for commercial use.\n   See README.txt for further details.\n*/", fn, filename);
}

void
ident(FILE *fd, char *fn)
{ time_t t = time(NULL), *p = &t;
  char tmp[256];
  strftime(tmp, 256, "%Y-%m-%d %H:%M:%S GMT", gmtime(p));
  fprintf(fd, "\n\nSOAP_SOURCE_STAMP(\"@(#) %s ver "VERSION" %s\")\n", fn, tmp);
}

void
compile(Table *table)
{	Entry *p;
	Tnode *typ;
	Pragma *pragma;
	char *s;
	char base[1024];
	char soapStub[1024];
	char soapH[1024];
	char soapC[1024];
	char soapClient[1024];
	char soapServer[1024];
	char soapClientLib[1024];
	char soapServerLib[1024];
	char pathsoapStub[1024];
	char pathsoapH[1024];
	char pathsoapC[1024];
	char pathsoapClient[1024];
	char pathsoapServer[1024];
	char pathsoapClientLib[1024];
	char pathsoapServerLib[1024];
      	char soapMatlab[1024];
      	char pathsoapMatlab[1024];
  	char soapMatlabHdr[1024];
      	char pathsoapMatlabHdr[1024];
    
	DBGLOG(fprintf(stderr,"\n IN dumptable."));
	if (*dirpath)
	  fprintf(stderr, "Using project directory path: %s\n", dirpath);

	if (namespaceid)
	{ prefix = namespaceid;
	  fprintf(stderr, "Using code namespace: %s\n", namespaceid);
	}
	strcpy(base, prefix);
	if (cflag)
		s = ".c";
	else
		s = ".cpp";

  	strcpy(soapMatlab, base);
  	strcat(soapMatlab, "Matlab.c");
  	strcpy(pathsoapMatlab, dirpath);
  	strcat(pathsoapMatlab, soapMatlab );
  
  	strcpy(soapMatlabHdr, base);
  	strcat(soapMatlabHdr, "Matlab.h");
  	strcpy(pathsoapMatlabHdr, dirpath);
  	strcat(pathsoapMatlabHdr, soapMatlabHdr);

	strcpy(soapStub, base);
	strcat(soapStub, "Stub.h");
	strcpy(pathsoapStub, dirpath);
	strcat(pathsoapStub, soapStub);
	strcpy(soapH, base);
	strcat(soapH, "H.h");
	strcpy(pathsoapH, dirpath);
	strcat(pathsoapH, soapH);
	strcpy(soapC, base);
	strcat(soapC, "C");
	strcat(soapC, s);
	strcpy(pathsoapC, dirpath);
	strcat(pathsoapC, soapC);
	strcpy(soapClient, base);
	strcat(soapClient, "Client");
	strcat(soapClient, s);
	strcpy(pathsoapClient, dirpath);
	strcat(pathsoapClient, soapClient);
	strcpy(soapServer, base);
	strcat(soapServer, "Server");
	strcat(soapServer, s);
	strcpy(pathsoapServer, dirpath);
	strcat(pathsoapServer, soapServer);
	strcpy(soapClientLib, base);
	strcat(soapClientLib, "ClientLib");
	strcat(soapClientLib, s);
	strcpy(pathsoapClientLib, dirpath);
	strcat(pathsoapClientLib, soapClientLib);
	strcpy(soapServerLib, base);
	strcat(soapServerLib, "ServerLib");
	strcat(soapServerLib, s);
	strcpy(pathsoapServerLib, dirpath);
	strcat(pathsoapServerLib, soapServerLib);

	if (mflag)
 	{ fprintf(stderr, "Saving %s\n", pathsoapMatlab);
 	  fmatlab=fopen(pathsoapMatlab, "w");
 	  if (!fmatlab)
 		execerror("Cannot write to file");
 	  copyrightnote(fmatlab, soapMatlab);
 	  fprintf(fmatlab,"\n#include \"%s\"\n", soapMatlabHdr);
 	  fprintf(stderr, "Saving %s\n", pathsoapMatlabHdr);
 	  fmheader=fopen(pathsoapMatlabHdr, "w");
 	  if (!fmheader)
		execerror("Cannot write to file");
 	  copyrightnote(fmheader, soapMatlabHdr);
 	  fprintf(fmheader,"\n#include \"mex.h\"\n#include \"%s\"\n", soapStub);
	}

	fprintf(stderr, "Saving %s\n", pathsoapStub);
	fheader=fopen(pathsoapStub, "w");
	if (!fheader)
		execerror("Cannot write to file");
	copyrightnote(fheader, soapStub);
	fprintf(stderr, "Saving %s\n", pathsoapH);
	fhead=fopen(pathsoapH,"w");
	if (!fhead)
		execerror("Cannot write to file");
	copyrightnote(fhead, soapH);
	fprintf(stderr, "Saving %s\n", pathsoapC);
	fout=fopen(pathsoapC,"w");
	if (!fout)
		execerror("Cannot write to file");
	copyrightnote(fout, soapC);
	fprintf(stderr, "Saving %s\n", pathsoapClient);
        fclient=fopen(pathsoapClient,"w");
	if (!fclient)
		execerror("Cannot write to file");
	copyrightnote(fclient, soapClient);
	fprintf(stderr, "Saving %s\n", pathsoapServer);
        fserver=fopen(pathsoapServer,"w");
	if (!fserver)
		execerror("Cannot write to file");
	copyrightnote(fserver, soapServer);
	flib=fopen(pathsoapClientLib,"w");
	if (!flib)
		execerror("Cannot write to file");
	copyrightnote(flib, soapClientLib);
	fprintf(stderr, "Saving %s\n", pathsoapClientLib);
	fprintf(flib, "\n#define WITH_NOGLOBAL");
	fprintf(flib, "\n#define SOAP_FMAC3 static");
	fprintf(flib, "\n#include \"%s\"", soapC);
	fprintf(flib, "\n#include \"%s\"", soapClient);
	fprintf(flib, "\n\n/* end of %s */\n", soapClientLib);
	fclose(flib);
	flib=fopen(pathsoapServerLib,"w");
	if (!flib)
		execerror("Cannot write to file");
	copyrightnote(flib, soapServerLib);
	fprintf(stderr, "Saving %s\n", pathsoapServerLib);
	fprintf(flib, "\n#define WITH_NOGLOBAL");
	fprintf(flib, "\n#define SOAP_FMAC3 static");
	fprintf(flib, "\n#include \"%s\"", soapC);
	fprintf(flib, "\n#include \"%s\"", soapServer);
	fprintf(flib, "\n\n/* end of %s */\n", soapServerLib);
	fclose(flib);

	fprintf(fhead,"\n#ifndef %sH_H\n#define %sH_H", prefix, prefix);
	fprintf(fhead,"\n#include \"%s\"", soapStub);
	if (cflag)
	  fprintf(fhead,"\n#ifdef __cplusplus\nextern \"C\" {\n#endif");
	fprintf(fhead,"\n\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
	fprintf(fheader,"\n#ifndef %sStub_H\n#define %sStub_H", prefix, prefix);
	for (pragma = pragmas; pragma; pragma = pragma->next)
	  fprintf(fheader,"\n%s", pragma->pragma);
	if (nflag)
	  fprintf(fheader,"\n#define WITH_NONAMESPACES");
	if (namespaceid)
	{ fprintf(fheader,"\n#define WITH_NOGLOBAL");
	  fprintf(fheader,"\n#define SOAP_BEGIN_NAMESPACE(name) namespace name {");
	  fprintf(fheader,"\n#define SOAP_END_NAMESPACE(name) }");
	}
	fprintf(fheader,"\n#include \"stdsoap2.h\"");
	if (cflag)
	  fprintf(fheader,"\n#ifdef __cplusplus\nextern \"C\" {\n#endif");
	fprintf(fheader,"\n\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
	generate_header(table);
	generate_schema(table);
	fprintf(fout,"\n#include \"%sH.h\"", prefix);
	if (cflag)
	  fprintf(fout,"\n#ifdef __cplusplus\nextern \"C\" {\n#endif");
	fprintf(fout,"\n\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
	ident(fout, soapC);
        fprintf(fclient,"\n#include \"%sH.h\"", prefix);
	if (cflag)
	  fprintf(fclient,"\n#ifdef __cplusplus\nextern \"C\" {\n#endif");
	fprintf(fclient,"\n\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
	ident(fclient, soapClient);
        fprintf(fserver,"\n#include \"%sH.h\"", prefix);
	if (cflag)
	  fprintf(fserver,"\n#ifdef __cplusplus\nextern \"C\" {\n#endif");
	fprintf(fserver,"\n\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
	ident(fserver, soapServer);

        soap_serve(table);

	if (!lflag)
	{
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_ignore_element(struct soap*);");
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_putindependent(struct soap*);");
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_getindependent(struct soap*);");
	fprintf(fhead, "\nSOAP_FMAC3 void SOAP_FMAC4 soap_markelement(struct soap*, const void*, int);");
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_putelement(struct soap*, const void*, const char*, int, int);");
	fprintf(fhead, "\nSOAP_FMAC3 void * SOAP_FMAC4 soap_getelement(struct soap*, int*);");
	/*
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_putattachments(struct soap*);");
	*/
	fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_resolve_attachments(struct soap*);");

	fprintf(fout,"\n\n#ifndef WITH_NOGLOBAL");
        if (entry(classtable, lookup("SOAP_ENV__Header"))->info.typ->type == Tstruct)
	  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serializeheader(struct soap *soap)\n{\n\tif (soap->header)\n\t\tsoap_serialize_SOAP_ENV__Header(soap, soap->header);\n}");
	else
	  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serializeheader(struct soap *soap)\n{\n\tif (soap->header)\n\t\tsoap->header->soap_serialize(soap);\n}");
	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putheader(struct soap *soap)\n{\n\tif (soap->header)\n\t{\tsoap->part = SOAP_IN_HEADER;\n\t\tsoap_out_SOAP_ENV__Header(soap, \"SOAP-ENV:Header\", 0, soap->header, NULL);\n\t\tsoap->part = SOAP_END_HEADER;\n\t}\n\treturn SOAP_OK;\n}");
	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_getheader(struct soap *soap)\n{\n\tsoap->part = SOAP_IN_HEADER;\n\tsoap->header = soap_in_SOAP_ENV__Header(soap, \"SOAP-ENV:Header\", NULL, NULL);\n\tsoap->part = SOAP_END_HEADER;\n\treturn soap->header == NULL;\n}");
        if ((p = entry(classtable, lookup("SOAP_ENV__Fault")))->info.typ->type == Tstruct && !has_class(p->info.typ))
	{ fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_fault(struct soap *soap)\n{\n\tif (!soap->fault)\n\t{\tsoap->fault = (struct SOAP_ENV__Fault*)soap_malloc(soap, sizeof(struct SOAP_ENV__Fault));\n\t\tsoap_default_SOAP_ENV__Fault(soap, soap->fault);\n\t}\n\tif (soap->version != 1 && !soap->fault->SOAP_ENV__Code)\n\t{\tsoap->fault->SOAP_ENV__Code = (struct SOAP_ENV__Code*)soap_malloc(soap, sizeof(struct SOAP_ENV__Code));\n\t\tsoap_default_SOAP_ENV__Code(soap, soap->fault->SOAP_ENV__Code);\n\t}\n}");
	  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serializefault(struct soap *soap)\n{\n\tif (soap->fault)\n\t\tsoap_serialize_SOAP_ENV__Fault(soap, soap->fault);\n}");
	}
	else
	{ fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_fault(struct soap *soap)\n{\n\tif (!soap->fault)\n\t{\tsoap->fault = soap_new_SOAP_ENV__Fault(soap, -1);\n\t\tsoap->fault->soap_default(soap);\n\t}\n\tif (soap->version != 1 && !soap->fault->SOAP_ENV__Code)\n\t{\tsoap->fault->SOAP_ENV__Code = (struct SOAP_ENV__Code*)soap_malloc(soap, sizeof(struct SOAP_ENV__Code));\n\t\tsoap_default_SOAP_ENV__Code(soap, soap->fault->SOAP_ENV__Code);\n\t}\n}");
	  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serializefault(struct soap *soap)\n{\n\tsoap_fault(soap);\n\tif (soap->fault)\n\t\tsoap->fault->soap_serialize(soap);\n}");
	}
        if ((p = entry(classtable, lookup("SOAP_ENV__Fault")))->info.typ->type == Tstruct)
	{ fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putfault(struct soap *soap)\n{\n\tif (soap->fault)\n\t\treturn soap_put_SOAP_ENV__Fault(soap, soap->fault, \"SOAP-ENV:Fault\", NULL);\n\treturn SOAP_OK;\n}");
	  fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_getfault(struct soap *soap)\n{\n\treturn (soap->fault = soap_get_SOAP_ENV__Fault(soap, NULL, \"SOAP-ENV:Fault\", NULL)) == NULL;\n}");
	}
	else
	{ fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putfault(struct soap *soap)\n{\n\tsoap_fault(soap);\n\tif (soap->fault)\n\t\treturn soap->fault->soap_put(soap, \"SOAP-ENV:Fault\", NULL);\n\treturn SOAP_EOM;\n}");
	  fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_getfault(struct soap *soap)\n{\n\tsoap_fault(soap);\n\tif (soap->fault)\n\t\treturn soap->fault->soap_get(soap, \"SOAP-ENV:Fault\", NULL) == NULL;\n\treturn SOAP_EOM;\n}");
	}
	fprintf(fout,"\n\nSOAP_FMAC3 const char ** SOAP_FMAC4 soap_faultcode(struct soap *soap)\n{\n\tsoap_fault(soap);\n\tif (soap->version == 2)\n\t\treturn (const char**)&soap->fault->SOAP_ENV__Code->SOAP_ENV__Value;\n\treturn (const char**)&soap->fault->faultcode;\n}");
	fprintf(fout,"\n\nSOAP_FMAC3 const char ** SOAP_FMAC4 soap_faultstring(struct soap *soap)\n{\n\tsoap_fault(soap);\n\tif (soap->version == 2)\n\t\treturn (const char**)&soap->fault->SOAP_ENV__Reason;\n\treturn (const char**)&soap->fault->faultstring;\n}");
	fprintf(fout,"\n\nSOAP_FMAC3 const char ** SOAP_FMAC4 soap_faultdetail(struct soap *soap)\n{\n\tsoap_fault(soap);");
	if (has_detail_string())
	  fprintf(fout,"\n\tif (soap->version == 1)\n\t{\tif (!soap->fault->detail)\n\t\t{\tsoap->fault->detail = (struct SOAP_ENV__Detail*)soap_malloc(soap, sizeof(struct SOAP_ENV__Detail));\n\t\t\tsoap_default_SOAP_ENV__Detail(soap, soap->fault->detail);\n\t\t}\n\t\treturn (const char**)&soap->fault->detail->__any;\n\t}");
	if (has_Detail_string())
	  fprintf(fout,"\n\tif (!soap->fault->SOAP_ENV__Detail)\n\t{\tsoap->fault->SOAP_ENV__Detail = (struct SOAP_ENV__Detail*)soap_malloc(soap, sizeof(struct SOAP_ENV__Detail));\n\t\tsoap_default_SOAP_ENV__Detail(soap, soap->fault->SOAP_ENV__Detail);\n\t}\n\treturn (const char**)&soap->fault->SOAP_ENV__Detail->__any;\n}");
        else
	  fprintf(fout,"\n\treturn NULL;\n}");
	fprintf(fout,"\n\n#endif");

	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_getindependent(struct soap *soap)\n{");
	fprintf(fout,"\n\tint t;\n\tfor (;;)");
	fprintf(fout,"\n\t\tif (!soap_getelement(soap, &t))");
        fprintf(fout,"\n\t\t\tif (soap->error || soap_ignore_element(soap))\n\t\t\t\tbreak;");
        fprintf(fout,"\n\tif (soap->error == SOAP_NO_TAG || soap->error == SOAP_EOF)");
        fprintf(fout,"\n\t\tsoap->error = SOAP_OK;");
	/*
	if (Tptr[Ttemplate])
          fprintf(fout,"\n\tsoap_resolve_containers(soap);");
	*/
        fprintf(fout,"\n\treturn soap->error;");
        fprintf(fout,"\n}");

	fprintf(fout,"\n\nSOAP_FMAC3 void * SOAP_FMAC4 soap_getelement(struct soap *soap, int *type)\n{");
        fprintf(fout,"\n\tif (soap_peek_element(soap))\n\t\treturn NULL;");
	fprintf(fout,"\n\tif (!*soap->id || !(*type = soap_lookup_type(soap, soap->id)))\n\t\t*type = soap_lookup_type(soap, soap->href);");
	fprintf(fout,"\n\tswitch (*type)\n\t{");
	DBGLOG(fprintf(stderr,"\n Calling in_defs( )."));
	fflush(fout);
	in_defs(table);
	DBGLOG(fprintf(stderr,"\n Completed in_defs( )."));
        fprintf(fout,"\n\tdefault:\n\t{\tconst char *t = soap->type;\n\t\tif (!*t)\n\t\t\tt = soap->tag;");
	fflush(fout);
	in_defs2(table);
        fprintf(fout,"\n\t}\n\t}\n\tsoap->error = SOAP_TAG_MISMATCH;\n\treturn NULL;\n}");

	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_ignore_element(struct soap *soap)");
	fprintf(fout,"\n{\tif (!soap_peek_element(soap))");
	fprintf(fout,"\n\t{\tint t;");
	fprintf(fout,"\n\t\tif (soap->mustUnderstand && !soap->other)");
	fprintf(fout,"\n\t\t\treturn soap->error = SOAP_MUSTUNDERSTAND;");
	fprintf(fout,"\n\t\tif (((soap->mode & SOAP_XML_STRICT) && soap->part != SOAP_IN_HEADER) || !soap_match_tag(soap, soap->tag, \"SOAP-ENV:\"))\n\t\t\treturn soap->error = SOAP_TAG_MISMATCH;");
	fprintf(fout,"\n\t\tif (!*soap->id || !soap_getelement(soap, &t))");
	fprintf(fout,"\n\t\t{\tsoap->peeked = 0;");
	fprintf(fout,"\n\t\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Unknown element '%%s' (level=%%u, %%d)\\n\", soap->tag, soap->level, soap->body));");
	fprintf(fout,"\n\t\t\tif (soap->fignore)\n\t\t\t\tsoap->error = soap->fignore(soap, soap->tag);\n\t\t\telse\n\t\t\t\tsoap->error = SOAP_OK;");
	fprintf(fout,"\n\t\t\tDBGLOG(TEST, if (!soap->error) SOAP_MESSAGE(fdebug, \"IGNORING element '%%s'\\n\", soap->tag));");
	fprintf(fout,"\n\t\t\tif (!soap->error && soap->body)");
	fprintf(fout,"\n\t\t\t{\tsoap->level++;");
	fprintf(fout,"\n\t\t\t\twhile (!soap_ignore_element(soap))");
	fprintf(fout,"\n\t\t\t\t\t;");
	fprintf(fout,"\n\t\t\t\tif (soap->error == SOAP_NO_TAG)");
	fprintf(fout,"\n\t\t\t\t\tsoap->error = soap_element_end_in(soap, NULL);");
	fprintf(fout,"\n\t\t\t}");
	fprintf(fout,"\n\t\t}");
	fprintf(fout,"\n\t}");
	fprintf(fout,"\n\treturn soap->error;");
	fprintf(fout,"\n}");

	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putindependent(struct soap *soap)\n{\n\tint i;\n\tstruct soap_plist *pp;");
	fprintf(fout,"\n\tif (soap->version == 1 && soap->encodingStyle && !(soap->mode & (SOAP_XML_TREE | SOAP_XML_GRAPH)))");
	fprintf(fout,"\n\t\tfor (i = 0; i < SOAP_PTRHASH; i++)");
	fprintf(fout,"\n\t\t\tfor (pp = soap->pht[i]; pp; pp = pp->next)");
	fprintf(fout,"\n\t\t\t\tif (pp->mark1 == 2 || pp->mark2 == 2)");
	fprintf(fout,"\n\t\t\t\t\tif (soap_putelement(soap, pp->ptr, \"id\", pp->id, pp->type))\n\t\t\t\t\t\treturn soap->error;");
	fprintf(fout,"\n\treturn SOAP_OK;\n}");

	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putelement(struct soap *soap, const void *ptr, const char *tag, int id, int type)\n{");
	fprintf(fout,"\n\tswitch (type)\n\t{");
	fflush(fout);
        out_defs(table);
	fprintf(fout,"\n\t}\n\treturn SOAP_OK;\n}");

	if (is_anytype_flag)
	{ fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_markelement(struct soap *soap, const void *ptr, int type)\n{");
	  fprintf(fout,"\n\tswitch (type)\n\t{");
	  fflush(fout);
          mark_defs(table);
          fprintf(fout,"\n\t}\n}");
        }
	else
	  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_markelement(struct soap *soap, const void *ptr, int type)\n{ }");

	fprintf(fout,"\n\n#ifndef WITH_LEANER");
	fprintf(fout,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_resolve_attachments(struct soap *soap)\n{");
	fprintf(fout,"\n\tif (!(soap->mode & SOAP_ENC_DIME))\n\t\treturn SOAP_OK;");
	fprintf(fout,"\n\twhile (soap->dime.flags & SOAP_DIME_CF)\n\t{\tif (soap_getdimehdr(soap))\n\t\t\treturn soap->error;\n\t\tif (soap_move(soap, soap->dime.size))\n\t\t\treturn soap->error = SOAP_EOF;\n\t}");
	fprintf(fout,"\n\tif (soap_move(soap, ((soap->dime.size+3)&(~3))-soap_tell(soap)))\n\t\treturn soap->error = SOAP_EOF;");
	fprintf(fout,"\n\tfor (;;)");
        fprintf(fout,"\n\t{\tif (soap_getdime(soap))\n\t\t\tbreak;");
	fprintf(fout,"\n\t\tswitch (soap_lookup_type(soap, soap->dime.id))\n\t\t{");
	fflush(fout);
        in_attach(table);
        fprintf(fout,"\n\t\t}");
        fprintf(fout,"\n\t}");
        fprintf(fout,"\n\tif (soap->error == SOAP_DIME_END)");
        fprintf(fout,"\n\t\tsoap->error = SOAP_OK;");
        fprintf(fout,"\n\treturn soap->error;");
        fprintf(fout,"\n}");

	/*
	fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_putattachments(struct soap *soap)\n{\n\tint i;\n\tstruct soap_plist *pp;");
	fprintf(fout,"\n\tif (!(soap->mode & SOAP_ENC_DIME))\n\t\treturn SOAP_OK;");
	fprintf(fout,"\n\tfor (i = 0; i < SOAP_PTRHASH; i++)");
	fprintf(fout,"\n\t\tfor (pp = soap->pht[i]; pp; pp = pp->next)");
	fprintf(fout,"\n\t\t\tif (pp->mark2 == 3)");
	fprintf(fout,"\n\t\t\t\tswitch (pp->type)\n\t\t\t\t{");
	fflush(fout);
        out_attach(table);
	fprintf(fout,"\n\t\t\t\t}\n\t\t\n\treturn SOAP_OK;\n}");
	*/

        fprintf(fout,"\n#endif");
	}
	
	if (Tptr[Tclass] || Tptr[Ttemplate])
	{

	if (cflag)
		semwarn("Option -c conflicts with the use of classes");

	fprintf(fhead,"\n\nSOAP_FMAC3 void * SOAP_FMAC4 soap_instantiate(struct soap*, int, const char*, const char*, size_t*);");
	if (!lflag)
	{
	fprintf(fout,"\n\nSOAP_FMAC3 void * SOAP_FMAC4 soap_instantiate(struct soap *soap, int t, const char *type, const char *arrayType, size_t *n)\n{\n\tswitch (t)\n\t{");
	if (classtable)
	  for (p = classtable->list; p; p = p->next)
	    if ((p->info.typ->type == Tclass || has_class(p->info.typ)) && !is_transient(p->info.typ))
	      fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn (void*)soap_instantiate_%s(soap, -1, type, arrayType, n);", c_ident(p->info.typ), c_ident(p->info.typ));
	if (typetable)
	  for (p = typetable->list; p; p = p->next)
	    if ((p->info.typ->type == Tclass || has_class(p->info.typ)) && !is_transient(p->info.typ))
	      fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn (void*)soap_instantiate_%s(soap, -1, type, arrayType, n);", c_ident(p->info.typ), c_ident(p->info.typ));
	for (typ = Tptr[Ttemplate]; typ; typ = typ->next)
	  if (typ->ref && !is_transient(typ->ref))
	    fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn (void*)soap_instantiate_%s(soap, -1, type, arrayType, n);", c_ident(typ), c_ident(typ));

	fprintf(fout,"\n\t}\n\treturn NULL;\n}");
	}

	fprintf(fhead,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_fdelete(struct soap_clist*);");
	if (!lflag)
	{
	fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_fdelete(struct soap_clist *p)");
	fprintf(fout,"\n{\tswitch (p->type)\n\t{");
	if (classtable)
	  for (p = classtable->list; p; p = p->next)
	    if ((p->info.typ->type == Tclass || has_class(p->info.typ)) && !is_transient(p->info.typ))
	    { fprintf(fout,"\n\tcase SOAP_TYPE_%s:", c_ident(p->info.typ));
	      fprintf(fout,"\n\t\tif (p->size < 0)\n\t\t\tdelete (%s*)p->ptr;\n\t\telse\n\t\t\tdelete[] (%s*)p->ptr;\n\t\tbreak;", c_type(p->info.typ), c_type(p->info.typ));
	    }
	if (typetable)
	  for (p = typetable->list; p; p = p->next)
	    if (p->info.typ->type == Tclass) /* && is_external(p->info.typ)) */
	    { fprintf(fout,"\n\tcase SOAP_TYPE_%s:", c_ident(p->info.typ));
	      fprintf(fout,"\n\t\tif (p->size < 0)\n\t\t\tdelete (%s*)p->ptr;\n\t\telse\n\t\t\tdelete[] (%s*)p->ptr;\n\t\tbreak;", c_type(p->info.typ), c_type(p->info.typ));
	    }
	for (typ = Tptr[Ttemplate]; typ; typ = typ->next)
	  if (typ->ref && !is_transient(typ->ref))
	  { fprintf(fout,"\n\tcase SOAP_TYPE_%s:", c_ident(typ));
	    fprintf(fout,"\n\t\tif (p->size < 0)\n\t\t\tdelete (%s*)p->ptr;\n\t\telse\n\t\t\tdelete[] (%s*)p->ptr;\n\t\tbreak;", c_type(typ), c_type(typ));
	  }
	fprintf(fout,"\n\t}");
	fprintf(fout,"\n}");
	}

	fprintf(fhead,"\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_class_id_enter(struct soap*, const char*, void*, int, size_t, const char*, const char*);");
	if (!lflag)
	{
	fprintf(fout,"\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_class_id_enter(struct soap *soap, const char *id, void *p, int t, size_t n, const char *type, const char *arrayType)");
	fprintf(fout, "\n{\treturn soap_id_enter(soap, id, p, t, n, 0, type, arrayType, soap_instantiate);\n}");
	/*
	fprintf(fout, "\n{\tstruct soap_ilist *ip;");
	fprintf(fout, "\n\tDBGLOG(TEST,SOAP_MESSAGE(fdebug, \"Class id enter id='%%s' t=%%d loc=%%p type='%%s' arrayType='%%s'\\n\", id, t, p, type?type:\"\", arrayType?arrayType:\"\"));");
	fprintf(fout, "\n\tsoap->alloced = 0;\n\tif (!*id)\n\t{\tif (!p)\n\t\t\treturn soap_instantiate(soap, t, type, arrayType, NULL);\n\t\telse\n\t\t\treturn p;\n\t}");
	fprintf(fout, "\n\tip = soap_lookup(soap, id);");
	fprintf(fout, "\n\tif (!ip)");
	fprintf(fout, "\n\t{\tip = soap_enter(soap, id);");
	fprintf(fout, "\n\t\tip->type = t;\n\t\tip->link = NULL;\n\t\tip->copy = NULL;\n\t\tip->flist = NULL;");
	fprintf(fout, "\n\t\tif (!p)\n\t\t\tp = soap_instantiate(soap, t, type, arrayType, &n);");
	fprintf(fout, "\n\t\tip->ptr = p;\n\t\tip->size = n;\n\t\tip->level = 0;");
	fprintf(fout, "\n\t}\n\telse if (ip->ptr)");
	fprintf(fout, "\n\t{\tif (p)\n\t\t{\tsoap->error = SOAP_MULTI_ID;\n\t\t\treturn NULL;\n\t\t}");
	fprintf(fout, "\n\t}\n\telse");
	fprintf(fout, "\n\t{\tif (!p)\n\t\t\tp = soap_instantiate(soap, t, type, arrayType, &n);");
	fprintf(fout, "\n\t\tip->ptr = p;\n\t\tip->size = n;\n\t\tip->level = 0;");
	fprintf(fout, "\n\t\tif (!soap->blist)\n\t\t\tsoap_resolve_ptr(ip);");
	fprintf(fout, "\n\t}\n\treturn ip->ptr;\n}");
	*/
	}

	/*
	fprintf(fhead, "\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_class_id_forward(struct soap*, const char*, void*, int, int, size_t);");
	if (!lflag)
	{
	fprintf(fout, "\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_class_id_forward(struct soap *soap, const char *href, void *p, int st, int tt, size_t n)");
	fprintf(fout, "\n{\treturn soap_id_forward(soap, href, p, st, tt, n, 0, soap_fcopy);\n}");

	fprintf(fout, "\n{\tstruct soap_ilist *ip;");
	fprintf(fout, "\n\tif (!p || !*href)\n\t\treturn NULL;");
	fprintf(fout, "\n\tip = soap_lookup(soap, href);");
  	fprintf(fout, "\n\tif (p && ip)");
  	fprintf(fout, "\n\t{\tif (ip->ptr)");
      	fprintf(fout, "\n\t\t\tsoap_class_insert(soap, t, p, ip->ptr, n);");
    	fprintf(fout, "\n\t\telse");
      	fprintf(fout, "\n\t\t{\tregister struct soap_flist *fp = (struct soap_flist*)SOAP_MALLOC(sizeof(struct soap_flist));");
      	fprintf(fout, "\n\t\t\tif (fp)\n\t\t\t{\tfp->next = ip->flist;");
      	fprintf(fout, "\n\t\t\t\tfp->type = t;");
      	fprintf(fout, "\n\t\t\t\tfp->ptr = p;");
      	fprintf(fout, "\n\t\t\t\tfp->level = 0;");
      	fprintf(fout, "\n\t\t\t\tfp->finsert = soap_class_insert;");
      	fprintf(fout, "\n\t\t\t\tip->flist = fp;");
	fprintf(fout, "\n\t\t\t}\n\t\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Class forwarding type=%%d location=%%p\\n\", t, p));");
    	fprintf(fout, "\n\t\t}");
  	fprintf(fout, "\n\t}");
  	fprintf(fout, "\n\treturn p;");
	fprintf(fout, "\n}");
	}
	*/

	/*
	fprintf(fhead, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_class_insert(struct soap*, int, void*, void*, size_t);");
	*/
	if (!lflag)
	{
	/*
	fprintf(fout, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_class_insert(struct soap *soap, int t, void *p, void *q, size_t n)");
	fprintf(fout, "\n{\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Copy class content from %%p to %%p type %%d size %%lu\\n\", q, p, t, (unsigned long)n));");
	fprintf(fout, "\n\tmemcpy(p, q, n);\n}");
	*/
	}

	if (Tptr[Ttemplate])
	{
	fprintf(fhead, "\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_container_id_forward(struct soap*, const char*, void*, int, int, size_t, unsigned int);");
	if (!lflag)
	{
	fprintf(fout, "\n\nSOAP_FMAC3 void* SOAP_FMAC4 soap_container_id_forward(struct soap *soap, const char *href, void *p, int st, int tt, size_t n, unsigned int k)");
	fprintf(fout, "\n{\treturn soap_id_forward(soap, href, p, st, tt, n, k, soap_container_insert);\n}");
	/*
	fprintf(fout, "\n{\tstruct soap_ilist *ip = soap_lookup(soap, href);");
  	fprintf(fout, "\n\tif (ip)");
  	fprintf(fout, "\n\t{\tif (ip->ptr && ip->level == k)");
      	fprintf(fout, "\n\t\t\tsoap_container_insert(soap, t, p, ip->ptr, 0);");
    	fprintf(fout, "\n\t\telse");
      	fprintf(fout, "\n\t\t{\tregister struct soap_flist *fp = (struct soap_flist*)SOAP_MALLOC(sizeof(struct soap_flist));");
      	fprintf(fout, "\n\t\t\tif (fp)\n\t\t\t{\tfp->next = ip->flist;");
      	fprintf(fout, "\n\t\t\t\tfp->type = t;");
      	fprintf(fout, "\n\t\t\t\tfp->ptr = p;");
      	fprintf(fout, "\n\t\t\t\tfp->level = k;");
      	fprintf(fout, "\n\t\t\t\tfp->finsert = soap_container_insert;");
      	fprintf(fout, "\n\t\t\t\tip->flist = fp;");
	fprintf(fout, "\n\t\t\t}\n\t\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Container forwarding type=%%d location=%%p level=%%d\\n\", t, p, k));");
    	fprintf(fout, "\n\t\t}");
  	fprintf(fout, "\n\t}");
	fprintf(fout, "\n}");
	*/
	}

	/*
	fprintf(fhead, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_resolve_containers(struct soap*);");
	if (!lflag)
	{
	fprintf(fout, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_resolve_containers(struct soap *soap)");
	fprintf(fout, "\n{\tregister int i;");
  	fprintf(fout, "\n\tregister struct soap_ilist *ip;");
  	fprintf(fout, "\n\tregister struct soap_flist *fp;");
  	fprintf(fout, "\n\tfor (i = 0; i < SOAP_IDHASH; i++)");
    	fprintf(fout, "\n\t\tfor (ip = soap->iht[i]; ip; ip = ip->next)");
    	fprintf(fout, "\n\t\t{\tfor (fp = ip->flist; fp; fp = fp->next)");
      	fprintf(fout, "\n\t\t\t\tif (fp->ptr)\n\t\t\t\t\tsoap_container_insert(soap, fp->type, fp->ptr, ip->ptr);");
    	fprintf(fout, "\n\t\t}");
	fprintf(fout, "\n}");
	}
	*/

	fprintf(fhead, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_container_insert(struct soap*, int, int, void*, const void*, size_t);");
	if (!lflag)
	{
	fprintf(fout, "\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_container_insert(struct soap *soap, int st, int tt, void *p, const void *q, size_t n)");
	fprintf(fout, "\n{\tswitch (tt)\n\t{");
	for (typ = Tptr[Ttemplate]; typ; typ = typ->next)
	  if (typ->ref && !is_transient(typ->ref))
	  { fprintf(fout, "\n\tcase SOAP_TYPE_%s:", c_ident(typ));
	    fprintf(fout, "\n\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Container insert type=%%d in %%d location=%%p object=%%p\\n\", st, tt, p, q));");
	    fprintf(fout, "\n\t\t((%s)p)->insert(((%s)p)->end(), *(%s)q);", c_type_id(typ, "*"), c_type_id(typ, "*"), c_type_id(typ->ref, "*"));
	    fprintf(fout, "\n\t\tbreak;");
	  }
	fprintf(fout, "\n\tdefault:\n\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Could not insert type=%%d in %%d\\n\", st, tt));");
	fprintf(fout, "\n\t}");
	fprintf(fout, "\n}");
	}
	}

	}

	DBGLOG(fprintf(stderr,"\n Calling def_table( )."));
        def_table(table,0);
	DBGLOG(fprintf(stderr,"\n Completed def_table( )."));
	if (mflag)
 	{ DBGLOG(fprintf(stderr,"\n Calling matlab_def_table( )."));
	  matlab_def_table(table,0);
 	  DBGLOG(fprintf(stderr,"\n Completed matlab_def_table( )."));
 	  fclose(fmatlab);
 	  fclose(fmheader);
	}
	fprintf(fout,"\n\nSOAP_END_NAMESPACE(%s)", prefix);
	if (cflag)
	  fprintf(fout,"\n\n#ifdef __cplusplus\n}\n#endif");
	fprintf(fout, "\n\n/* end of %s */\n", soapC);
        fclose(fout);
	fprintf(fhead,"\n\nSOAP_END_NAMESPACE(%s)", prefix);
	if (cflag)
	  fprintf(fhead,"\n\n#ifdef __cplusplus\n}\n#endif");
	fprintf(fhead, "\n\n#endif");
	fprintf(fhead, "\n\n/* end of %s */\n", soapH);
	fclose(fhead);
	fprintf(fheader,"\n\nSOAP_END_NAMESPACE(%s)", prefix);
	if (cflag)
	  fprintf(fheader,"\n\n#ifdef __cplusplus\n}\n#endif");
	fprintf(fheader, "\n\n#endif");
	fprintf(fheader, "\n\n/* end of %s */\n", soapStub);
	fclose(fheader);
	fprintf(fclient,"\n\nSOAP_END_NAMESPACE(%s)", prefix);
	if (cflag)
	  fprintf(fclient,"\n\n#ifdef __cplusplus\n}\n#endif");
	fprintf(fclient, "\n\n/* end of %s */\n", soapClient);
        fclose(fclient);
	fprintf(fserver,"\n\nSOAP_END_NAMESPACE(%s)", prefix);
	if (cflag)
	  fprintf(fserver,"\n\n#ifdef __cplusplus\n}\n#endif");
	fprintf(fserver, "\n\n/* end of %s */\n", soapServer);
 	fclose(fserver);
}

void
gen_class(FILE *fd, Tnode *typ)
{ Entry *Eptr;
  char *x;
  x = xsi_type(typ);
  if (!x || !*x)
    x = wsdl_type(typ, "");
  typ->generated = True;
  if (is_volatile(typ))
    fprintf(fd, "\n\n#if 0 /* volatile */\n");
  /*
  else if (typ->ref)
    fprintf(fd, "\n\n#ifndef _SOAP_%s\n#define _SOAP_%s\n", c_ident(typ), c_ident(typ));
  */
  else
    fprintf(fd, "\n\n");
  if (is_transient(typ) && typ->ref)
    fprintf(fd, "/* transient type: */\n");
  else if (is_hexBinary(typ))
    fprintf(fd, "/* hexBinary schema type: */\n");
  else if (is_binary(typ))
    fprintf(fd, "/* Base64 schema type: */\n");
  else if (is_discriminant(typ))
    fprintf(fd, "/* Choice: */\n");
  else if (is_dynamic_array(typ))
  { Eptr = ((Table*)typ->ref)->list;
    if (is_external(Eptr->info.typ->ref) && ((Tnode*)Eptr->info.typ->ref)->type == Tclass || has_external(Eptr->info.typ->ref))
    { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containting an external object (declared at line %d)", Eptr->sym->name, c_type(Eptr->info.typ->ref), c_type(Eptr->info.typ->ref), Eptr->lineno);
      semwarn(errbuf);
    }
    else if (is_volatile(Eptr->info.typ->ref) && ((Tnode*)Eptr->info.typ->ref)->type == Tclass || has_volatile(Eptr->info.typ->ref))
    { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containting a volatile object (declared at line %d)", Eptr->sym->name, c_type(Eptr->info.typ->ref), c_type(Eptr->info.typ->ref), Eptr->lineno);
      semwarn(errbuf);
    }
    if (has_ns(typ) || is_untyped(typ))
      fprintf(fd, "/* Sequence of %s schema type: */\n", x);
    else
    { if (!eflag)
      { sprintf(errbuf, "array '%s' is not compliant with WS-I Basic Profile 1.0a, reason: SOAP encoded array", c_type(typ));
        compliancewarn(errbuf);
      }
      fprintf(fd, "/* SOAP encoded array of %s schema type: */\n", x);
    }
  }
  else if (is_primclass(typ))
    fprintf(fd, "/* Primitive %s schema type: */\n", x);
  else if (!strcmp(typ->id->name, "SOAP_ENV__Header"))
    fprintf(fd, "/* SOAP Header: */\n");
  else if (!strcmp(typ->id->name, "SOAP_ENV__Fault"))
    fprintf(fd, "/* SOAP Fault: */\n");
  else if (!strcmp(typ->id->name, "SOAP_ENV__Code"))
    fprintf(fd, "/* SOAP Fault Code: */\n");
  else if (x && *x && typ->ref)
    fprintf(fd, "/* %s: */\n", x);
  fflush(fd);
  if (typ->type == Tstruct)
  { { DBGLOG(fprintf(stderr,"\nstruct %s\n", typ->id->name));
      if (typ->ref)
      { fprintf(fd, "struct %s\n{", typ->id->name );
        for (Eptr = ((Table*)typ->ref)->list; Eptr; Eptr = Eptr->next)
	{ fprintf(fd, "\n\t%s", c_storage(Eptr->info.sto));
	  /*if (Eptr->info.typ->type == Tclass && !is_external(Eptr->info.typ) && Eptr->info.typ->generated == False || (Eptr->info.typ->type == Tpointer || Eptr->info.typ->type == Treference) && Eptr->info.typ->ref && ((Tnode*)Eptr->info.typ->ref)->type == Tclass && !is_external(Eptr->info.typ->ref) && ((Tnode*)Eptr->info.typ->ref)->generated == False)
	    fprintf(fd, "class ");
	  */
          fprintf(fd, "%s;", c_type_id(Eptr->info.typ,Eptr->sym->name));
	  if (Eptr->info.sto & Sreturn)
	    fprintf(fd, "\t/* RPC return element */");
	  if (Eptr->info.sto & Sconst)
	    fprintf(fd, "\t/* const field cannot be deserialized */");
	  if (is_external(Eptr->info.typ))
	    fprintf(fd, "\t/* external */");
	  if (is_transient(Eptr->info.typ))
	    fprintf(fd, "\t/* transient */");
	  if (Eptr->info.sto & Sattribute)
	    if (Eptr->info.minOccurs >= 1)
	      fprintf(fd, "\t/* required attribute */");
            else
	      fprintf(fd, "\t/* optional attribute */");
	  if (Eptr->info.sto & SmustUnderstand)
	    fprintf(fd, "\t/* mustUnderstand */");
	  else if (!is_dynamic_array(typ) && is_repetition(Eptr))
          { if (is_external(Eptr->next->info.typ->ref) && ((Tnode*)Eptr->next->info.typ->ref)->type == Tclass || has_external(Eptr->next->info.typ->ref))
            { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containing an external object (declared at line %d)", Eptr->next->sym->name, c_type(Eptr->next->info.typ->ref), c_type(Eptr->next->info.typ->ref), Eptr->next->lineno);
	      semwarn(errbuf);
	    }
            else if (is_volatile(Eptr->next->info.typ->ref) && ((Tnode*)Eptr->next->info.typ->ref)->type == Tclass || has_volatile(Eptr->next->info.typ->ref))
            { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containing a volatile object (declared at line %d)", Eptr->next->sym->name, c_type(Eptr->next->info.typ->ref), c_type(Eptr->next->info.typ->ref), Eptr->next->lineno);
	      semwarn(errbuf);
	    }
	    if (Eptr->info.maxOccurs > 1)
	      fprintf(fd, "\t/* sequence of elements <%s> with minOccurs=%ld and maxOccurs=%ld */", ns_convert(Eptr->next->sym->name), Eptr->info.minOccurs, Eptr->info.maxOccurs);
	    else
	      fprintf(fd, "\t/* sequence of elements <%s> */", ns_convert(Eptr->next->sym->name));
	  }
	  else if (is_anytype(Eptr))
	    fprintf(fd, "\t/* any type of element <%s> (defined below) */", ns_convert(Eptr->next->sym->name));
	  else if (!(Eptr->info.sto & Sconst) && !(Eptr->info.sto & Sattribute) && !is_transient(Eptr->info.typ) && !is_external(Eptr->info.typ) && strncmp(Eptr->sym->name, "__", 2))
	  { if (Eptr->info.maxOccurs > 1)
	      fprintf(fd, "\t/* sequence of elements with minOccurs=%ld and maxOccurs=%ld */", Eptr->info.minOccurs, Eptr->info.maxOccurs);
	    else if (Eptr->info.minOccurs >= 1)
	      fprintf(fd, "\t/* required element */");
            else
	      fprintf(fd, "\t/* optional element */");
	  }
	  if (!is_dynamic_array(typ) && !is_primclass(typ))
	  { if (!strncmp(Eptr->sym->name, "__size", 6))
	    { if (!Eptr->next || Eptr->next->info.typ->type != Tpointer)
              { sprintf(errbuf, "Field '%s' is not followed by a pointer field in struct '%s'", Eptr->sym->name, typ->id->name);
                semwarn(errbuf);
	      }
	    }
	    else if (!strncmp(Eptr->sym->name, "__type", 6))
	    { if (!Eptr->next || (Eptr->next->info.typ->type != Tunion && (Eptr->next->info.typ->type != Tpointer || ((Tnode*)Eptr->next->info.typ->ref)->type != Tvoid)))
              { sprintf(errbuf, "Field '%s' is not followed by a void pointer or union field in struct '%s'", Eptr->sym->name, typ->id->name);
                semwarn(errbuf);
	      }
	    }
	  }
	}
        fprintf(fd, "\n};");
      }
      else if (!is_transient(typ) && !is_external(typ) && !is_volatile(typ))
      { sprintf(errbuf, "struct '%s' is empty", typ->id->name);
        semwarn(errbuf);
      }
    }
  }
  else
  { DBGLOG(fprintf(stderr,"\nclass %s\n", typ->id->name));
    if (typ->ref)
    { int transient = 0;
      fprintf(fd,"class SOAP_CMAC %s", typ->id->name );
      if (typ->base)
        fprintf(fd," : public %s", typ->base->name);
      fprintf(fd,"\n{\npublic:");
      for (Eptr = ((Table*)typ->ref)->list; Eptr; Eptr = Eptr->next)
      { if (transient != Eptr->info.typ->transient)
        { if (Eptr->info.typ->transient > 0 && (Eptr->info.typ->transient & 2))
            fprintf(fd, "\nprivate:");
          else if (Eptr->info.typ->transient > 0 && (Eptr->info.typ->transient & 4))
            fprintf(fd, "\nprotected:");
          else if (Eptr->info.typ->transient >= 0)
            fprintf(fd, "\npublic:");
	  transient = Eptr->info.typ->transient;
	}
        fprintf(fd,"\n\t%s", c_storage(Eptr->info.sto));
	/* if (Eptr->info.typ->type == Tclass && !is_external(Eptr->info.typ) && Eptr->info.typ->generated == False || (Eptr->info.typ->type == Tpointer || Eptr->info.typ->type == Treference) && Eptr->info.typ->ref && ((Tnode*)Eptr->info.typ->ref)->type == Tclass && !is_external(Eptr->info.typ->ref) && ((Tnode*)Eptr->info.typ->ref)->generated == False)
	  fprintf(fd, "class ");
	*/
	fprintf(fd,"%s", c_type_id(Eptr->info.typ,Eptr->sym->name));
	if (Eptr->info.sto & Sconstobj)
	  fprintf(fd, " const");
	if (Eptr->info.sto & Sabstract)
	  fprintf(fd, " = 0;");
	else
	  fprintf(fd, ";");
	if (Eptr->info.sto & Sreturn)
	   fprintf(fd, "\t/* RPC return element */");
	 if (Eptr->info.sto & Sconst)
	   fprintf(fd, "\t/* const field cannot be deserialized */");
	 if (is_external(Eptr->info.typ))
	   fprintf(fd, "\t/* external */");
	 if (is_transient(Eptr->info.typ))
	   fprintf(fd, "\t/* transient */");
	 if (Eptr->info.sto & Sattribute)
	   if (Eptr->info.minOccurs >= 1)
	     fprintf(fd, "\t/* required attribute */");
           else
	     fprintf(fd, "\t/* optional attribute */");
	 if (Eptr->info.sto & SmustUnderstand)
	   fprintf(fd, "\t/* mustUnderstand */");
	 else if (!is_dynamic_array(typ) && is_repetition(Eptr))
         { if (is_external(Eptr->next->info.typ->ref) && ((Tnode*)Eptr->next->info.typ->ref)->type == Tclass || has_external(Eptr->next->info.typ->ref))
           { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containing an external object (declared at line %d)", Eptr->next->sym->name, c_type(Eptr->next->info.typ->ref), c_type(Eptr->next->info.typ->ref), Eptr->next->lineno);
	     semwarn(errbuf);
	   }
           else if (is_volatile(Eptr->next->info.typ->ref) && ((Tnode*)Eptr->next->info.typ->ref)->type == Tclass || has_volatile(Eptr->next->info.typ->ref))
           { sprintf(errbuf, "Field '%s' must be a pointer to an array of pointers to %s to prevent id-ref copying of %s instances containing a volatile object (declared at line %d)", Eptr->next->sym->name, c_type(Eptr->next->info.typ->ref), c_type(Eptr->next->info.typ->ref), Eptr->next->lineno);
	     semwarn(errbuf);
	   }
	   if (Eptr->info.maxOccurs > 1)
	     fprintf(fd, "\t/* sequence of elements <%s> with minOccurs=%ld and maxOccurs=%ld */", ns_convert(Eptr->next->sym->name), Eptr->info.minOccurs, Eptr->info.maxOccurs);
	   else
	     fprintf(fd, "\t/* sequence of elements <%s> */", ns_convert(Eptr->next->sym->name));
	 }
	 else if (is_anytype(Eptr))
	   fprintf(fd, "\t/* any type of element <%s> (defined below) */", ns_convert(Eptr->next->sym->name));
	 else if (!(Eptr->info.sto & Sconst) && !(Eptr->info.sto & Sattribute) && !is_transient(Eptr->info.typ) && !is_external(Eptr->info.typ) && strncmp(Eptr->sym->name, "__", 2))
	 { if (Eptr->info.maxOccurs > 1)
	     fprintf(fd, "\t/* sequence of elements with minOccurs=%ld and maxOccurs=%ld */", Eptr->info.minOccurs, Eptr->info.maxOccurs);
	   else if (Eptr->info.minOccurs >= 1)
	     fprintf(fd, "\t/* required element */");
           else
	     fprintf(fd, "\t/* optional element */");
	 }
	 if (!is_dynamic_array(typ) && !is_primclass(typ))
	 { if (!strncmp(Eptr->sym->name, "__size", 6))
	   { if (!Eptr->next || Eptr->next->info.typ->type != Tpointer)
             { sprintf(errbuf, "Field '%s' is not followed by a pointer field in struct '%s'", Eptr->sym->name, typ->id->name);
               semwarn(errbuf);
	     }
	   }
	   else if (!strncmp(Eptr->sym->name, "__type", 6))
	   { if (!Eptr->next || (Eptr->next->info.typ->type != Tunion && (Eptr->next->info.typ->type != Tpointer || ((Tnode*)Eptr->next->info.typ->ref)->type != Tvoid)))
             { sprintf(errbuf, "Field '%s' is not followed by a void pointer or union field in struct '%s'", Eptr->sym->name, typ->id->name);
               semwarn(errbuf);
	     }
	   }
	 }
      }
      if (!is_transient(typ) && !is_volatile(typ))
      { fprintf(fd,"\npublic:\n\tvirtual int soap_type() const { return %d; } /* = unique id SOAP_TYPE_%s */", typ->num, typ->id->name);
        fprintf(fd,"\n\tvirtual void soap_default(struct soap*); ");
        fprintf(fd,"\n\tvirtual void soap_serialize(struct soap*) const;");
        fprintf(fd,"\n\tvirtual void soap_mark(struct soap*) const;");
        fprintf(fd,"\n\tvirtual int soap_put(struct soap*, const char*, const char*) const;");
        fprintf(fd,"\n\tvirtual int soap_out(struct soap*, const char*, int, const char*) const;");
        fprintf(fd,"\n\tvirtual void *soap_get(struct soap*, const char*, const char*);");
        fprintf(fd,"\n\tvirtual void *soap_in(struct soap*, const char*, const char*); ");
      }
      fprintf(fd,"\n};");
    }
    else if (!is_transient(typ) && !is_external(typ) && !is_volatile(typ))
    { sprintf(errbuf, "class '%s' is empty", typ->id->name);
      semwarn(errbuf);
    }
  }
  if (is_volatile(typ))
    fprintf(fd, "\n#endif");
  /*
  if (typ->ref)
    fprintf(fd, "\n#endif");
  */
  fflush(fd);
}

void
gen_union(FILE *fd, Tnode *typ)
{ Entry *Eptr;
  typ->generated = True;
  { { DBGLOG(fprintf(stderr,"\nunion %s\n", typ->id->name));
      if (typ->ref)
      { fprintf(fd, "\n\nunion %s\n{", typ->id->name );
        for (Eptr = ((Table*)typ->ref)->list; Eptr; Eptr = Eptr->next)
        { fprintf(fd, "\n\t%s", c_storage(Eptr->info.sto));
          fprintf(fd, "%s;", c_type_id(Eptr->info.typ,Eptr->sym->name));
	  if (Eptr->info.sto & Sconst)
	    fprintf(fd, "\t/* const field cannot be deserialized */");
	  if (is_external(Eptr->info.typ))
	    fprintf(fd, "\t/* external */");
	  if (is_transient(Eptr->info.typ))
	    fprintf(fd, "\t/* transient */");
	  if (Eptr->info.sto & Sattribute)
	    fprintf(fd, "\t/* attribute */");
	  if (Eptr->info.sto & SmustUnderstand)
	    fprintf(fd, "\t/* mustUnderstand */");
	}
        fprintf(fd, "\n};");
      }
      else
      { sprintf(errbuf, "union '%s' is empty", typ->id->name);
        semwarn(errbuf);
      }
    }
  }
  fflush(fd);
}

void
generate_header(Table *t)
{ Entry *p, *q;
  fprintf(fheader, "\n\n/* Enumerations */");
  fflush(fheader);
  if (enumtable)
    for (p = enumtable->list; p; p = p->next)
    { if (is_imported(p->info.typ))
        continue;
      fprintf(fheader, "\n");
      if (is_mask(p->info.typ))
        fprintf(fheader, "\n/* Bitmask: */");
      fprintf(fheader, "\n#ifndef _SOAP_%s\n#define _SOAP_%s", c_ident(p->info.typ), c_ident(p->info.typ));
      fprintf(fheader, "\nenum %s {", p->info.typ->id->name);
      if ((Table*)p->info.typ->ref)
      { q = ((Table*)p->info.typ->ref)->list;
        if (q)
        { fprintf(fheader, "%s = "SOAP_LONG_FORMAT, q->sym->name, q->info.val.i);
          for (q = q->next; q; q = q->next)
            fprintf(fheader, ", %s = "SOAP_LONG_FORMAT, q->sym->name, q->info.val.i);
        }
      }
      fprintf(fheader, "};\n#endif");
    }
  fprintf(fheader, "\n\n/* Classes and Structs */");
  fflush(fheader);
  if (uniontable)
    for (p = uniontable->list; p; p = p->next)
      if (!is_imported(p->info.typ))
        gen_union(fheader, p->info.typ);
  if (classtable)
    for (p = classtable->list; p; p = p->next)
      if (!is_imported(p->info.typ))
        gen_class(fheader, p->info.typ);
  fprintf(fheader, "\n\n/* Types With Custom (De)serializers: */");
  fflush(fheader);
  if (typetable)
    for (p = typetable->list; p; p = p->next)
    { if (is_external(p->info.typ) && !is_volatile(p->info.typ) && !is_imported(p->info.typ))
        fprintf(fheader, "\n%s%s;", c_storage(p->info.sto), c_type_id(p->info.typ, p->sym->name));
    }
  fprintf(fheader, "\n\n/* Typedefs */");
  fflush(fheader);
  if (typetable)
    for (p = typetable->list; p; p = p->next)
    { if (!is_primitive_or_string(p->info.typ) && !is_external(p->info.typ) && !is_XML(p->info.typ) && !is_transient(p->info.typ) && !has_ns_t(p->info.typ) && !is_imported(p->info.typ))
      { sprintf(errbuf, "typedef '%s' is not namespace qualified: schema definition for '%s' in WSDL file output may be invalid", p->sym->name, p->sym->name);
        semwarn(errbuf);
      }
      if (!is_external(p->info.typ) && !is_imported(p->info.typ))
        fprintf(fheader,"\n%s%s;", c_storage(p->info.sto), c_type_id(p->info.typ, p->sym->name));
    }
  fprintf(fheader, "\n\n/* Extern */");
  fflush(fheader);
  if (t)
    for (p = t->list; p; p = p->next)
      if (p->info.typ->type != Tfun || p->info.sto & Sextern)
      { fprintf(fheader,"\n\nextern %s", c_storage(p->info.sto));
        fprintf(fheader,"%s;", c_type_id(p->info.typ, p->sym->name));
      }
  fflush(fheader);
}

void
get_namespace_prefixes()
{ Symbol *p, *q;
  int i, n;
  char *s, buf[256];
  if (nslist)
    return;
  for (p = symlist; p; p = p->next)
  { if (*p->name != '~')
    { s = p->name;
      while (*s == '_')
	s++;
      n = strlen(s) - 2;
      for (i = 1; i < n; i++)
      { if (s[i] == '_' && s[i+1] == '_' && s[i+2] && s[i+2] != '_')
        { strncpy(buf, s, i);
          buf[i] = '\0';
	  if (!strcmp(buf, "SOAP_ENV") || !strcmp(buf, "SOAP_ENC") || !strcmp(buf, "xsd") || !strcmp(buf, "xsi") || !strcmp(buf, "xml") || !strncmp(buf, "soap_", 5))
	    goto nsnext;
          for (q = nslist; q; q = q->next)
            if (!strcmp(q->name, buf))
              goto nsnext;
          q = (Symbol*)emalloc(sizeof(Symbol));
          q->name = (char*)emalloc(i+1);
	  strcpy(q->name, buf);
	  q->name[i] = '\0';
	  q->next = nslist;
	  nslist = q;
        }
      }
    }
nsnext:
    ;
  }
  q = (Symbol*)emalloc(sizeof(Symbol));
  q->name = "xsd";
  q->next = nslist;
  nslist = q;
  q = (Symbol*)emalloc(sizeof(Symbol));
  q->name = "xsi";
  q->next = nslist;
  nslist = q;
  q = (Symbol*)emalloc(sizeof(Symbol));
  q->name = "SOAP-ENC";
  q->next = nslist;
  nslist = q;
  q = (Symbol*)emalloc(sizeof(Symbol));
  q->name = "SOAP-ENV";
  q->next = nslist;
  nslist = q;
}

void
generate_schema(Table *t)
{ Entry *p;
  Symbol *ns, *ns1;
  char *name = NULL;
  char *URL = NULL;
  char *executable = NULL;
  char *URI = NULL;
  char *style = NULL;
  char *encoding = NULL;
  char *import = NULL;
  Service *sp;
  char buf[1024];
  FILE *fd;
  get_namespace_prefixes();
  for (ns = nslist; ns; ns = ns->next)
  { if (!strcmp(ns->name, "SOAP-ENV") || !strcmp(ns->name, "SOAP-ENC") || !strcmp(ns->name, "xsi") || !strcmp(ns->name, "xsd"))
      continue;
    name = NULL;
    URL = NULL;
    executable = NULL;
    URI = NULL;
    style = NULL;
    encoding = NULL;
    import = NULL;
    for (sp = services; sp; sp = sp->next)
    { if (!strcmp(sp->ns, ns->name))
	{	name = sp->name;
		URL = sp->URL;
		executable = sp->executable;
		URI = sp->URI;
		style = sp->style;
		encoding = sp->encoding;
		import = sp->import;
		break;
  	}
    }
    if (!URI)
      URI = tmpURI;
    if (is_document(style) && encoding && !*encoding)
    {	semwarn("Cannot use document style with SOAP encoding");
    	encoding = NULL;
    }
    if (!name)
  	name = "Service";
    if (!URL)
  	URL = "http://localhost:80";
    /*
    if (!executable)
    { executable = emalloc(strlen(name)+5);
      strcpy(executable, name);
      strcat(executable, ".cgi");
    }
    */
    if (t)
      for (p = t->list; p; p = p->next)
      { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns->name, p->sym->name))
	{ if (name)
	    fprintf(stderr, "Using %s service name: %s\n", ns->name, name);
	  if (style)
	    fprintf(stderr, "Using %s service style: %s\n", ns->name, style);
	  else if (!eflag)
	    fprintf(stderr, "Using %s service style: document\n", ns->name);
	  if (encoding && *encoding)
	    fprintf(stderr, "Using %s service encoding: %s\n", ns->name, encoding);
	  else if (encoding && !*encoding)
	    fprintf(stderr, "Using %s service encoding: encoded\n", ns->name);
	  else if (!eflag)
	    fprintf(stderr, "Using %s service encoding: literal\n", ns->name);
	  if (URL)
	    fprintf(stderr, "Using %s service location: %s\n", ns->name, URL);
	  if (executable)
	    fprintf(stderr, "Using %s service executable: %s\n", ns->name, executable);
	  if (import)
	    fprintf(stderr, "Using %s schema import: %s\n", ns->name, import);
	  else if (URI)
	    fprintf(stderr, "Using %s schema namespace: %s\n", ns->name, URI);
          if (sp && sp->name)
	    sprintf(buf, "%s%s.wsdl", dirpath, name);
	  else
	    sprintf(buf, "%s%s.wsdl", dirpath, ns_convert(ns->name));
	  if (!wflag)
	  { fprintf(stderr, "Saving %s Web Service description\n", buf);
            fd = fopen(buf, "w");
	    if (!fd)
	      execerror("Cannot write WSDL file");
            gen_wsdl(fd, t, ns->name, name, URL, executable, URI, style, encoding);
            fclose(fd);
	  }
          if (sp && sp->name)
	  { sprintf(buf, "%s%s%sProxy.h", dirpath, prefix, name);
	    fprintf(stderr, "Saving %s client proxy\n", buf);
            fd = fopen(buf, "w");
	    if (!fd)
	      execerror("Cannot write proxy file");
	    sprintf(buf, "%s%sProxy.h", prefix, name);
	    copyrightnote(fd, buf);
            gen_proxy(fd, t, ns, name, URL, executable, URI, encoding);
            fclose(fd);
	  }
	  else
	  { sprintf(buf, "%s%sProxy.h", dirpath, prefix);
	    fprintf(stderr, "Saving %s client proxy\n", buf);
            fd = fopen(buf, "w");
	    if (!fd)
	      execerror("Cannot write proxy file");
	    sprintf(buf, "%sProxy.h", prefix);
	    copyrightnote(fd, buf);
            gen_proxy(fd, t, ns, "Service", URL, executable, URI, encoding);
            fclose(fd);
	  }
          if (sp && sp->name)
	  { sprintf(buf, "%s%s%sObject.h", dirpath, prefix, name);
	    fprintf(stderr, "Saving %s server object\n", buf);
            fd = fopen(buf, "w");
	    if (!fd)
	      execerror("Cannot write server object file");
	    sprintf(buf, "%s%sObject.h", prefix, name);
	    copyrightnote(fd, buf);
            gen_object(fd, t, ns, name, URL, executable, URI, encoding);
            fclose(fd);
	  }
	  else
	  { sprintf(buf, "%s%sObject.h", dirpath, prefix);
	    fprintf(stderr, "Saving %s server object\n", buf);
            fd = fopen(buf, "w");
	    if (!fd)
	      execerror("Cannot write server object file");
	    sprintf(buf, "%sObject.h", prefix);
	    copyrightnote(fd, buf);
            gen_object(fd, t, ns, "Service", URL, executable, URI, encoding);
            fclose(fd);
	  }
	  strcpy(buf, dirpath);
          if (sp && sp->name)
	    strcat(buf, name);
	  else
	    strcat(buf, ns_convert(ns->name));
	  strcat(buf, ".");
          gen_data(buf, t, ns->name, name, URL, executable, URI, encoding);
	  if (nflag)
	    sprintf(buf, "%s%s.nsmap", dirpath, prefix, ns_convert(ns->name));
          else if (sp && sp->name)
	    sprintf(buf, "%s%s.nsmap", dirpath, name);
          else
	    sprintf(buf, "%s%s.nsmap", dirpath, ns_convert(ns->name));
	  fprintf(stderr, "Saving %s namespace mapping table\n", buf);
          fd = fopen(buf, "w");
	  if (!fd)
	    execerror("Cannot write nsmap file");
	  fprintf(fd, "\n#include \"%sH.h\"", prefix);
 	  if (nflag)
	    fprintf(fd, "\nSOAP_NMAC struct Namespace %s_namespaces[] =\n", prefix);
 	  else
 	    fprintf(fd, "\nSOAP_NMAC struct Namespace namespaces[] =\n"); 
	  gen_nsmap(fd, ns, URI);
          fclose(fd);
	  break;
        }
      }
    if (!wflag && !import)
    { sprintf(buf, "%s%s.xsd", dirpath, ns_convert(ns->name));
      fprintf(stderr, "Saving %s XML schema\n", buf);
      fd = fopen(buf, "w");
      if (!fd)
        execerror("Cannot write schema file");
      fprintf(fd, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!--\n");
      copyrightnote(fd, ns->name);
      fprintf(fd, "-->\n");
      if (t)
        for (p = t->list; p; p = p->next)
          if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns->name, p->sym->name))
          { gen_schema(fd, t, ns->name, ns->name, 0, 1, URL, URI, style, encoding);
	    break;
          }
      if (!t || !p)
        gen_schema(fd, t, ns->name, ns->name, 0, 0, URL, URI, style, encoding);
      fclose(fd);
    }
  }
}

int
chkhdr(char *part)
{ Entry *p;
  p = entry(classtable, lookup("SOAP_ENV__Header"));
  for (p = ((Table*)p->info.typ->ref)->list; p; p = p->next)
    if (has_ns_eq(NULL, p->sym->name) && (!strcmp(part, p->sym->name) || is_eq_nons(part, p->sym->name)))
      return 1;
  sprintf(errbuf, "Cannot define method-header-part in WSDL: SOAP_ENV__Header \"%s\" field is not qualified", part);
  semwarn(errbuf);
  return 0;
}      

void
gen_wsdl(FILE *fd, Table *t, char *ns, char *name, char *URL, char *executable, char *URI, char *style, char *encoding)
{ Entry *p, *q;
  Symbol *s;
  Service *sp, *sp2;
  Method *m;
  char *action, *comment, *method_style = NULL, *method_encoding = NULL, *method_response_encoding = NULL;
  fprintf(fd, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
  fprintf(fd, "<definitions name=\"%s\"\n", name);
  for (sp = services; sp; sp = sp->next)
    if (!strcmp(sp->ns, ns))
      break;
  if (sp && sp->WSDL)
    fprintf(fd, " targetNamespace=\"%s\"\n xmlns:tns=\"%s\"", sp->WSDL, sp->WSDL);
  else
    fprintf(fd, " targetNamespace=\"%s/%s.wsdl\"\n xmlns:tns=\"%s/%s.wsdl\"", URL, name, URL, name);
  for (s = nslist; s; s = s->next)
  { for (sp2 = services; sp2; sp2 = sp2->next)
      if (!strcmp(sp2->ns, s->name) && sp2->URI)
        break;
    if (sp2)
      fprintf(fd, "\n xmlns:%s=\"%s\"", ns_convert(s->name), sp2->URI);
    else if (!strcmp(s->name, "SOAP-ENV"))
      fprintf(fd, "\n xmlns:SOAP-ENV=\"%s\"", envURI);
    else if (!strcmp(s->name, "SOAP-ENC"))
      fprintf(fd, "\n xmlns:SOAP-ENC=\"%s\"", encURI);
    else if (!strcmp(s->name, "xsi"))
      fprintf(fd, "\n xmlns:xsi=\"%s\"", xsiURI);
    else if (!strcmp(s->name, "xsd"))
      fprintf(fd, "\n xmlns:xsd=\"%s\"", xsdURI);
    else
      fprintf(fd, "\n xmlns:%s=\"%s/%s.xsd\"", ns_convert(s->name), URI, ns_convert(s->name));
  }
  if (vflag != 2)
    fprintf(fd, "\n xmlns:SOAP=\"http://schemas.xmlsoap.org/wsdl/soap/\"");
  else
    fprintf(fd, "\n xmlns:SOAP=\"http://schemas.xmlsoap.org/wsdl/soap12/\"");
  fprintf(fd, "\n xmlns:MIME=\"http://schemas.xmlsoap.org/wsdl/mime/\"");
  fprintf(fd, "\n xmlns:DIME=\"http://schemas.xmlsoap.org/ws/2002/04/dime/wsdl/\"");
  fprintf(fd, "\n xmlns:WSDL=\"http://schemas.xmlsoap.org/wsdl/\"");
  fprintf(fd, "\n xmlns=\"http://schemas.xmlsoap.org/wsdl/\">\n\n");
  fprintf(fd, "<types>\n\n");
  for (s = nslist; s; s = s->next)
    gen_schema(fd, t, ns, s->name, !strcmp(s->name, ns), 1, URL, URI, style, encoding);
  fprintf(fd, "</types>\n\n");
  fflush(fd);
  if (t)
  { for (p = t->list; p; p = p->next)
    { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns, p->sym->name))
      { comment = NULL;
        method_style = style;
	method_encoding = encoding;
	method_response_encoding = NULL;
	if (sp)
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name))
                if (m->mess == ENCODING)
	          method_encoding = m->part;
                else if (m->mess == RESPONSE_ENCODING)
	          method_response_encoding = m->part;
                else if (m->mess == STYLE)
	          method_style = m->part;
	        else if (m->mess == COMMENT)
	          comment = m->part;
        if (!method_response_encoding)
          method_response_encoding = method_encoding;
        fprintf(fd, "<message name=\"%sRequest\">\n", ns_remove(p->sym->name));
        fflush(fd);
	if (is_document(method_style))
	{ if (is_invisible(p->sym->name))
	  { q = entry(classtable, p->sym);
	    if (q)
	    { q = ((Table*)q->info.typ->ref)->list;
	      if (q)
	        fprintf(fd, " <part name=\"parameters\" element=\"%s\"/>\n", ns_add(q->sym->name, ns));
	    }
	  }
	  else
	    fprintf(fd, " <part name=\"parameters\" element=\"%s\"/>\n", ns_add(p->sym->name, ns));
	}
	else
  	{ q = entry(classtable, p->sym);
  	  if (q)
	    for (q = ((Table*)q->info.typ->ref)->list; q; q = q->next)
	    { if (!is_transient(q->info.typ) && !(q->info.sto & Sattribute) && q->info.typ->type != Tfun && !is_repetition(q) && !is_anytype(q))
  	      { if (is_literal(method_encoding))
	          fprintf(fd, " <part name=\"%s\" element=\"%s\"/>\n", ns_remove(q->sym->name), ns_add(q->sym->name, ns));
	        else if (is_XML(q->info.typ))
	          fprintf(fd, " <part name=\"parameters\" type=\"xsd:anyType\"/>\n");
	        else
	          fprintf(fd, " <part name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns));
	      }
	    }
	}
        fprintf(fd, "</message>\n\n");
        fflush(fd);
	q = (Entry*)p->info.typ->ref;
	if (q && is_transient(q->info.typ))
	  ;
	else if (q && !is_response(q->info.typ))
	{ if (is_document(method_style))
	    fprintf(fd, "<message name=\"%sResponse\">\n <part name=\"parameters\" element=\"%sResponse\"/>\n</message>\n\n", ns_remove(p->sym->name), ns_add(p->sym->name, ns));
  	  else if (is_literal(method_response_encoding))
	    fprintf(fd, "<message name=\"%sResponse\">\n <part name=\"%s\" element=\"%s\"/>\n</message>\n\n", ns_remove(p->sym->name), ns_remove(q->sym->name), ns_add(q->sym->name, ns));
	  else if (is_XML(q->info.typ->ref))
	    fprintf(fd, "<message name=\"%sResponse\">\n <part name=\"parameters\" type=\"xsd:anyType\"/>\n</message>\n\n", ns_remove(p->sym->name));
	  else
	    fprintf(fd, "<message name=\"%sResponse\">\n <part name=\"%s\" type=\"%s\"/>\n</message>\n\n", ns_remove(p->sym->name), ns_remove(q->sym->name), wsdl_type(q->info.typ, ns));
	}
        else if (q && q->info.typ->wsdl == False)
	{ q->info.typ->wsdl = True;
	  fprintf(fd, "<message name=\"%s\">\n", ns_remove(((Tnode*)q->info.typ->ref)->id->name));
	  if (is_document(method_style))
	  { if (has_ns_eq(NULL, ((Entry*)p->info.typ->ref)->sym->name))
	      fprintf(fd, " <part name=\"parameters\" element=\"%s\"/>\n", ns_convert(((Entry*)p->info.typ->ref)->sym->name));
            else
	      fprintf(fd, " <part name=\"parameters\" element=\"%s\"/>\n", ns_convert(((Tnode*)q->info.typ->ref)->id->name));
	  }
	  else
	  { if (((Tnode*)q->info.typ->ref)->ref)
	    { for (q = ((Table*)((Tnode*)q->info.typ->ref)->ref)->list; q; q = q->next)
	      { if (!is_transient(q->info.typ) && !(q->info.sto & Sattribute) && q->info.typ->type != Tfun && !is_repetition(q) && !is_anytype(q))
  	          if (is_literal(method_response_encoding))
	            fprintf(fd, " <part name=\"%s\" element=\"%s\"/>\n", ns_remove(q->sym->name), ns_add(q->sym->name, ns));
	          else if (is_XML(q->info.typ))
	            fprintf(fd, " <part name=\"parameters\" type=\"xsd:anyType\"/>\n");
	          else
	            fprintf(fd, " <part name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns));
	      }
	    }
	  }
          fprintf(fd, "</message>\n\n");
	}
        fflush(fd);
      }
    }
    if (custom_header)
    { Table *r;
      fprintf(fd, "<message name=\"%sHeader\">\n", name);
      r = entry(classtable, lookup("SOAP_ENV__Header"))->info.typ->ref;
      if (r)
        for (q = r->list; q; q = q->next)
	  if (!is_transient(q->info.typ) && !(q->info.sto & Sattribute) && q->info.typ->type != Tfun && !is_repetition(q) && !is_anytype(q))
	    fprintf(fd, " <part name=\"%s\" element=\"%s\"/>\n", ns_remove(q->sym->name), ns_add(q->sym->name, ns));
      fprintf(fd, "</message>\n\n");
    }
    if (custom_fault)
    { Table *r;
      fprintf(fd, "<message name=\"%sFault\">\n", name);
      r = entry(classtable, lookup("SOAP_ENV__Detail"))->info.typ->ref;
      if (r)
        for (q = r->list; q; q = q->next)
	  if (!is_transient(q->info.typ) && !is_repetition(q) && !is_anytype(q) && !(q->info.sto & Sattribute) && q->info.typ->type != Tfun && has_ns_eq(NULL, q->sym->name))
	    fprintf(fd, " <part name=\"%s\" element=\"%s\"/>\n", ns_remove(q->sym->name), ns_add(q->sym->name, ns));
      fprintf(fd, "</message>\n\n");
    }
    if (sp)
      for (m = sp->list; m; m = m->next)
	if (m->mess&FAULT)
	{ if (typetable)
            for (p = typetable->list; p; p = p->next)
	      if ((m->mess&FAULT) && is_eq(m->part, p->info.typ->sym->name))
	        break;
	  if (!p && classtable) 
            for (p = classtable->list; p; p = p->next)
	      if ((m->mess&FAULT) && is_eq(m->part, p->info.typ->id->name))
	        break;
          if (p)
          { fprintf(fd, "<message name=\"%sFault\">\n", ns_remove(m->part));
            fprintf(fd, " <part name=\"fault\" element=\"%s\"/>\n", ns_convert(m->part));
            fprintf(fd, "</message>\n\n");
	  }
	  else
          { sprintf(errbuf, "//gsoap %s method-fault %s %s directive does not refer to class or typedef", sp->ns, m->name, m->part);
            semwarn(errbuf);
	  }
        }
    fflush(fd);
    if (sp && sp->port)
      fprintf(fd, "<portType name=\"%s\">\n", sp->port);
    else
      fprintf(fd, "<portType name=\"%sPortType\">\n", name);
    for (p = t->list; p; p = p->next)
    { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns, p->sym->name))
      { comment = NULL;
	if (sp)
	  for (m = sp->list; m; m = m->next)
	    if (m->mess == COMMENT && is_eq_nons(m->name, p->sym->name))
	      comment = m->part;
        fprintf(fd, " <operation name=\"%s\">\n", ns_remove(p->sym->name));
        if (comment)
          fprintf(fd, "  <documentation>%s</documentation>\n", comment);
        else
          fprintf(fd, "  <documentation>Service definition of function %s</documentation>\n", p->sym->name);
        fprintf(fd, "  <input message=\"tns:%sRequest\"/>\n", ns_remove(p->sym->name));
	q = (Entry*)p->info.typ->ref;
	if (q && is_transient(q->info.typ))
	  ;
	else if (q && !is_response(q->info.typ))
	  fprintf(fd, "  <output message=\"tns:%sResponse\"/>\n", ns_remove(p->sym->name));
        else if (q)
	  fprintf(fd, "  <output message=\"tns:%s\"/>\n", ns_remove(((Tnode*)q->info.typ->ref)->id->name));
	if (sp)
	  for (m = sp->list; m; m = m->next)
	    if ((m->mess&FAULT) && is_eq(m->name, p->sym->name))
	      fprintf(fd, "  <fault name=\"%s\" message=\"tns:%sFault\"/>\n", ns_remove(m->part), ns_remove(m->part));
        fprintf(fd, " </operation>\n");
      }
    }
    fprintf(fd, "</portType>\n\n");
    if (is_document(style))
      if (sp && sp->port)
        fprintf(fd, "<binding name=\"%s\" type=\"tns:%s\">\n <SOAP:binding style=\"document\" transport=\"http://schemas.xmlsoap.org/soap/http\"/>\n", name, sp->port);
      else
        fprintf(fd, "<binding name=\"%s\" type=\"tns:%sPortType\">\n <SOAP:binding style=\"document\" transport=\"http://schemas.xmlsoap.org/soap/http\"/>\n", name, name);
    else
      if (sp && sp->port)
        fprintf(fd, "<binding name=\"%s\" type=\"tns:%s\">\n <SOAP:binding style=\"rpc\" transport=\"http://schemas.xmlsoap.org/soap/http\"/>\n", name, sp->port);
      else
        fprintf(fd, "<binding name=\"%s\" type=\"tns:%sPortType\">\n <SOAP:binding style=\"rpc\" transport=\"http://schemas.xmlsoap.org/soap/http\"/>\n", name, name);
    fflush(fd);
    for (p = t->list; p; p = p->next)
    { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns, p->sym->name))
      { action = "";
          method_style = style;
	  method_encoding = encoding;
	  method_response_encoding = NULL;
	  if (sp)
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name))
                if (m->mess == ENCODING)
	          method_encoding = m->part;
                else if (m->mess == RESPONSE_ENCODING)
	          method_response_encoding = m->part;
                else if (m->mess == STYLE)
	          method_style = m->part;
	        else if (m->mess == ACTION)
	          action = m->part;
	if (!method_response_encoding)
	  method_response_encoding = method_encoding;
        fprintf(fd, " <operation name=\"%s\">\n", ns_remove(p->sym->name));
        if (is_document(style))
	{ if (is_document(method_style))
          { if (vflag == 2)
	      fprintf(fd, "  <SOAP:operation/>\n");
	    else if (*action == '"')
	      fprintf(fd, "  <SOAP:operation soapAction=%s/>\n", action);
            else
	      fprintf(fd, "  <SOAP:operation soapAction=\"%s\"/>\n", action);
	  }
	  else if (vflag == 2)
            fprintf(fd, "  <SOAP:operation style=\"rpc\"/>\n");
	  else if (*action == '"')
            fprintf(fd, "  <SOAP:operation style=\"rpc\" soapAction=%s/>\n", action);
          else
            fprintf(fd, "  <SOAP:operation style=\"rpc\" soapAction=\"%s\"/>\n", action);
	}
        else
	{ if (is_document(method_style))
          { if (vflag == 2)
	      fprintf(fd, "  <SOAP:operation style=\"document\"/>\n");
	    else if (*action == '"')
	      fprintf(fd, "  <SOAP:operation style=\"document\" soapAction=%s/>\n", action);
            else
	      fprintf(fd, "  <SOAP:operation style=\"document\" soapAction=\"%s\"/>\n", action);
	  }
	  else if (vflag == 2)
            fprintf(fd, "  <SOAP:operation/>\n");
	  else if (*action == '"')
            fprintf(fd, "  <SOAP:operation soapAction=%s/>\n", action);
          else
            fprintf(fd, "  <SOAP:operation soapAction=\"%s\"/>\n", action);
	}
  	q = entry(classtable, p->sym);
  	if (is_literal(method_encoding) || q && (q = (((Table*)q->info.typ->ref)->list)) && q && is_XML(q->info.typ))
	{ if (is_document(method_style))
	    fprintf(fd, "  <input>\n   <SOAP:body use=\"literal\"/>\n");
          else
	    fprintf(fd, "  <input>\n   <SOAP:body use=\"literal\" namespace=\"%s\"/>\n", URI);
	}
        else
	{ if (encoding && *encoding)
	    fprintf(fd, "  <input>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, encoding);
          else if (method_encoding && *method_encoding)
	    fprintf(fd, "  <input>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, method_encoding);
          else
	    fprintf(fd, "  <input>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, encURI);
          if (!eflag)
          { sprintf(errbuf, "operation '%s' is not compliant with WS-I Basic Profile 1.0a, reason: uses SOAP encoding", p->sym->name);
            compliancewarn(errbuf);
	  }
	}
	if (custom_header)
	{ int f = 0;
	  m = NULL;
	  if (sp)
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name) && (m->mess&HDRIN))
	      { f = 1;
	        if (chkhdr(m->part))
	          fprintf(fd, "   <SOAP:header use=\"literal\" message=\"tns:%sHeader\" part=\"%s\"/>\n", name, ns_remove(m->part));
	      }
	}
	fprintf(fd, "  </input>\n");
	q = (Entry*)p->info.typ->ref;
	if (!q || !q->info.typ->ref)
	{ fprintf(fd, " </operation>\n");
	  continue;
	}
	if (is_literal(method_response_encoding) || is_XML(q->info.typ->ref))
	{ if (is_document(method_style))
	    fprintf(fd, "  <output>\n   <SOAP:body use=\"literal\"/>\n");
          else
	    fprintf(fd, "  <output>\n   <SOAP:body use=\"literal\" namespace=\"%s\"/>\n", URI);
	}
	else if (encoding && *encoding)
	  fprintf(fd, "  <output>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, encoding);
	else if (method_response_encoding && *method_response_encoding)
	  fprintf(fd, "  <output>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, method_response_encoding);
	else
	  fprintf(fd, "  <output>\n   <SOAP:body use=\"encoded\" namespace=\"%s\" encodingStyle=\"%s\"/>\n", URI, encURI);
	if (custom_header)
	{ Method *m = NULL;
	  int f = 0;
	  if (sp)
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name) && (m->mess&HDROUT))
	      { f = 1;
	        if (chkhdr(m->part))
	          fprintf(fd, "   <SOAP:header use=\"literal\" message=\"tns:%sHeader\" part=\"%s\"/>\n", name, ns_remove(m->part));
	      }
	}
	fprintf(fd, "  </output>\n");
	if (sp)
	  for (m = sp->list; m; m = m->next)
	    if ((m->mess&FAULT) && is_eq(m->name, p->sym->name))
	      fprintf(fd, "  <fault name=\"%s\">\n   <SOAP:fault name=\"%s\" use=\"literal\"/>\n  </fault>\n", ns_remove(m->part), ns_remove(m->part));
	fprintf(fd, " </operation>\n");
        fflush(fd);
      }
    }
    fprintf(fd, "</binding>\n\n");
  }
  fprintf(fd, "<service name=\"%s\">\n", name);
  if (sp && sp->documentation)
    fprintf(fd, " <documentation>%s</documentation>\n", sp->documentation);
  else
    fprintf(fd, " <documentation>gSOAP "VERSION" generated service definition</documentation>\n");
  if (executable)
    fprintf(fd, " <port name=\"%s\" binding=\"tns:%s\">\n  <SOAP:address location=\"%s/%s\"/>\n </port>\n</service>\n\n</definitions>\n", name, name, URL, executable);
  else
    fprintf(fd, " <port name=\"%s\" binding=\"tns:%s\">\n  <SOAP:address location=\"%s\"/>\n </port>\n</service>\n\n</definitions>\n", name, name, URL);
}

char *
default_value(Entry *e, const char *a)
{ Entry *q;
  static char buf[1024];
  buf[0] = '\0';
  if (e->info.hasval)
    switch (e->info.typ->type)
    { case Tchar:
      case Twchar:
      case Tuchar:
      case Tshort:
      case Tushort:
      case Tint:
      case Tuint:
      case Tlong:
      case Tllong:
      case Tulong:
      case Tullong:
        sprintf(buf, " %s=\""SOAP_LONG_FORMAT"\"", a, e->info.val.i);
	break;
      case Tfloat:
      case Tdouble:
        sprintf(buf, " %s=\"%f\"", a, e->info.val.r);
	break;
      case Ttime:
        break; /* should get value? */
      case Tenum:
	for (q = ((Table*)e->info.typ->ref)->list; q; q = q->next)
	  if (q->info.val.i == e->info.val.i)
	  { sprintf(buf, " %s=\"%s\"", a, ns_convert(q->sym->name));
	    break;
	  }
        break;
      default:
	if (e->info.val.s && strlen(e->info.val.s) < sizeof(buf)-12)
          sprintf(buf, " %s=\"%s\"", a, e->info.val.s);
	break;
    }
  return buf;
}

const char *nillable(long minOccurs)
{ if (minOccurs)
    return "false";
  return "true";
}

void
gen_schema(FILE *fd, Table *t, char *ns1, char *ns, int all, int wsdl, char *URL, char *URI, char *style, char *encoding)
{ int i, d;
  char cbuf[4];
  Entry *p, *q, *r;
  Tnode *n;
  Symbol *s;
  Service *sp, *sp2;
  Method *m;
  int flag;
  if (!strcmp(ns, "SOAP-ENV") || !strcmp(ns, "SOAP-ENC") || !strcmp(ns, "xsi") || !strcmp(ns, "xsd"))
    return;
  for (sp = services; sp; sp = sp->next)
    if (!strcmp(sp->ns, ns) && sp->URI)
      break;
  if (sp && sp->import)
     return;
  fprintf(fd, " <schema ");
  if (sp)
    fprintf(fd, "targetNamespace=\"%s\"", sp->URI);
  else
    fprintf(fd, "targetNamespace=\"%s/%s.xsd\"", URI, ns_convert(ns));
  for (s = nslist; s; s = s->next)
  { for (sp = services; sp; sp = sp->next)
      if (!strcmp(sp->ns, s->name) && sp->URI)
        break;
    if (sp)
      fprintf(fd, "\n  xmlns:%s=\"%s\"", ns_convert(s->name), sp->URI);
    else if (!strcmp(s->name, "SOAP-ENV"))
      fprintf(fd, "\n  xmlns:SOAP-ENV=\"%s\"", envURI);
    else if (!strcmp(s->name, "SOAP-ENC"))
      fprintf(fd, "\n  xmlns:SOAP-ENC=\"%s\"", encURI);
    else if (!strcmp(s->name, "xsi"))
      fprintf(fd, "\n  xmlns:xsi=\"%s\"", xsiURI);
    else if (!strcmp(s->name, "xsd"))
      fprintf(fd, "\n  xmlns:xsd=\"%s\"", xsdURI);
    else
      fprintf(fd, "\n  xmlns:%s=\"%s/%s.xsd\"", ns_convert(s->name), URI, ns_convert(s->name));
  }
  fprintf(fd, "\n  xmlns=\"%s\"\n", xsdURI);
  if (sp && (sp->elementForm || sp->attributeForm))
    fprintf(fd, "  elementFormDefault=\"%s\"\n  attributeFormDefault=\"%s\">\n", sp->elementForm?sp->elementForm:"unqualified", sp->attributeForm?sp->attributeForm:"unqualified");
  else if (style && !strcmp(style, "document"))
    fprintf(fd, "  elementFormDefault=\"qualified\"\n  attributeFormDefault=\"qualified\">\n");
  else
    fprintf(fd, "  elementFormDefault=\"unqualified\"\n  attributeFormDefault=\"unqualified\">\n");
  fflush(fd);
  flag = 0;
  for (s = nslist; s; s = s->next)
  { for (sp = services; sp; sp = sp->next)
      if (!strcmp(sp->ns, s->name) && sp->import && sp->URI)
        break;
    if (sp)
    { fprintf(fd, "  <import namespace=\"%s\"", sp->URI);
      if (strcmp(sp->import, sp->URI))
        fprintf(fd, " schemaLocation=\"%s\"/>\n", sp->import);
      fprintf(fd, "/>\n");
      if (!strcmp(sp->URI, encURI))
        flag = 1;
    }
  }
  if (!flag)
    fprintf(fd, "  <import namespace=\"%s\"/>", encURI);
  fprintf(fd, "\n");
  fflush(fd);
  if (typetable)
    for (p = typetable->list; p; p = p->next)
    { if (p->info.typ->type != Ttemplate && !is_transient(p->info.typ) && (!is_external(p->info.typ) || is_volatile(p->info.typ)) && ((has_ns_eq(ns, p->sym->name))))
      { /* omit the typedefs that are used for SOAP Fault details */
        m = NULL;
        for (sp2 = services; sp2 && !m; sp2 = sp2->next)
	  for (m = sp2->list; m; m = m->next)
	    if ((m->mess&FAULT) && m->part && is_eq(m->part, p->sym->name))
	      break;
        if (m)
        { fprintf(fd, "  <!-- fault element -->\n");
          fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(p->sym->name), base_type(p->info.typ, ns1));
          continue;
        }
        if (is_primitive_or_string(p->info.typ) || p->info.typ->type == Tpointer && is_primitive_or_string(p->info.typ->ref))
        { /* gen_schema_element_names(fd, t, p, ns, ns1); */
	  fprintf(fd, "  <simpleType name=\"%s\">\n   <restriction base=\"%s\">\n", ns_remove(p->sym->name), base_type(p->info.typ, ns1));
	  if (p->info.typ->pattern)
            fprintf(fd, "    <pattern value=\"%s\"/>\n", p->info.typ->pattern);
	  if (p->info.typ->maxLength > 0 && p->info.typ->minLength == p->info.typ->maxLength)
            fprintf(fd, "    <length value=\"%d\"/>\n", p->info.typ->minLength);
	  else
	  { if (p->info.typ->minLength > 0)
              fprintf(fd, "    <minLength value=\"%d\"/>\n", p->info.typ->minLength);
	    if (p->info.typ->maxLength > 0)
              fprintf(fd, "    <maxLength value=\"%d\"/>\n", p->info.typ->maxLength);
	  }
          fprintf(fd, "   </restriction>\n  </simpleType>\n");
        }
        else
	{ /* gen_schema_element_names(fd, t, p, ns, ns1); */
	  fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"%s\">\n", ns_remove(p->sym->name), base_type(p->info.typ, ns1));
          fprintf(fd, "    </restriction>\n   </complexContent>\n  </complexType>\n");
        }
      }
    }
  fflush(fd);
  if (enumtable)
    for (p = enumtable->list; p; p = p->next)
    { if (!is_transient(p->info.typ) && (!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name)))
      { if (is_mask(p->info.typ))
        { /* gen_schema_element_names(fd, t, p, ns, ns1); */
          fprintf(fd, "  <simpleType name=\"%s\">\n   <list>\n    <restriction base=\"xsd:string\">\n", wsdl_type(p->info.typ, NULL));
          if ((Table*)p->info.typ->ref)
            for (q = ((Table*)p->info.typ->ref)->list; q; q = q->next)
              fprintf(fd, "     <enumeration value=\"%s\"/>\n", ns_remove2(q->sym->name));
          fprintf(fd, "    </restriction>\n   </list>\n  </simpleType>\n");
	}
	else
        { /* gen_schema_element_names(fd, t, p, ns, ns1); */
	  fprintf(fd, "  <simpleType name=\"%s\">\n   <restriction base=\"xsd:string\">\n", wsdl_type(p->info.typ, NULL));
          if ((Table*)p->info.typ->ref)
            for (q = ((Table*)p->info.typ->ref)->list; q; q = q->next)
              fprintf(fd, "    <enumeration value=\"%s\"/>\n", ns_remove2(q->sym->name));
          fprintf(fd, "   </restriction>\n  </simpleType>\n");
        }
      }
    }
  fflush(fd);
  if (classtable)
    for (p = classtable->list; p; p = p->next)
    { for (q = t->list; q; q = q->next)
        if (q->info.typ->type == Tfun && !(q->info.sto & Sextern) && p == get_response(q->info.typ))
	  break;
      /* omit the auto-generated and user-defined response struct/class (when necessary) */
      if (!q)
        for (q = t->list; q; q = q->next)
          if (q->info.typ->type == Tfun && !(q->info.sto & Sextern) && !has_ns_eq(NULL, ((Entry*)q->info.typ->ref)->sym->name))
          { r = entry(t, q->sym);
            if (r && r->info.typ->ref && is_response(((Entry*)r->info.typ->ref)->info.typ) && p->info.typ == ((Entry*)r->info.typ->ref)->info.typ->ref)
              break;
	  }
      if (q)
        continue;
      /* omit the classes that are used for SOAP Fault details */
      m = NULL;
      for (sp2 = services; sp2 && !m; sp2 = sp2->next)
	for (m = sp2->list; m; m = m->next)
	  if ((m->mess&FAULT) && m->part && is_eq(m->part, p->sym->name))
	    break;
      if (m)
      { if (!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name))
        { fprintf(fd, "  <!-- fault element -->\n");
          fprintf(fd, "  <element name=\"%s\">\n   <complexType>\n    <sequence>\n", ns_remove(p->sym->name));
	  gen_schema_elements(fd, p->info.typ, ns, ns1);
          fprintf(fd, "    </sequence>\n");
	  gen_schema_attributes(fd, p->info.typ, ns, ns1);
          fprintf(fd, "   </complexType>\n  </element>\n");
	}
        continue;
      }
      if (p->info.typ->ref && is_binary(p->info.typ))
      { if (!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name))
	{ /* gen_schema_element_names(fd, t, p, ns, ns1); */
	  if (is_attachment(p->info.typ))
	  { fprintf(fd, "  <complexType name=\"%s\">\n   <simpleContent>\n    <extension base=\"xsd:base64Binary\">\n", ns_remove(p->sym->name));
	    if (!eflag)
              fprintf(fd, "     <attribute name=\"href\" type=\"xsd:anyURI\" use=\"optional\"/>\n");
	    gen_schema_attributes(fd, p->info.typ, ns, ns1);
            fprintf(fd, "    </extension>\n   </simpleContent>\n  </complexType>\n");
	  }
	  else
	  { fprintf(fd, "  <simpleType name=\"%s\">\n   <restriction base=\"xsd:base64Binary\">\n", ns_remove(p->sym->name));
	    if (p->info.typ->maxLength > 0 && p->info.typ->minLength == p->info.typ->maxLength)
              fprintf(fd, "    <length value=\"%d\"/>\n", p->info.typ->minLength);
	    else
	    { if (p->info.typ->minLength > 0)
                fprintf(fd, "    <minLength value=\"%d\"/>\n", p->info.typ->minLength);
	      if (p->info.typ->maxLength > 0)
                fprintf(fd, "    <maxLength value=\"%d\"/>\n", p->info.typ->maxLength);
	    }
            fprintf(fd, "   </restriction>\n  </simpleType>\n");
	  }
        }
      }
      else if (p->info.typ->ref && !is_transient(p->info.typ) && is_primclass(p->info.typ))
      { if ((!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name)))
        { q = ((Table*)p->info.typ->ref)->list;
	  /* gen_schema_element_names(fd, t, p, ns, ns1); */
	  if (q && strncmp(q->sym->name, "xsd__anyType", 12))
	  { if (is_string(q->info.typ) || is_wstring(q->info.typ) || is_stdstring(q->info.typ) || is_stdwstring(q->info.typ))
            { fprintf(fd, "  <complexType name=\"%s\" mixed=\"true\">\n   <simpleContent>\n    <extension base=\"%s\">\n", ns_remove(p->sym->name), wsdl_type(q->info.typ, ns1));
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "    </extension>\n   </simpleContent>\n  </complexType>\n");
	    }
	    else if (is_primitive(q->info.typ))
            { fprintf(fd, "  <complexType name=\"%s\">\n   <simpleContent>\n    <extension base=\"%s\">\n", ns_remove(p->sym->name), wsdl_type(q->info.typ, ns1));
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "    </extension>\n   </simpleContent>\n  </complexType>\n");
	    }
	    else
            { fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <extension base=\"%s\">\n", ns_remove(p->sym->name), wsdl_type(q->info.typ, ns1));
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "    </extension>\n   </complexContent>\n  </complexType>\n");
	    }
	  }
        }
      }
      else if (p->info.typ->ref && !is_transient(p->info.typ))
      { q = ((Table*)p->info.typ->ref)->list;
        if (entry(t, p->sym) && (!q || !is_XML(q->info.typ)))
          ;
        else if (is_dynamic_array(p->info.typ))
        { if (!has_ns(p->info.typ) && !is_untyped(p->info.typ))
          { if (all)
	      if (1 /* wsdl */)
	      { d = get_Darraydims(p->info.typ)-1;
	        for (i = 0; i < d; i++)
	          cbuf[i] = ',';
	        cbuf[i] = '\0';
		if (q->info.maxOccurs == 1)
	          fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <sequence>\n      <element name=\"%s\" type=\"%s\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n     </sequence>\n     <attribute ref=\"SOAP-ENC:arrayType\" WSDL:arrayType=\"%s[%s]\"/>\n    </restriction>\n   </complexContent>\n  </complexType>\n", wsdl_type(p->info.typ, NULL), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1), wsdl_type(q->info.typ, ns1), cbuf);
                else
	          fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <sequence>\n      <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n     </sequence>\n     <attribute ref=\"SOAP-ENC:arrayType\" WSDL:arrayType=\"%s[%s]\"/>\n    </restriction>\n   </complexContent>\n  </complexType>\n", wsdl_type(p->info.typ, NULL), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1), q->info.minOccurs, q->info.maxOccurs, wsdl_type(q->info.typ, ns1), cbuf);
	      }
              else
		if (q->info.maxOccurs == 1)
	          fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <sequence>\n      <element name=\"%s\" type=\"%s\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n     </sequence>\n    </restriction>\n   </complexContent>\n  </complexType>\n", wsdl_type(p->info.typ, NULL), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1));
                else
	          fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <sequence>\n      <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n     </sequence>\n    </restriction>\n   </complexContent>\n  </complexType>\n", wsdl_type(p->info.typ, NULL), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1),  q->info.minOccurs, q->info.maxOccurs);
          }
	  else if (p->info.typ->ref && ((Table*)p->info.typ->ref)->prev && !is_transient(entry(classtable, ((Table*)p->info.typ->ref)->prev->sym)->info.typ) && strncmp(((Table*)p->info.typ->ref)->prev->sym->name, "xsd__anyType", 12))
	  { if (q->info.maxOccurs == 1)
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
              fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <extension base=\"%s\">\n     <sequence>\n", ns_remove(p->sym->name), ns_convert(((Table*)p->info.typ->ref)->prev->sym->name));
	      fprintf(fd, "      <element name=\"%s\" type=\"%s\" minOccurs=\"0\" maxOccurs=\"unbounded\" nillable=\"true\"/>\n", q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1));
              fprintf(fd, "     </sequence>\n    </extension>\n   </complexContent>\n");
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "  </complexType>\n");
	    }
	    else
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
              fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <extension base=\"%s\">\n     <sequence>\n", ns_remove(p->sym->name), ns_convert(((Table*)p->info.typ->ref)->prev->sym->name));
	      fprintf(fd, "      <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
              fprintf(fd, "     </sequence>\n    </extension>\n   </complexContent>\n");
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "  </complexType>\n");
	    }
	  }
	  else
	  { if (q->info.maxOccurs == 1)
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
	      fprintf(fd, "  <complexType name=\"%s\">\n   <sequence>\n    <element name=\"%s\" type=\"%s\" minOccurs=\"0\" maxOccurs=\"unbounded\" nillable=\"true\"/>\n   </sequence>\n  </complexType>\n", ns_remove(p->sym->name), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1));
	    }
            else
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
	      fprintf(fd, "  <complexType name=\"%s\">\n   <sequence>\n    <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n   </sequence>\n  </complexType>\n", ns_remove(p->sym->name), q->sym->name[5]?ns_remove(q->sym->name+5):"item", wsdl_type(q->info.typ, ns1), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
	    }
	  }
	}
        else if (p->info.typ->type == Tstruct && (!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name)))
        { if (p->info.typ->ref)
	  { /* gen_schema_element_names(fd, t, p, ns, ns1); */
            fprintf(fd, "  <complexType name=\"%s\">\n   <sequence>\n", ns_remove(p->sym->name));
	    gen_schema_elements(fd, p->info.typ, ns, ns1);
            fprintf(fd, "   </sequence>\n");
	    gen_schema_attributes(fd, p->info.typ, ns, ns1);
            fprintf(fd, "  </complexType>\n");
          }
        }
        else if (p->info.typ->type == Tclass && (!has_ns(p->info.typ) && all || has_ns_eq(ns, p->sym->name)))
        { if (p->info.typ->ref)
          { if (((Table*)p->info.typ->ref)->prev && !is_transient(entry(classtable, ((Table*)p->info.typ->ref)->prev->sym)->info.typ) && strncmp(((Table*)p->info.typ->ref)->prev->sym->name, "xsd__anyType", 12))
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
              fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <extension base=\"%s\">\n     <sequence>\n", ns_remove(p->sym->name), ns_convert(((Table*)p->info.typ->ref)->prev->sym->name));
	      gen_schema_elements(fd, p->info.typ, ns, ns1);
              fprintf(fd, "     </sequence>\n    </extension>\n   </complexContent>\n");
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "  </complexType>\n");
	    }
	    else
	    { /* gen_schema_element_names(fd, t, p, ns, ns1); */
              fprintf(fd, "  <complexType name=\"%s\">\n   <sequence>\n", ns_remove(p->sym->name));
	      gen_schema_elements(fd, p->info.typ, ns, ns1);
              fprintf(fd, "   </sequence>\n");
	      gen_schema_attributes(fd, p->info.typ, ns, ns1);
              fprintf(fd, "  </complexType>\n");
            }
	  }
        }
      }
    }
  fflush(fd);
  for (n = Tptr[Tarray]; n; n = n->next)
  { if (is_transient(n))
      continue;
    else if (1 /* wsdl */)
      fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <attribute ref=\"SOAP-ENC:arrayType\" WSDL:arrayType=\"%s[]\"/>\n    </restriction>\n   </complexContent>\n  </complexType>\n", c_ident(n), wsdl_type(n->ref, ns1));
    else
      fprintf(fd, "  <complexType name=\"%s\">\n   <complexContent>\n    <restriction base=\"SOAP-ENC:Array\">\n     <element name=\"item\" type=\"%s\" maxOccurs=\"unbounded\"/>\n    </restriction>\n   </complexContent>\n  </complexType>\n", c_ident(n), xsi_type(n->ref));
    fflush(fd);
  }
  gen_schema_elements_attributes(fd, t, ns, ns1, style, encoding);
  fprintf(fd, " </schema>\n\n");
}

void
gen_schema_elements(FILE *fd, Tnode *p, char *ns, char *ns1)
{ Entry *q;
  for (q = ((Table*)p->ref)->list; q; q = q->next)
    if (gen_schema_element(fd, q, ns, ns1))
      q = q->next;
}

int
gen_schema_element(FILE *fd, Entry *q, char *ns, char *ns1)
{ char *s, *t;
      if (is_transient(q->info.typ) || (q->info.sto & Sattribute) || q->info.typ->type == Tfun)
        return 0;
      if (is_repetition(q))
      { t = ns_convert(q->next->sym->name);
        if ((s = strchr(t, ':'))) /* && !has_ns_eq(ns, q->next->sym->name)) */
	{ if (((Tnode*)q->next->info.typ->ref)->type == Tpointer)
            if (q->info.maxOccurs == 1)
	      fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\" nillable=\"%s\"/>\n", t, q->info.minOccurs, nillable(q->info.minOccurs));
            else
	      fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", t, q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
            else
          if (q->info.maxOccurs == 1)
              fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\"/>\n", t, q->info.minOccurs);
	    else
              fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n", t, q->info.minOccurs, q->info.maxOccurs);
	}
	else
	{ if (s) s++; else s = t;
	  if (((Tnode*)q->next->info.typ->ref)->type == Tpointer)
            if (q->info.maxOccurs == 1)
	      fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\" nillable=\"%s\"/>\n", s, wsdl_type(q->next->info.typ->ref, ns1), q->info.minOccurs, nillable(q->info.minOccurs));
            else
	      fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", s, wsdl_type(q->next->info.typ->ref, ns1), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
          else
            if (q->info.maxOccurs == 1)
              fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\"/>\n", s, wsdl_type(q->next->info.typ->ref, ns1), q->info.minOccurs);
	    else
              fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n", s, wsdl_type(q->next->info.typ->ref, ns1), q->info.minOccurs, q->info.maxOccurs);
	}
        return 1;
      }
      else if (q->info.typ->type == Ttemplate || q->info.typ->type == Tpointer && ((Tnode*)q->info.typ->ref)->type == Ttemplate || q->info.typ->type == Treference && ((Tnode*)q->info.typ->ref)->type == Ttemplate)
      { t = ns_convert(q->sym->name);
        if ((s = strchr(t, ':'))) /* && !has_ns_eq(ns, q->sym->name)) */
	{ if (((Tnode*)q->info.typ->ref)->type == Tpointer)
            if (q->info.maxOccurs == 1)
	      fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\" nillable=\"%s\"/>\n", t, q->info.minOccurs, nillable(q->info.minOccurs));
            else
	      fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", t, q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
            else
          if (q->info.maxOccurs == 1)
              fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\"/>\n", t, q->info.minOccurs);
	    else
              fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n", t, q->info.minOccurs, q->info.maxOccurs);
	}
	else
	{ if (s) s++; else s = t;
	  if (((Tnode*)q->info.typ->ref)->type == Tpointer)
            if (q->info.maxOccurs == 1)
	      fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\" nillable=\"%s\"/>\n", s, wsdl_type(q->info.typ->ref, ns1), q->info.minOccurs, nillable(q->info.minOccurs));
            else
	      fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", s, wsdl_type(q->info.typ->ref, ns1), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
          else
            if (q->info.maxOccurs == 1)
              fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"unbounded\"/>\n", s, wsdl_type(q->info.typ->ref, ns1), q->info.minOccurs);
	    else
              fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"/>\n", s, wsdl_type(q->info.typ->ref, ns1), q->info.minOccurs, q->info.maxOccurs);
	}
      }
      else if (is_anytype(q)) /* ... maybe need to show all possible types rather than xsd:anyType */
      { fprintf(fd, "     <element name=\"%s\" type=\"xsd:anyType\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"/>\n", ns_convert(q->next->sym->name), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs));
        return 1;
      }
      else
      { t = ns_convert(q->sym->name);
        if ((s = strchr(t, ':'))) /* && !has_ns_eq(ns, q->sym->name)) */
	{ if (q->info.typ->type == Tpointer || q->info.typ->type == Tarray || is_dynamic_array(q->info.typ))
            fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"%s/>\n", t, q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs), default_value(q, "default"));
          else
            fprintf(fd, "     <element ref=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"%s/>\n", t, q->info.minOccurs, q->info.maxOccurs, default_value(q, "default"));
	}
        else
	{ if (s) s++; else s = t;
	  if (q->info.typ->type == Tpointer || q->info.typ->type == Tarray || is_dynamic_array(q->info.typ))
            fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\" nillable=\"%s\"%s/>\n", s, wsdl_type(q->info.typ, ns1), q->info.minOccurs, q->info.maxOccurs, nillable(q->info.minOccurs), default_value(q, "default"));
          else
            fprintf(fd, "     <element name=\"%s\" type=\"%s\" minOccurs=\"%ld\" maxOccurs=\"%ld\"%s/>\n", s, wsdl_type(q->info.typ, ns1), q->info.minOccurs, q->info.maxOccurs, default_value(q, "default"));
        }
      }
      fflush(fd);
  return 0;
}

void
gen_schema_elements_attributes(FILE *fd, Table *t, char *ns, char *ns1, char *style, char *encoding)
{ Entry *p, *q, *e;
  Table *r;
  Service *sp;
  Method *m;
  char *method_style, *method_encoding, *method_response_encoding;
  int all = !strcmp(ns, ns1);
  r = mktable(NULL);
  for (p = classtable->list; p; p = p->next)
  { if (!p->info.typ->ref || is_invisible(p->info.typ->id->name) || is_transient(p->info.typ) || is_primclass(p->info.typ) || is_dynamic_array(p->info.typ))
      continue;
    for (q = ((Table*)p->info.typ->ref)->list; q; q = q->next)
    { if (!is_repetition(q) && !is_anytype(q) && has_ns_eq(ns, q->sym->name) && !is_transient(q->info.typ) && q->info.typ->type != Tfun)
      { e = entry(r, q->sym);
        if (e)
	{ if ((e->info.sto & Sattribute) != (q->info.sto & Sattribute)|| reftype(e->info.typ) != reftype(q->info.typ))
          { sprintf(errbuf, "Field '%s' of type '%s' at line %d has a type that does not correspond to the required unique type '%s' defined for elements '<%s>' in the WSDL namespace based on literal encoding: use SOAP RPC encoding or rename or use a namespace qualifier", q->sym->name, c_type(q->info.typ), q->lineno, c_type(e->info.typ), ns_convert(q->sym->name));
            semwarn(errbuf);
	  }
	}
        else
	{ if (q->info.sto & Sattribute)
	    fprintf(fd, "  <attribute name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	  else	
	    fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	  e = enter(r, q->sym);
	  e->info = q->info;
        }
      }
    }
  }
  if (t && all)
  { for (p = t->list; p; p = p->next)
    { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns, p->sym->name))
      { method_encoding = encoding;
	method_response_encoding = NULL;
	method_style = style;
	for (sp = services; sp; sp = sp->next)
	  if (!strcmp(sp->ns, ns))
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name))
                if (m->mess == ENCODING)
	          method_encoding = m->part;
                else if (m->mess == RESPONSE_ENCODING)
	          method_response_encoding = m->part;
                else if (m->mess == STYLE)
	          method_style = m->part;
	if (!eflag)
	{ if (!method_response_encoding)
            method_response_encoding = method_encoding;
          q = entry(classtable, p->sym);
          if (q)
	  { if (is_document(method_style))
            { fprintf(fd, "  <!-- operation request element -->\n");
	      fprintf(fd, "  <element name=\"%s\">\n   <complexType>\n    <sequence>\n", ns_remove(p->sym->name));
	      gen_schema_elements(fd, q->info.typ, ns, ns1);
	      fprintf(fd, "    </sequence>\n");
	      gen_schema_attributes(fd, q->info.typ, ns, ns1);
	      fprintf(fd, "   </complexType>\n  </element>\n");
	    }
	    else if (is_literal(method_encoding))
	    { for (q = ((Table*)q->info.typ->ref)->list; q; q = q->next)
              { if (!is_repetition(q) && !is_anytype(q) && !has_ns_eq(NULL, q->sym->name) && !is_transient(q->info.typ) && q->info.typ->type != Tfun && !(q->info.sto & Sattribute))
                { e = entry(r, q->sym);
                  if (e)
	          { if ((e->info.sto & Sattribute) != (q->info.sto & Sattribute)|| reftype(e->info.typ) != reftype(q->info.typ))
                    { sprintf(errbuf, "Parameter '%s' of type '%s' at line %d has a type that does not correspond to the required unique type '%s' defined for elements '<%s>' in the WSDL namespace based on literal encoding: use SOAP RPC encoding or rename or use a namespace qualifier", q->sym->name, c_type(q->info.typ), q->lineno, c_type(e->info.typ), ns_convert(q->sym->name));
                      semwarn(errbuf);
	            }
	          }
                  else
                  { fprintf(fd, "  <!-- operation request element -->\n");
	            fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	            e = enter(r, q->sym);
		    e->info = q->info;
	          }
	        }
	      }
	    }
            q = (Entry*)p->info.typ->ref;
            if (q && !is_transient(q->info.typ))
            { if (!is_response(q->info.typ))
	      { if (is_document(method_style))
                { fprintf(fd, "  <!-- operation response element -->\n");
	          fprintf(fd, "  <element name=\"%sResponse\">\n   <complexType>\n    <sequence>\n", ns_remove(p->sym->name));
	          gen_schema_element(fd, q, ns, ns1);
	          fprintf(fd, "    </sequence>\n");
	          fprintf(fd, "   </complexType>\n  </element>\n");
		}
  	        else if (is_literal(method_response_encoding))
                { e = entry(r, q->sym);
                  if (e)
	          { if ((e->info.sto & Sattribute) != (q->info.sto & Sattribute)|| reftype(e->info.typ) != reftype(q->info.typ))
                    { sprintf(errbuf, "Qualified field '%s' has a type that does not correspond to the unique type '%s' defined for elements '<%s>'", q->sym->name, c_type(q->info.typ), ns_convert(q->sym->name));
                      semwarn(errbuf);
	            }
	          }
                  else
                  { fprintf(fd, "  <!-- operation response element -->\n");
	            fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	            e = enter(r, q->sym);
		    e->info = q->info;
	          }
	        }
	      }
              else if (((Tnode*)q->info.typ->ref)->ref)
	      { if (is_document(method_style))
	        { if (!has_ns_eq(NULL, q->sym->name))
		  { e = entry(r, ((Tnode*)q->info.typ->ref)->id);
		    if (!e)
                    { fprintf(fd, "  <!-- operation response element -->\n");
		      fprintf(fd, "  <element name=\"%s\">\n   <complexType>\n    <sequence>\n", ns_remove(((Tnode*)q->info.typ->ref)->id->name));
	              gen_schema_elements(fd, q->info.typ->ref, ns, ns1);
	              fprintf(fd, "    </sequence>\n");
	              gen_schema_attributes(fd, q->info.typ->ref, ns, ns1);
	              fprintf(fd, "   </complexType>\n  </element>\n");
		      e = enter(r, ((Tnode*)q->info.typ->ref)->id);
		      e->info = q->info;
		    }
		  }
		}
		else if (is_literal(method_response_encoding))
	        { for (q = ((Table*)((Tnode*)q->info.typ->ref)->ref)->list; q; q = q->next)
  	          { if (!is_repetition(q) && !is_anytype(q) && !has_ns_eq(NULL, q->sym->name) && !is_transient(q->info.typ) && q->info.typ->type != Tfun && !(q->info.sto & Sattribute))
                    { e = entry(r, q->sym);
                      if (e)
	              { if ((e->info.sto & Sattribute) != (q->info.sto & Sattribute)|| reftype(e->info.typ) != reftype(q->info.typ))
                        { sprintf(errbuf, "Qualified field '%s' has a type that does not correspond to the unique type '%s' defined for elements '<%s>'", q->sym->name, c_type(q->info.typ), ns_convert(q->sym->name));
                          semwarn(errbuf);
	                }
	              }
                      else
                      { fprintf(fd, "  <!-- operation response element -->\n");
	                fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	                e = enter(r, q->sym);
		        e->info = q->info;
	              }
	            }
	          }
	        }
	      }
            }
          }
        }
      }
    }
  }
  if (t)
  { for (p = t->list; p; p = p->next)
    { if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && !eflag)
      { q = (Entry*)p->info.typ->ref;
        if (q && !is_transient(q->info.typ))
        { if (is_response(q->info.typ))
	  { if (has_ns_eq(ns, q->sym->name))
            { e = entry(r, q->sym);
              if (!e)
              { fprintf(fd, "  <!-- operation response element -->\n");
	        fprintf(fd, "  <element name=\"%s\" type=\"%s\"/>\n", ns_remove(q->sym->name), wsdl_type(q->info.typ, ns1));
	        e = enter(r, q->sym);
		e->info = q->info;
	      }
	    }
	  }
        }
      }
    }
  }
  freetable(r);
}

void
gen_schema_attributes(FILE *fd, Tnode *p, char *ns, char *ns1)
{ Entry *q;
  char *t, *s, *r;
  for (q = ((Table*)p->ref)->list; q; q = q->next)
  { if (q->info.sto & Sattribute)
    { t = ns_convert(q->sym->name);
      r = default_value(q, "value");
      if ((s = strchr(t, ':')))
      { if (r && *r)
          fprintf(fd, "     <attribute ref=\"%s\" use=\"default\"%s/>\n", t, r);
	else if (q->info.minOccurs)
          fprintf(fd, "     <attribute ref=\"%s\" use=\"required\"/>\n", t);
	else
          fprintf(fd, "     <attribute ref=\"%s\" use=\"optional\"/>\n", t);
      }
      else
      { if (!s)
          s = t;
        else
	  s++;
	if (r && *r)
          fprintf(fd, "     <attribute name=\"%s\" type=\"%s\" use=\"default\"%s/>\n", s, wsdl_type(q->info.typ, ns1), r);
	else if (q->info.minOccurs)
          fprintf(fd, "     <attribute name=\"%s\" type=\"%s\" use=\"required\"/>\n", s, wsdl_type(q->info.typ, ns1));
	else
          fprintf(fd, "     <attribute name=\"%s\" type=\"%s\" use=\"optional\"/>\n", s, wsdl_type(q->info.typ, ns1));
      }
      fflush(fd);
    }
  }
}

void
gen_nsmap(FILE *fd, Symbol *ns, char *URI)
{ Symbol *ns1;
  Service *sp;
  fprintf(fd, "{\n");
  for (ns1 = nslist; ns1; ns1 = ns1->next)
    /* if (ns1 != ns) */
    { for (sp = services; sp; sp = sp->next)
        if (!strcmp(sp->ns, ns1->name) && sp->URI)
	  break;
        if (sp)
        { if (!strcmp(ns1->name, "SOAP-ENV"))
	    fprintf(fd, "\t{\"%s\", \"%s\", \"%s\"},\n", ns_convert(ns1->name), sp->URI, envURI);
          else if (!strcmp(ns1->name, "SOAP-ENC"))
	    fprintf(fd, "\t{\"%s\", \"%s\", \"%s\"},\n", ns_convert(ns1->name), sp->URI, encURI);
          else
	    fprintf(fd, "\t{\"%s\", \"%s\"},\n", ns_convert(ns1->name), sp->URI);
        }
        else if (!strcmp(ns1->name, "SOAP-ENV"))
	  fprintf(fd, "\t{\"SOAP-ENV\", \"%s\", \"http://www.w3.org/*/soap-envelope\"},\n", envURI);
        else if (!strcmp(ns1->name, "SOAP-ENC"))
	  fprintf(fd, "\t{\"SOAP-ENC\", \"%s\", \"http://www.w3.org/*/soap-encoding\"},\n", encURI);
        else if (!strcmp(ns1->name, "xsi"))
	  fprintf(fd, "\t{\"xsi\", \"%s\", \"http://www.w3.org/*/XMLSchema-instance\"},\n", xsiURI);
        else if (!strcmp(ns1->name, "xsd"))
	  fprintf(fd, "\t{\"xsd\", \"%s\", \"http://www.w3.org/*/XMLSchema\"},\n", xsdURI);
        else
	  fprintf(fd, "\t{\"%s\", \"%s/%s.xsd\"},\n", ns_convert(ns1->name), URI, ns_convert(ns1->name)); 
    }
    /* fprintf(fd, "\t{\"%s\", \"%s\"},\n", ns_convert(ns->name), URI); */
    fprintf(fd, "\t{NULL, NULL}\n};\n");
}

void
gen_proxy(FILE *fd, Table *table, Symbol *ns, char *name, char *URL, char *executable, char *URI, char *encoding)
{ Entry *p, *q, *r;
  Table *t, *output;
  char *s;
  Service *sp;
  Method *m;
  int flag;
  char *name1;
  name1 = emalloc(strlen(name)+1);
  strcpy(name1, name);
  for (s = name1+1; *s; s++)
  { if (!isalnum(*s))
    { *s = '\0';
      break;
    }
  }
  for (sp = services; sp; sp = sp->next)
    if (!strcmp(sp->ns, ns->name))
      break;
  fprintf(fd, "\n\n#ifndef %s%s_H\n#define %s%s_H\n#include \"%sH.h\"", prefix, name1, prefix, name1, prefix);
  if (nflag)
    fprintf(fd, "\nextern SOAP_NMAC struct Namespace %s_namespaces[];", prefix);
  fprintf(fd, "\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
  fprintf(fd, "\nclass %s\n{   public:\n\tstruct soap *soap;\n\tconst char *endpoint;\n", name1);
  if (nflag)
    fprintf(fd, "\t%s() { soap = soap_new(); if (soap) soap->namespaces = %s_namespaces; endpoint = \"%s\"; };\n", name1, prefix, URL);
  else
  { fprintf(fd, "\t%s()\n\t{ soap = soap_new(); endpoint = \"%s\"; if (soap && !soap->namespaces) { static const struct Namespace namespaces[] = \n", name1, URL);
    gen_nsmap(fd, ns, URI);
    fprintf(fd, "\tsoap->namespaces = namespaces; } };\n", name1, URL);
  }
  fprintf(fd, "\tvirtual ~%s() { if (soap) { soap_destroy(soap); soap_end(soap); soap_done(soap); SOAP_FREE((void*)soap); } };\n", name1);
  fflush(fd);
  for (r = table->list; r; r = r->next)
    if (r->info.typ->type == Tfun && !(r->info.sto & Sextern) && has_ns_eq(ns->name, r->sym->name))
    { p = entry(table, r->sym);
      if (p)
        q = (Entry*)p->info.typ->ref;
      else
        fprintf(stderr, "Internal error: no table entry\n");
      p = entry(classtable, r->sym);
      if (!p)
        fprintf(stderr, "Internal error: no parameter table entry\n");
      output = (Table*)p->info.typ->ref;
      /*
      if ((s = strstr(r->sym->name, "__")))
        s += 2;
      else
        s = r->sym->name;
      fprintf(fd, "\tvirtual int %s(", s);
      */
      fprintf(fd, "\tvirtual int %s(", r->sym->name);
      flag = 0;
      for (t = output; t; t = t->prev)
      { p = t->list;
        if (p)
        { fprintf(fd, "%s%s%s", c_storage(p->info.sto), c_type_id(p->info.typ, p->sym->name), c_init(p));
          for (p = p->next; p; p = p->next)
            fprintf(fd, ", %s%s%s", c_storage(p->info.sto), c_type_id(p->info.typ, p->sym->name), c_init(p));
	  flag = 1;
        }
      }
      if (is_transient(q->info.typ))
        fprintf(fd,") { return soap ? soap_send_%s(soap, endpoint, NULL", r->sym->name);
      else if (flag)
        fprintf(fd,", %s%s) { return soap ? soap_call_%s(soap, endpoint, NULL", c_storage(q->info.sto), c_type_id(q->info.typ, q->sym->name), r->sym->name);
      else
        fprintf(fd,"%s%s) { return soap ? soap_call_%s(soap, endpoint, NULL", c_storage(q->info.sto), c_type_id(q->info.typ, q->sym->name), r->sym->name);
      /* the action is now handled by the soap_call/soap_send operation when we pass NULL
      m = NULL;
      if (sp && (s = strstr(r->sym->name, "__")))
        for (m = sp->list; m; m = m->next)
          if (m->part && m->mess == ACTION && !strcmp(m->name, s+2))
          { if (*m->part == '"')
	      fprintf(fd, "%s", m->part);
            else
	      fprintf(fd, "\"%s\"", m->part);
	    break;
	  }
      if (!m)
        fprintf(fd, "NULL");
      */
      for (t = output; t; t = t->prev)
        for (p = t->list; p; p = p->next)
          fprintf(fd, ", %s", p->sym->name);
      if (is_transient(q->info.typ))
        fprintf(fd,") : SOAP_EOM; };\n");
      else
        fprintf(fd,", %s) : SOAP_EOM; };\n", q->sym->name);
      fflush(fd);
    }
  fprintf(fd, "};");
  fprintf(fd, "\nSOAP_END_NAMESPACE(%s)", prefix);
  fprintf(fd, "\n#endif\n");
}

void
gen_object(FILE *fd, Table *table, Symbol *ns, char *name, char *URL, char *executable, char *URI, char *encoding)
{ char *s;
  char *name1;
  name1 = emalloc(strlen(name)+1);
  strcpy(name1, name);
  for (s = name1+1; *s; s++)
  { if (!isalnum(*s))
    { *s = '\0';
      break;
    }
  }
  fprintf(fd, "\n\n#ifndef %s%s_H\n#define %s%s_H\n#include \"%sH.h\"", prefix, name1, prefix, name1, prefix);
  if (nflag)
    fprintf(fd, "\nextern SOAP_NMAC struct Namespace %s_namespaces[];", prefix);
  fprintf(fd, "\nSOAP_BEGIN_NAMESPACE(%s)", prefix);
  fprintf(fd, "\nclass %s : public soap\n{    public:", name1);
  if (nflag)
    fprintf(fd, "\n\t%s() { soap_init(this); this->namespaces = %s_namespaces; };", name1, prefix);
  else
  { /* fprintf(fd, "\n\t%s() { soap_init(this); };", name1); */
    fprintf(fd, "\n\t%s() { static const struct Namespace namespaces[] =\n", name1);
    gen_nsmap(fd, ns, URI);
    fprintf(fd, "\tsoap_init(this); if (!this->namespaces) this->namespaces = namespaces; };");
  }
  fprintf(fd, "\n\t~%s() { soap_destroy(this); soap_end(this); soap_done(this); };", name1);
  if (nflag)
    fprintf(fd, "\n\tint serve() { return %s_serve(this); };", prefix);
  else
    fprintf(fd, "\n\tint serve() { return soap_serve(this); };");
  fprintf(fd, "\n};");
  fprintf(fd, "\nSOAP_END_NAMESPACE(%s)", prefix);
  fprintf(fd, "\n#endif\n");
}

void
gen_response_begin(FILE *fd, int n, char *s)
{ if (!is_invisible(s))
    fprintf(fd, "%*s<%sResponse>\n", n, "", s);
}

void
gen_response_end(FILE *fd, int n, char *s)
{ if (!is_invisible(s))
    fprintf(fd, "%*s</%sResponse>\n", n, "", s);
}

void
gen_element_begin(FILE *fd, int n, char *s, char *t)
{ if (!is_invisible(s))
    if (tflag && t && *t)
      fprintf(fd, "%*s<%s xsi:type=\"%s\"", n, "", s, t);
    else
      fprintf(fd, "%*s<%s", n, "", s);
}

void
gen_element_end(FILE *fd, int n, char *s)
{ if (!is_invisible(s))
    fprintf(fd, "%*s</%s>\n", n, "", s);
}

void
gen_data(char *buf, Table *t, char *ns, char *name, char *URL, char *executable, char *URI, char *encoding)
{ Entry *p, *q, *r;
  FILE *fd;
  char *method_encoding = NULL;
  char *method_response_encoding = NULL;
  if (t)
  { for (p = t->list; p; p = p->next)
      if (p->info.typ->type == Tfun && !(p->info.sto & Sextern) && has_ns_eq(ns, p->sym->name))
      { Service *sp;
        Method *m;
	char *nse = ns_qualifiedElement(p->info.typ);
	char *nsa = ns_qualifiedAttribute(p->info.typ);
        method_encoding = encoding;
	method_response_encoding = NULL;
	for (sp = services; sp; sp = sp->next)
	  if (!strcmp(sp->ns, ns))
	    for (m = sp->list; m; m = m->next)
	      if (is_eq_nons(m->name, p->sym->name))
                if (m->mess == ENCODING)
	          method_encoding = m->part;
                else if (m->mess == RESPONSE_ENCODING)
	          method_response_encoding = m->part;
	if (!method_response_encoding)
	  method_response_encoding = method_encoding;
	/* request */
        fd = gen_env(buf, ns_remove(p->sym->name), 0, t, ns, name, URL, executable, URI, method_encoding);
	if (!fd)
	  return;
  	q = entry(classtable, p->sym);
	gen_element_begin(fd, 2, ns_convert(p->sym->name), NULL);
  	if (q)
	{ gen_atts(fd, q->info.typ->ref, nsa);
          fprintf(fd, "\n");
	  for (q = ((Table*)q->info.typ->ref)->list; q; q = q->next)
	    gen_field(fd, 3, q, nse, nsa, method_encoding);
	}
        gen_element_end(fd, 2, ns_convert(p->sym->name));
        fprintf(fd, " </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>\n");
        fclose(fd);
	/* response */
	q = (Entry*)p->info.typ->ref;
	if (q && !is_transient(q->info.typ))
        { fd = gen_env(buf, ns_remove(p->sym->name), 1, t, ns, name, URL, executable, URI, method_response_encoding);
	  if (!fd)
	    return;
	  if (q && !is_response(q->info.typ))
	    if (is_XML(q->info.typ->ref))
	    { gen_response_begin(fd, 2, ns_convert(p->sym->name));
	      gen_response_end(fd, 2, ns_convert(p->sym->name));
	    }
	    else
	    { gen_response_begin(fd, 2, ns_convert(p->sym->name));
	      gen_field(fd, 3, q, nse, nsa, method_response_encoding);
	      gen_response_end(fd, 2, ns_convert(p->sym->name));
	    }
          else if (q && q->info.typ->ref && ((Tnode*)q->info.typ->ref)->ref)
          { char *xtag;
	    nse = ns_qualifiedElement(q->info.typ->ref);
	    nsa = ns_qualifiedAttribute(q->info.typ->ref);
	    if (has_ns_eq(NULL, q->sym->name))
              xtag = q->sym->name;
            else
              xtag = ((Tnode*)q->info.typ->ref)->id->name;
	    gen_element_begin(fd, 2, ns_add(xtag, nse), NULL);
	    gen_atts(fd, ((Tnode*)q->info.typ->ref)->ref, nsa);
            fprintf(fd, "\n");
	    for (r = ((Table*)((Tnode*)q->info.typ->ref)->ref)->list; r; r = r->next)
	      gen_field(fd, 3, r, nse, nsa, method_response_encoding);
	    gen_element_end(fd, 2, ns_add(xtag, nse));
	  }
          fflush(fd);
          fprintf(fd, " </SOAP-ENV:Body>\n</SOAP-ENV:Envelope>\n");
          fclose(fd);
	}
      }
  }
}

void
gen_field(FILE *fd, int n, Entry *p, char *nse, char *nsa, char *encoding)
{ Entry *q;
  char *s, tmp[32];
  int i, d;
  if (!(p->info.sto & Sattribute) && !is_transient(p->info.typ) && p->info.typ->type != Tfun && strncmp(p->sym->name, "__size", 6) && strncmp(p->sym->name, "__type", 6))
  { if (vflag == 2 && (p->info.sto & Sreturn) && !is_literal(encoding))
      fprintf(fd, "%*s<SOAP-RPC:result xmlns:SOAP-RPC=\"%s\">%s</SOAP-RPC:result>\n", n, "", rpcURI, ns_convert(p->sym->name));
    if (is_XML(p->info.typ))
    { gen_element_begin(fd, n, ns_add(p->sym->name, nse), NULL);
      fprintf(fd, ">");
      gen_element_end(fd, n, ns_add(p->sym->name, nse));
    }
    else
    { if (!is_string(p->info.typ) && n >= 10 && (p->info.typ->type == Tpointer || p->info.typ->type == Treference))
      { if (!is_invisible(p->sym->name))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), NULL);
          fprintf(fd, " xsi:nil=\"true\">");
        }
      }
      else if (p->info.typ->type == Tarray)
      { i = ((Tnode*) p->info.typ->ref)->width;
        if (i)
        { i = p->info.typ->width / i;
          if (i > 100)
            i = 100;
	}
	gen_element_begin(fd, n, ns_add(p->sym->name, nse), "SOAP-ENC:Array");
	fprintf(fd, " SOAP-ENC:arrayType=\"%s[%d]\">", xsi_type_Tarray(p->info.typ), i);
        fflush(fd);
	gen_val(fd, n, p->info.typ, nse, nsa, encoding);
      }
      else if (is_dynamic_array(p->info.typ) && !is_binary(p->info.typ))
      { if (has_ns(p->info.typ) || is_untyped(p->info.typ))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), xsi_type(p->info.typ));
	}
        else
	{ d = get_Darraydims(p->info.typ);
	  if (d)
	  { for (i = 0; i < d-1; i++)
	    { tmp[2*i] = ',';
	      tmp[2*i+1] = '1';
	    }
	    tmp[2*d-2] = '\0';
	  }
	  else
	    *tmp = '\0';
	  gen_element_begin(fd, n, ns_add(p->sym->name, nse), "SOAP-ENC:Array");
	  if (((Table*)p->info.typ->ref)->list->info.minOccurs > 1)
	    fprintf(fd, " SOAP-ENC:arrayType=\"%s[%ld%s]\">", wsdl_type(((Table*)p->info.typ->ref)->list->info.typ, ""), ((Table*)p->info.typ->ref)->list->info.minOccurs, tmp);
	  else
	    fprintf(fd, " SOAP-ENC:arrayType=\"%s[1%s]\">", wsdl_type(((Table*)p->info.typ->ref)->list->info.typ, ""), tmp);
	}
        fflush(fd);
        gen_val(fd, n, p->info.typ, nse, nsa, encoding);
      }
      else if ((p->info.typ->type == Tpointer || p->info.typ->type == Treference) && is_dynamic_array(p->info.typ->ref) && !is_binary(p->info.typ->ref))
      { if (has_ns(p->info.typ->ref) || is_untyped(p->info.typ->ref))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), xsi_type(p->info.typ->ref));
	  fprintf(fd, ">");
	}
        else
	{ d = get_Darraydims(p->info.typ->ref);
	  if (d)
	  { for (i = 0; i < d-1; i++)
	    { tmp[2*i] = ',';
	      tmp[2*i+1] = '1';
	    }
	    tmp[2*d-2] = '\0';
	  }
	  else
	    *tmp = '\0';
	  gen_element_begin(fd, n, ns_add(p->sym->name, nse), "SOAP-ENC:Array");
	  if ((((Tnode*)p->info.typ->ref)->type == Tstruct || ((Tnode*)p->info.typ->ref)->type == Tclass) && ((Table*)((Tnode*)p->info.typ->ref)->ref)->list->info.minOccurs > 1)
	    fprintf(fd, " SOAP-ENC:arrayType=\"%s[%ld%s]\">", wsdl_type(((Table*)((Tnode*)p->info.typ->ref)->ref)->list->info.typ, ""), ((Table*)((Tnode*)p->info.typ->ref)->ref)->list->info.minOccurs, tmp);
	  else
	    fprintf(fd, " SOAP-ENC:arrayType=\"%s[1%s]\">", wsdl_type(((Table*)((Tnode*)p->info.typ->ref)->ref)->list->info.typ, ""), tmp);
	}
        fflush(fd);
        gen_val(fd, n, p->info.typ->ref, nse, nsa, encoding);
      }
      else if (p->info.typ->type == Tstruct || p->info.typ->type == Tclass)
      { /*
        if (!is_primclass(p->info.typ))
        { char *nse1 = ns_qualifiedElement(p->info.typ);
  	  char *nsa1 = ns_qualifiedAttribute(p->info.typ);
	  if (nse1)
	    nse = nse1;
	  if (nsa1)
	    nsa = nsa1;
	}
	*/
        if (!is_invisible(p->sym->name))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), xsi_type_u(p->info.typ));
          gen_atts(fd, p->info.typ->ref, nsa);
        }
      }
      else if ((p->info.typ->type == Tpointer || p->info.typ->type == Treference)
             && (((Tnode*)p->info.typ->ref)->type == Tstruct || ((Tnode*)p->info.typ->ref)->type == Tclass))
      { /*
        if (!is_primclass(p->info.typ->ref))
        { char *nse1 = ns_qualifiedElement(p->info.typ->ref);
	  char *nsa1 = ns_qualifiedAttribute(p->info.typ->ref);
	  if (nse1)
	    nse = nse1;
	  if (nsa1)
	    nsa = nsa1;
	}
	*/
        if (!is_invisible(p->sym->name))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), xsi_type_u(p->info.typ));
          gen_atts(fd, ((Tnode*)p->info.typ->ref)->ref, nsa);
        }
      }
      else
      { if (!is_invisible(p->sym->name))
        { gen_element_begin(fd, n, ns_add(p->sym->name, nse), xsi_type_u(p->info.typ));
	  fprintf(fd, ">");
        }
      }
      switch (p->info.typ->type)
      { case Tchar:
        case Tshort:
        case Tint:
        case Tlong:
        case Tllong:
        case Tuchar:
        case Tushort:
        case Tuint:
        case Tulong:
        case Tullong:
          if (p->info.hasval)
	    fprintf(fd, SOAP_LONG_FORMAT, p->info.val.i);
	  else
	    fprintf(fd, "0");
	  break;
        case Tfloat:
        case Tdouble:
          if (p->info.hasval)
	    fprintf(fd, "%f", p->info.val.r);
	  else
	    fprintf(fd, "0.0");
          break;
        case Tenum:
	  if (p->info.hasval && p->info.typ->ref)
	  { for (q = ((Table*)p->info.typ->ref)->list; q; q = q->next)
	      if (p->info.val.i == q->info.val.i)
	      { fprintf(fd, "%s", ns_remove2(q->sym->name));
		break;
	      }
	  }
	  else
	    gen_val(fd, n+1, p->info.typ, nse, nsa, encoding);
	  break;
        case Tpointer:
        case Treference:
	  if (is_string(p->info.typ) || is_wstring(p->info.typ))
	  { if (p->info.hasval)
	      fprintf(fd, "%s", p->info.val.s);
	    else
	      gen_val(fd, n, p->info.typ, nse, nsa, encoding);
	  }
	  else if (!is_dynamic_array(p->info.typ->ref) && n < 10)
	    gen_val(fd, n, p->info.typ->ref, nse, nsa, encoding);
	  break;
        case Tclass:
	  if (is_stdstr(p->info.typ))
	  { if (p->info.hasval)
	      fprintf(fd, "%s", p->info.val.s);
	    else
	      gen_val(fd, n, p->info.typ, nse, nsa, encoding);
	    break;
	  }
        case Tstruct:
	  if (!is_dynamic_array(p->info.typ))
	    gen_val(fd, n, p->info.typ, nse, nsa, encoding);
	  break;
	case Ttemplate:
	  gen_val(fd, n, p->info.typ, nse, nsa, encoding);
	  break;
        }
	gen_element_end(fd, 0, ns_add(p->sym->name, nse));
        fflush(fd);
    }
  }
}

void
gen_atts(FILE *fd, Table *p, char *nsa)
{ Entry *q, *r;
  int i;
  if (p)
  for (q = p->list; q; q = q->next)
    if (q->info.sto & Sattribute)
    { fprintf(fd, " %s=\"", ns_add(q->sym->name, nsa));
      switch (q->info.typ->type)
      { case Tchar:
        case Tshort:
        case Tint:
        case Tlong:
        case Tllong:
        case Tuchar:
        case Tushort:
        case Tuint:
        case Tulong:
        case Tullong:
          if (q->info.hasval)
	    fprintf(fd, SOAP_LONG_FORMAT, q->info.val.i);
          else
            fprintf(fd, "0");
          break;
        case Tfloat:
        case Tdouble:
          if (q->info.hasval)
	    fprintf(fd, "%f", q->info.val.r);
	  else
	    fprintf(fd, "0.0");
          break;
        case Ttime:
	  break; /* should print value? */
        case Tenum:
	  if (q->info.hasval && q->info.typ->ref)
	  { for (r = ((Table*)q->info.typ->ref)->list; r; r = r->next)
	      if (r->info.val.i == q->info.val.i)
	      { fprintf(fd, "%s", ns_remove2(r->sym->name));
		break;
	      }
	  }
	  break;
        case Tpointer:
        case Treference:
	  if (is_string(q->info.typ))
	  { if (q->info.hasval)
	      fprintf(fd, "%s", q->info.val.s);
	    else if (q->info.typ->minLength > 0)
	      for (i = 0; i < q->info.typ->minLength; i++)
	        fprintf(fd, "X");
	  }
	  break;
        case Tclass:
	  if (is_stdstr(q->info.typ))
	  { if (q->info.hasval)
	      fprintf(fd, "%s", q->info.val.s);
	    else if (q->info.typ->minLength > 0)
	      for (i = 0; i < q->info.typ->minLength; i++)
	        fprintf(fd, "X");
          }
      }
      fprintf(fd, "\"");
    }
  fprintf(fd, ">");
  fflush(fd);
}

void
gen_val(FILE *fd, int n, Tnode *p, char *nse, char *nsa, char *encoding)
{ Entry *q;
  int i;
  if (!is_transient(p) && p->type != Tfun && !is_XML(p)) 
  { if (p->type == Tarray)
    { i = ((Tnode*) p->ref)->width;
      if (i)
      { i = p->width / i;
        if (i > 100)
          i = 100;
        fprintf(fd, "\n");
        for (; i > 0; i--)
        { fprintf(fd, "%*s<item>", n+1, "");
          gen_val(fd, n+1, p->ref, nse, nsa, encoding);
          fprintf(fd, "</item>\n");
        }
        fprintf(fd, "%*s", n, "");
      }
    }
    else if (is_dynamic_array(p))
    { if (!is_binary(p))
      { fprintf(fd, "\n");
        gen_field(fd, n+1, ((Table*)p->ref)->list, nse, nsa, encoding);
        fprintf(fd, "%*s", n, "");
      }
    }
    switch (p->type)
    { case Tchar:
      case Tshort:
      case Tint:
      case Tlong:
      case Tllong:
      case Tuchar:
      case Tushort:
      case Tuint:
      case Tulong:
      case Tullong:
	fprintf(fd, "0");
	break;
      case Tfloat:
      case Tdouble:
	fprintf(fd, "0.0");
        break;
      case Tenum:
        if (p->ref && (q = ((Table*)p->ref)->list))
          fprintf(fd, "%s", ns_remove(q->sym->name));
        else
          fprintf(fd, "0");
        break;
      case Ttime:
        { char tmp[256];
          time_t t = time(NULL), *p = &t;
          strftime(tmp, 256, "%Y-%m-%dT%H:%M:%SZ", gmtime(p));
	  fprintf(fd, "%s", tmp);
	}
	break;
      case Tpointer:
      case Treference:
	if (is_string(p) || is_wstring(p))
	{ if (p->minLength > 0)
	    for (i = 0; i < p->minLength; i++)
	      fprintf(fd, "X");
	}
        else if (n < 10)
	  gen_val(fd, n, p->ref, nse, nsa, encoding);
	break;
      case Tclass:
      case Tstruct:
	if (!is_dynamic_array(p) && !is_primclass(p) && p->ref)
        { nse = ns_qualifiedElement(p);
  	  nsa = ns_qualifiedAttribute(p);
	  fprintf(fd, "\n");
	  for (q = ((Table*)p->ref)->list; q; q = q->next)
	    gen_field(fd, n+1, q, nse, nsa, encoding);
	  fprintf(fd, "%*s", n, "");
        }
        break;
      case Ttemplate:
        gen_val(fd, n, p->ref, nse, nsa, encoding);
        break;
    }
  }
}

void
gen_header(FILE *fd, char *method, int response, char *encoding)
{ if (custom_header)
  { Service *sp;
    Method *m = NULL;
    Entry *q;
    Table *r;
    fprintf(fd, " <SOAP-ENV:Header>\n");
    r = entry(classtable, lookup("SOAP_ENV__Header"))->info.typ->ref;
    if (r)
    for (q = r->list; q; q = q->next)
      if (!is_transient(q->info.typ) && !(q->info.sto & Sattribute) && q->info.typ->type != Tfun)
      { for (sp = services; sp; sp = sp->next)
          for (m = sp->list; m; m = m->next)
	    if (is_eq(m->name, method) && (!strcmp(m->part, q->sym->name) || is_eq_nons(m->part, q->sym->name)) && (!response && (m->mess&HDRIN) || response && (m->mess&HDROUT)))
	    { gen_field(fd, 2, q, NULL, NULL, encoding);
	      break;
            }
      }
    fprintf(fd, " </SOAP-ENV:Header>\n");
  }
}

FILE *
gen_env(char *buf, char *method, int response, Table *t, char *ns, char *name, char *URL, char *executable, char *URI, char *encoding)
{ Symbol *s;
  Service *sp = NULL;
  char tmp[1024];
  FILE *fd;
  strcpy(tmp, buf);
  strcpy(strrchr(tmp, '.')+1, method);
  if (!response)
  { strcat(tmp, ".req.xml");
    fprintf(stderr, "Saving %s sample SOAP/XML request\n", tmp);
  }
  else
  { strcat(tmp, ".res.xml");
    fprintf(stderr, "Saving %s sample SOAP/XML response\n", tmp);
  }
  fd = fopen(tmp, "w");
  if (!fd)
    execerror("Cannot write XML file");
  fprintf(fd, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
  fprintf(fd, "<SOAP-ENV:Envelope");
  for (s = nslist; s; s = s->next)
  { for (sp = services; sp; sp = sp->next)
      if (!strcmp(sp->ns, s->name) && sp->URI)
        break;
    if (sp)
      fprintf(fd, "\n xmlns:%s=\"%s\"", ns_convert(s->name), sp->URI);
    else if (!strcmp(s->name, "SOAP-ENV"))
      fprintf(fd, "\n xmlns:SOAP-ENV=\"%s\"", envURI);
    else if (!strcmp(s->name, "SOAP-ENC"))
      fprintf(fd, "\n xmlns:SOAP-ENC=\"%s\"", encURI);
    else if (!strcmp(s->name, "xsi"))
      fprintf(fd, "\n xmlns:xsi=\"%s\"", xsiURI);
    else if (!strcmp(s->name, "xsd"))
      fprintf(fd, "\n xmlns:xsd=\"%s\"", xsdURI);
    else
      fprintf(fd, "\n xmlns:%s=\"%s/%s.xsd\"", ns_convert(s->name), URI, ns_convert(s->name));
  }
  fprintf(fd, ">\n");
  gen_header(fd, method, response, encoding);
  fprintf(fd, " <SOAP-ENV:Body");
  if (eflag && !encoding)
    fprintf(fd, " SOAP-ENV:encodingStyle=\"%s\"", encURI);
  else if (encoding && !*encoding)
    fprintf(fd, " SOAP-ENV:encodingStyle=\"%s\"", encURI);
  else if (encoding && strcmp(encoding, "literal"))
    fprintf(fd, " SOAP-ENV:encodingStyle=\"%s\"", encoding);
  fprintf(fd, ">\n");
  return fd;
}

char *
emalloc(unsigned int n)
{ char	*p;
  if ((p = (char*)malloc(n)) == NULL)
    execerror("out of memory");
  return p;
}

void
soap_serve(Table *table)
{ int i=1;
  Entry * p;
  fprintf(fserver,"\n\nSOAP_FMAC5 int SOAP_FMAC6 %s_serve(struct soap *soap)", nflag?prefix:"soap"); 
  fprintf(fserver,"\n{\n\tunsigned int n = soap->max_keep_alive;\n\tdo\n\t{\tsoap_begin(soap);");

  fprintf(fserver,"\n\t\tif (!--n)\n\t\t\tsoap->keep_alive = 0;");
  fprintf(fserver,"\n\t\tif (soap_begin_recv(soap))\n\t\t{\tif (soap->error < SOAP_STOP)\n\t\t\t\treturn soap_send_fault(soap);\n\t\t\tsoap_closesock(soap);\n\t\t\tcontinue;\n\t\t}");
  fprintf(fserver,"\n\t\tif (soap_envelope_begin_in(soap)\n\t\t || soap_recv_header(soap)\n\t\t || soap_body_begin_in(soap)\n\t\t || %s_serve_request(soap) || (soap->fserveloop && soap->fserveloop(soap)))\n\t\t\treturn soap_send_fault(soap);\n\t\t", nflag?prefix:"soap");
  fprintf(fserver,"\n\t} while (soap->keep_alive);");
  fprintf(fserver,"\n\treturn SOAP_OK;");
  fprintf(fserver,"\n}");

  fprintf(fserver,"\n\nSOAP_FMAC5 int SOAP_FMAC6 %s_serve_request(struct soap *soap)\n{", nflag?prefix:"soap");
  fprintf(fserver, "\n\tsoap_peek_element(soap);");
  for (p = table->list; p != (Entry*) 0; p = p->next)
    {
      if (p->info.typ->type == Tfun && !(p->info.sto & Sextern))
	{   if (is_invisible(p->sym->name))
	    { Entry *q = entry(classtable, p->sym);
	      if (q)
	      { q = ((Table*)q->info.typ->ref)->list;
	        if (q)
		{ fprintf(fserver,"\n\tif (!soap_match_tag(soap, soap->tag, \"%s\"))", ns_convert(q->sym->name));
	          fprintf(fserver,"\n\t\treturn soap_serve_%s(soap);", p->sym->name);
	        }
	      }
	    }
	    else
	    { fprintf(fserver,"\n\tif (!soap_match_tag(soap, soap->tag, \"%s\"))", ns_convert(p->sym->name));
	      fprintf(fserver,"\n\t\treturn soap_serve_%s(soap);", p->sym->name);
	    }
	}
    }
  fprintf(fserver,"\n\treturn soap->error = SOAP_NO_METHOD;\n}");

  fprintf(fheader, "\n\n/* Remote Methods */");
  for (p = table->list; p != (Entry*) 0; p = p->next)
    if (p->info.typ->type==Tfun && !(p->info.sto & Sextern))
	generate_proto(table, p);
  fprintf(fheader, "\n\n/* Stubs */");
  for (p = table->list; p != (Entry*) 0; p = p->next)
    if (p->info.typ->type==Tfun && !(p->info.sto & Sextern) && !is_imported(p->info.typ))
	generate_call(table, p);
  fprintf(fheader, "\n\n/* Skeletons */\n\nSOAP_FMAC5 int SOAP_FMAC6 %s_serve(struct soap*);", nflag?prefix:"soap");
  fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 %s_serve_request(struct soap*);", nflag?prefix:"soap");
  for (p = table->list; p != (Entry*) 0; p = p->next)
    if (p->info.typ->type==Tfun && !(p->info.sto & Sextern) && !is_imported(p->info.typ))
	generate_server(table, p);
}

void
generate_proto(Table *table, Entry *param)
{ Entry *pin,*q,*pout;
  Table *output,*t;
  q=entry(table, param->sym);
  if (q)
    pout = (Entry*)q->info.typ->ref;
  else
    fprintf(stderr, "Internal error: no table entry\n");
  q=entry(classtable, param->sym);
  output=(Table*) q->info.typ->ref;
  fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 %s(struct soap*",param->sym->name);
  for(t=output;t!=(Table*) 0;t=t->prev)
  { for (pin = t->list; pin != (Entry*) 0; pin = pin->next)
    {	if (pin->info.typ->type == Treference)
    	{	sprintf(errbuf, "Cannot pass input argument \"%s\" of remote method function \"%s\" by reference: use pointer", pin->sym->name, param->sym->name);
		semerror(errbuf);
	}
	  fprintf(fheader,", %s%s",c_storage(pin->info.sto), c_type(pin->info.typ));
    }
  }
  if (is_transient(pout->info.typ))
    fprintf(fheader,");");
  else
    fprintf(fheader,", %s%s);", c_storage(pout->info.sto), c_type(pout->info.typ));
}

void
generate_call(Table *table, Entry *param)
{ Service *sp;
  char *style, *encoding;
  Entry *pin,*q,*pout,*response=NULL;
  Tnode *temp;
  Table *output,*t;
  char *xtag, *xtyp;
  char *s, *action = NULL, *method_style = NULL, *method_encoding = NULL, *method_response_encoding = NULL;
  Method *m;
  int cardinality, element_width, i, flag = 0;
  q=entry(table, param->sym);
  if (q)
    pout = (Entry*)q->info.typ->ref;
  else
    fprintf(stderr, "Internal error: no table entry\n");
  q=entry(classtable, param->sym);
  output=(Table*) q->info.typ->ref;

  if (!is_response(pout->info.typ) && !is_XML(pout->info.typ))
  { response = get_response(param->info.typ);
  }
  
  /* soapStub.h*/
  if (is_transient(pout->info.typ))
  { fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_send_%s(struct soap*, const char*, const char*",param->sym->name);
    fprintf(fclient, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_send_%s(struct soap *soap, const char *URL, const char *action",param->sym->name);
  }
  else
  { fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_call_%s(struct soap*, const char*, const char*",param->sym->name);
    fprintf(fclient, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_call_%s(struct soap *soap, const char *URL, const char *action",param->sym->name);
  }
  fflush(fheader);
  fflush(fclient);

  /* Parameters being passed */
  for(t=output;t!=(Table*) 0;t=t->prev)
  { for (pin = t->list; pin != (Entry*) 0; pin = pin->next)
    { fprintf(fclient,", %s%s",c_storage(pin->info.sto), c_type_id(pin->info.typ,pin->sym->name));
      fprintf(fheader,", %s%s",c_storage(pin->info.sto), c_type(pin->info.typ));
    }
  }

  /* Return value */
  fflush(fclient);
  if (!is_transient(pout->info.typ))
  { fprintf(fclient, ", %s%s)\n{", c_storage(pout->info.sto), c_type_id(pout->info.typ, pout->sym->name));
    fprintf(fheader,", %s%s);", c_storage(pout->info.sto), c_type(pout->info.typ));
  }
  else
  { fprintf(fclient, ")\n{");
    fprintf(fheader,");");
  }
  fflush(fclient);
  
  fprintf(fclient,"\n\tstruct %s soap_tmp_%s;",param->sym->name,param->sym->name);
  if (!is_response(pout->info.typ) && response)
  { fprintf(fclient,"\n\tstruct %s *soap_tmp_%s;",c_ident(response->info.typ), c_ident(response->info.typ));
  } 
  for (sp = services; sp; sp = sp->next)
  { if (has_ns_eq(sp->ns, param->sym->name))
    { style = sp->style;
      encoding = sp->encoding;
      method_style = style;
      method_encoding = encoding;
      method_response_encoding = NULL;
      for (m = sp->list; m; m = m->next)
        if (is_eq_nons(m->name, param->sym->name))
          if (m->mess == ACTION)
            action = m->part;
          else if (m->mess == ENCODING)
            method_encoding = m->part;
          else if (m->mess == RESPONSE_ENCODING)
            method_response_encoding = m->part;
          else if (m->mess == STYLE)
            method_style = m->part;
      break;
    }
  }
  if (!method_response_encoding)
    method_response_encoding = method_encoding;
  if (sp && sp->URI && method_encoding)
  { flag = 1;
    /*
    fprintf(fclient, "\n\tconst char *soap_bak_encodingStyle = soap->encodingStyle;");
    */
    if (is_literal(method_encoding))
      fprintf(fclient, "\n\tsoap->encodingStyle = NULL;");
    else if (method_encoding)
      fprintf(fclient, "\n\tsoap->encodingStyle = \"%s\";", method_encoding);
  }
  else if (!eflag)
    fprintf(fclient, "\n\tsoap->encodingStyle = NULL;");
  if (sp && sp->URL)
  { fprintf(fclient, "\n\tif (!URL)\n\t\tURL = \"%s\";", sp->URL);
  }
  if (action)
  { fprintf(fclient, "\n\tif (!action)\n\t\taction = ");
    if (*action == '"')
      fprintf(fclient, "%s;", action);
    else
      fprintf(fclient, "\"%s\";", action);
  }
  fflush(fclient);
  for(t=output;t!=(Table*) 0;t=t->prev) 
    for (pin = t->list; pin != (Entry*) 0; pin = pin->next)
      {
	if(pin->info.typ->type==Tarray)
	  {
	    temp = pin->info.typ;
	    cardinality = 0;
	    while(temp->type == Tarray){
	      cardinality ++;
	      temp = temp->ref;
	    }
	    element_width = temp->width;
	    
	    fprintf(fclient,"\n\tmemcpy(&(soap_tmp_%s.%s",
		    param->sym->name,pin->sym->name);
	    for(i=0;i<cardinality;i++){
	      fprintf(fclient,"[0]");
	    }
	    fprintf(fclient,"), ");
            fprintf(fclient,"&(%s", pin->sym->name);
            for(i=0;i<cardinality;i++){
              fprintf(fclient,"[0]");
            }
            fprintf(fclient,"), %d*sizeof(%s));", (pin->info.typ->width)/(element_width), c_type(temp));      
	  }			
	else fprintf (fclient,"\n\tsoap_tmp_%s.%s=%s;",
		      param->sym->name,pin->sym->name,pin->sym->name);
      }	
  fprintf(fclient,"\n\tsoap_begin(soap);");
  fprintf(fclient,"\n\tsoap_serializeheader(soap);");
  fprintf(fclient,"\n\tsoap_serialize_%s(soap, &soap_tmp_%s);", param->sym->name,param->sym->name);
  fprintf(fclient,"\n\tsoap_begin_count(soap);");
  fprintf(fclient,"\n\tif (soap->mode & SOAP_IO_LENGTH)");
  fprintf(fclient,"\n\t{\tsoap_envelope_begin_out(soap);");
  fprintf(fclient,"\n\t\tsoap_putheader(soap);");
  fprintf(fclient,"\n\t\tsoap_body_begin_out(soap);");
  fprintf(fclient,"\n\t\tsoap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\");", param->sym->name,param->sym->name, ns_convert(param->sym->name)); 
  fprintf(fclient,"\n\t\tsoap_body_end_out(soap);");
  fprintf(fclient,"\n\t\tsoap_envelope_end_out(soap);");
  fprintf(fclient,"\n\t}");
  fprintf(fclient,"\n\tif (soap_connect(soap, URL, action)");
  fprintf(fclient,"\n\t || soap_envelope_begin_out(soap)");
  fprintf(fclient,"\n\t || soap_putheader(soap)");
  fprintf(fclient,"\n\t || soap_body_begin_out(soap)");
  fprintf(fclient,"\n\t || soap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\")", param->sym->name,param->sym->name,ns_convert(param->sym->name)); 
  fprintf(fclient,"\n\t || soap_body_end_out(soap)");
  fprintf(fclient,"\n\t || soap_envelope_end_out(soap)");
  fprintf(fclient,"\n\t || soap_end_send(soap))");
  /*
  if (flag)
  { fprintf(fclient, "\n\t{\tsoap->encodingStyle = soap_bak_encodingStyle;");
    fprintf(fclient, "\n\t\treturn soap_closesock(soap);\n\t}");
  }
  else
  */
    fprintf(fclient, "\n\t\treturn soap_closesock(soap);");
  fflush(fclient);
  
  if (is_transient(pout->info.typ))
  { fprintf(fclient, "\n\treturn SOAP_OK;\n}");
    fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_recv_%s(struct soap*, ",param->sym->name);
    fprintf(fclient, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_recv_%s(struct soap *soap, ",param->sym->name);
    fprintf(fheader,"struct %s*);\n",param->sym->name);
    fprintf(fclient,"struct %s *%s)\n{",param->sym->name,pout->sym->name);
    fprintf(fclient,"\n\tsoap_default_%s(soap, %s);", param->sym->name,pout->sym->name);
    fprintf(fclient,"\n\tsoap_begin(soap);");
  }
  else if(pout->info.typ->type == Tarray)
    fprintf(fclient,"\n\tsoap_default_%s(soap, %s);", c_ident(pout->info.typ), pout->sym->name);
  else if (pout->info.typ->type == Treference && ((Tnode*)pout->info.typ->ref)->type == Tclass && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref))
    fprintf(fclient,"\n\tif (!&%s)\n\t\treturn soap_closesock(soap);\n\t%s.soap_default(soap);", pout->sym->name, pout->sym->name);
  else if (((Tnode*)pout->info.typ->ref)->type == Tclass && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref))
    fprintf(fclient,"\n\tif (!%s)\n\t\treturn soap_closesock(soap);\n\t%s->soap_default(soap);", pout->sym->name, pout->sym->name);
  else if (pout->info.typ->type == Treference && ((Tnode*)pout->info.typ->ref)->type == Tpointer)
    fprintf(fclient,"\n\t%s = NULL;", pout->sym->name);
  else if (((Tnode*)pout->info.typ->ref)->type == Tpointer)
    fprintf(fclient,"\n\t*%s = NULL;", pout->sym->name);
  else if (pout->info.typ->type == Treference)
    fprintf(fclient,"\n\tsoap_default_%s(soap, &%s);", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name);
  else if (!is_void(pout->info.typ))
    fprintf(fclient,"\n\tsoap_default_%s(soap, %s);", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name);
  fflush(fclient);

  fprintf(fclient,"\n\tif (soap_begin_recv(soap)");
  fprintf(fclient,"\n\t || soap_envelope_begin_in(soap)");
  fprintf(fclient,"\n\t || soap_recv_header(soap)");
  fprintf(fclient,"\n\t || soap_body_begin_in(soap))");
  fprintf(fclient,"\n\t\treturn soap_closesock(soap);");
  fflush(fclient);

  if (is_transient(pout->info.typ))
  {
  fprintf(fclient,"\n\tsoap_get_%s(soap, %s, \"%s\", NULL);", param->sym->name, pout->sym->name, ns_convert(param->sym->name));
  fprintf(fclient,"\n\tif (soap->error == SOAP_TAG_MISMATCH && soap->level == 2)\n\t\tsoap->error = SOAP_NO_METHOD;");
  fprintf(fclient,"\n\tif (soap->error");
  fprintf(fclient,"\n\t || soap_body_end_in(soap)");
  fprintf(fclient,"\n\t || soap_envelope_end_in(soap)");
  fprintf(fclient,"\n#ifndef WITH_LEANER");
  fprintf(fclient,"\n\t || soap_resolve_attachments(soap)");
  fprintf(fclient,"\n#endif");
  fprintf(fclient,"\n\t || soap_end_recv(soap))");
  fprintf(fclient,"\n\t\treturn soap_closesock(soap);");
  fprintf(fclient,"\n\treturn soap_closesock(soap);\n}");
  fflush(fclient);
  return;
  }
  
  if (has_ns_eq(NULL, pout->sym->name))
  { xtyp = xsi_type(pout->info.typ);
    xtag = ns_convert(pout->sym->name);
  }
  else
  { xtyp = "";
    xtag = xml_tag(pout->info.typ);
  }
  if (pout->info.typ->type == Treference && ((Tnode *) pout->info.typ->ref)->type == Tclass && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fclient,"\n\t%s.soap_get(soap, \"%s\", \"%s\");", pout->sym->name, xtag, xtyp);
  else if (pout->info.typ->type == Tpointer && ((Tnode *) pout->info.typ->ref)->type == Tclass && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fclient,"\n\t%s->soap_get(soap, \"%s\", \"%s\");", pout->sym->name, xtag, xtyp);
  else if (pout->info.typ->type == Treference && ((Tnode *) pout->info.typ->ref)->type == Tstruct && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
  { fprintf(fclient,"\n\tsoap_get_%s(soap, &%s, \"%s\", \"%s\");", c_ident(pout->info.typ->ref), pout->sym->name, xtag, xtyp);
  }
  else if (pout->info.typ->type == Tpointer && ((Tnode *) pout->info.typ->ref)->type == Tstruct && !is_dynamic_array(pout->info.typ->ref))
  { 
    fprintf(fclient,"\n\tsoap_get_%s(soap, %s, \"%s\", \"%s\");", c_ident(pout->info.typ->ref), pout->sym->name, xtag, xtyp);
  }
  else if (pout->info.typ->type == Tpointer && is_XML(pout->info.typ->ref) && is_string(pout->info.typ->ref))
  { fprintf(fclient,"\n\tsoap_inliteral(soap, NULL, %s);", pout->sym->name);
  }
  else if (pout->info.typ->type == Treference && is_XML(pout->info.typ->ref) && is_string(pout->info.typ->ref))
  { fprintf(fclient,"\n\tsoap_inliteral(soap, NULL, &%s);", pout->sym->name);
  }
  else if (pout->info.typ->type == Tpointer && is_XML(pout->info.typ->ref) && is_wstring(pout->info.typ->ref))
  { fprintf(fclient,"\n\tsoap_inwliteral(soap, NULL, %s);", pout->sym->name);
  }
  else if (pout->info.typ->type == Treference && is_XML(pout->info.typ->ref) && is_wstring(pout->info.typ->ref))
  { fprintf(fclient,"\n\tsoap_inwliteral(soap, NULL, &%s);", pout->sym->name);
  }
  else if (response)
  { fprintf(fclient,"\n\tsoap_tmp_%s = soap_get_%s(soap, NULL, \"%s\", \"\");", c_ident(response->info.typ), c_ident(response->info.typ), xml_tag(response->info.typ));
  }
  else if (pout->info.typ->type == Treference)
  { fprintf(fclient,"\n\tsoap_get_%s(soap, &%s, \"%s\", \"%s\");", c_ident(pout->info.typ), pout->sym->name, xtag, xtyp);
  }
  else
  { fprintf(fclient,"\n\tsoap_get_%s(soap, %s, \"%s\", \"%s\");", c_ident(pout->info.typ), pout->sym->name, xtag, xtyp);
  }
  fflush(fclient);
  fprintf(fclient,"\n\tif (soap->error)");
  fprintf(fclient,"\n\t{\tif (soap->error == SOAP_TAG_MISMATCH && soap->level == 2)\n\t\t\treturn soap_recv_fault(soap);");
  fprintf(fclient,"\n\t\treturn soap_closesock(soap);");
  fprintf(fclient,"\n\t}");

  fprintf(fclient,"\n\tif (soap_body_end_in(soap)");
  fprintf(fclient,"\n\t || soap_envelope_end_in(soap)");
  fprintf(fclient,"\n#ifndef WITH_LEANER");
  fprintf(fclient,"\n\t || soap_resolve_attachments(soap)");
  fprintf(fclient,"\n#endif");
  fprintf(fclient,"\n\t || soap_end_recv(soap))");
  fprintf(fclient,"\n\t\treturn soap_closesock(soap);");
  if (!is_response(pout->info.typ) && response)
  { if (pout->info.typ->type == Tarray)
	{    temp = pout->info.typ;
	    cardinality = 0;
	    while(temp->type == Tarray){
	      cardinality ++;
	      temp = temp->ref;
	    }
	    element_width = temp->width;
	    
	    fprintf(fclient,"\n\tmemcpy(&(%s", pout->sym->name);
	    for(i=0;i<cardinality;i++){
	      fprintf(fclient,"[0]");
	    }
	    fprintf(fclient,"), ");
            fprintf(fclient,"&(soap_tmp_%s->%s", c_ident(response->info.typ), pout->sym->name);
            for(i=0;i<cardinality;i++){
              fprintf(fclient,"[0]");
            }
            fprintf(fclient,"), %d*sizeof(%s));", (pout->info.typ->width)/(element_width), c_type(temp));      
	}
    else if (pout->info.typ->type == Treference)
      fprintf(fclient,"\n\t%s = soap_tmp_%s->%s;", pout->sym->name, c_ident(response->info.typ), pout->sym->name);
    else if (!is_external(pout->info.typ->ref))
    { fprintf(fclient,"\n\tif (%s && soap_tmp_%s->%s)", pout->sym->name, c_ident(response->info.typ), pout->sym->name);
      fprintf(fclient,"\n\t\t*%s = *soap_tmp_%s->%s;", pout->sym->name, c_ident(response->info.typ), pout->sym->name);
      /* if (((Tnode*)pout->info.typ->ref)->type != Tclass)
        fprintf(fclient,"\n\telse\n\t\tsoap_default_%s(soap, %s);", c_ident(pout->info.typ->ref), pout->sym->name);
      */
    }
  }
  fprintf(fclient,"\n\treturn soap_closesock(soap);");
  fprintf(fclient ,"\n}");
  fflush(fclient);
}

void
generate_server(Table *table, Entry *param)
{ Service *sp = NULL;
  char *style, *encoding;
  int flag = 0;
  Entry *q,*pin, *pout, *response;
  Table *t, *input;
  char *xtag;
  Method *m;
  char *s, *method_style = NULL, *method_encoding = NULL, *method_response_encoding = NULL;
  q=entry(table, param->sym);
  if (q)
    pout = (Entry*)q->info.typ->ref;
  else
    fprintf(stderr, "Internal error: no table entry");
  if (!is_response(pout->info.typ))
    response = get_response(param->info.typ);
  fprintf(fheader, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_serve_%s(struct soap*);",param->sym->name);
  fprintf(fserver, "\n\nSOAP_FMAC5 int SOAP_FMAC6 soap_serve_%s(struct soap *soap)",param->sym->name);
  fprintf(fserver, "\n{\tstruct %s soap_tmp_%s;", param->sym->name, param->sym->name);
  for (sp = services; sp; sp = sp->next)
    if (has_ns_eq(sp->ns, param->sym->name))
    { style = sp->style;
      encoding = sp->encoding;
      method_style = style;
      method_encoding = encoding;
      method_response_encoding = NULL;
      for (m = sp->list; m; m = m->next)
	if (is_eq_nons(m->name, param->sym->name))
	  if (m->mess == ENCODING)
	    method_encoding = m->part;
	  else if (m->mess == RESPONSE_ENCODING)
	    method_response_encoding = m->part;
	  else if (m->mess == STYLE)
	    method_style = m->part;
      break;
    }
  if (!method_response_encoding)
    method_response_encoding = method_encoding;
  fflush(fserver);
  if (is_transient(pout->info.typ))
    ;
  else if(pout->info.typ->type == Tarray){
    fprintf(fserver,"\n\tstruct %s soap_tmp_%s;", c_ident(response->info.typ), c_ident(response->info.typ));
    fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", c_ident(response->info.typ),c_ident(response->info.typ));
  }else if(((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && (is_external(pout->info.typ->ref) || is_volatile(pout->info.typ->ref)) && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\t%s;", c_type_id((Tnode*)pout->info.typ->ref, pout->sym->name));
    fprintf(fserver,"\n\tsoap_default_%s(soap, &%s);", c_ident((Tnode *)pout->info.typ->ref),pout->sym->name);  
  }else if(((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\t%s;", c_type_id((Tnode*)pout->info.typ->ref, pout->sym->name));
    fprintf(fserver,"\n\t%s.soap_default(soap);", pout->sym->name);
  }else if(((Tnode *)pout->info.typ->ref)->type == Tstruct && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\t%s;", c_type_id((Tnode*)pout->info.typ->ref, pout->sym->name));
    fprintf(fserver,"\n\tsoap_default_%s(soap, &%s);", c_ident((Tnode *)pout->info.typ->ref),pout->sym->name);  
  } else if (pout->info.typ->type == Tpointer && !is_stdstring(pout->info.typ) && !is_stdwstring(pout->info.typ) && response)
  {
    fprintf(fserver,"\n\tstruct %s soap_tmp_%s;", c_ident(response->info.typ), c_ident(response->info.typ));
    fprintf(fserver,"\n\t%s soap_tmp_%s;", c_type(pout->info.typ->ref), c_ident(pout->info.typ->ref));
    fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", c_ident(response->info.typ),c_ident(response->info.typ));
    if (((Tnode*)pout->info.typ->ref)->type == Tclass && !is_external(pout->info.typ->ref) && !is_volatile(pout->info.typ->ref))
      fprintf(fserver,"\n\tsoap_tmp_%s.soap_default(soap);", c_ident(pout->info.typ->ref));
    else if (((Tnode*)pout->info.typ->ref)->type == Tpointer)
      fprintf(fserver,"\n\tsoap_tmp_%s = NULL;", c_ident(pout->info.typ->ref));
    else
      fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", c_ident(pout->info.typ->ref),c_ident(pout->info.typ->ref));
    fprintf(fserver,"\n\tsoap_tmp_%s.%s = &soap_tmp_%s;", c_ident(response->info.typ),pout->sym->name, c_ident(pout->info.typ->ref));
  } else if (response)
  {
    fprintf(fserver,"\n\tstruct %s soap_tmp_%s;", c_ident(response->info.typ), c_ident(response->info.typ));
    fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", c_ident(response->info.typ),c_ident(response->info.typ));
  }
  else
  { fprintf(fserver,"\n\t%s soap_tmp_%s;", c_type(pout->info.typ->ref), c_ident(pout->info.typ->ref));
    if (is_string(pout->info.typ->ref) || is_wstring(pout->info.typ->ref))
      fprintf(fserver,"\n\tsoap_tmp_%s = NULL;", c_ident(pout->info.typ->ref));
    else
      fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", c_ident(pout->info.typ->ref), c_ident(pout->info.typ->ref));
  }
  fprintf(fserver,"\n\tsoap_default_%s(soap, &soap_tmp_%s);", param->sym->name,param->sym->name);
  fflush(fserver);
  if (sp && sp->URI && method_response_encoding)
  {     if (is_literal(method_response_encoding))
          fprintf(fserver, "\n\tsoap->encodingStyle = NULL;");
        else if (sp->encoding)
          fprintf(fserver, "\n\tsoap->encodingStyle = \"%s\";", sp->encoding);
	else if (method_response_encoding)
	  fprintf(fserver, "\n\tsoap->encodingStyle = \"%s\";", method_response_encoding);
        else if (!eflag)
          fprintf(fserver, "\n\tsoap->encodingStyle = NULL;");
  }
  else if (!eflag)
    fprintf(fserver, "\n\tsoap->encodingStyle = NULL;");
  fprintf(fserver,"\n\tsoap_get_%s(soap, &soap_tmp_%s, \"%s\", NULL);", param->sym->name, param->sym->name, ns_convert(param->sym->name));
  fprintf(fserver,"\n\tif (soap->error)");
  fprintf(fserver,"\n\t\treturn soap->error;");
  fprintf(fserver,"\n\t");
  fprintf(fserver,"\n\tif (soap_body_end_in(soap)");
  fprintf(fserver,"\n\t || soap_envelope_end_in(soap)");
  fprintf(fserver,"\n#ifndef WITH_LEANER");
  fprintf(fserver,"\n\t || soap_resolve_attachments(soap)");
  fprintf(fserver,"\n#endif");
  fprintf(fserver,"\n\t || soap_end_recv(soap))\n\t\treturn soap->error;");
  fprintf(fserver, "\n\tsoap->error = %s(soap",param->sym->name);
  fflush(fserver);
  q=entry(classtable, param->sym);
  input=(Table*) q->info.typ->ref;

  for (t = input; t; t = t->prev) 
    for (pin = t->list; pin; pin = pin->next)
      fprintf(fserver,", soap_tmp_%s.%s",param->sym->name, pin->sym->name);

  if (is_transient(pout->info.typ))
    fprintf(fserver, ");");
  else if (pout->info.typ->type == Tarray)
    fprintf(fserver, ", soap_tmp_%s.%s);", c_ident(response->info.typ), pout->sym->name);
  else if (pout->info.typ->type == Treference && (((Tnode*)pout->info.typ->ref)->type == Tstruct || ((Tnode*)pout->info.typ->ref)->type == Tclass) && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, ", %s);", pout->sym->name);
  else if ((((Tnode*)pout->info.typ->ref)->type == Tstruct || ((Tnode*)pout->info.typ->ref)->type == Tclass) && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, ", &%s);", pout->sym->name);
  else if(pout->info.typ->type == Treference && response)
    fprintf(fserver, ", soap_tmp_%s.%s);", c_ident(response->info.typ), pout->sym->name);
  else if(pout->info.typ->type == Treference)
    fprintf(fserver, ", soap_tmp_%s);", c_ident(pout->info.typ->ref));
  else
    fprintf(fserver, ", &soap_tmp_%s);", c_ident(pout->info.typ->ref));
  fprintf(fserver,"\n\tif (soap->error)\n\t\treturn soap->error;");
  
  if (!is_transient(pout->info.typ))
  {
  fprintf(fserver,"\n\tsoap_serializeheader(soap);");
  if(pout->info.typ->type == Tarray){
    fprintf(fserver, "\n\tsoap_serialize_%s(soap, &soap_tmp_%s);", c_ident(response->info.typ), c_ident(response->info.typ));
  }else if(((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && (is_external(pout->info.typ->ref) || is_volatile(pout->info.typ->ref)) && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\tsoap_serialize_%s(soap, &%s);", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name);
  }else if(((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\t%s.soap_serialize(soap);", pout->sym->name);
  }else if(((Tnode *)pout->info.typ->ref)->type == Tstruct && !is_dynamic_array(pout->info.typ->ref)){
    fprintf(fserver, "\n\tsoap_serialize_%s(soap, &%s);", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name);
  }else if (response)
  {
    fprintf(fserver, "\n\tsoap_serialize_%s(soap, &soap_tmp_%s);", c_ident(response->info.typ), c_ident(response->info.typ));
  }
  else if (!is_XML(pout->info.typ->ref))
    fprintf(fserver, "\n\tsoap_serialize_%s(soap, &soap_tmp_%s);", c_ident(pout->info.typ->ref), c_ident(pout->info.typ->ref));
  if (has_ns_eq(NULL, pout->sym->name))
    xtag = ns_convert(pout->sym->name);
  else
    xtag = xml_tag(pout->info.typ);
  fprintf(fserver, "\n\tsoap_begin_count(soap);");
  fprintf(fserver, "\n\tif (soap->mode & SOAP_IO_LENGTH)");
  fprintf(fserver, "\n\t{\tsoap_envelope_begin_out(soap);");
  fprintf(fserver,"\n\t\tsoap_putheader(soap);");
  fprintf(fserver,"\n\t\tsoap_body_begin_out(soap);");
  if (pout->info.typ->type == Tarray)
    fprintf(fserver,"\n\t\tsoap_put_%s(soap, &soap_tmp_%s.%s, \"%s\", \"\");", c_ident(response->info.typ), c_ident(response->info.typ), pout->sym->name, xtag);
  else if (((Tnode*)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && (is_external(pout->info.typ->ref) || is_volatile(pout->info.typ->ref)) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t\tsoap_put_%s(soap, &%s, \"%s\", \"\");", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name, ns_convert(pout->sym->name));
  else if (((Tnode*)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t\t%s.soap_put(soap, \"%s\", \"\");", pout->sym->name, xtag);
  else if (((Tnode*)pout->info.typ->ref)->type == Tstruct && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t\tsoap_put_%s(soap, &%s, \"%s\", \"\");", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name, xtag);
  else if (response)
    fprintf(fserver,"\n\t\tsoap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\");", c_ident(response->info.typ), c_ident(response->info.typ), xml_tag(response->info.typ));
  else if (is_XML(pout->info.typ->ref) && is_string(pout->info.typ->ref))
    fprintf(fserver,"\n\t\tsoap_outliteral(soap, \"%s\", &soap_tmp_%s);", ns_convert(pout->sym->name), c_ident(pout->info.typ->ref));
  else if (is_XML(pout->info.typ->ref) && is_wstring(pout->info.typ->ref))
    fprintf(fserver,"\n\t\tsoap_outwliteral(soap, \"%s\", &soap_tmp_%s);", ns_convert(pout->sym->name), c_ident(pout->info.typ->ref));
  else
    fprintf(fserver,"\n\t\tsoap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\");", c_ident(pout->info.typ), c_ident(pout->info.typ->ref), ns_convert(pout->sym->name));
  fprintf(fserver,"\n\t\tsoap_body_end_out(soap);");
  fprintf(fserver,"\n\t\tsoap_envelope_end_out(soap);");
  fprintf(fserver,"\n\t};");

  fprintf(fserver,"\n\tif (soap_response(soap, SOAP_OK)");
  fprintf(fserver,"\n\t || soap_envelope_begin_out(soap)");
  fprintf(fserver,"\n\t || soap_putheader(soap)");
  fprintf(fserver,"\n\t || soap_body_begin_out(soap)");
  if (pout->info.typ->type == Tarray)
    fprintf(fserver,"\n\t || soap_put_%s(soap, &soap_tmp_%s.%s, \"%s\", \"\")", c_ident(response->info.typ), c_ident(response->info.typ), pout->sym->name, xtag);
  else if (((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && (is_external(pout->info.typ->ref) || is_volatile(pout->info.typ->ref)) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t || soap_put_%s(soap, &%s, \"%s\", \"\")", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name, ns_convert(pout->sym->name));
  else if(((Tnode *)pout->info.typ->ref)->type == Tclass && !is_stdstring(pout->info.typ->ref) && !is_stdwstring(pout->info.typ->ref) && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t || %s.soap_put(soap, \"%s\", \"\")", pout->sym->name, xtag);
  else if(((Tnode *)pout->info.typ->ref)->type == Tstruct && !is_dynamic_array(pout->info.typ->ref))
    fprintf(fserver, "\n\t || soap_put_%s(soap, &%s, \"%s\", \"\")", c_ident((Tnode*)pout->info.typ->ref), pout->sym->name, xtag);
  else if (response)
    fprintf(fserver,"\n\t || soap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\")", c_ident(response->info.typ), c_ident(response->info.typ), xml_tag(response->info.typ));
  else if (is_XML(pout->info.typ->ref) && is_string(pout->info.typ->ref))
    fprintf(fserver,"\n\t || soap_outliteral(soap, \"%s\", &soap_tmp_%s)", ns_convert(pout->sym->name), c_ident(pout->info.typ->ref));
  else if (is_XML(pout->info.typ->ref) && is_wstring(pout->info.typ->ref))
    fprintf(fserver,"\n\t || soap_outwliteral(soap, \"%s\", &soap_tmp_%s)", ns_convert(pout->sym->name), c_ident(pout->info.typ->ref));
  else
    fprintf(fserver,"\n\t || soap_put_%s(soap, &soap_tmp_%s, \"%s\", \"\")", c_ident(pout->info.typ), c_ident(pout->info.typ->ref), ns_convert(pout->sym->name));
  fprintf(fserver,"\n\t || soap_body_end_out(soap)");
  fprintf(fserver,"\n\t || soap_envelope_end_out(soap)");
  fprintf(fserver,"\n\t || soap_end_send(soap))");
  fprintf(fserver, "\n\t\treturn soap->error;");
  }

  fprintf(fserver,"\n\tsoap_closesock(soap);");
  fprintf(fserver,"\n\treturn SOAP_OK;");
  fprintf(fserver,"\n}");
  fflush(fserver);
}

int
is_qname(Tnode *p)
{ return p->sym && is_string(p) && (is_eq(p->sym->name, "xsd__QName") || is_eq(p->sym->name, "QName"));
}

int
is_XML(Tnode *p)
{ return p->sym && (is_string(p) || is_wstring(p)) && is_eq(p->sym->name, "XML") || (p->type == Tpointer || p->type == Treference) && is_XML(p->ref);
}

int
is_response(Tnode *p)
{ return (p->type == Tpointer || p->type == Treference) && p->ref && ((((Tnode*)p->ref)->type == Tstruct || ((Tnode*)p->ref)->type == Tclass) && !is_primclass(p->ref) && !is_dynamic_array(p->ref) && !is_stdstring(p->ref) && !is_stdwstring(p->ref));
}

Entry*
get_response(Tnode *p)
{ if (p->type == Tfun)
    return p->response;
  return 0;
}

int
is_unmatched(Symbol *sym)
{ return sym->name[0] == '_'
      && sym->name[1] != '_'
      && strncmp(sym->name, "_DOT", 4) 
      && strncmp(sym->name, "_USCORE", 7)
      && (strncmp(sym->name, "_x", 2) || !isxdigit(sym->name[2]) || !isxdigit(sym->name[3]) || !isxdigit(sym->name[4]) || !isxdigit(sym->name[5]));
}

int
is_invisible(const char *name)
{ return name[0] == '-' || name[0] == '_' && name[1] == '_' && strncmp(name, "__ptr", 5);
}

int
is_element(Tnode *typ)
{ if (typ->sym)
    return is_unmatched(typ->sym);
  if (typ->type == Tstruct || typ->type == Tclass)
    return is_unmatched(typ->id);
  return 0;
}

int
is_untyped(Tnode *typ)
{ Tnode *p;
  if (typ->sym)
    return is_unmatched(typ->sym);
  if (typ->type == Tpointer || typ->type == Treference || typ->type == Tarray)
    return is_untyped(typ->ref);
  if (typ->type == Tenum)
    return !has_ns(typ);
  if (is_primitive_or_string(typ) || is_stdstr(typ))
    return 1;
  if (typ->type == Tstruct || typ->type == Tclass)
    if (is_dynamic_array(typ) && !has_ns(typ) && !is_binary(typ))
    { p = ((Table*)typ->ref)->list->info.typ->ref;
      if (p->type == Tpointer)
        if (is_primitive_or_string(p->ref))
	  return 0;
        else
          return is_untyped(p);
      else if (is_primitive_or_string(p))
        return 0;
      else
        return is_untyped(p);
    }
    else
      return is_unmatched(typ->id);
  return 0;
}

int
is_primclass(Tnode *typ)
{ Table *t;
  if (typ->type == Tstruct || typ->type == Tclass)
  { if (!is_dynamic_array(typ))
    { t = (Table*)typ->ref;
      if (t)
      { if (!t->list || t->list->info.typ->type == Tfun)
        { do
          { if (t->list && is_item(t->list))
              return 1;
	    t = t->prev;
          } while (t);
	  return 0;
        }
        return is_item(t->list);
      }
    }
  }
  else if (typ->type == Tpointer || typ->type == Treference)
    return is_primclass(typ->ref);
  return 0;
}

int
is_mask(Tnode *typ)
{ return (typ->type == Tenum && typ->width == 8);
}

int
is_void(Tnode *typ)
{ if (!typ)
    return 1;
  if (typ->type == Tvoid)
    return 1;
  if (typ->type == Tpointer)
    return is_void(typ->ref); 
  if (typ->type == Treference)
    return is_void(typ->ref); 
  if (typ->type == Tarray)
    return is_void(typ->ref); 
  if (typ->type == Ttemplate)
    return is_void(typ->ref); 
  return 0;
}

int
is_transient(Tnode *typ)
{ if (!typ)
    return 1;
  if (is_external(typ) || is_volatile(typ))
    return 0;
  if (typ->transient || typ->type == Tvoid || typ->type == Tunion || typ->type == Tnone)
    return 1;
  switch (typ->type)
  { case Tpointer:
    case Treference:
    case Tarray:
    case Ttemplate:
      return is_transient(typ->ref);
    case Tstruct:
      return typ->id == lookup("soap");
  }
  return 0;
}

int
is_imported(Tnode* typ)
{ if (xflag || lflag)
    return typ->imports > 0;
  return 0;
}

int
is_external(Tnode* typ)
{ return typ->transient == -1;
}

int
is_volatile(Tnode* typ)
{ return typ->transient == -2;
}

int
is_template(Tnode *p)
{ if (p->type == Tpointer)
    return is_template(p->ref);
  return p->type == Ttemplate;
}

int
is_repetition(Entry *p)
{ if (p)
    return p->next && p->next->info.typ->type == Tpointer && (p->info.typ->type == Tint || p->info.typ->type == Tuint) && !strncmp(p->sym->name, "__size", 6);
  return 0;
} 

int
is_item(Entry *p)
{ if (p)
    return !strcmp(p->sym->name, "__item");
  return 0;
}

int
is_anytype(Entry *p)
{ if (p)
    if (p->next && p->next->info.typ->type == Tpointer && ((Tnode*)p->next->info.typ->ref)->type == Tvoid && p->info.typ->type == Tint && !strncmp(p->sym->name, "__type", 6))
    { is_anytype_flag = 1;
      return 1;
    }
  return 0;
} 

int
has_detail_string()
{ Entry *p = entry(classtable, lookup("SOAP_ENV__Fault"));
  if (p && p->info.typ->ref && (p->info.typ->type == Tstruct || p->info.typ->type == Tclass))
  { Entry *e = entry(p->info.typ->ref, lookup("detail"));
    if (e && e->info.typ->ref && e->info.typ->type == Tpointer && ((Tnode*)e->info.typ->ref)->type == Tstruct)
    { Entry *e2 = entry(((Tnode*)e->info.typ->ref)->ref, lookup("__any"));
      return e2 && is_string(e2->info.typ);
    }
  }
  return 0;
}

int
has_Detail_string()
{ Entry *p = entry(classtable, lookup("SOAP_ENV__Fault"));
  if (p && p->info.typ->ref && (p->info.typ->type == Tstruct || p->info.typ->type == Tclass))
  { Entry *e = entry(p->info.typ->ref, lookup("SOAP_ENV__Detail"));
    if (e && e->info.typ->ref && e->info.typ->type == Tpointer && ((Tnode*)e->info.typ->ref)->type == Tstruct)
    { Entry *e2 = entry(((Tnode*)e->info.typ->ref)->ref, lookup("__any"));
      return e2 && is_string(e2->info.typ);
    }
  }
  return 0;
}

int
has_class(Tnode *typ)
{ Entry *p;
  if (typ->type == Tstruct && typ->ref)
  { for (p = ((Table*)typ->ref)->list; p; p = p->next)
    { if (p->info.typ->type == Tclass || p->info.typ->type == Ttemplate)
        return 1;
      else if (p->info.typ->type == Tstruct && has_class(p->info.typ))
        return 1;
    }
  }
  return 0;
}

int
has_external(Tnode *typ)
{ Entry *p;
  if ((typ->type == Tstruct || typ->type == Tclass) && typ->ref)
  { for (p = ((Table*)typ->ref)->list; p; p = p->next)
    { if (p->info.typ->type == Tstruct || p->info.typ->type == Tclass)
      { if (is_external(p->info.typ) || has_external(p->info.typ))
          return 1;
      }
    }
  }
  return 0;
}

int
has_volatile(Tnode *typ)
{ Entry *p;
  if ((typ->type == Tstruct || typ->type == Tclass) && typ->ref)
  { for (p = ((Table*)typ->ref)->list; p; p = p->next)
    { if (p->info.typ->type == Tstruct || p->info.typ->type == Tclass)
      { if (is_volatile(p->info.typ) || has_volatile(p->info.typ))
          return 1;
      }
    }
  }
  return 0;
}

int
has_ns(Tnode *typ)
{ char *s;
  if (typ->type == Tstruct || typ->type == Tclass || typ->type == Tenum)
    return has_ns_eq(NULL, typ->id->name);
  return 0;
}

int
has_ns_t(Tnode *typ)
{ char *s;
  if (typ->sym)
  { s = strstr(typ->sym->name + 1, "__");
    if (!s)
      s = strstr(typ->sym->name, "::");
    return s && s[2] && s[2] != '_';
  }
  return has_ns(typ);
}

int
is_eq_nons(const char *s, const char *t)
{ int n, m;
  char *r;
  while (*s == '_')
    s++;
  while (*t == '_')
    t++;
  if (!*s || !*t)
    return 0;
  r = strstr(t, "__");
  if (r)
    t = r + 2;
  for (n = strlen(s) - 1; n && s[n] == '_'; n--)
    ;
  for (m = strlen(t) - 1; m && t[m] == '_'; m--)
    ;
  if (n != m)
    return 0;
  return !strncmp(s, t, n + 1);
}

int
is_eq(const char *s, const char *t)
{ int n, m;
  while (*s == '_')
    s++;
  while (*t == '_')
    t++;
  if (!*s || !*t)
    return 0;
  for (n = strlen(s) - 1; n && s[n] == '_'; n--)
    ;
  for (m = strlen(t) - 1; m && t[m] == '_'; m--)
    ;
  if (n != m)
    return 0;
  return !strncmp(s, t, n + 1);
}

int
has_ns_eq(char *ns, char *s)
{ int n;
  while (*s == '_')
    s++;
  if (!ns)
  { char *t = strstr(s + 1, "__");
    if (!t)
      t = strstr(s, "::");
    return t && t[2] && t[2] != '_';
  }
  if ((n = strlen(ns)) < strlen(s))
    return s[n] == '_' && s[n+1] == '_' && !strncmp(ns, s, n);
  return 0;
}

char *
ns_of(char *name)
{ Service *sp;
  for (sp = services; sp; sp = sp->next)
    if (has_ns_eq(sp->ns, name))
      break;
  if (sp)
    return sp->URI;
  return NULL;
}

char *
prefix_of(char *s)
{ int n;
  char *t;
  while (*s == '_')
    s++;
  t = strstr(s + 1, "__");
  if (!t)
    t = strstr(s, "::");
  if (t && t[2] && t[2] != '_')
  { char *r  = (char*)emalloc(t - s + 1);
    strncpy(r, s, t - s);
    r[t - s] = '\0';
    return r;
  }
  return s;
}

char *
ns_overridden(Table *t, Entry *p)
{ Entry *q;
  Symbol *s = t->sym;
  char *n;
  if (s)
    while (t = t->prev)
      for (q = t->list; q; q = q->next)
        if (!strcmp(q->sym->name, p->sym->name))
        { n = (char*)emalloc(strlen(s->name)+strlen(p->sym->name)+2);
	  strcpy(n, s->name);
	  strcat(n, ".");
	  strcat(n, p->sym->name);
	  return ns_convert(n);
        }
  return ns_convert(p->sym->name);
}

char *
ns_add_overridden(Table *t, Entry *p, char *ns)
{ Entry *q;
  Symbol *s = t->sym;
  char *n;
  if (s)
    while (t = t->prev)
      for (q = t->list; q; q = q->next)
        if (!strcmp(q->sym->name, p->sym->name))
        { n = (char*)emalloc(strlen(s->name)+strlen(p->sym->name)+2);
	  strcpy(n, s->name);
	  strcat(n, ".");
	  strcat(n, p->sym->name);
	  return ns_add(n, ns);
        }
  return ns_add(p->sym->name, ns);
}


/*c_ident gives the names of the function for a type*/

char *
c_ident(Tnode *typ)
{ char *p, *q;
  /*if( dumptypename(typetable,typ)!=0)
    {
      p=(char*) malloc(strlen(dumptypename(typetable,typ))*sizeof(char));
      strcpy(p,dumptypename(typetable,typ));
      DBGLOG(fprintf(stderr,"\n Return before switch in c_ident() %s\n", p));
      return p;
    }*/
  if (typ->sym && strcmp(typ->sym->name, "/*?*/"))
    return typ->sym->name;
  switch(typ->type)
  {
  case Tnone:
    return "";
  case Tvoid:
    return "void";
  case Tchar:
    return "byte";
  case Twchar:
    return "wchar";
  case Tshort:
    return "short";
  case Tint:
    return "int";
  case Tlong:
    return "long";
  case Tllong:
    return "LONG64";
  case Tfloat:
    return "float";
  case Tdouble:
    return "double";
  case Tuchar:
    return "unsignedByte";
  case Tushort:
    return "unsignedShort";
  case Tuint:
    return "unsignedInt";
  case Tulong:
    return "unsignedLong";
  case Tullong:
    return "unsignedLONG64";
  case Ttime:
    return "time";
  case Tstruct:
  case Tclass:
  case Tunion:
  case Tenum:
    if (typ->ref == booltable)
      return "bool";
    return res_remove(typ->id->name);
  case Treference:
    return c_ident(typ->ref);
  case Tpointer:
    if(((Tnode*)typ->ref)->type == Tchar) 
	return "string";
    if(((Tnode*)typ->ref)->type == Twchar) 
	return "wstring";
    p=(char*) emalloc((10+strlen(q = c_ident(typ->ref)))*sizeof(char));
    strcpy(p,"PointerTo");
    strcat(p,q);
    return p;
  case Tarray:
    p=(char*) emalloc((16+strlen(c_ident(typ->ref)))*sizeof(char));
    if (((Tnode*)typ->ref)->width)
      sprintf(p, "Array%dOf%s",typ->width / ((Tnode*) typ->ref)->width,c_ident(typ->ref));
    else
      sprintf(p, "ArrayOf%s", c_ident(typ->ref));
    return p;
  case Ttemplate:
    if (typ->ref)
    { p=(char*) emalloc((11+strlen(res_remove(typ->id->name))+strlen(q = c_ident(typ->ref)))*sizeof(char));
      strcpy(p, res_remove(typ->id->name));
      strcat(p, "TemplateOf");
      strcat(p, q);
      return p;
    }
  case Tfun:
    return "Function";
  }
  return "anyType";
}

void
utf8(char **t, long c)
{ if (c < 0x0080)
    *(*t)++ = (char)c;
  else
  { if (c < 0x0800)
      *(*t)++ = (char)(0xC0 | ((c >> 6) & 0x1F));
    else
    { if (c < 0x010000)
        *(*t)++ = (char)(0xE0 | ((c >> 12) & 0x0F));
      else
      { if (c < 0x200000)
          *(*t)++ = (char)(0xF0 | ((c >> 18) & 0x07));
        else
        { if (c < 0x04000000)
            *(*t)++ = (char)(0xF8 | ((c >> 24) & 0x03));
          else
          { *(*t)++ = (char)(0xFC | ((c >> 30) & 0x01));
            *(*t)++ = (char)(0x80 | ((c >> 24) & 0x3F));
          }
          *(*t)++ = (char)(0x80 | ((c >> 18) & 0x3F));
        }     
        *(*t)++ = (char)(0x80 | ((c >> 12) & 0x3F));
      }
      *(*t)++ = (char)(0x80 | ((c >> 6) & 0x3F));
    }
    *(*t)++ = (char)(0x80 | (c & 0x3F));
  }
  *(*t) = '\0';
}

char *
ns_convert(char *tag)
{ char *t, *s;
  int i, n;
  if (*tag == '_')
  { if (!strncmp(tag, "__ptr", 5))
      if (tag[5])
        tag += 5;
      else
        tag = "item";
    else if (strncmp(tag, "_DOT", 4) 
          && strncmp(tag, "_USCORE", 7)
          && (strncmp(tag, "_x", 2) || !isxdigit(tag[2]) || !isxdigit(tag[3]) || !isxdigit(tag[4]) || !isxdigit(tag[5])))
      tag++; /* skip leading _ */
  }
  for (n = strlen(tag); n > 0; n--)
    if (tag[n-1] != '_')
      break;
  s = t = (char*)emalloc(n+1);
  for (i = 0; i < n; i++)
  { if (tag[i] == '_')
      if (tag[i+1] == '_')
        break;
      else if (!strncmp(tag+i, "_DOT", 4))
      { *s++ = '.';
        i += 3;
      }
      else if (!strncmp(tag+i, "_USCORE", 7))
      { *s++ = '_';
        i += 6;
      }
      else if (!strncmp(tag+i, "_x", 2) && isxdigit(tag[i+2]) && isxdigit(tag[i+3]) && isxdigit(tag[i+4]) && isxdigit(tag[i+5]))
      { char d[5];
	strncpy(d, tag+i+2, 4);
	d[4] = '\0';
        utf8(&s, strtoul(d, NULL, 16));
        i += 5;
      }
      else
        *s++ = '-';
    else if (tag[i] == ':' && tag[i+1] == ':')
      break;
    else
      *s++ = tag[i];
  }
  if (i < n)
  { *s++ = ':';
    for (i += 2; i < n; i++)
      if (tag[i] == '_')
        if (!strncmp(tag+i, "_DOT", 4))
        { *s++ = '.';
          i += 3;
        }
        else if (!strncmp(tag+i, "_USCORE", 7))
        { *s++ = '_';
          i += 6;
        }
        else if (!strncmp(tag+i, "_x", 2) && isxdigit(tag[i+2]) && isxdigit(tag[i+3]) && isxdigit(tag[i+4]) && isxdigit(tag[i+5]))
        { char d[5];
	  strncpy(d, tag+i+2, 4);
	  d[4] = '\0';
          utf8(&s, strtoul(d, NULL, 16));
          i += 5;
        }
	else
	  *s++ = '-';
      else
        *s++ = tag[i];
  }
  else
  { /* consider adding default prefix when element/attributeFormDefault is qualified */
  }
  *s = '\0';
  return t;
}

char *
res_remove(char *tag)
{ char *s, *t;
  if (!(s = strchr(tag, ':')))
    return tag;
  s = emalloc(strlen(tag) + 1);
  strcpy(s, tag);
  while ((t = strchr(s, ':')))
    *t = '_';
  return s;
}

char *
ns_qualifiedElement(Tnode *typ)
{ Service *sp;
  char *s = NULL;
  if (typ->sym)
    s = prefix_of(typ->sym->name);
  if (!s && typ->id)
    s = prefix_of(typ->id->name);
  if (!s)
    return NULL;
  for (sp = services; sp; sp = sp->next)
  { if (sp->elementForm && !strcmp(sp->ns, s))
    { if (!strcmp(sp->elementForm, "qualified"))
        return s;
      return NULL;
    }
  }
  for (sp = services; sp; sp = sp->next)
    if (!strcmp(sp->ns, s))
      if (sp->style && !strcmp(sp->style, "document"))
        return s;
  return NULL;
}

char *
ns_qualifiedAttribute(Tnode *typ)
{ Service *sp;
  char *s = NULL;
  if (typ->sym)
    s = prefix_of(typ->sym->name);
  if (!s && typ->id)
    s = prefix_of(typ->id->name);
  if (!s)
    return NULL;
  for (sp = services; sp; sp = sp->next)
  { if (sp->attributeForm && !strcmp(sp->ns, s))
    { if (!strcmp(sp->attributeForm, "qualified"))
        return s;
      return NULL;
    }
  }
  for (sp = services; sp; sp = sp->next)
    if (!strcmp(sp->ns, s))
      if (sp->style && !strcmp(sp->style, "document"))
        return s;
  return NULL;
}

char *
ns_add(char *tag, char *ns)
{ char *t, *s = ns_convert(tag);
  if (!ns || (t = strchr(s, ':')))
    return s;
  t = emalloc(strlen(ns) + strlen(s) + 2);
  strcpy(t, ns);
  strcat(t, ":");
  strcat(t, s);
  return t;
}

char *
ns_remove(char *tag)
{ char *t, *s = tag;
  if (*s)
    for (t = s + 1; *t; t++)
      if (t[0] == '_' && t[1] == '_')
      { s = t + 2;
        t++;
      }
  return ns_convert(s);
}

char *
ns_remove2(char *tag)
{ char *t, *s = tag;
  int n = 2;
  if (*s)
  { for (t = s + 1; *t && n; t++)
      if (t[0] == '_' && t[1] == '_')
      { s = t + 2;
        t++;
	n--;
      }
    if (n)
      s = tag;
  }
  return ns_convert(s);
}

char *
xsi_type_cond(Tnode *typ, int flag)
{ if (flag)
    return xsi_type(typ);
  return "";
}

char *
xsi_type_cond_u(Tnode *typ, int flag)
{ if (flag && tflag)
    return xsi_type(typ);
  return "";
}

char *
xsi_type_u(Tnode *typ)
{ if (tflag)
    return xsi_type(typ);
  return "";
}

char *
xsi_type(Tnode *typ)
{ if (!typ)
    return "NULL";
  if (is_dynamic_array(typ) && !has_ns(typ))
    return xsi_type_Darray(typ);
  if (typ->type == Tarray)
    return xsi_type_Tarray(typ);
  if (is_untyped(typ))
    return "";
  if (typ->sym)
    if (!strncmp(typ->sym->name, "SOAP_ENV__", 10))
      return "";
    else if (is_XML(typ))
      return "xsd:anyType";
    else if (typ->type != Ttemplate)
      return ns_convert(typ->sym->name);
  if (is_string(typ) || is_wstring(typ) || is_stdstring(typ) || is_stdwstring(typ))
    return "xsd:string";
  switch(typ->type){
  case Tchar:
    return "xsd:byte";
  case Twchar:
    return "wchar";
  case Tshort:
    return "xsd:short";
  case Tint:
    return "xsd:int";
  case Tlong:
  case Tllong:
    return "xsd:long";
  case Tfloat:
    return "xsd:float";
  case Tdouble:
    return "xsd:double";
  case Tuchar:
    return "xsd:unsignedByte";
  case Tushort:
    return "xsd:unsignedShort";
  case Tuint:
    return "xsd:unsignedInt";
  case Tulong:
  case Tullong:
    return "xsd:unsignedLong";
  case Ttime:
    return "xsd:dateTime";
  case Tpointer:
  case Treference:
    return xsi_type(typ->ref);
  case Tenum:
    if (typ->ref == booltable)
      return "xsd:boolean";
  case Tstruct:
  case Tclass:
    if (!has_ns(typ) || !strncmp(typ->id->name, "SOAP_ENV__", 10))
      return "";
    return ns_convert(typ->id->name);
  case Ttemplate:
    if (typ->ref)
      return xsi_type(typ->ref);
  }
  return "";
}

char *
xml_tag(Tnode *typ)
{ if (!typ)
    return "NULL";
  if (typ->type == Tpointer || typ->type == Treference)
    return xml_tag(typ->ref);
  if (typ->sym)
    return ns_convert(typ->sym->name);
  return the_type(typ);
}

char *
wsdl_type(Tnode *typ, char *ns)
{ if (!typ)
    return "NULL";
  /*
  if (is_qname(typ) && ns)
      return "xsd:QName";
  */
  if (typ->sym)
    if (is_XML(typ))
      return "xsd:anyType";
    else if (ns)
      return ns_convert(typ->sym->name);
    else
      return ns_remove(typ->sym->name);
  return base_type(typ, ns);
}

char *
base_type(Tnode *typ, char *ns)
{ int d;
  char *s, *t;
  if (is_string(typ) || is_wstring(typ) || is_stdstring(typ) || is_stdwstring(typ))
    if (ns)
      return "xsd:string";
    else
      return "string";
  if (is_dynamic_array(typ) && !is_binary(typ) && !has_ns(typ) && !is_untyped(typ))
  { s = ns_remove(wsdl_type(((Table*)typ->ref)->list->info.typ, NULL));
    if (ns && *ns)
    { t = (char*)emalloc(strlen(s)+strlen(ns_convert(ns))+13);
      strcpy(t, ns_convert(ns));
      strcat(t, ":");
      strcat(t, "ArrayOf");
    }
    else
    { t = (char*)emalloc(strlen(s)+12);
      strcpy(t, "ArrayOf");
    }
    strcat(t, s);
    d = get_Darraydims(typ);
    if (d)
      sprintf(t+strlen(t), "%dD", d);
    return t;
  }
  switch (typ->type){
  case Tchar :
    if (ns)
      return "xsd:byte";
    else
      return "byte";
  case Twchar :
    if (ns)
      return "xsd:wchar";
    else
      return "wchar";
  case Tshort :
    if (ns)
      return "xsd:short";
    else
      return "short";
  case Tint  :
    if (ns)
      return "xsd:int";
    else
      return "int";
  case Tlong  :
  case Tllong  :
    if (ns)
      return "xsd:long";
    else
      return "long";
  case Tfloat:
    if (ns)
      return "xsd:float";
    else
      return "float";
  case Tdouble:
    if (ns)
      return "xsd:double";
    else
      return "double";
  case Tuchar:
    if (ns)
      return "xsd:unsignedByte";
    else
      return "unsignedByte";
  case Tushort:
    if (ns)
      return "xsd:unsignedShort";
    else
      return "unsignedShort";
  case Tuint:
    if (ns)
      return "xsd:unsignedInt";
    else
      return "unsignedInt";
  case Tulong:
  case Tullong:
    if (ns)
      return "xsd:unsignedLong";
    else
      return "unsignedLong";
  case Ttime:
    if (ns)
      return "xsd:dateTime";
    else
      return "dateTime";
  case Tpointer:
  case Treference:
    return wsdl_type(typ->ref, ns);
  case Tarray:
    if (ns && *ns)
    { s = (char*)emalloc((strlen(ns_convert(ns))+strlen(c_ident(typ))+2)*sizeof(char));
      strcpy(s, ns_convert(ns));
      strcat(s, ":");
      strcat(s, c_ident(typ));
      return s;
    }
    else
      return c_ident(typ);
  case Tenum:
    if (typ->ref == booltable)
      if (ns)
        return "xsd:boolean";
      else
        return "boolean";
  case Tstruct:
  case Tclass:
    if (!has_ns(typ) && ns && *ns)
    { s = (char*)emalloc((strlen(ns_convert(ns))+strlen(typ->id->name)+2)*sizeof(char));
      strcpy(s, ns_convert(ns));
      strcat(s, ":");
      strcat(s, ns_convert(typ->id->name));
      return s;
    }
    else if (ns)
      return ns_convert(typ->id->name);
    else
      return ns_remove(typ->id->name);
  case Ttemplate:
    if (typ->ref)
      return wsdl_type(typ->ref, ns);
  }
  return "";
}

char *
the_type(Tnode *typ)
{ if (!typ)
    return "NULL";
  if (typ->type == Tarray || is_dynamic_array(typ) && !has_ns(typ) && !is_untyped(typ))
    return "SOAP-ENC:Array";
  if (is_string(typ) || is_wstring(typ) || is_stdstring(typ) || is_stdwstring(typ))
    return "string";
  switch (typ->type)
  {
  case Tchar:
    return "byte";
  case Twchar:
    return "wchar";
  case Tshort:
    return "short";
  case Tint :
    return "int";
  case Tlong :
  case Tllong :
    return "long";
  case Tfloat:
    return "float";
  case Tdouble:
    return "double";
  case Tuchar:
    return "unsignedByte";
  case Tushort:
    return "unsignedShort";
  case Tuint:
    return "unsignedInt";
  case Tulong:
  case Tullong:
    return "unsignedLong";
  case Ttime:
    return "dateTime";
  case Tpointer:
  case Treference:
    return the_type(typ->ref);
  case Tarray:
    return "SOAP-ENC:Array";
  case Tenum:
    if (typ->ref == booltable)
      return "boolean";
  case Tstruct:
  case Tclass:
    return ns_convert(typ->id->name);
  }
  return "";
}

/* c_type returns the type to be used in parameter declaration*/
char *
c_type(Tnode *typ)
{
  char *p, *q, tempBuf[10];
  Tnode *temp;
  if (typ==0)
    return "NULL";
  switch(typ->type){
  case Tnone:
    return "";
  case Tvoid:
    return "void";
  case Tchar:
    return "char";
  case Twchar:
    return "wchar_t";
  case Tshort:
    return "short";
  case Tint  :
    return "int";
  case Tlong  :
    return "long";
  case Tllong  :
    return "LONG64";
  case Tfloat:
    return "float";
  case Tdouble:
    return "double";
  case Tuchar:
    return "unsigned char";
  case Tushort:
    return "unsigned short";
  case Tuint:
    return "unsigned int";
  case Tulong:
    return "unsigned long";
  case Tullong:
    return "ULONG64";
  case Ttime:
    return "time_t";
  case Tstruct:p=(char*) emalloc((8+strlen(typ->id->name)) *sizeof(char));
    strcpy(p,"struct ");
    strcat(p,typ->id->name);
    break;
  case Tclass:
   p = typ->id->name;
   break;
  case Tunion: p=(char*) emalloc((7+strlen(typ->id->name)) *sizeof(char));
    strcpy(p,"union ");
    strcat(p,typ->id->name);
    break;
  case Tenum:
    if (typ->ref == booltable)
      return "bool";
    p=(char*) emalloc((6+strlen(typ->id->name)) *sizeof(char));
    strcpy(p,"enum ");
    strcat(p,typ->id->name);
    break;
  case Tpointer:
    p = c_type_id(typ->ref, "*");
    break;
  case Treference:
    p = c_type_id(typ->ref, "&");
    break;
  case Tarray:
    p=(char*) emalloc((12+strlen(q = c_type(typ->ref))) *sizeof(char));
    temp = typ;
    while(((Tnode*) (typ->ref))->type==Tarray){
      typ = typ->ref;
    }
    if (((Tnode*)typ->ref)->type == Tpointer)
      sprintf(p,"%s",c_type(typ->ref));
    else
      strcpy(p, q);
    typ = temp;
    if (((Tnode*) typ->ref)->width)
    { sprintf(tempBuf,"[%d]",(typ->width / ((Tnode*) typ->ref)->width));
      strcat(p,tempBuf);
    }
    break;
  case Ttemplate:
    if (typ->ref)
    { p=(char*)emalloc((strlen(q = c_type(typ->ref))+strlen(typ->id->name)+4) *sizeof(char));
      strcpy(p, typ->id->name);
      strcat(p, "<");
      strcat(p, q);
      strcat(p, " >");
      break;
    }
  default:
    return "UnknownType";   
  }
  return p;
}

char *
c_storage(Storage sto)
{ char *p;
  static char buf[256];
  if (sto & Sconst)
  { p = c_storage(sto & ~Sconst);
    strcat(p, "const ");
    return p;
  }
  if (sto & Sauto)
  { p = c_storage(sto & ~Sauto);
    strcat(p, "auto ");
    return p;
  }
  if (sto & Sregister)
  { p = c_storage(sto & ~Sregister);
    strcat(p, "register ");
    return p;
  }
  if (sto & Sstatic)
  { p = c_storage(sto & ~Sstatic);
    strcat(p, "static ");
    return p;
  }
  if (sto & Sexplicit)
  { p = c_storage(sto & ~Sexplicit);
    strcat(p, "explicit ");
    return p;
  }
  if (sto & Sextern)
  { p = c_storage(sto & ~Sextern);
    return p;
  }
  if (sto & Stypedef)
  { p = c_storage(sto & ~Stypedef);
    strcat(p, "typedef ");
    return p;
  }
  if (sto & Svirtual)
  { p = c_storage(sto & ~Svirtual);
    strcat(p, "virtual ");
    return p;
  }
  if (sto & Sfriend)
  { p = c_storage(sto & ~Sfriend);
    strcat(p, "friend ");
    return p;
  }
  if (sto & Sinline)
  { p = c_storage(sto & ~Sinline);
    strcat(p, "inline ");
    return p;
  }
  buf[0]= '\0';
  return buf;
}

char *
c_init(Entry *e)
{ static char buf[1024];
  buf[0] = '\0';
  if (e->info.hasval)
    switch (e->info.typ->type)
    { case Tchar:
      case Twchar:
      case Tuchar:
      case Tshort:
      case Tushort:
      case Tint:
      case Tuint:
      case Tlong:
      case Tllong:
      case Tulong:
      case Tullong:
      case Ttime:
        sprintf(buf, " = "SOAP_LONG_FORMAT, e->info.val.i);
	break;
      case Tfloat:
      case Tdouble:
        sprintf(buf, " = %f", e->info.val.r);
	break;
      case Tenum:
        sprintf(buf, " = (%s)"SOAP_LONG_FORMAT, c_type(e->info.typ), e->info.val.i);
	break;
      default:
	if (e->info.val.s && strlen(e->info.val.s) < sizeof(buf)-6)
          sprintf(buf, " = \"%s\"", e->info.val.s);
	else if (e->info.typ->type == Tpointer)
          sprintf(buf, " = NULL");
	break;
    }
  return buf;
}

/* c_type_id returns the arraytype to be used in parameter declaration
   Allows you to specify the identifier that acts acts as teh name of teh
   type of array */
char *
c_type_id(Tnode *typ, char *ident)
{
  char *p,*q,tempBuf[10];
  Tnode *temp;
  Entry *e;
  if (!typ)
    return "NULL";
  
  switch(typ->type)
  {
  case Tnone:
    p = ident;
    break;
  case Tvoid:
    p = (char*)emalloc(6+strlen(ident));
    strcpy(p, "void ");
    strcat(p, ident);
    break;
  case Tchar:
    p = (char*)emalloc(6+strlen(ident));
    strcpy(p, "char ");
    strcat(p, ident);
    break;
  case Twchar:
    p = (char*)emalloc(9+strlen(ident));
    strcpy(p, "wchar_t ");
    strcat(p, ident);
    break;
  case Tshort:
    p = (char*)emalloc(7+strlen(ident));
    strcpy(p, "short ");
    strcat(p, ident);
    break;
  case Tint  :
    p = (char*)emalloc(5+strlen(ident));
    strcpy(p, "int ");
    strcat(p, ident);
    break;
  case Tlong  :
    p = (char*)emalloc(6+strlen(ident));
    strcpy(p, "long ");
    strcat(p, ident);
    break;
  case Tllong  :
    p = (char*)emalloc(8+strlen(ident));
    strcpy(p, "LONG64 ");
    strcat(p, ident);
    break;
  case Tfloat:
    p = (char*)emalloc(7+strlen(ident));
    strcpy(p, "float ");
    strcat(p, ident);
    break;
  case Tdouble:
    p = (char*)emalloc(8+strlen(ident));
    strcpy(p, "double ");
    strcat(p, ident);
    break;
  case Tuchar:
    p = (char*)emalloc(15+strlen(ident));
    strcpy(p, "unsigned char ");
    strcat(p, ident);
    break;
  case Tushort:
    p = (char*)emalloc(16+strlen(ident));
    strcpy(p, "unsigned short ");
    strcat(p, ident);
    break;
  case Tuint:
    p = (char*)emalloc(14+strlen(ident));
    strcpy(p, "unsigned int ");
    strcat(p, ident);
    break;
  case Tulong:
    p = (char*)emalloc(15+strlen(ident));
    strcpy(p, "unsigned long ");
    strcat(p, ident);
    break;
  case Tullong:
    p = (char*)emalloc(9+strlen(ident));
    strcpy(p, "ULONG64 ");
    strcat(p, ident);
    break;
  case Ttime:
    p = (char*)emalloc(8+strlen(ident));
    strcpy(p, "time_t ");
    strcat(p, ident);
    break;
  case Tstruct:
    p=(char*) emalloc((9+strlen(typ->id->name)+strlen(ident)) *sizeof(char));
    strcpy(p,"struct ");
    strcat(p,typ->id->name);
    strcat(p, " ");
    strcat(p,ident);
    break;
  case Tclass:
    if (!typ->generated)
    { p=(char*) emalloc((8+strlen(typ->id->name)+strlen(ident)) *sizeof(char));
      strcpy(p, "class ");
      strcat(p, typ->id->name);
      typ->generated = True;
    }
    else
    { p=(char*) emalloc((2+strlen(typ->id->name)+strlen(ident)) *sizeof(char));
      strcpy(p, typ->id->name);
    }
    strcat(p, " ");
    strcat(p, ident);
    break;
  case Tunion:
    p=(char*) emalloc((8+strlen(typ->id->name)+strlen(ident)) *sizeof(char));
    strcpy(p,"union ");
    strcat(p,typ->id->name);
    strcat(p, " ");
    strcat(p, ident);
    break;
  case Tenum:
    if (typ->ref == booltable)
    { p = (char*)emalloc((strlen(ident)+6)*sizeof(char));
      strcpy(p, "bool ");
      strcat(p, ident);
      return p;
    }
    p=(char*) emalloc((7+strlen(typ->id->name)+strlen(ident)) *sizeof(char));
    strcpy(p, "enum ");
    strcat(p, typ->id->name);
    strcat(p, " ");
    strcat(p, ident);
    break;
  case Tpointer:
    p = (char*)emalloc(strlen(ident)+2);
    strcpy(p+1, ident);
    p[0] = '*';
    p = c_type_id(typ->ref, p);
    break;
  case Treference:
    p = (char*)emalloc(strlen(ident)+2);
    strcpy(p+1, ident);
    p[0] = '&';
    p = c_type_id(typ->ref, p);
    break;
  case Tarray:
    temp = typ;
    while(((Tnode*) (typ->ref))->type==Tarray){
      typ = typ->ref;
    }
    p=(char*) emalloc((12+strlen(q = c_type_id(typ->ref, ident))) *sizeof(char));
    strcpy(p, q);
    typ = temp;
    while(typ->type==Tarray){
      if (((Tnode*) typ->ref)->width)
      { sprintf(tempBuf,"[%d]",(typ->width / ((Tnode*) typ->ref)->width));
        strcat(p,tempBuf);
      }
      typ = typ->ref;
    }
    /*if(((Tnode*) (typ->ref))->type==Tarray){
      sprintf(p,"%s [%d]",c_type(typ->ref),(typ->width / ((Tnode*) typ->ref)->width));
    }else
    sprintf(p,"%s a[%d]",c_type(typ->ref),(typ->width /((Tnode*) typ->ref)->width));*/
    break;
  case Tfun:
    if (strncmp(ident, "operator ", 9))
      q = c_type_id(((FNinfo*)typ->ref)->ret, ident);
    else
      q = ident;
    p = (char*)emalloc(1024);
    strcpy(p, q);
    strcat(p, "(");
    for (e = ((FNinfo*)typ->ref)->args->list; e; e = e->next)
    { strcat(p, c_storage(e->info.sto));
      strcat(p, c_type_id(e->info.typ, e->sym->name));
      strcat(p, c_init(e));
      if (e->next)
        strcat(p, ", ");
    }
    strcat(p, ")");
    break;
  case Ttemplate:
    if (typ->ref)
    { p=(char*)emalloc((strlen(q = c_type(typ->ref))+strlen(typ->id->name)+strlen(ident)+4) *sizeof(char));
      strcpy(p, typ->id->name);
      strcat(p, "<");
      strcat(p, q);
      strcat(p, " >");
      strcat(p, ident);
      break;
    }
  default:
    return "UnknownType";   
  }
  return p;
}

char *
xsi_type_Tarray(Tnode *typ)
{ Tnode *t;
  int cardinality;
  char *p, *s;
  t = typ->ref;
  cardinality = 1;
  while (t->type == Tarray || (is_dynamic_array(t) && !has_ns(t) && !is_untyped(typ)))
  { if( t->type == Tarray)
      t = t->ref;
    else
      t = ((Table*)t->ref)->list->info.typ->ref;
    cardinality++;
  }
  s = xsi_type(t);
  if (!*s)
    s = wsdl_type(t, "");
  p = (char*)emalloc(strlen(s)+cardinality+3);
  strcpy(p, s);
  if (cardinality > 1)
  { strcat(p, "[");
    for (; cardinality > 2; cardinality--)
      strcat(p, ",");
    strcat(p, "]");
  }
  /*
  for (; cardinality; cardinality--)
  { t = typ;
    for (i = 1; i < cardinality; i++)
      t = t->ref;
    sprintf(temp,"[%d]",get_dimension(t));
    strcat(p, temp);
  }
  */
  return p;
}

char *
xsi_type_Darray(Tnode *typ)
{ Tnode *t;
  int cardinality;
  char *p, *s;
  if (!typ->ref)
    return "";
  t = ((Table*)typ->ref)->list->info.typ->ref;
  cardinality = 1;
  while (t->type == Tarray || (is_dynamic_array(t) && !has_ns(t) && !is_untyped(typ)))
  { if( t->type == Tarray)
      t = t->ref;
    else
      t = ((Table*)t->ref)->list->info.typ->ref;
    cardinality++;
  }
  s = xsi_type(t);
  if (!*s)
    s = wsdl_type(t, "");
  p = (char*)emalloc(strlen(s)+cardinality*2+1);
  strcpy(p, s);
  if (cardinality > 1)
  { strcat(p, "[");
    for (; cardinality > 2; cardinality--)
      strcat(p, ",");
    strcat(p, "]");
  }
  return p;
}

void
out_generate(Tnode *typ)
{
	if (is_transient(typ) || typ->type == Twchar || is_XML(typ) || is_void(typ))
	  return;
	if (is_imported(typ))
	  return;
	if (lflag && typ->type == Tint && !typ->sym)
	{ fprintf(fhead,"\n\n#ifndef SOAP_TYPE_%s",c_ident(typ));	
	  fprintf(fhead,"\n#define SOAP_TYPE_%s (%d)",c_ident(typ),typ->num);	
	  fprintf(fhead,"\n#endif");	
          fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_int(struct soap*, int*);"); 
          fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_int(struct soap*, const char*, int, const int*, const char*);"); 
          fprintf(fhead,"\nSOAP_FMAC1 int* SOAP_FMAC2 soap_in_int(struct soap*, const char*, int*, const char*);"); 
	  return; /* do not generate int serializers in libs */
	}
	if (is_primitive(typ) || is_string(typ) || is_wstring(typ))
	{	      /* typeNO++; */
	              fprintf(fhead,"\n\n#ifndef SOAP_TYPE_%s",c_ident(typ));	
		      fprintf(fhead,"\n#define SOAP_TYPE_%s (%d)",c_ident(typ),typ->num);	
		      fprintf(fhead,"\n#endif");	
			fflush(fhead);
			defaults(typ);
			serialize(typ);
			mark(typ);
			soap_put(typ);
			soap_out(typ);
			soap_get(typ);
			soap_in(typ);
	  return;
	}
        switch(typ->type)
        {
	 case Ttemplate:
	   if (!typ->ref)
	     break;
	case Tenum:
        case Tpointer:
        case Tarray:
        case Tstruct:
	 case Tclass:
	 	      /* typeNO++; */
  			if (is_header_or_fault(typ) || is_body(typ))
			{ fprintf(fhead,"\n\n#ifndef WITH_NOGLOBAL");
			  fprintf(fout,"\n\n#ifndef WITH_NOGLOBAL");
		        }
		      fprintf(fhead,"\n\n#ifndef SOAP_TYPE_%s",c_ident(typ));	
		      fprintf(fhead,"\n#define SOAP_TYPE_%s (%d)",c_ident(typ),typ->num);	
		      fprintf(fhead,"\n#endif");	
			fflush(fhead);
                      serialize(typ);  
                      mark(typ);
		      defaults(typ);	
		      soap_put(typ);
		      soap_out(typ);
		      soap_get(typ);	
		      soap_in(typ);
		      if (typ->type == Tclass || typ->type == Ttemplate || has_class(typ))
		        soap_instantiate_class(typ);
  		      if (is_header_or_fault(typ) || is_body(typ))
    		      { fprintf(fhead,"\n\n#endif");
    		        fprintf(fout,"\n\n#endif");
		      }
                      break;
              default:break;
         }
}

void
matlab_gen_sparseStruct(void)
{
  fprintf(fmheader,"\nstruct soapSparseArray{\n");
  fprintf(fmheader,"  int *ir;\n");
  fprintf(fmheader,"  int *jc;\n");
  fprintf(fmheader,"  double *pr;\n");
  fprintf(fmheader,"  int num_columns;\n");
  fprintf(fmheader,"  int num_rows;\n");
  fprintf(fmheader,"  int nzmax;\n");
  fprintf(fmheader,"};\n");
}

void
matlab_c_to_mx_sparse(void)
{
  fprintf(fmheader,"\nmxArray* c_to_mx_soapSparseArray(struct soapSparseArray);\n");
  fprintf(fmatlab,"\nmxArray* c_to_mx_soapSparseArray(struct soapSparseArray a)\n");
  fprintf(fmatlab,"{\n");
  fprintf(fmatlab,"  mxArray *b;\n");
  fprintf(fmatlab,"  b = mxCreateSparse(a.num_rows, a.num_columns, a.nzmax, mxREAL);\n");
  fprintf(fmatlab,"  mxSetIr(b,a.ir);\n");
  fprintf(fmatlab,"  mxSetJc(b,a.jc);\n");
  fprintf(fmatlab,"  mxSetPr(b,a.pr);\n");
  fprintf(fmatlab,"  return b;\n");
  fprintf(fmatlab,"}\n");
}

void 
matlab_mx_to_c_sparse(void)
{
  fprintf(fmheader,"\nmxArray* mx_to_c_soapSparseArray(const mxArray *, struct soapSparseArray *);\n");
  fprintf(fmatlab,"\nmxArray* mx_to_c_soapSparseArray(const mxArray *a, struct soapSparseArray *b)\n");
  fprintf(fmatlab,"{\n");
  fprintf(fmatlab,"  if(!mxIsSparse(a))\n");
  fprintf(fmatlab,"    {\n");
  fprintf(fmatlab,"      mexErrMsgTxt(\"Input should be a sparse array.\");\n");
  fprintf(fmatlab,"    }\n");
  
  fprintf(fmatlab,"  /* Get the starting positions of the data in the sparse array. */  \n");
  fprintf(fmatlab,"  b->pr = mxGetPr(a);\n");
  fprintf(fmatlab,"  b->ir = mxGetIr(a);\n");
  fprintf(fmatlab,"  b->jc = mxGetJc(a);\n");
  fprintf(fmatlab,"  b->num_columns = mxGetN(a);\n");
  fprintf(fmatlab,"  b->num_rows = mxGetM(a);\n");
  fprintf(fmatlab,"  b->nzmax = mxGetNzmax(a);\n");
  fprintf(fmatlab,"}\n");
}

void
matlab_mx_to_c_dynamicArray(typ)
Tnode* typ;
{  
  int d,i;
  Entry *p;

  p = is_dynamic_array(typ);

  fprintf(fmatlab,"{\n");  
  fprintf(fmatlab,"\tint i, numdims;\n");
  fprintf(fmatlab,"\tconst int *dims;\n");
  fprintf(fmatlab,"\tdouble *temp;\n");
  fprintf(fmatlab,"\tint size = 1;\n");
  fprintf(fmatlab,"\tint ret;\n");
  fprintf(fmatlab,"\tnumdims = mxGetNumberOfDimensions(a);\n");
  fprintf(fmatlab,"\tdims = mxGetDimensions(a);\n");

  d = get_Darraydims(typ);
  fprintf(fmatlab,"\tif (numdims != %d)\n", d);
  fprintf(fmatlab,"\t\tmexErrMsgTxt(\"Incompatible array specifications in C and mx.\");\n");
  
  /*
  fprintf(fmatlab,"\tfor(i=0;i<numdims; i++) {\n");
  fprintf(fmatlab,"\t  b->__size[i] = dims[i];\n");
  fprintf(fmatlab,"\t}\n");
  */

  if((((Tnode *)p->info.typ->ref)->type != Tchar) && (((Tnode *)p->info.typ->ref)->type != Tuchar))
    {
      fprintf(fmatlab,"\ttemp = (double*)mxGetPr(a);\n");
      fprintf(fmatlab,"\tif (!temp)\n\t\tmexErrMsgTxt(\"mx_to_c_ArrayOfdouble: Pointer to data is NULL\");\n");
    }

  fprintf(fmatlab,"\tfor (i = 0; i < numdims; i++) {\n");
  fprintf(fmatlab,"\t\tif (b->__size[i] < dims[i])\n");
  fprintf(fmatlab,"\t\t\tmexErrMsgTxt(\"Incompatible array dimensions in C and mx.\");\n");
  fprintf(fmatlab,"\t\tsize *= dims[i];\n");
  fprintf(fmatlab,"\t}\n");

  if((((Tnode *)p->info.typ->ref)->type != Tchar) && (((Tnode *)p->info.typ->ref)->type != Tuchar))
    { 
      fprintf(fmatlab,"\tfor (i = 0; i < size; i++)\n");
      fprintf(fmatlab,"\t\tb->__ptr[i] = (%s)*temp++;\n", c_type(p->info.typ->ref));
    }
  else
    {
      fprintf(fmatlab,"\tret = mxGetString(a, b->__ptr, size + 1);\n");
      fprintf(fmatlab,"\tmexPrintf(\"ret = %%d, b->__ptr = %%s, size = %%d\", ret, b->__ptr, size);\n");
    }
  fprintf(fmatlab,"\n}\n");

  fflush(fmatlab);
}


void
matlab_c_to_mx_dynamicArray(typ)
Tnode* typ;
{  
  int d,i;
  Entry *p;

  p = is_dynamic_array(typ);

  fprintf(fmatlab,"{\n");  
  fprintf(fmatlab,"\tmxArray *out;\n");
  fprintf(fmatlab,"\t%s;\n",c_type_id(p->info.typ->ref,"*temp"));
  d = get_Darraydims(typ);
  fprintf(fmatlab,"\tint i;\n");

  fprintf(fmatlab,"\tint ndim = %d, dims[%d] = {", d, d);
  for (i = 0; i < d; i++)
    { 
      if(i==0)
	fprintf(fmatlab,"a.__size[%d]",i);
      else
	fprintf(fmatlab,", a.__size[%d]",i);
    }
  fprintf(fmatlab,"};\n");

  fprintf(fmatlab,"\tint size = ");
   for (i = 0; i < d; i++)
    { 
      if(i==0)
	fprintf(fmatlab,"dims[%d]",i);
      else
	fprintf(fmatlab,"*dims[%d]",i);
    }
   fprintf(fmatlab,";\n");
   if((((Tnode *)p->info.typ->ref)->type != Tchar) && (((Tnode *)p->info.typ->ref)->type != Tuchar))
     {
       fprintf(fmatlab,"\tout = mxCreateNumericArray(ndim, dims, %s, mxREAL);\n",get_mxClassID(p->info.typ->ref));
       fprintf(fmatlab,"\tif (!out)\n\t\tmexErrMsgTxt(\"Could not create mxArray.\");\n");
       fprintf(fmatlab,"\ttemp = (%s) mxGetPr(out);\n",c_type_id(p->info.typ->ref,"*"));
       fprintf(fmatlab,"\tif (!temp)\n\t\tmexErrMsgTxt(\"matlab_array_c_to_mx: Pointer to data is NULL\");\n");

       fprintf(fmatlab,"\tfor (i = 0; i < size; i++)\n");
       fprintf(fmatlab,"\t\t*temp++ = a.__ptr[i];\n");
     }
   else
     {
       fprintf(fmatlab,"\tout = mxCreateString(a.__ptr);\n");
       fprintf(fmatlab,"\tif (!out)\n\t\tmexErrMsgTxt(\"Could not create mxArray.\");\n");
     }
  fprintf(fmatlab,"\treturn out;\n}\n");
  fflush(fmatlab);
}

char* 
get_mxClassID(Tnode* typ)
{
  
  switch(typ->type)
    {
    case Tdouble: 
      return "mxDOUBLE_CLASS";
    case Tfloat:
      return "mxSINGLE_CLASS";
    case Tshort:
      return "mxINT16_CLASS";
    case Tushort:
      return "mxUINT16_CLASS";
    case Tint:
      return "mxINT32_CLASS";
    case Tuint:
      return "mxUINT32_CLASS";
    case Tlong:
      return "mxINT32_CLASS";
    case Tulong:
      return "mxUINT32_CLASS";
    case Tllong:
      return "mxINT64_CLASS";
    case Tullong:
      return "mxUINT64_CLASS";
    case Tchar:
      return "mxCHAR_CLASS";
    case Tuchar:
      return "mxCHAR_CLASS";
    default:
      return "";
    };
}

/*Function not in use.*/
void 
matlab_array_c_to_mx(typ)
Tnode* typ;
{
  Tnode* temp;
  int cardinality;
  int d,i;
  
  fprintf(fmatlab,"{\n\tint rows, r, cols, c;\n");
  fprintf(fmatlab,"\tmxArray* out;\n");
  fprintf(fmatlab,"\tdouble* temp;\n");
  d = get_dimension(typ);
  fprintf(fmatlab,"\tint ndim = %d, dims[%d] = {",d,d);
  temp=typ;
  for(i=0;i<d; i++)
    {
      if(i==0)
	fprintf(fmatlab,"%d",temp->width / ((Tnode*) temp->ref)->width);
      else
	fprintf(fmatlab,",%d",temp->width / ((Tnode*) temp->ref)->width);
      temp=typ->ref;
    }
  fprintf(fmatlab,"};\n");

  fprintf(fmatlab,"\tout = mxCreateNumericArray(ndim, dims, mxDOUBLE_CLASS, mxREAL);\n");
  fprintf(fmatlab,"\ttemp = (double *) mxGetPr(out);\n");
  fprintf(fmatlab,"\tif (!out)\n\t\tmexErrMsgTxt(\"Could not create mxArray.\");\n");
  fprintf(fmatlab,"\tif (!temp)\n\t\tmexErrMsgTxt(\"matlab_array_c_to_mx: Pointer to data is NULL\");\n");
  fprintf(fmatlab,"\trows = mxGetM(out);\n");
  fprintf(fmatlab,"\tif (!rows)\n\t\tmexErrMsgTxt(\"matlab_array_c_to_mx: Data has zero rows\");\n");
  fprintf(fmatlab,"\tcols = mxGetN(out);\n");
  fprintf(fmatlab,"\tif (!cols)\n\t\tmexErrMsgTxt(\"matlab_array_c_to_mx: Data has zero columns\");\n");
  fprintf(fmatlab,"\tfor (c = 0; c < cols; c++)\n");
  fprintf(fmatlab,"\t\tfor (r = 0; r < rows; r++)\n");
  fprintf(fmatlab,"\t\t\t*temp++ = z->a[r][c];\n");
  fprintf(fmatlab,"\treturn out;\n}\n");
  fflush(fmatlab);
}


void matlab_c_to_mx_pointer(typ)
Tnode* typ;
{
  if (!typ->ref)
    return;

  /*  if(((Tnode*)typ->ref)->type == Tstruct)
    {
      fprintf(fmheader,"\nmxArray* c_to_mx_%s(%s);\n",c_ident(typ),c_type_id(typ, "*"));
      fprintf(fmatlab,"\nmxArray* c_to_mx_%s(%s)\n",c_ident(typ),c_type_id(typ, "*a"));
    }
  else
  {*/
  fprintf(fmheader,"\nmxArray* c_to_mx_%s(%s);\n",c_ident(typ),c_type_id(typ, ""));
  fprintf(fmatlab,"\nmxArray* c_to_mx_%s(%s)\n",c_ident(typ),c_type_id(typ, "a"));
      /*  }*/
  fprintf(fmatlab,"{\n");
  fprintf(fmatlab,"\tmxArray  *fout;\n");
  fprintf(fmatlab,"\tfout = c_to_mx_%s(*a);\n",c_ident(typ->ref));
  fprintf(fmatlab,"\treturn fout;\n");
  fprintf(fmatlab,"}\n");
}

void matlab_mx_to_c_pointer(typ)
Tnode* typ;
{
  if (!typ->ref)
    return;
  fprintf(fmheader,"\nvoid mx_to_c_%s(const mxArray*,%s);\n",c_ident(typ),c_type_id(typ, "*"));
  fprintf(fmatlab,"\nvoid mx_to_c_%s(const mxArray* a,%s)\n",c_ident(typ),c_type_id(typ, "*b"));
  fprintf(fmatlab,"{\n\tmx_to_c_%s(a,*b);\n",c_ident(typ->ref));
  fprintf(fmatlab,"\n}\n");
}

void func2(typ)
Tnode* typ;
{
  Table *table,*t;
  Entry *p;

  fprintf(fmatlab,"\tif(!mxIsStruct(a))\n\t\tmexErrMsgTxt(\"Input must be a structure.\");\n");

  table=(Table*)typ->ref;
  for (t = table; t != (Table *) 0; t = t->prev) { 
    for (p = t->list; p != (Entry*) 0; p = p->next) {
      if (p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_XML(p->info.typ))
	{
	  fprintf(fmatlab,"\t{mxArray *tmp = mxGetField(a,0,\"%s\");\n",p->sym->name);
	  fprintf(fmatlab,"\tif (!tmp) {\n");
	  fprintf(fmatlab,"\t\tmexErrMsgTxt(\"Above field is empty!\");\n\t}\n");   
	  fprintf(fmatlab,"\tmx_to_c_%s(tmp,&(b->%s));}\n",c_ident(p->info.typ),p->sym->name);
	}
    }
  }
}

void 
matlab_mx_to_c_struct(typ)
Tnode* typ;
{
  if (!typ->ref)
    return;

  
  if (is_dynamic_array(typ))
    {
      fprintf(fmheader,"\nvoid mx_to_c_%s(const mxArray*, %s);\n",c_ident(typ),c_type_id(typ, "*"));
      fprintf(fmatlab,"\nvoid mx_to_c_%s(const mxArray* a, %s)\n",c_ident(typ),c_type_id(typ, "*b"));
      matlab_mx_to_c_dynamicArray(typ);
      return;
    }
  else if(strstr(c_type_id(typ, ""),"soapSparseArray"))
    {
      return;
    }

  fprintf(fmheader,"\nvoid mx_to_c_%s(const mxArray*, %s);\n",c_ident(typ),c_type_id(typ, "*"));
  fprintf(fmatlab,"\nvoid mx_to_c_%s(const mxArray* a, %s)\n",c_ident(typ),c_type_id(typ, "*b"));
  fprintf(fmatlab,"{\n");
  
  func2(typ);
  fprintf(fmatlab,"\n}\n");
  
  return;
}



void
matlab_c_to_mx_struct(typ)
Tnode* typ;
{
  Table *table,*t;
  Entry *p;
  int number_of_fields=0;

  if (!typ->ref)
    return;

  if (is_dynamic_array(typ))
    {
      fprintf(fmheader,"\nmxArray* c_to_mx_%s(%s);\n",c_ident(typ),c_type_id(typ, ""));
      fprintf(fmatlab,"\nmxArray* c_to_mx_%s(%s)\n",c_ident(typ),c_type_id(typ, "a"));
      matlab_c_to_mx_dynamicArray(typ);
      return;
    }
  else if(strstr(c_type_id(typ, ""),"soapSparseArray"))
    {
      return;
    }
  
  fprintf(fmheader,"\nmxArray* c_to_mx_%s(%s);\n",c_ident(typ),c_type_id(typ, ""));
  fprintf(fmatlab,"\nmxArray* c_to_mx_%s(%s)\n",c_ident(typ),c_type_id(typ, "a"));
  table=(Table*)typ->ref;	
  fprintf(fmatlab,"{\n\tconst char* fnames[] = {"); 
  for (t = table; t != (Table *) 0; t = t->prev) { 
    for (p = t->list; p != (Entry*) 0; p = p->next) {
      if (p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_XML(p->info.typ))
	{
	  if(number_of_fields)
	    fprintf(fmatlab,",\"%s\"",p->sym->name);
	  else 
	    fprintf(fmatlab,"\"%s\"",p->sym->name);
	  number_of_fields++;
	}
    }
  }	
  fprintf(fmatlab,"}; /* pointers to field names*/\n"); 
  
  fprintf(fmatlab,"\tint rows = 1, cols = 1;\n\tint index = 0;\n\tint number_of_fields = %d;\n\tmxArray *struct_array_ptr;\n",number_of_fields);
  fprintf(fmatlab,"\t/* Create a 1x1 struct matrix for output  */\n");
  fprintf(fmatlab,"\tstruct_array_ptr = mxCreateStructMatrix(rows, cols, number_of_fields, fnames);\n\tmexPrintf(\"6\");\n\tif(struct_array_ptr == NULL) {\n\t\tmexPrintf(\"COULDNT CREATE A MATRIX\");}\n\tmexPrintf(\"7\");\n");
  
  
  for (t = table; t != (Table *) 0; t = t->prev) { 
    for (p = t->list; p != (Entry*) 0; p = p->next) {
      if (p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_XML(p->info.typ))
	{
	  fprintf(fmatlab,"\t{mxArray *fout = c_to_mx_%s(a.%s);\n",c_ident(p->info.typ),p->sym->name);
	  fprintf(fmatlab,"\tmxSetField(struct_array_ptr, index,\"%s\" , fout);}\n",p->sym->name);
	}
    }
  }
  fprintf(fmatlab,"\treturn struct_array_ptr;\n}\n");
  return;
}
/*
char*
matlab_c_to_mx(typ)
Tnode* typ;
{

  switch(typ->type)
    {
    case Tstruct:
      break;
    case Tarray:
      matlab_array_c_to_mx(typ);break;
    case Tpointer:
      fprintf(fmheader,"\npointer in matlab_c_to_mx\n");break;
    default:break;
    }
  
  return NULL;
}
*/

void
matlab_c_to_mx_primitive(typ)
Tnode *typ;
{
  fprintf(fmheader,"\nmxArray* c_to_mx_%s(%s);",c_ident(typ),c_type_id(typ, ""));
  fprintf(fmatlab,"\nmxArray* c_to_mx_%s(%s)\n",c_ident(typ),c_type_id(typ, "a"));

  fprintf(fmatlab,"{\n\tmxArray  *fout;\n");
  if((typ->type == Tchar) || (typ->type == Tuchar))
    {
      fprintf(fmatlab,"\tchar buf[2];\n");
      fprintf(fmatlab,"\tbuf[0] = a;\n");
      fprintf(fmatlab,"\tbuf[1] = \'\\0\';\n");
      fprintf(fmatlab,"\tfout = mxCreateString(buf);\n");
      fprintf(fmatlab,"\tif (!fout)\n");
      fprintf(fmatlab,"\t\tmexErrMsgTxt(\"Could not create mxArray.\");\n");
    }
  else
    {
      fprintf(fmatlab,"\tint ndim = 1, dims[1] = {1};\n");
      fprintf(fmatlab,"\tfout = mxCreateNumericArray(ndim, dims, %s, mxREAL);\n",get_mxClassID(typ));
      fprintf(fmatlab,"\t%s = (%s)mxGetPr(fout);\n",c_type_id(typ,"*temp"),c_type_id(typ,"*"));
      fprintf(fmatlab,"\tif (!fout)\n");
      fprintf(fmatlab,"\t\tmexErrMsgTxt(\"Could not create mxArray.\");\n");
      fprintf(fmatlab,"\tif (!temp) \n");
      fprintf(fmatlab,"\t\tmexErrMsgTxt(\"matlab_array_c_to_mx: Pointer to data is NULL\");\n");
      fprintf(fmatlab,"\t*temp++= a;\n");
    }
  fprintf(fmatlab,"\treturn fout;\n}\n");
}

void
matlab_mx_to_c_primitive(typ)
Tnode *typ;
{
  fprintf(fmheader, "\nvoid mx_to_c_%s(const mxArray *, %s);\n",c_ident(typ),c_type_id(typ, "*"));
  fprintf(fmatlab, "\nvoid mx_to_c_%s(const mxArray *a, %s)\n",c_ident(typ),c_type_id(typ, "*b"));
  if((typ->type == Tchar) || (typ->type == Tuchar))
    {
      fprintf(fmatlab,"{\n\tint ret;\n");
      fprintf(fmatlab,"\tchar buf[2];\n");
      fprintf(fmatlab,"\tret = mxGetString(a, buf, 2);\n");
      fprintf(fmatlab,"\tmexPrintf(\"ret = %%d, buf = %%s\", ret, buf);\n");
      fprintf(fmatlab,"\t*b = buf[0];\n");
    }
  else
    {
      fprintf(fmatlab,"{\n\tdouble* data = (double*)mxGetData(a);\n");
      fprintf(fmatlab,"\t*b = (%s)*data;\n",c_type(typ));
    }
      fprintf(fmatlab,"\n}\n");
}

void
matlab_out_generate(typ)
Tnode *typ;
{

  if (is_transient(typ) || typ->type == Twchar || is_XML(typ))
    return;

  /*
  typeNO++;
  if (typeNO>=1024)
    execerror("Too many user-defined data types");
    */

  if(is_primitive(typ))
    {
      matlab_c_to_mx_primitive(typ);
      matlab_mx_to_c_primitive(typ);
      return;
    }

  switch(typ->type)
    {
    case Tstruct:
      matlab_c_to_mx_struct(typ);
      matlab_mx_to_c_struct(typ);
      break;
    case Tpointer:
      matlab_c_to_mx_pointer(typ);
      matlab_mx_to_c_pointer(typ);
      break;
    case Tarray:
      break;
    default:break;
    }
}

/*his function is called first it first generates all routines
  and then in the second pass calls all routines to generate
  matlab_out for the table*/

void
func1(Table *table, Entry *param)
{ 

  Service *sp;
  Entry *pin,*q,*pout,*response=NULL;
  Tnode *temp;
  Table *output,*t;
  int cardinality, element_width, i, flag = 0;
  q=entry(table, param->sym);
  if (q)
    pout = (Entry*)q->info.typ->ref;
  else	fprintf(stderr, "Internal error: no table entry\n");
  q=entry(classtable, param->sym);
  output=(Table*) q->info.typ->ref;

  if (!is_response(pout->info.typ))
  { response = get_response(param->info.typ);
  }
  
  fprintf(fmheader,"\n\toutside loop struct %s soap_tmp_%s;",param->sym->name,param->sym->name);
  if (!is_response(pout->info.typ) && response)
  { fprintf(fmheader,"\n\tif..inside loop struct %s *soap_tmp_%s;",c_ident(response->info.typ), c_ident(response->info.typ));
  } 

  fflush(fmheader);
}

void
matlab_def_table(table,typ)
     Table *table;
     Tnode *typ;
{
  Entry *q1,*q,*pout,*e,*response;
  int i;
  Tnode *p;
  char temp[100];

  /*  for (q1 = table->list; q1 != (Entry*) 0; q1 = q1->next)
    if (q1->info.typ->type==Tfun)
      func1(table, q1);
  */

  /* Sparse matrix code will be present by default */
  matlab_gen_sparseStruct();
  matlab_c_to_mx_sparse();
  matlab_mx_to_c_sparse();  

  for(i=0;i<TYPES;i++)
    for(p=Tptr[i];p!=(Tnode*) 0;p=p->next)
      {
	/* This is generated for everything declared in the ".h" file. To make
	   sure that it doesnt get generated for functions do a comparison with
	   p->sym->name, so that its not generated for functions.
	*/
	if(is_XML(p))
	  continue;
	if(strstr(c_ident(p),"SOAP_ENV_") != NULL)
	  continue;
	
	for(q = table->list; q != (Entry*) 0; q = q->next)
	  {
	    if(strcmp(c_ident(p),q->sym->name) == 0)
	      break;
	    

	    e=entry(table, q->sym);
	    if (e)
	      pout = (Entry*)e->info.typ->ref;
	    else	fprintf(stderr, "Internal error: no table entry\n");
	    
	    if (!is_response(pout->info.typ))
	    { response = get_response(q->info.typ);
	    }
	    if (!is_response(pout->info.typ) && response)
	    {
	      if(strcmp(c_ident(p),c_ident(response->info.typ)) == 0)
		 break;
	    }
	  }

	if(q == (Entry*) 0)
	  matlab_out_generate(p);	  
      }
}

void
def_table(table,typ)
Table *table;  
Tnode *typ; 
{ 
  int i;  
  Tnode *p; 
  for(i=0;i<TYPES;i++)
    for(p=Tptr[i];p!=(Tnode*) 0;p=p->next)
	out_generate(p);
}

         
int 
no_of_var(typ)
Tnode * typ;
{
  Entry *p;
  Table *t;
  int i=0;
  if(typ->type==Tstruct || typ->type==Tclass)
    {
      t=typ->ref;
      for (p = t->list; p != (Entry*) 0; p = p->next) {
	if(p->info.typ->type==Tpointer)
	  i++;
      }
    }
  if((((Tnode *)(typ->ref))->type==Tstruct) ||
     (((Tnode *)(typ->ref))->type==Tclass) )
    {
      t=((Tnode*)(typ->ref))->ref;
      for (p = t->list; p != (Entry*) 0; p = p->next) {
	if(p->info.typ->type==Tpointer)
	  i++;
      }
    }
  return i;
}      

void
in_defs(Table *table)
{ int i;  
  Tnode *p;
  for (i = 0; i < TYPES; i++)
  { for (p = Tptr[i]; p; p = p->next)
    { if (!is_transient(p) && p->type != Twchar && p->type != Tfun && p->type != Treference && !is_XML(p) && !is_header_or_fault(p) && !is_body(p) && !is_template(p))
      { char *s = xsi_type(p);
        if (!*s)
          s = wsdl_type(p, "");
	if (*s == '-')
	  continue;
	if (is_string(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t{\tchar **s;\n\t\ts = soap_in_%s(soap, NULL, NULL, \"%s\");\n\t\treturn s ? *s : NULL;\n\t}",c_ident(p), c_ident(p), s);
	else if (is_wstring(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t{\twchar_t **s;\n\t\ts = soap_in_%s(soap, NULL, NULL, \"%s\");\n\t\treturn s ? *s : NULL;\n\t}",c_ident(p), c_ident(p), s);
	else
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_in_%s(soap, NULL, NULL, \"%s\");",c_ident(p), c_ident(p), s);
      }
    }
  }
}

void
in_defs2(Table *table)
{ int i;
  Tnode *p;
  char *s;
  for (i = 0; i < TYPES; i++)
  { for (p = Tptr[i]; p; p = p->next)
    { if (!is_transient(p) && !is_template(p) && p->type != Twchar && p->type != Tfun && p->type != Tpointer && p->type != Treference && !is_XML(p) && !is_header_or_fault(p) && !is_body(p) || is_string(p) && !is_XML(p))
      { s = xsi_type(p);
	if (!*s)
	  s = wsdl_type(p, "");
	if (*s == '-')
	  continue;
	if (*s)
	  if (is_dynamic_array(p) && !is_binary(p) && !has_ns(p) && !is_untyped(p))
	    fprintf(fout,"\n\t\tif (*soap->arrayType && !soap_match_array(soap, \"%s\"))\n\t\t{\t*type = SOAP_TYPE_%s;\n\t\t\treturn soap_in_%s(soap, NULL, NULL, NULL);\n\t\t}", s, c_ident(p), c_ident(p));
	  else if (is_string(p))
	    fprintf(fout,"\n\t\tif (!soap_match_tag(soap, t, \"%s\"))\n\t\t{\tchar **s;\n\t\t\t*type = SOAP_TYPE_%s;\n\t\t\ts = soap_in_%s(soap, NULL, NULL, NULL);\n\t\t\treturn s ? *s : NULL;\n\t\t}", s, c_ident(p), c_ident(p));
	  else if (is_wstring(p))
	    fprintf(fout,"\n\t\tif (!soap_match_tag(soap, t, \"%s\"))\n\t\t{\twchar_t **s;\n\t\t\t*type = SOAP_TYPE_%s;\n\t\t\ts = soap_in_%s(soap, NULL, NULL, NULL);\n\t\t\treturn s ? *s : NULL;\n\t\t}", s, c_ident(p), c_ident(p));
          else
	    fprintf(fout,"\n\t\tif (!soap_match_tag(soap, t, \"%s\"))\n\t\t{\t*type = SOAP_TYPE_%s;\n\t\t\treturn soap_in_%s(soap, NULL, NULL, NULL);\n\t\t}", s, c_ident(p), c_ident(p));
      }
    }
  }
}

void
out_defs(Table *table)
{ int i;  
  char *s;
  Tnode *p;
  for (i = 0; i < TYPES; i++)
  { for (p = Tptr[i]; p; p = p->next)
    { if (is_transient(p) || is_template(p) || is_XML(p) || is_header_or_fault(p) || is_body(p))
        continue;
      if (is_element(p))
      { s = wsdl_type(p, "");
	if (*s == '-')
	  continue;
        if (p->type == Tarray)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, \"%s\", id, (%s)ptr, NULL);",c_ident(p),c_ident(p),s,c_type_id(p->ref, "(*)"));
        else if(p->type == Tclass && !is_external(p) && !is_volatile(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn ((%s)ptr)->soap_out(soap, \"%s\", id, NULL);",c_ident(p), c_type_id(p, "*"),s);
        else if (is_string(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_string(soap, \"%s\", id, (char**)&ptr, NULL);",c_ident(p),s);
        else if (is_wstring(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_wstring(soap, \"%s\", id, (wchar_t**)&ptr, NULL);",c_ident(p),s);
        else if (p->type == Tpointer)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, \"%s\", id, (%s)ptr, NULL);",c_ident(p),c_ident(p),s,c_type_id(p, "const*"));
        else if(p->type != Tnone && p->type != Ttemplate && p->type != Twchar && !is_void(p) && p->type != Tfun && p->type != Treference && p->type != Tunion)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, \"%s\", id, (const %s)ptr, NULL);",c_ident(p),c_ident(p),s,c_type_id(p, "*"));
      }
      else
      { s = xsi_type(p);
        if (!*s)
          s = wsdl_type(p, "");
        if (*s == '-')
          continue;
        if (p->type == Tarray)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, tag, id, (%s)ptr, \"%s\");",c_ident(p), c_ident(p),c_type_id(p->ref, "(*)"), s);
        else if(p->type == Tclass && !is_external(p) && !is_volatile(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn ((%s)ptr)->soap_out(soap, tag, id, \"%s\");",c_ident(p), c_type_id(p, "*"), s);
        else if (is_string(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_string(soap, tag, id, (char**)&ptr, \"%s\");",c_ident(p), s);
        else if (is_wstring(p))
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_wstring(soap, tag, id, (wchar_t**)&ptr, \"%s\");",c_ident(p), s);
        else if (p->type == Tpointer)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, tag, id, (%s)ptr, \"%s\");",c_ident(p), c_ident(p),c_type_id(p, "const*"), s);
        else if(p->type != Tnone && p->type != Ttemplate && p->type != Twchar && !is_void(p) && p->type != Tfun && p->type != Treference && p->type != Tunion)
          fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\treturn soap_out_%s(soap, tag, id, (const %s)ptr, \"%s\");",c_ident(p), c_ident(p),c_type_id(p, "*"), s);
      }
    }
  }
}

void
mark_defs(Table *table)
{ int i;  
  Tnode *p;
  for (i = 0; i < TYPES; i++)
  { for (p = Tptr[i]; p; p = p->next)
    { if (is_transient(p) || is_template(p) || is_XML(p) || is_header_or_fault(p) || is_body(p) || is_void(p))
        continue;
      if (p->type == Tarray)
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_%s(soap, (%s)ptr);\n\t\tbreak;",c_ident(p), c_ident(p),c_type_id(p->ref, "(*)"));
      else if(p->type == Tclass && !is_external(p) && !is_volatile(p))
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\t((%s)ptr)->soap_mark(soap);\n\t\tbreak;",c_ident(p), c_type_id(p, "*"));
      else if (is_string(p))
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_string(soap, (char**)&ptr);\n\t\tbreak;",c_ident(p));
      else if (is_wstring(p))
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_wstring(soap, (wchar_t**)&ptr);\n\t\tbreak;",c_ident(p));
      else if (p->type == Tpointer)
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_%s(soap, (%s)ptr);\n\t\tbreak;",c_ident(p), c_ident(p),c_type_id(p, "const*"));
      else if(p->type == Ttemplate && p->ref)
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_%s(soap, (const %s)ptr);\n\t\tbreak;",c_ident(p), c_ident(p),c_type_id(p, "*"));
      else if(!is_primitive(p) && p->type != Tnone && p->type != Ttemplate && !is_void(p) && p->type != Tfun && p->type != Treference && p->type != Tunion)
        fprintf(fout,"\n\tcase SOAP_TYPE_%s:\n\t\tsoap_mark_%s(soap, (const %s)ptr);\n\t\tbreak;",c_ident(p), c_ident(p),c_type_id(p, "*"));
    }
  }
}

void
in_attach(Table *table)
{ int i;  
  Tnode *p;
  for (i = 0; i < TYPES; i++)
    for (p = Tptr[i]; p; p = p->next)
      if (is_attachment(p))
        if (p->type == Tclass)
	  fprintf(fout,"\n\t\tcase SOAP_TYPE_%s:\n\t\t{\t%s a;\n\t\t\ta = (%s)soap_class_id_enter(soap, soap->dime.id, NULL, SOAP_TYPE_%s, sizeof(%s), NULL, NULL);\n\t\t\tif (a)\n\t\t\t{\ta->__ptr = (unsigned char*)soap->dime.ptr;\n\t\t\t\ta->__size = soap->dime.size;\n\t\t\t\ta->id = (char*)soap->dime.id;\n\t\t\t\ta->type = (char*)soap->dime.type;\n\t\t\t\ta->options = (char*)soap->dime.options;\n\t\t\t}\n\t\t\telse\n\t\t\t\treturn soap->error;\n\t\t\tbreak;\n\t\t}", c_ident(p), c_type_id(p, "*"), c_type_id(p, "*"), c_ident(p), c_type(p));
	else
	  fprintf(fout,"\n\t\tcase SOAP_TYPE_%s:\n\t\t{\t%s a;\n\t\t\ta = (%s)soap_id_enter(soap, soap->dime.id, NULL, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);\n\t\t\tif (!a)\n\t\t\t\treturn soap->error;\n\t\t\ta->__ptr = (unsigned char*)soap->dime.ptr;\n\t\t\ta->__size = soap->dime.size;\n\t\t\ta->id = (char*)soap->dime.id;\n\t\t\ta->type = (char*)soap->dime.type;\n\t\t\ta->options = (char*)soap->dime.options;\n\t\t\tbreak;\n\t\t}", c_ident(p), c_type_id(p, "*"), c_type_id(p, "*"), c_ident(p), c_type(p));
      else if (is_binary(p) && !is_transient(p))
        if (p->type == Tclass)
	  fprintf(fout,"\n\t\tcase SOAP_TYPE_%s:\n\t\t{\t%s a;\n\t\t\ta = (%s)soap_class_id_enter(soap, soap->dime.id, NULL, SOAP_TYPE_%s, sizeof(%s), NULL, NULL);\n\t\t\tif (!a)\n\t\t\t\treturn soap->error;\n\t\t\ta->__ptr = (unsigned char*)soap->dime.ptr;\n\t\t\ta->__size = soap->dime.size;\n\t\t\tbreak;\n\t\t}", c_ident(p), c_type_id(p, "*"), c_type_id(p, "*"), c_ident(p), c_type(p));
	else
	  fprintf(fout,"\n\t\tcase SOAP_TYPE_%s:\n\t\t{\t%s a;\n\t\t\ta = (%s)soap_id_enter(soap, soap->dime.id, NULL, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);\n\t\t\tif (!a)\n\t\t\t\treturn soap->error;\n\t\t\ta->__ptr = (unsigned char*)soap->dime.ptr;\n\t\t\ta->__size = soap->dime.size;\n\t\t\tbreak;\n\t\t}", c_ident(p), c_type_id(p, "*"), c_type_id(p, "*"), c_ident(p), c_type(p));
}

/*
void
out_attach(Table *table)
{ int i;  
  char *s;
  Tnode *p;
  for (i = 0; i < TYPES; i++)
    for (p = Tptr[i]; p; p = p->next)
      if (is_attachment(p))
      { s = c_type_id(p, "*");
        fprintf(fout,"\n\t\t\t\tcase SOAP_TYPE_%s:\n\t\t\t\t\tsoap_putdime(soap, pp->id, ((%s)pp->ptr)->id, ((%s)pp->ptr)->type, ((%s)pp->ptr)->options, ((%s)pp->ptr)->__ptr, ((%s)pp->ptr)->__size);\n\t\t\t\t\tbreak;", c_ident(p), s, s, s, s, s);
      }
}
*/

void
soap_instantiate_class(Tnode *typ)
{ Table *Tptr;
  Entry *Eptr;
  int derclass = 0;
  char *s;
  
  fprintf(fhead,"\n\nSOAP_FMAC5 %s * SOAP_FMAC6 soap_new_%s(struct soap*, int);", c_type(typ), c_ident(typ));
  fprintf(fout,"\n\nSOAP_FMAC5 %s * SOAP_FMAC6 soap_new_%s(struct soap *soap, int n)\n{\treturn soap_instantiate_%s(soap, n, NULL, NULL, NULL);\n}", c_type(typ), c_ident(typ), c_ident(typ));
  fprintf(fhead,"\n\nSOAP_FMAC5 void SOAP_FMAC6 soap_delete_%s(struct soap*, %s*);", c_ident(typ), c_type(typ));
  fprintf(fout,"\n\nSOAP_FMAC5 void SOAP_FMAC6 soap_delete_%s(struct soap *soap, %s)\n{\tsoap_delete(soap, p);\n}", c_ident(typ), c_type_id(typ, "*p"));
  fprintf(fhead,"\nSOAP_FMAC5 %s * SOAP_FMAC6 soap_instantiate_%s(struct soap*, int, const char*, const char*, size_t*);", c_type(typ), c_ident(typ));
  fprintf(fout,"\n\nSOAP_FMAC5 %s * SOAP_FMAC6 soap_instantiate_%s(struct soap *soap, int n, const char *type, const char *arrayType, size_t *size)", c_type(typ), c_ident(typ));
  fprintf(fout,"\n{");
  fprintf(fout, "\nDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"soap_instantiate_%s(%%d, %%s, %%s)\\n\", n, type?type:\"\", arrayType?arrayType:\"\"));", c_ident(typ));

  /*
  fprintf(fout,"\n\tstruct soap_clist *cp = (struct soap_clist*)malloc(sizeof(struct soap_clist));");
  */
  fprintf(fout,"\n\tstruct soap_clist *cp = soap_link(soap, NULL, SOAP_TYPE_%s, n, soap_fdelete);", c_ident(typ));
  fprintf(fout,"\n\tif (!cp)\n\t\treturn NULL;");
  /*
  fprintf(fout,"\n\tsoap->alloced = 1;");
  */
  for (Eptr = classtable->list; Eptr; Eptr = Eptr->next)
  {
    Tptr = ((Table *) Eptr->info.typ->ref);
    if(Tptr == ((Table *) typ->ref)){
      continue;
    }
    
    derclass = 0;
    while(Tptr)
    {
      if(Tptr == typ->ref){
	derclass = 1;
      }

      Tptr = Tptr->prev;
    }

    if(derclass == 1 && !is_transient(Eptr->info.typ)){
      if (is_dynamic_array(Eptr->info.typ) && !is_binary(Eptr->info.typ) && !has_ns(Eptr->info.typ) && !is_untyped(Eptr->info.typ))
        fprintf(fout,"\n\tif (arrayType && !soap_match_tag(soap, arrayType, \"%s\"))", xsi_type(Eptr->info.typ));
      else
        fprintf(fout,"\n\tif (type && !soap_match_tag(soap, type, \"%s\"))", the_type(Eptr->info.typ));
      /*
      fprintf(fout,"\n\t{\tcp->next = soap->clist;");
      fprintf(fout,"\n\t\tcp->size = n;");
      fprintf(fout,"\n\t\tsoap->clist = cp;");
      fprintf(fout,"\n\t\tif (n < 0)");
      */
      fprintf(fout,"\n\t{\tcp->type = SOAP_TYPE_%s;", c_ident(Eptr->info.typ));
      fprintf(fout,"\n\t\tif (n < 0)");
      fprintf(fout,"\n\t\t{\tcp->ptr = (void*)new %s;", c_type(Eptr->info.typ));
      fprintf(fout,"\n\t\t\tif (size)\n\t\t\t\t*size = sizeof(%s);", c_type(Eptr->info.typ));
      if ((s = has_soapref(Eptr->info.typ)))
        fprintf(fout,"\n\t\t\t((%s*)cp->ptr)->%s = soap;", c_type(Eptr->info.typ), s);
      fprintf(fout,"\n\t\t}\n\t\telse");
      fprintf(fout,"\n\t\t{\tcp->ptr = (void*)new %s[n];", c_type(Eptr->info.typ));
      fprintf(fout,"\n\t\t\tif (size)\n\t\t\t\t*size = n * sizeof(%s);", c_type(Eptr->info.typ));
      if (s)
        fprintf(fout,"\n\t\t\tfor (int i = 0; i < n; i++)\n\t\t\t\t((%s*)cp->ptr)[i].%s = soap;", c_type(Eptr->info.typ), s);
      fprintf(fout,"\n\t\t}");
      fprintf(fout,"\n\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Instantiated location=%%p\\n\", cp->ptr));");
      fprintf(fout,"\n\t\treturn (%s*)cp->ptr;", c_type(Eptr->info.typ));
      fprintf(fout,"\n\t}");

      derclass = 0;
    }
  }

      /*
      fprintf(fout,"\n\tcp->next = soap->clist;");
      fprintf(fout,"\n\tcp->type = SOAP_TYPE_%s;", c_ident(typ));
      fprintf(fout,"\n\tcp->size = n; ");
      fprintf(fout,"\n\tsoap->clist = cp;");
      */
      fprintf(fout,"\n\tif (n < 0)");
      fprintf(fout,"\n\t{\tcp->ptr = (void*)new %s;", c_type(typ));
      fprintf(fout,"\n\t\tif (size)\n\t\t\t*size = sizeof(%s);", c_type(typ));
      if ((s = has_soapref(typ)))
        fprintf(fout,"\n\t\t((%s*)cp->ptr)->%s = soap;", c_type(typ), s);
      fprintf(fout,"\n\t}\n\telse");
      fprintf(fout,"\n\t{\tcp->ptr = (void*)new %s[n];", c_type(typ));
      fprintf(fout,"\n\t\tif (size)\n\t\t\t*size = n * sizeof(%s);", c_type(typ));
      if (s)
        fprintf(fout,"\n\t\tfor (int i = 0; i < n; i++)\n\t\t\t((%s*)cp->ptr)[i].%s = soap;", c_type(typ), s);
      fprintf(fout,"\n\t}");
      fprintf(fout,"\n\t\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Instantiated location=%%p\\n\", cp->ptr));");
      fprintf(fout,"\n\treturn (%s*)cp->ptr;", c_type(typ));
  
  fprintf(fout,"\n}");

  fprintf(fhead,"\nSOAP_FMAC5 void SOAP_FMAC6 soap_copy_%s(struct soap*, int, int, void*, const void*, size_t);", c_ident(typ));
  fprintf(fout,"\n\nSOAP_FMAC5 void SOAP_FMAC6 soap_copy_%s(struct soap *soap, int st, int tt, void *p, const void *q, size_t n)", c_ident(typ));
  fprintf(fout,"\n{");
  fprintf(fout,"\n\tDBGLOG(TEST, SOAP_MESSAGE(fdebug, \"Copying %s %%p -> %%p\\n\", q, p));", c_type(typ));
  fprintf(fout,"\n\t*(%s*)p = *(%s*)q;\n}", c_type(typ), c_type(typ));
}

void
serialize(Tnode *typ) 
{ if(typ->type==Tarray){
    /* ARRAY */
    fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap*, %s);", c_ident(typ), c_type_id(typ, "const"));
    fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap *soap, %s)", c_ident(typ), c_type_id(typ, "const a"));
    fprintf(fout,"\n{\tsoap_mark_%s(soap, a);\n}",c_ident(typ));
    fflush(fout);
    return;   
  }
  if (is_dynamic_array(typ))
    if (typ->type == Tclass)
    { fprintf(fout,"\n\nvoid %s::soap_serialize(struct soap *soap) const\n{",c_ident(typ));
      fprintf(fout,"\n\tthis->soap_mark(soap);\n}");
      fflush(fout);
      return;
    }
    else
    { fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "const*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap *soap, %s)\n{",c_ident(typ),c_type_id(typ, "const*a")); 
      fprintf(fout,"\n\tsoap_mark_%s(soap, a);\n}", c_ident(typ));
      fflush(fout);
      return;
    }
  if(typ->type==Tclass && !is_external(typ) && !is_volatile(typ)){
    fprintf(fout,"\n\nvoid %s::soap_serialize(struct soap *soap) const\n{", typ->id->name);
    fprintf(fout,"\n\tthis->soap_mark(soap);\n}",c_ident(typ));
      fflush(fout);
    return;
  }

  fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "const*"));
  fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_serialize_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "const*a"));
  if (!is_primitive(typ) && !is_stdstring(typ) && !is_stdwstring(typ))
    fprintf(fout,"\n\tsoap_mark_%s(soap, a);\n}",c_ident(typ));
  else
    fprintf(fout,"\n}");
  fflush(fout);
}

int
get_dimension(Tnode *typ)
{ if (((Tnode*)typ->ref)->width)
    return typ->width / ((Tnode*) typ->ref)->width;
  return 0;
}


void
mark(Tnode *typ)
{ int d;
  Table *table,*t;
  Entry *p;
  Tnode* temp;
  int cardinality;

    if ((p = is_dynamic_array(typ)))
    { if (typ->type == Tclass && !is_volatile(typ))
      { if (is_external(typ))
          return;
        fprintf(fout,"\n\nvoid %s::soap_mark(struct soap *soap) const\n{",c_ident(typ));
        if (has_setter(typ))
          fprintf(fout, "\n\t((%s)this)->set(soap);", c_type_id(typ, "*"));
        if (is_binary(typ))
	{ if (is_attachment(typ))
          { fprintf(fout,"\n\tif (this->__ptr && !soap_array_reference(soap, this, (struct soap_array*)&this->__ptr, 1, SOAP_TYPE_%s))", c_ident(typ));
            fprintf(fout,"\n\t\tif (this->id || this->type)\n\t\tsoap->mode |= SOAP_ENC_DIME;\n}");
	  }
          else
            fprintf(fout,"\n\tif (this->__ptr)\n\t\tsoap_array_reference(soap, this, (struct soap_array*)&this->%s, 1, SOAP_TYPE_%s);\n}", p->sym->name, c_ident(typ));
      fflush(fout);
      return;
	}
	else
	{
      if (is_XML(p->info.typ->ref))
      { fprintf(fout,"\n}");
        return;
      }
      d = get_Darraydims(typ);
      if (d)
      { fprintf(fout,"\n\tif (this->%s && !soap_array_reference(soap, this, (struct soap_array*)&this->%s, %d, SOAP_TYPE_%s))", p->sym->name, p->sym->name, d, c_ident(typ));
        fprintf(fout,"\n\t\tfor (int i = 0; i < soap_size(this->__size, %d); i++)", d);
      }
      else
      { fprintf(fout,"\n\tif (this->%s && !soap_array_reference(soap, this, (struct soap_array*)&this->%s, 1, SOAP_TYPE_%s))", p->sym->name, p->sym->name, c_ident(typ));
        fprintf(fout,"\n\t\tfor (int i = 0; i < this->__size; i++)");
      }
      fprintf(fout,"\n\t\t{\tsoap_embedded(soap, this->%s + i, SOAP_TYPE_%s);", p->sym->name, c_ident(p->info.typ->ref));
      if (((Tnode*)p->info.typ->ref)->type == Tclass && !is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
        fprintf(fout,"\n\t\t\tthis->%s[i].soap_mark(soap);", p->sym->name);
      else if (!is_primitive(p->info.typ->ref))
        fprintf(fout,"\n\t\t\tsoap_mark_%s(soap, this->%s + i);", c_ident(p->info.typ->ref), p->sym->name);
      fprintf(fout,"\n\t\t}\n}");
      return;
      }
      }
      else
      { if (is_external(typ))
        { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "const*")); 
          return;
	}
        fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "const*")); 
        fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, %s)\n{",c_ident(typ),c_type_id(typ, "const*a")); 
        if (is_binary(typ))
	{ if (is_attachment(typ))
          { fprintf(fout,"\n\tif (a->__ptr && !soap_array_reference(soap, a, (struct soap_array*)&a->__ptr, 1, SOAP_TYPE_%s))", c_ident(typ));
            fprintf(fout,"\n\t\tif (a->id || a->type)\n\t\t\tsoap->mode |= SOAP_ENC_DIME;\n}");
	  }
          else
            fprintf(fout,"\n\tif (a->__ptr)\n\t\tsoap_array_reference(soap, a, (struct soap_array*)&a->%s, 1, SOAP_TYPE_%s);\n}", p->sym->name, c_ident(typ));
      fflush(fout);
      return;
	}
	else
	{
      if (is_XML(p->info.typ->ref))
      { fprintf(fout,"\n}");
        return;
      }
      fprintf(fout,"\n\tint i;");
      d = get_Darraydims(typ);
      if (d)
      { fprintf(fout,"\n\tif (a->%s && !soap_array_reference(soap, a, (struct soap_array*)&a->%s, %d, SOAP_TYPE_%s))", p->sym->name, p->sym->name, d, c_ident(typ));
        fprintf(fout,"\n\t\tfor (i = 0; i < soap_size(a->__size, %d); i++)", d);
      }
      else
      { fprintf(fout,"\n\tif (a->%s && !soap_array_reference(soap, a, (struct soap_array*)&a->%s, 1, SOAP_TYPE_%s))", p->sym->name, p->sym->name, c_ident(typ));
        fprintf(fout,"\n\t\tfor (i = 0; i < a->__size; i++)");
      }
      fprintf(fout,"\n\t\t{\tsoap_embedded(soap, a->%s + i, SOAP_TYPE_%s);", p->sym->name, c_ident(p->info.typ->ref));
      if (((Tnode*)p->info.typ->ref)->type == Tclass && !is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
        fprintf(fout,"\n\t\t\ta->%s[i].soap_mark(soap);", p->sym->name);
      else if (!is_primitive(p->info.typ->ref))
        fprintf(fout,"\n\t\t\tsoap_mark_%s(soap, a->%s + i);", c_ident(p->info.typ->ref), p->sym->name);
      fprintf(fout,"\n\t\t}\n}");
      fflush(fout);
      return;
      }
      }
      }
  if (is_stdstring(typ) || is_stdwstring(typ))
  { fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
    fprintf(fout,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, const %s)\n{ }",c_ident(typ),c_type_id(typ, "*p")); 
    return;
  }
  switch(typ->type)
    {	
    case Tclass:
      if (!is_volatile(typ))
      {
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      table=(Table*)typ->ref;
      fprintf(fout,"\n\nvoid %s::soap_mark(struct soap *soap) const\n{", typ->id->name); 
      if (has_setter(typ))
        fprintf(fout, "\n\t((%s)this)->set(soap);", c_type_id(typ, "*"));
      for (t = table; t != (Table *) 0; t = t->prev) { 
	for (p = t->list; p != (Entry*) 0; p = p->next) {
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_repetition(p))
	  { 
      if (!is_XML(p->next->info.typ)) {
      fprintf(fout,"\n\tif (((%s*)this)->%s)", t->sym->name, p->next->sym->name);
      fprintf(fout,"\n\t{\tint i;\n\t\tfor (i = 0; i < ((%s*)this)->%s; i++)", t->sym->name, p->sym->name);
      fprintf(fout,"\n\t\t{\tsoap_embedded(soap, ((%s*)this)->%s + i, SOAP_TYPE_%s);", t->sym->name, p->next->sym->name, c_ident(p->next->info.typ->ref));
      if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
        fprintf(fout,"\n\t\t\t((%s*)this)->%s[i].soap_mark(soap);", t->sym->name, p->next->sym->name);
      else if (!is_primitive(p->next->info.typ->ref))
        fprintf(fout,"\n\t\t\tsoap_mark_%s(soap, ((%s*)this)->%s + i);", c_ident(p->next->info.typ->ref), t->sym->name, p->next->sym->name);
      fprintf(fout,"\n\t\t}\n\t}");
	  }
          p = p->next;
	  }
	  else if (is_anytype(p))
	  {
	    fprintf(fout,"\n\tsoap_markelement(soap, this->%s, this->%s);", p->next->sym->name, p->sym->name);
            p = p->next;
	  }
	  else if(p->info.typ->type==Tarray)
	    {
	      fprintf(fout,"\n\tsoap_embedded(soap, ((%s*)this)->%s, SOAP_TYPE_%s);", t->sym->name, p->sym->name,c_ident(p->info.typ));
	      fprintf(fout,"\n\tsoap_mark_%s(soap, ((%s*)this)->%s);", c_ident(p->info.typ),t->sym->name, p->sym->name);
	    }
	  else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	    {
	      fprintf(fout,"\n\tsoap_embedded(soap, &((%s*)this)->%s, SOAP_TYPE_%s);", t->sym->name, p->sym->name, c_ident(p->info.typ));
	      fprintf(fout,"\n\t((%s*)this)->%s.soap_mark(soap);", t->sym->name, p->sym->name );
	    }
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_XML(p->info.typ))
	  {
	    if (!is_template(p->info.typ))
	      fprintf(fout,"\n\tsoap_embedded(soap, &((%s*)this)->%s, SOAP_TYPE_%s);", t->sym->name,p->sym->name,c_ident(p->info.typ));
	    if (!is_primitive(p->info.typ))
	      fprintf(fout,"\n\tsoap_mark_%s(soap, &((%s*)this)->%s);", c_ident(p->info.typ),t->sym->name,p->sym->name);
	  }
	}
      }
      fprintf(fout,"\n}");	 
      break;
      }
    case Tstruct:

      if (is_external(typ) && !is_volatile(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
      if (!typ->ref)
        return;
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, const %s)\n{",c_ident(typ),c_type_id(typ, "*a")); 
      /* DYNAMIC ARRAY */
      
      table=(Table*)typ->ref;
      for (t = table; t != (Table *) 0; t = t->prev) { 
	for (p = t->list; p != (Entry*) 0; p = p->next) {
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_repetition(p))
	  { 
      if (!is_XML(p->next->info.typ)) {
      fprintf(fout,"\n\tif (a->%s)", p->next->sym->name);
      fprintf(fout,"\n\t{\tint i;\n\t\tfor (i = 0; i < a->%s; i++)", p->sym->name);
      fprintf(fout,"\n\t\t{\tsoap_embedded(soap, a->%s + i, SOAP_TYPE_%s);", p->next->sym->name, c_ident(p->next->info.typ->ref));
      if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
        fprintf(fout,"\n\t\t\ta->%s[i].soap_mark(soap);", p->next->sym->name);
      else if (!is_primitive(p->next->info.typ->ref))
        fprintf(fout,"\n\t\t\tsoap_mark_%s(soap, a->%s + i);", c_ident(p->next->info.typ->ref), p->next->sym->name);
      fprintf(fout,"\n\t\t}\n\t}");
	  }
          p = p->next;
	  }
	  else if (is_anytype(p))
	  {
	    fprintf(fout,"\n\tsoap_markelement(soap, a->%s, a->%s);", p->next->sym->name, p->sym->name);
            p = p->next;
	  }
	  else if(p->info.typ->type==Tarray)
	    {
	      fprintf(fout,"\n\tsoap_embedded(soap, a->%s, SOAP_TYPE_%s);", p->sym->name,c_ident(p->info.typ));
	      fprintf(fout,"\n\tsoap_mark_%s(soap, a->%s);", c_ident(p->info.typ),p->sym->name);
	    }
	  else if(p->info.typ->type == Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	    {
	      /* CLASS ( within STRUCT ) */
	      fprintf(fout,"\n\tsoap_embedded(soap, &a->%s, SOAP_TYPE_%s);", p->sym->name,c_ident(p->info.typ));
	      fprintf(fout,"\n\ta->%s.soap_mark(soap);",p->sym->name);
	    }
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_XML(p->info.typ))
	    {
	      if (!is_template(p->info.typ))
	        fprintf(fout,"\n\tsoap_embedded(soap, &a->%s, SOAP_TYPE_%s);", p->sym->name,c_ident(p->info.typ));
	      if (!is_primitive(p->info.typ))
	        fprintf(fout,"\n\tsoap_mark_%s(soap, &a->%s);", c_ident(p->info.typ),p->sym->name);
	    }
	}
      }
      fprintf(fout,"\n}");	 
      break;
      
    case Tpointer:
      if (((Tnode*)typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
      { if (is_external(typ))
        { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const*"));
          return;
	}
        fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const*"));
	fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "const*a"));
	if (p = is_dynamic_array(typ->ref))
        { d = get_Darraydims(typ->ref);
          if (d)
	    fprintf(fout,"\n\tif (*a && !soap_array_reference(soap, *a, (struct soap_array*)&(*a)->%s, %d, SOAP_TYPE_%s))", p->sym->name, d, c_ident(typ->ref));
	  else
	    fprintf(fout,"\n\tif (*a && !soap_array_reference(soap, *a, (struct soap_array*)&(*a)->%s, 1, SOAP_TYPE_%s))", p->sym->name, c_ident(typ->ref));
	}
	else
	  fprintf(fout,"\n\tif (!soap_reference(soap, *a, SOAP_TYPE_%s))", c_ident(typ->ref));
	fprintf(fout,"\n\t\t(*a)->soap_mark(soap);\n}");
	break;	
      }
      else
      {
        if (is_external(typ))
	{ fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const*"));
          return;
	}
	fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const*"));
	fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "const*a"));
	if (is_string(typ) || is_wstring(typ))
	  fprintf(fout,"\n\tsoap_reference(soap, *a, SOAP_TYPE_%s);\n}", c_ident(typ));
	else if (is_primitive(typ->ref))
	  fprintf(fout,"\n\tsoap_reference(soap, *a, SOAP_TYPE_%s);\n}", c_ident(typ->ref));
	else if (p = is_dynamic_array(typ->ref))
        { d = get_Darraydims(typ->ref);
          if (d)
	    fprintf(fout,"\n\tif (*a && !soap_array_reference(soap, *a, (struct soap_array*)&(*a)->%s, %d, SOAP_TYPE_%s))", p->sym->name, d, c_ident(typ->ref));
	  else
	    fprintf(fout,"\n\tif (*a && !soap_array_reference(soap, *a, (struct soap_array*)&(*a)->%s, 1, SOAP_TYPE_%s))", p->sym->name, c_ident(typ->ref));
	  fprintf(fout,"\n\t\tsoap_mark_%s(soap, *a);\n}", c_ident(typ->ref));
	}
	else
	{ fprintf(fout,"\n\tif (!soap_reference(soap, *a, SOAP_TYPE_%s))", c_ident(typ->ref));
	  fprintf(fout,"\n\t\tsoap_mark_%s(soap, *a);\n}", c_ident(typ->ref));
	}
	break;
      }
	
    case Tarray :
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, %s);", c_ident(typ),c_type_id(typ, "const"));
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, %s)", c_ident(typ),c_type_id(typ, "const a"));
      if (is_primitive(typ->ref))
        fprintf(fout, "\n{");
      else
      { fprintf(fout,"\n{\tint i;");
        fprintf(fout,"\n\tfor(i = 0; i < %d; i++)", get_dimension(typ));
	
        temp=typ->ref;;
        cardinality = 1;
        while(temp->type==Tarray)
	{
	  temp=temp->ref;
	  cardinality++;
	}
        fprintf(fout,"\n\t{\tsoap_embedded(soap, a", c_ident(typ->ref));
        if(cardinality > 1){
	  fprintf(fout,"[i]");
        }else {
	  fprintf(fout,"+i");
        }
      
        fprintf(fout,", SOAP_TYPE_%s);",c_ident(typ->ref));

	if (((Tnode *)typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
      	{	fprintf(fout,"\n\ta[i].soap_mark(soap)");
	}
	else if (!is_primitive(typ->ref))
      	{	fprintf(fout,"\n\tsoap_mark_%s(soap, a",c_ident(typ->ref));
      		if(cardinality > 1){
		fprintf(fout,"[i])");
      		}else {
	  	fprintf(fout,"+i)");
      		}
	}
        fprintf(fout,";\n\t}");
      }
      fprintf(fout,"\n}");
      break;	
    case Ttemplate:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap*, const %s);",c_ident(typ),c_type_id(typ, "*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_mark_%s(struct soap *soap, const %s)\n{",c_ident(typ),c_type_id(typ, "*a")); 
      temp = typ->ref;
      if (!is_primitive(temp))
      { fprintf(fout, "\n\tfor (%s::const_iterator i = a->begin(); i != a->end(); ++i)", c_type(typ));
        if (temp->type==Tclass && !is_external(temp) && !is_volatile(temp))
	  fprintf(fout,"\n\t\t(*i).soap_mark(soap);");
        else if (temp->type != Tfun && !is_void(temp) && !is_XML(temp))
          fprintf(fout,"\n\t\tsoap_mark_%s(soap, &(*i));", c_ident(temp));
      }
      fprintf(fout, "\n}");
    default:     break;
    }
}

void
defaults(typ)
Tnode* typ;
{ int i, d;
  Table *table,*t;
  Entry *p;
  Tnode *temp;
  char *s;
  int cardinality;
  if (typ->type == Tpointer)
    return;
  if (p = is_dynamic_array(typ))
  { if (typ->type == Tclass && !is_volatile(typ))
    { if (is_external(typ))
        return;
        fprintf(fout,"\n\nvoid %s::soap_default(struct soap *soap)\n{", c_ident(typ)); 
        if ((s = has_soapref(typ)))
          fprintf(fout,"\n\tthis->%s = soap;", s);
	d = get_Darraydims(typ);
        if (d)
	{ fprintf(fout,"\n\tthis->%s = NULL;", p->sym->name);
	  for (i = 0; i < d; i++)
	  { fprintf(fout,"\n\tthis->__size[%d] = 0;", i);
            if (has_offset(typ) && (((Table*)typ->ref)->list->next->next->info.sto & Sconst) == 0)
              fprintf(fout, "\n\tthis->__offset[%d] = 0;", i);
	  }
	}
	else
	{ fprintf(fout,"\n\tthis->__size = 0;\n\tthis->%s = NULL;", p->sym->name);
          if (has_offset(typ) && (((Table*)typ->ref)->list->next->next->info.sto & Sconst) == 0)
            fprintf(fout, "\n\tthis->__offset = 0;");
	}
	if (is_attachment(typ))
          fprintf(fout,"\n\tthis->id = NULL;\n\tthis->type = NULL;\n\tthis->options = NULL;");
        fprintf(fout,"\n}");
      }
      else
      { if (is_external(typ))
        { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
          return;
	}
        fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
        fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "*a")); 
	d = get_Darraydims(typ);
        if (d)
	{ fprintf(fout,"\n\ta->%s = NULL;", p->sym->name);
	  for (i = 0; i < d; i++)
	  { fprintf(fout,"\n\ta->__size[%d] = 0;", i);
            if (has_offset(typ) && (((Table*)typ->ref)->list->next->next->info.sto & Sconst) == 0)
              fprintf(fout, "\n\ta->__offset[%d] = 0;", i);
	  }
	}
	else
	{ fprintf(fout,"\n\ta->__size = 0;\n\ta->%s = NULL;", p->sym->name);
          if (has_offset(typ) && (((Table*)typ->ref)->list->next->next->info.sto & Sconst) == 0)
            fprintf(fout, "\n\ta->__offset = 0;");
	}
	if (is_attachment(typ))
          fprintf(fout,"\n\ta->id = NULL;\n\ta->type = NULL;\n\ta->options = NULL;");
        fprintf(fout,"\n}");
      }
      fflush(fout);
      return;
  }
  if (is_primitive(typ))
  {   if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{\n#ifdef SOAP_DEFAULT_%s\n\t*a = SOAP_DEFAULT_%s;\n#else\n\t*a = (%s)0;\n#endif\n}",c_ident(typ),c_type_id(typ, "*a"), c_ident(typ), c_ident(typ), c_type(typ));
      return;
  }
  if (is_stdstring(typ) || is_stdwstring(typ))
  { fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
    fprintf(fout,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{\tp->erase();\n}",c_ident(typ),c_type_id(typ, "*p")); 
    return;
  }
  switch(typ->type)
    {
    case Tclass:
      /* CLASS */
      if (!is_volatile(typ))
      {
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      table=(Table*)typ->ref;
      fprintf(fout,"\n\nvoid %s::soap_default(struct soap *soap)\n{", typ->id->name ); 
      /*
      fprintf(fout,"\n\tstatic %s a;\n\tmemcpy(this, &a, sizeof(%s));", typ->id->name, typ->id->name);
      */
      if ((s = has_soapref(typ)))
        fprintf(fout,"\n\tthis->%s = soap;", s);
	
      /*      fprintf(fhead,"\nvirtual void %s::soap_default();",
	     ((Table *) typ->ref)->sym->name ); */
      fflush(fout);
      for (t = table; t != (Table *) 0; t = t->prev) { 
	if(t==(Table *) 0 ) return;
	for (p = t->list; p != (Entry*) 0; p = p->next)
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (is_repetition(p) || is_anytype(p))
	  { fprintf(fout, "\n\t((%s*)this)->%s = 0;\n\t((%s*)this)->%s = NULL;", t->sym->name, p->sym->name, t->sym->name, p->next->sym->name);
	    p = p->next;
	  }
	  else
	  {
	  if(p->info.typ->type==Tarray){
	    fprintf(fout,"\n\tsoap_default_%s(soap, ((%s*)this)->%s);", c_ident(p->info.typ),t->sym->name,p->sym->name);
	  }
	  else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	    fprintf(fout,"\n\t((%s*)this)->%s.%s::soap_default(soap);",t->sym->name, p->sym->name, c_ident(p->info.typ));
	  else if (p->info.hasval)
	  { if (p->info.typ->type == Tpointer && is_stdstring(p->info.typ->ref))
	      fprintf(fout,"\n\tstatic std::string s(\"%s\");\n\t((%s*)this)->%s = &s;", p->info.val.s, t->sym->name, p->sym->name);
	    else
	      fprintf(fout,"\n\t((%s*)this)->%s%s;", t->sym->name,p->sym->name,c_init(p));
	  }
	  else if (p->info.typ->type == Tpointer)
	    fprintf(fout,"\n\t((%s*)this)->%s = NULL;", t->sym->name,p->sym->name);
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	    fprintf(fout,"\n\tsoap_default_%s(soap, &((%s*)this)->%s);", c_ident(p->info.typ),t->sym->name,p->sym->name);
	  }
	}
      }
      fprintf(fout,"\n}");	 
      fflush(fout);
      break;
      }
      
    case Tstruct:
      table=(Table*)typ->ref;

      if (is_external(typ) && !is_volatile(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "*a")); 
      fflush(fout);
      /* DYNAMIC ARRAY */

      for (t = table; t != (Table *) 0; t = t->prev) { 
	if(t==(Table *) 0 ) return;
	for (p = t->list; p != (Entry*) 0; p = p->next)
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (is_repetition(p) || is_anytype(p))
	  { fprintf(fout, "\n\ta->%s = 0;\n\ta->%s = NULL;", p->sym->name, p->next->sym->name);
	    p = p->next;
	  }
	  else
	  {
	  if(p->info.typ->type==Tarray)
	    fprintf(fout,"\n\tsoap_default_%s(soap, a->%s);",
		    c_ident(p->info.typ),p->sym->name);
	  else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)){
	    /* CLASS (within STRUCT) */
	    fprintf(fout,"\n\ta->%s.%s::soap_default(soap);",p->sym->name, c_ident(p->info.typ));
	  }
	  else if (p->info.hasval)
	  { if (p->info.typ->type == Tpointer && is_stdstring(p->info.typ->ref))
	      fprintf(fout,"\n\tstatic std::string s(\"%s\");\n\ta->%s = &s;", p->info.val.s, p->sym->name);
	    else
	      fprintf(fout,"\n\ta->%s%s;", p->sym->name,c_init(p));
	  }
	  else if (p->info.typ->type == Tpointer)
	    fprintf(fout,"\n\ta->%s = NULL;", p->sym->name);
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ))
	    fprintf(fout,"\n\tsoap_default_%s(soap, &a->%s);", c_ident(p->info.typ),p->sym->name);
	}
      }
      fprintf(fout,"\n}");	 
      fflush(fout);
      break;
      
    case Tarray:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type(typ));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type(typ));
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{", c_ident(typ),c_type_id(typ, "a"));
      fprintf(fout,"\n\tint i;");
      fprintf(fout,"\n\tfor (i = 0; i < %d; i++)",get_dimension(typ));
      temp = typ->ref;
      cardinality = 1;
      while(temp->type==Tarray)
	{
	  temp=temp->ref;
	  cardinality++;
	}
	if (((Tnode *)typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
	{
      	if(cardinality>1){
		fprintf(fout,"a[i].%s::soap_default(soap)", c_ident(typ->ref));
     	 }else {
		fprintf(fout,"(a+i)->soap_default(soap)");
      		}
	}
      else if (((Tnode*)typ->ref)->type == Tpointer)
      	fprintf(fout,"\n\ta[i] = NULL");
      else
      {
      	fprintf(fout,"\n\tsoap_default_%s(soap, a",c_ident(typ->ref));
      	if (cardinality>1)
		fprintf(fout,"[i])");
     	 else
		fprintf(fout,"+i)");
      }
      fprintf(fout,";\n}");
      break;	
      
    case Ttemplate:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 void SOAP_FMAC2 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap*, %s);",c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 void SOAP_FMAC4 soap_default_%s(struct soap *soap, %s)\n{",c_ident(typ),c_type_id(typ, "*p"));
      fprintf(fout,"\n\tp->clear();");
      fprintf(fout,"\n}");
      fflush(fout);
      break;
    default    :break;
    }
  
}

void
soap_put(Tnode *typ)
{ int d;
  Entry *p;
  char *ci = c_ident(typ);
  char *ct = c_type(typ);
  char *cta = c_type_id(typ, "a");
  char *ctp = c_type_id(typ, "*");
  char *ctpa = c_type_id(typ, "*a");
  char *ctc = c_type_id(typ, "const");
  char *ctca = c_type_id(typ, "const a");
  char *ctcp = c_type_id(typ, "const*");
  char *ctcpa = c_type_id(typ, "const*a");
  char *xt = xsi_type(typ);
  if (typ->type == Tarray)
  { fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap*, %s, const char*, const char*);", ci,ctc);
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap *soap, %s, const char *tag, const char *type)\n{", ci,ctca);
  }
  else if (typ->type == Tclass && !is_external(typ) && !is_volatile(typ))
    fprintf(fout,"\n\nint %s::soap_put(struct soap *soap, const char *tag, const  char *type) const\n{", ct);
  else if (typ->type == Tpointer)
  { fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap*, %s, const char*, const char*);", ci,ctcp);
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap *soap, %s, const char *tag, const char *type)\n{", ci,ctcpa);
  }
  else
  { fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap*, const %s, const char*, const char*);", ci,ctp);
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_put_%s(struct soap *soap, const %s, const char *tag, const char *type)\n{", ci,ctpa);
  }
  fflush(fout);
  fprintf(fout,"\n\tregister int id = ");
  if (p = is_dynamic_array(typ))
  { d = get_Darraydims(typ);
    if (typ->type == Tclass)
      if (d)
        fprintf(fout,"soap_embed(soap, (void*)this, (struct soap_array*)&this->%s, %d, tag, SOAP_TYPE_%s);", p->sym->name, d, ci);
      else
        fprintf(fout,"soap_embed(soap, (void*)this, (struct soap_array*)&this->%s, 1, tag, SOAP_TYPE_%s);", p->sym->name, ci);
    else if (d)
      fprintf(fout,"soap_embed(soap, (void*)a, (struct soap_array*)&a->%s, %d, tag, SOAP_TYPE_%s);", p->sym->name, d, ci);
    else
      fprintf(fout,"soap_embed(soap, (void*)a, (struct soap_array*)&a->%s, 1, tag, SOAP_TYPE_%s);", p->sym->name, ci);
  }
  else if (typ->type == Tclass && !is_external(typ) && !is_volatile(typ))
    fprintf(fout,"soap_embed(soap, (void*)this, NULL, 0, tag, SOAP_TYPE_%s);", ci);
  else
    fprintf(fout,"soap_embed(soap, (void*)a, NULL, 0, tag, SOAP_TYPE_%s);", ci);
  if (typ->type == Tclass && !is_external(typ) && !is_volatile(typ))
    fprintf(fout,"\n\tif (this->soap_out(soap, tag, id, type))\n\t\treturn soap->error;");
  else
    fprintf(fout,"\n\tif (soap_out_%s(soap, tag, id, a, type))\n\t\treturn soap->error;", ci);
  fprintf(fout,"\n\treturn soap_putindependent(soap);\n}");
  fflush(fout);
}

Entry *
is_dynamic_array(Tnode *typ)
{ Entry *p;
  Table *t;
  if ((typ->type == Tstruct || typ->type == Tclass) && typ->ref)
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { p = t->list;
      if (p && p->info.typ->type == Tpointer && !strncmp(p->sym->name, "__ptr", 5))
        if (p->next && (p->next->info.typ->type == Tint || p->next->info.typ->type == Tulong || p->next->info.typ->type == Tarray && (((Tnode*)p->next->info.typ->ref)->type == Tint || ((Tnode*)p->next->info.typ->ref)->type == Tuint)) && !strcmp(p->next->sym->name, "__size"))
	  return p;
    }
  }
  return 0;
}

Entry *
is_discriminant(Tnode *typ)
{ Entry *p;
  Table *t;
  if ((typ->type == Tstruct || typ->type == Tclass) && typ->ref)
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { p = t->list;
      if (p && p->info.typ->type == Tint && !strncmp(p->sym->name, "__type", 6))
        if (p->next && p->next->info.typ->type == Tunion)
	  return p;
    }
  }
  return 0;
}

int
get_Darraydims(Tnode *typ)
{ Entry *p;
  Table *t;
  if ((typ->type == Tstruct || typ->type == Tclass) && typ->ref)
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { p = t->list;
      if (p && p->info.typ->type == Tpointer && !strncmp(p->sym->name, "__ptr", 5))
        if (p->next && p->next->info.typ->type == Tarray && (((Tnode*)p->next->info.typ->ref)->type == Tint || ((Tnode*)p->next->info.typ->ref)->type == Tuint) && !strcmp(p->next->sym->name, "__size"))
          return get_dimension(p->next->info.typ);
    }
  }
  return 0;
}

int
has_offset(Tnode *typ)
{ Entry *p;
  Table *t;
  if (typ->type == Tstruct || typ->type == Tclass)
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { for (p = t->list; p; p = p->next)
      { if ((p->info.typ->type == Tint || p->info.typ->type == Tarray && ((Tnode*)p->info.typ->ref)->type == Tint) && !strcmp(p->sym->name, "__offset"))
          return 1;
      }
    }
  }
  return 0;
}

int
is_hexBinary(Tnode *typ)
{ Entry *p;
  Table *t;
  int n = strlen(typ->id->name);
  if ((typ->type == Tstruct || typ->type == Tclass) && n >= 9 && is_eq(typ->id->name + n - 9, "hexBinary"))
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { p = t->list;
      if (p && p->info.typ->type == Tpointer && ((Tnode*)p->info.typ->ref)->type == Tuchar && !strcmp(p->sym->name, "__ptr"))
      { p = p->next;
        return p && (p->info.typ->type == Tint || p->info.typ->type == Tuint) && !strcmp(p->sym->name, "__size");
      }
    }
  }
  return 0;
}

int
is_binary(Tnode *typ)
{ Entry *p;
  Table *t;
  if (!has_ns(typ) && !is_element(typ))
    return 0;
  if (typ->type == Tstruct || typ->type == Tclass) 
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { p = t->list;
      if (p && p->info.typ->type == Tpointer && ((Tnode*)p->info.typ->ref)->type == Tuchar && !strcmp(p->sym->name, "__ptr"))
      { p = p->next;
        return p && (p->info.typ->type == Tint || p->info.typ->type == Tuint) && !strcmp(p->sym->name, "__size");
      }
    }
  }
  return 0;
}

is_attachment(Tnode *typ)
{ Entry *p;
  Table *t;
  if (!is_binary(typ) || is_transient(typ))
    return 0;
  for (t = (Table*)typ->ref; t; t = t->prev)
  { for (p = t->list; p; p = p->next)
    { if (is_string(p->info.typ) && !strcmp(p->sym->name, "id"))
      { p = p->next;
        if (!p || !is_string(p->info.typ) || strcmp(p->sym->name, "type"))
          break;
        p = p->next;
        if (!p || !is_string(p->info.typ) || strcmp(p->sym->name, "options"))
          break;
        return 1;
      }
    }
  }
  return 0;
}

int
is_header_or_fault(Tnode *typ)
{ if (typ->type == Tpointer || typ->type == Treference)
    return is_header_or_fault(typ->ref);
  return (typ->type == Tstruct || typ->type == Tclass) && (!strcmp(typ->id->name, "SOAP_ENV__Header") || !strcmp(typ->id->name, "SOAP_ENV__Fault") || !strcmp(typ->id->name, "SOAP_ENV__Code") || !strcmp(typ->id->name, "SOAP_ENV__Detail"));
}

int
is_body(Tnode *typ)
{ if (typ->type == Tpointer || typ->type == Treference)
    return is_body(typ->ref);
  return (typ->type == Tstruct || typ->type == Tclass) && !strcmp(typ->id->name, "SOAP_ENV__Body");
}

int
is_document(const char *style)
{ return !eflag && !style || style && !strcmp(style, "document");
}

int
is_literal(const char *encoding)
{ return !eflag && !encoding || encoding && !strcmp(encoding, "literal");
}

char *
has_soapref(Tnode *typ)
{ Entry *p;
  Table *t;
  if (typ->type == Tstruct || typ->type == Tclass) 
  { for (t = (Table*)typ->ref; t; t = t->prev)
    { for (p = t->list; p; p = p->next)
        if (p->info.typ->type == Tpointer && ((Tnode*)p->info.typ->ref)->type == Tstruct && ((Tnode*)p->info.typ->ref)->id == lookup("soap"))
          return p->sym->name;
    }
  }
  return NULL;
}

int
has_getter(Tnode *typ)
{ Entry *p, *q;
  Table *t;
  if (typ->type == Tclass) 
    for (t = (Table*)typ->ref; t; t = t->prev)
      for (p = t->list; p; p = p->next)
        if (p->info.typ->type == Tfun && !strcmp(p->sym->name, "get") && ((FNinfo *)p->info.typ->ref)->ret->type == Tint)
	{ q = ((FNinfo*)p->info.typ->ref)->args->list;
          if (q && q->info.typ->type == Tpointer && ((Tnode*)q->info.typ->ref)->type == Tstruct && ((Tnode*)q->info.typ->ref)->id == lookup("soap"))
	    return 1;
        }
  return 0;
}

int
has_setter(Tnode *typ)
{ Entry *p, *q;
  Table *t;
  if (typ->type == Tclass) 
    for (t = (Table*)typ->ref; t; t = t->prev)
      for (p = t->list; p; p = p->next)
        if (p->info.typ->type == Tfun && !strcmp(p->sym->name, "set") && ((FNinfo *)p->info.typ->ref)->ret->type == Tint)
	{ q = ((FNinfo*)p->info.typ->ref)->args->list;
          if (q && q->info.typ->type == Tpointer && ((Tnode*)q->info.typ->ref)->type == Tstruct && ((Tnode*)q->info.typ->ref)->id == lookup("soap"))
	    return 1;
        }
  return 0;
}

int
is_primitive_or_string(Tnode *typ)
{ return is_primitive(typ) || is_string(typ) || is_wstring(typ) || is_stdstring(typ) || is_stdwstring(typ); 
}

int
is_primitive(Tnode *typ)
{ return typ->type <= Ttime; 
}

int
is_string(Tnode *typ)
{ return typ->type == Tpointer && ((Tnode*)typ->ref)->type == Tchar;
}

int
is_wstring(Tnode *typ)
{ return typ->type == Tpointer && ((Tnode*)typ->ref)->type == Twchar;
}

int
is_stdstring(Tnode *typ)
{ return typ->type == Tclass && typ->id == lookup("std::string");
}

int
is_stdwstring(Tnode *typ)
{ return typ->type == Tclass && typ->id == lookup("std::wstring");
}

int
is_stdstr(Tnode *typ)
{ if (typ->type == Tpointer)
    return is_stdstring(typ->ref) || is_stdwstring(typ->ref);
  return is_stdstring(typ) || is_stdwstring(typ);
}

int
reflevel(Tnode *typ)
{ int level;
  for (level = 0; typ->type == Tpointer; level++)
    typ = (Tnode*)typ->ref;
  return level;
}

Tnode *
reftype(Tnode *typ)
{ while (typ->type == Tpointer || typ->type == Treference)
    typ = typ->ref;
  return typ;
}

void
soap_set_attr(Tnode *typ, char *obj, char *name, char *tag)
{ if (is_qname(typ))
    fprintf(fout, "\n\tif (%s->%s)\n\t\tsoap_set_attr(soap, \"%s\", soap_QName2s(soap, %s->%s));", obj, name, tag, obj, name);
  else if (is_string(typ))
    fprintf(fout, "\n\tif (%s->%s)\n\t\tsoap_set_attr(soap, \"%s\", %s->%s);", obj, name, tag, obj, name);
  else if (is_stdstring(typ))
    fprintf(fout, "\n\tif (!%s->%s.empty())\n\t\tsoap_set_attr(soap, \"%s\", %s->%s.c_str());", obj, name, tag, obj, name);
  else if (typ->type == Tllong || typ->type == Tullong)
    fprintf(fout, "\n\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, %s->%s));", tag, c_type(typ), obj, name);
  else if (typ->type == Tenum)
    fprintf(fout, "\n\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, %s->%s));", tag, c_ident(typ), obj, name);
  else if (typ->type == Tpointer)
  { Tnode *ptr = typ->ref;
    fprintf(fout, "\n\tif (%s->%s)", obj, name);
    if (ptr->type == Tllong || ptr->type == Tullong)
      fprintf(fout, "\n\t\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, *%s->%s));", tag, c_type(ptr), obj, name);
    else if (ptr->type == Tenum)
      fprintf(fout, "\n\t\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, *%s->%s));", tag, c_ident(ptr), obj, name);
    else if (is_stdstring(ptr))
      fprintf(fout, "\n\t\tsoap_set_attr(soap, \"%s\", %s->%s->c_str());", tag, obj, name);
    else if (is_primitive(ptr))
      fprintf(fout, "\n\t\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, *%s->%s));", tag, the_type(ptr), obj, name);
    else
    { sprintf(errbuf, "Field '%s' cannot be serialized as XML attribute", name);
      semwarn(errbuf);
    }
  }
  else if (is_primitive(typ))
    fprintf(fout, "\n\tsoap_set_attr(soap, \"%s\", soap_%s2s(soap, %s->%s));", tag, the_type(typ), obj, name);
  else
  { sprintf(errbuf, "Field '%s' cannot be serialized as XML attribute", name);
    semwarn(errbuf);
  }
}

void
soap_attr_value(Entry *p, char *obj, char *name, char *tag)
{ int flag = 0;
  Tnode *typ = p->info.typ;
  if (p->info.maxOccurs == 0)
    flag = 2; /* prohibited */
  else if (p->info.minOccurs >= 1 && !p->info.hasval)
    flag = 1; /* required */
  if (typ->type == Tllong || typ->type == Tullong)
    fprintf(fout, "\n\tif (soap_s2%s(soap, soap_attr_value(soap, \"%s\", %d), &%s->%s))\n\t\treturn NULL;", c_type(typ), tag, flag, obj, name);
  else if (typ->type == Tenum)
    fprintf(fout, "\n\tif (soap_s2%s(soap, soap_attr_value(soap, \"%s\", %d), &%s->%s))\n\t\treturn NULL;", c_ident(typ), tag, flag, obj, name);
  else if (is_qname(typ))
    fprintf(fout, "\n\tif (soap_s2QName(soap, soap_attr_value(soap, \"%s\", %d), &%s->%s))\n\t\treturn NULL;", tag, flag, obj, name);
  else if (is_string(typ))
    fprintf(fout, "\n\tif (soap_s2string(soap, soap_attr_value(soap, \"%s\", %d), &%s->%s))\n\t\treturn NULL;", tag, flag, obj, name);
  else if (is_stdstring(typ))
    fprintf(fout, "\n\t{\tconst char *t = soap_attr_value(soap, \"%s\", %d);\n\t\tif (t)\n\t\t{\tchar *s;\n\t\t\tif (soap_s2string(soap, t, &s))\n\t\t\t\treturn NULL;\n\t\t\t%s->%s.assign(s);\n\t\t}", tag, flag, obj, name);
  else if (typ->type == Tpointer)
  { Tnode *ptr = typ->ref;
    fprintf(fout, "\n\t{\tconst char *t = soap_attr_value(soap, \"%s\", %d);\n\t\tif (t)", tag, flag);
    fprintf(fout, "\n\t\t{\tif (!(%s->%s = (%s)soap_malloc(soap, sizeof(%s))))\n\t\t\t{\tsoap->error = SOAP_EOM;\n\t\t\t\treturn NULL;\n\t\t\t}", obj, name, c_type(typ), c_type(ptr));
    if (ptr->type == Tllong || ptr->type == Tullong)
      fprintf(fout, "\n\tif (soap_s2%s(soap, t, %s->%s))\n\t\treturn NULL;", c_type(ptr), obj, name);
    else if (ptr->type == Tenum)
      fprintf(fout, "\n\tif (soap_s2%s(soap, t, %s->%s))\n\t\treturn NULL;", c_ident(ptr), obj, name);
    else if (is_stdstring(ptr))
      fprintf(fout, "\n\tchar *s;\n\t\tif (soap_s2string(soap, t, &s))\n\t\t\treturn NULL;\n\t\tif (s)\n\t\t{\t%s->%s = soap_new_std__string(soap, -1);\n\t\t\t%s->%s->assign(s);\n\t\t}", obj, name, obj, name);
    else
      fprintf(fout, "\n\tif (soap_s2%s(soap, t, %s->%s))\n\t\treturn NULL;", the_type(ptr), obj, name);
    fprintf(fout, "\n\t\t}");
  }
  else
    fprintf(fout, "\n\tif (soap_s2%s(soap, soap_attr_value(soap, \"%s\", %d), &%s->%s))\n\t\treturn NULL;", the_type(typ), tag, flag, obj, name);
  if (!is_string(typ) && (typ->type == Tpointer || is_stdstring(typ)))
    fprintf(fout, "\n\t}");
}

char *
ptr_cast(Tnode *typ, char *name)
{ char *s = c_type_id(typ, "*");
  char *t = emalloc(strlen(s) + strlen(name) + 6);
  sprintf(t, "((%s)%s)", s, name);
  return t;
}

void
soap_out(Tnode *typ)
{ Table *table,*t;
  Entry *p;
  int cardinality,i,j,d;
  Tnode *n;
  char *nse = ns_qualifiedElement(typ);
  char *nsa = ns_qualifiedAttribute(typ);
  if (is_dynamic_array(typ))
  { soap_out_Darray(typ);
    return;
  }
  if (is_primitive(typ) && typ->type != Tenum)
  { if (is_external(typ))
    { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
      return;
    }
    fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "*a")); 
    if (typ->type == Tllong || typ->type == Tullong)
      fprintf(fout,"\n\treturn soap_out%s(soap, tag, id, a, type, SOAP_TYPE_%s);\n}", c_type(typ),c_ident(typ)); 
    else
      fprintf(fout,"\n\treturn soap_out%s(soap, tag, id, a, type, SOAP_TYPE_%s);\n}", the_type(typ),c_ident(typ)); 
    return;
  }
  if (is_string(typ))
  { if (is_external(typ))
    { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, char*const*, const char*);", c_ident(typ));
      return;
    }
    fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, char*const*, const char*);", c_ident(typ));
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, char *const*a, const char *type)\n{", c_ident(typ));
    fprintf(fout,"\n\treturn soap_outstring(soap, tag, id, a, type, SOAP_TYPE_%s);\n}", c_ident(typ)); 
    return;
  }
  if (is_wstring(typ))
  { if (is_external(typ))
    { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, wchar_t*const*, const char*);", c_ident(typ));
        return;
    }
    fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, wchar_t*const*, const char*);", c_ident(typ));
    fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, wchar_t *const*a, const char *type)\n{", c_ident(typ));
    fprintf(fout,"\n\treturn soap_outwstring(soap, tag, id, a, type, SOAP_TYPE_%s);\n}", c_ident(typ)); 
    return;
  }
  if (is_stdstring(typ))
  { fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const std::string*, const char*);", c_ident(typ)); 
    fprintf(fout,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const std::string *s, const char *type)\n{\n\tif (soap_element_begin_out(soap, tag, soap_embedded_id(soap, id, s, SOAP_TYPE_%s), type) || soap_string_out(soap, s->c_str(), 0) || soap_element_end_out(soap, tag))\n\t\treturn soap->error;\n\treturn SOAP_OK;\n}", c_ident(typ), c_ident(typ));
    return;
  }
  if (is_stdwstring(typ))
  { fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const std::wstring*, const char*);", c_ident(typ)); 
    fprintf(fout,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const std::wstring *s, const char *type)\n{\n\tif (soap_element_begin_out(soap, tag, soap_embedded_id(soap, id, s, SOAP_TYPE_%s), type) || soap_wstring_out(soap, s->c_str(), 0) || soap_element_end_out(soap, tag))\n\t\treturn soap->error;\n\treturn SOAP_OK;\n}", c_ident(typ), c_ident(typ));
    return;
  }
  switch(typ->type)
  { case Tstruct:
      table=(Table*)typ->ref;
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "*a")); 
      for (t = table; t; t = t->prev)
      {	for (p = t->list; p; p = p->next)
	{ if (p->info.sto & Sattribute)
	    soap_set_attr(p->info.typ, "a", p->sym->name, ns_add(p->sym->name, nsa));
	  else if (is_qname(p->info.typ))
            fprintf(fout,"\n\tconst char *soap_tmp_%s = soap_QName2s(soap, a->%s);", p->sym->name, p->sym->name);
        }
      }
       if (is_primclass(typ))
       {
	for (table = (Table*)typ->ref; table; table = table->prev)
	{ p = table->list;
	  if (p && is_item(p))
	    break;
        }
	  if ((p->info.sto & SmustUnderstand) && !(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && !is_transient(p->info.typ) && !is_void(p->info.typ) && p->info.typ->type != Tfun)
	    fprintf(fout, "\n\tsoap->mustUnderstand = 1;");
	  else if(p->info.typ->type==Tarray){
	    fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, a->%s, \"%s\");", c_ident(p->info.typ), p->sym->name, xsi_type_u(typ));
	  }
	  else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)){
	    /* CLASS ( within CLASS ) */
	    fprintf(fout,"\n\t(a->%s.soap_out(soap, tag, id, \"%s\");", p->sym->name,xsi_type_u(typ));
	  }
	  else if (is_qname(p->info.typ))
	    fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, (char*const*)&soap_tmp_%s, \"%s\");", c_ident(p->info.typ), p->sym->name, xsi_type_u(typ));
	  else if (is_XML(p->info.typ) && is_string(p->info.typ))
	    fprintf(fout,"\n\tsoap_outliteral(soap, \"%s\", &a->%s);", ns_add(p->sym->name, nse),p->sym->name);
	  else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    fprintf(fout,"\n\tsoap_outwliteral(soap, \"%s\", &a->%s);", ns_add(p->sym->name, nse),p->sym->name);
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	    fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, &a->%s, \"%s\");", c_ident(p->info.typ), p->sym->name, xsi_type_u(typ));
	  }
   }
   else
   {  fprintf(fout,"\n\tsoap_element_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), type);",c_ident(typ));
      fflush(fout);
      for (t = table; t; t = t->prev)
      {	for (p = t->list; p; p = p->next)
	{ if (p->info.sto & Sreturn)
	    if (p->info.typ->type == Tpointer)
	      fprintf(fout,"\n\tif (a->%s)\n\t\tsoap_element_result(soap, \"%s\");", p->sym->name, ns_add(p->sym->name, nse));
            else
	      fprintf(fout,"\n\tsoap_element_result(soap, \"%s\");", ns_add(p->sym->name, nse));
	  if ((p->info.sto & SmustUnderstand) && !(p->info.sto & Sconst) && !is_transient(p->info.typ) && !is_void(p->info.typ) && p->info.typ->type != Tfun)
	    fprintf(fout, "\n\tsoap->mustUnderstand = 1;");
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_repetition(p))
	  { fprintf(fout,"\n\tif (a->%s)", p->next->sym->name);
            fprintf(fout,"\n\t{\tint i;\n\t\tfor (i = 0; i < a->%s; i++)", p->sym->name);
            if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
              fprintf(fout,"\n\t\t\ta->%s[i].soap_out(soap, \"%s\", -1, \"%s\");", p->next->sym->name, ns_add(p->next->sym->name, nse),xsi_type_cond_u(p->next->info.typ->ref, !has_ns_eq(NULL, p->next->sym->name)));
            else if (is_XML(p->next->info.typ->ref) && is_string(p->next->info.typ->ref))
	      fprintf(fout,"\n\t\t\tsoap_outliteral(soap, \"%s\", a->%s + i);", ns_add(p->next->sym->name, nse),p->next->sym->name);
            else if (is_XML(p->next->info.typ->ref) && is_wstring(p->next->info.typ->ref))
	      fprintf(fout,"\n\t\t\tsoap_outwliteral(soap, \"%s\", a->%s + i);", ns_add(p->next->sym->name, nse),p->next->sym->name);
	    else
              fprintf(fout,"\n\t\t\tsoap_out_%s(soap, \"%s\", -1, a->%s + i, \"%s\");", c_ident(p->next->info.typ->ref), ns_add(p->next->sym->name, nse), p->next->sym->name, xsi_type_cond_u(p->next->info.typ->ref, !has_ns_eq(NULL, p->next->sym->name)));
            fprintf(fout,"\n\t}");
            p = p->next;
	  }
	  else if (is_anytype(p))
	  { fprintf(fout,"\n\tsoap_putelement(soap, a->%s, \"%s\", -1, a->%s);", p->next->sym->name, ns_add(p->next->sym->name, nse),p->sym->name);
            p = p->next;
	  }
	  else if (p->info.typ->type==Tarray)
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, a->%s, \"%s\");", c_ident(p->info.typ),ns_add(p->sym->name, nse),p->sym->name,xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	    fprintf(fout,"\n\ta->%s.soap_out(soap, \"%s\", -1, \"%s\");", p->sym->name,ns_add(p->sym->name, nse),xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (is_qname(p->info.typ))
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, (char*const*)&soap_tmp_%s, \"%s\");", c_ident(p->info.typ),ns_add(p->sym->name, nse),p->sym->name, xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (is_XML(p->info.typ) && is_string(p->info.typ))
	    fprintf(fout,"\n\tsoap_outliteral(soap, \"%s\", &a->%s);", ns_add(p->sym->name, nse),p->sym->name);
	  else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    fprintf(fout,"\n\tsoap_outwliteral(soap, \"%s\", &a->%s);", ns_add(p->sym->name, nse),p->sym->name);
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ))
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, &a->%s, \"%s\");", c_ident(p->info.typ),ns_add(p->sym->name, nse),p->sym->name, xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	}
      }	
      fprintf(fout,"\n\tsoap_element_end_out(soap, tag);");
    }
    fprintf(fout,"\n\treturn SOAP_OK;\n}");	 
    fflush(fout);
    break;
      
    case Tclass:
      table=(Table*)typ->ref;
      if (!is_volatile(typ))
      { 
        if (is_external(typ))
        { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
          return;
        }
        fprintf(fout,"\n\nint %s::soap_out(struct soap *soap, const char *tag, int id, const char *type) const", typ->id->name); 
        fprintf(fout,"\n{\n\treturn soap_out_%s(soap, tag, id, this, type);\n}", typ->id->name); 
      }
      fprintf(fhead,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", typ->id->name, c_type_id(typ, "*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)\n{", typ->id->name, c_type_id(typ, "*a")); 
      fflush(fout);
      for (t = table; t; t = t->prev)
      {	for (p = t->list; p; p = p->next)
	{ if (p->info.sto & Sattribute)
	    soap_set_attr(p->info.typ, ptr_cast(typ, "a"), p->sym->name, ns_add(p->sym->name, nsa));
	  else if (is_qname(p->info.typ))
            fprintf(fout,"\n\tconst char *soap_tmp_%s = soap_QName2s(soap, a->%s);", p->sym->name, p->sym->name);
        }
     }
     if (is_primclass(typ))
     {
	for (t = table; t; t = t->prev)
	{ p = t->list;
	  if (p && is_item(p))
	    break;
        }
	if (p->info.sto & Sreturn)
	  if (p->info.typ->type == Tpointer)
	    fprintf(fout,"\n\tif (a->%s)\n\t\tsoap_element_result(soap, \"%s\");", p->sym->name, ns_add(p->sym->name, nse));
          else
	    fprintf(fout,"\n\tsoap_element_result(soap, \"%s\");", ns_add(p->sym->name, nse));
	  if ((p->info.sto & SmustUnderstand) && !(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && !is_transient(p->info.typ) && !is_void(p->info.typ) && p->info.typ->type != Tfun)
	    fprintf(fout, "\n\tsoap->mustUnderstand = 1;");
	  if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_XML(p->info.typ) && is_string(p->info.typ))
	    fprintf(fout,"\n\tsoap_outliteral(soap, \"%s\", &(((%s*)a)->%s));", ns_add(p->sym->name, nse),t->sym->name,p->sym->name);
	  else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    fprintf(fout,"\n\tsoap_outwliteral(soap, \"%s\", &(((%s*)a)->%s));", ns_add(p->sym->name, nse),t->sym->name,p->sym->name);
	  else if (table->prev)
	  {
	    if(p->info.typ->type==Tarray)
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, ((%s*)a)->%s, \"%s\");", c_ident(p->info.typ), t->sym->name,p->sym->name, xsi_type(typ));
	    else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	      fprintf(fout,"\n\t(((%s*)a)->%s).soap_out(soap, tag, id, \"%s\");", t->sym->name, p->sym->name,xsi_type(typ));
	    else if (is_qname(p->info.typ))
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, (char*const*)&soap_tmp_%s, \"%s\");", c_ident(p->info.typ), p->sym->name, xsi_type(typ));
	    else if (p->info.typ->type != Tfun && !is_void(p->info.typ))
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, &(((%s*)a)->%s), \"%s\");", c_ident(p->info.typ), t->sym->name,p->sym->name,xsi_type(typ));
	  }
	  else
	  { if(p->info.typ->type==Tarray)
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, ((%s*)a)->%s, \"%s\");", c_ident(p->info.typ), t->sym->name,p->sym->name, xsi_type_u(typ));
	    else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	      fprintf(fout,"\n\t(((%s*)a)->%s).soap_out(soap, tag, id, \"%s\");", t->sym->name, p->sym->name,xsi_type_u(typ));
	    else if (is_qname(p->info.typ))
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, (char*const*)&soap_tmp_%s, \"%s\");", c_ident(p->info.typ), p->sym->name, xsi_type_u(typ));
	    else if (p->info.typ->type != Tfun && !is_void(p->info.typ))
	      fprintf(fout,"\n\tsoap_out_%s(soap, tag, id, &(((%s*)a)->%s), \"%s\");", c_ident(p->info.typ), t->sym->name,p->sym->name,xsi_type_u(typ));
	  }
     }
     else
     { if (table && table->prev)
         fprintf(fout,"\n\tsoap_element_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), \"%s\");", c_ident(typ), xsi_type(typ));
       else
         fprintf(fout,"\n\tsoap_element_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), type);", c_ident(typ));
       fflush(fout);
      
      i=0;
      /* Get the depth of the inheritance hierarchy */
      for (t = table; t; t = t->prev)
	i++;

      /* Call routines to output the member data of the class */
      /* Data members of the Base Classes are outputed first
	 followed by the data members of the Derived classes.
	 Overridden data members are output twice once for the base class
	 they are defined in and once for the derived class that overwrites
	 them */
      
      for (; i > 0; i--)
      { t = table;
	for (j = 0; j< i-1; j++)
	  t = t->prev;
	for (p = t->list; p != (Entry*) 0; p = p->next)
	{ if ((p->info.sto & SmustUnderstand) && !(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && !is_transient(p->info.typ) && !is_void(p->info.typ) && p->info.typ->type != Tfun)
	    fprintf(fout, "\n\tsoap->mustUnderstand = 1;");
	  if (is_item(p))
	    ;
	  else if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_repetition(p))
	  { fprintf(fout,"\n\tif (((%s*)a)->%s)", table->sym->name, p->next->sym->name);
            fprintf(fout,"\n\t{\tint i;\n\t\tfor (i = 0; i < ((%s*)a)->%s; i++)", table->sym->name, p->sym->name);
            if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
              fprintf(fout,"\n\t\t\t((%s*)a)->%s[i].soap_out(soap, \"%s\", -1, \"%s\");", table->sym->name, p->next->sym->name, ns_add_overridden(t, p->next, nse),xsi_type_cond_u(p->next->info.typ->ref, !has_ns_eq(NULL, p->next->sym->name)));
            else if (is_XML(p->next->info.typ->ref) && is_string(p->next->info.typ->ref))
	      fprintf(fout,"\n\t\t\tsoap_outliteral(soap, \"%s\", ((%s*)a)->%s + i);", ns_add(p->next->sym->name, nse),table->sym->name, p->next->sym->name);
            else if (is_XML(p->next->info.typ->ref) && is_wstring(p->next->info.typ->ref))
              fprintf(fout,"\n\t\t\tsoap_outwliteral(soap, \"%s\", ((%s*a)->%s + i);", ns_add(p->next->sym->name, nse),table->sym->name, p->next->sym->name);
            else
              fprintf(fout,"\n\t\t\tsoap_out_%s(soap, \"%s\", -1, ((%s*)a)->%s + i, \"%s\");", c_ident(p->next->info.typ->ref), ns_add_overridden(t, p->next, nse), table->sym->name, p->next->sym->name, xsi_type_cond_u(p->next->info.typ->ref, !has_ns_eq(NULL, p->next->sym->name)));
            fprintf(fout,"\n\t}");
            p = p->next;
	  }
	  else if (is_anytype(p))
	  { fprintf(fout,"\n\tsoap_putelement(soap, ((%s*)a)->%s, \"%s\", -1, ((%s*)a)->%s);", table->sym->name, p->next->sym->name, ns_add(p->sym->name, nse),table->sym->name,p->sym->name);
            p = p->next;
	  }
	  else if (p->info.typ->type==Tarray)
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, ((%s*)a)->%s, \"%s\");", c_ident(p->info.typ),ns_add_overridden(t, p, nse), t->sym->name,p->sym->name, xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ))
	    fprintf(fout,"\n\t(((%s*)a)->%s).soap_out(soap, \"%s\", -1, \"%s\");", t->sym->name, p->sym->name,ns_add_overridden(t, p, nse),xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (is_qname(p->info.typ))
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, (char*const*)&soap_tmp_%s, \"%s\");", c_ident(p->info.typ),ns_add_overridden(t, p, nse),p->sym->name, xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
	  else if (is_XML(p->info.typ) && is_string(p->info.typ))
	    fprintf(fout,"\n\tsoap_outliteral(soap, \"%s\", &(((%s*)a)->%s));", ns_add_overridden(t, p, nse),t->sym->name,p->sym->name);
	  else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    fprintf(fout,"\n\tsoap_outwliteral(soap, \"%s\", &(((%s*)a)->%s));", ns_add_overridden(t, p, nse),t->sym->name,p->sym->name);
	  else if (p->info.typ->type != Tfun && !is_void(p->info.typ))
	    fprintf(fout,"\n\tsoap_out_%s(soap, \"%s\", -1, &(((%s*)a)->%s), \"%s\");", c_ident(p->info.typ),ns_add_overridden(t, p, nse), t->sym->name,p->sym->name,xsi_type_cond_u(p->info.typ, !has_ns_eq(NULL, p->sym->name)));
  	  fflush(fout);
	}
      }
      fprintf(fout,"\n\tsoap_element_end_out(soap, tag);");
      }
      fprintf(fout,"\n\treturn SOAP_OK;\n}");	 
      fflush(fout);
      break;
      
    case Tpointer:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char *, int, %s, const char *);", c_ident(typ),c_type_id(typ, "const*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char *, int, %s, const char *);", c_ident(typ),c_type_id(typ, "const*")); 
      fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "const*a")); 
      if (is_template(typ))
      { fprintf(fout,"\n\tif (!*a)");
        fprintf(fout,"\n\t\treturn soap_element_null(soap, tag, id, type);");
        fprintf(fout,"\n\treturn soap_out_%s(soap, tag, id, *a, type);", c_ident(typ->ref));
      }
      else
      { if (p = is_dynamic_array(typ->ref))
        { d = get_Darraydims(typ->ref);
	  if (d)
            fprintf(fout,"\n\tid = soap_element_id(soap, tag, id, *a, (struct soap_array*)&(*a)->%s, %d, type, SOAP_TYPE_%s);", p->sym->name, d, c_ident(typ->ref));
	  else
            fprintf(fout,"\n\tid = soap_element_id(soap, tag, id, *a, (struct soap_array*)&(*a)->%s, 1, type, SOAP_TYPE_%s);", p->sym->name, c_ident(typ->ref));
        }
	else
          fprintf(fout,"\n\tid = soap_element_id(soap, tag, id, *a, NULL, 0, type, SOAP_TYPE_%s);",c_ident(typ->ref));
	fprintf(fout,"\n\tif (id < 0)\n\t\treturn soap->error;");
        if(((Tnode *) typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
	  fprintf(fout,"\n\treturn (*a)->soap_out(soap, tag, id, type);", c_ident(typ->ref));
        else
	  fprintf(fout,"\n\treturn soap_out_%s(soap, tag, id, *a, type);",c_ident(typ->ref));
      }
      fprintf(fout,"\n}");
      break;

    case Tarray:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, %s, const char*);", c_ident(typ),c_type_id(typ, "const")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, %s, const char*);", c_ident(typ),c_type_id(typ, "const")); 
      fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "const a")); 
      fprintf(fout,"\n\tint i;");
      /*
      if (is_untyped(typ->ref))
        fprintf(fout,"\n\tsoap_array_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), \"%s[%d]\", 0);",c_ident(typ), wsdl_type(typ->ref, "xsd"), get_dimension(typ));
      else
      */
        fprintf(fout,"\n\tsoap_array_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), \"%s[%d]\", 0);",c_ident(typ), xsi_type_Tarray(typ), get_dimension(typ));
      n=typ->ref;
      cardinality = 1;
      while(n->type==Tarray)
	{
	  n=n->ref;
	  cardinality++;
	}

      fprintf(fout,"\n\tfor (i = 0; i < %d; i++)\n\t{",get_dimension(typ));
     if (((Tnode *)typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
     { if(cardinality>1)
         fprintf(fout,"\n\t\ta[i].soap_out(soap, \"item\", -1, \"%s\")", xsi_type_u(typ->ref));
       else fprintf(fout,"\n\t\t(a+i)->soap_out(soap, \"item\", -1, \"%s\")", xsi_type_u(typ->ref));
     }
     else
     { if(((Tnode *)typ->ref)->type != Tarray)
       { if(((Tnode *)typ->ref)->type == Tpointer)
	  fprintf(fout,"\n\t\tsoap->position = 1;\n\t\tsoap->positions[0] = i;\n\t\tsoap_out_%s(soap, \"item\", -1, a", c_ident(typ->ref));
	 else
	  fprintf(fout,"\n\t\tsoap_out_%s(soap, \"item\", -1, a",c_ident(typ->ref));
       }
       else
         fprintf(fout,"\n\t\tsoap_out_%s(soap, \"item\", -1, a",c_ident(typ->ref));
       if(cardinality>1)
         fprintf(fout,"[i], \"%s\")", xsi_type_u(typ->ref));
       else
         fprintf(fout,"+i, \"%s\")", xsi_type_u(typ->ref));
      }
      if(((Tnode *)typ->ref)->type == Tpointer)
        fprintf(fout,";\n\t}\n\tsoap->position = 0;\n\tsoap_element_end_out(soap, tag);");
      else
        fprintf(fout,";\n\t}\n\tsoap_element_end_out(soap, tag);");		
      fprintf(fout,"\n\treturn SOAP_OK;\n}");
      break;

    case Tenum:
      if (is_external(typ))
      { fprintf(fhead, "\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ), c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead, "\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ), c_type_id(typ, "*"));
      fprintf(fout, "\n\nstatic const struct soap_code_map soap_codes_%s[] =\n{", c_ident(typ));
      for (t = (Table*)typ->ref; t; t = t->prev)
      { for (p = t->list; p; p = p->next)
	  fprintf(fout, "\t{ (long)%s, \"%s\" },\n", p->sym->name, ns_remove2(p->sym->name));
      }	
      fprintf(fout, "\t{ 0, NULL }\n");
      fprintf(fout, "};");
      if (!is_mask(typ))
      { fprintf(fhead, "\n\nSOAP_FMAC3 const char* SOAP_FMAC4 soap_%s2s(struct soap*, %s);", c_ident(typ), c_type(typ));
        fprintf(fout, "\n\nSOAP_FMAC3 const char* SOAP_FMAC4 soap_%s2s(struct soap *soap, %s)", c_ident(typ), c_type_id(typ, "n"));
        fprintf(fout, "\n{\tconst char *s = soap_str_code(soap_codes_%s, (long)n);", c_ident(typ));
        fprintf(fout, "\n\tif (s)\n\t\treturn s;");
        fprintf(fout, "\n\treturn soap_long2s(soap, (long)n);");
        fprintf(fout, "\n}");
      }
      if (is_mask(typ))
      { fprintf(fout, "\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)", c_ident(typ), c_type_id(typ, "*a"));
        fprintf(fout, "\n{\tlong i;\n\tsoap_element_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), type);", c_ident(typ));
        fprintf(fout, "\n\tfor (i = 1; i; i <<= 1)");
        fprintf(fout, "\n\t\tswitch ((long)*a & i)\n\t\t{");
        for (t = (Table*)typ->ref; t; t = t->prev)
        { for (p = t->list; p; p = p->next)
	    fprintf(fout, "\n\t\t\tcase "SOAP_LONG_FORMAT": soap_send(soap, \"%s \"); break;", p->info.val.i, ns_remove2(p->sym->name));
        }	
        fprintf(fout, "\n\t\t}");
      }
      else
      { fprintf(fout, "\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)", c_ident(typ), c_type_id(typ, "*a"));
        fprintf(fout, "\n{\tsoap_element_begin_out(soap, tag, soap_embedded_id(soap, id, a, SOAP_TYPE_%s), type);", c_ident(typ));
        fprintf(fout, "\n\tsoap_send(soap, soap_%s2s(soap, *a));", c_ident(typ));
      }
      fprintf(fout, "\n\treturn soap_element_end_out(soap, tag);\n}");
      break;
    case Ttemplate:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 int SOAP_FMAC2 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "*a")); 

      fprintf(fout, "\n\tfor (%s::const_iterator i = a->begin(); i != a->end(); ++i)", c_type(typ));
      n = typ->ref;
      if (n->type==Tarray)
	fprintf(fout,"\n\t\tif (soap_out_%s(soap, tag, id, *i, \"%s\"))", c_ident(n), xsi_type_u(typ));
      else if (n->type==Tclass && !is_external(n) && !is_volatile(n))
	fprintf(fout,"\n\t\tif ((*i).soap_out(soap, tag, id, \"%s\"))", xsi_type_u(typ));
      else if (is_XML(n) && is_string(n))
        fprintf(fout,"\n\t\tif (soap_outliteral(soap, \"%s\", &(*i)))", xsi_type_u(typ));
      else if (is_XML(n) && is_wstring(n))
        fprintf(fout,"\n\t\tif (soap_outwliteral(soap, \"%s\", &(*i)))", xsi_type_u(typ));
      else
	fprintf(fout,"\n\t\tif (soap_out_%s(soap, tag, id, &(*i), \"%s\"))", c_ident(n), xsi_type_u(typ));
      fprintf(fout, "\n\t\t\treturn soap->error;");
      fprintf(fout, "\n\treturn SOAP_OK;\n}");
      break;
    default: break;
    }
}	  

void
soap_out_Darray(Tnode *typ)
{ int i, j, d = 0;
  Table *t, *table;
  Entry *p, *q;
  char *nsa = ns_qualifiedAttribute(typ);

  table=(Table*)typ->ref;
  fprintf(fhead,"\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap*, const char*, int, const %s, const char*);", c_ident(typ),c_type_id(typ, "*")); 
      if (is_external(typ))
        return;
  if (typ->type == Tclass)
  { fprintf(fout,"\n\nint %s::soap_out(struct soap *soap, const char *tag, int id, const char *type) const", c_type(typ)); 
    fprintf(fout,"\n{\treturn soap_out_%s(soap, tag, id, this, type);\n}", c_ident(typ)); 
  }
  fflush(fout);
  fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_out_%s(struct soap *soap, const char *tag, int id, const %s, const char *type)\n{", c_ident(typ),c_type_id(typ, "*a")); 
  if (!is_binary(typ))
  { d = get_Darraydims(typ);
    if (d)
      fprintf(fout,"\n\tint i, n = soap_size(a->__size, %d);", d);
    else
      fprintf(fout,"\n\tint i, n = a->__size;");
  }
  if (typ->type == Tclass)
  { for (t = table; t; t = t->prev)
    {	for (p = t->list; p; p = p->next)
	{ if (p->info.sto & Sattribute)
	    soap_set_attr(p->info.typ, ptr_cast(typ, "a"), p->sym->name, ns_add(p->sym->name, nsa));
        }
    }
  }
  else
  { for (t = table; t; t = t->prev)
    {	for (p = t->list; p; p = p->next)
	{ if (p->info.sto & Sattribute)
	    soap_set_attr(p->info.typ, "a", p->sym->name, ns_add(p->sym->name, nsa));
        }
    }
  }
  p = is_dynamic_array(typ);
  q = table->list;
  if (!has_ns(typ) && !is_untyped(typ) && !is_binary(typ))
  { if (is_untyped(p->info.typ))
    { if (has_offset(typ))
        if (d)
          fprintf(fout,"\n\tchar *t = soap_putsizesoffsets(soap, \"%s\", a->__size, a->__offset, %d);", wsdl_type(p->info.typ, "xsd"), d); 
        else
          fprintf(fout,"\n\tchar *t = soap_putsize(soap, \"%s\", n + a->__offset);", wsdl_type(p->info.typ, "xsd"));
      else if (d)
	fprintf(fout,"\n\tchar *t = soap_putsizes(soap, \"%s\", a->__size, %d);", wsdl_type(p->info.typ, "xsd"), d);
      else
        fprintf(fout,"\n\tchar *t = soap_putsize(soap, \"%s\", n);", wsdl_type(p->info.typ, "xsd"));
    }
    else
    { if (has_offset(typ))
        if (d)
          fprintf(fout,"\n\tchar *t = soap_putsizesoffsets(soap, \"%s\", a->__size, a->__offset, %d);", xsi_type(typ), d);
        else
          fprintf(fout,"\n\tchar *t = soap_putsize(soap, \"%s\", n + a->__offset);",xsi_type(typ));
      else if (d)
        fprintf(fout,"\n\tchar *t = soap_putsizes(soap, \"%s\", a->__size, %d);", xsi_type(typ),d);
      else
        fprintf(fout,"\n\tchar *t = soap_putsize(soap, \"%s\", a->__size);" ,xsi_type(typ));
    }
  }
  if (d)
    fprintf(fout,"\n\tid = soap_element_id(soap, tag, id, a, (struct soap_array*)&a->%s, %d, type, SOAP_TYPE_%s);", p->sym->name, d, c_ident(typ));
  else if (is_attachment(typ))
    fprintf(fout,"\n\tid = soap_element_dime(soap, tag, id, a, (struct soap_array*)&a->%s, a->id, a->type, a->options, 1, type, SOAP_TYPE_%s);", p->sym->name, c_ident(typ));
  else
    fprintf(fout,"\n\tid = soap_element_id(soap, tag, id, a, (struct soap_array*)&a->%s, 1, type, SOAP_TYPE_%s);", p->sym->name, c_ident(typ));
  fprintf(fout,"\n\tif (id < 0)\n\t\treturn soap->error;");
  if (has_ns(typ) || is_untyped(typ) || is_binary(typ))
  { if (table->prev)
      fprintf(fout,"\n\tsoap_element_begin_out(soap, tag, id, \"%s\");", xsi_type(typ));
    else
      fprintf(fout,"\n\tsoap_element_begin_out(soap, tag, id, type);");
  }
  else if (has_offset(typ))
    if (d)
      fprintf(fout,"\n\tsoap_array_begin_out(soap, tag, id, t, soap_putoffsets(soap, a->__offset, %d));", d);
    else
      fprintf(fout,"\n\tsoap_array_begin_out(soap, tag, id, t, soap_putoffset(soap, a->__offset));");
  else
    fprintf(fout,"\n\tsoap_array_begin_out(soap, tag, id, t, NULL);");
  if (is_binary(typ) && !is_hexBinary(typ))
    fprintf(fout, "\n\tsoap_putbase64(soap, a->__ptr, a->__size);");
  else if (is_hexBinary(typ))
    fprintf(fout, "\n\tsoap_puthex(soap, a->__ptr, a->__size);");
  else
  { fprintf(fout,"\n\tfor (i = 0; i < n; i++)\n\t{");
    if (!has_ns(typ) && !is_untyped(typ))
    { if (d)
      { fprintf(fout,"\n\t\tsoap->position = %d;", d);
        for (i = 0; i < d; i++)
	{ fprintf(fout, "\n\t\tsoap->positions[%d] = i", i);
          for (j = i+1; j < d; j++)
	    fprintf(fout, "/a->__size[%d]", j);
	  fprintf(fout, "%%a->__size[%d];", i);
        }
        if (is_XML(p->info.typ->ref) && is_string(p->info.typ->ref))
          fprintf(fout,"\n\t\tsoap_outliteral(soap, \"%s\", &a->%s[i]);", p->sym->name[5]?ns_convert(p->sym->name+5):"item",p->sym->name);
        else if (is_XML(p->info.typ->ref) && is_wstring(p->info.typ->ref))
          fprintf(fout,"\n\t\tsoap_outwliteral(soap, \"%s\", &a->%s[i]);", p->sym->name[5]?ns_convert(p->sym->name+5):"item",p->sym->name);
        else if (((Tnode *)p->info.typ->ref)->type == Tclass && !is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
          fprintf(fout,"\n\t\ta->%s[i].soap_out(soap, \"item\", -1, \"%s\");", p->sym->name, xsi_type_u(((Tnode *)p->info.typ->ref)));
	else
	  fprintf(fout, "\n\t\tsoap_out_%s(soap, \"%s\", -1, &a->%s[i], \"%s\");",c_ident(((Tnode *)p->info.typ->ref)), p->sym->name[5]?ns_convert(p->sym->name+5):"item", p->sym->name, xsi_type_u(((Tnode *)p->info.typ->ref)));
      }
      else
      { fprintf(fout,"\n\t\tsoap->position = 1;\n\t\tsoap->positions[0] = i;");
        if (is_XML(p->info.typ->ref) && is_string(p->info.typ->ref))
          fprintf(fout,"\n\t\tsoap_outliteral(soap, \"%s\", &a->%s[i]);", p->sym->name[5]?ns_convert(p->sym->name+5):"item",p->sym->name);
        else if (is_XML(p->info.typ->ref) && is_wstring(p->info.typ->ref))
          fprintf(fout,"\n\t\tsoap_outwliteral(soap, \"%s\", &a->%s[i]);", p->sym->name[5]?ns_convert(p->sym->name+5):"item",p->sym->name);
        else if (((Tnode *)p->info.typ->ref)->type == Tclass && !is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
          fprintf(fout,"\n\t\ta->%s[i].soap_out(soap, \"item\", -1, \"%s\");", p->sym->name, xsi_type_u(((Tnode *)p->info.typ->ref)));
	else
          fprintf(fout,"\n\t\tsoap_out_%s(soap, \"%s\", -1, &a->%s[i], \"%s\");",c_ident(((Tnode *)p->info.typ->ref)), p->sym->name[5]?ns_convert(p->sym->name+5):"item", p->sym->name,xsi_type_u(((Tnode *)p->info.typ->ref)));
      }
    }
    else
      fprintf(fout,"\n\t\tsoap_out_%s(soap, \"%s\", -1, &a->%s[i], \"%s\");",c_ident(((Tnode *)p->info.typ->ref)), 
	p->sym->name[5]?ns_convert(p->sym->name+5):"item", p->sym->name, xsi_type_u(((Tnode *)p->info.typ->ref)));
  }
  if (is_binary(typ))
    fprintf(fout,"\n\tsoap_element_end_out(soap, tag);");
  else if (!has_ns(typ) && !is_untyped(typ))
    fprintf(fout,"\n\t}\n\tsoap->position = 0;\n\tsoap_element_end_out(soap, tag);");
  else
    fprintf(fout,"\n\t}\n\tsoap_element_end_out(soap, tag);");
  fprintf(fout,"\n\treturn SOAP_OK;\n}");	 
}

void
soap_get(Tnode *typ)
{
  Tnode *temp;
  
  if(typ->type==Tarray)
    {
      /* ARRAY */
      temp = typ;
      while(temp->type == Tarray){
	temp = temp->ref;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s * SOAP_FMAC4 soap_get_%s(struct soap*, %s, const char*, const char*);", c_type(temp),c_ident(typ),c_type(typ));
      fprintf(fout,"\n\nSOAP_FMAC3 %s * SOAP_FMAC4 soap_get_%s(struct soap *soap, %s, const char *tag, const char *type)", c_type(temp),c_ident(typ),c_type_id(typ, "a"));
      fprintf(fout,"\n{\t%s;",c_type_id(temp, "(*p)"));
      fprintf(fout,"\n\tif ((p = soap_in_%s(soap, tag, a, type)))", c_ident(typ));
    }
  else if(typ->type==Tclass && !is_external(typ) && !is_volatile(typ))
    {
      /* CLASS  */
      fprintf(fout,"\n\nvoid *%s::soap_get(struct soap *soap, const char *tag, const char *type)", c_type(typ));
      fprintf(fout,"\n{\n\treturn soap_get_%s(soap, this, tag, type);\n}", c_ident(typ));
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_get_%s(struct soap*, %s, const char*, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_get_%s(struct soap *soap, %s, const char *tag, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*p"));
      fprintf(fout,"\n\tif ((p = soap_in_%s(soap, tag, p, type)))", c_ident(typ));
    }
  else 
    {
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_get_%s(struct soap*, %s, const char*, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_get_%s(struct soap *soap, %s, const char *tag, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*p"));
      fprintf(fout,"\n\tif ((p = soap_in_%s(soap, tag, p, type)))", c_ident(typ));
    }
  fprintf(fout,"\n\t\tsoap_getindependent(soap);");
  fprintf(fout,"\n\treturn p;\n}");
  fflush(fout);
}

void
soap_in(Tnode *typ)
{ Entry *p;
  Table *table,*t;
  int total,a, cardinality,i,j;
  Tnode *n, *temp;
  char *nse = ns_qualifiedElement(typ);
  char *nsa = ns_qualifiedAttribute(typ);
  if (is_dynamic_array(typ))
  { soap_in_Darray(typ);
    return;
  }
  if (is_primitive_or_string(typ) && typ->type != Tenum)
  {
      if (is_stdstring(typ))
      { fprintf(fhead,"\nSOAP_FMAC3 std::string * SOAP_FMAC4 soap_in_%s(struct soap*, const char*, std::string*, const char*);", c_ident(typ));
        fprintf(fout,"\n\nSOAP_FMAC1 std::string * SOAP_FMAC2 soap_in_%s(struct soap *soap, const char *tag, std::string *s, const char *type)\n{\n\tif (soap_element_begin_in(soap, tag, 1))\n\t\treturn NULL;\n\tif (!s)\n\t\ts = soap_new_std__string(soap, -1);\n\tif (soap->null)\n\t\tif (s)\n\t\t\ts->erase();\n\tif (*soap->type && soap_match_tag(soap, soap->type, type))\n\t{\tsoap->error = SOAP_TYPE;\n\t\treturn NULL;\n\t}", c_ident(typ));
        fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{\tchar *t;\n\t\ts = (std::string*)soap_class_id_enter(soap, soap->id, s, SOAP_TYPE_%s, sizeof(std::string), soap->type, soap->arrayType);\n\t\tif (s)\n\t\t\tif ((t = soap_string_in(soap, 1, %ld, %ld)))\n\t\t\t\ts->assign(t);\n\t\t\telse\n\t\t\t\treturn NULL;\n\t}\n\telse\n\t\ts = (std::string*)soap_id_forward(soap, soap->href, soap_class_id_enter(soap, soap->id, s, SOAP_TYPE_%s, sizeof(std::string), soap->type, soap->arrayType), SOAP_TYPE_%s, 0, sizeof(std::string), 0, soap_copy_%s);\n\tif (soap->body && soap_element_end_in(soap, tag))\n\t\treturn NULL;\n\treturn s;\n}", c_ident(typ), typ->minLength, typ->maxLength, c_ident(typ), c_ident(typ), c_ident(typ));
        return;
      }
      if (is_stdwstring(typ))
      { fprintf(fhead,"\nSOAP_FMAC3 std::wstring * SOAP_FMAC4 soap_in_%s(struct soap*, const char*, std::wstring*, const char*);", c_ident(typ));
        fprintf(fout,"\n\nSOAP_FMAC1 std::wstring * SOAP_FMAC2 soap_in_%s(struct soap *soap, const char *tag, std::wstring *s, const char *type)\n{\n\tif (soap_element_begin_in(soap, tag, 1))\n\t\treturn NULL;\n\tif (!s)\n\t\ts = soap_new_std__wstring(soap, -1);\n\tif (soap->null)\n\t\tif (s)\n\t\t\ts->erase();\n\tif (*soap->type && soap_match_tag(soap, soap->type, type))\n\t{\tsoap->error = SOAP_TYPE;\n\t\treturn NULL;\n\t}", c_ident(typ));
        fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{\twchar *t;\n\t\ts = (std::wstring*)soap_class_id_enter(soap, soap->id, s, SOAP_TYPE_%s, sizeof(std::wstring), soap->type, soap->arrayType);\n\t\tif (s)\n\t\t\tif ((t = soap_wstring_in(soap, 1, %ld, %ld)))\n\t\t\t\ts->assign(t);\n\t\t\telse\n\t\t\t\treturn NULL;\n\t}\n\telse\n\t\ts = (std::wstring*)soap_id_forward(soap, soap->href, soap_class_id_enter(soap, soap->id, s, SOAP_TYPE_%s, sizeof(std::wstring), soap->type, soap->arrayType), SOAP_TYPE_%s, 0, sizeof(std::wstring), 0, soap_copy_%s);\n\tif (soap->body && soap_element_end_in(soap, tag))\n\t\treturn NULL;\n\treturn s;\n}", c_ident(typ), typ->minLength, typ->maxLength, c_ident(typ), c_ident(typ), c_ident(typ));
        return;
      }
    if (is_external(typ))
    { fprintf(fhead,"\nSOAP_FMAC1 %s * SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type(typ), c_ident(typ),c_type_id(typ, "*")); 
      return;
    }
    fprintf(fhead,"\nSOAP_FMAC3 %s * SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type(typ), c_ident(typ),c_type_id(typ, "*")); 
    fprintf(fout,"\n\nSOAP_FMAC3 %s * SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{", c_type(typ), c_ident(typ),c_type_id(typ, "*a")); 
    if (typ->type == Tllong || typ->type == Tullong)
      fprintf(fout,"\n\treturn soap_in%s(soap, tag, a, type, SOAP_TYPE_%s);\n}", c_type(typ), c_ident(typ));
    else if (is_wstring(typ))
      fprintf(fout,"\n\treturn soap_inwstring(soap, tag, a, type, SOAP_TYPE_%s, %ld, %ld);\n}", c_ident(typ), typ->minLength, typ->maxLength);
    else if (is_string(typ))
      fprintf(fout,"\n\treturn soap_instring(soap, tag, a, type, SOAP_TYPE_%s, %d, %ld, %ld);\n}", c_ident(typ), is_qname(typ)+1, typ->minLength, typ->maxLength);
    else
      fprintf(fout,"\n\treturn soap_in%s(soap, tag, a, type, SOAP_TYPE_%s);\n}", the_type(typ), c_ident(typ));
    fflush(fout);
    return;
  }
  switch(typ->type)
  { case Tstruct:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a"));
      table = (Table *)typ->ref;
      if (is_primclass(typ))
      { fprintf(fout, "\n\tif (soap_peek_element(soap))\n\t\treturn NULL;");
	if (has_class(typ))
          fprintf(fout,"\n\tif (!(a = (%s)soap_class_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), soap->type, soap->arrayType)))\n\t\treturn NULL;", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
	else
          fprintf(fout,"\n\tif (!(a = (%s)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL)))\n\t\treturn NULL;", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
        fprintf(fout,"\n\t*soap->id = '\\0';");
        fprintf(fout,"\n\tif (soap->alloced)");
        fprintf(fout,"\n\t\tsoap_default_%s(soap, a);",c_ident(typ));
          for (t = (Table*)typ->ref; t; t = t->prev)
          { for (p = t->list; p; p = p->next) 
	      if (p->info.sto & Sattribute)
		soap_attr_value(p, "a", p->sym->name, ns_add(p->sym->name, nsa));
	  }
      fflush(fout);
	for (table = (Table*)typ->ref; table; table = table->prev)
	{ p = table->list;
	  if (p && is_item(p))
	    break;
        }
	    if (is_XML(p->info.typ) && is_string(p->info.typ))
	    { fprintf(fout,"\n\tif (!soap_inliteral(soap, tag, &a->%s))", p->sym->name,xsi_type(typ));
	    }
	    else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    { fprintf(fout,"\n\tif (!soap_inwliteral(soap, tag, &a->%s))", p->sym->name,xsi_type(typ));
	    }
	    else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\tif (!soap_in_%s(soap, tag, a->%s, \"%s\"))", c_ident(p->info.typ),p->sym->name,xsi_type(typ));
	    }
	    else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      fprintf(fout,"\n\tif (!a->%s.soap_in(soap, tag, \"%s\"))", p->sym->name,xsi_type(typ));
	    }
	    else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\tif (!soap_in_%s(soap, tag, &a->%s, \"%s\"))", c_ident(p->info.typ),p->sym->name,xsi_type(typ));
	    }
           fprintf(fout,"\n\t\treturn NULL;");
           fprintf(fout, "\n\treturn a;\n}");
      }
      else
      { a=0;
        table = (Table *)typ->ref;
        for (t = table; t; t = t->prev)
	  for (p = t->list; p; p = p->next)
	  { if (!(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_transient(p->info.typ) && !is_repetition(p) && !is_template(p->info.typ))
	    { if (is_anytype(p))
	        p = p->next;
	      if (a==0)
	      { fprintf(fout,"\n\tshort soap_flag_%s = 1", p->sym->name);
	        a=1;
              }
	      else
	        fprintf(fout,", soap_flag_%s = 1", p->sym->name);
	    }
	  }
	  fprintf(fout,";");
          fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 0))\n\t\treturn NULL;");
          fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type))");
          fprintf(fout,"\n\t{\tsoap->error = SOAP_TYPE;");
          fprintf(fout,"\n\t\treturn NULL;\n\t}");
	  if (has_class(typ))
            fprintf(fout,"\n\ta = (%s)soap_class_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), soap->type, soap->arrayType);",c_type_id(typ, "*"), c_ident(typ), c_type(typ));
	  else
            fprintf(fout,"\n\ta = (%s)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);",c_type_id(typ, "*"), c_ident(typ), c_type(typ));
	  fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
          fprintf(fout,"\n\tif (soap->alloced)\n\t\tsoap_default_%s(soap, a);",c_ident(typ));
          for (t = table; t; t = t->prev)
          { for (p = t->list; p; p = p->next) 
	      if (p->info.sto & Sattribute)
		soap_attr_value(p, "a", p->sym->name, ns_add(p->sym->name, nsa));
	  }
        fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{\tfor (;;)\n\t\t{\tsoap->error = SOAP_TAG_MISMATCH;");
        a=0;
        for (t = table; t; t = t->prev)
	{ for (p = t->list; p; p = p->next) 
	    if (p->info.sto & Sconst)
	      fprintf(fout, "\n\t\t/* const %s skipped */", p->sym->name);
	    else if (is_transient(p->info.typ))
	      fprintf(fout, "\n\t\t/* transient %s skipped */", p->sym->name);
	    else if (p->info.sto & Sattribute)
	      ;
	    else if (is_repetition(p))
	    { 
    fprintf(fout,"\n\t\t\tif (soap_flag_%s && soap->error == SOAP_TAG_MISMATCH)",p->next->sym->name);
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass || has_class(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t{\t%s;\n\t\t\t\t%s;\n\t\t\t\tsoap_new_block(soap);", c_type_id(p->next->info.typ, "p"), c_type_id(p->next->info.typ->ref, "q"));
    else
      fprintf(fout,"\n\t\t\t{\t%s;\n\t\t\t\tsoap_new_block(soap);", c_type_id(p->next->info.typ, "p"));
    /*
    */
    fprintf(fout,"\n\t\t\t\tfor (a->%s = 0; !soap_element_begin_in(soap, \"%s\", 1); a->%s++)", p->sym->name,ns_add(p->next->sym->name, nse), p->sym->name);
    fprintf(fout,"\n\t\t\t\t{\tp = (%s)soap_push_block(soap, sizeof(%s));", c_type(p->next->info.typ), c_type(p->next->info.typ->ref));
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass || has_class(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tmemcpy(p, &q, sizeof(%s));", c_type(p->next->info.typ->ref));
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tp->soap_default(soap);");
    else if (((Tnode*)p->next->info.typ->ref)->type != Tpointer  && !is_XML(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tsoap_default_%s(soap, p);", c_ident(p->next->info.typ->ref));
    else
      fprintf(fout,"\n\t\t\t\t\t*p = NULL;");
    fprintf(fout,"\n\t\t\t\t\tsoap_revert(soap);");
    if (is_XML(p->next->info.typ->ref) && is_string(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tif (soap_inliteral(soap, \"%s\", p))", ns_add(p->next->sym->name, nse));
    else if (is_XML(p->next->info.typ->ref) && is_wstring(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tif (soap_inwliteral(soap, \"%s\", p))", ns_add(p->next->sym->name, nse));
    else
      fprintf(fout,"\n\t\t\t\t\tif (!soap_in_%s(soap, \"%s\", p, \"%s\"))", c_ident(p->next->info.typ->ref), ns_add(p->next->sym->name, nse), xsi_type(p->next->info.typ->ref));
    fprintf(fout,"\n\t\t\t\t\t\tbreak;");
    fprintf(fout,"\n\t\t\t\t\tsoap_flag_%s = 0;", p->next->sym->name);
    fprintf(fout,"\n\t\t\t\t}");
    if (((Tnode*)p->next->info.typ->ref)->type == Tpointer)
      fprintf(fout,"\n\t\t\t\ta->%s = (%s)soap_save_block(soap, NULL, 0);", p->next->sym->name, c_type(p->next->info.typ));
    else
      fprintf(fout,"\n\t\t\t\ta->%s = (%s)soap_save_block(soap, NULL, 1);", p->next->sym->name, c_type(p->next->info.typ));
    fprintf(fout,"\n\t\t\t\tif (!soap_flag_%s && soap->error == SOAP_TAG_MISMATCH)\n\t\t\t\t\tcontinue;\n\t\t\t}", p->next->sym->name);
          p = p->next;
	  }
	  else if (is_anytype(p))
          { fprintf(fout,"\n\t\t\tif (soap_flag_%s && soap->error == SOAP_TAG_MISMATCH)",p->next->sym->name);
	    fprintf(fout,"\n\t\t\t\tif ((a->%s = soap_getelement(soap, &a->%s)))", p->next->sym->name, p->sym->name);
	    fprintf(fout,"\n\t\t\t\t{\tsoap_flag_%s = 0;", p->next->sym->name);
	    fprintf(fout,"\n\t\t\t\t\tcontinue;");
	    fprintf(fout,"\n\t\t\t\t}");
            p = p->next;
	  }
	  else
	  { 
	   if (is_template(p->info.typ))
	     fprintf(fout,"\n\t\t\tif (soap->error == SOAP_TAG_MISMATCH)");
	   else if (!is_primclass(typ) && p->info.typ->type != Tfun && !is_void(p->info.typ))
	     fprintf(fout,"\n\t\t\tif (soap_flag_%s && soap->error == SOAP_TAG_MISMATCH)",p->sym->name);
	   if (is_unmatched(p->sym))
	   {
	    if (is_XML(p->info.typ) && is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inliteral(soap, NULL, &a->%s))", p->sym->name);
	    } else if (is_XML(p->info.typ) && is_wstring(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inwliteral(soap, NULL, &a->%s))", p->sym->name);
	    } else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, NULL, a->%s, \"%s\"))", c_ident(p->info.typ),p->sym->name,xsi_type(p->info.typ));
	    } else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (a->%s.soap_in(soap, NULL, \"%s\"))", p->sym->name,xsi_type(p->info.typ));
	    } else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, NULL, &a->%s, \"%s\"))", c_ident(p->info.typ),p->sym->name,xsi_type(p->info.typ));
	    }
	   }
	   else
	   {
	    if (is_XML(p->info.typ) && is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inliteral(soap, \"%s\", &a->%s))", ns_add(p->sym->name, nse), p->sym->name);
	    } else if (is_XML(p->info.typ) && is_wstring(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inwliteral(soap, \"%s\", &a->%s))", ns_add(p->sym->name, nse), p->sym->name);
	    } else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, \"%s\", a->%s, \"%s\"))", c_ident(p->info.typ),ns_add(p->sym->name, nse),p->sym->name,xsi_type(p->info.typ));
	    } else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (a->%s.soap_in(soap, \"%s\", \"%s\"))", p->sym->name,ns_add(p->sym->name, nse),xsi_type(p->info.typ));
	    } else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, \"%s\", &a->%s, \"%s\"))", c_ident(p->info.typ),ns_add(p->sym->name, nse),p->sym->name,xsi_type(p->info.typ));
	    }
	   }
	    if (is_template(p->info.typ))
	      fprintf(fout,"\n\t\t\t\t\tcontinue;");
	    else if (!is_primclass(typ) && p->info.typ->type != Tfun && !is_void(p->info.typ))
	    { fprintf(fout,"\n\t\t\t\t{\tsoap_flag_%s = 0;", p->sym->name);
	      fprintf(fout,"\n\t\t\t\t\tcontinue;");
	      fprintf(fout,"\n\t\t\t\t}");
	    }
	fflush(fout);
	}
      }
      fprintf(fout,"\n\t\t\tif (soap->error == SOAP_TAG_MISMATCH)");
      fprintf(fout,"\n\t\t\t\tsoap->error = soap_ignore_element(soap);");
      fprintf(fout,"\n\t\t\tif (soap->error == SOAP_NO_TAG)");
      fprintf(fout,"\n\t\t\t\tbreak;");
      fprintf(fout,"\n\t\t\tif (soap->error)\n\t\t\t\treturn NULL;");
      fprintf(fout,"\n\t\t}");
	a = 0;
	if (table)
	  for (p = table->list; p; p = p->next)
	  { if (p->info.minOccurs > 0 && !(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_transient(p->info.typ) && !is_template(p->info.typ) && !is_repetition(p) && p->info.hasval == False)
	    { if (is_anytype(p))
	        p = p->next;
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (soap_flag_%s", p->sym->name);
	        a=1;
              }
	      else
	        fprintf(fout," || soap_flag_%s", p->sym->name);
	    }
	    else if (p->info.minOccurs > 1 && is_template(p->info.typ))
	    { char *s;
	      if (p->info.typ->type == Tpointer)
	        s = "->";
              else
		s = ".";
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (a->%s%ssize() < %ld", p->sym->name, s, p->info.minOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || a->%s%ssize() < %ld", p->sym->name, s, p->info.minOccurs);
	    }
	    else if (p->info.maxOccurs > 1 && is_template(p->info.typ))
	    { char *s;
	      if (p->info.typ->type == Tpointer)
	        s = "->";
              else
		s = ".";
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (a->%s%ssize() > %ld", p->sym->name, s, p->info.maxOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || a->%s%ssize() > %ld", p->sym->name, s, p->info.maxOccurs);
	    }
	    else if (p->info.minOccurs > 1 && is_repetition(p))
	    { if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (a->%s < %ld", p->sym->name, p->info.minOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || a->%s < %ld", p->sym->name, p->info.minOccurs);
	      p = p->next;
	    }
	    else if (p->info.maxOccurs > 1 && is_repetition(p))
	    { if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (a->%s > %ld", p->sym->name, p->info.maxOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || a->%s > %ld", p->sym->name, p->info.maxOccurs);
	      p = p->next;
	    }
	    else if (is_repetition(p))
	      p = p->next;
	  }
	  if (a)
	    fprintf(fout,"))\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}");
          fprintf(fout,"\n\t\tif (soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
          fprintf(fout,"\n\t}\n\telse\n\t{\t");
	  if (has_class(typ))
            fprintf(fout,"a = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, soap_copy_%s);",c_type_id(typ, "*"), c_ident(typ), c_type(typ), c_ident(typ));
	  else
            fprintf(fout,"a = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, NULL);",c_type_id(typ, "*"), c_ident(typ), c_type(typ));
          fprintf(fout,"\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
          fprintf(fout, "\n\t}\n\treturn a;\n}");
      }
      break;
    
     case Tclass:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
      if (!is_volatile(typ))
      { fprintf(fout,"\n\nvoid *%s::soap_in(struct soap *soap, const char *tag, const char *type)", c_type(typ));
	fprintf(fout,"\n{\treturn soap_in_%s(soap, tag, this, type);\n}",c_ident(typ));
        fflush(fout);
      }
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a"));
      /*fprintf(fout,"\n\tvoid *p = soap->alist;");
      fprintf(fout,"\n\tshort f;");
      */
      if (is_primclass(typ))
      {
        fprintf(fout, "\n\tif (soap_peek_element(soap))\n\t\treturn NULL;");
        fprintf(fout,"\n\tif (!(a = (%s)soap_class_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), soap->type, soap->arrayType)))\n\t{\tsoap->error = SOAP_TAG_MISMATCH;\n\t\treturn NULL;\n\t}", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
        fprintf(fout,"\n\t*soap->id = '\\0';");
        fprintf(fout,"\n\tif (soap->alloced)");
        fprintf(fout,"\n\t{\ta->soap_default(soap);",c_ident(typ));
        fprintf(fout,"\n\t\tif (soap->clist->type != SOAP_TYPE_%s)", c_ident(typ));
        fprintf(fout,"\n\t\t\treturn (%s)a->soap_in(soap, tag, type);", c_type_id(typ, "*"));
        fprintf(fout,"\n\t}");
          for (t = (Table*)typ->ref; t; t = t->prev)
          { for (p = t->list; p; p = p->next) 
	      if (p->info.sto & Sattribute)
		soap_attr_value(p, ptr_cast(typ, "a"), p->sym->name, ns_add(p->sym->name, nsa));
	  }
      fflush(fout);
	for (table = (Table*)typ->ref; table; table = table->prev)
	{ p = table->list;
	  if (p && is_item(p))
	    break;
        }
	    if (is_XML(p->info.typ) && is_string(p->info.typ))
	    { fprintf(fout,"\n\tif (!soap_inliteral(soap, tag, &(((%s*)a)->%s)))", table->sym->name,p->sym->name,xsi_type(typ));
	    }
	    else if (is_XML(p->info.typ) && is_wstring(p->info.typ))
	    { fprintf(fout,"\n\tif (!soap_inwliteral(soap, tag, &(((%s*)a)->%s)))", table->sym->name,p->sym->name,xsi_type(typ));
	    }
	    else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\tif (!soap_in_%s(soap, tag, ((%s*)a)->%s, \"%s\"))",
		      c_ident(p->info.typ),table->sym->name,p->sym->name,xsi_type(typ));
	    }
	    else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      fprintf(fout,"\n\tif (!(((%s*)a)->%s).soap_in(soap, tag, \"%s\"))",
		     table->sym->name,p->sym->name,xsi_type(typ));
	    }
	    else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\tif (!soap_in_%s(soap, tag, &(((%s*)a)->%s), \"%s\"))",
		      c_ident(p->info.typ),table->sym->name,p->sym->name,xsi_type(typ));
	    }
            /*fprintf(fout,"\n\t\tif (soap->error)\n\t\t\t{\tif (f)\n\t\t\t\t\tdelete a;\n\t\t\t\tsoap_dealloc(soap, * p);\n\t\t\t\treturn NULL;\n\t\t\t}");*/
           fprintf(fout,"\n\t\treturn NULL;");
           if (has_getter(typ))
             fprintf(fout,"\n\ta->get(soap);");
           fprintf(fout,"\n\treturn a;\n}");
      }
      else
      { fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 0))\n\t\treturn NULL;");
        fprintf(fout,"\n\ta = (%s)soap_class_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), soap->type, soap->arrayType);", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
        fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
        fprintf(fout,"\n\tif (soap->alloced)");
	if (is_volatile(typ))
          fprintf(fout,"\n\t{\tsoap_default_%s(soap, a);",c_ident(typ));
	else
          fprintf(fout,"\n\t{\ta->soap_default(soap);");
        fprintf(fout,"\n\t\tif (soap->clist->type != SOAP_TYPE_%s)", c_ident(typ));
        fprintf(fout,"\n\t\t{\tsoap_revert(soap);");
        fprintf(fout,"\n\t\t\t*soap->id = '\\0';");
	if (is_volatile(typ))
          fprintf(fout,"\n\t\t\treturn soap_in_%s(soap, tag, a, type);", c_ident(typ));
	else
          fprintf(fout,"\n\t\t\treturn (%s)a->soap_in(soap, tag, type);", c_type_id(typ, "*"));
        fprintf(fout,"\n\t\t}\n\t}");
        table=(Table *)typ->ref;
        for (t = table; t; t = t->prev)
        { for (p = t->list; p; p = p->next) 
	    if (p->info.sto & Sattribute)
              soap_attr_value(p, ptr_cast(typ, "a"), p->sym->name, ns_add(p->sym->name, nsa));
	}
        fflush(fout);
       
      i=0;
      for (t = table; t; t = t->prev)
	i++;
      a=0;
      for(;i>0;i--){
	t = table;
	for(j=0;j<i-1;j++){
	  t = t->prev;
	}
	{for (p = t->list; p != (Entry*) 0; p = p->next)
	  { if (!(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_transient(p->info.typ) && !is_repetition(p) && !is_template(p->info.typ))
	    { if (is_item(p))
	        continue;
	      if (is_anytype(p))
	        p = p->next;
	      if( a==0)
	      { fprintf(fout,"\n\tshort soap_flag_%s%d = 1", p->sym->name, i );
	        a = 1;
              }
	      else
	        fprintf(fout,", soap_flag_%s%d = 1", p->sym->name, i );
	    }
	  }
        }
      }
      fprintf(fout,";"); 
      fflush(fout);
      fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{\tfor (;;)\n\t\t{\tsoap->error = SOAP_TAG_MISMATCH;"); 
      table=(Table *)typ->ref;
      a=0;
      i=0;
      for (t = table; t; t = t->prev)
	i++;
      for(;i>0;i--){
	t = table;
	for(j=0;j<i-1;j++){
	  t = t->prev;
	}
	for (p = t->list; p != (Entry*) 0; p = p->next)
	  if (is_item(p))
	    ;
	  else if (p->info.sto & Sconst)
	    fprintf(fout, "\n\t\t\t/* const %s skipped */", p->sym->name);
	  else if (is_transient(p->info.typ))
	    fprintf(fout, "\n\t\t\t/* transient %s skipped */", p->sym->name);
	  else if (p->info.sto & Sattribute)
	    ;
	  else if (is_repetition(p))
	  { 
    fprintf(fout,"\n\t\t\tif (soap_flag_%s%d && soap->error == SOAP_TAG_MISMATCH)",p->next->sym->name,i);
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass || has_class(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t{\t%s;\n\t\t\t\t%s;\n\t\t\t\tsoap_new_block(soap);", c_type_id(p->next->info.typ, "p"), c_type_id(p->next->info.typ->ref, "q"));
    else
      fprintf(fout,"\n\t\t\t{\t%s;\n\t\t\t\tsoap_new_block(soap);", c_type_id(p->next->info.typ, "p"));
    /*
     if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tq.soap_default(soap);");
    */
    fprintf(fout,"\n\t\t\t\tfor (((%s*)a)->%s = 0; !soap_element_begin_in(soap, \"%s\", 1); ((%s*)a)->%s++)", t->sym->name, p->sym->name, ns_add_overridden(t, p->next, nse), t->sym->name, p->sym->name);
    fprintf(fout,"\n\t\t\t\t{\tp = (%s)soap_push_block(soap, sizeof(%s));\n\t\t\t\t\tif (!p)\n\t\t\t\t\t\treturn NULL;", c_type(p->next->info.typ), c_type(p->next->info.typ->ref));
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass || has_class(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tmemcpy(p, &q, sizeof(%s));", c_type(p->next->info.typ->ref));
    if (((Tnode*)p->next->info.typ->ref)->type == Tclass && !is_external(p->next->info.typ->ref) && !is_volatile(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tp->soap_default(soap);");
    else if (((Tnode*)p->next->info.typ->ref)->type != Tpointer  && !is_XML(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tsoap_default_%s(soap, p);", c_ident(p->next->info.typ->ref));
    else
      fprintf(fout,"\n\t\t\t\t\t*p = NULL;");
    fprintf(fout,"\n\t\t\t\t\tsoap_revert(soap);");
    if (is_XML(p->next->info.typ->ref) && is_string(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tif (soap_inliteral(soap, \"%s\", p))", ns_add_overridden(t, p->next, nse));
    else if (is_XML(p->next->info.typ->ref) && is_wstring(p->next->info.typ->ref))
      fprintf(fout,"\n\t\t\t\t\tif (soap_inwliteral(soap, \"%s\", p))", ns_add_overridden(t, p->next, nse));
    else
      fprintf(fout,"\n\t\t\t\t\tif (!soap_in_%s(soap, \"%s\", p, \"%s\"))", c_ident(p->next->info.typ->ref), ns_add_overridden(t, p->next, nse), xsi_type(p->next->info.typ->ref));
    fprintf(fout,"\n\t\t\t\t\t\tbreak;");
    fprintf(fout,"\n\t\t\t\t\tsoap_flag_%s%d = 0;", p->next->sym->name, i);
    fprintf(fout,"\n\t\t\t\t}");
    if (((Tnode*)p->next->info.typ->ref)->type == Tpointer)
      fprintf(fout,"\n\t\t\t\t((%s*)a)->%s = (%s)soap_save_block(soap, NULL, 0);", t->sym->name, p->next->sym->name, c_type(p->next->info.typ));
    else
      fprintf(fout,"\n\t\t\t\t((%s*)a)->%s = (%s)soap_save_block(soap, NULL, 1);", t->sym->name, p->next->sym->name, c_type(p->next->info.typ));
    fprintf(fout,"\n\t\t\t\tif (!soap_flag_%s%d && soap->error == SOAP_TAG_MISMATCH)\n\t\t\t\t\tcontinue;\n\t\t\t}", p->next->sym->name, i);
          p = p->next;
	  }
	  else if (is_anytype(p))
          { fprintf(fout,"\n\t\t\tif (soap_flag_%s%d && soap->error == SOAP_TAG_MISMATCH)",p->next->sym->name,i);
	    fprintf(fout,"\n\t\t\t\tif ((((%s*)a)->%s = soap_getelement(soap, &((%s*)a)->%s)))", t->sym->name, p->next->sym->name, t->sym->name, p->sym->name);
	    fprintf(fout,"\n\t\t\t\t{\tsoap_flag_%s%d = 0;", p->next->sym->name,i);
	    fprintf(fout,"\n\t\t\t\t\tcontinue;");
	    fprintf(fout,"\n\t\t\t\t}");
            p = p->next;
	  }
	  else
	  {
	    if (is_template(p->info.typ))
	      fprintf(fout,"\n\t\t\tif (soap->error == SOAP_TAG_MISMATCH)");
	    else if (!is_primclass(typ) && p->info.typ->type != Tfun && !is_void(p->info.typ))
	      fprintf(fout,"\n\t\t\tif (soap_flag_%s%d && soap->error == SOAP_TAG_MISMATCH)",p->sym->name,i);
	   if (is_unmatched(p->sym))
	   { 
	    if (is_XML(p->info.typ) && is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inliteral(soap, NULL, &(((%s*)a)->%s)))", t->sym->name, p->sym->name);
	    } else if (is_XML(p->info.typ) && is_wstring(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inwliteral(soap, NULL, &(((%s*)a)->%s)))", t->sym->name, p->sym->name);
	    }
	    /*else if (is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_string(soap, NULL, &(((%s*)a)->%s), \"%s\"))", t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    } else if (is_wstring(p->info.typ))
	    { fprintf(fout,"\n\t\t\t\tif (soap_in_wstring(soap, NULL, &(((%s*)a)->%s), \"%s\"))", t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    }*/
	    else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, NULL, ((%s*)a)->%s, \"%s\"))",
		      c_ident(p->info.typ),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    } else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      /*CLASS IN CLASS */
	      fprintf(fout,"\n\t\t\t\tif ((((%s*)a)->%s).soap_in(soap, NULL, \"%s\"))", t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    } else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, NULL, &(((%s*)a)->%s), \"%s\"))",
		      c_ident(p->info.typ),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    }
           }
	   else
	   { 
	    if (is_XML(p->info.typ) && is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inliteral(soap, \"%s\", &(((%s*)a)->%s)))", ns_add_overridden(t, p, nse), t->sym->name,p->sym->name);
	    } else if (is_XML(p->info.typ) && is_wstring(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_inwliteral(soap, \"%s\", &(((%s*)a)->%s)))", ns_add_overridden(t, p, nse), t->sym->name,p->sym->name);
	    }/*
	    else if (is_string(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_string(soap, \"%s\", &(((%s*)a)->%s), \"%s\"))", ns_overridden(t, p),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    } else if (is_wstring(p->info.typ))
	    { fprintf(fout,"\n\t\t\t\tif (soap_in_wstring(soap, \"%s\", &(((%s*)a)->%s), \"%s\"))", ns_overridden(t, p),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    }
	    */
	    else if(p->info.typ->type==Tarray) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, \"%s\", ((%s*)a)->%s, \"%s\"))", c_ident(p->info.typ),ns_add_overridden(t, p, nse),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    } else if(p->info.typ->type==Tclass && !is_external(p->info.typ) && !is_volatile(p->info.typ)) {
	      /*CLASS IN CLASS */
	      fprintf(fout,"\n\t\t\t\tif ((((%s*)a)->%s).soap_in(soap, \"%s\", \"%s\"))", t->sym->name,p->sym->name,ns_add_overridden(t, p, nse),xsi_type(p->info.typ));
	    } else if (p->info.typ->type != Tfun && !is_void(p->info.typ)) {
	      fprintf(fout,"\n\t\t\t\tif (soap_in_%s(soap, \"%s\", &(((%s*)a)->%s), \"%s\"))", c_ident(p->info.typ),ns_add_overridden(t, p, nse),t->sym->name,p->sym->name,xsi_type(p->info.typ));
	    }
           }
	    a=1;
	    	    
	    if (is_template(p->info.typ))
	      fprintf(fout,"\n\t\t\t\t\tcontinue;");
	    else if (!is_primclass(typ) && p->info.typ->type != Tfun && !is_void(p->info.typ))
	    { fprintf(fout,"\n\t\t\t\t{\tsoap_flag_%s%d = 0;", p->sym->name, i);
	      fprintf(fout,"\n\t\t\t\t\tcontinue;");
	      fprintf(fout,"\n\t\t\t\t}");
	    }
	fflush(fout);
	  }
	
      }
        fprintf(fout,"\n\t\t\tif (soap->error == SOAP_TAG_MISMATCH)");
        fprintf(fout,"\n\t\t\t\tsoap->error = soap_ignore_element(soap);");
        fprintf(fout,"\n\t\t\tif (soap->error == SOAP_NO_TAG)");
        fprintf(fout,"\n\t\t\t\tbreak;");
      fprintf(fout,"\n\t\t\tif (soap->error)\n\t\t\treturn NULL;");
      fprintf(fout,"\n\t\t}");
      i=0;
      for (t = table; t; t = t->prev)
	i++;
      a=0;
      for (; i > 0; i--)
      { t = table;
	for (j = 0; j < i-1; j++)
	  t = t->prev;
	for (p = t->list; p; p = p->next)
	  { if (p->info.minOccurs && !(p->info.sto & Sconst) && !(p->info.sto & Sattribute) && p->info.typ->type != Tfun && !is_void(p->info.typ) && !is_transient(p->info.typ) && !is_template(p->info.typ) && !is_repetition(p) && p->info.hasval == False)
	    { if (is_item(p))
	        continue;
	      if (is_anytype(p))
	        p = p->next;
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (soap_flag_%s%d", p->sym->name, i);
	        a=1;
              }
	      else
	        fprintf(fout," || soap_flag_%s%d", p->sym->name, i);
	    }
	    else if (p->info.minOccurs > 1 && is_template(p->info.typ))
	    { char *s;
	      if (p->info.typ->type == Tpointer)
	        s = "->";
              else
		s = ".";
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (((%s*)a)->%s%ssize() < %ld", t->sym->name, p->sym->name, s, p->info.minOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || ((%s*)a)->%s%ssize() < %ld", t->sym->name, p->sym->name, s, p->info.minOccurs);
	    }
	    else if ( p->info.maxOccurs > 1 && is_template(p->info.typ))
	    { char *s;
	      if (p->info.typ->type == Tpointer)
	        s = "->";
              else
		s = ".";
	      if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (((%s*)a)->%s%ssize() > %ld", t->sym->name, p->sym->name, s, p->info.maxOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || ((%s*)a)->%s%ssize() > %ld", t->sym->name, p->sym->name, s, p->info.maxOccurs);
	    }
	    else if (p->info.minOccurs > 1 && is_repetition(p))
	    { if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (((%s*)a)->%s < %ld", t->sym->name, p->sym->name, p->info.minOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || ((%s*)a)->%s < %ld", t->sym->name, p->sym->name, p->info.minOccurs);
	      p = p->next;
	    }
	    else if (p->info.maxOccurs > 1 && is_repetition(p))
	    { if (a==0)
	      { fprintf(fout,"\n\t\tif ((soap->mode & SOAP_XML_STRICT) && (((%s*)a)->%s > %ld", t->sym->name, p->sym->name, p->info.maxOccurs);
	        a=1;
              }
	      else
	        fprintf(fout," || ((%s*)a)->%s > %ld", t->sym->name, p->sym->name, p->info.maxOccurs);
	      p = p->next;
	    }
	    else if (is_repetition(p))
	      p = p->next;
	  }
	}
	if (a)
	  fprintf(fout,"))\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}");
      if (has_getter(typ))
        fprintf(fout,"\n\t\ta->get(soap);");
      fprintf(fout, "\n\t\tif (soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
      fprintf(fout,"\n\t}\n\telse\n\t{\ta = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, soap_copy_%s);",c_type_id(typ, "*"), c_ident(typ), c_type(typ), c_ident(typ));
      fprintf(fout, "\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
      fprintf(fout, "\n\t}");
      fprintf(fout,"\n\treturn a;\n}");
      }

      break;   
           
    case Tpointer:
      
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a"));
      fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 1))");
      fprintf(fout,"\n\t\treturn NULL;");

      if (is_template(typ))
      { fprintf(fout,"\n\tsoap_revert(soap);");
	fprintf(fout,"\n\tif (!a)\n\t\tif (!(a = (%s)soap_malloc(soap, sizeof(%s))))\n\t\t\treturn NULL;", c_type_id(typ, "*"), c_type(typ));
	fprintf(fout,"\n\tif (!(*a = soap_in_%s(soap, tag, *a, type)))\n\t\treturn NULL;", c_ident(typ->ref));
	fprintf(fout,"\n\treturn a;\n}");
      }
      else if(((Tnode *) typ->ref)->type == Tclass && !is_external(typ->ref) && !is_volatile(typ->ref))
      {
	fprintf(fout,"\n\tif (!a)\n\t\tif (!(a = (%s)soap_malloc(soap, sizeof(%s))))\n\t\t\treturn NULL;", c_type_id(typ, "*"), c_type(typ));
	fprintf(fout,"\n\t*a = NULL;\n\tif (!soap->null && *soap->href != '#')");
	fprintf(fout,"\n\t{\tsoap_revert(soap);");
	fprintf(fout, "\n\t\tif (!(*a = (%s)soap_instantiate_%s(soap, -1, soap->type, soap->arrayType, NULL)))", c_type(typ), c_ident(typ->ref));
	fprintf(fout, "\n\t\t\treturn NULL;");
	fprintf(fout, "\n\t\t(*a)->soap_default(soap);");
	fprintf(fout, "\n\t\tif (!(*a)->soap_in(soap, tag, NULL))"); 
	fprintf(fout, "\n\t\t\treturn NULL;");
	fprintf(fout,"\n\t}\n\telse\n\t{\ta = (%s)soap_id_lookup(soap, soap->href, (void**)a, SOAP_TYPE_%s, sizeof(%s), %d);", c_type_id(typ, "*"), c_ident(typ->ref), c_type(typ->ref), reflevel(typ->ref) );
	fprintf(fout,"\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
	fprintf(fout,"\n\t}\n\treturn a;\n}");
      }
      else
      {
	fprintf(fout,"\n\tif (!a)\n\t\tif (!(a = (%s)soap_malloc(soap, sizeof(%s))))\n\t\t\treturn NULL;", c_type_id(typ, "*"), c_type(typ));
	fprintf(fout,"\n\t*a = NULL;\n\tif (!soap->null && *soap->href != '#')");
	fprintf(fout,"\n\t{\tsoap_revert(soap);");
	fprintf(fout,"\n\t\tif (!(*a = soap_in_%s(soap, tag, *a, type)))", c_ident(typ->ref));
	fprintf(fout,"\n\t\t\treturn NULL;");

	fprintf(fout,"\n\t}\n\telse\n\t{\ta = (%s)soap_id_lookup(soap, soap->href, (void**)a, SOAP_TYPE_%s, sizeof(%s), %d);", c_type_id(typ, "*"), c_ident(typ->ref), c_type(typ->ref), reflevel(typ->ref) );
	fprintf(fout,"\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
	fprintf(fout,"\n\t}\n\treturn a;\n}");
      }
    
      break;
  
    case Tarray:
      temp = typ;
      while(temp->type == Tarray){
	temp = temp->ref;
      }
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);",c_type_id(temp, "*"),c_ident(typ),c_type(typ));  
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);",c_type_id(temp, "*"),c_ident(typ),c_type(typ));  
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{",c_type_id(temp, "*"),c_ident(typ),c_type_id(typ, "a"));  
      fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 0))");
      fprintf(fout,"\n\t\treturn NULL;");
      fprintf(fout,"\n\tif (soap_match_array(soap, type))");
      fprintf(fout,"\n\t{\tsoap->error = SOAP_TYPE;\n\t\treturn NULL;\n\t}");
      fprintf(fout,"\n\ta = (%s)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);", c_type_id(typ->ref, "*"), c_ident(typ), c_type(typ));
      fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
      fprintf(fout,"\n\tsoap_default_%s(soap, a);",c_ident(typ));
      fprintf(fout,"\n\tif (soap->body && !*soap->href)");
      total=get_dimension(typ);  
      n=typ->ref;
      cardinality = 1;
      while(n->type==Tarray)
	{
	  total=total*get_dimension(n);
	  n = n->ref;
	  cardinality++;
	}
      fprintf(fout,"\n\t{\tint i;\n\t\tfor (i = 0; i < %d; i++)",get_dimension(typ));
  fprintf(fout,"\n\t\t{\tsoap_peek_element(soap);\n\t\t\tif (soap->position)\n\t\t\t{\ti = soap->positions[0];\n\t\t\t\tif (i < 0 || i >= %d)\n\t\t\t\t{\tsoap->error = SOAP_IOB;\n\t\t\t\t\treturn NULL;\n\t\t\t\t}\n\t\t\t}", get_dimension(typ));
	fprintf(fout,"\n\t\t\tif (!soap_in_%s(soap, NULL, a", c_ident(typ->ref));

      if(cardinality > 1){
	fprintf(fout,"[i]");
      }else {
	fprintf(fout,"+i");
      }
      fprintf(fout,", \"%s\"))", xsi_type(typ->ref));
      fprintf(fout,"\n\t\t\t{\tif (soap->error != SOAP_NO_TAG)\n\t\t\t\t\treturn NULL;");
      fprintf(fout,"\n\t\t\t\tsoap->error = SOAP_OK;");
      fprintf(fout,"\n\t\t\t\tbreak;");
      fprintf(fout,"\n\t\t\t}");
      fprintf(fout,"\n\t\t}");
      fprintf(fout,"\n\t\tif (soap->mode & SOAP_C_NOIOB)\n\t\t\twhile (soap_element_end_in(soap, tag) == SOAP_SYNTAX_ERROR)\n\t\t\t{\tsoap->peeked = 1;\n\t\t\t\tsoap_ignore_element(soap);\n\t\t\t}");
      fprintf(fout,"\n\t\telse if (soap_element_end_in(soap, tag))\n\t\t{\tif (soap->error == SOAP_SYNTAX_ERROR)\n\t\t\t\tsoap->error = SOAP_IOB;\n\t\t\treturn NULL;\n\t\t}");
      fprintf(fout,"\n\t}\n\telse\n\t{\ta = (%s)soap_id_forward(soap, soap->href, (void**)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL), SOAP_TYPE_%s, 0, sizeof(%s), 0, NULL);", c_type_id(typ->ref, "*"), c_ident(typ), c_type(typ), c_ident(typ), c_type(typ));
      fprintf(fout,"\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
      fprintf(fout,"\n\t}\n\treturn (%s)a;\n}", c_type_id(temp, "*"));
      break;

    case Tenum:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);",c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));  
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);",c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));  
      if (!is_mask(typ))
      { fprintf(fhead,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_s2%s(struct soap*, const char*, %s);",c_ident(typ),c_type_id(typ, "*"));  
        fprintf(fout,"\n\nSOAP_FMAC3 int SOAP_FMAC4 soap_s2%s(struct soap *soap, const char *s, %s)\n{\n\t",c_ident(typ),c_type_id(typ, "*a"));  
	fprintf(fout, "const struct soap_code_map *map;");
	fprintf(fout, "\n\tif (!s)\n\t\treturn SOAP_OK;");
	fprintf(fout, "\n\tmap = soap_code(soap_codes_%s, s);", c_ident(typ));
	fprintf(fout, "\n\tif (map)\n\t\t*a = (%s)map->code;\n\telse if (soap->mode & SOAP_XML_STRICT)\n\t\treturn soap->error = SOAP_TYPE;\n\telse\n\t{\tlong n;\n\t\tif (soap_s2long(soap, s, &n))\n\t\t\treturn soap->error;\n\t\t*a = (%s)n;\n\t}\n\treturn SOAP_OK;\n}", c_type(typ), c_type(typ));
      }
      fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{",c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a"));  
      if (is_mask(typ))
        fprintf(fout,"\n\tconst char *s;\n\tLONG64 i;");
      fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 0))");
      fprintf(fout,"\n\t\treturn NULL;");
      if (typ->sym)
      { fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type) && soap_match_tag(soap, soap->type, \"%s\"))", base_type(typ, ""));
      }
      else
        fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type))");
      fprintf(fout,"\n\t{\tsoap->error = SOAP_TYPE;\n\t\treturn NULL;\n\t}");
      fprintf(fout,"\n\ta = (%s)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
      fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
      fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{");
      if (is_mask(typ))
      { fprintf(fout,"\ti = 0;\n\t\twhile (*(s = soap_value(soap)))\n\t\t\t");
        for (t = (Table*)typ->ref; t; t = t->prev)
        { for (p = t->list; p; p = p->next)
	    fprintf(fout, "if (!strcmp(s, \"%s\"))\n\t\t\t\ti |= (LONG64)%s;\n\t\t\telse ", ns_remove2(p->sym->name), p->sym->name);
        }	
        fprintf(fout, "\n\t\t\t{\tsoap->error = SOAP_TYPE;\n\t\t\t\treturn NULL;\n\t\t\t}");
        fprintf(fout, "\n\t\t*a = (%s)i;", c_type(typ));
        fprintf(fout, "\n\t\tif (soap_element_end_in(soap, tag))\n\t\t\treturn NULL;", c_type(typ));
      }
      else
        fprintf(fout,"\tif (!a || soap_s2%s(soap, soap_value(soap), a) || soap_element_end_in(soap, tag))\n\t\t\treturn NULL;", c_ident(typ));
      fprintf(fout, "\n\t}\n\telse\n\t{\ta = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, NULL);", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
      fprintf(fout, "\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
      fprintf(fout,"\n\t}\n\treturn a;\n}");
      break;

    case Ttemplate:
      if (is_external(typ))
      { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*")); 
        return;
      }
      fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*")); 
      fprintf(fout, "\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)\n{", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a")); 
      n = typ->ref;
      fprintf(fout, "\n\tif (soap_element_begin_in(soap, tag, 1))\n\t\treturn NULL;");
      fprintf(fout, "\n\tif (!a && !(a = soap_new_%s(soap, -1)))\n\t\treturn NULL;", c_ident(typ));
      /* fprintf(fout, "\n\t%s::iterator i;\n\t;", c_type(typ)); */
      fprintf(fout, "\n\t%s;\n\t%s;", c_type_id(n, "n"), c_type_id(n, "*p"));
      fprintf(fout, "\n\tdo");
      fprintf(fout, "\n\t{\tsoap_revert(soap);");
      fprintf(fout, "\n\t\tif (*soap->id || *soap->href)");
      fprintf(fout, "\n\t\t{\tif (!soap_container_id_forward(soap, *soap->id?soap->id:soap->href, a, SOAP_TYPE_%s, SOAP_TYPE_%s, sizeof(%s), %d))\n\t\t\t\tbreak;\n\t\t\t", c_ident(reftype(n)), c_ident(typ), c_type(reftype(n)), reflevel(n));
      if (is_XML(n) && is_string(n))
        fprintf(fout, "if (!(p = soap_inliteral(soap, tag, NULL)))", xsi_type(n));
      else if (is_XML(n) && is_wstring(n))
        fprintf(fout, "if (!(p = soap_inwliteral(soap, tag, NULL)))", xsi_type(n));
      else if (n->type==Tarray)
        fprintf(fout, "if (!(p = soap_in_%s(soap, tag, NULL, \"%s\")))", c_ident(n),xsi_type(n));
      else if (n->type != Tfun && !is_void(n))
        fprintf(fout, "if (!(p = soap_in_%s(soap, tag, NULL, \"%s\")))", c_ident(n),xsi_type(n));
      fprintf(fout, "\n\t\t\t\tbreak;");

      /*
      fprintf(fout, "\n\t\t\ta->insert(a->end(), *p);");
      fprintf(fout, "\n\t\t}\n\t\telse if (*soap->href)\n\t\t{\t");
      if (is_XML(n) && is_string(n))
        fprintf(fout, "if (!(p = soap_inliteral(soap, tag, NULL)))", xsi_type(n));
      else if (is_XML(n) && is_wstring(n))
        fprintf(fout, "if (!(p = soap_inwliteral(soap, tag, NULL)))", xsi_type(n));
      else if (n->type==Tarray)
        fprintf(fout, "if (!(p = soap_in_%s(soap, tag, NULL, \"%s\")))", c_ident(n),xsi_type(n));
      else if (n->type != Tfun && !is_void(n))
        fprintf(fout, "if (!(p = soap_in_%s(soap, tag, NULL, \"%s\")))", c_ident(n),xsi_type(n));
      fprintf(fout, "\n\t\t\t\tbreak;");
      fprintf(fout, "\n\t\t\tif (!soap_container_id_forward(soap, soap->href, a, SOAP_TYPE_%s, SOAP_TYPE_%s, sizeof(%s), %d))\n\t\t\t\tbreak;", c_ident(reftype(n)), c_ident(typ), c_type(reftype(n)), reflevel(n));
      */
      fprintf(fout, "\n\t\t}\n\t\telse\n\t\t{\t");
      if (n->type == Tpointer)
	fprintf(fout,"n = NULL;");
      else if (n->type == Tarray)
	fprintf(fout,"soap_default_%s(soap, &n);", c_ident(n));
      else if(n->type==Tclass && !is_external(n) && !is_volatile(n))
	fprintf(fout,"n.soap_default(soap);");
      else if (n->type != Tfun && !is_void(n) && !is_XML(n))
        fprintf(fout,"soap_default_%s(soap, &n);", c_ident(n));
      if (is_XML(n) && is_string(n))
        fprintf(fout, "\n\t\t\tif (!soap_inliteral(soap, tag, &n))", xsi_type(n));
      else if (is_XML(n) && is_wstring(n))
        fprintf(fout, "\n\t\t\tif (!soap_inwliteral(soap, tag, &n))", xsi_type(n));
      else if (n->type==Tarray)
        fprintf(fout, "\n\t\t\tif (!soap_in_%s(soap, tag, &n, \"%s\"))", c_ident(n),xsi_type(n));
      else if (n->type != Tfun && !is_void(n))
        fprintf(fout, "\n\t\t\tif (!soap_in_%s(soap, tag, &n, \"%s\"))", c_ident(n),xsi_type(n));
      fprintf(fout, "\n\t\t\t\tbreak;");
      fprintf(fout, "\n\t\t\ta->insert(a->end(), n);");
      fprintf(fout, "\n\t\t}");
      /*
      fprintf(fout, "\n\t}\n\twhile (!soap_peek_element(soap));");
      */
      fprintf(fout, "\n\t}\n\twhile (!soap_element_begin_in(soap, tag, 1));");
      fprintf(fout, "\n\tif (soap->error == SOAP_TAG_MISMATCH || soap->error == SOAP_NO_TAG)\n\t{\tsoap->error = SOAP_OK;\n\t\treturn a;\n\t}\n\treturn NULL;\n}");
      break;
    default: break;
    }
  fflush(fout);
}


void
soap_in_Darray(Tnode *typ)
{ int i, j, d;
  Entry *p, *q;
  Table *t, *table;
  char *nsa = ns_qualifiedAttribute(typ);

  table=(Table *)typ->ref;
  q = table->list;
  p = is_dynamic_array(typ);
  d = get_Darraydims(typ);
  
  if (is_external(typ))
  { fprintf(fhead,"\nSOAP_FMAC1 %s SOAP_FMAC2 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
    return;
  }
  fprintf(fhead,"\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap*, const char*, %s, const char*);", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*"));
  if (typ->type == Tclass)
  { fprintf(fout,"\n\nvoid *%s::soap_in(struct soap *soap, const char *tag, const char *type)", c_type(typ));
    fprintf(fout,"\n{\treturn soap_in_%s(soap, tag, this, type);\n}", c_ident(typ));
  }
  fflush(fout);
  fprintf(fout,"\n\nSOAP_FMAC3 %s SOAP_FMAC4 soap_in_%s(struct soap *soap, const char *tag, %s, const char *type)", c_type_id(typ, "*"),c_ident(typ),c_type_id(typ, "*a"));
  if ((has_ns(typ) || is_untyped(typ)) && is_binary(typ))
    fprintf(fout,"\n{");
  else if (d)
    fprintf(fout,"\n{\tint i, j, n;\n\t%s;", c_type_id(p->info.typ, "p"));
  else
    fprintf(fout,"\n{\tint i, j;\n\t%s;", c_type_id(p->info.typ, "p"));
  fprintf(fout,"\n\tif (soap_element_begin_in(soap, tag, 1))\n\t\treturn NULL;");
  if (has_ns(typ) || is_untyped(typ))
    if (is_hexBinary(typ))
      fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type) && soap_match_tag(soap, soap->type, \":hexBinary\"))");
    else if (is_binary(typ))
      fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type) && soap_match_tag(soap, soap->type, \":base64Binary\") && soap_match_tag(soap, soap->type, \":base64\"))");
    else
      fprintf(fout,"\n\tif (*soap->type && soap_match_tag(soap, soap->type, type))");
  else
    fprintf(fout,"\n\tif (soap_match_array(soap, type))");
  fprintf(fout,"\n\t{\tsoap->error = SOAP_TYPE;\n\t\treturn NULL;\n\t}");
  if (typ->type == Tclass)
  { fprintf(fout,"\n\ta = (%s)soap_class_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), soap->type, soap->arrayType);",c_type_id(typ, "*"), c_ident(typ), c_type(typ)); 
    fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
    fprintf(fout,"\n\tif (soap->alloced)\n\t\ta->soap_default(soap);");
    for (t = (Table*)typ->ref; t; t = t->prev)
    { for (p = t->list; p; p = p->next) 
	if (p->info.sto & Sattribute)
          soap_attr_value(p, ptr_cast(typ, "a"), p->sym->name, ns_add(p->sym->name, nsa));
    }
  }
  else
  { fprintf(fout,"\n\ta = (%s)soap_id_enter(soap, soap->id, a, SOAP_TYPE_%s, sizeof(%s), 0, NULL, NULL, NULL);",c_type_id(typ, "*"), c_ident(typ), c_type(typ));
    fprintf(fout,"\n\tif (!a)\n\t\treturn NULL;");
    fprintf(fout,"\n\tif (soap->alloced)\n\t\tsoap_default_%s(soap, a);", c_ident(typ));
    for (t = (Table*)typ->ref; t; t = t->prev)
    { for (p = t->list; p; p = p->next) 
	if (p->info.sto & Sattribute)
          soap_attr_value(p, "a", p->sym->name, ns_add(p->sym->name, nsa));
    }
  }
  fprintf(fout,"\n\tif (soap->body && !*soap->href)\n\t{");
  p = is_dynamic_array(typ);
  if ((has_ns(typ) || is_untyped(typ)) && is_binary(typ))
  { if (is_hexBinary(typ))
      fprintf(fout,"\n\t\ta->__ptr = soap_gethex(soap, &a->__size);");
    else
      fprintf(fout,"\n\t\ta->__ptr = soap_getbase64(soap, &a->__size, 0);");
    fprintf(fout,"\n\t\tif ((!a->__ptr && soap->error) || soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
  }
  else
  { if (d)
    { fprintf(fout,"\n\t\tn = soap_getsizes(soap->arraySize, a->__size, %d);", d);
      if (has_offset(typ))
        fprintf(fout,"\n\t\tn -= j = soap_getoffsets(soap->arrayOffset, a->__size, a->__offset, %d);", d);
      else
        fprintf(fout,"\n\t\tn -= j = soap_getoffsets(soap->arrayOffset, a->__size, NULL, %d);", d);
      if (p->info.minOccurs > 1)
        fprintf(fout,"\n\t\tif (n >= 0 && n < %ld)\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}", p->info.minOccurs);
      if (p->info.maxOccurs > 1)
        fprintf(fout,"\n\t\tif (n > %ld)\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}", p->info.maxOccurs);
      fprintf(fout,"\n\t\tif (n >= 0)");
      if (((Tnode*)p->info.typ->ref)->type == Tclass)
      { fprintf(fout,"\n\t\t{\ta->%s = soap_new_%s(soap, n);", p->sym->name, c_ident(p->info.typ->ref));
        if (!is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
          fprintf(fout, "\n\t\t\tfor (i = 0; i < n; i++)\n\t\t\t\t(a->%s+i)->%s::soap_default(soap);", p->sym->name, c_type(p->info.typ->ref));
        else if (((Tnode*)p->info.typ->ref)->type == Tpointer)
          fprintf(fout, "\n\t\t\tfor (i = 0; i < n; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      else if (has_class(p->info.typ->ref))
      { fprintf(fout,"\n\t\t{\ta->%s = soap_new_%s(soap, n);", p->sym->name, c_ident(p->info.typ->ref));
        fprintf(fout, "\n\t\t\tfor (i = 0; i < n; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      else
      { fprintf(fout,"\n\t\t{\ta->%s = (%s)soap_malloc(soap, n*sizeof(%s));", p->sym->name, c_type_id(p->info.typ->ref, "*"),  c_type(p->info.typ->ref));
        if (((Tnode*)p->info.typ->ref)->type == Tpointer)
          fprintf(fout, "\n\t\t\tfor (i = 0; i < n; i++)\n\t\t\t\ta->%s[i] = NULL;", p->sym->name);
	else if (!is_XML(p->info.typ->ref))
          fprintf(fout, "\n\t\t\tfor (i = 0; i < n; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      fprintf(fout,"\n\t\t\tfor (i = 0; i < n; i++)");
      fprintf(fout,"\n\t\t\t{\tsoap_peek_element(soap);\n\t\t\t\tif (soap->position == %d)", d);
      fprintf(fout,"\n\t\t\t\t{\ti = ");
	for (i = 0; i < d; i++)
	{ fprintf(fout,"soap->positions[%d]", i);
	  for (j = 1; j < d-i; j++)
	    fprintf(fout,"*a->__size[%d]", j);
	  if (i < d-1)
	    fprintf(fout,"+");
	}
	fprintf(fout,"-j;");
	fprintf(fout,"\n\t\t\t\t\tif (i < 0 || i >= n)\n\t\t\t\t\t{\tsoap->error = SOAP_IOB;\n\t\t\t\t\t\treturn NULL;\n\t\t\t\t\t}\n\t\t\t\t}");
        fprintf(fout,"\n\t\t\t\tif (!soap_in_%s(soap, NULL, a->%s + i, \"%s\"))", c_ident(p->info.typ->ref), p->sym->name, xsi_type(p->info.typ->ref));
      fprintf(fout,"\n\t\t\t\t{\tif (soap->error != SOAP_NO_TAG)\n\t\t\t\t\t\treturn NULL;");
      fprintf(fout,"\n\t\t\t\t\tsoap->error = SOAP_OK;");
      fprintf(fout,"\n\t\t\t\t\tbreak;");
      fprintf(fout,"\n\t\t\t\t}");
    }
    else
    { fprintf(fout,"\n\t\ta->__size = soap_getsize(soap->arraySize, soap->arrayOffset, &j);");
      if (has_offset(typ) && (p->next->next->info.sto & Sconst) == 0)
      { fprintf(fout,"\n\t\ta->__offset = j;");
      }
      if (p->info.minOccurs > 1)
        fprintf(fout,"\n\t\tif (a->__size >= 0 && a->__size < %ld)\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}", p->info.minOccurs);
      if (p->info.maxOccurs > 1)
        fprintf(fout,"\n\t\tif (a->__size > %ld)\n\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\treturn NULL;\n\t\t}", p->info.maxOccurs);
      fprintf(fout,"\n\t\tif (a->__size >= 0)");
      if (((Tnode*)p->info.typ->ref)->type == Tclass)
      { fprintf(fout,"\n\t\t{\ta->%s = soap_new_%s(soap, a->__size);", p->sym->name, c_ident(p->info.typ->ref));
        if (!is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
          fprintf(fout, "\n\t\t\tfor (i = 0; i < a->__size; i++)\n\t\t\t\t(a->%s+i)->%s::soap_default(soap);", p->sym->name, c_type(p->info.typ->ref));
        else
          fprintf(fout, "\n\t\t\tfor (i = 0; i < a->__size; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      else if (has_class(p->info.typ->ref))
      { fprintf(fout,"\n\t\t{\ta->%s = soap_new_%s(soap, a->__size);", p->sym->name, c_ident(p->info.typ->ref));
        fprintf(fout, "\n\t\t\tfor (i = 0; i < a->__size; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      else
      { fprintf(fout,"\n\t\t{\ta->%s = (%s)soap_malloc(soap, sizeof(%s) * a->__size);", p->sym->name, c_type_id(p->info.typ->ref, "*"),  c_type(p->info.typ->ref));
	if (((Tnode*)p->info.typ->ref)->type == Tpointer)
          fprintf(fout, "\n\t\t\tfor (i = 0; i < a->__size; i++)\n\t\t\t\ta->%s[i] = NULL;", p->sym->name);
	else if (!is_XML(p->info.typ->ref))
          fprintf(fout, "\n\t\t\tfor (i = 0; i < a->__size; i++)\n\t\t\t\tsoap_default_%s(soap, a->%s+i);", c_ident(p->info.typ->ref), p->sym->name);
      }
      fprintf(fout,"\n\t\t\tfor (i = 0; i < a->__size; i++)");
      fprintf(fout,"\n\t\t\t{\tsoap_peek_element(soap);\n\t\t\t\tif (soap->position)\n\t\t\t\t{\ti = soap->positions[0]-j;\n\t\t\t\t\tif (i < 0 || i >= a->__size)\n\t\t\t\t\t{\tsoap->error = SOAP_IOB;\n\t\t\t\t\t\treturn NULL;\n\t\t\t\t\t}\n\t\t\t\t}");
      if (is_XML(p->info.typ->ref) && is_string(p->info.typ->ref))
        fprintf(fout,"\n\t\t\t\tif (!soap_inliteral(soap, NULL, a->%s + i))", p->sym->name);
      else if (is_XML(p->info.typ->ref) && is_wstring(p->info.typ->ref))
        fprintf(fout,"\n\t\t\t\tif (!soap_inwliteral(soap, NULL, a->%s + i))", p->sym->name);
      else
        fprintf(fout,"\n\t\t\t\tif (!soap_in_%s(soap, NULL, a->%s + i, \"%s\"))", c_ident(p->info.typ->ref), p->sym->name, xsi_type(p->info.typ->ref));
      fprintf(fout,"\n\t\t\t\t{\tif (soap->error != SOAP_NO_TAG)\n\t\t\t\t\t\treturn NULL;");
      fprintf(fout,"\n\t\t\t\t\tsoap->error = SOAP_OK;");
      fprintf(fout,"\n\t\t\t\t\tbreak;");
      fprintf(fout,"\n\t\t\t\t}");
    }
    fprintf(fout,"\n\t\t\t}\n\t\t}\n\t\telse");
    if (((Tnode*)p->info.typ->ref)->type == Tclass || has_class(p->info.typ->ref))
      fprintf(fout,"\n\t\t{\t%s;\n\t\t\tsoap_new_block(soap);", c_type_id(p->info.typ->ref, "q"));
    else
      fprintf(fout,"\n\t\t{\tsoap_new_block(soap);");
    if (p->info.maxOccurs > 1)
    { if (d)
        fprintf(fout,"\n\t\t\tfor (a->__size[0] = 0; a->__size[0] <= %ld; a->__size[0]++)", p->info.maxOccurs);
      else
        fprintf(fout,"\n\t\t\tfor (a->__size = 0; a->__size <= %ld; a->__size++)", p->info.maxOccurs);
    }
    else
    { if (d)
        fprintf(fout,"\n\t\t\tfor (a->__size[0] = 0; ; a->__size[0]++)");
      else
        fprintf(fout,"\n\t\t\tfor (a->__size = 0; ; a->__size++)");
    }
    fprintf(fout,"\n\t\t\t{\tp = (%s)soap_push_block(soap, sizeof(%s));\n\t\t\t\tif (!p)\n\t\t\t\t\treturn NULL;", c_type(p->info.typ), c_type(p->info.typ->ref));
    if (((Tnode*)p->info.typ->ref)->type == Tclass || has_class(p->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tmemcpy(p, &q, sizeof(%s));", c_type(p->info.typ->ref));
    if (((Tnode*)p->info.typ->ref)->type == Tclass && !is_external(p->info.typ->ref) && !is_volatile(p->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tp->soap_default(soap);");
    else if (((Tnode*)p->info.typ->ref)->type == Tpointer)
      fprintf(fout,"\n\t\t\t\t*p = NULL;");
    else if (!is_XML(p->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tsoap_default_%s(soap, p);", c_ident(p->info.typ->ref));
    if (is_XML(p->info.typ->ref) && is_string(p->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tif (!soap_inliteral(soap, NULL, p))");
    else if (is_XML(p->info.typ->ref) && is_wstring(p->info.typ->ref))
      fprintf(fout,"\n\t\t\t\tif (!soap_inwliteral(soap, NULL, p))");
    else
      fprintf(fout,"\n\t\t\t\tif (!soap_in_%s(soap, NULL, p, \"%s\"))", c_ident(p->info.typ->ref), xsi_type(p->info.typ->ref));
    fprintf(fout,"\n\t\t\t\t{\tif (soap->error != SOAP_NO_TAG)\n\t\t\t\t\t\treturn NULL;");
    fprintf(fout,"\n\t\t\t\t\tsoap->error = SOAP_OK;");
    fprintf(fout,"\n\t\t\t\t\tbreak;");
    fprintf(fout,"\n\t\t\t\t}");
    fprintf(fout,"\n\t\t\t}");
    fprintf(fout,"\n\t\t\tsoap_pop_block(soap);");
    if (p->info.minOccurs > 1)
      fprintf(fout,"\n\t\t\tif (a->__size < %ld)\n\t\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\t\treturn NULL;\n\t\t\t}", p->info.minOccurs);
    if (p->info.maxOccurs > 1)
      fprintf(fout,"\n\t\t\tif (a->__size > %ld)\n\t\t\t{\tsoap->error = SOAP_OCCURS;\n\t\t\t\treturn NULL;\n\t\t\t}", p->info.maxOccurs);
    if (((Tnode*)p->info.typ->ref)->type == Tclass || has_class(p->info.typ->ref))
      fprintf(fout,"\n\t\t\tif (soap->blist->size)\n\t\t\t\ta->%s = soap_new_%s(soap, soap->blist->size/sizeof(%s));\n\t\t\telse\n\t\t\t\ta->%s = NULL;", p->sym->name, c_ident(p->info.typ->ref), c_type(p->info.typ->ref), p->sym->name);
    else
      fprintf(fout,"\n\t\t\ta->%s = (%s)soap_malloc(soap, soap->blist->size);", p->sym->name, c_type(p->info.typ));
    if (((Tnode*)p->info.typ->ref)->type == Tpointer)
      fprintf(fout,"\n\t\t\tsoap_save_block(soap, (char*)a->%s, 0);", p->sym->name);
    else
      fprintf(fout,"\n\t\t\tsoap_save_block(soap, (char*)a->%s, 1);", p->sym->name);
    fprintf(fout,"\n\t\t}");
    fprintf(fout,"\n\t\tif (soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
  }
  if (has_getter(typ))
    fprintf(fout,"\n\t\ta->get(soap);");
  fprintf(fout,"\n\t}\n\telse\n\t{\t");
  if (typ->type == Tclass)
    fprintf(fout,"a = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, soap_copy_%s);", c_type_id(typ, "*"), c_ident(typ), c_type(typ), c_ident(typ));
  else
    fprintf(fout,"a = (%s)soap_id_forward(soap, soap->href, (void**)a, SOAP_TYPE_%s, 0, sizeof(%s), 0, NULL);", c_type_id(typ, "*"), c_ident(typ), c_type(typ));
  fprintf(fout,"\n\t\tif (soap->body && soap_element_end_in(soap, tag))\n\t\t\treturn NULL;");
  fprintf(fout,"\n\t}");
  fprintf(fout,"\n\treturn a;\n}");
}

