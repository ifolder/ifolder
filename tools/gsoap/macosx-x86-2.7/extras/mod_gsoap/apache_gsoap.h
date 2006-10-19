/** Apache gSOAP module for Apache 2.0
 * Interface between the apache http - server (http://httpd.apache.org) and the gsoap SOAP stack (http://www.cs.fsu.edu/~engelen/soap.html)  
 * @file apache_gsoap.h
 *
 * author Christian Aberger (http://www.aberger.at)
 * ported to 2.0 Mick Wall (mick@mickandwendy.com)
 * updated by Robert van Engelen (engelen@acm.org)
 * updated by David Viner (dviner@yahoo-inc.com)
 *
 * Contributed to the gSOAP package under the terms and conditions of the gSOAP
 * open source public license.
 *
 */

#ifndef _APACHE_GSOAP_H_INCLUDED
#define _APACHE_GSOAP_H_INCLUDED

#ifdef __cplusplus
extern "C" {
#endif

#define APACHE_GSOAP_INTERFACE_VERSION 6
#define APACHE_HTTPSERVER_ENTRY_POINT "apache_init_soap_interface"

typedef SOAP_FMAC1 void (SOAP_FMAC2 *apache_soap_init_fn)(struct soap *, request_rec *); ///< calls soap_serve inside shared library 
typedef SOAP_FMAC1 int (SOAP_FMAC2 *apache_soap_serve_fn)(struct soap *, request_rec *);  ///< calls soap_init inside shared library
typedef SOAP_FMAC1 void (SOAP_FMAC2 *apache_soap_destroy_fn)(struct soap*, request_rec *); ///< calls soap_destroy inside shared library
typedef SOAP_FMAC1 void (SOAP_FMAC2 *apache_soap_end_fn)(struct soap*, request_rec *); ///< calls soap_end inside shared library
typedef SOAP_FMAC1 void (SOAP_FMAC2 *apache_soap_done_fn)(struct soap*, request_rec *); ///< calls soap_done inside shared library
typedef SOAP_FMAC1 int (SOAP_FMAC2 *apache_soap_register_plugin_fn)(struct soap*, int (*fcreate)(struct soap *, struct soap_plugin *, void*), void *arg, request_rec *);
typedef SOAP_FMAC1 void* SOAP_FMAC2 (*apache_soap_lookup_plugin_fn)(struct soap*, const char*, request_rec *);

/* the callbacks normally used in the apache_soap_interface */
SOAP_FMAC1 void SOAP_FMAC2 apache_soap_soap_destroy(struct soap *, request_rec *r);
SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_init(struct soap *soap, request_rec *r);
SOAP_FMAC1 int SOAP_FMAC2 apache_default_soap_serve(struct soap *soap, request_rec *r);
SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_end(struct soap *soap, request_rec *r);
SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_done(struct soap *soap, request_rec *r);
SOAP_FMAC1 int SOAP_FMAC2 apache_default_soap_register_plugin_arg(struct soap *soap, request_rec *r);
SOAP_FMAC1 void* SOAP_FMAC2 apache_default_soap_lookup_plugin(struct soap *soap, request_rec *r);

struct apache_soap_interface {
    unsigned int len; ///< length of this struct in bytes (for version control).
    unsigned int interface_version; 
    apache_soap_init_fn fsoap_init;
    apache_soap_serve_fn fsoap_serve;
    apache_soap_destroy_fn fsoap_destroy;
    apache_soap_end_fn fsoap_end;
    apache_soap_done_fn fsoap_done;
    apache_soap_register_plugin_fn fsoap_register_plugin_arg;
    apache_soap_lookup_plugin_fn fsoap_lookup_plugin;
    void *reserved; ///< variable reserved for apache module, must not be changed by server shared library.
};

typedef void (*apache_init_soap_interface_fn)(struct apache_soap_interface *, request_rec *);

/** exported shared library function called by mod_gsoap from within apache http server 
  * This function fills the members of the apache_soap_interface struct. 
  */
SOAP_FMAC1 void SOAP_FMAC2 apache_init_soap_interface(struct apache_soap_interface *, request_rec *r);

#define IMPLEMENT_GSOAP_SERVER() \
static SOAP_FMAC1 void SOAP_FMAC2 apache_soap_soap_destroy(struct soap *soap, request_rec *r) {soap_destroy(soap);}\
static SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_init(struct soap *soap, request_rec *r) {soap_init(soap);};\
static SOAP_FMAC1 int SOAP_FMAC2 apache_default_soap_serve(struct soap *soap, request_rec *r) {soap_serve(soap);};\
static SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_end(struct soap *soap, request_rec *r) {soap_end(soap);};\
static SOAP_FMAC1 void SOAP_FMAC2 apache_default_soap_done(struct soap *soap, request_rec *r) {soap_done(soap);};\
static SOAP_FMAC1 int SOAP_FMAC2 apache_default_soap_register_plugin_arg(struct soap *soap, request_rec *r) {soap_register_plugin_arg(soap);};\
static SOAP_FMAC1 void* SOAP_FMAC2 apache_default_soap_lookup_plugin(struct soap *soap, request_rec *r) {soap_lookup_plugin(soap);};\
void apache_init_soap_interface(struct apache_soap_interface *pInt, request_rec *r) {\
pInt->len = sizeof(struct apache_soap_interface);\
pInt->interface_version = APACHE_GSOAP_INTERFACE_VERSION;\
pInt->fsoap_init = apache_default_soap_init; \
pInt->fsoap_serve = apache_default_soap_serve;\
pInt->fsoap_destroy = apache_soap_soap_destroy;\
pInt->fsoap_end = apache_default_soap_end;\
pInt->fsoap_done = apache_default_soap_done;\
pInt->fsoap_register_plugin_arg = apache_default_soap_register_plugin_arg;\
pInt->fsoap_lookup_plugin = apache_default_soap_lookup_plugin;\
pInt->reserved = 0;\
}

#ifdef __cplusplus
}
#endif //__cplusplus

#endif //_APACHE_GSOAP_H_INCLUDED
