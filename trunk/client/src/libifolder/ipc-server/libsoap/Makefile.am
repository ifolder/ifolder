IFOLDER_CLIENT_WSDL = iFolderWeb.wsdl

#INCLUDES =							\
#	-I$(top_srcdir)					\
#	-I$(top_builddir)

EXTRA_DIST = $(IFOLDER_CLIENT_WSDL)

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep iFolderClientLib.cpp`
	rm -rf `find $(distdir) |grep envC.cpp`
	rm -rf `find $(distdir) |grep stdsoap2.cpp`

noinst_LIBRARIES = libsoap.a
#lib_LTLIBRARIES=libsoap.la

#override CFLAGS =

libsoap_a_SOURCES =				\
	envC.cpp iFolderClientLib.cpp stdsoap2.cpp

libsoap_a_CXXFLAGS = \
	-DWITH_NONAMESPACES				\
	-DWITH_COOKIES

if DARWIN
endif

#libsoap_sources =					\
#	envC.cpp					\
#	iFolderClientLib.cpp				\
#	stdsoap2.cpp

#all: $(libsoap_sources)

#libsoap_a_LIBADD =

envC.cpp: $(srcdir)/stdsoap2.cpp
	> $(srcdir)/env.h
	$(GSOAP_PATH)/soapcpp2 -penv $(srcdir)/env.h

$(srcdir)/stdsoap2.cpp:
	cp $(GSOAP_PATH)/stdsoap2.h $(srcdir)
	cp $(GSOAP_PATH)/stdsoap2.cpp $(srcdir)
	cp $(GSOAP_PATH)/stl.h $(srcdir)
	cp $(GSOAP_PATH)/stldeque.h $(srcdir)
	cp $(GSOAP_PATH)/stllist.h $(srcdir)
	cp $(GSOAP_PATH)/stlvector.h $(srcdir)
	cp $(GSOAP_PATH)/stlset.h $(srcdir)

$(srcdir)/iFolderWebClient.h: $(srcdir)/stdsoap2.cpp $(IFOLDER_CLIENT_WSDL)
	$(GSOAP_PATH)/wsdl2h -o $@ $(IFOLDER_CLIENT_WSDL)

$(srcdir)/iFolderClientLib.cpp: $(srcdir)/iFolderWebClient.h
	$(GSOAP_PATH)/soapcpp2 -n -piFolder $(srcdir)/iFolderWebClient.h

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
	rm -f stdsoap2.* *.xml iFolderiFolder* *.nsmap env* soap* *.c *.cpp *.h iFolderWebClient*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
