DIST_SUBDIRS = libsimiasbonjour Bonjour 

if DARWIN
SUBDIRS = libsimiasbonjour Bonjour 

endif

if LINUX
SUBDIRS = libsimiasbonjour Bonjour 

endif

if WINDOWS
SUBDIRS = libsimiasbonjour Bonjour 

endif

EXTRA_DIST = 

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir)

installcheck-summary:
	rm -f $(REPORT_DIR)/*.test.xml
	$(MAKE) -i installcheck
	$(REPORT_EXE) $(REPORT_DIR)

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)


