/*

schema.cpp

XSD binding schema implementation

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

*/

#include "wsdlH.h"		// cannot include "schemaH.h"

#ifdef WITH_NONAMESPACES
extern struct Namespace namespaces[];
#endif

using namespace std;

extern int warn_ignore(struct soap*, const char*);
extern const char *qname_token(const char*, const char*);
extern int is_builtin_qname(const char*);

////////////////////////////////////////////////////////////////////////////////
//
//	schema
//
////////////////////////////////////////////////////////////////////////////////

xs__schema::xs__schema()
{ soap = soap_new1(SOAP_XML_TREE | SOAP_C_UTFSTRING);
#ifdef WITH_NONAMESPACES
  soap_set_namespaces(soap, namespaces);
#endif
  soap_default(soap);
  soap->fignore = warn_ignore;
  soap->encodingStyle = NULL;
  soap->proxy_host = proxy_host;
  soap->proxy_port = proxy_port;
  targetNamespace = NULL;
  version = NULL;
}

xs__schema::xs__schema(struct soap *copy)
{ soap = soap_copy(copy);
  soap_default(soap);
  soap->fignore = warn_ignore;
  soap->encodingStyle = NULL;
  targetNamespace = NULL;
  version = NULL;
}

xs__schema::xs__schema(struct soap *copy, const char *location)
{ soap = soap_copy(copy);
  soap->socket = SOAP_INVALID_SOCKET;
  soap->recvfd = 0;
  soap->sendfd = 1;
  strcpy(soap->host, copy->host);
  soap_default(soap);
  soap->fignore = warn_ignore;
  soap->encodingStyle = NULL;
  targetNamespace = NULL;
  version = NULL;
  read(location);
}

xs__schema::~xs__schema()
{ }

int xs__schema::traverse()
{ if (vflag)
    cerr << "schema " << (targetNamespace?targetNamespace:"") << endl;
  if (!targetNamespace)
  { if (vflag)
      fprintf(stderr, "Warning: Schema has no targetNamespace\n");
    targetNamespace = "";
  }
  // process include, but should check if not already included!
  for (vector<xs__include>::iterator in = include.begin(); in != include.end(); ++in)
  { (*in).traverse(*this);
    const xs__schema *s = (*in).schemaPtr();
    if (s)
    { import.insert(import.end(), s->import.begin(), s->import.end());
      attribute.insert(attribute.end(), s->attribute.begin(), s->attribute.end());
      element.insert(element.end(), s->element.begin(), s->element.end());
      group.insert(group.end(), s->group.begin(), s->group.end());
      simpleType.insert(simpleType.end(), s->simpleType.begin(), s->simpleType.end());
      complexType.insert(complexType.end(), s->complexType.begin(), s->complexType.end());
    }
  }
  // process import
  for (vector<xs__import>::iterator im = import.begin(); im != import.end(); ++im)
    (*im).traverse(*this);
  // process attributes
  for (vector<xs__attribute>::iterator at = attribute.begin(); at != attribute.end(); ++at)
    (*at).traverse(*this);
  // process elements
  for (vector<xs__element>::iterator el = element.begin(); el != element.end(); ++el)
    (*el).traverse(*this);
  // process groups
  for (vector<xs__group>::iterator gp = group.begin(); gp != group.end(); ++gp)
    (*gp).traverse(*this);
  // process attributeGroups
  for (vector<xs__attributeGroup>::iterator ag = attributeGroup.begin(); ag != attributeGroup.end(); ++ag)
    (*ag).traverse(*this);
  // process simpleTypes
  for (vector<xs__simpleType>::iterator st = simpleType.begin(); st != simpleType.end(); ++st)
    (*st).traverse(*this);
  // process complexTypes
  for (vector<xs__complexType>::iterator ct = complexType.begin(); ct != complexType.end(); ++ct)
    (*ct).traverse(*this);
  if (vflag)
    cerr << "end of schema " << (targetNamespace?targetNamespace:"") << endl;
  return SOAP_OK;
}

