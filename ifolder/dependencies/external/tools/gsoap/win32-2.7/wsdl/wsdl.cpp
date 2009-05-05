/*

wsdl.cpp

WSDL 1.1 binding schema implementation

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

#include "wsdlH.h"

#ifdef WITH_NONAMESPACES
extern struct Namespace namespaces[];
#endif

using namespace std;

const char *qname_token(const char *QName, const char *URI)
{ if (QName && URI && *QName == '"') // QNames are stored in the format "URI":name, unless the URI is in the nsmap
  { int n = strlen(URI);
    if (!strncmp(QName + 1, URI, n) && QName[n + 1] == '"')
      return QName + n + 3;
  }
  else if (QName && (!URI || !*URI) && *QName != '"' && !strchr(QName, ':')) // Empty namespace
    return QName;
  return NULL;
}

int is_builtin_qname(const char *QName)
{ return iflag || (QName ? *QName != '"' : 0); // if the QName does not start with a ", it must be in the nsmap
}

////////////////////////////////////////////////////////////////////////////////
//
//	wsdl
//
////////////////////////////////////////////////////////////////////////////////

int warn_ignore(struct soap*, const char*);
int show_ignore(struct soap*, const char*);

wsdl__definitions::wsdl__definitions()
{ soap = soap_new1(SOAP_XML_TREE | SOAP_C_UTFSTRING);
#ifdef WITH_NONAMESPACES
  soap_set_namespaces(soap, namespaces);
#endif
  soap_default(soap);
  if (vflag)
    soap->fignore = show_ignore;
  else
    soap->fignore = warn_ignore;
  soap->encodingStyle = NULL;
  soap->proxy_host = proxy_host;
  soap->proxy_port = proxy_port;
  name = NULL;
  targetNamespace = "";
  documentation = NULL;
  types = NULL;
}

wsdl__definitions::wsdl__definitions(struct soap *copy, const char *location)
{ soap = soap_copy(copy);
  soap->socket = SOAP_INVALID_SOCKET;
  soap->recvfd = 0;
  soap->sendfd = 1;
  strcpy(soap->host, copy->host);
  soap_default(soap);
  soap->fignore = warn_ignore;
  soap->encodingStyle = NULL;
  read(location);
}

wsdl__definitions::~wsdl__definitions()
{ soap_done(soap);
  free(soap);
}

int wsdl__definitions::get(struct soap *soap)
{ return traverse();
}

int wsdl__definitions::read(const char *location)
{ if (location)
  { if (!strncmp(location, "http://", 7) || !strncmp(location, "https://", 8))
    { fprintf(stderr, "Connecting to '%s' to retrieve WSDL... ", location);
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
      fprintf(stderr, "Connecting to '%s' to retrieve '%s' WSDL... ", URL, location);
      if (soap_connect_command(soap, SOAP_GET, URL, NULL))
      { fprintf(stderr, "connection failed\n");
        exit(1);
      }
      fprintf(stderr, "done\n");
    }
    else
    { fprintf(stderr, "Reading file '%s'\n", location);
      soap->recvfd = open(location, O_RDONLY, 0);
      if (soap->recvfd < 0)
      { fprintf(stderr, "Cannot open '%s'\n", location);
        exit(1);
      }
    }
  }
  if (!soap_begin_recv(soap))
    this->soap_in(soap, "wsdl:definitions", NULL);
  if (soap->error)
  { // deal with sloppy WSDLs that import schemas at the top level rather than importing them in <types>
    if (soap->error == SOAP_TAG_MISMATCH && soap->level == 0)
    { soap_retry(soap);
      xs__schema *schema = new xs__schema(soap);
      schema->soap_in(soap, "xs:schema", NULL);
      if (schema->error())
      { soap_print_fault(schema->soap, stderr);
        soap_print_fault_location(schema->soap, stderr);
        exit(1);
      }
      name = NULL;
      targetNamespace = schema->targetNamespace;
      if (vflag)
        cerr << "Found schema " << (targetNamespace?targetNamespace:"") << " when expecting WSDL" << endl;
      types = new wsdl__types();
      types->documentation = NULL;
      types->xs__schema_.push_back(schema);
      traverse();
    }
    else if ((soap->error >= 301 && soap->error <= 303) || soap->error == 307) // HTTP redirect, socket was closed
    { fprintf(stderr, "Redirected to '%s'\n", soap->endpoint);
      return read(soap_strdup(soap, soap->endpoint));
    }
    else
    { fprintf(stderr, "An error occurred while parsing WSDL from '%s'\n", location?location:"");
      soap_print_fault(soap, stderr);
      soap_print_fault_location(soap, stderr);
      exit(1);
    }
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

int wsdl__definitions::traverse()
{ if (vflag)
    cerr << "wsdl definitions " << (name?name:"") << " " << (targetNamespace?targetNamespace:"") << endl;
  if (!targetNamespace)
  { if (vflag)
      fprintf(stderr, "Warning: WSDL %s has no targetNamespace\n", name?name:"");
    targetNamespace = "";
  }
  // process import first
  for (vector<wsdl__import>::iterator im = import.begin(); im != import.end(); ++im)
    (*im).traverse(*this);
  // merge imported WSDLs
again:
  for (vector<wsdl__import>::iterator i = import.begin(); i != import.end(); ++i)
  { if ((*i).definitionsPtr())
    { for (vector<wsdl__import>::iterator j = (*i).definitionsPtr()->import.begin(); j != (*i).definitionsPtr()->import.end(); ++j)
      { bool found = false;
        for (vector<wsdl__import>::iterator k = import.begin(); k != import.end(); ++k)
        { if ((*j).definitionsPtr() && (*k).definitionsPtr() && !strcmp((*j).definitionsPtr()->targetNamespace, (*k).definitionsPtr()->targetNamespace))
          { found = true;
	    break;
	  }
        }
        if (!found)
        { import.push_back(*j);
          goto again;
        }
      }
    }
  }
  // then process the types
  if (types)
    types->traverse(*this);
  // process messages before portTypes
  for (vector<wsdl__message>::iterator mg = message.begin(); mg != message.end(); ++mg)
    (*mg).traverse(*this);
  // process portTypes before bindings
  for (vector<wsdl__portType>::iterator pt = portType.begin(); pt != portType.end(); ++pt)
    (*pt).traverse(*this);
  // process bindings
  for (vector<wsdl__binding>::iterator bg = binding.begin(); bg != binding.end(); ++bg)
    (*bg).traverse(*this);
  // process services
  for (vector<wsdl__service>::iterator sv = service.begin(); sv != service.end(); ++sv)
    (*sv).traverse(*this);
  if (vflag)
    cerr << "end of wsdl definitions " << (name?name:"") << " " << (targetNamespace?targetNamespace:"") << endl;
  return SOAP_OK;
}

int wsdl__definitions::error()
{ return soap->error;
}

void wsdl__definitions::print_fault()
{ soap_print_fault(soap, stderr);
  soap_print_fault_location(soap, stderr);
}

void wsdl__definitions::builtinType(const char *type)
{ builtinTypeSet.insert(type);
}

void wsdl__definitions::builtinTypes(const SetOfString& types)
{ for (SetOfString::const_iterator tp = types.begin(); tp != types.end(); ++tp)
    builtinTypeSet.insert(*tp);
}

void wsdl__definitions::builtinElement(const char *element)
{ builtinElementSet.insert(element);
}

void wsdl__definitions::builtinElements(const SetOfString& elements)
{ for (SetOfString::const_iterator el = elements.begin(); el != elements.end(); ++el)
   builtinElementSet.insert(*el);
}

void wsdl__definitions::builtinAttribute(const char *attribute)
{ builtinAttributeSet.insert(attribute);
}

void wsdl__definitions::builtinAttributes(const SetOfString& attributes)
{ for (SetOfString::const_iterator at = attributes.begin(); at != attributes.end(); ++at)
    builtinAttributeSet.insert(*at);
}

const SetOfString& wsdl__definitions::builtinTypes() const
{ return builtinTypeSet;
}

const SetOfString& wsdl__definitions::builtinElements() const
{ return builtinElementSet;
}

const SetOfString& wsdl__definitions::builtinAttributes() const
{ return builtinAttributeSet;
}

int wsdl__service::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl service " << (name?name:"") << endl;
  // process ports
  for (vector<wsdl__port>::iterator i = port.begin(); i != port.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

wsdl__port::wsdl__port()
{ bindingRef = NULL;
}

wsdl__port::~wsdl__port()
{ }

int wsdl__port::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl port" << endl;
  // search binding name
  const char *token = qname_token(binding, definitions.targetNamespace);
  bindingRef = NULL;
  if (token)
  { for (vector<wsdl__binding>::iterator binding = definitions.binding.begin(); binding != definitions.binding.end(); ++binding)
    { if ((*binding).name && !strcmp((*binding).name, token))
      { bindingRef = &(*binding);
        if (vflag)
	  cerr << "Found port " << (name?name:"") << " binding " << (token?token:"") << endl;
        break;
      }
    }
  }
  else
  { for (vector<wsdl__import>::iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    { wsdl__definitions *importdefinitions = (*import).definitionsPtr();
      if (importdefinitions)
      { token = qname_token(binding, importdefinitions->targetNamespace);
        if (token)
        { for (vector<wsdl__binding>::iterator binding = importdefinitions->binding.begin(); binding != importdefinitions->binding.end(); ++binding)
          { if ((*binding).name && !strcmp((*binding).name, token))
            { bindingRef = &(*binding);
              if (vflag)
	        cerr << "Found port " << (name?name:"") << " binding " << (token?token:"") << endl;
              break;
            }
          }
	}
      }
    }
  }
  if (!bindingRef)
    cerr << "Warning: could not find port " << (name?name:"") << " binding " << (binding?binding:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  return SOAP_OK;
}

void wsdl__port::bindingPtr(wsdl__binding *binding)
{ bindingRef = binding;
}

wsdl__binding *wsdl__port::bindingPtr() const
{ return bindingRef;
}

wsdl__binding::wsdl__binding()
{ portTypeRef = NULL;
}

wsdl__binding::~wsdl__binding()
{ }

int wsdl__binding::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl binding" << endl;
  const char *token = qname_token(type, definitions.targetNamespace);
  portTypeRef = NULL;
  if (token)
  { for (vector<wsdl__portType>::iterator portType = definitions.portType.begin(); portType != definitions.portType.end(); ++portType)
    { if ((*portType).name && !strcmp((*portType).name, token))
      { portTypeRef = &(*portType);
        if (vflag)
	  cerr << "Found binding " << (name?name:"") << " portType " << (token?token:"") << endl;
        break;
      }
    }
  }
  else
  { for (vector<wsdl__import>::iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    { wsdl__definitions *importdefinitions = (*import).definitionsPtr();
      if (importdefinitions)
      { token = qname_token(type, importdefinitions->targetNamespace);
        if (token)
        { for (vector<wsdl__portType>::iterator portType = importdefinitions->portType.begin(); portType != importdefinitions->portType.end(); ++portType)
          { if ((*portType).name && !strcmp((*portType).name, token))
            { portTypeRef = &(*portType);
              if (vflag)
	        cerr << "Found binding " << (name?name:"") << " portType " << (token?token:"") << endl;
              break;
            }
          }
	}
      }
    }
  }
  if (!portTypeRef)
    cerr << "Warning: could not find binding " << (name?name:"") << " portType " << (type?type:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  for (vector<wsdl__binding_operation>::iterator i = operation.begin(); i != operation.end(); ++i)
    (*i).traverse(definitions, portTypeRef);
  return SOAP_OK;
}

void wsdl__binding::portTypePtr(wsdl__portType *portType)
{ portTypeRef = portType;
}

wsdl__portType *wsdl__binding::portTypePtr() const
{ return portTypeRef;
}

wsdl__binding_operation::wsdl__binding_operation()
{ operationRef = NULL;
}

wsdl__binding_operation::~wsdl__binding_operation()
{ }

int wsdl__binding_operation::traverse(wsdl__definitions& definitions, wsdl__portType *portTypeRef)
{ if (vflag)
    cerr << "wsdl binding operation" << endl;
  if (input)
    input->traverse(definitions);
  if (output)
    output->traverse(definitions);
  for (vector<wsdl__ext_fault>::iterator i = fault.begin(); i != fault.end(); ++i)
    (*i).traverse(definitions);
  operationRef = NULL;
  if (portTypeRef)
  { for (vector<wsdl__operation>::iterator i = portTypeRef->operation.begin(); i != portTypeRef->operation.end(); ++i)
    { if ((*i).name && !strcmp((*i).name, name))
      { operationRef = &(*i);
        if (vflag)
	  cerr << "Found operation " << (name?name:"") << endl;
        break;
      }
    }
  }
  if (!operationRef)
    cerr << "Warning: could not find operation " << (name?name:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  else
  { for (vector<wsdl__ext_fault>::iterator i = fault.begin(); i != fault.end(); ++i)
    { if ((*i).name)
      { for (vector<wsdl__fault>::iterator j = operationRef->fault.begin(); j != operationRef->fault.end(); ++j)
        { if ((*j).name && !strcmp((*j).name, (*i).name))
          { (*i).messagePtr((*j).messagePtr());
            if (vflag)
              cerr << "Found fault " << ((*j).name?(*j).name:"") << " message " << endl;
            break;
          }
        }
      }
      else if ((*i).soap__fault_ && (*i).soap__fault_->name) // try the soap:fault name, this is not elegant, but neither is WSDL 1.1 
      { for (vector<wsdl__fault>::iterator j = operationRef->fault.begin(); j != operationRef->fault.end(); ++j)
        { if ((*j).name && !strcmp((*j).name, (*i).soap__fault_->name))
          { (*i).messagePtr((*j).messagePtr());
            if (vflag)
              cerr << "Found fault " << ((*j).name?(*j).name:"") << " message " << endl;
            break;
          }
        }
      }
      if (!(*i).messagePtr())
        cerr << "Warning: could not find soap:fault message in WSDL definitions " << (definitions.name?definitions.name:"") << " operation " << (name?name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
    }
  }
  return SOAP_OK;
}

void wsdl__binding_operation::operationPtr(wsdl__operation *operation)
{ operationRef = operation;
}

wsdl__operation *wsdl__binding_operation::operationPtr() const
{ return operationRef;
}

int wsdl__ext_input::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl ext input" << endl;
  for (vector<soap__header>::iterator i = soap__header_.begin(); i != soap__header_.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

int wsdl__ext_output::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl ext output" << endl;
  for (vector<soap__header>::iterator i = soap__header_.begin(); i != soap__header_.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

wsdl__ext_fault::wsdl__ext_fault()
{ messageRef = NULL;
}

wsdl__ext_fault::~wsdl__ext_fault()
{ }

int wsdl__ext_fault::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl ext fault" << endl;
  messageRef = NULL;
  return SOAP_OK;
}

void wsdl__ext_fault::messagePtr(wsdl__message *message)
{ messageRef = message;
}

wsdl__message *wsdl__ext_fault::messagePtr() const
{ return messageRef;
}

int wsdl__portType::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl portType" << endl;
  for (vector<wsdl__operation>::iterator i = operation.begin(); i != operation.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

int wsdl__operation::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl operation" << endl;
  if (input)
    input->traverse(definitions);
  if (output)
    output->traverse(definitions);
  for (vector<wsdl__fault>::iterator i = fault.begin(); i != fault.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

wsdl__input::wsdl__input()
{ messageRef = NULL;
}

wsdl__input::~wsdl__input()
{ }

int wsdl__input::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl input" << endl;
  const char *token = qname_token(message, definitions.targetNamespace);
  messageRef = NULL;
  if (token)
  { for (vector<wsdl__message>::iterator message = definitions.message.begin(); message != definitions.message.end(); ++message)
    { if ((*message).name && !strcmp((*message).name, token))
      { messageRef = &(*message);
        if (vflag)
	  cerr << "Found input " << (name?name:"") << " message " << (token?token:"") << endl;
        break;
      }
    }
  }
  else
  { for (vector<wsdl__import>::iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    { wsdl__definitions *importdefinitions = (*import).definitionsPtr();
      if (importdefinitions)
      { token = qname_token(message, importdefinitions->targetNamespace);
        if (token)
        { for (vector<wsdl__message>::iterator message = importdefinitions->message.begin(); message != importdefinitions->message.end(); ++message)
          { if ((*message).name && !strcmp((*message).name, token))
            { messageRef = &(*message);
              if (vflag)
	        cerr << "Found input " << (name?name:"") << " message " << (token?token:"") << endl;
              break;
            }
          }
	}
      }
    }
  }
  if (!messageRef)
    cerr << "Warning: could not find input " << (name?name:"") << " message " << (message?message:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  return SOAP_OK;
}

void wsdl__input::messagePtr(wsdl__message *message)
{ messageRef = message;
}

wsdl__message *wsdl__input::messagePtr() const
{ return messageRef;
}

wsdl__output::wsdl__output()
{ messageRef = NULL;
}

wsdl__output::~wsdl__output()
{ }

int wsdl__output::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl output" << endl;
  const char *token = qname_token(message, definitions.targetNamespace);
  messageRef = NULL;
  if (token)
  { for (vector<wsdl__message>::iterator message = definitions.message.begin(); message != definitions.message.end(); ++message)
    { if ((*message).name && !strcmp((*message).name, token))
      { messageRef = &(*message);
        if (vflag)
	  cerr << "Found output " << (name?name:"") << " message " << (token?token:"") << endl;
        break;
      }
    }
  }
  else
  { for (vector<wsdl__import>::iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    { wsdl__definitions *importdefinitions = (*import).definitionsPtr();
      if (importdefinitions)
      { token = qname_token(message, importdefinitions->targetNamespace);
        if (token)
        { for (vector<wsdl__message>::iterator message = importdefinitions->message.begin(); message != importdefinitions->message.end(); ++message)
          { if ((*message).name && !strcmp((*message).name, token))
            { messageRef = &(*message);
              if (vflag)
	        cerr << "Found output " << (name?name:"") << " message " << (token?token:"") << endl;
              break;
            }
          }
	}
      }
    }
  }
  if (!messageRef)
    cerr << "Warning: could not find output " << (name?name:"") << " message " << (message?message:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  return SOAP_OK;
}

void wsdl__output::messagePtr(wsdl__message *message)
{ messageRef = message;
}

wsdl__message *wsdl__output::messagePtr() const
{ return messageRef;
}

wsdl__fault::wsdl__fault()
{ messageRef = NULL;
}

wsdl__fault::~wsdl__fault()
{ }

int wsdl__fault::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl fault" << endl;
  const char *token = qname_token(message, definitions.targetNamespace);
  messageRef = NULL;
  if (token)
  { for (vector<wsdl__message>::iterator message = definitions.message.begin(); message != definitions.message.end(); ++message)
    { if ((*message).name && !strcmp((*message).name, token))
      { messageRef = &(*message);
        if (vflag)
	  cerr << "Found fault " << (name?name:"") << " message " << (token?token:"") << endl;
        break;
      }
    }
  }
  else
  { for (vector<wsdl__import>::iterator import = definitions.import.begin(); import != definitions.import.end(); ++import)
    { wsdl__definitions *importdefinitions = (*import).definitionsPtr();
      if (importdefinitions)
      { token = qname_token(message, importdefinitions->targetNamespace);
        if (token)
        { for (vector<wsdl__message>::iterator message = importdefinitions->message.begin(); message != importdefinitions->message.end(); ++message)
          { if ((*message).name && !strcmp((*message).name, token))
            { messageRef = &(*message);
              if (vflag)
	        cerr << "Found output " << (name?name:"") << " message " << (token?token:"") << endl;
              break;
            }
          }
	}
      }
    }
  }
  if (!messageRef)
    cerr << "Warning: could not find fault " << (name?name:"") << " message " << (message?message:"") << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  return SOAP_OK;
}

void wsdl__fault::messagePtr(wsdl__message *message)
{ messageRef = message;
}

wsdl__message *wsdl__fault::messagePtr() const
{ return messageRef;
}

int wsdl__message::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl message" << endl;
  for (vector<wsdl__part>::iterator i = part.begin(); i != part.end(); ++i)
    (*i).traverse(definitions);
  return SOAP_OK;
}

wsdl__part::wsdl__part()
{ elementRef = NULL;
  simpleTypeRef = NULL;
  complexTypeRef = NULL;
}

wsdl__part::~wsdl__part()
{ }

int wsdl__part::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl part" << endl;
  elementRef = NULL;
  simpleTypeRef = NULL;
  complexTypeRef = NULL;
  if (definitions.types)
  { for (vector<xs__schema*>::iterator schema = definitions.types->xs__schema_.begin(); schema != definitions.types->xs__schema_.end(); ++schema)
    { const char *token = qname_token(element, (*schema)->targetNamespace);
      if (token)
      { for (vector<xs__element>::iterator element = (*schema)->element.begin(); element != (*schema)->element.end(); ++element)
        { if ((*element).name && !strcmp((*element).name, token))
          { elementRef = &(*element);
            if (vflag)
	      cerr << "Found part " << (name?name:"") << " element " << (token?token:"") << endl;
            break;
          }
        }
      }
      token = qname_token(type, (*schema)->targetNamespace);
      if (token)
      { for (vector<xs__simpleType>::iterator simpleType = (*schema)->simpleType.begin(); simpleType != (*schema)->simpleType.end(); ++simpleType)
        { if ((*simpleType).name && !strcmp((*simpleType).name, token))
          { simpleTypeRef = &(*simpleType);
            if (vflag)
              cerr << "Found part " << (name?name:"") << " simpleType " << (token?token:"") << endl;
            break;
          }
        }
      }
      token = qname_token(type, (*schema)->targetNamespace);
      if (token)
      { for (vector<xs__complexType>::iterator complexType = (*schema)->complexType.begin(); complexType != (*schema)->complexType.end(); ++complexType)
        { if ((*complexType).name && !strcmp((*complexType).name, token))
          { complexTypeRef = &(*complexType);
            if (vflag)
	      cerr << "Found part " << (name?name:"") << " complexType " << (token?token:"") << endl;
            break;
          }
        }
      }
    }
  }
  if (!elementRef && !simpleTypeRef && !complexTypeRef)
  { if (element)
    { if (is_builtin_qname(element))
	definitions.builtinElement(element);
      else
        cerr << "Warning: could not find part " << (name?name:"") << " element " << element << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
    }
    else if (type)
    { if (is_builtin_qname(type))
	definitions.builtinType(type);
      else
        cerr << "Warning: could not find part " << (name?name:"") << " type " << type << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
    }
    else
      cerr << "Warning: could not find part " << (name?name:"") << " element or type" << " in WSDL definitions " << (definitions.name?definitions.name:"") << " namespace " << (definitions.targetNamespace?definitions.targetNamespace:"") << endl;
  }
  return SOAP_OK;
}

void wsdl__part::elementPtr(xs__element *element)
{ elementRef = element;
}

void wsdl__part::simpleTypePtr(xs__simpleType *simpleType)
{ simpleTypeRef = simpleType;
}

void wsdl__part::complexTypePtr(xs__complexType *complexType)
{ complexTypeRef = complexType;
}

xs__element *wsdl__part::elementPtr() const
{ return elementRef;
}

xs__simpleType *wsdl__part::simpleTypePtr() const
{ return simpleTypeRef;
}

xs__complexType *wsdl__part::complexTypePtr() const
{ return complexTypeRef;
}

int wsdl__types::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl types" << endl;
  // import external schemas
again:
  for (vector<xs__schema*>::iterator schema1 = xs__schema_.begin(); schema1 != xs__schema_.end(); ++schema1)
  { for (vector<xs__import>::iterator import = (*schema1)->import.begin(); import != (*schema1)->import.end(); ++import)
    { if ((*import).namespace_ && (*import).schemaLocation)
      { bool found = false;
        for (vector<xs__schema*>::const_iterator schema2 = xs__schema_.begin(); schema2 != xs__schema_.end(); ++schema2)
        { if ((*schema2)->targetNamespace && !strcmp((*import).namespace_, (*schema2)->targetNamespace))
          { found = true;
	    break;
	  }
        }
	if (!found)
	{ if ((*import).schemaPtr())
	  { if (strcmp((*import).schemaPtr()->targetNamespace, (*import).namespace_))
	      cerr << "Schema import namespace " << (*import).namespace_ << " does not correspond to imported targetNamespace " << (*import).schemaPtr()->targetNamespace << endl;
	    else
	    { xs__schema_.push_back((*import).schemaPtr());
	      goto again;
	    }
	  }
	  else
          { struct Namespace *p = definitions.soap->local_namespaces;
            const char *s = (*import).schemaLocation;
            if (s && (*import).namespace_)
            { if (p)
              { for (; p->id; p++)
                { if (p->in)
                  { if (!soap_tag_cmp((*import).namespace_, p->in))
                      break;
	          }
	          if (p->ns)
                  { if (!soap_tag_cmp((*import).namespace_, p->ns))
                      break;
	          }
                }
              }
	      else
	        fprintf(stderr, "Warning: no namespace table\n");
              if (!iflag && (!p || !p->id)) // don't import any of the schemas in the .nsmap table (or when -i option is used)
	      { xs__schema *importschema = new xs__schema(definitions.soap, s);
	        (*import).schemaPtr(importschema);
	        if (!importschema->targetNamespace)
	          importschema->targetNamespace = (*import).namespace_;
	        if (strcmp(importschema->targetNamespace, (*import).namespace_))
	          cerr << "Schema import namespace " << (*import).namespace_ << " does not correspond to imported targetNamespace " << importschema->targetNamespace << endl;
	        else
                { importschema->traverse();
	          xs__schema_.push_back(importschema);
	          goto again;
	        }
              }
	    }
	    else if (vflag)
	      fprintf(stderr, "Warning: no schemaLocation for namespace '%s'\n", (*import).namespace_?(*import).namespace_:"");
	  }
        }
      }
    }
  }
  for (vector<xs__schema*>::iterator schema2 = xs__schema_.begin(); schema2 != xs__schema_.end(); ++schema2)
  { // artificially extend the <import> of each schema to include others so when we traverse schemas we can resolve references
    for (vector<xs__schema*>::iterator importschema = xs__schema_.begin(); importschema != xs__schema_.end(); ++importschema)
    { if (schema2 != importschema)
      { xs__import *import = new xs__import();
        import->namespace_ = (*importschema)->targetNamespace;
        import->schemaPtr(*importschema);
        (*schema2)->import.push_back(*import);
      }
    }
    for (vector<xs__import>::iterator import = (*schema2)->import.begin(); import != (*schema2)->import.end(); ++import)
    { if ((*import).namespace_)
      { bool found = false;
        for (vector<xs__schema*>::const_iterator importschema = xs__schema_.begin(); importschema != xs__schema_.end(); ++importschema)
	{ if ((*importschema)->targetNamespace && !strcmp((*import).namespace_, (*importschema)->targetNamespace))
          { found = true;
	    break;
	  }
        }
	if (!found)
	  cerr << "Schema import namespace " << (*import).namespace_ << " refers to a known external Schema" << endl;
      }
      else
        cerr << "<xs:import> has no namespace" << endl;
    }
  }
  for (vector<xs__schema*>::iterator schema3 = xs__schema_.begin(); schema3 != xs__schema_.end(); ++schema3)
  { (*schema3)->traverse();
    if (vflag)
      for (SetOfString::const_iterator i = (*schema3)->builtinTypes().begin(); i != (*schema3)->builtinTypes().end(); ++i)
        cerr << "Schema builtin type: " << (*i) << endl;
    definitions.builtinTypes((*schema3)->builtinTypes());
    definitions.builtinElements((*schema3)->builtinElements());
    definitions.builtinAttributes((*schema3)->builtinAttributes());
  }
  return SOAP_OK;
}

int wsdl__import::traverse(wsdl__definitions& definitions)
{ if (vflag)
    cerr << "wsdl import" << endl;
  definitionsRef = NULL;
  if (location)
  { // parse imported definitions
    definitionsRef = new wsdl__definitions(definitions.soap, location);
    if (!definitionsRef)
      return SOAP_EOF;
    if (namespace_)
      if (strcmp(namespace_, definitionsRef->targetNamespace))
        cerr << "Import " << location << " namespace " << namespace_ << " does not match imported targetNamespace " << definitionsRef->targetNamespace << endl;
    // merge <types>
    if (definitionsRef->types)
    { if (definitions.types)
      { // import schemas
	/* Removed: now handled by xs__schema::traverse()
        for (vector<xs__schema>::iterator schema = definitions.types->xs__schema_.begin(); schema != definitions.types->xs__schema_.end(); ++schema)
        { for (vector<xs__schema>::iterator importschema = definitionsRef->types->xs__schema_.begin(); importschema != definitionsRef->types->xs__schema_.end(); ++importschema)
	  { xs__import *import = new xs__import();
	    if (vflag)
	      cerr << "Importing schema " << (*importschema).targetNamespace << endl;
	    import->namespace_ = (*importschema).targetNamespace; // the schemaRef pointer will be set when traversing <types>
            (*schema).import.push_back(*import);
	  }
	}
	*/
        // merge <types>, check for duplicates
        for (vector<xs__schema*>::const_iterator importschema = definitionsRef->types->xs__schema_.begin(); importschema != definitionsRef->types->xs__schema_.end(); ++importschema)
	{ bool found = false;
	  for (vector<xs__schema*>::const_iterator schema = definitions.types->xs__schema_.begin(); schema != definitions.types->xs__schema_.end(); ++schema)
	  { if (!strcmp((*importschema)->targetNamespace, (*schema)->targetNamespace))
	    { found = true;
	      break;
	    }
	  }
	  if (!found)
	    definitions.types->xs__schema_.push_back(*importschema);
	}
      }
      else
        definitions.types = definitionsRef->types;
    }
    // combine sets of builtin types, elements, and attributes
    definitions.builtinTypes(definitionsRef->builtinTypes());
    definitions.builtinElements(definitionsRef->builtinElements());
    definitions.builtinAttributes(definitionsRef->builtinAttributes());
  }
  return SOAP_OK;
}

