INSTALL_EXTENSION_PATH = $(gnome_lib)/nautilus/$(extension)
INSTALL_EMBLEM_PATH = $(DESTDIR)$(gnome_root)/share/icons/gnome/48x48/emblems
INSTALL_ICONS_PATH = $(DESTDIR)$(pixmaps_dir)
INSTALL_EXE_PATH = $(DESTDIR)$(libdir)

INCLUDES =						\
	-DG_LOG_DOMAIN=\"Nautilus-iFolder\"		        \
	-DDATADIR=\"$(datadir)\"			\
	-DGNOMELOCALEDIR=\""$(datadir)/locale"\" 	\
	-DOLD_CLIENT=$(old)                             \
	-I$(top_srcdir)					\
	-I$(top_builddir)				\
        -I$(gnome_vfs_path)                             \
	$(WARN_CFLAGS)                                  \
	$(DISABLE_DEPRECATED_CFLAGS)			\
	$(NAUTILUS_CFLAGS)						\
	$(SIMIAS_CLIENT_C_CFLAGS)	\
	$(IFOLDER_C_CFLAGS)

EXTRA_DIST = nautilus-ifolder.h nautilus-ifolder.cs emblem-ifolder.icon emblem-ifolder.png ifolder-folder.png AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile 
	sed -e 's,@_VERSION_@,@VERSION@,g' $<  > $@
	chmod +x $@

# Make sure the following files are not included in the dist
dist-hook:
	rm -rf `find $(distdir) |grep nautilus-ifolder.exe`
	rm -rf `find $(distdir) |grep nautilus-ifolder.h$$`

# FIXME: get this from the .pc file
nautilus_extensiondir=$(INSTALL_EXTENSION_PATH)

nautilus_extension_LTLIBRARIES=libnautilus-ifolder.la

libnautilus_ifolder_la_SOURCES =	\
	nautilus-ifolder.c				\
	nautilus-ifolder-holder.c		\
	nautilus-ifolder-holder.h

libnautilus_ifolder_la_LDFLAGS = -module -export-dynamic -avoid-version $(SIMIAS_CLIENT_C_LIBS) $(IFOLDER_C_LIBS) $(XML2_LIBS)
libnautilus_ifolder_la_LIBADD  = $(NAUTILUS_LIBS) $(GNOME_VFS2_LIBS)

libnautilus_ifolder_la_DEPENDENCIES = nautilus-ifolder.exe

nautilus-ifolder.exe: nautilus-ifolder.cs AssemblyInfo.cs
	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_CLIENT_LIBS) $(IFOLDER_LIBS) nautilus-ifolder.cs AssemblyInfo.cs

install-exec-local: libnautilus-ifolder.la
	$(mkinstalldirs) $(INSTALL_EMBLEM_PATH)
	$(INSTALL_PROGRAM) emblem-ifolder.icon $(INSTALL_EMBLEM_PATH)
	$(INSTALL_PROGRAM) emblem-ifolder.png $(INSTALL_EMBLEM_PATH) -m 644
	$(mkinstalldirs) $(INSTALL_ICONS_PATH)
	$(INSTALL_PROGRAM) ifolder-folder.png $(INSTALL_ICONS_PATH)  -m 644
	$(mkinstalldirs) $(INSTALL_EXE_PATH)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder.exe $(INSTALL_EXE_PATH)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_OPTS) nautilus-ifolder $(INSTALL_EXE_PATH)

uninstall-local:
	cd $(INSTALL_EXTENSION_PATH); rm -f libnautilus-ifolder*
	cd $(INSTALL_EMBLEM_PATH); rm -f emblem-ifolder.icon; rm -f emblem-ifolder.png
	cd $(INSTALL_ICONS_PATH); rm -f ifolder-folder.png
	cd $(INSTALL_EXE_PATH); rm -f nautilus-ifolder*

clean-local:
	rm -f nautilus-ifolder.exe
	rm -rf $(COMMON_CLEAN_FILES)
	
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
