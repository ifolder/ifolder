SUBDIRS = $(COMMON_SUBDIRS) $(PLATFORM_SUBDIRS)

DIST_SUBDIRS = $(COMMON_SUBDIRS) $(LINUX_SUBDIRS) $(WINDOWS_SUBDIRS)

EXTRA_DIST = $(srcdir)/addressbook-snakeoil.keys $(srcdir)/install-addressbook-to-gac.in

install-exec-local: install-addressbook-to-gac
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) install-addressbook-to-gac $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f install-addressbook-to-gac

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