void wsdl__import::definitionsPtr(wsdl__definitions *definitions)
{ definitionsRef = definitions;
}

wsdl__definitions *wsdl__import::definitionsPtr() const
{ return definitionsRef;
}

wsdl__import::wsdl__import()
{ definitionsRef = NULL;
}

wsdl__import::~wsdl__import()
{ // if (definitionsRef)
    // delete definitionsRef;
}

////////////////////////////////////////////////////////////////////////////////
//
//	streams
//
////////////////////////////////////////////////////////////////////////////////

ostream &operator<<(ostream &o, const wsdl__definitions &e)
{ if (!e.soap)
  { struct soap soap;
    soap_init2(&soap, SOAP_IO_DEFAULT, SOAP_XML_TREE | SOAP_C_UTFSTRING);
#ifdef WITH_NONAMESPACES
    soap_set_namespaces(&soap, namespaces);
#endif
    e.soap_serialize(&soap);
    soap_begin_send(&soap);
    e.soap_out(&soap, "wsdl:definitions", 0, NULL);
    soap_end_send(&soap);
    soap_end(&soap);
    soap_done(&soap);
  }
  else
  { ostream *os = e.soap->os;
    e.soap->os = &o;
    e.soap_serialize(e.soap);
    soap_begin_send(e.soap);
    e.soap_out(e.soap, "wsdl:definitions", 0, NULL);
    soap_end_send(e.soap);
    e.soap->os = os;
  }
  return o;
}

