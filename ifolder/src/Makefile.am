if LINUX
SUBDIRS = common/WebService common/libifolder common/iFolder UI/Linux/TrayIcon UI/Linux/LinuxClient
endif

if DARWIN
SUBDIRS = common/WebService common/libifolder UI/Mac/OSXClient
endif

if WINDOWS
SUBDIRS = common/WebService common/iFolder UI/Windows/iFolderShell UI/Windows/FormsTrayApp
endif

DIST_SUBDIRS = $(SUBDIRS) 

# remove all of the .svn directories
dist-hook:
	rm -rf `find $(distdir) -name .svn`

EXTRA_DIST = $(srcdir)/ifolder-snakeoil.keys images

install-util uninstall-util:
if WINDOWS
	$(MAKE) -C UI/Windows/FormsTrayApp $@
endif

register unregister:
if WINDOWS
	$(MAKE) -C UI/Windows/iFolderShell $@
endif

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

