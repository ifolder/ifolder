/*

long_double.c

Custom serializer for the long double (extended double) type as xsd:decimal.

Compile this file and link it with your code.

gSOAP XML Web services tools
Copyright (C) 2000-2006, Robert van Engelen, Genivia Inc., All Rights Reserved.
This part of the software is released under one of the following licenses:
GPL, the gSOAP public license, or Genivia's license for commercial use.
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
Copyright (C) 2000-2006, Robert van Engelen, Genivia, Inc., All Rights Reserved.
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

#include "soapH.h"

int soap_s2decimal(struct soap *soap, const char *s, long double *p)
{ if (s)
  { if (!*s)
      return soap->error = SOAP_TYPE;
    if (!soap_tag_cmp(s, "INF"))
      *p = (long double)DBL_PINFTY;
    else if (!soap_tag_cmp(s, "+INF"))
      *p = (long double)DBL_PINFTY;
    else if (!soap_tag_cmp(s, "-INF"))
      *p = (long double)DBL_NINFTY;
    else if (!soap_tag_cmp(s, "NaN"))
      *p = (long double)DBL_NAN;
    else
    {
#ifdef HAVE_STRTOLD
      char *r;
      *p = strtold(s, &r);
      if (*r)
#endif
#ifdef HAVE_SSCANF
        if (sscanf(s, "%Lg", p) != 1)
          soap->error = SOAP_TYPE;
#else
        soap->error = SOAP_TYPE;
#endif
    }
  }
  return soap->error;
}

const char *soap_decimal2s(struct soap *soap, long double n)
{ const char *s;
  if (soap_isnan(n))
    s = "NaN";
  else if (soap_ispinfd(n))
    s = "INF";
  else if (soap_isninfd(n))
    s = "-INF";
  else
  { sprintf(soap->tmpbuf, "%Lg", n);
    s = soap->tmpbuf;
  }
  return s;
}

int
soap_outdecimal(struct soap *soap, const char *tag, int id, const long double *p, const char *type, int n)
{ if (soap_element_begin_out(soap, tag, soap_embedded_id(soap, id, p, n), type)
   || soap_string_out(soap, soap_decimal2s(soap, *p), 0))
    return soap->error;
  return soap_element_end_out(soap, tag);
}

long double *
soap_indecimal(struct soap *soap, const char *tag, long double *p, const char *type, int t)
{ if (soap_element_begin_in(soap, tag, 0, type))
    return NULL;
  p = (long double*)soap_id_enter(soap, soap->id, p, t, sizeof(long double), 0, NULL, NULL, NULL);
  if (*soap->href)
    p = (long double*)soap_id_forward(soap, soap->href, p, 0, t, 0, sizeof(long double), 0, NULL);
  else if (p)
  { if (soap_s2decimal(soap, soap_value(soap), p))
      return NULL;
  }
  if (soap->body && soap_element_end_in(soap, tag))
    return NULL;
  return p;
}