istream &operator>>(istream &i, wsdl__definitions &e)
{ if (!e.soap)
    e.soap = soap_new1(SOAP_XML_TREE | SOAP_C_UTFSTRING);
#ifdef WITH_NONAMESPACES
  soap_set_namespaces(e.soap, namespaces);
#endif
  istream *is = e.soap->is;
  e.soap->is = &i;
  if (soap_begin_recv(e.soap)
   || !e.soap_in(e.soap, "wsdl:definitions", NULL)
   || soap_end_recv(e.soap))
  { // handle error? Note: e.soap->error is set and app should check
  }
  e.soap->is = is;
  return i;
}

////////////////////////////////////////////////////////////////////////////////
//
//	Miscellaneous
//
////////////////////////////////////////////////////////////////////////////////

int warn_ignore(struct soap *soap, const char *tag)
{ // We don't warn if the omitted element was an annotation or a documentation in an unexpected place
  if (soap_match_tag(soap, tag, "xs:annotation") && soap_match_tag(soap, tag, "xs:documentation"))
    fprintf(stderr, "Warning: element '%s' at level %d was not recognized and will be ignored\n", tag, soap->level);
  return SOAP_OK;
}

int show_ignore(struct soap *soap, const char *tag)
{ warn_ignore(soap, tag);
  soap_print_fault_location(soap, stderr);
  return SOAP_OK;
}