int xs__schema::read(const char *location)
{ if (location)
  { if (!strncmp(location, "http://", 7) || !strncmp(location, "https://", 8))
    { fprintf(stderr, "Connecting to '%s' to retrieve schema... ", location);
      if (soap_connect_command(soap, SOAP_GET, location, NULL))
      { fprintf(stderr, "connection failed\n");
        exit(1);
      }
      fprintf(stderr, "done\n");
    }
    else if (*soap->host)
    { char *URL;
      URL = strrchr(soap->path, '/');
      if (URL)
        *URL = '\0';
      URL = (char*)soap_malloc(soap, strlen(soap->host) + strlen(soap->path) + strlen(location) + 32);
      sprintf(URL, "http://%s:%d/%s/%s", soap->host, soap->port, soap->path, location);
      fprintf(stderr, "Connecting to '%s' to retrieve '%s' schema... ", URL, location);
      if (soap_connect_command(soap, SOAP_GET, URL, NULL))
      { fprintf(stderr, "connection failed\n");
        exit(1);
      }
      fprintf(stderr, "done\n");
    }
    else
    { fprintf(stderr, "Reading schema file '%s'\n", location);
      soap->recvfd = open(location, O_RDONLY, 0);
      if (soap->recvfd < 0)
      { fprintf(stderr, "Cannot open '%s' to retrieve schema\n", location);
        exit(1);
      }
    }
  }
  if (!soap_begin_recv(soap))
    this->soap_in(soap, "xs:schema", NULL);
  if ((soap->error >= 301 && soap->error <= 303) || soap->error == 307) // HTTP redirect, socket was closed
  { fprintf(stderr, "Redirected to '%s'\n", soap->endpoint);
    return read(soap_strdup(soap, soap->endpoint));
  }
  if (soap->error)
  { fprintf(stderr, "An error occurred while parsing schema from '%s'\n", location?location:"");
    soap_print_fault(soap, stderr);
    soap_print_fault_location(soap, stderr);
    exit(1);
  }
  soap_end_recv(soap);
  if (soap->recvfd >= 0)
  { close(soap->recvfd);
    soap->recvfd = -1;
  }
  else
    soap_closesock(soap);
  return SOAP_OK;
}

int xs__schema::error()
{ return soap->error;
}

void xs__schema::print_fault()
{ soap_print_fault(soap, stderr);
  soap_print_fault_location(soap, stderr);
}

void xs__schema::builtinType(const char *type)
{ builtinTypeSet.insert(type);
}

void xs__schema::builtinElement(const char *element)
{ builtinElementSet.insert(element);
}

void xs__schema::builtinAttribute(const char *attribute)
{ builtinAttributeSet.insert(attribute);
}

const SetOfString& xs__schema::builtinTypes() const
{ return builtinTypeSet;
}

const SetOfString& xs__schema::builtinElements() const
{ return builtinElementSet;
}

const SetOfString& xs__schema::builtinAttributes() const
{ return builtinAttributeSet;
}

xs__include::xs__include()
{ schemaLocation = NULL;
  schemaRef = NULL;
}

xs__include::~xs__include()
{ }

int xs__include::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema include" << endl;
  if (!schemaRef)
    if (schemaLocation)
      schemaRef = new xs__schema(schema.soap, schemaLocation);
  return SOAP_OK;
}

void xs__include::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema *xs__include::schemaPtr() const
{ return schemaRef;
}

xs__import::xs__import()
{ namespace_ = NULL;
  schemaLocation = NULL;
  schemaRef = NULL;
}

xs__import::~xs__import()
{ }

int xs__import::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema import " << (namespace_?namespace_:"") << endl;
  if (!schemaRef)
  { struct Namespace *p = schema.soap->local_namespaces;
    const char *s = schemaLocation;
    if (s && namespace_)
    { if (p)
      { for (; p->id; p++)
        { if (p->in)
          { if (!soap_tag_cmp(namespace_, p->in))
              break;
	  }
	  if (p->ns)
          { if (!soap_tag_cmp(namespace_, p->ns))
              break;
	  }
        }
      }
      else
	fprintf(stderr, "Warning: no namespace table\n");
      if (!iflag && (!p || !p->id)) // don't import any of the schemas in the .nsmap table (or when -i option is used)
      { schemaRef = new xs__schema(schema.soap, s);
        if (schemaPtr())
        { if (!schemaPtr()->targetNamespace || !*schemaPtr()->targetNamespace)
            schemaPtr()->targetNamespace = namespace_;
	  else if (strcmp(schemaPtr()->targetNamespace, namespace_))
            fprintf(stderr, "Warning: schema import '%s' with schema targetNamespace '%s' mismatch\n", namespace_?namespace_:"", schemaPtr()->targetNamespace);
	  schemaPtr()->traverse();
        }
      }
    }
    else if (vflag)
      fprintf(stderr, "Warning: no schemaLocation for namespace import '%s'\n", namespace_?namespace_:"");
  }
  return SOAP_OK;
}

