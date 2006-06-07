/*	plugin.c

	Example gSOAP plug-in

	Copyright (C) 2000-2002 Robert A. van Engelen. All Rights Reserved.

	Compile & link with gSOAP clients and services to view SOAP messages.

	Usage (client/server code):
	struct soap soap;
	soap_init(&soap);
	soap_register_plugin(&soap, plugin); // register plugin
	...
	... = soap_copy(&soap); // copies plugin too
	...
	soap_done(&soap); // detach plugin
*/

#include "plugin.h"

static const char plugin_id[] = PLUGIN_ID;

static int plugin_init(struct soap *soap, struct plugin_data *data);
static int plugin_copy(struct soap *soap, struct soap_plugin *dst, struct soap_plugin *src);
static void plugin_delete(struct soap *soap, struct soap_plugin *p);
static int plugin_send(struct soap *soap, const char *buf, size_t len);
static size_t plugin_recv(struct soap *soap, char *buf, size_t len);

int plugin(struct soap *soap, struct soap_plugin *p, void *arg)
{ p->id = plugin_id;
  p->data = (void*)malloc(sizeof(struct plugin_data));
  p->fcopy = plugin_copy;
  p->fdelete = plugin_delete;
  if (p->data)
    if (plugin_init(soap, (struct plugin_data*)p->data))
    { free(p->data); /* error: could not init */
      return SOAP_EOM; /* return error */
    }
  return SOAP_OK;
}

static int plugin_init(struct soap *soap, struct plugin_data *data)
{ data->fsend = soap->fsend; /* save old recv callback */
  data->frecv = soap->frecv; /* save old send callback */
  soap->fsend = plugin_send; /* replace send callback with ours */
  soap->frecv = plugin_recv; /* replace recv callback with ours */
  return SOAP_OK;
}

static int plugin_copy(struct soap *soap, struct soap_plugin *dst, struct soap_plugin *src)
{ *dst = *src;
}

static void plugin_delete(struct soap *soap, struct soap_plugin *p)
{ free(p->data); /* free allocated plugin data (this function is not called for shared plugin data) */
}

static int plugin_send(struct soap *soap, const char *buf, size_t len)
{ struct plugin_data *data = (struct plugin_data*)soap_lookup_plugin(soap, plugin_id);
  fwrite(buf, len, 1, stderr);
  return data->fsend(soap, buf, len); /* pass data on to old send callback */
}

static size_t plugin_recv(struct soap *soap, char *buf, size_t len)
{ struct plugin_data *data = (struct plugin_data*)soap_lookup_plugin(soap, plugin_id);
  size_t res = data->frecv(soap, buf, len); /* get data from old recv callback */
  fwrite(buf, res, 1, stderr);
  return res;
}
