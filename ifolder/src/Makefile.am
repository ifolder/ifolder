if LINUX
SUBDIRS = WebService iFolder ClientUpgrade TrayIcon LinuxClient
endif

if DARWIN
SUBDIRS = WebService libifolder OSXClient
endif

if WINDOWS
SUBDIRS = WebService iFolder ClientUpgrade iFolderShell FormsTrayApp
endif

DIST_SUBDIRS = images iFolder TrayIcon GtkTrayApp iFolderShell FormsTrayApp WebService ClientUpgrade libifolder OSXClient

# remove all of the CVS directories
dist-hook:
	rm -rf `find $(distdir) -name CVS`

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