void xs__import::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema *xs__import::schemaPtr() const
{ return schemaRef;
}

xs__attribute::xs__attribute()
{ schemaRef = NULL;
  attributeRef = NULL;
  simpleTypeRef = NULL;
}

xs__attribute::~xs__attribute()
{ }

int xs__attribute::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema attribute" << endl;
  schemaRef = &schema;
  const char *token = qname_token(ref, schema.targetNamespace);
  attributeRef = NULL;
  if (token)
  { for (vector<xs__attribute>::iterator i = schema.attribute.begin(); i != schema.attribute.end(); ++i)
      if (!strcmp((*i).name, token))
      { attributeRef = &(*i);
        if (vflag)
	  cerr << "Found attribute " << (name?name:"") << " ref " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(ref, s->targetNamespace);
        if (token)
        { for (vector<xs__attribute>::iterator j = s->attribute.begin(); j != s->attribute.end(); ++j)
            if (!strcmp((*j).name, token))
            { attributeRef = &(*j);
              if (vflag)
	        cerr << "Found attribute " << (name?name:"") << " ref " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (simpleType)
  { simpleType->traverse(schema);
    simpleTypeRef = simpleType;
  }
  else
  { token = qname_token(type, schema.targetNamespace);
    simpleTypeRef = NULL;
    if (token)
    { for (vector<xs__simpleType>::iterator i = schema.simpleType.begin(); i != schema.simpleType.end(); ++i)
        if (!strcmp((*i).name, token))
        { simpleTypeRef = &(*i);
          if (vflag)
	    cerr << "Found attribute " << (name?name:"") << " type " << (token?token:"") << endl;
          break;
        }
    }
    else
    { for (vector<xs__import>::iterator i = schema.import.begin(); i != schema.import.end(); ++i)
      { xs__schema *s = (*i).schemaPtr();
        if (s)
        { token = qname_token(type, s->targetNamespace);
          if (token)
          { for (vector<xs__simpleType>::iterator j = s->simpleType.begin(); j != s->simpleType.end(); ++j)
              if (!strcmp((*j).name, token))
              { simpleTypeRef = &(*j);
                if (vflag)
	          cerr << "Found attribute " << (name?name:"") << " type " << (token?token:"") << endl;
                break;
              }
            break;
          }
        }
      }
    }
  }
  if (!attributeRef && !simpleTypeRef)
  { if (ref)
    { if (is_builtin_qname(ref))
        schema.builtinAttribute(ref);
      else
        cerr << "Warning: could not find attribute " << (name?name:"") << " ref " << ref << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else if (type)
    { if (is_builtin_qname(type))
        schema.builtinType(type);
      else
        cerr << "Warning: could not find attribute " << (name?name:"") << " type " << type << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else
      cerr << "Warning: could not find attribute " << (name?name:"") << " ref or type" << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
  }
  return SOAP_OK;
}

void xs__attribute::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema* xs__attribute::schemaPtr() const
{ return schemaRef;
}

void xs__attribute::attributePtr(xs__attribute *attribute)
{ attributeRef = attribute;
}

void xs__attribute::simpleTypePtr(xs__simpleType *simpleType)
{ simpleTypeRef = simpleType;
}

xs__attribute *xs__attribute::attributePtr() const
{ return attributeRef;
}

xs__simpleType *xs__attribute::simpleTypePtr() const
{ return simpleTypeRef;
}

xs__element::xs__element()
{ schemaRef = NULL;
  elementRef = NULL;
  simpleTypeRef = NULL;
  complexTypeRef = NULL;
}

xs__element::~xs__element()
{ }

int xs__element::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema element" << endl;
  schemaRef = &schema;
  const char *token = qname_token(ref, schema.targetNamespace);
  elementRef = NULL;
  if (token)
  { for (vector<xs__element>::iterator i = schema.element.begin(); i != schema.element.end(); ++i)
      if (!strcmp((*i).name, token))
      { elementRef = &(*i);
        if (vflag)
	  cerr << "Found element " << (name?name:"") << " ref " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(ref, s->targetNamespace);
        if (token)
        { for (vector<xs__element>::iterator j = s->element.begin(); j != s->element.end(); ++j)
            if (!strcmp((*j).name, token))
            { elementRef = &(*j);
              if (vflag)
	        cerr << "Found element " << (name?name:"") << " ref " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (simpleType)
  { simpleType->traverse(schema);
    simpleTypeRef = simpleType;
  }
  else
  { token = qname_token(type, schema.targetNamespace);
    simpleTypeRef = NULL;
    if (token)
    { for (vector<xs__simpleType>::iterator i = schema.simpleType.begin(); i != schema.simpleType.end(); ++i)
        if (!strcmp((*i).name, token))
        { simpleTypeRef = &(*i);
          if (vflag)
	    cerr << "Found element " << (name?name:"") << " simpleType " << (token?token:"") << endl;
          break;
        }
    }
    else
    { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
      { xs__schema *s = (*i).schemaPtr();
        if (s)
        { token = qname_token(type, s->targetNamespace);
          if (token)
          { for (vector<xs__simpleType>::iterator j = s->simpleType.begin(); j != s->simpleType.end(); ++j)
              if (!strcmp((*j).name, token))
              { simpleTypeRef = &(*j);
                if (vflag)
	          cerr << "Found element " << (name?name:"") << " simpleType " << (token?token:"") << endl;
                break;
              }
            break;
          }
        }
      }
    }
  }
  if (complexType)
  { complexType->traverse(schema);
    complexTypeRef = complexType;
  }
  else
  { token = qname_token(type, schema.targetNamespace);
    complexTypeRef = NULL;
    if (token)
    { for (vector<xs__complexType>::iterator i = schema.complexType.begin(); i != schema.complexType.end(); ++i)
        if (!strcmp((*i).name, token))
        { complexTypeRef = &(*i);
          if (vflag)
	    cerr << "Found element " << (name?name:"") << " complexType " << (token?token:"") << endl;
          break;
        }
    }
    else
    { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
      { xs__schema *s = (*i).schemaPtr();
        if (s)
        { token = qname_token(type, s->targetNamespace);
          if (token)
          { for (vector<xs__complexType>::iterator j = s->complexType.begin(); j != s->complexType.end(); ++j)
              if (!strcmp((*j).name, token))
              { complexTypeRef = &(*j);
                if (vflag)
	          cerr << "Found element " << (name?name:"") << " complexType " << (token?token:"") << endl;
                break;
              }
            break;
          }
        }
      }
    }
  }
  if (!elementRef && !simpleTypeRef && !complexTypeRef)
  { if (ref)
    { if (is_builtin_qname(ref))
        schema.builtinElement(ref);
      else
        cerr << "Warning: could not find element " << (name?name:"") << " ref " << ref << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else if (type)
    { if (is_builtin_qname(type))
        schema.builtinType(type);
      else
        cerr << "Warning: could not find element " << (name?name:"") << " type " << type << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else
      cerr << "Warning: could not find element " << (name?name:"") << " ref or type" << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
  }
  return SOAP_OK;
}

void xs__element::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema* xs__element::schemaPtr() const
{ return schemaRef;
}

void xs__element::elementPtr(xs__element *element)
{ elementRef = element;
}

void xs__element::simpleTypePtr(xs__simpleType *simpleType)
{ simpleTypeRef = simpleType;
}

void xs__element::complexTypePtr(xs__complexType *complexType)
{ complexTypeRef = complexType;
}

xs__element *xs__element::elementPtr() const
{ return elementRef;
}

xs__simpleType *xs__element::simpleTypePtr() const
{ return simpleTypeRef;
}

xs__complexType *xs__element::complexTypePtr() const
{ return complexTypeRef;
}

xs__simpleType::xs__simpleType()
{ level = 0;
}

xs__simpleType::~xs__simpleType()
{ }

int xs__simpleType::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema simpleType" << endl;
  schemaRef = &schema;
  if (list)
    list->traverse(schema);
  else if (restriction)
    restriction->traverse(schema);
  else if (union_)
    union_->traverse(schema);
  return SOAP_OK;
}

void xs__simpleType::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema *xs__simpleType::schemaPtr() const
{ return schemaRef;
}

int xs__simpleType::baseLevel()
{ if (!level)
  { if (restriction)
    { level = -1;
      if (restriction->simpleTypePtr())
        level = restriction->simpleTypePtr()->baseLevel() + 1;
      else
        level = 2;
    }
    else if (list && list->restriction)
    { level = -1;
      if (list->restriction->simpleTypePtr())
        level = list->restriction->simpleTypePtr()->baseLevel() + 1;
      else
        level = 2;
    }
    else
      level = 1;
  }
  else if (level < 0)
  { cerr << "Cyclic restriction/extension base dependency" << endl;
  }
  return level;
}

xs__complexType::xs__complexType()
{ level = 0;
}

xs__complexType::~xs__complexType()
{ }

int xs__complexType::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema complexType" << endl;
  schemaRef = &schema;
  if (simpleContent)
    simpleContent->traverse(schema);
  else if (complexContent)
    complexContent->traverse(schema);
  else if (all)
    all->traverse(schema);
  else if (choice)
    choice->traverse(schema);
  else if (sequence)
    sequence->traverse(schema);
  else if (any)
    any->traverse(schema);
  for (vector<xs__attribute>::iterator at = attribute.begin(); at != attribute.end(); ++at)
    (*at).traverse(schema);
  for (vector<xs__attributeGroup>::iterator ag = attributeGroup.begin(); ag != attributeGroup.end(); ++ag)
    (*ag).traverse(schema);
  return SOAP_OK;
}

void xs__complexType::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema *xs__complexType::schemaPtr() const
{ return schemaRef;
}

int xs__complexType::baseLevel()
{ if (!level)
  { if (simpleContent)
    { if (simpleContent->restriction)
      { level = -1;
        if (simpleContent->restriction->simpleTypePtr())
          level = simpleContent->restriction->simpleTypePtr()->baseLevel() + 1;
        else
	  level = 2;
      }
      else if (simpleContent->extension)
      { level = -1;
        if (simpleContent->extension->simpleTypePtr())
          level = simpleContent->extension->simpleTypePtr()->baseLevel() + 1;
        else
	  level = 2;
      }
    }
    else if (complexContent)
    { if (complexContent->restriction)
      { level = -1;
        if (complexContent->restriction->simpleTypePtr())
          level = complexContent->restriction->simpleTypePtr()->baseLevel() + 1;
        else if (complexContent->restriction->complexTypePtr())
          level = complexContent->restriction->complexTypePtr()->baseLevel() + 1;
        else
	  level = 2;
      }
      else if (complexContent->extension)
      { level = -1;
        if (complexContent->extension->simpleTypePtr())
          level = complexContent->extension->simpleTypePtr()->baseLevel() + 1;
        else if (complexContent->extension->complexTypePtr())
          level = complexContent->extension->complexTypePtr()->baseLevel() + 1;
        else
	  level = 2;
      }
    }
    else
      level = 1;
  }
  else if (level < 0)
  { cerr << "Cyclic restriction/extension base dependency" << endl;
  }
  return level;
}

int xs__simpleContent::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema simpleContent" << endl;
  if (extension)
    extension->traverse(schema);
  else if (restriction)
    restriction->traverse(schema);
  return SOAP_OK;
}

int xs__complexContent::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema complexContent" << endl;
  if (extension)
    extension->traverse(schema);
  else if (restriction)
    restriction->traverse(schema);
  return SOAP_OK;
}

xs__extension::xs__extension()
{ simpleTypeRef = NULL;
  complexTypeRef = NULL;
}

xs__extension::~xs__extension()
{ }

int xs__extension::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema extension" << endl;
  if (group)
    group->traverse(schema);
  else if (all)
    all->traverse(schema);
  else if (choice)
    choice->traverse(schema);
  else if (sequence)
    sequence->traverse(schema);
  for (vector<xs__attribute>::iterator at = attribute.begin(); at != attribute.end(); ++at)
    (*at).traverse(schema);
  const char *token = qname_token(base, schema.targetNamespace);
  simpleTypeRef = NULL;
  if (token)
  { for (vector<xs__simpleType>::iterator i = schema.simpleType.begin(); i != schema.simpleType.end(); ++i)
      if (!strcmp((*i).name, token))
      { simpleTypeRef = &(*i);
        if (vflag)
	  cerr << "Found extension base type " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(base, s->targetNamespace);
        if (token)
        { for (vector<xs__simpleType>::iterator j = s->simpleType.begin(); j != s->simpleType.end(); ++j)
            if (!strcmp((*j).name, token))
            { simpleTypeRef = &(*j);
              if (vflag)
	        cerr << "Found extension base type " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  token = qname_token(base, schema.targetNamespace);
  complexTypeRef = NULL;
  if (token)
  { for (vector<xs__complexType>::iterator i = schema.complexType.begin(); i != schema.complexType.end(); ++i)
      if (!strcmp((*i).name, token))
      { complexTypeRef = &(*i);
        if (vflag)
	  cerr << "Found extension base type " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(base, s->targetNamespace);
        if (token)
        { for (vector<xs__complexType>::iterator j = s->complexType.begin(); j != s->complexType.end(); ++j)
            if (!strcmp((*j).name, token))
            { complexTypeRef = &(*j);
              if (vflag)
	        cerr << "Found extension base type " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (!simpleTypeRef && !complexTypeRef)
  { if (base)
    { if (is_builtin_qname(base))
        schema.builtinType(base);
      else
        cerr << "Warning: could not find extension base type " << base << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else
      cerr << "Extension has no base" << endl;
  }
  return SOAP_OK;
}

void xs__extension::simpleTypePtr(xs__simpleType *simpleType)
{ simpleTypeRef = simpleType;
}

void xs__extension::complexTypePtr(xs__complexType *complexType)
{ complexTypeRef = complexType;
}

xs__simpleType *xs__extension::simpleTypePtr() const
{ return simpleTypeRef;
}

xs__complexType *xs__extension::complexTypePtr() const
{ return complexTypeRef;
}

xs__restriction::xs__restriction()
{ simpleTypeRef = NULL;
  complexTypeRef = NULL;
}

xs__restriction::~xs__restriction()
{ }

int xs__restriction::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema restriction" << endl;
  if (group)
    group->traverse(schema);
  else if (all)
    all->traverse(schema);
  else if (choice)
    choice->traverse(schema);
  else if (sequence)
    sequence->traverse(schema);
  else
  { for (vector<xs__enumeration>::iterator en = enumeration.begin(); en != enumeration.end(); ++en)
      (*en).traverse(schema);
    for (vector<xs__pattern>::iterator pn = pattern.begin(); pn != pattern.end(); ++pn)
      (*pn).traverse(schema);
  }
  for (vector<xs__attribute>::iterator at = attribute.begin(); at != attribute.end(); ++at)
    (*at).traverse(schema);
  const char *token = qname_token(base, schema.targetNamespace);
  simpleTypeRef = NULL;
  if (token)
  { for (vector<xs__simpleType>::iterator i = schema.simpleType.begin(); i != schema.simpleType.end(); ++i)
      if (!strcmp((*i).name, token))
      { simpleTypeRef = &(*i);
        if (vflag)
	  cerr << "Found restriction base type " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(base, s->targetNamespace);
        if (token)
        { for (vector<xs__simpleType>::iterator j = s->simpleType.begin(); j != s->simpleType.end(); ++j)
            if (!strcmp((*j).name, token))
            { simpleTypeRef = &(*j);
              if (vflag)
	        cerr << "Found restriction base type " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  token = qname_token(base, schema.targetNamespace);
  complexTypeRef = NULL;
  if (token)
  { for (vector<xs__complexType>::iterator i = schema.complexType.begin(); i != schema.complexType.end(); ++i)
      if (!strcmp((*i).name, token))
      { complexTypeRef = &(*i);
        if (vflag)
	  cerr << "Found restriction base type " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(base, s->targetNamespace);
        if (token)
        { for (vector<xs__complexType>::iterator j = s->complexType.begin(); j != s->complexType.end(); ++j)
            if (!strcmp((*j).name, token))
            { complexTypeRef = &(*j);
              if (vflag)
	        cerr << "Found restriction base type " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (!simpleTypeRef && !complexTypeRef)
  { if (base)
    { if (is_builtin_qname(base))
        schema.builtinType(base);
      else
        cerr << "Warning: could not find restriction base type " << base << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
    }
    else
      cerr << "Restriction has no base" << endl;
  }
  return SOAP_OK;
}

void xs__restriction::simpleTypePtr(xs__simpleType *simpleType)
{ simpleTypeRef = simpleType;
}

void xs__restriction::complexTypePtr(xs__complexType *complexType)
{ complexTypeRef = complexType;
}

xs__simpleType *xs__restriction::simpleTypePtr() const
{ return simpleTypeRef;
}

xs__complexType *xs__restriction::complexTypePtr() const
{ return complexTypeRef;
}

xs__list::xs__list()
{ itemTypeRef = NULL;
}

xs__list::~xs__list()
{ }

int xs__list::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema list" << endl;
  if (restriction)
    restriction->traverse(schema);
  for (vector<xs__simpleType>::iterator i = simpleType.begin(); i != simpleType.end(); ++i)
    (*i).traverse(schema);
  itemTypeRef = NULL;
  const char *token = qname_token(itemType, schema.targetNamespace);
  if (token)
  { for (vector<xs__simpleType>::iterator i = schema.simpleType.begin(); i != schema.simpleType.end(); ++i)
      if (!strcmp((*i).name, token))
      { itemTypeRef = &(*i);
        if (vflag)
	  cerr << "Found list itemType " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(itemType, s->targetNamespace);
        if (token)
        { for (vector<xs__simpleType>::iterator j = s->simpleType.begin(); j != s->simpleType.end(); ++j)
            if (!strcmp((*j).name, token))
            { itemTypeRef = &(*j);
              if (vflag)
	        cerr << "Found list itemType " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (itemType && !itemTypeRef)
  { if (is_builtin_qname(itemType))
      schema.builtinType(itemType);
    else
      cerr << "Warning: could not find list itemType " << itemType << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
  }
  return SOAP_OK;
}

void xs__list::itemTypePtr(xs__simpleType *simpleType)
{ itemTypeRef = simpleType;
}

xs__simpleType *xs__list::itemTypePtr() const
{ return itemTypeRef;
}

int xs__union::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema union" << endl;
  for (vector<xs__simpleType>::iterator i = simpleType.begin(); i != simpleType.end(); ++i)
    (*i).traverse(schema);
  return SOAP_OK;
}

int xs__all::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema all" << endl;
  for (vector<xs__element>::iterator i = element.begin(); i != element.end(); ++i)
    (*i).traverse(schema);
  return SOAP_OK;
}

int xs__choice::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema choice" << endl;
  for (vector<xs__element>::iterator el = element.begin(); el != element.end(); ++el)
    (*el).traverse(schema);
  for (vector<xs__group>::iterator gp = group.begin(); gp != group.end(); ++gp)
    (*gp).traverse(schema);
  for (vector<xs__sequence*>::iterator sq = sequence.begin(); sq != sequence.end(); ++sq)
    (*sq)->traverse(schema);
  for (vector<xs__any>::iterator an = any.begin(); an != any.end(); ++an)
    (*an).traverse(schema);
  return SOAP_OK;
}

int xs__sequence::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema sequence" << endl;
  for (vector<xs__element>::iterator el = element.begin(); el != element.end(); ++el)
    (*el).traverse(schema);
  for (vector<xs__group>::iterator gp = group.begin(); gp != group.end(); ++gp)
    (*gp).traverse(schema);
  for (vector<xs__choice>::iterator ch = choice.begin(); ch != choice.end(); ++ch)
    (*ch).traverse(schema);
  for (vector<xs__sequence*>::iterator sq = sequence.begin(); sq != sequence.end(); ++sq)
    (*sq)->traverse(schema);
  for (vector<xs__any>::iterator an = any.begin(); an != any.end(); ++an)
    (*an).traverse(schema);
  return SOAP_OK;
}

xs__attributeGroup::xs__attributeGroup()
{ schemaRef = NULL;
  attributeGroupRef = NULL;
}

xs__attributeGroup::~xs__attributeGroup()
{ }

int xs__attributeGroup::traverse(xs__schema& schema)
{ if (vflag)
    cerr << "attributeGroup" << endl;
  for (vector<xs__attribute>::iterator at = attribute.begin(); at != attribute.end(); ++at)
    (*at).traverse(schema);
  return SOAP_OK;
}

void xs__attributeGroup::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

void xs__attributeGroup::attributeGroupPtr(xs__attributeGroup *attributeGroup)
{ attributeGroupRef = attributeGroup;
}

xs__schema *xs__attributeGroup::schemaPtr() const
{ return schemaRef;
}

xs__attributeGroup *xs__attributeGroup::attributeGroupPtr() const
{ return attributeGroupRef;
}

int xs__any::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema any" << endl;
  for (vector<xs__element>::iterator i = element.begin(); i != element.end(); ++i)
    (*i).traverse(schema);
  return SOAP_OK;
}

xs__group::xs__group()
{ schemaRef = NULL;
  groupRef = NULL;
}

xs__group::~xs__group()
{ }

int xs__group::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema group" << endl;
  schemaRef = &schema;
  if (all)
    all->traverse(schema);
  else if (choice)
    choice->traverse(schema);
  else if (sequence)
    sequence->traverse(schema);
  const char *token = qname_token(ref, schema.targetNamespace);
  groupRef = NULL;
  if (token)
  { for (vector<xs__group>::iterator i = schema.group.begin(); i != schema.group.end(); ++i)
      if (!strcmp((*i).name, token))
      { groupRef = &(*i);
        if (vflag)
	    cerr << "Found group " << (name?name:"") << " ref " << (token?token:"") << endl;
        break;
      }
  }
  else
  { for (vector<xs__import>::const_iterator i = schema.import.begin(); i != schema.import.end(); ++i)
    { xs__schema *s = (*i).schemaPtr();
      if (s)
      { token = qname_token(ref, s->targetNamespace);
        if (token)
        { for (vector<xs__group>::iterator j = s->group.begin(); j != s->group.end(); ++j)
            if (!strcmp((*j).name, token))
            { groupRef = &(*j);
              if (vflag)
	          cerr << "Found group " << (name?name:"") << " ref " << (token?token:"") << endl;
              break;
            }
          break;
        }
      }
    }
  }
  if (!groupRef)
    cerr << "Warning: could not find group " << (name?name:"") << " ref " << (ref?ref:"") << " in schema " << (schema.targetNamespace?schema.targetNamespace:"") << endl;
  return SOAP_OK;
}

void xs__group::schemaPtr(xs__schema *schema)
{ schemaRef = schema;
}

xs__schema* xs__group::schemaPtr() const
{ return schemaRef;
}

void xs__group::groupPtr(xs__group *group)
{ groupRef = group;
}

xs__group* xs__group::groupPtr() const
{ return groupRef;
}

int xs__enumeration::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema enumeration" << endl;
  return SOAP_OK;
}

int xs__pattern::traverse(xs__schema &schema)
{ if (vflag)
    cerr << "schema pattern" << endl;
  return SOAP_OK;
}

////////////////////////////////////////////////////////////////////////////////
//
//	I/O
//
////////////////////////////////////////////////////////////////////////////////

ostream &operator<<(ostream &o, const xs__schema &e)
{ if (!e.soap)
  { struct soap soap;
    soap_init2(&soap, SOAP_IO_DEFAULT, SOAP_XML_TREE | SOAP_C_UTFSTRING);
#ifdef WITH_NONAMESPACES
    soap_set_namespaces(&soap, namespaces);
#endif
    e.soap_serialize(&soap);
    soap_begin_send(&soap);
    e.soap_out(&soap, "xs:schema", 0, NULL);
    soap_end_send(&soap);
    soap_end(&soap);
    soap_done(&soap);
  }
  else
  { ostream *os = e.soap->os;
    e.soap->os = &o;
    e.soap_serialize(e.soap);
    soap_begin_send(e.soap);
    e.soap_out(e.soap, "xs:schema", 0, NULL);
    soap_end_send(e.soap);
    e.soap->os = os;
  }
  return o;
}

istream &operator>>(istream &i, xs__schema &e)
{ if (!e.soap)
    e.soap = soap_new();
#ifdef WITH_NONAMESPACES
  soap_set_namespaces(e.soap, namespaces);
#endif
  istream *is = e.soap->is;
  e.soap->is = &i;
  if (soap_begin_recv(e.soap)
   || !e.soap_in(e.soap, "xs:schema", NULL)
   || soap_end_recv(e.soap))
  { // handle error? Note: e.soap->error is set and app should check
  }
  e.soap->is = is;
  return i;
}

