/*

service.cpp

Service structures.

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

TODO:	add support for HTTP operations (non-SOAP access)
        add headerfault output definitions

*/

#include "types.h"
#include "service.h"

static void comment(const char *start, const char *middle, const char *end, const char *text);

////////////////////////////////////////////////////////////////////////////////
//
//	Definitions methods
//
////////////////////////////////////////////////////////////////////////////////

Definitions::Definitions()
{ }

void Definitions::collect(const wsdl__definitions &definitions)
{ // Collect information: analyze WSDL definitions and imported definitions
  if (definitions.service.empty())
    fprintf(stderr, "Warning: no wsdl:definitions/service in definitions %s targetNamespace %s\n", definitions.name?definitions.name:"", definitions.targetNamespace);
  analyze(definitions);
  for (vector<wsdl__import>::const_iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    if ((*import).definitionsPtr())
      analyze(*(*import).definitionsPtr());
}

void Definitions::analyze(const wsdl__definitions &definitions)
{ // Analyze WSDL and build Service information
  for (vector<wsdl__binding>::const_iterator binding = definitions.binding.begin(); binding != definitions.binding.end(); ++binding)
  { // /definitions/binding/documentation
    const char *binding_documentation = (*binding).documentation;
    // /definitions/binding/soap:binding
    soap__binding *soap__binding_ = (*binding).soap__binding_;
    // /definitions/binding/soap:binding/@transport
    const char *soap__binding_transport = NULL;
    if (soap__binding_)
      soap__binding_transport = soap__binding_->transport;
    // /definitions/binding/soap:binding/@style
    soap__styleChoice soap__binding_style = rpc;
    if (soap__binding_ && soap__binding_->style)
      soap__binding_style = *soap__binding_->style;
    // /definitions/binding/http:binding
    http__binding *http__binding_ = (*binding).http__binding_;
    const char *http__binding_verb = NULL;
    if (http__binding_)
       http__binding_verb = http__binding_->verb;
    // /definitions/binding/operation*
    for (vector<wsdl__binding_operation>::const_iterator operation = (*binding).operation.begin(); operation != (*binding).operation.end(); ++operation)
    { // /definitions/portType/operation/ associated with /definitions/binding/operation
      wsdl__operation *wsdl__operation_ = (*operation).operationPtr();
      // /definitions/binding/operation/soap:operation
      soap__operation *soap__operation_ = (*operation).soap__operation_;
      // /definitions/binding/operation/soap:operation/@style
      soap__styleChoice soap__operation_style = soap__binding_style;
      if (soap__operation_ && soap__operation_->style)
        soap__operation_style = *soap__operation_->style;
      // /definitions/binding/operation/http:operation
      http__operation *http__operation_ = (*operation).http__operation_;
      // /definitions/binding/operation/http:operation/@location
      const char *http__operation_location = NULL;
      if (http__operation_)
        http__operation_location = http__operation_->location;
      // /definitions/binding/operation/input
      wsdl__ext_input *ext_input = (*operation).input;
      // /definitions/binding/operation/output
      wsdl__ext_output *ext_output = (*operation).output;
      // /definitions/portType/operation
      if (wsdl__operation_)
      { wsdl__input *input = wsdl__operation_->input;
        wsdl__output *output = wsdl__operation_->output;
        if (http__operation_)
        { // TODO: HTTP operation
        }
        else if (input && ext_input)
	{ soap__body *input_body = ext_input->soap__body_;
	  if (ext_input->mime__multipartRelated_)
	  { for (vector<mime__part>::const_iterator part = ext_input->mime__multipartRelated_->part.begin(); part != ext_input->mime__multipartRelated_->part.end(); ++part)
	      if ((*part).soap__body_)
	      { input_body = (*part).soap__body_;
	        break;
	      }
	  }
          // MUST have an input, otherwise can't generate a service operation
          if (input_body)
          { const char *URI;
            if (soap__operation_style == rpc)
              URI = input_body->namespace_;
            else
              URI = definitions.targetNamespace;
            if (URI)
            { const char *prefix = types.nsprefix(NULL, URI);
              const char *name = types.aname(NULL, NULL, (*binding).name); // name of service is binding name
              Service *s = services[prefix];
              if (!s)
              { s = services[prefix] = new Service();
                s->prefix = prefix;
                s->URI = URI;
                s->name = name;
                if ((*binding).portTypePtr())
                  s->type = types.aname(NULL, NULL, (*binding).portTypePtr()->name);
                else
                  s->type = NULL;
              }
              for (vector<wsdl__service>::const_iterator service = definitions.service.begin(); service != definitions.service.end(); ++service)
              { for (vector<wsdl__port>::const_iterator port = (*service).port.begin(); port != (*service).port.end(); ++port)
                { if ((*port).bindingPtr() == &(*binding))
                  { if ((*port).soap__address_)
                      s->location.insert((*port).soap__address_->location);
                    // TODO: HTTP address for HTTP operations
                    // if ((*port).http__address_)
                      // http__address_location = http__address_->location;
                    if ((*service).documentation)
                      s->service_documentation[(*service).name] = (*service).documentation;
                    if ((*port).documentation && (*port).name)
                      s->port_documentation[(*port).name] = (*port).documentation;
                    if (binding_documentation)
                      s->binding_documentation[(*binding).name] = binding_documentation;
                  }
                }
              }
              Operation *o = new Operation();
              o->name = types.aname(NULL, NULL, wsdl__operation_->name);
              o->prefix = prefix;
              o->URI = URI;
              o->style = soap__operation_style;
              o->documentation = wsdl__operation_->documentation;
              o->operation_documentation = (*operation).documentation;
              o->parameterOrder = wsdl__operation_->parameterOrder;
              if ((*operation).soap__operation_)
                o->soapAction = (*operation).soap__operation_->soapAction;
              else
              { o->soapAction = "";
                // determine if we use SOAP 1.2 in which case soapAction is absent, this is a bit of a hack due to the lack of WSDL1.1/SOAP1.2 support and better alternatives
                for (Namespace *p = definitions.soap->local_namespaces; p && p->id; p++)
                { if (p->out && !strcmp(p->id, "soap") && !strcmp(p->out, "http://schemas.xmlsoap.org/wsdl/soap12/"))
                  { o->soapAction = NULL;
                    break;
                  }
                }
              }
              o->input = new Message();
              o->input->name = (*operation).name; // RPC uses operation/@name
              if (soap__operation_style == rpc && !input_body->namespace_)
              { o->input->URI = "";
                fprintf(stderr, "Error: no soap:body namespace attribute\n");
              }
              else
                o->input->URI = input_body->namespace_;
              o->input->use = input_body->use;
              o->input->encodingStyle = input_body->encodingStyle;
              o->input->message = input->messagePtr();
	      o->input->body_parts = input_body->parts;
              o->input->part = NULL;
              o->input->header = ext_input->soap__header_;
	      o->input->multipartRelated = ext_input->mime__multipartRelated_;
	      if (ext_input->dime__message_)
	        o->input->layout = ext_input->dime__message_->layout;
	      else
	        o->input->layout = NULL;
              o->input->documentation = input->documentation;
              o->input->ext_documentation = ext_input->documentation;
              if (soap__operation_style == document)
                o->input_name = types.oname("__", o->URI, o->input->name);
              else
                o->input_name = types.oname(NULL, o->input->URI, o->input->name);
              if (output && ext_output)
	      { soap__body *output_body = ext_output->soap__body_;
                if (ext_output->mime__multipartRelated_)
	        { for (vector<mime__part>::const_iterator part = ext_output->mime__multipartRelated_->part.begin(); part != ext_output->mime__multipartRelated_->part.end(); ++part)
	            if ((*part).soap__body_)
	            { output_body = (*part).soap__body_;
	              break;
	            }
		}
                o->output = new Message();
                o->output->name = (*operation).name; // RPC uses operation/@name with suffix 'Response' as set below
                o->output->use = output_body->use;
                // the code below is a hack around the RPC encoded response message element tag mismatch with Axis:
                if (!output_body->namespace_ || o->output->use == encoded)
                  o->output->URI = o->input->URI; // encoded seems (?) to require the request's namespace
                else
                  o->output->URI = output_body->namespace_;
                o->output->encodingStyle = output_body->encodingStyle;
                o->output->message = output->messagePtr();
	        o->output->body_parts = output_body->parts;
                o->output->part = NULL;
                o->output->header = ext_output->soap__header_;
	        o->output->multipartRelated = ext_output->mime__multipartRelated_;
	        if (ext_output->dime__message_)
	          o->output->layout = ext_output->dime__message_->layout;
	        else
	          o->output->layout = NULL;
                o->output->documentation = output->documentation;
                o->output->ext_documentation = ext_output->documentation;
                char *s = (char*)soap_malloc(definitions.soap, strlen(o->output->name) + 9);
                strcpy(s, o->output->name);
                strcat(s, "Response");
                if (soap__operation_style == document)
                  o->output_name = types.oname("__", o->URI, s);
                else
                  o->output_name = types.oname(NULL, o->output->URI, s);
              }
              else
              { o->output_name = NULL;
                o->output = NULL;
              }
              // collect input headers and headerfaults
              if (ext_input)
              { for (vector<soap__header>::const_iterator header = ext_input->soap__header_.begin(); header != ext_input->soap__header_.end(); ++header)
                { Message *h = new Message();
                  h->message = (*header).messagePtr();
	          h->body_parts = NULL;
                  h->part = (*header).partPtr();
                  h->URI = (*header).namespace_;
                  if (h->part && h->part->element)
                    h->name = types.aname(NULL, NULL, h->part->element);
                  else if (h->URI && h->part && h->part->name && h->part->type)
                    h->name = types.aname(NULL, h->URI, h->part->name);
                  else
                  { fprintf(stderr, "Error in SOAP Header part definition\n");
                    h->name = "";
                  }
                  h->encodingStyle = (*header).encodingStyle;
                  h->use = (*header).use;
		  h->multipartRelated = NULL;
		  h->layout = NULL;
                  h->ext_documentation = NULL;	// TODO: add document content
                  h->documentation = NULL;		// TODO: add document content
                  s->header[h->name] = h;
                  for (vector<soap__headerfault>::const_iterator headerfault = (*header).headerfault.begin(); headerfault != (*header).headerfault.end(); ++headerfault)
                  { // TODO: complete headerfault processing
                  }
                }
              }
              // collect output headers and headerfaults
              if (ext_output)
              { for (vector<soap__header>::const_iterator header = ext_output->soap__header_.begin(); header != ext_output->soap__header_.end(); ++header)
                { Message *h = new Message();
                  h->message = (*header).messagePtr();
	          h->body_parts = NULL;
    	          h->part = (*header).partPtr();
    	          h->URI = (*header).namespace_;
    	          if (h->part && h->part->element)
    	            h->name = types.aname(NULL, NULL, h->part->element);
    	          else if (h->URI && h->part && h->part->name && h->part->type)
    	            h->name = types.aname(NULL, h->URI, h->part->name);
    	          else
    	          { fprintf(stderr, "Error in SOAP Header part definition\n");
    	            h->name = "";
    	          }
    	          h->encodingStyle = (*header).encodingStyle;
    	          h->use = (*header).use;
		  h->multipartRelated = NULL;
		  h->layout = NULL;
    	          h->ext_documentation = NULL;	// TODO: add document content
    	          h->documentation = NULL;		// TODO: add document content
    	          s->header[h->name] = h;
    	          for (vector<soap__headerfault>::const_iterator headerfault = (*header).headerfault.begin(); headerfault != (*header).headerfault.end(); ++headerfault)
    	          { // TODO: complete headerfault processing
    	          }
    	        }
    	      }
    	      // collect faults
              for (vector<wsdl__ext_fault>::const_iterator ext_fault = (*operation).fault.begin(); ext_fault != (*operation).fault.end(); ++ext_fault)
    	      { if ((*ext_fault).soap__fault_ && (*ext_fault).messagePtr())
    	        { Message *f = new Message();
    	          f->message = (*ext_fault).messagePtr();
	          f->body_parts = NULL;
    	          f->part = NULL;
    	          f->encodingStyle = (*ext_fault).soap__fault_->encodingStyle;
    	          f->URI = (*ext_fault).soap__fault_->namespace_;
    	          f->use = (*ext_fault).soap__fault_->use;
		  f->multipartRelated = NULL;
		  f->layout = NULL;
    	          f->ext_documentation = (*ext_fault).documentation;
                  f->name = types.aname("_", f->URI, f->message->name);
    	          f->documentation = f->message->documentation;
    	          o->fault.push_back(f);
    	          s->fault[f->name] = f;
    	        }
    	        else
    	          fprintf(stderr, "Error: no wsdl:definitions/binding/operation/fault/soap:fault\n");
    	      }
    	      s->operation.push_back(o);
            }
    	    else
    	      fprintf(stderr, "Warning: no SOAP RPC operation namespace, operations will be ignored\n");
          }
          else
            fprintf(stderr, "Error: no wsdl:definitions/binding/operation/input/soap:body\n");
        }
        else
          fprintf(stderr, "Error: no wsdl:definitions/portType/operation/input\n");
      }
      else
        fprintf(stderr, "Error: no wsdl:definitions/portType/operation\n");
    }
  }
}

void Definitions::compile(const wsdl__definitions& definitions)
{ // compile the definitions and generate gSOAP header file
  collect(definitions);
  fprintf(stream, "/** @mainpage %s Definitions\n\n", definitions.name?definitions.name:outfile?outfile:"Web Service");
  // copy documentation from WSDL definitions
  if (definitions.documentation)
    fprintf(stream, "@section Service Documentation\n\n%s\n\n", definitions.documentation);
  for (MapOfStringToService::const_iterator service = services.begin(); service != services.end(); ++service)
  { Service *sv = (*service).second;
    if (sv && (*sv).name)
    { fprintf(stream, "@section %s Service Binding \"%s\"\n\n", (*sv).name, (*sv).name);
      for (MapOfStringToString::const_iterator service_doc = (*sv).service_documentation.begin(); service_doc != (*sv).service_documentation.end(); ++service_doc)
        fprintf(stream, "@subsection %s \"%s\" Documentation\n\n%s\n\n", (*service_doc).first, (*service_doc).first, (*service_doc).second);
      for (MapOfStringToString::const_iterator port_doc = (*sv).port_documentation.begin(); port_doc != (*sv).port_documentation.end(); ++port_doc)
        fprintf(stream, "@subsection %s Port \"%s\" Documentation\n\n%s\n\n", (*port_doc).first, (*port_doc).first, (*port_doc).second);
      for (MapOfStringToString::const_iterator binding_doc = (*sv).binding_documentation.begin(); binding_doc != (*sv).binding_documentation.end(); ++binding_doc)
        fprintf(stream, "@subsection %s Binding \"%s\" Documentation\n\n%s\n\n", (*binding_doc).first, (*binding_doc).first, (*binding_doc).second);
      fprintf(stream, "@subsection %s_operations Operations\n\n", (*sv).name);
      for (vector<Operation*>::const_iterator op = (*sv).operation.begin(); op != (*sv).operation.end(); ++op)
      { if (*op && (*op)->input_name)
          fprintf(stream, "  - @ref %s\n", (*op)->input_name);
      }
      fprintf(stream, "\n@subsection %s_ports Endpoint Ports\n\n", (*sv).name);
      for (SetOfString::const_iterator port = (*sv).location.begin(); port != (*sv).location.end(); ++port)
        fprintf(stream, "  - %s\n", *port);
    }
  }
  fprintf(stream, "\n*/\n\n//  Note: modify this file to customize the generated data type declarations\n\n");
  if (lflag)
    fprintf(stream, "/*\n%s*/\n\n", licensenotice);
  // gsoap compiler options: 'w' disables WSDL/schema output to avoid file collisions
  if (cflag)
    fprintf(stream, "//gsoapopt cw\n");
  else
    fprintf(stream, "//gsoapopt w\n");
  // determine if we must use SOAP 1.2, this is a bit of a hack due to the lack of WSDL1.1/SOAP1.2 support and better alternatives
  for (Namespace *p = definitions.soap->local_namespaces; p && p->id; p++)
  { // p->out is set to the actual namespace name that matches the p->in pattern
    if (p->out && !strcmp(p->id, "soap") && !strcmp(p->out, "http://schemas.xmlsoap.org/wsdl/soap12/"))
    { fprintf(stream, "// This service uses SOAP 1.2 namespaces:\n");
      fprintf(stream, schemaformat, "SOAP-ENV", "namespace", "http://www.w3.org/2003/05/soap-envelope");
      fprintf(stream, schemaformat, "SOAP-ENC", "namespace", "http://www.w3.org/2003/05/soap-encoding");
      break;
    }
  }
  if (!cflag && !sflag)
    fprintf(stream, "#import \"stl.h\"\n");
  if (mflag)
    fprintf(stream, "#import \"base.h\"\n");
  // generate the prototypes first: these should allow use before def, e.g. class names then generate the defs
  // check if xsd:anyType is used
  if (!cflag && !pflag)
  { for (SetOfString::const_iterator i = definitions.builtinTypes().begin(); i != definitions.builtinTypes().end(); ++i)
    { if (!cflag && !strcmp(*i, "xs:anyType"))
      { pflag = 1;
        break;
      }
    }
  }
  // define xsd:anyType first, if used
  if (!cflag && pflag)
  { const char *s, *t;
    t = types.cname(NULL, NULL, "xs:anyType");
    s = types.deftypemap[t];
    if (s)
    { if (*s)
      { if (!mflag)
          fprintf(stream, "%s\n", s);
      }
      s = types.usetypemap[t];
      if (s)
      { if (mflag)
          fprintf(stream, "// base.h must define type: %s\n", s);
        types.knames.insert(s);
      }
    }
    else
    { fprintf(stderr, "Error: no xsd__anyType defined in type map\n");
      pflag = 0;
    }
  }
  // produce built-in primitive types, limited to the ones that are used only
  for (SetOfString::const_iterator i = definitions.builtinTypes().begin(); i != definitions.builtinTypes().end(); ++i)
  { const char *s, *t;
    if (!cflag && pflag && !strcmp(*i, "xs:anyType"))
      continue;
    t = types.cname(NULL, NULL, *i);
    s = types.deftypemap[t];
    if (s)
    { if (*s)
      { if (!mflag)
        { if (**i == '"')
	    fprintf(stream, "/// Imported type %s\n", *i);
          else
	    fprintf(stream, "/// Built-in type \"%s\"\n", *i);
          fprintf(stream, "%s\n", s);
        }
      }
      s = types.usetypemap[t];
      if (s)
      { if (mflag && **i != '"')
          fprintf(stream, "// base.h must define type: %s\n", s);
        if (types.knames.find(s) == types.knames.end())
          types.knames.insert(s);
      }
    }
    else
    { if (!mflag)
      { if (**i == '"')
          fprintf(stream, "// Imported type %s defined by %s\n", *i, t);
        else
        { fprintf(stream, "/// Primitive built-in type \"%s\"\n", *i);
          fprintf(stream, "typedef char *%s;\n", t);
        }
      }
      else if (**i == '"')
        fprintf(stream, "// Imported type %s defined by %s\n", *i, t);
      else
        fprintf(stream, "// base.h must define type: %s\n", t);
      types.ptrtypemap[t] = types.deftname(TYPEDEF, NULL, NULL, NULL, *i);	// already pointer
    }
    if (pflag && !strncmp(*i, "xs:", 3))		// only xsi types are polymorph
    { s = types.aname(NULL, NULL, *i);
      if (!mflag)
      { fprintf(stream, "/// Class wrapper for built-in type \"%s\" derived from xsd__anyType\n", *i);
        fprintf(stream, "class %s : public xsd__anyType\n{ public:\n", s);
        fprintf(stream, elementformat, types.tname(NULL, NULL, *i), "__item;");
        fprintf(stream, "\n};\n");
      }
      types.knames.insert(s);
    }
  }
  // produce built-in primitive elements, limited to the ones that are used only
  for (SetOfString::const_iterator j = definitions.builtinElements().begin(); j != definitions.builtinElements().end(); ++j)
  { const char *s, *t = types.cname("_", NULL, *j);
    s = types.deftypemap[t];
    if (s && *s)
    { if (mflag)
      { if (**j == '"')
	  fprintf(stream, "/// Imported element %s\n", *j);
        else
	  fprintf(stream, "/// Built-in element \"%s\"\n", *j);
      }
      else
        fprintf(stream, "// base.h must define element: ");
      fprintf(stream, "%s\n", s);
      s = types.usetypemap[t];
      if (s && *s && types.knames.find(s) == types.knames.end())
        types.knames.insert(s);
    }
    else
    { if (!mflag)
      { if (**j == '"')
	  fprintf(stream, "// Imported element %s defined by %s\n", *j, t);
        else
	{ fprintf(stream, "/// Built-in element \"%s\"\n", *j);
          fprintf(stream, "typedef _XML %s;\n", t);
        }
      }
      else if (**j == '"')
	fprintf(stream, "// Imported element %s defined by %s\n", *j, t);
      else
        fprintf(stream, "// base.h must define element: %s\n", t);
      types.ptrtypemap[t] = types.deftname(TYPEDEF, NULL, NULL, NULL, *j);	// already pointer
      types.knames.insert(t);
    }
  }
  // produce built-in primitive attributes, limited to the ones that are used only
  for (SetOfString::const_iterator k = definitions.builtinAttributes().begin(); k != definitions.builtinAttributes().end(); ++k)
  { const char *s, *t = types.cname(NULL, NULL, *k);
    s = types.deftypemap[t];
    if (s && *s)
    { if (mflag)
      { if (**k == '"')
	  fprintf(stream, "/// Imported attribute %s\n", *k);
        else
	  fprintf(stream, "/// Built-in attribute \"%s\"\n", *k);
      }
      else
        fprintf(stream, "// base.h must define attribute: ");
      fprintf(stream, "%s\n", s);
      s = types.usetypemap[t];
      if (s && *s && types.knames.find(s) == types.knames.end())
        types.knames.insert(s);
    }
    else
    { if (!mflag)
      { if (**k == '"')
          fprintf(stream, "// Imported attribute %s defined by %s\n", *k, t);
        else
        { fprintf(stream, "/// Built-in attribute \"%s\"\n", *k);
          fprintf(stream, "typedef char *%s;\n", t);
        }
      }
      else if (**k == '"')
        fprintf(stream, "// Imported attribute %s defined by %s\n", *k, t);
      else
        fprintf(stream, "// base.h must define attribute: %s\n", t);
      types.ptrtypemap[t] = types.deftname(TYPEDEF, NULL, NULL, NULL, *k);	// already pointer
      types.knames.insert(t);
    }
  }
  // produce types
  if (definitions.types)
  { fprintf(stream, "\n/*\nTo customize the names of the namespace prefixes generated by wsdl2h, modify\nthe prefix names below and add the modified lines to typemap.dat to run wsdl2h:\n\n");
    for (vector<xs__schema*>::const_iterator schema1 = definitions.types->xs__schema_.begin(); schema1 != definitions.types->xs__schema_.end(); ++schema1)
      fprintf(stream, "%s = %s\n", types.nsprefix(NULL, (*schema1)->targetNamespace), (*schema1)->targetNamespace);
    fprintf(stream, "*/\n\n");
    comment("Definitions", definitions.name?definitions.name:"", "types", definitions.types->documentation);
    for (vector<xs__schema*>::const_iterator schema2 = definitions.types->xs__schema_.begin(); schema2 != definitions.types->xs__schema_.end(); ++schema2)
      fprintf(stream, schemaformat, types.nsprefix(NULL, (*schema2)->targetNamespace), "namespace", (*schema2)->targetNamespace);
    for (vector<xs__schema*>::const_iterator schema3 = definitions.types->xs__schema_.begin(); schema3 != definitions.types->xs__schema_.end(); ++schema3)
    { if ((*schema3)->elementFormDefault == (*schema3)->attributeFormDefault)
        fprintf(stream, schemaformat, types.nsprefix(NULL, (*schema3)->targetNamespace), "form", (*schema3)->elementFormDefault == qualified ? "qualified" : "unqualified");
      else
      { fprintf(stream, schemaformat, types.nsprefix(NULL, (*schema3)->targetNamespace), "elementForm", (*schema3)->elementFormDefault == qualified ? "qualified" : "unqualified");
        fprintf(stream, schemaformat, types.nsprefix(NULL, (*schema3)->targetNamespace), "attributeForm", (*schema3)->attributeFormDefault == qualified ? "qualified" : "unqualified");
      }
    }
    // define class/struct types first
    fprintf(stream, "\n// Forward declarations\n");
    for (vector<xs__schema*>::const_iterator schema4 = definitions.types->xs__schema_.begin(); schema4 != definitions.types->xs__schema_.end(); ++schema4)
    { for (vector<xs__complexType>::const_iterator complexType = (*schema4)->complexType.begin(); complexType != (*schema4)->complexType.end(); ++complexType)
        types.define((*schema4)->targetNamespace, NULL, *complexType);
      for (vector<xs__element>::const_iterator element = (*schema4)->element.begin(); element != (*schema4)->element.end(); ++element)
        if (!(*element).type && (*element).complexTypePtr())
          types.define((*schema4)->targetNamespace, (*element).name, *(*element).complexTypePtr());
    }  
    fprintf(stream, "\n// End of forward declarations\n\n");
    // visit types with lowest base level first
    int baseLevel = 1;
    bool found;
    do
    { found = (baseLevel == 1);
      for (vector<xs__schema*>::iterator schema = definitions.types->xs__schema_.begin(); schema != definitions.types->xs__schema_.end(); ++schema)
      { for (vector<xs__simpleType>::iterator simpleType = (*schema)->simpleType.begin(); simpleType != (*schema)->simpleType.end(); ++simpleType)
        { if ((*simpleType).baseLevel() == baseLevel)
          { found = true;
            types.gen((*schema)->targetNamespace, NULL, *simpleType);
          }
        }
        for (vector<xs__element>::iterator element = (*schema)->element.begin(); element != (*schema)->element.end(); ++element)
        { if (!(*element).type && (*element).simpleTypePtr() && (*element).simpleTypePtr()->baseLevel() == baseLevel)
          { found = true;
            types.gen((*schema)->targetNamespace, (*element).name, *(*element).simpleTypePtr());
          }
          if (!(*element).type && (*element).complexTypePtr() && (*element).complexTypePtr()->baseLevel() == baseLevel)
            found = true;
        }
        for (vector<xs__attribute>::const_iterator attribute = (*schema)->attribute.begin(); attribute != (*schema)->attribute.end(); ++attribute)
        { if (!(*attribute).type && (*attribute).simpleTypePtr() && (*attribute).simpleTypePtr()->baseLevel() == baseLevel)
          { found = true;
            types.gen((*schema)->targetNamespace, (*attribute).name, *(*attribute).simpleTypePtr()); // URI = NULL won't generate type in schema (type without namespace qualifier)
          }
        }
        for (vector<xs__complexType>::iterator complexType = (*schema)->complexType.begin(); complexType != (*schema)->complexType.end(); ++complexType)
        { if ((*complexType).baseLevel() == baseLevel)
            found = true;
        }
      }
      ++baseLevel;
    } while (found);
    // generate complex type defs. Problem: what if a simpleType restriction/extension depends on a complexType simpleContent restriction/extension?
    int maxLevel = baseLevel;
    for (baseLevel = 1; baseLevel < maxLevel; ++baseLevel)
    { for (vector<xs__schema*>::iterator schema = definitions.types->xs__schema_.begin(); schema != definitions.types->xs__schema_.end(); ++schema)
      { for (vector<xs__complexType>::iterator complexType = (*schema)->complexType.begin(); complexType != (*schema)->complexType.end(); ++complexType)
        { if ((*complexType).baseLevel() == baseLevel)
            types.gen((*schema)->targetNamespace, NULL, *complexType);
        }
        for (vector<xs__element>::iterator element = (*schema)->element.begin(); element != (*schema)->element.end(); ++element)
        { if (!(*element).type && (*element).complexTypePtr() && (*element).complexTypePtr()->baseLevel() == baseLevel)
            types.gen((*schema)->targetNamespace, (*element).name, *(*element).complexTypePtr());
        }
      }
    }
    // option to consider: generate local complexTypes iteratively
    /*
    for (MapOfStringToType::const_iterator local = types.locals.begin(); local != types.locals.end(); ++local)
    { types.gen(NULL, (*local).first, *(*local).second);
    }
    */
  }
  generate();
  fprintf(stream, "\n/*  End of %s Definitions */\n", definitions.name?definitions.name:outfile?outfile:"Web Service");
}

void Definitions::generate()
{ MapOfStringToMessage headers;
  MapOfStringToMessage faults;
  for (MapOfStringToService::const_iterator service1 = services.begin(); service1 != services.end(); ++service1)
  { if ((*service1).second)
    { for (MapOfStringToMessage::const_iterator header = (*service1).second->header.begin(); header != (*service1).second->header.end(); ++header)
        headers[(*header).first] = (*header).second;
      for (MapOfStringToMessage::const_iterator fault = (*service1).second->fault.begin(); fault != (*service1).second->fault.end(); ++fault)
        faults[(*fault).first] = (*fault).second;
    }
  }
  // Generate SOAP Header definition
  if (!headers.empty())
  { fprintf(stream, "// SOAP Header\n");
    //if (cflag) always use structs to avoid compilation warnings
      fprintf(stream, "struct SOAP_ENV__Header\n{\n");
    //else
      //fprintf(stream, "class SOAP_ENV__Header\n{ public:\n");
    for (MapOfStringToMessage::const_iterator header = headers.begin(); header != headers.end(); ++header)
    { if ((*header).second->URI && !types.uris[(*header).second->URI])
        fprintf(stream, schemaformat, types.nsprefix(NULL, (*header).second->URI), "namespace", (*header).second->URI);
      comment("Header", (*header).first, "WSDL", (*header).second->ext_documentation);
      comment("Header", (*header).first, "SOAP", (*header).second->documentation);
      if ((*header).second->part && (*header).second->part->elementPtr())
      { fprintf(stream, "/// \"%s\" SOAP Header part element\n", (*header).second->part->name);
        types.gen((*header).second->part->elementPtr()->schemaPtr()->targetNamespace, *(*header).second->part->elementPtr());
      }
      else if ((*header).second->part && (*header).second->part->name && (*header).second->part->type)
      { fprintf(stream, elementformat, types.pname(true, NULL, NULL, (*header).second->part->type), (*header).first);
        fprintf(stream, ";\n");
      }
      else
        fprintf(stderr, "Error in SOAP Header part\n");
    }
    fprintf(stream, "\n};\n");
  }
  // Generate Fault detail element definitions
  for (MapOfStringToMessage::const_iterator fault = faults.begin(); fault != faults.end(); ++fault)
  { fprintf(stream, "/// SOAP Fault Detail element\n\n");
    fprintf(stream, "/// The SOAP Fault Detail element contains one of the following types serialized\n// in the __type and fault fields of the SOAP_ENV__Detail struct (see docs)\n");
    if ((*fault).second->URI && !types.uris[(*fault).second->URI])
      fprintf(stream, schemaformat, types.nsprefix(NULL, (*fault).second->URI), "namespace", (*fault).second->URI);
    comment("Fault", (*fault).first, "WSDL", (*fault).second->ext_documentation);
    comment("Fault", (*fault).first, "SOAP", (*fault).second->documentation);
    if (cflag)
      fprintf(stream, "struct %s\n{\n", (*fault).first);
    else
      fprintf(stream, "class %s\n{ public:", (*fault).first);
    (*fault).second->generate(types, ";", false, true);
    if (!cflag)
    { fprintf(stream, "\n");
      fprintf(stream, pointerformat, "struct soap", "soap");
      fprintf(stream, ";");
    }
    fprintf(stream, "\n};\n");
  }
  /* The SOAP Fault struct below is autogenerated by soapcpp2 (kept here for future mods)
  if (!mflag && !faults.empty())
  { fprintf(stream, "struct SOAP_ENV__Code\n{\n"); 
    fprintf(stream, elementformat, "_QName", "SOAP_ENV__Value");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "char", "SOAP_ENV__Node");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "char", "SOAP_ENV__Role");
    fprintf(stream, ";\n};\n");
    fprintf(stream, "struct SOAP_ENV__Detail\n{\n"); 
    fprintf(stream, elementformat, "int", "__type");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "void", "fault");
    fprintf(stream, ";\n");
    fprintf(stream, elementformat, "_XML", "__any");
    fprintf(stream, ";\n};\n");
    fprintf(stream, "struct SOAP_ENV__Fault\n{\n"); 
    fprintf(stream, elementformat, "_QName", "faultcode");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "char", "faultstring");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "char", "faultactor");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "struct SOAP_ENV__Detail", "detail");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "struct SOAP_ENV__Code", "SOAP_ENV__Code");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "char", "SOAP_ENV__Reason");
    fprintf(stream, ";\n");
    fprintf(stream, pointerformat, "struct SOAP_ENV__Detail", "SOAP_ENV__Detail");
    fprintf(stream, ";\n};\n");
  }
  */
  for (MapOfStringToService::const_iterator service2 = services.begin(); service2 != services.end(); ++service2)
    if ((*service2).second)
      (*service2).second->generate(types);
} 

////////////////////////////////////////////////////////////////////////////////
//
//	Service methods
//
////////////////////////////////////////////////////////////////////////////////

Service::Service()
{ prefix = NULL;
  URI = NULL;
  name = NULL;
}

void Service::generate(Types& types)
{ fprintf(stream, "\n");
  fprintf(stream, serviceformat, prefix, "name", name, "");
  fprintf(stream, serviceformat, prefix, "type", type, "");
  for (SetOfString::const_iterator port = location.begin(); port != location.end(); ++port)
    fprintf(stream, serviceformat, prefix, "port", (*port), "");
  fprintf(stream, serviceformat, prefix, "namespace", URI, "");
  for (vector<Operation*>::const_iterator op2 = operation.begin(); op2 != operation.end(); ++op2)
  { if (*op2 && (*op2)->input)
    { bool flag = false, anonymous = (*op2)->parameterOrder != NULL;
      if ((*op2)->output && (*op2)->output_name)
      { flag = ((*op2)->style == document && (*op2)->output->message && (*op2)->output->message->part.size() == 1);
        if (flag && (*op2)->input->message && (*(*op2)->output->message->part.begin()).element)
          for (vector<wsdl__part>::const_iterator part = (*op2)->input->message->part.begin(); part != (*op2)->input->message->part.end(); ++part)
            if ((*part).element && !strcmp((*part).element, (*(*op2)->output->message->part.begin()).element))
              flag = false;
	if (!flag)
        { fprintf(stream, "\n/// Operation response struct \"%s\" of service binding \"%s\" operation \"%s\"\n", (*op2)->output_name, name, (*op2)->input_name);
          fprintf(stream, "struct %s\n{", (*op2)->output_name);
          (*op2)->output->generate(types, ";", anonymous, true);
          fprintf(stream, "\n};\n");
        }
      }
      fprintf(stream, "\n/// Operation \"%s\" of service binding \"%s\"\n\n/**\n\nOperation details:\n\n", (*op2)->input_name, name);
      if ((*op2)->documentation)
        fprintf(stream, "%s\n\n", (*op2)->documentation);
      if ((*op2)->operation_documentation)
        fprintf(stream, "%s\n\n", (*op2)->operation_documentation);
      if ((*op2)->input->documentation)
        fprintf(stream, "Input:\n%s\n\n", (*op2)->input->documentation);
      if ((*op2)->input->ext_documentation)
        fprintf(stream, "Input:\n%s\n\n", (*op2)->input->ext_documentation);
      if ((*op2)->output)
      { if ((*op2)->output->documentation)
          fprintf(stream, "Output:\n%s\n\n", (*op2)->output->documentation);
        if ((*op2)->output->ext_documentation)
          fprintf(stream, "Output:\n%s\n\n", (*op2)->output->ext_documentation);
      }
      if ((*op2)->style == document)
        fprintf(stream, "  - SOAP document/literal style\n");
      else
      { if ((*op2)->input->use == literal)
          fprintf(stream, "  - SOAP RPC literal style\n");
        else if ((*op2)->input->encodingStyle)
          fprintf(stream, "  - SOAP RPC encodingStyle=\"%s\"\n", (*op2)->input->encodingStyle);
        else
          fprintf(stream, "  - SOAP RPC encoded\n");
      }
      if ((*op2)->output)
      { if ((*op2)->input->use != (*op2)->output->use)
        { if ((*op2)->output->use == literal)
            fprintf(stream, "  - SOAP RPC literal response\n");
          else if ((*op2)->output->encodingStyle)
            fprintf(stream, "  - SOAP RPC response encodingStyle=\"%s\"\n", (*op2)->output->encodingStyle);
          else
            fprintf(stream, "  - SOAP RPC encoded response\n");
        }
      }
      if ((*op2)->soapAction)
        if (*(*op2)->soapAction)
          fprintf(stream, "  - SOAP action=\"%s\"\n", (*op2)->soapAction);
      for (vector<Message*>::const_iterator message = (*op2)->fault.begin(); message != (*op2)->fault.end(); ++message)
        if ((*message)->message && (*message)->message->name)
          fprintf(stream, "  - SOAP Fault: %s\n", (*message)->name);
      for (vector<soap__header>::const_iterator inputheader = (*op2)->input->header.begin(); inputheader != (*op2)->input->header.end(); ++inputheader)
        if ((*inputheader).part)
          fprintf(stream, "  - Request message has mandatory header part: %s\n", types.aname(NULL, (*inputheader).namespace_, (*inputheader).part));
      if ((*op2)->input->multipartRelated)
        fprintf(stream, "  - Request message has MIME multipart/related attachments\n");
      if ((*op2)->input->layout)
        fprintf(stream, "  - Request message has DIME attachments in compliance with %s\n", (*op2)->input->layout);
      if ((*op2)->output)
        for (vector<soap__header>::const_iterator outputheader = (*op2)->output->header.begin(); outputheader != (*op2)->output->header.end(); ++outputheader)
          if ((*outputheader).part)
            fprintf(stream, "  - Response message has mandatory header part: %s\n", types.aname(NULL, (*outputheader).namespace_, (*outputheader).part));
      if ((*op2)->output && (*op2)->output_name && (*op2)->output->multipartRelated)
        fprintf(stream, "  - Response message has MIME multipart/related attachments\n");
      if ((*op2)->output && (*op2)->output_name && (*op2)->output->layout)
        fprintf(stream, "  - Response message has DIME attachments in compliance with %s\n", (*op2)->output->layout);
      fprintf(stream, "\nC stub function (defined in soapClient.c[pp]):\n@code\n  int soap_call_%s(struct soap *soap,\n    NULL, // char *endpoint = NULL selects default endpoint for this operation\n    NULL, // char *action = NULL selects default action for this operation", (*op2)->input_name);
      (*op2)->input->generate(types, ",", false, false);
      if ((*op2)->output && (*op2)->output_name)
      { if (flag)
        { // Shortcut: do not generate wrapper struct
          (*op2)->output->generate(types, "", false, false);
        }
        else
          fprintf(stream, "\n    struct %s%s", (*op2)->output_name, cflag ? "*" : "&");
      }
      fprintf(stream, "\n  );\n@endcode\n\n");
      if (!cflag)
      { fprintf(stream, "C++ proxy class (defined in soap%sProxy.h):\n", name);
        fprintf(stream, "  class %s;\n\n", name);
      }
      fprintf(stream, "*/\n\n");
      (*op2)->generate(types);
    }
  }
}

////////////////////////////////////////////////////////////////////////////////
//
//	Operation methods
//
////////////////////////////////////////////////////////////////////////////////

void Operation::generate(Types &types)
{ bool flag = false, anonymous = parameterOrder != NULL;
  const char *method_name = strstr(input_name + 1, "__") + 2;
  if (!method_name)
    method_name = input_name;
  if (style == document)
    fprintf(stream, serviceformat, prefix, "method-style", method_name, "document");
  else
    fprintf(stream, serviceformat, prefix, "method-style", method_name, "rpc");
  if (input->use == literal)
    fprintf(stream, serviceformat, prefix, "method-encoding", method_name, "literal");
  else if (input->encodingStyle)
    fprintf(stream, serviceformat, prefix, "method-encoding", method_name, input->encodingStyle);
  else
    fprintf(stream, serviceformat, prefix, "method-encoding", method_name, "encoded");
  if (output)
  { if (input->use != output->use)
    { if (output->use == literal)
        fprintf(stream, serviceformat, prefix, "method-response-encoding", method_name, "literal");
      else if (output->encodingStyle)
        fprintf(stream, serviceformat, prefix, "method-response-encoding", method_name, output->encodingStyle);
      else
        fprintf(stream, serviceformat, prefix, "method-response-encoding", method_name, "encoded");
    }
    if (style == rpc && input->URI && output->URI && strcmp(input->URI, output->URI))
      fprintf(stream, schemaformat, types.nsprefix(NULL, output->URI), "namespace", output->URI);
  }
  if (soapAction)
    if (*soapAction)
      fprintf(stream, serviceformat, prefix, "method-action", method_name, soapAction);
    else
      fprintf(stream, serviceformat, prefix, "method-action", method_name, "\"\"");
  for (vector<Message*>::const_iterator message = fault.begin(); message != fault.end(); ++message)
    if ((*message)->message && (*message)->message->name)
      fprintf(stream, serviceformat, prefix, "method-fault", method_name, (*message)->name);
  // TODO: add headerfault directives
  for (vector<soap__header>::const_iterator inputheader = input->header.begin(); inputheader != input->header.end(); ++inputheader)
    if ((*inputheader).part)
      fprintf(stream, serviceformat, prefix, "method-input-header-part", method_name, types.aname(NULL, (*inputheader).namespace_, (*inputheader).part));
  if (output)
    for (vector<soap__header>::const_iterator outputheader = output->header.begin(); outputheader != output->header.end(); ++outputheader)
      if ((*outputheader).part)
        fprintf(stream, serviceformat, prefix, "method-output-header-part", method_name, types.aname(NULL, (*outputheader).namespace_, (*outputheader).part));
  if (output_name)
  { flag = (style == document && output->message && output->message->part.size() == 1);
    if (flag && input->message && (*output->message->part.begin()).element)
      for (vector<wsdl__part>::const_iterator part = input->message->part.begin(); part != input->message->part.end(); ++part)
        if ((*part).element && !strcmp((*part).element, (*output->message->part.begin()).element))
          flag = false;
  }
  fprintf(stream, "int %s(", input_name);
  input->generate(types, ",", anonymous, true);
  if (output_name)
  { if (flag)
    { // Shortcut: do not generate wrapper struct
      output->generate(types, "", anonymous, true);
      fprintf(stream, " );\n");
    }
    else
    { fprintf(stream, "\n    struct %s%s );\n", output_name, cflag ? "*" : "&");
    }
  }
  else
    fprintf(stream, " void );\n");
}

////////////////////////////////////////////////////////////////////////////////
//
//	Message methods
//
////////////////////////////////////////////////////////////////////////////////

void Message::generate(Types &types, const char *sep, bool anonymous, bool remark)
{ if (message)
  { for (vector<wsdl__part>::const_iterator part = message->part.begin(); part != message->part.end(); ++part)
    { if ((*part).name)
      { if (remark && (*part).documentation)
          comment("", (*part).name, "parameter", (*part).documentation);
        else
          fprintf(stream, "\n");
        if ((*part).elementPtr())
        { const char *name, *type, *URI;
          name = (*part).elementPtr()->name;
          /* comment out to use a type that refers to an element defined with typedef */
          if ((*part).elementPtr()->type)
            type = (*part).elementPtr()->type;
          else
          /* */
            type = name;
          if ((*part).elementPtr()->schemaPtr())
            URI = (*part).elementPtr()->schemaPtr()->targetNamespace;
          else
            URI = NULL;
          fprintf(stream, anonymous ? anonformat : paraformat, types.tname(NULL, URI, type), types.aname(NULL, URI, name), sep);
        }
        else if ((*part).type)
        { if (use == literal)
            fprintf(stderr, "Warning: part '%s' uses literal style and must refer to an element rather than a type\n", (*part).name);
	  fprintf(stream, anonymous ? anonformat : paraformat, types.tname(NULL, NULL, (*part).type), types.aname(NULL, NULL, (*part).name), sep);
        }
        else
          fprintf(stderr, "Error: no wsdl:definitions/message/part/@type in part '%s'\n", (*part).name);
      }
      else
        fprintf(stderr, "Error: no part name in message '%s'\n", message->name?message->name:"");
    }
  }
  else
    fprintf(stderr, "Error: no wsdl:definitions/message\n");
}

////////////////////////////////////////////////////////////////////////////////
//
//	Miscellaneous
//
////////////////////////////////////////////////////////////////////////////////

static void comment(const char *start, const char *middle, const char *end, const char *text)
{ if (text)
  { if (strchr(text, '\r') || strchr(text, '\n'))
      fprintf(stream, "\n/** %s %s %s documentation:\n%s\n*/\n\n", start, middle, end, text);
    else
      fprintf(stream, "\n/// %s %s %s: %s\n", start, middle, end, text);
  }
}

