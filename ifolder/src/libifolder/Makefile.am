IFOLDER_CLIENT_WSDL = ../WebService/iFolderWebService.wsdl

EXTRA_DIST = ifolder.h

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep iFolderClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`


lib_LTLIBRARIES = libifolderclient.la

override CFLAGS =

libifolderclient_la_SOURCES = \
 	envC.c iFolderClientLib.c stdsoap2.c ifolder.c ifolder.h

libifolderclient_la_CFLAGS = \
	-DWITH_NONAMESPACES

envC.c: $(srcdir)/stdsoap2.c
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -c -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.c:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.c $(srcdir)

$(srcdir)/iFolderClient.h: $(srcdir)/stdsoap2.c $(IFOLDER_CLIENT_WSDL)
	$(GSOAP_PATH)/wsdl2h -c -o $@ $(IFOLDER_CLIENT_WSDL)

$(srcdir)/iFolderClientLib.c: $(srcdir)/iFolderClient.h
	$(GSOAP_PATH)/soapcpp2 -c -n -piFolder $(srcdir)/iFolderClient.h

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(includedir)
	$(INSTALL_PROGRAM) iFolderStub.h $(DESTDIR)$(includedir)/ifolder
	$(INSTALL_PROGRAM) iFolder.nsmap $(DESTDIR)$(includedir)/ifolder
	$(INSTALL_PROGRAM) stdsoap2.h $(DESTDIR)$(includedir)/ifolder
	$(INSTALL_PROGRAM) iFolderH.h $(DESTDIR)$(includedir)/ifolder
	$(INSTALL_PROGRAM) ifolder.h $(DESTDIR)$(includedir)/ifolder

uninstall-local:
	rm -f $(DESTDIR)$(includedir)/ifolder/iFolderStub.h
	rm -f $(DESTDIR)$(includedir)/ifolder/iFolder.nsmap
	rm -f $(DESTDIR)$(includedir)/ifolder/stdsoap2.h
	rm -f $(DESTDIR)$(includedir)/ifolder/iFolderH.h
	rm -f $(DESTDIR)$(includedir)/ifolder/ifolder.h

clean-local:
	rm -f stdsoap2.* *.xml iFolder* env* soap*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
