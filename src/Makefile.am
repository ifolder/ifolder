if LINUX
SUBDIRS = WebService iFolder ClientUpgrade TrayIcon GtkTrayApp
endif

if DARWIN
SUBDIRS = WebService
endif

if WINDOWS
SUBDIRS = WebService iFolder ClientUpgrade iFolderShell FormsTrayApp
endif

DIST_SUBDIRS = iFolder TrayIcon GtkTrayApp iFolderShell FormsTrayApp WebService	ClientUpgrade

EXTRA_DIST = $(srcdir)/ifolder-snakeoil.keys

register unregister:
if WINDOWS
	$(MAKE) -C iFolderShell $@
endif

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

