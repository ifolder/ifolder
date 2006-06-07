/*

dom.c[pp]

gSOAP DOM implementation

gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.

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

	This level-2 DOM parser features automatic XML namespace handling and
	allows mixing with gSOAP's data type (de)serializers.

	DOM element nodes and attribute nodes can have an optional namespace
	names. The namespace prefix and name bindings (xmlns in XML) are
	automatically resolved.
	
	The DOM elements can be used anywhere within a C/C++ data structure and
	also as the arguments to a service method. For example:
	  struct SOAP_ENV__Header
	  { xsd__anyType *authentication;
	  };
	  int ns__test(xsd__anyType in, xsd__anyType *out);
	which defines a custom SOAP Header with an authentication element
	parsed as a DOM.

	DOM node fields:
	           *elts	optional child elements (list of DOM nodes)
	           *atts	optional attributes of element node
	const char *nstr	element namespace name (URI)
	      char *name	element name with optional ns: prefix
	      char *data	optional element data (see comment below)
           wchar_t *wide	optional element data (see comment below)
	       int  type	optional type (SOAP_TYPE_X as defined in soapH.h)
	      void *node	and optional element pointer to C/C++ data type

	DOM Parsing:
	The namespace name (URI) parsing is smart and fills the 'nstr' field
	with the namespace URIs. The algorithm checks the names in the nsmap
	table first. After parsing, the 'nstr' namespace strings will point to
	the nsmap table contents in case the names (URIs) match a table entry.
	Otherwise, the names are dynamically allocated. This enables quick
	pointer-based checks on the DOM node's namespace names by comparing the
	pointer to the namespace table entries 'namespaces[i].ns'.

	Character data parsing:
	The parser fills the 'wide' string fields of the DOM nodes only (the
	'wide' fields contain Unicode XML cdata), unless the input-mode flag
	SOAP_C_UTFSTRING or SOAP_C_MBSTRING is set using soap_init2() or
	soap_set_imode(). In that case the 'data' fields are set with UTF8
	contents or multibyte character strings.

	The following input-mode flags (set with soap_set_imode()) control the
	parsing of C/C++ data types (stored in the 'node' and 'type' fields):

	default:	only elements with an 'id' attribute are deserialized
			as C/C++ data types (when a deserializer is available)
	SOAP_DOM_TREE:	never deserialize C/C++ types (produces DOM tree)
	SOAP_DOM_NODE:	always deserialize C/C++ types (when a deserializer is
			available and the xsi:type attribute is present in the
			XML node or the XML element tag matches the type name)

	The following output-mode flag (set with soap_set_omode()) controls the
	serialization of XML:

	SOAP_XML_CANONICAL:	serialize XML in canonical form

	The entire deserialized DOM is freed with
	soap_destroy(DOM.soap);
	soap_end(DOM.soap);

	Examples (XML parsing and generation):

	struct soap *soap = soap_new();
	xsd__anyType dom(soap); // need a soap struct to parse XML using '>>'
	cin >> dom; // parse XML
	if (soap->error)
	  ... input error ...
	soap_destroy(soap); // delete DOM
	soap_end(soap); // delete data
	soap_done(soap); // finalize
	free(soap);

	struct soap *soap = soap_new();
	xsd__anyType dom(soap, ...); // populate the DOM
	cout << dom; // print 
	if (soap->error)
	  ... output error ...
	soap_destroy(soap); // clean up objects
	soap_end(soap); // clean up
	soap_done(soap); // finalize
	free(soap);

	To retain the DOM but remove all other data, use:
	dom.unlink();

	Compile:
	soapcpp2 dom.h
	g++ -c dom.cpp

	To use a DOM in your Web service application, add to the gSOAP header
	file that defines your service:
	#import "dom.h"
	Then use the xsd__anyType to refer to the DOM in your data structures.

	Link the application with dom.o

	Development note:
	Reused the gSOAP struct soap id hash table for handling namespace
	bindings when transmitting DOMs

Changes:
	Renamed __type to type (correction)
	dom.c, dom++.cpp, and dom.cpp are equivalent
	Renamed SOAP_XML_TREE to SOAP_DOM_TREE
	Renamed SOAP_XML_GRAPH to SOAP_DOM_NODE

TODO:	Improve mixed content handling
*/

