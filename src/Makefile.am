if LINUX
SUBDIRS = WebService libifolder iFolder ClientUpgrade TrayIcon LinuxClient
endif

if DARWIN
SUBDIRS = WebService libifolder OSXClient
endif

if WINDOWS
SUBDIRS = WebService iFolder ClientUpgrade iFolderShell FormsTrayApp
endif

DIST_SUBDIRS = iFolder TrayIcon LinuxClient iFolderShell FormsTrayApp WebService ClientUpgrade libifolder OSXClient

# remove all of the CVS directories
dist-hook:
	rm -rf `find $(distdir) -name CVS`

EXTRA_DIST = $(srcdir)/ifolder-snakeoil.keys images

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

