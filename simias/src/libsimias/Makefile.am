SIMIAS_WSDL = ../WebService/Simias.wsdl

EXTRA_DIST =

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep simiasClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`


lib_LTLIBRARIES = libsimias.la

override CFLAGS =

libsimias_la_SOURCES = \
 	envC.c simiasClientLib.c stdsoap2.c simias.c simias.h
 	
libsimias_la_CFLAGS = \
	-DWITH_NONAMESPACES

envC.c: $(srcdir)/stdsoap2.c
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -c -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.c:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.c $(srcdir)

$(srcdir)/libsimias.h: $(srcdir)/stdsoap2.c $(SIMIAS_WSDL)
	$(GSOAP_PATH)/wsdl2h -c -o $@ $(SIMIAS_WSDL)

$(srcdir)/simiasClientLib.c: $(srcdir)/libsimias.h
	$(GSOAP_PATH)/soapcpp2 -c -n -psimias $(srcdir)/libsimias.h
	
install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(includedir)
	$(INSTALL_PROGRAM) simiasStub.h $(DESTDIR)$(includedir)/simias
	$(INSTALL_PROGRAM) simias.nsmap $(DESTDIR)$(includedir)/simias
	$(INSTALL_PROGRAM) stdsoap2.h $(DESTDIR)$(includedir)/simias
	$(INSTALL_PROGRAM) simiasH.h $(DESTDIR)$(includedir)/simias
	$(INSTALL_PROGRAM) simias.h $(DESTDIR)$(includedir)/simias

uninstall-local:
	rm -f $(DESTDIR)$(includedir)/simias/simiasStub.h
	rm -f $(DESTDIR)$(includedir)/simias/simias.nsmap
	rm -f $(DESTDIR)$(includedir)/simias/stdsoap2.h
	rm -f $(DESTDIR)$(includedir)/simias/simiasH.h
	rm -f $(DESTDIR)$(includedir)/simias/simias.h

clean-local:
	rm -f stdsoap2.* *.xml simias.nsmap simiasC* simiasH* simiasS* env* soap*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