#include "stdsoap2.h"

SOAP_FMAC3 void SOAP_FMAC4 soap_serialize_xsd__anyType(struct soap*, struct soap_dom_element const*);
SOAP_FMAC1 void SOAP_FMAC2 soap_mark_xsd__anyType(struct soap*, const struct soap_dom_element *);
SOAP_FMAC1 void SOAP_FMAC2 soap_default_xsd__anyType(struct soap*, struct soap_dom_element *);
SOAP_FMAC3 int SOAP_FMAC4 soap_put_xsd__anyType(struct soap*, const struct soap_dom_element *, const char*, const char*);
SOAP_FMAC1 int SOAP_FMAC2 soap_out_xsd__anyType(struct soap*, const char*, int, const struct soap_dom_element *, const char*);
SOAP_FMAC3 struct soap_dom_element * SOAP_FMAC4 soap_get_xsd__anyType(struct soap*, struct soap_dom_element *, const char*, const char*);
SOAP_FMAC1 struct soap_dom_element * SOAP_FMAC2 soap_in_xsd__anyType(struct soap*, const char*, struct soap_dom_element *, const char*);

SOAP_FMAC3 void SOAP_FMAC4 soap_markelement(struct soap*, const void*, int);
SOAP_FMAC3 int SOAP_FMAC4 soap_putelement(struct soap*, const void*, const char*, int, int);
SOAP_FMAC3 void *SOAP_FMAC4 soap_getelement(struct soap*, int*);

/* format string for generating DOM namespace prefixes (<= 16 chars total) */
#define SOAP_DOMID_FORMAT "SOAP-DOM%lu"

/* namespace name (URI) lookup and store routines */
static struct soap_ilist *soap_lookup_ns(struct soap*, const char*);
static struct soap_ilist *soap_enter_ns(struct soap*, const char*, const char*);

/*
**	DOM custom (de)serializers
*/

SOAP_FMAC1
void
SOAP_FMAC2
soap_mark_xsd__anyType(struct soap *soap, const struct soap_dom_element *node)
{ if (node)
  { if (node->type && node->node)
      soap_markelement(soap, node->node, node->type);
    else if (!node->data && !node->wide)
    { struct soap_dom_element *elt;
      for (elt = node->elts; elt; elt = elt->next)
        soap_mark_xsd__anyType(soap, elt);
    }
  }
}

SOAP_FMAC1
void
SOAP_FMAC2
soap_default_xsd__anyType(struct soap *soap, struct soap_dom_element *node)
{ node->next = NULL;
  node->prnt = NULL;
  node->elts = NULL;
  node->atts = NULL;
  node->nstr = NULL;
  node->name = NULL;
  node->data = NULL;
  node->wide = NULL;
  node->node = NULL;
  node->type = 0;
}

static int element(struct soap *soap, const char *name)
{ short part = soap->part;
  soap->part = SOAP_IN_ENVELOPE; /* use this to avoid SOAP encoding and literal encoding issues */
  soap_element(soap, name, 0, NULL);
  soap->part = part;
  return soap->error;
}

static int out_element(struct soap *soap, const struct soap_dom_element *node, const char *prefix, const char *name, const char *nstr)
{ if (!prefix)
  { if (node->type && node->node)
      return soap_putelement(soap, node->node, name, 0, node->type);
    return element(soap, name);
  }
  if (node->type && node->node)
  { char *s = (char*)malloc(strlen(prefix) + strlen(name) + 2);
    if (!s)
      return soap->error = SOAP_EOM;
    sprintf(s, "%s:%s", prefix, name);
    soap_putelement(soap, node->node, s, 0, node->type);
    free(s);
  }
  else if (strlen(prefix) + strlen(name) < sizeof(soap->msgbuf))
  { sprintf(soap->msgbuf, "%s:%s", prefix, name);
    if (element(soap, soap->msgbuf))
      return soap->error;
    if (nstr)
    { sprintf(soap->msgbuf, "xmlns:%s", prefix);
      soap_attribute(soap, soap->msgbuf, nstr);
    }
  }
  else
  { char *s = (char*)malloc(strlen(prefix) + strlen(name) + 2);
    if (!s)
      return soap->error = SOAP_EOM;
    sprintf(s, "%s:%s", prefix, name);
    if (element(soap, s))
      return soap->error;
    if (nstr)
    { sprintf(s, "xmlns:%s", prefix);
      soap_attribute(soap, s, nstr);
    }
    free(s);
  }
  return soap->error;
}

