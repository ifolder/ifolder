GSOAP = ../external/gsoap-linux-2.7
IFOLDER_CLIENT_WSDL = ../external/ifolder/iFolderWebService.wsdl
GSOAP_INCLUDES = -I.
NONAMESPACE_CFLAGS = -DWITH_NONAMESPACES
INSTALL_EXTENSION_PATH = $(gnome_root)/lib/nautilus/extensions-1.0
INSTALL_EMBLEM_PATH = $(DESTDIR)$(gnome_root)/share/icons/gnome/48x48/emblems
INSTALL_ICONS_PATH = $(DESTDIR)$(gnome_root)/share/icons/gnome/24x24/apps
INSTALL_EXE_PATH = $(DESTDIR)$(libdir)

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
	$(GSOAP_INCLUDES)				\
	$(SIMIAS_CLIENT_C_CFLAGS)	\
	$(XML2_CFLAGS)

EXTRA_DIST = nautilus-ifolder.h nautilus-ifolder.cs emblem-ifolder.png ifolder-folder.png stdsoap2.h

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep iFolderClientClientLib.c`
	rm -rf `find $(distdir) |grep envC.c`
	rm -rf `find $(distdir) |grep nautilus-ifolder.exe`
	rm -rf `find $(distdir) |grep nautilus-ifolder.h$$`

# FIXME: get this from the .pc file
nautilus_extensiondir=$(INSTALL_EXTENSION_PATH)

nautilus_extension_LTLIBRARIES=libnautilus-ifolder.la

libnautilus_ifolder_la_SOURCES =     \
	envC.c iFolderClientClientLib.c stdsoap2.c nautilus-ifolder.c

# Statically link libsimias-event.a
SIMIAS_EVENT_CLIENT_STATIC_LIB = $(SIMIAS_CLIENT_C_PREFIX)/lib/libsimias-event.a

libnautilus_ifolder_la_LDFLAGS = -module -export-dynamic -avoid-version $(SIMIAS_CLIENT_C_LIBS) $(SIMIAS_EVENT_CLIENT_STATIC_LIB) $(XML2_LIBS)
libnautilus_ifolder_la_LIBADD  = $(NAUTILUS_LIBS)

libnautilus_ifolder_la_DEPENDENCIES = nautilus-ifolder.exe

# FIXME: Figure out a better way to force nautilus-ifolder.exe to be built
iFolderClient.h:	$(IFOLDER_CLIENT_WSDL)
	$(GSOAP)/wsdl2h -c -o $@ $(IFOLDER_CLIENT_WSDL)
	
envC.c:
	> env.h
	$(GSOAP)/soapcpp2 -c -penv env.h
	
iFolderClientClientLib.c:	iFolderClient.h
	$(GSOAP)/soapcpp2 -c -n -piFolderClient iFolderClient.h
	
nautilus-ifolder.exe: nautilus-ifolder.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_CLIENT_LIBS) $(IFOLDER_LIBS) nautilus-ifolder.cs

install-exec-local: libnautilus-ifolder.la
	$(mkinstalldirs) $(INSTALL_EMBLEM_PATH)
	$(INSTALL_PROGRAM) emblem-ifolder.png $(INSTALL_EMBLEM_PATH)
	$(mkinstalldirs) $(INSTALL_ICONS_PATH)
	$(INSTALL_PROGRAM) ifolder-folder.png $(INSTALL_ICONS_PATH)
	$(mkinstalldirs) $(INSTALL_EXE_PATH)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder.exe $(INSTALL_EXE_PATH)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder $(INSTALL_EXE_PATH)

uninstall-local:
	cd $(INSTALL_EXTENSION_PATH); rm -f libnautilus-ifolder*
	cd $(INSTALL_EMBLEM_PATH); rm -f emblem-ifolder.png
	cd $(INSTALL_ICONS_PATH); rm -f ifolder-folder.png
	cd $(INSTALL_EXE_PATH); rm -f nautilus-ifolder*

clean-local:
	rm -f iFolderClient* env* iFolder_* soap*
	rm -f nautilus-ifolder.exe
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
