INSTALL_EXTENSION_PATH = $(extension_install_dir)
INSTALL_EMBLEM_PATH = $(emblems_dir)
INSTALL_ICONS_PATH = $(pixmaps_dir)

INCLUDES =						\
	-DG_LOG_DOMAIN=\"Nautilus-iFolder\"		        \
	-DDATADIR=\"$(datadir)\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\" 	\
	-I$(top_srcdir)					\
	-I$(top_builddir)				\
	$(WARN_CFLAGS)                                  \
	$(DISABLE_DEPRECATED_CFLAGS)			\
	$(NAUTILUS_CFLAGS)						\
	$(SIMIAS_CLIENT_C_CFLAGS)	\
	$(IFOLDER_C_CFLAGS)	\
	$(XML2_CFLAGS)

EXTRA_DIST = nautilus-ifolder.h nautilus-ifolder.cs emblem-ifolder.png emblem-ifolder.icon ifolder-folder.png

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep nautilus-ifolder.exe`
	rm -rf `find $(distdir) |grep nautilus-ifolder.h$$`

# FIXME: get this from the .pc file
nautilus_extensiondir=$(INSTALL_EXTENSION_PATH)

nautilus_extension_LTLIBRARIES=libnautilus-ifolder.la

libnautilus_ifolder_la_SOURCES =     \
	nautilus-ifolder.c

libnautilus_ifolder_la_LDFLAGS = -module -export-dynamic -avoid-version $(SIMIAS_CLIENT_C_LIBS) $(IFOLDER_C_LIBS) $(XML2_LIBS)
libnautilus_ifolder_la_LIBADD  = $(NAUTILUS_LIBS)

libnautilus_ifolder_la_DEPENDENCIES = nautilus-ifolder.exe

nautilus-ifolder.exe: nautilus-ifolder.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_CLIENT_LIBS) $(IFOLDER_LIBS) nautilus-ifolder.cs

install-exec-local: libnautilus-ifolder.la
	$(mkinstalldirs) $(DESTDIR)$(INSTALL_EMBLEM_PATH)
	$(INSTALL_PROGRAM) emblem-ifolder.png $(DESTDIR)$(INSTALL_EMBLEM_PATH)
	$(INSTALL_PROGRAM) emblem-ifolder.icon $(DESTDIR)$(INSTALL_EMBLEM_PATH)
	$(mkinstalldirs) $(DESTDIR)$(INSTALL_ICONS_PATH)
	$(INSTALL_PROGRAM) ifolder-folder.png $(DESTDIR)$(INSTALL_ICONS_PATH)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder.exe $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder $(DESTDIR)$(libdir)

#	This is not possible to do during a make install
#	$(DESTDIR)$(gnome_root)/bin/gtk-update-icon-cache $(gnome_root)/share/icons/gnome

uninstall-local:
	cd $(DESTDIR)$(INSTALL_EXTENSION_PATH); rm -f libnautilus-ifolder*
	cd $(DESTDIR)$(INSTALL_EMBLEM_PATH); rm -f emblem-ifolder.*
	cd $(DESTDIR)$(INSTALL_ICONS_PATH); rm -f ifolder-folder.png
	cd $(DESTDIR)$(libdir); rm -f nautilus-ifolder*
#	$(gnome_root)/bin/gtk-update-icon-cache $(gnome_root)/share/icons/gnome

clean-local:
	rm -f nautilus-ifolder.exe
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