static int out_attribute(struct soap *soap, const char *prefix, const char *name, const char *data)
{ if (!prefix)
    return soap_attribute(soap, name, data);
  if (strlen(prefix) + strlen(name) < sizeof(soap->msgbuf))
  { sprintf(soap->msgbuf, "%s:%s", prefix, name);
    soap_attribute(soap, soap->msgbuf, data);
  }
  else
  { char *s = (char*)malloc(strlen(prefix) + strlen(name) + 2);
    if (!s)
      return soap->error = SOAP_EOM;
    sprintf(s, "%s:%s", prefix, name);
    soap_attribute(soap, s, data);
    free(s);
  }
  return soap->error;
}

SOAP_FMAC1
int
SOAP_FMAC2
soap_out_xsd__anyType(struct soap *soap, const char *tag, int id, const struct soap_dom_element *node, const char *type)
{ if (node)
  { struct soap_dom_element *elt;
    struct soap_dom_attribute *att;
    register struct soap_ilist *p = NULL, *q;
    struct Namespace *ns = NULL;
    const char *prefix;		/* namespace prefix, if namespace is present */
    size_t colon = 0;
    if (node->name)
      tag = node->name;
    if (!tag)
      tag = "_";
    if ((prefix = strchr(tag, ':')))
    { colon = prefix - tag + 1;
      if (colon > sizeof(soap->tag))
        colon = sizeof(soap->tag);
    }
    prefix = NULL;
    DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node '%s'\n", tag));
    if (node->nstr)
    { if ((p = soap_lookup_ns(soap, node->nstr)))
      { prefix = p->id;
        p = NULL;
        if (out_element(soap, node, prefix, tag + colon, NULL))
          return soap->error;
      }
      else if (colon)
      { strncpy(soap->tag, tag, colon - 1);
        soap->tag[colon - 1] = '\0';
        if (!(p = soap_enter_ns(soap, soap->tag, node->nstr)))
          return soap->error = SOAP_EOM;
        prefix = p->id;
        if (out_element(soap, node, prefix, tag + colon, node->nstr))
          return soap->error;
      }
      else
      { for (ns = soap->local_namespaces; ns && ns->id; ns++)
          if (ns->ns == node->nstr || !strcmp(ns->ns, node->nstr))
          { /* if (soap->encodingStyle || ns == soap->local_namespaces) */
              prefix = ns->id;
            if (out_element(soap, node, ns->id, tag + colon, NULL))
              return soap->error;
            break;
          }
        if (!ns || !ns->id)
        { sprintf(soap->tag, SOAP_DOMID_FORMAT, soap->idnum++);
          if (!(p = soap_enter_ns(soap, soap->tag, node->nstr)))
            return soap->error = SOAP_EOM;
          prefix = p->id;
          if (out_element(soap, node, prefix, tag + colon, node->nstr))
            return soap->error;
        }
      }
    }
    else if (out_element(soap, node, NULL, tag + colon, NULL))
      return soap->error;
    if (node->type && node->node)
      return SOAP_OK;
    for (att = node->atts; att; att = att->next)
    { if (att->name)
      { if (att->nstr)
        { if ((att->nstr == node->nstr || (node->nstr && !strcmp(att->nstr, node->nstr))) && prefix)
	  { if (out_attribute(soap, prefix, att->name, att->data))
	      return soap->error;
	  }
	  else if ((q = soap_lookup_ns(soap, att->nstr)))
	  { if (out_attribute(soap, q->id, att->name, att->data))
	      return soap->error;
	  }
	  else
	  { for (ns = soap->local_namespaces; ns && ns->id; ns++)
            { if (ns->ns == att->nstr || !strcmp(ns->ns, att->nstr))
	      { if (out_attribute(soap, ns->id, att->name, att->data))
	          return soap->error;
	        break;
	      }
	    }
	    if (!ns || !ns->id)
            { sprintf(soap->msgbuf, "xmlns:"SOAP_DOMID_FORMAT, soap->idnum++);
	      if (soap_attribute(soap, soap->msgbuf, att->nstr))
	        return soap->error;
	      strcat(soap->msgbuf, ":");
	      strcat(soap->msgbuf, att->name);
	      if (soap_attribute(soap, soap->msgbuf + 6, att->data))
	        return soap->error;
            }
          }
        }
	else if (soap_attribute(soap, att->name, att->data))
          return soap->error;
      }
    }
    if (soap_element_start_end_out(soap, NULL))
      return soap->error;
    if (node->data || node->wide || node->elts)
    { if (node->data)
      { if (soap_string_out(soap, node->data, 0))
          return soap->error;
      }
      else if (node->wide)
      { if (soap_wstring_out(soap, node->wide, 0))
          return soap->error;
      }
      else
      { for (elt = node->elts; elt; elt = elt->next)
          if (soap_out_xsd__anyType(soap, tag, 0, elt, NULL))
            return soap->error;
      }
    }
    DBGLOG(TEST, SOAP_MESSAGE(fdebug, "End of DOM node '%s'\n", tag + colon));
    if (soap_send_raw(soap, "</", 2))
      return soap->error;
    if (prefix)
      if (soap_send(soap, prefix) || soap_send_raw(soap, ":", 1))
        return soap->error;
    if (soap_send(soap, tag + colon) || soap_send_raw(soap, ">", 1))
      return soap->error;
    if (p)
      p->level = 0; /* xmlns binding is out of scope */
  }
  return SOAP_OK;
}

