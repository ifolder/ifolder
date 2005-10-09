SIMIAS_WSDL = ../WebService/SimiasAPI.wsdl

EXTRA_DIST =

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep simiasapiClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`

lib_LTLIBRARIES = libsimias2.la

libsimias2_la_SOURCES = \
 	envC.c simiaswebClientLib.c stdsoap2.c simias_internal.c simias_internal.h simias_handle.c simias.h simias_domain.c simias_node.c simias_nodelist.c simias_collection.c
 	
libsimias2_la_CFLAGS = \
	$(XML2_CFLAGS) -DWITH_NONAMESPACES -DWITH_COOKIES

libsimias2_la_LDFLAGS = \
	$(XML2_LIBS)

bin_PROGRAMS = testapi
testapi_SOURCES = test.c
LDADD = libsimias2.la

envC.c: $(srcdir)/stdsoap2.c
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -c -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.c:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.c $(srcdir)

$(srcdir)/libsimias.h: $(srcdir)/stdsoap2.c $(SIMIAS_WSDL)
	$(GSOAP_PATH)/wsdl2h -c -o $@ $(SIMIAS_WSDL)

$(srcdir)/simiaswebClientLib.c: $(srcdir)/libsimias.h
	$(GSOAP_PATH)/soapcpp2 -c -n -psimiasweb $(srcdir)/libsimias.h
	
install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(includedir)
	$(INSTALL_PROGRAM) simias.h $(DESTDIR)$(includedir)/simias

uninstall-local:
	rm -f $(DESTDIR)$(includedir)/simias/simias.h

clean-local:
	rm -f stdsoap2.* *.xml libsimias.h simiasweb.nsmap simiaswebC* simiaswebH* simiaswebS* env* soap*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
