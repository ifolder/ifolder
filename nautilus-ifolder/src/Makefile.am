GSOAP = ../external/gsoap-linux-2.7
IFOLDER_CLIENT_WSDL = ../external/ifolder/iFolderWebService.wsdl
GSOAP_INCLUDES = -I.
NONAMESPACE_CFLAGS = -DWITH_NONAMESPACES

INCLUDES =						\
	-DG_LOG_DOMAIN=\"Nautilus-iFolder\"		        \
	-DDATADIR=\"$(datadir)\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\" 	\
	-I$(top_srcdir)					\
	-I$(top_builddir)				\
	$(WARN_CFLAGS)                                  \
	$(DISABLE_DEPRECATED_CFLAGS)			\
	$(NAUTILUS_CFLAGS)						\
	$(NONAMESPACE_CFLAGS)					\
	$(GSOAP_INCLUDES)

# FIXME: get this from the .pc file
nautilus_extensiondir=$(libdir)/nautilus/extensions-1.0

nautilus_extension_LTLIBRARIES=libnautilus-ifolder.la

libnautilus_ifolder_la_SOURCES =     \
	envC.c iFolderClientClientLib.c stdsoap2.c nautilus-ifolder.c

libnautilus_ifolder_la_LDFLAGS = -module -export-dynamic -avoid-version
libnautilus_ifolder_la_LIBADD  = $(NAUTILUS_LIBS)

iFolderClient.h:	$(IFOLDER_CLIENT_WSDL)
	$(GSOAP)/wsdl2h -c -o $@ $(IFOLDER_CLIENT_WSDL)
	
envC.c:
	> env.h
	$(GSOAP)/soapcpp2 -c -penv env.h
	
iFolderClientClientLib.c:	iFolderClient.h
	$(GSOAP)/soapcpp2 -c -n -piFolderClient iFolderClient.h
	
#nautilus-ifolder.exe: nautilus-ifolder.cs
#	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_CLIENT_LIBS) $(IFOLDER_LIBS) nautilus-ifolder.cs
	
bin_PROGRAMS = nautilus-ifolder.exe

nautilus_ifolder_SOURCES = \
	./nautilus-ifolder.cs
	
nautilus-ifolder.exe: $(nautilus_ifolder_SOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_CLIENT_LIBS) $(IFOLDER_LIBS) nautilus-ifolder.cs

install-exec-local: libnautilus-ifolder.la
	$(mkinstalldirs) $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems
	$(INSTALL_PROGRAM) emblem-ifolder.png $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems

uninstall-local:
	cd $(DESTDIR)$(libdir)/nautilus/extensions-1.0; rm -f libnautilus-ifolder*
	cd $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems; rm -f emblem-ifolder.png

clean-local:
	rm -f iFolderClient* env* iFolder_* soap*
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