SOAP_FMAC1
struct soap_dom_element *
SOAP_FMAC2
soap_in_xsd__anyType(struct soap *soap, const char *tag, struct soap_dom_element *node, const char *type)
{ register struct soap_attribute *tp;
  register struct soap_dom_attribute **att;
  register struct soap_nlist *np;
  register char *s;
  if (soap_peek_element(soap))
    return NULL;
  if (!node)
    if (!(node = (struct soap_dom_element*)soap_malloc(soap, sizeof(struct soap_dom_element))))
    { soap->error = SOAP_EOM;
      return NULL;
    }
  node->next = NULL;
  node->prnt = NULL;
  node->elts = NULL;
  node->atts = NULL;
  node->nstr = NULL;
  node->name = NULL;
  node->data = NULL;
  node->wide = NULL;
  node->node = NULL;
  node->type = 0;
  DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node %s\n", soap->tag));
  np = soap->nlist;
  if (!(s = strchr(soap->tag, ':')))
  { while (np && *np->id) /* find default namespace, if present */
      np = np->next;
    s = soap->tag;
  }
  else
  { while (np && (strncmp(np->id, soap->tag, s - soap->tag) || np->id[s - soap->tag]))
      np = np->next;
    s++;
    if (!np)
    { soap->error = SOAP_NAMESPACE;
      return NULL;
    }
  }
  if (np)
  { if (np->index >= 0)
      node->nstr = soap->namespaces[np->index].ns;
    else if (np->ns)
      node->nstr = soap_strdup(soap, np->ns);
    DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node namespace='%s'\n", node->nstr?node->nstr:""));
  }
  node->name = soap_strdup(soap, soap->tag);
  if ((soap->mode & SOAP_DOM_NODE) || (!(soap->mode & SOAP_DOM_TREE) && *soap->id))
  { if ((node->node = soap_getelement(soap, &node->type)))
    { DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node contains type %d from xsi:type\n", node->type));
      return node;
    }
    if (soap->error == SOAP_TAG_MISMATCH)
      soap->error = SOAP_OK;
    else
      return NULL;
  }
  att = &node->atts;
  for (tp = soap->attributes; tp; tp = tp->next)
    if (tp->visible)
    { np = soap->nlist;
      if (!(s = strchr(tp->name, ':')))
      { while (np && *np->id) /* find default namespace, if present */
          np = np->next;
        s = tp->name;
      }
      else
      { while (np && (strncmp(np->id, tp->name, s - tp->name) || np->id[s - tp->name]))
          np = np->next;
        s++;
        if (!np)
        { soap->error = SOAP_NAMESPACE;
          return NULL;
        }
      }
      DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node attribute='%s'\n", tp->name));
      *att = (struct soap_dom_attribute*)soap_malloc(soap, sizeof(struct soap_dom_attribute));
      if (!*att)
      { soap->error = SOAP_EOM;
        return NULL;
      }
      (*att)->next = NULL;
      (*att)->nstr = NULL;
      if (np)
      { if (np->index >= 0)
          (*att)->nstr = soap->namespaces[np->index].ns;
        else if (np->ns)
	  (*att)->nstr = soap_strdup(soap, np->ns);
        DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM attribute namespace='%s'\n", (*att)->nstr?(*att)->nstr:""));
      }
      (*att)->name = soap_strdup(soap, s);
      if (tp->visible == 2)
        (*att)->data = soap_strdup(soap, tp->value);
      else
        (*att)->data = NULL;
      (*att)->wide = NULL;
      att = &(*att)->next;
      tp->visible = 0;
    }
  soap_element_begin_in(soap, NULL, 1);
  DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node '%s' accepted\n", node->name));
  if (soap->body)
  { wchar c;
    DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node '%s' has content\n", node->name));
    do c = soap_getchar(soap);
    while (c > 0 && c <= 32);
    if (c == EOF)
    { soap->error = SOAP_EOF;
      return NULL;
    }
    soap_unget(soap, c);
    if (c == '<')
    { struct soap_dom_element **elt;
      DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node '%s' has subelements\n", node->name));
      elt = &node->elts;
      for (;;)
      { if (!(*elt = soap_in_xsd__anyType(soap, NULL, NULL, NULL)))
        { if (soap->error == SOAP_NO_TAG)
            soap->error = SOAP_OK;
          else
            return NULL;
          break;
        }
	(*elt)->prnt = node;
        elt = &(*elt)->next;
      }
    }
    else
    { DBGLOG(TEST, SOAP_MESSAGE(fdebug, "DOM node '%s' has cdata\n", node->name));
      if ((soap->mode & SOAP_C_UTFSTRING) || (soap->mode & SOAP_C_MBSTRING))
      { if (!(node->data = soap_string_in(soap, 1, -1, -1)))
          return NULL;
      }
      else if (!(node->wide = soap_wstring_in(soap, 1, -1, -1)))
        return NULL;
    }
    if (soap_element_end_in(soap, node->name))
      return NULL;
    DBGLOG(TEST, SOAP_MESSAGE(fdebug, "End of DOM node '%s'\n", node->name));
  }
  return node;
}

