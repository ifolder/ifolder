IFOLDER_CLIENT_WSDL = ../WebService/iFolderWebService.wsdl

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep iFolderClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep stdsoap2.c`

INCLUDES = \
	-I$(top_srcdir)/libifolder-gtk \
	$(LIBIFOLDER_GTK_CFLAGS) \
	$(WARN_CFLAGS)

lib_LTLIBRARIES = libifolder-gtk.la

libifolder_gtk_la_SOURCES = \
	envC.c \
	iFolderClientLib.c \
	stdsoap2.c \
	general-property-page.c \
	sharing-property-page.c \
	ifolder-gtk-internal.c \
	ifolder-gtk-internal.h \
	ifolder-gtk.h
	
libifolder_gtk_la_CFLAGS = \
	$(SIMIAS_CLIENT_C_CFLAGS) \
	-DWITH_NONAMESPACES \
	-DWITH_COOKIES

libifolder_gtk_la_LDFLAGS = \
	$(SIMIAS_CLIENT_C_LIBS) \
	-no-undefined
	-export-symbols-regex "^[^_].*"

libifolder_gtk_la_LIBADD = \
	$(LIBIFOLDER_GTK_LIBS)

libifoldergtkincludedir = $(includedir)/ifolder3
libifoldergtkinclude_HEADERS = \
	ifolder-gtk.h

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

clean-local:
	rm -f stdsoap2.* *.xml iFolder* env* soap*
	rm -rf $(COMMON_CLEAN_FILES)
