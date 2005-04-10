DIST_SUBDIRS = libsimdezvous Simdezvous 

if DARWIN
SUBDIRS = libsimdezvous Simdezvous 

endif

if LINUX
SUBDIRS = libsimdezvous Simdezvous 

endif

if WINDOWS
SUBDIRS = libsimdezvous Simdezvous 

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