/*
**	Namespace lookup/store routines
*/

static struct soap_ilist *
soap_lookup_ns(struct soap *soap, const char *nstr)
{ register struct soap_ilist *ip;
  for (ip = soap->iht[soap_hash(nstr)]; ip; ip = ip->next)
    if (!strcmp((char*)ip->ptr, nstr) && ip->level)
      return ip;
  return NULL;
}

static struct soap_ilist *
soap_enter_ns(struct soap *soap, const char *prefix, const char *nstr)
{ int h;
  register struct soap_ilist *ip;
  for (ip = soap->iht[soap_hash(nstr)]; ip; ip = ip->next)
  { if (!strcmp((char*)ip->ptr, nstr) && !ip->level)
    { strcpy(ip->id, prefix);
      ip->level = 1;
      return ip;
    }
  }
  ip = (struct soap_ilist*)malloc(sizeof(struct soap_ilist) + strlen(nstr) + SOAP_TAGLEN);
  if (ip)
  { h = soap_hash(nstr);
    strcpy(ip->id, prefix);
    ip->ptr = ip->id + SOAP_TAGLEN;
    strcpy((char*)ip->ptr, nstr);
    ip->next = soap->iht[h];
    soap->iht[h] = ip;
    ip->flist = NULL;
    ip->level = 1;
    return ip;
  }
  return NULL;
}

#ifdef __cplusplus

/*
**	Class soap_dom_element
*/

soap_dom_element::soap_dom_element()
{ soap = NULL;
  next = NULL;
  prnt = NULL;
  elts = NULL;
  atts = NULL;
  nstr = NULL;
  name = NULL;
  data = NULL;
  wide = NULL;
  node = NULL;
  type = 0;
}

