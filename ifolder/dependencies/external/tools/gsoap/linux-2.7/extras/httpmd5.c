/*

httpmd5.c

gSOAP HTTP Content-MD5 digest plugin.

gSOAP XML Web services tools
Copyright (C) 2000-2004, Robert van Engelen, Genivia, Inc., All Rights Reserved.

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
Copyright (C) 2000-2004, Robert van Engelen, Genivia, Inc., All Rights Reserved.
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

	Requires an md5 engine invoked via a handler:

	int http_md5_handler(struct soap *soap, void **context, enum http_md5_action action, char *buf, size_t len)
	context can be set and passed to subsequent calls. Parameters:
	action =
	HTTP_MD5_INIT:		init context
	HTTP_MD5_UPDATE:	update context with data from buf with size len
	HTTP_MD5_FINAL:		fill buf with 16 bytes MD5 hash value
	HTTP_MD5_DELETE:	delete context
	buf			input data, output MD5 128 bit hash value
	len			length of input data

	Example code:
	httpmd5test.h, httpmd5test.c

	Limitations:
	Does not work with combined chunked + compressed messages.
	When sending DIME/MIME attachments, you MUST use the SOAP_IO_STORE flag
	to compute the MD5 hash of the message with attachments. The flag
	disables streaming DIME.
*/

#include "httpmd5.h"

const char http_md5_id[13] = HTTP_MD5_ID;

static int http_md5_init(struct soap *soap, struct http_md5_data *data, int (*handler)(struct soap*, void**, enum http_md5_action, char*, size_t));
static int http_md5_copy(struct soap *soap, struct soap_plugin *dst, struct soap_plugin *src);
static void http_md5_delete(struct soap *soap, struct soap_plugin *p);

static int http_md5_post_header(struct soap *soap, const char *key, const char *val);
static int http_md5_parse_header(struct soap *soap, const char *key, const char *val);
static int http_md5_prepareinit(struct soap *soap);
static int http_md5_preparesend(struct soap *soap, const char *buf, size_t len);
static int http_md5_preparerecv(struct soap *soap, const char *buf, size_t len);
static int http_md5_disconnect(struct soap *soap);

int http_md5(struct soap *soap, struct soap_plugin *p, void *arg)
{ p->id = http_md5_id;
  p->data = (void*)malloc(sizeof(struct http_md5_data));
  p->fcopy = http_md5_copy;
  p->fdelete = http_md5_delete;
  if (p->data)
    if (http_md5_init(soap, (struct http_md5_data*)p->data, (int (*)(struct soap*, void**, enum http_md5_action, char*, size_t))arg))
    { free(p->data); /* error: could not init */
      return SOAP_EOM; /* return error */
    }
  return SOAP_OK;
}

static int http_md5_init(struct soap *soap, struct http_md5_data *data, int (*handler)(struct soap*, void**, enum http_md5_action, char*, size_t))
{ data->fposthdr = soap->fposthdr;
  soap->fposthdr = http_md5_post_header;
  data->fparsehdr = soap->fparsehdr;
  soap->fparsehdr = http_md5_parse_header;
  data->fprepareinit = soap->fprepareinit;
  soap->fprepareinit = http_md5_prepareinit;
  data->fpreparesend = soap->fpreparesend;
  soap->fpreparesend = http_md5_preparesend;
  data->fhandler = handler;
  data->context = NULL;
  memset(data->md5_digest, 0, sizeof(data->md5_digest));
  return SOAP_OK;
}

static int http_md5_copy(struct soap *soap, struct soap_plugin *dst, struct soap_plugin *src)
{ *dst = *src;
  dst->data = (void*)malloc(sizeof(struct http_md5_data));
  return SOAP_OK;
}

static void http_md5_delete(struct soap *soap, struct soap_plugin *p)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  if (data)
  { data->fhandler(soap, &data->context, HTTP_MD5_DELETE, NULL, 0);
    free(data);
  }
}

static int http_md5_post_header(struct soap *soap, const char *key, const char *val)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  char buf64[25]; /* 24 base64 chars + '\0' */
  int err;
  if (!data)
    return SOAP_PLUGIN_ERROR;
  if (!key) /* last line */
  { if ((err = data->fhandler(soap, &data->context, HTTP_MD5_FINAL, data->md5_digest, 0)))
      return err;
    data->fposthdr(soap, "Content-MD5", soap_s2base64(soap, data->md5_digest, buf64, 16));
  }
  return data->fposthdr(soap, key, val);
}

static int http_md5_parse_header(struct soap *soap, const char *key, const char *val)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  if (!data)
    return SOAP_PLUGIN_ERROR;
  if (!soap_tag_cmp(key, "Content-MD5"))
  { soap_base642s(soap, val, data->md5_digest, 16, NULL);
    data->fpreparerecv = soap->fpreparerecv;
    soap->fpreparerecv = http_md5_preparerecv;
    data->fdisconnect = soap->fdisconnect;
    soap->fdisconnect = http_md5_disconnect;
    return SOAP_OK;
  }
  return data->fparsehdr(soap, key, val);
}

static int http_md5_prepareinit(struct soap *soap)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  if (!data)
    return SOAP_PLUGIN_ERROR;
  data->fhandler(soap, &data->context, HTTP_MD5_INIT, NULL, 0);
  if (soap->fpreparerecv == http_md5_preparerecv)
    soap->fpreparerecv = data->fpreparerecv;
  if (soap->fdisconnect == http_md5_disconnect)
    soap->fdisconnect = data->fdisconnect;
  if (data->fprepareinit)
    return data->fprepareinit(soap);
  return SOAP_OK;
}

static int http_md5_preparesend(struct soap *soap, const char *buf, size_t len)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  if (!data)
    return SOAP_PLUGIN_ERROR;
  data->fhandler(soap, &data->context, HTTP_MD5_UPDATE, (char*)buf, len);
  if (data->fpreparesend)
    return data->fpreparesend(soap, buf, len);
  return SOAP_OK;
}

static int http_md5_preparerecv(struct soap *soap, const char *buf, size_t len)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  if (!data)
    return SOAP_PLUGIN_ERROR;
  data->fhandler(soap, &data->context, HTTP_MD5_UPDATE, (char*)buf, len);
  if (data->fpreparerecv)
    return data->fpreparerecv(soap, buf, len);
  return SOAP_OK;
}

static int http_md5_disconnect(struct soap *soap)
{ struct http_md5_data *data = (struct http_md5_data*)soap_lookup_plugin(soap, http_md5_id);
  int err;
  char md5_digest[16];
  if ((err = data->fhandler(soap, &data->context, HTTP_MD5_FINAL, md5_digest, 0)))
    return err;
  soap->fpreparerecv = data->fpreparerecv;
  soap->fdisconnect = data->fdisconnect;
  if (memcmp(md5_digest, data->md5_digest, 16))
    return soap_sender_fault(soap, "MD5 digest mismatch: message corrupted", NULL);
  if (soap->fdisconnect)
    return soap->fdisconnect(soap);
  return SOAP_OK;
}
