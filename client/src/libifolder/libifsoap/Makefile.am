IFOLDER_CLIENT_WSDL = iFolderWeb.wsdl

#INCLUDES =							\
#	-I$(top_srcdir)					\
#	-I$(top_builddir)

EXTRA_DIST = $(IFOLDER_CLIENT_WSDL)

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep iFolderClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`

#lib_LTLIBRARIES=libifsoap.la

#override CFLAGS =

#libifsoap_la_SOURCES =				\
#	envC.c iFolderClientLib.c stdsoap2.c

#libifsoap_la_CFLAGS =				\
#	-DWITH_NONAMESPACES				\
#	-DWITH_COOKIES

libifsoap_sources =					\
	envC.c							\
	iFolderClientLib.c				\
	stdsoap2.c

all: $(libifsoap_sources)

#libifsoap_la_LDFLAGS =

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

#install-exec-local:
#	$(mkinstalldirs) $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolderStub.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolder.nsmap $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) stdsoap2.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) iFolderH.h $(DESTDIR)$(includedir)/ifolder3
#	$(INSTALL_PROGRAM) libifsoap.h $(DESTDIR)$(includedir)/ifolder3

#uninstall-local:
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolderStub.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolder.nsmap
#	rm -f $(DESTDIR)$(includedir)/ifolder3/stdsoap2.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/iFolderH.h
#	rm -f $(DESTDIR)$(includedir)/ifolder3/libifsoap.h

clean-local:
	rm -f stdsoap2.* *.xml iFolderiFolder* *.nsmap env* soap* *.c *.h
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