soap_dom_element::soap_dom_element(struct soap *soap)
{ this->soap = soap;
  next = NULL;
  prnt = NULL;
  elts = NULL;
  atts = NULL;
  nstr = NULL;
  name = NULL;
  data = NULL;
  wide = NULL;
  node = NULL;
  type = 0;
}

soap_dom_element::soap_dom_element(struct soap *soap, const char *nstr, const char *name)
{ this->soap = soap;
  this->next = NULL;
  this->prnt = NULL;
  this->elts = NULL;
  this->atts = NULL;
  this->nstr = soap_strdup(soap, nstr);
  this->name = soap_strdup(soap, name);
  this->data = NULL;
  this->wide = NULL;
  this->node = NULL;
  this->type = 0;
}

soap_dom_element::soap_dom_element(struct soap *soap, const char *nstr, const char *name, const char *data)
{ this->soap = soap;
  this->next = NULL;
  this->prnt = NULL;
  this->nstr = soap_strdup(soap, nstr);
  this->name = soap_strdup(soap, name);
  this->data = soap_strdup(soap, data);
  this->wide = NULL;
  this->atts = NULL;
  this->elts = NULL;
  this->node = NULL;
  this->type = 0;
}

soap_dom_element::soap_dom_element(struct soap *soap, const char *nstr, const char *name, void *node, int type)
{ this->soap = soap;
  this->next = NULL;
  this->prnt = NULL;
  this->nstr = soap_strdup(soap, nstr);
  this->name = soap_strdup(soap, name);
  this->data = NULL;
  this->wide = NULL;
  this->atts = NULL;
  this->elts = NULL;
  this->node = node;
  this->type = type;
}

soap_dom_element::~soap_dom_element()
{ }

soap_dom_element &soap_dom_element::set(const char *nstr, const char *name)
{ this->nstr = soap_strdup(soap, nstr);
  this->name = soap_strdup(soap, name);
  return *this;
}

soap_dom_element &soap_dom_element::set(const char *data)
{ this->data = soap_strdup(soap, data);
  return *this;
}

soap_dom_element &soap_dom_element::set(void *node, int type)
{ this->node = node;
  this->type = type;
  return *this;
}

soap_dom_element &soap_dom_element::add(struct soap_dom_element *elt)
{ elt->prnt = this;
  for (struct soap_dom_element *e = elts; e; e = e->next)
    if (!e->next)
    { e->next = elt;
      return *this;
    }
  elts = elt;
  return *this;
}

soap_dom_element &soap_dom_element::add(struct soap_dom_element &elt)
{ return add(&elt);
}

soap_dom_element &soap_dom_element::add(struct soap_dom_attribute *att)
{ for (struct soap_dom_attribute *a = atts; a; a = a->next)
    if (!a->next)
    { a->next = att;
      return *this;
    }
  atts = att;
  return *this;
}

soap_dom_element &soap_dom_element::add(struct soap_dom_attribute &att)
{ return add(&att);
}

soap_dom_iterator soap_dom_element::begin()
{ soap_dom_iterator iter(this);
  return iter;
}

soap_dom_iterator soap_dom_element::end()
{ soap_dom_iterator iter(NULL);
  return iter;
}

soap_dom_iterator soap_dom_element::find(const char *nstr, const char *name)
{ soap_dom_iterator iter(this);
  iter.nstr = nstr;
  iter.name = name;
  if (name && soap_tag_cmp(this->name, name))
    return ++iter;
  if (nstr && this->nstr && soap_tag_cmp(this->nstr, nstr))
    return ++iter;
  return iter;
}

soap_dom_iterator soap_dom_element::find(int type)
{ soap_dom_iterator iter(this);
  iter.type = type;
  if (this->type != type)
    return ++iter;
  return iter;
}

void soap_dom_element::unlink()
{ soap_unlink(soap, this);
  soap_unlink(soap, nstr);
  soap_unlink(soap, name);
  soap_unlink(soap, data);
  soap_unlink(soap, wide);
  if (elts)
    elts->unlink();
  if (atts)
    elts->unlink();
  if (next)
    next->unlink();
  node = NULL;
  type = 0;
}

/*
**	Class soap_dom_attribute
*/

soap_dom_attribute::soap_dom_attribute()
{ soap = NULL;
}

soap_dom_attribute::soap_dom_attribute(struct soap *soap)
{ this->soap = soap;
  this->next = NULL;
  this->nstr = NULL;
  this->name = NULL;
  this->data = NULL;
  this->wide = NULL;
}

soap_dom_attribute::soap_dom_attribute(struct soap *soap, const char *nstr, const char *name, const char *data)
{ this->soap = soap;
  this->next = NULL;
  this->nstr = soap_strdup(soap, nstr);
  this->name = soap_strdup(soap, name);
  this->data = soap_strdup(soap, data);
  this->wide = NULL;
}

soap_dom_attribute::~soap_dom_attribute()
{ }

void soap_dom_attribute::unlink()
{ soap_unlink(soap, this);
  soap_unlink(soap, nstr);
  soap_unlink(soap, name);
  soap_unlink(soap, data);
  soap_unlink(soap, wide);
  if (next)
    next->unlink();
}

/*
**	Class soap_dom_iterator
*/

soap_dom_iterator::soap_dom_iterator()
{ elt = NULL;
  nstr = NULL;
  name = NULL;
  type = 0;
}

soap_dom_iterator::soap_dom_iterator(struct soap_dom_element *elt)
{ this->elt = elt;
  nstr = NULL;
  name = NULL;
  type = 0;
}

soap_dom_iterator::~soap_dom_iterator()
{ }

bool soap_dom_iterator::operator==(const soap_dom_iterator &iter) const
{ return this->elt == iter.elt;
}

bool soap_dom_iterator::operator!=(const soap_dom_iterator &iter) const
{ return this->elt != iter.elt;
}

struct soap_dom_element &soap_dom_iterator::operator*() const
{ return *this->elt;
}

soap_dom_iterator &soap_dom_iterator::operator++()
{ while (this->elt)
  { if (this->elt->elts)
      this->elt = elt->elts;
    else if (this->elt->next)
      this->elt = this->elt->next;
    else
    { do this->elt = this->elt->prnt;
      while (this->elt && !this->elt->next);
      if (this->elt)
        this->elt = this->elt->next;
      if (!this->elt)
        break;
    }
    if (this->name && this->elt->name)
    { if (!soap_tag_cmp(this->elt->name, this->name))
      { if (this->nstr && this->elt->nstr)
        { if (!soap_tag_cmp(this->elt->nstr, this->nstr))
	    break;
        }
        else
          break;
      }
    }
    else if (this->type)
    { if (this->elt->type == this->type)
        break;
    }
    else
      break;
  }
  return *this;
}

/*
**	IO
*/

#ifndef UNDER_CE

std::ostream &operator<<(std::ostream &o, const struct soap_dom_element &e)
{ if (!e.soap)
  { struct soap soap;
    soap_init2(&soap, SOAP_IO_DEFAULT, SOAP_XML_GRAPH);
    soap_mark_xsd__anyType(&soap, &e);
    soap_begin_send(&soap);
    soap_put_xsd__anyType(&soap, &e, NULL, NULL);
    soap_end_send(&soap);
    soap_end(&soap);
    soap_done(&soap);
  }
  else
  { std::ostream *os = e.soap->os;
    e.soap->os = &o;
    short omode = e.soap->omode;
    soap_set_omode(e.soap, SOAP_XML_GRAPH);
    soap_mark_xsd__anyType(e.soap, &e);
    soap_begin_send(e.soap);
    soap_put_xsd__anyType(e.soap, &e, NULL, NULL);
    soap_end_send(e.soap);
    e.soap->os = os;
    e.soap->omode = omode;
  }
  return o;
}

std::istream &operator>>(std::istream &i, struct soap_dom_element &e)
{ if (!e.soap)
    e.soap = soap_new();
  std::istream *is = e.soap->is;
  e.soap->is = &i;
  if (soap_begin_recv(e.soap)
   || !soap_in_xsd__anyType(e.soap, NULL, &e, NULL)
   || soap_end_recv(e.soap))
    ; /* handle error? Note: e.soap->error is set and app should check */
  e.soap->is = is;
  return i;
}

#endif

#endif
